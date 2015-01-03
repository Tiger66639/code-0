// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event arguments for the Split event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Event arguments for the Split event.
    /// </summary>
    public class SplitEventArgs : System.EventArgs
    {
        /// <summary>The f to split.</summary>
        private readonly System.Collections.Generic.List<Neuron> fToSplit =
            new System.Collections.Generic.List<Neuron>();

        /// <summary>
        ///     Gets or sets the list of neurons on which we need to split.
        /// </summary>
        /// <remarks>
        ///     This should also include the value that should be used for the
        ///     current processor.
        /// </remarks>
        public System.Collections.Generic.List<Neuron> ToSplit
        {
            get
            {
                return fToSplit;
            }
        }

        #region SubProcessors

        /// <summary>
        ///     Gets the list of sub processors that was created, can be
        ///     <see langword="null" /> if none.
        /// </summary>
        public System.Collections.Generic.List<Processor> SubProcessors { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets the variable that will store the split result in each processor.
        /// </summary>
        /// <value>
        ///     The variable.
        /// </value>
        public Variable Variable { get; internal set; }

        /// <summary>
        ///     Gets the neuroncluster with the callback code that needs to be called
        ///     when the split is done.
        /// </summary>
        /// <value>
        ///     The callback.
        /// </value>
        public NeuronCluster Callback { get; internal set; }

        /// <summary>
        ///     Gets the processor for which a split needs to be performed.
        /// </summary>
        /// <value>
        ///     The processor.
        /// </value>
        public Processor Processor { get; internal set; }
    }

    /// <summary>
    ///     <see langword="delegate" /> used for events triggered during a split.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SplitHandler(object sender, SplitEventArgs e);
}