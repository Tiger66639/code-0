// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusItemCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides data sync with the network for the root list of thesaurus items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Provides data sync with the network for the root list of thesaurus items.
    /// </summary>
    /// <remarks>
    ///     This is a direct monitor on the brain, we don't pass through the event
    ///     manager since we need to inspect a bit different and more optimal: the
    ///     root can contain a lot of items and we only need to inspect a couple of
    ///     data items, on every neuron.
    /// </remarks>
    internal class ThesaurusItemCollectionEventMonitor
    {
        /// <summary>The f default.</summary>
        private static readonly ThesaurusItemCollectionEventMonitor fDefault = new ThesaurusItemCollectionEventMonitor();

        /// <summary>The f lookup.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, ThesaurusItem> fLookup =
            new System.Collections.Generic.Dictionary<ulong, ThesaurusItem>();

                                                                                     // stores the thes items in the object that we provide the service for. can't use index, if we remove an item we would have to build the entire dict, so use the objects instead.

        /// <summary>The f sub lists.</summary>
        private readonly System.Collections.Generic.List<ThesChildItemsCollection> fSubLists =
            new System.Collections.Generic.List<ThesChildItemsCollection>();

                                                                                   // we also need to let children of expanded items know when they might have a children or not.

        /// <summary>Prevents a default instance of the <see cref="ThesaurusItemCollectionEventMonitor"/> class from being created.</summary>
        private ThesaurusItemCollectionEventMonitor()
        {
            Brain.Current.Cleared += Brain_Cleared;
            Brain.Current.LinkChanged += Brain_LinkChanged;
            Brain.Current.NeuronListChanged += Brain_NeuronListChanged;
            Brain.Current.NeuronChanged += Brain_NeuronChanged;
        }

        /// <summary>
        ///     Gets the default event monitor for the thesaurus.
        /// </summary>
        /// <value>
        ///     The default.
        /// </value>
        public static ThesaurusItemCollectionEventMonitor Default
        {
            get
            {
                return fDefault;
            }
        }

        /// <summary>Handles the NeuronChanged event of the <see cref="Brain"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Brain_NeuronChanged(object sender, NeuronChangedEventArgs e)
        {
            if (!(e is NeuronPropChangedEventArgs) && BrainData.Current.Thesaurus != null)
            {
                // can be null when loading from template.
                var iItems = BrainData.Current.Thesaurus.InternalItems;
                if (iItems != null)
                {
                    try
                    {
                        switch (e.Action)
                        {
                            case BrainAction.Created:

                                // don't do anything
                                break;
                            case BrainAction.Changed:
                                break;
                            case BrainAction.Removed:
                                ThesaurusItem iIndex;
                                if (fLookup.TryGetValue(e.OriginalSourceID, out iIndex))
                                {
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                        System.Windows.Threading.DispatcherPriority.Normal, 
                                        new System.Action<ThesaurusItem>(iItems.RemoveFromEvent), 
                                        iIndex); // event handler for the collectionChange will update the eventManager
                                    fLookup.Remove(e.OriginalSourceID);

                                        // we remove from here cause the item has been deleted, this is the only place frm where we still have the id.
                                }

                                break;
                            default:
                                throw new System.InvalidOperationException();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogService.Log.LogError("DebugNeuronEventMonitor.NeuronChanged", ex.ToString());
                    }
                }
            }
        }

        /// <summary>Handles the LinkChanged event of the <see cref="Brain"/> control.
        ///     Check if any of the root items changes it's link to the related items.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void Brain_LinkChanged(object sender, LinkChangedEventArgs e)
        {
            if (BrainData.Current != null && BrainData.Current.Thesaurus != null)
            {
                var iItems = BrainData.Current.Thesaurus.InternalItems;
                if (iItems != null)
                {
                    var iMeaning = BrainData.Current.Thesaurus.SelectedRelationship;
                    if (iMeaning != null)
                    {
                        if (e.NewMeaning == iMeaning.ID || e.OldMeaning == iMeaning.ID)
                        {
                            var iFrom = e.NewFrom != Neuron.EmptyId ? e.NewFrom : e.OldFrom;
                            ThesaurusItem iItem;
                            if (fLookup.TryGetValue(iFrom, out iItem))
                            {
                                iItem.CheckHasItems(iMeaning);

                                    // only need to update if it has any items. If the list of items is loaded, it will update any changes itself, except this.
                            }

                            foreach (var i in Enumerable.ToList(fSubLists))
                            {
                                // we make a copy of the list, cause the checkHasItems modifies fSubLists.
                                i.CheckHasItems(iFrom);
                            }
                        }
                        else if (e.NewMeaning == (ulong)PredefinedNeurons.POS
                                 || e.OldMeaning == (ulong)PredefinedNeurons.POS)
                        {
                            var iFrom = e.NewFrom != Neuron.EmptyId ? e.NewFrom : e.OldFrom;
                            ThesaurusItem iItem;
                            if (fLookup.TryGetValue(iFrom, out iItem))
                            {
                                BrainData.Current.Thesaurus.UpdateForFilter(iItem);
                            }
                        }
                        else if (e.NewMeaning == (ulong)PredefinedNeurons.TextPatternTopic
                                 || e.OldMeaning == (ulong)PredefinedNeurons.TextPatternTopic)
                        {
                            var iFrom = e.NewFrom != Neuron.EmptyId ? e.NewFrom : e.OldFrom;
                            ThesaurusItem iItem;
                            if (fLookup.TryGetValue(iFrom, out iItem))
                            {
                                iItem.HasTextPattern =
                                    iItem.Item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic) != null;
                            }
                            else
                            {
                                // it's perhaps in one of the sublists.
                                foreach (var i in fSubLists)
                                {
                                    foreach (var u in i)
                                    {
                                        if (u.Item != null && u.Item.ID == iFrom)
                                        {
                                            u.CheckEditors();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else if (e.NewMeaning == (ulong)PredefinedNeurons.Attribute
                                 || e.OldMeaning == (ulong)PredefinedNeurons.Attribute)
                        {
                            var iFrom = e.NewFrom != Neuron.EmptyId ? e.NewFrom : e.OldFrom;
                            ThesaurusItem iItem;
                            if (fLookup.TryGetValue(iFrom, out iItem))
                            {
                                iItem.ResetAttrib();
                            }
                            else
                            {
                                // it's perhaps in one of the sublists.
                                foreach (var i in fSubLists)
                                {
                                    foreach (var u in i)
                                    {
                                        if (u.Item != null && u.Item.ID == iFrom)
                                        {
                                            u.ResetAttrib();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Handles the NeuronListChanged event of the <see cref="Brain"/>
        ///     control. This is used to see if any of the thesaurus item's pos is
        ///     changed because it is removed/added to a different posgroup (and
        ///     doesn't have a pos out).</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NeuronListChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Brain_NeuronListChanged(object sender, NeuronListChangedEventArgs e)
        {
            if (BrainData.Current != null && BrainData.Current.Thesaurus != null)
            {
                var iItems = BrainData.Current.Thesaurus.InternalItems;
                if (iItems != null)
                {
                    // if nothing loaded, don't need to check.
                    var iPos = BrainData.Current.Thesaurus.SelectedPOSFilter;
                    if (iPos != null)
                    {
                        if (e.ListType == typeof(ClusterList))
                        {
                            var iCluster = e.Item as NeuronCluster;
                            if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.POSGroup)
                            {
                                ThesaurusItem iItem;
                                if (fLookup.TryGetValue(e.ListOwner.ID, out iItem))
                                {
                                    BrainData.Current.Thesaurus.UpdateForFilter(iItem);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Handles the Cleared event of the <see cref="Brain"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Brain_Cleared(object sender, System.EventArgs e)
        {
            Clear();
        }

        /// <summary>
        ///     Clears the list of neurons that we are monitoring.
        /// </summary>
        internal void Clear()
        {
            fLookup.Clear();
        }

        /// <summary>The add item.</summary>
        /// <param name="id">The id.</param>
        internal void AddItem(ThesaurusItem id)
        {
            fLookup.Add(id.ID, id);
        }

        /// <summary>The remove item.</summary>
        /// <param name="id">The id.</param>
        internal void RemoveItem(ulong id)
        {
            fLookup.Remove(id);
        }

        /// <summary>The add list.</summary>
        /// <param name="list">The list.</param>
        internal void AddList(ThesChildItemsCollection list)
        {
            fSubLists.Add(list);
        }

        /// <summary>The remove list.</summary>
        /// <param name="list">The list.</param>
        internal void RemoveList(ThesChildItemsCollection list)
        {
            fSubLists.Remove(list);
        }
    }
}