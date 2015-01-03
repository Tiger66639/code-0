// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusLinkedItems.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the <see cref="ThesaurusLinkedItem" /> objects for a single
//   thesaurus item and a limited set of allowed 'meanings'.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Manages all the <see cref="ThesaurusLinkedItem" /> objects for a single
    ///     thesaurus item and a limited set of allowed 'meanings'.
    /// </summary>
    public class ThesaurusLinkedItems : Data.OwnedObject<Thesaurus>
    {
        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f items.</summary>
        private System.Collections.ObjectModel.ObservableCollection<ThesaurusLinkedItem> fItems =
            new System.Collections.ObjectModel.ObservableCollection<ThesaurusLinkedItem>();

        /// <summary>The f selected index.</summary>
        private int fSelectedIndex = -1;

        /// <summary>The f event monitor.</summary>
        private readonly ThesaurusLinkedItemsEventMonitor fEventMonitor;

        /// <summary>Initializes a new instance of the <see cref="ThesaurusLinkedItems"/> class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="relationships">The relationships.</param>
        public ThesaurusLinkedItems(Neuron item, ThesaurusRelItemCollection relationships)
        {
            Item = item;
            fEventMonitor = new ThesaurusLinkedItemsEventMonitor(this);
            Relationships = relationships;
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

        #region Relationships

        /// <summary>
        ///     Getsthe list of possible relationships for this item. This value is
        ///     assigned from the thesaurus.
        /// </summary>
        public ThesaurusRelItemCollection Relationships { get; private set; }

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

        /// <summary>
        ///     Gets the relationships as neuron data. This is used to display in the
        ///     drop down.
        /// </summary>
        /// <value>
        ///     The relationships neuron data.
        /// </value>
        public System.Collections.Generic.IEnumerable<NeuronData> RelationshipsNeuronData
        {
            get
            {
                return (from i in Relationships select i.NeuronInfo).ToList();
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
                var iRelationships = (from i in Relationships where i.Item != null select i.Item.ID).ToList();
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                try
                {
                    using (var iList = Item.LinksOut)
                        iLinks.AddRange(iList);

                            // make a copy of the list so we don't have a deadlock while writing to the cache.
                    foreach (var i in iLinks)
                    {
                        if (iRelationships.Contains(i.MeaningID))
                        {
                            var iNew = new ThesaurusLinkedItem(Item, i.Meaning, i.To);
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
            fItems.Clear();
            fEventMonitor.Unregister();
        }
    }
}