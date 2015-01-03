// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Modules.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the modules in aa network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Manages all the modules in aa network.
    /// </summary>
    [System.Xml.Serialization.XmlRoot("Modules", Namespace = "http://www.janbogaerts.name", IsNullable = false)]
    public class Modules
    {
        /// <summary>The f modules.</summary>
        private System.Collections.Generic.List<Module> fModules;

        /// <summary>The f text binding.</summary>
        private int fTextBinding = -1;

        /// <summary>The f to delete.</summary>
        private System.Collections.Generic.List<Module> fToDelete;

                                                        // keeps track of modules that need to be deleted the next time the system is flushed.

        /// <summary>The f varmanager.</summary>
        private VarManager fVarmanager;

        #region Path

        /// <summary>
        ///     Gets/sets the path where all the network files are stored for the
        ///     modules.
        /// </summary>
        public string Path { get; set; }

        #endregion

        #region TextBinding

        /// <summary>
        ///     Gets/sets the index of the module that contains the precompiled
        ///     binding data which should be used during the compilation of text
        ///     patterns.
        /// </summary>
        public int TextBinding
        {
            get
            {
                return fTextBinding;
            }

            set
            {
                fTextBinding = value;
            }
        }

        #endregion

        #region CallOsCallback

        /// <summary>
        ///     Gets/sets the callback that was declared in the last loaded module for
        ///     triggering OS function calls.
        /// </summary>
        public ulong CallOsCallback { get; set; }

        #endregion

        #region Modules

        /// <summary>
        ///     Gets the list of imported modules. For removing a module: don't remove
        ///     from the list directly, but use the <see cref="Modules.Release" />
        ///     function, to make certain that the operation can still be undone when
        ///     the project isn't saved.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.List<Module> Items
        {
            get
            {
                if (fModules == null)
                {
                    LoadModules();
                }

                return fModules;
            }
        }

        #endregion

        #region Varmanager

        /// <summary>
        ///     Gets the object that manages all the common locals, ints and doubles
        ///     that can be shared accross modules.
        /// </summary>
        public VarManager Varmanager
        {
            get
            {
                if (fVarmanager == null)
                {
                    fVarmanager = new VarManager();
                }

                return fVarmanager;
            }
        }

        #endregion

        /// <summary>Instructs the object to make the <paramref name="path"/> absolute so
        ///     that the data can be loaded.</summary>
        /// <param name="path">The path.</param>
        internal void MakePathAbsoluteTo(string path)
        {
            if (Path != null && System.IO.Path.IsPathRooted(Path) == false)
            {
                var iPath = new System.Uri(Path, System.UriKind.Relative);
                var iTemp = new System.Uri(new System.Uri(path), iPath);
                Path = iTemp.LocalPath;
            }
        }

        /// <summary>Instructs the object to make the<see cref="JaStDev.HAB.Modules.Path"/> relative to the specified<paramref name="path"/> so that the info can be streamed together with
        ///     the project.</summary>
        /// <param name="path">The path.</param>
        internal void MakePathRelativeTo(string path)
        {
            Path = PathUtil.RelativePathTo(
                PathUtil.VerifyPathEnd(System.IO.Path.GetDirectoryName(path)), 
                PathUtil.VerifyPathEnd(Path));
        }

        /// <summary>
        ///     Flushes all the data in the modules that was buffered, to disk.
        /// </summary>
        internal void Flush()
        {
            if (fModules != null)
            {
                // if the list of modules wasn't even loaded, we don't need to flush, cause nothing changed.
                if (fToDelete != null)
                {
                    // do this before flushing the new items. This way, there is no conflict when a module is removed and reloaled in 1 session.
                    foreach (var i in fToDelete)
                    {
                        i.Flush(); // flush before removing the deleted items, this is to remove the files.
                        fModules.Remove(i);
                    }

                    fToDelete = null;
                }

                foreach (var i in fModules)
                {
                    i.Flush();
                }
            }

            if (fVarmanager != null)
            {
                fVarmanager.Flush();
            }
        }

        /// <summary>The load modules.</summary>
        private void LoadModules()
        {
            fModules = new System.Collections.Generic.List<Module>();
            var iPath = Path;
            if (string.IsNullOrEmpty(iPath) == false)
            {
                foreach (var i in System.IO.Directory.EnumerateFiles(Path, "*.mod"))
                {
                    try
                    {
                        fModules.Add(Module.LoadXml(i));
                    }
                    catch
                    {
                        LogService.Log.LogError("Modules.load", "failed to load module info for: " + i);
                    }
                }
            }
        }

        /// <summary>
        ///     makes certain that all the data is loaded, and set to 'changed', so
        ///     that things need to be stored. This is for loading template projects.
        /// </summary>
        internal void TouchMem()
        {
            foreach (var i in Items)
            {
                i.TouchMem();
            }

            Varmanager.TouchMem();
        }

        /// <summary>makes certain that the module is deleted the next time that a flush is
        ///     performed. Note: this doesn't delete the neurons themselves, this has
        ///     to be done with <see cref="NNLModuleCompiler.RemovePreviousDef"/> ,
        ///     so this can also be used to release the module without deletion.</summary>
        /// <param name="item"></param>
        public void Release(Module item)
        {
            if (fToDelete == null)
            {
                fToDelete = new System.Collections.Generic.List<Module>();
            }

            item.Release(); // still need to do a release, primarely to let the module know it is changed.
            fToDelete.Add(item);
            Items.Remove(item); // can't have access to this module anymore.
        }
    }
}