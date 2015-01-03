// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LargeIDCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   provides network event monitoring for large IDCollections.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides network event monitoring for large IDCollections.
    /// </summary>
    internal class LargeIDCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="LargeIDCollectionEventMonitor"/> class. Initializes a new instance of the<see cref="LargeIDCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public LargeIDCollectionEventMonitor(System.Collections.Generic.ICollection<ulong> toWrap)
            : base(toWrap)
        {
            EventManager.Current.AddAnyChangedMonitor(this);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public System.Collections.Generic.ICollection<ulong> Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (System.Collections.Generic.ICollection<ulong>)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>Called when any neuron has changed, when deleted, simply delete from
        ///     the collection.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action == BrainAction.Removed)
            {
                Item.Remove(e.OriginalSourceID);
            }
        }
    }
}