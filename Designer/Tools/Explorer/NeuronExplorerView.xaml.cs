// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronExplorerView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for NeuronExplorer.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Interaction logic for NeuronExplorer.xaml
    /// </summary>
    public partial class NeuronExplorerView : System.Windows.Controls.UserControl
    {
        #region fields

        /// <summary>
        ///     defines the nr of items that is considered large enough to start a multi threaded operation, like Delete.
        /// </summary>
        private const int LARGEDDATASIZE = 200;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronExplorerView"/> class. 
        ///     Initializes a new instance of the <see cref="NeuronExplorer"/> class.</summary>
        public NeuronExplorerView()
        {
            InitializeComponent();

            // CommandBindings.AddRange(App.Current.MainWindow.CommandBindings);                                  //we do this so we can access the command bindings, declared on the main window, even when the form is floating.
            Default = this;
        }

        #endregion

        /// <summary>Handles the Click event of the MenuItemRemove control.</summary>
        /// <remarks>Removes the currently selected neuron in the children list, from the cluster in the items list.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuItemRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            System.Diagnostics.Debug.Assert(iExplorer != null);
            var iClusterWrap = iExplorer.Selection.SelectedItem as NeuronExplorerItem;
            if (iClusterWrap != null)
            {
                var iCluster = iClusterWrap.Item as NeuronCluster;
                if (iCluster != null)
                {
                    var iItem = LstChildren.SelectedItem as NeuronExplorerItem;
                    if (iItem != null && iCluster.ChildrenIdentifier != null)
                    {
                        using (var iList = iCluster.ChildrenW) iList.Remove(iItem.Item);
                    }
                }
            }

            e.Handled = true;
        }

        #region Goto

        /// <summary>Handles the Executed event of the GotoPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void GotoPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new DlgStringQuestion();
            iDlg.Title = "Goto";
            iDlg.Question = "ID:";
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes == true)
            {
                ulong iId;
                if (ulong.TryParse(iDlg.Answer, out iId))
                {
                    var iExplorer = DataContext as NeuronExplorer;
                    System.Diagnostics.Debug.Assert(iExplorer != null);
                    iExplorer.Selection.SelectedID = iId;
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Not a valid id value, must be an unsigned (long) integer", 
                        "Invalid id", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region prop

        /// <summary>
        ///     Gets the default explorer.
        /// </summary>
        /// <value>
        ///     The default.
        /// </value>
        public static NeuronExplorerView Default { get; private set; }

        #region IsEditing

        /// <summary>
        ///     IsEditing Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsEditingProperty =
            System.Windows.DependencyProperty.Register(
                "IsEditing", 
                typeof(bool), 
                typeof(NeuronExplorerView), 
                new System.Windows.FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets the IsEditing property.  This dependency property
        ///     indicates if the currently selected item should be in edit mode or not.
        /// </summary>
        public bool IsEditing
        {
            get
            {
                return (bool)GetValue(IsEditingProperty);
            }

            set
            {
                SetValue(IsEditingProperty, value);
            }
        }

        #endregion

        #endregion

        #region Functions

        #region Event handlers

        /// <summary>Handles the MouseLeftButtonDown event of the ExplorerItem control. Selects the item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ExplorerItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iItem = iSender.DataContext as NeuronExplorerItem;
            if (iItem != null)
            {
                var iExplorer = DataContext as NeuronExplorer;
                System.Diagnostics.Debug.Assert(iExplorer != null);
                if (iExplorer.Selection.Contains(iItem.ID) == false)
                {
                    // when the new item being clicked on is already selected, don't do anything, the user is probably going to drag.
                    if (IsEditing)
                    {
                        TxtTitle_LostKeybFocus(System.Windows.Input.Keyboard.FocusedElement, null);

                            // need to make certain that the data gets saved before removing selection, otherwise we loose the edit.
                    }

                    if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift)
                        == System.Windows.Input.ModifierKeys.Shift)
                    {
                        iExplorer.Selection.SelectRange(iItem.ID);
                    }
                    else
                    {
                        iExplorer.Selection.SelectedID = iItem.ID;
                    }
                }
            }

            iSender.Focus();

                // we focus this, to enable keyboard events + to allow some other things to work who need a single neuron, like the 'BrowseNeuron' command.
            e.Handled = true; // this is important, otherwise the focus doesn't work.
        }

        #region Rename

        /// <summary>The rename_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            IsEditing = true;
            e.Handled = true;
        }

        /// <summary>The rename_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectedItem is NeuronExplorerItem;
            e.Handled = true; // need to prevent that the main also checks?
        }

        /// <summary>Handles the LostFocus event of the TxtTitle control.</summary>
        /// <remarks>When looses focus, need to make certain that is editing is turned off.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_LostKeybFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null && IsEditing)
            {
                // need to check on isEditing, since the keydown can stop the editing, whch also triggers this
                var iBinding = iSender.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                if (iBinding != null)
                {
                    iBinding.UpdateSource();
                }

                IsEditing = false;
            }
        }

        /// <summary>Handles the PrvKeyDown event of the TxtTitle control.</summary>
        /// <remarks>when enter is pressed, need to stop editing.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_PrvKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                var iBinding = iSender.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                var iEnded = false;
                if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
                {
                    iBinding.UpdateSource();
                    iEnded = true;
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    iBinding.UpdateTarget();

                        // we ask a target update to reset the value back to it's original for an escape.
                    iEnded = true;
                }

                if (iEnded)
                {
                    LstItems.Focus();
                    IsEditing = false;
                    e.Handled = true; // otherwise the focus wont work.
                }
            }
        }

        #endregion

        /// <summary>The merge neurons_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MergeNeurons_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = (NeuronExplorer)DataContext;
            var iItem = iExplorer.Selection.SelectedItem as NeuronExplorerItem;
            if (iItem != null)
            {
                var iDlg = new DlgMergeNeurons(iItem.Item);
                iDlg.Owner = System.Windows.Application.Current.MainWindow;
                iDlg.ShowDialog();
            }
        }

        /// <summary>Need to make certain mouse wheel scrolls work.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TheCtrl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            if (e.Delta > 0)
            {
                if (iExplorer.CurrentScrollPos > 2)
                {
                    // otherwise the scroll is illegal.
                    iExplorer.CurrentScrollPos -= (ulong)(e.Delta / 120);
                }
            }
            else if (iExplorer.CurrentScrollPos + (ulong)iExplorer.MaxVisible <= iExplorer.ItemCount + 1)
            {
                iExplorer.CurrentScrollPos += (ulong)(-e.Delta / 120);
            }

            e.Handled = true;
        }

        #endregion

        #region overrides

        /// <summary>need to calculate the maximum nr of visible items.</summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var iExplorer = DataContext as NeuronExplorer;
            if (iExplorer != null)
            {
                iExplorer.ActualHeight = LstItems.ActualHeight;
            }
        }

        #endregion

        #endregion

        #region MoveDownByLine

        /// <summary>Handles the CanExecute event of the MoveDownByLine control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveDownByLine_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectedID < iExplorer.ItemCount;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the MoveDownByLine control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveDownByLine_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            var iNew = iExplorer.Selection.SelectedID + 1;
            if (iNew < iExplorer.Selection.SelectedID || iNew >= iExplorer.ItemCount)
            {
                // check for overflow
                iNew = iExplorer.ItemCount - 1; // itemcount is 1 over.
            }

            iExplorer.Selection.SelectedID = iNew;
            e.Handled = true;
        }

        #endregion

        #region MoveUpByLine

        /// <summary>Handles the CanExecute event of the MoveUpByLine control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveUpByLine_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectedID > Neuron.EmptyId;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the MoveUpByLine control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveUpByLine_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            var iNew = iExplorer.Selection.SelectedID - 1;
            if (iNew > iExplorer.Selection.SelectedID)
            {
                // check for overflow
                iNew = Neuron.StartId;
            }

            iExplorer.Selection.SelectedID = iNew;
            e.Handled = true;
        }

        #endregion

        #region MoveDownByPage

        /// <summary>Handles the CanExecute event of the MoveDownByPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveDownByPage_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectedID < iExplorer.ItemCount;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the MoveDownByPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveDownByPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            var iNew = iExplorer.Selection.SelectedID + (ulong)iExplorer.MaxVisible;
            if (iNew < iExplorer.Selection.SelectedID || iNew > iExplorer.ItemCount)
            {
                // check for overflow
                iNew = iExplorer.ItemCount;
            }

            iExplorer.Selection.SelectedID = iNew;
            e.Handled = true;
        }

        #endregion

        #region MoveUpByPage

        /// <summary>Handles the CanExecute event of the MoveUpByPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveUpByPage_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectedID > Neuron.EmptyId;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the MoveUpByPage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveUpByPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            var iNew = iExplorer.Selection.SelectedID - (ulong)iExplorer.MaxVisible;
            if (iNew > iExplorer.Selection.SelectedID)
            {
                // check for overflow
                iNew = Neuron.StartId;
            }

            iExplorer.Selection.SelectedID = iNew;
            e.Handled = true;
        }

        #endregion

        #region Moveto end/start

        /// <summary>Handles the Executed event of the MoveToDocumentEnd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveToDocumentEnd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            iExplorer.Selection.SelectedID = iExplorer.ItemCount - 1;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the MoveToDocumentStart control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void MoveToDocumentStart_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            iExplorer.Selection.SelectedID = Neuron.StartId;
            e.Handled = true;
        }

        #endregion

        #region Delete

        /// <summary>Handles the CanExecute event of the DeleteMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteMain_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectionCanBeDeleted();
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the DeleteMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteMain_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            System.Windows.MessageBoxResult iRes;
            System.Diagnostics.Debug.Assert(iExplorer != null);

            if (iExplorer.Selection.SelectionCount == 1)
            {
                iRes =
                    System.Windows.MessageBox.Show(
                        string.Format("Delete neuron: '{0}'?", iExplorer.Selection.SelectedItem.ID), 
                        "Delete neuron", 
                        System.Windows.MessageBoxButton.OKCancel, 
                        System.Windows.MessageBoxImage.Question, 
                        System.Windows.MessageBoxResult.OK);
            }
            else if (iExplorer.Selection.SelectionCount <= LARGEDDATASIZE)
            {
                iRes =
                    System.Windows.MessageBox.Show(
                        string.Format("Delete {0} selected neurons?", iExplorer.Selection.SelectionCount), 
                        "Delete neuron", 
                        System.Windows.MessageBoxButton.OKCancel, 
                        System.Windows.MessageBoxImage.Question, 
                        System.Windows.MessageBoxResult.OK);
            }
            else
            {
                iRes =
                    System.Windows.MessageBox.Show(
                        string.Format(
                            "Delete {0} selected neurons?\nWarning: no undo data will be generated because there are to many neurons involved with the operation, are you certain?", 
                            iExplorer.Selection.SelectionCount), 
                        "Delete neuron", 
                        System.Windows.MessageBoxButton.OKCancel, 
                        System.Windows.MessageBoxImage.Warning, 
                        System.Windows.MessageBoxResult.Cancel);
            }

            if (iRes == System.Windows.MessageBoxResult.OK)
            {
                if (iExplorer.Selection.SelectionCount > LARGEDDATASIZE)
                {
                    System.Action iDelete = iExplorer.DeleteSelectedItemsNoUndo;
                    iDelete.BeginInvoke(null, null);
                }
                else
                {
                    iExplorer.DeleteSelectedItems();
                }
            }

            e.Handled = true;
        }

        #endregion

        #region Delete children

        /// <summary>Handles the Executed event of the DeleteChildren control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteChildren_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                foreach (ExplorerItem i in LstChildren.SelectedItems)
                {
                    Neuron iNeuron;
                    if (Brain.Current.TryFindNeuron(i.ID, out iNeuron))
                    {
                        WindowMain.DeleteItemFromBrain(iNeuron);
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the DeleteChildren control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteChildren_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstChildren.SelectedItems.Count > 0)
            {
                e.CanExecute = true;
                foreach (ExplorerItem i in LstChildren.SelectedItems)
                {
                    Neuron iNeuron;
                    if (Brain.Current.TryFindNeuron(i.ID, out iNeuron))
                    {
                        if (iNeuron.CanBeDeleted == false)
                        {
                            e.CanExecute = false;
                            break;
                        }
                    }
                    else
                    {
                        e.CanExecute = false;
                        break;
                    }
                }
            }
            else
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        #endregion

        #region Copy

        /// <summary>Handles the CanExecute event of the CopyMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void CopyMain_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            e.CanExecute = iExplorer != null && iExplorer.Selection.SelectionCount > 0;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the CopyMain control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CopyMain_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            System.Diagnostics.Debug.Assert(iExplorer != null);
            var iData = EditorsHelper.GetDataObject();

            if (iExplorer.Selection.SelectionCount > 1)
            {
                var iValues = Enumerable.ToList(iExplorer.Selection.SelectedIds);
                iData.SetData(Properties.Resources.MultiNeuronIDFormat, iValues, false);
            }
            else
            {
                iData.SetData(Properties.Resources.NeuronIDFormat, iExplorer.Selection.SelectedID, false);
            }

            System.Windows.Clipboard.SetDataObject(iData, false);
            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the CopyChildren control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void CopyChildren_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstChildren.SelectedItems.Count > 0;
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the CopyChildren control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CopyChildren_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iData = EditorsHelper.GetDataObject();

            if (LstChildren.SelectedItems.Count > 1)
            {
                var iValues = (from ExplorerItem i in LstChildren.SelectedItems select i.ID).ToList();
                iData.SetData(Properties.Resources.MultiNeuronIDFormat, iValues, false);
            }
            else
            {
                iData.SetData(Properties.Resources.NeuronIDFormat, ((ExplorerItem)LstChildren.SelectedItem).ID, false);
            }

            System.Windows.Clipboard.SetDataObject(iData, false);
            e.Handled = true;
        }

        #endregion

        #region FindNext

        /// <summary>Handles the KeyDown event of the TxtToSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TxtToSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                FindNextCmd_Executed(sender, null);
                e.Handled = true;
            }
            else if (e.Key != System.Windows.Input.Key.F3)
            {
                // when the textbox is focused and we press f3 to go to the next item, we don't want to reset the search.
                var iExplorer = DataContext as NeuronExplorer;
                System.Diagnostics.Debug.Assert(iExplorer != null);
                iExplorer.SearchData = null;
            }
        }

        /// <summary>Handles the Executed event of the Find control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Find_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            TxtToSearch.Focus();
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void FindNextCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iExplorer = DataContext as NeuronExplorer;
            System.Diagnostics.Debug.Assert(iExplorer != null);
            if (iExplorer.SearchData == null)
            {
                iExplorer.SearchData = new ExplorerSearcher(iExplorer);
                iExplorer.SearchData.Start(TxtToSearch.Text.ToLower());
            }
            else
            {
                iExplorer.SearchData.Continue();
            }
        }

        /// <summary>Handles the CanExecute event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void FindNextCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.IsNullOrEmpty(TxtToSearch.Text) == false;
        }

        #endregion
    }
}