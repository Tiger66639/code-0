//-----------------------------------------------------------------------
// <copyright file="ReflectionSin.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>23/10/2012</date>
//-----------------------------------------------------------------------


#if ANDROID
using Android.Content;
#endif

namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     eventargs for library loading/unloading.
    /// </summary>
    public class LibEventargs : System.EventArgs
    {
        public string Location { get; set; }

        public System.Reflection.Assembly Value { get; set; }
    }

    /// <summary>
    ///     A sensory interface able to communicate with the .net environment.
    /// </summary>
    /// <remarks>
    ///     You can either call a function or generate code. A function is called if a link is found on
    ///     the 'output neuron', with meaning 'reflectionSinCall'. All other neurons are interpreted as
    ///     dll rendering instructions.
    ///     <para>
    ///         You can currently only call public static methods.
    ///         If there are no arguments, you don't have to attach a parameter list.
    ///         To call a function, send a neuron to the output which points to a neuron that is mapped to a
    ///         MethodInfo, using the meaning <see cref="PredefinedNeurons.ReflectionSinCall" />
    ///     </para>
    ///     <para>
    ///         Supported function-results:
    ///         int, double, string, IEnumerable: int, double, string, which are returned through the 'value' link on
    ///         the input
    ///     </para>
    ///     <para>
    ///         For generating IL code, the sin needs to know which neurons represent which opcode.
    ///         Since the opcode list depends on the system that the network runs on, there are no
    ///         static defined neurons for this.  Instead, a reflection sin is able to generate these
    ///         neurons through <see cref="ReflectionSin.CreateOpcodes" />.
    ///         In the current implementation, IL code generates classes, fields, methods, opcode in the methods.
    ///         The generated libraries are stored in a directory, defined by <see cref="Settings.LibraryStore" />
    ///     </para>
    ///     <para>
    ///         When the reflection sin is loaded into the brain, all the previously rendered libraries are also
    ///         loaded into .net so they are available in the host app.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Questions, typeof(Neuron))] //should be removed
    [NeuronID((ulong)PredefinedNeurons.ReflectionSinCall, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.NameOfMember, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.MemberType, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.BodyOfMember, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TypeOfMember, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ReflectionSinException, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ReflectionSin, typeof(Neuron))]
    public class ReflectionSin : Sin
    {
        #region Fields

        private System.Collections.Generic.Dictionary<ulong, System.Reflection.MethodInfo> fEntryPoints;

#if ANDROID
      static Context fContext;
#else
        private bool fOpcodesChanged; //keeps track of changed opcodes.

        private System.Collections.Generic.Dictionary<ulong, int> fOpcodes;

        private System.Collections.Generic.List<string> fDynamicAssemblies =
            new System.Collections.Generic.List<string>();
#endif

        private bool fEntryPointsChanged;
                     //keeps track if the entrypoints list has changed, so we don't do to many saves.

        private System.Collections.Generic.Dictionary<string, int> fFunctionAssemblies;

        private bool fFunctionAssembliesChanged; //keeps track of changes in the list of function references.

        private bool fDynamicAssembliesChanged = true;
                     //by default, set to true, so all newly created reflectionsins will save this in a prop. when loaded from disk, the loadFunction sets this to false.

        #endregion

        /// <summary>
        ///     Occurs when a library was added to the reflection sin.
        /// </summary>
        public event System.EventHandler<LibEventargs> LibraryAdded;

        public event System.EventHandler<LibEventargs> LibraryRemoved;

        #region prop

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReflectionSin;
            }
        }

        #endregion

        #region FunctionAssemblies

        /// <summary>
        ///     Gets the dictionary that stores all the assemblies that need to be loaded for the referenced methods.
        ///     Per assembly, we keep track of the number of loaded functions, so we know when to remove an assembly.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> FunctionAssemblies
        {
            get
            {
                if (fFunctionAssemblies == null && Brain.Current != null)
                {
                    LoadFunctionAssemblies();
                }
                return fFunctionAssemblies;
            }
        }

        /// <summary>
        ///     Loads the function assemblies.
        /// </summary>
        private void LoadFunctionAssemblies()
        {
            fFunctionAssemblies = new System.Collections.Generic.Dictionary<string, int>();
            var iList =
                Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ReflectionSinFunctionAssembly>>(
                    typeof(ReflectionSin),
                    ID + "FunctionAssemblies");
            if (iList != null)
            {
                foreach (var i in iList)
                {
                    var iName = i.Name.ToLower();
                    if (fFunctionAssemblies.ContainsKey(iName) == false)
                    {
                        fFunctionAssemblies.Add(iName, i.RefCount);
                            //we do to lower to make certain that the dll name is always the same case (older versions didn't do this, so we also do it during loading, to make certain that old projects are also working ok)
                        var iAssem = GetAssembly(iName);
                            //make certain that the assembly is loaded in the project, so we can get to the functions.
                        if (LibraryAdded != null)
                        {
                            LibraryAdded(this, new LibEventargs { Location = iName, Value = iAssem });
                        }
                    }
                }
            }
        }

        #endregion

        #region Entrypoints

        /// <summary>
        ///     Gets the entry points. They are loaded if needed.
        /// </summary>
        /// <value>The entry points.</value>
        public System.Collections.Generic.Dictionary<ulong, System.Reflection.MethodInfo> EntryPoints
        {
            get
            {
                if (fEntryPoints == null && Brain.Current != null)
                {
                    LoadEntryPoints();
                }
                return fEntryPoints;
            }
        }

        private void LoadEntryPoints()
        {
            fEntryPoints = new System.Collections.Generic.Dictionary<ulong, System.Reflection.MethodInfo>();
            var iList =
                Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ReflectionSinEntryPoint>>(
                    typeof(ReflectionSin),
                    ID + "Methods");
            if (iList != null)
            {
                LoadEntryPoints(iList);
            }
        }

        /// <summary>
        ///     loads the entry poitns from the specified list.
        /// </summary>
        /// <param name="list"></param>
        public void LoadEntryPoints(System.Collections.Generic.List<ReflectionSinEntryPoint> list)
        {
            var iBadEntryPoints = new System.Collections.Generic.List<ReflectionSinEntryPoint>();
                //stores objects that represent entry points which were not found and have to be removed.
            foreach (var i in list)
            {
                var iType = Brain.Current.GetNeuronType(i.TypeName);
                if (iType != null)
                {
                    var iTypes = (from t in i.ParameterTypes let tt = System.Type.GetType(t) select tt).ToArray();
                    var iInfo = iType.GetMethod(i.MethodName, iTypes);
                    if (iInfo != null)
                    {
                        fEntryPoints.Add(i.ID, iInfo);
                    }
                    else
                    {
                        iBadEntryPoints.Add(i);
                        LogService.Log.LogError(
                            "ReflectionSin.EntryPoints",
                            string.Format("Can't find method '{0}', in '{1}'.", i.MethodName, i.TypeName));
                    }
                }
                else
                {
                    iBadEntryPoints.Add(i);
                    LogService.Log.LogError(
                        "ReflectionSin.EntryPoints",
                        string.Format("Can't find type '{0}'.", i.TypeName));
                }
            }
            if (iBadEntryPoints.Count > 0)
            {
                foreach (var i in iBadEntryPoints)
                    //remove all neurons that couldn't be loaded (need to do this, otherwise we can have an inconsistency.
                {
                    Brain.Current.Delete(i.ID);
                }
                if (fEntryPoints.Count == 0)
                    //this is to make certain that you can always unload all assemblies. (there could be problems otherwise, when the assembly was stored with a full path (original implementation) while we now work with relative paths.
                {
                    FunctionAssemblies.Clear();
                    fFunctionAssembliesChanged = true;
                }
                fEntryPointsChanged = true;
            }
        }

        #endregion

