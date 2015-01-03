// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetItemEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through an asset collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through an asset collection.
    /// </summary>
    public class AssetItemEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable, INeuronInfo
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1; // -1 to indicate that at first run, a reset needs to be done

        /// <summary>Initializes a new instance of the <see cref="AssetItemEnumerator"/> class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="title">The title.</param>
        public AssetItemEnumerator(AssetCollection list, string title)
        {
            Owner = list;
            Title = title;
        }

        /// <summary>
        ///     Gets the title to display.
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
        ///     Gets the owner.
        /// </summary>
        public AssetCollection Owner { get; private set; }

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
                return Owner.Count > 0;
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
                return new AssetDataEnumerator(Owner[Counter] as AssetItem);
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
            return Counter < Owner.Count;
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

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null. need to
        ///     implement this <see langword="interface" /> for the
        ///     <see langword="double" /> click action.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return ((INeuronInfo)Owner.Owner).NeuronInfo;
            }
        }

        #endregion
    }
}