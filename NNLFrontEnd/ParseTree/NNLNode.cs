// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLNode.cs" company="">
//   
// </copyright>
// <summary>
//   defines the type of node
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     defines the type of node
    /// </summary>
    internal enum NodeType
    {
        /// <summary>The class.</summary>
        Class, 

        /// <summary>The neuron.</summary>
        Neuron, 

        /// <summary>The bind.</summary>
        Bind, 

        /// <summary>The var.</summary>
        Var, 

        /// <summary>The thes.</summary>
        Thes, 

        /// <summary>The asset.</summary>
        Asset, 

        /// <summary>The assign.</summary>
        Assign, 

        /// <summary>The statement.</summary>
        Statement, 

        /// <summary>The result statement.</summary>
        ResultStatement, 

        /// <summary>The if.</summary>
        If, 

        /// <summary>The do.</summary>
        Do, 

        /// <summary>The while.</summary>
        While, 

        /// <summary>The for each.</summary>
        ForEach, 

        /// <summary>The switch.</summary>
        Switch, 

        /// <summary>The looped switch.</summary>
        LoopedSwitch, 

        /// <summary>The branch.</summary>
        Branch, 

        /// <summary>The code list.</summary>
        CodeList, 

        /// <summary>The union.</summary>
        Union, 

        /// <summary>The path.</summary>
        Path, // assetpaths, thes-paths,...

        /// <summary>The index.</summary>
        Index, // ex: var1[0]

        /// <summary>The for.</summary>
        For, 

        /// <summary>The link.</summary>
        Link
    }

    /// <summary>
    ///     represents a parse node of the 1st stage.
    /// </summary>
    internal class NNLNode : NNLStatementNode
    {
        /// <summary>The f functions.</summary>
        private System.Collections.Generic.List<NNLFunctionNode> fFunctions;

        /// <summary>The f links.</summary>
        private System.Collections.Generic.List<NNLLinkNode> fLinks;

        /// <summary>The f bindings.</summary>
        private readonly System.Collections.Generic.Dictionary<Token, NNLBinding> fBindings =
            new System.Collections.Generic.Dictionary<Token, NNLBinding>();

        /// <summary>The f children.</summary>
        private readonly System.Collections.Generic.Dictionary<string, NNLStatementNode> fChildren =
            new System.Collections.Generic.Dictionary<string, NNLStatementNode>();

        /// <summary>The f usings.</summary>
        private readonly System.Collections.Generic.List<NNLPathNode> fUsings =
            new System.Collections.Generic.List<NNLPathNode>();

        /// <summary>Initializes a new instance of the <see cref="NNLNode"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLNode(NodeType type)
            : base(type)
        {
        }

        #region Children

        /// <summary>
        ///     Gets the list of children of this node.
        /// </summary>
        public virtual System.Collections.Generic.Dictionary<string, NNLStatementNode> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion

        #region Links

        /// <summary>
        ///     Gets the list of links that need to be created for this object.
        /// </summary>
        public System.Collections.Generic.List<NNLLinkNode> Links
        {
            get
            {
                if (fLinks == null)
                {
                    fLinks = new System.Collections.Generic.List<NNLLinkNode>();
                }

                return fLinks;
            }
        }

        #endregion

        #region ExternalDef

        /// <summary>
        ///     Gets/sets the data needed to bind this item to an external function.
        /// </summary>
        public string ExternalDef { get; set; }

        #endregion

        #region ModuleProperty

        /// <summary>
        ///     Gets the data needed to register this neuron as a module property.
        /// </summary>
        public string ModuleProperty { get; internal set; }

        #endregion

        /// <summary>checks if the specified <paramref name="name"/> is declared in one of
        ///     the parent nodes. Warning: this is recursive.</summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsDefinedInParent(string name)
        {
            var iParent = Parent as NNLNode;
            if (iParent != null && iParent.Parent != null)
            {
                if (iParent.Children.ContainsKey(name))
                {
                    return true;
                }

                return ((NNLNode)Parent).IsDefinedInParent(name);
            }

            return false;
        }

        /// <summary>Looks for a neuron in the network with the specified id (if defined)
        ///     or in the module-compiler, with the specified name (in case it is a
        ///     re-compile, and the neurons still existed).</summary>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron FindObject(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                if (ExternalDef != null)
                {
                    Item = renderTo.GetLibRef(ExternalDef, this);
                    if (Item == null)
                    {
                        LogPosError(string.Format("Can't find mapping to {0}.", ExternalDef), renderTo);
                    }
                }
                else if (ID > Neuron.EmptyId)
                {
                    Item = Brain.Current[ID];
                    renderTo.AddExternal(Item);
                }
            }

            return Item;
        }

        /// <summary>The render functions.</summary>
        /// <param name="renderTo">The render to.</param>
        protected void RenderFunctions(NNLModuleCompiler renderTo)
        {
            foreach (var i in Functions)
            {
                if (i.Item == null)
                {
                    i.Render(renderTo);
                }

                if (Item != null)
                {
                    // bindings aren't represented by real functions at the moment
                    var iMeaning = i.FirstNodeParent.Find(i.Name, renderTo);

                        // the linkmeaning. needs to come from a parent, otherwise we get the functioncluster itself.
                    if (iMeaning != null && i.Item != null && Link.Exists(Item, i.Item, iMeaning.ID) == false)
                    {
                        // only try to create the link if everything worked ok and the link hasn't been created already.
                        Link.Create(Item, i.Item, iMeaning);
                    }
                }
            }
        }

        /// <summary>renders all the links</summary>
        /// <param name="renderTo"></param>
        protected void RenderLinks(NNLModuleCompiler renderTo)
        {
            if (fLinks != null)
            {
                foreach (var i in Links)
                {
                    i.Render(renderTo);
                    var iMeaning = i.FirstNodeParent.Find(i.Name, renderTo);

                        // the linkmeaning. needs to come from a parent, otherwise we get the functioncluster itself.
                    if (iMeaning != null && i.To.Item != null && Link.Exists(Item, i.To.Item, iMeaning.ID) == false)
                    {
                        // only try to create the link if everything worked ok and the link hasn't been created already.
                        Link.Create(Item, i.To.Item, iMeaning);
                    }
                    else if (iMeaning == null)
                    {
                        LogPosError("failed to create link: " + i.Name + " not found", renderTo);
                    }
                    else if (i.To.Item == null)
                    {
                        LogPosError("failed to create link: " + i.To + " not found", renderTo);
                    }
                }
            }
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            RenderChildren(renderTo);
            ProcessAttributes(renderTo);
        }

        /// <summary>checks some of the attributes assigned to this object and further
        ///     processes them.</summary>
        /// <param name="renderTo">The render To.</param>
        internal void ProcessAttributes(NNLModuleCompiler renderTo)
        {
            if (ModuleProperty != null)
            {
                NNLModuleCompiler.NetworkDict.SetModuleProp(Item, renderTo.Module.ID.ToString(), ModuleProperty);
            }
        }

        /// <summary>creates a duplicate of the children list before rendering. This is
        ///     used to render the data from the root object, so taht we can still add
        ///     things during rendering to this list.</summary>
        /// <param name="renderTo">The render To.</param>
        internal void RenderFromDupList(NNLModuleCompiler renderTo)
        {
            foreach (var i in Enumerable.ToArray(Children))
            {
                i.Value.Render(renderTo);
            }
        }

        /// <summary>renders the list of children.</summary>
        /// <param name="renderTo"></param>
        protected void RenderChildren(NNLModuleCompiler renderTo)
        {
            foreach (var i in Children)
            {
                i.Value.Render(renderTo);
            }
        }

        /// <summary>looks for a common parent cluster with the specified<paramref name="meaning"/> that contains the list of children, nothing
        ///     more or less.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected internal NeuronCluster GetParentForChildren(ulong meaning, NNLModuleCompiler renderTo)
        {
            var iChildren = (from i in Children select i.Value.Item).ToList();
            return GetParentsFor(iChildren, meaning, renderTo, Name);
        }

        /// <summary>tries to find an object in the dictionaries with the specified name.
        ///     makes certain it is compiled and returns the neuron.</summary>
        /// <param name="p"></param>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal Neuron Find(string p, NNLModuleCompiler renderTo)
        {
            NNLStatementNode iReffered = null;
            var iParent = FirstNodeParent;
            var iFound = false;
            while (iParent != null && iFound == false)
            {
                iFound = iParent.Children.TryGetValue(p, out iReffered);
                if (iFound == false)
                {
                    iParent = iParent.FirstNodeParent;
                }
            }

            if (iFound && iReffered != null)
            {
                iReffered.Render(renderTo);
                return iReffered.Item;
            }

            return null;
        }

        /// <summary>write the item to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(fChildren.Count);
            foreach (var i in fChildren)
            {
                stream.Write(i.Value.GetType().ToString()); // so we know which type of bind item.
                stream.Write(i.Key);
                i.Value.Write(stream);
            }
        }

        /// <summary>The read.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            var iNrChildren = reader.ReadInt32();
            while (iNrChildren > 0)
            {
                var iType = reader.ReadString();
                var iNew = (NNLStatementNode)System.Activator.CreateInstance(System.Type.GetType(iType), true);
                var iName = reader.ReadString();
                iNew.Read(reader);
                fChildren.Add(iName, iNew);
                iNrChildren--;
            }
        }

        #region Usings

        /// <summary>
        ///     Gets the list of usings defined in this node.
        /// </summary>
        public System.Collections.Generic.List<NNLPathNode> Usings
        {
            get
            {
                return fUsings;
            }
        }

        /// <summary>returns the list of all usings, including those of all the owners.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<NNLPathNode> AllUsings()
        {
            var iCurrent = this;
            while (iCurrent != null)
            {
                foreach (var i in iCurrent.Usings)
                {
                    yield return i;
                }

                var iParent = iCurrent.Parent;
                if (iParent is NNLNode)
                {
                    iCurrent = (NNLNode)iParent;

                        // a PathNode is an NNLNode, but can be owned by other statement nodes, so watch out for this.
                }
                else if (iParent != null)
                {
                    iCurrent = iParent.FirstNodeParent;
                }
                else
                {
                    iCurrent = null;
                }
            }
        }

        #endregion

        #region Bindings

        /// <summary>
        ///     all the bindings that are declared in this node.
        /// </summary>
        public System.Collections.Generic.Dictionary<Token, NNLBinding> Bindings
        {
            get
            {
                return fBindings;
            }
        }

        /// <summary>Gets the binding for the specified token.</summary>
        /// <param name="id">The id.</param>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="NNLBinding"/>.</returns>
        public NNLBinding GetBindingFor(Token id, NNLModuleCompiler renderTo)
        {
            var iPrev = new System.Collections.Generic.HashSet<NNLNode>();
            return InternalGetBindingFor(id, renderTo, iPrev);
        }

        /// <summary>The internal get binding for.</summary>
        /// <param name="id">The id.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="NNLBinding"/>.</returns>
        private NNLBinding InternalGetBindingFor(
            Token id, 
            NNLModuleCompiler renderTo, System.Collections.Generic.HashSet<NNLNode> prev)
        {
            prev.Add(this);
            NNLBinding iRes;
            if (Bindings.TryGetValue(id, out iRes) == false)
            {
                foreach (var i in Usings)
                {
                    // check all the usings declared at this level, perhaps they have a binding.
                    if (prev.Contains(i) == false)
                    {
                        // dont want to redo any of the previously processed items -> ethernal loop
                        var iUsing = i.GetPathTerminus(renderTo) as NNLNode;

                            // get the nod to which the using clause is pointing.
                        iRes = iUsing.InternalGetBindingFor(id, renderTo, prev);
                        if (iRes != null)
                        {
                            return iRes;
                        }
                    }
                }

                var iParent = FirstNodeParent;
                if (iParent != null && prev.Contains(iParent) == false)
                {
                    return iParent.InternalGetBindingFor(id, renderTo, prev);
                }

                if (renderTo.RegisteredBindings != null && renderTo.RegisteredBindings.TryGetValue(id, out iRes))
                {
                    return iRes;
                }

                return null;
            }

            return iRes;
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Gets the list of function that were declared for this node. this is
        ///     used to render the links. The name is actually the name of the
        ///     link-meaning
        /// </summary>
        public System.Collections.Generic.List<NNLFunctionNode> Functions
        {
            get
            {
                if (fFunctions == null)
                {
                    fFunctions = new System.Collections.Generic.List<NNLFunctionNode>();
                }

                return fFunctions;
            }
        }

        /// <summary>
        ///     gets if this node has any functions.
        /// </summary>
        public bool HasFunctions
        {
            get
            {
                return fFunctions != null && fFunctions.Count > 0;
            }
        }

        #endregion
    }
}