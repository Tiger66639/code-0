// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicFilterCollection.cs" company="">
//   
// </copyright>
// <summary>
//   maintains a list of topic filters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     maintains a list of topic filters.
    /// </summary>
    public class TopicFilterCollection : PatternEditorCollection<TopicFilterPattern>
    {
        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="TopicFilterPattern"/>.</returns>
        public override TopicFilterPattern GetWrapperFor(Neuron toWrap)
        {
            return new TopicFilterPattern((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.TopicFilter;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TopicFilterCollection"/> class. Initializes a new instance of the<see cref="InputPatternCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public TopicFilterCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TopicFilterCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public TopicFilterCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        #endregion
    }
}