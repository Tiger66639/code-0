// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Needs to monitor removes and replaces, cause the list keeps a<see langword="ref" /> to the object, not the id.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>Needs to monitor removes and replaces, cause the list keeps a<see langword="ref"/> to the object, not the id.</summary>
    /// <typeparam name="T"></typeparam>
    internal class NeuronCollectionEventMonitor<T> : EventMonitor
        where T : Neuron
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronCollectionEventMonitor{T}"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronCollectionEventMonitor(NeuronCollection<T> toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public NeuronCollection<T> Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (NeuronCollection<T>)Reference.Target;
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
                                    new System.Action<T, T>(iItem.Replace), 
                                    e.OriginalSource, 
                                    e.NewValue); // event handler for the collectionChange will update the eventManager
                                break;
                            case BrainAction.Removed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<T>(iItem.RemoveAll), 
                                    e.OriginalSource);

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
            EventManager.Current.RemoveNeuronChangedMonitor(this, from i in Item where i != null select i.ID);
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