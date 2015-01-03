// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowConditonalPartDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow conditonal part drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The flow conditonal part drop advisor.</summary>
    public class FlowConditonalPartDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Tries the move item.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FlowItem iItem)
        {
            var iList = Items;
            var iIndex = iList.IndexOf(iItem);
            if (iIndex == -1)
            {
                // we need to create a new item if the item being moved wasn't in this list or when a copy was requested because shift was pressed.
                var iNew = FlowEditor.CreateFlowItemFor(iItem.Item);

                    // we always create a new item, even with a move, cause we don't know how the item was being dragged (with shift pressed or not), this is the savest and easiest way.
                iList.Add(iNew);
            }
            else
            {
                iList.Move(iIndex, iList.Count - 1);
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
            System.Diagnostics.Debug.Assert(iNew != null);
            if (iNew is FlowItemConditionalPart)
            {
                // if the new item is also a cond part, we want to do an insert at that location.
                var iPart = FLowItem;
                System.Diagnostics.Debug.Assert(iPart != null);
                var iBlock = iPart.Owner as FlowItemBlock;

                FlowItemCollection iList = null;
                if (iBlock != null)
                {
                    iList = iBlock.Items;
                }
                else
                {
                    var iFlow = iPart.Owner as Flow;
                    iList = iFlow.Items;
                }

                System.Diagnostics.Debug.Assert(iList != null);
                var iIndex = iList.IndexOf(iPart);
                iList.Insert(iIndex, iNew);
            }
            else
            {
                var iCodeList = Items;
                System.Diagnostics.Debug.Assert(iCodeList != null);
                iCodeList.Add(iNew);
            }
        }

        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public System.Windows.Controls.Border Target
        {
            get
            {
                return (System.Windows.Controls.Border)TargetUI;
            }
        }

        #endregion

        #region FlowItem

        /// <summary>Gets the f low item.</summary>
        public FlowItemConditionalPart FLowItem
        {
            get
            {
                var iBorder = Target;
                if (iBorder != null)
                {
                    return iBorder.DataContext as FlowItemConditionalPart;
                }

                return null;
            }
        }

        #endregion

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
        public FlowItemCollection Items
        {
            get
            {
                var iCond = Target.DataContext as FlowItemConditionalPart;
                if (iCond != null)
                {
                    return iCond.Items;
                }

                return null;
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

        /// <summary>Determines whether [is valid data object] [the specified obj].</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise,<c>false</c> .</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            var iTarget = Items;
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
            var iTarget = Items;
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