// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequenceListDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for <see cref="FrameSequenceCollection" /> objects. Allows
//   only to move sequences around, you can't add new sequences like this,
//   since this is an illegal edit type (a sequence belongs to a frame, you
//   can't share it or anything).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for <see cref="FrameSequenceCollection" /> objects. Allows
    ///     only to move sequences around, you can't add new sequences like this,
    ///     since this is an illegal edit type (a sequence belongs to a frame, you
    ///     can't share it or anything).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This object should be attached to an ItemsControl that is bound to a code
    ///         list with it's
    ///         <see cref="System.Windows.Controls.ItemsControl.ItemsSource" /> property.
    ///     </para>
    ///     <para>
    ///         Provides only add functionality for the list, no inserts. This has to be
    ///         done with <see cref="FrameSequenceListItemDropAdvisor" />
    ///     </para>
    /// </remarks>
    public class FrameSequenceListDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Tries the move item.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FrameSequence iItem)
        {
            var iList = Items;
            var iIndex = iList.IndexOf(iItem);
            if (iIndex == -1)
            {
                // we need to create a new item if the item being moved wasn't in this list or when a copy was requested because shift was pressed.
                var iNew = new FrameSequence(iItem.Item);

                    // we always create a new item, even with a move, cause we don't know how the item was being dragged (with shift pressed or not), this is the savest and easiest way.
                iList.Add(iNew);
            }
            else
            {
                iList.Move(iIndex, iList.Count - 1);
                arg.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <summary>The try create new item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="or">The or.</param>
        private void TryCreateNewItem(
            System.Windows.DragEventArgs obj, 
            System.Windows.Point dropPoint, 
            FrameSequence or)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);

            var iNew = new FrameSequence(EditorsHelper.DuplicateFrameSequence((NeuronCluster)or.Item, null, null));
            var iList = Items;
            System.Diagnostics.Debug.Assert(iList != null);
            iList.Add(iNew);
        }

        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public System.Windows.Controls.ItemsControl Target
        {
            get
            {
                return (System.Windows.Controls.ItemsControl)TargetUI;
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

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public FrameOrderCollection Items
        {
            get
            {
                return Target.ItemsSource as FrameOrderCollection;
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
                    TryCreateNewItem(arg, dropPoint, iItem);
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

        // public override DragDropEffects GetEffect(DragEventArgs e)
        // {
        // DragDropEffects iRes = base.GetEffect(e);
        // if (iRes == DragDropEffects.Move)
        // {
        // CodeItem iItem = e.Data.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
        // if (iItem != null && CodeList.Contains(iItem) == true)
        // return DragDropEffects.Copy;                                                                 //when we move on the same list, the drag source doesn't have to do anything.
        // }
        // return iRes;
        // }
        #endregion
    }
}