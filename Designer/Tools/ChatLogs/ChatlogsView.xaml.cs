// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatlogsView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ChatlogsView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for ChatlogsView.xaml
    /// </summary>
    public partial class ChatlogsView : System.Windows.Controls.UserControl
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChatlogsView" /> class.
        /// </summary>
        public ChatlogsView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Selected event of the LstChatLogs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void LstChatLogs_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var iLogs = (ChatLogs)DataContext;
            var iSender = sender as System.Windows.Controls.ListBoxItem;
            if (iLogs != null && iSender != null)
            {
                iLogs.SelectedItem = (ChatLog)iSender.DataContext;
            }
        }

        /// <summary>The delete all_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iLogs = (ChatLogs)DataContext;
            iLogs.DeleteAll();
            iLogs.SelectedItem = null;
        }

        #region delete

        /// <summary>Handles the CanExecute event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iLogs = (ChatLogs)DataContext;
            e.CanExecute = iLogs != null && iLogs.SelectedItem != null;
        }

        /// <summary>Handles the Executed event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iLogs = (ChatLogs)DataContext;
            var iIndex = iLogs.Logs.IndexOf(iLogs.SelectedItem);
            iLogs.SelectedItem.Delete();
            if (iIndex < iLogs.Logs.Count)
            {
                iLogs.SelectedItem = iLogs.Logs[iIndex];
            }
            else if (iLogs.Logs.Count > 0)
            {
                iLogs.SelectedItem = iLogs.Logs[iLogs.Logs.Count - 1];
            }
            else
            {
                iLogs.SelectedItem = null;
            }
        }

        #endregion
    }
}