// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponsesForGroupEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through all the data of a <see cref="ResponsesForGroup" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through all the data of a <see cref="ResponsesForGroup" />
    /// </summary>
    public class ResponsesForGroupEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1; // first item (0) is empty pos, which we don't use

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<Neuron> fItems;

        /// <summary>The f item.</summary>
        private readonly NeuronCluster fItem;

        /// <summary>Initializes a new instance of the <see cref="ResponsesForGroupEnumerator"/> class.</summary>
        /// <param name="cond">The cond.</param>
        public ResponsesForGroupEnumerator(NeuronCluster cond)
        {
            fItem = cond;
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

        /// <summary>
        ///     <para>Gets the owner.</para>
        ///     <para>Gets the title that should be used for this item.</para>
        /// </summary>
        public string Title
        {
            get
            {
                var iRes = new System.Text.StringBuilder();
                using (IDListAccessor iChildren = fItem.Children)
                {
                    foreach (var i in iChildren)
                    {
                        if (iRes.Length > 0)
                        {
                            iRes.Append(", ");
                        }

                        iRes.Append(BrainData.Current.NeuronInfo[i].DisplayTitle);
                    }
                }

                if (iRes.Length == 0)
                {
                    return "empty";
                }

                return iRes.ToString();
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
                var iRes = new ConditionEnumerator((NeuronCluster)fItems[Counter]);
                return iRes;
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
            Counter = -1;
        }
    }
}