// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternRuleOutput.cs" company="">
//   
// </copyright>
// <summary>
//   This class wraps the cluster with output patterns. It also contains a
//   <see langword="ref" /> to the 'doPatterns' and a possible condition, in
//   case it is part of the 'conditonal' outputs of a patternRule.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This class wraps the cluster with output patterns. It also contains a
    ///     <see langword="ref" /> to the 'doPatterns' and a possible condition, in
    ///     case it is part of the 'conditonal' outputs of a patternRule.
    /// </summary>
    public class PatternRuleOutput : PatternEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="PatternRuleOutput"/> class. 
        ///     Initializes a new instance of the <see cref="PatternRuleOutput"/>
        ///     class. For when there is no cluster to wrap (outputs has been deleted,
        ///     but we still need to supply a temp list, so we can recreate the object
        ///     again.</summary>
        public PatternRuleOutput()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PatternRuleOutput"/> class. Initializes a new instance of the <see cref="PatternRuleOutput"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public PatternRuleOutput(NeuronCluster toWrap)
            : base(toWrap)
        {
        }

        #region Outputs

        /// <summary>
        ///     Gets the outputs available for this pattern definition.
        /// </summary>
        public PatternOutputsCollection Outputs
        {
            get
            {
                return fOutputs;
            }

            internal set
            {
                if (fOutputs != value)
                {
                    if (fOutputs != null)
                    {
                        foreach (var i in fOutputs)
                        {
                            i.UnloadUIData();
                        }
                    }

                    fOutputs = value;
                    OnPropertyChanged("Outputs");
                }
            }
        }

        #endregion

        #region DoPatterns

        /// <summary>
        ///     Gets the list of 'do patterns' that are declared for this output set.
        ///     Deprecated, use 'Do' instead.
        /// </summary>
        public DoPatternCollection DoPatterns
        {
            get
            {
                return fDoPatterns;
            }

            internal set
            {
                if (fDoPatterns != value)
                {
                    if (fDoPatterns != null)
                    {
                        foreach (var i in fDoPatterns)
                        {
                            i.UnloadUIData();
                        }
                    }

                    fDoPatterns = value;
                    OnPropertyChanged("DoPatterns");
                    OnPropertyChanged("HasDoPatterns");
                }
            }
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
                        if (Item == null)
                        {
                            BuildItem();
                        }

                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.DoPatterns, iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fDo;
                    }

                    OnPropertyChanged("HasDo");
                }
            }
        }

        #endregion

        #region Condition

        /// <summary>
        ///     Gets the condition that is assigned to this set of outputs. Note: the
        ///     last set of outputs in a rule, shouldn't have a condition.
        /// </summary>
        public ConditionPattern Condition
        {
            get
            {
                return fCondition;
            }

            private set
            {
                if (fCondition != null)
                {
                    // this is not part of a list, so remove/add to the observation list manually.
                    fCondition.UnloadUIData();
                    UnRegisterChild(fCondition);
                }

                fCondition = value;
                if (fCondition != null)
                {
                    RegisterChild(fCondition);
                }

                OnPropertyChanged("Condition");
            }
        }

        #endregion

        #region IsDoPatternVisible

        /// <summary>
        ///     Gets/sets the wether the do patterns are visible or not.
        /// </summary>
        public bool IsDoPatternVisible
        {
            get
            {
                return fIsDoPatternVisible;
            }

            set
            {
                if (value != fIsDoPatternVisible)
                {
                    fIsDoPatternVisible = value;
                    OnPropertyChanged("IsDoPatternVisible");
                }
            }
        }

        #endregion

        #region IsConditionVisible

        /// <summary>
        ///     Gets the wether the condition for this item should be visible or not.
        ///     It isn't visible when this is the last set of outputs/does.
        /// </summary>
        public bool IsConditionVisible
        {
            get
            {
                var iRule = Owner as PatternRule;
                if (iRule != null && iRule.OutputSet == this)
                {
                    return false;
                }

                if (Owner is PatternRule)
                {
                    // if the owner is the rule, it's for the root conditionals, which always have a conditional and the x button should always be visible.
                    return true;
                }

                var iOwner = Owner as IConditionalOutputsCollectionOwner;

                    // for topic questions and responses-for, the last item is always the fallback.
                return iOwner == null
                        || (iOwner.Conditionals.Count > 0 && iOwner.Conditionals[iOwner.Conditionals.Count - 1] != this);
            }
        }

        #endregion

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
                if (Outputs != null && Outputs.Count > 0)
                {
                    return false;
                }

                if (DoPatterns != null && DoPatterns.Count > 0)
                {
                    return false;
                }

                if (Condition != null)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets the rule to which this object belongs, if any.
        /// </summary>
        public override PatternRule Rule
        {
            get
            {
                return Owner as PatternRule;
            }
        }

        /// <summary>
        ///     Gets the ruleOutput to which this object belongs: returns this item.
        ///     Is provided so that there is a common <see langword="interface" /> for
        ///     all objects.
        /// </summary>
        public override PatternRuleOutput RuleOutput
        {
            get
            {
                return this;
            }
        }

        #region Owner

        /// <summary>
        ///     Gets or sets the owner. We <see langword="override" /> the
        ///     <see cref="Owner" /> property so that we can create the Output list at
        ///     the moment that we know the owner. This is important cause we need
        ///     this value to build the collection list.
        /// </summary>
        /// <value>
        ///     The owner.
        /// </value>
        public override Data.IOwnedObject Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                base.Owner = value;
                if (value != null && Outputs == null && Item != null)
                {
                    // the root item gets the outputs prop assigned, otherwise it needs to be created.
                    Outputs = new PatternOutputsCollection(this, (NeuronCluster)Item);
                }

                // Outputs = new PatternOutputsCollection((INeuronWrapper)Owner, (NeuronCluster)Item);
            }
        }

        #endregion

        /// <summary>
        ///     creates a new neuroncluster and attaches to the owner. This makes
        ///     certain that the 'Item' property gets filled for this class.
        /// </summary>
        internal void BuildItem()
        {
            var iOwner = Owner as PatternRule;
            if (iOwner != null && iOwner.Item != null)
            {
                var iNew = NeuronFactory.GetCluster();
                Brain.Current.Add(iNew);
                iOwner.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.TextPatternOutputs, iNew);
            }
        }

        /// <summary>Sets the item, when there is a neuroncluster, we start to monitor it
        ///     for changes + load the possible values.</summary>
        /// <param name="item">The neuron cluster.</param>
        internal void SetItem(NeuronCluster item)
        {
            Item = item;
        }

        /// <summary>
        ///     Called when all the data kept in memory for the UI section can be
        ///     unloaded.
        /// </summary>
        internal override void UnloadUIData()
        {
            base.UnloadUIData();
            foreach (var i in Outputs)
            {
                i.UnloadUIData();
            }

            if (fCondition != null)
            {
                fCondition.UnloadUIData();
            }

            if (DoPatterns != null)
            {
                foreach (var i in DoPatterns)
                {
                    i.UnloadUIData();
                }
            }

            if (Do != null)
            {
                Do.UnloadUIData();
            }
        }

        /// <summary>The rebuild.</summary>
        /// <param name="errors">The errors.</param>
        internal void Rebuild(System.Collections.Generic.List<string> errors)
        {
            if (Outputs != null)
            {
                Outputs.Rebuild(errors);
            }

            if (DoPatterns != null)
            {
                DoPatterns.Rebuild(errors);
            }

            if (Do != null)
            {
                Do.ForceParse();
                if (Do.HasError)
                {
                    errors.Add(Do.ParseError);
                }
            }

            if (Condition != null)
            {
                Condition.ForceParse();
                if (Condition.HasError)
                {
                    errors.Add(Condition.ParseError);
                }
            }
        }

        /// <summary>
        ///     called when a topic gets unloaded, in which case, all the do,
        ///     condition patterns and inputs get unloaded.
        /// </summary>
        internal void ReleaseOutputs()
        {
            if (Outputs != null)
            {
                foreach (var i in Outputs)
                {
                    Parsers.OutputParser.RemoveOutputPattern(i.Item);
                }
            }

            if (DoPatterns != null)
            {
                foreach (var i in DoPatterns)
                {
                    Parsers.DoParser.RemoveDoPattern(i.Item);
                }
            }

            if (Do != null)
            {
                Parsers.DoParser.RemoveDoPattern(Do.Item);
            }

            if (Condition != null)
            {
                Parsers.ConditionParser.RemoveCondPattern(fCondition.Item);
            }
        }

        #region fields

        /// <summary>The f outputs.</summary>
        private PatternOutputsCollection fOutputs;

        /// <summary>The f do patterns.</summary>
        private DoPatternCollection fDoPatterns;

        /// <summary>The f do.</summary>
        private DoPattern fDo;

        /// <summary>The f condition.</summary>
        private ConditionPattern fCondition;

        /// <summary>The f is do pattern visible.</summary>
        private bool fIsDoPatternVisible = true;

        // GridLength fOutputPatternsWidth = new GridLength(1, GridUnitType.Star);
        // GridLength fDoPatternsWidth = new GridLength(0);                                          //not visible by default.
        /// <summary>The f is output sequenced.</summary>
        private bool fIsOutputSequenced;

        #endregion

        #region IsOutputSequenced

        /// <summary>
        ///     Gets/sets the value that indicates wether to use the list of outputs
        ///     randomly (default), or in sequence, which is better for story telling.
        /// </summary>
        public bool IsOutputSequenced
        {
            get
            {
                return fIsOutputSequenced;
            }

            set
            {
                if (value != fIsOutputSequenced)
                {
                    InternalChange = true;
                    try
                    {
                        if (value)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.OutputListTraversalMode, 
                                (ulong)PredefinedNeurons.Sequence);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.OutputListTraversalMode, 
                                (Neuron)null); // simply use the default, uses much less links.
                        }
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    SetIsOutputSequenced(value);
                }
            }
        }

        /// <summary>The set is output sequenced.</summary>
        /// <param name="value">The value.</param>
        private void SetIsOutputSequenced(bool value)
        {
            fIsOutputSequenced = value;
            OnPropertyChanged("IsOutputSequenced");
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
        public bool FocusNewDo
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus && TextPatternEditorResources.FocusOn.PropName == "FocusNewDo"
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
                        "FocusNewDo"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        ///     Called by the editor when the user has requested to remove the
        ///     selected items from the list. The PatternDef should remove from the
        ///     TextPatternEditor, other items can aks their owner to remove the
        ///     child.
        /// </summary>
        internal override void RemoveFromOwner()
        {
            var iRule = Owner as PatternRule;
            System.Diagnostics.Debug.Assert(iRule != null);
            iRule.Outputs = null;
        }

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            EditorsHelper.DeleteConditionalPattern((NeuronCluster)Item);
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.DoPatterns)
                {
                    DoPatterns = null;
                    Do = null;

                        // always reset to null when removed. for new data, always use 'do', which can be set to null.
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    Condition = null;
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.OutputListTraversalMode)
                {
                    SetIsOutputSequenced(false);
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
                if (link.MeaningID == (ulong)PredefinedNeurons.DoPatterns)
                {
                    SetDo((TextNeuron)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    Condition = new ConditionPattern((TextNeuron)link.To);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.OutputListTraversalMode)
                {
                    SetIsOutputSequenced(link.ToID == (ulong)PredefinedNeurons.Sequence);
                }
            }
            else
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.DoPatterns)
                {
                    OnPropertyChanged("HasDoPatterns");
                }
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            if (value == null || Neuron.IsEmpty(value.ID))
            {
                DoPatterns = null;
                Condition = null;
                Do = null;
            }
            else
            {
                var iTraversal = value.FindFirstOut((ulong)PredefinedNeurons.OutputListTraversalMode);
                if (iTraversal != null && iTraversal.ID == (ulong)PredefinedNeurons.Sequence)
                {
                    SetIsOutputSequenced(true);
                }
                else
                {
                    SetIsOutputSequenced(false);
                }

                var iDos = value.FindFirstOut((ulong)PredefinedNeurons.DoPatterns);
                if (iDos != null)
                {
                    if (iDos is NeuronCluster)
                    {
                        // if it's an old school list, use the collection for rendering to stream.
                        DoPatterns = new DoPatternCollection(this, (NeuronCluster)iDos);
                    }
                    else
                    {
                        SetDo(iDos as TextNeuron); // when 'do' is used, don't create the collection, no need for it?
                    }
                }

                var iCondition = value.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                if (iCondition != null)
                {
                    Condition = new ConditionPattern(iCondition);
                }
                else
                {
                    Condition = null;
                }

                if (Owner != null && Outputs == null && value != null)
                {
                    // the root item gets the outputs prop assigned, otherwise it needs to be created.
                    // Outputs = new PatternOutputsCollection((INeuronWrapper)Owner, (NeuronCluster)Item);
                    Outputs = new PatternOutputsCollection(this, (NeuronCluster)Item);

                        // best to link to this, cause that works best for 
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list. Note: this isn't normally
        ///     called, the rule handles this all.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iCond = child as ConditionPattern;
            if (iCond != null)
            {
                Condition = null;
            }
            else
            {
                var iOut = child as OutputPattern;
                if (iOut != null)
                {
                    if (Outputs.Remove(iOut) == false)
                    {
                        base.RemoveChild(child);
                    }
                }
                else
                {
                    var iDo = child as DoPattern;
                    if (iDo == Do)
                    {
                        Do = null;
                    }
                    else
                    {
                        base.RemoveChild(child);
                    }
                }
            }
        }

        /// <summary>Gets the display path that points to the current object. This item
        ///     doesn't directly have a path, cause it's unselectable, but the 2
        ///     textboxes used to create a new object (output/do) are available, so
        ///     provide a path for them.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIRoot iRoot = null;
            FillDisplayPathForThis(ref iRoot);
            if (iRoot != null)
            {
                var iText = System.Windows.Input.Keyboard.FocusedElement as System.Windows.Controls.TextBox;
                if (iText != null)
                {
                    var iTextTag = new Search.DPITextTag((string)iText.Tag);
                    iRoot.Items.Add(iTextTag);
                }

                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iEditor = Root as TextPatternEditor;
            if (iEditor != null)
            {
                var iOwner = Owner as PatternEditorItem;
                if (iOwner != null)
                {
                    iOwner.FillDisplayPathForThis(ref list);
                    var iRule = iOwner as PatternRule;
                    if (iRule != null)
                    {
                        if (iRule.OutputSet == this)
                        {
                            var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.TextPatternOutputs);
                            list.Items.Add(iOut);
                        }
                        else
                        {
                            var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.Condition);
                            list.Items.Add(iOut);
                            var iIndex = iRule.Conditionals.IndexOf(this);
                            var iChild = new Search.DPIChild(iIndex);
                            list.Items.Add(iChild);
                        }
                    }
                    else
                    {
                        var iGroup = Owner as ResponsesForGroup;
                        if (iGroup != null)
                        {
                            var iIndex = iGroup.Conditionals.IndexOf(this);
                            var iChild = new Search.DPIChild(iIndex);
                            list.Items.Add(iChild);
                        }
                    }
                }
                else
                {
                    list = new Search.DPITextPatternEditorRoot();
                    list.Item = iEditor.Item;
                    list.Items.Add(new Search.DPITopicQuestionsSelect());
                    var iIndex = iEditor.Questions.IndexOf(this);
                    var iChild = new Search.DPIChild(iIndex);
                    list.Items.Add(iChild);
                }
            }
            else
            {
                var iProps = Root as ChatbotProperties;
                if (iProps != null)
                {
                    list = new Search.DPIChatbotPropsRoot(ChatbotProperties.SelectedUI.IsRepetetitionSelected);
                    var iIndex = iProps.ResponsesOnRepeat.IndexOf(this);
                    var iChild = new Search.DPIChild(iIndex);
                    list.Items.Add(iChild);
                }
            }
        }

        #endregion
    }
}