// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputPatternCollection.cs" company="">
//   
// </copyright>
// <summary>
//   wraps the textpatterns of a single TextPattern def.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     wraps the textpatterns of a single TextPattern def.
    /// </summary>
    public class InputPatternCollection : PatternEditorCollection<InputPattern>
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="InputPatternCollection"/> class. Initializes a new instance of the<see cref="InputPatternCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public InputPatternCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        #endregion

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="InputPattern"/>.</returns>
        public override InputPattern GetWrapperFor(Neuron toWrap)
        {
            return new InputPattern((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.PatternRule;
        }
    }
}