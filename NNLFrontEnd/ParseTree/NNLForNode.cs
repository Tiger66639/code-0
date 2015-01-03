// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLForNode.cs" company="">
//   
// </copyright>
// <summary>
//   ex: for(int i = count(y); i &gt; 0; i++)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     ex: for(int i = count(y); i > 0; i++)
    /// </summary>
    internal class NNLForNode : NNLForEachNode
    {
        /// <summary>The f incrementor.</summary>
        private NNLStatementNode fIncrementor;

        /// <summary>Initializes a new instance of the <see cref="NNLForNode"/> class.</summary>
        public NNLForNode()
            : base(NodeType.For)
        {
        }

        #region Incrmentor

        /// <summary>
        ///     Gets/sets the code bit that is responsible for incrementing the
        ///     variable.
        /// </summary>
        public NNLStatementNode Incrementor
        {
            get
            {
                return fIncrementor;
            }

            set
            {
                fIncrementor = value;
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
                var iDecl = LoopVar as NNLLocalDeclNode;
                if (iDecl == null)
                {
                    Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                        // we assign a neuron to prevent that the error gets rendered to many times.
                    LogPosError("Var decleration expected as first part of for loop.", renderTo);
                }
                else
                {
                    var iStart = NeuronFactory.Get<ConditionalStatement>();

                        // need an Item before rendering the children, for recursion.
                    Brain.Current.Add(iStart);
                    Item = iStart;
                    renderTo.Add(this);

                    LoopVar.Render(renderTo);

                        // generate the loop var. The parser has already made certain that the init is performed before the loop
                    var iCondExtras = BuildCondition(renderTo);
                    RenderItems(renderTo);
                    RenderChildren(renderTo);

                        // this has to be done after rendering the items, otherwise any possible local decls will have rendered some code in the wrong list (they are also included as children, for the parser).
                    HandleIncrementor(renderTo);
                    var iCode = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, Name);
                    var iExp = GetCondPart(renderTo, iCode);

                    Item = GetForLoop(renderTo, iExp, iCode, iCondExtras); // get or build the conditional.
                }
            }
        }

        /// <summary>The get for loop.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iExp">The i exp.</param>
        /// <param name="iCode">The i code.</param>
        /// <param name="iCondExtras">The i cond extras.</param>
        /// <returns>The <see cref="ConditionalStatement"/>.</returns>
        private ConditionalStatement GetForLoop(
            NNLModuleCompiler renderTo, 
            ConditionalExpression iExp, 
            NeuronCluster iCode, 
            NeuronCluster iCondExtras)
        {
            ConditionalStatement iCond = null;
            var iExpsList = new System.Collections.Generic.List<Neuron> { iExp };
            var iExps = GetParentsFor(iExpsList, (ulong)PredefinedNeurons.Code, renderTo, Name);
            foreach (var i in iExps.FindAllIn((ulong)PredefinedNeurons.Condition))
            {
                var iTemp = i as ConditionalStatement;
                if (iTemp != null && iTemp.LoopStyle.ID == (ulong)PredefinedNeurons.Looped)
                {
                    if (iCondExtras == null || Link.Exists(iTemp, iCondExtras, (ulong)PredefinedNeurons.Statements))
                    {
                        iCond = iTemp;
                        break;
                    }
                }
            }

            if (iCond == null)
            {
                iCond = (ConditionalStatement)Item;

                    // item was already build so that there was a default value during rendering (a child might be a reg to a parent)
                Link.Create(iCond, (ulong)PredefinedNeurons.Looped, (ulong)PredefinedNeurons.LoopStyle);
                if (iCondExtras != null)
                {
                    Link.Create(iCond, iCondExtras, (ulong)PredefinedNeurons.Statements);
                }

                iCond.ConditionsCluster = iExps;
            }
            else
            {
                renderTo.Remove(Item); // found a better candidate, delete the temp object, which was stored in Item.
                Brain.Current.Delete(Item);
            }

            return iCond;
        }

        /// <summary>The build condition.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster BuildCondition(NNLModuleCompiler renderTo)
        {
            Condition.Render(renderTo);
            if (Condition.ExtraItems != null && Condition.ExtraItems.Count > 0)
            {
                return GetParentsFor(Condition.ExtraItems, (ulong)PredefinedNeurons.Code, renderTo, string.Empty);
            }

            return null;
        }

        /// <summary>The get cond part.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iCode">The i code.</param>
        /// <returns>The <see cref="ConditionalExpression"/>.</returns>
        private ConditionalExpression GetCondPart(NNLModuleCompiler renderTo, NeuronCluster iCode)
        {
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
                renderTo.Add(iExp);
            }

            return iExp;
        }

        /// <summary>The handle incrementor.</summary>
        /// <param name="renderTo">The render to.</param>
        private void HandleIncrementor(NNLModuleCompiler renderTo)
        {
            Incrementor.Render(renderTo);

                // has to be done after rendering the items, cause it could add some stuff at the end of the list.
            var iBin = Incrementor as NNLBinaryStatement;
            if (iBin != null && iBin.ExtraItems != null)
            {
                // if the incrementor contained a function all, do that before adding the actual incementor.
                RenderedItems.AddRange(iBin.ExtraItems);
            }

            RenderedItems.Add(Incrementor.Item);
        }
    }
}