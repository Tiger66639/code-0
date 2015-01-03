// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeListItemDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for the child items of <see cref="CodeItemCollection" />
//   objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for the child items of <see cref="CodeItemCollection" />
    ///     objects.
    /// </summary>
    public class CodeListItemDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try move item.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="iItem">The i item.</param>
        private void TryMoveItem(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint, CodeItem iItem)
        {
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as CodeItem;
            var iList = CodeList;
            var iNewIndex = iList.IndexOf(iDroppedOn);
            var iOldIndex = iList.IndexOf(iItem);
            if (iOldIndex == -1)
            {
                var iNew = EditorsHelper.CreateCodeItemFor(iItem.Item);

                    // we always create a new item, even with a move, cause we don't know how the item was being dragged (with shift pressed or not), this is the savest and easiest way.
                iList.Insert(iNewIndex, iNew);
            }
            else
            {
                iList.Move(iOldIndex, iNewIndex);
                arg.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <summary>The try create new code item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewCodeItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iNew = EditorsHelper.CreateCodeItemFor(Brain.Current[iId]);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as CodeItem;
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);
            var iCodeList = CodeList;
            System.Diagnostics.Debug.Assert(iCodeList != null);
            iCodeList.Insert(iCodeList.IndexOf(iDroppedOn), iNew);
        }

        #region prop

        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub lists don't get used but
        ///     only the main list cause this gets the events first, while we usually
        ///     want to drop in a sublist.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
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
                var iCodeItem = ((System.Windows.FrameworkElement)TargetUI).DataContext as CodeItem;
                System.Diagnostics.Debug.Assert(iCodeItem != null);
                var iOwner = iCodeItem.Owner as ICodeItemsOwner;
                System.Diagnostics.Debug.Assert(iOwner != null);
                return iOwner.Items;
            }
        }

        #endregion

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

        #endregion

        #region Overrides

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iItem = arg.Data.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
            if (iItem != null && CodeList.Contains(iItem)
                && (arg.Effects & System.Windows.DragDropEffects.Move) == System.Windows.DragDropEffects.Move)
            {
                TryMoveItem(arg, dropPoint, iItem);
            }
            else
            {
                TryCreateNewCodeItem(arg, dropPoint); // we are not moving around an item, so add new code item.
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            var iTarget = CodeList;
            if (iTarget != null)
            {
                if (AllowedType != null)
                {
                    var iResultType = GetResultType(obj);
                    if (iResultType != null)
                    {
                        return AllowedType.IsAssignableFrom(iResultType) && CheckNoDataRecursion(obj);
                    }

                    return false;
                }

                if (obj.GetDataPresent(Properties.Resources.CodeItemFormat)
                    || obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                    || obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
                {
                    return CheckNoDataRecursion(obj);
                }
            }

            return false;
        }

        /// <summary>The check no data recursion.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckNoDataRecursion(System.Windows.IDataObject obj)
        {
            var iTarget = CodeList;
            System.Diagnostics.Debug.Assert(iTarget != null);
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                return true;
            }

            if (iTarget.Cluster == null)
            {
                return true;
            }

            if (obj.GetDataPresent(Properties.Resources.CodeItemFormat))
            {
                var iItem = obj.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
                if (iItem.Item != null && iItem.Item.ID != iTarget.Cluster.ID)
                {
                    return CheckOwners(iItem, iTarget.Owner as CodeItem);
                }

                return false;
            }

            if (obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iNeuronID = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                return iNeuronID != iTarget.Cluster.ID;
            }

            return true;
        }

        /// <summary>The check owners.</summary>
        /// <param name="item">The item.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckOwners(CodeItem item, CodeItem parent)
        {
            if (item is CodeItemCodeBlock)
            {
                // codeblocks may be nested.
                return true;
            }

            while (parent != null)
            {
                if (item.Item.ID == parent.Item.ID)
                {
                    return false;
                }

                parent = parent.Owner as CodeItem;
            }

            return true;
        }

        /// <summary>The get result type.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        private System.Type GetResultType(System.Windows.IDataObject obj)
        {
            System.Type iResultType = null;
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                var iSource = obj.GetData(Properties.Resources.DelayLoadResultType) as DnD.DragSourceBase;
                System.Diagnostics.Debug.Assert(iSource != null);
                iResultType = EditorsHelper.GetCodeItemTypeFor(iSource.GetDelayLoadResultType(obj));
            }

            if (obj.GetDataPresent(Properties.Resources.CodeItemFormat))
            {
                var iItem = obj.GetData(Properties.Resources.CodeItemFormat) as CodeItem;
                if (iResultType == null)
                {
                    iResultType = iItem.GetType();
                }
            }
            else if (obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                Neuron iNeuron = null;
                if (iResultType == null)
                {
                    var iNeuronID = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                    iNeuron = Brain.Current[iNeuronID];
                    iResultType = EditorsHelper.GetCodeItemTypeFor(iNeuron);
                }
            }

            return iResultType;
        }

        #endregion
    }
}