// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleUpdater.cs" company="">
//   
// </copyright>
// <summary>
//   The module updater.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The module updater.</summary>
    internal class ModuleUpdater : ProjectOperation
    {
        /// <summary>The f has error.</summary>
        private bool fHasError;

        /// <summary>The fmodule.</summary>
        private Module fmodule;

        /// <summary>The update.</summary>
        /// <param name="module">The module.</param>
        internal void Update(Module module)
        {
            fmodule = module;
            DisableUI();
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // block undo data, so that no data gets recorded, otherwise mem overload
            System.Action iImportMod = LoadModule;

                // can be a big file, best async, should be less problems, cause we are importing from the root
            iImportMod.BeginInvoke(null, null);
        }

        /// <summary>The load module.</summary>
        private void LoadModule()
        {
            try
            {
                var iCompiler = new Parsers.NNLModuleCompiler();
                iCompiler.Compile(fmodule.FullPath, fmodule);
                ProjectManager.Default.ProjectChanged = true;

                    // make certain that the system knows something has changed.
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "Module updater", 
                    string.Format("Update of {0} failed with message: {1}", fmodule.FileName, e));
                fHasError = true;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            LogService.Log.LogInfo("Module updater", string.Format("Module '{0}' updated.", fmodule.FileName));
        }

        /// <summary>
        ///     This function is called by <see cref="End" /> and EndOk. So it provides
        ///     a location to put common code that always needs to be called at the
        ///     end. When you <see langword="override" /> this function, don't forget
        ///     to call the base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            WindowMain.UndoStore.UndoStateStack.Pop();
            base.PerformOnEnd();
        }

        /// <summary>
        ///     the final function to call when not all things went ok. Display a
        ///     message to indicate the fail.
        /// </summary>
        protected override void End()
        {
            base.End();
            if (fHasError)
            {
                System.Windows.MessageBox.Show(
                    "Failed to Update the module! See log for more info.", 
                    "Update module", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     The final function to call when the whole process succeeded. It calls
        ///     the <see cref="JaStDev.HAB.Designer.ProjectOperation.EndedOk" />
        ///     event.
        /// </summary>
        protected override void EndOk()
        {
            base.EndOk();
            if (Brain.Current.Modules.Items.IndexOf(fmodule) == Brain.Current.Modules.TextBinding)
            {
                System.Windows.MessageBox.Show(
                    "The module implements bindings with topic patterns, it is best to rebuild all the patterns.", 
                    "Module updated", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
            }
        }
    }
}