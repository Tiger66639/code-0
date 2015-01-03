// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEditorSelection.cs" company="">
//   
// </copyright>
// <summary>
//   A simple <see langword="interface" /> to change the selected values.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A simple <see langword="interface" /> to change the selected values.
    /// </summary>
    public interface IEditorSelection
    {
        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        System.Collections.IList SelectedItems { get; }

        /// <summary>
        ///     Gets/sets the currently selected item. If there are multiple
        ///     selections, the first is returned.
        /// </summary>
        object SelectedItem { get; }
    }
}