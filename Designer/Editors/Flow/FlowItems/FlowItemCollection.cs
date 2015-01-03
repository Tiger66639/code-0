// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection for <see cref="FlowItem" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection for <see cref="FlowItem" /> objects.
    /// </summary>
    public class FlowItemCollection : ClusterCollection<FlowItem>
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItemCollection"/> class. Initializes a new instance of the <see cref="FlowItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toWrap">The cluster to wrap.</param>
        public FlowItemCollection(INeuronWrapper owner, NeuronCluster toWrap)
            : base(owner, toWrap)
        {
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

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FlowItem"/>.</returns>
        public override FlowItem GetWrapperFor(Neuron toWrap)
        {
            return FlowEditor.CreateFlowItemFor(toWrap);
        }
    }
}