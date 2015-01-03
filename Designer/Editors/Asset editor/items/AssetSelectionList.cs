// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetSelectionList.cs" company="">
//   
// </copyright>
// <summary>
//   a list that contains all the selected assets in an
//   <see cref="ObjectEditor" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a list that contains all the selected assets in an
    ///     <see cref="ObjectEditor" /> .
    /// </summary>
    /// <remarks>
    ///     OVerrides some
    /// </remarks>
    internal class AssetSelectionList : System.Collections.ObjectModel.ObservableCollection<AssetBase>
    {
        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                i.SetSelected(false);
            }

            base.ClearItems();
        }

        /// <summary>Inserts an <paramref name="item"/> into the collection at the
        ///     specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, AssetBase item)
        {
            item.SetSelected(true);
            base.InsertItem(index, item);
        }

        /// <summary>Removes the item at the specified <paramref name="index"/> of the
        ///     collection.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                iItem.SetSelected(false);
            }

            base.RemoveItem(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, AssetBase item)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                iItem.SetSelected(false);
            }

            item.SetSelected(true);
            base.SetItem(index, item);
        }

        /// <summary>Adds the <paramref name="asset"/> without updating the item itself.</summary>
        /// <param name="asset">The asset.</param>
        internal void AddInternal(AssetBase asset)
        {
            base.InsertItem(Count, asset);
        }

        /// <summary>Removes the <paramref name="asset"/> without updating the item itself.</summary>
        /// <param name="asset">The asset.</param>
        internal void RemoveInternal(AssetBase asset)
        {
            var iIndex = IndexOf(asset);
            base.RemoveItem(iIndex);
        }
    }
}