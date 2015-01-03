// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubRelationshipsDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The sub relationships drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The sub relationships drop advisor.</summary>
    public class SubRelationshipsDropAdvisor : DnD.DropTargetBase
    {
        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public System.Windows.Controls.ItemsControl Target
        {
            get
            {
                return ControlFramework.Utility.TreeHelper.FindInTree<System.Windows.Controls.ItemsControl>(TargetUI);
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
        public SubRelationshipsCollection Items
        {
            get
            {
                return Target.ItemsSource as SubRelationshipsCollection;
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
                var iFound = (from i in iItems where i.Relationship.ID == iId select i).FirstOrDefault();
                if (iFound == null)
                {
                    // Get the relationship neuron and add it to the thesaurus in the list of non recursive relationships (since a subItem relationship is always none recursive)
                    var iRelNeuron = Brain.Current[iId];
                    System.Diagnostics.Debug.Assert(iRelNeuron != null);
                    var iRelItem = new ThesaurusRelItem(iRelNeuron);
                    BrainData.Current.Thesaurus.NoRecursiveRelationships.Add(iRelItem);

                    // create the cluster that will contain all the related objects, link that to the source object, so that it
                    // knows all the related words for the specified relationship. the source object is the currently selected
                    // thesaurus neuron, since that's the only location that we can do a drop.
                    var iCluster = NeuronFactory.GetCluster();
                    WindowMain.AddItemToBrain(iCluster);
                    iCluster.Meaning = iRelNeuron.ID;
                    Link.Create(BrainData.Current.Thesaurus.SelectedItem.Item, iCluster, iRelNeuron.ID);
                    var iSubCol = new ThesaurusSubItemCollection(iItems, iCluster, iRelNeuron);
                    iItems.Add(iSubCol);
                }
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