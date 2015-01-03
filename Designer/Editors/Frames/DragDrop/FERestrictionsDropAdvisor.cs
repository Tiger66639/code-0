// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionsDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   a drop advisor for creating new Frame element restrictions, based on the
//   data being dropped.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a drop advisor for creating new Frame element restrictions, based on the
    ///     data being dropped.
    /// </summary>
    public class FERestrictionsDropAdvisor : DnD.DropTargetBase
    {
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

        #region Items

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public RestrictionsCollection Items
        {
            get
            {
                var iRoot = TargetUI as WPF.Controls.TreeViewPanel;
                if (iRoot != null && iRoot.ItemsSource != null)
                {
                    return ((FrameElement)iRoot.ItemsSource).RestrictionsRoot.Items;
                }

                var iItem = TargetUI as WPF.Controls.TreeViewPanelItem;
                if (iItem != null)
                {
                    var iGrp = iItem.DataContext as FERestrictionGroup;
                    if (iGrp != null)
                    {
                        return iGrp.Items;
                    }

                    var iData = iItem.DataContext as FERestrictionBase;
                    if (iData.ParentTreeItem != null)
                    {
                        return iData.ParentTreeItem.TreeItems as RestrictionsCollection;
                    }

                    return ((FrameElement)iData.Owner).RestrictionsRoot.Items;
                }

                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance is handling a drop for
        ///     an ObjectFrames editor and not for a (non attached/loose) frames
        ///     editor.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is for object frame; otherwise,
        ///     <c>false</c> .
        /// </value>
        public bool IsForObjectFrame
        {
            get
            {
                var iRoot = TargetUI as WPF.Controls.TreeViewPanel;
                if (iRoot != null && iRoot.ItemsSource != null)
                {
                    return ((Frame)((FrameElement)iRoot.ItemsSource).Owner).Owner is ObjectFramesEditor;
                }

                var iItem = TargetUI as WPF.Controls.TreeViewPanelItem;
                if (iItem != null)
                {
                    var iData = iItem.DataContext as FERestrictionBase;
                    return ((Frame)iData.Root.Owner).Owner is ObjectFramesEditor;
                }

                return false;
            }
        }

        #endregion

        #region Overrides

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            var iItems = Items;
            System.Diagnostics.Debug.Assert(iItems != null);
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                Neuron iItem;
                FERestrictionBase iObject = null;
                if (arg.Data.GetDataPresent(Properties.Resources.NeuronIDFormat))
                {
                    iItem = Brain.Current[(ulong)arg.Data.GetData(Properties.Resources.NeuronIDFormat)];
                }
                else
                {
                    return; // no data to drop.
                }

                if (iItem is BoolExpression)
                {
                    iObject = new FERestrictionBool(iItem);
                }
                else if (iItem is ExpressionsBlock)
                {
                    iObject = new FECustomRestriction(iItem);
                }
                else
                {
                    if (IsForObjectFrame == false)
                    {
                        FERestrictionSegment isegment;
                        iObject = EditorsHelper.MakeFERestriction(out isegment);
                        isegment.Restriction = iItem;
                        if (arg.Data.GetDataPresent(Properties.Resources.ThesaurusRelationshipFormat))
                        {
                            isegment.SearchDirection =
                                Brain.Current[(ulong)arg.Data.GetData(Properties.Resources.ThesaurusRelationshipFormat)];
                        }
                    }
                    else
                    {
                        iObject = EditorsHelper.MakeFERestriction();
                        ((FERestriction)iObject).Restriction = iItem;
                        if (arg.Data.GetDataPresent(Properties.Resources.ThesaurusRelationshipFormat))
                        {
                            ((FERestriction)iObject).SearchDirection =
                                Brain.Current[(ulong)arg.Data.GetData(Properties.Resources.ThesaurusRelationshipFormat)];
                        }
                    }
                }

                if (iObject != null)
                {
                    iItems.Add(iObject);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            System.Type iResultType = null;
            if (obj.GetDataPresent(Properties.Resources.DelayLoadResultType))
            {
                var iSource = obj.GetData(Properties.Resources.DelayLoadResultType) as DnD.DragSourceBase;
                System.Diagnostics.Debug.Assert(iSource != null);
                iResultType = EditorsHelper.GetCodeItemTypeFor(iSource.GetDelayLoadResultType(obj));
            }

            if (iResultType != null)
            {
                return iResultType != typeof(BoolExpression) && iResultType != typeof(ExpressionsBlock);
            }

            return Items != null
                   && (obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                       || obj.GetDataPresent(Properties.Resources.ThesaurusRelationshipFormat));
        }

        #endregion
    }
}