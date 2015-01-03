// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordnetEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event handler decleration for events from the <see cref="WordNetSin" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Event handler decleration for events from the <see cref="WordNetSin" /> .
    /// </summary>
    public delegate void WordNetEventHandler(object sender, WordNetEventArgs e);

    /// <summary>
    ///     Event arguments for events comming from the <see cref="WordNetSin" />
    /// </summary>
    public class WordNetEventArgs : BrainEventArgs
    {
        /// <summary>
        ///     Gets or sets the neuron that is involved in the operation.
        /// </summary>
        /// <value>
        ///     The neuron.
        /// </value>
        public Neuron Neuron { get; set; }
    }
}