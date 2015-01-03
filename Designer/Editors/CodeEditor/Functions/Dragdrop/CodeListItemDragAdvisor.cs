// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeListItemDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The code list item drag advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code list item drag advisor.</summary>
    public class CodeListItemDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="CodeListItemDragAdvisor"/> class.</summary>
        public CodeListItemDragAdvisor()
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
        public CodeItem Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).DataContext as CodeItem;
            }
        }

        #endregion

        #region CodeList

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public CodeItemCollection CodeList
        {
            get
            {
                var iCodeItem = ((System.Windows.FrameworkElement)SourceUI).DataContext as CodeItem;
                System.Diagnostics.Debug.Assert(iCodeItem != null);
                var iOwner = iCodeItem.Owner as ICodeItemsOwner;
                if (iOwner != null)
                {
                    var iStatement = iOwner as CodeItemConditionalStatement;
                    if (iStatement != null)
                    {
                        // if(iStatement.child
                    }

                    return iOwner.Items;
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
                var iList = CodeList;
                if (iList != null)
                {
                    iList.Remove(Item);
                }
            }
        }

        /// <summary>We can drag when there is a content in the presenter.</summary>
        /// <param name="dragElt">The drag Elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            return Item != null && CodeList != null;
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
            iObj.SetData(Properties.Resources.CodeItemFormat, iContent);
            iObj.SetData(Properties.Resources.NeuronIDFormat, iContent.Item.ID);
            var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.CodePagePanel>(SourceUI);
            return iObj;
        }
    }
}