// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides drag features for a frame in a frame list of a frame editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides drag features for a frame in a frame list of a frame editor.
    /// </summary>
    public class FrameDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="FrameDragAdvisor"/> class.</summary>
        public FrameDragAdvisor()
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
                return true;
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the code item that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        public Frame Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).DataContext as Frame;
            }
        }

        #endregion

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public System.Collections.Generic.IList<Frame> Items
        {
            get
            {
                var iDataRow =
                    ControlFramework.Utility.TreeHelper.FindInTree<System.Windows.Controls.ListBoxItem>(SourceUI);
                if (iDataRow != null)
                {
                    var iItemsControl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(iDataRow);
                    System.Diagnostics.Debug.Assert(iItemsControl != null);
                    return iItemsControl.ItemsSource as System.Collections.Generic.IList<Frame>;
                }

                return null;
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
            if ((finalEffects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
            {
                var iItem = Item;
                var iList = Items;
                System.Diagnostics.Debug.Assert(iList != null);
                iList.Remove(Item);
            }
        }

        /// <summary>We can drag when there is a content in the presenter.</summary>
        /// <param name="dragElt">The drag Elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            return Item != null && Items != null;
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

            AddUIElementToData(iObj, iDragged);

            // iObj.SetData(Properties.Resources.FrameElementFormat, iContent);
            iObj.SetData(Properties.Resources.NeuronIDFormat, iContent.Item.ID);

            return iObj;
        }
    }
}