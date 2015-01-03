// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusListItemDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   provides drag featuer for thesaurus items. A thesaurus item is stored as
//   a NeuronId together with the selected relationship under
//   "ThesaurusRelationshipFormat"
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides drag featuer for thesaurus items. A thesaurus item is stored as
    ///     a NeuronId together with the selected relationship under
    ///     "ThesaurusRelationshipFormat"
    /// </summary>
    public class ThesaurusListItemDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="ThesaurusListItemDragAdvisor"/> class.</summary>
        public ThesaurusListItemDragAdvisor()
        {
            SupportedFormat = Properties.Resources.NeuronIDFormat;

                // this is not really used, simply informtive: this is our main data type.
        }

        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub drop points don't get used
        ///     but only the main list cause this gets the events first, while we
        ///     usually want to drop in a sub drop point.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the code item that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        public ThesaurusItem Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).DataContext as ThesaurusItem;
            }
        }

        #endregion

        /// <summary>The finish drag.</summary>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <param name="finalEffects">The final effects.</param>
        public override void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects)
        {
            // do nothing
        }

        /// <summary>We can drag when there is a content in the presenter.</summary>
        /// <param name="dragElt">The drag Elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            return Item != null && Item.Item != null;
        }

        /// <summary>we <see langword="override"/> cause we put the image to use + an<see langword="ulong"/> if it is a neuron, or a <see langword="ref"/>
        ///     to the mind map item. If the item is a link, we also store which side
        ///     of the link it was, so we can adjust it again (+ update it).</summary>
        /// <param name="draggedElt">The dragged Elt.</param>
        /// <returns>The <see cref="DataObject"/>.</returns>
        public override System.Windows.DataObject GetDataObject(System.Windows.UIElement draggedElt)
        {
            var iDragged = (System.Windows.FrameworkElement)draggedElt;
            var iObj = new System.Windows.DataObject();

            var iContent = Item;

            if (iContent != null && iContent.Item != null)
            {
                AddUIElementToData(iObj, iDragged);
                iObj.SetData(Properties.Resources.NeuronIDFormat, iContent.Item.ID);
                iObj.SetData(Properties.Resources.IsThesRelRecursiveFormat, true);
                iObj.SetText(iContent.NeuronInfo.DisplayTitle); // also add this, so we can support regular drag drop.
                var iRel = BrainData.Current.Thesaurus.SelectedRelationship;
                if (iRel != null)
                {
                    iObj.SetData(Properties.Resources.ThesaurusRelationshipFormat, iRel.ID);
                }
            }

            return iObj;
        }
    }
}