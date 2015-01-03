// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for asset clusters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for asset clusters.
    /// </summary>
    public class AssetCollection : CascadedClusterCollection<AssetBase>
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c> .
        /// </value>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }

            set
            {
                base.IsActive = value;
                if (value == false)
                {
                    // when changing the active state, we need to make certain that all the child neurons also stop monitoring.
                    foreach (AssetItem i in this)
                    {
                        i.Owner = null;
                    }
                }
                else
                {
                    foreach (AssetItem i in this)
                    {
                        i.Owner = (IAssetOwner)Owner;
                    }
                }
            }
        }

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="AssetBase"/>.</returns>
        public override AssetBase GetWrapperFor(Neuron toWrap)
        {
            var iCluster = toWrap as NeuronCluster;
            if (iCluster != null)
            {
                if (iCluster.Meaning == (ulong)PredefinedNeurons.And || iCluster.Meaning == (ulong)PredefinedNeurons.Or
                    || iCluster.Meaning == (ulong)PredefinedNeurons.List
                    || iCluster.Meaning == (ulong)PredefinedNeurons.ArgumentsList
                    || iCluster.Meaning == (ulong)PredefinedNeurons.Arguments)
                {
                    return new AssetGroup(iCluster);
                }
            }

            return new AssetItem(toWrap);
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.Asset;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="AssetCollection"/> class. Initializes a new instance of the <see cref="AssetCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="asset">The asset.</param>
        public AssetCollection(INeuronWrapper owner, NeuronCluster asset)
            : base(owner, asset)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AssetCollection"/> class. Initializes a new instance of the <see cref="AssetCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public AssetCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
        {
        }

        #endregion
    }
}