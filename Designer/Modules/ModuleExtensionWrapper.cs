// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleExtensionWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   wraps an module extension filename, so that the ui can observe changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     wraps an module extension filename, so that the ui can observe changes.
    /// </summary>
    public class ModuleExtensionWrapper : Data.OwnedObject<ModuleWrapper>
    {
        /// <summary>Initializes a new instance of the <see cref="ModuleExtensionWrapper"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="ext">The ext.</param>
        public ModuleExtensionWrapper(ModuleWrapper owner, string ext)
        {
            Owner = owner;
            Extension = ext;
        }

        #region Extension

        /// <summary>
        ///     Gets the name of the object
        /// </summary>
        public string Extension { get; internal set; }

        #endregion
    }
}