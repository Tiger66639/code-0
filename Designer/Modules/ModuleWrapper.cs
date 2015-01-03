// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   wraps a module, so that the changes can be monitored by the manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     wraps a module, so that the changes can be monitored by the manager.
    /// </summary>
    public class ModuleWrapper : Data.ObservableObject
    {
        /// <summary>The f index.</summary>
        private readonly int fIndex;

        /// <summary>Initializes a new instance of the <see cref="ModuleWrapper"/> class.</summary>
        /// <param name="mod">The mod.</param>
        /// <param name="index">The index.</param>
        public ModuleWrapper(Module mod, int index)
        {
            Module = mod;
            fIndex = index;
        }

        #region Extension

        /// <summary>
        ///     Gets the extensions for this module.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<ModuleExtensionWrapper> Extension { get;
            internal set; }

        #endregion

        #region Module

        /// <summary>
        ///     Gets the module that this object wraps.
        /// </summary>
        public Module Module { get; private set; }

        #endregion

        #region HasBindsForText

        /// <summary>
        ///     Gets/sets the value that indicates if this module should provide the
        ///     pre-compiled bindings for the text pattern matcher or not.
        /// </summary>
        public bool HasBindsForText
        {
            get
            {
                return Brain.Current.Modules.TextBinding == fIndex;
            }

            set
            {
                if (value)
                {
                    Brain.Current.Modules.TextBinding = fIndex;
                    Parsers.OutputParser.ResetExpressionsHandler();
                }

                ProjectManager.Default.ProjectChanged = true; // make certain that the project saves these changes.
                OnPropertyChanged("HasBindsForText");
            }
        }

        #endregion
    }
}