// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flow.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a way to visually express interoperability between
//   <see cref="Frame" /> s over a period of time. Ex: conversation flow.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Provides a way to visually express interoperability between
    ///     <see cref="Frame" /> s over a period of time. Ex: conversation flow.
    /// </summary>
    public class Flow : NeuronEditor, IEditorSelection
    {
        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <remarks>This is used to determin which neurons need to be exported when an
        ///     editor is selected for export.</remarks>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            yield return Item;
        }

        #region fields

        /// <summary>The f items.</summary>
        private FlowItemCollection fItems;

        /// <summary>The f selected items.</summary>
        private readonly EditorItemSelectionList<FlowItem> fSelectedItems = new EditorItemSelectionList<FlowItem>();

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f popup does insert.</summary>
        private bool fPopupDoesInsert;

        /// <summary>The f popup is open.</summary>
        private bool fPopupIsOpen;

        /// <summary>The f is floating.</summary>
        private bool fIsFloating;

        /// <summary>The f keeps data.</summary>
        private bool fKeepsData;

        /// <summary>The f is nd floating.</summary>
        private bool fIsNDFloating;

        /// <summary>The f event monitor.</summary>
        private FlowEventMonitor fEventMonitor;

        /// <summary>The f zoom.</summary>
        private double fZoom = 1.0;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        /// <summary>The f is focused.</summary>
        private bool fIsFocused;

        #endregion

        #region Prop

        #region InternalChange

        /// <summary>
        ///     <para>
        ///         a <see langword="switch" /> that determins if the class is doing an
        ///         <see langword="internal" /> change or not. This is used by the
        ///         LinkChanged event manager to see if there needs to be an update in
        ///         response to a linkchange or not.
        ///     </para>
        ///     <para>
        ///         Gets the resource path to the icon that should be used for this
        ///         editor. This is usually class specific.
        ///     </para>
        /// </summary>
        /// <value>
        /// </value>
        public override string Icon
        {
            get
            {
                return "/Images/Flow/flow_Enabled.png";
            }
        }

        #endregion

        #region  IsSelected

        /// <summary>
        ///     Gets/sets if the flow is currently selected or not.
        /// </summary>
        /// <remarks>
        ///     Used to select the item visually, must be bound to from xaml. We
        ///     override, cause we don't want to update the editors list.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public override bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    SetSelected(value);
                    var iEditor = (FlowEditor)Owner;
                    if (iEditor != null)
                    {
                        iEditor.SetSelectedFlow(this, value);
                    }
                }
            }
        }

        /// <summary>The set selected.</summary>
        /// <param name="value">The value.</param>
        internal void SetSelected(bool value)
        {
            fIsSelected = value;
            if (value)
            {
                // only let ui update when item gets selected, otherwise the listbox looses the correct selected item on occation.
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region IsFocused

        /// <summary>
        ///     Gets/sets the wether this object is focused or not.
        /// </summary>
        /// <remarks>
        ///     this is to fix some shortcommings with wpf.
        /// </remarks>
        public bool IsFocused
        {
            get
            {
                return fIsFocused;
            }

            set
            {
                if (value != fIsFocused)
                {
                    fIsFocused = value;
                    OnPropertyChanged("IsFocused");
                }
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the <see cref="Neuron" /> that this object provides a wraper for.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public override Neuron Item
        {
            get
            {
                return base.Item;
            }

            internal set
            {
                if (fEventMonitor != null)
                {
                    EventManager.Current.UnregisterFlow(this);
                }

                base.Item = value;
                if (value != null && value.ID > Neuron.EmptyId)
                {
                    fEventMonitor = EventManager.Current.RegisterFlow(this);
                }
                else
                {
                    fEventMonitor = null;

                        // if assigning null, need to reset monitor, otherwise we get into trouble the next time that this prop is set.
                    Items = null;
                }
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of flow items defined in this flow.
        /// </summary>
        public FlowItemCollection Items
        {
            get
            {
                return fItems;
            }

            internal set
            {
                fItems = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        #region PopupDoesInsert

        /// <summary>
        ///     Gets/sets the wether the popup does an insert or an add. This prop is
        ///     provided so that the editor can easely link this data with the
        ///     visualizer for this object.
        /// </summary>
        public bool PopupDoesInsert
        {
            get
            {
                return fPopupDoesInsert;
            }

            set
            {
                fPopupDoesInsert = value;
                OnPropertyChanged("PopupDoesInsert");
            }
        }

        #endregion

        #region PopupIsOpen

        /// <summary>
        ///     Gets/sets wether the popup should be opened or closed. This prop is
        ///     provided so that the editor can easely link this data with the
        ///     visualizer for this object.
        /// </summary>
        public bool PopupIsOpen
        {
            get
            {
                return fPopupIsOpen;
            }

            set
            {
                fPopupIsOpen = value;
                OnPropertyChanged("PopupIsOpen");
            }
        }

        #endregion

        #region FloatingFlowKeepsData

        /// <summary>
        ///     Gets/sets wether a floating flow stores it's result or not. (by
        ///     default, it doesn't).
        /// </summary>
        public bool KeepsData
        {
            get
            {
                return fKeepsData;
            }

            set
            {
                if (value != fKeepsData)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(
                            Item, 
                            (ulong)PredefinedNeurons.FloatingFlowKeepsData, 
                            value);
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region KeepsDataAllowed

        /// <summary>
        ///     Gets the wether it is allowed to set the 'KeepsData' prop.
        /// </summary>
        public bool KeepsDataAllowed
        {
            get
            {
                return IsFloating || IsNDFloating;
            }
        }

        #endregion

        #region IsFloating

        /// <summary>
        ///     Gets/sets wether the flow is (destructive) floating or not
        /// </summary>
        public bool IsFloating
        {
            get
            {
                return fIsFloating;
            }

            set
            {
                if (value != fIsFloating)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        if (value)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.FlowType, 
                                (ulong)PredefinedNeurons.FlowIsFloating);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.FlowType, 
                                (EditorItem)null);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region IsNDFloating

        /// <summary>
        ///     Gets/sets wether the flow is non destructive floating or not. Non
        ///     destructive floats can also occur in the middle of conditional parts
        ///     or flows (in between 2 statics).
        /// </summary>
        public bool IsNDFloating
        {
            get
            {
                return fIsNDFloating;
            }

            set
            {
                if (value != fIsNDFloating)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        if (value)
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.FlowType, 
                                (ulong)PredefinedNeurons.FlowIsNonDestructiveFloating);
                        }
                        else
                        {
                            EditorsHelper.SetFirstOutgoingLinkTo(
                                Item, 
                                (ulong)PredefinedNeurons.FlowType, 
                                (EditorItem)null);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets a value indicating whether this instance is a normal flow
        ///     (not floating). Useful to bind against.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is normal; otherwise, <c>false</c> .
        /// </value>
        public bool IsNormal
        {
            get
            {
                return !(IsFloating || IsNDFloating);
            }

            set
            {
                IsFloating = false;
                IsNDFloating = false;
                KeepsData = false;
            }
        }

        #region SelectedItems Members

        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.IList SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        /// <summary>
        ///     Gets the currently selected item. If there are multiple selections,
        ///     the first is returned.
        /// </summary>
        /// <value>
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public FlowItem SelectedItem
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

        #region IEditorSelection Members

        /// <summary>
        ///     Gets/sets the currently selected item. If there are multiple
        ///     selections, the first is returned.
        /// </summary>
        /// <value>
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        object IEditorSelection.SelectedItem
        {
            get
            {
                return SelectedItem;
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
                var iVal = value / 100;
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

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll position
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                if (fHorScrollPos != value)
                {
                    fHorScrollPos = value;
                    OnPropertyChanged("HorScrollPos");
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
                return "Flow: " + Name + "(Neuron: " + Item.ID + ")";
            }
        }

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
                return "Flow";
            }
        }

        #endregion

        #region Functions

        /// <summary>The internal set is floating.</summary>
        /// <param name="value">The value.</param>
        internal void InternalSetIsFloating(bool value)
        {
            fIsFloating = value;
            OnPropertyChanged("IsFloating");
            OnPropertyChanged("KeepsDataAllowed");
            OnPropertyChanged("IsNormal");
        }

        /// <summary>The clear flow type.</summary>
        internal void ClearFlowType()
        {
            fIsFloating = false;
            fIsNDFloating = false;
            OnPropertyChanged("IsNDFloating");
            OnPropertyChanged("IsFloating");
            OnPropertyChanged("KeepsDataAllowed");
            OnPropertyChanged("IsNormal");
        }

        /// <summary>The internal set is nd floating.</summary>
        /// <param name="value">The value.</param>
        internal void InternalSetIsNDFloating(bool value)
        {
            fIsNDFloating = value;
            OnPropertyChanged("IsNDFloating");
            OnPropertyChanged("KeepsDataAllowed");
            OnPropertyChanged("IsNormal");
        }

        /// <summary>The internal set keeps data.</summary>
        /// <param name="value">The value.</param>
        internal void InternalSetKeepsData(bool value)
        {
            fKeepsData = value;
            OnPropertyChanged("KeepsData");
        }

        /// <summary>
        ///     Registers the item that was read from xml.
        /// </summary>
        /// <remarks>
        ///     This must be called when the editor is read from xml. In that
        ///     situation, the brainData isn't always loaded properly yet. At this
        ///     point, this can be resolved. It is called by the brainData.
        /// </remarks>
        public override void Register()
        {
            base.Register();

            if (Item.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                using (var iList = Item.LinksOut) iLinks.AddRange(iLinks); // make a local copy so we don't create a deadlock.
                try
                {
                    var iFound =
                        (from i in iLinks where i.MeaningID == (ulong)PredefinedNeurons.FlowType select i)
                            .FirstOrDefault();
                    if (iFound != null)
                    {
                        if (iFound.ToID == (ulong)PredefinedNeurons.FlowIsFloating)
                        {
                            InternalSetIsFloating(true);
                        }
                        else if (iFound.ToID == (ulong)PredefinedNeurons.FlowIsNonDestructiveFloating)
                        {
                            InternalSetIsNDFloating(true);
                        }
                    }

                    iFound =
                        (from i in iLinks where i.MeaningID == (ulong)PredefinedNeurons.FloatingFlowKeepsData select i)
                            .FirstOrDefault();
                    if (iFound != null && iFound.ToID == (ulong)PredefinedNeurons.True)
                    {
                        InternalSetKeepsData(true);
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        protected override void UnloadUIData()
        {
            foreach (var i in Items)
            {
                // this resets 'fNeuronInfo' for each flow item. not much, but a bit. This doesn't need to be reloaded.
                i.UnloadUIData();
            }

            Items = null;
        }

        /// <summary>The load ui data.</summary>
        protected override void LoadUIData()
        {
            LoadItems();
        }

        /// <summary>
        ///     Builds a <see cref="FlowItem" /> for each neuron in the wrapped
        ///     cluster.
        /// </summary>
        private void LoadItems()
        {
            Items = new FlowItemCollection(this, (NeuronCluster)Item);
        }

        #region Clipboard

        /// <summary>Copies to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            var iValues = (from i in fSelectedItems select i.Item.ID).ToList();

            if (iValues.Count == 1)
            {
                data.SetData(Properties.Resources.NeuronIDFormat, iValues[0]);
            }
            else
            {
                data.SetData(Properties.Resources.MultiNeuronIDFormat, iValues);
            }
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
                return System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat)
                       || System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat);
            }

            return false;
        }

        /// <summary>
        ///     Pastes the data from the clipboard.
        /// </summary>
        /// <remarks>
        ///     Statics are simply linked, conditionals and conditional parts are
        ///     duplicated. This is to ensure that the parsing algorithm works
        ///     properly: if you simply link, you can do for instance the following:
        ///     the first and last item in a list are links of the same object, only
        ///     the first item is scanned in, so it is an invalid scan cause the rest
        ///     is not found, but because the first is also linked as the last item,
        ///     it is seen as 'complete'. This is prevented if we use all original
        ///     items.
        /// </remarks>
        public override void PasteFromClipboard()
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

            if (iItems != null)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);
                try
                {
                    var iSelected = SelectedItem;
                    SelectedItems.Clear();
                    foreach (var i in iItems)
                    {
                        var iNeuron = Brain.Current[i];
                        if (iNeuron is NeuronCluster)
                        {
                            var iCluster = (NeuronCluster)iNeuron;
                            if (iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditional
                                || iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditionalPart)
                            {
                                iNeuron = iCluster.Duplicate();
                            }
                        }

                        var iNew = EditorsHelper.CreateFlowItemFor(iNeuron);
                        EditorsHelper.AddFlowItem(iNew, iSelected, this);
                        iSelected = iNew;
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion

        #endregion

        #region delete

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            throw new System.InvalidOperationException();
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

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            var iToRemove = fSelectedItems.ToList(); // we make a copy just in case we don't brake any loop code.
            iToRemove.Reverse();

                // we reverse the list so that we remove the last code item first.  This allows for better consistency in editing.
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we group all the data together so a single undo command cand restore the previous state.
            try
            {
                foreach (var i in iToRemove)
                {
                    RemoveFlowItem(i);
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
            return fSelectedItems.Count > 0;
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
            WindowMain.UndoStore.BeginUndoGroup(false, true);

                // we group all the data together so a single undo command cand restore the previous state. All deletes must be undone in the reverse order, since it's links are deleted first.
            try
            {
                var iDeleter = new NeuronDeleter<FlowItem>();
                iDeleter.BranchHandling = branchHandling;
                iDeleter.NeuronDeletionMethod = neuronDeletionMethod;
                var iToRemove = new System.Collections.Generic.List<NeuronDeleter<FlowItem>.DeleteRequestRecord>();
                foreach (var i in fSelectedItems)
                {
                    // we need to make a custom delete list, with the correct owner's list for each item that needs to be deleted.
                    var iRec = new NeuronDeleter<FlowItem>.DeleteRequestRecord();
                    iRec.Item = i;
                    if (i.Owner == this)
                    {
                        iRec.Owner = Items;
                    }
                    else
                    {
                        iRec.Owner = ((FlowItemBlock)i.Owner).Items;
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

        /// <summary>The remove flow item.</summary>
        /// <param name="toRemove">The to remove.</param>
        private void RemoveFlowItem(FlowItem toRemove)
        {
            if (toRemove.Owner == this)
            {
                if (Items.Remove(toRemove) == false)
                {
                    System.Windows.MessageBox.Show("Failed to remove the item from the main code list!");
                }
            }
            else if (toRemove.Owner is FlowItemBlock)
            {
                ((FlowItemBlock)toRemove.Owner).RemoveChild(toRemove);
            }
            else
            {
                System.Windows.MessageBox.Show("Unkown owner type, can't Remove!");
            }
        }

        /// <summary>The can delete special.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDeleteSpecial()
        {
            return CanDelete();
        }

        #endregion
    }
}