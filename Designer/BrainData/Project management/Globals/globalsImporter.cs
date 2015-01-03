// --------------------------------------------------------------------------------------------------------------------
// <copyright file="globalsImporter.cs" company="">
//   
// </copyright>
// <summary>
//   a project operation to import the global patterns and bot props.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a project operation to import the global patterns and bot props.
    /// </summary>
    internal class globalsImporter : ProjectOperation
    {
        /// <summary>The f file.</summary>
        private string fFile; // this list of files that need to be imported.

        /// <summary>The f problems.</summary>
        private bool fProblems; // flag to keep track if there were problems.

        /// <summary>
        ///     Gets or sets the currently running topic importer. So we can keep
        ///     track if is is already running or not.
        /// </summary>
        /// <value>
        ///     The importer.
        /// </value>
        public static globalsImporter Importer { get; set; }

        /// <summary>The import.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Import()
        {
            if (Importer != null)
            {
                throw new System.InvalidOperationException("Can only import 1 globals file at a time.");
            }

            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.globalsFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.Multiselect = false;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                Importer = this;
                fFile = iDlg.FileName;
                DisableUI();
                System.Action iImportMod = InternalImport;
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>The disable ui.</summary>
        protected override void DisableUI()
        {
            base.DisableUI();
            BrainData.Current.LearnCount++; // we indicate that we are learning, cause we are importing data.
        }

        /// <summary>
        ///     Imports the topic.
        /// </summary>
        private void InternalImport()
        {
            var iErrors = new System.Collections.Generic.List<ParseErrors>();

            try
            {
                var iErr = new ParseErrors();
                iErr.FileName = fFile;
                iErr.Errors = new System.Collections.Generic.List<ParsableTextPatternBase>();
                iErrors.Add(iErr);
                var iBotProps = BrainData.Current.ChatbotProps;
                if (iBotProps == null)
                {
                    iBotProps = new ChatbotProperties();
                }

                iBotProps.ClearPatterns();
                ProjectXmlStreamer.ImportProps(fFile);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Import globals", e.Message);
                fProblems = true;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalImportUI));
        }

        /// <summary>
        ///     Performs the UI section of the import.
        /// </summary>
        private void InternalImportUI()
        {
            EndOk();
            if (fProblems)
            {
                System.Windows.MessageBox.Show(
                    "There were problems while importing the file. Please check the log for more info.", 
                    "Import globals", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to
        ///     put common code that always needs to be called at the end. When you
        ///     <see langword="override" /> this function, don't forget to call the
        ///     base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            Importer = null;
            BrainData.Current.LearnCount--; // also need to disable learning.
            base.PerformOnEnd();
        }

        #region internal types

        /// <summary>
        ///     So we can store the errors, handle them after all topics are done +
        ///     still know which file had the problem.
        /// </summary>
        private class ParseErrors
        {
            /// <summary>The errors.</summary>
            public System.Collections.Generic.List<ParsableTextPatternBase> Errors;

            /// <summary>The file name.</summary>
            public string FileName;
        }

        #endregion
    }
}