// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseValuesCollection.cs" company="">
//   
// </copyright>
// <summary>
//   a collection of ConditionalOutputCollections that should be used when a
//   previously stated question is active.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a collection of ConditionalOutputCollections that should be used when a
    ///     previously stated question is active.
    /// </summary>
    public class ResponseValuesCollection : PatternEditorCollection<ResponsesForGroup>
    {
        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ResponsesForGroup"/>.</returns>
        public override ResponsesForGroup GetWrapperFor(Neuron toWrap)
        {
            return new ResponsesForGroup((NeuronCluster)toWrap);
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
                WindowMain.DeleteItemFromBrain(Cluster); // we can simply delete the cluster, nothing else to clean up.
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ResponseValuesCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public ResponseValuesCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ResponseValuesCollection"/> class. Initializes a new instance of the<see cref="InvalidPatternResponseCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="meaning">The meaning.</param>
        public ResponseValuesCollection(INeuronWrapper owner, ulong meaning)
            : base(owner, meaning)
        {
        }

        #endregion
    }
}