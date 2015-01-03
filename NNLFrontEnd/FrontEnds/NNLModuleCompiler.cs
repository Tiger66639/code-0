// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLModuleCompiler.cs" company="">
//   
// </copyright>
// <summary>
//   parses 1 or more files into a single node tree and renders this tree into
//   a module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using System.Linq;

    /// <summary>
    ///     parses 1 or more files into a single node tree and renders this tree into
    ///     a module.
    /// </summary>
    public class NNLModuleCompiler
    {
        /// <summary>The invalid chars.</summary>
        public static char[] InvalidChars =
            {
                ' ', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '=', '.', '<', 
                ',', '>', '\\', '/', '?', '\'', '"', '|'
            };

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NNLModuleCompiler"/> class. 
        ///     Initializes a new instance of the <see cref="NNLModuleCompiler"/>
        ///     class.</summary>
        /// <param name="networkDict">The network dict.</param>
        public NNLModuleCompiler()
        {
            BindingPathCodeRefs = null;
            AllowFunctionCalls = false;
            RenderingArguments = false;
            Root = new NNLClassNode();
        }

        #endregion

        /// <summary>
        ///     clears the compiler so it is ready to compile a new set.
        /// </summary>
        internal void Clear()
        {
            fPrevError = null; // make certain that there are no more errors kept in mem
            Errors.Clear();
            fIncluded.Clear();
            Module = null;
            if (fJumpPoints != null)
            {
                fJumpPoints.Clear();
            }

            fRenderingTo.Clear();
            if (fCondPartIndex != null)
            {
                fCondPartIndex.Clear();
            }

            AllowFunctionCalls = false;
            RenderingArguments = false;
        }

        /// <summary>Compiles the specified module.</summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="toCompile">To compile.</param>
        public void Compile(string moduleName, Module toCompile)
        {
            LogService.Log.LogInfo("NNL compiler", "begin compilation of " + moduleName);
            try
            {
                Errors.Clear();
                CheckDependencies(toCompile);
                fModulePath = new System.Uri(
                    PathUtil.VerifyPathEnd(System.IO.Path.GetDirectoryName(moduleName)), 
                    System.UriKind.Absolute);
                Module = toCompile;
                if (Brain.Current.Modules.TextBinding > -1
                    && Brain.Current.Modules.TextBinding < Brain.Current.Modules.Items.Count
                    && Brain.Current.Modules.Items[Brain.Current.Modules.TextBinding] == Module)
                {
                    // if we are recompiling the module that is providing the bindings for all the other parts of the parser, make certain that the bindings are reloaded after the module has been recompiled.
                    OutputParser.ResetExpressionsHandler();
                }

                LoadCompiledData(toCompile);
                var iParser = new NNLParser(Root, this);
                if (ParseFileList(iParser, toCompile.FileNames) == false
                    && ParseFileList(iParser, toCompile.ExtensionFiles) == false)
                {
                    RemovePreviousDef(Module);

                        // if all the source could be read properly, remove the previous def before rendering again.
                    Render();
                }

                if (HasErrors == false && Brain.Current.Modules.Items.Contains(Module) == false)
                {
                    // only add if there are no errors. if this is a recompile, don't add the module again, it's already in there.
                    Brain.Current.Modules.Items.Add(Module);
                }
            }
            finally
            {
                LogService.Log.LogInfo("NNL compiler", "end of compilation for" + moduleName);
            }
        }

        /// <summary>checks if all the dependencies that the module defines are loaded.</summary>
        /// <param name="toCompile"></param>
        private void CheckDependencies(Module toCompile)
        {
            foreach (var i in toCompile.DependsOn)
            {
                var iFound = (from u in Brain.Current.Modules.Items where u.ID.Name == i select u).FirstOrDefault();
                if (iFound == null)
                {
                    WriteError(
                        string.Format(
                            "The module you are trying to load requires that the module '{0}' is already loaded.", 
                            i));
                }
            }
        }

        /// <summary>used to compile a <paramref name="source"/> file that only contains
        ///     expressions, no neuron definitions. Also compiles any extra files
        ///     included. This is used to compile queries, which can be defined by
        ///     only the code body.</summary>
        /// <param name="name"></param>
        /// <param name="source"></param>
        public void Compile(string name, ICompilationSource source)
        {
            LoadCompiledData(null);
            Errors.Clear();
            foreach (var i in name)
            {
                if (InvalidChars.Contains(i))
                {
                    WriteError(string.Format("Invalid name: {0} for root.", name, i));
                }
            }

            LogService.Log.LogInfo("NNL compiler", "begin compilation of " + name);
            try
            {
                Module = source.Module;
                var iParser = new NNLParser(Root, this);
                if (ParseSource(name, iParser, source) == false && ParseFileList(iParser, source.SourceFiles) == false)
                {
                    RemovePreviousDef(Module);

                        // if all the source could be read properly, remove the previous def before rendering again.
                    using (var iChildren = source.Result.ChildrenW)
                        iChildren.Clear();

                            // before we render, make certain that the previous code is gone. Some items could stil have been left here, if they were referenced by multiple modules (and therefor didn't get deleted when the previous ref was remvoed.
                    Render();
                }
                else
                {
                    HasErrors = true;

                        // make certain that the rest of the system knows that something did not compile correcly.
                }
            }
            finally
            {
                LogService.Log.LogInfo("NNL compiler", "end of compilation for" + name);
            }
        }

        /// <summary>
        ///     loads all the statically defined instructions into the compiler.
        /// </summary>
        public void LoadInstructions()
        {
            var iStaticsClass = new NNLClassNode();

                // a special class for statics, so that they don't interfere with the rest of the code.
            iStaticsClass.Name = "statics";
            Root.Children.Add("statics", iStaticsClass);
            for (ulong i = 1; i < (ulong)PredefinedNeurons.EndOfStatic; i++)
            {
                if (Brain.Current.IsExistingID(i))
                {
                    var iName = ((PredefinedNeurons)i).ToString();
                    if (iName != null)
                    {
                        iName = iName.ToLower();
                        if (iName.EndsWith("instruction") && iName != "instruction")
                        {
                            // only add instructions, all the rest needs to be added through code decl
                            iName = iName.Remove(iName.Length - 11);
                            foreach (var u in iName)
                            {
                                if (InvalidChars.Contains(u))
                                {
                                    WriteError(string.Format("Invalid name: {0} for static: {1}", iName, i));
                                }
                            }
                        }

                        var iNode = new NNLNeuronNode(NeuronType.Neuron);
                        var iStatic = Brain.Current[i];
                        iNode.ID = i;
                        if (iStatic is Instruction || iStatic is Variable || i == (ulong)PredefinedNeurons.True
                            || i == (ulong)PredefinedNeurons.False || i == (ulong)PredefinedNeurons.Neuron
                            || i == (ulong)PredefinedNeurons.NeuronCluster || i == (ulong)PredefinedNeurons.IntNeuron
                            || i == (ulong)PredefinedNeurons.DoubleNeuron || i == (ulong)PredefinedNeurons.TextNeuron
                            || i == (ulong)PredefinedNeurons.TimeSpan || i == (ulong)PredefinedNeurons.Time)
                        {
                            // vars (like currentSin), instructios and some regular neurons go in the root, other statics go in a special 'statics' class. This is to prevent statics from interfering with items that come from includes. (like 'index', which is a static, but a very commonly used word, so sometimes declared in a seperate class taht gets included.
                            if (iStatic.ID == (ulong)PredefinedNeurons.ReturnValue)
                            {
                                // there is also an instruction 'returnValue'. If we don't put this in the statics, it would never be reachable.
                                iStaticsClass.Children.Add(iName, iNode);
                            }
                            else
                            {
                                Root.Children[iName] = iNode;
                            }
                        }
                        else
                        {
                            iStaticsClass.Children.Add(iName, iNode);
                        }
                    }
                    else
                    {
                        WriteError(string.Format("No name found for static: {1}", iName, i));
                    }
                }
            }
        }

        /// <summary>Writes the specified error to the log and stores it in the list of
        ///     errors.</summary>
        /// <param name="toLog"></param>
        /// <param name="source">The source.</param>
        internal void WriteError(string toLog, string source = "Module compiler")
        {
            if (toLog != fPrevError)
            {
                // don't need to log the same error 2 times. Can happen for '@' paths, when the var is not defined: GetTypeDecl will render the same error multiple times.
                HasErrors = true;
                Errors.Add(toLog);
                if (ParserBase.BlockLogErrors == false)
                {
                    LogService.Log.LogError(source, toLog);
                }

                fPrevError = toLog;
            }
        }

        /// <summary>parses a <paramref name="list"/> of files.</summary>
        /// <param name="parser"></param>
        /// <param name="list"></param>
        /// <returns><see langword="false"/> if there were no errors, otherwise<see langword="true"/></returns>
        private bool ParseFileList(NNLParser parser, System.Collections.Generic.IList<string> list)
        {
            var iHasErrors = false;
            for (var iCount = 0; iCount < list.Count; iCount++)
            {
                var i = list[iCount];
                try
                {
                    System.Uri iUri;
                    if (System.IO.Path.IsPathRooted(i) == false)
                    {
                        // if the path is relative, change it to absolute and store it in the module so that the engine knows where it can search for a possible upgrade (optional feature).
                        iUri = new System.Uri(fModulePath, new System.Uri(i, System.UriKind.Relative));
                        i = iUri.LocalPath; // need to use localPath, oterhwise spaces aren't handled correctly.
                        list[iCount] = i;

                            // need to convert from relative to specific, so that the module can be rebuild, while storing the mod file in the db but not the source files.
                    }

                    var iContent = System.IO.File.ReadAllText(i);
                    parser.Reset();
                    parser.Parse(i, iContent);
                    iHasErrors = iHasErrors ? iHasErrors : parser.HasErrors;

                        // update the error status: only 1 parse needs to have an error for the whole thing to be in error.
                }
                catch (System.Exception e)
                {
                    WriteError(e.Message, i);
                    return true;
                }
            }

            return iHasErrors;
        }

        /// <summary>parses a list of <paramref name="source"/> strings.</summary>
        /// <param name="name">The name.</param>
        /// <param name="parser"></param>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ParseSource(string name, NNLParser parser, ICompilationSource source)
        {
            var sourceName = source.Module.ID.Name;
            var toParse = source.SourceString;

            var iQuery = new NNLNeuronNode(NeuronType.Query);

                // the compilation is for a query (usually), we add it here to the ns, so it can be used within the code, whithout having to do anything special.
            iQuery.ID = source.Source.ID;
            parser.CollectNS(name.ToLower(), iQuery);
            var iRoot = new NNLClusterNode();

                // we create a fixed object for the source file that has no cluster wrapping.s
            iRoot.ID = source.Result.ID; // so we compile into the cluster.
            parser.CollectNS(name.ToLower() + "_code", iRoot);
            var iHasErrors = false;
            try
            {
                try
                {
                    parser.Reset();
                    iRoot.Content = parser.ParseCodeBlock(sourceName, toParse);
                    iHasErrors = parser.HasErrors;
                }
                finally
                {
                    parser.NS.Pop();

                        // remove the custom cluster from the namespace-stack, so taht we can parse the result of the source.
                    parser.NS.Pop(); // also remove the source neuron.
                }
            }
            catch (System.Exception e)
            {
                WriteError(e.Message, toParse);
                return true;
            }

            return iHasErrors;
        }

        /// <summary>The render.</summary>
        private void Render()
        {
            try
            {
                Root.RenderFromDupList(this);
                SaveCompiledData();
            }
            catch (System.Exception e)
            {
                WriteError(e.Message);
            }

            if (HasErrors)
            {
                RemovePreviousDef(Module);
            }
        }

        /// <summary>
        ///     saves all the registered bindings to disk in the module object.
        /// </summary>
        private void SaveCompiledData()
        {
            if (HasErrors == false)
            {
                SaveRegisteredBindings();
                SaveTree();
                if (CallOSFunction != null && CallOSFunction.Item != null)
                {
                    // if the module defines the OS callback, let the system know this, so it can be used in ohter parts as well.
                    Brain.Current.Modules.CallOsCallback = CallOSFunction.Item.ID;
                }
            }
        }

        /// <summary>
        ///     saves the parse tree of the currently compiled module, so that it can
        ///     be reloaded
        /// </summary>
        private void SaveTree()
        {
            var iStream = new System.IO.MemoryStream();
            var iWriter = new System.IO.BinaryWriter(iStream);

                // can't put this in a using clause, cause then we close the stream and we wouldn't be able to save it anymore.
            iWriter.Write(1); // the file version.
            foreach (var i in Root.Children)
            {
                if ((i.Value.Item == null && i.Value.Name != "statics")
                    || (i.Value.Item != null && i.Value.Item.ID > (ulong)PredefinedNeurons.EndOfStatic))
                {
                    // the statics are loaded automatically when the compiler is created.
                    iWriter.Write(i.Value.GetType().ToString()); // so we know which type of bind item.
                    i.Value.Write(iWriter);
                }
            }

            Module.CompiledData = iStream;
        }

        /// <summary>The save registered bindings.</summary>
        private void SaveRegisteredBindings()
        {
            if (RegisteredBindings != null)
            {
                var iStream = new System.IO.MemoryStream();
                var iWriter = new System.IO.BinaryWriter(iStream);

                    // can't put this in a using clause, cause then we close the stream and we wouldn't be able to save it anymore.
                iWriter.Write(1); // the file version.
                foreach (var i in RegisteredBindings)
                {
                    i.Value.Write(iWriter);
                }

                Module.RegisteredBindings = iStream;
            }
        }

        /// <summary>Removes the previous data from the network. This is done by looking up
        ///     the obj file and removing al the neurons that are declared in the file
        ///     and which are no longer referenced when everything is deleted.</summary>
        /// <param name="module">The module.</param>
        public static void RemovePreviousDef(Module module)
        {
            DeleteLibRefs(module);
            RemoveReferences(module);
            module.RegisteredBindings = new System.IO.MemoryStream();

                // we assign an empty memory stream to let the module know that a change has occured in the RegisteredBindigns, but since it is empty, it will know that the module has to delete the file.
            var iNewRetries = new System.Collections.Generic.List<Neuron>();
            iNewRetries.Capacity = 5000;

                // set the initial size to something decent so that there aren't to many resizes.
            foreach (var i in module.Neurons)
            {
                Neuron iN;
                if (Brain.Current.TryFindNeuron(i, out iN))
                {
                    iN.ModuleRefCount--; // one less reference count.
                    if (iN.ModuleRefCount == 0)
                    {
                        if (BrainHelper.HasIncommingReferences(iN) == false
                            || BrainHelper.OnlyIncommingFrom(iN, module.Neurons))
                        {
                            // HasIncommingreferences is fast, but also need to verify circular refs.
                            Brain.Current.Modules.Varmanager.TryRemove(iN, GetNameOfLocal(iN));
                            if (Brain.Current.Delete(iN) == false)
                            {
                                // if for some reason,can't  delete (maybe used as a link info somewhere), try again.
                                iNewRetries.Add(iN);
                            }
                        }
                        else
                        {
                            iNewRetries.Add(iN);
                        }
                    }
                }
            }

            module.Neurons.Clear();

            System.Collections.Generic.List<Neuron> iRetries = null;
            while (iRetries == null || iNewRetries.Count != iRetries.Count)
            {
                iRetries = iNewRetries;
                iNewRetries = new System.Collections.Generic.List<Neuron>();
                for (var i = 0; i < iRetries.Count; i++)
                {
                    if (BrainHelper.HasIncommingReferences(iRetries[i]) == false
                        || BrainHelper.OnlyIncommingFrom(iRetries[i], module.Neurons))
                    {
                        // don't need to check anymore if moduleRefCount == 0, this has already happened, simply need to check the incomming refs.
                        Brain.Current.Modules.Varmanager.TryRemove(iRetries[i], GetNameOfLocal(iRetries[i]));
                        if (Brain.Current.Delete(iRetries[i]) == false)
                        {
                            iNewRetries.Add(iRetries[i]);
                        }
                    }
                    else
                    {
                        iNewRetries.Add(iRetries[i]);
                    }
                }
            }
        }

        /// <summary>deletes all librefs.</summary>
        /// <param name="module"></param>
        private static void DeleteLibRefs(Module module)
        {
            var iSin = (from i in Brain.Current.Sins where i is ReflectionSin select (ReflectionSin)i).FirstOrDefault();
            foreach (var i in module.LibRefs)
            {
                Neuron iN;
                if (Brain.Current.TryFindNeuron(i, out iN))
                {
                    iN.ModuleRefCount--; // one less reference count.
                    if (iN.ModuleRefCount == 0)
                    {
                        if (iSin != null)
                        {
                            // if there is no sin, no need to try and unload.
                            iSin.UnloadMethod(iN);

                                // unload the ref to the function if there is no more module referencing this method. This will always delete the neuron
                        }
                    }
                }
            }

            module.LibRefs.Clear();
        }

        /// <summary>returnst hte name ofthe local if it is a local.</summary>
        /// <param name="neuron"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string GetNameOfLocal(Neuron neuron)
        {
            if (neuron is Local)
            {
                return NetworkDict.GetName(neuron.ID);
            }

            return null;
        }

        /// <summary>removes all the rendered items from the referenced clusters and also
        ///     links.</summary>
        /// <param name="module"></param>
        private static void RemoveReferences(Module module)
        {
            var iToDestroy = new System.Collections.Generic.List<Link>();
            foreach (var iID in module.ExternalRefs)
            {
                Neuron iN;
                if (Brain.Current.TryFindNeuron(iID, out iN))
                {
                    using (var iLinks = iN.LinksOut)
                        foreach (var i in iLinks)
                        {
                            if (module.Neurons.Contains(i.ToID) && (iN.ModuleRefCount == 1 || i.To.ModuleRefCount == 1))
                            {
                                // only try to delete the links if the item is included by 1 module (this one), and nothign else.
                                iToDestroy.Add(i);
                            }
                        }

                    using (var iLinks = iN.LinksIn)
                        foreach (var i in iLinks)
                        {
                            if (module.Neurons.Contains(i.FromID)
                                && (iN.ModuleRefCount == 1 || i.From.ModuleRefCount == 1))
                            {
                                // only try to delete the links if the item is included by 1 module (this one), and nothign else.
                                iToDestroy.Add(i);
                            }
                        }

                    foreach (var i in iToDestroy)
                    {
                        i.Destroy();
                    }

                    iToDestroy.Clear(); // no need to try and delete the same link multiple times.

                    var iCluster = iN as NeuronCluster;
                    if (iCluster != null)
                    {
                        foreach (var i in module.Neurons)
                        {
                            Neuron iToRemove;
                            if (Brain.Current.TryFindNeuron(i, out iToRemove))
                            {
                                if (iN.ModuleRefCount == 1 || iToRemove.ModuleRefCount == 1)
                                {
                                    using (var iChildren = iCluster.ChildrenW)
                                        while (iChildren.Remove(i))
                                        {
                                        }
 // could be that the same item is in the list multiple times.
                                }
                            }
                        }

                        foreach (var i in module.ExternalRefs)
                        {
                            Neuron iToRemove;
                            if (Brain.Current.TryFindNeuron(i, out iToRemove))
                            {
                                if (iN.ModuleRefCount == 1 || iToRemove.ModuleRefCount == 1)
                                {
                                    using (var iChildren = iCluster.ChildrenW)
                                        while (iChildren.Remove(i))
                                        {
                                        }
 // could be that the same item is in the list multiple times.
                                }
                            }
                        }
                    }

                    iN.ModuleRefCount--; // we need to remove the module ref count as well.
                }
            }

            module.ExternalRefs.Clear();
            module.RegisteredBindings = null;
        }

        // internal Neuron Find(string Meaning)
        // {
        // ulong iFound = fDict[Meaning];
        // return Brain.Current[iFound];
        // }

        /// <summary>Adds the specified <paramref name="item"/> to the dictionaries and the
        ///     module (if not yet added).</summary>
        /// <param name="item">The item.</param>
        internal void Add(NNLStatementNode item)
        {
            if (item.Item.ID > (ulong)PredefinedNeurons.EndOfStatic)
            {
                // statics don't need to be logged as being used, they arlways remain in the system + there name is already defined (can't overwrite them without changing the defaultData).
                Add(item.Item);
                if (string.IsNullOrEmpty(item.Name) == false)
                {
                    NetworkDict.SetName(item.Item, item.Name);
                }
            }
        }

        /// <summary>registers the specified neuron with the module, if not already
        ///     happened.</summary>
        /// <param name="item"></param>
        internal void Add(Neuron item)
        {
            if (item != null && fIncluded.Contains(item) == false)
            {
                if (Module != null)
                {
                    // when rendering for another parser (like textpatterns), this could be null
                    Module.Add(item);
                }
                else
                {
                    item.ModuleRefCount++;

                        // always make certain that the moduleRefcount is updated, so we know how many compilations are using the same neuron. When 0, we know we can remove.
                }

                fIncluded.Add(item);
            }
        }

        /// <summary>removes the specified neuron again from the list of rendered items (in
        ///     case it was a temp, for handling recursion).</summary>
        /// <param name="item"></param>
        internal void Remove(Neuron item)
        {
            if (item != null && fIncluded.Contains(item))
            {
                if (Module != null)
                {
                    // when rendering for another parser (like textpatterns), this could be null
                    Module.Remove(item);
                }
                else
                {
                    item.ModuleRefCount--;

                        // always make certain that the moduleRefcount is updated, so we know how many compilations are using the same neuron. When 0, we know we can remove.
                }

                fIncluded.Remove(item);
            }
        }

        /// <summary>registers the specified cluster with the module as a clustered that
        ///     was referenced, but not rendered by the module, if not already
        ///     happened.</summary>
        /// <param name="item">The item.</param>
        internal void AddExternal(Neuron item)
        {
            if (item != null)
            {
                if (fIncluded.Contains(item) == false)
                {
                    if (Module != null)
                    {
                        // when rendering for another parser (like textpatterns), this could be null
                        Module.AddExternalRef(item);
                    }
                    else
                    {
                        item.ModuleRefCount++;

                            // always make certain that the moduleRefcount is updated, so we know how many compilations are using the same neuron. When 0, we know we can remove.
                    }

                    fIncluded.Add(item);
                }
                else if (Module != null && Module.ExternalRefs.Contains(item.ID) == false)
                {
                    // it's already included, but not as an external. this can happen if the external is a cluster that already contained code and the list could be reused for something else.
                    Module.Remove(item);
                    Module.AddExternalRef(item);
                }
            }
        }

        /// <summary>Gets the doubleNeuron to represent the specified<see langword="double"/> value.</summary>
        /// <param name="p">The p.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal Neuron GetDouble(double p)
        {
            var iFound = Brain.Current.Modules.Varmanager.GetDouble(p);
            Add(iFound);
            return iFound;
        }

        /// <summary>Gets the <see cref="IntNeuron"/> to represent the specified<see langword="int"/> value.</summary>
        /// <param name="p">The p.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal Neuron GetInt(int p)
        {
            var iFound = Brain.Current.Modules.Varmanager.GetInt(p);
            Add(iFound);
            return iFound;
        }

        /// <summary>adds the <paramref name="var"/> in the appropriate dict, so that it
        ///     can potentially be re-used in other parts of the code.</summary>
        /// <param name="var">The var.</param>
        internal void RegisterVar(NNLLocalDeclNode var)
        {
            Add(var);
            if (var.Scope == VarScope.Local)
            {
                Brain.Current.Modules.Varmanager.AddLocal(var.Name, var.Item);
            }
        }

        /// <summary>adds the <paramref name="binding"/> the list of registered bindings so
        ///     that it gets saved when the parse is done. Also checks that there is
        ///     only 1 <paramref name="binding"/> per <see langword="operator"/>
        ///     registered.</summary>
        /// <param name="binding"></param>
        internal void AddRegisteredBinding(NNLBinding binding)
        {
            NNLBinding iFound;
            if (RegisteredBindings == null)
            {
                RegisteredBindings = new System.Collections.Generic.Dictionary<Token, NNLBinding>();
            }

            if (RegisteredBindings.TryGetValue(binding.Operator, out iFound))
            {
                binding.LogPosError(
                    string.Format(
                        "Can't register the binding {0}, another binding ({1}) is already handling the {2} operator", 
                        binding.Name, 
                        iFound.Name, 
                        Tokenizer.GetSymbolForToken(binding.Operator)), 
                    this);
            }
            else
            {
                RegisteredBindings.Add(binding.Operator, binding);
            }
        }

        /// <summary>finds or loads a reflection sin entry point as defined in the string
        ///     that contains an xml element of<see cref="ExportableReflectionSinEntryPoint"/></summary>
        /// <param name="xmlData"></param>
        /// <param name="requestor">The requestor.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal Neuron GetLibRef(string xmlData, NNLNode requestor)
        {
            try
            {
                ExportableReflectionSinEntryPoint iData;

                var iSettings = new System.Xml.XmlReaderSettings();
                iSettings.IgnoreComments = true;
                iSettings.IgnoreProcessingInstructions = true;
                iSettings.IgnoreWhitespace = true;
                using (
                    System.IO.TextReader iInput =
                        new System.IO.StringReader("<?xml version='1.0' encoding='utf-8'?>" + xmlData))
                {
                    // there is no xml header, just an element.
                    using (var iReader = System.Xml.XmlReader.Create(iInput, iSettings))
                    {
                        var valueSerializer =
                            new System.Xml.Serialization.XmlSerializer(typeof(ExportableReflectionSinEntryPoint));
                        iData = (ExportableReflectionSinEntryPoint)valueSerializer.Deserialize(iReader);
                    }
                }

                var iSin =
                    (from i in Brain.Current.Sins where i is ReflectionSin select (ReflectionSin)i).FirstOrDefault();
                if (iSin != null)
                {
                    var iRes = iSin.LoadMethod(iData);
                    Module.AddLibRef(iRes);
                    NetworkDict.SetName(iRes, iData.MappedName);
                    return iRes;
                }

                requestor.LogPosError("No Os channel loaded: can't bind to OS functions.", this);
            }
            catch (System.Exception e)
            {
                requestor.LogPosError(e.Message, this);
            }

            return null;
        }

        /// <summary>removes the specified neuron and all the related ones. Their
        ///     moduleRefCount goes down 1 and if it's 0, they get deleted. This is
        ///     used by the conditionParser, <see cref="DoParser"/> and output parser
        ///     to remove compiled code. where there is no list available of neurons.
        ///     Note: only expressions and code/argument clusteres get deleted right
        ///     away. All other neurons only get deleted if they are no longer
        ///     referenced. This is because there is no 'external' references section
        ///     for this type of removal. This type of code can't declare neurons
        ///     anyway, so all references to neurons other then code are always
        ///     references, which aren't part of the module to be deleted. This is
        ///     important, if it isn't done, to much can be deleted (Like textneurons
        ///     and clusters).</summary>
        /// <param name="toRemove">The to Remove.</param>
        internal static void RemovePreviousDef(Neuron toRemove)
        {
            var iHandled = new System.Collections.Generic.HashSet<Neuron>();
            var iCheckAtEnd = new System.Collections.Generic.List<Neuron>();

                // contains all the neurons that need to be checked at the end of removal. All items in this list can only be deleted when they no longer have any reference.
            var iCheckAtEndIds = new System.Collections.Generic.HashSet<ulong>(); // works faster
            var iProcessNext = new System.Collections.Generic.List<Neuron>();
            var iProcessNow = new System.Collections.Generic.List<Neuron>();

            iHandled.Add(toRemove);
            iProcessNow.Add(toRemove);
            while (iProcessNow.Count > 0)
            {
                foreach (var i in iProcessNow)
                {
                    i.ModuleRefCount--;
                    var iclusterMeaning = Neuron.EmptyId;
                    if (i is NeuronCluster)
                    {
                        // code clusters and argument lists should also be deleted.
                        iclusterMeaning = ((NeuronCluster)i).Meaning;
                        if (iclusterMeaning == (ulong)PredefinedNeurons.ArgumentsList)
                        {
                            iclusterMeaning = (ulong)PredefinedNeurons.Code;
                        }
                    }

                    if (i is Expression || (iclusterMeaning == (ulong)PredefinedNeurons.Code))
                    {
                        // only delete code parts.
                        if (i.ID > (ulong)PredefinedNeurons.EndOfStatic && i.ModuleRefCount <= 0)
                        {
                            // should never be smaller then 0, but check anyway
                            using (ListAccessor<Link> iOut = i.LinksOut)

                                // only get related items if the item itself no longer has any refs. This forms a barrier between code generated for patterns and code in the modules/queries.
                                foreach (var u in iOut)
                                {
                                    var iTo = u.To;
                                    if (iHandled.Contains(iTo) == false)
                                    {
                                        iProcessNext.Add(iTo);
                                        iHandled.Add(iTo);
                                    }
                                }

                            var iCluster = i as NeuronCluster;
                            if (iCluster != null)
                            {
                                using (IDListAccessor iChildren = iCluster.Children)
                                {
                                    var iTemp = iChildren.ConvertTo<Neuron>();
                                    if (iTemp != null)
                                    {
                                        foreach (var u in iTemp)
                                        {
                                            if (iHandled.Contains(u) == false)
                                            {
                                                iProcessNext.Add(u);
                                                iHandled.Add(u);
                                            }
                                        }

                                        Factories.Default.NLists.Recycle(iTemp);
                                    }
                                }
                            }

                            Brain.Current.Modules.Varmanager.TryRemove(i, GetNameOfLocal(i));
                            Brain.Current.Delete(i);
                        }
                    }
                    else if (i.ID > (ulong)PredefinedNeurons.EndOfStatic && i.ModuleRefCount <= 0)
                    {
                        iCheckAtEnd.Add(i);
                        iCheckAtEndIds.Add(i.ID);
                    }
                }

                iProcessNow = iProcessNext;
                iProcessNext = new System.Collections.Generic.List<Neuron>();
            }

            var iTryNext = new System.Collections.Generic.List<Neuron>();
            var iFound = true;
            while (iFound)
            {
                // try to delete the remainder of the items recursively untill no more items can be deleted.
                iFound = false;
                foreach (var i in iCheckAtEnd)
                {
                    if (i.CanBeDeleted && BrainHelper.HasOtherReferences(i, iCheckAtEndIds) == false)
                    {
                        Brain.Current.Modules.Varmanager.TryRemove(i, GetNameOfLocal(i));
                        Brain.Current.Delete(i);
                    }
                    else
                    {
                        iTryNext.Add(i);
                    }
                }

                if (iTryNext.Count > 0 && iTryNext.Count != iCheckAtEnd.Count)
                {
                    iFound = true;
                    var iTemp = iCheckAtEnd;
                    iCheckAtEnd = iTryNext;
                    iTryNext = iTemp;
                    iTryNext.Clear();
                }
            }
        }

        #region inner types

        /// <summary>
        ///     defines some extra data that is required for rendering conditionals.
        /// </summary>
        internal class ConditionalRenderData
        {
            /// <summary>The cond part return value.</summary>
            public IntNeuron CondPartReturnValue;

            /// <summary>The f cond part index.</summary>
            private int fCondPartIndex;

            /// <summary>
            ///     the total amount of parts ( to determin if a <see langword="bool" />
            ///     or <see langword="int" /> needs to be returned).
            /// </summary>
            public int TotalNrOfCondParts;

            /// <summary>
            ///     the index of the current part we are rendering.
            /// </summary>
            public int CondPartIndex
            {
                get
                {
                    return fCondPartIndex;
                }

                set
                {
                    fCondPartIndex = value;
                    CondPartReturnValue = null; // need to reset this value each time we go to a new part.
                }
            }

            /// <summary>gets the <see langword="int"/> neuron that represents the current
            ///     part.</summary>
            /// <param name="compiler"></param>
            /// <returns>The <see cref="IntNeuron"/>.</returns>
            public IntNeuron GetReturnValue(NNLModuleCompiler compiler)
            {
                if (CondPartReturnValue == null)
                {
                    CondPartReturnValue = (IntNeuron)compiler.GetInt(CondPartIndex);
                }

                return CondPartReturnValue;
            }
        }

        #endregion

        #region Fields

        /// <summary>The f module path.</summary>
        private System.Uri fModulePath;

        /// <summary>The f included.</summary>
        private readonly System.Collections.Generic.HashSet<Neuron> fIncluded =
            new System.Collections.Generic.HashSet<Neuron>();

                                                                    // stores all the neurons that have already been inlcuded in this project, so that we can make certain that we don't add duplicated to the module during rendering.

        /// <summary>The f rendering to.</summary>
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>> fRenderingTo =
            new System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>>();

        /// <summary>The f jump points.</summary>
        private System.Collections.Generic.Stack<IntNeuron> fJumpPoints;

        /// <summary>The f cond part index.</summary>
        private System.Collections.Generic.Stack<ConditionalRenderData> fCondPartIndex;

        /// <summary>The f errors.</summary>
        private readonly System.Collections.Generic.List<string> fErrors = new System.Collections.Generic.List<string>();

        /// <summary>The f prev error.</summary>
        private string fPrevError; // so that we don't write the same error multiple times.

        #endregion

        #region Prop

        #region root

        /// <summary>
        ///     Gets the root of the parse.
        /// </summary>
        internal NNLClassNode Root { get; private set; }

        #endregion

        #region NetworkDict

        /// <summary>
        ///     Gets the dictionary callback to the network. for assigning names,
        ///     retrieving externals,...
        /// </summary>
        public static IRendererDict NetworkDict { get; set; }

        #endregion

        /// <summary>
        ///     keeps track if there were errors during the generate stage, so that we
        ///     know when to clean up.
        /// </summary>
        public bool HasErrors { get; set; }

        #region Module

        /// <summary>
        ///     Gets the module that we are compiling to.
        /// </summary>
        public Module Module { get; private set; }

        #endregion

        #region Errors

        /// <summary>
        ///     Gets the errors that were generated during the compilation process.
        ///     These are also written to the log, but this property gives access to
        ///     the exact errors so that they can be displayed in a popup box for
        ///     instance.
        /// </summary>
        public System.Collections.Generic.List<string> Errors
        {
            get
            {
                return fErrors;
            }
        }

        #endregion

        #region RenderingTo

        /// <summary>
        ///     Gets the list to where we are currently rendering a list of items to
        ///     (code). It's a stack cause this can be nested. This allows us to
        ///     render multiple objects for code items. There can be no extra items
        ///     after the <see cref="JaStDev.HAB.Parsers.NNLStatementNode.Item" />
        ///     value.
        /// </summary>
        public System.Collections.Generic.Stack<System.Collections.Generic.List<Neuron>> RenderingTo
        {
            get
            {
                return fRenderingTo;
            }
        }

        #endregion

        #region RegisteredBindings

        /// <summary>
        ///     Gets the list of bindings that were labeled as being registered. Once
        ///     all the code is rendered, this list is written to the module. It is
        ///     also used during the parsing of bindings defined in other parts then
        ///     code. It's a dict so that, during the rendering, we can easily check
        ///     if there are multiple bindings for the same <see langword="operator" />
        ///     + it provides an easy lookup for when not during a full code parse,
        ///     but when called from another parser.
        /// </summary>
        internal System.Collections.Generic.Dictionary<Token, NNLBinding> RegisteredBindings { get; set; }

        #endregion

        #region RenderingArguments

        /// <summary>
        ///     <see langword="true" /> if we are rendering the arguments for a
        ///     statement or function call (so that a conditional doesn't render it's
        ///     extra bits needed to do a real call, but akts as a reference)
        /// </summary>
        public bool RenderingArguments { get; set; }

        #endregion

        #region AllowFunctionCalls

        /// <summary>
        ///     gets/sets wether we are currently rendering something that allows a
        ///     function-call as a child or not. Some instructions don't allow this,
        ///     even though they can contain elements that allow functioncalls. For
        ///     instance, all filter instructions can have an expresion to evaluate to
        ///     true/false for determining to include an item or not (the
        ///     filter-function). This is called from within the instruction call, so
        ///     there can't be any extra code lists that need to be executed.
        /// </summary>
        public bool AllowFunctionCalls { get; set; }

        #endregion

        #region CallOSFunction

        /// <summary>
        ///     Gets/sets the object that should be used for rendering calls to os
        ///     functions.
        /// </summary>
        internal NNLStatementNode CallOSFunction { get; set; }

        #endregion

        #region JumpPoints

        /// <summary>
        ///     Gets the queue of jump points that were rendered by and/or booleans,
        ///     and which have not yet been closed (the position still needs to be
        ///     assigned).
        /// </summary>
        public System.Collections.Generic.Stack<IntNeuron> JumpPoints
        {
            get
            {
                if (fJumpPoints == null)
                {
                    fJumpPoints = new System.Collections.Generic.Stack<IntNeuron>();
                }

                return fJumpPoints;
            }
        }

        #endregion

        #region CondPartIndex

        /// <summary>
        ///     Gets the stack with the index positions (and some extra info) of the
        ///     conditional parts that we are currently rendering. The last on the
        ///     stack is the index position of the current conditional part being
        ///     rendered. This is used by the booleans to determin
        /// </summary>
        internal System.Collections.Generic.Stack<ConditionalRenderData> CondPartIndex
        {
            get
            {
                if (fCondPartIndex == null)
                {
                    fCondPartIndex = new System.Collections.Generic.Stack<ConditionalRenderData>();
                }

                return fCondPartIndex;
            }
        }

        #endregion

        #region IsRenderingCondPart

        /// <summary>
        ///     when true, we are rendering the condition for a conditional part. This
        ///     is used by and/or booleans to determin how they should render the
        ///     extra code.
        /// </summary>
        public bool IsRenderingCondPart { get; set; }

        #endregion

        #region BindingPathCodeRefs

        /// <summary>
        ///     Gets/sets the list of neurons that were rendered for the current
        ///     binding (binding manages the stack) and which reference some other
        ///     part of the code (a variable that is used in a path, or the result of
        ///     an expression). This is used so that variables can be correctly passed
        ///     along to the binding's get or set overloaders.
        /// </summary>
        public System.Collections.Generic.List<Neuron> BindingPathCodeRefs { get; set; }

        #endregion

        #endregion

        #region IPatternExpressionParser Members

        /// <summary>The load os call function.</summary>
        private void LoadOsCallFunction()
        {
            Neuron iCallOSFunc;
            if (Brain.Current.Modules.CallOsCallback != Neuron.EmptyId)
            {
                iCallOSFunc = Brain.Current[Brain.Current.Modules.CallOsCallback];
                if (iCallOSFunc is ExpressionsBlock)
                {
                    CallOSFunction = new NNLExpBlockNode();
                }
                else if (iCallOSFunc is NeuronCluster)
                {
                    CallOSFunction = new NNLClusterNode();
                }
                else
                {
                    CallOSFunction = null;
                }

                if (CallOSFunction != null)
                {
                    CallOSFunction.Item = iCallOSFunc;
                }
            }
            else
            {
                CallOSFunction = null;
            }
        }

        /// <summary>The load bindings from.</summary>
        /// <param name="from">The from.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadBindingsFrom(Module from)
        {
            var iSource = from.RegisteredBindings;
            if (iSource != null)
            {
                if (iSource.Length == 0)
                {
                    throw new System.InvalidOperationException(
                        "No pre-compiled bindings found, can't parse $ ^ # ~ statements.");
                }

                var iBindingsClass = new NNLClassNode();
                iSource.Position = 0;
                var iReader = new System.IO.BinaryReader(iSource);

                    // don't use a using, cause this might close a memory stream, in which case we are in trouble.
                var iVer = iReader.ReadInt32();
                if (iVer != 1)
                {
                    throw new System.InvalidOperationException(
                        "The compiled data of the module is of a newer version, can't read the content!");
                }

                RegisteredBindings = new System.Collections.Generic.Dictionary<Token, NNLBinding>();
                while (iSource.Position < iSource.Length)
                {
                    var iNew = new NNLBinding();
                    iNew.Read(iReader);
                    RegisteredBindings.Add(iNew.Operator, iNew);
                    Root.Bindings.Add(iNew.Operator, iNew);

                        // needs to be registered as a binding so it can be used while generating the code.
                    iNew.Parent = Root;

                    // dont' add to dict, this is done by the compiled data section.
                }
            }
        }

        /// <summary>loads all the previously compiled data into the compiler so that it
        ///     can be reused in other modules. This loads every evailable module.
        ///     Each module is loaded into it's own <see langword="namespace"/> (the
        ///     name of the module).</summary>
        /// <param name="toCompile">To module being compiled. This allows us to determine if the bindings
        ///     need to be loaded + which module not to load.</param>
        public void LoadCompiledData(Module toCompile)
        {
            LoadInstructions(); // load all the instructions before trying to parse
            foreach (var i in Brain.Current.Modules.Items)
            {
                if (i != toCompile)
                {
                    // don't load previous compilation data of the module we are trying to recompile.
                    LoadCompiledDataFrom(i);
                }
            }

            if (Brain.Current.Modules.TextBinding > -1
                && Brain.Current.Modules.TextBinding < Brain.Current.Modules.Items.Count)
            {
                var iBindingsModule = Brain.Current.Modules.Items[Brain.Current.Modules.TextBinding];
                if (iBindingsModule != toCompile)
                {
                    LoadBindingsFrom(iBindingsModule);
                }
            }

            LoadOsCallFunction();
        }

        /// <summary>loads all the compiled data (except the bindings)<paramref name="from"/> the specified module into the compiler under a<see langword="namespace"/> with the name of hte module.</summary>
        /// <param name="from"></param>
        private void LoadCompiledDataFrom(Module from)
        {
            var iSource = from.CompiledData;
            if (iSource != null)
            {
                if (iSource.Length == 0)
                {
                    throw new System.InvalidOperationException(
                        "No pre-compiled bindings found, can't parse $ ^ # ~ statements.");
                }

                iSource.Position = 0;
                var iReader = new System.IO.BinaryReader(iSource);

                    // don't use a using, cause this might close a memory stream, in which case we are in trouble.
                var iVer = iReader.ReadInt32();
                if (iVer != 1)
                {
                    throw new System.InvalidOperationException(
                        "The compiled data of the module is of a newer version, can't read the content!");
                }

                var iRoot = new NNLClassNode();
                iRoot.Parent = Root;
                iRoot.Name = from.ID.Name.Replace(" ", string.Empty).ToLower();

                    // make certain that there are no spaces in the name.
                Root.Children.Add(iRoot.Name, iRoot);
                while (iSource.Position < iSource.Length)
                {
                    var iType = iReader.ReadString();
                    var iNew = (NNLStatementNode)System.Activator.CreateInstance(System.Type.GetType(iType), true);

                        // allow non public constructors as well
                    iNew.Read(iReader);
                    iNew.Parent = Root;
                    iRoot.Children.Add(iNew.Name, iNew); // also needs to be accessible through code by it's name.
                }
            }
        }

        /// <summary>parses and compiles a single expression, found in the current position
        ///     of the <paramref name="source"/> tokenizer.</summary>
        /// <param name="title">The title.</param>
        /// <param name="source">a tokenezir that contains the text to be parsed. It should be at the
        ///     position at which parsing needs to begin</param>
        /// <param name="asShort">When true, a unary expression is read (for variable/thes/topic/asset
        ///     paths). Otherwise a full expression is read (for conditionals).</param>
        /// <returns>The neuron that represents the expression</returns>
        public Neuron GetExpressionFrom(string title, Tokenizer source, bool asShort = true)
        {
            Clear();
            Neuron iRes = null;
            var iRoot = new NNLClassNode();

                // we create a temp root item, so that we can easily remove the object that was parsed (this allows us to reuse the compiler without having to continuously reload the static data like instructions and bindings)
            iRoot.Name = "tempRoot";
            iRoot.Parent = Root;
            Root.Children["tempRoot"] = iRoot;

                // don't use add, if something went wrong, temproot might still be loaded there.
            var iParser = new NNLParser(iRoot, this);
            iParser.PrepareForSectionParse(title, source);
            NNLStatementNode iExp;
            if (asShort)
            {
                iExp = iParser.ReadBindingPath();
            }
            else
            {
                iExp = iParser.ReadExpression();
            }

            var iItems = new System.Collections.Generic.List<Neuron>();
            RenderingTo.Push(iItems);
            try
            {
                if (iParser.HasErrors == false)
                {
                    // only render if nothing went wrong.
                    iExp.Parent = iRoot;
                    iExp.Render(this);
                    if (iExp.Item != null && iExp.Item.ID != (ulong)PredefinedNeurons.ReturnValue)
                    {
                        // if it's a return value, don't collect it, then we don't need to render a new return statement.
                        var iArgs =
                            NNLStatementNode.GetParentsFor(
                                new System.Collections.Generic.List<Neuron> { iExp.Item }, 
                                (ulong)PredefinedNeurons.ArgumentsList, 
                                this, 
                                string.Empty);
                        iRes = NNLStatementNode.GetStatement(
                            (ulong)PredefinedNeurons.ReturnValueInstruction, 
                            iArgs, 
                            this);
                        iItems.Add(iRes);
                    }

                    if (iItems.Count > 0)
                    {
                        iRes = NNLStatementNode.GetParentsFor(iItems, (ulong)PredefinedNeurons.Code, this, string.Empty);
                    }
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                RenderingTo.Pop();
                Root.Children.Remove("tempRoot"); // make certain that we can re-use the compiler object.
            }

            return iRes;

            // return iExp.Item;
        }

        /// <summary>Allows nnl code to bind to the compiler and parse simple expressions.</summary>
        /// <param name="title"></param>
        /// <param name="toParse"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron ParseExpression(string title, string toParse)
        {
            var iTokens = new Tokenizer(toParse);
            return OutputParser.ExpressionsHandler.GetExpressionFrom(title, iTokens);
        }

        /// <summary>parses and compiles a block of code, found in the current position of
        ///     the <paramref name="source"/> tokenizer.</summary>
        /// <param name="title">The title.</param>
        /// <param name="source">a tokenezir that contains the text to be parsed. It should be at the
        ///     position at which parsing needs to begin</param>
        /// <returns>The neuron(cluster) that represents the code block</returns>
        public Neuron GetCodeBlockFrom(string title, Tokenizer source)
        {
            Errors.Clear();
            var iRoot = new NNLClassNode();

                // we create a temp root item, so that we can easily remove the object that was parsed (this allows us to reuse the compiler without having to continuously reload the static data like instructions and bindings)
            iRoot.Name = "tempRoot";
            Root.Children["tempRoot"] = iRoot;

                // don't use add, if something went wrong, temproot might still be loaded there.
            iRoot.Parent = Root;
            var iParser = new NNLParser(iRoot, this);
            iParser.PrepareForSectionParse(title, source);
            var iFunction = iParser.ReadCodeForOtherParser();
            if (source.NextStart < source.ToParse.Length - 1)
            {
                iParser.LogPosWarning("Unexpected end found.");
            }

            iFunction.Parent = iRoot; // so objects can find the data
            try
            {
                if (iParser.HasErrors == false)
                {
                    // only render if nothing went wrong.
                    iFunction.Render(this);
                    return iFunction.Item;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                Root.Children.Remove("tempRoot"); // make certain that we can re-use the compiler object.
            }
        }

        #endregion
    }
}