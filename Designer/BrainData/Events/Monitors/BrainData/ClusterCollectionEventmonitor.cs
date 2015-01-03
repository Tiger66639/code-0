// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterCollectionEventmonitor.cs" company="">
//   
// </copyright>
// <summary>
//   provides logic to monitor the brain for all things intersted to to a
//   cluster collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>provides logic to monitor the brain for all things intersted to to a
    ///     cluster collection.</summary>
    /// <remarks>This contains any changes in the list, but also changes to the items in
    ///     the list (the object has been changed for a certain id due to a change or
    ///     similar).</remarks>
    /// <typeparam name="T"></typeparam>
    internal class ClusterCollectionEventmonitor<T> : EventMonitor
        where T : INeuronWrapper
    {
        /// <summary>Initializes a new instance of the <see cref="ClusterCollectionEventmonitor{T}"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ClusterCollectionEventmonitor(ClusterCollection<T> toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public ClusterCollection<T> Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (ClusterCollection<T>)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Action == BrainAction.Changed && !(e is NeuronPropChangedEventArgs))
            {
                var iItem = Item;
                if (iItem != null)
                {
                    // do a check, just in case it already went blanc.
                    if (e.OriginalSource != iItem.Cluster)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Neuron, Neuron>(iItem.NeuronChanged), 
                            e.OriginalSource, 
                            e.NewValue);
                    }
                    else
                    {
                        iItem.ReplaceCluster((NeuronCluster)e.NewValue);
                    }
                }
            }
        }

        /// <summary>The list changed.</summary>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void ListChanged(NeuronListChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NeuronListChangeAction.Insert:
                    EventManager.Current.AddNeuronChangedMonitor(this, e.Item.ID);
                    break;
                case NeuronListChangeAction.Remove:
                    int iContains;
                    if (e.Item != null)
                    {
                        // can be null when we are doing a database cleanup and we found an invalid child.
                        var iList = (NeuronAccessor)e.List;
                        iContains = Enumerable.Count(
                            (System.Collections.Generic.IList<ulong>)iList, 
                            i => i == e.Item.ID); // the enumerator takes care of the lock.
                        if (iContains == 1)
                        {
                            // to trap for multiple occurance of the same item in a list.
                            EventManager.Current.RemoveNeuronChangedMonitor(this, e.Item.ID);
                        }
                    }

                    break;
                default:
                    throw new System.InvalidOperationException("Unkown list change.");
            }

            var iItem = Item;
            if (iItem != null && iItem.IsInternalChange == false)
            {
                // do a check, just in case it already went blanc + for an internalChange, we don't burden the main ui thread.
                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    // important: when in UI thread, change the list immediate, without delay. Otherwise, we screw up the undo system. Also, this is faster.
                    iItem.NeuronlistChanged(e);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action<NeuronListChangedEventArgs>(iItem.NeuronlistChanged), 
                        e);
                }
            }
        }

        /// <summary>The from link created.</summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            var iItem = Item;
            if (iItem.UseFirstOut)
            {
                if (iItem != null && link.ToID != iItem.Cluster.ID && link.MeaningID == iItem.MeaningID)
                {
                    // don't call when the initial link is created when the first item is added to the collection.
                    var iCluster = link.From.FindFirstOut(link.MeaningID) as NeuronCluster;

                        // check if there is a new 'First link', If there is, use that one.
                    if (iCluster != null)
                    {
                        iItem.UpdateCluster(iCluster);
                    }
                    else
                    {
                        iItem.RemoveCluster(link.MeaningID);
                    }
                }
            }
        }

        /// <summary>The from link removed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            var iItem = Item;
            if (iItem != null && iItem.Item.ID == oldValue)
            {
                if (iItem.UseFirstOut)
                {
                    var iCluster = link.From.FindFirstOut(link.MeaningID) as NeuronCluster;

                        // check if there is a new 'First link', If there is, use that one.
                    if (iCluster != null)
                    {
                        iItem.UpdateCluster(iCluster);
                    }
                    else
                    {
                        iItem.RemoveCluster(link.MeaningID);
                    }
                }
                else
                {
                    iItem.RemoveCluster(link.MeaningID);
                }
            }
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null && iItem.Cluster.ID == oldVal)
            {
                // only update when the link to the old cluster is changed.
                iItem.UpdateCluster(link.To as NeuronCluster);
            }
        }
    }
}