// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesItemEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   The thes item enumerator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The thes item enumerator.</summary>
    public class ThesItemEnumerator : INeuronInfo
    {
        /// <summary>The f children.</summary>
        private NeuronCluster fChildren;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<object> fItems =
            new System.Collections.ObjectModel.ObservableCollection<object>();

        /// <summary>The f pos.</summary>
        private readonly Neuron fPOS;

        /// <summary>The f thes.</summary>
        private readonly Thesaurus fThes;

        /// <summary>Initializes a new instance of the <see cref="ThesItemEnumerator"/> class. Initializes a new instance of the <see cref="ThesItemEnumerator"/>
        ///     class.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="neuron">The neuron.</param>
        public ThesItemEnumerator(Thesaurus thes, Neuron pos, Neuron neuron)
        {
            fThes = thes;
            fPOS = pos;
            Item = neuron;
        }

        /// <summary>
        ///     Gets the item/object thtat this enumerator wraps.
        /// </summary>
        public Neuron Item { get; private set; }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public NeuronData Owner
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item];
            }
        }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public virtual bool HasChildren
        {
            get
            {
                if (fChildren == null)
                {
                    // so we don't try to get it to many times.
                    fChildren = Item.FindFirstOut(fThes.SelectedRelationship.ID) as NeuronCluster;
                }

                return fChildren != null;
            }
        }

        #endregion

        /// <summary>
        ///     Gets the items.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<object> Items
        {
            get
            {
                return fItems;
            }
        }

        #region IsExpanded

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public virtual bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (value != fIsExpanded)
                {
                    fIsExpanded = value;
                    if (value == false)
                    {
                        Items.Clear();
                    }
                    else
                    {
                        if (fChildren != null)
                        {
                            System.Collections.Generic.List<Neuron> iChildren;
                            using (var iList = fChildren.Children) iChildren = iList.ConvertTo<Neuron>();
                            foreach (var i in iChildren)
                            {
                                fItems.Add(new ThesItemPEEnumerator(fThes, fPOS, i));
                            }

                            Factories.Default.NLists.Recycle(iChildren);
                        }
                    }
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null. We need to
        ///     add this so we can select the node in the drop down box.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return Owner;
            }
        }

        #endregion
    }
}