#if ANDROID
      #region context
      /// <summary>
      /// Provides access to the context to the Android OS that can be used by the functions.
      /// </summary>
      public static Context Context
      {
         get
         {
            return fContext;
         }
         set
         {
            fContext = value;
         }
      } 
      #endregion
#else

        #region OpCodes

        /// <summary>
        ///     Gets the dictionary that maps the neurons to the opcode id that they represent.
        /// </summary>
        /// <value>The opcodes.</value>
        public System.Collections.Generic.Dictionary<ulong, int> Opcodes
        {
            get
            {
                if (fOpcodes == null && Brain.Current != null)
                {
                    LoadOpCodes();
                }
                return fOpcodes;
            }
        }

        /// <summary>
        ///     Loads the op codes.
        /// </summary>
        private void LoadOpCodes()
        {
            var iFound = Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                typeof(ReflectionSin),
                ID + "OpCodes");
            if (iFound != null)
            {
                fOpcodes = new System.Collections.Generic.Dictionary<ulong, int>();
                for (var i = 0; i < iFound.Count; i++)
                {
                    fOpcodes.Add(iFound[i], i + 1);
                }
            }
        }

        #endregion

        #region DynamicAssemblies

        /// <summary>
        ///     Gets the list of dynamic assembly names generated by this sin.
        ///     Warning: when you change the content of this list, also update the 'fDynamicAssembliesChanged' var,
        ///     otherwise the value won't be saved.
        /// </summary>
        public System.Collections.Generic.List<string> DynamicAssemblies
        {
            get
            {
                return fDynamicAssemblies;
            }
        }

        #endregion

        #region AssemblyPath

        /// <summary>
        ///     Gets the path where all the dynamic assemblies are saved to. The directory is created
        ///     if it doesn't already exist.
        /// </summary>
        /// <remarks>
        ///     This is a sub directory of the neurons data path.
        /// </remarks>
        /// <value>The assembly path.</value>
        public string AssemblyPath
        {
            get
            {
                string iPath = null;
                if (Brain.Current != null)
                {
                    iPath = System.IO.Path.Combine(Brain.Current.Storage.DataPath, ID + Text + "Assemblies");
                    if (System.IO.Directory.Exists(iPath) == false)
                    {
                        System.IO.Directory.CreateDirectory(iPath);
                    }
                }
                return iPath;
            }
        }

        #endregion

