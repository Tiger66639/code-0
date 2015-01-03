// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorsOverview.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Provides an overview of the project editor data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Provides an overview of the project editor data.
    /// </summary>
    public partial class EditorsOverview : System.Windows.Controls.UserControl
    {
        /// <summary>The f search data.</summary>
        private EditorsOverviewSearcher fSearchData;

                                        // stores the current search info: so we can continue async + show in UI an overview.

        /// <summary>The f selected.</summary>
        private System.Windows.Controls.TreeViewItem fSelected;

        /// <summary>Initializes a new instance of the <see cref="EditorsOverview"/> class.</summary>
        public EditorsOverview()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Selected event of the TrvItems_Item control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TrvItems_Item_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = e.OriginalSource as System.Windows.Controls.TreeViewItem;
            if (fSelected == TrvRoot)
            {
                // the root object doesn't have a backing code object which handles the IsSelected, so we need to do it here.
                BrainData.Current.CurrentEditorsList = BrainData.Current.Editors;
            }
            else if (sender == fSelected)
            {
                // the next bit of code only needs to be executed when raised on the event source, not one of it's parents UI elements.
                fSelected.BringIntoView();

                    // we do this, so when we select a data object from code, it scrolls into view automatically.
            }
        }

        /// <summary>Handles the Unselected event of the TrvItems_Item control. If we don't
        ///     reset when unselected, we can cause a mem leak when creating or
        ///     loading a new project.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TrvItems_Item_Unselected(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = null;
        }

        /// <summary>Handles the MouseDoubleClick event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iOrSource = e.OriginalSource as System.Windows.FrameworkElement;
            var iSource = sender as System.Windows.FrameworkElement;
            if (iOrSource.DataContext == iSource.DataContext)
            {
                // this is to prevent  parent tree items from also trying to open stuff
                OpenEditor(((System.Windows.FrameworkElement)sender).DataContext as EditorBase);
            }
        }

        /// <summary>Handles the Click event of the MnuOpen control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = (EditorBase)((System.Windows.FrameworkElement)sender).DataContext;
            OpenEditor(iData);
        }

        /// <summary>Opens the mind map.</summary>
        /// <param name="data">The data.</param>
        private void OpenEditor(EditorBase data)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            try
            {
                if (data != null && !(data is EditorFolder))
                {
                    var iIndex = BrainData.Current.OpenDocuments.IndexOf(data);
                    var iMain = (WindowMain)System.Windows.Application.Current.MainWindow;
                    if (iIndex == -1)
                    {
                        iIndex = BrainData.Current.OpenDocuments.Count;
                        BrainData.Current.OpenDocuments.Add(data);

                            // need to add directly, can't use windowMain.AddItemToOpenDocuments cause we need to make frame visible, even if editor is already added to open documents (always want to make it visible).
                    }

                    iMain.ActivateDocAtIndex(iIndex);
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<System.Windows.Input.Cursor>(ResetCursor), 
                    null);
            }
        }

        /// <summary>Resets the cursor. Used to reset it async.</summary>
        /// <param name="prev">The prev.</param>
        private void ResetCursor(System.Windows.Input.Cursor prev)
        {
            System.Windows.Input.Mouse.OverrideCursor = prev;
        }

        /// <summary>The tree view item_ preview mouse right button down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TreeViewItem_PreviewMouseRightButtonDown(
            object sender, 
            System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.TreeViewItem;
            if (item != null)
            {
                item.Focus();
            }
        }

        /// <summary>The wrap panel_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void WrapPanel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.Button;
            System.Diagnostics.Debug.Assert(iSender != null);
            var iSenderImage = iSender.Content as System.Windows.Controls.Image;
            System.Diagnostics.Debug.Assert(iSenderImage != null);
            BtnMainNewSelector.Command = iSender.Command;
            BtnMainNewSelector.ToolTip = iSender.ToolTip;
            var iFound = System.Windows.Data.BindingOperations.GetBinding(
                iSenderImage, 
                System.Windows.Controls.Image.SourceProperty);
            var iBind = new System.Windows.Data.Binding("IsEnabled");
            iBind.Source = BtnMainNewSelector;
            iBind.ConverterParameter = iFound.ConverterParameter;
            iBind.Converter = iFound.Converter;
            ImageMainNewSelector.SetBinding(System.Windows.Controls.Image.SourceProperty, iBind);

                // we use a bindig for the image source, so we have disable picture.
            ToggleShowNewSelectors.IsChecked = false; // close the popup
        }

        /// <summary>The unload topics cmd_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UnloadTopicsCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iToProcess = Enumerable.ToList(BrainData.Current.Editors.AllTextPatternEditors());
            var ierrors = new System.Collections.Generic.List<string>();
            foreach (var i in iToProcess)
            {
                try
                {
                    i.SetIsParsed(true, ierrors);
                }
                catch (System.Exception ex)
                {
                    ierrors.Add(string.Format("parse failed: '{0}', error: {1}.", i.Name, ex.Message));
                }
            }
        }

        #region IsEditing

        /// <summary>
        ///     <see cref="IsEditing" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsEditingProperty =
            System.Windows.DependencyProperty.Register(
                "IsEditing", 
                typeof(bool), 
                typeof(EditorsOverview), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsEditingChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IsEditing" /> property. This dependency
        ///     property indicates wether the currently selected item in the list is
        ///     in edit mode or not.
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

        /// <summary>Handles changes to the <see cref="IsEditing"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsEditingChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((EditorsOverview)d).OnIsEditingChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="IsEditing"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIsEditingChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (fSelected != null)
                {
                    System.Windows.Input.Keyboard.Focus(fSelected);

                        // need to focus to the container, and not the list so that nav keys keep working.
                }
                else
                {
                    TrvItems.Focus();
                }
            }
        }

        #endregion

        #region Readonly textboxes

        /// <summary>Handles the Click event of the EditTextBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void EditTextBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsEditing = true;
        }

        /// <summary>Handles the LostFocus event of the TxtTitle control.</summary>
        /// <remarks>When looses focus, need to make certain that is editing is turned off.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_LostKeybFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (IsEditing)
            {
                // could already have commited
                CommitTextBox(sender as System.Windows.Controls.TextBox);
            }
        }

        /// <summary>Handles the LostFocus event of the TxtTitle control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsEditing)
            {
                // could already have commited
                CommitTextBox(sender as System.Windows.Controls.TextBox);
            }
        }

        /// <summary>Handles the PrvKeyDown event of the TxtTitle control.</summary>
        /// <remarks>when enter is pressed, need to stop editing.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_PrvKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                CommitTextBox(sender as System.Windows.Controls.TextBox);
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                IsEditing = false;
            }
        }

        /// <summary>Commits the text in the textBox to the datasource.</summary>
        /// <param name="sender">The sender.</param>
        private void CommitTextBox(System.Windows.Controls.TextBox sender)
        {
            var iBinding = sender.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            iBinding.UpdateSource();
            IsEditing = false;
        }

        #endregion

        #region Rename

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            IsEditing = true;
            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (fSelected != null)
            {
                var iEditor = fSelected.DataContext as ObjectTextPatternEditor;
                if (iEditor != null)
                {
                    e.CanExecute = ((NeuronCluster)iEditor.Item).Meaning != (ulong)PredefinedNeurons.Asset;

                        // we can edit all names, except that name of a patternEditor that is linked to an asset (sub of asset, is always same name of asset).
                }
                else
                {
                    e.CanExecute = true;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion

        #region Delete

        /// <summary>Handles the Executed event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iToDelete = fSelected.DataContext as EditorBase;
                System.Diagnostics.Debug.Assert(iToDelete != null);
                string iError;
                var iIsOpen = iToDelete.IsOpen;
                iToDelete.IsOpen = true;

                    // make certain that the editor is open, otherwise we open and close to many times.
                try
                {
                    if (iToDelete.EditorCanBeDeleted(out iError))
                    {
                        var iRes =
                            System.Windows.MessageBox.Show(
                                string.Format("Delete editor: {0} from the project?", iToDelete.Name), 
                                "Delete", 
                                System.Windows.MessageBoxButton.YesNo, 
                                System.Windows.MessageBoxImage.Question);
                        if (iRes == System.Windows.MessageBoxResult.Yes)
                        {
                            WindowMain.UndoStore.BeginUndoGroup();
                            try
                            {
                                iToDelete.DeleteEditor();
                                var iFolder = iToDelete.Owner as IEditorsOwner;

                                    // important: this needs to be done after the delete. Some editors are neurons, when they get deleted, they automatically remove themselves from the editor. Other editors are only wrappers and don't delete themselves, so we need to check for this.
                                if (iFolder != null)
                                {
                                    iFolder.Editors.Remove(iToDelete);
                                }
                                else
                                {
                                    BrainData.Current.Editors.Remove(iToDelete);
                                }
                            }
                            finally
                            {
                                WindowMain.UndoStore.EndUndoGroup();
                            }

                            iToDelete = null;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            string.Format("'{0}' can't be deleted because: {1}.", iToDelete.Name, iError), 
                            "Delete", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
                finally
                {
                    if (iToDelete != null)
                    {
                        iToDelete.IsOpen = iIsOpen;
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(), 
                    "Delete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Handles the Executed event of the Remove control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Remove_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iToDelete = fSelected.DataContext as EditorBase;
                System.Diagnostics.Debug.Assert(iToDelete != null);
                var iRes =
                    System.Windows.MessageBox.Show(
                        string.Format("Remove editor: {0} from the project?", iToDelete.Name), 
                        "Delete", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Question);
                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        var iFolder = iToDelete.Owner as EditorFolder;
                        if (iFolder != null)
                        {
                            iFolder.Items.Remove(iToDelete);
                        }
                        else
                        {
                            BrainData.Current.Editors.Remove(iToDelete);
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
                System.Windows.MessageBox.Show(
                    ex.ToString(), 
                    "Delete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Handles the Executed event of the DeleteSpecial control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteSpecial_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                var iToDelete = fSelected.DataContext as EditorBase;
                System.Diagnostics.Debug.Assert(iToDelete != null);
                var iDlg = new DlgSelectDeletionMethod();
                iDlg.Owner = System.Windows.Application.Current.MainWindow;
                var iDlgRes = iDlg.ShowDialog();
                if (iDlgRes.HasValue && iDlgRes.Value)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        iToDelete.DeleteAll(iDlg.NeuronDeletionMethod, iDlg.BranchHandling);
                        var iFolder = iToDelete.Owner as EditorFolder;
                        if (iFolder != null)
                        {
                            iFolder.Items.Remove(iToDelete);
                        }
                        else
                        {
                            BrainData.Current.Editors.Remove(iToDelete);
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
                System.Windows.MessageBox.Show(
                    ex.ToString(), 
                    "Delete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region search

        /// <summary>Handles the Executed event of the Find control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Find_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            TxtToSearch.Focus();
            e.Handled = true;
        }

        /// <summary>Handles the Executed event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (fSearchData == null)
            {
                fSearchData = new EditorsOverviewSearcher(BrainData.Current.Editors);
                fSearchData.Start(TxtToSearch.Text.ToLower());
            }
            else
            {
                fSearchData.Continue();
            }
        }

        /// <summary>Handles the CanExecute event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.IsNullOrEmpty(TxtToSearch.Text) == false;
        }

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
                fSearchData = null;
            }
        }

        #endregion

        #region replace topic

        /// <summary>The replace topic_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ReplaceTopic_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = e.Parameter as TextPatternEditor;
            if (iEditor != null)
            {
                e.CanExecute = ((NeuronCluster)iEditor.Item).Meaning != (ulong)PredefinedNeurons.Asset;

                    // can't do a replace on a topic that's linked to an asset. This has to be done by the asset itself.
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The replace topic_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ReplaceTopic_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iPatterns = e.Parameter as TextPatternEditor;
            if (iPatterns != null)
            {
                var iImporter = new TopicReplacer();
                iImporter.Replace(iPatterns);
            }
        }

        /// <summary>Handles the EndedOk event of the iImporter control. When the import is
        ///     done, we need to continue and reload all the</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void iImporter_EndedOk(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}