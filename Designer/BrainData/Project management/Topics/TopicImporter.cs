// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicImporter.cs" company="">
//   
// </copyright>
// <summary>
//   Used to import topic.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to import topic.
    /// </summary>
    internal class TopicImporter : ProjectStreamingOperation
    {
        /// <summary>The f imported.</summary>
        protected System.Collections.Generic.List<TextPatternEditor> fImported =
            new System.Collections.Generic.List<TextPatternEditor>();

                                                                     // the list of imported items, so we can make the visible on the UI when all have been imported.

        /// <summary>The f process tracker.</summary>
        private Search.ProcessTrackerItem fProcessTracker;

                                          // so we can keep the user informed of the current position and allow for cancel.

        /// <summary>
        ///     Initializes a new instance of the <see cref="TopicImporter" /> class.
        /// </summary>
        public TopicImporter()
            : base("Import topic")
        {
        }

        /// <summary>
        ///     Gets or sets the currently running topic importer. So we can keep
        ///     track if is is already running or not.
        /// </summary>
        /// <value>
        ///     The importer.
        /// </value>
        public static TopicImporter Importer { get; set; }

        /// <summary>
        ///     Gets the list of imported topics.
        /// </summary>
        public System.Collections.Generic.List<TextPatternEditor> Imported
        {
            get
            {
                return fImported;
            }
        }

        /// <summary>The import.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Import()
        {
            if (Importer != null)
            {
                throw new System.InvalidOperationException("Can only import 1 topic at a time.");
            }

            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.ImportTopicFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                Importer = this;
                fFiles = iDlg.FileNames;
                DisableUI();
                fProcessTracker = Search.ProcessTracker.Default.InitProcess();
                LoadFileSizes(fProcessTracker);
                System.Action iImportMod = InternalImport;
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>
        ///     Disables the UI.
        /// </summary>
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
            var iStillToResolve = new System.Collections.Generic.List<ToResolve>();
            var iErrors = new System.Collections.Generic.List<ParseErrors>();
            Parsers.ParserBase.BlockLogErrors = true; // so we only report the errors at the end when we want it.
            try
            {
                fProcessTracker.IsRunning = true;
                foreach (var iFile in fFiles)
                {
                    TextPatternEditor iRes = null;
                    try
                    {
                        if (System.IO.Path.GetExtension(iFile).ToLower() == ".aiml")
                        {
                            fProblems = AIMLStreamer.Import(iFile, fImported, fProcessTracker);
                        }
                        else
                        {
                            try
                            {
                                var iErr = new ParseErrors();
                                iErr.FileName = iFile;
                                iErrors.Add(iErr);
                                TopicXmlStreamer.Import(iFile, iErr.Errors, iStillToResolve, ref iRes, fProcessTracker);
                            }
                            catch (System.Exception e)
                            {
                                LogService.Log.LogError(
                                    "Import topic", 
                                    string.Format(
                                        "An error occured while trying to imort the topic '{0}', with the error: {1}", 
                                        iFile, 
                                        e.Message));
                                fProblems = true;
                            }
                        }
                    }
                    finally
                    {
                        if (iRes != null)
                        {
                            fImported.Add(iRes); // we always try to show the topics that were imported.
                        }
                    }
                }
            }
            finally
            {
                Parsers.ParserBase.BlockLogErrors = false;
                fProcessTracker.IsRunning = false;
                fProcessTracker.TotalCount = 0; // reset, otherwise the value doesn't get removed from the bar.
            }

            ResolveProblems(iErrors, iStillToResolve);
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalImportUI));
        }

        /// <summary>
        ///     Performs the UI section of the import.
        /// </summary>
        private void InternalImportUI()
        {
            System.Collections.Generic.IList<EditorBase> iAddTo;
            if (BrainData.Current.CurrentEditorsList != null)
            {
                iAddTo = BrainData.Current.CurrentEditorsList;
            }
            else
            {
                iAddTo = BrainData.Current.Editors; // add to the root if there is no other option.
            }

            if (fImported.Count <= 10)
            {
                foreach (var i in fImported)
                {
                    if (i != null)
                    {
                        i.IsMasterDetailView = true; // we always show in masterdetail view, this loads the fastest.
                        iAddTo.Add(i);
                        WindowMain.Current.AddItemToOpenDocuments(i);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "All the topics were added to the project. There were too many to display all, so only the first will be opened", 
                    "Topic import", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
                fImported[0].IsMasterDetailView = true; // we always show in masterdetail view, this loads the fastest.
                foreach (EditorBase i in fImported)
                {
                    iAddTo.Add(i);
                }

                WindowMain.Current.AddItemToOpenDocuments(fImported[0]);
            }

            EndOk();
            if (fProblems)
            {
                if (fFiles.Length > 1)
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while importing the files. Please check the log for more info.", 
                        "Import", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while importing the file. Please check the log for more info.", 
                        "Import", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
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
    }
}