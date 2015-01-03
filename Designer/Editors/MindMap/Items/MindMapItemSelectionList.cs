// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapItemSelectionList.cs" company="">
//   
// </copyright>
// <summary>
//   Manages the list of selected items. Makes certain that there can only be
//   1 link selected at the same time. Also makes certain that all other
//   selected items are removed if a new one is added when the control key is
//   not pressed (when not in group insert).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Manages the list of selected items. Makes certain that there can only be
    ///     1 link selected at the same time. Also makes certain that all other
    ///     selected items are removed if a new one is added when the control key is
    ///     not pressed (when not in group insert).
    /// </summary>
    public class MindMapItemSelectionList : System.Collections.ObjectModel.ObservableCollection<MindMapItem>
    {
        #region DoGroupSelect

        /// <summary>
        ///     Gets/sets the value that indicates if we want to do a group add or
        ///     not. When not, the control key needs to be pressed to add items,
        ///     otherwise only the last selected item remains. When this prop is true,
        ///     the control key doesn't need to be pressed. Used to do a drawbox
        ///     selection.
        /// </summary>
        public bool DoGroupSelect { get; set; }

        #endregion

        /// <summary>Inserts an <paramref name="item"/> into the collection at the
        ///     specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, MindMapItem item)
        {
            if (item.IsSelected == false)
            {
                // if it's already selected, move to first, it's already in the list, but is reselected, so we want it first.
                if (DoGroupSelect == false
                    && (item is MindMapLink
                        || System.Windows.Input.Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Control))
                {
                    Clear();
                    base.InsertItem(0, item); // the index changes in this case.
                }
                else
                {
                    base.InsertItem(index, item);
                }

                item.SetIsSelected(true);
            }
            else
            {
                Move(IndexOf(item), 0);
            }
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            var iTemp = Enumerable.ToArray(this); // we make a copy, cause the list can get changed.
            base.ClearItems();
            foreach (var i in iTemp)
            {
                // we reset after clearing the list, this should generate the least overhead.
                i.SetIsSelected(false);
            }
        }

        /// <summary>Removes the item at the specified <paramref name="index"/> of the
        ///     collection.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                iItem.SetIsSelected(false);
            }

            base.RemoveItem(index);
        }
    }
}