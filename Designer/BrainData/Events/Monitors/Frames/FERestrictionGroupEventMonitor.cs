// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionGroupEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The event monitor for the frame element restriction groups.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     The event monitor for the frame element restriction groups.
    /// </summary>
    internal class FERestrictionGroupEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionGroupEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FERestrictionGroupEventMonitor(FERestrictionGroup toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FERestrictionGroup Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FERestrictionGroup)Reference.Target;
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
                if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetLogicValue)
                {
                    if (iItem.Owner is FrameElement)
                    {
                        // the root list of the frame element has the logicProp operator defined on the Frame element: not such a good idea, but easier.
                        ((FrameElement)iItem.Owner).CallPropertyChangedChanged("LogicOperator");
                    }
                    else
                    {
                        iItem.CallPropertyChangedChanged("LogicOperator");

                            // can be done from the sub thread, is a simple prop update.
                    }
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.RestrictionDefinesFullContent)
                {
                    iItem.CallPropertyChangedChanged("RestrictionDefinesFullContent");
                }
            }
        }
    }
}