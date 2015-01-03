// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewRootEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Event arguments for the NewRoot event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{

    #region NewRoot event types

    /// <summary>
    ///     Event arguments for the NewRoot event.
    /// </summary>
    public class NewRootEventArgs : System.EventArgs
    {
        /// <summary>
        ///     Gets or sets the neuron that represents the relationship for which
        ///     there was a new root item.
        /// </summary>
        /// <value>
        ///     The relationship.
        /// </value>
        public Neuron Relationship { get; set; }

        /// <summary>
        ///     Gets or sets the actual root item. Note: also check
        ///     <see cref="JaStDev.HAB.NewRootEventArgs.PossibleOtherRoot" />
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public NeuronCluster Item { get; set; }

        /// <summary>
        ///     For some relationships, there is a conversion from non recursive to
        ///     recursive (for 'also' and 'similar', to get root adjectives).
        ///     Unfortunatly, these relationhips are bidirectional declared: 'a' links
        ///     to 'b' and 'b' to 'a', so to make certain we don't get 2 roots
        ///     pointing to each other, we pass along the other object (the child) as
        ///     well, so the thes can check that it isn't already a root (in which
        ///     case, the item should not be added as a root.
        /// </summary>
        public Neuron PossibleOtherRoot { get; set; }
    }

    /// <summary>
    ///     <see langword="delegate" /> used for NewRoot event of the WordNetSin.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void NewRootHandler(object sender, NewRootEventArgs e);

    #endregion
}