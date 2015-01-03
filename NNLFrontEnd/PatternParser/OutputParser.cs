// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputParser.cs" company="">
//   
// </copyright>
// <summary>
//   Parses an output definition and stores it in the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     Parses an output definition and stores it in the network.
    /// </summary>
    /// <remarks>
    ///     When the output definition is a single string, it will point to itself as
    ///     the output value. If there were functions calls or sometihng else, there
    ///     will be a cluster containing either text neurons or neurons with 'call'
    ///     instructions for the functions.
    /// </remarks>
    public class OutputParser : ParserBase
    {
        /// <summary>Initializes a new instance of the <see cref="OutputParser"/> class. constructor for inheriters, doesn't try to set the title.</summary>
        /// <param name="toParse">To parse.</param>
        protected OutputParser(TextNeuron toParse)
        {
            InternalInit(toParse);
        }

        /// <summary>Initializes a new instance of the <see cref="OutputParser"/> class.</summary>
        /// <param name="toParse">The to parse.</param>
        /// <param name="source">The source.</param>
        public OutputParser(TextNeuron toParse, string source)
        {
            InternalInit(toParse);
            ParserTitle = source + ".output";
        }

        /// <summary>
        ///     Gets the valeu that is beign parsed.
        /// </summary>
        /// <value>
        ///     To parse.
        /// </value>
        public TextNeuron ToParse { get; private set; }

        /// <summary>
        ///     Gets the parser that should be used for parsing (and compiling)
        ///     expressions and code.
        /// </summary>
        public static NNLModuleCompiler ExpressionsHandler
        {
            get
            {
                if (fExpressionsHandler == null)
                {
                    var iExpressionsHandler = new NNLModuleCompiler();
                    iExpressionsHandler.LoadCompiledData(null);
                    fExpressionsHandler = iExpressionsHandler;

                        // do this after loading everything, so we try again if something went wrong.
                    Brain.Current.Loaded += Brain_Loaded;
                }

                return fExpressionsHandler;
            }
        }

        /// <summary>The internal init.</summary>
        /// <param name="toParse">The to parse.</param>
        private void InternalInit(TextNeuron toParse)
        {
            var iToParse = toParse.Text;
            if (iToParse == null)
            {
                iToParse = string.Empty;
            }

            Scanner = new Tokenizer(iToParse);

                // always make certain we do a parse of lower case values. Patterns are case insensitive.
            ToParse = toParse;
        }

        /// <summary>
        ///     resets the
        ///     <see cref="JaStDev.HAB.Parsers.OutputParser.ExpressionsHandler" /> to
        ///     <see langword="null" /> so that it will be recreated the next time it
        ///     is used.
        /// </summary>
        public static void ResetExpressionsHandler()
        {
            fExpressionsHandler = null;
        }

        /// <summary>whenever a new brain gets loaded, make certain taht the expression
        ///     ahndler is reset, if we don't do this, it will continue to work with
        ///     old data.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Brain_Loaded(object sender, System.EventArgs e)
        {
            fExpressionsHandler = null;
            Brain.Current.Loaded -= Brain_Loaded;

                // we remove the event handler, it's no longer needed to check for this. it will also allow us to have simple code while creating the expressionhandler.
        }

        /// <summary>
        ///     Parses the output text.
        /// </summary>
        public virtual void Parse()
        {
            if (ExpressionsHandler != null)
            {
                ExpressionsHandler.Clear();
            }

            RemoveOutputPattern(ToParse);
            while (Scanner.CurrentToken != Token.End)
            {
                if (Scanner.CurrentToken == Token.Word)
                {
                    CollectOutput();
                }
                else if (Scanner.CurrentToken == Token.Variable)
                {
                    Collect(HandleVariable(true));
                }
                else if (Scanner.CurrentToken == Token.ThesVariable)
                {
                    Collect(HandleThesVar());
                }
                else if (Scanner.CurrentToken == Token.AssetVariable)
                {
                    Collect(HandleAssetVar());
                }
                else if (Scanner.CurrentToken == Token.TopicRef)
                {
                    Collect(HandleTopicRef());
                }
                else
                {
                    CollectOutput();
                }
            }

            CloseOutput();
            if (ExpressionsHandler != null && ExpressionsHandler.Errors.Count > 0)
            {
                // if there is an expression parser, check if it generated any errors, if so, generate an exception  for them, so the UI can handle errors correctly.
                throw new System.InvalidOperationException(string.Join("\n", ExpressionsHandler.Errors));
            }
        }

        /// <summary>Reads in an asset var def.</summary>
        /// <remarks>asset variable = '#' (PathItem | 'bot' | 'user') {'.'(PathItem|*} [':'
        ///     assetEnd]; PathItem = idOrVar | '(' idOrVar ')' ; idOrVar = ['$'|'^']
        ///     identifier; assetEnd = 'why' | 'when' | 'where' | 'how' | 'count' |
        ///     'who'; (variable, depending on mappings declared in desginer).</remarks>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected NeuronCluster HandleAssetVar()
        {
            if (ExpressionsHandler == null)
            {
                System.Diagnostics.Debug.Assert(Statics != null);
                var iPath = new System.Collections.Generic.List<Neuron>();
                var iExtraOperands = new System.Collections.Generic.List<Neuron>();
                Neuron iItem = null;
                Scanner.GetNext();
                if (Scanner.CurrentToken != Token.End)
                {
                    var iCurVal = Scanner.CurrentValue.ToLower();
                    iItem = GetAssetVarStart(iCurVal);
                    iPath.Add(iItem);
                    ReadDotPath(iPath, iExtraOperands, false);
                    var iRes = CreateVar(iPath, (ulong)PredefinedNeurons.ParsedAssetVar);
                    if (iRes != null)
                    {
                        foreach (var i in iExtraOperands)
                        {
                            Link.Create(iRes, i, (ulong)PredefinedNeurons.Operand);
                        }
                    }

                    return iRes;
                }

                LogPosError("Asset requires a path");
                return null;
            }

            return (NeuronCluster)ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner);
        }

        /// <summary>The get asset var start.</summary>
        /// <param name="iCurVal">The i cur val.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetAssetVarStart(string iCurVal)
        {
            Neuron iItem = null;
            ulong iId;
            if (iCurVal == "bot")
            {
                if (Statics.TryGetValue(iCurVal, out iId))
                {
                    iItem = Brain.Current[iId];
                }
                else
                {
                    LogPosError("Internal error: invalid mapping defined for: " + iCurVal);
                }

                Scanner.GetNext();
            }
            else if (iCurVal == "user")
            {
                if (Statics.TryGetValue("you", out iId))
                {
                    // the user is registered with the 'you' keyword, for the online version: can have multiple users.
                    iItem = Brain.Current[iId];
                }
                else
                {
                    LogPosError("Internal error: invalid mapping defined for: " + iCurVal);
                }

                Scanner.GetNext();
            }
            else
            {
                iItem = GetBracketedIdentifier();
            }

            return iItem;
        }

        /// <summary>Handles a topic or rule ref.: '~'TopicName['.'RuleName] where
        ///     TopicName and RuleName = string | '(' {string}')'</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron HandleTopicRef()
        {
            if (ExpressionsHandler == null)
            {
                Neuron iRes = null;
                Scanner.GetNext();
                if (Scanner.CurrentToken != Token.End)
                {
                    var iName = ReadConstString();
                    Scanner.GetNext();
                    iRes = TopicsDictionary.Get(iName);
                    if (Scanner.CurrentToken == Token.Dot)
                    {
                        // there is a rule def as well, so resolve to a rule.
                        Scanner.GetNext();
                        iName = ReadConstString();
                        iRes = TopicsDictionary.GetRule(iRes, iName);
                    }
                }
                else
                {
                    LogPosError("Topic name and/or rule name expected (~TopicName[.RuleName])");
                }

                return iRes;
            }

            return ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner);
        }

        /// <summary>Reads a string, either 1 word, or something (anything) between
        ///     brackets and returns the string</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadConstString()
        {
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                Scanner.GetNext();
                var iBuilder = new System.Text.StringBuilder();
                while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.GroupEnd)
                {
                    iBuilder.Append(Scanner.CurrentValue);
                    Scanner.GetNext();
                }

                return iBuilder.ToString();
            }

            return Scanner.CurrentValue;
        }

        /// <summary>Reads in a thesaurus var def.</summary>
        /// <remarks>thesaurus variable = '^'identifier | [relationship-type] pos-type
        ///     {'.'PathItem|':' PathEnd} ; PathItem = idOrVar | '(' idOrVar ')' ;
        ///     idOrVar = ['$'|'^'] identifier; PathEnd =identifier | '(' identifier
        ///     {identifier}')';</remarks>
        /// <param name="forceAsThesVar">if set to <c>true</c> : we always store as a thesaurus variable,
        ///     otherwise only if there is a pos (for output, it's faster to treat
        ///     all variable refs as normal variables, including thes refs, but not
        ///     for do patterns).</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected NeuronCluster HandleThesVar(bool forceAsThesVar = false)
        {
            if (ExpressionsHandler == null)
            {
                NeuronCluster iRes = null;
                Scanner.GetNext(); // skip the thes var sign.
                var iPath = new System.Collections.Generic.List<Neuron>();
                var iExtraOperands = new System.Collections.Generic.List<Neuron>();
                var iPos = ReadThesPos();
                if (iPos != null)
                {
                    iPath.Add(iPos);
                    if (iPos.ID == (ulong)PredefinedNeurons.IntNeuron
                        || iPos.ID == (ulong)PredefinedNeurons.DoubleNeuron
                        || iPos.ID == (ulong)PredefinedNeurons.Number)
                    {
                        LogPosError("Thesaurus number references not allowed in output.");
                    }
                    else if (iPos.ID == (ulong)PredefinedNeurons.Empty)
                    {
                        // this is the 'any' keyword
                        LogPosError("'any' not allowed as a pos value in output.");
                    }
                }
                else
                {
                    if (Scanner.CurrentToken != Token.Word && Scanner.CurrentToken != Token.GroupStart)
                    {
                        LogPosError("Please provide a name or valid path for the the thesaurus variable.");
                    }

                    iPath.Add(GetBracketedIdentifier());
                }

                ReadDotPath(iPath, iExtraOperands);
                if (iPos != null || forceAsThesVar)
                {
                    iRes = CreateVar(iPath, (ulong)PredefinedNeurons.ParsedThesVar);
                }
                else
                {
                    iRes = CreateVar(iPath, (ulong)PredefinedNeurons.ParsedVariable, true);

                        // a var reference is always treated the same.
                }

                if (iRes != null)
                {
                    foreach (var i in iExtraOperands)
                    {
                        Link.Create(iRes, i, (ulong)PredefinedNeurons.Operand);
                    }
                }

                return iRes;
            }

            return (NeuronCluster)ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner);
        }

        /// <summary>Reads a bracketed string. Doesn't read the last bracked, this is up
        ///     to the caller to adance. makes certain that there is always only 1
        ///     space between the words. The first bracket is consumed.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadBracketedString()
        {
            string iRes = null;

            AdvanceWithSpaceSkip();
            if (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End)
            {
                iRes = Scanner.CurrentValue;
                while (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End)
                {
                    AdvanceWithSpaceSkip();
                    iRes = iRes + " " + Scanner.CurrentValue;
                }
            }

            if (Scanner.CurrentToken == Token.End)
            {
                LogPosError("Closing brackets ')' expected.");
            }

            return iRes;
        }

        /// <summary>The read dot path.</summary>
        /// <param name="path">The path.</param>
        /// <param name="extraOperands">The extra operands.</param>
        /// <param name="allowLinkOut">The allow link out.</param>
        private void ReadDotPath(System.Collections.Generic.List<Neuron> path, System.Collections.Generic.List<Neuron> extraOperands, 
            bool allowLinkOut = true)
        {
            var iStop = false;
            while (Scanner.CurrentToken != Token.End && iStop == false)
            {
                if (Scanner.CurrentToken == Token.Dot && Scanner.CurrentToken != Token.End)
                {
                    Scanner.GetNext();
                    path.Add(GetBracketedIdentifier());
                }
                else if (Scanner.CurrentToken == Token.OptionStart)
                {
                    var iRead = GetIndexDotPathPart(extraOperands, Token.OptionStart, Token.OptionEnd);
                    if (iRead != null)
                    {
                        path.Add(iRead);
                    }
                }
                else if (Scanner.CurrentToken == Token.LengthSpec)
                {
                    Scanner.GetNext();
                    ulong iId;
                    string iCurVal;
                    if (Scanner.CurrentToken == Token.GroupStart)
                    {
                        iCurVal = ReadBracketedString();
                    }
                    else
                    {
                        iCurVal = Scanner.CurrentValue.ToLower();
                    }

                    Scanner.GetNext();
                    if (Scanner.CurrentToken == Token.GroupStart)
                    {
                        // argumnents for the call
                        ReadArguments(path);
                        if (Scanner.CurrentToken != Token.GroupEnd)
                        {
                            LogPosError(") expected");
                        }
                        else
                        {
                            Scanner.GetNext();
                        }
                    }

                    if (Statics.TryGetValue(iCurVal, out iId) || AssetPronouns.TryGetValue(iCurVal, out iId))
                    {
                        var iItem = Brain.Current[iId];
                        path.Add(iItem);
                    }
                    else
                    {
                        LogPosError(string.Format("Unknown name: '{0}'.", iCurVal));
                    }
                }
                else if (Scanner.CurrentToken == Token.ArrowRight)
                {
                    if (allowLinkOut)
                    {
                        Scanner.GetNext();
                        var iItem = GetBracketedIdentifier(true);
                        path.Add(CollectLinkOut(iItem));
                    }
                    else
                    {
                        LogPosError("LinkOut operator not allowed in this type of path.");
                    }
                }
                else
                {
                    iStop = true;
                }
            }
        }

        /// <summary>The collect link out.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron CollectLinkOut(Neuron item)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            using (var iChildren = iCluster.ChildrenW) iChildren.Add(item);
            iCluster.Meaning = (ulong)PredefinedNeurons.LinkOut;
            return iCluster;
        }

        /// <summary>Reads the arguments for a :callback(...) in a path. for multiple
        ///     arguments, use the ',' token.</summary>
        /// <param name="path">The path.</param>
        private void ReadArguments(System.Collections.Generic.List<Neuron> path)
        {
            var iCollectSpaces = false; // so we can switch between collecting spaces or not.
            var iToAdd = new System.Collections.Generic.List<Neuron>();
            AdvanceWithSpaceSkip();
            while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.GroupEnd)
            {
                if (Scanner.CurrentToken == Token.Variable)
                {
                    iToAdd.Add(HandleVariable());
                }
                else if (Scanner.CurrentToken == Token.ThesVariable)
                {
                    iToAdd.Add(HandleThesVar());
                }
                else if (Scanner.CurrentToken == Token.AssetVariable)
                {
                    iToAdd.Add(HandleAssetVar());
                }
                else if (Scanner.CurrentValue == "\"")
                {
                    // we need to collect 1 set of arguments and start a new one. Also need to check that the "" is closed.
                    iCollectSpaces = !iCollectSpaces;
                    Scanner.GetNext();
                }
                else if (Scanner.CurrentToken == Token.Comma)
                {
                    // also chech on CurrentToken, so we can escape the , sign.
                    if (iCollectSpaces)
                    {
                        // we need to collect 1 set of arguments and start a new one. Also need to check that the "" is closed.
                        LogPosError("\" is not closed.");
                    }

                    CollectArgument(iToAdd, path);
                    iToAdd.Clear();
                    Scanner.GetNext();
                }
                else
                {
                    var iConst = ReadSingleConst();

                        // we read numbers correctly formatted, this should give more flexibility.
                    if (Scanner.CurrentCapitals != null && Scanner.CurrentCapitals.IsNoUpper == false)
                    {
                        // check if there is a capitalization map to build and pass it along to the output.
                        if (Scanner.CurrentCapitals.IsFirstUpper)
                        {
                            iToAdd.Add(Brain.Current[(ulong)PredefinedNeurons.FirstUppercase]);
                        }
                        else if (Scanner.CurrentCapitals.IsAllUpper)
                        {
                            iToAdd.Add(Brain.Current[(ulong)PredefinedNeurons.AllUppercase]);
                        }
                        else
                        {
                            iToAdd.Add(GetCapmapCluster());
                        }
                    }

                    Scanner.GetNext();
                    iToAdd.Add(iConst);
                }

                if (iCollectSpaces == false)
                {
                    SkipSpaces();
                }
            }

            CollectArgument(iToAdd, path);
        }

        /// <summary>The collect argument.</summary>
        /// <param name="values">The values.</param>
        /// <param name="path">The path.</param>
        private void CollectArgument(System.Collections.Generic.List<Neuron> values, System.Collections.Generic.List<Neuron> path)
        {
            if (values.Count > 0)
            {
                var iArgs = NeuronFactory.GetCluster();
                Brain.Current.Add(iArgs);
                iArgs.Meaning = (ulong)PredefinedNeurons.Arguments;
                using (var iChildren = iArgs.ChildrenW) iChildren.AddRange(values); // add the cluster to the output pattern.
                path.Add(iArgs);
            }
        }

        /// <summary>Gets an identifier that can be surrounded by brackets. The identifier
        ///     can be a <see langword="ref"/> to a normal or thesaurus variable. last
        ///     item is skipped, caller returns on next item after bracketed
        ///     identifier.</summary>
        /// <param name="tryStatics">if set to <c>true</c> all regular text will first be<see langword="checked"/> if it is a mapped-static and if so, we
        ///     return this, otherwise, we always get the text values..</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetBracketedIdentifier(bool tryStatics = false)
        {
            var iNeedClose = true; // when true, we have  brackets
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.End)
            {
                // if we are at the end, we need to make certain that we don't do any more checks, cause that causes strange error reporting
                LogPosError("Identifier expected.");
            }

            if (Scanner.CurrentToken == Token.GroupStart)
            {
                Scanner.GetNext();
            }
            else
            {
                iNeedClose = false;
            }

            if (Scanner.CurrentToken == Token.Variable)
            {
                iRes = HandleVariable();
            }
            else if (Scanner.CurrentToken == Token.ThesVariable)
            {
                iRes = HandleThesVar();
            }
            else if (Scanner.CurrentToken == Token.AssetVariable)
            {
                iRes = HandleAssetVar();
            }
            else if (Scanner.CurrentValue != "*")
            {
                if (iNeedClose == false && Scanner.CurrentToken != Token.Word && Scanner.CurrentValue != "*")
                {
                    // only check for a valid word if it isn't in brackets.
                    LogPosError("Please provide a valid identifier.");
                }

                iRes = ReadTextDotPathPart(iNeedClose, Token.GroupEnd, tryStatics);
            }
            else
            {
                iRes = ReadStarDotPart(iNeedClose);
            }

            if (iNeedClose && Scanner.CurrentToken != Token.GroupEnd)
            {
                LogPosError("')' expected");
            }

            Scanner.GetNext();
            return iRes;
        }

        /// <summary>reads a [indexval]</summary>
        /// <param name="extraOperands">The extra Operands.</param>
        /// <param name="startToken">The start Token.</param>
        /// <param name="endToken">The end Token.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetIndexDotPathPart(System.Collections.Generic.List<Neuron> extraOperands, 
            Token startToken, 
            Token endToken)
        {
            if (Scanner.CurrentToken == startToken)
            {
                AdvanceWithSpaceSkip();
                var iRes = CollectExtraOperands(ReadMuliplyPart(extraOperands), extraOperands);

                // AdvanceWithSpaceSkip();
                while (Scanner.CurrentToken == Token.Add || Scanner.CurrentToken == Token.Minus
                       || Scanner.CurrentToken == Token.GroupStart)
                {
                    if (Scanner.CurrentToken == Token.GroupStart)
                    {
                        iRes = GetIndexDotPathPart(extraOperands, Token.GroupStart, Token.GroupEnd);
                    }
                    else
                    {
                        var iToken = Scanner.CurrentToken;
                        var iOp = Scanner.CurrentToken == Token.Add
                                      ? (ulong)PredefinedNeurons.AdditionInstruction
                                      : (ulong)PredefinedNeurons.MinusInstruction;
                        AdvanceWithSpaceSkip();
                        var iArgs = new System.Collections.Generic.List<Neuron>
                                        {
                                            iRes, 
                                            CollectExtraOperands(
                                                ReadMuliplyPart(extraOperands), 
                                                extraOperands)
                                        };

                        // AdvanceWithSpaceSkip();
                        while (Scanner.CurrentToken == iToken)
                        {
                            AdvanceWithSpaceSkip();
                            iArgs.Add(CollectExtraOperands(ReadMuliplyPart(extraOperands), extraOperands));

                            // AdvanceWithSpaceSkip();
                        }

                        iRes = MakeResultStatement(iOp, iArgs);
                    }
                }

                if (Scanner.CurrentToken != endToken)
                {
                    LogPosError("Closing brackets expected.");
                }

                Scanner.GetNext();
                if (iRes != null)
                {
                    var iIndex = NeuronFactory.GetCluster();
                    Brain.Current.Add(iIndex);
                    iIndex.Meaning = (ulong)PredefinedNeurons.Index;
                    using (var iChildren = iIndex.ChildrenW) iChildren.Add(iRes);
                    return iIndex;
                }
            }

            return null;
        }

        /// <summary>creates a result statement for the specified<see langword="operator"/> with the specifid args.</summary>
        /// <param name="op">The op.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron MakeResultStatement(ulong op, System.Collections.Generic.List<Neuron> args)
        {
            var iInst = Brain.Current[op] as Instruction;
            var iArgs = NeuronFactory.GetCluster();
            Brain.Current.Add(iArgs);
            using (var iChildren = iArgs.ChildrenW) iChildren.AddRange(args);
            iArgs.Meaning = (ulong)PredefinedNeurons.ArgumentsList;
            var iRes = new ResultStatement(iInst, iArgs);
            return iRes;
        }

        /// <summary>Reads the muliply part: identifier { (*|/) identifier }</summary>
        /// <param name="extraOperands">The extra operands.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadMuliplyPart(System.Collections.Generic.List<Neuron> extraOperands)
        {
            var iRes = CollectExtraOperands(ReadCalcIdentifier(), extraOperands);

            // AdvanceWithSpaceSkip();
            while (Scanner.CurrentToken == Token.Divide || Scanner.CurrentToken == Token.Multiply)
            {
                var iToken = Scanner.CurrentToken;
                var iOp = Scanner.CurrentToken == Token.Multiply
                              ? (ulong)PredefinedNeurons.MultiplyInstruction
                              : (ulong)PredefinedNeurons.DivideInstruction;
                AdvanceWithSpaceSkip();
                var iArgs = new System.Collections.Generic.List<Neuron>
                                {
                                    iRes, 
                                    CollectExtraOperands(
                                        ReadCalcIdentifier(), 
                                        extraOperands)
                                };

                // AdvanceWithSpaceSkip();
                while (Scanner.CurrentToken == iToken)
                {
                    AdvanceWithSpaceSkip();
                    iArgs.Add(CollectExtraOperands(ReadCalcIdentifier(), extraOperands));

                    // AdvanceWithSpaceSkip();
                }

                iRes = MakeResultStatement(iOp, iArgs);
            }

            return iRes;
        }

        /// <summary>Checks if the specified neuron is a parsed variable of some sort. If
        ///     so, it converts it to an operand, puts it in the operands bag and
        ///     returns the var that needs to be used in it's place.</summary>
        /// <param name="toConv">To conv.</param>
        /// <param name="extraOperands">The extra operands.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron CollectExtraOperands(Neuron toConv, System.Collections.Generic.List<Neuron> extraOperands)
        {
            var iRes = toConv as NeuronCluster;
            if (iRes != null
                && (iRes.Meaning == (ulong)PredefinedNeurons.ParsedVariable
                    || iRes.Meaning == (ulong)PredefinedNeurons.ParsedThesVar
                    || iRes.Meaning == (ulong)PredefinedNeurons.ParsedAssetVar))
            {
                var iOp = NeuronFactory.GetCluster();
                Brain.Current.Add(iOp);
                using (var iChildren = iOp.ChildrenW) iChildren.Add(iRes);
                extraOperands.Add(iOp);

                var iVar = NeuronFactory.Get<Variable>();
                Brain.Current.Add(iVar);
                iOp.Meaning = iVar.ID;
                return iVar;
            }

            return toConv;
        }

        /// <summary>Reads a single identifier that can be used in a calculation. This can
        ///     be a variable, thesaurus or asset value or a number.</summary>
        /// <param name="extraOperands">The extra operands.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadCalcIdentifier()
        {
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.Word)
            {
                int iVal;
                if (int.TryParse(Scanner.CurrentValue, out iVal))
                {
                    iRes = NeuronFactory.GetInt(iVal);
                    Brain.Current.Add(iRes);
                    Scanner.GetNext();
                }
                else
                {
                    LogPosError("Integer constant expected or a variarble reference.");
                }
            }
            else if (Scanner.CurrentToken == Token.Variable)
            {
                iRes = HandleVariable();
            }
            else if (Scanner.CurrentToken == Token.ThesVariable)
            {
                iRes = HandleThesVar();
            }
            else if (Scanner.CurrentToken == Token.AssetVariable)
            {
                iRes = HandleAssetVar();
            }
            else
            {
                LogPosError("invlalid identifier: constant number or regular/thesaurus/asset variable expected.");
            }

            SkipSpaces();
            return iRes;
        }

        /// <summary>Reads the * in the path</summary>
        /// <param name="needClose">if set to <c>true</c> [need close].</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadStarDotPart(bool needClose)
        {
            Neuron iRes = null;
            ulong iId;
            if (Statics.TryGetValue("select all", out iId))
            {
                iRes = Brain.Current[iId];
            }
            else
            {
                LogPosError("Invalid mapping: can't find neuron for the * operator!");
            }

            if (needClose)
            {
                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken != Token.GroupEnd)
                {
                    LogPosError("Closing brackets ')' expected.");
                }
            }

            return iRes;
        }

        /// <summary>The read text dot path part.</summary>
        /// <param name="needClose">The need close.</param>
        /// <param name="endToken">The end token.</param>
        /// <param name="tryStatics">The try statics.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadTextDotPathPart(bool needClose, Token endToken, bool tryStatics)
        {
            var iValues = new System.Collections.Generic.List<string>();
            iValues.Add(Scanner.CurrentValue);
            if (needClose)
            {
                AdvanceWithSpaceSkip();
                while (Scanner.CurrentToken != endToken && Scanner.CurrentToken != Token.End)
                {
                    iValues.Add(Scanner.CurrentValue);
                    AdvanceWithSpaceSkip();
                }

                if (Scanner.CurrentToken == Token.End)
                {
                    LogPosError("Closing brackets ')' expected.");
                }
            }

            if (tryStatics)
            {
                var iStr = new System.Text.StringBuilder();
                foreach (var i in iValues)
                {
                    // rebuild the text to search with only 1 space, lower letter, so we can check for statics.
                    if (iStr.Length != 0)
                    {
                        iStr.Append(" ");
                    }

                    iStr.Append(i.ToLower());
                }

                ulong iFound;
                if (Statics.TryGetValue(iStr.ToString(), out iFound))
                {
                    return Brain.Current[iFound];
                }
            }

            var iCompound = new System.Collections.Generic.List<Neuron>();
            foreach (var i in iValues)
            {
                iCompound.Add(TextNeuron.GetFor(i));
            }

            if (iCompound.Count > 1)
            {
                return BrainHelper.GetCompoundWord(iCompound);
            }

            return iCompound[0];
        }

        /// <summary>Reads in the a var def.</summary>
        /// <remarks>Normal variable = '$'identifier;</remarks>
        /// <param name="allowOutput">if set to <c>true</c> the $output var is allowed to be used.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron HandleVariable(bool allowOutput = false)
        {
            Neuron iRule = null;
            if (ExpressionsHandler == null)
            {
                var iExtraOperands = new System.Collections.Generic.List<Neuron>();
                Scanner.GetNext();
                if (Scanner.CurrentToken == Token.GroupStart)
                {
                    AdvanceWithSpaceSkip();
                    iRule = ReadTopicRefForVar();
                }

                if (Scanner.CurrentToken != Token.Word)
                {
                    LogPosError("Please provide a name for the variable, an identifier is expected.");
                }

                var iVarName = Scanner.CurrentValue;
                Scanner.GetNext();

                    // make certain that we are at the next token, so that all var readers end on the same positionl
                var iPath = new System.Collections.Generic.List<Neuron>();
                ReadDotPath(iPath, iExtraOperands);
                var iRes = CreateVar(iVarName, iRule, iPath);
                if (iRes != null)
                {
                    if (iVarName.ToLower() == "output" && allowOutput == false)
                    {
                        LogPosError(
                            "$output var only allowed in output patterns and variable assignments in do-patterns.");
                    }

                    foreach (var i in iExtraOperands)
                    {
                        Link.Create(iRes, i, (ulong)PredefinedNeurons.Operand);
                    }
                }

                return iRes;
            }

            return ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner);
        }

        /// <summary>The read topic ref for var.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadTopicRefForVar()
        {
            Neuron iRule = null;
            if (Scanner.CurrentToken == Token.TopicRef)
            {
                iRule = HandleTopicRef();
                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken == Token.GroupEnd)
                {
                    Scanner.GetNext();
                    if (Scanner.CurrentToken == Token.Dot)
                    {
                        Scanner.GetNext();
                    }
                    else
                    {
                        LogPosError("dot (.) expected.");
                    }
                }
                else
                {
                    LogPosError("Closing brackets expected.");
                }
            }
            else
            {
                LogPosError("Start of topic reference expected (~).");
            }

            return iRule;
        }

        #region fields

        /// <summary>The f result.</summary>
        private NeuronCluster fResult; // the cluster that will store the list of compiled items that make up the output.

        /// <summary>The f function names.</summary>
        private static System.Collections.Generic.Dictionary<string, System.Action<OutputParser, System.Collections.Generic.List<FunctionArgument>>> fFunctionNames;

        /// <summary>The f expressions handler.</summary>
        private static NNLModuleCompiler fExpressionsHandler;

        #endregion

        #region FunctionNames

        /// <summary>
        ///     Gets/sets the list of function names to recognize + their
        ///     corresponding callback function for rendering the function call.
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, System.Action<OutputParser, System.Collections.Generic.List<FunctionArgument>>> FunctionNames
        {
            get
            {
                if (fFunctionNames == null)
                {
                    fFunctionNames =
                        new System.Collections.Generic.Dictionary
                            <string, System.Action<OutputParser, System.Collections.Generic.List<FunctionArgument>>>();
                    LoadFunctionNames();
                }

                return fFunctionNames;
            }

            set
            {
                fFunctionNames = value;
            }
        }

        /// <summary>
        ///     Loads all the available function names and the functions that handle
        ///     their presence.
        /// </summary>
        private static void LoadFunctionNames()
        {
            // fFunctionNames.Add("GetAssetValue", new Action<OutputParser, List<FunctionArgument>>(ReadGetAssetValue));
        }

        #endregion

        #region data handling

        /// <summary>
        ///     Gets the last output part and adds it to the list of outputs.
        /// </summary>
        private void CloseOutput()
        {
            if (fResult != null)
            {
                Link.Create(ToParse, fResult, (ulong)PredefinedNeurons.ParsedPatternOutput);
            }
            else
            {
                Link.Create(ToParse, ToParse, (ulong)PredefinedNeurons.ParsedPatternOutput);

                    // we create a loopback to indicate that there was no parsing required.
            }
        }

        /// <summary>
        ///     Collects the output as a simple textneuron.
        /// </summary>
        private void CollectOutput()
        {
            var iToAdd = ReadSingleConst(); // we read numbers correctly formatted, this should give more flexibility.
            if (Scanner.CurrentCapitals != null && Scanner.CurrentCapitals.IsNoUpper == false)
            {
                // check if there is a capitalization map to build and pass it along to the output.
                if (Scanner.CurrentCapitals.IsFirstUpper)
                {
                    Collect(Brain.Current[(ulong)PredefinedNeurons.FirstUppercase]);
                }
                else if (Scanner.CurrentCapitals.IsAllUpper)
                {
                    Collect(Brain.Current[(ulong)PredefinedNeurons.AllUppercase]);
                }
                else
                {
                    Collect(GetCapmapCluster());
                }
            }

            Scanner.GetNext();
            Collect(iToAdd);
        }

        /// <summary>builds a cluster that defines how a word should be capitalized based
        ///     on the current settings of the scanner.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetCapmapCluster()
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.UppercaseMap;
            foreach (var i in Scanner.CurrentCapitals.GetUpperCasePoints())
            {
                var iInt = NeuronFactory.GetInt(i);
                Brain.Current.Add(iInt);
                using (var iChildren = iCluster.ChildrenW) iChildren.Add(iInt);
            }

            return iCluster;
        }

        /// <summary>Reads a single constat: making certain that a converion to integers is
        ///     performed. Doesn't advance after reading the const, this needs to be
        ///     done manually which allows you to do some extra processing, like
        ///     checking for capitals.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadSingleConst()
        {
            Neuron iNew = null;

            switch (Scanner.WordType)
            {
                case WordTokenType.Word:
                    if (Scanner.CurrentValue != " ")
                    {
                        iNew = TextNeuron.GetFor(Scanner.CurrentValue);
                    }
                    else
                    {
                        iNew = GetSpaceNeuron();
                    }

                    break;
                case WordTokenType.Integer:
                    var iVal = int.Parse(Scanner.CurrentValue, System.Globalization.CultureInfo.InvariantCulture);
                    iNew = NeuronFactory.GetInt(iVal);
                    Brain.Current.Add(iNew);
                    break;
                case WordTokenType.Decimal:
                    var iD = double.Parse(Scanner.CurrentValue, System.Globalization.CultureInfo.InvariantCulture);
                    iNew = NeuronFactory.GetDouble(iD);
                    Brain.Current.Add(iNew);
                    break;
                default:
                    break;
            }

            System.Diagnostics.Debug.Assert(iNew != null);
            return iNew;
        }

        /// <summary>The f space.</summary>
        private TextNeuron fSpace;

        /// <summary>spaces are too common and would slow up the system cause of to many
        ///     parent-cluster relationships. We simply use a single, non indexed
        ///     'space' textneuron per output.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetSpaceNeuron()
        {
            if (fSpace == null)
            {
                fSpace = NeuronFactory.GetText(" ");
                Brain.Current.Add(fSpace);
            }

            return fSpace;
        }

        /// <summary>adds the specified item to the result.</summary>
        /// <param name="toAdd">To add.</param>
        private void Collect(Neuron toAdd)
        {
            if (toAdd != null)
            {
                // if it 's null, something went wrong.
                PrepareResult();
                using (var iChildren = fResult.ChildrenW) iChildren.Add(toAdd); // add the cluster to the output pattern.
            }
        }

        /// <summary>Adds the specified item to the list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="toAdd">To add.</param>
        protected void Collect(NeuronCluster list, Neuron toAdd)
        {
            if (toAdd != null)
            {
                using (var iChildren = list.ChildrenW) iChildren.Add(toAdd); // add the cluster to the output pattern.
            }
            else
            {
                LogPosError("Unknown parse error.");
            }
        }

        /// <summary>
        ///     Makes certain that there is a result cluster creates.
        /// </summary>
        private void PrepareResult()
        {
            if (fResult == null)
            {
                fResult = NeuronFactory.GetCluster();
                Brain.Current.Add(fResult);
                fResult.Meaning = (ulong)PredefinedNeurons.ParsedPatternOutput;
            }
        }

        /// <summary>Collects a variable into the output cluster. A variable is stored as a
        ///     cluster with meaning<see cref="JaStDev.HAB.PredefinedNeurons.ParsedVariable"/> and
        ///     contains the <paramref name="name"/> of the var as child.</summary>
        /// <param name="name">The name.</param>
        /// <param name="rule">The rule from which to get the result (in case there are multiple
        ///     results in the input)</param>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron CreateVar(string name, Neuron rule, System.Collections.Generic.List<Neuron> path)
        {
            Neuron iToAdd = null;
            var iName = name.ToLower();
            if (iName == "time")
            {
                iToAdd = Time.Current;
            }
            else if (iName == "repeatcount" || iName == "topic" || iName == "topics" || iName == "rules"
                     || iName == "error" || iName == "output" || iName == "input" || iName == "nexttopics")
            {
                ulong iId;
                if (Statics.TryGetValue(iName, out iId))
                {
                    iToAdd = Brain.Current[iId];
                }
                else if (iName == "output")
                {
                    // the 'output' var's name is the same as the 'output' instruction, which NNL can't handle really well, so it renders the name as _output.
                    iName = "_output";
                    if (Statics.TryGetValue(iName, out iId))
                    {
                        iToAdd = Brain.Current[iId];
                    }
                    else
                    {
                        LogPosError(string.Format("Invalid mapping: can't find reference to {0} variable", name));
                    }
                }
                else
                {
                    LogPosError(string.Format("Invalid mapping: can't find reference to {0} variable", name));
                }
            }
            else
            {
                iToAdd = TextNeuron.GetFor(name);
            }

            if (iName != "output")
            {
                NeuronCluster iVar;
                if (rule != null)
                {
                    iVar = CreateParsedVar(rule, iToAdd, path);
                }
                else
                {
                    path.Insert(0, iToAdd);
                    iVar = CreateVar(path, (ulong)PredefinedNeurons.ParsedVariable);
                }

                return iVar;
            }

            return iToAdd;

            // if (path.Count > 0)
            // LogPosError("$output is a special variable that doesn't allow for functions at the moment. Assign the content of $output to another variable first.");
        }

        /// <summary>The create parsed var.</summary>
        /// <param name="rule">The rule.</param>
        /// <param name="toAdd">The to add.</param>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster CreateParsedVar(Neuron rule, Neuron toAdd, System.Collections.Generic.List<Neuron> path)
        {
            var iVar = NeuronFactory.GetCluster();
            Brain.Current.Add(iVar);
            iVar.Meaning = (ulong)PredefinedNeurons.ParsedVariable;
            ulong iFound; // add the name of the var as a child of the cluster.
            if (Statics.TryGetValue("getresultfromrule", out iFound) == false)
            {
                LogPosError("Invalid mapping: can't find reference to 'getresultfromrule' variable");
            }

            using (var iChildren = iVar.ChildrenW)
            {
                iChildren.Add(Brain.Current[iFound]);
                iChildren.Add(rule);
                iChildren.Add(toAdd);
                iChildren.AddRange(path);
            }

            return iVar;
        }

        /// <summary>The create parsed var.</summary>
        /// <param name="toAdd">The to add.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster CreateParsedVar(Neuron toAdd)
        {
            var iVar = NeuronFactory.GetCluster();
            Brain.Current.Add(iVar);
            iVar.Meaning = (ulong)PredefinedNeurons.ParsedVariable;
            using (var iChildren = iVar.ChildrenW) iChildren.Add(toAdd); // add the name of the var as a child of the cluster.
            return iVar;
        }

        /// <summary>Collects a var that contains a list of neurons that describe it's
        ///     path.</summary>
        /// <param name="path">The path.</param>
        /// <param name="varType">Type of the var.</param>
        /// <param name="asThes">if set to <c>true</c> an extra link will be added between the cluster
        ///     and itself, meaning 'ParsedThesVar', to indicate that it is a regular
        ///     var pah, but it's value comes from a thes-path and the collected value
        ///     should not be resolved to the exact word that the user used, but
        ///     instead, the object can remain (so that possibly a synonym gets
        ///     printed).</param>
        /// <returns>the newly created variable</returns>
        private NeuronCluster CreateVar(System.Collections.Generic.List<Neuron> path, 
            ulong varType, 
            bool asThes = false)
        {
            var iVar = NeuronFactory.GetCluster();
            Brain.Current.Add(iVar);
            if (asThes)
            {
                Link.Create(iVar, iVar, (ulong)PredefinedNeurons.ParsedThesVar);

                    // indicate that it should be treated as a thesvar (so 'object' is used and not the text).
            }

            iVar.Meaning = varType;
            using (var iChildren = iVar.ChildrenW) iChildren.AddRange(path); // add the name of the var as a child of the cluster.
            return iVar;
        }

        /// <summary>The remove output pattern.</summary>
        /// <param name="fToParse">The f to parse.</param>
        public static void RemoveOutputPattern(Neuron fToParse)
        {
            var iFound = fToParse.FindFirstOut((ulong)PredefinedNeurons.ParsedPatternOutput);
            if (iFound == fToParse)
            {
                var iLink = Link.Find(fToParse, fToParse, Brain.Current[(ulong)PredefinedNeurons.ParsedPatternOutput]);
                System.Diagnostics.Debug.Assert(iLink != null);
                iLink.Destroy();
            }
            else if (iFound != null)
            {
                DeleteOutputList((NeuronCluster)iFound);
            }
        }

        /// <summary>The delete output list.</summary>
        /// <param name="toDel">The to del.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected static void DeleteOutputList(NeuronCluster toDel)
        {
            var iToDel = toDel;
            System.Collections.Generic.List<Neuron> iToDelete;
            using (var iList = iToDel.Children) iToDelete = iToDel.Children.ConvertTo<Neuron>();
            try
            {
                DeleteOperands(toDel);
                Brain.Current.Delete(toDel);

                    // delete cluster before children, so that we can check if the children have any refs or not.
                foreach (var i in iToDelete)
                {
                    if (i is TextNeuron || i is IntNeuron || i is DoubleNeuron || (i is Variable && i != Time.Current))
                    {
                        // variables can be located in argument lists of result statements. need to make certain taht we don't delete the time value
                        RemoveUnusedNeuron(i);
                    }
                    else
                    {
                        var iCluster = i as NeuronCluster;
                        if (iCluster != null)
                        {
                            // if it's not a cluster or textneuron, it's most likely a neuron, and only got there through one of the dicts like statics or functions.
                            if ((iCluster.Meaning == 0) || (iCluster.Meaning == (ulong)PredefinedNeurons.ParsedVariable)
                                || (iCluster.Meaning == (ulong)PredefinedNeurons.ParsedThesVar)
                                || (iCluster.Meaning == (ulong)PredefinedNeurons.ParsedAssetVar)
                                || (iCluster.Meaning == (ulong)PredefinedNeurons.Index)
                                || (iCluster.Meaning == (ulong)PredefinedNeurons.Arguments)
                                || (iCluster.Meaning == (ulong)PredefinedNeurons.LinkOut))
                            {
                                DeleteOutputList(iCluster);
                            }
                            else if (iCluster.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                            {
                                RemoveUnusedCompound(iCluster);
                            }
                            else if (iCluster.Meaning == (ulong)PredefinedNeurons.Code)
                            {
                                NNLModuleCompiler.RemovePreviousDef(iCluster);
                            }
                            else if (Statics.ContainsValue(iCluster.ID)
                                     || iCluster.Meaning == (ulong)PredefinedNeurons.TextPatternTopic
                                     || iCluster.Meaning == (ulong)PredefinedNeurons.PatternRule)
                            {
                                // statics, topics or rules that get referenced don't need to be deleted.
                                continue;
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }
                        }
                        else
                        {
                            var iStatement = i as ResultStatement; // this also handles variables.
                            if (iStatement != null && iStatement.ID != Time.Current.ID)
                            {
                                // don't want to delete the 'time' neuron.
                                var iArgs = iStatement.FindFirstOut((ulong)PredefinedNeurons.Arguments) as NeuronCluster;
                                if (iArgs != null)
                                {
                                    DeleteOutputList(iArgs);
                                }

                                Brain.Current.Delete(iStatement);
                            }
                        }
                    }
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iToDelete);
            }
        }

        /// <summary>Removes the unused compound.</summary>
        /// <param name="toDel">To del.</param>
        private static void RemoveUnusedCompound(NeuronCluster toDel)
        {
            if (toDel.IsDeleted == false && BrainHelper.HasReferences(toDel) == false
                && Statics.ContainsValue(toDel.ID) == false)
            {
                // only try to delete if there aren't any references anymore. + could already be deleted, words can be reused in the output + some items in a parsermap refer to statics, like 'bot' and 'user'.
                System.Collections.Generic.List<Neuron> iToDelete;
                using (var iList = toDel.Children) iToDelete = iList.ConvertTo<Neuron>();
                Brain.Current.Delete(toDel);
                foreach (var i in iToDelete)
                {
                    RemoveUnusedNeuron(i);
                }

                Factories.Default.NLists.Recycle(iToDelete);
            }
        }

        /// <summary>gets all the operands and Deletes them.</summary>
        /// <param name="toDel">To del.</param>
        private static void DeleteOperands(NeuronCluster toDel)
        {
            foreach (var i in toDel.FindAllOut((ulong)PredefinedNeurons.Operand))
            {
                var iToDel = i as NeuronCluster;
                if (iToDel != null)
                {
                    var iMeaning = Brain.Current[iToDel.Meaning] as Variable;

                        // don't forget the meaning which is registered as a var.
                    DeleteOutputList(iToDel);
                    if (iMeaning != null)
                    {
                        RemoveUnusedNeuron(iMeaning);
                    }
                }
            }
        }

        /// <summary>The remove unused neuron.</summary>
        /// <param name="i">The i.</param>
        private static void RemoveUnusedNeuron(Neuron i)
        {
            if (i.IsDeleted == false && BrainHelper.HasReferences(i) == false && Statics.ContainsValue(i.ID) == false)
            {
                // only try to delete if there aren't any references anymore. + could already be deleted, words can be reused in the output + some items in a parsermap refer to statics, like 'bot' and 'user'.
                Brain.Current.Delete(i);
            }
        }

        #endregion

        #region function builders

        /// <summary>Creates a count insruction and puts the specified item as argument</summary>
        /// <param name="toCount">To count.</param>
        /// <returns>The <see cref="ResultStatement"/>.</returns>
        protected ResultStatement MakeCount(Neuron toCount)
        {
            var iArgs = NeuronFactory.GetCluster();
            Brain.Current.Add(iArgs);
            using (var iChildren = iArgs.ChildrenW) iChildren.Add(toCount);
            var iRes = new ResultStatement((Instruction)Brain.Current[(ulong)PredefinedNeurons.CountInstruction], iArgs);

                // the contstructor adds it to the network.
            return iRes;
        }

        /// <summary>Creates and registers an operand that contains the '0' const.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron MakeStaticIntOperand(int value)
        {
            var iInt = NeuronFactory.GetInt();
            Brain.Current.Add(iInt);
            return iInt;
        }

        #endregion
    }
}