#endif

        #endregion

        #region Overrides

        /// <summary>
        ///     Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.
        /// </summary>
        /// <param name="toSend">output</param>
        /// <remarks>
        ///     <para>
        ///         for processing, so <see cref="ReflectionSin.State" />.
        ///     </para>
        ///     <para>
        ///         This method is called by the <see cref="Brain" /> itself during/after processing of input.
        ///     </para>
        ///     <para>
        ///         Neuron structure for rendering:
        ///         new assembly -> text: name of assembly (m: NameOfMember)
        ///         -> cluster: types in assembly (m: MemberBody)
        ///         foreach in cluster:
        ///         new type -> text: name of type (m: NameOfMember)
        ///         -> cluster: members in type (m: memberbody)
        ///         foreach in cluster
        ///         new ('method'        -> text: name of method (m: NameOfMember)
        ///         -> cluster: opcode for method (m: bodyofmember)
        ///         -> text: return type of method (m: TypeOfmember)
        ///         |'construction'  -> cluster: opcode for method (m: bodyofmember)
        ///         |'destructor     -> cluster: opcode for method (m: bodyofmember)
        ///         |'event          -> text: name of event (m: NameOfMember)
        ///         -> text: type of event (m: TypeOfmember)
        ///         |'field'         -> text: name of event (m: NameOfMember)
        ///         -> text: type of event (m: TypeOfmember)
        ///         |'GenericPar'    -> text: name of event (m: NameOfMember)
        ///         |'property'      -> text: name of event (m: NameOfMember)
        ///         -> text: type of event (m: TypeOfmember)
        ///         -> cluster: methods to generate (m: bodyOfmember, info 'get')
        ///         foreach method in cluster
        ///         new method -> text: name of method (m: NameOfMember) (should be 'get' or 'set')
        ///         -> cluster: opcode for method (m: bodyofmember)
        ///         -> text: return type of method (m: TypeOfmember)
        ///         ) -> 'if' determined by value found at link with meaning MemberType, content is a string.
        ///     </para>
        /// </remarks>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                var iFunction = i.FindFirstOut((ulong)PredefinedNeurons.ReflectionSinCall);
                if (iFunction != null)
                {
                    PerformCall(i, iFunction);
                }
#if !ANDROID
                else
                {
                    RenderIL(i);
                }
