// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerDragSourceAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The explorer drag source advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>The explorer drag source advisor.</summary>
    public class ExplorerDragSourceAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerDragSourceAdvisor"/> class.</summary>
        public ExplorerDragSourceAdvisor()
        {
            SupportedFormat = Properties.Resources.NeuronIDFormat;

                // this is not really used, simply informtive: this is our main data type.
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
            var iDragged = (System.Windows.FrameworkElement)dragElt;
            if (iDragged != null)
            {
                var iData = iDragged.DataContext as NeuronExplorerItem;
                return iData != null && iData.Item != null;
            }

            return false;
        }

        /// <summary>we <see langword="override"/> cause we put the image to use + an<see langword="ulong"/> if it is a neuron, or a <see langword="ref"/>
        ///     to the mind map item. If the item is a link, we also store which side
        ///     of the link it was, so we can adjust it again (+ update it).</summary>
        /// <param name="draggedElt">The dragged Elt.</param>
        /// <returns>The <see cref="DataObject"/>.</returns>
        public override System.Windows.DataObject GetDataObject(System.Windows.UIElement draggedElt)
        {
            var iDragged = (System.Windows.FrameworkElement)draggedElt;
            System.Diagnostics.Debug.Assert(SourceUI is System.Windows.Controls.ContentControl);
            var iSource = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(SourceUI);
            var iExplorer = iSource.DataContext as NeuronExplorer;
            System.Diagnostics.Debug.Assert(iExplorer != null);
            if (iSource != null)
            {
                var iObj = new System.Windows.DataObject();
                if (iExplorer.Selection.SelectionCount == 1)
                {
                    // need to store single data info.
                    var iData = iDragged.DataContext as NeuronExplorerItem;
                    System.Diagnostics.Debug.Assert(iData != null);

                    AddUIElementToData(iObj, iDragged);
                    iObj.SetData(Properties.Resources.NeuronIDFormat, iData.Item.ID);
                }
                else
                {
                    // need to store list info of all the dragged items.
                    if (iExplorer.Selection.SelectionCount < int.MaxValue)
                    {
                        var iSelectedUI = new System.Collections.Generic.List<System.Windows.UIElement>();
                        foreach (var i in iExplorer.Selection.SelectedItems)
                        {
                            iSelectedUI.Add(
                                (System.Windows.UIElement)iSource.ItemContainerGenerator.ContainerFromItem(i));
                        }

                        iObj.SetData(Properties.Resources.MultiUIElementFormat, iSelectedUI);
                        var iSelectedID = Enumerable.ToList(iExplorer.Selection.SelectedIds);
                        iObj.SetData(Properties.Resources.MultiNeuronIDFormat, iSelectedID);
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            "There are to many items selected to perform a drag operation!");
                    }
                }

                return iObj;
            }

            return null;
        }
    }
}