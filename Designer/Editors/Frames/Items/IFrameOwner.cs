// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFrameOwner.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> that all objects should implement that can
//   own frames, like a frame editor and the object-frames editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> that all objects should implement that can
    ///     own frames, like a frame editor and the object-frames editor.
    /// </summary>
    public interface IFrameOwner
    {
        /// <summary>
        ///     Gets or sets the selected frame.
        /// </summary>
        /// <value>
        ///     The selected frame.
        /// </value>
        Frame SelectedFrame { get; set; }

        /// <summary>
        ///     Gets/sets the index of the currently selected tab, which determins the
        ///     visible part af the frame editor: the elements or the sequences. The
        ///     ui links to this, so that we can control the selected item from here.
        /// </summary>
        int SelectedTabIndex { get; set; }

        /// <summary>
        ///     Gets the list of frames.
        /// </summary>
        /// <value>
        ///     The frames.
        /// </value>
        Data.ObservedCollection<Frame> Frames { get; }
    }
}