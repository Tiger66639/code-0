// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequenceListItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for the child items of
//   <see cref="FrameSequenceCollection" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for the child items of
    ///     <see cref="FrameSequenceCollection" /> objects.
    /// </summary>
    public class FrameSequenceListItemDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try move item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FrameSequence iItem)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FrameSequence;
            var iList = Items;
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iOldIndex = iList.IndexOf(iItem);
            if (iOldIndex == -1)
            {
                var iNew = new FrameSequence(iItem.Item);

                    // we always create a new item, even with a move, cause we don't know how the item was being dragged (with shift pressed or not), this is the savest and easiest way.
                iList.Insert(iNewIndex, iNew);
            }
            else
            {
                iList.Move(iOldIndex, iNewIndex);
                arg.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <summary>The try create new item item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="or">The or.</param>
        private void TryCreateNewItemItem(
            System.Windows.DragEventArgs obj, 
            System.Windows.Point dropPoint, 
            FrameSequence or)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FrameSequence;
            var iList = Items;
            System.Diagnostics.Debug.Assert(iList != null);
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iNew = new FrameSequence(EditorsHelper.DuplicateFrameSequence((NeuronCluster)or.Item, null, null));

                // we make a deep copy when the item gets copied to a new one.
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);

            iList.Insert(iNewIndex, iNew);
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

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public FrameOrderCollection Items
        {
            get
            {
                var iItemsControl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(TargetUI);
                System.Diagnostics.Debug.Assert(iItemsControl != null);
                return iItemsControl.ItemsSource as FrameOrderCollection;
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
            var iItem = arg.Data.GetData(Properties.Resources.FrameSequenceFormat) as FrameSequence;
            if (iItem != null && Items.Contains(iItem))
            {
                if ((arg.Effects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
                {
                    TryMoveItem(arg, dropPoint, iItem);
                }
                else
                {
                    TryCreateNewItemItem(arg, dropPoint, iItem);
                }
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            if (obj.GetDataPresent(Properties.Resources.FrameSequenceFormat))
            {
                var iItem = obj.GetData(Properties.Resources.FrameSequenceFormat) as FrameSequence;
                return iItem != null && Items.Contains(iItem);
            }

            return false;
        }

        #endregion
    }
}