#endif
            }
        }

        /// <summary>
        ///     Sets the id.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void SetId(ulong value)
        {
            base.SetId(value);
            fFunctionAssemblies = new System.Collections.Generic.Dictionary<string, int>();
                //so we don't try to load this the first time we want to create a reflectionsin.
        }

        /// <summary>
        ///     Called when all the data of the sensory interface needs to be loaded into memory.
        /// </summary>
        public override void TouchMem()
        {
            base.TouchMem();
            if (Brain.Current != null)
            {
                if (fFunctionAssemblies == null)
                {
                    LoadFunctionAssemblies();
                }
                if (fEntryPoints == null)
                {
                    LoadEntryPoints();
                }
#if !ANDROID
                if (fOpcodes == null)
                {
                    LoadOpCodes();
                }
                fOpcodesChanged = true;
                if (fDynamicAssemblies == null)
                {
                    LoadDynamicLibs();
                }
                fDynamicAssembliesChanged = true;
#endif
                fEntryPointsChanged = true; //need to make certain everything is saved correctly.
                fFunctionAssembliesChanged = true;
            }
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            if (Brain.Current != null)
            {
                FlushEntryPoints();
                FlushFunctionAssemblies();
#if !ANDROID
                FlushOpcodes();
                if (fDynamicAssembliesChanged)
                {
                    Brain.Current.Storage.SaveProperty(typeof(ReflectionSin), ID + "Assemblies", fDynamicAssemblies);
                    fDynamicAssembliesChanged = false;
                }
#endif
            }
        }

        private void FlushFunctionAssemblies()
        {
            if (fFunctionAssembliesChanged && fFunctionAssemblies != null)
            {
                var iAssem =
                    (from i in fFunctionAssemblies
                     let u = new ReflectionSinFunctionAssembly { Name = i.Key, RefCount = i.Value }
                     select u).ToList();
                Brain.Current.Storage.SaveProperty(typeof(ReflectionSin), ID + "FunctionAssemblies", iAssem);
                fFunctionAssembliesChanged = false;
            }
        }

#if !ANDROID
        private void FlushOpcodes()
        {
            if (fOpcodesChanged && fOpcodes != null)
            {
                var iOpcodes = (from i in fOpcodes orderby i.Value select i.Key).ToList();
                Brain.Current.Storage.SaveProperty(typeof(ReflectionSin), ID + "OpCodes", iOpcodes);
                fOpcodesChanged = false;
            }
        }
#endif

        private void FlushEntryPoints()
        {
            if (fEntryPointsChanged && fEntryPoints != null)
            {
                var iList = GetLoadedEntryPoints();
                Brain.Current.Storage.SaveProperty(typeof(ReflectionSin), ID + "Methods", iList);
                fEntryPointsChanged = false;
            }
        }

        /// <summary>
        ///     creates a list of objects that can be written to a file and which describe the methods that were loaded
        ///     and function as entry points into libraries.
        /// </summary>
        /// <returns></returns>
        public System.Collections.Generic.List<ReflectionSinEntryPoint> GetLoadedEntryPoints()
        {
            var iList = new System.Collections.Generic.List<ReflectionSinEntryPoint>();
            foreach (var i in EntryPoints)
            {
                var iParamTypeNames =
                    (from p in i.Value.GetParameters() select p.ParameterType.AssemblyQualifiedName).ToList();
                var iNew = new ReflectionSinEntryPoint
                               {
                                   ID = i.Key,
                                   MethodName = i.Value.Name,
                                   ParameterTypes = iParamTypeNames,
                                   TypeName = i.Value.ReflectedType.FullName
                               };
                iList.Add(iNew);
            }
            return iList;
        }

        /// <summary>
        ///     Rerturns the type of the specified property. This is used so we can read all the property types from the storage at
        ///     once
        ///     for converting storage type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        ///     Since this sin doesn't have any properties, nothing is returned.
        /// </remarks>
        public override System.Type GetTypeForProperty(string name)
        {
            if (name.EndsWith("Methods"))
            {
                return typeof(System.Collections.Generic.List<string>);
            }
            if (name.EndsWith("OpCodes"))
            {
                return typeof(System.Collections.Generic.List<ulong>);
            }
            if (name.EndsWith("FunctionAssemblies"))
            {
                return typeof(System.Collections.Generic.List<ReflectionSinFunctionAssembly>);
            }
#if !ANDROID
            if (name.EndsWith("Assemblies"))
            {
                return fDynamicAssemblies.GetType();
            }
#endif
            return base.GetTypeForProperty(name);
        }

        /// <summary>
        ///     Reads the XML.
        /// </summary>
        /// <remarks>
        ///     We override this function cause whenever this neuron is loaded, we want to make
        ///     certain that all the dll's are also loaded into the system.
        /// </remarks>
        /// <param name="reader">The reader.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
#if !ANDROID
            LoadDynamicLibs();
#endif
        }

        /// <summary>
        ///     Reads the neuron in file version 1 format.
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
#if !ANDROID
            LoadDynamicLibs();
