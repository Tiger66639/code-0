// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectOperation.cs" company="">
//   
// </copyright>
// <summary>
//   base class for all helper classes that perform a large action on the project, which is performed (partly) async.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     base class for all helper classes that perform a large action on the project, which is performed (partly) async.
    /// </summary>
    public class ProjectOperation
    {
        #region IsInOperation

        /// <summary>
        ///     Gets the value that indicates if the system is currently doing a large operation (ui is locked).
        ///     This is used to check if the app can be closed or not.
        /// </summary>
        public static bool IsInOperation { get; internal set; }

        #endregion

        /// <summary>The window_ preview can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Window_PreviewCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>Checks if the current project needs to be saved first and also performs this.
        ///     When the project gets saved, the event handler will be called upon completion (so that the caller process can
        ///     continue processing).
        ///     In this case, a true is returned so that the caller can terminate it's current function.
        ///     when the user pressed cancel, 'true' is also returned but the event handler is never called, so the process
        ///     terminates.</summary>
        /// <param name="eventHandler"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected static bool CheckSaveProject(System.EventHandler eventHandler)
        {
            if (ProjectManager.Default.ProjectChanged)
            {
                System.Windows.MessageBoxResult iSaveRes;
                if (ProjectManager.Default.AutoSave == false)
                {
                    // an autosaved project always needs to be saved, otherwise the database can get corrupted.
                    iSaveRes = System.Windows.MessageBox.Show(
                        "The project has changed, perform a save first?", 
                        "Project changed", 
                        System.Windows.MessageBoxButton.YesNoCancel);
                }
                else
                {
                    iSaveRes = System.Windows.MessageBoxResult.Yes;
                    LogService.Log.LogInfo("Open project", "Auto saving project.");
                }

                if (iSaveRes == System.Windows.MessageBoxResult.Yes)
                {
                    var iSaver = new ProjectSaver();
                    iSaver.EndedOk += eventHandler;
                    iSaver.Save();
                    return true;
                }

                if (iSaveRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>moves the data of the current project to the specified path (datapath for db) and file (for xml)
        ///     After the call, the database will point to the new location.</summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        protected void MoveData(string path, string file)
        {
            var iStorage = Brain.Current.Storage;
            System.Diagnostics.Debug.Assert(iStorage != null);
            ProjectManager.PreparePathForProject(path);
            BrainData.Current.NeuronInfo.Store.Dispose(); // this closes all the files
            iStorage.Dispose(); // closes all the files
            var iDataPath = StorageHelper.GetDataPath(path);
            try
            {
                System.IO.File.Copy(ProjectManager.Default.DesignerFile, file, true);

                    // also need to copy over the actual designer file as well
                CopyDesignerData(path);
                CopyModules(path);
                iStorage.CopyTo(iDataPath);
            }
            finally
            {
                iStorage.DataPath = iDataPath; // new path, opens the files also
                Brain.Current.Modules.Path = StorageHelper.GetModulesPath(path);
                ProjectManager.Default.Location = path;
                ProjectManager.Default.DesignerFile = file;
                iDataPath = System.IO.Path.Combine(path, NeuronDataDictionary.DATAPATH);
                BrainData.Current.NeuronInfo.Store.LoadFiles(iDataPath);
            }
        }

        /// <summary>copies all the data files of the current project to the specified locations, but re-opens the database
        ///     at the previous location.</summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        protected void CopyData(string path, string file)
        {
            var iStorage = Brain.Current.Storage;
            System.Diagnostics.Debug.Assert(iStorage != null);
            ProjectManager.PreparePathForProject(path);
            BrainData.Current.NeuronInfo.Store.Dispose(); // this closes all the files
            iStorage.Dispose(); // closes all the files
            var iPrevDataPath = iStorage.DataPath;
            var iPrevModulePath = Brain.Current.Modules.Path;
            var iDataPath = StorageHelper.GetDataPath(path);
            var iProjLoc = ProjectManager.Default.Location;
            try
            {
                System.IO.File.Copy(ProjectManager.Default.DesignerFile, file, true);

                    // also need to copy over the actual designer file as well
                System.IO.File.Copy(
                    System.IO.Path.Combine(iProjLoc, ProjectManager.NETWORKFILE), 
                    System.IO.Path.Combine(path, ProjectManager.NETWORKFILE));
                CopyDesignerData(path);
                CopyModules(path);
                iStorage.CopyTo(iDataPath);
            }
            finally
            {
                iStorage.DataPath = iPrevDataPath; // new path, opens the files also
                Brain.Current.Modules.Path = iPrevModulePath;
                iDataPath = System.IO.Path.Combine(iProjLoc, NeuronDataDictionary.DATAPATH);

                    // reload the files again after done.
                BrainData.Current.NeuronInfo.Store.LoadFiles(iDataPath);
            }
        }

        /// <summary>Copies all the descriptions of the project to a new directory.
        ///     Warning: this function doesn't close or open the backing files, this needs to be done before and after this call.</summary>
        /// <param name="to">The to.</param>
        protected void CopyDesignerData(string to)
        {
            var iOrLoc = System.IO.Path.Combine(ProjectManager.Default.Location, NeuronDataDictionary.DATAPATH);
            if (System.IO.Directory.Exists(iOrLoc))
            {
                var iPath = System.IO.Path.Combine(to, NeuronDataDictionary.DATAPATH);

                    // we don't need to create this dir, it is already done by the PreparePathForProject
                foreach (var iFile in System.IO.Directory.GetFiles(iOrLoc))
                {
                    System.IO.File.Copy(iFile, System.IO.Path.Combine(iPath, System.IO.Path.GetFileName(iFile)));
                }
            }
        }

        /// <summary>Copies all the descriptions of the project to a new directory.</summary>
        /// <param name="to">The to.</param>
        protected void CopyModules(string to)
        {
            // Brain.Current.Modules.CloseFiles();
            var iOrLoc = Brain.Current.Modules.Path;
            if (System.IO.Directory.Exists(iOrLoc))
            {
                var iPath = StorageHelper.GetModulesPath(to);
                foreach (var iFile in System.IO.Directory.GetFiles(iOrLoc))
                {
                    System.IO.File.Copy(iFile, System.IO.Path.Combine(iPath, System.IO.Path.GetFileName(iFile)));
                }
            }
        }

        /// <summary>Saves all the extra data of the designer file that is not stored in xml.</summary>
        /// <param name="file">The file that contains the data that needs to be saved.</param>
        /// <param name="path">The rootpath of the project.</param>
        protected void SaveDesignerData(DesignerDataFile file, string path)
        {
            var iPath = System.IO.Path.Combine(path, NeuronDataDictionary.DATAPATH);
            file.NeuronInfo.SaveNeuronData(iPath);
            file.Thesaurus.SaveData(iPath);
        }

        #region Fields

        /// <summary>The i prev.</summary>
        private System.Windows.Input.Cursor iPrev;

        /// <summary>The f handler.</summary>
        private System.Windows.Input.CanExecuteRoutedEventHandler fHandler;

        /// <summary>The f command bindings.</summary>
        private System.Collections.Generic.List<System.Windows.Input.CommandBinding> fCommandBindings;

        /// <summary>Initializes static members of the <see cref="ProjectOperation"/> class.</summary>
        static ProjectOperation()
        {
            IsInOperation = false;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the operation ended ok.
        /// </summary>
        public event System.EventHandler EndedOk;

        /// <summary>
        ///     occurs when the operation failed.
        /// </summary>
        public event System.EventHandler EndedNok;

        #endregion

        #region functions

        /// <summary>The disable ui.</summary>
        protected virtual void DisableUI()
        {
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // during project management operations, we block any changes to the undo system, cause they are invalid (it's a new, fresh project, it's a loaded project, it's copied,...).
            iPrev = System.Windows.Input.Mouse.OverrideCursor;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            System.Windows.Application.Current.MainWindow.IsHitTestVisible = false;

                // make certain that the user doesn't do anything with the UI during the load. 
            MakeGray();
            fHandler = Window_PreviewCanExecute;
            System.Windows.Application.Current.MainWindow.AddHandler(
                System.Windows.Input.CommandManager.PreviewCanExecuteEvent, 
                fHandler);
            fCommandBindings =
                new System.Collections.Generic.List<System.Windows.Input.CommandBinding>(
                    from System.Windows.Input.CommandBinding i in
                        System.Windows.Application.Current.MainWindow.CommandBindings
                    select i);
            System.Windows.Application.Current.MainWindow.CommandBindings.Clear();
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

                // we make certain al commands are re evaluated.
        }

        /// <summary>The make gray.</summary>
        private void MakeGray()
        {
            IsInOperation = true;
            var iEffect = new GrayscaleEffect.GrayscaleEffect();

                // we use the GrayscaleEffect lib and not the learnWPF.Effects lib cause the latter has a problem creating the effect when in sandbox mode, don't know why ,but this doesn't.
            WindowMain.Current.MainDock.Effect = iEffect;
            foreach (var i in WindowMain.Current.MainDock.FloatingWindows)
            {
                i.Effect = iEffect;
            }
        }

        /// <summary>The ungray.</summary>
        private void Ungray()
        {
            IsInOperation = false;
            WindowMain.Current.MainDock.Effect = null;
            foreach (var i in WindowMain.Current.MainDock.FloatingWindows)
            {
                i.Effect = null;
            }
        }

        /// <summary>
        ///     The final function to call when the whole process succeeded. It calls
        ///     the <see cref="ProjectOperation.EndedOk" /> event.
        /// </summary>
        protected virtual void EndOk()
        {
            try
            {
                PerformOnEnd();
            }
            finally
            {
                if (EndedOk != null)
                {
                    EndedOk(this, System.EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to put common code that always needs to be
        ///     called
        ///     at the end. When you override this function, don't forget to call the base.s
        /// </summary>
        protected virtual void PerformOnEnd()
        {
            EnableUI();
        }

        /// <summary>
        ///     the final function to call when not all things went ok.
        /// </summary>
        protected virtual void End()
        {
            try
            {
                PerformOnEnd();
            }
            finally
            {
                if (EndedNok != null)
                {
                    EndedNok(this, System.EventArgs.Empty);
                }
            }
        }

        /// <summary>The enable ui.</summary>
        private void EnableUI()
        {
            if (WindowMain.UndoStore.UndoStateStack.Count > 0)
            {
                // if this stack is not set, the ui hasn't been disabled yet and so don't try to enable again.
                WindowMain.UndoStore.UndoStateStack.Pop();
                System.Windows.Application.Current.MainWindow.IsHitTestVisible = true;
                System.Windows.Input.Mouse.OverrideCursor = iPrev;
                System.Windows.Application.Current.MainWindow.RemoveHandler(
                    System.Windows.Input.CommandManager.PreviewCanExecuteEvent, 
                    fHandler);
                System.Windows.Application.Current.MainWindow.CommandBindings.AddRange(fCommandBindings);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();

                    // we make certain al commands are re evaluated so that they are activated again.
                Ungray();
            }
        }

        #endregion
    }
}