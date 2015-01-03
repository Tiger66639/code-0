// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputEvent.cs" company="">
//   
// </copyright>
// <summary>
//   <see langword="delegate" /> used for output events without converted
//   arguments, but only the list of neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     <see langword="delegate" /> used for output events without converted
    ///     arguments, but only the list of neurons.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void OutputEventHandler(object sender, OutputEventArgs e);

    /// <summary>Generic <see langword="delegate"/> used for output events.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void OutputEventHandler<T>(object sender, OutputEventArgs<T> e);

    /// <summary>
    ///     base class for output event arguments. Contains a list of values.
    /// </summary>
    public class OutputEventArgs : System.EventArgs
    {
        /// <summary>Gets or sets the data.</summary>
        public System.Collections.Generic.IList<Neuron> Data { get; set; }
    }

    /// <summary>A default implementation for an EventArgs class used to raise output
    ///     events by <see cref="Sin"/> s.</summary>
    /// <remarks>Sins don't have to use this class (or events for output), this is just a
    ///     shortcut. It does offer an extra property Data, which is a reference to
    ///     the <see cref="Neuron"/> that triggered the output event.</remarks>
    /// <typeparam name="T">The type of output data.</typeparam>
    public class OutputEventArgs<T> : OutputEventArgs
    {
        /// <summary>Gets or sets the value.</summary>
        public T Value { get; set; }
    }
}