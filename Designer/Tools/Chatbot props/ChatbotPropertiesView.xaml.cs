// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatbotPropertiesView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ChatbotPropertiesView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for ChatbotPropertiesView.xaml
    /// </summary>
    public partial class ChatbotPropertiesView : System.Windows.Controls.UserControl, ITextPatternEditorView
    {
        /// <summary>The f current edit mode.</summary>
        private EditMode fCurrentEditMode = EditMode.Normal;

        /// <summary>Initializes a new instance of the <see cref="ChatbotPropertiesView"/> class.</summary>
        public ChatbotPropertiesView()
        {
            InitializeComponent();
        }

        #region CurrentEditMode (ITextPatternEditor)

        /// <summary>
        ///     Gets/sets the editing mode currently active. So we know when to move
        ///     keyboard focus or not.
        /// </summary>
        public EditMode CurrentEditMode
        {
            get
            {
                return fCurrentEditMode;
            }

            set
            {
                fCurrentEditMode = value;
            }
        }

        #endregion

        /// <summary>The date bot birthday_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DateBotBirthday_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var iProp = (ChatbotProperties)DataContext;
            if (iProp != null)
            {
                iProp.BotBirthdayText = DateBotBirthday.Text;
            }
        }

        /// <summary>The date picker_ key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DatePicker_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var iProp = (ChatbotProperties)DataContext;
            if (iProp != null)
            {
                iProp.UserBirthdayText = DateUserBDay.Text;
            }
        }

        /// <summary>Handles the CanExecute event of the DeleteMapItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteMapItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListBox;
            e.CanExecute = iSender.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the DeleteMapItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteMapItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListBox;
            var iVal = (ulong)iSender.SelectedItem;
            var iList = iSender.ItemsSource as SmallIDCollection;
            iList.Remove(iVal);
        }

        /// <summary>Handles the Executed event of the DeleteListPropItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteListPropItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListBox;
            var iVal = (ListBotPropValue)iSender.SelectedItem;
            var iList = iSender.ItemsSource as System.Collections.Generic.IList<ListBotPropValue>;
            iList.Remove(iVal);
        }

        /// <summary>The text box_ preview text input.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            var iSender = e.OriginalSource as System.Windows.Controls.TextBox;
            if (iProps != null && iSender != null)
            {
                iProps.TextSelectionStart = iSender.SelectionStart;
                iProps.TextSelectionLength = iSender.SelectionLength;
            }
        }

        /// <summary>When the selection range of the UI item has changed, we need to update
        ///     the data object as well.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.TextBox;
            if (iSender.IsLoaded)
            {
                var iProps = DataContext as ChatbotProperties;
                if (iProps != null && iSender != null)
                {
                    iProps.TextSelectionStart = iSender.SelectionStart;
                    iProps.TextSelectionLength = iSender.SelectionLength;
                }
            }
        }

        /// <summary>The date user b day_ lost focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DateUserBDay_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            if (iProps != null)
            {
                iProps.CommitUserBDay();
            }
        }

        /// <summary>The user control_ data context changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iOld = e.OldValue as ChatbotProperties;
            if (iOld != null && BrainData.Current != null && BrainData.Current.DesignerData != null)
            {
                // DesignerDat is null when we are opening another project.this can happen when the chatbot props are still open while opening another project.
                iOld.CommitUserBDay();
                iOld.CommitBotBDay();
            }
        }

        /// <summary>The date bot b day_ lost focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DateBotBDay_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            if (iProps != null)
            {
                iProps.CommitBotBDay();
            }
        }

        /// <summary>The do_ is keyboard focus within changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Do_IsKeyboardFocusWithinChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                var iProps = iSender.DataContext as ChatbotProperties;
                if (iProps != null)
                {
                    if ((bool)e.NewValue && iProps.HasDoAfter == false)
                    {
                        iProps.HasDoAfter = true;
                    }
                    else if ((bool)e.NewValue == false && iProps.DoAfter != null
                             && string.IsNullOrEmpty(iProps.DoAfter.Expression))
                    {
                        iProps.HasDoAfter = false;
                    }
                }
            }
        }

        /// <summary>The do startup_ is keyboard focus within changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DoStartup_IsKeyboardFocusWithinChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                var iProps = iSender.DataContext as ChatbotProperties;
                if (iProps != null)
                {
                    if ((bool)e.NewValue && iProps.HasDoStartup == false)
                    {
                        iProps.HasDoStartup = true;
                    }
                    else if ((bool)e.NewValue == false && iProps.DoStartup != null
                             && string.IsNullOrEmpty(iProps.DoStartup.Expression))
                    {
                        iProps.HasDoStartup = false;
                    }
                }
            }
        }

        /// <summary>The date user b day_ calendar closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DateUserBDay_CalendarClosed(object sender, System.Windows.RoutedEventArgs e)
        {
            var iProp = (ChatbotProperties)DataContext;
            if (iProp != null)
            {
                iProp.UserBirthdayText = DateUserBDay.Text;
            }
        }

        /// <summary>The date bot birthday_ calendar closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DateBotBirthday_CalendarClosed(object sender, System.Windows.RoutedEventArgs e)
        {
            var iProp = (ChatbotProperties)DataContext;
            if (iProp != null)
            {
                iProp.BotBirthdayText = DateBotBirthday.Text;
            }
        }

        #region delete

        /// <summary>Handles the CanExecute event of the DeleteItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            e.CanExecute = iProps != null && iProps.SelectedItems.Count > 0;
        }

        /// <summary>Handles the Executed event of the DeleteItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            var iSelected = new System.Collections.Generic.List<PatternEditorItem>();
            foreach (PatternEditorItem i in iProps.SelectedItems)
            {
                // need to make a local copy cause we are going to modify the list while deleting.
                iSelected.Add(i);
            }

            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                foreach (var i in iSelected)
                {
                    i.Delete();
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region InsertItem

        /// <summary>Handles the CanExecute event of the InsertItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = iFocused != null && iFocused.DataContext is ParsableTextPatternBase;
        }

        /// <summary>Handles the Executed event of the InsertItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertItem_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            if (iFocused != null)
            {
                WindowMain.UndoStore.BeginUndoGroup();
                try
                {
                    var iItem = iFocused.DataContext as ParsableTextPatternBase;
                    CurrentEditMode = iItem.RequiredEditMode;
                    var iNew = iItem.Insert();
                    if (iNew != null)
                    {
                        iItem.IsSelected = false;

                            // we deactivate the previous selection cause  the ctrl key is pressed during  the insert operation, causing  an 'add' in the selected prop, which we don't want.
                        iNew.IsSelected = true;
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion

        #region Cut

        /// <summary>The cut_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Cut_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            e.CanExecute = iProps != null && iProps.SelectedItems.Count > 0;
        }

        /// <summary>The cut_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Cut_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Copy_Executed(sender, e);
            var iProps = DataContext as ChatbotProperties;
            iProps.Delete();
        }

        #endregion

        #region copy

        /// <summary>The copy_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Copy_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            e.CanExecute = iProps != null && iProps.SelectedItems.Count > 0;
            e.Handled = true;
        }

        /// <summary>The copy_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Copy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            var iSelected = iProps.SelectedItems as SelectedPatternItemsCollection;
            var iValues = (from i in iSelected select i.Item.ID).ToList();
            var iData = EditorsHelper.GetDataObject();
            if (iValues.Count == 1)
            {
                iData.SetData(Properties.Resources.NeuronIDFormat, iValues[0]);
            }
            else
            {
                iData.SetData(Properties.Resources.MultiNeuronIDFormat, iValues);
            }

            var iPatterns = EditorsHelper.CopyPatternsToXml(iSelected.ToList(), null);
            iData.SetData(iSelected.GetClipboardID(), iPatterns);
            System.Windows.Clipboard.SetDataObject(iData, false);
        }

        #endregion

        #region Paste

        /// <summary>The paste_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Paste_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            e.CanExecute = iProps != null && iProps.CanPasteFromClipboard();
            e.Handled = true;
        }

        /// <summary>The paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Paste_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iProps = DataContext as ChatbotProperties;
            iProps.PasteFromClipboard();
        }

        #endregion
    }
}