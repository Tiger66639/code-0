// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubRelationShipsCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   event monintor for all the sub collections of the selected thesaurus
//   item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     event monintor for all the sub collections of the selected thesaurus
    ///     item.
    /// </summary>
    internal class SubRelationShipsCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="SubRelationShipsCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public SubRelationShipsCollectionEventMonitor(SubRelationshipsCollection toWrap)
            : base(toWrap)
        {
            EventManager.Current.RegisterForLinksOut(this, toWrap.Item.ID);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public SubRelationshipsCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (SubRelationshipsCollection)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            var iItem = Item;
            if (iItem != null && iItem.IsLoaded)
            {
                var iRel = (from i in BrainData.Current.Thesaurus.NoRecursiveRelationships
                            where i.Item != null && i.Item.ID == link.MeaningID
                            select i.Item).FirstOrDefault();
                if (iRel != null)
                {
                    var iNew = new ThesaurusSubItemCollection(iItem, link.To as NeuronCluster, iRel);
                    iItem.Add(iNew);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value of the from part in the link.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            var iItem = Item;
            if (iItem != null && iItem.IsLoaded)
            {
                var iFound = (from i in iItem where i.Cluster.ID == link.ToID select i).FirstOrDefault();
                if (iFound != null)
                {
                    iItem.Remove(iFound);
                }
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'to' value.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null && iItem.IsLoaded)
            {
                var iFound = (from i in iItem where i.Cluster.ID == oldVal select i).FirstOrDefault();
                if (iFound != null)
                {
                    var iNew = new ThesaurusSubItemCollection(iItem, link.To as NeuronCluster, iFound.Relationship);
                    iItem[iItem.IndexOf(iFound)] = iNew;
                }
                else
                {
                    iFound = (from i in iItem where i.Relationship.ID == oldVal select i).FirstOrDefault();
                    if (iFound != null)
                    {
                        var iNewRel =
                            (from i in BrainData.Current.Thesaurus.NoRecursiveRelationships
                             where i.Item.ID == link.MeaningID
                             select i.Item).FirstOrDefault();
                        if (iNewRel != null)
                        {
                            iFound.Relationship = iNewRel;
                        }
                        else
                        {
                            iItem.Remove(iFound);
                        }
                    }
                }
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
                    if (iItem != null && iItem.IsLoaded)
                    {
                        switch (e.Action)
                        {
                            case BrainAction.Created:

                                // don't do anything
                                break;
                            case BrainAction.Changed:
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.Normal, 
                                    new System.Action<Neuron, Neuron>(iItem.Replace), 
                                    e.OriginalSource, 
                                    e.NewValue);
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
            var iItems =
                (from i in Item where i.Relationship != null select i.Relationship.ID).Union(
                    from u in Item where u.Cluster != null select u.Cluster.ID);
            EventManager.Current.RemoveNeuronChangedMonitor(this, iItems);
        }

        /// <summary>The add item.</summary>
        /// <param name="item">The item.</param>
        internal void AddItem(ThesaurusSubItemCollection item)
        {
            if (item.Relationship != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(this, item.Relationship.ID);
            }

            if (item.Cluster != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(this, item.Cluster.ID);
            }
        }

        /// <summary>The remove item.</summary>
        /// <param name="item">The item.</param>
        internal void RemoveItem(ThesaurusSubItemCollection item)
        {
            if (item.Relationship != null)
            {
                EventManager.Current.RemoveNeuronChangedMonitor(this, item.Relationship.ID);
            }

            if (item.Cluster != null)
            {
                EventManager.Current.RemoveNeuronChangedMonitor(this, item.Cluster.ID);
            }
        }
    }
}