// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidPatternResponseCollection.cs" company="">
//   
// </copyright>
// <summary>
//   manages a collection of <see cref="InvalidPatternResponse" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     manages a collection of <see cref="InvalidPatternResponse" /> objects.
    /// </summary>
    public class InvalidPatternResponseCollection : PatternEditorCollection<InvalidPatternResponse>
    {
        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="InvalidPatternResponse"/>.</returns>
        public override InvalidPatternResponse GetWrapperFor(Neuron toWrap)
        {
            return new InvalidPatternResponse((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.InvalidResponsesForPattern;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="InvalidPatternResponseCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public InvalidPatternResponseCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="InvalidPatternResponseCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public InvalidPatternResponseCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        #endregion
    }
}