// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for AssetEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for AssetEditorView.xaml
    /// </summary>
    public partial class AssetEditorView : CtrlEditorBase
    {
        /// <summary>The togglebuttonsize.</summary>
        private const double TOGGLEBUTTONSIZE = 19.0;

        /// <summary>The attribvaluesplitsize.</summary>
        private const double ATTRIBVALUESPLITSIZE = 6.0;

        /// <summary>
        ///     Minimum drag distance that needs to be preserved.
        /// </summary>
        private const double DRAGMIN = 12.0;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="AssetEditorView"/> class.</summary>
        public AssetEditorView()
        {
            InitializeComponent();
        }

        #endregion

        /// <summary>Handles the TiltWheel event of the ThesPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.Designer.WPF.Controls.MouseTiltEventArgs"/> instance containing the event
        ///     data.</param>
        private void AssetsPanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            var iThes = (ObjectEditor)DataContext;
            if (iThes != null)
            {
                iThes.HorScrollPos += e.Tilt;
            }
        }

        #region AddAssetRecord

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void AddAssetRecord_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iAssets = GetCollectionForAdd();
                if (iAssets != null)
                {
                    var iNeuron = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNeuron);
                    var iNew = new AssetItem(iNeuron);
                    iAssets.Add(iNew);
                    iNew.NeedsBringIntoView = true;
                    iNew.IsSelected = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Gets the asset collection for addin a new item.</summary>
        /// <returns>The <see cref="AssetCollection"/>.</returns>
        private AssetCollection GetCollectionForAdd()
        {
            var iEditor = (ObjectEditor)DataContext;
            var iSelected = iEditor.SelectedItem as AssetBase;
            AssetCollection iAssets = null;
            if (iSelected != null)
            {
                var iGroup = iSelected as AssetGroup;
                if (iGroup != null)
                {
                    iGroup.IsExpanded = true; // make certian that the gorup is visible
                    return iGroup.Assets as AssetCollection;
                }

                if (iSelected.HasChildren)
                {
                    iSelected.IsExpanded = true;
                    iAssets = ((IAssetOwner)iSelected).Assets as AssetCollection;
                }
                else if (iSelected.Owner != iEditor)
                {
                    iAssets = iSelected.Owner.Assets as AssetCollection;
                }
                else
                {
                    iAssets = iEditor.Assets;
                }
            }
            else
            {
                iAssets = iEditor.Assets;
            }

            return iAssets;
        }

        /// <summary>Gets the asset collection of the currently selected row so that a child can be added of the row.</summary>
        /// <returns>The <see cref="AssetCollection"/>.</returns>
        private AssetCollection GetSubListOfSelected()
        {
            var iEditor = (ObjectEditor)DataContext;
            var iSelected = iEditor.SelectedItem as AssetItem;
            AssetCollection iAssets = null;
            if (iSelected != null)
            {
                var iData = iSelected.Data[iEditor.TreeColumn - 1];
                if (iData != null)
                {
                    if (iData.Value == null)
                    {
                        var iNew = NeuronFactory.GetCluster();
                        WindowMain.AddItemToBrain(iNew);
                        iNew.Meaning = (ulong)PredefinedNeurons.Asset;
                        iData.Value = iNew;
                    }

                    if (iData.HasChildren)
                    {
                        iSelected.IsExpanded = true;
                        iAssets = ((IAssetOwner)iSelected).Assets as AssetCollection;
                    }
                }
            }

            return iAssets;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void AddAssetRecord_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            e.CanExecute = iEditor != null;
            e.Handled = true;
        }

        #endregion

        #region AddAssetRecordWithSubAsset

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void AddAssetSubRecord_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iAssets = GetSubListOfSelected();
                if (iAssets != null)
                {
                    var iNeuron = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNeuron);
                    var iNew = new AssetItem(iNeuron);
                    iAssets.Add(iNew);
                    iNew.NeedsBringIntoView = true;
                    iNew.IsSelected = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void AddAssetSubRecord_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            e.Handled = true;
            if (iEditor != null && iEditor.SelectedItem is AssetItem)
            {
                var iSelected = (AssetItem)iEditor.SelectedItem;
                if (iEditor.TreeColumn >= 0 && iEditor.TreeColumn < iSelected.Data.Count)
                {
                    var iVal = iSelected.Data[iEditor.TreeColumn - 1].Value;
                    var iCluster = iVal as NeuronCluster;

                        // we do -1 cause first col is always attribute, is fixced and not included in the dataset
                    e.CanExecute = (iVal == null)
                                   || (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Asset);
                    return;
                }
            }

            e.CanExecute = false;
        }

        #endregion

        #region CreateSubAsset

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CreateSubAsset_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            var iAsset = iEditor.SelectedItem as AssetItem;
            Neuron iToDel = null; // in case we need to delete any data.
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iData = iAsset.Data[iEditor.SelectedColumn - 1];
                if (iData.Value != null)
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            "Previous data will be lost for the cell, do you wan to continue?", 
                            "Create sub asset", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Warning);
                    if (iRes == System.Windows.MessageBoxResult.Yes)
                    {
                        iToDel = iData.Value; // can only try to delete after the ref to the asset item is broken.
                    }
                    else
                    {
                        return;
                    }
                }

                var iNew = NeuronFactory.GetCluster();
                WindowMain.AddItemToBrain(iNew);
                iNew.Meaning = (ulong)PredefinedNeurons.Asset;
                iData.Value = iNew;
                if (iToDel != null)
                {
                    var iDel = new NeuronDeleter(DeletionMethod.Delete, DeletionMethod.DeleteIfNoRef);
                    iDel.Start(iToDel);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void CreateSubAsset_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            if (iEditor != null && iEditor.SelectedItem is AssetItem && iEditor.SelectedColumn > 0)
            {
                var iSelected = (AssetItem)iEditor.SelectedItem;
                if (iEditor.SelectedColumn <= iSelected.Data.Count)
                {
                    e.CanExecute = iSelected.Data[iEditor.SelectedColumn - 1].HasChildren == false;
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

            e.Handled = true;
        }

        #endregion

        #region ChangeToAssetItemListCmd

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeToAssetItemList_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            var iAsset = iEditor.SelectedItem as AssetItem;
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iData = iAsset.Data[iEditor.SelectedColumn - 1];
                var iNew = NeuronFactory.GetCluster();
                WindowMain.AddItemToBrain(iNew);
                iNew.Meaning = (ulong)PredefinedNeurons.And;
                if (iData.Value != null)
                {
                    using (var iList = iNew.ChildrenW) iList.Add(iData.Value);
                }

                iData.Value = iNew;
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeToAssetItemList_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            if (iEditor != null && iEditor.SelectedItem is AssetItem && iEditor.SelectedColumn > 0)
            {
                var iSelected = (AssetItem)iEditor.SelectedItem;
                var iCluster = iSelected.Data[iEditor.SelectedColumn - 1].Value as NeuronCluster;
                e.CanExecute = iCluster == null
                               || !(iCluster.Meaning == (ulong)PredefinedNeurons.And
                                    || iCluster.Meaning == (ulong)PredefinedNeurons.Or
                                    || iCluster.Meaning == (ulong)PredefinedNeurons.List
                                    || iCluster.Meaning == (ulong)PredefinedNeurons.Argument);
            }
            else
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        #endregion

        #region ChangeToAssetItemList

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeToAssetItemValue_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            var iAsset = iEditor.SelectedItem as AssetItem;
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iNewVal = Neuron.EmptyId;
                var iData = iAsset.Data[iEditor.SelectedColumn - 1];
                var iCluster = iData.Value as NeuronCluster;
                if (iCluster != null)
                {
                    IDListAccessor iChildren = iCluster.Children;
                    iChildren.Lock();
                    try
                    {
                        if (iChildren.CountUnsafe > 1)
                        {
                            var iRes =
                                System.Windows.MessageBox.Show(
                                    "Only the first item of the list will remain, all the others will be removed. Do you want to continue?", 
                                    "Remove list", 
                                    System.Windows.MessageBoxButton.YesNo, 
                                    System.Windows.MessageBoxImage.Warning);
                            if (iRes == System.Windows.MessageBoxResult.Yes)
                            {
                                iNewVal = iChildren.GetUnsafe(0);

                                    // can only try to delete after the ref to the asset item is broken.
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    finally
                    {
                        iChildren.Dispose(); // also unlocks.
                    }
                }

                var iOld = iData.Value;
                iData.Value = Brain.Current[iNewVal];
                if (iOld != null)
                {
                    var iDel = new NeuronDeleter(DeletionMethod.Delete, DeletionMethod.DeleteIfNoRef);
                    iDel.Start(iOld);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void ChangeToAssetItemValue_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            if (iEditor != null && iEditor.SelectedItem is AssetItem && iEditor.SelectedColumn > 0)
            {
                var iSelected = (AssetItem)iEditor.SelectedItem;
                var iCluster = iSelected.Data[iEditor.SelectedColumn - 1].Value as NeuronCluster;
                e.CanExecute = iCluster != null
                               && (iCluster.Meaning == (ulong)PredefinedNeurons.And
                                   || iCluster.Meaning == (ulong)PredefinedNeurons.Or
                                   || iCluster.Meaning == (ulong)PredefinedNeurons.List
                                   || iCluster.Meaning == (ulong)PredefinedNeurons.Argument);
            }
            else
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        #endregion

        #region And|Or asset items (no longer used)

        /// <summary>Handles the Executed event of the CreateOrCluster control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CreateOrCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iAssets = GetCollectionForAdd();
                if (iAssets != null)
                {
                    var iCluster = NeuronFactory.GetCluster();
                    WindowMain.AddItemToBrain(iCluster);
                    iCluster.Meaning = (ulong)PredefinedNeurons.Or;
                    var iNew = new AssetGroup(iCluster);
                    iAssets.Add(iNew);
                    iNew.NeedsBringIntoView = true;
                    iNew.IsSelected = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>The create and cluster_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CreateAndCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iAssets = GetCollectionForAdd();
                if (iAssets != null)
                {
                    var iCluster = NeuronFactory.GetCluster();
                    WindowMain.AddItemToBrain(iCluster);
                    iCluster.Meaning = (ulong)PredefinedNeurons.And;
                    var iNew = new AssetGroup(iCluster);
                    iAssets.Add(iNew);
                    iNew.NeedsBringIntoView = true;
                    iNew.IsSelected = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        #endregion

        #region recalc columsn

        #region Header dragging

        /// <summary>Handles the DragDelta event of the ThumbAttrib control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event
        ///     data.</param>
        private void ThumbCol_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            AssetColumn iCol = null;
            var iEditor = (ObjectEditor)DataContext;
            if (iEditor != null)
            {
                var iSender = sender as System.Windows.FrameworkElement;
                if (iSender != null)
                {
                    iCol = iSender.DataContext as AssetColumn;
                }

                if (iCol != null)
                {
                    var iChangeVal = e.HorizontalChange;
                    var iExpected = iCol.Width + e.HorizontalChange;
                    if (iExpected > DRAGMIN)
                    {
                        // we set a limit to the min size of a column
                        iCol.Width += iChangeVal;
                    }
                    else
                    {
                        iChangeVal = e.HorizontalChange + DRAGMIN - iExpected;
                        iCol.Width = DRAGMIN;
                    }

                    AssetColumn iLastCol;
                    var iCurUsed = iEditor.GetUsedSizeAndLastCol(out iLastCol);
                    if (iLastCol != null)
                    {
                        if (iCurUsed < ActualWidth)
                        {
                            iLastCol.Width = ActualWidth - iCurUsed;
                        }
                        else
                        {
                            iLastCol.Width = 0;
                        }
                    }

                    foreach (WPF.Controls.AssetPanelItem i in AssetsPanel.Children)
                    {
                        // all the visible elements also need to be adjusted. doing it this way, saves CPU -> easier binding (can't easely bind to 'root' object).
                        i.ChangeColumn(iCol.Index, iChangeVal);
                    }
                }
            }
        }

        #endregion

        /// <summary>Handles the Loaded event of the AssetRecord control.
        ///     When an asset record gets loaded, we need to let it refresh it's column widths from it's root.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>Handles the Loaded event of the AssetRecord control.
        ///     We also need to handle dataContext-Changed, cause otherwise expanded items aren't rednered correctly (they won't
        ///     have a
        ///     correct initial width + the invalidatemeasure is done on the incorrect type of object (it needs to be done on the
        ///     TreeViewITem)</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>Handles the MouseDown event of the AssetPanel control.
        ///     We need to make certain that the commands get activated and the list of selected items can be cleared.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void AssetPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == AssetsPanel)
            {
                // only try to handle when the user clicked on the background, and nothing else
                if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
                {
                    var iEditor = (ObjectEditor)DataContext;
                    if (iEditor != null)
                    {
                        iEditor.SelectedItems.Clear();
                    }
                }

                AssetsPanel.Focus();
                e.Handled = true;
            }
        }

        /// <summary>whenever the size changes, we need to recalculate the width of the final columns, so that it always fits nicely.
        ///     When first started, we divide evenly (when all columns are null and not the entire space is filled).
        ///     We don't need to assign this to the content of the asset panel, this does this automatically when loading.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CtrlEditorBase_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var iEditor = (ObjectEditor)DataContext;
            if (iEditor != null)
            {
                var iTotal = (from i in iEditor.Columns select i.Width).Sum();
                if (iTotal == 0)
                {
                    // init, divide evenly.
                    var iValue = (e.NewSize.Width - AssetsPanel.LevelDepth - ScrollVer.ActualWidth)
                                 / iEditor.Columns.Count;
                    foreach (var i in iEditor.Columns)
                    {
                        i.Width = iValue;
                    }
                }
                else
                {
                    AssetColumn iLastCol;
                    var iCurUsed = iEditor.GetUsedSizeAndLastCol(out iLastCol);
                    if (iLastCol != null)
                    {
                        if (iCurUsed < e.NewSize.Width)
                        {
                            iLastCol.Width = e.NewSize.Width - iCurUsed - AssetsPanel.LevelDepth - ScrollVer.ActualWidth;

                                // we add a single level depth, cause the columns are offset by that amount, 
                        }
                        else
                        {
                            iLastCol.Width = 0;
                        }
                    }
                }
            }
        }

        /// <summary>Handles the DataContextChanged event of the CtrlEditorBase control.
        ///     when we create a new asset editor when the previous item is also an asset editor, we dont' get a SizeChagned, only
        ///     this, so
        ///     calculate init col widhts here.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void CtrlEditorBase_DataContextChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iEditor = (ObjectEditor)e.NewValue;
            if (iEditor != null)
            {
                var iTotal = (from i in iEditor.Columns select i.Width).Sum();
                var iWidth = ActualWidth;
                if (iTotal == 0 && iWidth != 0)
                {
                    // init, divide evenly.
                    var iValue = (iWidth - AssetsPanel.LevelDepth - ScrollVer.ActualWidth) / 10;
                    foreach (var i in iEditor.Columns)
                    {
                        i.Width = iValue;
                    }
                }
            }
        }

        /// <summary>When a new cell gets focus, we need to store the active column, so that the rest of the system know which data
        ///     element to use
        ///     from within the selected row.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_IsKeyboardFocusWithinChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // only try to do this if we get focus, when loosing focus, doens't really matter, but we should better keep previous settings.
                var iSender = sender as System.Windows.FrameworkElement;
                var iIndex = (int)iSender.Tag;
                var iItem = iSender.DataContext as AssetItem;
                if (iItem != null)
                {
                    iItem.IsSelected = true; // make certain that the row is also selected.
                    var iEditor = iItem.Root;
                    if (iEditor != null)
                    {
                        iEditor.SelectedColumn = iIndex;
                    }
                }
            }
        }
    }
}