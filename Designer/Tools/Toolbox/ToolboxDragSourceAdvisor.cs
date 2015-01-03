// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolboxDragSourceAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides drag capabalities to the toolbox.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides drag capabalities to the toolbox.
    /// </summary>
    /// <remarks>
    ///     When an item is dragged from the toolbox, the ID and neuron is only
    ///     stored on the data object once the drag is finished.
    /// </remarks>
    public class ToolboxDragSourceAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="ToolboxDragSourceAdvisor"/> class.</summary>
        public ToolboxDragSourceAdvisor()
        {
            SupportedFormat = Properties.Resources.ToolboxItemFormat;

                // this is not really used, simply informtive: this is our main data type.
        }

        /// <summary>The finish drag.</summary>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <param name="finalEffects">The final effects.</param>
        public override void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects)
        {
            // do nothing
        }

        /// <summary>The is draggable.</summary>
        /// <param name="dragElt">The drag elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            var iDragged = (System.Windows.FrameworkElement)dragElt;
            if (iDragged != null)
            {
                return iDragged.DataContext is ToolBoxItem;
            }

            return false;
        }

        /// <summary>we <see langword="override"/> cause we put the image to use + the
        ///     neuron, or a <see langword="ref"/> to the toolbox item.</summary>
        /// <param name="draggedElt">The dragged Elt.</param>
        /// <returns>The <see cref="DataObject"/>.</returns>
        public override System.Windows.DataObject GetDataObject(System.Windows.UIElement draggedElt)
        {
            var iDragged = (System.Windows.FrameworkElement)draggedElt;
            var iObj = new System.Windows.DataObject();
            ToolBoxItem iToolboxItem;

            System.Diagnostics.Debug.Assert(SourceUI is System.Windows.Controls.ListBoxItem);
            var iSource =
                System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(SourceUI) as
                System.Windows.Controls.ListBox;
            if (iSource.SelectedItems.Count == 1)
            {
                // need to store single data info.
                iToolboxItem = iDragged.DataContext as ToolBoxItem;
                System.Diagnostics.Debug.Assert(iToolboxItem != null);

                AddUIElementToData(iObj, iDragged);
                iObj.SetData(Properties.Resources.ToolboxItemFormat, iToolboxItem);
                iObj.SetData(Properties.Resources.DelayLoadFormat, this);

                    // we store a ref to this object in the data so we can delay create the Neuron when it is really consumed.
                iObj.SetData(Properties.Resources.DelayLoadResultType, this);
                iObj.SetData(Properties.Resources.NeuronIDFormat, (ulong)0);
            }

            return iObj;
        }

        /// <summary>If this drag source object supports delay loading of the data,
        ///     descendents should reimplement this function.</summary>
        /// <param name="data">The data.</param>
        public override void DelayLoad(System.Windows.IDataObject data)
        {
            var iToolboxItem = data.GetData(Properties.Resources.ToolboxItemFormat) as ToolBoxItem;
            System.Diagnostics.Debug.Assert(iToolboxItem != null);
            data.SetData(Properties.Resources.NeuronIDFormat, iToolboxItem.GetData().ID);
        }

        /// <summary>The get delay load result type.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        public override System.Type GetDelayLoadResultType(System.Windows.IDataObject data)
        {
            var iToolboxItem = data.GetData(Properties.Resources.ToolboxItemFormat) as ToolBoxItem;
            System.Diagnostics.Debug.Assert(iToolboxItem != null);
            return iToolboxItem.GetResultType();
        }
    }
}