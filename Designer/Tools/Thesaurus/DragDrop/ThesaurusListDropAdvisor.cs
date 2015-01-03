// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusListDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for an ItemsControl containing thesaurus elements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Drop advisor for an ItemsControl containing thesaurus elements.
    /// </summary>
    public class ThesaurusListDropAdvisor : DnD.DropTargetBase
    {
        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public WPF.Controls.TreeViewPanel Target
        {
            get
            {
                return ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.TreeViewPanel>(TargetUI);
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
        public Data.ObservedCollection<ThesaurusItem> Items
        {
            get
            {
                return Target.ItemsSource.TreeItems as Data.ObservedCollection<ThesaurusItem>;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>Raises the <see cref="DropCompleted"/> event.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            ThesaurusItem iFound = null;
            NeuronCluster iDropped = null;
            if (arg.Data.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
                iDropped = Brain.Current[iId] as NeuronCluster;

                iFound = (from i in Items where i.Item.ID == iId select i).FirstOrDefault();
            }
            else if (arg.Data.GetDataPresent(Properties.Resources.WORDNETDRAGFORMAT))
            {
                var iRaw = (string)arg.Data.GetData(Properties.Resources.WORDNETDRAGFORMAT);
                var iData = WordNetItemDragData.ParseXml(iRaw);
                if (iData.Synonyms.Count > 0)
                {
                    iDropped = EditorsHelper.CreateObject(iData.Synonyms[0], iData.Description);
                    var iPos = Brain.Current[WordNetSin.GetPos(iData.POS)];
                    iDropped.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, iPos);
                    for (var i = 1; i < iData.Synonyms.Count; i++)
                    {
                        EditorsHelper.AddSynonym(iDropped, iData.Synonyms[i], iPos);
                    }
                }
            }

            if (iFound == null && iDropped != null)
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
            return Items != null && BrainData.Current.Thesaurus.SelectedRelationship != null
                   && (obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                       || obj.GetDataPresent(Properties.Resources.WORDNETDRAGFORMAT));
        }

        #endregion
    }
}