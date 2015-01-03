// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronListChangedEvent.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the different types of changes that can be applied to a
//   <see cref="NeuronList" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Defines the different types of changes that can be applied to a
    ///     <see cref="NeuronList" />
    /// </summary>
    public enum NeuronListChangeAction
    {
        /// <summary>The insert.</summary>
        Insert, 

        /// <summary>The remove.</summary>
        Remove
    }

    /// <summary>
    ///     A <see langword="delegate" /> for events that signals changes in
    ///     <see cref="Neuron" /> s.
    /// </summary>
    /// <param name="sender">
    ///     The <see cref="Neuron" /> who owns the list that's changed.
    /// </param>
    /// <param name="e">
    ///     The event arguments (the actual list that changed can be found here +
    ///     more data about what changed..
    /// </param>
    public delegate void NeuronListChangedEventHandler(object sender, NeuronListChangedEventArgs e);

    /// <summary>
    ///     Event arguments used for events that signal changes in
    ///     <see cref="Neuron" /> objects.
    /// </summary>
    public class NeuronListChangedEventArgs : BrainEventArgs
    {
        #region Prop

        /// <summary>
        ///     Identifies which neuron was added/removed to/from the list.
        /// </summary>
        public Neuron Item { get; set; }

        /// <summary>
        ///     Gets/sets the index at which the change occured.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Gets/sets the type of change that occured on the list.
        /// </summary>
        public NeuronListChangeAction Action { get; set; }

        /// <summary>
        ///     Gets a thread safe accessor for the list that changed.
        /// </summary>
        public Accessor List
        {
            get
            {
                return InternalList.GetAccessor();
            }
        }

        /// <summary>
        ///     Gets the type of the list.
        /// </summary>
        /// <remarks>
        ///     This is used by the designer to check the type of the list, for
        ///     redistribution of the event.
        /// </remarks>
        /// <value>
        ///     The type of the list.
        /// </value>
        public System.Type ListType
        {
            get
            {
                return InternalList.GetType();
            }
        }

        /// <summary>
        ///     Gets an object that uniquely identifies the list that was changed.
        ///     This can be used in dicationaries for instance, like the designer's
        ///     eventManager. This is used to make a difference between the cluster's
        ///     children and owners list.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public object Identifier
        {
            get
            {
                return InternalList;
            }
        }

        /// <summary>
        ///     Gets the owner of the list (if there is any).
        /// </summary>
        /// <value>
        ///     The list owner.
        /// </value>
        public Neuron ListOwner
        {
            get
            {
                var iList = InternalList as OwnedBrainList;
                if (iList != null)
                {
                    return iList.Owner;
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets or sets the <see langword="internal" /> (actual) list.
        /// </summary>
        /// <value>
        ///     The <see langword="internal" /> list.
        /// </value>
        internal BrainList InternalList { get; set; }

        #endregion
    }
}