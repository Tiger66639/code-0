// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FreezeEventAgrs.cs" company="">
//   
// </copyright>
// <summary>
//   A <see langword="delegate" /> for events that signals changes in a
//   <see cref="Neuron" /> 's <see cref="JaStDev.HAB.Neuron.IsFrozen" />
//   state.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see langword="delegate" /> for events that signals changes in a
    ///     <see cref="Neuron" /> 's <see cref="JaStDev.HAB.Neuron.IsFrozen" />
    ///     state.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The arguments for the event.</param>
    public delegate void FreezeEventHandler(object sender, FreezeEventAgrs e);

    /// <summary>
    ///     The evnet arguments triggered when a neuron's freeze state has changed.
    /// </summary>
    public class FreezeEventAgrs : BrainEventArgs
    {
        /// <summary>
        ///     Gets or sets the neuron that was involved in the freeze operation.
        /// </summary>
        /// <value>
        ///     The neuron.
        /// </value>
        public Neuron Neuron { get; set; }
    }
}