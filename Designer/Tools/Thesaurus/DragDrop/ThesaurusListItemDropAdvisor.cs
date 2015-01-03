// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusListItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The thesaurus list item drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The thesaurus list item drop advisor.</summary>
    public class ThesaurusListItemDropAdvisor : DnD.DropTargetBase
    {
        #region prop

        #region Target

        /// <summary>Gets the target.</summary>
        public ThesaurusItem Target
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as ThesaurusItem;
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

        #endregion

        #region Overrides

        /// <summary>Raises the <see cref="DropCompleted"/> event.</summary>
        /// <param name="arg">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iTarget = Target;
            System.Diagnostics.Debug.Assert(iTarget != null);
            if (arg.Data.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat);
                var iDropped = Brain.Current[iId];

                var iIsExpanded = iTarget.IsExpanded;
                iTarget.IsExpanded = true;

                    // we need to check if the droptarget already has the dragged object as a child, best and fastes way is by letting the item load it's children in the way it normally does.
                try
                {
                    var iFound = (from i in iTarget.Items where i.Item.ID == iId select i).FirstOrDefault();
                    if (iFound == null)
                    {
                        // we check that we don't want to add an item 2 times to the same list.
                        Thesaurus.CreateRelationship(iTarget.Item, iDropped, iTarget.Root.SelectedRelationship);
                    }
                }
                finally
                {
                    iTarget.IsExpanded = iIsExpanded;
                }
            }
            else if (arg.Data.GetDataPresent(Properties.Resources.WORDNETDRAGFORMAT))
            {
                var iRaw = (string)arg.Data.GetData(Properties.Resources.WORDNETDRAGFORMAT);
                var iData = WordNetItemDragData.ParseXml(iRaw);
                ThesauruscsvStreamer.ImportFromWordNet(
                    iData.Synonyms, 
                    iTarget, 
                    iTarget.Root.SelectedRelationship, 
                    iData.Description);
            }
        }

        /// <summary>Determines whether [is valid data object] [the specified obj].</summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if [is valid data object] [the specified obj]; otherwise,<c>false</c> .</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return Target != null
                   && (obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                       || obj.GetDataPresent(Properties.Resources.WORDNETDRAGFORMAT));
        }

        #endregion
    }
}