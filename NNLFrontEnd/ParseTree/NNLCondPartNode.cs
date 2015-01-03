// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLCondPartNode.cs" company="">
//   
// </copyright>
// <summary>
//   represents a single branch in a conditional.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     represents a single branch in a conditional.
    /// </summary>
    internal class NNLCondPartNode : NNLNodesList
    {
        /// <summary>The f condition.</summary>
        private NNLStatementNode fCondition;

        // List<Neuron> fExtraItems;

        /// <summary>Initializes a new instance of the <see cref="NNLCondPartNode"/> class.</summary>
        public NNLCondPartNode()
            : base(NodeType.Branch)
        {
        }

        #region Condition

        /// <summary>
        ///     Gets/sets the condition for this branch.
        /// </summary>
        public NNLStatementNode Condition
        {
            get
            {
                return fCondition;
            }

            set
            {
                fCondition = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        // #region ExtraItems

        ///// <summary>
        ///// Gets the list of items that were rendered as extras for calculating the statement. This happens when
        ///// there is a function call. In this case, we need to re-arrange the code so that everything works ok.
        ///// </summary>
        // public List<Neuron> ExtraItems
        // {
        // get { return fExtraItems; }
        // internal set { fExtraItems = value; }
        // }

        // #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iStart = NeuronFactory.Get<ConditionalExpression>();

                    // need to make certain that we can handle recursion through a reference
                Brain.Current.Add(iStart);
                Item = iStart;
                renderTo.Add(this);

                var iCond = RenderCondition(renderTo, iStart);
                RenderItems(renderTo);
                base.Render(renderTo);

                    // this has to be done after rendering the items, otherwise any possible local decls will have rendered some code in the wrong list (they are also included as children, for the parser).
                NeuronCluster iCluster;
                var iRes = TryFindCondExpression(renderTo, iCond, out iCluster);
                if (iRes == null)
                {
                    iRes = iStart;
                    iRes.StatementsCluster = iCluster;
                    iRes.Condition = iCond;
                    Item = iRes;
                    renderTo.Add(this);
                }
                else
                {
                    renderTo.Remove(iStart);
                    Item = iRes;
                    Brain.Current.Delete(iStart);
                }
            }
        }

        /// <summary>if there is 1 child in the code list, it could be the conditional part
        ///     that we refered to through it's name. In this case, we need to return
        ///     the child that was rendered and don't create a new conditional
        ///     expression. Otherwise, see if the conditional expression has already
        ///     been rendered.</summary>
        /// <param name="renderTo"></param>
        /// <param name="iCond"></param>
        /// <param name="iCluster"></param>
        /// <returns>The <see cref="ConditionalExpression"/>.</returns>
        private ConditionalExpression TryFindCondExpression(
            NNLModuleCompiler renderTo, 
            Neuron iCond, 
            out NeuronCluster iCluster)
        {
            if (RenderedItems.Count == 1 && RenderedItems[0] is ConditionalExpression
                && ((ConditionalExpression)RenderedItems[0]).Condition == iCond)
            {
                iCluster = null;
                return (ConditionalExpression)RenderedItems[0];
            }

            iCluster = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);
            ConditionalExpression iRes = null;
            foreach (var i in iCluster.FindAllIn((ulong)PredefinedNeurons.Statements))
            {
                var iTemp = i as ConditionalExpression;
                if (iTemp != null && iTemp.Condition == iCond)
                {
                    iRes = iTemp;
                    break;
                }
            }

            return iRes;
        }

        /// <summary>The render condition.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="addTo">The add to.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron RenderCondition(NNLModuleCompiler renderTo, ConditionalExpression addTo)
        {
            Neuron iCond = null;
            if (Condition != null)
            {
                var iCondItems = new System.Collections.Generic.List<Neuron>();
                renderTo.RenderingTo.Push(iCondItems);

                    // we add a temp list, which we can store seperatly so that the conditional statement can see we have a function call here, so it can re-arrange the code.
                renderTo.IsRenderingCondPart = true;
                try
                {
                    Condition.Render(renderTo);
                    iCond = Condition.Item;
                    addTo.Condition = iCond;

                        // need to assign this before rendering the code items of the part, caus one of hte children could refernce this conditional, in which case the condition itself has to be known cause the sub switch performs a check to see if only the last part has an empty condition. If this cond isn't filled in yet, it produces an unwanted error.
                }
                finally
                {
                    renderTo.IsRenderingCondPart = false;
                    renderTo.RenderingTo.Pop();
                    if (iCondItems.Count > 0)
                    {
                        // could be that the condition was a simple bool or statement that didn't catch it's own extra items.
                        ExtraItems = iCondItems;
                    }
                    else if (Condition.ExtraItems != null && Condition.ExtraItems.Count > 0)
                    {
                        ExtraItems = Condition.ExtraItems;
                    }
                }
            }

            return iCond;
        }
    }
}