// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITreeViewPanelItem.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that should be implemented by data items
//   that want to be displayed by a <see cref="TreeViewPanel" /> . So that it
//   can manipulate the data when required.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     An <see langword="interface" /> that should be implemented by data items
    ///     that want to be displayed by a <see cref="TreeViewPanel" /> . So that it
    ///     can manipulate the data when required.
    /// </summary>
    public interface ITreeViewPanelItem
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        bool IsExpanded { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is selected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is selected; otherwise, <c>false</c> .
        /// </value>
        bool IsSelected { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not.
        ///     When the list of children changes (becomes empty or gets the first
        ///     item), this should be raised when appropriate through a
        ///     propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        bool HasChildren { get; }

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>
        ///     The tree items.
        /// </value>
        System.Collections.IList TreeItems { get; }

        /// <summary>
        ///     Gets the parent tree item.
        /// </summary>
        /// <value>
        ///     The parent tree item.
        /// </value>
        ITreeViewPanelItem ParentTreeItem { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this object needs to be
        ///     brought into view.
        /// </summary>
        /// <remarks>
        ///     This needs to be a simple field set/get with propertyChanged call. The
        ///     treeViewPanel uses this to respond to it. It will also toglle it back
        ///     off again when the operation is done.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if [needs bring into view]; otherwise, <c>false</c> .
        /// </value>
        bool NeedsBringIntoView { get; set; }
    }
}