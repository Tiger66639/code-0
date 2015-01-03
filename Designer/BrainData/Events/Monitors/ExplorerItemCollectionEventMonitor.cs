// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerItemCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The explorer item collection event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The explorer item collection event monitor.</summary>
    internal class ExplorerItemCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerItemCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ExplorerItemCollectionEventMonitor(ExplorerItemCollection toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public ExplorerItemCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (ExplorerItemCollection)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (!(e is NeuronPropChangedEventArgs))
            {
                try
                {
                    var iItem = Item;
                    if (iItem != null)
                    {
                        switch (e.Action)
                        {
                            case BrainAction.Created:

                                // don't do anything
                                break;
                            case BrainAction.Changed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<Neuron, Neuron>(iItem.ChangeNeuron), 
                                    e.OriginalSource, 
                                    e.NewValue);
                                break;
                            case BrainAction.Removed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<Neuron, ulong>(iItem.RemoveNeuron), 
                                    e.OriginalSource, 
                                    e.OriginalSourceID);

                                    // event handler for the collectionChange will update the eventManager
                                break;
                            default:
                                throw new System.InvalidOperationException();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    LogService.Log.LogError("DebugNeuronEventMonitor.NeuronChanged", ex.ToString());
                }
            }
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            EventManager.Current.RemoveNeuronChangedMonitor(
                this, 
                from i in Item where i is NeuronExplorerItem select i.ID);
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