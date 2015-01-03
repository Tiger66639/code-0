// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkChangedEvent.cs" company="">
//   
// </copyright>
// <summary>
//   Delelate used for events that signals changes in a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Delelate used for events that signals changes in a link.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The arguments for the event.</param>
    public delegate void LinkChangedEventHandler(object sender, LinkChangedEventArgs e);

    /// <summary>
    ///     Identifies the different modifications that can be performed on a link or
    ///     <see cref="Neuron" /> (or possibly other objects stored by the
    ///     <see cref="Brain" /> ).
    /// </summary>
    /// <remarks>
    ///     This <see langword="enum" /> is used by the
    ///     <see cref="LinkChangedEventArgs" /> to indicate the type of change that
    ///     occured.
    /// </remarks>
    public enum BrainAction
    {
        /// <summary>
        ///     A new neuron/link was created.
        /// </summary>
        Created, 

        /// <summary>
        ///     The object representation of a neuron was replaced by a new one / a
        ///     link was changed.
        /// </summary>
        /// <remarks>
        ///     This can happen when a neuron's type was changed. for instance, when a
        ///     neuron is promoted to a cluster.
        /// </remarks>
        Changed, 

        /// <summary>
        ///     A neuron/link was removed.
        /// </summary>
        Removed
    }

    /// <summary>
    ///     Event arguments that contain information of how a link was changed and
    ///     the old/new values.
    /// </summary>
    public class LinkChangedEventArgs : BrainEventArgs
    {
        /// <summary>
        ///     Identifiies the link that was changed.
        /// </summary>
        public Link OriginalSource { get; set; }

        /// <summary>
        ///     a direct <see langword="ref" /> to the to value that the link has after
        ///     the operation. this is for faster access (+ to prevent deadlocks), so
        ///     that the neuron doesn't have to be retrieved from the cache while
        ///     handling the event.
        /// </summary>
        public Neuron ToValue { get; set; }

        /// <summary>
        ///     a direct <see langword="ref" /> to the from value that the link has
        ///     after the operation. this is for faster access (+ to prevent
        ///     deadlocks), so that the neuron doesn't have to be retrieved from the
        ///     cache while handling the event.
        /// </summary>
        public Neuron FromValue { get; set; }

        /// <summary>
        ///     Identifies what exactly happened with the link: created, changed or
        ///     removed.
        /// </summary>
        public BrainAction Action { get; set; }

        /// <summary>
        ///     <para>
        ///         Identifies the old value for <see cref="JaStDev.HAB.Link.From" /> ,
        ///         when <see cref="JaStDev.HAB.LinkChangedEventArgs.Action" />
        ///     </para>
        ///     <para>
        ///         is <see cref="LinkAction.Changed" /> or
        ///         <see cref="LinkAction.Removed" /> .
        ///     </para>
        /// </summary>
        public ulong OldFrom { get; set; }

        /// <summary>
        ///     <para>
        ///         Identifies the old value for <see cref="JaStDev.HAB.Link.To" /> , when
        ///         <see cref="JaStDev.HAB.LinkChangedEventArgs.Action" />
        ///     </para>
        ///     <para>
        ///         is <see cref="LinkAction.Changed" /> or
        ///         <see cref="LinkAction.Removed" /> .
        ///     </para>
        /// </summary>
        public ulong OldTo { get; set; }

        /// <summary>
        ///     <para>
        ///         Identifies the new value for <see cref="JaStDev.HAB.Link.From" /> ,
        ///         when <see cref="JaStDev.HAB.LinkChangedEventArgs.Action" />
        ///     </para>
        ///     <para>
        ///         is <see cref="LinkAction.Changed" /> or
        ///         <see cref="LinkAction.Created" /> .
        ///     </para>
        /// </summary>
        public ulong NewFrom { get; set; }

        /// <summary>
        ///     <para>
        ///         Identifies the new value for <see cref="JaStDev.HAB.Link.To" /> , when
        ///         <see cref="JaStDev.HAB.LinkChangedEventArgs.Action" />
        ///     </para>
        ///     <para>
        ///         is <see cref="LinkAction.Changed" /> or
        ///         <see cref="LinkAction.Created" /> .
        ///     </para>
        /// </summary>
        public ulong NewTo { get; set; }

        /// <summary>
        ///     Gets or sets the old value that was assigned to the meaning part of
        ///     the link.
        /// </summary>
        /// <value>
        ///     The new meaning.
        /// </value>
        public ulong OldMeaning { get; set; }

        /// <summary>
        ///     Gets or sets the new value assigned to the meaning part of the link.
        /// </summary>
        /// <value>
        ///     The new meaning.
        /// </value>
        public ulong NewMeaning { get; set; }

        /// <summary>Determines whether the specified <paramref name="item"/> is envolved
        ///     in this event. This allows you to check if the event needs to be
        ///     handled or not.</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the specified <paramref name="item"/> is envolved;
        ///     otherwise, <c>false</c> .</returns>
        public bool IsEnvolved(Neuron item)
        {
            return item.ID == OldTo || item.ID == OldFrom || item.ID == NewFrom || item.ID == NewTo;
        }
    }
}