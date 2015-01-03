// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesPOSItemEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   base class for browsing through all the objects for a single pos value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for browsing through all the objects for a single pos value.
    /// </summary>
    public class ThesPosItemEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>Initializes a new instance of the <see cref="ThesPosItemEnumerator"/> class. Initializes a new instance of the <see cref="ThesPosItemEnumerator"/>
        ///     class.</summary>
        /// <param name="thes">The thes.</param>
        /// <param name="pos">The pos.</param>
        public ThesPosItemEnumerator(Thesaurus thes, Neuron pos)
        {
            Thesaurus = thes;
            POS = pos;
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
        ///     Gets the list of items for this enumerator.
        /// </summary>
        public System.Collections.Generic.IList<Neuron> Items
        {
            get
            {
                if (fItems == null)
                {
                    fItems = new System.Collections.Generic.List<Neuron>();
                    Thesaurus.BuildItems(Thesaurus.SelectedRelationship, POS, fItems);
                }

                return fItems;
            }
        }

        /// <summary>
        ///     Gets the <see cref="POS" /> that this enumerates through.
        /// </summary>
        public Neuron POS { get; private set; }

        /// <summary>
        ///     Gets the thesaurus that we are browsing through.
        /// </summary>
        public Thesaurus Thesaurus { get; private set; }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        public NeuronData Owner
        {
            get
            {
                return BrainData.Current.NeuronInfo[POS];
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
                return Items.Count > 0;
            }
        }

        #endregion

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

        #region fields

        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<Neuron> fItems;

        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        #endregion

        #region IEnumerator Members

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
                return new ThesItemEnumerator(Thesaurus, POS, Items[fCounter]);
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
            fCounter++;
            return fCounter < Items.Count;
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
            fItems = null;
        }

        #endregion
    }
}