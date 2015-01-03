// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FSItemEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The event monitor for <see cref="FrameSequenceItem" /> s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The event monitor for <see cref="FrameSequenceItem" /> s.
    /// </summary>
    internal class FSItemEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FSItemEventMonitor"/> class. Initializes a new instance of the <see cref="FSItemEventMonitor"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FSItemEventMonitor(FrameSequenceItem toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FrameSequenceItem Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FrameSequenceItem)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            UpdateChange(link);
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            UpdateChange(link);
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            UpdateChange(link);
        }

        /// <summary>The update change.</summary>
        /// <param name="link">The link.</param>
        private void UpdateChange(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.FrameSequenceItemValue)
                {
                    iItem.CallPropertyChangedChanged("Element");

                        // can be done from the sub thread, is a simple prop update.
                }
            }
        }
    }
}