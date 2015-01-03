// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of <see cref="MindMapItem" /> s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection of <see cref="MindMapItem" /> s.
    /// </summary>
    /// <remarks>
    ///     This collection makes certain that whenever a <see cref="MindMapLink" />
    ///     is added, it is resolved. Whenever a neuron is added, all it's existing
    ///     links are drawn on the map, when it is removed, all it's links are also
    ///     removed.
    /// </remarks>
    public class MindMapItemCollection : Data.ObservedCollection<MindMapItem, MindMap>
    {
        /// <summary>The f event monitor.</summary>
        private readonly MindMapItemCollectionEventMonitor fEventMonitor;

        /// <summary><para>We only allow unique nuerons on a mindmap, so when we want to add a
        ///         neuron, check that it doesn't already exist.</para>
        /// <para>Only unique mindmap links are allowed, so if a duplicate is found, an
        ///         error is thrown.</para>
        /// <para>When a mind map link is added, we try to resolve it's neuron link so
        ///         it can get positional info from the mind map neurons it should link
        ///         to. We also make certain that all links are created for newly added
        ///         neuron objects. The ZIndex is also assigned, according to it's<paramref name="index"/> value.</para>
        /// </summary>
        /// <param name="index">The index at which the <paramref name="item"/> is inserted.</param>
        /// <param name="item">The item being added.</param>
        protected override void InsertItem(int index, MindMapItem item)
        {
            var iItem = item as MindMapLink;
            if (iItem != null)
            {
                var iFound = (from i in this
                              where
                                  i is MindMapLink && ((MindMapLink)i).To == iItem.To
                                  && ((MindMapLink)i).From == iItem.From && ((MindMapLink)i).Meaning == iItem.Meaning
                              select (MindMapLink)i).FirstOrDefault();
                if (iFound != null)
                {
                    ShowError(
                        string.Format(
                            "Link between '{0}' and '{1}' already on mind map!", 
                            iFound.ToMindMapItem.Item, 
                            iFound.FromMindMapItem.Item));
                    return;
                }

                if (iItem.Link == null)
                {
                    // if the link isn't resolved yet,do it now.  
                    if (iItem.ResolveLink(this) == false)
                    {
                        // If the link can't be resolved, we don't add the item.
                        LogService.Log.LogWarning("MindMapItemCollection.InsertItem", "Removed Link from mindmap.");
                        return;
                    }
                }

                fEventMonitor.AddLink(iItem.Link);
            }

            var iNeuron = item as MindMapNeuron;
            if (iNeuron != null)
            {
                // check if the operation is legal in case it is a neuron item (not yet on the mindmap).
                var iAlreadyOnMap =
                    (from i in this where i is MindMapNeuron && ((MindMapNeuron)i).ItemID == iNeuron.ItemID select i)
                        .FirstOrDefault();
                if (iAlreadyOnMap != null)
                {
                    ShowError(
                        string.Format(
                            "Neuron '{0}' already on mindmap, can't have duplicate items on map (otherwise links don't work properly).", 
                            iNeuron.Item));
                    return;
                }

                fEventMonitor.AddItem(iNeuron.ItemID);
            }

            if (Owner.CurrentState == EditorState.Loaded)
            {
                // when we are loading from xml, we don't want to adjust the ZIndex.
                item.ZIndex = index;
            }

            base.InsertItem(index, item);
            if (Owner != null && Owner.CurrentState == EditorState.Loaded)
            {
                // when loading, from disk, don't want to auto create existing links cause we are recovering a previous state, not building a new one.
                UpdateNeuronAfterInsert(iNeuron);
            }
        }

        /// <summary>Updates the mindmap <paramref name="neuron"/> after it has been
        ///     inserted to the list.</summary>
        /// <param name="neuron">The item.</param>
        private void UpdateNeuronAfterInsert(MindMapNeuron neuron)
        {
            if (neuron != null)
            {
                // we are adding a neuron, so check all it's to and from links for items that are already depicted on the map.
                var iExisting = (from i in this where i is MindMapNeuron select ((MindMapNeuron)i).ItemID).ToList();
                Owner.ShowExistingIn(neuron, iExisting, false);
                Owner.ShowExistingOut(neuron, iExisting, false);
                if (neuron is MindMapCluster)
                {
                    ((MindMapCluster)neuron).LoadChildren();

                        // if it's a cluster, we also need to search all the mindmap neurons that are a child of the cluster.                             
                }

                var iClusters = from i in this where i is MindMapCluster select (MindMapCluster)i;
                neuron.AssignToClusters(iClusters); // must also check if the newly added item belongs to any clusters
            }
        }

        /// <summary>Shows and logs the error message.</summary>
        /// <param name="msg">The MSG.</param>
        private void ShowError(string msg)
        {
            System.Windows.MessageBox.Show(
                msg, 
                "Invalid operation", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
            LogService.Log.LogError("MindMapItemCollection.InsertItem", msg);
        }

        /// <summary>must make certain that links are at the back.</summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (this[oldIndex] is MindMapLink)
            {
                while (newIndex < Count - 1 && !(this[newIndex] is MindMapLink))
                {
                    // move the index up untill we found something ok.
                    newIndex++;
                }
            }

            base.MoveItem(oldIndex, newIndex);
        }

        /// <summary>check if we are removing a neuron, if so remove all it's links as
        ///     well. If we are removing a link, let it know, so it can properly clean
        ///     up an events or other.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iToRemove = this[index];
            RemoveRelated(iToRemove, false);

                // needs to be done after the remove itself, so it is also undone in the  correct order: a delete is always redone in the same order as that it was recorded.
            base.RemoveItem(index);

                // this needs to be done after all links are removed for the undo system (this reverses the action, so the links must be removed before the neuron, so that they are recreated after it.
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            var iToRemove = this[index];
            RemoveRelated(iToRemove, true);

                // needs to be done after the remove itself, so it is also undone in the  correct order: a delete is always redone in the same order as that it was recorded.
            base.RemoveItemDirect(index);
        }

        /// <summary>The remove related.</summary>
        /// <param name="iToRemove">The i to remove.</param>
        /// <param name="asDirect">The as direct.</param>
        private void RemoveRelated(MindMapItem iToRemove, bool asDirect)
        {
            var iNeuron = iToRemove as MindMapNeuron;
            if (iNeuron != null)
            {
                // need to remove all the links that this point refers to.
                RemoveRelated(iNeuron, asDirect);
                fEventMonitor.RemoveItem(iNeuron.ItemID);
            }
            else
            {
                RemoveRelated(iToRemove as MindMapLink);
            }
        }

        /// <summary>The remove related.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="asDirect">The as direct.</param>
        private void RemoveRelated(MindMapNeuron neuron, bool asDirect)
        {
            var i = 0;
            while (i < Count)
            {
                var iItem = this[i] as MindMapLink;
                if (iItem != null && (iItem.To == neuron.ItemID || iItem.From == neuron.ItemID))
                {
                    RemoveRelated(iItem);
                    if (asDirect)
                    {
                        RemoveItemDirect(i);
                    }
                    else
                    {
                        RemoveAt(i);
                    }
                }
                else
                {
                    if (neuron.ChildCount > 0)
                    {
                        // if this item belongs to a mindmap cluster, we also need to brake that relationship.
                        var iCluster = this[i] as MindMapCluster;
                        if (iCluster != null)
                        {
                            iCluster.Children.Remove(neuron);
                        }
                    }

                    i++;
                }
            }
        }

        /// <summary>The remove related.</summary>
        /// <param name="ilink">The ilink.</param>
        private void RemoveRelated(MindMapLink ilink)
        {
            if (ilink != null)
            {
                ilink.DisconnectLink();
                fEventMonitor.RemoveLink(ilink.Link);
            }
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, MindMapItem item)
        {
            // this is required, when we are changing the neuron from one type to anotherone.
            base.SetItem(index, item);
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>Removes the neuron because it was deleted from the network (called by
        ///     the event monitor).</summary>
        /// <param name="item">The item.</param>
        internal void RemoveNeuron(Neuron item)
        {
            var iNeuron =
                (from i in this where i is MindMapNeuron && ((MindMapNeuron)i).Item == item select (MindMapNeuron)i)
                    .FirstOrDefault();
            if (iNeuron != null)
            {
                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    Remove(iNeuron);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action<int>(RemoveItemDirect), 
                        IndexOf(iNeuron)); // needs to be done async for the ui to respond (wpf requirement)
                }
            }
        }

        /// <summary>Removes the link because it was deleted from the network (called by
        ///     the event monitor).</summary>
        /// <param name="item">The item.</param>
        internal void RemoveLink(Link item)
        {
            var iLink =
                (from i in this where i is MindMapLink && ((MindMapLink)i).Link == item select (MindMapLink)i)
                    .FirstOrDefault();
            if (iLink != null)
            {
                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    Remove(iLink);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action<int>(RemoveItemDirect), 
                        IndexOf(iLink)); // needs to be done async for the ui to respond (wpf requirement)
                }
            }
        }

        /// <summary>Updates from part of the link that was changed. If the new from isn't
        ///     present on the mind map, the link is removed.</summary>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        internal void UpdateFromPart(LinkChangedEventArgs e)
        {
            var iLink =
                (from i in this
                 where i is MindMapLink && ((MindMapLink)i).Link == e.OriginalSource
                 select (MindMapLink)i).FirstOrDefault();
            if (iLink != null)
            {
                var iNeuron =
                    (from i in this
                     where i is MindMapNeuron && ((MindMapNeuron)i).Item.ID == e.NewFrom
                     select (MindMapNeuron)i).FirstOrDefault();
                if (iNeuron != null)
                {
                    iLink.UpdateFromMindMapItem(iNeuron);
                }
                else if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    Remove(iLink);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Func<MindMapItem, bool>(Remove), 
                        iLink); // needs to be done async for the ui to respond (wpf requirement)
                }
            }
        }

        /// <summary>Updates 'To' part of the link that was changed. If the new 'To' isn't
        ///     present on the mind map, the link is removed.</summary>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        internal void UpdateToPart(LinkChangedEventArgs e)
        {
            var iLink =
                (from i in this
                 where i is MindMapLink && ((MindMapLink)i).Link == e.OriginalSource
                 select (MindMapLink)i).FirstOrDefault();
            if (iLink != null)
            {
                var iNeuron =
                    (from i in this
                     where i is MindMapNeuron && ((MindMapNeuron)i).Item.ID == e.NewTo
                     select (MindMapNeuron)i).FirstOrDefault();
                if (iNeuron != null)
                {
                    iLink.UpdateToMindMapItem(iNeuron);
                }
                else if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    Remove(iLink);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Func<MindMapItem, bool>(Remove), 
                        iLink); // needs to be done async for the ui to respond (wpf requirement)
                }
            }
        }

        /// <summary>Updates the items in response to a changed neuron.</summary>
        /// <remarks>When a neuron is changed, we need to create a new mindmap neuron cause
        ///     it could have been or could become a cluster, which uses a different
        ///     class. Because of this, we must also update all references to this
        ///     mindmap neuron in the links and MindMapClusters.</remarks>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        internal void UpdateItemsForNeuronChange(Neuron old, Neuron value)
        {
            var iNeuron =
                (from i in this where i is MindMapNeuron && ((MindMapNeuron)i).Item == old select (MindMapNeuron)i)
                    .FirstOrDefault();
            if (iNeuron != null)
            {
                var iNew = MindMapNeuron.CreateFor(value);

                    // we need to create a new one because a cluster uses a different class.
                iNew.CopyValues(iNeuron);
                var iLinks = (from i in this

                              // need to convert to list, cause we will be modifying the list in the loop.
                              where i is MindMapLink && ((MindMapLink)i).ToMindMapItem == iNeuron
                              select (MindMapLink)i).ToList();
                foreach (var i in iLinks)
                {
                    // update 'To' part of all links
                    i.UpdateToMindMapItem(iNew);
                }

                iLinks =
                    (from i in this
                     where i is MindMapLink && ((MindMapLink)i).FromMindMapItem == iNeuron
                     select (MindMapLink)i).ToList();
                foreach (var i in iLinks)
                {
                    // updat 'from' part of all links.
                    i.UpdateFromMindMapItem(iNew);
                }

                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    UpdateItemsForChange(iNeuron, iNew);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action<MindMapNeuron, MindMapNeuron>(UpdateItemsForChange), 
                        iNeuron, 
                        iNew);
                }
            }
        }

        /// <summary>The second part of a neuron change, which has to be done in the ui
        ///     thread.</summary>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        private void UpdateItemsForChange(MindMapNeuron old, MindMapNeuron value)
        {
            var iClusters = (from i in this

                             // again, need to convert, cause we will change the list in the loop.
                             where i is MindMapCluster

                             // note: we don't filter on wether the cluster has the item as child, we simply let all the clusters handle this.  This is because the cluster needs to get the index of the child, which is the same CPU hit as checking for 'contains'.
                             select (MindMapCluster)i).ToList();

            foreach (var i in iClusters)
            {
                i.ReplaceChild(old, value);
            }

            this[IndexOf(old)] = value;
            old.Item = null;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMapItemCollection"/> class. Initializes a new instance of the <see cref="MindMapItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public MindMapItemCollection(MindMap owner)
            : base(owner)
        {
            fEventMonitor = new MindMapItemCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="MindMapItemCollection"/> class. 
        ///     Initializes a new instance of the <see cref="MindMapItemCollection"/>
        ///     class.</summary>
        public MindMapItemCollection()
        {
            fEventMonitor = new MindMapItemCollectionEventMonitor(this);
        }

        #endregion
    }
}