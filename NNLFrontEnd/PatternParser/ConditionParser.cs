// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionParser.cs" company="">
//   
// </copyright>
// <summary>
//   The condition parser.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The condition parser.</summary>
    public class ConditionParser : OutputParser
    {
        /// <summary>The containsoperator.</summary>
        private const string CONTAINSOPERATOR = "contains";

        // <summary>
        /// <summary>Initializes a new instance of the <see cref="ConditionParser"/> class.</summary>
        /// <param name="toParse">The to parse.</param>
        /// <param name="source">The source.</param>
        public ConditionParser(TextNeuron toParse, string source)
            : base(toParse)
        {
            ParserTitle = source + ".Condition";
        }

        /// <summary>
        ///     Parses the conditional text.
        /// </summary>
        public override void Parse()
        {
            RemoveCondPattern(ToParse);
            Neuron iExp;
            if (Scanner.CurrentToken != Token.End)
            {
                if (ExpressionsHandler == null)
                {
                    iExp = ReadBool();
                }
                else
                {
                    ExpressionsHandler.Clear();
                    iExp = ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner, false);
                    if (ExpressionsHandler.Errors.Count > 0)
                    {
                        // the compiler doesn't generate an exception for the errors normally
                        throw new System.InvalidOperationException(string.Join("\n", ExpressionsHandler.Errors));
                    }
                }

                if (iExp != null)
                {
                    Link.Create(ToParse, iExp, (ulong)PredefinedNeurons.BoolExpression);
                }
            }
        }

        /// <summary>The read bool.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadBool()
        {
            var iRes = ReadBoolPart();
            try
            {
                while (Scanner.CurrentToken != Token.End)
                {
                    var iBool = NeuronFactory.Get<BoolExpression>();
                    Brain.Current.Add(iBool);
                    Link.Create(iBool, iRes, (ulong)PredefinedNeurons.LeftPart);
                    ReadLogicalOperator(iBool);
                    AdvanceWithSpaceSkip();
                    if (Scanner.CurrentToken != Token.End)
                    {
                        var iRight = ReadBoolPart();
                        Link.Create(iBool, iRight, (ulong)PredefinedNeurons.RightPart);
                    }
                    else
                    {
                        LogPosError("The right part of the condition is missing.");
                    }

                    iRes = iBool;
                }

                return iRes;
            }
            catch
            {
                RemoveCondPattern(ToParse);
                if (iRes is BoolExpression)
                {
                    DeleteBool((BoolExpression)iRes);
                }
                else if (iRes is NeuronCluster)
                {
                    DeleteOutputList((NeuronCluster)iRes);
                }

                throw;
            }
        }

        /// <summary>The read bool part.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadBoolPart()
        {
            var iDoNot = false;
            if (Scanner.CurrentToken == Token.Not)
            {
                AdvanceWithSpaceSkip();
                iDoNot = true;
            }

            Neuron iRes = null;
            try
            {
                if (Scanner.CurrentToken == Token.GroupStart)
                {
                    AdvanceWithSpaceSkip();
                    if (Scanner.CurrentToken == Token.Not)
                    {
                        AdvanceWithSpaceSkip();
                        iDoNot = true;
                    }

                    iRes = ReadBoolPartContent(iDoNot);
                    if (Scanner.CurrentToken == Token.GroupEnd)
                    {
                        AdvanceWithSpaceSkip();
                    }
                    else
                    {
                        LogPosError("Closing brackets expected");
                    }
                }
                else
                {
                    iRes = ReadBoolPartContent(iDoNot); // let the boolPartContent handle the !
                }

                if (!(iRes is BoolExpression))
                {
                    // if it's not a bool expression, convert it to a count. the ! has already been consumed by the ReadBoolPartContent.
                    iRes = ResolveNonBool(iRes, false); // iDoNotHas already been used.
                }

                return iRes;
            }
            catch
            {
                RemoveCondPattern(ToParse);
                if (iRes is BoolExpression)
                {
                    DeleteBool((BoolExpression)iRes);
                }
                else if (iRes is NeuronCluster)
                {
                    DeleteOutputList((NeuronCluster)iRes);
                }

                throw;
            }
        }

        /// <summary>creates a <see langword="bool"/> <see langword="operator"/> of the
        ///     vlaue, taking into account if the ! <see langword="operator"/> needs
        ///     to be implemented.</summary>
        /// <param name="value">The value.</param>
        /// <param name="doNot">if set to <c>true</c> [do not].</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ResolveNonBool(Neuron value, bool doNot)
        {
            var iBool = NeuronFactory.Get<BoolExpression>();
            Brain.Current.Add(iBool);
            Neuron iRight;
            if (value is BoolExpression)
            {
                Link.Create(iBool, value, (ulong)PredefinedNeurons.LeftPart);
                iRight = Brain.Current[(ulong)PredefinedNeurons.False];
            }
            else
            {
                var iCount = MakeCount(value);
                Link.Create(iBool, iCount, (ulong)PredefinedNeurons.LeftPart);
                iRight = MakeStaticIntOperand(0);
            }

            if (doNot)
            {
                Link.Create(iBool, (ulong)PredefinedNeurons.Equal, (ulong)PredefinedNeurons.Operator);
            }
            else
            {
                Link.Create(iBool, (ulong)PredefinedNeurons.Different, (ulong)PredefinedNeurons.Operator);
            }

            Link.Create(iBool, iRight, (ulong)PredefinedNeurons.RightPart);
            return iBool;
        }

        /// <summary>The read bool part content.</summary>
        /// <param name="doNot">The do not.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadBoolPartContent(bool doNot = false)
        {
            BoolExpression iRes = null;
            Neuron iRight = null;
            var iLeft = ReadOperand();
            try
            {
                if (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.GroupEnd)
                {
                    // also need to make certain that it's not the end of the group or logical operator.
                    var iOp = ReadOperator(iRes);
                    if (iOp != null)
                    {
                        SkipSpaces();
                        if (iOp.ID != (ulong)PredefinedNeurons.And && iOp.ID != (ulong)PredefinedNeurons.Or)
                        {
                            iRight = ReadOperand();
                            iRes = NeuronFactory.Get<BoolExpression>();
                            Brain.Current.Add(iRes);
                            Link.Create(iRes, iLeft, (ulong)PredefinedNeurons.LeftPart);
                            Link.Create(iRes, iOp, (ulong)PredefinedNeurons.Operator);
                            Link.Create(iRes, iRight, (ulong)PredefinedNeurons.RightPart);
                            if (doNot)
                            {
                                return ResolveNonBool(iRes, true);
                            }
                        }
                        else
                        {
                            iLeft = ResolveNonBool(iLeft, doNot);
                            if (Scanner.CurrentToken != Token.End)
                            {
                                var iRightNot = false;
                                if (Scanner.CurrentToken == Token.Not)
                                {
                                    AdvanceWithSpaceSkip();
                                    iRightNot = true;
                                }

                                iRight = ReadBoolPartContent(iRightNot); // no !, so read a normal operand.
                                iRes = NeuronFactory.Get<BoolExpression>();
                                Brain.Current.Add(iRes);
                                Link.Create(iRes, iLeft, (ulong)PredefinedNeurons.LeftPart);
                                Link.Create(iRes, iOp, (ulong)PredefinedNeurons.Operator);
                                Link.Create(iRes, iRight, (ulong)PredefinedNeurons.RightPart);
                            }
                            else
                            {
                                LogPosError("The right part of the condition is missing.");
                            }
                        }

                        return iRes;
                    }
                }

                return ResolveNonBool(iLeft, doNot);
            }
            catch
            {
                RemoveCondPattern(ToParse);
                if (iRes != null)
                {
                    DeleteBool(iRes);
                }

                if (iLeft != null && iLeft.ID != Neuron.EmptyId)
                {
                    DeleteOperand(iLeft);
                }

                throw;
            }
        }

        /// <summary>Reads the left/right part of a condition. This is a list of mixed
        ///     statics or [thes |asset] variables.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadOperand()
        {
            SkipSpaces();
            if (Scanner.IsEscaped == false && Scanner.CurrentToken != Token.End)
            {
                Neuron iBoolRes = null;
                var iCurVal = Scanner.CurrentValue.ToLower();
                if (iCurVal == "true")
                {
                    iBoolRes = Brain.Current[(ulong)PredefinedNeurons.True];
                }
                else if (iCurVal == "false")
                {
                    iBoolRes = Brain.Current[(ulong)PredefinedNeurons.False];
                }

                if (iBoolRes != null)
                {
                    AdvanceWithSpaceSkip();
                    return iBoolRes;
                }
            }

            var iOp = ReadOperandContent();
            Link.Create(ToParse, iOp, (ulong)PredefinedNeurons.Operand);
            var iRes = Brain.Current[iOp.Meaning];

                // the meaning of an operand, is the variable that will store the result of the operand, so that is what we need to return, for evaluation in a bool expression.
            return iRes;
        }

        /// <summary>The read operator.</summary>
        /// <param name="iRes">The i res.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadOperator(BoolExpression iRes)
        {
            Neuron iOp = null;
            if (Scanner.CurrentToken == Token.IsEqual)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Equal];
            }
            else if (Scanner.CurrentToken == Token.NotAssign)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Different];
            }
            else if (Scanner.CurrentToken == Token.NotContains)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.NotContains];
            }
            else if (Scanner.CurrentValue.ToLower() == CONTAINSOPERATOR)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Contains];
            }
            else if (Scanner.CurrentToken == Token.IsBiggerThen)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Bigger];
            }
            else if (Scanner.CurrentToken == Token.IsBiggerThenOrEqual)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.BiggerOrEqual];
            }
            else if (Scanner.CurrentToken == Token.IsSmallerThen)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Smaller];
            }
            else if (Scanner.CurrentToken == Token.IsSmallerThenOrEqual)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.SmallerOrEqual];
            }
            else if (Scanner.CurrentToken == Token.Or)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Or];
            }
            else if (Scanner.CurrentToken == Token.DoubleAnd)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.And];
            }
            else
            {
                return null;
            }

            Scanner.GetNext(); // when we got here, it was a valid operator, so go to next token.
            return iOp;
        }

        /// <summary>The read logical operator.</summary>
        /// <param name="iRes">The i res.</param>
        private void ReadLogicalOperator(BoolExpression iRes)
        {
            Neuron iOp = null;
            if (Scanner.CurrentToken == Token.DoubleAnd)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.And];
                Scanner.GetNext();
            }
            else if (Scanner.CurrentToken == Token.Or)
            {
                iOp = Brain.Current[(ulong)PredefinedNeurons.Or];
                Scanner.GetNext();
            }
            else
            {
                LogPosError("Operator expected: &&, ||");
            }

            if (iOp != null)
            {
                Link.Create(iRes, iOp, (ulong)PredefinedNeurons.Operator);
            }
        }

        /// <summary>Reads a single operand: either for left or right part.</summary>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ReadOperandContent()
        {
            var iRes = NeuronFactory.GetCluster(); // the cluster tht contains the data for building the operand value.
            Brain.Current.Add(iRes);
            var iCollectSpaces = false;
            var iVar = NeuronFactory.Get<Variable>();

                // the variable that will hold the result of the operand, so it can be used in a boolean expression.
            Brain.Current.Add(iVar);
            iRes.Meaning = iVar.ID; // so we can find the variable that needs to store the content.

            try
            {
                while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.Not
                       && Scanner.CurrentToken != Token.IsEqual && Scanner.CurrentToken != Token.NotAssign
                       && Scanner.CurrentToken != Token.DoubleAnd && Scanner.CurrentValue.ToLower() != CONTAINSOPERATOR
                       && Scanner.CurrentToken != Token.NotContains && Scanner.CurrentToken != Token.GroupStart
                       && Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.Or
                       && Scanner.CurrentToken != Token.IsBiggerThen
                       && Scanner.CurrentToken != Token.IsBiggerThenOrEqual
                       && Scanner.CurrentToken != Token.IsSmallerThen
                       && Scanner.CurrentToken != Token.IsSmallerThenOrEqual)
                {
                    Neuron iNew;
                    if (Scanner.CurrentToken == Token.AssetVariable)
                    {
                        iNew = HandleAssetVar();
                    }
                    else if (Scanner.CurrentToken == Token.ThesVariable)
                    {
                        iNew = HandleThesVar();
                    }
                    else if (Scanner.CurrentToken == Token.Variable)
                    {
                        iNew = HandleVariable();
                    }
                    else if (Scanner.CurrentToken == Token.TopicRef)
                    {
                        iNew = HandleTopicRef();
                    }
                    else if (Scanner.CurrentToken == Token.Space && iCollectSpaces == false)
                    {
                        SkipSpaces();
                        continue;
                    }
                    else if (Scanner.CurrentToken == Token.Quote)
                    {
                        iCollectSpaces = !iCollectSpaces;
                        continue;
                    }
                    else
                    {
                        iNew = ReadSingleConst();
                        Scanner.GetNext();
                    }

                    Collect(iRes, iNew);
                    SkipSpaces();
                }

                return iRes;
            }
            catch (System.Exception e)
            {
                DeleteOutputList(iRes); // if there was a problem, delete the temp neurons, otherwise, they get lost.
                Brain.Current.Delete(iVar);
                throw e;
            }
        }

        /// <summary>There are 2 parts to be removed: all the operands + the<see langword="bool"/> expression.</summary>
        /// <param name="textNeuron">The text neuron.</param>
        public static void RemoveCondPattern(Neuron textNeuron)
        {
            var iOperands = Factories.Default.CLists.GetBuffer();
            
            var iLinks = Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iList = textNeuron.LinksOut) iLinks.AddRange(iList); // make clocal copy so we don't hve deadlocks.
                foreach (var i in iLinks)
                {
                    if (i.MeaningID == (ulong)PredefinedNeurons.Operand)
                    {
                        iOperands.Add(i.To as NeuronCluster);
                    }
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iLinks);
            }

            foreach (var i in iOperands)
            {
                DeleteOutputList(i);

                    // this will not delete the meaning of the cluster, which is the var, but it will be deleted later, with the bool.
            }

            Factories.Default.CLists.Recycle(iOperands);
            var iExp = textNeuron.FindFirstOut((ulong)PredefinedNeurons.BoolExpression);
            if (iExp != null)
            {
                textNeuron.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.BoolExpression, null);

                    // delete the link so that cleanup can happen properly (otherwise the 'HasReferences' fails and things don't get cleaned correctly.
                var iBoolExp = iExp as BoolExpression;
                if (iBoolExp != null)
                {
                    // it's still an old school item.
                    DeleteBool(iBoolExp);
                }
                else if (iExp is NeuronCluster)
                {
                    // it's code, let the compiler clean it up.
                    NNLModuleCompiler.RemovePreviousDef(iExp);
                }
            }
        }

        /// <summary>Deletes a <see langword="bool"/> expression.</summary>
        /// <param name="toDel">To del.</param>
        private static void DeleteBool(BoolExpression toDel)
        {
            var iLeft = toDel.FindFirstOut((ulong)PredefinedNeurons.LeftPart);
            var iRight = toDel.FindFirstOut((ulong)PredefinedNeurons.RightPart);
            Brain.Current.Delete(toDel); // delete it before the rest, so that the refs can be checked for being null.
            DeleteOperand(iLeft);
            DeleteOperand(iRight);
        }

        /// <summary>Deletes the neurons for a single operand.</summary>
        /// <param name="op">The op.</param>
        private static void DeleteOperand(Neuron op)
        {
            if (op is BoolExpression)
            {
                DeleteBool((BoolExpression)op);
            }
            else if (op is ResultStatement)
            {
                var iStat = (ResultStatement)op;
                var iArgs = iStat.FindFirstOut((ulong)PredefinedNeurons.Arguments) as NeuronCluster;
                System.Collections.Generic.List<Neuron> iToDel;
                using (var iChildren = iArgs.Children) iToDel = iChildren.ConvertTo<Neuron>();
                Brain.Current.Delete(iArgs);
                Brain.Current.Delete(iStat);
                foreach (var i in iToDel)
                {
                    DeleteOperand(i);
                }

                Factories.Default.NLists.Recycle(iToDel);
            }
            else
            {
                if (op != null && op.IsDeleted == false && BrainHelper.HasReferences(op) == false)
                {
                    Brain.Current.Delete(op);
                }
            }
        }
    }
}