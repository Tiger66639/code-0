// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for all neurons that can contain a value, like a string,
//   <see langword="double" /> or int.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A base class for all neurons that can contain a value, like a string,
    ///     <see langword="double" /> or int.
    /// </summary>
    /// <remarks>
    ///     This class is <see langword="abstract" /> cause it can't be created on
    ///     it's own.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Number, typeof(Neuron))]
    public abstract class ValueNeuron : Neuron
    {
        /// <summary>
        ///     Gets or sets the value of the neuron as a blob.
        /// </summary>
        /// <remarks>
        ///     This can be used during streaming of the neuron.
        /// </remarks>
        /// <value>
        ///     The object encapsulated by the neuron.
        /// </value>
        public abstract object Blob { get; set; }
    }
}