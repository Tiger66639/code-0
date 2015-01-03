// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLLockNode.cs" company="">
//   
// </copyright>
// <summary>
//   for lock statements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     for lock statements.
    /// </summary>
    internal class NNLLockNode : NNLNodesList
    {
        /// <summary>The f link locks.</summary>
        private System.Collections.Generic.List<NNLStatementNode> fLinkLocks;

        /// <summary>The f neuron locks.</summary>
        private System.Collections.Generic.List<NNLStatementNode> fNeuronLocks;

        /// <summary>Initializes a new instance of the <see cref="NNLLockNode"/> class.</summary>
        public NNLLockNode()
            : base(NodeType.Statement)
        {
        }

        #region NeuronLocks

        /// <summary>
        ///     Gets the list of neurons to lock
        /// </summary>
        public System.Collections.Generic.List<NNLStatementNode> NeuronLocks
        {
            get
            {
                return fNeuronLocks;
            }

            set
            {
                fNeuronLocks = value;
                foreach (var i in value)
                {
                    i.Parent = this; // need to build the tree as well.
                }
            }
        }

        #endregion

        #region LinkLocks

        /// <summary>
        ///     Gets the list of links (2 neurons per link) that need to be locked.
        /// </summary>
        public System.Collections.Generic.List<NNLStatementNode> LinkLocks
        {
            get
            {
                return fLinkLocks;
            }

            set
            {
                fLinkLocks = value;
                foreach (var i in value)
                {
                    i.Parent = this; // need to build the tree as well.
                }
            }
        }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                Item = NeuronFactory.Get<LockExpression>();

                    // in case that this lock gets referenced in one of the child code items.
                Brain.Current.MakeTemp(Item);
                RenderItems(renderTo);

                    // needs to be done before rendering any children, the childrenlist can contains var decls used in the code, if they are rendered first as children instead of code, they will render some extra bits in the incorrect lists.
                base.Render(renderTo);
                var iNeurons = new System.Collections.Generic.List<Neuron>();
                var iLinks = new System.Collections.Generic.List<Neuron>();
                NeuronCluster iNeuronsCl = null;
                NeuronCluster iLinksCl = null;
                var iCode = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, string.Empty);

                if (NeuronLocks != null)
                {
                    foreach (var i in NeuronLocks)
                    {
                        i.Render(renderTo);
                        if (i.Item != null)
                        {
                            iNeurons.Add(i.Item);
                        }
                    }

                    if (iNeurons.Count > 0)
                    {
                        iNeuronsCl = GetParentsFor(iNeurons, (ulong)PredefinedNeurons.Code, renderTo, Name);
                    }
                }

                if (LinkLocks != null)
                {
                    foreach (var i in LinkLocks)
                    {
                        i.Render(renderTo);
                        if (i.Item != null)
                        {
                            iLinks.Add(i.Item);
                        }
                    }

                    if (iLinks.Count > 0)
                    {
                        iLinksCl = GetParentsFor(iLinks, (ulong)PredefinedNeurons.Code, renderTo, Name);
                    }
                }

                var iLocks = iCode.FindAllIn((ulong)PredefinedNeurons.Statements);
                foreach (var i in iLocks)
                {
                    var iTemp = i as LockExpression;
                    if (iTemp != null && iTemp.NeuronsToLockCluster == iNeuronsCl
                        && iTemp.LinksToLockCluster == iLinksCl)
                    {
                        Item = iTemp;
                        break;
                    }
                }

                if (Item.ID == Neuron.TempId)
                {
                    var iTemp = (LockExpression)Item;
                    Brain.Current.Add(iTemp);
                    iTemp.StatementsCluster = iCode;
                    iTemp.NeuronsToLockCluster = iNeuronsCl;
                    iTemp.LinksToLockCluster = iLinksCl;
                }

                renderTo.Add(this);
            }
        }
    }
}