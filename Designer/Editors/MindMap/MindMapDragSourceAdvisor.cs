// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapDragSourceAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides logic for dragging items from a mindmappanel used by a mindmap
//   view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Provides logic for dragging items from a mindmappanel used by a mindmap
    ///     view.
    /// </summary>
    /// <remarks>
    ///     Supports for single item drag of neuron and links, but only multi item
    ///     support for neurons.
    /// </remarks>
    internal class MindMapDragSourceAdvisor : DnD.DragSourceBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMapDragSourceAdvisor"/> class. 
        ///     Initializes a new instance of the<see cref="MindMapDragSourceAdvisor"/> class.</summary>
        public MindMapDragSourceAdvisor()
            : base(Properties.Resources.NeuronIDFormat)
        {
            // this is not really used, simply informtive: this is our main data type.
        }

        #endregion

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
        private MindMap ItemsSource
        {
            get
            {
                var iItem = (MindMapItem)((System.Windows.FrameworkElement)SourceUI).DataContext;
                return iItem.Owner;
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

        /// <summary>We can drag all objects, except links, those can only be dragged if
        ///     the start or end part is selected.</summary>
        /// <remarks>The tag contains the info to indicate a start or end point.</remarks>
        /// <param name="dragElt"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            var iDragged = (System.Windows.FrameworkElement)dragElt;
            if (iDragged != null)
            {
                var iLink = iDragged.DataContext as MindMapLink;
                if (iLink != null)
                {
                    if (Equals(iDragged.Tag, "start") || Equals(iDragged.Tag, "end"))
                    {
                        return true;
                    }
                }
                else
                {
                    var iSource = ItemsSource;
                    var iItem = ((System.Windows.FrameworkElement)dragElt).DataContext as MindMapItem;
                    if (iItem != null)
                    {
                        return true;
                    }
                }
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
            var iObj = new System.Windows.DataObject();

            var iSource = ItemsSource;
            var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.MindMapPanel>(iDragged);

                // we need this to add the visual objects to the drag object.
            if (iSource != null)
            {
                if (iSource.SelectedItems.Count == 1)
                {
                    // need to store single data info.
                    var iData = iSource.SelectedItems[0];
                    System.Diagnostics.Debug.Assert(iData != null);

                    AddUIElementToData(iObj, iDragged);
                    iObj.SetData(Properties.Resources.MindMapItemFormat, iData);
                    if (iData is MindMapNeuron)
                    {
                        iObj.SetData(Properties.Resources.NeuronIDFormat, ((MindMapNeuron)iData).ItemID);
                    }
                    else if (iData is MindMapLink)
                    {
                        iObj.SetData(Properties.Resources.MindMapLinkSide, iDragged.Tag);
                    }
                }
                else
                {
                    // need to store list info of all the dragged items.
                    var iSelectedUI = new System.Collections.Generic.List<System.Windows.UIElement>();
                    var iSelected = new System.Collections.Generic.List<PositionedMindMapItem>();
                    foreach (var i in iSource.SelectedItems)
                    {
                        iSelectedUI.Add(iPanel.Children[iSource.Items.IndexOf(i)]);
                        if (i is PositionedMindMapItem)
                        {
                            iSelected.Add((PositionedMindMapItem)i);
                        }
                        else
                        {
                            throw new System.InvalidOperationException("Can't drag links and other objects at once.");
                        }
                    }

                    iObj.SetData(Properties.Resources.MultiMindMapItemFormat, iSelected);

                        // need to set the data is PositionedMindMapItems, cause that's easier to work with in other parts.
                    AddUIElementToData(iObj, iSelectedUI);
                    var iSelectedID =
                        (from i in iSource.SelectedItems where i is MindMapNeuron select ((MindMapNeuron)i).ItemID)
                            .ToList();
                    iObj.SetData(Properties.Resources.MultiNeuronIDFormat, iSelectedID);
                }
            }

            return iObj;
        }

        #region fiellds

        // List<MindMapItem> fSelected = new List<MindMapItem>(); 
        #endregion
    }
}