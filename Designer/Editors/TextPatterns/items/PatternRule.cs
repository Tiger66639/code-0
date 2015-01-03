// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternRule.cs" company="">
//   
// </copyright>
// <summary>
//   wraps a cluster that represents a single set of possible outputs and all
//   the text patterns tha should result in this list of outputs (1 output is
//   rendered, moved to the end of the list).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     wraps a cluster that represents a single set of possible outputs and all
    ///     the text patterns tha should result in this list of outputs (1 output is
    ///     rendered, moved to the end of the list).
    /// </summary>
    /// <remarks>
    ///     Inherits from <see cref="RangedPatternEditorItem" /> so that we have a
    ///     SelectionRange. this way, the 'Name' field also has a proper undo system.
    /// </remarks>
    public class PatternRule : RangedPatternEditorItem, IConditionalOutputsCollectionOwner
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="PatternRule"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        public PatternRule(NeuronCluster toWrap)
            : base(toWrap)
        {
        }

        #endregion

        /// <summary>
        ///     Gets or sets the owner. Need to make certain that the data is loaded
        ///     correctly.
        /// </summary>
        public override Data.IOwnedObject Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                base.Owner = value;
                var iEditor = Owner as TextPatternEditor;
                if (iEditor != null
                    && (iEditor.IsListView
                        || (iEditor.SelectedRuleIndex > -1 && iEditor.Items != null
                            && iEditor.SelectedRuleIndex < iEditor.Items.Count
                            && iEditor.Items[iEditor.SelectedRuleIndex] == this)))
                {
                    LoadUI();
                }
            }
        }

        #region ToCalculate

        /// <summary>
        ///     <para>
        ///         Deprecated, use <see cref="JaStDev.HAB.Designer.PatternRule.ToCal" />
        ///     </para>
        ///     <para>
        ///         Only still here so we can do convertions. Gets/sets the set of do
        ///         patterns that need to be executed before any conditional is
        ///         calculated.
        ///     </para>
        /// </summary>
        public DoPatternCollection ToCalculate
        {
            get
            {
                return fToCalculate;
            }

            internal set
            {
                fToCalculate = value;
                OnPropertyChanged("ToCalculate");
                OnPropertyChanged("HasToCalculate");
            }
        }

        #endregion

        #region HasToCal

        /// <summary>
        ///     Gets/sets wether there is a do section or not.
        /// </summary>
        public bool HasToCal
        {
            get
            {
                return fToCal != null;
            }

            set
            {
                if (value != HasToCal)
                {
                    if (value == false)
                    {
                        var iRes = System.Windows.MessageBox.Show(
                            "Delete all 'calculate' statements?", 
                            "Remove calculate section", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Question);
                        if (iRes == System.Windows.MessageBoxResult.Yes)
                        {
                            EditorsHelper.DeleteDoPattern(fToCal.Item as TextNeuron);
                            fToCal = null;
                        }
                    }
                    else
                    {
                        var iNew = NeuronFactory.GetText(string.Empty);
                        Brain.Current.Add(iNew);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Calculate, iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fToCal;
                    }

                    OnPropertyChanged("HasToCal");
                }
            }
        }

        #endregion

        #region ToEvaluate

        /// <summary>
        ///     Deprecated. Use <see cref="JaStDev.HAB.Designer.PatternRule.ToEval" />
        ///     instead. Only still here so we can do convertions. Gets/sets the list
        ///     of do patterns that should be executed at evalution time.
        /// </summary>
        public DoPatternCollection ToEvaluate
        {
            get
            {
                return fToEvaluate;
            }

            set
            {
                fToEvaluate = value;
                OnPropertyChanged("ToEvaluate");
                OnPropertyChanged("HasToEvaluate");
            }
        }

        #endregion

        #region HasToEval

        /// <summary>
        ///     Gets/sets wether there is a do section or not.
        /// </summary>
        public bool HasToEval
        {
            get
            {
                return fToEval != null;
            }

            set
            {
                if (value != HasToEval)
                {
                    if (value == false)
                    {
                        var iRes = System.Windows.MessageBox.Show(
                            "Delete all 'evaluate' statements?", 
                            "Remove evaluate section", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Question);
                        if (iRes == System.Windows.MessageBoxResult.Yes)
                        {
                            EditorsHelper.DeleteDoPattern(fToEval.Item as TextNeuron);
                            fToEval = null;
                        }
                    }
                    else
                    {
                        var iNew = NeuronFactory.GetText(string.Empty);
                        Brain.Current.Add(iNew);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Evaluate, iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fToEval;
                    }

                    OnPropertyChanged("HasToEval");
                }
            }
        }

        #endregion

        #region IsToCalculateVisible

        /// <summary>
        ///     Gets/sets the wether the do patterns are visible or not.
        /// </summary>
        public bool IsToCalculateVisible
        {
            get
            {
                return fIsToCalculateVisible;
            }

            set
            {
                if (value != fIsToCalculateVisible)
                {
                    fIsToCalculateVisible = value;
                    OnPropertyChanged("IsToCalculateVisible");
                }
            }
        }

        #endregion

        #region IsToEvaluateVisible

        /// <summary>
        ///     Gets/sets the wether the do patterns are visible or not.
        /// </summary>
        public bool IsToEvaluateVisible
        {
            get
            {
                return fIsToEvaluateVisible;
            }

            set
            {
                if (value != fIsToEvaluateVisible)
                {
                    fIsToEvaluateVisible = value;
                    OnPropertyChanged("IsToEvaluateVisible");
                }
            }
        }

        #endregion

        #region TextPatterns

        /// <summary>
        ///     Gets the list of text patttern items that make up this pattern
        ///     definition.
        /// </summary>
        public InputPatternCollection TextPatterns
        {
            get
            {
                return fTextPatterns;
            }

            internal set
            {
                fTextPatterns = value;
                OnPropertyChanged("TextPatterns");
            }
        }

        #endregion

        /// <summary>
        ///     Gets the list of outputs and conditionals that respones for a specific
        ///     output statement.
        /// </summary>
        public ResponseValuesCollection ResponsesFor
        {
            get
            {
                return fResponsesFor;
            }

            internal set
            {
                fResponsesFor = value;
                OnPropertyChanged("ResponsesFor");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has any children or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is empty; otherwise, <c>false</c> .
        /// </value>
        public bool IsEmpty
        {
            get
            {
                if (TextPatterns.Count > 0)
                {
                    return false;
                }

                if (OutputSet.IsEmpty == false)
                {
                    return false;
                }

                if (ToCal != null)
                {
                    return false;
                }

                if (ToEval != null)
                {
                    return false;
                }

                if (Conditionals != null)
                {
                    foreach (var iCond in Conditionals)
                    {
                        if (iCond.IsEmpty == false)
                        {
                            return false;
                        }
                    }
                }

                if (ToEvaluate != null && ToEvaluate.Count > 0)
                {
                    return false;
                }

                if (ResponsesFor != null)
                {
                    foreach (var iGrp in ResponsesFor)
                    {
                        if (iGrp.IsEmpty == false)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is loaded; otherwise, <c>false</c> .
        /// </value>
        public bool IsLoaded
        {
            get
            {
                return TextPatterns != null;
            }

            set
            {
                if (value != IsLoaded)
                {
                    if (value)
                    {
                        LoadUI();
                    }
                    else
                    {
                        UnloadUIData();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the rule to which this object belongs.
        /// </summary>
        public override PatternRule Rule
        {
            get
            {
                return this;
            }
        }

        #region Conditionals

        /// <summary>
        ///     Gets the list of conditional output sets.
        /// </summary>
        public ConditionalOutputsCollection Conditionals
        {
            get
            {
                return fConditionals;
            }

            internal set
            {
                fConditionals = value;
                OnPropertyChanged("Conditionals");
            }
        }

        #endregion

        /// <summary>Rebuilds all the patterns in this rule.</summary>
        /// <param name="errors">The errors.</param>
        internal void Rebuild(System.Collections.Generic.List<string> errors)
        {
            ParseInputs(errors);
            if (ToCalculate != null)
            {
                foreach (var i in ToCalculate)
                {
                    i.ForceParse();
                    if (i.HasError)
                    {
                        errors.Add(i.ParseError);
                    }
                }
            }

            if (ToCal != null)
            {
                ToCal.ForceParse();
                if (ToCal.HasError)
                {
                    errors.Add(ToCal.ParseError);
                }
            }

            OutputSet.Rebuild(errors);
            if (Conditionals != null)
            {
                foreach (var i in Conditionals)
                {
                    i.Rebuild(errors);
                }
            }

            if (ToEvaluate != null)
            {
                foreach (var i in ToEvaluate)
                {
                    i.ForceParse();
                    if (i.HasError)
                    {
                        errors.Add(i.ParseError);
                    }
                }
            }

            if (ToEval != null)
            {
                ToEval.ForceParse();
                if (ToEval.HasError)
                {
                    errors.Add(ToEval.ParseError);
                }
            }

            if (ResponsesFor != null)
            {
                foreach (var i in ResponsesFor)
                {
                    i.Rebuild(errors);
                }
            }
        }

        /// <summary>rebulds the inputs.</summary>
        /// <param name="errors">The errors.</param>
        internal void ParseInputs(System.Collections.Generic.List<string> errors)
        {
            foreach (var i in TextPatterns)
            {
                i.ForceParse();
                if (i.HasError)
                {
                    errors.Add(i.ParseError);
                }
            }
        }

        /// <summary>
        ///     unloads (removes the parse) of all data except the output patterns.
        ///     This is for unloading a topic so that others can properly be replaced
        /// </summary>
        internal void ReleaseAll()
        {
            ReleaseInputs();
            if (ToCalculate != null)
            {
                foreach (var i in ToCalculate)
                {
                    Parsers.DoParser.RemoveDoPattern(i.Item);
                }
            }

            if (ToCal != null)
            {
                Parsers.DoParser.RemoveDoPattern(ToCal.Item);
            }

            if (ToEvaluate != null)
            {
                foreach (var i in ToEvaluate)
                {
                    Parsers.DoParser.RemoveDoPattern(i.Item);
                }
            }

            if (ToEval != null)
            {
                Parsers.DoParser.RemoveDoPattern(ToEval.Item);
            }

            ReleaseOutputs();
            if (ResponsesFor != null)
            {
                foreach (var i in ResponsesFor)
                {
                    i.ReleaseAll();
                }
            }
        }

        /// <summary>The release inputs.</summary>
        internal void ReleaseInputs()
        {
            foreach (var i in TextPatterns)
            {
                Parsers.InputParser.RemoveInputPattern((TextNeuron)i.Item);
            }
        }

        /// <summary>
        ///     releases all the output patterns
        /// </summary>
        internal void ReleaseOutputs()
        {
            OutputSet.ReleaseOutputs();
            if (Conditionals != null)
            {
                foreach (var i in Conditionals)
                {
                    i.ReleaseOutputs();
                }
            }
        }

        #region fields

        /// <summary>The f text patterns.</summary>
        private InputPatternCollection fTextPatterns;

        /// <summary>The f conditionals.</summary>
        private ConditionalOutputsCollection fConditionals;

        /// <summary>The f responses for.</summary>
        private ResponseValuesCollection fResponsesFor;

        /// <summary>The f to calculate.</summary>
        private DoPatternCollection fToCalculate;

        /// <summary>The f is to calculate visible.</summary>
        private bool fIsToCalculateVisible = true;

        /// <summary>The f is to evaluate visible.</summary>
        private bool fIsToEvaluateVisible = true;

        /// <summary>The f to evaluate.</summary>
        private DoPatternCollection fToEvaluate;

        /// <summary>The f to cal.</summary>
        private DoPattern fToCal;

        /// <summary>The f to eval.</summary>
        private DoPattern fToEval;

        #endregion

        #region Outputs

        /// <summary>
        ///     Gets the outputs available for this pattern definition.
        /// </summary>
        public PatternOutputsCollection Outputs
        {
            get
            {
                if (OutputSet != null)
                {
                    return OutputSet.Outputs;
                }

                return null;
            }

            internal set
            {
                if (OutputSet == null && value != null)
                {
                    OutputSet = new PatternRuleOutput();
                    RegisterChild(OutputSet);
                }
                else if (OutputSet != null && value == null)
                {
                    UnRegisterChild(OutputSet);
                    OutputSet = null;
                }

                if (OutputSet != null)
                {
                    if (value != null && value.Cluster != null && Neuron.IsEmpty(value.Cluster.ID) == false)
                    {
                        OutputSet.SetItem(value.Cluster);
                    }

                    OutputSet.Outputs = value;
                }

                OnPropertyChanged("Outputs");
                OnPropertyChanged("OutputSet");
            }
        }

        /// <summary>
        ///     Gets the output as a PatternRuleOutput. This is used for the deletion
        ///     routines, so we can have an easy delete.
        /// </summary>
        public PatternRuleOutput OutputSet { get; private set; }

        #endregion

        #region Do

        /// <summary>
        ///     Deprecatd, use <see cref="JaStDev.HAB.Designer.PatternRule.Do" />
        ///     instead. Only still here so we can do convertions. Gets the list of
        ///     'do patterns' that are declared for the main output set (which doesn't
        ///     have any conditions).
        /// </summary>
        public DoPatternCollection DoPatterns
        {
            get
            {
                if (OutputSet != null)
                {
                    return OutputSet.DoPatterns;
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the 'do pattern' that is declared for the main output set (which
        ///     doesn't have any conditions).
        /// </summary>
        public DoPattern Do
        {
            get
            {
                if (OutputSet != null)
                {
                    return OutputSet.Do;
                }

                return null;
            }
        }

        #endregion

        #region ToCal

        /// <summary>
        ///     Gets/sets the do pattern that should be exeucted just before this
        ///     output pattern.
        /// </summary>
        public DoPattern ToCal
        {
            get
            {
                return fToCal;
            }

            internal set
            {
                if (value != fToCal)
                {
                    if (fToCal != null)
                    {
                        UnRegisterChild(fToCal);
                    }

                    fToCal = value;
                    if (fToCal != null)
                    {
                        RegisterChild(fToCal);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Calculate, fToCal);
                    }

                    OnPropertyChanged("ToCal");
                    OnPropertyChanged("HasToCal");
                }
            }
        }

        /// <summary>The set to cal.</summary>
        /// <param name="value">The value.</param>
        private void SetToCal(TextNeuron value)
        {
            if (fToCal != null)
            {
                UnRegisterChild(fToCal);
            }

            if (value != null)
            {
                fToCal = new DoPattern(value);
                if (fToCal != null)
                {
                    RegisterChild(fToCal);
                }
            }
            else
            {
                fToCal = null;
            }

            OnPropertyChanged("ToCal");
            OnPropertyChanged("HasToCal");
        }

        #endregion

        #region ToEval

        /// <summary>
        ///     Gets/sets the do pattern that should be exeucted just before this
        ///     output pattern.
        /// </summary>
        public DoPattern ToEval
        {
            get
            {
                return fToEval;
            }

            internal set
            {
                if (value != fToEval)
                {
                    if (fToEval != null)
                    {
                        UnRegisterChild(fToEval);
                    }

                    fToEval = value;
                    if (fToEval != null)
                    {
                        RegisterChild(fToEval);
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Evaluate, fToEval);
                    }

                    OnPropertyChanged("ToEval");
                    OnPropertyChanged("HasToEval");
                }
            }
        }

        /// <summary>The set to eval.</summary>
        /// <param name="value">The value.</param>
        private void SetToEval(TextNeuron value)
        {
            if (fToEval != null)
            {
                UnRegisterChild(fToEval);
            }

            if (value != null)
            {
                fToEval = new DoPattern(value);
                if (fToEval != null)
                {
                    RegisterChild(fToEval);
                }
            }
            else
            {
                fToEval = null;
            }

            OnPropertyChanged("ToEval");
        }

        #endregion

        #region FocusNew

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new-out
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewOut
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewOut"
                    && TextPatternEditorResources.FocusOn.Item == this)
                {
                    iVal = true;
                    TextPatternEditorResources.NeedsFocus = false;
                    TextPatternEditorResources.FocusOn.PropName = null;
                    TextPatternEditorResources.FocusOn.Item = null; // don't need mem leak.
                }
                else
                {
                    iVal = false;
                }

                if (iVal)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewOut"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new-in
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewIn
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus && TextPatternEditorResources.FocusOn.PropName == "FocusNewIn"
                    && TextPatternEditorResources.FocusOn.Item == this)
                {
                    iVal = true;
                    TextPatternEditorResources.NeedsFocus = false;
                    TextPatternEditorResources.FocusOn.PropName = null;
                    TextPatternEditorResources.FocusOn.Item = null; // don't need mem leak.
                }
                else
                {
                    iVal = false;
                }

                if (iVal)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewIn"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new-in
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusName
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus && TextPatternEditorResources.FocusOn.PropName == "FocusName"
                    && TextPatternEditorResources.FocusOn.Item == this)
                {
                    iVal = true;
                    TextPatternEditorResources.NeedsFocus = false;
                    TextPatternEditorResources.FocusOn.PropName = null;
                    TextPatternEditorResources.FocusOn.Item = null; // don't need mem leak.
                }
                else
                {
                    iVal = false;
                }

                if (iVal)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusName"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new
        ///     responses This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewResponsesGroup
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewResponsesGroup"
                    && TextPatternEditorResources.FocusOn.Item == this)
                {
                    iVal = true;
                    TextPatternEditorResources.NeedsFocus = false;
                    TextPatternEditorResources.FocusOn.PropName = null;
                    TextPatternEditorResources.FocusOn.Item = null; // don't need mem leak.
                }
                else
                {
                    iVal = false;
                }

                if (iVal)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewResponsesGroup"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        #endregion

        #region functions

        /// <summary>
        ///     Called by the editor when the user has requested to remove the
        ///     selected items from the list.
        /// </summary>
        internal override void RemoveFromOwner()
        {
            var iEditor = Owner as TextPatternEditor;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.Items.Remove(this);
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            InitValue((NeuronCluster)value);
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.TextPatternOutputs)
                {
                    Outputs = new PatternOutputsCollection(this, (ulong)PredefinedNeurons.TextPatternOutputs);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    Conditionals = new ConditionalOutputsCollection(this, (ulong)PredefinedNeurons.Condition);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Calculate)
                {
                    SetToCal(null);
                    ToCalculate = null;

                        // always reset, in case we go from old format to new (shouldn't be the case, but play save).
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Evaluate)
                {
                    SetToEval(null);
                    ToEvaluate = null;
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ResponseForOutputs)
                {
                    ResponsesFor = new ResponseValuesCollection(this, (ulong)PredefinedNeurons.ResponseForOutputs);
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
                if (link.MeaningID == (ulong)PredefinedNeurons.TextPatternOutputs)
                {
                    Outputs = new PatternOutputsCollection(this, (NeuronCluster)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    Conditionals = new ConditionalOutputsCollection(this, (NeuronCluster)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Calculate)
                {
                    var iTo = link.To as TextNeuron;
                    if (iTo != null)
                    {
                        // this is the new format, but be prepared for older types as well.
                        SetToCal(iTo);
                    }
                    else
                    {
                        ToCalculate = new DoPatternCollection(this, (NeuronCluster)link.To);
                    }
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ResponseForOutputs)
                {
                    ResponsesFor = new ResponseValuesCollection(this, (NeuronCluster)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Evaluate)
                {
                    var iTo = link.To as TextNeuron;
                    if (iTo != null)
                    {
                        // this is the new format, but be prepared for older types as well.
                        SetToEval(iTo);
                    }
                    else
                    {
                        ToEvaluate = new DoPatternCollection(this, (NeuronCluster)link.To);
                    }
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Calculate)
                {
                    var iTo = link.To as TextNeuron;
                    if (iTo != null)
                    {
                        // this is the new format, but be prepared for older types as well.
                        SetToCal(iTo);
                    }
                    else
                    {
                        ToCalculate = new DoPatternCollection(this, (NeuronCluster)link.To);
                    }
                }
            }
            else
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.TextPatternOutputs)
                {
                    OutputSet.SetItem((NeuronCluster)link.To);

                        // internal change for Outputs: need to register the Item also.
                }

                if (link.MeaningID == (ulong)PredefinedNeurons.Calculate)
                {
                    OnPropertyChanged("HasToCal");
                }

                if (link.MeaningID == (ulong)PredefinedNeurons.Evaluate)
                {
                    OnPropertyChanged("HasToEval");
                }
            }
        }

        /// <summary>
        ///     Called when all the data kept in memory for the UI section can be
        ///     unloaded.
        /// </summary>
        internal override void UnloadUIData()
        {
            base.UnloadUIData();
            if (TextPatterns != null)
            {
                foreach (var i in TextPatterns)
                {
                    i.UnloadUIData();
                }

                TextPatterns = null;
            }

            if (OutputSet != null)
            {
                OutputSet.UnloadUIData();
                OutputSet = null;
            }

            if (Conditionals != null)
            {
                foreach (var i in Conditionals)
                {
                    i.UnloadUIData();
                }

                Conditionals = null;
            }

            if (ToCalculate != null)
            {
                foreach (var i in ToCalculate)
                {
                    i.UnloadUIData();
                }

                ToCalculate = null;
            }

            if (ToCal != null)
            {
                ToCal.UnloadUIData();
                ToCal = null;
            }

            if (ToEval != null)
            {
                ToEval.UnloadUIData();
                ToEval = null;
            }

            if (ToEvaluate != null)
            {
                foreach (var i in ToEvaluate)
                {
                    i.UnloadUIData();
                }

                ToEvaluate = null;
            }

            if (ResponsesFor != null)
            {
                foreach (var i in ResponsesFor)
                {
                    i.UnloadUIData();
                }

                ResponsesFor = null;
            }
        }

        /// <summary>Inits the object. Always done when a new neuron is assigned.</summary>
        /// <param name="toWrap">To wrap.</param>
        private void InitValue(NeuronCluster toWrap)
        {
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // only load the ui if we really need to be depicted.
                LoadUI();
            }
        }

        /// <summary>The load ui.</summary>
        internal void LoadUI()
        {
            var iToWrap = Item as NeuronCluster;
            if (iToWrap != null)
            {
                TextPatterns = new InputPatternCollection(this, iToWrap);
                var iOuts = iToWrap.FindFirstOut((ulong)PredefinedNeurons.TextPatternOutputs) as NeuronCluster;
                if (iOuts != null)
                {
                    Outputs = new PatternOutputsCollection(this, iOuts);
                }
                else
                {
                    Outputs = new PatternOutputsCollection(this, (ulong)PredefinedNeurons.TextPatternOutputs);
                }

                var iConds = iToWrap.FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
                if (iConds != null)
                {
                    Conditionals = new ConditionalOutputsCollection(this, iConds);
                }
                else
                {
                    Conditionals = new ConditionalOutputsCollection(this, (ulong)PredefinedNeurons.Condition);
                }

                var iCode = iToWrap.FindFirstOut((ulong)PredefinedNeurons.Calculate);
                if (iCode != null)
                {
                    var iCl = iCode as NeuronCluster;
                    if (iCl != null)
                    {
                        ToCalculate = new DoPatternCollection(this, iCl);
                    }
                    else
                    {
                        SetToCal(iCode as TextNeuron);
                    }
                }

                var iFound = iToWrap.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
                if (iFound != null)
                {
                    ResponsesFor = new ResponseValuesCollection(this, iFound);
                }
                else
                {
                    ResponsesFor = new ResponseValuesCollection(this, (ulong)PredefinedNeurons.ResponseForOutputs);
                }

                iCode = iToWrap.FindFirstOut((ulong)PredefinedNeurons.Evaluate) as NeuronCluster;
                if (iCode != null)
                {
                    var iCl = iCode as NeuronCluster;
                    if (iCl != null)
                    {
                        ToEvaluate = new DoPatternCollection(this, iCl);
                    }
                    else
                    {
                        SetToEval(iCode as TextNeuron);
                    }
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iText = child as InputPattern;
            if (iText != null)
            {
                if (TextPatterns.Remove(iText) == false)
                {
                    base.RemoveChild(child);
                }
            }
            else if (child == OutputSet)
            {
                OutputSet = null;
            }
            else
            {
                var iOut = child as OutputPattern;
                if (iOut != null)
                {
                    if (RemoveOut(iOut) == false)
                    {
                        base.RemoveChild(child);
                    }
                }
                else
                {
                    var iDo = child as DoPattern;
                    if (iDo != null)
                    {
                        if (RemoveDo(iDo) == false)
                        {
                            base.RemoveChild(child);
                        }
                    }
                    else
                    {
                        var iCond = child as ConditionPattern;
                        if (iCond != null)
                        {
                            if (RemoveCond(iCond) == false)
                            {
                                base.RemoveChild(child);
                            }
                        }
                        else
                        {
                            var iruleOut = child as PatternRuleOutput;
                            if (iruleOut != null)
                            {
                                if (Conditionals.Remove(iruleOut) == false)
                                {
                                    base.RemoveChild(child);
                                }
                            }
                            else
                            {
                                var iGroup = child as ResponsesForGroup;
                                if (iGroup != null)
                                {
                                    if (ResponsesFor.Remove(iGroup) == false)
                                    {
                                        base.RemoveChild(child);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>A rule itself can't have a path, but the input/output boxes used to
        ///     create new items can + the name. So we use the same technique as<see cref="TextPatternEditor"/> objects.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            var iText = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iText != null && iText.Tag is string)
            {
                // the tag has to be a string for this function to be able to move focus.
                Search.DPIRoot iRoot = null;
                FillDisplayPathForThis(ref iRoot);
                var iTag = iText.Tag as string;
                var iTextTag = new Search.DPITextTag(iTag);
                if (iTextTag.Tag == "FocusName")
                {
                    // when doing for the Name textbox, also store the textrange so taht we can put cursor back at the corect location.
                    AddSelectionRange(iRoot);

                        // first add the range otherwise it wont be selected (TextTag is a terminator, range isn't, but keeps focus on the same object)
                }

                iRoot.Items.Add(iTextTag);
                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iEditor = Owner as TextPatternEditor;
            System.Diagnostics.Debug.Assert(iEditor != null);
            list = new Search.DPITextPatternEditorRoot();
            list.Item = iEditor.Item;
            var iChild = new Search.DPIChild(iEditor.Items.IndexOf(this));
            list.Items.Add(iChild);
        }

        /// <summary>Removes the condition from the list of contional items.</summary>
        /// <param name="cond">The cond.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool RemoveCond(ConditionPattern cond)
        {
            foreach (var i in Conditionals)
            {
                if (i.Condition == cond)
                {
                    i.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Condition, null);
                    return true;
                }
            }

            return false;
        }

        /// <summary>The remove out.</summary>
        /// <param name="outPattern">The out pattern.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool RemoveOut(OutputPattern outPattern)
        {
            if (Outputs.Remove(outPattern) == false)
            {
                foreach (var i in Conditionals)
                {
                    if (i.Outputs.Remove(outPattern))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>Removes the do pattern from the rule.</summary>
        /// <param name="doPattern">The do pattern.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool RemoveDo(DoPattern doPattern)
        {
            if (Do == doPattern)
            {
                OutputSet.Do = null;
                return true;
            }

            if (ToEval == doPattern)
            {
                ToEval = null;
                return true;
            }

            if (ToCal == doPattern)
            {
                ToCal = null;
                return true;
            }

            if (ToCalculate != null && ToCalculate.Remove(doPattern) == false)
            {
                if (DoPatterns != null && DoPatterns.Remove(doPattern) == false)
                {
                    if (ToEvaluate != null && ToEvaluate.Remove(doPattern) == false)
                    {
                        foreach (var i in Conditionals)
                        {
                            if (i.DoPatterns.Remove(doPattern))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iTryAgain = new System.Collections.Generic.HashSet<Neuron>();

                    // if there are refernces to rules on the same page, some items might need a second try before they can be deleted (after the relationship is delted)
                EditorsHelper.DeletePatternRule(this, iTryAgain);
                if (iTryAgain.Count > 0)
                {
                    EditorsHelper.DeleteTextPatternRetries(iTryAgain);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion
    }
}