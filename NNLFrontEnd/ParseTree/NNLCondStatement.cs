// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLCondStatement.cs" company="">
//   
// </copyright>
// <summary>
//   The nnl cond statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The nnl cond statement.</summary>
    internal class NNLCondStatement : NNLNodesList
    {
        /// <summary>The f case item.</summary>
        private Neuron fCaseItem;

                       // when we convert to case, we need to have a case item to evaluate (usually the 'return value')

        /// <summary>Initializes a new instance of the <see cref="NNLCondStatement"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLCondStatement(NodeType type)
            : base(type)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NNLCondStatement"/> class. 
        ///     default constructor for reading.</summary>
        public NNLCondStatement()
            : base(NodeType.Do)
        {
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iStart = NeuronFactory.Get<ConditionalStatement>();

                    // make an initial neuron, so we can properly build recursion.
                Brain.Current.Add(iStart);
                Item = iStart;

                ulong iCondType = 0;
                base.Render(renderTo);
                switch (Type)
                {
                    case NodeType.If:
                        iCondType = (ulong)PredefinedNeurons.Normal;
                        break;
                    case NodeType.Do:
                        iCondType = (ulong)PredefinedNeurons.Until;
                        break;
                    case NodeType.While:
                        iCondType = (ulong)PredefinedNeurons.Looped;
                        break;
                    case NodeType.Switch:
                        iCondType = (ulong)PredefinedNeurons.Case;
                        break;
                    case NodeType.LoopedSwitch:
                        iCondType = (ulong)PredefinedNeurons.CaseLooped;
                        break;
                    default:
                        LogPosError("internal error: unknown nodes-list type", renderTo);
                        break;
                }

                iCondType = RenderConditionParts(renderTo, iCondType);

                    // don't render conditional parts with RenderItems, but with a custom function, so that we can check if there were any function calls, which render multiple lines (for the return values)
                var iParts = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);
                NeuronCluster iExtraItems = null;
                if (ExtraItems != null)
                {
                    iExtraItems = GetParentsFor(ExtraItems, (ulong)PredefinedNeurons.Code, renderTo, string.Empty);
                    ExtraItems = null;

                        // need to reset the extra items, cause they have been used by the conditonal statement itself, they don't need to be rendered just before the conditional.
                }

                Item = GetCondStatement(renderTo, iStart, iCondType, iParts, iExtraItems);
            }
        }

        /// <summary>The get cond statement.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iStart">The i start.</param>
        /// <param name="iCondType">The i cond type.</param>
        /// <param name="iParts">The i parts.</param>
        /// <param name="iExtraItems">The i extra items.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetCondStatement(
            NNLModuleCompiler renderTo, 
            ConditionalStatement iStart, 
            ulong iCondType, 
            NeuronCluster iParts, 
            NeuronCluster iExtraItems)
        {
            ConditionalStatement iRes = null;
            foreach (var i in iParts.FindAllIn((ulong)PredefinedNeurons.Condition))
            {
                var iTemp = i as ConditionalStatement;
                if (iTemp != null && iTemp.LoopStyle.ID == iCondType)
                {
                    var iOtherExtraItems = iTemp.FindFirstOut((ulong)PredefinedNeurons.Statements);
                    var iOtherCaseItem = iTemp.FindFirstOut((ulong)PredefinedNeurons.CaseItem);
                    if (iExtraItems == iOtherExtraItems && fCaseItem == iOtherCaseItem)
                    {
                        iRes = iTemp;
                        break;
                    }
                }
            }

            if (iRes == null)
            {
                renderTo.Add(this);
                iRes = iStart;
                Link.Create(iRes, iCondType, (ulong)PredefinedNeurons.LoopStyle);
                Link.Create(iRes, iParts, (ulong)PredefinedNeurons.Condition);
                if (iExtraItems != null)
                {
                    Link.Create(iRes, iExtraItems, (ulong)PredefinedNeurons.Statements);
                }

                if (fCaseItem != null)
                {
                    Link.Create(iRes, fCaseItem, (ulong)PredefinedNeurons.CaseItem);
                }
            }
            else
            {
                renderTo.Remove(iStart);
                Brain.Current.Delete(iStart);
                renderTo.AddExternal(iRes); // we need to know 
            }

            return iRes;
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
        /// <param name="currentType">The current Type.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong RenderConditionParts(NNLModuleCompiler renderTo, ulong currentType)
        {
            RenderedItems = new System.Collections.Generic.List<Neuron>();
            var iHasFunctionCalls = false;
            var iNrOfConds = GetNrOfParts();

                // keeps track of the nr of conditional parts that have a conditional expression.
            var iData = new NNLModuleCompiler.ConditionalRenderData();

                // so that the boolean expressions can render and/or constructs correctly.
            renderTo.CondPartIndex.Push(iData);
            try
            {
                iData.TotalNrOfCondParts = iNrOfConds;
                foreach (NNLCondPartNode i in Items)
                {
                    iData.CondPartIndex++;
                    i.Render(renderTo);
                    if (i.Item != null)
                    {
                        RenderedItems.Add(i.Item);
                    }

                    iHasFunctionCalls |= i.ExtraItems != null; // calculate if any of the items has a function call.
                    var iCond = i.Item as ConditionalExpression;
                    if (iCond == null)
                    {
                        i.LogPosError("conditional part expected", renderTo);
                    }
                    else if (iCond.Condition == null && i != Items[Items.Count - 1])
                    {
                        // if empty condition and last in list -> problem.
                        i.LogPosError("'else' without condition in middle of conditional", renderTo);
                    }
                }
            }
            finally
            {
                renderTo.CondPartIndex.Pop(); // need to remove
            }

            if (iHasFunctionCalls)
            {
                // there was a function call, adjust the code so that there is 1 function to calculate which path to take. When this is a loop, each path needs to recalculate the value at the end of the path + the condtional always becomes a case (either looped or not).
                var iRenderingTo = renderTo.RenderingTo.Peek();
                var iCalcFunction = new System.Collections.Generic.List<Neuron>();
                for (var i = 0; i < Items.Count; i++)
                {
                    var iPart = Items[i] as NNLCondPartNode;
                    if (iPart != null)
                    {
                        if (iPart.ExtraItems != null)
                        {
                            iCalcFunction.AddRange(iPart.ExtraItems);
                            if (iNrOfConds > 1)
                            {
                                // if there is more than 1 conditional part, need to render a return that makes certain we don't calculate to much.
                                ConvertCondPartToIfForSub(renderTo, iPart, i, iCalcFunction);
                            }
                        }
                        else if (iPart.Condition != null)
                        {
                            // the last item can possibly have no condition, in that case, don't need to render an extra if for this.
                            ConvertCondPartToIfForSub(renderTo, iPart, i, iCalcFunction);
                        }
                    }
                    else
                    {
                        LogPosError("invalid conditional part found", renderTo);
                    }
                }

                ExtraItems = iCalcFunction;
                if (iNrOfConds > 1)
                {
                    // if there is only 1 conditional part and an empty one, then we don't need to change into a case.
                    if (Type == NodeType.Do)
                    {
                        // there were functioncalls, so the conditional needs to be changed to a case.
                        currentType = (ulong)PredefinedNeurons.CaseLooped;
                        fCaseItem = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];
                    }
                    else if (Type == NodeType.If)
                    {
                        currentType = (ulong)PredefinedNeurons.Case;
                        fCaseItem = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];
                    }
                }
            }

            return currentType;
        }

        /// <summary>calculates how many conditional parts there are in the conditional. If
        ///     the last one has no conditional, it doesn't count.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetNrOfParts()
        {
            var iLast = (NNLCondPartNode)Items[Items.Count - 1];
            if (iLast.Condition == null)
            {
                return Items.Count - 1;
            }

            return Items.Count;
        }

        /// <summary>creates an if statement with a return value for the conditional part.
        ///     if the if statement tests true, an <see langword="int"/> neuron will
        ///     be rendered as result value, which will be used as condition for the
        ///     conditional <paramref name="part"/> (the conditional itself becomes a
        ///     (looped)case )</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="part">The part.</param>
        /// <param name="index">The index.</param>
        /// <param name="addTo">The add To.</param>
        private static void ConvertCondPartToIfForSub(
            NNLModuleCompiler renderTo, 
            NNLCondPartNode part, 
            int index, System.Collections.Generic.List<Neuron> addTo)
        {
            var iExp = part.Item as ConditionalExpression;
            if (!(iExp.Condition is IntNeuron))
            {
                // if it already was an int, it was an and/or bool conditinal which generated it's own returns, so nothing else needs to be done.
                var iNewCond = renderTo.GetInt(index);
                var iOldCond = iExp.Condition;
                if (BrainHelper.HasIncommingReferences(iExp))
                {
                    // if it isn't used by another code bit, simply add the 
                    var iNew = NeuronFactory.Get<ConditionalExpression>();
                    Brain.Current.Add(iNew);
                    Link.Create(iNew, iNewCond, (ulong)PredefinedNeurons.Condition);
                    Link.Create(iNew, iExp.StatementsCluster, (ulong)PredefinedNeurons.Statements);
                    renderTo.Add(iNew);
                }
                else
                {
                    iExp.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Condition, iNewCond);
                }

                addTo.Add(GetConditionalReturn(iOldCond, renderTo, iNewCond));
            }
        }

        /// <summary>Reads the specified reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            Type = (NodeType)reader.ReadInt16();
        }

        /// <summary>write the item to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write((System.Int16)Type);
        }
    }
}