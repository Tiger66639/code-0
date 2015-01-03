// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatbotProperties.cs" company="">
//   
// </copyright>
// <summary>
//   Manages the properties of a chatbot project.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Manages the properties of a chatbot project.
    /// </summary>
    public class ChatbotProperties : Data.NamedObject, 
                                     IDocumentInfo, 
                                     INeuronWrapper, 
                                     IEditorSelection, 
                                     Search.IDisplayPathBuilder, 
                                     ITextPatternPasteHandler
    {
        #region internal types

        /// <summary>
        ///     to keep track of which is the currently selected ui tab item.
        /// </summary>
        public enum SelectedUI
        {
            /// <summary>The is pref selected.</summary>
            IsPrefSelected, 

            /// <summary>The is do after selected.</summary>
            IsDoAfterSelected, 

            /// <summary>The is opening stat selected.</summary>
            IsOpeningStatSelected, 

            /// <summary>The is fallback selected.</summary>
            IsFallbackSelected, 

            /// <summary>The is mappging selected.</summary>
            IsMappgingSelected, 

            /// <summary>The is repetetition selected.</summary>
            IsRepetetitionSelected, 

            /// <summary>The is do on startup selected.</summary>
            IsDoOnStartupSelected, 

            /// <summary>The is context selected.</summary>
            IsContextSelected
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ChatbotProperties"/> class. 
        ///     Initializes a new instance of the <see cref="ChatbotProperties"/>
        ///     class.</summary>
        public ChatbotProperties()
        {
            Name = "Chatbot properties";
            fItem = Brain.Current[(ulong)PredefinedNeurons.ResponsesForEmptyParse] as NeuronCluster;
            FallBacks = new PatternOutputsCollection(this, fItem);
            FallBacks.DisplayPathTag = "FocusNewFallback";
            var iStart = Brain.Current[(ulong)PredefinedNeurons.ConversationStarts] as NeuronCluster;
            ConversationStarts = new PatternOutputsCollection(this, iStart);
            ConversationStarts.DisplayPathTag = "FocusNewStart";
            var iDo = Brain.Current[(ulong)PredefinedNeurons.DoAfterStatement] as NeuronCluster;
            DoAfterStatement = new DoPatternCollection(this, iDo);
            SetDoAfter(iDo.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron);

            iDo = Brain.Current[(ulong)PredefinedNeurons.DoOnStartup] as NeuronCluster;
            DoOnStartup = new DoPatternCollection(this, iDo);
            SetDoStartup(iDo.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron);

            var iRepeat = Brain.Current[(ulong)PredefinedNeurons.RepeatOutputPatterns] as NeuronCluster;
            ResponsesOnRepeat = new ConditionalOutputsCollection(this, iRepeat);

            Neuron iFound;
            if (BrainData.Current.DesignerData.Chatbotdata.BotID != Neuron.EmptyId
                && Brain.Current.TryFindNeuron(BrainData.Current.DesignerData.Chatbotdata.BotID, out iFound))
            {
                var iContext = iFound.FindFirstOut((ulong)PredefinedNeurons.Context) as NeuronCluster;
                if (iContext == null)
                {
                    // if there is no context cluster defined yet, do it now, otherwise it gets attached to the incorrect object.
                    iContext = NeuronFactory.GetCluster();
                    Brain.Current.Add(iContext);
                    iContext.Meaning = (ulong)PredefinedNeurons.TextPatternOutputs;
                    Link.Create(iFound, iContext, (ulong)PredefinedNeurons.Context);
                }

                Context = new PatternOutputsCollection(this, iContext);
                Context.DisplayPathTag = "FocusNewContext";
            }

            if (BrainData.Current.DesignerData.Chatbotdata.SynonymResolveSwitchID != Neuron.EmptyId
                && Brain.Current.TryFindNeuron(
                    BrainData.Current.DesignerData.Chatbotdata.SynonymResolveSwitchID, 
                    out iFound))
            {
                var iInt = iFound as IntNeuron;
                if (iInt != null)
                {
                    fAutoResolveSyns = iInt.Value != 0;
                }
            }

            if (BrainData.Current.DesignerData.Chatbotdata.UseOutputVarSwitchID != Neuron.EmptyId
                && Brain.Current.TryFindNeuron(
                    BrainData.Current.DesignerData.Chatbotdata.UseOutputVarSwitchID, 
                    out iFound))
            {
                var iInt = iFound as IntNeuron;
                if (iInt != null)
                {
                    fUseOutputVar = iInt.Value != 0;
                }
            }

            if (BrainData.Current.DesignerData.Chatbotdata.UseSTTWeightID != Neuron.EmptyId
                && Brain.Current.TryFindNeuron(BrainData.Current.DesignerData.Chatbotdata.UseSTTWeightID, out iFound))
            {
                var iInt = iFound as IntNeuron;
                if (iInt != null)
                {
                    fUseSTTWeight = iInt.Value != 0;
                }
            }

            if (BrainData.Current.DesignerData.Chatbotdata.SingleTopPatternResultID != Neuron.EmptyId
                && Brain.Current.TryFindNeuron(
                    BrainData.Current.DesignerData.Chatbotdata.SingleTopPatternResultID, 
                    out iFound))
            {
                var iInt = iFound as IntNeuron;
                if (iInt != null)
                {
                    fSingleTopPatternResult = iInt.Value != 0;
                }
            }
        }

        #endregion

        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object. When this
        ///     object gets a request for a displaypath, it's for one fe field
        ///     editors, so we need to get the UI object that has focus and store it's
        ///     tag value. This is used as a 'property' changed property to let the ui
        ///     focus again.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public Search.DisplayPath GetDisplayPathFromThis()
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iFocused != null)
            {
                var iRoot = new Search.DPIChatbotPropsRoot(SelectedUI.IsPrefSelected);
                var iChild = new Search.DPITextTag((string)iFocused.Tag);
                iRoot.Items.Add(iChild);

                var iText = iFocused as System.Windows.Controls.TextBox;
                if (iText != null)
                {
                    var iRange = new Search.DPITextRange { Start = TextSelectionStart, Length = TextSelectionLength };
                    iRoot.Items.Add(iRange);
                }

                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        #endregion

        /// <summary>The paste from clipboard to list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        public void PasteFromClipboardToList(PatternOutputsCollection list, PatternEditorItem insertAt = null)
        {
            var iSelected = insertAt != null ? insertAt : SelectedItem;
            System.Collections.Generic.List<PatternEditorItem> iRes = null;
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                if (SelectedItems.Count > 1)
                {
                    // if there is more then 1 item selected, we do a delete
                    Delete();
                }
                else
                {
                    SelectedItems.Clear();
                }

                if (System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PastePatternOutputsFromClipboard(list, iSelected);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.INVALIDPATTERNFORMAT);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (var i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>The paste from clipboard to list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        public void PasteFromClipboardToList(InvalidPatternResponseCollection list, PatternEditorItem insertAt = null)
        {
            var iSelected = insertAt != null ? insertAt : SelectedItem;
            System.Collections.Generic.List<PatternEditorItem> iRes = null;
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                if (SelectedItems.Count > 1)
                {
                    // if there is more then 1 item selected, we do a delete
                    Delete();
                }
                else
                {
                    SelectedItems.Clear();
                }

                if (System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.INVALIDPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteInvalidPatternsFromClipboard(list, iSelected);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (var i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>The paste from clipboard to list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        public void PasteFromClipboardToList(InputPatternCollection list, InputPattern insertAt = null)
        {
            var iSelected = insertAt != null ? insertAt : SelectedItem;
            System.Collections.Generic.List<PatternEditorItem> iRes = null;
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                if (SelectedItems.Count > 1)
                {
                    // if there is more then 1 item selected, we do a delete
                    Delete();
                }
                else
                {
                    SelectedItems.Clear();
                }

                if (System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.INVALIDPATTERNFORMAT);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (var i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        // public void PasteFromClipboardToList(DoPatternCollection list, PatternEditorItem insertAt = null)
        // {
        // PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
        // List<PatternEditorItem> iRes = null;
        // WindowMain.UndoStore.BeginUndoGroup(false);
        // try
        // {
        // if (SelectedItems.Count > 1)                                                        //if there is more then 1 item selected, we do a delete
        // Delete();
        // else
        // SelectedItems.Clear();
        // if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
        // iRes = EditorsHelper.PasteExpressionsFromClipboard(list, iSelected, Properties.Resources.OUTPUTPATTERNFORMAT);
        // else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
        // iRes = EditorsHelper.PasteExpressionsFromClipboard(list, iSelected, Properties.Resources.TEXTPATTERNDEFFORMAT);
        // else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
        // iRes = EditorsHelper.PasteExpressionsFromClipboard(list, iSelected, Properties.Resources.TEXTPATTERNFORMAT);
        // else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
        // iRes = EditorsHelper.PasteExpressionsFromClipboard(list, iSelected, Properties.Resources.CONDITIONPATTERNFORMAT);
        // else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
        // iRes = EditorsHelper.PasteExpressionsFromClipboard(list, iSelected, Properties.Resources.INVALIDPATTERNFORMAT);
        // }
        // finally
        // {
        // WindowMain.UndoStore.EndUndoGroup();
        // }
        // if (iRes != null)
        // foreach (PatternEditorItem i in iRes)
        // SelectedItems.Add(i);
        // }

        /// <summary>The can paste from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanPasteFromClipboard()
        {
            return System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT);
        }

        /// <summary>Instructs the ui to put Focus on the item which has a tag bound to the
        ///     specified property. It's a trick to let the backend know that the UI
        ///     needs to put focus on one of the 'new item' textboxes.</summary>
        /// <param name="value">The value.</param>
        internal void FocusOn(string value)
        {
            TextPatternEditorResources.NeedsFocus = true;
            TextPatternEditorResources.FocusOn.PropName = value;
            OnPropertyChanged(value);
        }

        /// <summary>
        ///     Makes certain that all the patterns in the global sections have been
        ///     deleted from the network. This is used for loading new data.
        /// </summary>
        internal void ClearPatterns()
        {
            foreach (var i in DoOnStartup.ToArray())
            {
                i.Delete();
            }

            foreach (var i in DoAfterStatement.ToArray())
            {
                i.Delete();
            }

            foreach (var i in FallBacks.ToArray())
            {
                i.Delete();
            }

            foreach (var i in ConversationStarts.ToArray())
            {
                i.Delete();
            }

            foreach (var i in Context.ToArray())
            {
                i.Delete();
            }

            foreach (var i in ResponsesOnRepeat.ToArray())
            {
                i.Delete();
            }
        }

        /// <summary>
        ///     Deletes all the selected items.
        /// </summary>
        public void Delete()
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                foreach (var i in fSelectedItems.ToArray())
                {
                    i.Delete();
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The paste from clipboard.</summary>
        internal void PasteFromClipboard()
        {
            var iSelected = SelectedItem;
            System.Collections.Generic.List<PatternEditorItem> iRes = null;
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                if (SelectedItems.Count > 1)
                {
                    // if there is more then 1 item selected, we do a delete
                    Delete();
                }
                else
                {
                    SelectedItems.Clear();
                }

                if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT))
                {
                    iRes = EditorsHelper.PasteRulesFromClipboard(null, iSelected);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PastePatternInputsFromClipboard(null, iSelected);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT))
                {
                    if (IsOpeningStatSelected)
                    {
                        iRes = EditorsHelper.PastePatternOutputsFromClipboard(ConversationStarts, iSelected);
                    }
                    else if (IsRepetetitionSelected)
                    {
                        iRes = EditorsHelper.PastePatternOutputsFromClipboard(ResponsesOnRepeat, iSelected);
                    }
                    else if (IsFallbackSelected)
                    {
                        iRes = EditorsHelper.PastePatternOutputsFromClipboard(FallBacks, iSelected);
                    }
                    else
                    {
                        iRes = EditorsHelper.PasteExpressionsFromClipboard(
                            this, 
                            iSelected, 
                            Properties.Resources.OUTPUTPATTERNFORMAT);
                    }
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        this, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        this, 
                        iSelected, 
                        Properties.Resources.INVALIDPATTERNFORMAT);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (var i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>The paste from clipboard to list as cond.</summary>
        /// <param name="insertAt">The insert at.</param>
        internal void PasteFromClipboardToListAsCond(PatternEditorItem insertAt = null)
        {
            var iSelected = insertAt != null ? insertAt : SelectedItem;
            System.Collections.Generic.List<PatternEditorItem> iRes = null;
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                if (SelectedItems.Count > 1)
                {
                    // if there is more then 1 item selected, we do a delete
                    Delete();
                }
                else
                {
                    SelectedItems.Clear();
                }

                if (System.Windows.Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        ResponsesOnRepeat, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        ResponsesOnRepeat, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        ResponsesOnRepeat, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PastePatternConditionsFromClipboard(ResponsesOnRepeat, iSelected);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        ResponsesOnRepeat, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT))
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        ResponsesOnRepeat, 
                        iSelected, 
                        Properties.Resources.INVALIDPATTERNFORMAT);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (var i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>
        ///     Commits the user Birtdady text.
        /// </summary>
        internal void CommitUserBDay()
        {
            System.DateTime iDate;
            if (System.DateTime.TryParse(UserBirthdayText, out iDate))
            {
                if (iDate != UserBirthday)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        SetUserBDay(iDate);

                            // so we don't call ourselves again + make certain that PropChanged isn't called to much.
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>The commit bot b day.</summary>
        internal void CommitBotBDay()
        {
            System.DateTime iDate;
            if (System.DateTime.TryParse(BotBirthdayText, out iDate))
            {
                if (iDate != BotBirthday)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        SetBotBDay(iDate);

                            // so we don't call ourselves again + make certain that PropChanged isn't called to much.
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #region fields

        /// <summary>The f fall backs.</summary>
        private PatternOutputsCollection fFallBacks;

        /// <summary>The f conversation starts.</summary>
        private PatternOutputsCollection fConversationStarts;

        /// <summary>The f do after statement.</summary>
        private DoPatternCollection fDoAfterStatement;

        /// <summary>The f do on startup.</summary>
        private DoPatternCollection fDoOnStartup;

        /// <summary>The f context.</summary>
        private PatternOutputsCollection fContext;

        /// <summary>The f responses on repeat.</summary>
        private ConditionalOutputsCollection fResponsesOnRepeat;

        /// <summary>The f selected items.</summary>
        private readonly SelectedPatternItemsCollection fSelectedItems = new SelectedPatternItemsCollection();

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        /// <summary>The f do after.</summary>
        private DoPattern fDoAfter;

        /// <summary>The f do startup.</summary>
        private DoPattern fDoStartup;

        /// <summary>The f user birthday.</summary>
        private System.DateTime? fUserBirthday;

        /// <summary>The f bot birthday.</summary>
        private System.DateTime? fBotBirthday;

        /// <summary>The f user name.</summary>
        private string fUserName;

        /// <summary>The f bot gender.</summary>
        private int? fBotGender;

        /// <summary>The f user gender.</summary>
        private int? fUserGender;

        /// <summary>The f bot birthday text.</summary>
        private string fBotBirthdayText;

        /// <summary>The f user birthday text.</summary>
        private string fUserBirthdayText;

        /// <summary>The f bot name.</summary>
        private string fBotName;

        /// <summary>The f selected ui tab.</summary>
        private SelectedUI fSelectedUiTab = SelectedUI.IsPrefSelected;

        /// <summary>The f auto resolve syns.</summary>
        private bool fAutoResolveSyns;

        /// <summary>The f use output var.</summary>
        private bool fUseOutputVar;

        /// <summary>The f use stt weight.</summary>
        private bool fUseSTTWeight;

        /// <summary>The f single top pattern result.</summary>
        private bool fSingleTopPatternResult;

        /// <summary>The f module props.</summary>
        private System.Collections.Generic.List<BaseBotPropDecl> fModuleProps;

        #endregion

        #region prop

        #region IDocumentInfo Members

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        public string DocumentTitle
        {
            get
            {
                return "Chatbot properties";
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public string DocumentInfo
        {
            get
            {
                return "Shows the properties that are currently assigned to the chatbot project.";
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public string DocumentType
        {
            get
            {
                return "Properties";
            }
        }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        public object DocumentIcon
        {
            get
            {
                return "/Images/Properties.png";
            }
        }

        #endregion

        #region FallBacks

        /// <summary>
        ///     Gets the outputs available for this pattern definition.
        /// </summary>
        public PatternOutputsCollection FallBacks
        {
            get
            {
                return fFallBacks;
            }

            internal set
            {
                fFallBacks = value;
                OnPropertyChanged("FallBacks");
            }
        }

        #endregion

        #region ResponsesOnRepeat

        /// <summary>
        ///     Gets the conditional responses to use when a repetition has been
        ///     encountered (the first condition that passes is used).
        /// </summary>
        public ConditionalOutputsCollection ResponsesOnRepeat
        {
            get
            {
                return fResponsesOnRepeat;
            }

            internal set
            {
                fResponsesOnRepeat = value;
                OnPropertyChanged("ResponsesOnRepeat");
            }
        }

        #endregion

        #region ConversationStarts

        /// <summary>
        ///     Gets the list of output paterns that can be used to start a
        ///     conversation.
        /// </summary>
        public PatternOutputsCollection ConversationStarts
        {
            get
            {
                return fConversationStarts;
            }

            internal set
            {
                fConversationStarts = value;
                OnPropertyChanged("ConversationStarts");
            }
        }

        #endregion

        #region DoAfterStatement

        /// <summary>
        ///     Gets the list of do paterns that should be executed afater each
        ///     input/output pair.
        /// </summary>
        public DoPatternCollection DoAfterStatement
        {
            get
            {
                return fDoAfterStatement;
            }

            internal set
            {
                fDoAfterStatement = value;
                OnPropertyChanged("DoAfterStatement");
            }
        }

        #endregion

        #region Do

        /// <summary>
        ///     Gets/sets the do pattern that should be exeucted just before this
        ///     output pattern.
        /// </summary>
        public DoPattern DoAfter
        {
            get
            {
                return fDoAfter;
            }

            internal set
            {
                if (value != fDoAfter)
                {
                    if (fDoAfter != null)
                    {
                        UnRegisterChild(fDoAfter);
                    }

                    fDoAfter = value;
                    if (fDoAfter != null)
                    {
                        RegisterChild(fDoAfter);
                        EditorsHelper.SetFirstOutgoingLinkTo(
                            DoAfterStatement.Cluster, 
                            (ulong)PredefinedNeurons.DoPatterns, 
                            fDoAfter);
                    }

                    OnPropertyChanged("DoAfter");
                    OnPropertyChanged("HasDoAfter");
                }
            }
        }

        /// <summary>The set do after.</summary>
        /// <param name="value">The value.</param>
        private void SetDoAfter(TextNeuron value)
        {
            if (fDoAfter != null)
            {
                UnRegisterChild(fDoAfter);
            }

            if (value != null)
            {
                fDoAfter = new DoPattern(value);
                if (fDoAfter != null)
                {
                    RegisterChild(fDoAfter);
                }
            }
            else
            {
                fDoAfter = null;
            }

            OnPropertyChanged("DoAfter");
            OnPropertyChanged("HasDoAfter");
        }

        #endregion

        #region HasDoAfter

        /// <summary>
        ///     Gets/sets wether there is a do section or not.
        /// </summary>
        public bool HasDoAfter
        {
            get
            {
                return fDoAfter != null;
            }

            set
            {
                if (value != HasDoAfter)
                {
                    if (value == false)
                    {
                        if (fDoAfter.Item != null && fDoAfter.Item.ID != Neuron.TempId)
                        {
                            EditorsHelper.DeleteDoPattern(fDoAfter.Item as TextNeuron);
                        }

                        fDoAfter = null;
                        OnPropertyChanged("DoAfter");
                        OnPropertyChanged("HasDoAfter");
                    }
                    else
                    {
                        var iNew = NeuronFactory.GetText(string.Empty);
                        Brain.Current.Add(iNew);
                        EditorsHelper.SetFirstOutgoingLinkTo(
                            DoAfterStatement.Cluster, 
                            (ulong)PredefinedNeurons.DoPatterns, 
                            iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fDoAfter;
                        SetDoAfter(iNew);
                    }
                }
            }
        }

        #endregion

        #region DoOnStartup

        /// <summary>
        ///     Gets the list of do paterns that should be executed afater each
        ///     input/output pair.
        /// </summary>
        public DoPatternCollection DoOnStartup
        {
            get
            {
                return fDoOnStartup;
            }

            internal set
            {
                fDoOnStartup = value;
                OnPropertyChanged("DoOnStartup");
            }
        }

        #endregion

        #region DoStartup

        /// <summary>
        ///     Gets/sets the do pattern that should be exeucted just before this
        ///     output pattern.
        /// </summary>
        public DoPattern DoStartup
        {
            get
            {
                return fDoStartup;
            }

            internal set
            {
                if (value != fDoStartup)
                {
                    if (fDoStartup != null)
                    {
                        UnRegisterChild(fDoStartup);
                    }

                    fDoStartup = value;
                    if (fDoStartup != null)
                    {
                        RegisterChild(fDoStartup);
                        EditorsHelper.SetFirstOutgoingLinkTo(
                            DoOnStartup.Cluster, 
                            (ulong)PredefinedNeurons.DoPatterns, 
                            fDoStartup);
                    }

                    OnPropertyChanged("DoStartup");
                    OnPropertyChanged("HasDoStartup");
                }
            }
        }

        /// <summary>The set do startup.</summary>
        /// <param name="value">The value.</param>
        private void SetDoStartup(TextNeuron value)
        {
            if (fDoStartup != null)
            {
                UnRegisterChild(fDoStartup);
            }

            if (value != null)
            {
                fDoStartup = new DoPattern(value);
                if (fDoStartup != null)
                {
                    RegisterChild(fDoStartup);
                }
            }
            else
            {
                fDoStartup = null;
            }

            OnPropertyChanged("DoStartup");
            OnPropertyChanged("HasDoStartup");
        }

        #endregion

        #region HasDoStartup

        /// <summary>
        ///     Gets/sets wether there is a do section or not.
        /// </summary>
        public bool HasDoStartup
        {
            get
            {
                return fDoStartup != null;
            }

            set
            {
                if (value != HasDoStartup)
                {
                    if (value == false)
                    {
                        EditorsHelper.DeleteDoPattern(fDoStartup.Item as TextNeuron);
                        fDoStartup = null;
                        OnPropertyChanged("DoStartup");
                        OnPropertyChanged("HasDoStartup");
                    }
                    else
                    {
                        var iNew = NeuronFactory.GetText(string.Empty);
                        Brain.Current.Add(iNew);
                        EditorsHelper.SetFirstOutgoingLinkTo(
                            DoOnStartup.Cluster, 
                            (ulong)PredefinedNeurons.DoPatterns, 
                            iNew);
                        TextPatternEditorResources.NeedsFocus = true;
                        TextPatternEditorResources.FocusOn.Item = fDoStartup;
                        SetDoStartup(iNew);
                    }
                }
            }
        }

        #endregion

        #region Context

        /// <summary>
        ///     Gets the list of do-patterns who's results can be used as 'context'
        ///     for callbacks like 'Attribute'.
        /// </summary>
        public PatternOutputsCollection Context
        {
            get
            {
                return fContext;
            }

            internal set
            {
                fContext = value;
                OnPropertyChanged("Context");
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item
        {
            get
            {
                return fItem;
            }
        }

        #endregion

        #region IEditorSelection Members

        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        public System.Collections.IList SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        /// <summary>
        ///     Gets/sets the currently selected item. If there are multiple
        ///     selections, the first is returned.
        /// </summary>
        object IEditorSelection.SelectedItem
        {
            get
            {
                return SelectedItem;
            }
        }

        /// <summary>
        ///     Gets the currently selected item. If there are multiple selections,
        ///     the first is returned.
        /// </summary>
        /// <value>
        /// </value>
        public PatternEditorItem SelectedItem
        {
            get
            {
                if (fSelectedItems.Count > 0)
                {
                    return fSelectedItems[0];
                }

                return null;
            }
        }

        #endregion

        #region UserName

        /// <summary>
        ///     Gets/sets the name of the user
        /// </summary>
        public string UserName
        {
            get
            {
                if (fUserName == null)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.CreatorID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid CreatorID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.NameID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid NameID. This can only be done with a designer!");
                    }

                    fUserName = GetName(BrainData.Current.DesignerData.Chatbotdata.CreatorID);
                }

                return fUserName;
            }

            set
            {
                if (value != fUserName)
                {
                    OnPropertyChanging("UserName", fUserName, value);
                    SetName(BrainData.Current.DesignerData.Chatbotdata.CreatorID, value);
                    fUserName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }

        #endregion

        #region UserBirthdayText

        /// <summary>
        ///     Gets/sets the birthday of the bot, expressed as a string. We use this
        ///     cause the binding to a datepicker doesn't work properly with date.
        /// </summary>
        public string UserBirthdayText
        {
            get
            {
                if (fUserBirthdayText == null)
                {
                    var iDate = UserBirthday;
                    fUserBirthdayText = iDate.ToString();
                }

                return fUserBirthdayText;
            }

            set
            {
                if (fUserBirthdayText != value)
                {
                    OnPropertyChanging("UserBirthdayText", fUserBirthdayText, value);
                    SetUserBDayText(value);
                }
            }
        }

        #endregion

        #region UserBirthday

        /// <summary>
        ///     Gets/sets the birthday of the user
        /// </summary>
        public System.DateTime UserBirthday
        {
            get
            {
                if (fUserBirthday.HasValue == false)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.CreatorID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid CreatorID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.BirthdayID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid BirthdayID. This can only be done with a designer!");
                    }

                    fUserBirthday = GetBDay(BrainData.Current.DesignerData.Chatbotdata.CreatorID);
                }

                if (fUserBirthday.HasValue)
                {
                    return fUserBirthday.Value;
                }

                return System.DateTime.Today;
            }

            set
            {
                if (value != fUserBirthday)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        OnPropertyChanging("UserBirthday", UserBirthday, value);

                            // important: use the prop to store prev value, otherwise we don't get the same value
                        SetUserBDay(value);
                        SetUserBDayText(value.ToString());
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Sets the user B day. Don't calls OnPropertyChanged, cause that's
        ///     intrusive while editing the date.</summary>
        /// <param name="value">The value.</param>
        private void SetUserBDay(System.DateTime value)
        {
            SetBDay(BrainData.Current.DesignerData.Chatbotdata.CreatorID, value);
            fUserBirthday = value;
            OnPropertyChanged("UserBirthday");
        }

        /// <summary>The set user b day text.</summary>
        /// <param name="value">The value.</param>
        private void SetUserBDayText(string value)
        {
            fUserBirthdayText = value;
            OnPropertyChanged("UserBirthdayText");
        }

        #endregion

        #region BotName

        /// <summary>
        ///     Gets/sets the name of the user
        /// </summary>
        public string BotName
        {
            get
            {
                if (fBotName == null)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.BotID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid BotID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.NameID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid NameID. This can only be done with a designer!");
                    }

                    fBotName = GetName(BrainData.Current.DesignerData.Chatbotdata.BotID);
                }

                return fBotName;
            }

            set
            {
                if (value != fBotName)
                {
                    OnPropertyChanging("BotName", fBotName, value);
                    SetName(BrainData.Current.DesignerData.Chatbotdata.BotID, value);
                    fBotName = value;
                    OnPropertyChanged("BotName");
                }
            }
        }

        #endregion

        #region BotBirthdayText

        /// <summary>
        ///     Gets/sets the birthday of the bot, expressed as a string. We use this
        ///     cause the binding to a datepicker doesn't work properly with date.
        /// </summary>
        public string BotBirthdayText
        {
            get
            {
                if (fBotBirthdayText == null)
                {
                    var iDate = BotBirthday;
                    fBotBirthdayText = iDate.ToString();
                }

                return fBotBirthdayText;
            }

            set
            {
                if (BotBirthdayText != value)
                {
                    OnPropertyChanging("BotBirthdayText", fBotBirthdayText, value);
                    SetBotBDayText(value);
                }
            }
        }

        #endregion

        #region BotBirthday

        /// <summary>
        ///     Gets/sets the birthday of the chatbot.
        /// </summary>
        public System.DateTime BotBirthday
        {
            get
            {
                if (fBotBirthday.HasValue == false)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.BotID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid BotID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.BirthdayID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid BirthdayID. This can only be done with a designer!");
                    }

                    fBotBirthday = GetBDay(BrainData.Current.DesignerData.Chatbotdata.BotID);
                }

                if (fBotBirthday.HasValue)
                {
                    return fBotBirthday.Value;
                }

                return System.DateTime.Today;
            }

            set
            {
                if (value != fBotBirthday)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        OnPropertyChanging("BotBirthday", BotBirthday, value);

                            // important: use the prop to store prev value, otherwise we don't get the same value
                        SetBotBDay(value);
                        SetBotBDayText(value.ToString());
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        /// <summary>Sets the bot B day. Don't calls OnPropertyChanged, cause that's
        ///     intrusive while editing the date.</summary>
        /// <param name="value">The value.</param>
        private void SetBotBDay(System.DateTime value)
        {
            SetBDay(BrainData.Current.DesignerData.Chatbotdata.BotID, value);
            fBotBirthday = value;
            OnPropertyChanged("BotBirthday");
        }

        /// <summary>The set bot b day text.</summary>
        /// <param name="value">The value.</param>
        private void SetBotBDayText(string value)
        {
            fBotBirthdayText = value;
            OnPropertyChanged("BotBirthdayText");
        }

        #endregion

        #region ChatbotMode

        /// <summary>
        ///     Gets/sets the mode of the chatbot
        /// </summary>
        /// <remarks>
        ///     When we change from stand-alone to online, we make certain that there
        ///     is no current user registered. When we change from online to
        ///     stand-alone, we make certain that the creator is assigned as the
        ///     current user.
        /// </remarks>
        public int ChatbotMode
        {
            get
            {
                return BrainData.Current.DesignerData.Chatbotdata.BotType;
            }

            set
            {
                if (value != ChatbotMode)
                {
                    OnPropertyChanging("ChatbotMode", ChatbotMode, value);
                    var iChannel =
                        (from i in BrainData.Current.CommChannels where i is ChatBotChannel select (ChatBotChannel)i)
                            .FirstOrDefault();
                    if (iChannel != null && BrainData.Current.DesignerData.Chatbotdata.RefToUserID != Neuron.EmptyId)
                    {
                        // check if there is a 'meaning' id defined.
                        Neuron iMeaning;
                        if (Brain.Current.TryFindNeuron(
                            BrainData.Current.DesignerData.Chatbotdata.RefToUserID, 
                            out iMeaning))
                        {
                            if (value == 1)
                            {
                                // we change to online bot, so remove any ref to current user.
                                iChannel.TextSin.SetFirstOutgoingLinkTo(iMeaning.ID, null);
                            }
                            else
                            {
                                Neuron iUser;
                                if (Brain.Current.TryFindNeuron(
                                    BrainData.Current.DesignerData.Chatbotdata.CreatorID, 
                                    out iUser))
                                {
                                    Link.Create(iChannel.TextSin, iUser, iMeaning);
                                }
                                else
                                {
                                    throw new System.InvalidOperationException(
                                        "Please provide a valid ID for the neuron that identifies the creator of the chatbot!");
                                }
                            }
                        }
                        else
                        {
                            throw new System.InvalidOperationException(
                                "Please provide a valid ID for the neuron that identifies the relationship between the chatbot and the current user!");
                        }
                    }

                    BrainData.Current.DesignerData.Chatbotdata.BotType = value;
                    OnPropertyChanged("ChatbotMode");
                }
            }
        }

        #endregion

        #region ModuleProps

        /// <summary>
        ///     Gets the list of extra properties which are declared in the modules.
        /// </summary>
        public System.Collections.Generic.List<BaseBotPropDecl> ModuleProps
        {
            get
            {
                if (fModuleProps == null)
                {
                    fModuleProps = BaseBotPropDecl.CreateFor(BrainData.Current.DesignerData.ModulePropIds);
                }

                return fModuleProps;
            }
        }

        #endregion

        #region InstructionsView

        /// <summary>
        ///     Gets a view for all the module properties.
        /// </summary>
        public System.Windows.Data.ListCollectionView ModulePropsView
        {
            get
            {
                var iRes = new System.Windows.Data.ListCollectionView(ModuleProps);
                iRes.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription(
                        "Category", 
                        System.ComponentModel.ListSortDirection.Ascending));
                iRes.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription(
                        "Title", 
                        System.ComponentModel.ListSortDirection.Ascending));
                var iDesc = new System.Windows.Data.PropertyGroupDescription();
                iDesc.PropertyName = "Category";
                iRes.GroupDescriptions.Add(iDesc);
                return iRes;
            }
        }

        #endregion

        #region AutoResolveSyns

        /// <summary>
        ///     Gets/sets the value that indicates if the system should auto resovle
        ///     the synonyms or not.
        /// </summary>
        public bool AutoResolveSyns
        {
            get
            {
                return fAutoResolveSyns;
            }

            set
            {
                if (value != fAutoResolveSyns)
                {
                    Neuron iFound;
                    fAutoResolveSyns = value;
                    if (BrainData.Current.DesignerData.Chatbotdata.SynonymResolveSwitchID != Neuron.EmptyId
                        && Brain.Current.TryFindNeuron(
                            BrainData.Current.DesignerData.Chatbotdata.SynonymResolveSwitchID, 
                            out iFound))
                    {
                        var iInt = iFound as IntNeuron;
                        if (iInt != null)
                        {
                            iInt.Value = value ? 1 : 0;
                        }
                    }

                    OnPropertyChanged("AutoResolveSyns");
                }
            }
        }

        #endregion

        #region UseOutputVar

        /// <summary>
        ///     Gets/sets the value that indicates if the network needs to use the
        ///     output var for collecting previous output or if it is collected
        ///     automatically.
        /// </summary>
        public bool UseOutputVar
        {
            get
            {
                return fUseOutputVar;
            }

            set
            {
                if (value != fUseOutputVar)
                {
                    Neuron iFound;
                    fUseOutputVar = value;
                    if (BrainData.Current.DesignerData.Chatbotdata.UseOutputVarSwitchID != Neuron.EmptyId
                        && Brain.Current.TryFindNeuron(
                            BrainData.Current.DesignerData.Chatbotdata.UseOutputVarSwitchID, 
                            out iFound))
                    {
                        var iInt = iFound as IntNeuron;
                        if (iInt != null)
                        {
                            iInt.Value = value ? 1 : 0;
                        }
                    }

                    OnPropertyChanged("UseOutputVar");
                }
            }
        }

        #endregion

        #region UseSTTWeight

        /// <summary>
        ///     Gets/sets the value that indicates if the weight provided by the stt
        ///     should be used or not.
        /// </summary>
        public bool UseSTTWeight
        {
            get
            {
                return fUseSTTWeight;
            }

            set
            {
                if (value != fUseOutputVar)
                {
                    Neuron iFound;
                    fUseSTTWeight = value;
                    if (BrainData.Current.DesignerData.Chatbotdata.UseSTTWeightID != Neuron.EmptyId
                        && Brain.Current.TryFindNeuron(
                            BrainData.Current.DesignerData.Chatbotdata.UseSTTWeightID, 
                            out iFound))
                    {
                        var iInt = iFound as IntNeuron;
                        if (iInt != null)
                        {
                            iInt.Value = value ? 1 : 0;
                        }
                    }

                    OnPropertyChanged("UseSTTWeigth");
                }
            }
        }

        #endregion

        #region SingleTopPatternResult

        /// <summary>
        ///     Gets/sets the value that indicates if the bot only searches for a
        ///     single pattern result or if multiple top-patterns can follow each
        ///     other. When true, processing is faster. When false, more results can
        ///     be found.
        /// </summary>
        public bool SingleTopPatternResult
        {
            get
            {
                return fSingleTopPatternResult;
            }

            set
            {
                if (value != fSingleTopPatternResult)
                {
                    Neuron iFound;
                    if (BrainData.Current.DesignerData.Chatbotdata.SingleTopPatternResultID != Neuron.EmptyId
                        && Brain.Current.TryFindNeuron(
                            BrainData.Current.DesignerData.Chatbotdata.SingleTopPatternResultID, 
                            out iFound))
                    {
                        var iInt = iFound as IntNeuron;
                        if (iInt != null)
                        {
                            iInt.Value = value ? 1 : 0;
                        }
                    }

                    fSingleTopPatternResult = value;
                    OnPropertyChanged("SingleTopPatternResult");
                }
            }
        }

        #endregion

        #region BotGender

        /// <summary>
        ///     <para>Gets/sets the gender of the bot</para>
        ///     <list type="number">
        ///         <item>
        ///             <description>= male, 1 = female, 2 = herma</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public int BotGender
        {
            get
            {
                if (fBotGender.HasValue == false)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.BotID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid BotID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.GenderID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid GenderID. This can only be done with a designer!");
                    }

                    fBotGender = GetGender(BrainData.Current.DesignerData.Chatbotdata.BotID);
                }

                if (fBotGender.HasValue)
                {
                    return fBotGender.Value;
                }

                return -1;
            }

            set
            {
                if (value != fBotGender)
                {
                    OnPropertyChanging("BotGender", BotGender, value);

                        // important: use the prop to get the value, otherwise we get  null on the first run
                    SetGender(BrainData.Current.DesignerData.Chatbotdata.BotID, value);
                    fBotGender = value;
                    OnPropertyChanged("BotGender");
                }
            }
        }

        #endregion

        #region UserGender

        /// <summary>
        ///     <para>Gets/sets the gender of the user.</para>
        ///     <list type="number">
        ///         <item>
        ///             <description>= male, 1 = female, 2 = herma</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public int UserGender
        {
            get
            {
                if (fUserGender.HasValue == false)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.CreatorID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid CreatorID. This can only be done with a designer!");
                    }

                    if (BrainData.Current.DesignerData.Chatbotdata.GenderID == Neuron.EmptyId)
                    {
                        throw new System.InvalidOperationException(
                            "Please provide a valid GenderID. This can only be done with a designer!");
                    }

                    fUserGender = GetGender(BrainData.Current.DesignerData.Chatbotdata.CreatorID);
                }

                if (fUserGender.HasValue)
                {
                    return fUserGender.Value;
                }

                return -1;
            }

            set
            {
                if (value != fUserGender)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        OnPropertyChanging("UserGender", UserGender, value);

                            // important: use the prop to get the value, otherwise we get  null on the first run
                        SetGender(BrainData.Current.DesignerData.Chatbotdata.CreatorID, value);
                        fUserGender = value;
                        OnPropertyChanged("UserGender");
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region focusNew

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new-in
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewFallback
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewFallback")
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
                        "FocusNewFallback"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     new-Repeat textbox. This is used to shift focus in the UI. Therefor,
        ///     it always returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewRepeat
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewRepeat")
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
                        "FocusNewRepeat"); // to turn it back off, so we can set it again later on
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
        public bool FocusNewStart
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewStart")
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
                        "FocusNewStart"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        ///// <summary>
        ///// Gets a value indicating whether focus needs to be moved to the new-in textbox.
        ///// This is used to shift focus in the UI. Therefor, it always returns true, this way, the FocusManager can
        ///// bind to it.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [focus new out]; otherwise, <c>false</c>.
        ///// </value>
        // public bool FocusNewDo
        // {
        // get
        // {
        // bool iVal;
        // if (TextPatternEditorResources.NeedsFocus == true && TextPatternEditorResources.FocusOn.PropName == "FocusNewDo")
        // {
        // iVal = true;
        // TextPatternEditorResources.NeedsFocus = false;
        // TextPatternEditorResources.FocusOn.PropName = null;
        // TextPatternEditorResources.FocusOn.Item = null;                            //don't need mem leak.
        // }
        // else
        // iVal = false;
        // if (iVal == true)
        // App.Current.Dispatcher.BeginInvoke(new Action<string>(OnPropertyChanged), System.Windows.Threading.DispatcherPriority.Background, "FocusNewDo"); //to turn it back off, so we can set it again later on
        // return iVal;
        // }
        // }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the new-in
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewContext
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewContext")
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
                        "FocusNewContext"); // to turn it back off, so we can set it again later on
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
        public bool FocusNewDoOnStartup
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewDoOnStartup")
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
                        "FocusNewDoOnStartup"); // to turn it back off, so we can set it again later on
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
        public bool FocusNewCondOfRepeat
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewCondOfRepeat")
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
                        "FocusNewCondOfRepeat"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     username textbox. This is used to shift focus in the UI. Therefor, it
        ///     always returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusUserName
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusUserName")
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
                        "FocusUserName"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the bday
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusUserBDay
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusUserBDay")
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
                        "FocusUserBDay"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the gender
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusUserGender
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusUserGender")
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
                        "FocusUserGender"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the botname
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusBotName
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusBotName")
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
                        "FocusBotName"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the botbday
        ///     textbox. This is used to shift focus in the UI. Therefor, it always
        ///     returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusBotBDay
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusBotBDay")
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
                        "FocusBotBDay"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     botgender textbox. This is used to shift focus in the UI. Therefor, it
        ///     always returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusBotGender
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus
                    && TextPatternEditorResources.FocusOn.PropName == "FocusBotGender")
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
                        "FocusBotGender"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        #endregion

        #region TextSelectionStart

        /// <summary>
        ///     Gets or sets the selection start of the last textbox just before it's
        ///     value changed. This is used to store the correct displaypath for
        ///     restoring focus in an undo action.
        /// </summary>
        /// <value>
        ///     The text selection start.
        /// </value>
        public int TextSelectionStart { get; set; }

        #endregion

        #region TextSelectionLength

        /// <summary>
        ///     Gets or sets the selection length of the last textbox just before it's
        ///     value changed. This is used to store the correct displaypath for
        ///     restoring focus in an undo action.
        /// </summary>
        /// <value>
        ///     The text selection start.
        /// </value>
        public int TextSelectionLength { get; set; }

        #endregion

        #region IsPrefSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsPrefSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsPrefSelected;
            }

            set
            {
                if (value != IsPrefSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsPrefSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsPrefSelected");
                }
            }
        }

        #endregion

        #region IsDoAfterSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsDoAfterSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsDoAfterSelected;
            }

            set
            {
                if (value != IsDoAfterSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsDoAfterSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsDoAfterSelected");
                }
            }
        }

        #endregion

        #region IsDoOnStartupSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsDoOnStartupSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsDoOnStartupSelected;
            }

            set
            {
                if (value != IsDoOnStartupSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsDoOnStartupSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsDoOnStartupSelected");
                }
            }
        }

        #endregion

        #region IsContextSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsContextSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsContextSelected;
            }

            set
            {
                if (value != IsContextSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsContextSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsContextSelected");
                }
            }
        }

        #endregion

        #region IsOpeningStatSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsOpeningStatSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsOpeningStatSelected;
            }

            set
            {
                if (value != IsOpeningStatSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsOpeningStatSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsOpeningStatSelected");
                }
            }
        }

        #endregion

        #region IsFallbackSelected

        /// <summary>
        ///     Gets/sets the wether the preferences tab is selected.
        /// </summary>
        public bool IsFallbackSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsFallbackSelected;
            }

            set
            {
                if (value != IsFallbackSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsFallbackSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsFallbackSelected");
                }
            }
        }

        #endregion

        #region IsMappgingSelected

        /// <summary>
        ///     Gets/sets the wether the Mappgings tab is selected.
        /// </summary>
        public bool IsMappgingSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsMappgingSelected;
            }

            set
            {
                if (value != IsMappgingSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsMappgingSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsMappgingSelected");
                }
            }
        }

        #endregion

        #region IsRepetetitionSelected

        /// <summary>
        ///     Gets/sets the wether the Mappgings tab is selected.
        /// </summary>
        public bool IsRepetetitionSelected
        {
            get
            {
                return fSelectedUiTab == SelectedUI.IsRepetetitionSelected;
            }

            set
            {
                if (value != IsRepetetitionSelected)
                {
                    if (value)
                    {
                        var iPrev = fSelectedUiTab.ToString();
                        fSelectedUiTab = SelectedUI.IsRepetetitionSelected;
                        OnPropertyChanged(iPrev);

                            // the names of the enum items are the same as the prop, so this will raise the other prop of the previous value, so it can also be updated to false.
                    }

                    OnPropertyChanged("IsRepetetitionSelected");
                }
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>changes the selected tab to the new value. This is used by the
        ///     displaypath system (for changing focus).</summary>
        /// <param name="value">The f selected UI.</param>
        internal void SetSectedTab(SelectedUI value)
        {
            var iPrev = fSelectedUiTab;
            fSelectedUiTab = value;
            OnPropertyChanged(iPrev.ToString());
            OnPropertyChanged(value.ToString());
        }

        /// <summary>Sets the gender. Don't generate undo data. Undo is handled by the
        ///     propertyChanging technique, which is faster and more efficient for
        ///     this type.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        private void SetGender(ulong id, int value)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            Neuron iValue = null;

            switch (value)
            {
                case 0:
                    iValue = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.MaleID];
                    break;
                case 1:
                    iValue = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.FemaleID];
                    break;
                case 2:
                    iValue = Brain.Current[BrainData.Current.DesignerData.Chatbotdata.HermaID];
                    break;
                default:
                    break;
            }

            var iFound = false;
            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.GenderID)
                    {
                        i.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, iValue);
                        iFound = true;
                        break;
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            if (iFound == false)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                using (var iList = iCluster.ChildrenW) iList.Add(iNew);
                iNew.SetFirstOutgoingLinkTo(
                    (ulong)PredefinedNeurons.Attribute, 
                    Brain.Current[BrainData.Current.DesignerData.Chatbotdata.GenderID]);
                iNew.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, iValue);
            }
        }

        /// <summary>Gets the gender that is specified for the asset if there is any.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="int?"/>.</returns>
        private int? GetGender(ulong id)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.GenderID)
                    {
                        var iData =
                            (from u in i.Data where u.LinkID == (ulong)PredefinedNeurons.Value select u).FirstOrDefault(
                                );
                        if (iData.Value != null)
                        {
                            if (iData.Value.ID == BrainData.Current.DesignerData.Chatbotdata.MaleID)
                            {
                                return 0;
                            }
                            else if (iData.Value.ID == BrainData.Current.DesignerData.Chatbotdata.FemaleID)
                            {
                                return 1;
                            }
                            else if (iData.Value.ID == BrainData.Current.DesignerData.Chatbotdata.HermaID)
                            {
                                return 2;
                            }
                        }
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            return null;
        }

        /// <summary>Gets the name attached to the specified asset, if there is any.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetName(ulong id)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.NameID)
                    {
                        var iData =
                            (from u in i.Data where u.LinkID == (ulong)PredefinedNeurons.Value select u).FirstOrDefault(
                                );
                        return BrainHelper.GetTextFrom(iData.Value);
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            return null;
        }

        /// <summary>Sets the name <paramref name="value"/> for the specified asset. Note:
        ///     doesn't generate undo data. We use the property undo system for this,
        ///     since this allows for the easiest technique here: the location of the
        ///     data is always the same: it's a simply path.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        private void SetName(ulong id, string value)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            var iFound = false;
            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    // first check if there is a previous 'name' asset item, if so, assign new value + delete possible prev  value.
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.NameID)
                    {
                        var iData =
                            (from u in i.Data where u.LinkID == (ulong)PredefinedNeurons.Value select u).FirstOrDefault(
                                );
                        var iPrevVal = iData.Value;
                        i.Item.SetFirstOutgoingLinkTo(
                            (ulong)PredefinedNeurons.Value, 
                            BrainHelper.GetNeuronForText(value));
                        if (iPrevVal.CanBeDeleted && BrainHelper.HasReferences(iPrevVal) == false)
                        {
                            BrainHelper.DeleteText(iPrevVal);
                        }

                        iFound = true;
                        break;
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            if (iFound == false)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                using (var iList = iCluster.ChildrenW) iList.Add(iNew);
                iNew.SetFirstOutgoingLinkTo(
                    (ulong)PredefinedNeurons.Attribute, 
                    Brain.Current[BrainData.Current.DesignerData.Chatbotdata.NameID]);
                iNew.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, BrainHelper.GetNeuronForText(value));
            }
        }

        /// <summary>Gets the Birthdday attached to the specified asset, if there is any.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="DateTime?"/>.</returns>
        private System.DateTime? GetBDay(ulong id)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.BirthdayID)
                    {
                        var iData =
                            (from u in i.Data where u.LinkID == (ulong)PredefinedNeurons.Value select u).FirstOrDefault(
                                );
                        var iTime = iData.Value as NeuronCluster;
                        if (iTime != null)
                        {
                            return Time.GetTime(iTime);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            return null;
        }

        /// <summary>The set b day.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetBDay(ulong id, System.DateTime value)
        {
            var iCluster = Brain.Current[id] as NeuronCluster;
            if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Asset)
            {
                throw new System.InvalidOperationException("Internal error: Asset expected!");
            }

            var iFound = false;
            var iAssetEditor = new AssetEditor(iCluster);
            iAssetEditor.IsOpen = true;
            try
            {
                foreach (AssetItem i in iAssetEditor.Assets)
                {
                    var iAttrib = i.Attribute;
                    if (iAttrib.ID == BrainData.Current.DesignerData.Chatbotdata.BirthdayID)
                    {
                        var iData =
                            (from u in i.Data where u.LinkID == (ulong)PredefinedNeurons.Value select u).FirstOrDefault(
                                );
                        var iPrevVal = (NeuronCluster)iData.Value;
                        var iVal = Time.Current.GetTimeCluster(value);
                        i.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, iVal);
                        Time.Current.ReleaseTimeCluster(iPrevVal);

                            // we try to release any previous value so that we don't ahve mem leaks || has to be done after assigning the value, so that we can properly release the data, otherwise the time cluster is still referenced somewehere else.
                        iFound = true;
                        break;
                    }
                }
            }
            finally
            {
                iAssetEditor.IsOpen = false; // unload the data so we don't have mem leaks.
            }

            if (iFound == false)
            {
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                using (var iList = iCluster.ChildrenW) iList.Add(iNew);

                iNew.SetFirstOutgoingLinkTo(
                    (ulong)PredefinedNeurons.Attribute, 
                    Brain.Current[BrainData.Current.DesignerData.Chatbotdata.BirthdayID]);
                iNew.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, Time.Current.GetTimeCluster(value));
            }
        }

        #endregion
    }
}