// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   The code item drop advisor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item drop advisor.</summary>
    public class CodeItemDropAdvisor : DnD.DropTargetBase
    {
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
        ///     When true, drops will be done by the actual neuron, they won't be
        ///     wrapped inside a CodeItem.
        /// </summary>
        /// <remarks>
        ///     This is required cause some props map directly to the neuron (see
        ///     <see cref="CodeitemConditionalStatement.CaseItem" /> ). -> no longer
        ///     used for CaseItem, still suppported.
        /// </remarks>
        public bool IsRaw { get; set; }

        /// <summary>
        ///     Gets/sets the type of code editor object that can be accepted.
        /// </summary>
        /// <remarks>
        ///     This property is used to check for valid drop point when a code item
        ///     is being dragged around By default this is empty, indicating all types
        ///     are allowed. When set, only objects from this type (or descendents)
        ///     are allowed).
        /// </remarks>
        public System.Type AllowedType { get; set; }

        #region Item

        /// <summary>
        ///     Gets the code item that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        /// <remarks>
        ///     Only valid when <see cref="IsRaw" /> = <see langword="false" />
        /// </remarks>
        public CodeItem Item
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as CodeItem;
            }
        }

        #endregion

        #region Neuron

        /// <summary>
        ///     Gets the neuron that is the datacontext of the target of this drop
        ///     advisor.
        /// </summary>
        /// <remarks>
        ///     Only valid when <see cref="IsRaw" /> = <see langword="true" />
        /// </remarks>
        public Neuron Neuron
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).DataContext as Neuron;
            }
        }

        #endregion

        #region Presenter

        /// <summary>
        ///     Gets the content presenter that should be used to display the data on.
        /// </summary>
        public System.Windows.Controls.ContentPresenter Presenter
        {
            get
            {
                return ((System.Windows.FrameworkElement)TargetUI).Tag as System.Windows.Controls.ContentPresenter;
            }
        }

        #endregion

        #region Overrides

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iItem = arg.Data.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
            if (iItem == null || IsRaw)
            {
                // we are not moving around an item, so add new code item. Also, when raw, we can't work with code items, instead we work on the neuron directly.
                TryCreateNewCodeItem(arg, dropPoint);
            }
            else if ((arg.Effects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
            {
                TryMoveItem(arg, dropPoint, iItem);
            }
            else
            {
                TryCreateNewCodeItem(arg, dropPoint);
            }
        }

        /// <summary>moves the code item. This is actually a fake move, there is always a
        ///     new object created. This is done so we can easely create duplicate
        ///     visual items that point to the same code item, which is automatically
        ///     done when the dragsource doesn't remove the item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop Point.</param>
        /// <param name="iItem">The i Item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, CodeItem iItem)
        {
            var iNew = EditorsHelper.CreateCodeItemFor(iItem.Item);
            System.Diagnostics.Debug.Assert(iNew != null);
            Presenter.Content = iNew;
        }

        /// <summary>The try create new code item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewCodeItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iPresenter = Presenter;

            if (IsRaw == false)
            {
                var iNew = EditorsHelper.CreateCodeItemFor(Brain.Current[iId]);
                System.Diagnostics.Debug.Assert(iNew != null);
                iPresenter.Content = iNew;
            }
            else
            {
                iPresenter.Content = Brain.Current[iId];
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            var iRes = false;
            System.Type iResultType = null;
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                var iSource = obj.GetData(Properties.Resources.DelayLoadResultType) as DnD.DragSourceBase;
                System.Diagnostics.Debug.Assert(iSource != null);
                iResultType = EditorsHelper.GetCodeItemTypeFor(iSource.GetDelayLoadResultType(obj));
            }

            if (IsRaw == false)
            {
                if (obj.GetDataPresent(Properties.Resources.CodeItemFormat))
                {
                    var iItem = obj.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
                    if (AllowedType != null)
                    {
                        if (iResultType == null)
                        {
                            iResultType = iItem.GetType();
                        }

                        iRes = Presenter.Content != iItem && AllowedType.IsAssignableFrom(iResultType);
                    }
                    else
                    {
                        iRes = Presenter.Content != iItem;
                    }
                }
                else if (obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
                {
                    if (AllowedType != null)
                    {
                        Neuron iNeuron = null;
                        if (iResultType == null)
                        {
                            var iNeuronID = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                            iNeuron = Brain.Current[iNeuronID];
                            iResultType = EditorsHelper.GetCodeItemTypeFor(iNeuron);
                        }

                        iRes = AllowedType.IsAssignableFrom(iResultType);
                    }
                    else
                    {
                        iRes = true;
                    }
                }
            }
            else
            {
                iRes = obj.GetDataPresent(Properties.Resources.NeuronIDFormat);
                if (iRes)
                {
                    if (iResultType == null)
                    {
                        var iNeuron = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                        iResultType = EditorsHelper.GetCodeItemTypeFor(Brain.Current[iNeuron].GetType());
                    }

                    iRes = AllowedType.IsAssignableFrom(iResultType);
                }
            }

            return iRes;
        }

        #endregion
    }
}