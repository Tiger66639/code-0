// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The flow event monitor.</summary>
    internal class FlowEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FlowEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FlowEventMonitor(Flow toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Flow Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (Flow)Reference.Target;
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
            var iItem = Item;
            if (iItem != null)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.FlowType)
                {
                    iItem.ClearFlowType();
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.FloatingFlowKeepsData)
                {
                    iItem.InternalSetKeepsData(false);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            HandleLink(link);
        }

        /// <summary>The handle link.</summary>
        /// <param name="link">The link.</param>
        private void HandleLink(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.FlowType)
                {
                    if (link.ToID == (ulong)PredefinedNeurons.FlowIsNonDestructiveFloating)
                    {
                        iItem.InternalSetIsNDFloating(true);
                        iItem.InternalSetIsFloating(false);
                    }
                    else if (link.ToID == (ulong)PredefinedNeurons.FlowIsFloating)
                    {
                        iItem.InternalSetIsNDFloating(false);
                        iItem.InternalSetIsFloating(true);
                    }
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.FloatingFlowKeepsData)
                {
                    iItem.InternalSetKeepsData(link.ToID == (ulong)PredefinedNeurons.True);
                }
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old Val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            HandleLink(link);
        }
    }
}