﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow collection event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The flow collection event monitor.</summary>
    internal class FlowCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FlowCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FlowCollectionEventMonitor(FlowCollection toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FlowCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FlowCollection)Reference.Target;
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
                                    new System.Action<Neuron>(iItem.RemoveAll), 
                                    e.OriginalSource);
                                break;
                            case BrainAction.Removed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<Neuron>(iItem.RemoveAll), 
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
            EventManager.Current.RemoveNeuronChangedMonitor(this, from i in Item select i.Item.ID);
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