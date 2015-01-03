// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   The topic enumerator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The topic enumerator.</summary>
    public class TopicEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable, INeuronInfo
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -2; // -1 to indicate that at first run, a reset needs to be done

        /// <summary>The f editor.</summary>
        protected TextPatternEditor fEditor;

        /// <summary>The f has children.</summary>
        private bool fHasChildren;

        /// <summary>The f was open.</summary>
        private bool fWasOpen;

                     // so we can close the editor again once we have enumerated through the data. If we don't do this, we get mem leaks, 

        /// <summary>Initializes a new instance of the <see cref="TopicEnumerator"/> class.</summary>
        /// <param name="editor">The editor.</param>
        public TopicEnumerator(TextPatternEditor editor)
        {
            fEditor = editor;
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
        public TextPatternEditor Owner
        {
            get
            {
                return fEditor;
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
                    return new RulesEnumerator(fEditor);
                }

                if (Counter == 1)
                {
                    return new QuestionsEnumerator(fEditor);
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
            if (Counter == -2)
            {
                Reset();
            }

            Counter++;
            if (Counter == 0 || (Counter == 1 && fEditor.Questions != null && fEditor.Questions.Count > 0))
            {
                return true;
            }

            fEditor.IsOpen = fWasOpen;
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
            fWasOpen = fEditor.IsOpen;
            fEditor.IsOpen = true;
            if (fEditor.Items.Count > 0)
            {
                Counter = -1; // after a reset, we always advance 1 step, so take this into accoutn
            }
            else if (fEditor.Questions != null && fEditor.Questions.Count > 0)
            {
                Counter = 0;
            }
            else
            {
                Counter = 1;
            }

            if (Counter != 1)
            {
                fHasChildren = true; // if we don't cach this value, we get an incorrect calcuation.
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
                return fEditor.NeuronInfo;
            }
        }

        #endregion
    }
}