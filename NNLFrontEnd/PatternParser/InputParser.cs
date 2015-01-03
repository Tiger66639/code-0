// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputParser.cs" company="">
//   
// </copyright>
// <summary>
//   parses a pattern definition and stores it in the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     parses a pattern definition and stores it in the network.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There is still a problem in the pattern path building for the {}
    ///         operator. Take the examples: a {b c} a {b d}
    ///     </para>
    ///     <para>
    ///         with the imput: a b c b d b c, the first pattern will still match, due to
    ///         intermingeling of the patterns. The solution is to start a new path for
    ///         each const/var after a { or | within a {}
    ///     </para>
    /// </remarks>
    public class InputParser : ParserBase
    {
        /// <summary>Initializes a new instance of the <see cref="InputParser"/> class.</summary>
        /// <param name="toParse">To parse.</param>
        /// <param name="source">The source of the pattern, used for the parse title</param>
        /// <param name="localTo">Possibly a neuron for which the parsed pattern needs to be local (for
        ///     local topics).</param>
        public InputParser(TextNeuron toParse, string source, Neuron localTo)
        {
            if (toParse.Text != null)
            {
                Scanner = new Tokenizer(toParse.Text.ToLower());

                    // always make certain we do a parse of lower case values. Patterns are case insensitive.
            }
            else
            {
                Scanner = new Tokenizer(null);
            }

            fToParse = toParse;
            ParserTitle = source + ".input";
            fLocalTo = localTo;
        }

        /// <summary>
        ///     Parses the string and stores it as a pattern in the network.
        /// </summary>
        public void Parse()
        {
            RemoveInputPattern(fToParse);
            ParseSection();
            ClosePatternDef();
        }

        /// <summary>The parse section.</summary>
        private void ParseSection()
        {
            while (Scanner.CurrentToken != Token.End)
            {
                if (fCurrentPos == null)
                {
                    // whenever the currentpos gets reset, we need to start calculating a new 'CurrentFrom'. This happens because of the '|' and && operators. 
                    PrepareStart();
                }

                if (Scanner.CurrentToken != Token.End)
                {
                    if (fEndReached && Scanner.CurrentToken != Token.Space)
                    {
                        LogPosError("The >| token has to be the end of the pattern.");
                    }

                    switch (Scanner.CurrentToken)
                    {
                        case Token.Space:
                            Scanner.GetNext();
                            break; // spaces are simply ignored.
                        case Token.Variable:
                            HandleVariable();
                            break;
                        case Token.ThesVariable:
                            HandleThesVariable();
                            break;
                        case Token.AssetVariable:
                            HandleAssetVar();
                            break;
                        case Token.OptionStart:
                            StartGrouping(Token.OptionStart);
                            Scanner.GetNext();
                            break;
                        case Token.OptionEnd:
                            CheckCloseGrouping(Token.OptionEnd);
                            Scanner.GetNext();
                            break;
                        case Token.LoopStart:
                            StartGrouping(Token.LoopStart);
                            Scanner.GetNext();
                            break;
                        case Token.LoopEnd:
                            CheckCloseGrouping(Token.LoopEnd);
                            Scanner.GetNext();
                            break;
                        case Token.GroupStart:
                            StartGrouping(Token.GroupStart);
                            Scanner.GetNext();
                            break;
                        case Token.GroupEnd:
                            CheckCloseGrouping(Token.GroupEnd);
                            Scanner.GetNext();
                            break;
                        case Token.Choice:
                            HandleChoice();
                            Scanner.GetNext();
                            break;
                        case Token.DoubleAnd:
                            HandleAnd();
                            Scanner.GetNext();
                            break;
                        case Token.Sign:
                            handleWord();
                            Scanner.GetNext();
                            break;
                        case Token.StartOfInput:
                            LogPosError("start token only allowed at start of pattern.");
                            break;
                        case Token.TopicRef:
                            HandleTopic(false);
                            break;
                        case Token.EndOfInput:
                            HandleEndOfInput();
                            Scanner.GetNext();
                            break;
                        default:
                            handleWord();
                            Scanner.GetNext();
                            break;
                    }

                    CheckIfPrevWasChoice();
                }
            }
        }

        /// <summary>Handles a topic and possibly input pattern reference, inside another
        ///     input pattern (sub patterns). sub <see langword="ref"/> = '~'
        ///     TopicName ['.' RuleName] ;</summary>
        /// <param name="asStart">if set to <c>true</c> [as start].</param>
        private void HandleTopic(bool asStart)
        {
            Scanner.GetNext(); // read the ~
            var iTopic = ReadBracketedId(); // read topic name
            string iRule = null;
            if (Scanner.CurrentToken == Token.Dot)
            {
                Scanner.GetNext();
                iRule = ReadBracketedId(); // read rule name
            }

            if (iRule != null)
            {
                CollectSub(asStart, iTopic, iRule, (ulong)PredefinedNeurons.SubRules);
            }
            else
            {
                CollectSub(asStart, iTopic, iRule, (ulong)PredefinedNeurons.SubTopics);
            }
        }

        /// <summary>The read bracketed id.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadBracketedId()
        {
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                Scanner.GetNext();
                var iBuild = new System.Text.StringBuilder();
                while (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End)
                {
                    iBuild.Append(Scanner.CurrentValue);
                    Scanner.GetNext();
                }

                if (Scanner.CurrentToken == Token.GroupEnd)
                {
                    Scanner.GetNext();
                }
                else
                {
                    LogPosError(") expected");
                }

                return iBuild.ToString();
            }

            if (Scanner.CurrentToken != Token.Word)
            {
                // only check for a valid word if it isn't in brackets.
                LogPosError("Please provide a valid identifier.");
            }

            if (Scanner.CurrentToken != Token.End)
            {
                var iRes = Scanner.CurrentValue;
                Scanner.GetNext();
                return iRes;
            }

            return null;
        }

        /// <summary>Collects the rule or topic and links it with the rest of the pattern.</summary>
        /// <remarks>for sub-patterns at the start of other pattern (ex is for topics,
        ///     rules are the same): GlobalTopics -(sub-topic)&gt; neuron not at start:
        ///     neuronA -(topics)&gt; {neuronB -(sub-topic)&gt; neuronC -(word)&gt; neuronD }
        ///     //a neuron links out with meaning 'topics', containing neurons that
        ///     represent the sub topics.</remarks>
        /// <param name="asStart">if set to <c>true</c> [as start].</param>
        /// <param name="iTopic">The i topic.</param>
        /// <param name="iRule">The i rule.</param>
        /// <param name="topicOrRule">The topic or rule.</param>
        private void CollectSub(bool asStart, string iTopic, string iRule, ulong topicOrRule)
        {
            Neuron iRef = null;
            try
            {
                iRef = TopicsDictionary.Get(iTopic, iRule);
            }
            catch (System.Exception e)
            {
                LogPosError(e.Message);
            }

            if (iRef == null)
            {
                if (iRule != null)
                {
                    LogPosError(string.Format("Failed to find topic and rule: {0}.{1}", iTopic, iRule));
                }
                else
                {
                    LogPosError(string.Format("Failed to find topic: {0}", iTopic));
                }
            }

            Neuron iTopicRef; // so we can check if it is local or not.
            if (string.IsNullOrEmpty(iRule))
            {
                iTopicRef = iRef;
            }
            else
            {
                iTopicRef = iRef.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);

                    // when we get here, we know there is a ref, otherwise the function would have exited.
            }

            if (asStart)
            {
                if (fLocalTo != null)
                {
                    LogPosError("sub topics or rules not allowed at the start of a pattern in a local topic.");
                }

                if (iTopicRef.FindFirstOut((ulong)PredefinedNeurons.Local) != null)
                {
                    LogPosError("local sub topics or rules are not yet allowed at the start of a pattern.");
                }

                fCurrentPos = GetSubFromStart(iRef, topicOrRule);
            }
            else
            {
                fCurrentPos = GetSubFrom(fCurrentPos, iRef, topicOrRule);
            }

            ProcessExtraCurrrentsForSub(iRef, topicOrRule);
        }

        /// <summary>The process extra currrents for sub.</summary>
        /// <param name="iRef">The i ref.</param>
        /// <param name="topicOrRule">The topic or rule.</param>
        private void ProcessExtraCurrrentsForSub(Neuron iRef, ulong topicOrRule)
        {
            if (fExtraCurrents.Count > 0)
            {
                // If there are 'extraCurrents' specified, we need to link the currentFrom to them, so that we can also go from those 'extra current' points, to the current point.
                var iNewExtraCurrents = new System.Collections.Generic.List<Neuron>();
                foreach (var i in fExtraCurrents)
                {
                    var iNew = GetSubFrom(i, iRef, topicOrRule);
                    iNewExtraCurrents.Add(iNew);
                }

                fExtraCurrents = iNewExtraCurrents;

                    // we need to replace the current positions with the ones that we created.
            }
        }

        /// <summary>Gets the neuron that can be used as next 'CurrentPos' when the 'sub'
        ///     rule/topic is at the start of the current pattern.</summary>
        /// <param name="sub">The i ref.</param>
        /// <param name="topicOrRule">The topic or rule.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetSubFromStart(Neuron sub, ulong topicOrRule)
        {
            var iRoot = Brain.Current[topicOrRule];
            Neuron iNext = null;
            var iNeedsUnique = false;
            if (fPrevWasChoice == false || fGroupings.Count == 0 || fGroupings.Peek().GroupingType != Token.LoopStart)
            {
                // if start of loop, always get new link, cause we want a unique path.
                iNext = iRoot.FindFirstOutEmptyInfo(sub.ID);
            }
            else
            {
                iNeedsUnique = true;
            }

            if (iNext == null)
            {
                iNext = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNext);
                var iLink = Link.Create(iRoot, iNext, sub);
                if (iNeedsUnique)
                {
                    iLink.InfoW.Add((ulong)PredefinedNeurons.True);

                        // we add a neuron to the link.info, so we know that this link needs to remain unique cause it's part of a loop (if this wasn't unique, we can get different patterns to intermingle).   
                }
            }

            Link.Create(fToParse, iNext, (ulong)PredefinedNeurons.ParsedPatternStart);
            return iNext;
        }

        /// <summary>The get sub from.</summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="topicOrRule">The topic or rule.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetSubFrom(Neuron currentFrom, Neuron meaning, ulong topicOrRule)
        {
            Neuron iRes = null;
            var iNeedsUnique = false;
            var iSubs = currentFrom.FindFirstOut(topicOrRule) as NeuronCluster;

                // gets the cluster for 'sub' items, attached to the 'currentFrom'
            if (iSubs == null)
            {
                iSubs = NeuronFactory.GetCluster();
                Brain.Current.Add(iSubs);
                iSubs.Meaning = topicOrRule;
                Link.Create(currentFrom, iSubs, topicOrRule);
            }
            else if (fPrevWasChoice == false || fGroupings.Count == 0
                     || fGroupings.Peek().GroupingType != Token.LoopStart)
            {
                // if start of loop, always get new link, cause we want a unique path.
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = iSubs.Children)
                    iChildren = iList.ConvertTo<Neuron>();

                        // check if there already is a neuron for the specified topic/rule
                try
                {
                    foreach (var i in iChildren)
                    {
                        iRes = i.FindFirstOutEmptyInfo(meaning.ID);
                        if (iRes != null)
                        {
                            Link.Create(fToParse, i, topicOrRule);
                            Link.Create(fToParse, iRes, (ulong)PredefinedNeurons.ParsedPatternPart);

                                // need to remember that the parsed pattern uses this neuron as a topic/rule placeholder, so we can remove it correctly when no longer referenced.
                            return iRes;
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iChildren);
                }
            }
            else
            {
                iNeedsUnique = true;
            }

            var itemp = NeuronFactory.GetNeuron();

                // we need a new neuron for the prev item, so that we can do a split on the children when we have reached a sub rule/topic in a root pattern.
            Brain.Current.Add(itemp);
            using (var iChildren = iSubs.ChildrenW) iChildren.Add(itemp);
            Link.Create(fToParse, itemp, topicOrRule);

                // need to remember that the parsed pattern uses this neuron as a topic/rule placeholder, so we can remove it correctly when no longer referenced.
            iRes = NeuronFactory.GetNeuron();
            Brain.Current.Add(iRes);
            var iLink = Link.Create(itemp, iRes, meaning);
            if (iNeedsUnique)
            {
                iLink.InfoW.Add((ulong)PredefinedNeurons.True);

                    // we add a neuron to the link.info, so we know that this link needs to remain unique cause it's part of a loop (if this wasn't unique, we can get different patterns to intermingle).
            }

            Link.Create(fToParse, iRes, (ulong)PredefinedNeurons.ParsedPatternPart);
            return iRes;
        }

        /// <summary>The get unique sub from.</summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="topicOrRule">The topic or rule.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetUniqueSubFrom(Neuron currentFrom, Neuron meaning, ulong topicOrRule)
        {
            Neuron iRes = null;
            var iSubs = currentFrom.FindFirstOut(topicOrRule) as NeuronCluster;

                // gets the cluster for 'sub' items, attached to the 'currentFrom'
            if (iSubs == null)
            {
                iSubs = NeuronFactory.GetCluster();
                Brain.Current.Add(iSubs);
                iSubs.Meaning = topicOrRule;
                Link.Create(currentFrom, iSubs, topicOrRule);
            }

            var itemp = NeuronFactory.GetNeuron();

                // we need a new neuron for the prev item, so that we can do a split on the children when we have reached a sub rule/topic in a root pattern.
            Brain.Current.Add(itemp);
            using (var iChildren = iSubs.ChildrenW) iChildren.Add(itemp);
            Link.Create(fToParse, itemp, topicOrRule);

                // need to remember that the parsed pattern uses this neuron as a topic/rule placeholder, so we can remove it correctly when no longer referenced.
            iRes = NeuronFactory.GetNeuron();
            Brain.Current.Add(iRes);
            Link.Create(itemp, iRes, meaning);
            Link.Create(fToParse, iRes, (ulong)PredefinedNeurons.ParsedPatternPart);
            return iRes;
        }

        /// <summary>
        ///     Handles the >| symbol. Only allowed at the end of the pattern.
        /// </summary>
        private void HandleEndOfInput()
        {
            if (fEndReached)
            {
                LogPosError("Duplicate >| token");
            }
            else
            {
                if (fGroupings.Count > 0)
                {
                    LogPosError(
                        "groups(), options[] and loops{} can't contain an >| token. This can only be used at the end of the pattern.");
                }

                Link.Create(fToParse, fToParse, (ulong)PredefinedNeurons.PatternAtEndOfInput);
            }

            fEndReached = true;
        }

        /// <summary>The handle and.</summary>
        private void HandleAnd()
        {
            if (fPRevWasGroup)
            {
                if (fCurrentPos is NeuronCluster
                    && ((NeuronCluster)fCurrentPos).Meaning == (ulong)PredefinedNeurons.ParsedVariable)
                {
                    // if it's a neuronCluster, it's a var, )
                    LogPosError("A variable can't precede an && operator");
                }

                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken == Token.GroupStart || Scanner.CurrentToken == Token.OptionStart
                    || Scanner.CurrentToken == Token.LoopStart)
                {
                    fPrevWasAnd = true;

                        // needs to be done before starting the group, so that it can record that there was an && operator, important cause we need to check that $vars aren't used as a start after &&
                    StartGrouping(Scanner.CurrentToken);
                    if (fCurrentPos != null)
                    {
                        // currentPos can be null when pattern of form '[aa] && (bc)'. In this case, we simply need to allow for the bc part to be a start of the pattern.
                        var iNext = fCurrentPos.FindFirstOut((ulong)PredefinedNeurons.And);
                        if (iNext == null)
                        {
                            iNext = NeuronFactory.GetNeuron();
                            Brain.Current.Add(iNext);
                            Link.Create(fToParse, iNext, (ulong)PredefinedNeurons.ParsedPatternPart);
                            var iLink = Link.Create(fCurrentPos, iNext, (ulong)PredefinedNeurons.And);
                        }

                        fCurrentPos = iNext;
                    }

                    for (var i = 0; i < fExtraCurrents.Count; i++)
                    {
                        var iExtra = fExtraCurrents[i];
                        var iNext = iExtra.FindFirstOut((ulong)PredefinedNeurons.And);
                        if (iNext == null)
                        {
                            iNext = NeuronFactory.GetNeuron();
                            Brain.Current.Add(iNext);
                            Link.Create(fToParse, iNext, (ulong)PredefinedNeurons.ParsedPatternPart);
                            var iLink = Link.Create(iExtra, iNext, (ulong)PredefinedNeurons.And);
                        }

                        fExtraCurrents[i] = iNext;
                    }

                    fPrevWasAnd = true;

                        // need to do this again cause StartGrouping had it reset to false (which is the normal handling)
                }
                else if (Scanner.CurrentToken == Token.End)
                {
                    LogPosError("Unexpected end of pattern: an && operator needs to be followed by (), [] or {}");
                }
                else
                {
                    LogPosError("an && operator needs to be followed by (), [] or {}");
                }
            }
            else
            {
                LogPosError("The && operator can't be used here. Usage: 'group && group', where group = (), [] or {}");
            }
        }

        /// <summary>
        ///     Processes a | in the input. This will roll back the current position
        ///     to the previous current from. This can be null, in which case we need
        ///     to redo the start again.
        /// </summary>
        private void HandleChoice()
        {
            if (fGroupings.Count > 0)
            {
                var iLast = fGroupings.Peek();
                iLast.EndPoints.Add(fCurrentPos);
                iLast.EndPoints.AddRange(fExtraCurrents); // all the current active exta currents are also endpoints.
                fCurrentPos = iLast.PrevPos;

                    // this provides the option part: the next item can follow the current or the one before the option/loop
                fExtraCurrents.Clear();

                    // need to copy the data cause the list can be used again later on, so clear the list first.
                fExtraCurrents.AddRange(iLast.ExtraCurrents);

                    // these also need to be rolled back for an optional grouping, same as prevPos
                fPrevWasChoice = true; // so we can collect the 'First item' of the next pattern section.
                fPrevPos = fCurrentPos;

                    // we can only collect the new item when we are certain that the currentPos is changed.
                fPrevWasAnd = iLast.PrevWasAnd;
            }
            else
            {
                fExtraCurrents.Add(fCurrentPos);

                    // need to collect the last item, so we can contineu from there after the choice. Usually this will be the closing of the parsed pattern.
                fCurrentPos = null;
            }

            fPRevWasGroup = false; // so we can handle the && operator correct.
        }

        /// <summary>If the text <paramref name="pattern"/> had already been parsed, it has
        ///     a reference to the previous parsed path. This needs to be cleened up
        ///     cause we are going to build a new one.</summary>
        /// <param name="pattern">The pattern.</param>
        public static void RemoveInputPattern(Neuron pattern)
        {
            var iParts = Factories.Default.LinkLists.GetBuffer();
            var iSubs = Factories.Default.LinkLists.GetBuffer();
            var iStarts = Factories.Default.LinkLists.GetBuffer();
            var iVars = Factories.Default.LinkLists.GetBuffer();
            var iParsedVars = Factories.Default.LinkLists.GetBuffer();
            var iAssets = Factories.Default.LinkLists.GetBuffer();
            var iToDelete = Factories.Default.NLists.GetBuffer();
            var iLinks = Factories.Default.LinkLists.GetBuffer();
            if (pattern.LinksOutIdentifier != null)
            {
                using (var iList = pattern.LinksOut) iLinks.AddRange(iList); // make a local copy so it is threadsafe.
            }

            foreach (var i in iLinks)
            {
                if (i.MeaningID == (ulong)PredefinedNeurons.ParsedPatternPart)
                {
                    iParts.Add(i);
                }
                else if (i.MeaningID == (ulong)PredefinedNeurons.ParsedPatternStart)
                {
                    iStarts.Add(i);
                }
                else if (i.MeaningID == (ulong)PredefinedNeurons.ParsedVariable)
                {
                    iParsedVars.Add(i);
                }
                else if (i.MeaningID == (ulong)PredefinedNeurons.ParsedAssetVar)
                {
                    iAssets.Add(i);
                }
                else if (i.MeaningID == (ulong)PredefinedNeurons.SubRules
                         || i.MeaningID == (ulong)PredefinedNeurons.SubTopics)
                {
                    iSubs.Add(i);
                }
                else if (i.To is Variable)
                {
                    iVars.Add(i);
                }
            }

            var iToDeleteIfEmpty = new System.Collections.Generic.HashSet<Neuron>();
            RemoveParsedVars(iParsedVars, iToDeleteIfEmpty);
            RemoveAssets(iAssets, iToDeleteIfEmpty);
            RemoveStarts(pattern, iStarts, iToDeleteIfEmpty);
            RemoveParts(pattern, iParts, iToDelete, iToDeleteIfEmpty);
            RemoveVars(iVars, iToDeleteIfEmpty);
            RemoveSubsCluster(iSubs);

            foreach (var i in iToDelete)
            {
                Brain.Current.Delete(i);
            }

            ClearTerminators(pattern);
            var iRetries = Enumerable.ToList(iToDeleteIfEmpty);
            var iToDelIfEmpty = new System.Collections.Generic.List<Neuron>();

                // some neurons aren't empty yet the first try, but because others got deleted, they might get freed, so we need to retry until we can't delete anymore neurons.
            while (iRetries.Count > 0 && iRetries.Count != iToDelIfEmpty.Count)
            {
                // as long as the lists are of different lengths, we should try to delete more items.
                iToDelIfEmpty = iRetries;
                iRetries = new System.Collections.Generic.List<Neuron>();
                foreach (var i in iToDelIfEmpty)
                {
                    if (BrainHelper.DeleteIfEmpty(i) == false)
                    {
                        iRetries.Add(i);
                    }
                }
            }

            Factories.Default.NLists.Recycle(iToDelete);
            Factories.Default.LinkLists.Recycle(iLinks);
            Factories.Default.LinkLists.Recycle(iParts);
            Factories.Default.LinkLists.Recycle(iSubs);
            Factories.Default.LinkLists.Recycle(iStarts);
            Factories.Default.LinkLists.Recycle(iVars);
            Factories.Default.LinkLists.Recycle(iParsedVars);
            Factories.Default.LinkLists.Recycle(iAssets);
        }

        /// <summary>walk through the list of asset neurons, delete the link. If the 'to'
        ///     parts have no more incomming 'assetvar' links, delete the neuron.
        ///     before deleting the neuron however, make certain that all of it's
        ///     outgoing links are also cleaned up: every meaning is the code to
        ///     calculate the asset. every 'to' can also be deleted cause it doesn't
        ///     have any incomming links anymore.</summary>
        /// <param name="items">The items.</param>
        /// <param name="toDeleteIfEmpty">The to Delete If Empty.</param>
        private static void RemoveAssets(System.Collections.Generic.List<Link> items, System.Collections.Generic.HashSet<Neuron> toDeleteIfEmpty)
        {
            foreach (var i in items)
            {
                var iTo = i.To;
                if (iTo.ID != Neuron.EmptyId)
                {
                    // could already be deleted?
                    i.Destroy();
                    var iFound = iTo.FindAllIn((ulong)PredefinedNeurons.ParsedAssetVar);
                    if (iFound == null || iFound.Count == 0)
                    {
                        var iAssets = Factories.Default.LinkLists.GetBuffer();
                        using (var iList = iTo.LinksOut) iAssets.AddRange(iList);
                        foreach (var u in iAssets)
                        {
                            var iNext = u.To;
                            NNLModuleCompiler.RemovePreviousDef(u.Meaning);
                            Brain.Current.Delete(iNext);
                        }

                        Factories.Default.LinkLists.Recycle(iAssets);
                        Brain.Current.Delete(iTo);
                    }
                }
            }
        }

        /// <summary>The remove subs cluster.</summary>
        /// <param name="subs">The subs.</param>
        private static void RemoveSubsCluster(System.Collections.Generic.List<Link> subs)
        {
            foreach (var i in subs)
            {
                if (Brain.Current.IsExistingID(i.ToID))
                {
                    // the sub ref could already have been cleened because it's root item got deleted.
                    var iTo = i.To;
                    i.Destroy();
                    var iIsEmpty = iTo.LinksInIdentifier == null;
                    if (iIsEmpty == false)
                    {
                        using (var iLinks = iTo.LinksIn) iIsEmpty = iLinks.Count == 0;
                    }

                    if (iIsEmpty)
                    {
                        Brain.Current.Delete(iTo);
                    }
                }
            }
        }

        /// <summary>The remove parsed vars.</summary>
        /// <param name="parsedVars">The parsed vars.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void RemoveParsedVars(System.Collections.Generic.List<Link> parsedVars, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            foreach (var i in parsedVars)
            {
                if (Brain.Current.IsExistingID(i.ToID))
                {
                    // the var could already have been cleened by the parsedVar list
                    var iTo = i.To as NeuronCluster;
                    i.Destroy();
                    var iFound = iTo.FindAllIn((ulong)PredefinedNeurons.ParsedVariable);
                    if (iFound == null || iFound.Count == 0)
                    {
                        // if the parsed var is no longer used by any other pattern, delete it.
                        DeleteParseTree(iTo, DelIfEmpty);

                            // only delete the parse-tree if the thesvar can also be deleted.
                        CleanOutgoingLinks(iTo, DelIfEmpty);
                        RemoveVarCluster(iTo, DelIfEmpty);
                    }
                }
            }
        }

        /// <summary>removes all the neurons that were created to build a parse tree for
        ///     the thesaurus. Items are only removed if they are no longer used.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void DeleteParseTree(NeuronCluster thes, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            var iFound = thes.FindAllOut((ulong)PredefinedNeurons.ParsedPatternPart);
            foreach (var i in iFound)
            {
                var iRels = i.FindAllIn((ulong)PredefinedNeurons.ParsedPatternPart);
                if (iRels.Count == 1)
                {
                    Brain.Current.Delete(i);

                        // when a path neuron only has 1 link left, it's to the current thes, so it can be deleted. the link meaning, if it can be cleaned, will be done when the content of the thes path gets cleaned.
                }
                else if (DelIfEmpty.Contains(i) == false)
                {
                    // the start of a parse tree can be referenced by multiple parsedThes clusters (because of loops), if we don't retry, we create leaks.
                    DelIfEmpty.Add(i);
                }
            }
        }

        /// <summary>Removes any remaining incomming links to the<paramref name="pattern"/> that indicate an end. There can still be a
        ///     link when a var was a terminator which is also used by another<paramref name="pattern"/></summary>
        /// <param name="pattern">The pattern.</param>
        private static void ClearTerminators(Neuron pattern)
        {
            var iVars = Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iLinks = pattern.LinksIn)
                {
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == (ulong)PredefinedNeurons.PatternRule
                            || i.MeaningID == (ulong)PredefinedNeurons.PatternAtEndOfInput
                            || i.MeaningID == (ulong)PredefinedNeurons.PatternAtStartOfInput)
                        {
                            iVars.Add(i);
                        }
                    }
                }

                foreach (var i in iVars)
                {
                    i.Destroy();
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iVars);
            }
        }

        /// <summary>The remove vars.</summary>
        /// <param name="list">The list.</param>
        /// <param name="iToDeleteIfEmpty">The i to delete if empty.</param>
        private static void RemoveVars(System.Collections.Generic.List<Link> list, System.Collections.Generic.HashSet<Neuron> iToDeleteIfEmpty)
        {
            foreach (var i in list)
            {
                var iItem = i.Meaning;
                if (iToDeleteIfEmpty.Contains(iItem) == false)
                {
                    iToDeleteIfEmpty.Add(iItem);
                }

                iItem = i.To;
                if (iItem.ID != Neuron.EmptyId)
                {
                    // could be that it is already deleted.
                    Brain.Current.Delete(iItem); // the var is always local to the patten so it should always be deleted.
                }
            }
        }

        /// <summary>The remove parts.</summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="list">The list.</param>
        /// <param name="toDelete">The to delete.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void RemoveParts(
            Neuron pattern, System.Collections.Generic.List<Link> list, System.Collections.Generic.List<Neuron> toDelete, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            // List<Neuron> iAllParts = (from i in list select i.To).ToList();                              //we need to know all the parts that are involved in this pattern, so we can remove every link between the parts. If we don't do this, we get leaks, cause we check the nr of incomming links on a part to check if it needs to be removed. Since we can have loopbacks, we need to remove all the links.
            foreach (var i in list)
            {
                // delete all the links.
                var iTo = i.To;
                var iLink = Link.Find(iTo, pattern, Brain.Current[(ulong)PredefinedNeurons.PatternRule]);

                    // any part can be used as a 'last item', if  this is the case, remvoe the link.
                if (iLink != null)
                {
                    // if the parse failed the last time, there is no closing link.
                    iLink.Destroy();
                }

                i.Destroy();
                var iPartOf = iTo.FindAllIn((ulong)PredefinedNeurons.ParsedPatternPart);

                    // check if this part is still used by another pattern. If not, delete and clean.
                if (iPartOf.Count == 0)
                {
                    CleanOutgoingLinks(iTo, DelIfEmpty);
                    DeleteIncommingLinks(iTo, DelIfEmpty);
                    Brain.Current.Delete(iTo); // it's not a part of something, so delete it.
                }
            }
        }

        /// <summary>Gets every outgoing link on the item to clean and adds every meaning
        ///     to the 'delIfEmpty' list.</summary>
        /// <param name="toClean">To clean.</param>
        /// <param name="DelIfEmpty">The Del If Empty.</param>
        private static void CleanOutgoingLinks(Neuron toClean, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            var iToCheck = Factories.Default.LinkLists.GetBuffer();
            if (toClean.LinksOutIdentifier != null)
            {
                using (var iList = toClean.LinksOut) iToCheck.AddRange(iList);
            }

            for (var iC = 0; iC < iToCheck.Count; iC++)
            {
                if (Brain.Current.IsExistingID(iToCheck[iC].MeaningID))
                {
                    var iMeaning = iToCheck[iC].Meaning;
                    if (iMeaning.ID == (ulong)PredefinedNeurons.ParsedVariable
                        || iMeaning.ID == (ulong)PredefinedNeurons.ParsedThesVar)
                    {
                        var iVar = iToCheck[iC].To as NeuronCluster;
                        RemoveVarClusters(iVar, DelIfEmpty);
                    }
                    else if (iMeaning.ID == (ulong)PredefinedNeurons.SubTopics
                             || iMeaning.ID == (ulong)PredefinedNeurons.SubRules)
                    {
                        var iVar = iToCheck[iC].To as NeuronCluster;
                        RemoveSubsCluster(iVar, DelIfEmpty);
                    }
                    else if (iMeaning.ID != (ulong)PredefinedNeurons.ParsedPatternPart
                             && iMeaning.ID != (ulong)PredefinedNeurons.And
                             && iMeaning.ID != (ulong)PredefinedNeurons.Variable
                             && iMeaning.ID != (ulong)PredefinedNeurons.PatternRule)
                    {
                        // don't need to try and delete the && operator or terminator refs.
                        iToCheck[iC].Destroy();

                            // need to remove the link to make certain that the meaning gets deleted
                        if (DelIfEmpty.Contains(iMeaning) == false
                            && (!(iMeaning is NeuronCluster)
                                || (((NeuronCluster)iMeaning).Meaning
                                    != (ulong)PredefinedNeurons.TextPatternTopic
                                    && ((NeuronCluster)iMeaning).Meaning != (ulong)PredefinedNeurons.PatternRule)))
                        {
                            // don't delete the meaning of the link if it's a topic or rule.
                            DelIfEmpty.Add(iMeaning);
                        }
                    }
                }
            }

            Factories.Default.LinkLists.Recycle(iToCheck);
        }

        /// <summary>The remove subs cluster.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void RemoveSubsCluster(
            NeuronCluster cluster, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = cluster.Children) iChildren = iList.ConvertTo<Neuron>();
            Brain.Current.Delete(cluster);
            foreach (var i in iChildren)
            {
                CleanOutgoingLinks(i, DelIfEmpty);
                if (DelIfEmpty.Contains(i) == false)
                {
                    DelIfEmpty.Add(i);
                }
            }

            Factories.Default.NLists.Recycle(iChildren);
        }

        /// <summary>The remove var clusters.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void RemoveVarClusters(
            NeuronCluster cluster, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            System.Collections.Generic.List<NeuronCluster> iChildren;
            using (var iList = cluster.Children) iChildren = iList.ConvertTo<NeuronCluster>();
            Brain.Current.Delete(cluster);
            if (iChildren != null)
            {
                foreach (var i in iChildren)
                {
                    CleanOutgoingLinks(i, DelIfEmpty);

                    // don't delete the sub items, these are linked to the main pattern definition and are deleted as variables in the main Delete function, which is better, otherwise we get leaks.
                    // RemoveVarCluster(i, DelIfEmpty);
                }

                Factories.Default.CLists.Recycle(iChildren);
            }
        }

        /// <summary>The remove var cluster.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void RemoveVarCluster(
            NeuronCluster cluster, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            if (cluster != null)
            {
                var iToDeleteIfEmpty = new System.Collections.Generic.List<Neuron>();
                if (cluster.Meaning == (ulong)PredefinedNeurons.ParsedVariable)
                {
                    // it's a cluster that contains variable clusters.
                    System.Collections.Generic.List<Neuron> iChildren;
                    using (var iList = cluster.Children) iChildren = iList.ConvertTo<Neuron>();
                    foreach (var u in iChildren)
                    {
                        iToDeleteIfEmpty.Add(u); // add before it's children, so 
                        var iClusterU = u as NeuronCluster;
                        if (iClusterU != null && iClusterU.ChildrenIdentifier != null)
                        {
                            System.Collections.Generic.List<Neuron> iSubs;
                            using (var iList = iClusterU.Children) iSubs = iList.ConvertTo<Neuron>();
                            iToDeleteIfEmpty.AddRange(iSubs);
                            Factories.Default.NLists.Recycle(iSubs);
                        }
                    }

                    Factories.Default.NLists.Recycle(iChildren);
                }
                else if (cluster.Meaning == (ulong)PredefinedNeurons.ParsedThesVar)
                {
                    // it contains thes vars, so no more children need to be processed.
                    System.Collections.Generic.List<Neuron> iChildren;
                    using (var iList = cluster.Children) iChildren = iList.ConvertTo<Neuron>();
                    iToDeleteIfEmpty.AddRange(iChildren);
                    Factories.Default.NLists.Recycle(iChildren);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "Internal error: Unknown variable cluster, can't delete it.");
                }

                if (Neuron.IsEmpty(cluster.ID) == false)
                {
                    // could be that the neuron already got deleted.
                    Brain.Current.Delete(cluster);
                }

                foreach (var i in iToDeleteIfEmpty)
                {
                    // this is done seperatly cause the vars need to check if they don't have any parents left anymore instead of incomming links.
                    if (DelIfEmpty.Contains(i) == false)
                    {
                        DelIfEmpty.Add(i);
                    }
                }
            }
        }

        /// <summary>we also delete all the incomming links, cause some operators can cause
        ///     nested paths. The onl way to clean these properly and easy is by also
        ///     hanlding the incomming links. need to watch out that we don't delete
        ///     sub topics/rules that reference to the neurons who's incomming links
        ///     we are cleaning.</summary>
        /// <param name="toClean">To clean.</param>
        /// <param name="DelIfEmpty">The del if empty.</param>
        private static void DeleteIncommingLinks(Neuron toClean, System.Collections.Generic.HashSet<Neuron> DelIfEmpty)
        {
            var iToCheck = Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iLinks = toClean.LinksIn) iToCheck.AddRange(iLinks);
                for (var iC = 0; iC < iToCheck.Count; iC++)
                {
                    var iMeaning = iToCheck[iC].Meaning;

                    // can't delete topics or rules that are used as meaning.
                    if (DelIfEmpty.Contains(iMeaning) == false
                        && (!(iMeaning is NeuronCluster)
                            || (((NeuronCluster)iMeaning).Meaning != (ulong)PredefinedNeurons.TextPatternTopic
                                && ((NeuronCluster)iMeaning).Meaning != (ulong)PredefinedNeurons.PatternRule)))
                    {
                        DelIfEmpty.Add(iMeaning);
                    }
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iToCheck);
            }
        }

        /// <summary>Removes the starts. Can handle normal, var or sub starts.</summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="list">The list.</param>
        /// <param name="iToDeleteIfEmpty">The i to delete if empty.</param>
        private static void RemoveStarts(
            Neuron pattern, System.Collections.Generic.List<Link> list, System.Collections.Generic.HashSet<Neuron> iToDeleteIfEmpty)
        {
            foreach (var i in list)
            {
                if (Brain.Current.IsExistingID(i.ToID))
                {
                    // if the start was a var, it is already deleted.
                    var iTo = i.To;
                    if (iToDeleteIfEmpty.Contains(iTo) == false)
                    {
                        // try not to add duplicates, which is possible cause there can be multiple starts + they can point to the same word
                        var iLink = Link.Find(iTo, pattern, Brain.Current[(ulong)PredefinedNeurons.PatternRule]);

                            // any part can be used as a 'last item', if  this is the case, remvoe the link.
                        if (iLink != null)
                        {
                            // if the parse failed the last time, there is no closing link.
                            iLink.Destroy();
                        }

                        iToDeleteIfEmpty.Add(iTo);

                            // to is the textneuron that needs to be deleted if it is no longe used.
                    }

                    i.Destroy();
                    var iStartOf = iTo.FindAllIn((ulong)PredefinedNeurons.ParsedPatternStart);
                    if (iStartOf.Count == 0)
                    {
                        CleanLoop(pattern, iTo);
                        RemoveRefToSubTopicsAndRules(iTo);
                        var iClusterTo = iTo as NeuronCluster;
                        if (iClusterTo != null)
                        {
                            // for vars that are starting points, we still need to delete the link between the varcluster and the var neuron
                            NeuronCluster iParent = null;

                                // starting vars are clustered by a root object, we need to remove them from this root, otherwise we have a mem leak.
                            if (iClusterTo.Meaning == (ulong)PredefinedNeurons.ParsedVariable)
                            {
                                iParent = (NeuronCluster)Brain.Current[(ulong)PredefinedNeurons.ParsedVariable];
                            }
                            else if (iClusterTo.Meaning == (ulong)PredefinedNeurons.ParsedThesVar)
                            {
                                iParent = (NeuronCluster)Brain.Current[(ulong)PredefinedNeurons.ParsedThesVar];
                            }

                            using (var iChildren = iParent.ChildrenW) iChildren.Remove(iClusterTo);
                            CleanOutgoingLinks(iClusterTo, iToDeleteIfEmpty);
                            RemoveVarCluster(iClusterTo, iToDeleteIfEmpty);
                        }

                        CleanOutgoingLinks(iTo, iToDeleteIfEmpty);
                    }
                }
            }
        }

        /// <summary>remove all refs from SubTopics and SubRules.</summary>
        /// <param name="item"></param>
        private static void RemoveRefToSubTopicsAndRules(Neuron item)
        {
            var iToDel = new System.Collections.Generic.List<Link>();

                // need to keep trakc of all links that need to be removed, can't do delete inside loop cause then we change the list.
            using (var iLinks = item.LinksIn)
            {
                foreach (var i in iLinks)
                {
                    if (i.FromID == (ulong)PredefinedNeurons.SubRules || i.FromID == (ulong)PredefinedNeurons.SubTopics)
                    {
                        iToDel.Add(i);
                    }
                }
            }

            foreach (var i in iToDel)
            {
                i.Destroy();
            }
        }

        /// <summary>The clean loop.</summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="to">The to.</param>
        private static void CleanLoop(Neuron pattern, Neuron to)
        {
            var iLoop = Link.Find(to, to, to); // check if there is a feedback loop, if so, remove that as well.
            if (iLoop != null)
            {
                var iDel = false; // we can only delete the link if it is no longer used by any other pattern.
                var iInfo = iLoop.InfoW;
                iInfo.Lock(pattern);
                try
                {
                    if (iInfo.List.Contains(pattern.ID))
                    {
                        iInfo.List.Remove(pattern.ID);
                    }

                    iDel = iInfo.List.Count == 0;
                }
                finally
                {
                    iInfo.Unlock(pattern);
                }

                if (iDel)
                {
                    iLoop.Destroy();
                }
            }
        }

        /// <summary>
        ///     Closes the parsed pattern definition, so that at the end of the path,
        ///     the pattern defition itself is found, for transition to an output.
        /// </summary>
        private void ClosePatternDef()
        {
            System.Collections.Generic.List<Neuron> iDuplicates = null;
            if (fCurrentPos != null)
            {
                var iIsDupliate = CheckDuplicate(fCurrentPos, ref iDuplicates);
                if (Link.Exists(fCurrentPos, fToParse, (ulong)PredefinedNeurons.PatternRule) == false)
                {
                    Link.Create(fCurrentPos, fToParse, (ulong)PredefinedNeurons.PatternRule);
                }

                CheckDuplicate(iIsDupliate, iDuplicates);
            }

            foreach (var i in fExtraCurrents)
            {
                // also need to do the same for the extra paths that had to be created because of the {} [] or | operators
                if (Link.Exists(i, fToParse, (ulong)PredefinedNeurons.PatternRule) == false)
                {
                    // could already have been created.
                    var iIsDupliate = CheckDuplicate(i, ref iDuplicates);
                    Link.Create(i, fToParse, (ulong)PredefinedNeurons.PatternRule);
                    CheckDuplicate(iIsDupliate, iDuplicates);
                }
            }

            if (fGroupings.Count > 0)
            {
                switch (fGroupings.Peek().GroupingType)
                {
                    case Token.OptionStart:
                        LogPosError("Option is not closed properly.");
                        break;
                    case Token.LoopStart:
                        LogPosError("Loop is not closed properly.");
                        break;
                    case Token.GroupStart:
                        LogPosError("Group is not closed properly.");
                        break;
                    default:
                        LogPosError("Invalid and unclosed type of grouping found.");
                        break;
                }
            }
        }

        /// <summary>The check duplicate.</summary>
        /// <param name="isDupliate">The is dupliate.</param>
        /// <param name="duplicates">The duplicates.</param>
        private void CheckDuplicate(bool isDupliate, System.Collections.Generic.List<Neuron> duplicates)
        {
            // static LogMethod fDuplicatePatternLogMethod = LogMethod.Warning;
            if (isDupliate)
            {
                // log the error after we make the connection, cause usully the parsed path will have to be deleted, we best provide all the info for that.
                switch (Settings.DuplicatePatternLogMethod)
                {
                    case LogMethod.Info:
                        LogPosInfo("Duplicate patterns detected: " + GetRuleNames(duplicates));
                        break;
                    case LogMethod.Warning:
                        LogPosWarning("Duplicate patterns detected: " + GetRuleNames(duplicates));
                        break;
                    case LogMethod.Error:
                        LogPosError("Duplicate patterns detected: " + GetRuleNames(duplicates));
                        break;
                }
            }
        }

        /// <summary>walks through the list of <paramref name="rules"/> and builds for each
        ///     rule, a name in the form of: {topicname}.{rulename} This is used to
        ///     print duplicates.</summary>
        /// <param name="rules"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetRuleNames(System.Collections.Generic.List<Neuron> rules)
        {
            var iRes = new System.Text.StringBuilder();
            foreach (var i in rules)
            {
                Neuron iRule = i.FindFirstClusteredBy((ulong)PredefinedNeurons.PatternRule);
                if (iRule != null)
                {
                    if (iRes.Length > 0)
                    {
                        iRes.Append(", ");
                    }

                    var iRuleName = iRule.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);
                    Neuron iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
                    if (iTopic != null)
                    {
                        var iName = TopicsDictionary.NameGetter(iTopic.ID);
                        if (string.IsNullOrEmpty(iName) == false)
                        {
                            iRes.AppendFormat("{0}.", iName);
                        }
                    }

                    if (iRuleName != null)
                    {
                        iRes.Append(BrainHelper.GetTextFrom(iRuleName));
                    }
                }
            }

            return iRes.ToString();
        }

        /// <summary>Checks if the pattern is a duplicate of another pattern, keeping
        ///     account for partials. When it's a partial, we check if there is any
        ///     partial from the same editor, or if it's a fallback, if there aren't
        ///     any other fallbacks already.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="duplicates">The duplicates.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckDuplicate(Neuron toCheck, ref System.Collections.Generic.List<Neuron> duplicates)
        {
            var iPartialMode = fToParse.FindFirstOut((ulong)PredefinedNeurons.InputPatternPartialMode);
            if (iPartialMode == null)
            {
                duplicates = toCheck.FindAllOut((ulong)PredefinedNeurons.PatternRule);
                return duplicates != null && duplicates.Count > 0 && CheckStartAndEndForDuplicate(fToParse, duplicates);
            }

            var iRule = fToParse.FindFirstClusteredBy((ulong)PredefinedNeurons.PatternRule);

                // get the rule so we can get the topic
            if (iRule != null)
            {
                var iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);

                    // get the topic so we can check if there is any other textpattern that 'toCheck' points to which are also in the topic.
                if (iTopic != null)
                {
                    var iAllPatterns = toCheck.FindAllOut((ulong)PredefinedNeurons.PatternRule);
                    if (iPartialMode.ID == (ulong)PredefinedNeurons.PartialInputPattern)
                    {
                        // it's a normal partial
                        foreach (var i in iAllPatterns)
                        {
                            iPartialMode = i.FindFirstOut((ulong)PredefinedNeurons.InputPatternPartialMode);
                            if (iPartialMode == null)
                            {
                                // another one is still marked as none partial
                                return true;
                            }

                            iRule = i.FindFirstClusteredBy((ulong)PredefinedNeurons.PatternRule);
                            using (var iList = iTopic.Children)
                                if (iList.Contains(iRule)
                                    && iPartialMode.ID != (ulong)PredefinedNeurons.PartialInputPatternFallback)
                                {
                                    // the fallback is also allowewd, we don't need to check against that.
                                    duplicates.Add(i);
                                }
                        }
                    }
                    else
                    {
                        foreach (var i in iAllPatterns)
                        {
                            iPartialMode = i.FindFirstOut((ulong)PredefinedNeurons.InputPatternPartialMode);
                            if (iPartialMode == null)
                            {
                                // another one is still marked as none partial
                                return true;
                            }

                            if (iPartialMode.ID == (ulong)PredefinedNeurons.PartialInputPatternFallback)
                            {
                                duplicates.Add(i);
                            }
                        }
                    }

                    if (duplicates != null && duplicates.Count > 0)
                    {
                        return CheckStartAndEndForDuplicate(fToParse, duplicates);
                    }
                }
            }

            return false;
        }

        /// <summary>Checks the first argument against to list, to see if there isn't any
        ///     other item in the list tha has the same settings for start and end
        ///     positions of the pattern in the input.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckStartAndEndForDuplicate(Neuron toCheck, System.Collections.Generic.List<Neuron> items)
        {
            var iAtStart = toCheck.FindFirstOut((ulong)PredefinedNeurons.PatternAtStartOfInput) != null;
            var iAtEnd = toCheck.FindFirstOut((ulong)PredefinedNeurons.PatternAtEndOfInput) != null;

            foreach (var i in items)
            {
                var iSecAtStart = i.FindFirstOut((ulong)PredefinedNeurons.PatternAtStartOfInput) != null;
                var iSecAtEnd = i.FindFirstOut((ulong)PredefinedNeurons.PatternAtEndOfInput) != null;
                if (iSecAtEnd == iAtEnd && iSecAtStart == iAtStart)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The prepare start.</summary>
        private void PrepareStart()
        {
            while (fCurrentPos == null && Scanner.CurrentToken != Token.End)
            {
                SkipSpaces();
                switch (Scanner.CurrentToken)
                {
                    case Token.Word:
                        HandleStartingWord();
                        break;
                    case Token.Variable:
                        HandleVariable();
                        break;
                    case Token.ThesVariable:
                        HandleThesVariable();
                        break;
                    case Token.AssetVariable:
                        HandleAssetVar();
                        break;
                    case Token.OptionStart:
                        StartGrouping(Token.OptionStart);
                        Scanner.GetNext();
                        break;
                    case Token.GroupStart:
                        StartGrouping(Token.GroupStart);
                        Scanner.GetNext();
                        break;
                    case Token.LoopStart:
                        StartGrouping(Token.LoopStart);
                        Scanner.GetNext();
                        break;
                    case Token.DoubleAnd:
                        HandleAnd();
                        Scanner.GetNext();
                        break;
                    case Token.StartOfInput:
                        HandleStartOfInput();
                        Scanner.GetNext();
                        break;
                    case Token.TopicRef:
                        HandleTopic(true);
                        break;
                    case Token.GroupEnd:
                    case Token.OptionEnd:
                    case Token.LoopEnd:
                        LogError(string.Format("Invalid start", Scanner.ToParse));
                        break;
                    case Token.End:
                        break;
                    default: // all others are just words.
                        HandleStartingWord();
                        break;
                }
            }

            if (fCurrentPos != null
                && Link.Exists(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternStart) == false)
            {
                // if the same word is used multiple times as a start, we don't need to create an extra link (would fail)
                Link.Create(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternStart);

                    // store the start of the pattern so we can delete it later on if it gets reparsed.
            }

            AddStartPointToGroup();
        }

        /// <summary>
        ///     Handles the start of input token, which indicates that the pattern
        ///     only matches if the first word of the input is matched by the pattern.
        /// </summary>
        private void HandleStartOfInput()
        {
            if (fCurrentPos != null)
            {
                LogPosError("|< can only be used at the start of the pattern.");
            }

            if (Link.Exists(fToParse, fToParse, (ulong)PredefinedNeurons.PatternAtStartOfInput) == false)
            {
                Link.Create(fToParse, fToParse, (ulong)PredefinedNeurons.PatternAtStartOfInput);
            }
            else
            {
                LogPosError("Duplicate |<");
            }
        }

        /// <summary>
        ///     Adds the the currentPos item to the list of starting points of a loop.
        ///     this is used so we can close the loop at the end.
        /// </summary>
        private void AddStartPointToGroup()
        {
            if (fGroupings.Count > 0)
            {
                var iLast = fGroupings.Peek();
                if (iLast.GroupingType == Token.LoopStart)
                {
                    iLast.StartingPoints.Add(fCurrentPos);
                }

                fPrevWasChoice = false;

                    // we have just added a starting point, so the even if the prev was a choice, we have already collected the starting point. This situation can happen when a group/option/loop was at the start and it contains |  ,meaning you have multiple starting points.
            }
        }

        /// <summary>
        ///     Checks if the <see cref="fPrevWasChoice" /> flag is set (the item
        ///     before <see cref="fCurrentPos" /> was a choice), if so we need to
        ///     collect the new currentPos as the start of a section between '|..|' or
        ///     '|..}' or '|..]'
        /// </summary>
        private void CheckIfPrevWasChoice()
        {
            if (fPrevWasChoice)
            {
                if (fPrevPos != fCurrentPos)
                {
                    // we can only collect the new item when there was a change.
                    if (fGroupings.Count > 0)
                    {
                        var iLast = fGroupings.Peek();
                        if (fPrevWasAnd == false)
                        {
                            // when we are handling the loop start of a pattern like '(a) &&{b}', we need to use the prevPos as startingpoint and not the current pos, cause that is pointing to the && neuron, which is what we need for handling pattern endings.
                            iLast.StartingPoints.Add(fCurrentPos);
                        }
                        else
                        {
                            iLast.StartingPoints.Add(iLast.PrevPos);
                        }
                    }
                    else
                    {
                        LogPosError("Internal error: grouping expected!");
                    }

                    fPrevWasChoice = false;
                }
            }
        }

        /// <summary>The check close grouping.</summary>
        /// <param name="token">The token.</param>
        private void CheckCloseGrouping(Token token)
        {
            if (fGroupings.Count > 0)
            {
                var iLast = fGroupings.Pop();
                if (iLast.IsReverse(token) == false)
                {
                    LogPosError(
                        string.Format("Invalid closing brackets: {0} expected.", Tokenizer.GetSymbolForToken(token)));
                    if (token != Token.GroupEnd)
                    {
                        fPRevWasGroup = false; // so we can handle the && operator correct.
                    }
                }
                else
                {
                    if (token != Token.GroupEnd)
                    {
                        // the option and loop can use the previous position + the current last as starting points.
                        iLast.EndPoints.Add(fCurrentPos);

                            // don't do this when it's a grouping, cause this remains the currentpos for a grouping -> it can't continue the path from the item that was before the grouping, this is only reserved for options and loops.
                        iLast.EndPoints.AddRange(fExtraCurrents);

                            // the extra currents are also end points, we need those for closing the loop. 
                        fExtraCurrents.Clear();

                            // Cause the endpoints will become the new extra currents, we need to clear the list of extra currents before filling it again.
                        if (token == Token.LoopEnd)
                        {
                            CloseLoop(iLast);
                        }

                        fCurrentPos = iLast.PrevPos;

                            // this provides the option part: the next item can follow the current or the one before the option/loop
                        fExtraCurrents.AddRange(iLast.ExtraCurrents);

                            // the previous extra currents still need to remain so.  
                        fPRevWasGroup = false; // so we can handle the && operator correct.
                    }

                    fPrevWasAnd = false; // when we close a grouping, a previous && operator is completely finished.
                    fPRevWasGroup = true; // so we can check if the && operator is allowed.
                    fExtraCurrents.AddRange(iLast.EndPoints);

                        // we add all the end points in the grouping, to the extra curents, so that the paths continue.
                }
            }
            else
            {
                LogPosError("Invalid closing brackets: no corresponding starting brackets.");
                if (token != Token.GroupEnd)
                {
                    fPRevWasGroup = false; // so we can handle the && operator correct.
                }
            }
        }

        /// <summary>Closes the loop in the parsed patten so that the last items (all items
        ///     just before a | or } ) can link back to all the first items. So: every
        ///     end <paramref name="point"/> can link back to every starting point.</summary>
        /// <param name="point">The point.</param>
        private void CloseLoop(GroupingPoint point)
        {
            for (var iCount = 0; iCount < point.EndPoints.Count; iCount++)
            {
                var i = point.EndPoints[iCount];
                foreach (var iTo in point.StartingPoints)
                {
                    if (iTo is TextNeuron)
                    {
                        // a textneuron is a startingpoint in the parse. so we use the textneuron itself to go to the next item, it's a feedback loop.
                        CloseLoopForTextNeuron(i, (TextNeuron)iTo, point.EndPoints, iCount);
                    }
                    else if (iTo is NeuronCluster)
                    {
                        // the start is some sort of variable, so check if this is legal (the endpoint is not a var with unspecified length, followed by the start which is also a var) and if so, collect the variable in the var collection list.
                        var iToCluster = (NeuronCluster)iTo;
                        if (iToCluster.Meaning == (ulong)PredefinedNeurons.ParsedVariable)
                        {
                            CheckPrevIsNotVar(iToCluster);
                        }

                        var iCol = GetVarCollection(i, iToCluster.Meaning);

                        var iParent = iTo.FindFirstClusteredBy((ulong)PredefinedNeurons.ParsedThesVar);

                            // if the starting point (to link back to), is a thes var, get the parent cluster that has the compiled tree structure, so we can link to it again.
                        if (iParent != null)
                        {
                            var iLinks = Factories.Default.LinkLists.GetBuffer();
                            using (var iList = iParent.LinksOut) iLinks.AddRange(iList); // make a local copy so it is treadsave (cachewrites).
                            foreach (var iLink in iLinks)
                            {
                                Link.Create(iCol, iLink.To, iLink.Meaning);

                                    // by duplicating the outgoing links, we duplicate all compiled trees.
                            }

                            Factories.Default.LinkLists.Recycle(iLinks);
                        }

                        using (var iChildren = iCol.ChildrenW) iChildren.Add(iTo);
                    }
                    else
                    {
                        using (var iLinks = iTo.LinksIn)
                        {
                            System.Diagnostics.Debug.Assert(iLinks.Count > 0);
                            var iMeaning = iLinks[0].Meaning;

                                // it's a regular neuron, which can be reached by a link pointing to it, the meaning of all these links, is the same text neuron, cause that's what this iTo represents
                            if (iMeaning is NeuronCluster)
                            {
                                // if the meaning is a topic or rule, we are referencing back to the start of the pattern, which is another sub topic/rule, which needs to be reffed differently.
                                if (CloseLoopForSub(i, iTo, (NeuronCluster)iMeaning) == false
                                    && Link.Exists(i, iTo, iMeaning.ID) == false)
                                {
                                    Link.Create(i, iTo, iMeaning);
                                }
                            }
                            else
                            {
                                if (Link.Exists(i, iTo, iMeaning.ID) == false)
                                {
                                    Link.Create(i, iTo, iMeaning);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>The close loop for sub.</summary>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CloseLoopForSub(Neuron from, Neuron to, NeuronCluster meaning)
        {
            ulong iMeaning;
            if (meaning.Meaning == (ulong)PredefinedNeurons.PatternRule)
            {
                iMeaning = (ulong)PredefinedNeurons.SubRules;
            }
            else if (meaning.Meaning == (ulong)PredefinedNeurons.SubRules)
            {
                iMeaning = (ulong)PredefinedNeurons.SubTopics;
            }
            else
            {
                iMeaning = 0;
            }

            if (iMeaning != 0)
            {
                NeuronCluster iList;
                iList = from.FindFirstOut(iMeaning) as NeuronCluster;
                if (iList == null)
                {
                    iList = NeuronFactory.GetCluster();
                    Brain.Current.Add(iList);
                    iList.Meaning = iMeaning;
                    Link.Create(from, iList, iMeaning);
                }

                var iSub = NeuronFactory.GetNeuron();
                Brain.Current.Add(iSub);
                using (var iChildren = iList.ChildrenW) iChildren.Add(iSub);
                Link.Create(iSub, to, meaning); // this is the final step.
                return true;
            }

            return false;
        }

        /// <summary>The close loop for text neuron.</summary>
        /// <param name="i">The i.</param>
        /// <param name="iTo">The i to.</param>
        /// <param name="endPoints">The end points.</param>
        /// <param name="iCount">The i count.</param>
        private void CloseLoopForTextNeuron(
            Neuron i, 
            TextNeuron iTo, System.Collections.Generic.List<Neuron> endPoints, 
            int iCount)
        {
            if (i != iTo)
            {
                var iLoop = Link.Find(i, iTo, iTo);

                    // if the link already exists, it depends on what the end point is: if it's a regular one, it's ok, everything is set up. 
                if (iLoop == null)
                {
                    iLoop = Link.Create(i, iTo, iTo);
                }
            }
            else
            {
                var iNew = NeuronFactory.GetNeuron();

                    // add an extra step, so we make certain that the path becomes unique and the loop doesn't intermingle with other pattersn.
                Brain.Current.Add(iNew);
                Link.Create(iTo, iNew, iTo);
                Link.Create(fToParse, iNew, (ulong)PredefinedNeurons.ParsedPatternPart);
                Link.Create(iNew, iNew, iTo); // this is the actual closed loop.
                endPoints[iCount] = iNew; // need to replace the endpoint, so that there is a proper continuation.
            }
        }

        /// <summary>Creates a new grouping point for the specified type.</summary>
        /// <param name="token">The token.</param>
        private void StartGrouping(Token token)
        {
            var iNew = new GroupingPoint();
            iNew.GroupingType = token;
            iNew.PrevPos = fCurrentPos;
            iNew.ExtraCurrents.AddRange(fExtraCurrents);

                // need to back these up as well. When we have a choice, we need to restore to this point.
            iNew.PrevWasAnd = fPrevWasAnd;
            fGroupings.Push(iNew);
            if (token != Token.GroupStart)
            {
                // for loops, we need to store every start point, also the first one.
                fPrevWasChoice = true;
                fPrevPos = fCurrentPos;
            }

            fPRevWasGroup = false; // so we can handle the && operator correct.
            fPrevWasAnd = false;
        }

        /// <summary>
        ///     reads an asset path (simple form with no refs to other variables) see
        ///     nnlParser for full definition of asset path (depends on how it is
        ///     declared in the nnl definition).
        /// </summary>
        private void HandleAssetVar()
        {
            if (OutputParser.ExpressionsHandler == null)
            {
                LogPosError("can't process asset paths in input: no module found that handles the bindings.");
            }
            else
            {
                Neuron iRes = (NeuronCluster)OutputParser.ExpressionsHandler.GetExpressionFrom(ParserTitle, Scanner);
                Neuron iVarCol;
                if (fCurrentPos != null)
                {
                    iVarCol = GetSubPoint(fCurrentPos, (ulong)PredefinedNeurons.ParsedAssetVar);
                }
                else if (fLocalTo == null)
                {
                    iVarCol = Brain.Current[(ulong)PredefinedNeurons.ParsedAssetVar];
                }
                else
                {
                    iVarCol = GetSubPoint(fLocalTo, (ulong)PredefinedNeurons.ParsedAssetVar);

                        // parsing local to the topic, so add the thespath to the topic instead of global.
                }

                fCurrentPos = iVarCol.FindFirstOut(iRes.ID);
                if (fCurrentPos == null)
                {
                    fCurrentPos = NeuronFactory.GetNeuron();
                    Brain.Current.Add(fCurrentPos);
                    Link.Create(iVarCol, fCurrentPos, iRes);
                }

                if (Link.Exists(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedAssetVar) == false)
                {
                    // we also create a link between the pattern and the parsed thes variable (using a normal parsed var link), oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                    Link.Create(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedAssetVar);
                }

                if (fExtraCurrents.Count > 0)
                {
                    // also need to process the extra current items.
                    var iNewCurrents = new System.Collections.Generic.List<Neuron>();
                    foreach (var i in fExtraCurrents)
                    {
                        iVarCol = GetSubPoint(i, (ulong)PredefinedNeurons.ParsedAssetVar);
                        var iVar = iVarCol.FindFirstOut(iRes.ID);
                        if (iVar == null)
                        {
                            iVar = NeuronFactory.GetNeuron();
                            Brain.Current.Add(iVar);
                            Link.Create(iVarCol, iVar, iRes);
                        }

                        if (Link.Exists(fToParse, iVar, (ulong)PredefinedNeurons.ParsedAssetVar) == false)
                        {
                            // we also create a link between the pattern and the parsed thes variable (using a normal parsed var link), oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                            Link.Create(fToParse, iVar, (ulong)PredefinedNeurons.ParsedAssetVar);
                        }

                        iNewCurrents.Add(iVar);
                    }

                    fExtraCurrents = iNewCurrents;
                }
            }
        }

        /// <summary>
        ///     reads in the contents of a thes variable and processes it.
        /// </summary>
        /// <remarks>
        ///     thesaurus variable = '^'identifier ['->' relationship ] ':' pos-type {
        ///     PathItem } [:Conjugation] ; PathItem = '.' ( identifier | '('
        ///     identifier ')' | '*' ); conjugation = one of the predefined neurons.
        /// </remarks>
        private void HandleThesVariable()
        {
            Neuron iRel = null;
            fPRevWasGroup = false; // so we can handle the && operator correct.
            fPrevWasAnd = false;
            Scanner.GetNext(); // skip the thes var sign.
            if (Scanner.CurrentToken != Token.Word)
            {
                LogPosError("Please provide a name for the thesaurus variable, an identifier is expected.");
            }

            var iPath = new System.Collections.Generic.List<Neuron>();
            var iVarName = Scanner.CurrentValue;
            Scanner.GetNext();

            if (Scanner.CurrentToken == Token.ArrowRight)
            {
                // read relationship type.
                iRel = ReadThesRelationhip();
                if (iRel != null)
                {
                    iPath.Add(iRel);
                }
                else
                {
                    LogPosError("Unknown relationship type");
                }
            }

            if (Scanner.CurrentToken != Token.LengthSpec)
            {
                LogPosError("Please provide a thesaurus variable path, ':' expected.");
            }

            AdvanceWithSpaceSkip();
            var iPos = ReadThesPos();
            if (iPos != null)
            {
                iPath.Add(iPos);
            }
            else
            {
                LogPosError("Unknown part of speech value");
            }

            while (Scanner.CurrentToken == Token.Dot)
            {
                if (iPos.ID == (ulong)PredefinedNeurons.IntNeuron || iPos.ID == (ulong)PredefinedNeurons.DoubleNeuron
                    || iPos.ID == (ulong)PredefinedNeurons.Number)
                {
                    LogPosError("Thesaurus number references can't have further path identifiers");
                }

                Scanner.GetNext();
                if (Scanner.CurrentValue == "*")
                {
                    // a star means that anything is allowed.
                    iPath.Add(Brain.Current[(ulong)PredefinedNeurons.Empty]);
                }
                else
                {
                    iPath.Add(GetBrackedtedIdentifier());
                }

                Scanner.GetNext();
            }

            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                Scanner.GetNext();
                var iName = ReadBracketedId();
                ulong iFound;
                if (Statics.TryGetValue(iName, out iFound))
                {
                    iPath.Add(Brain.Current[iFound]);
                }
                else
                {
                    LogPosError("Unknown conjugation: " + iName);
                }
            }

            AddThesVar(iVarName, iPath, iRel, iPos);
        }

        /// <summary>The read thes relationhip.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadThesRelationhip()
        {
            Neuron iRes = null;
            var iName = GetBrackedtedText();
            var iRels = Brain.Current[(ulong)PredefinedNeurons.WordNetRelationships] as NeuronCluster;
            if (iRels != null)
            {
                System.Collections.Generic.List<ulong> iChildren = null;
                using (var iList = iRels.Children)
                {
                    iChildren = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);

                        // make local copy so we dont create a deadlock when accessing the cache.
                    iChildren.AddRange(iList);
                }

                try
                {
                    foreach (var i in iChildren)
                    {
                        if (TopicsDictionary.NameGetter(i) == iName)
                        {
                            iRes = Brain.Current[i];
                            break;
                        }
                    }
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iChildren);
                }
            }

            if (iRes != null)
            {
                // only advance if there was a pos declared, othewise we need to continue searching.
                Scanner.GetNext();
            }

            return iRes;
        }

        /// <summary>reads a string identifier that can possibly be enclosed in brackets,
        ///     like '(' ')'</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetBrackedtedIdentifier()
        {
            var iNeedClose = false;
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                Scanner.GetNext();
                iNeedClose = true;
            }

            if (iNeedClose == false && Scanner.CurrentToken != Token.Word)
            {
                // only check for a valid word if it isn't in brackets.
                LogPosError("Please provide a valid identifier.");
            }

            var iId = TextNeuron.GetFor(Scanner.CurrentValue);
            if (iNeedClose)
            {
                var iCompound = new System.Collections.Generic.List<Neuron>();
                iCompound.Add(iId);
                AdvanceWithSpaceSkip();
                while (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End)
                {
                    iCompound.Add(TextNeuron.GetFor(Scanner.CurrentValue));
                    AdvanceWithSpaceSkip();
                }

                if (Scanner.CurrentToken == Token.End)
                {
                    LogPosError("Closing brackets ')' expected.");
                }

                return BrainHelper.GetCompoundWord(iCompound);
            }

            return iId;
        }

        /// <summary>The get brackedted text.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetBrackedtedText()
        {
            var iRes = new System.Text.StringBuilder();
            var iNeedClose = false;
            Scanner.GetNext();
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                Scanner.GetNext();
                iNeedClose = true;
            }

            if (iNeedClose == false && Scanner.CurrentToken != Token.Word)
            {
                // only check for a valid word if it isn't in brackets.
                LogPosError("Please provide a valid identifier.");
            }

            iRes.Append(Scanner.CurrentValue);
            if (iNeedClose)
            {
                Scanner.GetNext();
                if (Scanner.CurrentToken == Token.Space)
                {
                    // need to make certain that spaces are always 1 char long.
                    iRes.Append(" ");
                    SkipSpaces();
                }

                while (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End)
                {
                    iRes.Append(Scanner.CurrentValue);
                    Scanner.GetNext();
                    if (Scanner.CurrentToken == Token.Space)
                    {
                        iRes.Append(" ");
                        SkipSpaces();
                    }
                }

                if (Scanner.CurrentToken == Token.End)
                {
                    LogPosError("Closing brackets ')' expected.");
                }
            }

            return iRes.ToString();
        }

        /// <summary>
        ///     reads in the contents of a variable and processes it.
        /// </summary>
        /// <remarks>
        ///     Normal variable =
        ///     '$'identifier[':'length['-'range]][:collectspaces]['%'double];
        /// </remarks>
        private void HandleVariable()
        {
            if (fPrevWasAnd)
            {
                LogPosError("A variable can't be used as the start after a && operator.");
            }

            fPRevWasGroup = false; // so we can handle the && operator correct.
            fPrevWasAnd = false;
            Scanner.GetNext(); // skip the var sign.
            if (Scanner.CurrentToken != Token.Word)
            {
                LogPosError("Please provide a name for the variable, an identifier is expected.");
            }

            var iVarName = Scanner.CurrentValue;
            Scanner.GetNext();
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                Scanner.GetNext();
                int? iLength = null;
                int? iRange = null;
                var icollectSpaces = false;
                if (Scanner.WordType != WordTokenType.Word)
                {
                    // it's a range first.
                    int iVal;
                    if (int.TryParse(Scanner.CurrentValue, out iVal))
                    {
                        iLength = iVal;
                    }
                    else
                    {
                        LogPosError("Integer expected as length specifier for a variable");
                    }

                    Scanner.GetNext();
                    if (Scanner.CurrentToken == Token.Minus)
                    {
                        Scanner.GetNext();
                        if (Scanner.CurrentToken == Token.Word)
                        {
                            if (int.TryParse(Scanner.CurrentValue, out iVal))
                            {
                                iRange = iVal;
                            }
                            else
                            {
                                LogPosError("Integer expected as range specifier for a variable");
                            }
                        }
                        else
                        {
                            LogPosError("Integer expected as range specifier for a variable");
                        }

                        Scanner.GetNext();
                    }

                    if (Scanner.CurrentToken == Token.LengthSpec)
                    {
                        Scanner.GetNext();
                        if (Scanner.CurrentValue.ToLower() == "collectspaces")
                        {
                            icollectSpaces = true;
                        }
                        else
                        {
                            LogPosError("'CollectSpaces' expected");
                        }

                        Scanner.GetNext();
                    }
                }
                else if (Scanner.CurrentValue.ToLower() == "collectspaces")
                {
                    icollectSpaces = true;
                    Scanner.GetNext();
                }
                else
                {
                    LogPosError("'CollectSpaces' expected");
                }

                var iWeight = ReadVarWeight();
                AddVariable(iVarName, iLength, iRange, icollectSpaces, iWeight);
            }
            else
            {
                var iWeight = ReadVarWeight();
                AddVariable(iVarName, null, null, false, iWeight);
            }
        }

        /// <summary>The read var weight.</summary>
        /// <returns>The <see cref="double"/>.</returns>
        private double ReadVarWeight()
        {
            if (Scanner.CurrentToken == Token.Modulus)
            {
                double iVal = 0;
                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken == Token.Word)
                {
                    if (Scanner.WordType == WordTokenType.Decimal)
                    {
                        iVal = double.Parse(Scanner.CurrentValue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (Scanner.WordType == WordTokenType.Integer)
                    {
                        iVal = int.Parse(Scanner.CurrentValue, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        LogPosError("number expected", false);
                        AdvanceWithSpaceSkip();
                    }
                }
                else
                {
                    LogPosError("number expected", false);
                    AdvanceWithSpaceSkip();
                }

                AdvanceWithSpaceSkip();
                return iVal;
            }

            return 0.0;
        }

        /// <summary>Adds the thes var.</summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="pos">The pos.</param>
        private void AddThesVar(string name, System.Collections.Generic.List<Neuron> path, Neuron rel, Neuron pos)
        {
            NeuronCluster iVar = null;
            NeuronCluster iVarCol;
            if (fCurrentPos != null)
            {
                iVarCol = GetVarCollection(fCurrentPos, (ulong)PredefinedNeurons.ParsedThesVar);
            }
            else if (fLocalTo == null)
            {
                iVarCol = Brain.Current[(ulong)PredefinedNeurons.ParsedThesVar] as NeuronCluster;
            }
            else
            {
                iVarCol = GetVarCollection(fLocalTo, (ulong)PredefinedNeurons.ParsedThesVar);

                    // parsing local to the topic, so add the thespath to the topic instead of global.
            }

            var iName = TextNeuron.GetFor(name); // get the name of the var and attach it to the 
            var iCollector = fToParse.FindFirstOut(iName.ID) as Variable;

                // we need to make certain that vars with the same name reference the same neuron.
            iVar = GetVarFrom(iVarCol, path, ref iCollector, rel);
            if (Link.Exists(fToParse, iCollector, iName.ID) == false)
            {
                // same name can be used multiple times.
                Link.Create(fToParse, iCollector, iName);

                    // we link to the collector, cause this will store the data during execution, which is what we really need, not the actual parsed var object.
            }

            if (Link.Exists(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable) == false)
            {
                // we also create a link between the pattern and the parsed thes variable (using a normal parsed var link), oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                Link.Create(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable);
            }

            fCurrentPos = iVar;

            if (fExtraCurrents.Count > 0)
            {
                // also need to process the extra current items.
                var iNewCurrents = new System.Collections.Generic.List<Neuron>();
                foreach (var i in fExtraCurrents)
                {
                    iVarCol = GetVarCollection(i, (ulong)PredefinedNeurons.ParsedThesVar);
                    iVar = GetVarFrom(iVarCol, path, ref iCollector, rel);
                    if (Link.Exists(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable) == false)
                    {
                        // we also create a link between the pattern and the parsed thes variable (using a normal parsed var link), oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                        Link.Create(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable);
                    }

                    iNewCurrents.Add(iVar);
                }

                fExtraCurrents = iNewCurrents;
            }
        }

        /// <summary>Creates a variable and adds it to the pattern path. When there is no
        ///     fCurrentPos, we need to add the variable as a start. Variables that
        ///     are the start of a pattern, are put into a special 'start' cluster, so
        ///     that the pattern matcher can easily find them + so that we can make
        ///     certain that there are no duplicates.</summary>
        /// <remarks><list type="bullet"><item><description>
        ///                 a var can not follow another var when the first doesn't have a or
        ///                 range. When there are multiple 'currentFrom's (because of | {} []
        ///                 operators, we must also generate vars for those paths.
        ///             </description></item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        /// <param name="range">The range.</param>
        /// <param name="collectSpaces">if set to <c>true</c> [collect spaces].</param>
        /// <param name="weight">The weight that has to be assigned to the variable. if 0, no weight is
        ///     assigned.</param>
        private void AddVariable(string name, int? length, int? range, bool collectSpaces, double weight)
        {
            if (fPrevWasAnd && fPRevWasGroup)
            {
                LogPosError("A variable can't be used as the start after a && operator.");
            }
            else
            {
                NeuronCluster iVar = null;
                NeuronCluster iVarCol;
                if (fCurrentPos != null)
                {
                    CheckPrevIsNotVar(fCurrentPos as NeuronCluster);
                    iVarCol = GetVarCollection(fCurrentPos, (ulong)PredefinedNeurons.ParsedVariable);
                }
                else if (fLocalTo == null)
                {
                    iVarCol = Brain.Current[(ulong)PredefinedNeurons.ParsedVariable] as NeuronCluster;
                }
                else
                {
                    iVarCol = GetVarCollection(fLocalTo, (ulong)PredefinedNeurons.ParsedVariable);

                        // if we are parsing something local to the topic, make certain we add it to the correct place.
                }

                var iName = TextNeuron.GetFor(name);

                    // get the name of the var and attach it to the patten, so we can retrieve the parsed var ref. 
                var iCollector = fToParse.FindFirstOut(iName.ID) as Variable;

                    // we need to make certain that vars with the same name reference the same neuron.
                iVar = GetVarFrom(iVarCol, length, range, collectSpaces, ref iCollector, weight);
                if (Link.Exists(fToParse, iCollector, iName.ID) == false)
                {
                    // same var name can be used multiple times, so link can already exist.
                    Link.Create(fToParse, iCollector, iName);

                        // we link to the collector, cause this will store the data during execution, which is what we really need, not the actual parsed var object.
                }

                if (Link.Exists(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable) == false)
                {
                    // we also create a link between the pattern and the parsed variable, oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                    Link.Create(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable);
                }

                fCurrentPos = iVar;

                if (fExtraCurrents.Count > 0)
                {
                    // also need to process the extra current items.
                    var iNewCurrents = new System.Collections.Generic.List<Neuron>();
                    foreach (var i in fExtraCurrents)
                    {
                        CheckPrevIsNotVar(i as NeuronCluster);
                        iVarCol = GetVarCollection(i, (ulong)PredefinedNeurons.ParsedVariable);
                        iVar = GetVarFrom(iVarCol, length, range, collectSpaces, ref iCollector, weight);
                        if (Link.Exists(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable) == false)
                        {
                            // we also create a link between the pattern and the parsed variable, oteherwise it becomes hard to find out which variables to remove for a specific pattern.
                            Link.Create(fToParse, iVar, (ulong)PredefinedNeurons.ParsedVariable);
                        }

                        iNewCurrents.Add(iVar);
                    }

                    fExtraCurrents = iNewCurrents;
                }
            }
        }

        /// <summary>check that we don't ahve 2 consecutive vars, where the first is of
        ///     undefined length: this is illegal.</summary>
        /// <param name="cluster">The cluster.</param>
        private void CheckPrevIsNotVar(NeuronCluster cluster)
        {
            if (cluster != null && cluster.Meaning == (ulong)PredefinedNeurons.ParsedVariable)
            {
                using (var iList = cluster.Children)
                    if (iList.Count == 0)
                    {
                        LogPosError(
                            "With 2 consecutive variables, the first needs to have a length and/or range value!");
                    }
            }
        }

        /// <summary><para>Checks if there is an outgoing link on the specified neuron, with the
        ///         meaning <see cref="JaStDev.HAB.PredefinedNeurons.ParsedVariable"/></para>
        /// <para>and if so, returns this. If there is non, a cluster is created and
        ///         attached to the argument, whichis returned.</para>
        /// </summary>
        /// <param name="from">The f current pos.</param>
        /// <param name="typeOfVar">The type Of Var.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster GetVarCollection(Neuron from, ulong typeOfVar)
        {
            var iFound = from.FindFirstOut(typeOfVar) as NeuronCluster;
            if (iFound == null)
            {
                iFound = NeuronFactory.GetCluster();
                Brain.Current.Add(iFound);
                iFound.Meaning = typeOfVar;
                Link.Create(from, iFound, typeOfVar);
            }

            return iFound;
        }

        /// <summary>Checks if there is an outgoing link on the specified neuron, with the
        ///     specified meaning and if so, returns this, otherwise it is created.</summary>
        /// <param name="from">From.</param>
        /// <param name="typeOfVar">The type of var.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetSubPoint(Neuron from, ulong typeOfVar)
        {
            var iFound = from.FindFirstOut(typeOfVar);
            if (iFound == null)
            {
                iFound = NeuronFactory.GetNeuron();
                Brain.Current.Add(iFound);
                Link.Create(from, iFound, typeOfVar);
            }

            return iFound;
        }

        /// <summary>Looks for a pattern-varaible <paramref name="cluster"/> in the
        ///     'ParsedVariables' <paramref name="cluster"/> that is passed on as
        ///     argument and returns the pattern-variable <paramref name="cluster"/>
        ///     with the same <paramref name="length"/> and <paramref name="range"/>
        ///     value as specified. If non can be found, a new one is created and
        ///     added to the cluster.</summary>
        /// <remarks>If the variable (used to collect all the input that is recognised by
        ///     the pattern-var, is already declared, we don't create a new one, but
        ///     make certain that the pattern-var can reach the variable. When this<paramref name="var"/> is the starting point in a loop {}, either
        ///     directly after the { or after a |, we always take a new<paramref name="var"/> cause we need to have a unique path for this.</remarks>
        /// <param name="cluster">The cluster.</param>
        /// <param name="length">The length.</param>
        /// <param name="range">The range.</param>
        /// <param name="collectSpaces">if set to <c>true</c> [collect spaces].</param>
        /// <param name="var">The var.</param>
        /// <param name="weight">The weight that the variable should have. When 0, no weight is used.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster GetVarFrom(
            NeuronCluster cluster, 
            int? length, 
            int? range, 
            bool collectSpaces, 
            ref Variable var, 
            double weight)
        {
            if (fPrevWasChoice == false || fGroupings.Count == 0 || fGroupings.Peek().GroupingType != Token.LoopStart)
            {
                // if this is a new starting point for a {}, we always want a new var cause we don't want to allow the mixing of different {} paths through each other. taking a new var gurarantees a unique path for each pattern.
                System.Collections.Generic.List<NeuronCluster> iChildren;
                using (var iList = cluster.Children) iChildren = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iTempList = Factories.Default.IDLists.GetBuffer();
                    try
                    {
                        foreach (var i in iChildren)
                        {
                            iTempList.Clear();
                            using (IDListAccessor iList = i.Children)
                                iTempList.AddRange(iList);

                                    // put this is a templist, so that we don't need to keep a lock on i.children, otherwise we geta potential deadlock.
                            if (CompareContent(iTempList, length, range)
                                && CompareWeight(i.FindFirstOut((ulong)PredefinedNeurons.Width), weight))
                            {
                                if ((i.FindFirstOut((ulong)PredefinedNeurons.CollectSpaces) != null) == collectSpaces)
                                {
                                    if (var == null)
                                    {
                                        var = NeuronFactory.Get<Variable>();

                                            // each pattern needs to have it's own variable for every name, can't reuse (2 patterns using the same var could not be found in the same result otherwise).
                                        Brain.Current.Add(var);
                                        Link.Create(i, var, (ulong)PredefinedNeurons.Variable);
                                    }
                                    else if (Link.Exists(i, var, (ulong)PredefinedNeurons.Variable) == false)
                                    {
                                        Link.Create(i, var, (ulong)PredefinedNeurons.Variable);
                                    }

                                    return i;
                                }
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.IDLists.Recycle(iTempList);
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iChildren);
                }
            }

            var iRes = NeuronFactory.GetCluster(); // no result found, so create one.
            Brain.Current.Add(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.ParsedVariable;
            if (length.HasValue)
            {
                // add the length ifthere is one, in the first pos.
                var iVal = NeuronFactory.GetInt(length.Value);
                Brain.Current.Add(iVal);
                using (var iChildren = iRes.ChildrenW) iChildren.Add(iVal);
            }

            if (range.HasValue)
            {
                // add the range if there is one, in the first pos.
                var iVal = NeuronFactory.GetInt(range.Value);
                Brain.Current.Add(iVal);
                using (var iChildren = iRes.ChildrenW) iChildren.Add(iVal);
            }

            if (var == null)
            {
                var = NeuronFactory.Get<Variable>();

                    // new variable neuron is also needed that will store the collected values.
                Brain.Current.Add(var);
            }

            Link.Create(iRes, var, (ulong)PredefinedNeurons.Variable);
            if (collectSpaces)
            {
                Link.Create(iRes, iRes, (ulong)PredefinedNeurons.CollectSpaces);
            }

            if (weight != 0)
            {
                // if there is a weight, attach it to the variable.
                var iWeight = NeuronFactory.GetDouble(weight);
                Brain.Current.Add(iWeight);
                Link.Create(iRes, iWeight, (ulong)PredefinedNeurons.Width);
            }

            using (var iChildren = cluster.ChildrenW) iChildren.Add(iRes);
            return iRes;
        }

        /// <summary>checks if the <paramref name="weight"/> attached to the<paramref name="neuron"/> is the same as the specified<paramref name="weight"/> (when no <paramref name="weight"/>
        ///     specified, the value should be 0).</summary>
        /// <param name="neuron"></param>
        /// <param name="weight"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareWeight(Neuron neuron, double weight)
        {
            if (neuron != null)
            {
                double iValue;
                if (Instruction.TryGetAsDouble(neuron, out iValue))
                {
                    return weight == iValue;
                }

                return false;
            }

            return weight == 0;
        }

        /// <summary>Looks for an already existing thes <paramref name="var"/> in the
        ///     specified list, for the specified path. If non exists, a new one is
        ///     created and added to this list.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="path">The path.</param>
        /// <param name="var">The neuron that will be used to store any data in that is collected
        ///     by this variable.</param>
        /// <param name="rel">The relationship neurron (if any), for building the tree quickly
        ///     (should also be included in the path)</param>
        /// <returns>a <paramref name="cluster"/> that can be used in the parse path. It
        ///     declares how to calculate the value during input.</returns>
        private NeuronCluster GetVarFrom(
            NeuronCluster cluster, System.Collections.Generic.List<Neuron> path, 
            ref Variable var, 
            Neuron rel)
        {
            if (fPrevWasChoice == false || fGroupings.Count == 0 || fGroupings.Peek().GroupingType != Token.LoopStart)
            {
                // if this is a new starting point for a {}, we always want a new var cause we don't want to allow the mixing of different {} paths through each other. taking a new var gurarantees a unique path for each pattern.
                System.Collections.Generic.List<NeuronCluster> iChildren;
                using (var iList = cluster.Children) iChildren = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iTempList = Factories.Default.IDLists.GetBuffer();
                    try
                    {
                        foreach (var i in iChildren)
                        {
                            iTempList.Clear();
                            using (IDListAccessor iList = i.Children)
                                iTempList.AddRange(iList);

                                    // put this is a templist, so that we don't need to keep a lock on i.children, otherwise we geta potential deadlock.
                            if (CompareContent(iTempList, path))
                            {
                                if (var == null)
                                {
                                    var = NeuronFactory.Get<Variable>();

                                        // each pattern needs to have it's own variable for every name, can't reuse (2 patterns using the same var could not be found in the same result otherwise).
                                    Brain.Current.Add(var);
                                    Link.Create(i, var, (ulong)PredefinedNeurons.Variable);
                                }
                                else if (Link.Exists(i, var, (ulong)PredefinedNeurons.Variable) == false)
                                {
                                    Link.Create(i, var, (ulong)PredefinedNeurons.Variable);
                                }

                                return i;
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.IDLists.Recycle(iTempList);
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iChildren);
                }
            }

            return BuildNewThesVar(cluster, path, ref var, rel);
        }

        /// <summary>The build new thes var.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="path">The path.</param>
        /// <param name="var">The var.</param>
        /// <param name="rel">The rel.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private static NeuronCluster BuildNewThesVar(
            NeuronCluster cluster, System.Collections.Generic.List<Neuron> path, 
            ref Variable var, 
            Neuron rel)
        {
            var iRes = NeuronFactory.GetCluster(); // no result found, so create one.
            Brain.Current.Add(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.ParsedThesVar;
            if (var == null)
            {
                var = NeuronFactory.Get<Variable>();

                    // new variable neuron is also needed that will store the collected values.
                Brain.Current.Add(var);
            }

            Link.Create(iRes, var, (ulong)PredefinedNeurons.Variable);
            for (var i = 0; i < path.Count; i++)
            {
                using (var iChildren = iRes.ChildrenW) iChildren.Add(path[i]);
            }

            using (var iChildren = cluster.ChildrenW) iChildren.Add(iRes);

            Neuron iCurPos = cluster; // the start of the tree.
            var iStart = 0;
            if (rel == null)
            {
                iCurPos = BuildTree(iRes, iCurPos, Brain.Current[(ulong)PredefinedNeurons.Empty]);

                    // we use the 'empty' neuron to indicate the 'is a' or default thes relationship
            }

            for (var i = iStart; i < path.Count; i++)
            {
                iCurPos = BuildTree(iRes, iCurPos, path[i]);
            }

            if (Link.Exists(iCurPos, iRes, (ulong)PredefinedNeurons.ParsedThesVar) == false)
            {
                Link.Create(iCurPos, iRes, (ulong)PredefinedNeurons.ParsedThesVar);
            }

            return iRes;
        }

        /// <summary>The build tree.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="curPos">The cur pos.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron BuildTree(Neuron thes, Neuron curPos, Neuron meaning)
        {
            var iNext = curPos.FindFirstOut(meaning.ID);
            if (iNext == null)
            {
                iNext = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNext);
                Link.Create(curPos, iNext, meaning);
            }

            if (Link.Exists(thes, iNext, (ulong)PredefinedNeurons.ParsedPatternPart) == false)
            {
                Link.Create(thes, iNext, (ulong)PredefinedNeurons.ParsedPatternPart);
            }

            return iNext;
        }

        /// <summary>Compares the content of the list Acccessor with the path.</summary>
        /// <param name="iSubC">The i sub C.</param>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareContent(System.Collections.Generic.IList<ulong> iSubC, System.Collections.Generic.List<Neuron> path)
        {
            if (iSubC.Count != path.Count)
            {
                return false;
            }

            for (var i = 0; i < path.Count; i++)
            {
                if (path[i].ID != iSubC[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares the content of the list Acccessor with the possible<paramref name="length"/> and <paramref name="range"/> var.</summary>
        /// <param name="iSubC">The i sub C.</param>
        /// <param name="length">The length.</param>
        /// <param name="range">The range.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CompareContent(System.Collections.Generic.IList<ulong> iSubC, int? length, int? range)
        {
            if (length.HasValue && iSubC.Count > 0)
            {
                var iVal = Brain.Current[iSubC[0]] as IntNeuron;
                if (iVal != null && iVal.Value == length.Value)
                {
                    if (range.HasValue)
                    {
                        if (iSubC.Count == 2)
                        {
                            iVal = Brain.Current[iSubC[0]] as IntNeuron;
                            if (iVal.Value == range.Value)
                            {
                                return true;
                            }
                        }
                    }
                    else if (iSubC.Count == 1)
                    {
                        return true;
                    }
                }
            }
            else if (iSubC.Count == 0 && length.HasValue == false)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Handles the starting word.
        /// </summary>
        private void HandleStartingWord()
        {
            var iText = TextNeuron.GetFor(Scanner.CurrentValue);
            if (fLocalTo != null)
            {
                var iNew = fLocalTo.FindFirstOut(iText.ID);
                if (iNew == null)
                {
                    iNew = NeuronFactory.GetNeuron();
                    Brain.Current.Add(iNew);
                    Link.Create(fLocalTo, iNew, iText);
                }

                fCurrentPos = iNew;
                if (Link.Exists(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternPart) == false)
                {
                    // a word can be used 2 times in the same input sentence.
                    Link.Create(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternPart);

                        // we also keep track that this new neuron is part of the pattern, for deleting.
                }
            }
            else
            {
                fCurrentPos = iText;
            }

            ProcessExtraCurrentsForWord(iText);
            Scanner.GetNext();
        }

        /// <summary>Processes the extra currents when the new current item is a word. The
        ///     extra currents are generated by the | opeator and the loop/option.</summary>
        /// <param name="toLink">To link.</param>
        private void ProcessExtraCurrentsForWord(Neuron toLink)
        {
            if (fExtraCurrents.Count > 0)
            {
                // If there are 'extraCurrents' specified, we need to link the currentFrom to them, so that we can also go from those 'extra current' points, to the current point.
                var iUniqueLink = false;
                var iNewExtraCurrents = new System.Collections.Generic.List<Neuron>();
                foreach (var i in fExtraCurrents)
                {
                    Neuron iTo = null;
                    if (fPrevWasChoice == false || fGroupings.Count == 0
                        || fGroupings.Peek().GroupingType != Token.LoopStart)
                    {
                        // if this is a new starting point for a {}, we always want a new var cause we don't want to allow the mixing of different {} paths through each other. taking a new var gurarantees a unique path for each pattern.
                        iTo = i.FindFirstOutEmptyInfo(toLink.ID);
                    }
                    else
                    {
                        iUniqueLink = true;
                    }

                    if (iTo == null)
                    {
                        iTo = NeuronFactory.GetNeuron();
                        Brain.Current.Add(iTo);
                        var iLink = Link.Create(i, iTo, toLink);
                        iNewExtraCurrents.Add(iTo);
                        if (iUniqueLink)
                        {
                            iLink.InfoW.Add((ulong)PredefinedNeurons.True);

                                // we add a neuron to the link to indicate that the link needs to be unique, cause it's for a loop
                        }
                    }
                    else
                    {
                        iNewExtraCurrents.Add(iTo);
                    }

                    if (Link.Exists(fToParse, iTo, (ulong)PredefinedNeurons.ParsedPatternPart) == false)
                    {
                        Link.Create(fToParse, iTo, (ulong)PredefinedNeurons.ParsedPatternPart);
                    }
                }

                fExtraCurrents = iNewExtraCurrents;

                    // we need to replace the current positions with the ones that we created.
            }
        }

        /// <summary>
        ///     Handles a word.
        /// </summary>
        private void handleWord()
        {
            var iText = TextNeuron.GetFor(Scanner.CurrentValue);
            var iUniqueLink = false;
            Neuron iTo = null;
            if (fPrevWasChoice == false || fGroupings.Count == 0 || fGroupings.Peek().GroupingType != Token.LoopStart)
            {
                // if this is a new starting point for a {}, we always want a new var cause we don't want to allow the mixing of different {} paths through each other. taking a new var gurarantees a unique path for each pattern.
                iTo = fCurrentPos.FindFirstOutEmptyInfo(iText.ID);

                    // need to make certain that we have a link that doesn't require uniquenes.
            }
            else
            {
                iUniqueLink = true;
            }

            if (iTo == null)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                var iLink = Link.Create(fCurrentPos, iNew, iText);
                if (iUniqueLink)
                {
                    iLink.InfoW.Add((ulong)PredefinedNeurons.True);

                        // we add a neuron to the link to indicate that the link needs to be unique, cause it's for a loop
                }

                fCurrentPos = iNew;
            }
            else
            {
                fCurrentPos = iTo;
            }

            if (Link.Exists(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternPart) == false)
            {
                // a word can be used 2 times in the same input sentence.
                Link.Create(fToParse, fCurrentPos, (ulong)PredefinedNeurons.ParsedPatternPart);

                    // we also keep trac that this new neuron is part of the 
            }

            ProcessExtraCurrentsForWord(iText);
            fPRevWasGroup = false; // so we can handle the && operator correct.
            fPrevWasAnd = false;
        }

        #region inner types

        /// <summary>
        ///     Whenever the input pattern contains a ([{ we need to start a new
        ///     grouping point. This is used for the | operator, to determin where it
        ///     needs to fall back too + the ]} closers also need this to create a
        ///     link back in the parsed pattern path.
        /// </summary>
        private class GroupingPoint
        {
            /// <summary>
            ///     stores all the neurons in the parsed path that are end points for
            ///     this grouping (when it is a loop/option/grouping). For each '|', we
            ///     have a new starting point. We need those, to close the loop.
            /// </summary>
            public readonly System.Collections.Generic.List<Neuron> EndPoints =
                new System.Collections.Generic.List<Neuron>();

            /// <summary>
            ///     contains all the neurons that also need to be used as points to
            ///     continue from, like PrevPos. Whenever we see a '|', we need to add
            ///     the last item to this list cause it is an end point, that can be
            ///     followed by the item that follows. To build this path, we use this
            ///     list.
            /// </summary>
            public readonly System.Collections.Generic.List<Neuron> ExtraCurrents =
                new System.Collections.Generic.List<Neuron>();

            /// <summary>
            ///     stores all the neurons in the parsed path that are starting points
            ///     for this grouping (when it is a loop). For each '|', we have a new
            ///     starting point. We need those, to close the loop.
            /// </summary>
            public readonly System.Collections.Generic.List<Neuron> StartingPoints =
                new System.Collections.Generic.List<Neuron>();

            /// <summary>
            ///     Gets or sets the prev position in the parse path, so we can roll
            ///     back/create multiple paths when the grouping is closed again.
            /// </summary>
            /// <value>
            ///     The prev pos.
            /// </value>
            public Neuron PrevPos { get; set; }

            /// <summary>
            ///     Gets or sets the type of the grouping: {([ , so we can check if the
            ///     closing brackets were correct.
            /// </summary>
            /// <value>
            ///     The type of the grouping.
            /// </value>
            public Token GroupingType { get; set; }

            /// <summary>Gets or sets a value indicating whether prev was and.</summary>
            public bool PrevWasAnd { get; set; }

            /// <summary>Determines whether the specified <paramref name="token"/> is the
            ///     reverse of the <paramref name="token"/> stored in the object.</summary>
            /// <param name="token">The token.</param>
            /// <returns><c>true</c> if the specified <paramref name="token"/> is reverse;
            ///     otherwise, <c>false</c> .</returns>
            internal bool IsReverse(Token token)
            {
                if (GroupingType == Token.LoopStart)
                {
                    return token == Token.LoopEnd;
                }

                if (GroupingType == Token.GroupStart)
                {
                    return token == Token.GroupEnd;
                }

                if (GroupingType == Token.OptionStart)
                {
                    return token == Token.OptionEnd;
                }

                return false;
            }
        }

        #endregion

        #region fields

        /// <summary>The f local to.</summary>
        private readonly Neuron fLocalTo;

                                // when assigned, all starts should be local to this neuron, so the pattern can only be matched when started from the topic and not globally.

        /// <summary>The f groupings.</summary>
        private readonly System.Collections.Generic.Stack<GroupingPoint> fGroupings =
            new System.Collections.Generic.Stack<GroupingPoint>();

                                                                         // keeps track of condition and loop starts so we can close correctly

        /// <summary>The f current pos.</summary>
        private Neuron fCurrentPos; // stores the neuron that was last generated in the pattern.

        /// <summary>The f extra currents.</summary>
        private System.Collections.Generic.List<Neuron> fExtraCurrents = new System.Collections.Generic.List<Neuron>();

                                                        // contains all the neurons that also need to be regarded as CurrentPos values. These get generated because of the {} and [] operators.

        /// <summary>The f to parse.</summary>
        private readonly TextNeuron fToParse;

                                    // so the text pattern can record the parsed pattern for in case we need to remove a previous value. 

        /// <summary>The f prev was choice.</summary>
        private bool fPrevWasChoice;

        // bool fPrevWasLoopStart = false;                                      //the first node after a loop start must always be a unique path, otherwise it's possible to have intermingles in a loop, which 
        /// <summary>The f prev pos.</summary>
        private Neuron fPrevPos;

                       // we keep track of the previous pos, which gets set when there was a choice('|') cause we need to collect the first item after a choice sometimes, but we can only do that once the currentpos is changed, so this var is used to check if the change occured.

        /// <summary>The f p rev was group.</summary>
        private bool fPRevWasGroup;

                     // switch to keep track of the prev item was a group, so we can check if an && operator is allowed.

        /// <summary>The f prev was and.</summary>
        private bool fPrevWasAnd;

        /// <summary>The f end reached.</summary>
        private bool fEndReached;

                     // when a >| symbol is defined, the end of the pattern must be reached, so we check for this.
        #endregion
    }
}