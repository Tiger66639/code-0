// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubThesaurusCollectionDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   provides drop features to an ItemsControl that contains ThesaurusSubItems
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides drop features to an ItemsControl that contains ThesaurusSubItems
    /// </summary>
    public class SubThesaurusCollectionDropAdvisor : DnD.DropTargetBase
    {
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
        public ThesaurusSubItemCollection Items
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as ThesaurusSubItemCollection;
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
            var iItems = Items;
            if (iItems != null)
            {
                var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
                var iItem = Brain.Current[iId];
                var iCluster = iItem as NeuronCluster;

                    // usually, we only use objects in a thesaurus relationship but this is not required, so check for this and ask the user what to do.
                if (iCluster == null || iCluster.Meaning != (ulong)PredefinedNeurons.Object)
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            "Are you certain you want to use a non-object in a thesaurus relationship?", 
                            "Add thesaurus relationship", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Question, 
                            System.Windows.MessageBoxResult.No);
                    if (iRes == System.Windows.MessageBoxResult.No)
                    {
                        return;
                    }
                }

                var iSubItem = new ThesaurusSubItem(iItem);
                iItems.Add(iSubItem);
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return Items != null && obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
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