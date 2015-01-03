// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElementCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The frame element collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame element collection.</summary>
    public class FrameElementCollection : ClusterCollection<FrameElement>
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameElementCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public FrameElementCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FrameElementCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public FrameElementCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
        }

        #endregion

        #region Functions

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FrameElement"/>.</returns>
        public override FrameElement GetWrapperFor(Neuron toWrap)
        {
            return new FrameElement(toWrap);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.Frame;
        }

        #endregion
    }
}