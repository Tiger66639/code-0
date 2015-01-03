// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputPattern.cs" company="">
//   
// </copyright>
// <summary>
//   represents a single response that can be used to any of the textpattern
//   defined in this textpattern-def. This is a string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     represents a single response that can be used to any of the textpattern
    ///     defined in this textpattern-def. This is a string.
    /// </summary>
    public class OutputPattern : ParsableTextPatternBase
    {
        #region fields

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded = true;

        /// <summary>The f is do expanded.</summary>
        private bool fIsDoExpanded = true;

        /// <summary>The f invalid responses.</summary>
        private InvalidPatternResponseCollection fInvalidResponses;

        /// <summary>The f question can follow.</summary>
        private bool fQuestionCanFollow;

        /// <summary>The f is invalid responses sequenced.</summary>
        private bool fIsInvalidResponsesSequenced;

        /// <summary>The f do.</summary>
        private DoPattern fDo;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="OutputPattern"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.Designer.OutputPattern.Output"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public OutputPattern(TextNeuron toWrap)
            : base(toWrap)
        {
            InitValue(toWrap);
        }

        /// <summary>gets the prop values from the neuron.</summary>
        /// <param name="toWrap">To wrap.</param>
        private void InitValue(TextNeuron toWrap)
        {
            NeuronCluster iTemp;
            iTemp = toWrap.FindFirstOut((ulong)PredefinedNeurons.InvalidResponsesForPattern) as NeuronCluster;
            if (iTemp != null)
            {
                InvalidResponses = new InvalidPatternResponseCollection(this, iTemp);
            }
            else
            {
                InvalidResponses = new InvalidPatternResponseCollection(
                    this, 
                    (ulong)PredefinedNeurons.InvalidResponsesForPattern);
            }

            fQuestionCanFollow = toWrap.FindFirstOut((ulong)PredefinedNeurons.Questions) == null;

            var iDo = toWrap.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iDo != null)
            {
                SetDo(iDo);
            }
        }

        #endregion

        #region prop

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the item is expanded or not. This is to help with
        ///     the UI virtualization.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                fIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        #endregion

        #region IsDoExpanded

        /// <summary>
        ///     Gets/sets the wether the item is expanded or not. This is to help with
        ///     the UI virtualization.
        /// </summary>
        public bool IsDoExpanded
        {
            get
            {
                return fIsDoExpanded;
            }

            set
            {
                if (fIsDoExpanded != value)
                {
                    fIsDoExpanded = value;
                    OnPropertyChanged("IsDoExpanded");
                    if (value)
                    {
                        fDo.RefreshSelectionRange(); // this should move the focus again
                    }
                }
            }
        }

        #endregion

        #region HasInvalidResponses

        /// <summary>
        ///     gets/sets if this output has an "invalidRespones' cluster attached or
        ///     not.
        /// </summary>
        public bool HasInvalidResponses
        {
            get
            {
                return fInvalidResponses != null && fInvalidResponses.Cluster != null
                        && fInvalidResponses.Cluster.ID != Neuron.TempId;
            }

            set
            {
                if (value != HasInvalidResponses)
                {
                    if (value)
                    {
                        if (fInvalidResponses != null)
                        {
                            fInvalidResponses.CreateOwnerLinkIfNeeded(); // create the cluster so that we can add items.
                        }
                        else
                        {
                            throw new System.NotSupportedException("still to implement: list needs to be created");
                        }
                    }
                    else
                    {
                        if (fInvalidResponses.Count > 0)
                        {
                            var iRes = System.Windows.MessageBox.Show(
                                "Delete all 'invalid response' statements?", 
                                "Remove Invalid responses section", 
                                System.Windows.MessageBoxButton.YesNo, 
                                System.Windows.MessageBoxImage.Question);
                            if (iRes == System.Windows.MessageBoxResult.Yes)
                            {
                                EditorsHelper.DeleteInvalidResponses(fInvalidResponses.Cluster);
                            }
                        }
                        else
                        {
                            WindowMain.DeleteItemFromBrain(fInvalidResponses.Cluster);

                                // simply delete the cluster, will also delete the link.
                        }
                    }
                }
            }
        }

        #endregion

        #region InvalidResponses

        /// <summary>
        ///     Gets/sets the list of responses that can be used in case an invalid
        ///     resonse was given for this output.
        /// </summary>
        public InvalidPatternResponseCollection InvalidResponses
        {
            get
            {
                return fInvalidResponses;
            }

            set
            {
                if (value != fInvalidResponses)
                {
                    fInvalidResponses = value;
                    if (fInvalidResponses != null && fInvalidResponses.Cluster.ID != Neuron.TempId)
                    {
                        var iFound = value.Cluster.FindFirstOut((ulong)PredefinedNeurons.OutputListTraversalMode);
                        fIsInvalidResponsesSequenced = iFound != null && iFound.ID == (ulong)PredefinedNeurons.Sequence;
                    }
                    else
                    {
                        fIsInvalidResponsesSequenced = false;
                    }

                    OnPropertyChanged("InvalidResponses");
                    OnPropertyChanged("IsInvalidResponsesSequenced");
                    OnPropertyChanged("HasInvalidResponses");
                }
            }
        }

        #endregion

        #region IsInvalidResponsesSequenced

        /// <summary>Gets or sets a value indicating whether is invalid responses sequenced.</summary>
        public bool IsInvalidResponsesSequenced
        {
            get
            {
                return fIsInvalidResponsesSequenced;
            }

            set
            {
                if (value != fIsInvalidResponsesSequenced)
                {
                    InternalChange = true;
                    try
                    {
                        if (fInvalidResponses.Cluster.ID == Neuron.TempId)
                        {
                            // if the cluster is still temp, make certain it is created first, otherwise we get inconsistencies.
                            fInvalidResponses.CreateOwnerLinkIfNeeded();
                        }

                        if (value)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                fInvalidResponses.Cluster, 
                                (ulong)PredefinedNeurons.OutputListTraversalMode, 
                                (ulong)PredefinedNeurons.Sequence);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                fInvalidResponses.Cluster, 
                                (ulong)PredefinedNeurons.OutputListTraversalMode, 
                                (Neuron)null); // simply use the default, uses much less links.
                        }
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    fIsInvalidResponsesSequenced = value;
                    OnPropertyChanged("IsInvalidResponsesSequenced");
                }
            }
        }

        #endregion

        #region QuestionCanFollow

        /// <summary>
        ///     Gets/sets the wether a question can follow this output or not (default
        ///     is true).
        /// </summary>
        public bool QuestionCanFollow
        {
            get
            {
                return fQuestionCanFollow;
            }

            set
            {
                if (value != fQuestionCanFollow)
                {
                    InternalChange = true;
                    try
                    {
                        if (value == false)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Questions, Item);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Questions, (Neuron)null);
                        }
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    SetQuestionCanFollow(value);
                }
            }
        }

        /// <summary>Sets the question can follow.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void SetQuestionCanFollow(bool value)
        {
            fQuestionCanFollow = value;
            OnPropertyChanged("QuestionCanFollow");
        }

        #endregion

        #region RequiredEditMode

        /// <summary>
        ///     Gets the edit mode that should be activated when this pattern performs
        ///     an insert. This is required for the editor, so it can put the focus on
        ///     the correct item.
        /// </summary>
        public override EditMode RequiredEditMode
        {
            get
            {
                if (Root is ChatbotProperties && !(Owner is PatternRuleOutput))
                {
                    // the chatbotProperties UI uses the same template as invalidResponses for all outputs (except for the repetition handler), so we need to change the editmode, so that the correct objects get focus when we do an insert. 
                    return EditMode.AddInvalid;
                }

                return EditMode.AddOutput;
            }
        }

        #endregion

        #region Do

        /// <summary>
        ///     Gets/sets the do pattern that should be exeucted just before this
        ///     output pattern.
        /// </summary>
        public DoPattern Do
        {
            get
            {
                return fDo;
            }

            internal set
            {
                if (value != fDo)
                {
                    if (fDo != null)
                    {
                        UnRegisterChild(fDo);
                    }

                    fDo = value;
                    if (fDo != null)
                    {
                        RegisterChild(fDo);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.DoPatterns, fDo);
                    }

                    OnPropertyChanged("Do");
                    OnPropertyChanged("HasDo");
                }
            }
        }

        /// <summary>The set do.</summary>
        /// <param name="value">The value.</param>
        private void SetDo(TextNeuron value)
        {
            if (fDo != null)
            {
                UnRegisterChild(fDo);
            }

            if (value != null)
            {
                fDo = new DoPattern(value);
                if (fDo != null)
                {
                    RegisterChild(fDo);
                }
            }
            else
            {
                fDo = null;
            }

            OnPropertyChanged("Do");
            OnPropertyChanged("HasDo");
        }

        #endregion

        #region HasDo

        /// <summary>
        ///     Gets/sets wether there is a do section or not.
        /// </summary>
        public bool HasDo
        {
            get
            {
                return fDo != null;
            }

            set
            {
                if (value != HasDo)
                {
                    if (value == false)
                    {
                        var iRes = System.Windows.MessageBox.Show(
                            "Delete all 'do' statements?", 
                            "Remove do section", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Question);
                        if (iRes == System.Windows.MessageBoxResult.Yes)
                        {
                            EditorsHelper.DeleteDoPattern(fDo.Item as TextNeuron);
                            fDo = null;
                        }
                    }
                    else
                    {
                        var iNew = NeuronFactory.GetText(string.Empty);
                        Brain.Current.Add(iNew);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.DoPatterns, iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fDo;
                    }

                    OnPropertyChanged("Do");
                    OnPropertyChanged("HasDo");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the output pattern to which this item belongs.
        /// </summary>
        public override OutputPattern Output
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        ///     Gets the rule to which this object belongs.
        /// </summary>
        public override PatternRule Rule
        {
            get
            {
                if (Owner is PatternRule)
                {
                    return (PatternRule)Owner;
                }

                return base.Rule;
            }
        }

        /// <summary>
        ///     Gets the ruleOutput to which this object belongs (null by default).
        /// </summary>
        public override PatternRuleOutput RuleOutput
        {
            get
            {
                if (Owner is PatternRuleOutput)
                {
                    return (PatternRuleOutput)Owner;
                }

                if (Owner is PatternRule)
                {
                    return ((PatternRule)Owner).OutputSet;
                }

                return base.RuleOutput;
            }
        }

        #endregion

        #region functions

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            InitValue((TextNeuron)value);
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.InvalidResponsesForPattern)
                {
                    InvalidResponses = new InvalidPatternResponseCollection(
                        this, 
                        (ulong)PredefinedNeurons.InvalidResponsesForPattern);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Questions)
                {
                    SetQuestionCanFollow(true);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.DoPatterns)
                {
                    SetDo(null);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.InvalidResponsesForPattern)
                {
                    InvalidResponses = new InvalidPatternResponseCollection(this, (NeuronCluster)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Questions)
                {
                    SetQuestionCanFollow(false);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.DoPatterns)
                {
                    SetDo(link.To as TextNeuron);
                }
            }
            else
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.InvalidResponsesForPattern)
                {
                    OnPropertyChanged("HasInvalidResponses");
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (fDo == child)
            {
                SetDo(null);
            }
            else
            {
                var iInvalid = child as InvalidPatternResponse;
                if (iInvalid != null)
                {
                    if (InvalidResponses.Remove(iInvalid) == false)
                    {
                        base.RemoveChild(child);
                    }
                }
                else
                {
                    base.RemoveChild(child);
                }
            }
        }

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            EditorsHelper.DeletePatternOutput((TextNeuron)Item);
        }

        /// <summary>
        ///     Instructs to Parse this instance.
        /// </summary>
        protected internal override void ForceParse()
        {
            ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
            if (string.IsNullOrEmpty(Expression.Trim()) == false)
            {
                // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                try
                {
                    var iRoot = (Data.NamedObject)Root;
                    var iTitle = iRoot != null ? iRoot.Name : string.Empty;
                    var iParser = new Parsers.OutputParser((TextNeuron)Item, iTitle);
                    iParser.Parse();
                    IsDirty = false;
                    OnPropertyChanged("ParseError");

                        // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                }
                catch (System.Exception e)
                {
                    Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                        // in case of error, don't store the parsed path.
                    ParseError = e.Message;
                }
            }
            else
            {
                Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                    // in case of error, don't store the parsed path.
            }
        }

        /// <summary>
        ///     Instructs to parse this instance, while generating undo data for the
        ///     aprse.
        /// </summary>
        protected internal override void ParseWithUndo()
        {
            if (IsDirty || HasError || IsParsed == false)
            {
                ResetParseError(); // assign the empty string and not null, to indicate that it has already been parsed.
                if (string.IsNullOrEmpty(Expression.Trim()) == false)
                {
                    // we do a trim on the expression, cause if we try a parse of an empty expression, we get in an ethernal loop: OnpropertyChanged(parseError) forces a recompile cause there is no error
                    try
                    {
                        var iRoot = (Data.NamedObject)Root;
                        var iTitle = iRoot != null ? iRoot.Name : string.Empty;
                        var iParser = new Parsers.OutputParser((TextNeuron)Item, iTitle);
                        iParser.Parse();
                        IsDirty = false;
                        GenerateBuildUndoData();
                        OnPropertyChanged("ParseError");

                            // hasn't been done yet (reset doesn't trigger this), so if there were any prev errors, need to make certain that they are gone.
                    }
                    catch (System.Exception e)
                    {
                        Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                            // in case of error, don't store the parsed path.
                        ParseError = e.Message;
                        GenerateCleanUndoData();
                    }
                }
                else
                {
                    Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);

                        // in case of error, don't store the parsed path.
                }
            }
        }

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any data change at the correct
        ///     moment.
        /// </summary>
        protected internal override void GenerateBuildUndoData()
        {
            var iUndo = new BuildOutputPatternUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>
        ///     called when the
        ///     <see cref="JaStDev.HAB.Designer.ParsableTextPatternBase.IsDirty" />
        ///     value is changed, so we can undo any compile events at the correct
        ///     moment.
        /// </summary>
        protected internal override void GenerateCleanUndoData()
        {
            var iUndo = new ClearOutputPatterUndoItem();
            iUndo.Pattern = Item as TextNeuron;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
        }

        /// <summary>
        ///     Clears the parse while generating undo data. This is used when the
        ///     item gets deleted with the delete command.
        /// </summary>
        protected internal override void ClearParseWithUndo()
        {
            if (Item != null)
            {
                // can be when loosing focus, no data has been entered.
                Parsers.OutputParser.RemoveOutputPattern((TextNeuron)Item);
                var iUndo = new ClearOutputPatterUndoItem();
                iUndo.Pattern = Item as TextNeuron;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            }
        }

        /// <summary>Inserts a new item of the same type in the place of this instance (of
        ///     it's owner list). This is used to have a generic insert method for all
        ///     items.</summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="ParsableTextPatternBase"/>.</returns>
        public override ParsableTextPatternBase Insert(int offset = 0)
        {
            var iRuleOutput = Owner as PatternRuleOutput;
            OutputPattern iNew = null;
            if (iRuleOutput != null)
            {
                iNew = EditorsHelper.AddNewOutputPattern(
                    iRuleOutput.Outputs, 
                    string.Empty, 
                    iRuleOutput.Outputs.IndexOf(this) + offset);
            }
            else
            {
                var iRule = Owner as PatternRule;
                if (iRule != null)
                {
                    iNew = EditorsHelper.AddNewOutputPattern(iRule.Outputs, string.Empty, iRule.Outputs.IndexOf(this) + offset);
                }
                else
                {
                    PatternOutputsCollection iCol = null;
                    var iProps = Owner as ChatbotProperties;
                    if (iProps.ConversationStarts.IndexOf(this) > -1)
                    {
                        iCol = iProps.ConversationStarts;
                    }
                    else if (iProps.FallBacks.IndexOf(this) > -1)
                    {
                        iCol = iProps.FallBacks;
                    }
                    else if (iProps.Context.IndexOf(this) > -1)
                    {
                        iCol = iProps.Context;
                    }

                    if (iCol != null)
                    {
                        iNew = EditorsHelper.AddNewOutputPattern(iCol, string.Empty, iCol.IndexOf(this) + offset);
                    }
                }
            }

            return iNew;
        }

        /// <summary>Gets the display path that points to the current object.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIRoot iRoot = null;
            FillDisplayPathForThis(ref iRoot);
            if (iRoot != null)
            {
                AddSelectionRange(iRoot);
                return new Search.DisplayPath(iRoot);
            }

            return null; // if there is no owning editor, can't return a path yet for a textpattern.
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iIndex = -1;
            var iOwner = Owner as PatternEditorItem;
            if (iOwner != null)
            {
                iOwner.FillDisplayPathForThis(ref list);
                var iRuleOutput = iOwner as PatternRuleOutput;
                if (iRuleOutput != null)
                {
                    iIndex = iRuleOutput.Outputs.IndexOf(this);
                }
                else
                {
                    var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.TextPatternOutputs);

                        // need to get the outputset first
                    list.Items.Add(iOut);
                    iIndex = ((PatternRule)iOwner).Outputs.IndexOf(this);
                }
            }
            else
            {
                var iProps = Root as ChatbotProperties;
                if (iProps != null)
                {
                    iIndex = iProps.ConversationStarts.IndexOf(this);
                    if (iIndex != -1)
                    {
                        list = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsOpeningStatSelected);
                    }
                    else
                    {
                        iIndex = iProps.FallBacks.IndexOf(this);
                        if (iIndex != -1)
                        {
                            list = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsFallbackSelected);
                        }
                        else
                        {
                            if (iProps.Context != null)
                            {
                                iIndex = iProps.Context.IndexOf(this);
                                if (iIndex != -1)
                                {
                                    list = new Search.DPIChatbotPropsRoot(
                                        ChatbotProperties.SelectedUI.IsContextSelected);
                                }
                            }
                        }
                    }
                }
            }

            if (list != null)
            {
                var iChild = new Search.DPIChild(iIndex);
                list.Items.Add(iChild);
            }
        }

        #endregion
    }
}