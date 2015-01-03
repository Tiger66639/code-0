// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionSegmentEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The fe restriction segment event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The fe restriction segment event monitor.</summary>
    internal class FERestrictionSegmentEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionSegmentEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestrictionSegmentEventMonitor(FERestrictionSegment toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FERestrictionSegment Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FERestrictionSegment)Reference.Target;
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
                if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestriction)
                {
                    iItem.CallPropertyChangedChanged("Restriction");

                        // can be done from the sub thread, is a simple prop update.
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection)
                {
                    iItem.CallPropertyChangedChanged("SearchDirection");
                    iItem.CallPropertyChangedChanged("RequiresRestriction");
                }
            }
        }
    }
}