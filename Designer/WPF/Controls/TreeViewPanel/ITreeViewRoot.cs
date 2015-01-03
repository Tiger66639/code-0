// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITreeViewRoot.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by all objects
//   that function as root for a Treeview object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by all objects
    ///     that function as root for a Treeview object.
    /// </summary>
    public interface ITreeViewRoot : Data.ICascadedNotifyCollectionChanged, Data.INotifyCascadedPropertyChanged
    {
        /// <summary>
        ///     Gets a list to all the children of the tree root.
        /// </summary>
        /// <value>
        ///     The tree items.
        /// </value>
        System.Collections.IList TreeItems { get; }
    }
}