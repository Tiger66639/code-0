// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The timer drag advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The timer drag advisor.</summary>
    public class TimerDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="TimerDragAdvisor"/> class.</summary>
        public TimerDragAdvisor()
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
        public NeuralTimer Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).DataContext as NeuralTimer;
            }
        }

        #endregion

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public TimerCollection Items
        {
            get
            {
                var iDataRow =
                    ControlFramework.Utility.TreeHelper.FindInTree<System.Windows.Controls.DataGridRow>(SourceUI);
                if (iDataRow != null)
                {
                    var iItemsControl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(iDataRow);
                    System.Diagnostics.Debug.Assert(iItemsControl != null);
                    return iItemsControl.ItemsSource as TimerCollection;
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
            // don't do anything, a drag will always leat the timer on the list.
            // if ((finalEffects & DragDropEffects.Move) == DragDropEffects.Move)
            // {
            // NeuralTimer iItem = Item;
            // TimerCollection iList = Items;
            // Debug.Assert(iList != null);
            // iList.Remove(Item);
            // }
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
            iObj.SetData(Properties.Resources.NeuronIDFormat, iContent.Item.ID);

            return iObj;
        }
    }
}