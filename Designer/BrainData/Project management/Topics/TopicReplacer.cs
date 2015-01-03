// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicReplacer.cs" company="">
//   
// </copyright>
// <summary>
//   The topic replacer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>The topic replacer.</summary>
    internal class TopicReplacer : TopicImporter
    {
        /// <summary>The f all editors.</summary>
        private System.Collections.Generic.List<TextPatternEditor> fAllEditors;

        /// <summary>The f index.</summary>
        private int fIndex = -1;

        /// <summary>The f owner.</summary>
        private IEditorsOwner fOwner;

        /// <summary>The f process tracker.</summary>
        private Search.ProcessTrackerItem fProcessTracker;

                                          // so we can keep the user informed of the current position and allow for cancel.

        /// <summary>The f to replace.</summary>
        private TextPatternEditor fToReplace;

        /// <summary>The f prev values.</summary>
        private readonly System.Collections.Generic.List<bool> fPrevValues = new System.Collections.Generic.List<bool>();

        /// <summary>Imports a single topic.</summary>
        /// <param name="iToReplace">The i To Replace.</param>
        public void Replace(TextPatternEditor iToReplace)
        {
            if (Importer != null)
            {
                throw new System.InvalidOperationException("Can only import 1 topic at a time.");
            }

            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.TopicFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                fToReplace = iToReplace;
                Importer = this;
                fFiles = iDlg.FileNames;
                DisableUI();
                fOwner = fToReplace.Owner as IEditorsOwner;
                fIndex = fOwner.Editors.IndexOf(fToReplace);
                fOwner.Editors.RemoveAt(fIndex);

                    // we remove here, cause it's the UI thread, can't do this in background thread for wPF + need to be done before reloading.
                fAllEditors = Enumerable.ToList(BrainData.Current.Editors.AllTextPatternEditors());

                    // do after removing, so that the list doesn't contain the topic to replace
                BrainData.Current.OpenDocuments.Remove(fToReplace);

                    // we make certain that the UI is unloaded cause we are going to do changes from another thread, which don't need to colide with the UI
                fProcessTracker = Search.ProcessTracker.Default.InitProcess();
                LoadFileSizes(fProcessTracker);
                System.Action iImportMod = InternalReplace;
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>
        ///     Imports the topic.
        /// </summary>
        private void InternalReplace()
        {
            var iName = fToReplace.Name;
            try
            {
                var iStillToResolve = new System.Collections.Generic.List<ToResolve>();
                var iErrors = new System.Collections.Generic.List<ParseErrors>();
                Parsers.ParserBase.BlockLogErrors = true; // so we only report the errors at the end when we want it.
                try
                {
                    UnloadProject();
                    try
                    {
                        fToReplace.DeleteEditor(); // unload the editor.
                        LoadTopics(iErrors, iStillToResolve);
                    }
                    finally
                    {
                        ReloadProject();
                    }
                }
                finally
                {
                    Parsers.ParserBase.BlockLogErrors = false;
                    ReloadProject();
                }

                ResolveProblems(iErrors, iStillToResolve);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalImportUI));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "Replace topic", 
                    string.Format(
                        "failed to replace topic {0}: {1}\nThe topic that needed to be replaced could be missing, it's best to save the project to a new location and check for missing data.", 
                        iName, 
                        e));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }
        }

        /// <summary>
        ///     the final function to call when not all things went ok.
        /// </summary>
        protected override void End()
        {
            base.End();
            System.Windows.MessageBox.Show(
                "Something went wrong during the operation, check the log for more info.", 
                "Replace", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }

        /// <summary>The load topics.</summary>
        /// <param name="errors">The errors.</param>
        /// <param name="stillToResolve">The still to resolve.</param>
        private void LoadTopics(System.Collections.Generic.List<ParseErrors> errors, System.Collections.Generic.List<ToResolve> stillToResolve)
        {
            fProcessTracker.IsRunning = true;
            foreach (var iFile in fFiles)
            {
                TextPatternEditor iRes = null;
                try
                {
                    try
                    {
                        var iErr = new ParseErrors();
                        iErr.FileName = iFile;
                        errors.Add(iErr);
                        TopicXmlStreamer.Import(iFile, iErr.Errors, stillToResolve, ref iRes, fProcessTracker);
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "Replace topic", 
                            string.Format(
                                "An error occured while trying to imort the topic '{0}', with the error: {1}", 
                                iFile, 
                                e.Message));
                        fProblems = true;
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

            fProcessTracker.IsRunning = false;
        }

        /// <summary>The reload project.</summary>
        private void ReloadProject()
        {
            var iCount = 0;
            foreach (var i in fAllEditors)
            {
                i.IsParsed = fPrevValues[iCount++];
            }
        }

        /// <summary>The unload project.</summary>
        private void UnloadProject()
        {
            foreach (var i in fAllEditors)
            {
                fPrevValues.Add(i.IsParsed);
                i.IsParsed = false;
            }
        }

        /// <summary>
        ///     Performs the UI section of the import.
        /// </summary>
        private void InternalImportUI()
        {
            if (fOwner != null && fIndex > -1)
            {
                foreach (var i in fImported)
                {
                    fOwner.Editors.Insert(fIndex++, i);
                }
            }

            EndOk();
            if (fProblems)
            {
                if (fFiles.Length > 1)
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while replacing the file. Please check the log for more info.", 
                        "Replace", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "There were problems while replacing the file. Please check the log for more info.", 
                        "Replace", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
            }
        }
    }
}