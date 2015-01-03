// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerChildrenCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection containin explorer items. This is used by another explorer
//   item that wraps a cluster. It provides automatic syncing with the
//   network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection containin explorer items. This is used by another explorer
    ///     item that wraps a cluster. It provides automatic syncing with the
    ///     network.
    /// </summary>
    public class ExplorerChildrenCollection : ClusterCollection<NeuronExplorerItem>
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerChildrenCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toWrap">The to wrap.</param>
        public ExplorerChildrenCollection(INeuronWrapper owner, NeuronCluster toWrap)
            : base(owner, toWrap)
        {
        }

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="NeuronExplorerItem"/>.</returns>
        public override NeuronExplorerItem GetWrapperFor(Neuron toWrap)
        {
            return new NeuronExplorerItem(toWrap);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return Neuron.EmptyId;
        }
    }
}