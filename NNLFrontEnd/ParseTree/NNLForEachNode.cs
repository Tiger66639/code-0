// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLForEachNode.cs" company="">
//   
// </copyright>
// <summary>
//   for <see langword="foreach" /> loops
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     for <see langword="foreach" /> loops
    /// </summary>
    internal class NNLForEachNode : NNLSwitchNode
    {
        /// <summary>The f loop var.</summary>
        private NNLStatementNode fLoopVar;

        /// <summary>Initializes a new instance of the <see cref="NNLForEachNode"/> class.</summary>
        public NNLForEachNode()
            : base(NodeType.ForEach)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NNLForEachNode"/> class.</summary>
        /// <param name="type">The type.</param>
        internal NNLForEachNode(NodeType type)
            : base(type)
        {
        }

        #region Condition

        /// <summary>
        ///     Gets/sets the condition for this branch.
        /// </summary>
        public NNLStatementNode LoopVar
        {
            get
            {
                return fLoopVar;
            }

            set
            {
                fLoopVar = value;
                if (value != null)
                {
                    value.Parent = this;
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
                var iStart = NeuronFactory.Get<ConditionalStatement>();
                Brain.Current.Add(iStart);
                Item = iStart;
                renderTo.Add(this);

                LoopVar.Render(renderTo);
                Condition.Render(renderTo);
                RenderItems(renderTo);
                RenderChildren(renderTo);

                    // this has to be done after rendering the items, otherwise any possible local decls will have rendered some code in the wrong list (they are also included as children, for the parser).
                var iCode = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);
                ConditionalExpression iExp = null;
                foreach (var i in iCode.FindAllIn((ulong)PredefinedNeurons.Statements))
                {
                    var iTemp = i as ConditionalExpression;
                    if (iTemp != null && iTemp.Condition == Condition.Item)
                    {
                        iExp = iTemp;
                        break;
                    }
                }

                if (iExp == null)
                {
                    iExp = NeuronFactory.Get<ConditionalExpression>();
                    Brain.Current.Add(iExp);
                    iExp.Condition = Condition.Item;
                    iExp.StatementsCluster = iCode;
                }

                renderTo.Add(iExp);

                ConditionalStatement iCond = null; // get or build the conditional.
                var iExpsList = new System.Collections.Generic.List<Neuron> { iExp };
                var iExps = GetParentsFor(iExpsList, (ulong)PredefinedNeurons.Code, renderTo, null);
                foreach (var i in iExps.FindAllIn((ulong)PredefinedNeurons.Condition))
                {
                    var iTemp = i as ConditionalStatement;
                    if (iTemp != null && iTemp.LoopStyle.ID == (ulong)PredefinedNeurons.ForEach
                        && iTemp.LoopItem == fLoopVar.Item)
                    {
                        iCond = iTemp;
                        break;
                    }
                }

                if (iCond == null)
                {
                    iCond = iStart;
                    Link.Create(iCond, (ulong)PredefinedNeurons.ForEach, (ulong)PredefinedNeurons.LoopStyle);
                    iCond.LoopItem = (Variable)fLoopVar.Item;
                    iCond.ConditionsCluster = iExps;
                }
                else
                {
                    Item = iCond;
                    renderTo.Remove(iStart);
                    Brain.Current.Delete(iStart);
                }
            }
        }
    }
}