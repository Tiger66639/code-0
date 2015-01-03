// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FramesCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of frames.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection of frames.
    /// </summary>
    public class FramesCollection : ClusterCollection<Frame>
    {
        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="Frame"/>.</returns>
        public override Frame GetWrapperFor(Neuron toWrap)
        {
            return new Frame(toWrap as NeuronCluster);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return linkMeaning;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FramesCollection"/> class. Initializes a new instance of the <see cref="FramesCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="cluster">The cluster.</param>
        public FramesCollection(INeuronWrapper owner, NeuronCluster cluster)
            : base(owner, cluster)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FramesCollection"/> class. Initializes a new instance of the <see cref="FramesCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public FramesCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
        {
        }

        #endregion
    }
}