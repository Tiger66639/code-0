// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INeuronInfo.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> similar to <see cref="IDescriptionable" />
//   in that it is used by the main window to edit description and title. This
//   provides a shortcuts since it provides a way to retrieve the
//   <see cref="NeuronData" /> object and work directly on this.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> similar to <see cref="IDescriptionable" />
    ///     in that it is used by the main window to edit description and title. This
    ///     provides a shortcuts since it provides a way to retrieve the
    ///     <see cref="NeuronData" /> object and work directly on this.
    /// </summary>
    public interface INeuronInfo
    {
        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        NeuronData NeuronInfo { get; }
    }
}