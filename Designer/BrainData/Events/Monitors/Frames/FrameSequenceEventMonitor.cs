// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequenceEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The frame sequence event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame sequence event monitor.</summary>
    internal class FrameSequenceEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FrameSequenceEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FrameSequenceEventMonitor(FrameSequence toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FrameSequence Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FrameSequence)Reference.Target;
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
            // FrameSequence iItem = Item;
            // if (iItem != null)
            // {
            // if (link.MeaningID == (ulong)PredefinedNeurons.SequenceResultRestrictions)
            // {
            // NeuronCluster iResrictionsCluster = iItem.Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions) as NeuronCluster;
            // if (iResrictionsCluster != null)
            // {
            // EventManager.Current.RegisterForLinksOut(this, iResrictionsCluster.ID);
            // iItem.Restrictions = new RestrictionsCollection(iItem, iResrictionsCluster);
            // }
            // else
            // iItem.Restrictions = null;
            // }
            // else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetLogicValue)
            // iItem.CallPropertyChangedChanged("LogicOperator");
            // }
        }
    }
}