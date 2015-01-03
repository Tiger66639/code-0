// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEditorsOwner.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> for objects that own editor objects.
//   (EditorFolder and BrainData). So we can have a simple accessor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an <see langword="interface" /> for objects that own editor objects.
    ///     (EditorFolder and BrainData). So we can have a simple accessor.
    /// </summary>
    public interface IEditorsOwner
    {
        /// <summary>
        ///     Gets the list of editor objects that are stored in this item.
        /// </summary>
        EditorCollection Editors { get; }
    }
}