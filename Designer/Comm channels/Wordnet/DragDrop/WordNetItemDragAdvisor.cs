// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItemDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Supplies drag support for wordnet items. Code is almost identical to
//   <see cref="WordNetItemGroupDragAdvisor" /> except for this pesque IsLoaded
//   prop.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Supplies drag support for wordnet items. Code is almost identical to
    ///     <see cref="WordNetItemGroupDragAdvisor" /> except for this pesque IsLoaded
    ///     prop.
    /// </summary>
    public class WordNetItemDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="WordNetItemDragAdvisor"/> class.</summary>
        public WordNetItemDragAdvisor()
        {
            SupportedFormat = Properties.Resources.NeuronIDFormat;

                // this is not really used, simply informtive: this is our main data type.
        }

        #region Item

        /// <summary>
        ///     Gets the code item that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        public WordNetItem Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).DataContext as WordNetItem;
            }
        }

        #endregion

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     Don't use preview events, otherwise the checkbox wont work.
        /// </remarks>
        /// <value>
        /// </value>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        /// <summary>Never do anything after a drag. Items simply stay where they are in
        ///     the same order.</summary>
        /// <param name="draggedElt">The dragged Elt.</param>
        /// <param name="finalEffects">The final Effects.</param>
        public override void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects)
        {
            // throw new NotImplementedException();
        }

        /// <summary>The is draggable.</summary>
        /// <param name="dragElt">The drag elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            // WordNetItem iItem = Item;
            return Item != null;
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

            var iData = new WordNetItemDragData(Item);

            AddUIElementToData(iObj, iDragged);
            iObj.SetData(Properties.Resources.WORDNETDRAGFORMAT, iData.GetXml());

            return iObj;
        }
    }
}