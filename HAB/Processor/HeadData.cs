// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeadData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data specific for a split of a processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Contains all the data specific for a split of a processor.
    /// </summary>
    internal class HeadData
    {
        /// <summary>
        ///     A dictionary containing the id's of all the clones that were made to
        ///     copy the stack of this processor. Each id references to the original
        ///     neuron, as should be used on this processor.
        /// </summary>
        /// <remarks>
        ///     This is a dictionary since it is mostly used for looking up neurons,
        ///     to see if they are cloned.
        /// </remarks>
        public System.Collections.Generic.Dictionary<ulong, Neuron> Clones =
            new System.Collections.Generic.Dictionary<ulong, Neuron>();

        /// <summary>
        ///     Keeps track of how many sub processors are still active.
        /// </summary>
        public int StillActive { get; set; }

        /// <summary>
        ///     Keeps a record of all the sub processors.
        /// </summary>
        public System.Collections.Generic.List<SplitData> SubProcessors { get; set; }

        /// <summary>
        ///     Gets or sets the neuroncluster with the callback code that needs to be
        ///     called when the split is done.
        /// </summary>
        /// <value>
        ///     The callback.
        /// </value>
        public NeuronCluster Callback { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="SplitData" /> object that is owned by the
        ///     processor that requested the split.
        /// </summary>
        /// <value>
        ///     The requestor.
        /// </value>
        public SplitData Requestor { get; set; }

        /// <summary>
        ///     Gets or sets <see cref="HeadData" /> object that was previously active
        ///     before a new split was done. When this headData is ready the specified
        ///     object gets signalled as one more channel ready.
        /// </summary>
        /// <value>
        ///     Previous.
        /// </value>
        public HeadData Previous { get; set; }
    }
}