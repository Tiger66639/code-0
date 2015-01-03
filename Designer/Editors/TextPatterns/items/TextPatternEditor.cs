// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternEditor.cs" company="">
//   
// </copyright>
// <summary>
//   an editor that allows the edit of clusters that represent text patterns
//   for a text pattern based chatbot. This is a neuron editor, so that we can
//   group related pattern defintitions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using JaStDev.HAB.Designer.Dialogs;
using JaStDev.HAB.Designer.Search;
using JaStDev.HAB.Designer.WPF.Controls;
using JaStDev.HAB.Parsers;
using JaStDev.LogService;
using JaStDev.UndoSystem.Interfaces;

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an editor that allows the edit of clusters that represent text patterns
    ///     for a text pattern based chatbot. This is a neuron editor, so that we can
    ///     group related pattern defintitions.
    /// </summary>
    public class TextPatternEditor : NeuronEditor, 
                                     IEditorSelection, 
                                     IDisplayPathBuilder, 
                                     ITextPatternPasteHandler, 
                                     IConditionalOutputsCollectionOwner
    {
        #region inner types

        /// <summary>
        ///     all the different tabs/datasources that can be selected.
        /// </summary>
        private enum SelectedDataSource
        {
            /// <summary>The items.</summary>
            Items, 

            /// <summary>The questions.</summary>
            Questions, 

            /// <summary>The filters.</summary>
            Filters
        }

        #endregion

        #region fields

        /// <summary>The f items.</summary>
        private PatternDefCollection fItems;

        /// <summary>The f questions.</summary>
        private ConditionalOutputsCollection fQuestions;

        /// <summary>The f topic filters.</summary>
        private TopicFilterCollection fTopicFilters;

        /// <summary>The f selected items.</summary>
        private SelectedPatternItemsCollection fSelectedItems = new SelectedPatternItemsCollection();

        /// <summary>The f selected rule index.</summary>
        private int fSelectedRuleIndex = -1; // init to -1 so that create first new item works correctly.

        /// <summary>The f zoom.</summary>
        private double fZoom = 1.0;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f add val split pos.</summary>
        private GridLength fAddValSplitPos = new GridLength(1, GridUnitType.Star);

        /// <summary>The f master detail width.</summary>
        private GridLength fMasterDetailWidth = new GridLength(120, GridUnitType.Pixel);

        // GridLength fCalculatePatternSize = new GridLength(1, GridUnitType.Star);

        /// <summary>The f is parsed.</summary>
        private bool fIsParsed = true;

        /// <summary>The f is local.</summary>
        private bool fIsLocal = false;

        /// <summary>The f list requires vertualization.</summary>
        private bool fListRequiresVertualization;

        /// <summary>The f is list view.</summary>
        private bool fIsListView = false;

        /// <summary>The f selected data.</summary>
        private SelectedDataSource fSelectedData = SelectedDataSource.Items;

        /// <summary>The f has duplicate name.</summary>
        private bool fHasDuplicateName = false;

        /// <summary>The f is add mode.</summary>
        private bool fIsAddMode = true;

        /// <summary>The f prev path.</summary>
        private DisplayPath[] fPrevPath = new DisplayPath[3];

                              // allows us to put focus back on the correct position, after a tab or docuement switch.
        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TextPatternEditor"/> class. Initializes a new instance of the <see cref="TextPatternEditor"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public TextPatternEditor(Neuron toWrap)
            : base(toWrap)
        {
            fIsAddMode = Properties.Settings.Default.TextPatternIsAddMode; // get the default add mode from the settings.
            TextPatternEditorResources.NeedsFocus = true;

                // when newly created, we make certain that the 'you say' gets focus. This is the first that consumes the focused property.
        }

        /// <summary>Initializes a new instance of the <see cref="TextPatternEditor"/> class. 
        ///     Initializes a new instance of the <see cref="TextPatternEditor"/>
        ///     class. Required for xml serialation.</summary>
        public TextPatternEditor()
        {
            TextPatternEditorResources.NeedsFocus = true;

                // when newly created, we make certain that the 'you say' gets focus. This is the first that consumes the focused property.
        }

        #endregion

        #region prop

        #region Items

        /// <summary>
        ///     Gets the pattern groups that can be edited.
        /// </summary>
        public PatternDefCollection Items
        {
            get
            {
                return fItems;
            }

            internal set
            {
                fItems = value;
                OnPropertyChanged("Items");
                OnPropertyChanged("EditableItems");
            }
        }

        #endregion

        #region EditableItems

        /// <summary>
        ///     This property is provided so that the list can bind to a single
        ///     property but still allows to <see langword="switch" /> between
        ///     different lists: the regular items list or the questions.
        /// </summary>
        public object EditableItems
        {
            get
            {
                if (IsItemsSelected == true)
                {
                    if (IsMasterDetailView == true)
                    {
                        if (Items != null && SelectedRuleIndex >= 0 && SelectedRuleIndex < Items.Count)
                        {
                            return Items[SelectedRuleIndex];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return Items;
                    }
                }
                else if (IsQuestionsSelected == true)
                {
                    return Questions;
                }
                else
                {
                    return TopicFilters;
                }
            }
        }

        #endregion

        #region CurrentList

        /// <summary>
        ///     Gets the list that currently provides the items for display. This is
        ///     used by the displayPath to get the current item
        /// </summary>
        public IList CurrentList
        {
            get
            {
                if (IsItemsSelected == true)
                {
                    return Items;
                }
                else if (IsQuestionsSelected == true)
                {
                    return Questions;
                }
                else
                {
                    return TopicFilters;
                }
            }
        }

        #endregion

        #region Questions

        /// <summary>
        ///     Gets the list of conditional output sets.
        /// </summary>
        public ConditionalOutputsCollection Questions
        {
            get
            {
                return fQuestions;
            }

            internal set
            {
                fQuestions = value;
                OnPropertyChanged("Questions");
                OnPropertyChanged("Conditionals");
                OnPropertyChanged("EditableItems");
            }
        }

        #endregion

        #region TopicFilters

        /// <summary>
        ///     Gets/sets the list of topic filters
        /// </summary>
        public TopicFilterCollection TopicFilters
        {
            get
            {
                return fTopicFilters;
            }

            set
            {
                fTopicFilters = value;
                OnPropertyChanged("TopicFilters");
            }
        }

        #endregion

        #region IsLocal

        /// <summary>
        ///     Gets/sets the value that indicates if this editor has local input
        ///     patterns or global.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return fIsLocal;
            }

            set
            {
                if (value != fIsLocal)
                {
                    if (value == true)
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Local, Item);
                    }
                    else
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Local, (Neuron)null);
                    }
                }
            }
        }

        /// <summary>The set is local.</summary>
        /// <param name="value">The value.</param>
        private void SetIsLocal(bool value)
        {
            fIsLocal = value;
            OnPropertyChanged("IsLocal");
            try
            {
                List<string> iErrors = new List<string>();
                Rebuild(iErrors);
                if (iErrors.Count > 0)
                {
                    foreach (string i in iErrors)
                    {
                        Log.LogError("IsLocal", i);
                    }

                    MessageBox.Show(
                        string.Join("\n", iErrors), 
                        "Compilation errors", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format("parse failed with error: {1}.", e.Message), 
                    "Compilation errors", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region IConditionalOutputsCollectionOwner Members

        /// <summary>
        ///     Gets the list of conditional output sets.
        /// </summary>
        ConditionalOutputsCollection IConditionalOutputsCollectionOwner.Conditionals
        {
            get
            {
                return Questions;
            }
        }

        #endregion

        #region BrowsableItems

        /// <summary>
        ///     <para>
        ///         Inheriters return a list of children that can be used to browse
        ///         through the content and select a neuron. This is used by the
        ///         <see cref="NeuronDataBrowser" /> objects.
        ///     </para>
        ///     <para>
        ///         Returns all the rules, inputs, conditions,do and output patterns.
        ///     </para>
        /// </summary>
        public override IEnumerator BrowsableItems
        {
            get
            {
                return new TopicEnumerator(this);
            }
        }

        #endregion

        #region BrowsableOutputs

        /// <summary>
        ///     Gets an enumerator for all the outputs in the topic.
        /// </summary>
        public IEnumerator<INeuronInfo> BrowsableOutputs
        {
            get
            {
                BrowsableOutputsEnumerator iRes = new BrowsableOutputsEnumerator(this);

                    // we return an object that can defer the query until actually needed.
                return iRes;
            }
        }

        #endregion

        #region HasDuplicateName

        /// <summary>
        ///     Gets the wether this item has a duplicate name with another topic or
        ///     not
        /// </summary>
        public bool HasDuplicateName
        {
            get
            {
                return fHasDuplicateName;
            }

            protected set
            {
                if (value != fHasDuplicateName)
                {
                    fHasDuplicateName = value;
                    OnPropertyChanged("HasDuplicateName");
                }
            }
        }

        #endregion

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific. start with /
        /// </summary>
        public override string Icon
        {
            get
            {
                return "/Images/TextPatterns/TextPattern_Enabled.png";
            }
        }

        #endregion

        #region DocumentInfo

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public override string DocumentInfo
        {
            get
            {
                return "Text patterns: " + Name;
            }
        }

        #endregion

        #region DocumentType

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public override string DocumentType
        {
            get
            {
                return "Text patterns";
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
        public IList SelectedItems
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
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region SelectedRuleIndex

        /// <summary>
        ///     Gets/sets the index of the rule that is currently selected. This is
        ///     used for the master detail view.
        /// </summary>
        public int SelectedRuleIndex
        {
            get
            {
                return fSelectedRuleIndex;
            }

            set
            {
                if (value != fSelectedRuleIndex)
                {
                    int iPrev = fSelectedRuleIndex;
                    fSelectedRuleIndex = value;
                    OnPropertyChanged("SelectedRuleIndex");
                    if (IsMasterDetailView == true)
                    {
                        // only update when in master detail view. Otherwise, when in list view, the entire list-ui is reloaded, which screws up the focus.
                        if (fItems != null && iPrev >= 0 && iPrev < fItems.Count)
                        {
                            fItems[iPrev].UnloadUIData();
                        }

                        OnPropertyChanged("EditableItems");

                            // update the list as well otherwise the UI doesn't get updated.
                        if (fItems != null && value >= 0 && value < fItems.Count)
                        {
                            fItems[value].LoadUI();
                            fItems[value].IsActive = true; // item needs to monitor changes
                        }
                    }
                }
            }
        }

        #endregion

        #region Zoom

        /// <summary>
        ///     Gets/sets the zoom that should be applied to the visual.
        /// </summary>
        public double Zoom
        {
            get
            {
                return fZoom;
            }

            set
            {
                if (value < 0.001)
                {
                    // need to make certain we don't make it to small.
                    value = 0.001;
                }

                if (fZoom != value)
                {
                    fZoom = value;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                }
            }
        }

        /// <summary>
        ///     Gets/sets the inverse value of the zoom factor that should be applied.
        ///     This is used to re-adjust zoom values for overlays (bummer, need to
        ///     work this way for wpf).
        /// </summary>
        public double ZoomInverse
        {
            get
            {
                return 1 / fZoom;
            }
        }

        /// <summary>
        ///     Gets/sets the zoom factor that should be applied, expressed in procent
        ///     values.
        /// </summary>
        public double ZoomProcent
        {
            get
            {
                return fZoom * 100;
            }

            set
            {
                double iVal = value / 100;
                if (fZoom != iVal)
                {
                    fZoom = iVal;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                }
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scroll position
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                if (value < 0)
                {
                    // can't have values smaller than 0.
                    value = 0;
                }

                if (value != fVerScrollPos)
                {
                    fVerScrollPos = value;
                    OnPropertyChanged("VerScrollPos");
                }
            }
        }

        #endregion

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scrollposition of the ui
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                fHorScrollPos = value;
                OnPropertyChanged("HorScrollPos");
            }
        }

        #endregion

        #region AddValSplitPos

        /// <summary>
        ///     Gets/sets the position of the 'add' new item splitter.
        /// </summary>
        public GridLength AddValSplitPos
        {
            get
            {
                return fAddValSplitPos;
            }

            set
            {
                fAddValSplitPos = value;
                OnPropertyChanged("AddValSplitPos");
            }
        }

        #endregion

        #region IsAddMode

        /// <summary>
        ///     Gets/sets the switchvalue that determins if there are extra lines at
        ///     the bottom of each list for adding new items or not.
        /// </summary>
        public bool IsAddMode
        {
            get
            {
                return fIsAddMode;
            }

            set
            {
                fIsAddMode = value;
                OnPropertyChanged("IsAddMode");
            }
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
                if (TextPatternEditorResources.NeedsFocus == true
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

                if (iVal == true)
                {
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged), 
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
                if (TextPatternEditorResources.NeedsFocus == true
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewIn"
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

                if (iVal == true)
                {
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewIn"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     new-question textbox. This is used to shift focus in the UI. Therefor,
        ///     it always returns true, this way, the FocusManager can bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewQuestion
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus == true
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewQuestion"
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

                if (iVal == true)
                {
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewQuestion"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     new-topic-filter textbox. This is used to shift focus in the UI.
        ///     Therefor, it always returns true, this way, the FocusManager can bind
        ///     to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewTopicFilter
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus == true
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewTopicFilter"
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

                if (iVal == true)
                {
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewTopicFilter"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether focus needs to be moved to the
        ///     new-condition of question textbox. This is used to shift focus in the
        ///     UI. Therefor, it always returns true, this way, the FocusManager can
        ///     bind to it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [focus new out]; otherwise, <c>false</c> .
        /// </value>
        public bool FocusNewCondOfQuestion
        {
            get
            {
                bool iVal;
                if (TextPatternEditorResources.NeedsFocus == true
                    && TextPatternEditorResources.FocusOn.PropName == "FocusNewCondOfQuestion"
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

                if (iVal == true)
                {
                    App.Current.Dispatcher.BeginInvoke(
                        new Action<string>(OnPropertyChanged), 
                        System.Windows.Threading.DispatcherPriority.Background, 
                        "FocusNewCondOfQuestion"); // to turn it back off, so we can set it again later on
                }

                return iVal;
            }
        }

        #endregion

        #region ListRequiresVertualization

        /// <summary>
        ///     Gets/sets if the list that displays the data should be virtualized or
        ///     not. This depends on the nr of items. We do this cause WPF can't
        ///     handle large datasets.
        /// </summary>
        public bool ListRequiresVertualization
        {
            get
            {
                return fListRequiresVertualization;
            }

            set
            {
                fListRequiresVertualization = value;
                OnPropertyChanged("ListRequiresVertualization");
            }
        }

        #endregion

        #region LastSearchQuery

        /// <summary>
        ///     Gets or sets the last search query.
        /// </summary>
        /// <remarks>
        ///     keeps track of the last search that was started, so we can use the
        ///     'f3' for continue search, while the search box has already been
        ///     closed.
        /// </remarks>
        /// <value>
        ///     The last search query.
        /// </value>
        public FindInTextData LastSearchQuery { get; set; }

        #endregion

        #region IsItemsSelected

        /// <summary>
        ///     Gets/sets wether the <see cref="Items" /> list is selected or the
        ///     <see cref="Questions" /> list.
        /// </summary>
        public bool IsItemsSelected
        {
            get
            {
                return fSelectedData == SelectedDataSource.Items;
            }

            set
            {
                if (IsItemsSelected != value && value == true)
                {
                    StorePrevPath();
                    fSelectedData = SelectedDataSource.Items;
                    if (fPrevPath[0] != null)
                    {
                        fPrevPath[0].SelectPathResult();

                            // this allows us to move focus back to the correct position, as it was when we left the tab.
                    }

                    OnPropertyChanged("IsItemsSelected");
                    OnPropertyChanged("IsQuestionsSelected");
                    OnPropertyChanged("EditableItems");
                    OnPropertyChanged("IsTopicsFiltersSelected");
                }
            }
        }

        /// <summary>
        ///     stores the cursor position of the previous tab.
        /// </summary>
        private void StorePrevPath()
        {
            int iIndex;
            switch (fSelectedData)
            {
                case SelectedDataSource.Items:
                    iIndex = 0;
                    break;
                case SelectedDataSource.Questions:
                    iIndex = 1;
                    break;
                default:
                    iIndex = 2;
                    break;
            }

            fPrevPath[iIndex] = GetFocused();
        }

        #endregion

        #region IsTopicsFiltersSelected

        /// <summary>
        ///     Gets/sets the value that indicates if the list is currently visible on
        ///     the ui or not.
        /// </summary>
        public bool IsTopicsFiltersSelected
        {
            get
            {
                return fSelectedData == SelectedDataSource.Filters;
            }

            set
            {
                if (IsTopicsFiltersSelected != value && value == true)
                {
                    StorePrevPath();
                    fSelectedData = SelectedDataSource.Filters;
                    if (fPrevPath[2] != null)
                    {
                        fPrevPath[2].SelectPathResult();

                            // this allows us to move focus back to the correct position, as it was when we left the tab.
                    }
                    else
                    {
                        TextPatternEditorResources.NeedsFocus = true;
                        OnPropertyChanged("FocusNewTopicFilter");
                    }

                    OnPropertyChanged("IsItemsSelected");
                    OnPropertyChanged("IsQuestionsSelected");
                    OnPropertyChanged("EditableItems");
                    OnPropertyChanged("IsTopicsFiltersSelected");
                }
            }
        }

        #endregion

        #region IsQuestionsSelected

        /// <summary>
        ///     Gets/sets wether the <see cref="Items" /> list is selected or the
        ///     <see cref="Questions" /> list.
        /// </summary>
        public bool IsQuestionsSelected
        {
            get
            {
                return fSelectedData == SelectedDataSource.Questions;
            }

            set
            {
                if (IsQuestionsSelected != value && value == true)
                {
                    StorePrevPath();
                    fSelectedData = SelectedDataSource.Questions;
                    if (fPrevPath[1] != null)
                    {
                        fPrevPath[1].SelectPathResult();

                            // this allows us to move focus back to the correct position, as it was when we left the tab.
                    }
                    else
                    {
                        TextPatternEditorResources.NeedsFocus = true;
                        OnPropertyChanged("FocusNewQuestion");
                    }

                    OnPropertyChanged("IsItemsSelected");
                    OnPropertyChanged("IsQuestionsSelected");
                    OnPropertyChanged("EditableItems");
                    OnPropertyChanged("IsTopicsFiltersSelected");
                }
            }
        }

        /// <summary>The get focused.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        private DisplayPath GetFocused()
        {
            FrameworkElement iFocused = Keyboard.FocusedElement as FrameworkElement;
            if (iFocused != null)
            {
                IDisplayPathBuilder iPathBuilder = iFocused.DataContext as IDisplayPathBuilder;
                if (iPathBuilder != null)
                {
                    return iPathBuilder.GetDisplayPathFromThis();
                }
            }

            return null;
        }

        #endregion

        #region IsParsed

        /// <summary>
        ///     Gets/sets the value that indicates if all the input patterns are
        ///     parsed or not.
        /// </summary>
        public bool IsParsed
        {
            get
            {
                return fIsParsed;
            }

            set
            {
                if (value != fIsParsed)
                {
                    List<string> ierrors = new List<string>();
                    SetIsParsed(value, ierrors);
                }
            }
        }

        /// <summary>The set is parsed.</summary>
        /// <param name="value">The value.</param>
        /// <param name="errors">The errors.</param>
        internal void SetIsParsed(bool value, List<string> errors)
        {
            fIsParsed = value;
            bool iIsOpen = IsOpen;
            IsOpen = true;
            try
            {
                if (value == true)
                {
                    foreach (PatternRule i in Items)
                    {
                        i.Rebuild(errors);
                    }
                }
                else
                {
                    foreach (PatternRule i in Items)
                    {
                        i.ReleaseAll();
                    }
                }
            }
            finally
            {
                IsOpen = iIsOpen;
            }

            OnPropertyChanged("IsParsed");
        }

        #endregion

        #region IsListView

        /// <summary>
        ///     Gets/sets the value that indicates if the main view is currently
        ///     displaying everyithing in a single list or as a master detail view.
        /// </summary>
        public bool IsListView
        {
            get
            {
                return fIsListView;
            }

            set
            {
                if (value != fIsListView)
                {
                    fIsListView = value;
                    if (value == true)
                    {
                        LoadRules();
                    }
                    else
                    {
                        PrepareMasterDetailView();
                    }

                    OnPropertyChanged("IsListView");
                    OnPropertyChanged("IsMasterDetailView");
                    OnPropertyChanged("MasterDetailWidth");
                    OnPropertyChanged("EditableItems");
                }
            }
        }

        /// <summary>
        ///     unloads all the data for the rules except for the currently selected
        ///     one.
        /// </summary>
        private void PrepareMasterDetailView()
        {
            if (Items != null)
            {
                int iCount = 0;
                foreach (PatternRule i in Items)
                {
                    if (iCount != SelectedRuleIndex)
                    {
                        i.UnloadUIData(); // this will also set the item to inactive
                    }
                    else if (i.TextPatterns == null)
                    {
                        // if the rule is not loaded, load it now.
                        i.LoadUI();
                        i.IsActive = true; // item needs to monitor changes
                    }

                    iCount++;
                }

                ListRequiresVertualization = false;
            }
        }

        /// <summary>
        ///     makes certain that all the rules are loaded.
        /// </summary>
        private void LoadRules()
        {
            if (Items != null)
            {
                foreach (PatternRule i in Items)
                {
                    i.LoadUI();
                    i.IsActive = true; // item needs to monitor changes
                }

                ListRequiresVertualization = Items.Count > PatternDefCollection.MAXBEFOREVIRTUALIZATION;
            }
        }

        #endregion

        #region IsMasterDetailView

        /// <summary>
        ///     Gets/sets the value that indicates if the main view is currently
        ///     displaying everyithing in a single list or as a master detail view.
        /// </summary>
        public bool IsMasterDetailView
        {
            get
            {
                return !fIsListView;
            }

            set
            {
                if (value != !fIsListView)
                {
                    fIsListView = !value;
                    if (value == false)
                    {
                        LoadRules();
                    }
                    else
                    {
                        PrepareMasterDetailView();
                    }

                    OnPropertyChanged("IsMasterDetailView");
                    OnPropertyChanged("IsListView");
                    OnPropertyChanged("MasterDetailWidth");
                    OnPropertyChanged("EditableItems");
                }
            }
        }

        #endregion

        #region MasterDetailWidth

        /// <summary>
        ///     Gets/sets the width of the master part in the master detail view. When
        ///     not in this mode, 'Auto' is returned.
        /// </summary>
        public GridLength MasterDetailWidth
        {
            get
            {
                if (IsListView == true)
                {
                    return new GridLength(0, GridUnitType.Auto);
                }
                else
                {
                    return fMasterDetailWidth;
                }
            }

            set
            {
                if (value != fMasterDetailWidth && IsListView == false)
                {
                    fMasterDetailWidth = value;
                    OnPropertyChanged("MasterDetailWidth");
                }
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            Items = new PatternDefCollection(this, (NeuronCluster)Item);
            if (Items.Count > 0 && SelectedRuleIndex == -1)
            {
                // make certain that there is an item selected when in master-detail view (init to -1 for new items)
                SelectedRuleIndex = 0;
            }

            LoadQuestions();
            LoadTopicFilters();
            if (IsMasterDetailView == true)
            {
                if (Items != null && SelectedRuleIndex >= 0 && SelectedRuleIndex < Items.Count)
                {
                    Items[SelectedRuleIndex].LoadUI();

                        // also need to make certain that the currently visible item is correctly loaded.
                    Items[SelectedRuleIndex].IsActive = true; // item needs to monitor changes
                }
            }
        }

        /// <summary>
        ///     Loads the questions section.
        /// </summary>
        protected virtual void LoadTopicFilters()
        {
            NeuronCluster iFound = Item.FindFirstOut((ulong)PredefinedNeurons.TopicFilter) as NeuronCluster;
            if (iFound != null)
            {
                TopicFilters = new TopicFilterCollection(this, iFound);
            }
            else
            {
                TopicFilters = new TopicFilterCollection(this, (ulong)PredefinedNeurons.TopicFilter);
            }
        }

        /// <summary>
        ///     Loads the questions section.
        /// </summary>
        protected virtual void LoadQuestions()
        {
            NeuronCluster iFound = Item.FindFirstOut((ulong)PredefinedNeurons.Questions) as NeuronCluster;
            if (iFound != null)
            {
                Questions = new ConditionalOutputsCollection(this, iFound);
            }
            else
            {
                Questions = new ConditionalOutputsCollection(this, (ulong)PredefinedNeurons.Questions);
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        protected override void UnloadUIData()
        {
            TextPatternEditorResources.FocusOn.Item = null; // for the mem leak, needs to go
            foreach (PatternRule i in Items)
            {
                i.UnloadUIData();
            }

            if (Questions != null)
            {
                foreach (PatternRuleOutput i in Questions)
                {
                    i.UnloadUIData();
                }

                Questions = null;
            }

            if (TopicFilters != null)
            {
                foreach (TopicFilterPattern i in TopicFilters)
                {
                    i.UnloadUIData();
                }

                TopicFilters = null;
            }

            Items = null;
            if (LastSearchQuery != null)
            {
                LastSearchQuery.Cancel();
                LastSearchQuery = null;
            }
        }

        /// <summary>
        ///     Registers the item that was read from xml. Topics must be registed
        ///     with the parser (when not in viewer mode).
        /// </summary>
        public override void Register()
        {
            base.Register();
            if (ProjectManager.Default.IsNotViewer == true)
            {
                if (TopicsDictionary.Add(Name, Item) == false)
                {
                    Log.LogError("Topic name", string.Format("Duplicate topic names detected: {0}", Name));
                    HasDuplicateName = true;
                }
            }
        }

        /// <summary>
        ///     sets everything up for the neuron info. need to load the 'IsLocal'
        ///     value.
        /// </summary>
        protected override void RegisterItem()
        {
            base.RegisterItem();
            if (Link.Exists(Item, Item, (ulong)PredefinedNeurons.Local) == true)
            {
                fIsLocal = true;
            }
        }

        /// <summary>
        ///     Registers but not the topic name (for objectTextPatterns).
        /// </summary>
        protected void RegisterNoTopicName()
        {
            base.Register();
        }

        /// <summary>Changes the name.</summary>
        /// <param name="value">The value.</param>
        protected override void ChangeName(string value)
        {
            if (string.IsNullOrEmpty(base.Name) == false && Item != null && ProjectManager.Default.IsNotViewer == true)
            {
                if (BrainHelper.HasIncommingReferences(Item) == true || Item.MeaningUsageCount > 0)
                {
                    // only check for this when there is already an item and a name, cause otherwise we are loading from disk. We only check for incomming refs, cause there is always an outgoing ref to the name.
                    MessageBoxResult iMBres =
                        MessageBox.Show(
                            "This editor is referenced by other patterns, changing it's name will break this reference. Do you want to continue or not (you can change the reference in the patterns after you have changed the name)?", 
                            "Rename", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Warning, 
                            MessageBoxResult.Yes);
                    if (iMBres == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                TopicsDictionary.Remove(base.Name, this.Item);
            }

            HasDuplicateName = false;
            base.ChangeName(value);
            if (string.IsNullOrEmpty(value) == false && Item != null && ProjectManager.Default.IsNotViewer == true)
            {
                // during xml loading, Item is still null, so this only gets called when the actual name is changed.
                if (TopicsDictionary.Add(value, Item) == false)
                {
                    Log.LogError("Topic name", string.Format("Duplicate topic names detected: {0}", value));
                    MessageBox.Show(
                        "Duplicate topic name: " + value, 
                        "Duplicate names", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    HasDuplicateName = true;
                }
            }
        }

        /// <summary>so the objectPatternEditor can auto generate a name without
        ///     registering it.</summary>
        /// <param name="value">The value.</param>
        protected void ChangeNameNoRegister(string value)
        {
            base.ChangeName(value);
        }

        #region event monitors

        /// <summary>Called when an outgoing <paramref name="link"/> got removed.
        ///     Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        protected internal override void OutgoingLinkChanged(Link link, ulong oldVal)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.Questions)
            {
                if (link.To != Questions.Cluster)
                {
                    // only try to change the object if it is a different cluster, otherwise we can simply keep it.
                    Questions = new ConditionalOutputsCollection(this, (NeuronCluster)link.To);
                }
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.TopicFilter)
            {
                if (link.To != TopicFilters.Cluster)
                {
                    TopicFilters = new TopicFilterCollection(this, (NeuronCluster)link.To);
                }
            }
        }

        /// <summary>Called when an outgoing <paramref name="link"/> got created.
        ///     Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.Questions)
            {
                if (Questions != null && link.To != Questions.Cluster)
                {
                    // only try to change the object if it is a different cluster and already loaded (ui is loaded), otherwise we can simply keep it.
                    Questions = new ConditionalOutputsCollection(this, (NeuronCluster)link.To);
                }
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.TopicFilter)
            {
                if (TopicFilters != null && link.To != TopicFilters.Cluster)
                {
                    // only try to change the object if it is a different cluster and already loaded (uiLoaded), otherwise we can simply keep it.
                    TopicFilters = new TopicFilterCollection(this, (NeuronCluster)link.To);
                }
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.Local)
            {
                SetIsLocal(true);
            }
        }

        /// <summary>Called when an outgoing <paramref name="link"/> got removed.
        ///     Inheriters can use this to respond to changes.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.Questions)
            {
                Questions = new ConditionalOutputsCollection(this, (ulong)PredefinedNeurons.Questions);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.TopicFilter)
            {
                TopicFilters = new TopicFilterCollection(this, (ulong)PredefinedNeurons.TopicFilter);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.Local)
            {
                SetIsLocal(false);
            }
        }

        #endregion

        #region clipboard

        /// <summary>Copies to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(DataObject data)
        {
            List<ulong> iValues = (from i in fSelectedItems select i.Item.ID).ToList();

            if (iValues.Count == 1)
            {
                data.SetData(Properties.Resources.NeuronIDFormat, iValues[0]);
            }
            else
            {
                data.SetData(Properties.Resources.MultiNeuronIDFormat, iValues);
            }

            string iPatterns = EditorsHelper.CopyPatternsToXml(fSelectedItems, this);

            data.SetData(fSelectedItems.GetClipboardID(), iPatterns);

            // data.SetText(iPatterns, TextDataFormat.Text);
        }

        /// <summary>The cut to clipboard.</summary>
        public override void CutToClipboard()
        {
            DataObject iData = EditorsHelper.GetDataObject();
            CopyToClipboard(iData);
            iData.SetData(Properties.Resources.CUTOPERATION, true);

                // we also store the fact that it is a cut operation. This way, a paste knows it doesn't have to perform a duplicate.
            Clipboard.SetDataObject(iData, false);
            Delete();
        }

        /// <summary>
        ///     Determines whether this instance can copy the selected data to the
        ///     clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can copy to the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanCopyToClipboard()
        {
            return fSelectedItems.Count > 0;
        }

        /// <summary>
        ///     Determines whether this instance can paste special from the clipboard.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste special from the clipboard;
        ///     otherwise, <c>false</c> .
        /// </returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return false;
        }

        /// <summary>
        ///     Pastes the data in a special way from the clipboard.
        /// </summary>
        public override void PasteSpecialFromClipboard()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Determines whether this instance can paste from the clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste from the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanPasteFromClipboard()
        {
            // note: when a dropDownSelector is focused, don't allow paste cause it handles this itself.
            return ((Keyboard.FocusedElement is DropDownNSSelector) == false)
                   && (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT)
                       || Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT)
                       || Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT)
                       || Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT)
                       || Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT)
                       || Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT));
        }

        /// <summary>
        ///     Pastes the data from the clipboard.
        /// </summary>
        public override void PasteFromClipboard()
        {
            PatternEditorItem iSelected = SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteRulesFromClipboard(this, iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PastePatternInputsFromClipboard(this, iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    if (iSelected is ResponseForOutput)
                    {
                        EditorsHelper.PasteOutputsToResonponseFor(
                            ((ResponseForOutput)iSelected).Owner as ResponsesForGroup, 
                            (ResponseForOutput)iSelected);
                    }
                    else
                    {
                        iRes = EditorsHelper.PastePatternOutputsFromClipboard(this, iSelected);
                    }
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PastePatternConditionsFromClipboard(this, iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteInvalidPatternsFromClipboard(this, iSelected);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            if (iRes != null)
            {
                foreach (PatternEditorItem i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>Pastes from clipboard to the specified list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert At.</param>
        public void PasteFromClipboardToList(PatternOutputsCollection list, PatternEditorItem insertAt = null)
        {
            PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PastePatternOutputsFromClipboard(list, iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
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
                foreach (PatternEditorItem i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>The paste from clipboard to list as cond.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        internal void PasteFromClipboardToListAsCond(
            ConditionalOutputsCollection list, 
            PatternEditorItem insertAt = null)
        {
            PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PastePatternConditionsFromClipboard(list, iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
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
                foreach (PatternEditorItem i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>Pastes from clipboard to the specified list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert At.</param>
        public void PasteFromClipboardToList(InputPatternCollection list, InputPattern insertAt = null)
        {
            PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PastePatternInputsFromClipboard(iSelected);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
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
                foreach (PatternEditorItem i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        /// <summary>Pastes from clipboard to the specified list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert At.</param>
        public void PasteFromClipboardToList(TopicFilterCollection list, TopicFilterPattern insertAt = null)
        {
            PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
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
                foreach (PatternEditorItem i in iRes)
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
            PatternEditorItem iSelected = insertAt != null ? insertAt : SelectedItem;
            List<PatternEditorItem> iRes = null;
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

                if (Clipboard.ContainsData(Properties.Resources.OUTPUTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.OUTPUTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNDEFFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNDEFFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.TEXTPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.TEXTPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.CONDITIONPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.CONDITIONPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.DOPATTERNFORMAT) == true)
                {
                    iRes = EditorsHelper.PasteExpressionsFromClipboard(
                        list, 
                        iSelected, 
                        Properties.Resources.DOPATTERNFORMAT);
                }
                else if (Clipboard.ContainsData(Properties.Resources.INVALIDPATTERNFORMAT) == true)
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
                foreach (PatternEditorItem i in iRes)
                {
                    SelectedItems.Add(i);
                }
            }
        }

        #endregion

        #region delete

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            List<PatternEditorItem> iSelected = new List<PatternEditorItem>(fSelectedItems);

                // we make a copy cause we are going to change the list.
            foreach (PatternEditorItem i in iSelected)
            {
                i.RemoveFromOwner();
            }
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                foreach (PatternEditorItem i in fSelectedItems.ToArray())
                {
                    i.Delete();
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return SelectedItems.Count > 0;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanDeleteSpecial()
        {
            return false;
        }

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        public override void DeleteEditor()
        {
            EditorsHelper.DeleteTextPatternEditor(this);
        }

        /// <summary>Returns <see langword="false"/> if the editor can't be deleted for
        ///     some reason + the <paramref name="error"/> message why it can't be
        ///     deleted. When there is a <see langword="ref"/> to this editor or one
        ///     of it's rules, can't delete.</summary>
        /// <param name="error">The error.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool EditorCanBeDeleted(out string error)
        {
            bool iPrevIsOpen = IsOpen;
            IsOpen = true;
            try
            {
                // note: a topic or rule's meaning count is set when it it used as sub item (it's the linkmeaning), but other links or children shouldn't be important during the delete.

                // List<ulong> iToExclude;
                // using (IDListAccessor iChildren = Items.Cluster.Children)
                // iToExclude = new List<ulong>(iChildren);
                // iToExclude.Add(ItemID);
                // Neuron iQuestions = Item.FindFirstOut((ulong)PredefinedNeurons.Questions);
                // if (iQuestions != null)                                                                               //if the topic has a list of questions, it should still be deletable.
                // iToExclude.Add(iQuestions.ID);
                if (Item.MeaningUsageCount > 0)
                {
                    // BrainHelper.HasOtherReferences(Item, iToExclude) == true ||
                    error = string.Format("the topic '{0}' is still referenced by other patterns", Name);
                    return false;
                }
                else
                {
                    foreach (PatternRule i in Items)
                    {
                        if (Item.MeaningUsageCount > 0)
                        {
                            // BrainHelper.HasOtherReferences(Item, iToExclude) == true || 
                            error = string.Format(
                                "the rule '{0}.{1}' is still referenced by other patterns", 
                                Name, 
                                i.NeuronInfo.DisplayTitle);
                            return false;
                        }
                    }
                }
            }
            finally
            {
                IsOpen = iPrevIsOpen;
            }

            error = null;
            return true;
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            NeuronDeleter iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start((from i in Items select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        #endregion

        #region IDisplayPathBuilder Members

        /// <summary>Gets the display path that points to the current object. When a ui
        ///     element has this object as context and there is an edit going on, it's
        ///     most likely the bottom input or output pattern creator, so check which
        ///     one it is and build a displaypath for it, so we can reselect it.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public DisplayPath GetDisplayPathFromThis()
        {
            TextBox iText = Keyboard.FocusedElement as TextBox;
            if (iText != null)
            {
                DPITextPatternEditorRoot iRoot = new DPITextPatternEditorRoot();
                iRoot.Item = Item;
                DPITextTag iChild = new DPITextTag((string)iText.Tag);
                iRoot.Items.Add(iChild);
                return new DisplayPath(iRoot);
            }

            return null;
        }

        #endregion

        #region xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement<bool>(writer, "HasDuplicateName", HasDuplicateName);
            XmlStore.WriteElement<bool>(writer, "IsParsed", IsParsed);
            XmlStore.WriteElement<bool>(writer, "IsTopicsFiltersExpanded", IsTopicsFiltersSelected);
            XmlStore.WriteElement<double>(writer, "MasterDetailWidth", fMasterDetailWidth.Value);
            XmlStore.WriteElement<int>(writer, "SelectedRule", fSelectedRuleIndex);
        }

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (base.ReadXmlInternal(reader) == false)
            {
                if (reader.Name == "HasDuplicateName")
                {
                    HasDuplicateName = XmlStore.ReadElement<bool>(reader, "HasDuplicateName");
                    return true;
                }
                else if (reader.Name == "IsParsed")
                {
                    fIsParsed = XmlStore.ReadElement<bool>(reader, "IsParsed");
                    return true;
                }
                else if (reader.Name == "IsTopicsFiltersExpanded")
                {
                    IsTopicsFiltersSelected = XmlStore.ReadElement<bool>(reader, "IsTopicsFiltersExpanded");
                    return true;
                }
                else if (reader.Name == "MasterDetailWidth")
                {
                    double iVal = XmlStore.ReadElement<double>(reader, "MasterDetailWidth");
                    fMasterDetailWidth = new GridLength(iVal, GridUnitType.Pixel);
                    return true;
                }
                else if (reader.Name == "SelectedRule")
                {
                    fSelectedRuleIndex = XmlStore.ReadElement<int>(reader, "SelectedRule");
                    return true;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        /// <summary><para>Raises the propertyChanged event for the specified property. This is
        ///         used by the <see cref="DPITextPatternEditorRoot"/></para>
        /// <para>to execute the selection of a <see cref="DPITextTag"/> item. It's
        ///         used to set focus to either the input or output textbox for creating
        ///         new rules.</para>
        /// </summary>
        /// <param name="value">The value.</param>
        internal void PutFocusOn(string value)
        {
            TextPatternEditorResources.NeedsFocus = true;
            TextPatternEditorResources.FocusOn.PropName = value;
            TextPatternEditorResources.FocusOn.Item = this;
            OnPropertyChanged(value);
        }

        /// <summary>Rebuilds all the patterns in this topic.</summary>
        /// <param name="errors">The errors.</param>
        internal void Rebuild(List<string> errors)
        {
            bool iWasOpen = IsOpen;
            IsOpen = true;
            try
            {
                foreach (PatternRule i in Items)
                {
                    i.Rebuild(errors);
                    if (TopicsDictionary.CheckUniqueRuleName(i.NeuronInfo.DisplayTitle, Item, i.Item) == false)
                    {
                        errors.Add(
                            string.Format(
                                "'{0}' is already used as the name of a rule in the context of the '{1}' topic. Names should be unique. Do you wan to continue and assign this duplicate name?", 
                                i.NeuronInfo.DisplayTitle, 
                                Name));
                    }
                }

                foreach (TopicFilterPattern i in TopicFilters)
                {
                    i.ForceParse();
                    if (i.HasError == true)
                    {
                        errors.Add(i.ParseError);
                    }
                }

                foreach (PatternRuleOutput i in Questions)
                {
                    i.Rebuild(errors);
                }
            }
            finally
            {
                IsOpen = iWasOpen;
            }
        }

        #endregion
    }
}