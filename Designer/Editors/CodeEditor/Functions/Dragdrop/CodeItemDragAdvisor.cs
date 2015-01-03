// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemDragAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The code item drag advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item drag advisor.</summary>
    internal class CodeItemDragAdvisor : DnD.DragSourceBase
    {
        /// <summary>Initializes a new instance of the <see cref="CodeItemDragAdvisor"/> class.</summary>
        public CodeItemDragAdvisor()
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

        /// <summary>
        ///     When true, drags will be done with the actual neuron, they won't be
        ///     wrapped inside a CodeItem.
        /// </summary>
        /// <remarks>
        ///     This is required cause some props map directly to the neuron (see
        ///     <see cref="CodeitemConditionalStatement.CaseItem" /> ). -> no longer
        ///     used, still suppported.
        /// </remarks>
        public bool IsRaw { get; set; }

        #region Presenter

        /// <summary>
        ///     Gets the content presenter that should be used to display the data on.
        /// </summary>
        public System.Windows.Controls.ContentPresenter Presenter
        {
            get
            {
                return ((System.Windows.FrameworkElement)SourceUI).Tag as System.Windows.Controls.ContentPresenter;
            }
        }

        #endregion

        /// <summary>Finishes the drag.</summary>
        /// <param name="draggedElt">The dragged elt.</param>
        /// <param name="finalEffects">The final effects.</param>
        public override void FinishDrag(
            System.Windows.UIElement draggedElt, 
            System.Windows.DragDropEffects finalEffects)
        {
            if ((finalEffects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
            {
                var iPresenter = Presenter;
                System.Diagnostics.Debug.Assert(iPresenter != null);
                iPresenter.Content = null;
            }
        }

        /// <summary>We can drag when there is a content in the presenter.</summary>
        /// <param name="dragElt">The drag Elt.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsDraggable(System.Windows.UIElement dragElt)
        {
            var iPresenter = Presenter;
            System.Diagnostics.Debug.Assert(iPresenter != null);
            return iPresenter.Content != null;
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

            var iPresenter = Presenter;
            if (iPresenter != null)
            {
                var iContent = (CodeItem)iPresenter.Content;
                if (iContent != null)
                {
                    AddUIElementToData(iObj, iDragged);
                    if (IsRaw == false)
                    {
                        iObj.SetData(Properties.Resources.CodeItemFormat, iContent);
                        iObj.SetData(Properties.Resources.NeuronIDFormat, iContent.Item.ID);
                    }
                    else
                    {
                        var iNeuron = (Neuron)iPresenter.Content;
                        iObj.SetData(Properties.Resources.NeuronIDFormat, iNeuron.ID);
                    }

                    return iObj;
                }
            }

            return null;
        }
    }
}