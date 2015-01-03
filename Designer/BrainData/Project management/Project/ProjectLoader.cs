// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectLoader.cs" company="">
//   
// </copyright>
// <summary>
//   A helper class to load the project partly async, allowing screen updates.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A helper class to load the project partly async, allowing screen updates.
    /// </summary>
    internal class ProjectLoader : ProjectOperation
    {
        /// <summary>
        ///     The sub path that is appended to the project path
        /// </summary>
        public const string DESCRIPTIONPATH = "Descriptions";

        /// <summary>The f designer file.</summary>
        private string fDesignerFile;

        /// <summary>The f path.</summary>
        private string fPath;

        /// <summary>The f result.</summary>
        private DesignerDataFile fResult;

        /// <summary>
        ///     Shows a diolg box to open a project.
        /// </summary>
        internal void Open()
        {
            var iDlg = GetOpenDlg();
            if (string.IsNullOrEmpty(fDesignerFile) == false)
            {
                iDlg.FileName = fDesignerFile;
            }

            if (iDlg.ShowDialog() == true)
            {
                Open(iDlg.FileName);
            }
        }

        /// <summary>build  an open Dialog.</summary>
        /// <returns>The <see cref="OpenFileDialog"/>.</returns>
        public static Microsoft.Win32.OpenFileDialog GetOpenDlg()
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Multiselect = false;
            iDlg.Filter = ProjectManager.FILE_DIALOG_FILTER;
            iDlg.FilterIndex = 0;
            iDlg.AddExtension = true;
            iDlg.CheckFileExists = true;
            iDlg.DefaultExt = ProjectManager.PROJECT_EXTENTION;
            return iDlg;
        }

        /// <summary>Opens the project in the specified path, after checking the state of the current.</summary>
        /// <param name="path">The path.</param>
        /// <remarks>Checks the state of the project before opening a new project to see if it is allowed, asking the user for the
        ///     appropriate action.
        ///     Note that the current project can get saved during this operation.</remarks>
        internal void Open(string path)
        {
            if (ProcessorManager.Current.HasProcessors)
            {
                var iMbRes =
                    System.Windows.MessageBox.Show(
                        "There are still processors running, opening a new project will stop them, continue?", 
                        "Open project", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Warning);
                if (iMbRes == System.Windows.MessageBoxResult.No)
                {
                    return;
                }
            }

            fDesignerFile = path;
            fPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(path), 
                System.IO.Path.GetFileNameWithoutExtension(path)); // easiest way to pass it along to the eventhandler.
            if (CheckSaveProject(OpenSaver_EndedOk))
            {
                return;
            }

            ProjectManager.Default.DataError = false; // need to reset the data errors when we open another project.
            Start();
        }

        /// <summary>The open saver_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OpenSaver_EndedOk(object sender, System.EventArgs e)
        {
            ProjectManager.Default.DataError = false; // need to reset the data errors when we open another project.
            Start();
        }

        /// <summary>
        ///     Starts the operation without performing any checking (if the previous project was not yet loaded,
        ///     it is lost). Other entry point: <see cref="ProjectLoader.Open" />
        /// </summary>
        /// <param name="from">From.</param>
        public void Start()
        {
            DisableUI();
            try
            {
                ProjectManager.Default.ClearProject();
                if (string.IsNullOrEmpty(fPath) == false)
                {
                    ProjectManager.LoadBrain(fPath);
                    if (BrainUndoMonitor.IsInstantiated == false)
                    {
                        // we need to make certain the the brain undo monitor registers it's
                        BrainUndoMonitor.Instantiate();
                    }

                    System.Action iLoadDesigner = LoadDesigner;
                    iLoadDesigner.BeginInvoke(null, null);
                }
                else
                {
                    LogService.Log.LogError("ProjectManager.LoadProject", "Path to load is not defined.");
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectManager.LoadProject", e.ToString());
                ProjectManager.Default.DataError = true;
                End();
            }
        }

        /// <summary>The perform on end.</summary>
        protected override void PerformOnEnd()
        {
            base.PerformOnEnd();
            try
            {
                BrainData.Current.OnLoaded();

                    // we always need to call this event, even if the load didn't succeed properly, we need to let the other parts of the system know that stuff has been reloaded.
            }
            catch
            {
                // don't do anything with the error when something went wrong in this case, it was because of a pervious error, which was already logged.
            }
        }

        /// <summary>The end.</summary>
        protected override void End()
        {
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // When 'end' is called something went wrong,  need to show the user what went wrong.
            base.End();
        }

        /// <summary>
        ///     Loads the designer. Done in other thread, takes long time to load all neuronInfo.
        /// </summary>
        private void LoadDesigner()
        {
            try
            {
                if (System.IO.File.Exists(fDesignerFile))
                {
                    using (
                        var iFile = new System.IO.FileStream(
                            fDesignerFile, 
                            System.IO.FileMode.Open, 
                            System.IO.FileAccess.Read))
                    {
                        var iSer = new System.Xml.Serialization.XmlSerializer(typeof(DesignerDataFile));
                        iSer.UnknownNode += ProjectManager.serializer_UnknownNode;
                        fResult = (DesignerDataFile)iSer.Deserialize(iFile);

                            // assigning the result must be done in the ui thread because it sets some UI items.
                        var iPath = System.IO.Path.Combine(fPath, NeuronDataDictionary.DATAPATH);
                        if (fResult.Thesaurus.Data.Count == 0)
                        {
                            // old code had thesaurus data inlined in the project xml file and didn't need a second load. The new format stores this data externally and needs to load it seperatly.
                            fResult.Thesaurus.LoadData(iPath);

                                // we need to load this data seperatly, since it is stored in the designer subdir as a binary file, so the xmlreader doesn't yet know where to store it (projectpath only gets sets after complete succesfull operation).
                        }

                        fResult.NeuronInfo.Store.LoadFiles(iPath); // same for the neurondata.s
                    }
                }
                else
                {
                    LogService.Log.LogWarning(
                        "ProjectManager.LoadDesigner", 
                        string.Format(
                            "{0} designer data file not found, all designer data will be lost!", 
                            fDesignerFile));
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(LoadNewBrain));
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "ProjectManager.LoadProject", 
                    string.Format("Failed to load project '{0}': {1}.", fPath, e.Message));
                ProjectManager.Default.DataError = true;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action(ContinueAfterLoadDesigner));
        }

        /// <summary>
        ///     Continues the load after the designer has been read from mem. Called from UI thread.
        /// </summary>
        private void ContinueAfterLoadDesigner()
        {
            ProjectManager.Default.Location = fPath;
            ProjectManager.Default.DesignerFile = fDesignerFile;
            BrainData.Current.DesignerData = fResult;
            BrainData.Current.LoadAttachedTopics(fPath);

                // has to be done after assigning the designer file, otherwise we can't get to the neuronInfo data.
            ProjectManager.Default.AddLastOpened(fDesignerFile);
            if (ProjectManager.Default.DataError == false)
            {
                LogService.Log.LogInfo(
                    "ProjectManager.LoadProject", 
                    string.Format("Project '{0}' succesfully loaded.", fDesignerFile));
                EndOk();
                ConvertProjectIfNeeded();
            }
            else if (BrainData.Current != null && BrainData.Current.DesignerData != null
                     && BrainData.Current.DesignerData.TemplateVersion != ProjectManager.PROJECT_TEMPLATE_VER)
            {
                // if the project needs to be upgraded, try to do an upgrade. This is important: when the user installed a new version that needs an upgrade for existing projects, the odds are that existing projects produce errors, so first try an upgrade.
                EndOk();
                ConvertProjectIfNeeded();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "There were errors while opening the project, please see the log for more info.", 
                    "Load project", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                End(); // call the end when the designer data has been loaded
            }
        }

        /// <summary>
        ///     Checks the template version + if the project is from an older version, asks the user if it needs
        ///     to be converted. Done in UI thread, cause we need to ask the user if he wants to convert.
        /// </summary>
        internal static void ConvertProjectIfNeeded()
        {
            if (BrainData.Current.DesignerData.TemplateVersion != ProjectManager.PROJECT_TEMPLATE_VER)
            {
                var iRes =
                    System.Windows.MessageBox.Show(
                        "This project is using an old template, would you like to convert it now?", 
                        "Load project", 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Question, 
                        System.Windows.MessageBoxResult.Yes);
                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    var iConverter = new ProjectConverter();
                    iConverter.EndedOk += Convert_EndedOk;
                    iConverter.Convert();
                }
                else
                {
                    iRes =
                        System.Windows.MessageBox.Show(
                            "Touch the project so that the project opens normally next time?", 
                            "Load project", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Question, 
                            System.Windows.MessageBoxResult.Yes);
                    if (iRes == System.Windows.MessageBoxResult.Yes)
                    {
                        BrainData.Current.DesignerData.TemplateVersion = ProjectManager.PROJECT_TEMPLATE_VER;
                    }
                }
            }
        }

        /// <summary>Handles the EndedOk event of the DescImporter control. When all the data has been converted and
        ///     descriptions imported, we need to save the project again</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void Convert_EndedOk(object sender, System.EventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Convertion completed succesfully", 
                "Convert project", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
        }

        /// <summary>The load new brain.</summary>
        private void LoadNewBrain()
        {
            BrainData.New();
            End();
        }
    }
}