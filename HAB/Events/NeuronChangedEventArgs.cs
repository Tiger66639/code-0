// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronChangedEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   A <see langword="delegate" /> for events that signals changes in
//   <see cref="Neuron" /> s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see langword="delegate" /> for events that signals changes in
    ///     <see cref="Neuron" /> s.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The arguments for the event.</param>
    public delegate void NeuronChangedEventHandler(object sender, NeuronChangedEventArgs e);

    /// <summary>
    ///     Event arguments used for events that signal changes in
    ///     <see cref="Neuron" /> objects.
    /// </summary>
    public class NeuronChangedEventArgs : BrainEventArgs
    {
        /// <summary>
        ///     Identifies what exactly happened with the Neuron: created, changed or
        ///     removed.
        /// </summary>
        public BrainAction Action { get; set; }

        /// <summary>
        ///     Identifies the neuron that was changed.
        /// </summary>
        public Neuron OriginalSource { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the neuron that was changed..
        /// </summary>
        /// <remarks>
        ///     This property is usefull when a neuron is removed. This event is
        ///     usually handled on a different thread than where it originated from
        ///     (handled in UI while orignated in a procoessor). This might have the
        ///     effect that the id of the neuron has already been set to 0 when the
        ///     event is handled.
        /// </remarks>
        /// <value>
        ///     The Id of the neuron that changed.
        /// </value>
        public ulong OriginalSourceID { get; set; }

        /// <summary>
        ///     Identifies a possible new value that replaces the old OrignalSource.
        /// </summary>
        /// <remarks>
        ///     This property is filled in when a neuron was changed (the object got
        ///     replaced with a new one).
        /// </remarks>
        /// <value>
        ///     The new value.
        /// </value>
        public Neuron NewValue { get; set; }
    }

    /// <summary>
    ///     Event args for a change action, includes the property name that changed.
    /// </summary>
    public class NeuronPropChangedEventArgs : NeuronChangedEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronPropChangedEventArgs"/> class.</summary>
        /// <param name="propName">The prop name.</param>
        /// <param name="source">The source.</param>
        public NeuronPropChangedEventArgs(string propName, Neuron source)
        {
            Action = BrainAction.Changed;
            Property = propName;
            OriginalSource = source;
            OriginalSourceID = source.ID;
        }

        /// <summary>
        ///     Identifies the property that changed.
        /// </summary>
        public string Property { get; set; }
    }
}