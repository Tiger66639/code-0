// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoParser.cs" company="">
//   
// </copyright>
// <summary>
//   The do parser.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>The do parser.</summary>
    public class DoParser : OutputParser
    {
        #region fields

        /// <summary>The f result.</summary>
        private Neuron fResult;

                       // the neuron that will store the action that needs to be perfomed + all the left and right operand.
        #endregion

        // <summary>
        /// <summary>Initializes a new instance of the <see cref="DoParser"/> class.</summary>
        /// <param name="toParse">The to parse.</param>
        /// <param name="source">The source.</param>
        public DoParser(TextNeuron toParse, string source)
            : base(toParse)
        {
            ParserTitle = source + ".Do";
        }

        #region Functions

        /// <summary>
        ///     Gets/sets the list of actions/functions and their mappings to neuron
        ///     ids so that we can convert actions to neurons, like 'Cmdshell'
        /// </summary>
        /// <remarks>
        ///     This dictionay should be filled
        /// </remarks>
        public static System.Collections.Generic.Dictionary<string, ulong> Functions { get; set; }

        #endregion

        /// <summary>
        ///     Parses the 'do' text.
        /// </summary>
        public override void Parse()
        {
            RemoveDoPattern(ToParse);
            if (ExpressionsHandler == null)
            {
                fResult = NeuronFactory.GetNeuron();
                Brain.Current.Add(fResult);
                try
                {
                    ReadDoStatement();
                }
                catch
                {
                    Brain.Current.Delete(fResult);
                    fResult = null;
                    throw;
                }
            }
            else
            {
                // start a try-finally block so that we can see all errors, even if there were first some regular errors followed by one that caused an exception. If we didn't do this, we would only see the exception, not the errors  that came before
                ExpressionsHandler.Clear();
                try
                {
                    fResult = ExpressionsHandler.GetCodeBlockFrom(ParserTitle, Scanner);
                }
                finally
                {
                    if (ExpressionsHandler.Errors.Count > 0)
                    {
                        // the compiler doesn't generate an exception for the errors normally
                        throw new System.InvalidOperationException(string.Join("\n", ExpressionsHandler.Errors));
                    }
                }
            }

            if (fResult != null && Link.Exists(ToParse, fResult, (ulong)PredefinedNeurons.ParsedDoPattern) == false)
            {
                Link.Create(ToParse, fResult, (ulong)PredefinedNeurons.ParsedDoPattern);

                    // let the do pattern know where to find the compiled version.
            }
        }

        /// <summary>
        ///     Reads the actual do statement.
        /// </summary>
        private void ReadDoStatement()
        {
            if (Scanner.CurrentToken != Token.End)
            {
                if (Scanner.CurrentToken == Token.AssetVariable)
                {
                    ReadAssetOperation();
                }
                else if (Scanner.CurrentToken == Token.ThesVariable)
                {
                    ReadThesOperation();
                }
                else if (Scanner.CurrentToken == Token.Variable)
                {
                    ReadVarOperation();
                }
                else
                {
                    ReadAction();
                }
            }
        }

        /// <summary>Gets the reference to an OS function. ex: CopyFile</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetFunctionRef()
        {
            Scanner.GetNext(); // skip the ?
            var iCurVal = Scanner.CurrentValue.ToLower(); // get the name of the function
            Scanner.GetNext();
            ulong iId;
            if (Functions.TryGetValue(iCurVal, out iId))
            {
                return Brain.Current[iId];
            }

            LogPosError("Function name expected");
            return null;
        }

        /// <summary>
        ///     Reads the action.
        /// </summary>
        private void ReadAction()
        {
            ulong iId;
            var iCurVal = Scanner.CurrentValue.ToLower();
            if (iCurVal == "delay")
            {
                ReadDelayAction();
            }
            else if (iCurVal == "call")
            {
                // dynamic call, the first argument should be a ref to the function that should be called.
                ReadCall();
            }
            else if (Functions.TryGetValue(iCurVal, out iId))
            {
#if BASIC
               LogPosError("Function calls are not supported in this version"); 
#else
                ReadOSAction(iId);
#endif
            }
            else
            {
                LogPosError(string.Format("Failed to map {0} to a valid function.", iCurVal));
            }
        }

        /// <summary>
        ///     Reads the delay action. This is just another sub do statement, which
        ///     will create an extra link on the result, indicating that the
        ///     statement's execution needs to be delayed untill the rest has been
        ///     processed.
        /// </summary>
        private void ReadDelayAction()
        {
            Link.Create(fResult, fResult, (ulong)PredefinedNeurons.Delay);

                // simply indicate that the execution needs to be delayed a little.
            AdvanceWithSpaceSkip();
            ReadDoStatement();
        }

        /// <summary>The read os action.</summary>
        /// <param name="id">The id.</param>
        private void ReadOSAction(ulong id)
        {
            var iFunc = Brain.Current[id];
            AdvanceWithSpaceSkip();
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                AdvanceWithSpaceSkip();
                var iArgs = NeuronFactory.GetCluster(); // all the arguments need to be stored in a cluster.
                Brain.Current.Add(iArgs);
                try
                {
                    iArgs.Meaning = (ulong)PredefinedNeurons.ArgumentsList;
                    var iParam = ReadParam();
                    Collect(iArgs, iParam);
                    while (Scanner.CurrentToken == Token.Comma)
                    {
                        AdvanceWithSpaceSkip();
                        iParam = ReadParam();
                        Collect(iArgs, iParam);
                    }

                    if (Scanner.CurrentToken == Token.GroupEnd)
                    {
                        Scanner.GetNext();
                    }
                    else
                    {
                        LogPosError("Closing brackets expected");
                    }

                    Link.Create(fResult, iFunc, (ulong)PredefinedNeurons.ReflectionSinCall);
                    Link.Create(fResult, iArgs, (ulong)PredefinedNeurons.Arguments);
                }
                catch
                {
                    Brain.Current.Delete(iArgs);

                        // if something went wrong during the parse, make certain there are no mem leaks.
                    throw;
                }
            }
        }

        /// <summary>
        ///     similar to <see cref="DoParser.ReadOSAction" /> except that the
        ///     <see langword="static" /> 'call' is used, which is declared in the
        ///     general mappings.
        /// </summary>
        private void ReadCall()
        {
            ulong iId;
            if (Statics.TryGetValue("call", out iId))
            {
                ReadOSAction(iId);
            }
            else
            {
                LogPosError("No function mapping found for 'call'");
            }
        }

        /// <summary>Checks if the next token is a word (that is not escaped) that is
        ///     registered as an OS action, if so, this is read and<see langword="true"/> returned, if not an os action,<see langword="false"/> is returned.</summary>
        /// <param name="iRightPart">The i right part.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryReadOSAction()
        {
            ulong iId;
            if (Scanner.CurrentToken == Token.Word && Scanner.IsEscaped == false)
            {
                // we check if it is not escaped cause os function names are just words for th tokenizer.
                var iCurVal = Scanner.CurrentValue.ToLower();
                if (iCurVal == "call")
                {
                    ReadCall();
                }
                else if (Functions.TryGetValue(iCurVal, out iId))
                {
#if BASIC
                  LogPosError("Function calls are not supported in this version"); 
               #else
                    ReadOSAction(iId);
#endif
                    return true;
                }
            }

            return false;
        }

        /// <summary>The read var operation.</summary>
        private void ReadVarOperation()
        {
            Neuron iThes = null;
            NeuronCluster iRightPart = null;
            try
            {
                iThes = HandleVariable();
                if (iThes != null)
                {
                    Link.Create(fResult, iThes, (ulong)PredefinedNeurons.LeftPart);
                }

                SkipSpaces();
                var iOp = ReadVarOperator();
                if (iOp != null)
                {
                    Link.Create(fResult, iOp, (ulong)PredefinedNeurons.Operator);
                }

                SkipSpaces();
                if (TryReadOSAction() == false)
                {
                    iRightPart = ReadOperand(true);
                    if (iRightPart != null)
                    {
                        Link.Create(fResult, iRightPart, (ulong)PredefinedNeurons.RightPart);
                    }
                }
            }
            catch
            {
                if (iRightPart != null)
                {
                    DeleteOutputList(iRightPart);
                }

                if (iThes != null && iThes is NeuronCluster)
                {
                    DeleteOutputList((NeuronCluster)iThes);
                }

                throw;
            }
        }

        /// <summary>The read var operator.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadVarOperator()
        {
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.AddAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignAdd];
            }
            else if (Scanner.CurrentToken == Token.MinusAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignRemove];
            }
            else if (Scanner.CurrentToken == Token.Assign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.Assignment];
            }
            else if (Scanner.CurrentToken != Token.End)
            {
                LogPosError(string.Format("Invalid operator: {0}. Expected: +=, -= or =", Scanner.CurrentValue));
            }
            else
            {
                LogPosError("Invalid end-of-pattern. Expected: +=, -= or =");
            }

            Scanner.GetNext();
            return iRes;
        }

        /// <summary>
        ///     Reads an operation performed on the thesaurus.
        /// </summary>
        private void ReadThesOperation()
        {
            NeuronCluster iThes = null;
            NeuronCluster iRightPart = null;
            try
            {
                iThes = HandleThesVar(true);
                Link.Create(fResult, iThes, (ulong)PredefinedNeurons.LeftPart);
                SkipSpaces();
                var iOp = ReadThesOperator();
                SkipSpaces();
                if (iOp.ID == (ulong)PredefinedNeurons.Assignment && Scanner.CurrentValue.ToLower() == "null")
                {
                    // assign to null means remove the link.
                    iOp = Brain.Current[(ulong)PredefinedNeurons.Different];
                }
                else if (TryReadOSAction() == false)
                {
                    iRightPart = ReadOperand(true);
                }

                UpdateThesForOperator(iThes, iOp);
                Link.Create(fResult, iOp, (ulong)PredefinedNeurons.Operator);
                if (iRightPart != null)
                {
                    Link.Create(fResult, iRightPart, (ulong)PredefinedNeurons.RightPart);
                }
            }
            catch
            {
                if (iRightPart != null)
                {
                    DeleteOutputList(iRightPart);
                }

                if (iThes != null)
                {
                    DeleteOutputList(iThes);
                }

                throw;
            }
        }

        /// <summary>If the <see langword="operator"/> is = or !=, the last item in the<paramref name="thes"/> path has to be a direct out link, check for
        ///     this and make certain that the direct out link is attached to the
        ///     result and not as a child of the thesaurus path, cause that can't be
        ///     solved correctly.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="op">The op.</param>
        private void UpdateThesForOperator(NeuronCluster thes, Neuron op)
        {
            if (op.ID == (ulong)PredefinedNeurons.Assignment || op.ID == (ulong)PredefinedNeurons.Different)
            {
                NeuronCluster iLast = null;
                int iCount;
                using (var iChildren = thes.Children) iCount = iChildren.Count;
                if (iCount > 0)
                {
                    Neuron iFound;
                    using (var iChildren = thes.ChildrenW)
                    {
                        if (Brain.Current.TryFindNeuron(iChildren[iCount - 1], out iFound))
                        {
                            iLast = iFound as NeuronCluster;
                        }

                        if (iLast == null || iLast.Meaning != (ulong)PredefinedNeurons.LinkOut)
                        {
                            LogPosError(
                                "The = and != thesaurus operations require a left part that ends with a ->identifier ");
                        }

                        iChildren.RemoveAt(iCount - 1);
                    }
                }

                if (iLast != null)
                {
                    Link.Create(fResult, iLast, (ulong)PredefinedNeurons.LinkOut);
                }
            }
        }

        /// <summary>Reads the thes operator. ThesOp = '+=' | '-=' | '=' ;</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadThesOperator()
        {
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.AddAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignAdd];
            }
            else if (Scanner.CurrentToken == Token.MinusAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignRemove];
            }
            else if (Scanner.CurrentToken == Token.Assign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.Assignment];
            }
            else
            {
                LogPosError(string.Format("Invalid operator: {0}. Expected: +=, -= or =", Scanner.CurrentValue));
            }

            Scanner.GetNext();
            return iRes;
        }

        /// <summary>The read asset operation.</summary>
        private void ReadAssetOperation()
        {
            NeuronCluster iAsset = null;
            NeuronCluster iRightPart = null;
            try
            {
                iAsset = HandleAssetVar();
                Link.Create(fResult, iAsset, (ulong)PredefinedNeurons.LeftPart);
                SkipSpaces();
                var iOp = ReadAssetOperator();
                Link.Create(fResult, iOp, (ulong)PredefinedNeurons.Operator);
                SkipSpaces();
                if (TryReadOSAction() == false)
                {
                    iRightPart = ReadOperand(true);
                    Link.Create(fResult, iRightPart, (ulong)PredefinedNeurons.RightPart);
                }
            }
            catch
            {
                if (iRightPart != null)
                {
                    DeleteOutputList(iRightPart);
                }

                if (iAsset != null)
                {
                    DeleteOutputList(iAsset);
                }

                throw;
            }
        }

        /// <summary>The read operand.</summary>
        /// <param name="allowOutput">The allow output.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ReadOperand(bool allowOutput = false)
        {
            var iCollectSpaces = false;
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            try
            {
                while (Scanner.CurrentToken != Token.End)
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
                        iNew = HandleVariable(allowOutput);
                    }
                    else if (Scanner.CurrentToken == Token.TopicRef)
                    {
                        iNew = HandleTopicRef();
                    }
                    else if (Scanner.CurrentToken == Token.QuestionMark)
                    {
                        iNew = GetFunctionRef();
                    }
                    else if (Scanner.CurrentToken == Token.Space && iCollectSpaces == false)
                    {
                        SkipSpaces();
                        continue;
                    }
                    else if (Scanner.CurrentToken == Token.Quote)
                    {
                        iCollectSpaces = !iCollectSpaces;
                        Scanner.GetNext();
                        continue;
                    }
                    else
                    {
                        iNew = ReadSingleConst();
                        Scanner.GetNext();
                    }

                    Collect(iRes, iNew);
                    if (!iCollectSpaces)
                    {
                        SkipSpaces();
                    }
                }

                if (iCollectSpaces)
                {
                    LogPosWarning("Closing \" expected.");
                }

                return iRes;
            }
            catch
            {
                DeleteOutputList(iRes);
                throw;
            }
        }

        /// <summary>The read param.</summary>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ReadParam()
        {
            var iCollectSpaces = false;
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            try
            {
                while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.GroupEnd
                       && Scanner.CurrentToken != Token.Comma)
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
                        Scanner.GetNext();
                        continue;
                    }
                    else
                    {
                        iNew = ReadSingleConst();
                        Scanner.GetNext();
                    }

                    Collect(iRes, iNew);
                    if (iCollectSpaces == false)
                    {
                        SkipSpaces();
                    }
                }

                return iRes;
            }
            catch
            {
                DeleteOutputList(iRes);
                throw;
            }
        }

        /// <summary>The read asset operator.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadAssetOperator()
        {
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.AddAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignAdd];
            }
            else if (Scanner.CurrentToken == Token.MinusAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignRemove];
            }
            else if (Scanner.CurrentToken == Token.AddNotAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.AssignAddNot];
            }
            else if (Scanner.CurrentToken == Token.NotAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.Different];
            }
            else if (Scanner.CurrentToken == Token.AndAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.And];
            }
            else if (Scanner.CurrentToken == Token.OrAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.Or];
            }
            else if (Scanner.CurrentToken == Token.ListAssign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.List];
            }
            else if (Scanner.CurrentToken == Token.Assign)
            {
                iRes = Brain.Current[(ulong)PredefinedNeurons.Assignment];
            }
            else if (Scanner.CurrentToken != Token.End)
            {
                LogPosError(
                    string.Format(
                        "Invalid operator: {0}. Expected: +=  -=  |=  &=  ;=  =  !=  !+=", 
                        Scanner.CurrentValue));
            }
            else
            {
                LogPosError("Invalid end-of-pattern. Expected: +=  -=  |=  &=  ;=  =  !=  !+=");
            }

            Scanner.GetNext();
            return iRes;
        }

        /// <summary>Removes the previous def.</summary>
        /// <param name="toClean">To clean.</param>
        public static void RemoveDoPattern(Neuron toClean)
        {
            var iStart = toClean.FindFirstOut((ulong)PredefinedNeurons.ParsedDoPattern);
            if (iStart != null)
            {
                if (iStart is NeuronCluster)
                {
                    // if it's a cluster, it's code, otherwise it's the old style of do pattern. Make certain that the clean up is correctly done. 
                    NNLModuleCompiler.RemovePreviousDef(iStart);
                }
                else
                {
                    var iArgs = iStart.FindFirstOut((ulong)PredefinedNeurons.LeftPart) as NeuronCluster;
                    if (iArgs != null)
                    {
                        // it's either thes or asset operation.
                        DeleteOutputList(iArgs);
                        iArgs = iStart.FindFirstOut((ulong)PredefinedNeurons.RightPart) as NeuronCluster;
                        if (iArgs != null)
                        {
                            DeleteOutputList(iArgs);
                        }
                    }
                    else
                    {
                        iArgs = iStart.FindFirstOut((ulong)PredefinedNeurons.Arguments) as NeuronCluster;
                        if (iArgs != null)
                        {
                            // its an action call, so delete the args.
                            DeleteOutputList(iArgs);
                        }
                    }

                    Brain.Current.Delete(iStart);
                }
            }
        }
    }
}