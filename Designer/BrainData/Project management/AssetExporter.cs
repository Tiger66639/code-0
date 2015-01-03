// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetExporter.cs" company="">
//   
// </copyright>
// <summary>
//   exports assets.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     exports assets.
    /// </summary>
    internal class AssetExporter : ProjectOperation
    {
        /// <summary>The f editor.</summary>
        private ObjectEditor fEditor;

        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>The export.</summary>
        /// <param name="toExport">The to export.</param>
        internal void Export(ObjectEditor toExport)
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.AssetFileFilter;
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
                try
                {
                    AssetXmlStreamer.Export(fEditor, fFileName);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
                }
                finally
                {
                    BaseXmlStreamer.AlreadyRendered = null;

                        // need to make certain that these are reset after import/export
                    BaseXmlStreamer.AlreadyRead = null;
                }
            }
            catch (System.Exception e)
            {
                var iEr = string.Format(
                    "An error occured while trying to export the asset '{0}', with the error: {1}", 
                    fFileName, 
                    e.Message);
                LogService.Log.LogError("Export Asset", iEr);
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
                "Asset export", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }
}