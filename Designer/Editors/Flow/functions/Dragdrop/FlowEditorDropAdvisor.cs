// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowEditorDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow editor drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The flow editor drop advisor.</summary>
    public class FlowEditorDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>Tries the move item.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, Flow iItem)
        {
            var iList = Items;
            var iIndex = iList.IndexOf(iItem);
            if (iIndex == -1)
            {
                // we need to create a new item if the item being moved wasn't in this list or when a copy was requested because shift was pressed.
                var iOwner = iItem.Owner as FlowEditor;
                if (iOwner != null)
                {
                    iOwner.Flows.Remove(iItem); // it's in the same list, so first remove it.
                }

                iList.Add(iItem);
            }
            else
            {
                iList.Move(iIndex, iList.Count - 1);
            }

            arg.Effects = System.Windows.DragDropEffects.None;
        }

        /// <summary>The try create new item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);

            var iCluster = Brain.Current[iId] as NeuronCluster;
            if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Flow)
            {
                var iNew = new Flow();
                iNew.ItemID = iId;
                iNew.Name = iNew.NeuronInfo.DisplayTitle;
                System.Diagnostics.Debug.Assert(iNew != null);
                var iList = Items;
                System.Diagnostics.Debug.Assert(iList != null);
                iList.Add(iNew);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "The dropped object was not a flow, can't add it to the flow editor", 
                    "Drag drop", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
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
        public FlowCollection Items
        {
            get
            {
                return Target.ItemsSource as FlowCollection;
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
            var iItem = arg.Data.GetData(Properties.Resources.FLOWEDITORFORMAT) as Flow;
            if (iItem != null
                && (Items.Contains(iItem)
                    || (arg.Effects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move))
            {
                TryMoveItem(arg, dropPoint, iItem);
            }
            else
            {
                TryCreateNewItem(arg, dropPoint); // we are not moving around an item, so add new code item.
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return obj.GetDataPresent(Properties.Resources.FLOWEDITORFORMAT)
                   || obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
        }

        #endregion
    }
}