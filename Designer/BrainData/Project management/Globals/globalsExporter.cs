// --------------------------------------------------------------------------------------------------------------------
// <copyright file="globalsExporter.cs" company="">
//   
// </copyright>
// <summary>
//   a project operation to export all the global patterns and chatbot props.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a project operation to export all the global patterns and chatbot props.
    /// </summary>
    internal class GlobalsExporter : ProjectOperation
    {
        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>
        ///     Gets or sets the currently running exporter. So that only 1 exporter
        ///     can run at the same time.
        /// </summary>
        /// <value>
        ///     The exporter.
        /// </value>
        public GlobalsExporter Exporter { get; set; }

        /// <summary>The export.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Export()
        {
            if (Exporter != null)
            {
                throw new System.InvalidOperationException("Can't run 2 topic exports at the same time!");
            }

            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.globalsFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.FileName = "globals";
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                Exporter = this;
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
                ProjectXmlStreamer.ExportProps(fFileName);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr = string.Format(
                    "An error occured while trying to export the topic '{0}', with the error: {1}", 
                    fFileName, 
                    e.Message);
                LogService.Log.LogError("Export globals", iEr);
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
                "Export globals", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }
}