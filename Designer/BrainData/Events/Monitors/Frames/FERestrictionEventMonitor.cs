// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   This is a restriction event monitor that can be used by both a
//   <see cref="FERestriction" /> and a FESemanticRestriction
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This is a restriction event monitor that can be used by both a
    ///     <see cref="FERestriction" /> and a FESemanticRestriction
    /// </summary>
    internal class FERestrictionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestrictionEventMonitor(FrameItemBase toWrap)
            : base(toWrap)
        {
            EventManager.Current.RegisterForLinksOut(this, toWrap.Item.ID);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FERestriction Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FERestriction)Reference.Target;
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
                if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestrictionModifier)
                {
                    iItem.CallPropertyChangedChanged("InclusionModifier");

                        // can be done from the sub thread, is a simple prop update.
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.RestrictionDefinesFullContent)
                {
                    iItem.CallPropertyChangedChanged("RestrictionDefinesFullContent");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.ValueToInspect)
                {
                    iItem.CallPropertyChangedChanged("ValueToInspect");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestrictionSearchDirection)
                {
                    iItem.CallPropertyChangedChanged("SearchDirection");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestriction)
                {
                    iItem.CallPropertyChangedChanged("Restriction");
                }
            }
        }
    }
}