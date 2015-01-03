// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectEditor.cs" company="">
//   
// </copyright>
// <summary>
//   An editor that allows a user to attach frames to an object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     An editor that allows a user to attach frames to an object.
    /// </summary>
    public class ObjectEditor : NeuronEditor, 
                                WPF.Controls.ITreeViewRoot, 
                                IAssetOwner, 
                                Data.IOnCascadedChanged, 
                                IEditorSelection
    {
        /// <summary>Called when an asset's selected property has changed.</summary>
        /// <param name="asset">The asset.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetSelected(AssetBase asset, bool value)
        {
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                fSelectedItems.Clear();
            }

            if (value)
            {
                fSelectedItems.AddInternal(asset);
            }
            else if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                fSelectedItems.RemoveInternal(asset);
            }
        }

        /// <summary>Gets the size used by the columns except the last one and returns the last col.</summary>
        /// <param name="lastCol">The last col.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetUsedSizeAndLastCol(out AssetColumn lastCol)
        {
            lastCol = null;
            double iRes = 0;
            foreach (var i in Columns)
            {
                if (i.Index == Columns.Count - 1)
                {
                    lastCol = i;
                }
                else
                {
                    iRes += i.Width;
                }
            }

            return iRes;
        }

        #region fields

        /// <summary>The f assets.</summary>
        protected AssetCollection fAssets;

        /// <summary>The f selected items.</summary>
        private readonly AssetSelectionList fSelectedItems = new AssetSelectionList();

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f tree column.</summary>
        private int fTreeColumn = 1;

        /// <summary>The f selected column.</summary>
        private int fSelectedColumn = 1;

        /// <summary>The f icon visibility.</summary>
        private System.Windows.Visibility fIconVisibility = System.Windows.Visibility.Visible;

        /// <summary>The f columns.</summary>
        private readonly System.Collections.Generic.List<AssetColumn> fColumns =
            new System.Collections.Generic.List<AssetColumn>();

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ObjectEditor"/> class.</summary>
        public ObjectEditor()
        {
            LoadColumns();
        }

        /// <summary>
        ///     loads all the columns
        /// </summary>
        private void LoadColumns()
        {
            if (BrainData.Current != null && BrainData.Current.AssetPronounIds != null)
            {
                // when loading from xml, the AssetPRonounsId's aren't loaded yet, so load after xml has loaded.
                AssetColumn iNew;
                iNew = new AssetColumn();
                iNew.Name = "Attribute"; // init the columsn
                iNew.Index = 0;
                iNew.Owner = this;
                iNew.LinkID = (ulong)PredefinedNeurons.Attribute;
                fColumns.Add(iNew);

                iNew = new AssetColumn();
                iNew.Name = "Value";
                iNew.Index = 1;
                iNew.Owner = this;
                iNew.LinkID = (ulong)PredefinedNeurons.Value;
                fColumns.Add(iNew);

                var iIndex = 2;
                foreach (var i in BrainData.Current.AssetPronounIds)
                {
                    if (i > (ulong)PredefinedNeurons.EndOfStatic)
                    {
                        // statics shouldn't be part of this list, they are handled seperatly.
                        if (fColumns.Count >= 9)
                        {
                            LogService.Log.LogError(
                                "AssetEditor.Columns", 
                                "There are to many asset pronouns mapped, can only display the first 7 columns");
                            break;
                        }

                        iNew = new AssetColumn();
                        iNew.Name = BrainData.Current.NeuronInfo[i].DisplayTitle; // init the columsn
                        iNew.Index = iIndex++;
                        iNew.LinkID = i;
                        iNew.Owner = this;
                        fColumns.Add(iNew);
                    }
                }

                iNew = new AssetColumn();
                iNew.Name = "Amount";
                iNew.Index = iIndex;
                iNew.Owner = this;
                iNew.LinkID = (ulong)PredefinedNeurons.Amount;
                fColumns.Add(iNew);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectEditor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ObjectEditor(Neuron toWrap)
            : base(toWrap)
        {
            LoadColumns();
        }

        #endregion

        #region Events  (ITreeViewRoot: NotifyCascadedPropertyChanged Members + ICascadedNotifyCollectionChanged Members)

        /// <summary>
        ///     Occurs when a property was changed in one of the thesaurus items. This is used for the tree display.
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when a collection was changed in one of the child items or the root list. This is used for the tree display.
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region Prop

        #region Name

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public override string Name
        {
            get
            {
                var iRes = base.Name;
                if (iRes == null && NeuronInfo != null && string.IsNullOrEmpty(NeuronInfo.DisplayTitle) == false)
                {
                    // if we loaded from xml or disk, the name still needs to be retrieved from the neuronInfo.
                    base.Name = NeuronInfo.DisplayTitle;
                    iRes = NeuronInfo.DisplayTitle;
                }

                return iRes;
            }

            set
            {
                base.Name = value;
            }
        }

        #endregion

        #region Assets

        /// <summary>
        ///     Gets the assets connected to the object.
        /// </summary>
        public AssetCollection Assets
        {
            get
            {
                return fAssets;
            }

            private set
            {
                fAssets = value;
                OnPropertyChanged("Assets");
                OnPropertyChanged("TreeItems");
            }
        }

        #endregion

        #region Columns

        /// <summary>
        ///     Gets the columns of the editor.
        /// </summary>
        public System.Collections.Generic.List<AssetColumn> Columns
        {
            get
            {
                return fColumns;
            }
        }

        #endregion

        #region IAssetOwner Assets

        /// <summary>
        ///     Gets the list of items
        /// </summary>
        /// <value></value>
        System.Collections.Generic.IList<AssetBase> IAssetOwner.Assets
        {
            get
            {
                return fAssets;
            }
        }

        #endregion

        #region TreeItems

        /// <summary>
        ///     Gets a list to all the children of the tree root.
        /// </summary>
        /// <value>The tree items.</value>
        public System.Collections.IList TreeItems
        {
            get
            {
                return fAssets;
            }
        }

        #endregion

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this editor.  This is usually class specific.
        ///     start with /
        /// </summary>
        /// <value></value>
        public override string Icon
        {
            get
            {
                return "/Images/Asset/objectAsset_Enabled.png";
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scrollbar position.
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                fVerScrollPos = value;
                OnPropertyChanged("VerScrollPos");
            }
        }

        #endregion

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll position.
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

        #region Root

        /// <summary>
        ///     Gets the root of the thesaurus
        /// </summary>
        /// <value>The root.</value>
        public ObjectEditor Root
        {
            get
            {
                return this;
            }
        }

        #endregion

        #region DocumentInfo

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>The document info.</value>
        public override string DocumentInfo
        {
            get
            {
                return "Object editor: " + Name;
            }
        }

        #endregion

        #region DocumentType

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>The type of the document.</value>
        public override string DocumentType
        {
            get
            {
                return "Object editor";
            }
        }

        #endregion

        #region IEditorSelection Members

        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        /// <value>The selected items.</value>
        public System.Collections.IList SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        /// <summary>
        ///     Gets/sets the currently selected item. If there are multiple selections, the first is returned.
        /// </summary>
        /// <value></value>
        public object SelectedItem
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

        #region TreeColumn

        /// <summary>
        ///     Gets/sets the index of the column that fills the tree, which can be expanded/collapsed for sub asset items.
        ///     The default is the 'value' column
        /// </summary>
        public int TreeColumn
        {
            get
            {
                return fTreeColumn;
            }

            set
            {
                if (value != fTreeColumn)
                {
                    fTreeColumn = value;
                    OnPropertyChanged("TreeColumn");
                    var iItemCol = value - 1;
                    foreach (AssetItem i in Assets)
                    {
                        i.UpdateAssetsForCol(iItemCol);
                    }
                }
            }
        }

        #endregion

        #region SelectedColumn

        /// <summary>
        ///     Gets/sets the index of the currently selected column, so we can find the data element that is currently being
        ///     edited.
        /// </summary>
        public int SelectedColumn
        {
            get
            {
                return fSelectedColumn;
            }

            set
            {
                fSelectedColumn = value;
                OnPropertyChanged("SelectedColumn");
            }
        }

        #endregion

        #region IconVisibility

        /// <summary>
        ///     Gets/sets the wether the icons on the NeuronEditors are visible for this editor or not.
        /// </summary>
        public System.Windows.Visibility IconVisibility
        {
            get
            {
                return fIconVisibility;
            }

            set
            {
                fIconVisibility = value;
                OnPropertyChanged("IconVisibility");
                OnPropertyChanged("IsIconVisible");
            }
        }

        #endregion

        #region IsIconVisible

        /// <summary>
        ///     Gets or sets a value indicating whether  the icons on the NeuronEditors are visible for this editor or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is icon visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsIconVisible
        {
            get
            {
                return fIconVisibility == System.Windows.Visibility.Visible;
            }

            set
            {
                if (value)
                {
                    IconVisibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    IconVisibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Inheriters return a list of children that can be used to browse through the content and select
        ///     a neuron. This is used by the <see cref="WPF.Controls.NeuronDataBrowser" /> objects.
        ///     returns all the asset items
        /// </summary>
        public override System.Collections.IEnumerator BrowsableItems
        {
            get
            {
                var iIsOpen = IsOpen;
                try
                {
                    IsOpen = true;

                        // we always open cause we don't know how the asset data is connected to this object. This technique is easiest to get around the problem. 
                    return new AssetItemEnumerator(Assets, Name);
                }
                finally
                {
                    IsOpen = iIsOpen;
                }
            }
        }

        #endregion

        #region overrides

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            var iItem = Item;
            if (iItem != null)
            {
                var iFound = iItem.FindFirstOut((ulong)PredefinedNeurons.Asset) as NeuronCluster;
                if (iFound != null)
                {
                    Assets = new AssetCollection(this, iFound);
                }
                else
                {
                    Assets = new AssetCollection(this, (ulong)PredefinedNeurons.Asset);
                }
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be unloaded.
        /// </summary>
        protected override void UnloadUIData()
        {
            fAssets.IsActive = false; // we disable the list so that it doesn't get any updates anymore, just in case.
            fAssets = null;
        }

        /// <summary>
        ///     Registers the item that was read from xml.
        ///     Need to load all the columns cause they couldn't be loaded during creation (AssetPRonounIDs werent loaded yet).
        /// </summary>
        public override void Register()
        {
            LoadColumns();
            base.Register();
        }

        #endregion

        #region Clipboard

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            if (fSelectedItems.Count == 1)
            {
                var iAsset = fSelectedItems[0] as AssetItem;
                if (iAsset != null)
                {
                    data.SetData(Properties.Resources.NeuronIDFormat, iAsset.Item.ID);
                    data.SetData(Properties.Resources.ASSETRECORDFORMAT, iAsset);
                }
                else
                {
                    data.SetData(Properties.Resources.ASSETRECORDFORMAT, fSelectedItems[0]);
                }
            }
            else
            {
                var iNeurons = new System.Collections.Generic.List<ulong>();
                foreach (var i in fSelectedItems)
                {
                    var iAsset = i as AssetItem;
                    if (iAsset != null)
                    {
                        iNeurons.Add(iAsset.Item.ID);
                    }
                    else
                    {
                        iNeurons = null;
                        break;
                    }
                }

                if (iNeurons != null)
                {
                    data.SetData(Properties.Resources.MultiNeuronIDFormat, iNeurons);
                    data.SetData(Properties.Resources.MULTIASSETRECORDFORMAT, Enumerable.ToList(fSelectedItems));
                }
                else
                {
                    data.SetData(Properties.Resources.MULTIASSETRECORDFORMAT, Enumerable.ToList(fSelectedItems));
                }
            }
        }

        /// <summary>
        ///     Determines whether this instance can copy the selected data to the clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can copy to the clipboard; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanCopyToClipboard()
        {
            return fSelectedItems.Count > 0;
        }

        /// <summary>
        ///     Determines whether this instance can paste special from the clipboard.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste special from the clipboard; otherwise, <c>false</c>.
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

        /// <summary>The can paste from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanPasteFromClipboard()
        {
            if (base.CanPasteFromClipboard())
            {
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.ASSETRECORDFORMAT)
                    || System.Windows.Clipboard.ContainsData(Properties.Resources.MULTIASSETRECORDFORMAT))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The paste from clipboard.</summary>
        public override void PasteFromClipboard()
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
                    {
                        DoPasteNeuron();
                    }
                    else if (System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
                    {
                        DoPasteNeurons();
                    }
                    else if (System.Windows.Clipboard.ContainsData(Properties.Resources.ASSETRECORDFORMAT))
                    {
                        DoPasteAssetRecord(
                            (AssetItem)System.Windows.Clipboard.GetData(Properties.Resources.ASSETRECORDFORMAT));
                    }
                    else if (System.Windows.Clipboard.ContainsData(Properties.Resources.MULTIASSETRECORDFORMAT))
                    {
                        DoPasteAssetRecords();
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("Paste", iMsg);
            }
        }

        /// <summary>The do paste asset records.</summary>
        private void DoPasteAssetRecords()
        {
            var iToPaste =
                (System.Collections.Generic.List<AssetItem>)
                System.Windows.Clipboard.GetData(Properties.Resources.MULTIASSETRECORDFORMAT);
            foreach (var i in iToPaste)
            {
                DoPasteAssetRecord(i);
            }
        }

        /// <summary>The do paste asset record.</summary>
        /// <param name="toPaste">The to paste.</param>
        private void DoPasteAssetRecord(AssetItem toPaste)
        {
            AssetItem iRes = null;

            if (System.Windows.Clipboard.ContainsData(Properties.Resources.CUTOPERATION) == false)
            {
                // if it was a cut operation, we don't need to make a duplicate of the data.
                var iNew = toPaste.Item.Duplicate();
                var iUndo = new NeuronUndoItem { Action = BrainAction.Created, Neuron = iNew };
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                iRes = new AssetItem(iNew);

                    // real copy, co make a duplicate of the asset record (same links out, no parent)
            }
            else
            {
                iRes = new AssetItem(toPaste.Item); // in  a cut, we can reuse the neuron itself.
            }

            fAssets.Add(iRes);
        }

        /// <summary>The do paste neurons.</summary>
        private void DoPasteNeurons()
        {
            var iIds =
                (System.Collections.Generic.List<ulong>)
                System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat);
            foreach (var i in iIds)
            {
                var iToPaste = Brain.Current[i];
                System.Diagnostics.Debug.Assert(iToPaste != null);
                fAssets.Add(new AssetItem(iToPaste));
            }
        }

        /// <summary>The do paste neuron.</summary>
        private void DoPasteNeuron()
        {
            var iToPaste = Brain.Current[(ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat)];
            System.Diagnostics.Debug.Assert(iToPaste != null);
            fAssets.Add(new AssetItem(iToPaste));
        }

        #endregion

        #region Delete

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them. This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            var iIndex = 0;
            IAssetOwner iOwner = null;
            foreach (AssetItem i in Enumerable.ToArray(fSelectedItems))
            {
                // we make local copy cause we are going to modify the list.
                iOwner = i.Owner;
                iIndex = iOwner.Assets.IndexOf(i);
                iOwner.Assets.RemoveAt(iIndex);
            }

            fSelectedItems.Clear();
            if (iOwner != null)
            {
                if (iOwner.Assets.Count > iIndex)
                {
                    fSelectedItems.Add(iOwner.Assets[iIndex]);
                }
                else if (iOwner.Assets.Count > 0)
                {
                    fSelectedItems.Add(iOwner.Assets[iOwner.Assets.Count - 1]);
                }
            }
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iIndex = 0;
                    IAssetOwner iOwner = null;
                    foreach (AssetItem i in Enumerable.ToArray(fSelectedItems))
                    {
                        // we make local copy cause we are going to modify the list.
                        iOwner = i.Owner;
                        iIndex = iOwner.Assets.IndexOf(i);
                        iOwner.Assets.RemoveAt(iIndex);

                            // important to remove the frame from the list  before it gets deleted for the undo data, otherwise we try to add the frame to the list when the backing neuron is still deleted.
                    }

                    var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.Nothing);
                    iDeleter.Start((from i in fSelectedItems where i is AssetItem select ((AssetItem)i).Item).ToArray());

                        // make a local copy, cause the flows-list will change.
                    fSelectedItems.Clear();
                    if (iOwner != null)
                    {
                        if (iOwner.Assets.Count > iIndex)
                        {
                            fSelectedItems.Add(iOwner.Assets[iIndex]);
                        }
                        else if (iOwner.Assets.Count > 0)
                        {
                            fSelectedItems.Add(iOwner.Assets[iOwner.Assets.Count - 1]);
                        }

                        if (fSelectedItems.Count > 0)
                        {
                            fSelectedItems[0].NeedsBringIntoView = true; // to make certain it is visible and focused.
                        }
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("Paste to Frame editor", iMsg);
            }
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return fSelectedItems.Count > 0;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            var iDlg = new DlgSelectDeletionMethod();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                switch (iDlg.NeuronDeletionMethod)
                {
                    case DeletionMethod.Nothing:
                        break;
                    case DeletionMethod.Remove:
                        Remove();
                        break;
                    case DeletionMethod.DeleteIfNoRef:
                        DeleteSelectedSpecial(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                        break;
                    case DeletionMethod.Delete:
                        DeleteSelectedSpecial(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>The delete selected special.</summary>
        /// <param name="rootDelMethod">The root del method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        private void DeleteSelectedSpecial(DeletionMethod rootDelMethod, DeletionMethod branchHandling)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iIndex = 0;
                IAssetOwner iOwner = null;
                var iSelected = new System.Collections.Generic.List<Neuron>();
                foreach (AssetItem i in fSelectedItems.ToArray())
                {
                    // we make local copy cause we are going to modify the list.
                    iOwner = i.Owner;
                    iIndex = iOwner.Assets.IndexOf(i);
                    iOwner.Assets.RemoveAt(iIndex);
                    iSelected.Add(i.Item);
                }

                fSelectedItems.Clear();
                NeuronDeleter iDeleter;
                iDeleter = new NeuronDeleter(rootDelMethod, branchHandling);
                iDeleter.Start(iSelected);
                if (iOwner != null)
                {
                    if (iOwner.Assets.Count > iIndex)
                    {
                        fSelectedItems.Add(iOwner.Assets[iIndex]);
                    }
                    else if (iOwner.Assets.Count > 0)
                    {
                        fSelectedItems.Add(iOwner.Assets[iOwner.Assets.Count - 1]);
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanDeleteSpecial()
        {
            return CanDelete();
        }

        /// <summary>Deletes all the neurons on the editor according to the specified deletion and branch-handling methods.
        ///     This is called when the editor is deleted from the project using the special delete.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        /// <remarks>Doesn't need to be undo save, this is done by the caller.</remarks>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start((from i in fAssets select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event. (used to display the treeview).</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion
    }
}