// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates the contents of a single condition (list of outputs and do
//   patterns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates the contents of a single condition (list of outputs and do
    ///     patterns.
    /// </summary>
    public class ConditionEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable, INeuronInfo
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -2; // -1 to indicate that at first run, a reset needs to be done

        /// <summary>The f has children.</summary>
        private bool fHasChildren = true;

        /// <summary>The f condition.</summary>
        private readonly NeuronCluster fCondition;

        /// <summary>Initializes a new instance of the <see cref="ConditionEnumerator"/> class.</summary>
        /// <param name="cond">The cond.</param>
        public ConditionEnumerator(NeuronCluster cond)
        {
            fCondition = cond;
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
        /// <value>
        ///     The title.
        /// </value>
        public string Title
        {
            get
            {
                var iCondition = fCondition.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                if (iCondition != null)
                {
                    return iCondition.Text;
                }

                return null;
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
                if (fCounter == -2)
                {
                    // at first try, calculate the first item.
                    Reset();
                }

                return fHasChildren;
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
                if (Counter == 0)
                {
                    return new OutputsEnumerator(fCondition);
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
            if (fCounter == -2)
            {
                // at first try, calculate the first item.
                Reset();
            }

            Counter++;
            if (Counter == 0)
            {
                return true;
            }

            return false;
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
            using (IDListAccessor iChildren = fCondition.Children)
            {
                if (iChildren.Count > 0)
                {
                    Counter = -1; // we will add 1 to the counter before getting the first value, so adjust for this.
                }
                else
                {
                    Counter = 1;
                    fHasChildren = false;
                }
            }
        }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[fCondition];
            }
        }

        #endregion
    }
}