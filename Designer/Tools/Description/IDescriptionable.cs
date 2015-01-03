// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDescriptionable.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that enables the hab Description editor,
//   displayed in <see cref="WindowMain" /> to retrieve all the required data
//   from the currently selected object. This is primarely used for non neuron
//   objects, like a mindmap.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an <see langword="interface" /> that enables the hab Description editor,
    ///     displayed in <see cref="WindowMain" /> to retrieve all the required data
    ///     from the currently selected object. This is primarely used for non neuron
    ///     objects, like a mindmap.
    /// </summary>
    /// <remarks>
    ///     This <see langword="interface" /> allows multiple flowdocuments
    /// </remarks>
    public interface IDescriptionable
    {
        /// <summary>
        ///     Gets a title that the description editor can use to display in the
        ///     header.
        /// </summary>
        string DescriptionTitle { get; }

        /// <summary>
        ///     Gets/sets the description data.
        /// </summary>
        System.Windows.Documents.FlowDocument Description { get; set; }
    }
}