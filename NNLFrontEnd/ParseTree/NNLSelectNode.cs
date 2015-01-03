// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLSelectNode.cs" company="">
//   
// </copyright>
// <summary>
//   The nnl select node.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The nnl select node.</summary>
    internal class NNLSelectNode : NNLSwitchNode
    {
        /// <summary>Initializes a new instance of the <see cref="NNLSelectNode"/> class.</summary>
        public NNLSelectNode()
            : base(NodeType.ForEach)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NNLSelectNode"/> class.</summary>
        /// <param name="type">The type.</param>
        internal NNLSelectNode(NodeType type)
            : base(type)
        {
        }

        /// <summary>
        ///     the loopvars to use.
        /// </summary>
        public System.Collections.Generic.List<NNLStatementNode> Loopvars { get; set; }

        /// <summary>
        ///     can be: "in", "out", "child", "cluster" used to change the way that
        ///     the select runs the query: on a query or on the links, children,
        ///     parents.
        /// </summary>
        public string Modifier { get; set; }

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
                var iLoopStyle = GetLoopStyle(renderTo);

                var iLoopVars = GetLoopVars(renderTo);
                Condition.Render(renderTo);
                RenderItems(renderTo);
                RenderChildren(renderTo);

                    // this has to be done after rendering the items, otherwise any possible local decls will have rendered some code in the wrong list (they are also included as children, for the parser).
                var iCode = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);
                ConditionalExpression iExp = null;
                foreach (var i in iCode.FindAllIn((ulong)PredefinedNeurons.Statements))
                {
                    var iTemp = i as ConditionalExpression;
                    if (iTemp != null && iTemp.Condition == iLoopVars)
                    {
                        iExp = iTemp;
                        break;
                    }
                }

                if (iExp == null)
                {
                    iExp = NeuronFactory.Get<ConditionalExpression>();
                    Brain.Current.Add(iExp);
                    iExp.Condition = iLoopVars;
                    iExp.StatementsCluster = iCode;
                }

                renderTo.Add(iExp);

                ConditionalStatement iCond = null; // get or build the conditional.
                var iExpsList = new System.Collections.Generic.List<Neuron> { iExp };
                var iExps = GetParentsFor(iExpsList, (ulong)PredefinedNeurons.Code, renderTo, null);
                foreach (var i in iExps.FindAllIn((ulong)PredefinedNeurons.Condition))
                {
                    var iTemp = i as ConditionalStatement;
                    if (iTemp != null && iTemp.LoopStyle.ID == iLoopStyle && iTemp.LoopItem == Condition.Item)
                    {
                        iCond = iTemp;
                        break;
                    }
                }

                if (iCond == null)
                {
                    iCond = iStart;
                    Link.Create(iCond, iLoopStyle, (ulong)PredefinedNeurons.LoopStyle);
                    iCond.QuerySource = Condition.Item;
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

        /// <summary>The get loop style.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong GetLoopStyle(NNLModuleCompiler renderTo)
        {
            if (string.IsNullOrEmpty(Modifier))
            {
                return (ulong)PredefinedNeurons.QueryLoop;
            }

            if (Modifier == "in")
            {
                return (ulong)PredefinedNeurons.QueryLoopIn;
            }

            if (Modifier == "out")
            {
                return (ulong)PredefinedNeurons.QueryLoopOut;
            }

            if (Modifier == "child")
            {
                return (ulong)PredefinedNeurons.QueryLoopChildren;
            }

            if (Modifier == "cluster")
            {
                return (ulong)PredefinedNeurons.QueryLoopClusters;
            }

            LogPosError("Invalid loopstyle: " + Modifier, renderTo);
            return (ulong)PredefinedNeurons.QueryLoop;
        }

        /// <summary>The get loop vars.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetLoopVars(NNLModuleCompiler renderTo)
        {
            var iVars = new System.Collections.Generic.List<Neuron>();
            foreach (var i in Loopvars)
            {
                i.Render(renderTo);
                if (i.Item != null)
                {
                    iVars.Add(GetByRef(i.Item, renderTo));

                        // needs to be byref, cause we need to get the vars themselves, from a union statement, not the most optimal/fastest solution, but it works.
                }
            }

            var iArgs = GetParentsFor(iVars, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
            return GetResultStatement((ulong)PredefinedNeurons.UnionInstruction, iArgs, renderTo);
        }
    }
}