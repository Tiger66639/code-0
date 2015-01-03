// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatLogExporter.cs" company="">
//   
// </copyright>
// <summary>
//   export the chatlog history.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     export the chatlog history.
    /// </summary>
    internal class ChatLogExporter : ProjectOperation
    {
        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>The export.</summary>
        internal void Export()
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.ChatLogFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.FileName = "chatlog " + BrainData.Current.Name;
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                DisableUI();
                fFileName = iDlg.FileName;
                System.Action iExportMod = InternalExport;
                iExportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>The internal export.</summary>
        private void InternalExport()
        {
            try
            {
                LogHistoryStreamer.Export(fFileName);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr = string.Format(
                    "An error occured while trying to export the query '{0}', with the error: {1}", 
                    fFileName, 
                    e.Message);
                LogService.Log.LogError("Export chatlog", iEr);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new System.Action<string>(ShowExportError), 
                    iEr);
            }
        }

        /// <summary>The show export error.</summary>
        /// <param name="err">The err.</param>
        private void ShowExportError(string err)
        {
            System.Windows.MessageBox.Show(
                err, 
                "Query export", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }
}