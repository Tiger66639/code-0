// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronEditorEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The neuron editor event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The neuron editor event monitor.</summary>
    internal class NeuronEditorEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronEditorEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronEditorEventMonitor(NeuronEditor toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public NeuronEditor Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronEditor)Reference.Target;
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
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<NeuronChangedEventArgs>(iItem.Item_NeuronChanged), 
                    e);
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
                iItem.OutgoingLinkRemoved(link);
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
                iItem.OutgoingLinkCreated(link);
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the <paramref name="link"/> (so the other
        ///     side of the <paramref name="link"/> is changed, not the item on this
        ///     side).</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old Val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                iItem.OutgoingLinkChanged(link, oldVal);
            }
        }
    }
}