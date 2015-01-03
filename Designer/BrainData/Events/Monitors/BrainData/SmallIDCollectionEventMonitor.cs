// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmallIDCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The small id collection event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The small id collection event monitor.</summary>
    internal class SmallIDCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="SmallIDCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public SmallIDCollectionEventMonitor(SmallIDCollection toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public SmallIDCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (SmallIDCollection)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (!(e is NeuronPropChangedEventArgs))
            {
                try
                {
                    var iItem = Item;
                    if (iItem != null && e.Action == BrainAction.Removed)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<ulong>(iItem.RemoveAll), 
                            e.OriginalSourceID); // event handler for the collectionChange will update the eventManager
                    }
                }
                catch (System.Exception ex)
                {
                    LogService.Log.LogError("NeuronIDCollectionEventMonitor.NeuronChanged", ex.ToString());
                }
            }
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            EventManager.Current.RemoveNeuronChangedMonitor(this, Item);
        }

        /// <summary>The add item.</summary>
        /// <param name="id">The id.</param>
        internal void AddItem(ulong id)
        {
            EventManager.Current.AddNeuronChangedMonitor(this, id);
        }

        /// <summary>The remove item.</summary>
        /// <param name="id">The id.</param>
        internal void RemoveItem(ulong id)
        {
            EventManager.Current.RemoveNeuronChangedMonitor(this, id);
        }
    }
}