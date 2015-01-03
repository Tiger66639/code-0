// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicsExporter.cs" company="">
//   
// </copyright>
// <summary>
//   a project operation that exports all the <see cref="TextPatternEditor" />
//   s in the project to a directory. This includes all the patterns that are
//   attached to the thesaurus.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a project operation that exports all the <see cref="TextPatternEditor" />
    ///     s in the project to a directory. This includes all the patterns that are
    ///     attached to the thesaurus.
    /// </summary>
    internal class TopicsExporter : ProjectOperation
    {
        /// <summary>The f dir.</summary>
        private string fDir;

        /// <summary>
        ///     Gets or sets the currently running exporter. So that only 1 exporter
        ///     can run at the same time.
        /// </summary>
        /// <value>
        ///     The exporter.
        /// </value>
        public TopicsExporter Exporter { get; set; }

        // TextPatternEditor fEditor;

        /// <summary>The export.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Export()
        {
            if (Exporter != null)
            {
                throw new System.InvalidOperationException("Can't run 2 brain-file exports at the same time!");
            }

            var iDlg = new System.Windows.Forms.FolderBrowserDialog();
            iDlg.ShowNewFolderButton = true;
            iDlg.Description = "Select output location";
            var iRes = iDlg.ShowDialog();
            if (iRes == System.Windows.Forms.DialogResult.OK)
            {
                Exporter = this;
                DisableUI();
                fDir = iDlg.SelectedPath;
                WindowMain.UndoStore.BeginUndoGroup(); // begin an undo group in the main UI, just to be save.
                System.Action iExportMod = InternalExport;
                iExportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>The internal export.</summary>
        private void InternalExport()
        {
            string iPath = null;
            try
            {
                foreach (var i in BrainData.Current.Editors.AllTextPatternEditors())
                {
                    iPath = System.IO.Path.Combine(fDir, i.Name + "." + ProjectManager.TOPIC_EXTENTION);
                    TopicXmlStreamer.Export(i, iPath);
                }

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr =
                    string.Format(
                        "An error occured while trying to export the brain-file '{0}', with the error: {1}", 
                        iPath, 
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
                "topics export", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }

        /// <summary>
        ///     This function is called by <see cref="End" /> and EndOk. So it provides
        ///     a location to put common code that always needs to be called at the
        ///     end. When you <see langword="override" /> this function, don't forget
        ///     to call the base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            WindowMain.UndoStore.EndUndoGroup();
            base.PerformOnEnd();
        }

        /// <summary>
        ///     the final function to call when not all things went ok.
        /// </summary>
        protected override void End()
        {
            base.End();
        }
    }
}