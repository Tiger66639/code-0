// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetValueItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection for the value of an asset, in case it is an 'or' or 'and'
//   cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection for the value of an asset, in case it is an 'or' or 'and'
    ///     cluster.
    /// </summary>
    public class AssetValueItemCollection : ClusterCollection<AssetValueItem>
    {
        /// <summary>The f data.</summary>
        private readonly AssetData fData; // the object that manages the list, not the owner, which is the asset. 

        /// <summary>Initializes a new instance of the <see cref="AssetValueItemCollection"/> class. Initializes a new instance of the<see cref="AssetValueItemCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="list">The list.</param>
        /// <param name="data">The data.</param>
        public AssetValueItemCollection(INeuronWrapper owner, NeuronCluster list, AssetData data)
            : base(list, owner)
        {
            fData = data; // neds to be done before we can load the children.
            LoadList(list);
            InternalCreate();
        }

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="AssetValueItem"/>.</returns>
        public override AssetValueItem GetWrapperFor(Neuron toWrap)
        {
            return new AssetValueItem(toWrap, fData);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            if (fData != null)
            {
                var iMode = fData.GroupMode;
                if (iMode != null)
                {
                    return iMode.ID;
                }
            }

            return (ulong)PredefinedNeurons.Or;
        }

        /// <summary>
        ///     We don't call base, this is done by the event handler that monitors
        ///     changes to the list. If we don't use this technique, actions performed
        ///     by the user will be done 2 times: once normally, once in response to
        ///     the list change in the neuron.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            ResetOwnerValue();
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            base.ClearItemsDirect();
            ResetOwnerValue();
        }

        /// <summary>Removes the item.</summary>
        /// <remarks>Always raises the event, so that undo works correctly (through the
        ///     CodeItemDropAdvisor).</remarks>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (Count <= 1)
            {
                ResetOwnerValue();
            }
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            base.RemoveItemDirect(index);
            if (Count <= 1)
            {
                ResetOwnerValue();
            }
        }

        /// <summary>
        ///     If there is only 1 item left (or non at all), we need to reset the
        ///     property of the asset back to a normal relationship and not a cluster.
        /// </summary>
        private void ResetOwnerValue()
        {
            if (fData != null)
            {
                Neuron iVal = null;
                var iCount = Count;

                    // we need to delete the cluster before removing any items, otherwise the cluster gets recreated first and we have a temp cluster, which we don't want to delete.
                if (iCount == 1)
                {
                    iVal = Items[0].Value;
                }

                WindowMain.DeleteItemFromBrain(Cluster); // need to clean up.

                // no longer needed to do 'Data.value = null;' cause this is done when the cluster gets deleted.
            }
        }
    }
}