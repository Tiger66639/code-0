// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through a single output
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through a single output
    /// </summary>
    public class OutputEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable, INeuronInfo
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1; // -1 to indicate that at first run, a reset needs to be done

        /// <summary>The f invalid responses.</summary>
        private NeuronCluster fInvalidResponses;

        /// <summary>The f output.</summary>
        private readonly Neuron fOutput;

        /// <summary>Initializes a new instance of the <see cref="OutputEnumerator"/> class.</summary>
        /// <param name="output">The output.</param>
        public OutputEnumerator(Neuron output)
        {
            fOutput = output;
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
        // public OutputPattern Owner
        // {
        // get { return fOutput; }
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
                if (fCounter == -1)
                {
                    // at first try, calculate the first item.
                    Reset();
                }

                return fCounter < 2;
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
                    return new ResponseForInvalidsEnumerator(fInvalidResponses);
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
            if (fCounter == -1)
            {
                // at first try, calculate the first item.
                Reset();
            }

            Counter++;
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
            fInvalidResponses =
                fOutput.FindFirstOut((ulong)PredefinedNeurons.InvalidResponsesForPattern) as NeuronCluster;
            if (fInvalidResponses != null)
            {
                using (IDListAccessor iChildren = fInvalidResponses.Children)
                {
                    if (iChildren.Count > 0)
                    {
                        Counter = 0;
                    }
                    else
                    {
                        Counter = 2;
                    }
                }
            }
            else
            {
                Counter = 2;
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
                if (fOutput != null)
                {
                    return BrainData.Current.NeuronInfo[fOutput];
                }

                return null;
            }
        }

        #endregion
    }
}