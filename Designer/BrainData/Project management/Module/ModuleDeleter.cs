// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleDeleter.cs" company="">
//   
// </copyright>
// <summary>
//   deletes a module
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     deletes a module
    /// </summary>
    internal class ModuleDeleter : ProjectOperation
    {
        /// <summary>The f has error.</summary>
        private bool fHasError;

        /// <summary>The f to delete.</summary>
        private Module fToDelete;

        /// <summary>
        ///     A general purspose tag that can be used to pass along some parameters
        ///     to the 'endok' event.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>The delete.</summary>
        /// <param name="toDelete">The to delete.</param>
        internal void Delete(Module toDelete)
        {
            fToDelete = toDelete;
            DisableUI();
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // block undo data, so that no data gets recorded, otherwise mem overload
            System.Action iImportMod = InternalImport;

                // can be a big file, best async, should be less problems, cause we are importing from the root
            iImportMod.BeginInvoke(null, null);
        }

        /// <summary>The internal import.</summary>
        private void InternalImport()
        {
            try
            {
                Parsers.NNLModuleCompiler.RemovePreviousDef(fToDelete);
                Brain.Current.Modules.Release(fToDelete);
                ProjectManager.Default.ProjectChanged = true;

                    // make certain that the system knows something has changed.
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ModuleDeleter.Import", string.Format("Deletion failed with message: ", e));
                fHasError = true;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            LogService.Log.LogInfo("ModuleImporter.Import", string.Format("Module '{0}' deleted.", fToDelete.FileName));
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
                    "Failed to import module! See log for more info.", 
                    "Import module", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}