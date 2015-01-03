// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLSwitchNode.cs" company="">
//   
// </copyright>
// <summary>
//   The nnl switch node.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The nnl switch node.</summary>
    internal class NNLSwitchNode : NNLNodesList
    {
        /// <summary>The f condition.</summary>
        private NNLStatementNode fCondition;

        /// <summary>Initializes a new instance of the <see cref="NNLSwitchNode"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLSwitchNode(NodeType type)
            : base(type)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NNLSwitchNode"/> class.</summary>
        public NNLSwitchNode()
            : base(NodeType.Switch)
        {
        }

        /// <summary>Gets or sets a value indicating whether is looped.</summary>
        public bool IsLooped { get; set; }

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

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iRes = NeuronFactory.Get<ConditionalStatement>();

                    // before we generate the children and try to look for an item that already exists, create a new one, so that we can handle recursion correctly.
                Brain.Current.Add(iRes);
                Item = iRes;
                renderTo.Add(this);

                Condition.Render(renderTo);
                RenderChildren(renderTo);
                RenderConditionParts(renderTo);
                var iCode = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);
                ConditionalStatement iCond = null;
                foreach (var i in iCode.FindAllIn((ulong)PredefinedNeurons.Condition))
                {
                    var iTemp = i as ConditionalStatement;
                    if (IsLooped)
                    {
                        if (iTemp != null && iTemp.LoopStyle.ID == (ulong)PredefinedNeurons.CaseLooped
                            && iTemp.CaseItem == Condition.Item)
                        {
                            iCond = iTemp;
                            break;
                        }
                    }
                    else
                    {
                        if (iTemp != null && iTemp.LoopStyle.ID == (ulong)PredefinedNeurons.Case
                            && iTemp.CaseItem == Condition.Item)
                        {
                            iCond = iTemp;
                            break;
                        }
                    }
                }

                if (iCond != null)
                {
                    renderTo.Remove(iRes);
                    Item = iCond;
                    Brain.Current.Delete(iRes);
                }
                else
                {
                    iCond = iRes;
                    if (IsLooped)
                    {
                        Link.Create(iCond, (ulong)PredefinedNeurons.CaseLooped, (ulong)PredefinedNeurons.LoopStyle);
                    }
                    else
                    {
                        Link.Create(iCond, (ulong)PredefinedNeurons.Case, (ulong)PredefinedNeurons.LoopStyle);
                    }

                    iCond.CaseItem = Condition.Item as ResultExpression;

                        // we do an 'as' cast cause it can have the value 'empyy', if something went wrong, which isn't a resultExp.
                    iCond.ConditionsCluster = iCode;
                }
            }
        }

        /// <summary><para>renders all the contitional parts. When one or more of the conditional
        ///         parts calls a function (and therefor needs to get the function
        ///         result), then we need to change the structure of the conditional and
        ///         calculate the path to take, in a seperate function, so that the
        ///         contitional parts only need to check the result of the call.</para>
        /// <para>ex: if( callxx(y) == 1) zzzz(); else if( b == callzz(y)) zzz(); else
        ///         zzz(); =&gt; call(newFunction); result = returnValue; if(result == 0)
        ///         zzz(); else if(result == 1) zzz(); else zzz();</para>
        /// <para>for a while: while( callxx(y) == 1) zzzz(); else while( b ==
        ///         callzz(y)) zzz(); else zzz(); =&gt; call(newFunction); result =
        ///         returnValue; while(result == 0){ zzz(); call(newFunction); } else
        ///         while(result == 1){ zzz(); call(newFunction); } else{ zzz();
        ///         call(newFunction); }</para>
        /// </summary>
        /// <param name="renderTo"></param>
        private void RenderConditionParts(NNLModuleCompiler renderTo)
        {
            RenderedItems = new System.Collections.Generic.List<Neuron>();
            foreach (NNLNode i in Items)
            {
                i.Render(renderTo);
                if (i.Item != null)
                {
                    // don't try to add a null, that causes problems while trying to build the lock.
                    RenderedItems.Add(i.Item);
                }

                var iCondNode = i as NNLCondPartNode;
                if (iCondNode != null && iCondNode.ExtraItems != null)
                {
                    iCondNode.Condition.LogPosError("function calls not allowed in a case conditional", renderTo);
                }

                var iCond = i.Item as ConditionalExpression;
                if (iCond == null)
                {
                    i.LogPosError("case expected", renderTo);
                }
                else if (iCond.Condition == null && i != Items[Items.Count - 1])
                {
                    // if empty condition and last in list -> problem.
                    i.LogPosError("case value expected, the default can only appear at the end", renderTo);
                }
            }
        }
    }
}