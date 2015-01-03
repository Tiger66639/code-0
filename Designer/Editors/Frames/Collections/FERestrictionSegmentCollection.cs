// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionSegmentCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The fe restriction segment collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The fe restriction segment collection.</summary>
    public class FERestrictionSegmentCollection : ClusterCollection<FERestrictionSegment>
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FERestrictionSegmentCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public FERestrictionSegmentCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        #endregion

        #region Functions

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FERestrictionSegment"/>.</returns>
        public override FERestrictionSegment GetWrapperFor(Neuron toWrap)
        {
            return new FERestrictionSegment(toWrap);
        }

        #endregion

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.VerbNetRestriction;
        }
    }
}