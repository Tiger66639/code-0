// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusFileImporter.cs" company="">
//   
// </copyright>
// <summary>
//   used to import thesaurus data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     used to import thesaurus data.
    /// </summary>
    internal class ThesaurusFileImporter : ProjectOperation
    {
        /// <summary>The f files.</summary>
        private string[] fFiles; // this list of files that need to be imported.

        /// <summary>The f import into.</summary>
        private ThesaurusItem fImportInto; // we can import into a

        /// <summary>The f large data.</summary>
        private bool fLargeData;

                     // very large files need to be imported differently (auto streaming + no undo), otherwise we get mem crash.

        /// <summary>The f problems.</summary>
        private bool fProblems; // flag to keep track if there were problems.

        // NeuronStorageMode fPrevMode;
        /// <summary>The f process tracker.</summary>
        private Search.ProcessTrackerItem fProcessTracker;

                                          // so we can keep the user informed of the current position and allow for cancel.

        /// <summary>The f relationship.</summary>
        private Neuron fRelationship;

        /// <summary>
        ///     Gets or sets the currently running topic importer. So we can keep
        ///     track if is is already running or not.
        /// </summary>
        /// <value>
        ///     The importer.
        /// </value>
        public static ThesaurusFileImporter Importer { get; set; }

        /// <summary>Imports files into the specified thesaurus item for the specified
        ///     relationship. When both are null, a full thesaurus is imported,
        ///     otherwise only a thesaurus section.</summary>
        /// <param name="importInto">The import into.</param>
        /// <param name="relationship">The relationship.</param>
        public void Import(ThesaurusItem importInto, Neuron relationship)
        {
            if (Importer != null)
            {
                throw new System.InvalidOperationException("Can only perform 1 import operation at a time.");
            }

            var iDlg = new Microsoft.Win32.OpenFileDialog();
            if (relationship != null)
            {
                iDlg.Filter = Properties.Resources.ThesaurusSectionFileFilter + Properties.Resources.ThesaurusFileFilter;
                iDlg.FilterIndex = 2;
            }
            else
            {
                iDlg.Filter = Properties.Resources.ThesaurusFileFilter;
                iDlg.FilterIndex = 1;
            }

            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                if (iDlg.FileName.EndsWith("thesaurus.xml"))
                {
                    // if it's a full thesaurus file that was selected, make certain we don't try to add to an item.
                    fImportInto = null;
                }
                else
                {
                    fImportInto = importInto;
                }

                fRelationship = relationship;
                fFiles = iDlg.FileNames;
                fProcessTracker = Search.ProcessTracker.Default.InitProcess();
                if (LoadFileSizes())
                {
                    // if the user asked to stop the operation, quit it now.
                    return;
                }

                Importer = this;

                    // needs to be done after LoadFileSizes, cause we can quit at that stage, don't want to cause a lock here.
                DisableUI();
                if (fLargeData == false)
                {
                    WindowMain.UndoStore.BeginUndoGroup(); // begin an undo group in the main UI, just to be save.
                }
                else
                {
                    WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                        // block undo data, so that no data gets recorded, otherwise mem overload
                }

                if (fImportInto != null)
                {
                    InternalImport(); // do sync import, otherwise lots of problems with thesaurus (treeview) syncing up.
                }
                else
                {
                    System.Action iImportMod = InternalImport;

                        // can be a big file, best async, should be less problems, cause we are importing from the root
                    iImportMod.BeginInvoke(null, null);
                }
            }
        }

        /// <summary>Checks the size of the files that need to be imported, to make certain
        ///     we don't blow out mem, when a file is to big, auto saving is turned on
        ///     until all has been imported.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool LoadFileSizes()
        {
            var iSizeOk = true;
            fProcessTracker.TotalCount = 0;
            foreach (var iFile in fFiles)
            {
                var iInfo = new System.IO.FileInfo(iFile);
                fProcessTracker.TotalCount += iInfo.Length;
                if (iSizeOk && fProcessTracker.TotalCount > 1024 * 1024)
                {
                    // 1 meg is the limit: 1 byte * 1024 = 1kb * 1024 = 1meg
                    iSizeOk = false;
                }
            }

            if (iSizeOk == false)
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        "Undo is not available for importing large dataset. Do you want to continue?", 
                        "Large data files", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Warning);
                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    fLargeData = true;
                }
                else
                {
                    fProcessTracker.TotalCount = 0;

                        // reset the counter if we cancel the operation otherwise the total count will get screwed up.
                    return true;
                }
            }

            return false;
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
            try
            {
                BrainData.Current.Thesaurus.IsBuildingList = true;

                    // so the ui doens't continuasly update while loading, this is faster.
                fProcessTracker.IsRunning = true;
                foreach (var iFile in fFiles)
                {
                    if (fProcessTracker.IsCanceled)
                    {
                        break;
                    }

                    try
                    {
                        ThesaurusXmlStreamer.Import(iFile, fImportInto, fRelationship, fProcessTracker);
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "Import thesaurus file", 
                            string.Format(
                                "An error occured while trying to imort the thesaurus-file '{0}', with the message: {1}", 
                                iFile, 
                                e.Message));
                        fProblems = true;
                    }
                }
            }
            finally
            {
                fProcessTracker.IsRunning = false;
                fProcessTracker.TotalCount = 0;

                    // reset, otherwise the count remains up, giving errors in the process-tracker for next items.
                BrainData.Current.Thesaurus.IsBuildingList = false;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to
        ///     put common code that always needs to be called at the end. When you
        ///     <see langword="override" /> this function, don't forget to call the
        ///     base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            // if (fImportInto != null)
            // fImportInto.IsSelected = true; 
            Importer = null;
            if (fLargeData == false)
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
            else
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }

            // Settings.StorageMode = fPrevMode;
            BrainData.Current.LearnCount--; // also need to disable learning.
            base.PerformOnEnd();
            if (fProblems)
            {
                if (fFiles.Length > 0)
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
    }
}