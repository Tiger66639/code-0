// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLFunctionNode.cs" company="">
//   
// </copyright>
// <summary>
//   a node for all types of functions: expressionBlocks, clusters,....
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a node for all types of functions: expressionBlocks, clusters,....
    /// </summary>
    internal class NNLFunctionNode : NNLNodesList
    {
        /// <summary>The f params.</summary>
        private System.Collections.Generic.List<NNLLocalDeclNode> fParams;

        /// <summary>The f return values.</summary>
        private System.Collections.Generic.List<NNLLocalDeclNode> fReturnValues;

        /// <summary>Initializes a new instance of the <see cref="NNLFunctionNode"/> class.</summary>
        public NNLFunctionNode()
            : base(NodeType.CodeList)
        {
        }

        #region Params

        /// <summary>
        ///     Gets/sets the list of parameters for this function . When assigned,
        ///     the items will be added to the dict.
        /// </summary>
        public System.Collections.Generic.List<NNLLocalDeclNode> Params
        {
            get
            {
                return fParams;
            }

            set
            {
                if (value != fParams)
                {
                    fParams = value;
                    if (value != null)
                    {
                        foreach (var i in value)
                        {
                            if (i != null)
                            {
                                // if something went wrong during the parse, it could be that there are nulls in the list, check for this.
                                i.Parent = this; // need to build the tree as well.
                                if (Children.ContainsKey(i.Name) == false)
                                {
                                    // when reading from obj files, the children have already been read and the item is already in the children list, so don't try  to add again, cause that gives an error.
                                    Children.Add(i.Name, i);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region ReturnValues

        /// <summary>
        ///     Gets/sets the list of locals that return a value.
        /// </summary>
        public System.Collections.Generic.List<NNLLocalDeclNode> ReturnValues
        {
            get
            {
                return fReturnValues;
            }

            set
            {
                if (value != fReturnValues)
                {
                    fReturnValues = value;
                    foreach (var i in value)
                    {
                        i.Parent = this; // need to build the tree as well.
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     gets/sets if this function should be rendered inline or not. Note:
        ///     this isn't always possible at the moment.
        /// </summary>
        public bool IsInline { get; set; }

        /// <summary>
        ///     generates the name with all the parameter types appended.
        /// </summary>
        public string FullName
        {
            get
            {
                var iName = new System.Text.StringBuilder(Name);

                foreach (var i in Params)
                {
                    if (iName.Length > 0)
                    {
                        iName.Append("-");
                    }

                    iName.Append(i.TypeDecl);
                }

                return iName.ToString();
            }
        }

        /// <summary>returns the type decl for when a function call is done with this
        ///     function.</summary>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal DeclType GetTypeDeclForCall(NNLModuleCompiler renderTo)
        {
            if (fReturnValues != null)
            {
                return fReturnValues[fReturnValues.Count - 1].GetTypeDecl(renderTo);
            }

            return DeclType.none;
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            Render(renderTo, (ulong)PredefinedNeurons.Code);
        }

        /// <summary>The render.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="clusterMeaning">The cluster meaning.</param>
        internal void Render(NNLModuleCompiler renderTo, ulong clusterMeaning)
        {
            // even inline items get rendered: in case they get called from outside the module.
            if (Item == null)
            {
                // could already be loaded?
                var iPrevIsForArgs = renderTo.RenderingArguments;
                var iAllowFunctionCall = renderTo.AllowFunctionCalls;
                renderTo.RenderingArguments = true;

                    // while rendering code, this should be allowed. Could be turned off, cause this can be called while rendering the content of some arg values, so it needs to be restored after we are done.
                renderTo.AllowFunctionCalls = true;
                try
                {
                    Item = NeuronFactory.GetCluster();
                    Brain.Current.MakeTemp(Item);

                        // make the Item a temp cluster, if there is recursion, it gets consumed and we know that we need to fill the list with items.
                    RenderCode(renderTo);
                    base.Render(renderTo);

                        // this renders all the child nodes. has to be done after rendering the code, otherwise the declerations for the locals are rendered incorrectly.
                    if (Item.ID == Neuron.TempId)
                    {
                        Item = GetParentsFor(RenderedItems, clusterMeaning, renderTo, Name);

                            // builds a new list or re-uses an already existing list when possible.
                    }
                    else
                    {
                        ((NeuronCluster)Item).Meaning = clusterMeaning;

                            // the neuron has been consumed, it's no longer temp. but make certain it has all it's children.
                        using (var iChildren = ((NeuronCluster)Item).ChildrenW) iChildren.AddRange(RenderedItems);
                    }
                }
                finally
                {
                    renderTo.RenderingArguments = iPrevIsForArgs;
                    renderTo.AllowFunctionCalls = iAllowFunctionCall;
                }
            }
            else if (Item.ID == Neuron.TempId)
            {
                Brain.Current.Add(Item);
            }

            renderTo.Add(this);

                // important to always register, even if it's already rendered: when it gets used, it needs to be registered in the system.
        }

        /// <summary>renders the content of this function into the specified cluster.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="renderTo">The render To.</param>
        internal void RenderChildren(NeuronCluster cluster, NNLModuleCompiler renderTo)
        {
            foreach (var i in Children)
            {
                i.Value.Render(renderTo);
                using (var iChildren = cluster.ChildrenW) iChildren.Add(i.Value.Item);
            }
        }

        /// <summary>renders the items into an already existing cluster.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="renderTo"></param>
        internal void RenderItemsInto(NeuronCluster cluster, NNLModuleCompiler renderTo)
        {
            if (RenderedItems == null || RenderedItems.Count == 0)
            {
                // make certain we only try to render 1 time.
                RenderCode(renderTo);

                    // make certain that code is rendered before the base class renders the chidlren, cause that also contains the variables, but they need to be rendered during code rendering.
                base.Render(renderTo); // this renders all the child nodes (var decls).
            }

            using (var iChildren = cluster.ChildrenW) iChildren.AddRange(RenderedItems);
        }

        /// <summary>The render items into.</summary>
        /// <param name="list">The list.</param>
        /// <param name="renderTo">The render to.</param>
        internal void RenderItemsInto(System.Collections.Generic.List<Neuron> list, NNLModuleCompiler renderTo)
        {
            if (RenderedItems == null || RenderedItems.Count == 0)
            {
                // make certain we only try to render 1 time.
                base.Render(renderTo); // this renders all the child nodes (var decls).
                RenderCode(renderTo);
            }

            list.AddRange(RenderedItems);
        }

        /// <summary>The render code.</summary>
        /// <param name="renderTo">The render to.</param>
        private void RenderCode(NNLModuleCompiler renderTo)
        {
            NeuronCluster iArgsCl = null;
            if (Params != null && Params.Count > 0)
            {
                // first thing to render is retrieving the param values from the stack.
                var iArgs = new System.Collections.Generic.List<Neuron>();
                foreach (var i in Params)
                {
                    i.Render(renderTo); // make certain that the local is rendered.
                    if (i.Item != null)
                    {
                        // there could ahve been an error in the rendering, make certain we don't get null in the iArgs list, otherwise we get errors.
                        iArgs.Add(i.Item);
                    }
                }

                iArgsCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
            }

            RenderItems(renderTo);

                // render the items before inserting the parameter init code, otherwise the list isn't created yet, and if we were to create the list before rendering the items, they wouldn't get rendered.
            if (iArgsCl != null && RenderedItems != null)
            {
                RenderedItems.Insert(0, GetStatement((ulong)PredefinedNeurons.PopValueInstruction, iArgsCl, renderTo));
            }
        }

        /// <summary>Renders a call to this function using the speceified args.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="args">The args.</param>
        /// <param name="renderingTo">The rendering To.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal Neuron RenderCall(
            NNLModuleCompiler renderTo, System.Collections.Generic.List<NNLStatementNode> args, System.Collections.Generic.List<Neuron> renderingTo = null)
        {
            if (Item == null)
            {
                Render(renderTo); // mmake certain taht there is a function to render.
            }

            System.Collections.Generic.List<Neuron> iRenderingTo;
            if (renderingTo == null)
            {
                iRenderingTo = renderTo.RenderingTo.Peek(); // we need to add multiple items in this list.
            }
            else
            {
                iRenderingTo = renderingTo;
            }

            var iNrPar = 0;
            var iReversArgs = new System.Collections.Generic.List<NNLStatementNode>(args);

                // put the arguments on the stack in reverse order, so they can be retrieved in order of decleration.
            iReversArgs.Reverse();
            foreach (var i in iReversArgs)
            {
                iNrPar += RenderParam(i, renderTo, iRenderingTo);
            }

            CheckNrOfParameters(renderTo, iNrPar, this);
            var iToCall = new System.Collections.Generic.List<Neuron>();
            renderTo.Add(Item);

                // make certain that the argument (the binding function) is also registered as being used in this module
            iToCall.Add(Item);
            renderTo.Add(Item);

                // make certain that this module references the function neuron. Important for when the function is declared in other module (like for bindings)
            var iArgs = GetParentsFor(iToCall, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
            if (ReturnValues != null && ReturnValues.Count > 0)
            {
                iRenderingTo.Add(GetStatement((ulong)PredefinedNeurons.CallInstruction, iArgs, renderTo));
                return Brain.Current[(ulong)PredefinedNeurons.ReturnValue];
            }

            return GetStatement((ulong)PredefinedNeurons.CallInstruction, iArgs, renderTo);
        }

        /// <summary>goes through the <see langword="params"/> and builds a string based on
        ///     the types (for <see langword="operator"/> overloading, so we can
        ///     easily find items again). This also includes any possible result
        ///     values.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="string"/>.</returns>
        internal string BuildNameFromParams(string name)
        {
            var iRes = new System.Text.StringBuilder(name);
            foreach (var i in Params)
            {
                iRes.Append("-");
                iRes.Append(i.TypeDecl);
            }

            if (ReturnValues != null)
            {
                foreach (var i in ReturnValues)
                {
                    iRes.Append(":");
                    iRes.Append(i.TypeDecl);
                }
            }

            return iRes.ToString();
        }

        /// <summary>write the function to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(IsInline);
            if (fParams != null)
            {
                stream.Write(fParams.Count);
                foreach (var i in fParams)
                {
                    i.Write(stream);
                }
            }
            else
            {
                stream.Write(0);
            }

            if (fReturnValues != null)
            {
                stream.Write(fReturnValues.Count);
                foreach (var i in fReturnValues)
                {
                    i.Write(stream);
                }
            }
            else
            {
                stream.Write(0);
            }
        }

        /// <summary>reads the function from a pre-compiled stream.</summary>
        /// <param name="reader"></param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            IsInline = reader.ReadBoolean();

            var iCount = reader.ReadInt32();
            if (iCount > 0)
            {
                var iParams = new System.Collections.Generic.List<NNLLocalDeclNode>();
                while (iCount > 0)
                {
                    var iLocal = new NNLLocalDeclNode();
                    iLocal.Read(reader);
                    iParams.Add(iLocal);
                    iCount--;
                }

                Params = iParams;
            }

            iCount = reader.ReadInt32();
            if (iCount > 0)
            {
                var iReturns = new System.Collections.Generic.List<NNLLocalDeclNode>();
                while (iCount > 0)
                {
                    var iLocal = new NNLLocalDeclNode();
                    iLocal.Read(reader);
                    iReturns.Add(iLocal);
                    iCount--;
                }

                ReturnValues = iReturns;
            }
        }
    }
}