#endif
            return iRes;
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Loads the specified method in the dictionary, when possible.
        /// </summary>
        /// <param name="toLoad">To load.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public Neuron LoadMethod(System.Reflection.MethodInfo toLoad)
        {
            if (toLoad.IsStatic)
            {
                if (toLoad.IsPublic)
                {
                    var iNew = NeuronFactory.GetNeuron();
                    Brain.Current.Add(iNew);
                    EntryPoints.Add(iNew.ID, toLoad);

                    var iAssemName = GetRelativePath(toLoad.Module.Assembly.Location).ToLower();

                    int iRefCount;
                    if (FunctionAssemblies.TryGetValue(iAssemName, out iRefCount))
                    {
                        FunctionAssemblies[iAssemName] = iRefCount + 1;
                    }
                    else
                    {
                        FunctionAssemblies[iAssemName] = 1;
                        if (LibraryAdded != null)
                        {
                            LibraryAdded(
                                this,
                                new LibEventargs { Location = iAssemName, Value = toLoad.Module.Assembly });
                        }
                    }

                    fFunctionAssembliesChanged = true;
                    fEntryPointsChanged = true;
                    return iNew;
                }
                LogService.Log.LogError("ReflectionSin.Load", "Methods must be public!");
            }
            else
            {
                LogService.Log.LogError("ReflectionSin.Load", "Methods must be static!");
            }
            return null;
        }

        /// <summary>
        ///     Gets the name of the assembly, relative to
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static string GetRelativePath(string location)
        {
            if (string.IsNullOrEmpty(location) == false)
            {
                var iAppPath =
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                return PathUtil.RelativePathTo(PathUtil.VerifyPathEnd(iAppPath), new System.Uri(location).LocalPath);
                    //we first cast to uri and then get localPath, to make certain that the path is formatted correctly.
            }
            return null;
        }

        /// <summary>
        ///     Gets the absolute path for the assembly name.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public string GetAbsPath(string location)
        {
            if (System.IO.Path.IsPathRooted(location) == false)
            {
                var iPath = new System.Uri(location, System.UriKind.Relative);
                var iAppPath =
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                var iTemp = new System.Uri(new System.Uri(PathUtil.VerifyPathEnd(iAppPath)), iPath);
                return iTemp.LocalPath;
            }
            return location;
        }

        /// <summary>
        ///     Removes the function map to the specified neuron from the dictionary.
        /// </summary>
        /// <param name="toUnload">The function to unload.</param>
        public void UnloadMethod(Neuron toUnload)
        {
            System.Reflection.MethodInfo iMethod;
            if (EntryPoints.TryGetValue(toUnload.ID, out iMethod))
            {
                var iAssemName = GetRelativePath(iMethod.Module.Assembly.Location).ToLower();
                int iRefCount;
                if (FunctionAssemblies.TryGetValue(iAssemName, out iRefCount))
                {
                    if (iRefCount > 1)
                    {
                        FunctionAssemblies[iAssemName] = iRefCount - 1;
                    }
                    else
                    {
                        FunctionAssemblies.Remove(iAssemName);
                        if (LibraryRemoved != null)
                        {
                            LibraryRemoved(
                                this,
                                new LibEventargs { Location = iAssemName, Value = iMethod.Module.Assembly });
                        }
                    }
                    fFunctionAssembliesChanged = true;
                }
                EntryPoints.Remove(toUnload.ID);
                if (EntryPoints.Count == 0)
                    //this is to make certain that you can always unload all assemblies. (there could be problems otherwise, when the assembly was stored with a full path (original implementation) while we now work with relative paths.
                {
                    FunctionAssemblies.Clear();
                    fFunctionAssembliesChanged = true;
                }
                fEntryPointsChanged = true;
                Brain.Current.Delete(toUnload);
            }
        }

        /// <summary>
        ///     Gets the ID for the specified method. If the method is not mapped, the <see cref="Neuron.EmptyID" /> is returned.
        /// </summary>
        /// <param name="value">The ID of the neuron used to identify the method or <see cref="Neuron.EmptyID" /> if invalid.</param>
        public ulong GetMethodId(System.Reflection.MethodInfo value)
        {
            foreach (var i in EntryPoints)
            {
                if (i.Value.Equals(value))
                {
                    return i.Key;
                }
            }
            return EmptyId;
        }

