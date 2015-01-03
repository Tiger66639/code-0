// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The editor item drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The editor item drop advisor.</summary>
    public class EditorItemDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try move item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, EditorBase iItem)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorBase;
            var iList = Items;
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iOldIndex = iList.IndexOf(iItem);
            if (iOldIndex == -1)
            {
                iItem.RemoveFromOwner();
                if (iNewIndex > -1)
                {
                    iList.Insert(iNewIndex, iItem);
                }
                else
                {
                    iList.Add(iItem);
                }
            }
            else
            {
                iList.Move(iOldIndex, iNewIndex == -1 ? iList.Count - 1 : iNewIndex);

                    // if iNewIndex ==-1, we must add to end of list.
            }

            arg.Effects = System.Windows.DragDropEffects.None;

                // we need to prevent that the drag source removes the item because we already did that.  we need to do this because we can't assign an item to a new list if it hasn't already been removed from the previous (Owner prop prevents this).
        }

        /// <summary>The try create new code item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewCodeItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iNew = new CodeEditor(Brain.Current[iId]);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorBase;
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);
            var iList = Items;
            System.Diagnostics.Debug.Assert(iList != null);
            var iIndex = iList.IndexOf(iDroppedOn);
            if (iIndex > -1)
            {
                iList.Insert(iIndex, iNew);
            }
            else
            {
                iList.Add(iNew);
            }
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

        /// <summary>The f was expanded.</summary>
        private bool fWasExpanded; // so we can open/close folders while dragging over.

        #region TryAddAsChild

        /// <summary>
        ///     Gets/sets the value that indicates if we want to try and move to the
        ///     child list or always to the list that contains the item to which we
        ///     are attached.
        /// </summary>
        public bool TryAddAsChild { get; set; }

        #endregion

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public EditorCollection Items
        {
            get
            {
                var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorBase;
                if (iDroppedOn is EditorFolder && TryAddAsChild)
                {
                    // if we drop on a folder, we want to put it in the folder, otherwise, we want to put it at the same level as the item we drop on.
                    return ((EditorFolder)iDroppedOn).Items;
                }

                if (iDroppedOn.Owner is BrainData)
                {
                    return BrainData.Current.Editors;
                }

                return ((EditorFolder)iDroppedOn.Owner).Items;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>Called whenever an item is dragged into the drop target. By default,
        ///     doesn't do anything. Allows descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public override void OnDragEnter(System.Windows.DragEventArgs e)
        {
            base.OnDragEnter(e);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorFolder;
            if (iDroppedOn != null && TryAddAsChild)
            {
                fWasExpanded = iDroppedOn.IsExpanded;
                iDroppedOn.IsExpanded = true;
            }
        }

        /// <summary>Called whenever an item is dragged out of the drop target. By default,
        ///     doesn't do anything. Allows descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public override void OnDragLeave(System.Windows.DragEventArgs e)
        {
            base.OnDragLeave(e);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorFolder;
            if (iDroppedOn != null && TryAddAsChild)
            {
                iDroppedOn.IsExpanded = fWasExpanded;
            }
        }

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iItem = arg.Data.GetData(Properties.Resources.EDITORFORMAT) as EditorBase;
            if (iItem != null)
            {
                // && (arg.Effects & DragDropEffects.Move) == DragDropEffects.Move
                TryMoveItem(arg, dropPoint, iItem);
            }
            else
            {
                TryCreateNewCodeItem(arg, dropPoint);
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            if (obj.GetDataPresent(Properties.Resources.EDITORFORMAT))
            {
                var iItem = obj.GetData(Properties.Resources.EDITORFORMAT) as EditorBase;
                var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as EditorBase;
                return !iDroppedOn.IsChildOf(iItem);

                    // when the dragged item is a parent of the drop target, the drop is illegal.
            }

            return obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
        }

        #endregion
    }
}