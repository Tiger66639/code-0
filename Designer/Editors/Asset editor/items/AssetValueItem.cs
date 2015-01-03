// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetValueItem.cs" company="">
//   
// </copyright>
// <summary>
//   The asset value item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The asset value item.</summary>
    public class AssetValueItem : Data.OwnedObject<AssetItem>, INeuronWrapper
    {
        /// <summary>The f data.</summary>
        private readonly AssetData fData;

                                   // a ref to the data element (the cluster that owns this item), the 'owner' points to the asset item.

        /// <summary>Initializes a new instance of the <see cref="AssetValueItem"/> class.</summary>
        /// <param name="value">The value.</param>
        /// <param name="data">The data.</param>
        public AssetValueItem(Neuron value, AssetData data)
        {
            Item = value;
            fData = data;
        }

        #region Value

        /// <summary>
        ///     Gets/sets the value neuron to wrap
        /// </summary>
        public Neuron Value
        {
            get
            {
                return Item;
            }

            set
            {
                if (value != Item)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        // we create a new object and replace it in  the collection cause this prop represents the item directly in the colleciton.
                        var iIndex = fData.AsGroup.IndexOf(this); // asGroup must exist cause this item exists.
                        if (value != null)
                        {
                            fData.AsGroup[iIndex] = new AssetValueItem(value, fData);
                        }
                        else
                        {
                            fData.AsGroup.RemoveAt(iIndex);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #endregion
    }
}