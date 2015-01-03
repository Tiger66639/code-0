// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputsEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   The outputs enumerator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The outputs enumerator.</summary>
    public class OutputsEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<Neuron> fItems;

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        /// <summary>Initializes a new instance of the <see cref="OutputsEnumerator"/> class.</summary>
        /// <param name="item">The item.</param>
        public OutputsEnumerator(NeuronCluster item)
        {
            fItem = item;
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

        ///// <summary>
        ///// Gets the owner.
        ///// </summary>
        // public PatternOutputsCollection Owner
        // {
        // get { return fItems; }
        // }
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
                using (IDListAccessor iChildren = fItem.Children) return iChildren.Count > 0;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is expanded; otherwise, <c>false</c> .
        /// </value>
        public bool IsExpanded
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
                    if (value)
                    {
                        using (IDListAccessor iChildren = fItem.Children) fItems = iChildren.ConvertTo<Neuron>();
                    }
                    else if (fItems != null)
                    {
                        Factories.Default.NLists.Recycle(fItems);
                    }
                }
            }
        }

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
                if (fItems != null)
                {
                    var iRes = new OutputEnumerator(fItems[Counter]);
                    return iRes;
                }

                return null;
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
            if (fItems == null)
            {
                using (IDListAccessor iChildren = fItem.Children) fItems = iChildren.ConvertTo<Neuron>();
            }

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
            Counter = -1;
        }
    }
}