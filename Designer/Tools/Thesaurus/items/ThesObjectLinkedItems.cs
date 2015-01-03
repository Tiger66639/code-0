// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesObjectLinkedItems.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the <see cref="ThesaurusLinkedItem" /> objects for a single
//   thesaurus item, with all the meanings that are objects and pointing to
//   objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages all the <see cref="ThesaurusLinkedItem" /> objects for a single
    ///     thesaurus item, with all the meanings that are objects and pointing to
    ///     objects.
    /// </summary>
    public class ThesObjectLinkedItems : Data.OwnedObject<Thesaurus>
    {
        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f items.</summary>
        private System.Collections.ObjectModel.ObservableCollection<ThesaurusLinkedItem> fItems =
            new System.Collections.ObjectModel.ObservableCollection<ThesaurusLinkedItem>();

        /// <summary>The f selected index.</summary>
        private int fSelectedIndex = -1;

        /// <summary>The f event monitor.</summary>
        private readonly ThesObjectLinkedItemsEventMonitor fEventMonitor;

        /// <summary>Initializes a new instance of the <see cref="ThesObjectLinkedItems"/> class.</summary>
        /// <param name="item">The item.</param>
        public ThesObjectLinkedItems(Neuron item)
        {
            Item = item;
            fEventMonitor = new ThesObjectLinkedItemsEventMonitor(this);
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #region Items

        /// <summary>
        ///     Gets the list of related items and their relatinship.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<ThesaurusLinkedItem> Items
        {
            get
            {
                return fItems;
            }

            internal set
            {
                // we allow for an internal setter, so we can reset the UI when clearing the items. This is to prevent the ui from updating the last item, to 'null', when
                fItems = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        #region SelectedIndex

        /// <summary>
        ///     Gets/sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return fSelectedIndex;
            }

            set
            {
                fSelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        #endregion

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

                    OnPropertyChanged("IsLoaded");
                }
            }
        }

        #endregion

        /// <summary>The load items.</summary>
        private void LoadItems()
        {
            if (Item != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                using (var iList = Item.LinksOut) iLinks.AddRange(iList); // make clocal copy so we can savely write to cache.
                try
                {
                    foreach (var i in iLinks)
                    {
                        var iMeaning = i.Meaning as NeuronCluster;
                        if (iMeaning != null && iMeaning.Meaning == (ulong)PredefinedNeurons.Object)
                        {
                            var iNew = new ThesaurusLinkedItem(Item, iMeaning, i.To);
                            fItems.Add(iNew);
                        }
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }
        }

        /// <summary>
        ///     Clears the loaded items.
        /// </summary>
        public void Clear()
        {
            SelectedIndex = -1;
            fItems.Clear();
            fEventMonitor.Unregister();
        }
    }
}