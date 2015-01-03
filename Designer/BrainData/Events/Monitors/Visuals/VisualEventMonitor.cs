// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides monitoring for a VFrame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides monitoring for a VFrame.
    /// </summary>
    internal class VisualEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="VisualEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public VisualEventMonitor(VisualFrame toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public VisualFrame Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (VisualFrame)Reference.Target;
                }

                return null;
            }
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

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            // VisualFrame iItem = Item;
            // if (iItem != null && iItem.IsLoaded == true)
            // {
            // if (link.MeaningID == (ulong)PredefinedNeurons.Subordinates)
            // iItem.Subordinates = null;
            // }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            // VisualFrame iItem = Item;
            // if (iItem != null && iItem.IsLoaded == true)
            // {
            // if (link.MeaningID == (ulong)PredefinedNeurons.Subordinates)
            // iItem.RefreshSubordinates();
            // }
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            // VisualFrame iItem = Item;
            // if (iItem != null && iItem.IsLoaded == true)
            // {
            // if (link.MeaningID == (ulong)PredefinedNeurons.Subordinates)
            // iItem.RefreshSubordinates();
            // }
        }

        /// <summary>The from meaning changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void FromMeaningChanged(Link link, ulong oldVal)
        {
            // VisualFrame iItem = Item;
            // if (iItem != null && iItem.IsLoaded == true)
            // {
            // if (link.MeaningID == (ulong)PredefinedNeurons.Subordinates)
            // iItem.RefreshSubordinates();
            // }
        }
    }
}