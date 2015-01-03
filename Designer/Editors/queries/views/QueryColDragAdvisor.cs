// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryColDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The query col drag advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The query col drag advisor.</summary>
    public class QueryColDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="QueryColDragAdvisor"/> class.</summary>
        public QueryColDragAdvisor()
            : base(Properties.Resources.ASSETCOLUMN)
        {
        }

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <value>
        /// </value>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets the items source.
        /// </summary>
        private QueryColumn ItemsSource
        {
            get
            {
                return (QueryColumn)((System.Windows.FrameworkElement)SourceUI).DataContext;
            }
        }

        /// <summary>Called when the drag is finished</summary>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <param name="finalEffects">The final effects.</param>
        public override void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects)
        {
        }

        /// <summary>We can drag when there are items selected.</summary>
        /// <remarks>The tag contains the info to indicate a start or end point.</remarks>
        /// <param name="dragElt"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            var iDragged = (System.Windows.FrameworkElement)dragElt;
            return iDragged != null && iDragged.DataContext is QueryColumn;
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

            var iSource = ItemsSource;
            if (iSource != null)
            {
                AddUIElementToData(iObj, iDragged);
                iObj.SetData(Properties.Resources.ASSETCOLUMN, iSource.Index);
            }

            return iObj;
        }
    }
}