// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   An undo data item that is used to handle changes with neurons themselves,
//   like delete or create.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An undo data item that is used to handle changes with neurons themselves,
    ///     like delete or create.
    /// </summary>
    /// <remarks>
    ///     Sometimes, these events are handled because the underlying collection
    ///     containers will also update the <see cref="Brain" /> . Often, the
    ///     designer provides direct commands to create or destroy neurons, these
    ///     commands also need to be undoable.
    /// </remarks>
    public class NeuronUndoItem : BrainUndoItem
    {
        #region Inner types

        /// <summary>
        ///     Stores info about a link so that it can be recreated easily.
        /// </summary>
        private class LinkInfo
        {
            /// <summary>
            ///     Gets/sets the meaning that was assigned to the link.
            /// </summary>
            /// <remarks>
            ///     This is a neuron, not an id because this is undo data. If the other
            ///     neuron would also be destroyed, it's id could get lost. This is not
            ///     a problem for other parts of the system, but the undo data needs to
            ///     correctly relink to the same neuron (in case they also got deleted
            ///     and restored), not id.
            /// </remarks>
            public Neuron Meaning { get; set; }

            /// <summary>
            ///     Gets/sets the other side of the link.
            /// </summary>
            public Neuron To { get; set; }

            /// <summary>
            ///     Gets/sets the list of info items that were assigned to the link.
            /// </summary>
            public System.Collections.Generic.List<Neuron> Info { get; set; }
        }

        /// <summary>
        ///     Stores info about the clusters in which an item that was removed
        ///     belonged.
        /// </summary>
        private class ClusterInfo
        {
            /// <summary>
            ///     Gets or sets the cluster in which the item belonged.
            /// </summary>
            /// <value>
            ///     The cluster.
            /// </value>
            public NeuronCluster Cluster { get; set; }

            /// <summary>
            ///     Gets or sets the index at which the item was located in the child
            ///     list.
            /// </summary>
            /// <value>
            ///     The index.
            /// </value>
            public int Index { get; set; }
        }

        #endregion

        #region Fields

        /// <summary>The f links in.</summary>
        private readonly System.Collections.Generic.List<LinkInfo> fLinksIn =
            new System.Collections.Generic.List<LinkInfo>();

        /// <summary>The f links out.</summary>
        private readonly System.Collections.Generic.List<LinkInfo> fLinksOut =
            new System.Collections.Generic.List<LinkInfo>();

        /// <summary>The f clustered by.</summary>
        private readonly System.Collections.Generic.List<ClusterInfo> fClusteredBy =
            new System.Collections.Generic.List<ClusterInfo>();

                                                                      // keeps track of all the clusters that this belonged to, so we can recreate.

        /// <summary>The f children.</summary>
        private System.Collections.Generic.List<Neuron> fChildren;

                                                        // if the neuron is a cluster, we also keep track of all the children it had, so we can reassign them.

        /// <summary>The f neuron.</summary>
        private Neuron fNeuron;

        /// <summary>The f blob.</summary>
        private object fBlob;

        /// <summary>The f meaning.</summary>
        private Neuron fMeaning;

        #endregion

        #region Prop

        #region Neuron

        /// <summary>
        ///     Gets or sets the neuron that is involved with the change.
        /// </summary>
        /// <remarks>
        ///     we keep a reference to this object so we can use the same physical
        ///     object again without having to create a new one.
        /// </remarks>
        /// <value>
        ///     The neuron.
        /// </value>
        public Neuron Neuron
        {
            get
            {
                return fNeuron;
            }

            set
            {
                if (fNeuron != value)
                {
                    fLinksIn.Clear();
                    fLinksOut.Clear();
                    fChildren = null;
                    fBlob = null;
                    fMeaning = null;

                    fNeuron = value;
                    if (value != null)
                    {
                        StoreLinksIn(value);
                        StoreLinksOut(value);
                        StoreClusteredBy(value);
                        StoreClusterData(value as NeuronCluster);
                        StoreValue(value as ValueNeuron);
                        var iData = BrainData.Current.NeuronInfo[fNeuron.ID];

                            // we always need to get the neurondata item, cause a neuron delete will also remove the data object.
                        if (iData.IsChanged || iData.NeedsPersisting)
                        {
                            // also store the extra project info if it changed/needs persisting.
                            Data = iData;
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the <see cref="ID" /> of the neuron that was changed.
        /// </summary>
        /// <remarks>
        ///     Not really required, but useful to restore to exactly the same id.
        /// </remarks>
        /// <value>
        ///     The ID.
        /// </value>
        public ulong ID { get; set; }

        /// <summary>
        ///     Gets the project data that is associated with the
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public NeuronData Data { get; private set; }

        #endregion

        #region Store data

        /// <summary>The store value.</summary>
        /// <param name="value">The value.</param>
        private void StoreValue(ValueNeuron value)
        {
            if (value != null)
            {
                fBlob = value.Blob;
            }
        }

        /// <summary>The store cluster data.</summary>
        /// <param name="value">The value.</param>
        private void StoreClusterData(NeuronCluster value)
        {
            if (value != null)
            {
                fChildren = new System.Collections.Generic.List<Neuron>();
                if (value.Meaning != Neuron.EmptyId)
                {
                    fMeaning = Brain.Current[value.Meaning];

                        // we store the meaning as a neuron cause the id can change if this neuron is deleted but later recovered.
                }

                if (value.Meaning != (ulong)PredefinedNeurons.Time)
                {
                    using (var iList = value.Children)
                        foreach (var i in iList)
                        {
                            fChildren.Add(Brain.Current[i]);
                        }
                }
                else
                {
                    fBlob = Time.GetTime(value);

                        // we store the DateTime object, so we can recreated the dateTime exactly as normal.
                    var iList = value.Children;
                    iList.Lock();
                    try
                    {
                        for (var i = 5; i < value.ChildrenDirect.Count; i++)
                        {
                            // could be that there were extra children in the time cluster, so store a ref to them as well.
                            fChildren.Add(Brain.Current[value.ChildrenDirect[i]]);
                        }
                    }
                    finally
                    {
                        iList.Dispose();
                    }
                }
            }
        }

        /// <summary>The store clustered by.</summary>
        /// <param name="value">The value.</param>
        private void StoreClusteredBy(Neuron value)
        {
            if (value.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = value.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                foreach (var iCluster in iClusters)
                {
                    // keep track of the clusters that own this neuron as a child so we can add again later on.
                    if (iCluster != null)
                    {
                        ClusterInfo iInfo;
                        using (var iList = iCluster.Children) iInfo = new ClusterInfo { Cluster = iCluster, Index = iList.IndexOf(value.ID) };
                        fClusteredBy.Add(iInfo);
                    }
                }

                Factories.Default.CLists.Recycle(iClusters);
            }
        }

        /// <summary>The store links out.</summary>
        /// <param name="value">The value.</param>
        private void StoreLinksOut(Neuron value)
        {
            if (value.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                try
                {
                    using (var iList = value.LinksOut) iLinks.AddRange(iList); // make local copy so we can savely write to cache.
                    foreach (var i in iLinks)
                    {
                        var iNew = new LinkInfo { Meaning = i.Meaning, To = i.To };
                        if (iNew.Info != null)
                        {
                            iNew.Info = i.Info.ConvertTo<Neuron>();
                        }

                        fLinksOut.Add(iNew);
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }
        }

        /// <summary>The store links in.</summary>
        /// <param name="value">The value.</param>
        private void StoreLinksIn(Neuron value)
        {
            if (value.LinksInIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer(); // make local copy so we can savely write to cache.
                try
                {
                    using (var iList = value.LinksIn) iLinks.AddRange(iList);
                    foreach (var i in iLinks)
                    {
                        var iNew = new LinkInfo { Meaning = i.Meaning, To = i.From };
                        if (iNew.Info != null)
                        {
                            iNew.Info = i.Info.ConvertTo<Neuron>();
                        }

                        fLinksIn.Add(iNew);
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }
        }

        #endregion

        #region Overrides

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            switch (Action)
            {
                case BrainAction.Created: // we need to remove it from the brain.
                    var iUndoData = new NeuronUndoItem { Neuron = Neuron, ID = Neuron.ID, Action = BrainAction.Removed };
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    Brain.Current.Delete(Neuron);
                    break;
                case BrainAction.Removed:

                    // we need to recreate it, preferebly using the same object with the same ID.  Normally, this is not a problem, but it can be (when the brain was running and it already consumed the ID).  In that case we use a new ID, which is still ok, cause most lists will create new viewer objects anyway (like mindmap and code editor).
                    RestoreNeuron();

                        // important to call restore last, otherwise the recreation of the links will mess up the order of some things.
                    iUndoData = new NeuronUndoItem { Neuron = Neuron, ID = Neuron.ID, Action = BrainAction.Created };
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    break;
                default:
                    throw new System.InvalidOperationException(
                        string.Format("Unsuported BrainAction type: {0}.", Action));
            }
        }

        /// <summary>
        ///     Restores the neuron's <see langword="internal" /> data like links and
        ///     cluster relationships.
        /// </summary>
        private void RestoreNeuron()
        {
            Brain.Current.Add(Neuron, ID);
            if (Data != null)
            {
                BrainData.Current.NeuronInfo.AddItem(Neuron.ID, Data); // also restore the project data for the neuron.
            }

            foreach (var i in fClusteredBy)
            {
                if (i.Cluster.ID != Neuron.EmptyId)
                {
                    i.Cluster.ChildrenW.Insert(i.Index, Neuron.ID);
                }
            }

            foreach (var i in fLinksIn)
            {
                if (i.To.ID != Neuron.EmptyId && i.Meaning.ID != Neuron.EmptyId)
                {
                    var iLink = new Link(Neuron, i.To, i.Meaning);
                    if (i.Info != null)
                    {
                        var iList = iLink.InfoW;
                        iList.AddRange(i.Info);
                    }
                }
            }

            foreach (var i in fLinksOut)
            {
                if (i.To.ID != Neuron.EmptyId && i.Meaning.ID != Neuron.EmptyId)
                {
                    var iLink = new Link(i.To, Neuron, i.Meaning);
                    if (i.Info != null)
                    {
                        var iList = iLink.InfoW;
                        iList.AddRange(i.Info);
                    }
                }
            }

            if (fChildren != null)
            {
                var iCluster = (NeuronCluster)Neuron;
                if (fMeaning != null)
                {
                    iCluster.Meaning = fMeaning.ID;
                }
                else
                {
                    iCluster.Meaning = Neuron.EmptyId;
                }

                if (fMeaning.ID == (ulong)PredefinedNeurons.Time)
                {
                    Time.Current.FillTimeCluster((System.DateTime)fBlob, iCluster);
                }

                var iList = iCluster.ChildrenW;
                iList.AddRange(fChildren);
            }
            else if (Neuron is ValueNeuron)
            {
                // the cluster also uses the blob, but for something else.
                ((ValueNeuron)Neuron).Blob = fBlob;
            }
        }

        #endregion
    }
}