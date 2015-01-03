// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicExporter.cs" company="">
//   
// </copyright>
// <summary>
//   A project operation that exports a <see cref="TextPatternEditor" /> to a
//   topic file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A project operation that exports a <see cref="TextPatternEditor" /> to a
    ///     topic file.
    /// </summary>
    internal class TopicExporter : ProjectOperation
    {
        /// <summary>The f editor.</summary>
        private TextPatternEditor fEditor;

        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>
        ///     Gets or sets the currently running exporter. So that only 1 exporter
        ///     can run at the same time.
        /// </summary>
        /// <value>
        ///     The exporter.
        /// </value>
        public TopicExporter Exporter { get; set; }

        /// <summary>The export.</summary>
        /// <param name="editor">The editor.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Export(TextPatternEditor editor)
        {
            if (Exporter != null)
            {
                throw new System.InvalidOperationException("Can't run 2 topic exports at the same time!");
            }

            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Filter = Properties.Resources.TopicFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.FileName = editor.Name; // we provide a default name, the same as the name of the editor.
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                Exporter = this;
                DisableUI();
                fEditor = editor;
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
                TopicXmlStreamer.Export(fEditor, fFileName);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr = string.Format(
                    "An error occured while trying to export the topic '{0}', with the error: {1}", 
                    fFileName, 
                    e.Message);
                LogService.Log.LogError("Export topic", iEr);
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
                "Topic export", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }
}