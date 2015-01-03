// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDataEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The neuron data event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The neuron data event monitor.</summary>
    internal class NeuronDataEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronDataEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronDataEventMonitor(NeuronData toWrap)
            : base(toWrap)
        {
        }

        /// <summary>Gets the item.</summary>
        public NeuronData Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronData)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                var iFound =
                    (from i in BrainData.Current.Overlays where i.ItemID == link.MeaningID select i).FirstOrDefault();
                if (iFound != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.DataBind, 
                        new System.Action<OverlayText>(iItem.AddOverlay), 
                        iFound);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value of the from part in the link.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            var iItem = Item;
            if (iItem != null && iItem.HasOverlays)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.DataBind, 
                    new System.Action<ulong>(iItem.RemoveOverlay), 
                    link.MeaningID);
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'to' value.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (iItem.HasOverlays)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.DataBind, 
                        new System.Action<ulong>(iItem.RemoveOverlay), 
                        link.MeaningID);
                }

                var iFound =
                    (from i in BrainData.Current.Overlays where i.ItemID == link.MeaningID select i).FirstOrDefault();
                if (iFound != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.DataBind, 
                        new System.Action<OverlayText>(iItem.AddOverlay), 
                        iFound);
                }
            }
        }
    }
}