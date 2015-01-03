// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetClusterEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through a cluster's children as if it were asset items. for
//   browsing through an asset.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through a cluster's children as if it were asset items. for
    ///     browsing through an asset.
    /// </summary>
    public class AssetClusterEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<ulong> fItems;

        /// <summary>The f root.</summary>
        private readonly ObjectEditor fRoot; // for passing to the children, so they know which values to extract.

        /// <summary>Initializes a new instance of the <see cref="AssetClusterEnumerator"/> class. Initializes a new instance of the <see cref="AssetDataEnumerator"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="root">The root.</param>
        /// <param name="title">The title.</param>
        public AssetClusterEnumerator(NeuronCluster item, ObjectEditor root, string title)
        {
            System.Diagnostics.Debug.Assert(item != null);
            using (var iList = item.Children)
                fItems = new System.Collections.Generic.List<ulong>(iList);

                    // take a local copy, this is fastest: only need to lock 1 time.
            fRoot = root;
            Title = title;
            fItem = item;
        }

        /// <summary>
        ///     the title to depict in the Ui for this item.
        /// </summary>
        public string Title { get; private set; }

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
                return true;
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
            return new AssetClusterEnumerator(fItem, fRoot, Title);
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
                return new AssetItemNeuronEnumerator(Brain.Current[fItems[Counter]], fRoot);
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
    }
}