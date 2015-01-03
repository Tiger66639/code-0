// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BreakPointCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides event monitoring for the BreakPointCollection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Provides event monitoring for the BreakPointCollection.
    /// </summary>
    /// <remarks>
    ///     This class is almost the same as the
    ///     <see cref="JaStDev.HAB.Designer.NeuronCollectionEventMonitor`1" /> , but
    ///     that can delete the neurons through a <see langword="ref" /> to the
    ///     neuron, while the breakpoint collection needs to remove items through the
    ///     id, otherwise it gets a bad result.
    /// </remarks>
    internal class BreakPointCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="BreakPointCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public BreakPointCollectionEventMonitor(BreakPointCollection toWrap)
            : base(toWrap)
        {
            toWrap.CollectionChanged += toWrap_CollectionChanged;
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public BreakPointCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (BreakPointCollection)Reference.Target;
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
                                    new System.Action<ulong, Expression>(iItem.Replace), 
                                    e.OriginalSourceID, 
                                    e.NewValue); // event handler for the collectionChange will update the eventManager
                                break;
                            case BrainAction.Removed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Func<ulong, bool>(iItem.Remove), 
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

        /// <summary>We automatically register and unregister for all children of the
        ///     breakpoint collection. This allows us to filter for only the wanted
        ///     items.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void toWrap_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    EventManager.Current.AddNeuronChangedMonitor(this, ((Expression)e.NewItems[0]).ID);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    EventManager.Current.RemoveNeuronChangedMonitor(this, ((Expression)e.OldItems[0]).ID);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    EventManager.Current.RemoveNeuronChangedMonitor(this, ((Expression)e.OldItems[0]).ID);
                    EventManager.Current.AddNeuronChangedMonitor(this, ((Expression)e.NewItems[0]).ID);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:

                    // the Clear function raises the event before it clears out the internal lists, so we can still loop through the items.
                    EventManager.Current.RemoveNeuronChangedMonitor(this, from i in Item select i.ID);
                    break;
                default:
                    break;
            }
        }
    }
}