// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FrameEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for FrameEditorView.xaml
    /// </summary>
    public partial class FrameEditorView : CtrlEditorBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameEditorView"/> class. 
        ///     Initializes a new instance of the <see cref="FrameEditorView"/>
        ///     class.</summary>
        public FrameEditorView()
        {
            InitializeComponent();
        }

        #endregion

        #region Functions

        /// <summary>Adds the new frame to the frame editor object and makes it the
        ///     selected one.</summary>
        /// <param name="toAdd">To add.</param>
        private void AddNewFrame(Frame toAdd)
        {
            var iEditor = (IFrameOwner)DataContext;
            toAdd.IsLoaded = true;
            iEditor.Frames.Add(toAdd);
            iEditor.SelectedFrame = toAdd;
        }

        /// <summary>The add new frame sequence.</summary>
        /// <param name="toAdd">The to add.</param>
        private void AddNewFrameSequence(FrameSequence toAdd)
        {
            var iEditor = (IFrameOwner)DataContext;
            var iSelected = iEditor.SelectedFrame;
            System.Diagnostics.Debug.Assert(iSelected != null);
            iSelected.Sequences.Add(toAdd);
            TabContent.SelectedIndex = 2; // need to make certain that the correct page is visible
        }

        #endregion

        #region Add Frame/element/evoker/sequence

        /// <summary>Handles the Executed event of the AddFrame command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddFrame_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iObject = EditorsHelper.MakeFrame();
                    if (iObject != null)
                    {
                        AddNewFrame(iObject);
                    }
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame", iMsg);
            }
        }

        /// <summary>The add frame_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFrame_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            e.CanExecute = iEditor != null;
        }

        /// <summary>Handles the CanExecute event of the AddToSelected control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddToSelected_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            e.CanExecute = iEditor != null && iEditor.SelectedFrame != null;
        }

        /// <summary>Handles the Executed event of the <see cref="AddFrameSequence"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddFrameSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iObject = EditorsHelper.MakeFrameSequence();
                    if (iObject != null)
                    {
                        AddNewFrameSequence(iObject);
                    }
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame sequence", iMsg);
            }
        }

        /// <summary>Handles the Executed event of the <see cref="AddFrameElement"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddFrameElement_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iObject = EditorsHelper.MakeFrameElement();
                    if (iObject != null)
                    {
                        AddFrameElement(iObject);
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
                LogService.Log.LogError("New Frame element", iMsg);
            }
        }

        #endregion

        #region MoveElementUp

        /// <summary>The move element up_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveElementUp_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstSequenceContent != null)
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                if (LstSequenceContent.SelectedItems.Count > 0)
                {
                    var iIndex = iCollection.IndexOf((FrameSequenceItem)LstSequenceContent.SelectedItems[0]);
                    e.CanExecute = iIndex > 0; // if the first item is at the first pos, we can no longer move up.
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The move element up_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveElementUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                var iSelected = (from FrameSequenceItem i in LstSequenceContent.SelectedItems select i).ToList();
                foreach (var i in iSelected)
                {
                    var iIndex = iCollection.IndexOf(i);
                    iCollection.Move(iIndex, iIndex - 1);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region MoveElementDown

        /// <summary>Handles the CanExecute event of the MoveElementDown control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void MoveElementDown_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstSequenceContent != null)
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                var iSelected = LstSequenceContent.SelectedItems;
                if (iSelected.Count > 0)
                {
                    var iIndex = iCollection.IndexOf((FrameSequenceItem)iSelected[iSelected.Count - 1]);
                    e.CanExecute = iIndex < iCollection.Count - 1;

                        // if the last selected item is at the last pos, we can no longer move up.
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the MoveElementDown control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void MoveElementDown_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                var iSelected =
                    (from FrameSequenceItem i in LstSequenceContent.SelectedItems select i).Reverse().ToList();

                    // we need to reverse the list cause the last item needs to be moved first.
                foreach (var i in iSelected)
                {
                    var iIndex = iCollection.IndexOf(i);
                    iCollection.Move(iIndex, iIndex + 1);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region AddElementToSequence

        /// <summary>Handles the CanExecute event of the AddElementToSequence control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddElementToSequence_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstNotUsed != null && LstNotUsed.SelectedItems.Count > 0;
        }

        /// <summary>Handles the Executed event of the AddElementToSequence control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddElementToSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                var iList = (from FrameElement i in LstNotUsed.SelectedItems select i).ToList();

                    // need to make a copy cause we are going to modify this list.
                foreach (var i in iList)
                {
                    var iNew = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNew);
                    var iItem = new FrameSequenceItem(iNew);
                    iItem.Element = i.Item;
                    iCollection.Add(iItem);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region RemoveElementFromSequence

        /// <summary>Handles the CanExecute event of the RemoveElementFromSequence control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveElementFromSequence_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstSequenceContent != null && LstSequenceContent.SelectedItems.Count > 0;
        }

        /// <summary>Handles the Executed event of the RemoveElementFromSequence control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void RemoveElementFromSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iCollection = (FrameSequenceItemCollection)LstSequenceContent.ItemsSource;
                var iList = (from FrameSequenceItem i in LstSequenceContent.SelectedItems select i).ToList();

                    // need to make a copy cause we are going to modify this list.
                var iDel = new NeuronDeleter<FrameSequenceItem>();
                iDel.BranchHandling = DeletionMethod.Nothing;
                iDel.NeuronDeletionMethod = DeletionMethod.Delete;
                iDel.Start(iCollection, iList);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region Add frame element restrictions

        /// <summary>Handles the CanExecute event of the AddToSelectedElement command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddToSelectedElement_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GetFilterCollection(DataContext as IFrameOwner) != null;
        }

        /// <summary>The add frame element filter_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFrameElementFilter_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iCol = GetFilterCollection(DataContext as IFrameOwner);
                if (iCol != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup(false);

                        // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                    try
                    {
                        FERestrictionSegment iSegment;
                        var iObject = EditorsHelper.MakeFERestriction(out iSegment);
                        if (iObject != null)
                        {
                            iCol.Add(iObject);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame element filter", iMsg);
            }
        }

        /// <summary>Gets the collection that is currently selected for adding items too.</summary>
        /// <param name="iEditor">The i Editor.</param>
        /// <returns>The <see cref="RestrictionsCollection"/>.</returns>
        internal static RestrictionsCollection GetFilterCollection(IFrameOwner iEditor)
        {
            if (iEditor != null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused == null)
                {
                    return GetSelectedRestictionCol(iEditor, iEditor.SelectedFrame);
                }

                var iPrev = iFocused.DataContext as FERestriction;
                if (iPrev != null)
                {
                    if (iPrev.Owner is FrameElement)
                    {
                        return ((FrameElement)iPrev.Owner).RestrictionsRoot.Items;
                    }

                    if (iPrev.Owner != null)
                    {
                        return ((FERestrictionGroup)iPrev.Owner).Items;
                    }
                }
                else
                {
                    var iGroup = iFocused.DataContext as FERestrictionGroup;
                    if (iGroup != null)
                    {
                        return iGroup.Items;
                    }

                    var iEl = iFocused.DataContext as FrameElement;
                    if (iEl != null)
                    {
                        return iEl.RestrictionsRoot.Items;
                    }

                    return GetSelectedRestictionCol(iEditor, iEditor.SelectedFrame);
                }
            }

            return null;
        }

        /// <summary>Gets the restiction col from the selected <paramref name="frame"/>
        ///     element or sequence, depending on the tabvalue.</summary>
        /// <param name="iEditor">The i editor.</param>
        /// <param name="frame">The frame.</param>
        /// <returns>The <see cref="RestrictionsCollection"/>.</returns>
        private static RestrictionsCollection GetSelectedRestictionCol(IFrameOwner iEditor, Frame frame)
        {
            if (frame != null)
            {
                if (iEditor.SelectedTabIndex == 0 && frame.SelectedElement != null)
                {
                    return frame.SelectedElement.RestrictionsRoot.Items;
                }
            }

            return null;
        }

        /// <summary>The add frame element filter group_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFrameElementFilterGroup_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iCol = GetFilterCollection(DataContext as IFrameOwner);
                if (iCol != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup(false);

                        // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                    try
                    {
                        var iObject = EditorsHelper.MakeFERestrictionGroup();
                        if (iObject != null)
                        {
                            iCol.Add(iObject);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame element filter group", iMsg);
            }
        }

        /// <summary>The add fe custom filter_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFECustomFilter_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iCol = GetFilterCollection(DataContext as IFrameOwner);
                if (iCol != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup(false);

                        // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                    try
                    {
                        var iObject = EditorsHelper.MakeFECustomFilter();
                        if (iObject != null)
                        {
                            iCol.Add(iObject);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame element custom filter", iMsg);
            }
        }

        /// <summary>The add fe bool filter_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFEBoolFilter_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iCol = GetFilterCollection(DataContext as IFrameOwner);
                if (iCol != null)
                {
                    WindowMain.UndoStore.BeginUndoGroup(false);

                        // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                    try
                    {
                        var iObject = EditorsHelper.MakeFEBoolFilter();
                        if (iObject != null)
                        {
                            iCol.Add(iObject);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Frame element bool filter", iMsg);
            }
        }

        #endregion

        #region AddFEFilterSegment

        /// <summary>The add fe filter segment cmd_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFEFilterSegmentCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            FERestriction iRestriction = null;
            var iEl = e.OriginalSource as System.Windows.FrameworkElement;
            if (iEl != null)
            {
                iRestriction = iEl.DataContext as FERestriction;
            }

            if (iRestriction == null)
            {
                iRestriction = GetSelectedRestriction();
            }

            e.CanExecute = iRestriction != null;
        }

        /// <summary>Gets the currently selected restriction.</summary>
        /// <returns>The <see cref="FERestriction"/>.</returns>
        private FERestriction GetSelectedRestriction()
        {
            var iEditor = (IFrameOwner)DataContext;
            if (iEditor != null)
            {
                if (iEditor.SelectedTabIndex == 0)
                {
                    var iFrame = iEditor.SelectedFrame;
                    if (iFrame != null)
                    {
                        if (iFrame.SelectedElement != null)
                        {
                            return iFrame.SelectedElement.SelectedRestriction as FERestriction;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>The add fe filter segment cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFEFilterSegmentCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    FERestriction iRestriction = null;
                    var iEl = e.OriginalSource as System.Windows.FrameworkElement;
                    if (iEl != null)
                    {
                        iRestriction = iEl.DataContext as FERestriction;
                    }

                    if (iRestriction == null)
                    {
                        iRestriction = GetSelectedRestriction();
                    }

                    if (iRestriction != null)
                    {
                        EditorsHelper.MakeFESegment(iRestriction);
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
                LogService.Log.LogError("New Frame element filter group", iMsg);
            }
        }

        #endregion

        #region Sequence commands

        /// <summary>The delete selected sequence_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedSequence_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            e.CanExecute = iEditor.SelectedFrame != null && iEditor.SelectedFrame.SelectedSequence != null;
        }

        /// <summary>The copy selected sequence_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedSequence_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedSequence_CanExecute(sender, e);
        }

        /// <summary>The paste to selected sequence_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PasteToSelectedSequence_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            if (EditorsHelper.IsValidCipboardData()
                && System.Windows.Clipboard.ContainsData(Properties.Resources.FrameSequenceFormat)
                && System.Windows.Clipboard.ContainsData(Properties.Resources.FrameOriginFormat))
            {
                var iFrameId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.FrameOriginFormat);

                    // a sequence can only be copied to the same frame, not to another one since another frame has other frame elemenets, and a sequence points to the frame elements.
                e.CanExecute = iFrameId == iEditor.SelectedFrame.Item.ID;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The cut selected sequence_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedSequence_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedSequence_CanExecute(sender, e);
        }

        /// <summary>The delete selected sequence_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false, true);
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedSequence;
                EditorsHelper.DeleteRecursiveChildren((NeuronCluster)iSelected.Item);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The copy selected sequence_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            var iSelected = iEditor.SelectedFrame.SelectedSequence;
            var iData = iSelected.CopyToClipboard();
            System.Windows.Clipboard.SetDataObject(iData);
        }

        /// <summary>The paste to selected sequence_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void PasteToSelectedSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                FrameSequence iEl = null;
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.FrameSequenceFormat))
                {
                    var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.FrameSequenceFormat);
                    Neuron iSource;
                    if (Brain.Current.TryFindNeuron(iId, out iSource) && iSource is NeuronCluster)
                    {
                        Neuron iNew = EditorsHelper.DuplicateFrameSequence((NeuronCluster)iSource, null, null);
                        iEl = new FrameSequence(iNew);
                        AddFrameSequence(iEl);
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            "The id on the clipboard is no longer present in the network.");
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The cut selected sequence_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedSequence_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedSequence;
                var iData = iSelected.CopyToClipboard();
                System.Windows.Clipboard.SetDataObject(iData);
                iEditor.SelectedFrame.Sequences.Remove(iSelected);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region restrictions commands

        /// <summary>The delete selected restriction_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedRestriction_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            e.CanExecute = iEditor.SelectedFrame != null && iEditor.SelectedFrame.SelectedElement != null
                           && iEditor.SelectedFrame.SelectedElement.SelectedRestriction != null;
        }

        /// <summary>The copy selected restriction_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedRestriction_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedRestriction_CanExecute(sender, e);
        }

        /// <summary>The paste to selected restriction_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PasteToSelectedRestriction_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (EditorsHelper.IsValidCipboardData())
            {
                e.CanExecute = System.Windows.Clipboard.ContainsData(Properties.Resources.FERestrictionFormat)
                               || System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The cut selected restriction_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedRestriction_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedRestriction_CanExecute(sender, e);
        }

        /// <summary>The delete selected restriction_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedRestriction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false, true);
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedElement.SelectedRestriction;
                EditorsHelper.DeleteFERestriction(iSelected.Item);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The copy selected restriction_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedRestriction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            var iSelected = iEditor.SelectedFrame.SelectedElement.SelectedRestriction;
            var iData = iSelected.CopyToClipboard();
            System.Windows.Clipboard.SetDataObject(iData, false);
        }

        /// <summary>The paste to selected restriction_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void PasteToSelectedRestriction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                FERestrictionBase iEl = null;
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
                {
                    var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat);
                    Neuron iSource;
                    if (Brain.Current.TryFindNeuron(iId, out iSource))
                    {
                        var iNew = EditorsHelper.DuplicateFERestriction(iSource);
                        if (iNew is NeuronCluster)
                        {
                            iEl = new FERestrictionGroup(iNew);
                        }
                        else
                        {
                            iEl = new FERestriction(iNew);
                        }

                        var iListToAddTo = GetCurrentRestrictionsList(); // the list to add the newly duplicated item to.
                        System.Diagnostics.Debug.Assert(iListToAddTo != null);
                        iListToAddTo.Add(iEl);
                        iEl.IsSelected = true;
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            "The id on the clipboard is no longer present in the network.");
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Gets the restrictions list that is currently selected to add items to.
        ///     This is the one of the currently selected restrictionsGroup, the
        ///     parent of the selected restriction or the root if there is no item
        ///     selected.</summary>
        /// <returns>The <see cref="RestrictionsCollection"/>.</returns>
        private RestrictionsCollection GetCurrentRestrictionsList()
        {
            var iEditor = (IFrameOwner)DataContext; // still need to add the duplicated item.
            System.Diagnostics.Debug.Assert(
                iEditor.SelectedFrame != null && iEditor.SelectedFrame.SelectedElement != null);
            var iSelected = iEditor.SelectedFrame.SelectedElement.SelectedRestriction;
            if (iSelected != null)
            {
                if (iSelected is FERestrictionGroup)
                {
                    return ((FERestrictionGroup)iSelected).Items;
                }

                if (iSelected.Owner is FERestrictionGroup)
                {
                    return ((FERestrictionGroup)iSelected.Owner).Items;
                }

                return iEditor.SelectedFrame.SelectedElement.RestrictionsRoot.Items;
            }

            return iEditor.SelectedFrame.SelectedElement.RestrictionsRoot.Items;
        }

        /// <summary>The cut selected restriction_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedRestriction_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedElement.SelectedRestriction;
                iSelected.CopyToClipboard();
                var iGroup = iSelected.Owner as FERestrictionGroup;
                if (iGroup != null)
                {
                    iGroup.Items.Remove(iSelected);
                }
                else
                {
                    iEditor.SelectedFrame.SelectedElement.RestrictionsRoot.Items.Remove(iSelected);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region element commands

        /// <summary>The delete selected element_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedElement_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            e.CanExecute = iEditor != null && iEditor.SelectedFrame != null
                           && iEditor.SelectedFrame.SelectedElement != null;

                // frameeditor can be null when it is unloading.
        }

        /// <summary>The copy selected element_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedElement_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedElement_CanExecute(sender, e);
        }

        /// <summary>The paste to selected element_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PasteToSelectedElement_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            if (iEditor != null && iEditor.SelectedFrame != null && EditorsHelper.IsValidCipboardData())
            {
                e.CanExecute = System.Windows.Clipboard.ContainsData(Properties.Resources.FrameElementFormat)
                               || System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The cut selected element_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedElement_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            DeleteSelectedElement_CanExecute(sender, e);
        }

        /// <summary>The delete selected element_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSelectedElement_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false, true); // all deletes must be recorded in reverse order.
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedElement;
                EditorsHelper.DeleteFrameElement(iSelected.Item);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The copy selected element_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CopySelectedElement_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            var iSelected = iEditor.SelectedFrame.SelectedElement;
            var iData = iSelected.CopyToClipboard();
            System.Windows.Clipboard.SetDataObject(iData);
        }

        /// <summary>The paste to selected element_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void PasteToSelectedElement_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                FrameElement iEl = null;
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
                {
                    var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat);
                    Neuron iSource;
                    if (Brain.Current.TryFindNeuron(iId, out iSource))
                    {
                        var iEditor = (IFrameOwner)DataContext;
                        System.Diagnostics.Debug.Assert(iEditor != null);
                        var iNew = EditorsHelper.DuplicateFrameElement(iSource, iEditor.SelectedFrame.Item);
                        iEl = new FrameElement(iNew);
                        AddFrameElement(iEl);
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            "The id on the clipboard is no longer present in the network.");
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The cut selected element_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CutSelectedElement_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iEditor = (IFrameOwner)DataContext;
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iSelected = iEditor.SelectedFrame.SelectedElement;
                var iData = iSelected.CopyToClipboard();
                System.Windows.Clipboard.SetDataObject(iData);
                iEditor.SelectedFrame.Elements.Remove(iSelected);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region element commands

        /// <summary>The rename_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iItem = e.Parameter as INeuronWrapper;

            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = iItem != null || (iFocused != null && iFocused.DataContext is INeuronWrapper);
        }

        /// <summary>The rename_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = e.Parameter as INeuronWrapper;
            if (iItem == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is INeuronWrapper)
                {
                    iItem = (INeuronWrapper)iFocused.DataContext;
                }
            }

            if (iItem != null)
            {
                EditorsHelper.RenameItem(iItem.Item, "Change name of frame item");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>Handles the Loaded event of the LstFrames control. We need to make
        ///     certain that the selected items is visible. If it isn't, we don't
        ///     display the flow data itself.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LstFrames_Loaded(object sender, System.Windows.RoutedEventArgs args)
        {
            var iEditor = (IFrameOwner)DataContext;
            if (iEditor != null)
            {
                object iSelected = iEditor.SelectedFrame;
                if (iSelected != null)
                {
                    LstFrames.ScrollIntoView(iSelected);
                }
            }
        }

        /// <summary>The txt element name_ got focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtElementName_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEditor = (IFrameOwner)DataContext;
            var iSender = sender as System.Windows.Controls.TextBox;
            System.Diagnostics.Debug.Assert(iSender != null);
            iEditor.SelectedFrame.SelectedElement = iSender.DataContext as FrameElement;
            e.Handled = true;
        }

        /// <summary>Handles the MouseDown event of the DataElements control.</summary>
        /// <remarks>We need to move keyboard focus to the dataElemetns control when the
        ///     user clicks on it (but not on a row), so that the copy paste commands
        ///     work.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void DataElements_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataElements.Focus();
            e.Handled = true;
        }

        #endregion

        #region Helpers

        /// <summary>Adds the new frame element to the currently selected frame and makes
        ///     it the selected one + visual.</summary>
        /// <param name="toAdd">The to Add.</param>
        private void AddFrameElement(FrameElement toAdd)
        {
            var iEditor = (IFrameOwner)DataContext;
            var iSelected = iEditor.SelectedFrame;
            System.Diagnostics.Debug.Assert(iSelected != null);
            iSelected.Elements.Add(toAdd);
            iEditor.SelectedTabIndex = 0; // need to make certain that the correct page is visible
            toAdd.IsSelected = true;
        }

        /// <summary>Adds the new frame sequence to the currently selected frame and makes
        ///     it the selected one + visual.</summary>
        /// <param name="toAdd">To add.</param>
        private void AddFrameSequence(FrameSequence toAdd)
        {
            var iEditor = (IFrameOwner)DataContext;

            var iSelected = iEditor.SelectedFrame;
            System.Diagnostics.Debug.Assert(iSelected != null);
            iSelected.Sequences.Add(toAdd);
            iEditor.SelectedTabIndex = 1; // need to make certain that the correct page is visible
            iSelected.SelectedSequence = toAdd;
        }

        #endregion
    }
}