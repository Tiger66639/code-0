// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectManager.cs" company="">
//   
// </copyright>
// <summary>
//   A helper class that manages the both the designer and the brain data in regards to loading and saving.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A helper class that manages the both the designer and the brain data in regards to loading and saving.
    /// </summary>
    public class ProjectManager : Data.ObservableObject
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProjectManager" /> class.
        /// </summary>
        public ProjectManager()
        {
            Brain.Current.Changed += Project_Changed;
            ThreadManager.Default.ActivityStarted += Brain_ActivityStarted;
            BrainData.Current.AfterLoad += ProjectData_AfterLoad;
            BrainData.Current.BeforeSave += ProjectData_BeforeSave;
            SandboxLocation = Properties.Settings.Default.SandboxPath;
            if (string.IsNullOrEmpty(SandboxLocation))
            {
                var iPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NND");
                if (System.IO.Directory.Exists(iPath) == false)
                {
                    System.IO.Directory.CreateDirectory(iPath);
                }

                iPath = System.IO.Path.Combine(iPath, "Sandbox");
                if (System.IO.Directory.Exists(iPath) == false)
                {
                    System.IO.Directory.CreateDirectory(iPath);
                }

                SandboxLocation = iPath; // init the sandbox path to something usefull
            }

            LastOpened = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (Properties.Settings.Default.LastOpened != null)
            {
                foreach (var i in Properties.Settings.Default.LastOpened)
                {
                    LastOpened.Add(i);
                }
            }
        }

        #endregion

        /// <summary>
        ///     Updates the project's paths to the correct location in case we were started as sandbox.
        /// </summary>
        internal void UpdateProjectForSandbox()
        {
            if (IsSandBox)
            {
                Brain.Current.Storage.DataPath = StorageHelper.GetDataPath(Location);

                    // we must update the data path of the project cause we couldn't do this during the save while building the sandbox (the save would have put some files in the wrong place, which broke the copy).
                Brain.Current.Modules.Path = StorageHelper.GetModulesPath(Location);
            }
        }

        #region Fields

        /// <summary>The maxfilecount.</summary>
        private const int MAXFILECOUNT = 10;

        /// <summary>
        ///     The name of the module file containing all the designer data.
        /// </summary>
        public const string MODULEDESIGNERFILE = "BrainModule.xml";

        /// <summary>
        ///     The name of the project file containing info about the network itself.
        /// </summary>
        public const string NETWORKFILE = "Brain.xml";

        /// <summary>
        ///     The name of the app when it is the basic build, as it is displayed in the title bar.
        /// </summary>
        public const string SHORTAPPNAMEBASIC = "Chatbot designer";

        /// <summary>
        ///     The name of the app, as it is displayed in the title bar.
        /// </summary>
        public const string SHORTAPPNAME = "NND";

        /// <summary>
        ///     gets or sets the string to use in the file open and save dialogs.
        /// </summary>
        public const string FILE_DIALOG_FILTER = "Designer project (*.dpl)|*.dpl*|All Files|*.*";

        /// <summary>The projec t_ extention.</summary>
        public const string PROJECT_EXTENTION = "dpl";

        /// <summary>The topi c_ extention.</summary>
        public const string TOPIC_EXTENTION = "topic.xml";

        /// <summary>The projec t_ templat e_ ver.</summary>
        public const string PROJECT_TEMPLATE_VER = "1.4";

        /// <summary>The f default.</summary>
        private static ProjectManager fDefault;

        /// <summary>The f location.</summary>
        private static string fLocation;

        /// <summary>
        ///     The name of the project file containing all the designer data.
        /// </summary>
        private string fDesignerFile;

        /// <summary>The f sandbox location.</summary>
        private string fSandboxLocation;

        /// <summary>The f sandbox running.</summary>
        private bool fSandboxRunning;

        /// <summary>The f is sand box.</summary>
        private bool fIsSandBox;

        /// <summary>The f is viewer visibility.</summary>
        private System.Windows.Visibility fIsViewerVisibility;

        /// <summary>The f data error.</summary>
        private bool fDataError;

        /// <summary>The f project changed.</summary>
        private bool fProjectChanged; // for data changes that don't effect the undo system but which need to be saved.

        /// <summary>The f auto save.</summary>
        private bool fAutoSave;

        #endregion

        #region Prop

        #region ProjectChanged

        /// <summary>
        ///     Gets if the project has been changed since the last save.
        /// </summary>
        public bool ProjectChanged
        {
            get
            {
                return Brain.Current.IsChanged || BrainData.Current.IsChanged || fProjectChanged;
            }

            internal set
            {
                if (value != fProjectChanged)
                {
                    fProjectChanged = value;
                    OnPropertyChanged("ProjectChanged");
                }
                else if (value == false)
                {
                    // when resetting, sometimes 'fprojectChanged was still false.
                    OnPropertyChanged("ProjectChanged");
                }
            }
        }

        #endregion

        #region LastOpened

        /// <summary>
        ///     Gets the list of last opened project paths.
        /// </summary>
        /// <value>The last opened.</value>
        public System.Collections.ObjectModel.ObservableCollection<string> LastOpened { get; private set; }

        #endregion

        #region Location

        /// <summary>
        ///     Gets/sets the location of the currently opened project.
        /// </summary>
        /// <remarks>
        ///     This needs to be a local prop (not static) cause the UI needs to gets warned when it changes so that it can display
        ///     this. And the
        ///     UI requires a normal prop.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public string Location
        {
            get
            {
                return fLocation;
            }

            internal set
            {
                if (value != fLocation)
                {
                    fLocation = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        /// <summary>
        ///     Sets the main window title to the currently loaded project.
        /// </summary>
        private void SetMainWindowTitle()
        {
            if (string.IsNullOrEmpty(DesignerFile) == false)
            {
                if (IsSandBox == false)
                {
                    System.Windows.Application.Current.MainWindow.Title =
                        System.IO.Path.GetFileNameWithoutExtension(DesignerFile) + " - " + ShortAppName;

                        // update the path so the user can see wich project is loaded from the windowbar.
                }
                else
                {
                    System.Windows.Application.Current.MainWindow.Title = "Sandbox ("
                                                                          + System.IO.Path.GetFileNameWithoutExtension(
                                                                              DesignerFile) + ") - " + ShortAppName;
                }
            }
            else
            {
                System.Windows.Application.Current.MainWindow.Title = ShortAppName;
            }
        }

        #endregion

        #region DesignerFile

        /// <summary>
        ///     Gets/sets the file name and location of the designer file itself, which is seperate from the project data.
        /// </summary>
        public string DesignerFile
        {
            get
            {
                return fDesignerFile;
            }

            set
            {
                if (value != fDesignerFile)
                {
                    fDesignerFile = value;
                    OnPropertyChanged("DesignerFile");
                    if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread
                        && System.Windows.Application.Current.MainWindow != null)
                    {
                        SetMainWindowTitle();
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action(SetMainWindowTitle));
                    }
                }
            }
        }

        #endregion

        #region SandboxLocation

        /// <summary>
        ///     Gets/sets the location to copy projects to that need to be started in a sandbox environment.
        /// </summary>
        public string SandboxLocation
        {
            get
            {
                return fSandboxLocation;
            }

            set
            {
                fSandboxLocation = value;
                OnPropertyChanged("SandboxLocation");
            }
        }

        #endregion

        #region SandboxRunning

        /// <summary>
        ///     Gets the wether the sandbox is running or not.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool SandboxRunning
        {
            get
            {
                return fSandboxRunning;
            }

            internal set
            {
                fSandboxRunning = value;
                OnPropertyChanged("SandboxRunning");
            }
        }

        #endregion

        #region IsSandBox

        /// <summary>
        ///     Gets/sets wether this project instance is a sandbox of another project.
        /// </summary>
        public bool IsSandBox
        {
            get
            {
                return fIsSandBox;
            }

            set
            {
                if (fIsSandBox != value)
                {
                    fIsSandBox = value;
                    OnPropertyChanged("IsSandBox");
                }
            }
        }

        #endregion

        #region IsNotViewer

        /// <summary>
        ///     Gets the a bool indicating if we are currently not running as a viewer(true) or false if we are. This can be used
        ///     to bind against from the UI.
        /// </summary>
        public bool IsNotViewer
        {
            get
            {
                return IsViewerVisibility != System.Windows.Visibility.Collapsed;
            }
        }

        #endregion

        #region IsViewerVisibility

        /// <summary>
        ///     Gets/sets  wether this application should be displayed as a viewer only, so without any editing features.
        /// </summary>
        public System.Windows.Visibility IsViewerVisibility
        {
            get
            {
                return fIsViewerVisibility;
            }

            set
            {
                if (fIsViewerVisibility != value)
                {
                    fIsViewerVisibility = value;
                    OnPropertyChanged("IsViewerVisibility");
                }
            }
        }

        #endregion

        #region Default

        /// <summary>Gets the default.</summary>
        public static ProjectManager Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new ProjectManager();
                }

                return fDefault;
            }
        }

        #endregion

        #region DataError

        /// <summary>
        ///     Gets if the data load function failed.
        /// </summary>
        /// <remarks>
        ///     This is used when saving the data to see if it is ok to save it.  When the data file was not correctly read (and so
        ///     nothing
        ///     was loaded), we don't want to overwrite it, cause than we loose all the info. We also use this to provide a visual
        ///     indication
        ///     about the error.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public bool DataError
        {
            get
            {
                return fDataError;
            }

            internal set
            {
                if (fDataError != value)
                {
                    fDataError = value;
                    OnPropertyChanged("DataError");
                }
            }
        }

        #endregion

        #region AutoSave

        /// <summary>Gets or sets a value indicating whether auto save.</summary>
        public bool AutoSave
        {
            get
            {
                return fAutoSave;
            }

            set
            {
                if (value != fAutoSave)
                {
                    if (string.IsNullOrEmpty(Location) == false)
                    {
                        fAutoSave = value;
                        if (fAutoSave == false)
                        {
                            Settings.StorageMode = NeuronStorageMode.StreamWhenPossible;

                                // only keep in mem what has changed, don't save, but remove from mem what hasn't changed.
                        }
                        else
                        {
                            Settings.StorageMode = NeuronStorageMode.AlwaysStream;

                                // always save when something is changed.
                        }
                    }

                    OnPropertyChanged("AutoSave");
                }
            }
        }

        #endregion

        #region ApplicationName

        /// <summary>
        ///     Gets the name of the application. This allows us to dynamically change this, depending on the  version (basic, pro,
        ///     desiger).
        /// </summary>
        public string ShortAppName
        {
            get
            {
                if (WindowMain.Current.DesignerVisibility == System.Windows.Visibility.Visible)
                {
                    return SHORTAPPNAME;
                }

                return SHORTAPPNAMEBASIC;
            }
        }

        #endregion

        #endregion

        #region Common interface

        /// <summary>
        ///     Creates a new project. If there is a template defined, this is loaded as the new brain.
        /// </summary>
        public void CreateNew()
        {
            if (ProjectChanged)
            {
                System.Windows.MessageBoxResult iRes;
                if (AutoSave == false)
                {
                    // auto saved projects are always saved when a new project is loaded.
                    iRes = System.Windows.MessageBox.Show(
                        "The project has changed, perform a save first?", 
                        "New project", 
                        System.Windows.MessageBoxButton.YesNoCancel);
                }
                else
                {
                    iRes = System.Windows.MessageBoxResult.Yes;
                }

                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    var iSaver = new ProjectSaver();
                    iSaver.EndedOk += CreateNewSaver_EndedOk;
                    iSaver.Save();
                    return; // we must exit here, CreateNewSaver will call CreateOrLoadFromTemplate when it is done.
                }

                if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            CreateOrLoadFromTemplate();
        }

        /// <summary>The create new saver_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CreateNewSaver_EndedOk(object sender, System.EventArgs e)
        {
            CreateOrLoadFromTemplate();
        }

        /// <summary>
        ///     Creates or loada a new project from template.
        /// </summary>
        public void CreateOrLoadFromTemplate()
        {
            CreateOrLoadFromTemplate(Properties.Settings.Default.DefaultTemplatePath);
        }

        /// <summary>Creates or loada a new project from the specified template.</summary>
        /// <param name="templateName">Name of the template.</param>
        public void CreateOrLoadFromTemplate(string templateName)
        {
            DataError = false; // when initially creating a new project, there is no data error.
            var iPrev = System.Windows.Input.Mouse.OverrideCursor;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // during creation of the new project, we block any changes to the undo system, cause they are invalid (it's a new, fresh project).
            var iPrevStorageMode = Settings.StorageMode;
            Settings.StorageMode = NeuronStorageMode.AlwaysInMem;

                // we need to change the settings to keep everything in mem when creating a new project since we don't know the path yet.  If we don't do this, the template neurons get lost.
            try
            {
                try
                {
                    var iLoaded = false;
                    string iFromPath = null;
                    if (string.IsNullOrEmpty(templateName) == false && System.IO.File.Exists(templateName))
                    {
                        try
                        {
                            iFromPath = System.IO.Path.Combine(
                                System.IO.Path.GetDirectoryName(templateName), 
                                System.IO.Path.GetFileNameWithoutExtension(templateName));
                            if (string.IsNullOrEmpty(iFromPath) == false && System.IO.File.Exists(templateName))
                            {
                                // only try to load the template if it really exists.
                                ClearProject();
                                var iSuccess = LoadBrain(iFromPath, true);

                                    // when loading the template, always load as readonly. otherwise we can get a problem when multile instances are trying to start at the same time.
                                if (iSuccess)
                                {
                                    // when created new, because LoadBrain failed, we don't need to touch mem, cause everythign is already in mem.
                                    Brain.Current.TouchMem();
                                }

                                Brain.Current.Storage.DataPath = null;

                                    // there is no place yet to save the braindata, so reset to null.
                                Brain.Current.Modules.Path = null;

                                    // same for the modules, there is no location yet to store everything.
                                if (BrainUndoMonitor.IsInstantiated == false)
                                {
                                    // we need to make certain the the brain undo monitor registers it's
                                    BrainUndoMonitor.Instantiate();
                                }

                                LoadDesigner(templateName, iFromPath, true);

                                    // when loading the template, always load as readonly. otherwise we can get a problem when multile instances are trying to start at the same time.
                                BrainData.Current.TouchMem();

                                    // load the entire designer data in memory, so we can close all files.
                                BrainData.Current.NeuronInfo.Store.Dispose();

                                    // we close the store so that the template becomes completely detached.
                                if (iSuccess)
                                {
                                    // the load brain could have failed, in which case a new one was created. This has already been communicated with the user.
                                    LogService.Log.LogInfo(
                                        "ProjectManager.new", 
                                        string.Format(
                                            "Succesfully created new project from template {0}.", 
                                            templateName));
                                }

                                iLoaded = true;
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogError(
                                "ProjectManager.CreateNew", 
                                "Failed to create new project from template: " + e);
                        }
                    }

                    if (iLoaded == false)
                    {
                        if (string.IsNullOrEmpty(templateName) == false)
                        {
                            LogService.Log.LogWarning(
                                "ProjectManager.new", 
                                string.Format("Failed to find template project: {0}", templateName));
                        }

                        CreateProjectUnsave();
                    }

                    DesignerFile = null;

                        // we  reset the name of the designerfile when we create a new template. This makes certain that the title of the main window is also updated.
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("ProjectManager.CreateNew", e.ToString());
                    DataError = true;
                }
            }
            finally
            {
                BrainData.Current.OnLoaded();
                WindowMain.UndoStore.UndoStateStack.Pop();
                System.Windows.Input.Mouse.OverrideCursor = iPrev;
                Settings.StorageMode = iPrevStorageMode;
            }

            Location = null; // it's a new project, but we used the load to get it, so reset the location         
        }

        /// <summary>Adds path to the list of last opened paths, making certain that when there are no doubles in the list + not to
        ///     many.</summary>
        /// <param name="path">The path.</param>
        internal void AddLastOpened(string path)
        {
            var iIndex = LastOpened.IndexOf(path);
            if (iIndex > -1)
            {
                // already in list, move to front.
                LastOpened.Move(iIndex, 0);
            }
            else
            {
                LastOpened.Insert(0, path);
                if (LastOpened.Count > MAXFILECOUNT)
                {
                    LastOpened.RemoveAt(LastOpened.Count - 1);
                }
            }
        }

        /// <summary>Checks the state of the project before a save is performed to see if it is allowed, asking the user for the
        ///     appropriate action..</summary>
        /// <param name="title">The title.</param>
        /// <returns><c>true</c> if the user asked to stop the save, otherwise <c>false</c>.</returns>
        internal bool CheckProjectStateForSave(string title)
        {
            if (DataError)
            {
                var iMbRes =
                    System.Windows.MessageBox.Show(
                        "There were errors loading the project, saving might loose data, continue?", 
                        title, 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Warning);
                if (iMbRes == System.Windows.MessageBoxResult.No)
                {
                    return true;
                }
            }

            if (ProcessorManager.Current.HasProcessors)
            {
                var iMbRes =
                    System.Windows.MessageBox.Show(
                        "There are still processors running, a save will stop them, continue?", 
                        title, 
                        System.Windows.MessageBoxButton.YesNo, 
                        System.Windows.MessageBoxImage.Warning);
                if (iMbRes == System.Windows.MessageBoxResult.No)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Starts the project in a sandbox for testing.
        /// </summary>
        /// <remarks>
        ///     Before we can start the sandbox, we need save the project.
        /// </remarks>
        public void StartSandBox()
        {
            try
            {
                var iSaver = new ProjectSaver();
                iSaver.EndedOk += StartSandBoxSaver_EndedOk;
                iSaver.Save();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.Message, 
                    "Start sandbox", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>The start sand box saver_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void StartSandBoxSaver_EndedOk(object sender, System.EventArgs e)
        {
            if (System.IO.Directory.Exists(SandboxLocation) == false)
            {
                throw new System.IO.DirectoryNotFoundException("Can't find the sandbox location: " + SandboxLocation);
            }

            var iSandbox = System.IO.Path.Combine(
                SandboxLocation, 
                System.IO.Path.GetFileNameWithoutExtension(DesignerFile));

            PreparePathForProject(iSandbox);
            var iCopier = new ProjectCopier();
            iCopier.EndedOk += StartSandBoxCopier_EndedOk;
            iCopier.Start(SandboxLocation, iSandbox);
        }

        /// <summary>The start sand box copier_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StartSandBoxCopier_EndedOk(object sender, System.EventArgs e)
        {
            var iStartInfo = new System.Diagnostics.ProcessStartInfo();
            iStartInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var iFileName = System.IO.Path.GetFileName(DesignerFile);
            iStartInfo.Arguments = "\"" + System.IO.Path.Combine(SandboxLocation, iFileName) + "\" sandbox";

                // the 'sandbox' arg lets the sub process know it's a sandbox.
            var iStarted = System.Diagnostics.Process.Start(iStartInfo);
            iStarted.EnableRaisingEvents = true; // we need to be warned when the process stops, so we need to set this.
            iStarted.Exited += Sandbox_Exited;
            SandboxRunning = true;
            LogService.Log.LogInfo("ProjectManager.StartSandbox", "Sandbox succesfully started.");
        }

        #endregion

        #region Project changed updating

        /// <summary>
        ///     Cleans the sandbox directory from any data.
        /// </summary>
        /// <remarks>
        ///     Currently simply deletes the temp dir.
        /// </remarks>
        internal void CleanSandbox()
        {
            if (System.IO.Directory.Exists(SandboxLocation))
            {
                // could be that it is non existing, this raises an error.
                if (Brain.Current != null)
                {
                    if (Brain.Current.Storage != null)
                    {
                        Brain.Current.Storage.Dispose(); // need to make certain all the files are closed.
                    }

                    // if (Brain.Current.Modules != null)
                    // Brain.Current.Modules.CloseFiles();
                }

                if (BrainData.Current != null && BrainData.Current.NeuronInfo != null
                    && BrainData.Current.NeuronInfo.Store != null)
                {
                    // getting strange errors here sometimes (don't think its because of threading), anyway, check on it.
                    BrainData.Current.NeuronInfo.Store.Dispose();
                }

                System.IO.Directory.Delete(SandboxLocation, true);

                    // we do a clean before we copy all the data to this dir.
                System.IO.Directory.CreateDirectory(SandboxLocation);

                    // recreate it again so it can be reused later on. If we don't do this, we can get an exception on some systems that there are not enough access rights to create the directory path (due to the root dir, this one, not existing)
            }
        }

        /// <summary>Handles the Exited event of the Sandbox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal void Sandbox_Exited(object sender, System.EventArgs e)
        {
            SandboxRunning = false;
            LogService.Log.LogInfo("ProjectManager.StartSandbox", "Sandbox project has been closed.");
        }

        /// <summary>Handles the BeforeSave event of the BrainData.</summary>
        /// <remarks>The first time that the undo data changes after a project has been saved, we need to update the project status.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ProjectData_BeforeSave(object sender, System.EventArgs e)
        {
            WindowMain.UndoStore.UndoStoreChanged += UndoStore_UndoStoreChanged;
        }

        /// <summary>The undo store_ undo store changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UndoStore_UndoStoreChanged(object sender, UndoSystem.UndoStoreEventArgs e)
        {
            if (e.List == WindowMain.UndoStore.UndoData)
            {
                OnPropertyChanged("ProjectChanged");
                WindowMain.UndoStore.UndoStoreChanged -= UndoStore_UndoStoreChanged;
            }
        }

        /// <summary>Handles the AfterLoad event of the BrainData.</summary>
        /// <remarks>The first time that the undo data changes after a project has been loaded, we need to update the project status.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ProjectData_AfterLoad(object sender, System.EventArgs e)
        {
            WindowMain.UndoStore.UndoStoreChanged += UndoStore_UndoStoreChanged;
        }

        /// <summary>Handles the Changed event of the Brain: when the user changed something, update the project changed state, but not
        ///     when
        ///     the change comes from a processor cause that would be to taxing for the system.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Project_Changed(object sender, System.EventArgs e)
        {
            if (Processor.CurrentProcessor == null)
            {
                // only try to update the project changed state when it is comming from user interaction.  if done, for every change, we tax the UI to much + we can call this at the end of the processor run
                OnPropertyChanged("ProjectChanged"); // don't need to call async, this is taken care of by WPF.
            }
        }

        /// <summary>Handles the ActivityStarted event of the Brain control. When a processor has started, the brain has always been
        ///     changed, so a single
        ///     update is enough.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Brain_ActivityStarted(object sender, System.EventArgs e)
        {
            // Brain.Current.IsEditMode = false;
            OnPropertyChanged("ProjectChanged"); // don't need to call async, this is taken care of by WPF.
        }

        #endregion

        #region Helpers

        /// <summary>
        ///     Creates a new, empty project.
        /// </summary>
        public void CreateProject()
        {
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // during creation of the new project, we block any changes to the undo system, cause they are invalid (it's a new, fresh project).
            try
            {
                try
                {
                    CreateProjectUnsave();
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("ProjectManager.CreateNewProject", e.ToString());
                    DataError = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>The create project unsave.</summary>
        private void CreateProjectUnsave()
        {
            ClearProject();
            Brain.New();
            if (BrainUndoMonitor.IsInstantiated == false)
            {
                // we need to make certain the the brain undo monitor registers it's
                BrainUndoMonitor.Instantiate();
            }

            BrainData.New();
        }

        /// <summary>Loads the brain data and designer data from disk.
        ///     If the new location doesn't contain a brain and/or designer file, a new one is created.</summary>
        /// <remarks>Doesn't check if the previous project needs to be saved, simply clears it.</remarks>
        /// <param name="from">the directory to get the data from.</param>
        public void LoadProjectForSandbox(string from)
        {
            var iPrev = System.Windows.Input.Mouse.OverrideCursor;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // during creation of the new project, we block any changes to the undo system, cause they are invalid (it's a new, fresh project).
            try
            {
                try
                {
                    ClearProject();
                    if (string.IsNullOrEmpty(from) == false)
                    {
                        var iFromPath = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(from), 
                            System.IO.Path.GetFileNameWithoutExtension(from));
                        LoadBrain(iFromPath);
                        if (BrainUndoMonitor.IsInstantiated == false)
                        {
                            // we need to make certain the the brain undo monitor registers it's
                            BrainUndoMonitor.Instantiate();
                        }

                        LoadDesigner(from, iFromPath);
                        Location = iFromPath;
                        UpdateProjectForSandbox();
                        DesignerFile = from;
                        LogService.Log.LogInfo(
                            "ProjectManager.LoadProject", 
                            string.Format("Project '{0}' succesfully loaded.", from));
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Action(ProjectLoader.ConvertProjectIfNeeded), 
                            System.Windows.Threading.DispatcherPriority.Background);

                            // check if the project is of the correct template version, if not, we ask the user to update.
                    }
                    else
                    {
                        LogService.Log.LogError("ProjectManager.LoadProject", "Path to load is not defined.");
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("ProjectManager.LoadProject", e.ToString());
                    DataError = true;
                }
            }
            finally
            {
                BrainData.Current.OnLoaded();

                    // always needs to be called, even if we have created a new one, we need to let the other parts know that it was reloaded or newly created.
                WindowMain.UndoStore.UndoStateStack.Pop();
                System.Windows.Input.Mouse.OverrideCursor = iPrev;
            }
        }

        /// <summary>
        ///     Called when the project was saved by the <see cref="ProjectSaver" />. Performs some tasks that are local to this
        ///     class.
        /// </summary>
        internal void OnProjectSaved()
        {
            BrainData.Current.StoreUndoLocation();
            AddLastOpened(DesignerFile);
            ProjectChanged = false;
        }

        /// <summary>
        ///     Clears all the data from the project.
        /// </summary>
        internal void ClearProject()
        {
            if (Brain.Current.IsInitialized)
            {
                // we only clear when it is initialize, this is important, if we don't do this, the WordnetSin.StopImports will fail when it is not initiliazed since there will be no wordnedtsin.
                ChatLogs.Current.IsVisible = false;

                    // close any chatlogs when the project is closed, this unloads any data.
                ProcessorManager.Current.StopAndUnDeadLock(); // make certain all processors are sopped before we clear
                WindowMain.UndoStore.UndoData.Clear();

                    // make certain there is no undo data available from the previous project.
                WindowMain.UndoStore.RedoData.Clear();
                WordNetSin.Default.StopImports();

                    // when a project is cleared, we also need to make certain that there are no longer auto imports going on, this is done before the brainData and brain are cleared to make certain they are completely empty and didn't get filled again by a still active import after they were cleared
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.APPREFFORMAT))
                {
                    System.Windows.Clipboard.Clear();

                        // the clipboard might contain refs to neurons (ids), those are no longer valid in a new project.
                }

                BrainData.Current.Clear();

                    // we release the designer data before the brain data so that the designer doesn't flip because all neurons are removed.
                Brain.Current.Clear();

                Location = null;

                    // when we clear the project, there should be no location. Otherwise, this causes problems while creating a new project (after a previous, existing one was loaded), for the description text: those load the old data, while creating the new data.
            }

            Search.SearchResults.Default.Items.Clear();

                // make certain that there are no more searchresults visible when the project is cleared, otherwise we can keep some stuff referenced.
        }

        /// <summary>Loads the designer part of the project
        ///     Doesn't raise the 'OnLoaded' event. This has to be done manually.</summary>
        /// <param name="from">From.</param>
        /// <param name="projectPath">The project Path.</param>
        /// <param name="readOnly">The read Only.</param>
        internal static void LoadDesigner(string from, string projectPath, bool readOnly = false)
        {
            if (System.IO.File.Exists(@from))
            {
                using (var iFile = new System.IO.FileStream(from, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    // we make a local memory copy of the xml file cause this loads faster.
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(DesignerDataFile));
                    iSer.UnknownNode += serializer_UnknownNode;
                    var iRes = (DesignerDataFile)iSer.Deserialize(iFile);
                    var iPath = System.IO.Path.Combine(projectPath, NeuronDataDictionary.DATAPATH);
                    if (iRes.Thesaurus.Data.Count == 0)
                    {
                        iRes.Thesaurus.LoadData(iPath);

                            // we need to load this data seperatly, since it is stored in the designer subdir as a binary file, so the xmlreader doesn't yet know where to store it (projectpath only gets sets after complete succesfull operation).
                    }

                    iRes.NeuronInfo.Store.LoadFiles(iPath, readOnly); // same for the neurondata.
                    BrainData.Current.DesignerData = iRes;
                    BrainData.Current.LoadAttachedTopics(projectPath);
                }
            }
            else
            {
                BrainData.New();
                LogService.Log.LogWarning(
                    "ProjectManager.LoadDesigner", 
                    string.Format("{0} designer data file not found, all designer data will be lost!", from));
                Default.DataError = true;
            }
        }

        /// <summary>The serializer_ unknown node.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        internal static void serializer_UnknownNode(object sender, System.Xml.Serialization.XmlNodeEventArgs e)
        {
            // We don't show an error for xsi:type attributes which are used to specify the type.
            if (e.Name != "xsi:type")
            {
                LogService.Log.LogError(
                    "BrainData.Load", 
                    "L: " + e.LineNumber + "P: " + e.LinePosition + ": unknown node in stream: " + e.Name
                    + "/n contains: " + e.Text);
            }
        }

        /// <summary>Loads the brain part of the project</summary>
        /// <param name="from">The location from where to load the data.</param>
        /// <param name="readOnly">The read Only.</param>
        /// <returns>True when the file existed and project was loaded, or false when the file wasn't found and a new
        ///     project was created.</returns>
        internal static bool LoadBrain(string from, bool readOnly = false)
        {
            var iPath = System.IO.Path.Combine(from, NETWORKFILE);
            if (System.IO.File.Exists(iPath))
            {
                Brain.Load(iPath, readOnly);
                return true;
            }

            PreparePathForProject(@from);
            Brain.New();
            AssignPathToProject(@from);
            LogService.Log.LogWarning(
                "ProjectManager.LoadBrain", 
                string.Format("{0} not found, created new brain!", iPath));
            return false;
        }

        /// <summary>
        ///     Saves the settings used by the ProjectManager to the application settings.
        /// </summary>
        internal void SaveSettings()
        {
            if (Properties.Settings.Default.LastOpened != null)
            {
                Properties.Settings.Default.LastOpened.Clear();
            }
            else
            {
                Properties.Settings.Default.LastOpened = new System.Collections.Specialized.StringCollection();
            }

            Properties.Settings.Default.LastOpened.AddRange(Enumerable.ToArray(LastOpened));
            Properties.Settings.Default.SandboxPath = SandboxLocation;
        }

        /// <summary>Prepares the path for the project and returns the data path to use for the new location.</summary>
        /// <remarks>Checks if the location exists + wether it is a sub path called 'data'.  If not, both are created.</remarks>
        /// <param name="loc">The path to prepare for the project.</param>
        internal static void PreparePathForProject(string loc)
        {
            ClearOrCreateDir(loc);
            var iDataPath = StorageHelper.GetDataPath(loc);
            if (System.IO.Directory.Exists(iDataPath))
            {
                // we delete all files in the data path when we prepare a location.  We do this to make certain that any previous data is gone. We don't delete the entire directory, cause then we might brake things like source control, which can put sub directories in the dirs it has checked in.
                StorageHelper.ClearDir(iDataPath);
            }

            if (System.IO.Directory.Exists(iDataPath) == false)
            {
                // try to slow down the creation a bit, seems to fail if it is done to fast after each other.
                System.IO.Directory.CreateDirectory(iDataPath);
            }

            ClearOrCreateDir(System.IO.Path.Combine(loc, NeuronDataDictionary.DATAPATH));
            var iModulesPath = StorageHelper.GetModulesPath(loc);
            ClearOrCreateDir(iModulesPath);
        }

        /// <summary>Assigns the specified path to the project.</summary>
        /// <param name="path">The path.</param>
        internal static void AssignPathToProject(string path)
        {
            var iDataPath = StorageHelper.GetDataPath(path);
            Brain.Current.Storage.DataPath = iDataPath;
            Brain.Current.Modules.Path = StorageHelper.GetModulesPath(path);
        }

        /// <summary>Creates or Clears the specified dir.</summary>
        /// <param name="loc">The loc.</param>
        internal static void ClearOrCreateDir(string loc)
        {
            if (System.IO.Directory.Exists(loc))
            {
                // we delete all files in the data path when we prepare a location.  We do this to make certain that any previous data is gone. We don't delete the entire directory, cause then we might brake things like source control, which can put sub directories in the dirs it has checked in.
                foreach (var iFile in System.IO.Directory.GetFiles(loc))
                {
                    // need to delete all the file, can't simply delete the dir, cause this could break version control systems.
                    System.IO.File.Delete(iFile);
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(loc);
            }
        }

        #endregion

        // used for providing bindings between nnl and designer.
        #region nnl references

        /// <summary>this function can be used by external scripts to add a topic to the project view. (used by AIML import)
        ///     The topic is also registered with the topics-dictionary, so that other patterns can also find it.</summary>
        /// <param name="toAdd"></param>
        /// <param name="name">The name.</param>
        public static void AddTopic(Neuron toAdd, string name)
        {
            if (Parsers.TopicsDictionary.Add(name, toAdd))
            {
                // the topic could be added with the specified name.
                var iNew = new TextPatternEditor(toAdd);
                BrainData.Current.CurrentEditorsList.Add(iNew);
            }
        }

        /// <summary>this function can be used by external scripts to add a topic to the project view. (used by AIML import)
        ///     Searches for a topic with the specified name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron FindTopic(string name)
        {
            return Parsers.TopicsDictionary.Get(name);
        }

        #endregion
    }
}