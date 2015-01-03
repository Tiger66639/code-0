// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   drag advisor for UI element that displays the root asset.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     drag advisor for UI element that displays the root asset.
    /// </summary>
    public class AssetDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="AssetDragAdvisor"/> class.</summary>
        public AssetDragAdvisor()
            : base(Properties.Resources.ASSETRECORDFORMAT)
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

        /// <summary>Gets the items source.</summary>
        private ObjectEditor ItemsSource
        {
            get
            {
                var iItem = (AssetItem)((System.Windows.FrameworkElement)SourceUI).DataContext;
                return iItem.Root;
            }
        }

        /// <summary>The finish drag.</summary>
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
            return iDragged != null && iDragged.DataContext is AssetItem;
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
            var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.TreeViewPanel>(iDragged);

                // we need this to add the visual objects to the drag object.
            if (iSource != null)
            {
                if (iSource.SelectedItems.Count == 1)
                {
                    // need to store single data info.
                    var iData = iSource.SelectedItems[0] as AssetItem;
                    System.Diagnostics.Debug.Assert(iData != null);
                    AddUIElementToData(iObj, iDragged);
                    iObj.SetData(Properties.Resources.ASSETRECORDFORMAT, iData);
                    iObj.SetData(Properties.Resources.NeuronIDFormat, iData.Item.ID);
                }
                else
                {
                    // need to store list info of all the dragged items.
                    var iSelectedUI = new System.Collections.Generic.List<System.Windows.UIElement>();
                    var iSelAssets = new System.Collections.Generic.List<AssetItem>();
                    var iSelected = new System.Collections.Generic.List<ulong>();
                    foreach (AssetItem i in iSource.SelectedItems)
                    {
                        iSelectedUI.Add(FindUIElFor(i, iPanel));
                        iSelected.Add(i.Item.ID);
                        iSelAssets.Add(i); // 1 loop to get all the items
                    }

                    iObj.SetData(Properties.Resources.MultiMindMapItemFormat, iSelAssets);
                    AddUIElementToData(iObj, iSelectedUI);
                    iObj.SetData(Properties.Resources.MultiNeuronIDFormat, iSelected);
                }
            }

            return iObj;
        }

        /// <summary>The find ui el for.</summary>
        /// <param name="toFind">The to find.</param>
        /// <param name="panel">The panel.</param>
        /// <returns>The <see cref="UIElement"/>.</returns>
        private static System.Windows.UIElement FindUIElFor(AssetItem toFind, WPF.Controls.TreeViewPanel panel)
        {
            foreach (System.Windows.FrameworkElement i in panel.Children)
            {
                if (i.DataContext == toFind)
                {
                    return i;
                }
            }

            return null;
        }
    }
}