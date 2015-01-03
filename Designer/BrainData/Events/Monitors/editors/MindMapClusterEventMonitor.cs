// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapClusterEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The mind map cluster event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The mind map cluster event monitor.</summary>
    internal class MindMapClusterEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="MindMapClusterEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public MindMapClusterEventMonitor(MindMapCluster toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public MindMapCluster Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (MindMapCluster)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e is NeuronPropChangedEventArgs)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<NeuronPropChangedEventArgs>(Item.PropChanged), 
                    e);
            }
        }

        /// <summary>The list changed.</summary>
        /// <param name="e">The e.</param>
        public override void ListChanged(NeuronListChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<NeuronListChangedEventArgs>(Item.ChildrenChanged), 
                e);
        }
    }
}