// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowListItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow list item drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The flow list item drop advisor.</summary>
    public class FlowListItemDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try move item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FlowItem iItem)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FlowItem;
            var iList = FlowList;
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iOldIndex = iList.IndexOf(iItem);
            if (iOldIndex == -1)
            {
                var iNew = FlowEditor.CreateFlowItemFor(iItem.Item);

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
            var iNew = FlowEditor.CreateFlowItemFor(Brain.Current[iId]);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as FlowItem;
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);
            var iCodeList = FlowList;
            System.Diagnostics.Debug.Assert(iCodeList != null);
            iCodeList.Insert(iCodeList.IndexOf(iDroppedOn), iNew);
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

        #region Item

        /// <summary>
        ///     Gets the code item that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        public FlowItem Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as FlowItem;
            }
        }

        #endregion

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public FlowItemCollection FlowList
        {
            get
            {
                var iItem = Item;
                var iOwner = iItem.Owner as Flow;
                if (iOwner != null)
                {
                    return iOwner.Items;
                }

                var iBlock = iItem.Owner as FlowItemBlock;
                System.Diagnostics.Debug.Assert(iBlock != null);
                return iBlock.Items;
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
            var iItem = arg.Data.GetData(Properties.Resources.FLOWITEMFORMAT) as FlowItem;
            if (iItem != null && FlowList.Contains(iItem)
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
            var iTarget = FlowList;
            if (iTarget != null)
            {
                if (obj.GetDataPresent(Properties.Resources.FLOWITEMFORMAT)
                    || obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                    || obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
                {
                    return CheckNoDataRecursion(obj);
                }
            }

            return false;
        }

        /// <summary>The check no data recursion.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckNoDataRecursion(System.Windows.IDataObject obj)
        {
            var iTarget = FlowList;
            System.Diagnostics.Debug.Assert(iTarget != null);
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                return true;
            }

            if (iTarget.Cluster == null)
            {
                return true;
            }

            if (obj.GetDataPresent(Properties.Resources.CodeItemFormat))
            {
                var iItem = obj.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
                return iItem.Item != null && iItem.Item.ID != iTarget.Cluster.ID;
            }

            if (obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iNeuronID = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                return iNeuronID != iTarget.Cluster.ID;
            }

            return true;
        }

        #endregion
    }
}