#if !ANDROID
        /// <summary>
        ///     Creates all the neurons that represent the opcodes that can be generated when in cill mode.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This implementation loads all the .net cill opcodes (including exceptionblocks/load variables,...).
        ///         All previous opcodes are removed but no previous neurons are deleted.
        ///     </para>
        /// </remarks>
        /// <returns>The list of objects that wrap the neurons that represent recognisable values to the system.</returns>
        public System.Collections.Generic.IEnumerable<NeuronCluster> CreateOpcodes()
        {
            fOpcodesChanged = true;
            if (fOpcodes != null)
            {
                fOpcodes.Clear();
            }
            else
            {
                fOpcodes = new System.Collections.Generic.Dictionary<ulong, int>();
            }
            var iRes = new System.Collections.Generic.List<NeuronCluster>();
            CreateOpcode("BeginExceptionBlock", iRes);
            CreateOpcode("BeginExceptionFilterBlock", iRes);
            CreateOpcode("BeginCatchBlock", iRes);
            CreateOpcode("BeginFaultBlock", iRes);
            CreateOpcode("BeginFinallyBlock", iRes);
            CreateOpcode("BeginScope", iRes);
            CreateOpcode("DeclareLocal", iRes);
            CreateOpcode("DefineLabel", iRes);

            var iOpcodeType = typeof(System.Reflection.Emit.OpCodes);
            foreach (
                var i in
                    iOpcodeType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                )
            {
                CreateOpcode(i.Name, iRes);
            }
            return iRes;
        }

        /// <summary>
        ///     Creates an object for the opcode and adds it to the list.
        /// </summary>
        /// <param name="value">The value for which to create a new object and opcode for.</param>
        /// <param name="list">The list to add the new object to.</param>
        private void CreateOpcode(string value, System.Collections.Generic.List<NeuronCluster> list)
        {
            Neuron iCreated;
            var iRes = BrainHelper.CreateObject(value, out iCreated);
            fOpcodes.Add(iCreated.ID, list.Count);
            list.Add(iRes);
        }
#endif

        #endregion

        #region Helpers

