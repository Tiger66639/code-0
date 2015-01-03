// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleExporter.cs" company="">
//   
// </copyright>
// <summary>
//   A project operation that exports the neurons specified in the list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A project operation that exports the neurons specified in the list.
    /// </summary>
    /// <remarks>
    ///     When exporting thesaurus data, only data that hasn't been exported yet to
    ///     another module, can be exported.
    /// </remarks>
    internal class ModuleExporter : ProjectOperation
    {
        // string fPath;
        // bool fHasErrors = false;
        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>The f to export.</summary>
        private EditorBase fToExport;

        /// <summary>
        ///     Gets or sets the currently running exporter (there can only be 1
        ///     export running at the same time). This is used by objects to find out
        ///     if they need to do some mappping during a module export or not.
        /// </summary>
        /// <value>
        ///     The exporter.
        /// </value>
        public static ModuleExporter Exporter { get; set; }

        /// <summary>Shows a dialog box so the user can select the object that need to be
        ///     exported.</summary>
        /// <param name="item">The item.</param>
        internal void Export(EditorBase item)
        {
            if (Exporter != null)
            {
                throw new System.InvalidOperationException("Can't run 2 module exports at the same time!");
            }

            var iDlg = new Microsoft.Win32.SaveFileDialog();

            iDlg.Filter = Properties.Resources.NNLFileFilter;
            if (item != null)
            {
                iDlg.FileName = item.Name;
            }
            else
            {
                iDlg.FileName = BrainData.Current.Name;

                    // if no editor assigned, we are exporting the entire project, so give appropriate name
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
                fToExport = item;
                WindowMain.UndoStore.BeginUndoGroup(); // begin an undo group in the main UI, just to be save.
                System.Action iImportMod = InternalExport;
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>The internal export.</summary>
        private void InternalExport()
        {
            try
            {
                NNLSourceStreamer.Export(fFileName, fToExport);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                var iEr =
                    string.Format(
                        "An error occured while trying to export the NNL code '{0}', with the message: {1}", 
                        fFileName, 
                        e.Message);
                LogService.Log.LogError("Module exporter", iEr);
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
                "Module export", 
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
            base.PerformOnEnd();
        }
    }
}