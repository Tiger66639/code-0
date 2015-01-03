// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   drop advisor for ui elements that dipslay a root asset.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     drop advisor for ui elements that dipslay a root asset.
    /// </summary>
    public class AssetDropAdvisor : DnD.DropTargetBase
    {
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

        /// <summary>
        ///     Gets the target.
        /// </summary>
        public WPF.Controls.TreeViewPanel Target
        {
            get
            {
                return ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.TreeViewPanel>(TargetUI);
            }
        }

        #region Items

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public System.Collections.Generic.IList<AssetItem> Items
        {
            get
            {
                return Target.ItemsSource.TreeItems as System.Collections.Generic.IList<AssetItem>;
            }
        }

        #endregion

        #region Overrides

        /// <summary>Raises the <see cref="DropCompleted"/> event.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iDropped = Brain.Current[iId];

            var iFound = (from i in Items where i.Item.ID == iId select i).FirstOrDefault();
            if (iFound == null)
            {
                // we check that we don't want to add an item 2 times to the same list.
                var iRel = BrainData.Current.Thesaurus.SelectedRelationship;
                BrainData.Current.Thesaurus.AddRootItem(iRel, iDropped);
                var iUndo = new RootItemUndoData(BrainAction.Created, iRel, iDropped);
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            }
        }

        /// <summary>Determines whether [is valid data object] [the specified obj].</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise,<c>false</c> .</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return Items != null
                   && (obj.GetDataPresent(Properties.Resources.ASSETRECORDFORMAT)
                       || obj.GetDataPresent(Properties.Resources.MULTIASSETRECORDFORMAT)
                       || obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                       || obj.GetDataPresent(Properties.Resources.MultiNeuronIDFormat));
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