#if !ANDROID

        /// <summary>
        ///     Loads all the dynamically renerated libs.
        /// </summary>
        private void LoadDynamicLibs()
        {
            try
            {
                fDynamicAssemblies =
                    Brain.Current.Storage.GetProperty<System.Collections.Generic.List<string>>(
                        typeof(ReflectionSin),
                        ID + "Assemblies");
                fDynamicAssembliesChanged = false;
                if (fDynamicAssemblies != null)
                {
                    foreach (var i in fDynamicAssemblies)
                    {
                        //Log.LogInfo("ReflectionSin.LoadDynamicLibs", "Still need to load dynmacilly generated assemblies.");
                        var iAssem = System.Reflection.Assembly.LoadFrom(i);
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ReflectionSin.LoadDynamicLibs",
                        string.Format(
                            "Failed to find dynamically generated assemblies list in properties of: {0}.",
                            this));
                    fDynamicAssemblies = new System.Collections.Generic.List<string>();
                        //need to recreate the list, cause it was destroyed while loading something invalid.
                }

                var iFuncAssem = FunctionAssemblies; //also need to load all the functions that can be called.
                if (iFuncAssem != null)
                {
                    foreach (var i in iFuncAssem)
                    {
                        var iAssem = System.Reflection.Assembly.LoadFrom(GetAbsPath(i.Key));
                            //load the assembly, so that the methods can be accessed.
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ReflectionSin.LoadDynamicLibs", "Failed to load dynamic libs with error: " + e);
            }
        }

        #region Render

        /// <summary>
        ///     Creates or loads a dynamic library and renders all the types and the code in the types as defined
        ///     in the output neuron.
        /// </summary>
        /// <param name="toSend">The root neuron that contains all the info to generate a single class.</param>
        private void RenderIL(Neuron toSend)
        {
            var iModule = GetModule(toSend);
        }

        /// <summary>
        ///     Extracts the name from the neuron (through Out: NameOfMember) and creates an assembly and module with that name.
        /// </summary>
        /// <param name="toSend">To send.</param>
        /// <returns>a module or null when the operation failed.</returns>
        private System.Reflection.Emit.ModuleBuilder GetModule(Neuron toSend)
        {
            var iName = toSend.FindFirstOut((ulong)PredefinedNeurons.NameOfMember) as TextNeuron;
            if (iName != null)
            {
                var iAssemName = new System.Reflection.AssemblyName(iName.Text);
                var iAssemBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(
                    iAssemName,
                    System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
                var iPath = iName.Text;
                iPath = System.IO.Path.Combine(AssemblyPath, iPath + ".dll");
                var iRes = iAssemBuilder.DefineDynamicModule(iName.Text, iPath);
                DynamicAssemblies.Add(iPath);
                fDynamicAssembliesChanged = true;
                return iRes;
            }
            return null;
        }

        #endregion

#endif

        #region Call

        /// <summary>
        ///     Calls the specified function on the specified class.  This must be a static member.
        /// </summary>
        /// <param name="toSend">To send.</param>
        private void PerformCall(Neuron toSend, Neuron iToCall)
        {
            if (toSend != null && iToCall != null)
            {
                System.Reflection.MethodInfo iFound;
                if (EntryPoints.TryGetValue(iToCall.ID, out iFound))
                {
                    var iPar = GetParameters(toSend, iFound);
                    if (iPar != null)
                    {
                        try
                        {
                            var iRes = iFound.Invoke(null, iPar.ToArray());
                            var iValue = ConvertResult(iRes);
                            if (iValue != null)
                            {
                                toSend.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, iValue);
                            }
                        }
                        catch (System.Exception e)
                        {
                            TextNeuron iEx;
                            if (e.InnerException != null)
                                //if there is an inner exception, it was caused by the function that was called, otherwise it is caused by trying to call the function.
                            {
                                iEx = NeuronFactory.GetText(e.InnerException.ToString());
                                    //the outer exception is a target Invokation, which wraps the original exception.
                            }
                            else
                            {
                                iEx = NeuronFactory.GetText(e.ToString());
                            }
                            Brain.Current.Add(iEx);
                            toSend.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ReflectionSinException, iEx);
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ReflectionSin.PerformCall",
                        string.Format("There is no function attached to neuron {0}!", iToCall));
                }
            }
            else
            {
                LogService.Log.LogError("ReflectionSin.PerformCall", "null send to reflection sin!");
            }
        }

        private Neuron ConvertResult(object iRes)
        {
            Neuron iValue = null;
            if (iRes is int)
            {
                iValue = NeuronFactory.GetInt((int)iRes);
                Brain.Current.Add(iValue);
            }
            else if (iRes is double)
            {
                iValue = NeuronFactory.GetDouble((double)iRes);
                Brain.Current.Add(iValue);
            }
            else if (iRes is string)
            {
                iValue = GetFor((string)iRes);
            }
            else if (iRes is char)
            {
                var iVal = new string((char)iRes, 1);
                iValue = GetFor(iVal);
            }
            else if (iRes is bool)
            {
                var iBoolTemp = (bool)iRes;
                if (iBoolTemp)
                {
                    return Brain.Current[(ulong)PredefinedNeurons.True];
                }
                return Brain.Current[(ulong)PredefinedNeurons.False];
            }
            else if (iRes is System.DateTime)
            {
                iValue = Time.Current.GetTimeCluster((System.DateTime)iRes);
            }
            else if (iRes is System.TimeSpan)
            {
                iValue = Time.Current.GetTimeSpanCluster((System.TimeSpan)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<int>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<int>)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<double>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<double>)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<string>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<string>)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<char>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<char>)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<System.DateTime>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<System.DateTime>)iRes);
            }
            else if (iRes is System.Collections.Generic.IEnumerable<System.TimeSpan>)
            {
                iValue = BuildClusterResult((System.Collections.Generic.IEnumerable<System.TimeSpan>)iRes);
            }

            else if (iRes != null)
            {
                LogService.Log.LogError(
                    "ReflectionSin.PerformCall",
                    string.Format("Invalid return value: can't translate to neuron:  {0}!", iRes));
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<System.DateTime> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                Neuron iNew = Time.Current.GetTimeCluster(i);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<System.TimeSpan> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                Neuron iNew = Time.Current.GetTimeSpanCluster(i);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<char> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                var iVal = new string(i, 1);
                var iNew = GetFor(iVal);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<string> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                var iNew = GetFor(i);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<double> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                var iNew = NeuronFactory.GetDouble(i);
                Brain.Current.Add(iNew);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        private Neuron BuildClusterResult(System.Collections.Generic.IEnumerable<int> items)
        {
            var iValue = NeuronFactory.GetCluster();
            Brain.Current.Add(iValue);
            foreach (var i in items)
            {
                var iNew = NeuronFactory.GetInt(i);
                Brain.Current.Add(iNew);
                using (var iChildren = iValue.ChildrenW) iChildren.Add(iNew);
            }
            return iValue;
        }

        /// <summary>
        ///     Extracts the parameters arguments from the neuron and returns the values as a list of objects.
        /// </summary>
        /// <param name="toSend">To send.</param>
        /// <param name="method">The method to be called, so we can check for arguments with default values.</param>
        /// <returns>
        ///     null if an invalid value was found, otherwise a list of parameter values (in .net value types, not neurons)
        /// </returns>
        private System.Collections.Generic.List<object> GetParameters(
            Neuron toSend,
            System.Reflection.MethodInfo method)
        {
            var iCluster = toSend.FindFirstOut((ulong)PredefinedNeurons.Arguments) as NeuronCluster;
            var iPar = new System.Collections.Generic.List<object>();
            if (iCluster != null)
            {
                var iParams = method.GetParameters();
                    //this defines all the parameters, so we can check if there were any optional onces, so we can supply the default values.
                System.Collections.Generic.List<ulong> iItems = null;
                using (var iList = iCluster.Children)
                {
                    iItems = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);
                    iItems.AddRange(iList);
                }
                int i;
                for (i = 0; i < iItems.Count; i++)
                {
                    var iNeuron = Brain.Current[iItems[i]];
                    if (iNeuron is TextNeuron)
                    {
                        iPar.Add(((TextNeuron)iNeuron).Text);
                    }
                    else if (iNeuron is IntNeuron)
                    {
                        iPar.Add(((IntNeuron)iNeuron).Value);
                    }
                    else if (iNeuron is DoubleNeuron)
                    {
                        iPar.Add(((DoubleNeuron)iNeuron).Value);
                    }
                    else
                    {
                        var iFound = BrainHelper.GetTextFrom(iNeuron);
                        if (iFound != null)
                        {
                            iPar.Add(iFound);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "ReflectionSin.GetParameters",
                                string.Format(
                                    "Invalid neuron type found as parameter value: {0}!",
                                    iNeuron.GetType().AssemblyQualifiedName));
                            return null;
                        }
                    }
                }
                Factories.Default.IDLists.Recycle(iItems);
                while (iParams.Length > i)
                {
                    if (iParams[i].IsOptional)
                    {
                        iPar.Add(iParams[i++].DefaultValue);
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Parameter is not optional: " + iParams[i].Name);
                    }
                }
            }
            return iPar;
        }

        #endregion

        #endregion

        /// <summary>
        ///     Tries to load the assembly.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static System.Reflection.Assembly GetAssembly(string path)
        {
            var iAppPath =
                PathUtil.VerifyPathEnd(
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
            //string iAppPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ReflectionSin)).CodeBase);
            var iBase = new System.Uri(iAppPath);
            var iPathUri = new System.Uri(path, System.UriKind.Relative);
            if (iPathUri.IsAbsoluteUri == false)
            {
                iPathUri = new System.Uri(iBase, path);
            }
            var iPath = iPathUri.LocalPath.ToLower();
                //need to take localpath, otherwise the spaces aren't handled correctly.
            System.Reflection.Assembly iAsm = null;
            foreach (var i in System.AppDomain.CurrentDomain.GetAssemblies())
                //don't load the same assem 2 times. This is important for the exe, if we load 2 times: xaml wont load anymore.
            {
                if (i.IsDynamic == false && string.IsNullOrEmpty(i.Location) == false)
                    //dynamic libs apparently don't have a location and throw an exception.
                {
                    var iFileInfo = new System.IO.FileInfo(i.Location);
                    if (iFileInfo.FullName.ToLower() == iPath)
                    {
                        iAsm = i;
                        break;
                    }
                }
            }
            try
            {
                if (iAsm == null)
                {
                    iAsm = System.Reflection.Assembly.LoadFrom(iPath);
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "ReflectionSin",
                    string.Format("failed to load assembly {0} at {1}, error: {2}", path, iPath, e));
            }

            return iAsm;
        }

        /// <summary>
        ///     Loads the method.
        /// </summary>
        /// <param name="toLoad">The definition of the method that needs to be loaded.</param>
        /// <returns></returns>
        public Neuron LoadMethod(ExportableReflectionSinEntryPoint toLoad)
        {
            var iAsm = GetAssembly(toLoad.AssemblyName);

            var iType = Brain.Current.GetNeuronType(toLoad.TypeName);
            if (iType != null)
            {
                var iTypes = (from t in toLoad.ParameterTypes let tt = System.Type.GetType(t) select tt).ToArray();
                var iInfo = iType.GetMethod(toLoad.MethodName, iTypes);
                if (GetMethodId(iInfo) == EmptyId) //don't need to load it when already loaded.
                {
                    return LoadMethod(iInfo);
                }
            }
            else
            {
                LogService.Log.LogError(
                    "ReflectionSin.EntryPoints",
                    string.Format("Can't find type '{0}'.", toLoad.TypeName));
            }
            return null;
        }
    }
}