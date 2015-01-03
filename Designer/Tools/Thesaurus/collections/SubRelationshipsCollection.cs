// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubRelationshipsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of thesaurusSubItem collections. It contains all the
//   relationships of an object that are non recursive.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection of thesaurusSubItem collections. It contains all the
    ///     relationships of an object that are non recursive.
    /// </summary>
    public class SubRelationshipsCollection :
        System.Collections.ObjectModel.ObservableCollection<ThesaurusSubItemCollection>, 
        INeuronInfo, 
        INeuronWrapper
    {
        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f selected item.</summary>
        private ThesaurusSubItemCollection fSelectedItem;

        /// <summary>The f event monitor.</summary>
        private readonly SubRelationShipsCollectionEventMonitor fEventMonitor;

        /// <summary>Initializes a new instance of the <see cref="SubRelationshipsCollection"/> class. Initializes a new instance of the<see cref="SubRelationshipsCollection"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public SubRelationshipsCollection(Neuron toWrap)
        {
            Item = toWrap;
            fEventMonitor = new SubRelationShipsCollectionEventMonitor(this);

                // we add and remove items as needed to the event monitor, when items are loaded.
        }

        #region SelectedItem

        /// <summary>
        ///     Gets/sets the currently selected item in the list. Note: no checks are
        ///     done, this is simply to support the ui, please make certain that the
        ///     assigned item is in this list.
        /// </summary>
        public ThesaurusSubItemCollection SelectedItem
        {
            get
            {
                return fSelectedItem;
            }

            set
            {
                fSelectedItem = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedItem"));
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item.ID];
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #endregion

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ThesaurusSubItemCollection item)
        {
            if (item.Relationship != null)
            {
                fEventMonitor.AddItem(item);
            }

            base.InsertItem(index, item);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (iItem.Relationship != null)
            {
                fEventMonitor.RemoveItem(iItem);
            }

            base.RemoveItem(index);
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ThesaurusSubItemCollection item)
        {
            var iItem = this[index];
            if (iItem.Relationship != null)
            {
                fEventMonitor.RemoveItem(iItem);
            }

            if (item.Relationship != null)
            {
                fEventMonitor.AddItem(item);
            }

            base.SetItem(index, item);
        }

        /// <summary>The remove all.</summary>
        /// <param name="item">The item.</param>
        public void RemoveAll(Neuron item)
        {
            var i = 0;
            while (i < Count)
            {
                if (this[i].Relationship == item || this[i].Cluster == item)
                {
                    RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>The replace.</summary>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        public void Replace(Neuron old, Neuron value)
        {
            for (var i = 0; i < Count; i++)
            {
                var iItem = this[i];
                if (iItem.Relationship == old)
                {
                    iItem.Relationship = value;
                }

                if (iItem.Cluster == old)
                {
                    this[i] = new ThesaurusSubItemCollection(this, (NeuronCluster)value, this[i].Relationship);
                }
            }
        }

        #region IsLoaded

        /// <summary>
        ///     Gets/sets the wether the sub items are loaded or not.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return fIsLoaded;
            }

            set
            {
                if (value != fIsLoaded)
                {
                    fIsLoaded = value;
                    if (value)
                    {
                        LoadItems();
                    }
                    else
                    {
                        Clear();
                    }

                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsLoaded"));
                }
            }
        }

        /// <summary>
        ///     looks up
        /// </summary>
        private void LoadItems()
        {
            var iRelationships = new System.Collections.Generic.HashSet<ulong>();

                // hashset is faster to do a 'contains'.
            foreach (var i in from i in BrainData.Current.Thesaurus.NoRecursiveRelationships select i.Item.ID)
            {
                iRelationships.Add(i);
            }

            var iLinks = Factories.Default.LinkLists.GetBuffer();
            using (var iList = Item.LinksOut) iLinks.AddRange(iList); // this locks.
            try
            {
                foreach (var i in iLinks)
                {
                    if (iRelationships.Contains(i.MeaningID))
                    {
                        var iTo = i.To as NeuronCluster; // can't do this inside a lock.
                        if (iTo != null)
                        {
                            var iSubs = new ThesaurusSubItemCollection(this, iTo, i.Meaning);
                            Add(iSubs);
                        }
                        else
                        {
                            LogService.Log.LogWarning(
                                "SubRelationshipsCollection.LoadItems", 
                                "Found a relationship link that is not pointint to a cluster, can't build list of related words!");
                        }
                    }
                }
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iLinks);
            }
        }

        #endregion
    }
}