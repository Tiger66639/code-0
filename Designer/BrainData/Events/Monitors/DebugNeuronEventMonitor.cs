// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugNeuronEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The debug neuron event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The debug neuron event monitor.</summary>
    internal class DebugNeuronEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="DebugNeuronEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public DebugNeuronEventMonitor(DebugNeuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public DebugNeuron Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (DebugNeuron)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            ToChanged(link, Neuron.EmptyId);
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            ToChanged(link, Neuron.EmptyId);
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (iItem.LinksOut != null)
                {
                    if (iItem.LinksOut.IsLoaded)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<DebugLink>(iItem.LinksOut.Children.Add), 
                            new DebugLink(link, iItem));
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action(iItem.LinksOut.UpdateHasChildren));
                    }
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void ToLinkRemoved(Link link)
        {
            FromChanged(link, Neuron.EmptyId);
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void ToLinkCreated(Link link)
        {
            FromChanged(link, Neuron.EmptyId);
        }

        /// <summary>The from changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void FromChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (iItem.LinksIn != null)
                {
                    if (iItem.LinksIn.IsLoaded)
                    {
                        var iFound =
                            (from i in iItem.LinksIn.Children where ((DebugLink)i).Item == link select i).FirstOrDefault
                                ();
                        if (iFound != null)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Func<DebugRef, bool>(iItem.LinksIn.Children.Remove), 
                                iFound);
                        }
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action(iItem.LinksIn.UpdateHasChildren));
                    }
                }
            }
        }

        /// <summary>The neuron changed.</summary>
        /// <param name="e">The e.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            try
            {
                var iItem = Item;
                if (iItem != null)
                {
                    var iPropArgs = e as NeuronPropChangedEventArgs;
                    if (iPropArgs != null)
                    {
                        if (iPropArgs.Property == "Meaning")
                        {
                            var iCluster = (NeuronCluster)e.OriginalSource;
                            if (Brain.Current.IsValidID(iCluster.Meaning))
                            {
                                // a meaning for a cluster can be null, or some invalid value.
                                iItem.SetClusterMeaning(Brain.Current[iCluster.Meaning]);
                            }
                        }
                    }
                    else
                    {
                        if (e.Action == BrainAction.Removed)
                        {
                            iItem.IsDeleted = true; // prop setters can be done across threads.
                        }
                        else if (e.Action == BrainAction.Created)
                        {
                            iItem.IsTemp = false;
                            iItem.IsDeleted = false;
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<Neuron>(iItem.ReplaceNeuron), 
                                e.NewValue);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogService.Log.LogError("DebugNeuronEventMonitor.NeuronChanged", ex.ToString());
            }
        }

        /// <summary>The list changed.</summary>
        /// <param name="e">The e.</param>
        public override void ListChanged(NeuronListChangedEventArgs e)
        {
            if (e.ListType == typeof(ChildList))
            {
                ChildrenChanged(e);
            }
            else
            {
                ClusteredByChanged(e);
            }
        }

        /// <summary>The clustered by changed.</summary>
        /// <param name="e">The e.</param>
        private void ClusteredByChanged(NeuronListChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem.ClusteredBy != null)
            {
                if (iItem.ClusteredBy.IsLoaded)
                {
                    switch (e.Action)
                    {
                        case NeuronListChangeAction.Insert:
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<int, DebugRef>(iItem.ClusteredBy.Children.Insert), 
                                e.Index, 
                                new DebugChild(e.Item, iItem));
                            break;
                        case NeuronListChangeAction.Remove:
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<int>(iItem.ClusteredBy.Children.RemoveAt), 
                                e.Index);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(iItem.ClusteredBy.UpdateHasChildren));
                }
            }
        }

        /// <summary>The children changed.</summary>
        /// <param name="e">The e.</param>
        private void ChildrenChanged(NeuronListChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem.Children != null)
            {
                if (iItem.Children.IsLoaded)
                {
                    switch (e.Action)
                    {
                        case NeuronListChangeAction.Insert:
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<int, DebugRef>(iItem.Children.Children.Insert), 
                                e.Index, 
                                new DebugChild(e.Item, iItem));
                            break;
                        case NeuronListChangeAction.Remove:
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<int>(iItem.Children.Children.RemoveAt), 
                                e.Index);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(iItem.Children.UpdateHasChildren));
                }
            }
        }
    }
}