// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleImporter.cs" company="">
//   
// </copyright>
// <summary>
//   The module importer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The module importer.</summary>
    internal class ModuleImporter : ProjectOperation
    {
        /// <summary>The f files.</summary>
        private string[] fFiles; // this list of files that need to be imported.

        /// <summary>The f has error.</summary>
        private bool fHasError;

        /// <summary>
        ///     Gets or sets the currently running module-importer. This is used by
        ///     designer objects, to check if they need to do some sort of convertion
        ///     on the id's they read in.
        /// </summary>
        /// <value>
        ///     The importer.
        /// </value>
        public static ModuleImporter Importer { get; set; }

        /// <summary>
        ///     general purpose field to store some extra data that can be passed
        ///     along to the endOk event.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        ///     Shows the open module dialog box so the user can select a module to
        ///     import. If the user doens't cancel the operation, the module is
        ///     imported.
        /// </summary>
        internal void Import()
        {
            if (Importer != null)
            {
                throw new System.InvalidOperationException("Can only import 1 code module at a time.");
            }

            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Filter = Properties.Resources.ModuleFileFilter;
            iDlg.FilterIndex = 1;
            iDlg.Multiselect = true;

            var iRes = iDlg.ShowDialog();
            if (iRes.HasValue && iRes == true)
            {
                fFiles = iDlg.FileNames;
                Importer = this;

                    // needs to be done after LoadFileSizes, cause we can quit at that stage, don't want to cause a lock here.
                DisableUI();
                WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                    // block undo data, so that no data gets recorded, otherwise mem overload
                System.Action iImportMod = InternalImport;

                    // can be a big file, best async, should be less problems, cause we are importing from the root
                iImportMod.BeginInvoke(null, null);
            }
        }

        /// <summary>
        ///     import the module.
        /// </summary>
        private void InternalImport()
        {
            try
            {
                foreach (var i in fFiles)
                {
                    LoadFile(i);
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Module importer", string.Format("Load failed with message: {0}", e));
                fHasError = true;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
        }

        /// <summary>The load file.</summary>
        /// <param name="fileName">The file name.</param>
        private void LoadFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var iMod = Module.LoadXml(fileName);
                var iCompiler = new Parsers.NNLModuleCompiler();
                iCompiler.Compile(fileName, iMod);
            }
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
                    "Failed to import the module! See log for more info.", 
                    "Import module", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     This function is called by <see cref="End" /> and EndOk. So it provides
        ///     a location to put common code that always needs to be called at the
        ///     end. When you <see langword="override" /> this function, don't forget
        ///     to call the base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            Importer = null;
            WindowMain.UndoStore.UndoStateStack.Pop();
            base.PerformOnEnd();
        }

        /// <summary>The import.</summary>
        /// <param name="module">The module.</param>
        internal void Import(Module module)
        {
            Importer = this;

                // needs to be done after LoadFileSizes, cause we can quit at that stage, don't want to cause a lock here.
            DisableUI();
            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // block undo data, so that no data gets recorded, otherwise mem overload
            System.Action<Module> iImportMod = LoadModule;

                // can be a big file, best async, should be less problems, cause we are importing from the root
            iImportMod.BeginInvoke(module, null, null);
        }

        /// <summary>The load module.</summary>
        /// <param name="mod">The mod.</param>
        private void LoadModule(Module mod)
        {
            try
            {
                var iCompiler = new Parsers.NNLModuleCompiler();
                iCompiler.Compile(mod.FileName, mod);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Module importer", string.Format("Load failed with message: {0}", e));
                fHasError = true;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(End));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(EndOk));
            LogService.Log.LogInfo("Module importer", string.Format("Module '{0}' Loaded.", mod.FileName));
        }
    }
}