// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequenceItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The frame sequence item collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame sequence item collection.</summary>
    public class FrameSequenceItemCollection : ClusterCollection<FrameSequenceItem>
    {
        #region Functions

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FrameSequenceItem"/>.</returns>
        public override FrameSequenceItem GetWrapperFor(Neuron toWrap)
        {
            return new FrameSequenceItem(toWrap);
        }

        #endregion

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.Frame;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameSequenceItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public FrameSequenceItemCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FrameSequenceItemCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public FrameSequenceItemCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
        }

        #endregion
    }
}