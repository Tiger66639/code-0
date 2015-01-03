// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data of an item that was registered for monitoring.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the data of an item that was registered for monitoring.
    /// </summary>
    internal abstract class EventMonitor
    {
        // abstract cause we want to prevent it from being used directly.
        /// <summary>Initializes a new instance of the <see cref="EventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public EventMonitor(object toWrap)
        {
            Reference = new System.WeakReference(toWrap);
        }

        /// <summary>
        ///     Gets a weak reference to the object for which this object provides
        ///     monitoring functionality.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public System.WeakReference Reference { get; private set; }

        /// <summary>Called when a neuron was changed.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void NeuronChanged(NeuronChangedEventArgs e)
        {
        }

        /// <summary>Called when a list was changed.</summary>
        /// <param name="e">The <see cref="NeuronListChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public virtual void ListChanged(NeuronListChangedEventArgs e)
        {
            // don't do anything by default
        }

        /// <summary>Called when a link was changed.</summary>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public virtual void LinkChanged(LinkChangedEventArgs e)
        {
            // don't do anything by default
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public virtual void ToLinkRemoved(Link link)
        {
            // don't do anything by default
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public virtual void ToLinkCreated(Link link)
        {
            // don't do anything by default
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value of the from part in the link.</param>
        public virtual void FromLinkRemoved(Link link, ulong oldValue)
        {
            // don't do anything by default
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public virtual void FromLinkCreated(Link link)
        {
            // don't do anything by default
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'to' value.</param>
        public virtual void ToChanged(Link link, ulong oldVal)
        {
            // don't do anything by default
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'To' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'from' value.</param>
        public virtual void FromChanged(Link link, ulong oldVal)
        {
            // don't do anything by default
        }

        /// <summary>Called when the meaning of a <paramref name="link"/> was changed where
        ///     this monitor wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'from' value.</param>
        public virtual void FromMeaningChanged(Link link, ulong oldVal)
        {
            // don't do anything by default
        }

        /// <summary>Called when the meaning of a <paramref name="link"/> was changed where
        ///     this monitor wraps the 'To' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'from' value.</param>
        public virtual void ToMeaningChanged(Link link, ulong oldVal)
        {
            // don't do anything by default
        }

        /// <summary>Checks if the eventmonitor references an object with the specified id.
        ///     This is used to resolve freshly created 'temp' items that were
        ///     monitored.</summary>
        /// <param name="newId">The new id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool WrapsID(ulong newId)
        {
            System.Diagnostics.Debug.Assert(Reference.IsAlive);
            var iWrapper = Reference.Target as INeuronWrapper;
            System.Diagnostics.Debug.Assert(iWrapper != null);
            return iWrapper.Item != null && iWrapper.Item.ID == newId;
        }
    }
}