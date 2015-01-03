// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusFileExporter.cs" company="">
//   
// </copyright>
// <summary>
//   exports a thesaurus(section) through a project operation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     exports a thesaurus(section) through a project operation.
    /// </summary>
    internal class ThesaurusFileExporter : ProjectOperation
    {
        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>The f relationship.</summary>
        private Neuron fRelationship;

        /// <summary>The f to export.</summary>
        private ThesaurusItem fToExport; // we can import into a

        /// <summary>
        ///     Gets or sets the currently running exporter. So that only 1 exporter
        ///     can run at the same time.
        /// </summary>
        /// <value>
        ///     The exporter.
        /// </value>
        public ThesaurusFileExporter Exporter { get; set; }

        /// <summary>The export.</summary>
        /// <param name="item">The item.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="compact">The compact.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Export(ThesaurusItem item, Neuron relationship, bool compact = false)
        {
            if (Exporter != null)
            {
                throw new System.InvalidOperationException("Can't run 2 thesaurus exports at the same time!");
            }

            fToExport = item;
            fRelationship = relationship;
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            if (fToExport != null && fRelationship != null)
            {
                iDlg.Filter = Properties.Resources.ThesaurusSectionFileFilter + Properties.Resources.ThesaurusFileFilter;
                iDlg.FileName = fToExport.NeuronInfo.DisplayTitle;
            }
            else
            {
                iDlg.Filter = Properties.Resources.ThesaurusFileFilter;
                iDlg.FileName = "full";
            }

            iDlg.FilterIndex = 1;
            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                Exporter = this;
                if (item != null)
                {
                    item.IsSelected = false;

                        // make certain that nothing is selected, otherwise the export will fail: while selection is turned off in another thread, a list that is monitored by the UI is chagned.
                }

                DisableUI();
                fFileName = iDlg.FileName;
                if (fFileName.EndsWith("thesaurus.xml"))
                {
                    fToExport = null;
                }

                WindowMain.UndoStore.BeginUndoGroup(); // begin an undo group in the main UI, just to be save.
                System.Action<bool> iImportMod = InternalExport;
                iImportMod.BeginInvoke(compact, null, null);

                // InternalExport(compact);                                                          //do a sync call, otherwise all kinds of problems with the thesaurus.
            }
        }

        /// <summary>The internal export.</summary>
        /// <param name="compact">The compact.</param>
        private void InternalExport(bool compact = false)
        {
            try
            {
                ThesaurusXmlStreamer.Export(fFileName, fToExport, fRelationship, compact);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr =
                    string.Format(
                        "An error occured while trying to export the thesaurus-file '{0}', with the message: {1}", 
                        fFileName, 
                        e.Message);
                LogService.Log.LogError("Export thesaurus file", iEr);
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
                "thesaurus file export", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to
        ///     put common code that always needs to be called at the end. When you
        ///     <see langword="override" /> this function, don't forget to call the
        ///     base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            Exporter = null;
            WindowMain.UndoStore.EndUndoGroup();
            if (fToExport != null)
            {
                fToExport.IsSelected = true;
            }

            base.PerformOnEnd();
        }
    }
}