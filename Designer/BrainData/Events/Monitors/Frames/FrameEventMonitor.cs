// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides monitoring capabilities for a <see cref="Frame" /> . Note that
//   this is not yet complete: we don't track if any of the required links are
//   removed from the frame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides monitoring capabilities for a <see cref="Frame" /> . Note that
    ///     this is not yet complete: we don't track if any of the required links are
    ///     removed from the frame.
    /// </summary>
    internal class FrameEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FrameEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FrameEventMonitor(Frame toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Frame Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (Frame)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>
        ///     Registers the sequences list, when it is created.
        /// </summary>
        public void RegisterSequences()
        {
            EventManager.Current.AddNeuronChangedMonitor(this, Item.Sequences.Cluster.ID);
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            UpdateLinkChange(link);
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            UpdateLinkChange(link);
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem != null)
            {
                iItem.NeuronChanged(e);
            }
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            UpdateLinkChange(link);
        }

        /// <summary>The update link change.</summary>
        /// <param name="link">The link.</param>
        private void UpdateLinkChange(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.POS)
                {
                    iItem.PosChanged();
                }
            }
        }
    }
}