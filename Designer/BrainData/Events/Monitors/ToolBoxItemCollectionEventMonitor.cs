// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolBoxItemCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The tool box item collection event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The tool box item collection event monitor.</summary>
    internal class ToolBoxItemCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="ToolBoxItemCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ToolBoxItemCollectionEventMonitor(ToolBoxItemCollection toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public ToolBoxItemCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (ToolBoxItemCollection)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action == BrainAction.Removed)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<Neuron>(Item.RemoveNeuron), 
                    e.OriginalSource);
            }
            else if (e.Action == BrainAction.Changed && !(e is NeuronPropChangedEventArgs))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<Neuron, Neuron>(Item.ReplaceNeuron), 
                    e.OriginalSource, 
                    e.NewValue);
            }
        }
    }
}