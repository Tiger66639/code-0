// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponsesForCollection.cs" company="">
//   
// </copyright>
// <summary>
//   manages a collection of <see cref="ResponseForOutput" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     manages a collection of <see cref="ResponseForOutput" /> objects.
    /// </summary>
    public class ResponsesForCollection : PatternEditorCollection<ResponseForOutput>
    {
        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ResponseForOutput"/>.</returns>
        public override ResponseForOutput GetWrapperFor(Neuron toWrap)
        {
            return new ResponseForOutput((TextNeuron)toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.ResponseForOutputs;
        }

        /// <summary>
        ///     If the list is empty and the owner is a responesesForGroup, check if
        ///     the group is empty, if so delete the group as well.
        /// </summary>
        public void CheckEmptyList()
        {
            if (Count == 0 && WindowMain.UndoStore.CurrentState == UndoSystem.UndoState.none)
            {
                // only delete the empty item if we are not undoing/redoing something, cause in that case, we shouldn't do anything autmatically cause the undo manager determins what happens.
                var iGrp = Owner as ResponsesForGroup;
                var iRule = iGrp.Rule;
                iGrp.Delete();
                iRule.ResponsesFor.CheckEmptyList();
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ResponsesForCollection"/> class. Initializes a new instance of the<see cref="ResponsesForCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public ResponsesForCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ResponsesForCollection"/> class. Initializes a new instance of the<see cref="ResponsesForCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public ResponsesForCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        #endregion
    }
}