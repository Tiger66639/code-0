// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryExporter.cs" company="">
//   
// </copyright>
// <summary>
//   exports queries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     exports queries.
    /// </summary>
    internal class QueryExporter : ProjectOperation
    {
        /// <summary>The f editor.</summary>
        private QueryEditor fEditor;

        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>The export.</summary>
        /// <param name="toExport">The to export.</param>
        internal void Export(QueryEditor toExport)
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.QueryFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.FileName = toExport.Name; // we provide a default name, the same as the name of the editor.
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                DisableUI();
                fEditor = toExport;
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
                QueryXmlStreamer.Export(fEditor, fFileName);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr = string.Format(
                    "An error occured while trying to export the query '{0}', with the error: {1}", 
                    fFileName, 
                    e.Message);
                LogService.Log.LogError("Export query", iEr);
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