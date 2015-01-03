// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   enumerates through the data of a rule (inputs, outputs, do,
//   conditionals).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     enumerates through the data of a rule (inputs, outputs, do,
    ///     conditionals).
    /// </summary>
    public class RuleEnumerator : System.Collections.IEnumerator, System.Collections.IEnumerable, INeuronInfo
    {
        /// <summary>The f counter.</summary>
        private int fCounter = -1;

        /// <summary>The f items.</summary>
        private System.Collections.Generic.List<System.Collections.IEnumerator> fItems;

                                                                                // stores the items that we can return, easiest to precalculate.

        /// <summary>The f rule.</summary>
        protected PatternRule fRule;

        /// <summary>Initializes a new instance of the <see cref="RuleEnumerator"/> class.</summary>
        /// <param name="rule">The rule.</param>
        public RuleEnumerator(PatternRule rule)
        {
            fRule = rule;
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
        // public PatternRule Owner
        // {
        // get { return fRule; }
        // }

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
                if (fItems == null)
                {
                    Reset();
                }

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
                return fItems[fCounter];
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
                Reset();
            }

            Counter++;
            return Counter < fItems.Count; // there are only 2 items: the rules and the questions.
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
            fItems = new System.Collections.Generic.List<System.Collections.IEnumerator>();

            var iRule = (NeuronCluster)fRule.Item;

            using (IDListAccessor iChildren = iRule.Children)
                if (iChildren.Count > 0)
                {
                    fItems.Add(new InputsEnumerator(iRule));
                }

            // if (fRule.ToCalculate.Count > 0)
            // fItems.Add(new DoEnumerator(fRule.ToCalculate, "Calculate"));
            var iOuts = iRule.FindFirstOut((ulong)PredefinedNeurons.TextPatternOutputs) as NeuronCluster;
            if (iOuts != null)
            {
                fItems.Add(new OutputsEnumerator(iOuts));
            }

            // if (fRule.DoPatterns != null && fRule.DoPatterns.Count > 0)
            // fItems.Add(new DoEnumerator(fRule.DoPatterns, "Do"));
            iOuts = iRule.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
            if (iOuts != null)
            {
                fItems.Add(new ResponsesForGroupsEnumerator(iOuts));
            }

            iOuts = iRule.FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
            if (iOuts != null)
            {
                fItems.Add(new ConditionsEnumerator(iOuts));
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
                return fRule.NeuronInfo;
            }
        }

        #endregion
    }
}