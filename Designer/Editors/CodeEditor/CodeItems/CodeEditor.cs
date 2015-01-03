// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeEditor.cs" company="">
//   
// </copyright>
// <summary>
//   Encapsulates the 2 NeuronLists from a <see cref="Neuron" /> containing
//   executable items for editing in a <see cref="CodeEditor" /> . If the item
//   being encapsulated is a <see cref="NeuronCluster" /> , the children of
//   the cluster are also depicted in a code view, if possible.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Encapsulates the 2 NeuronLists from a <see cref="Neuron" /> containing
    ///     executable items for editing in a <see cref="CodeEditor" /> . If the item
    ///     being encapsulated is a <see cref="NeuronCluster" /> , the children of
    ///     the cluster are also depicted in a code view, if possible.
    /// </summary>
    public class CodeEditor : NeuronEditor
    {
        /// <summary>The f fixed nr entry points.</summary>
        private int fFixedNrEntryPoints;

        /// <summary>The f selected index.</summary>
        private int fSelectedIndex;

        /// <summary>Gets the document info.</summary>
        public override string DocumentInfo
        {
            get
            {
                return "Code editor: " + Name + "(Neuron: " + Item.ID + ")";
            }
        }

        /// <summary>Gets the document type.</summary>
        public override string DocumentType
        {
            get
            {
                return "Code editor";
            }
        }

        #region IWeakEventListener Members

        /// <summary>The item_ neuron changed.</summary>
        /// <param name="e">The e.</param>
        protected internal override void Item_NeuronChanged(NeuronChangedEventArgs e)
        {
            base.Item_NeuronChanged(e);
            if (e.Action == BrainAction.Removed)
            {
                BrainData.Current.CodeEditors.Remove(this);
            }
        }

        #endregion

        /// <summary>
        ///     Refreshes all <see langword="break" /> points of every code item. This
        ///     is used when the list of breakpoints is cleared.
        /// </summary>
        public void ResetAllBreakPoints()
        {
            foreach (var iPage in EntryPoints)
            {
                iPage.ResetAllBreakPoints();
            }
        }

        #region ctor -dtor

        /// <summary>Initializes a new instance of the <see cref="CodeEditor"/> class. Constructor with the item to wrap.</summary>
        /// <param name="toWrap"></param>
        public CodeEditor(Neuron toWrap)
            : base(toWrap)
        {
            EntryPoints = new Data.ObservedCollection<CodeEditorPage>(this);
        }

        /// <summary>Initializes a new instance of the <see cref="CodeEditor"/> class. 
        ///     Default empty constructor, for streaming.</summary>
        public CodeEditor()
        {
            EntryPoints = new Data.ObservedCollection<CodeEditorPage>(this);
        }

        #endregion

        #region Prop

        #region Icon

        /// <summary>Gets the icon.</summary>
        public override string Icon
        {
            get
            {
                return "/Images/ViewCode_Enabled.png";
            }
        }

        #endregion

        #region EntryPoints

        /// <summary>
        ///     Gets all the entry points defined by the neuron that this object
        ///     wraps.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Data.ObservedCollection<CodeEditorPage> EntryPoints { get; private set; }

        #endregion

        #region FixedNrEntryPoints

        /// <summary>
        ///     Gets the number of entry points that can't be deleted, which are the
        ///     first in the list.
        /// </summary>
        /// <remarks>
        ///     THis is used while deleting pages to check if it is allowed.
        /// </remarks>
        public int FixedNrEntryPoints
        {
            get
            {
                return fFixedNrEntryPoints;
            }

            internal set
            {
                fFixedNrEntryPoints = value;
                OnPropertyChanged("FixedNrEntryPoints");
            }
        }

        #endregion

        #region PossiblePages

        /// <summary>
        ///     Gets a list of neuron data objects represening neurons that can still
        ///     be used as meaning for a link from the currently displayed item and a
        ///     new code cluster.
        /// </summary>
        /// <value>
        ///     The possible pages.
        /// </value>
        public System.Collections.Generic.List<NeuronData> PossiblePages
        {
            get
            {
                if (BrainData.Current != null && Brain.Current.Storage != null)
                {
                    var iShowing = (from i in EntryPoints
                                    where i.Items != null && Brain.Current.IsValidID(i.Items.MeaningID)
                                    select Brain.Current[i.Items.MeaningID]).ToList();
                    var iFound = (from i in BrainData.Current.DefaultMeanings
                                  where iShowing.Contains(i) == false
                                  select BrainData.Current.NeuronInfo[i.ID]).ToList();
                    return iFound;
                }

                return null;
            }
        }

        #endregion

        #region SelectedIndex

        /// <summary>
        ///     Gets/sets the index of the page that is currently selected.
        /// </summary>
        /// <remarks>
        ///     We store this so that this is remembered when the user switches
        ///     screen.
        /// </remarks>
        public int SelectedIndex
        {
            get
            {
                return fSelectedIndex;
            }

            set
            {
                if (value != fSelectedIndex && value != -1)
                {
                    // we don't want to loose the index value, so we skip -1, if we didn't do that, this prop would have no result.
                    fSelectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        #endregion

        #region SelectedListType

        /// <summary>
        ///     Gets or sets the type of the selected list. This allows you to change the
        ///     currently selected tab, based on debug info.
        /// </summary>
        /// <value>
        ///     The type of the selected list.
        /// </value>
        public ExecListType SelectedListType
        {
            get
            {
                if (EntryPoints[SelectedIndex].Items.MeaningID == (ulong)PredefinedNeurons.Rules)
                {
                    return ExecListType.Rules;
                }

                if (EntryPoints[SelectedIndex].Items.MeaningID == (ulong)PredefinedNeurons.Actions)
                {
                    return ExecListType.Actions;
                }

                return ExecListType.Children;
            }

            set
            {
                ulong iToSearch;
                switch (value)
                {
                    case ExecListType.Rules:
                        iToSearch = (ulong)PredefinedNeurons.Rules;
                        break;
                    case ExecListType.Actions:
                        iToSearch = (ulong)PredefinedNeurons.Actions;
                        break;
                    case ExecListType.Children:
                        iToSearch = (ulong)PredefinedNeurons.Statements;
                        break;
                    default:
                        iToSearch = Neuron.EmptyId;
                        break;
                }

                if (iToSearch == Neuron.EmptyId)
                {
                    for (var i = 0; i < EntryPoints.Count; i++)
                    {
                        if (EntryPoints[i].Items.MeaningID == iToSearch)
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>
        ///     Clears all the entry points.
        /// </summary>
        protected override void UnloadUIData()
        {
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // while clearing the entrypoints, we don't want to generate undo data.
            try
            {
                foreach (var i in EntryPoints)
                {
                    i.UnloadUIData();
                }

                EntryPoints.Clear();
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>
        ///     Loads the CodeEditorPages for the current item.
        /// </summary>
        protected override void LoadUIData()
        {
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // while building the entrypoints, we don't want to generate undo data.
            try
            {
                UnloadUIData();
                FixedNrEntryPoints = 0;
                if (Item != null)
                {
                    NeuronCluster iCluster;

                    if (Item is NeuronCluster)
                    {
                        FixedNrEntryPoints++;
                        EntryPoints.Add(
                            new CodeEditorPage("Children (this list)", Item, Neuron.EmptyId, (NeuronCluster)Item));
                    }
                    else if (Item is ExpressionsBlock)
                    {
                        FixedNrEntryPoints++;
                        iCluster = ((ExpressionsBlock)Item).StatementsCluster;
                        if (iCluster != null)
                        {
                            EntryPoints.Add(
                                new CodeEditorPage("Statements", Item, (ulong)PredefinedNeurons.Statements, iCluster));
                        }
                        else
                        {
                            EntryPoints.Add(new CodeEditorPage("Statements", Item, (ulong)PredefinedNeurons.Statements));
                        }
                    }
                    else if (Item is TimerNeuron)
                    {
                        FixedNrEntryPoints++;
                        iCluster = ((TimerNeuron)Item).StatementsCluster;
                        if (iCluster != null)
                        {
                            EntryPoints.Add(
                                new CodeEditorPage("Statements", Item, (ulong)PredefinedNeurons.Statements, iCluster));
                        }
                        else
                        {
                            EntryPoints.Add(new CodeEditorPage("Statements", Item, (ulong)PredefinedNeurons.Statements));
                        }
                    }

                    AddSinCode();
                    AddRulesPage();
                    AddActionsPage();
                    AddCodePages();
                }
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>
        ///     Adds all the code clusters that have not yet been loaded by the
        ///     predefined items.
        /// </summary>
        private void AddCodePages()
        {
            if (Item.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                using (var iList = Item.LinksOut) iLinks.AddRange(iLinks);
                try
                {
                    foreach (var i in iLinks)
                    {
                        var iTo = i.To as NeuronCluster;
                        if (iTo != null && iTo.Meaning == (ulong)PredefinedNeurons.Code)
                        {
                            var iFound =
                                (from iPage in EntryPoints where iPage.Items.Cluster == iTo select iPage).FirstOrDefault
                                    ();
                            if (iFound == null)
                            {
                                EntryPoints.Add(
                                    new CodeEditorPage(
                                        BrainData.Current.NeuronInfo[i.MeaningID].DisplayTitle, 
                                        Item, 
                                        i.MeaningID, 
                                        iTo));
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }
        }

        /// <summary>
        ///     Checks if the wrapeed neuron is a sin, if so, adds a page for input
        ///     actions.
        /// </summary>
        private void AddSinCode()
        {
            if (Item is Sin)
            {
                FixedNrEntryPoints++;
                var iCluster = ((Sin)Item).ActionsForInput;
                if (iCluster != null)
                {
                    EntryPoints.Add(
                        new CodeEditorPage("Input actions", Item, (ulong)PredefinedNeurons.ActionsForInput, iCluster));
                }
                else
                {
                    EntryPoints.Add(new CodeEditorPage("Input actions", Item, (ulong)PredefinedNeurons.ActionsForInput));
                }
            }
        }

        /// <summary>
        ///     Adds the rules page.
        /// </summary>
        private void AddRulesPage()
        {
            FixedNrEntryPoints++;
            var iCluster = Item.RulesCluster;
            if (iCluster == null)
            {
                EntryPoints.Add(new CodeEditorPage("Rules", Item, (ulong)PredefinedNeurons.Rules));
            }
            else
            {
                EntryPoints.Add(new CodeEditorPage("Rules", Item, (ulong)PredefinedNeurons.Rules, iCluster));
            }
        }

        /// <summary>
        ///     Adds the actions page.
        /// </summary>
        private void AddActionsPage()
        {
            FixedNrEntryPoints++;
            var iCluster = Item.ActionsCluster;
            if (iCluster == null)
            {
                EntryPoints.Add(new CodeEditorPage("Actions", Item, (ulong)PredefinedNeurons.Actions));
            }
            else
            {
                EntryPoints.Add(new CodeEditorPage("Actions", Item, (ulong)PredefinedNeurons.Actions, iCluster));
            }
        }

        ///// <summary>
        ///// Asks each page on this code editor to remove all their registered variables (toolbox items).
        ///// </summary>
        // public void ParkRegisteredVariables()
        // {
        // foreach (CodeEditorPage i in EntryPoints)
        // i.ParkVariables();
        // }

        // public void UnParkRegisteredVariables()
        // {
        // foreach (CodeEditorPage i in EntryPoints)
        // i.UnParkVariables();
        // }
        #endregion

        #region Clipboard

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            var iValues = (from i in EntryPoints[SelectedIndex].SelectedItems select i.Item.ID).ToList();

            data.SetData(Properties.Resources.MultiNeuronIDFormat, iValues, false);
        }

        /// <summary>The can copy to clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanCopyToClipboard()
        {
            if (SelectedIndex > -1 && EntryPoints.Count > SelectedIndex)
            {
                // in case it got closed, we need to check for count > selectedIndex
                return EntryPoints[SelectedIndex].SelectedItems.Count > 0;
            }

            return false;
        }

        /// <summary>The can paste special from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return false;
        }

        /// <summary>The paste special from clipboard.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void PasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
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
            if (base.CanPasteFromClipboard())
            {
                if (SelectedIndex > -1)
                {
                    return System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat)
                           || System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat);
                }

                return false;
            }

            return false;
        }

        /// <summary>The paste from clipboard.</summary>
        public override void PasteFromClipboard()
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            object iTarget = null;
            if (iFocused != null)
            {
                iTarget = iFocused.Tag;
            }

            if (iTarget is System.Windows.Controls.ContentPresenter)
            {
                // we paste on a drop target for property values.
                DoPropetyPaste((System.Windows.Controls.ContentPresenter)iTarget);
            }
            else if (iTarget is System.Windows.Controls.ItemsControl)
            {
                // we paste on an argument list or other form of list.
                DoArgPaste(((System.Windows.Controls.ItemsControl)iTarget).ItemsSource as CodeItemCollection);
            }
            else
            {
                DoCodePaste(); // we dropped on something that requires on insert or add
            }
        }

        /// <summary>Does a paste on a property place holder. This paste will only handle
        ///     the 1st item on the clipboard.</summary>
        /// <param name="pasteOn">The paste on.</param>
        private void DoPropetyPaste(System.Windows.Controls.ContentPresenter pasteOn)
        {
            var iItems = GetDataAsListFromClipboard();
            if (iItems != null && iItems.Count > 0)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);
                try
                {
                    var iNeuron = Brain.Current[iItems[0]];
                    var iNew = EditorsHelper.CreateCodeItemFor(iNeuron);
                    pasteOn.Content = iNew;
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The do arg paste.</summary>
        /// <param name="col">The col.</param>
        private void DoArgPaste(CodeItemCollection col)
        {
            var iItems = GetDataAsListFromClipboard();
            if (iItems != null)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);
                try
                {
                    foreach (var i in iItems)
                    {
                        var iNeuron = Brain.Current[i];
                        var iNew = EditorsHelper.CreateCodeItemFor(iNeuron);
                        col.Add(iNew);
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The do code paste.</summary>
        private void DoCodePaste()
        {
            var iItems = GetDataAsListFromClipboard();
            if (iItems != null)
            {
                CodeItemCollection iCol = null;
                var iIndex = -1;
                var iPasteOn = EntryPoints[SelectedIndex].SelectedItem;
                if (iPasteOn != null)
                {
                    var iOwner = iPasteOn.Owner as ICodeItemsOwner;
                    while (iOwner != null)
                    {
                        iCol = iOwner.Items;
                        iIndex = iCol.IndexOf(iPasteOn);
                        if (iIndex != -1)
                        {
                            // if the owner has children, but also other items, than we have a problem, should not be possible, but just in case, to have a working algorithm, we go up 1 owner.
                            break;
                        }

                        if (iPasteOn != null)
                        {
                            iPasteOn = iPasteOn.Owner as CodeItem;
                            iOwner = iPasteOn.Owner as ICodeItemsOwner;
                        }
                        else
                        {
                            iOwner = null;
                        }
                    }
                }

                if (iIndex == -1)
                {
                    iCol = EntryPoints[SelectedIndex].Items;
                }

                WindowMain.UndoStore.BeginUndoGroup(false);
                try
                {
                    foreach (var i in iItems)
                    {
                        var iNeuron = Brain.Current[i];
                        var iNew = EditorsHelper.CreateCodeItemFor(iNeuron);
                        if (iIndex == -1)
                        {
                            iCol.Add(iNew);
                        }
                        else
                        {
                            iCol.Insert(iIndex, iNew);
                            iIndex++; // need to add the next item after the currently added one.
                        }
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The get data as list from clipboard.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<ulong> GetDataAsListFromClipboard()
        {
            System.Collections.Generic.List<ulong> iItems = null;
            if (System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
            {
                iItems =
                    System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat) as
                    System.Collections.Generic.List<ulong>;
            }
            else
            {
                iItems = new System.Collections.Generic.List<ulong>();
                iItems.Add((ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat));
            }

            return iItems;
        }

        #endregion

        #region Delete

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is deleted from the project using the special delete.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start(Item);
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            var iRes =
                System.Windows.MessageBox.Show(
                    "Remove the selected neurons from the designer and from the brain when no longer referenced?", 
                    "Delete items", 
                    System.Windows.MessageBoxButton.OKCancel, 
                    System.Windows.MessageBoxImage.Question, 
                    System.Windows.MessageBoxResult.Cancel);
            if (iRes == System.Windows.MessageBoxResult.OK)
            {
                PerformDelete(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
            }
        }

        /// <summary>The remove.</summary>
        public override void Remove()
        {
            var iPage = EntryPoints[SelectedIndex];
            System.Diagnostics.Debug.Assert(iPage != null);

            var iToRemove = iPage.SelectedItems.ToList(); // we make a copy just in case we don't brake any loop code.
            iToRemove.Reverse();

                // we reverse the list so that we remove the last code item first.  This allows for better consistency in editing.
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we group all the data together so a single undo command cand restore the previous state.
            try
            {
                foreach (var i in iToRemove)
                {
                    RemoveCodeItem(i);
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
            if (SelectedIndex < EntryPoints.Count)
            {
                var iPage = EntryPoints[SelectedIndex];
                System.Diagnostics.Debug.Assert(iPage != null);
                return iPage.SelectedItems.Count > 0;
            }

            return false;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            var iDlg = new DlgSelectDeletionMethod();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                PerformDelete(iDlg.NeuronDeletionMethod, iDlg.BranchHandling);
            }
        }

        /// <summary>The perform delete.</summary>
        /// <param name="neuronDeletionMethod">The neuron deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        private void PerformDelete(DeletionMethod neuronDeletionMethod, DeletionMethod branchHandling)
        {
            var iPage = EntryPoints[SelectedIndex];
            System.Diagnostics.Debug.Assert(iPage != null);

            WindowMain.UndoStore.BeginUndoGroup(false, true);

                // we group all the data together so a single undo command cand restore the previous state. All deletes must be undone in the reverse order, since it's links are deleted first.
            try
            {
                var iDeleter = new NeuronDeleter<CodeItem>();
                iDeleter.BranchHandling = branchHandling;
                iDeleter.NeuronDeletionMethod = neuronDeletionMethod;
                var iToRemove = new System.Collections.Generic.List<NeuronDeleter<CodeItem>.DeleteRequestRecord>();

                foreach (var i in iPage.SelectedItems)
                {
                    // we need to make a custom delete list, with the correct owner's list for each item that needs to be deleted.
                    var iRec = new NeuronDeleter<CodeItem>.DeleteRequestRecord();
                    iRec.Item = i;
                    if (i.Owner is ICodeItemsOwner)
                    {
                        iRec.Owner = ((ICodeItemsOwner)i.Owner).Items;
                        if (iRec.Owner != null)
                        {
                            // sometimes the item is reffed through  link instead of parent-child (boolExpression's & Assignment's, children have this). They need to be deleted differently.
                            if (iRec.Owner.Contains(i) == false)
                            {
                                iRec.Owner = null;
                                iRec.OtherSide = i.Owner as CodeItem;
                            }
                        }
                        else
                        {
                            iRec.OtherSide = i.Owner as CodeItem;
                        }
                    }
                    else
                    {
                        iRec.OtherSide = i.Owner as CodeItem;
                    }

                    iToRemove.Add(iRec);
                }

                iDeleter.Start(iToRemove);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The remove code item.</summary>
        /// <param name="toRemove">The to remove.</param>
        private void RemoveCodeItem(CodeItem toRemove)
        {
            var iPage = EntryPoints[SelectedIndex];
            System.Diagnostics.Debug.Assert(iPage != null);

            if (toRemove.Owner == iPage)
            {
                if (iPage.Items.Remove(toRemove) == false)
                {
                    System.Windows.MessageBox.Show("Failed to remove the item from the main code list!");
                }
            }
            else if (toRemove.Owner is CodeItem)
            {
                ((CodeItem)toRemove.Owner).RemoveChild(toRemove);
            }
            else if (toRemove.Owner is CodeItemConditionalStatement.CodeItemConditionalStatements)
            {
                ((ICodeItemsOwner)toRemove.Owner).Items.Remove(toRemove);
            }
            else
            {
                System.Windows.MessageBox.Show("Unkown owner type, can't Remove!");
            }
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
            return CanDelete();
        }

        #endregion
    }
}