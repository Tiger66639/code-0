// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElementListDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for <see cref="CodeItemCollection" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for <see cref="CodeItemCollection" /> objects.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This object should be attached to an ItemsControl that is bound to a code
    ///         list with it's
    ///         <see cref="System.Windows.Controls.ItemsControl.ItemsSource" /> property.
    ///     </para>
    ///     <para>
    ///         Provides only add functionality for the list, no inserts. This has to be
    ///         done with <see cref="CodeListItemDropAdvisor" />
    ///     </para>
    /// </remarks>
    public class FrameElementListDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Tries the move item.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, FrameElement iItem)
        {
            var iList = Items;
            var iIndex = iList.IndexOf(iItem);
            if (iIndex == -1)
            {
                // we need to create a new item if the item being moved wasn't in this list or when a copy was requested because shift was pressed.
                var iNew = new FrameElement(iItem.Item);

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

            var iNew = EditorsHelper.MakeFrameElement(Brain.Current[iId]);
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

        #region Items

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public FrameElementCollection Items
        {
            get
            {
                return Target.ItemsSource as FrameElementCollection;
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
            return Items != null
                   && (obj.GetDataPresent(Properties.Resources.FrameElementFormat)
                       || obj.GetDataPresent(Properties.Resources.NeuronIDFormat));
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