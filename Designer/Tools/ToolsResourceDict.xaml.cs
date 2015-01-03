// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolsResourceDict.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   The tools resource dict.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>The tools resource dict.</summary>
    public partial class ToolsResourceDict : System.Windows.ResourceDictionary
    {
        /// <summary>Handles the MouseDoubleClick event of the <see cref="LogService.LogItem"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void LogItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListViewItem;
            if (iSender != null && iSender.Content is LogService.LogItem)
            {
                var iItem = (LogService.LogItem)iSender.Content;
                var iPath = iItem.Tag as Search.DisplayPath;
                if (iPath != null)
                {
                    iPath.SelectPathResult();
                }
            }
        }

        /// <summary>Handles the Click event of the MnuItemClear control.</summary>
        /// <remarks>Clears all the log data.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuItemClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WPFLog.WPFLog.Default.Items.Clear();
        }

        /// <summary>copies the curently selected log item to the clipboard.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MnuItemCopyLogItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.MenuItem;
            if (iSender != null)
            {
                var iMenu = iSender.Parent as System.Windows.Controls.ContextMenu;
                if (iMenu != null)
                {
                    var iCtrl = iMenu.PlacementTarget as System.Windows.Controls.ContentControl;
                    if (iCtrl != null)
                    {
                        var iContent = iCtrl.Content as LogService.LogItem;
                        if (iContent != null)
                        {
                            System.Windows.Clipboard.SetText(
                                string.Format(
                                    "{0}   {1}   {2}: {3}", 
                                    iContent.Level, 
                                    iContent.Time, 
                                    iContent.Source, 
                                    iContent.Text));
                        }
                    }
                }
            }
        }
    }
}