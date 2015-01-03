// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INeuronWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> for all objects that are wrappers for a
//   neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> for all objects that are wrappers for a
    ///     neuron.
    /// </summary>
    public interface INeuronWrapper
    {
        /// <summary>Gets the item.</summary>
        Neuron Item { get; }
    }
}