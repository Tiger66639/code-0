// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetItemNeuronEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through each data item attached to the neuron that represents
//   an asset item. This mimics the behaviour of
//   <see cref="AssetDataEnumerator" /> except that this doesn't work on an
//   <see cref="AssetItem" /> but on a neuron (to save mem/speed, we try to
//   reuse assetItems where possible.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     enumerates through each data item attached to the neuron that represents
    ///     an asset item. This mimics the behaviour of
    ///     <see cref="AssetDataEnumerator" /> except that this doesn't work on an
    ///     <see cref="AssetItem" /> but on a neuron (to save mem/speed, we try to
    ///     reuse assetItems where possible.
    /// </summary>
    public class AssetItemNeuronEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>Initializes a new instance of the <see cref="AssetItemNeuronEnumerator"/> class. Initializes a new instance of the <see cref="AssetDataEnumerator"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="root">The root.</param>
        public AssetItemNeuronEnumerator(Neuron item, ObjectEditor root)
        {
            System.Diagnostics.Debug.Assert(item != null);
            fItems = new System.Collections.Generic.List<AssetItemNeuronChild>();
            ListAccessor<Link> iLinksOut = item.LinksOut;
            iLinksOut.Lock();
            try
            {
                foreach (var i in root.Columns)
                {
                    var iFound = (from u in iLinksOut where u.MeaningID == i.LinkID select u.To).FirstOrDefault();
                    if (iFound != null)
                    {
                        if (i.LinkID == (ulong)PredefinedNeurons.Attribute)
                        {
                            fAttributeValue = iFound;
                        }
                        else
                        {
                            fItems.Add(new AssetItemNeuronChild { Item = iFound, Title = i.Name });
                        }
                    }
                }
            }
            finally
            {
                iLinksOut.Dispose(); // also unlocks.
            }

            Owner = item;
            fRoot = root;
        }

        /// <summary>
        ///     Gets or sets the current pos counter.
        /// </summary>
        /// <value>
        ///     The counter.
        /// </value>
        public int Counter
        {
            get
            {
                return fCounter;
            }

            set
            {
                fCounter = value;
            }
        }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public Neuron Owner { get; private set; }

        /// <summary>
        ///     Gets the attribute for this asset item. This is used to depict a text
        ///     for this item, so that the children can all <see langword="ref" />
        /// </summary>
        public string Title
        {
            get
            {
                return BrainData.Current.NeuronInfo[fAttributeValue].DisplayTitle;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether can be selected. This is a group, so
        ///     it can't be selected, the children can.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow selection]; otherwise, <c>false</c> .
        /// </value>
        public bool AllowSelection
        {
            get
            {
                return false;
            }
        }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public bool HasChildren
        {
            get
            {
                return fItems.Count > 0;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public bool IsExpanded { get; set; }

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        /// <summary>
        ///     Gets the current element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The enumerator is positioned before the first element of the
        ///     collection or after the last element.
        /// </exception>
        /// <returns>
        ///     The current element in the collection.
        /// </returns>
        public virtual object Current
        {
            get
            {
                var iCluster = fItems[Counter].Item as NeuronCluster;
                if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Asset)
                {
                    return new AssetClusterEnumerator(iCluster, fRoot, fItems[Counter].Title);
                }

                return new AssetItemNeuronChildEnumerator(fItems[Counter].Item, fItems[Counter].Title);
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            Counter++;
            return Counter < fItems.Count;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first
        ///     element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            fCounter = -1;
        }

        #region inernal types

        /// <summary>
        ///     a single child that this enumerator can return. We need this, so that
        ///     we also have the title to return.
        /// </summary>
        private class AssetItemNeuronChild
        {
            /// <summary>Gets or sets the item.</summary>
            public Neuron Item { get; set; }

            /// <summary>Gets or sets the title.</summary>
            public string Title { get; set; }
        }

        #endregion

        #region fields

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<AssetItemNeuronChild> fItems;

        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        /// <summary>The f root.</summary>
        private readonly ObjectEditor fRoot; // for passing to other children.

        /// <summary>The f attribute value.</summary>
        private readonly Neuron fAttributeValue;

                                // we store this seperatly cause this is used to depict the text for this record. 
        #endregion
    }
}