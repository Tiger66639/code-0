// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatBotChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ChatBotChannelView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for ChatBotChannelView.xaml
    /// </summary>
    public partial class ChatBotChannelView : System.Windows.Controls.UserControl, ITextPatternEditorView
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ChatBotChannelView"/> class.</summary>
        public ChatBotChannelView()
        {
            InitializeComponent();
        }

        #endregion

        #region CurrentEditMode

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

        /// <summary>The toggle button_ target updated.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleButton_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            var iChannel = DataContext as ChatBotChannel;
            if (iChannel != null)
            {
                if (iChannel.ShowFloatingChar == false)
                {
                    iChannel.ShowFloatingCharUI = false;
                    SplitVertical.Visibility = System.Windows.Visibility.Visible;
                    CharacterView.DataContext = DataContext;
                    CharacterView.Visibility = System.Windows.Visibility.Visible;
                    ColHead.Width = fPrevWidth;
                }
                else
                {
                    fPrevWidth = ColHead.Width;
                    SplitVertical.Visibility = System.Windows.Visibility.Collapsed;
                    CharacterView.Visibility = System.Windows.Visibility.Collapsed;
                    CharacterView.DataContext = null;
                    ColHead.Width = new System.Windows.GridLength(0);
                    iChannel.ShowFloatingCharUI = true;
                }
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the SldZoomGrid control. Reset
        ///     the zoom value to default state.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void SldZoomGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iChannel = DataContext as ChatBotChannel;
            if (iChannel != null)
            {
                iChannel.ZoomValue = 1.0;
            }
        }

        /// <summary>Handles the MouseLeftButtonDown event of the Hyperlink control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void Hyperlink_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iChannel = DataContext as ChatBotChannel;
            if (iChannel != null && iChannel.SelectedCharacter != null
                && string.IsNullOrEmpty(iChannel.SelectedCharacter.AuthorWebsite) == false)
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(iChannel.SelectedCharacter.AuthorWebsite));
            }

            e.Handled = true;
        }

        /// <summary>Handles the CanExecute event of the Delete control. Need to prevent
        ///     the user from deleting the channel with the global X button.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>Handles the SelectionChanged event of the CmbAlternates control.
        ///     Whenever one of the alternates is changed, we ask the channel to
        ///     rebuild the display text.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CmbAlternates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iChannel = DataContext as ChatBotChannel;
            if (iChannel != null)
            {
                iChannel.RebuildInputForAlternates();
            }
        }

        #region fields

        /// <summary>The f prev width.</summary>
        private System.Windows.GridLength fPrevWidth = new System.Windows.GridLength(
            1, 
            System.Windows.GridUnitType.Star); // so we can restore the character column correctly.

        /// <summary>The f current edit mode.</summary>
        private EditMode fCurrentEditMode = EditMode.Normal;

        #endregion

        #region event handlers

        /// <summary>The txt send_ prv key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtSend_PrvKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                SendTextToSin();
            }
        }

        /// <summary>The btn send_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendTextToSin();
        }

        /// <summary>
        ///     Tries to send the text in the send text box to the <see cref="Sin" />
        ///     (if there is anything to send).
        /// </summary>
        private void SendTextToSin()
        {
            var iChannel = (ChatBotChannel)DataContext;
            if (iChannel != null)
            {
                if (iChannel.RecordedWords != null && iChannel.RecordedWords.Count > 0)
                {
                    iChannel.SendRecordedWords();
                }
                else if (string.IsNullOrEmpty(iChannel.InputText) == false)
                {
                    iChannel.SendTextToSin(iChannel.InputText);
                }

                iChannel.InputText = string.Empty;

                    // clear the selected text (also RecordedWords) so that the use can enter new data.recorded items gets cleared when we set input text to null.
            }
        }

        /// <summary>The btn clear_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ChatBotChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.ClearData();
            }
        }

        /// <summary>Handles the Click event of the SaveConv control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCopy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (ChatBotChannel)DataContext;
            System.Diagnostics.Debug.Assert(iChannel != null);
            var iRes = new System.Text.StringBuilder();
            foreach (var i in iChannel.ConversationLog)
            {
                iRes.Append(i);
                iRes.AppendLine();
            }

            System.Windows.Clipboard.SetText(iRes.ToString(), System.Windows.TextDataFormat.Text);
        }

        /// <summary>Handles the Click event of the SaveConv control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveConv_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.AddExtension = true;
            iDlg.DefaultExt = "txt";
            iDlg.Filter = "Text files (*.txt)|*.txt";
            var iDlgRes = iDlg.ShowDialog(WindowMain.Current);
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                var iChannel = (ChatBotChannel)DataContext;
                System.Diagnostics.Debug.Assert(iChannel != null);
                var iRes = new System.Text.StringBuilder();
                using (var iWriter = System.IO.File.CreateText(iDlg.FileName))
                    foreach (var i in iChannel.ConversationLog)
                    {
                        iWriter.WriteLine(i);
                    }
            }
        }

        #endregion
    }
}