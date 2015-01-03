// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElementListItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for the child items of <see cref="FrameElementCollection" />
//   objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for the child items of <see cref="FrameElementCollection" />
    ///     objects.
    /// </summary>
    public class FrameElementListItemDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try move item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FrameElement iItem)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FrameElement;
            var iList = Items;
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iOldIndex = iList.IndexOf(iItem);
            if (iOldIndex == -1)
            {
                var iNew = new FrameElement(iItem.Item);

                    // we always create a new item, even with a move, cause we don't know how the item was being dragged (with shift pressed or not), this is the savest and easiest way.
                iList.Insert(iNewIndex, iNew);
            }
            else
            {
                iList.Move(iOldIndex, iNewIndex);
                arg.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <summary>The try create new code item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewCodeItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iNew = EditorsHelper.MakeFrameElement(Brain.Current[iId]);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FrameElement;
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);
            var iList = Items;
            System.Diagnostics.Debug.Assert(iList != null);
            iList.Insert(iList.IndexOf(iDroppedOn), iNew);
        }

        #region prop

        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub lists don't get used but
        ///     only the main list cause this gets the events first, while we usually
        ///     want to drop in a sublist.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public FrameElementCollection Items
        {
            get
            {
                var iItemsControl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(TargetUI);
                System.Diagnostics.Debug.Assert(iItemsControl != null);
                return iItemsControl.ItemsSource as FrameElementCollection;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iItem = arg.Data.GetData(Properties.Resources.FrameElementFormat) as FrameElement;
            if (iItem != null && Items.Contains(iItem)
                && (arg.Effects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
            {
                TryMoveItem(arg, dropPoint, iItem);
            }
            else
            {
                TryCreateNewCodeItem(arg, dropPoint); // we are not moving around an item, so add new code item.
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return obj.GetDataPresent(Properties.Resources.FrameElementFormat)
                   || obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
        }

        #endregion
    }
}