// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameOrderCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The frame order collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame order collection.</summary>
    public class FrameOrderCollection : ClusterCollection<FrameSequence>
    {
        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FrameSequence"/>.</returns>
        public override FrameSequence GetWrapperFor(Neuron toWrap)
        {
            return new FrameSequence(toWrap);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.FrameSequence;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameOrderCollection"/> class. Initializes a new instance of the <see cref="FrameOrderCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public FrameOrderCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FrameOrderCollection"/> class. Initializes a new instance of the <see cref="FrameOrderCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public FrameOrderCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
        }

        #endregion
    }
}