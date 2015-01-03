// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPITextPatternEditorRoot.cs" company="">
//   
// </copyright>
// <summary>
//   A display path root item for text pattern editor results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     A display path root item for text pattern editor results.
    /// </summary>
    public class DPITextPatternEditorRoot : DPIRoot
    {
        /// <summary>Duplicates this instance.</summary>
        /// <returns>The <see cref="DPIRoot"/>.</returns>
        internal override DPIRoot Duplicate()
        {
            var iRes = new DPITextPatternEditorRoot();
            iRes.Item = Item;
            iRes.Items.AddRange(Items);
            return iRes;
        }

        /// <summary>
        ///     Selects the path result.
        /// </summary>
        internal override void SelectPathResult()
        {
            var iEditor = WindowMain.Current.ViewPatternEditor((NeuronCluster)Item);
            if (DoTextTag(iEditor) == false)
            {
                PatternEditorItem iOwner = null;
                IList iSubList = null;

                    // some operators will return a collection, like link out, this needs to be handled differently.
                var iChild = Items[0] as DPIChild;
                if (iChild == null)
                {
                    var iQuestion = Items[0] as DPITopicQuestionsSelect;
                    if (iQuestion != null)
                    {
                        iEditor.IsItemsSelected = false;
                        iSubList = iEditor.Questions;
                    }
                    else
                    {
                        var iTopicFilter = Items[0] as DPILinkOut;
                        if (iTopicFilter != null)
                        {
                            iEditor.IsTopicsFiltersSelected = true; // makecertain that the topic filters are visible.
                            iSubList = iEditor.TopicFilters;
                        }
                    }
                }
                else if (iChild.Index < iEditor.Items.Count)
                {
                    // if there is only 1 path for undo/redo -> index is out of range for redo. Solution-> use 2 paths: 1 taken before the action, 1 taken after the action.
                    iEditor.IsItemsSelected = true; // need to make certain that the tab is switched again.
                    iOwner = iEditor.CurrentList[iChild.Index] as PatternEditorItem;
                    iEditor.SelectedRuleIndex = iChild.Index; // in case it's a master-detail view.
                }

                if (Items.Count > 1)
                {
                    object iTemp;
                    int i;
                    for (i = 1; i < Items.Count && (iOwner != null || iSubList != null); i++)
                    {
                        if (iOwner != null)
                        {
                            var iOut = iOwner as OutputPattern;
                            if (iOut != null && (i < Items.Count - 1 || !(Items[Items.Count - 1] is DPITextRange)))
                            {
                                // need to expand the output if required, not required when last or just before a textrange.
                                iOut.IsExpanded = true;
                            }

                            iTemp = Items[i].GetFrom(iOwner);
                        }
                        else
                        {
                            iTemp = Items[i].GetFrom(iSubList);
                        }

                        if (iOwner is PatternRule && iTemp is DoPatternCollection)
                        {
                            // we are going from rule to 'to calculate' so make certain that the rule is expanded, so everything is visible.
                            ((PatternRule)iOwner).IsToCalculateVisible = true;
                        }

                        iOwner = iTemp as PatternEditorItem;
                        iSubList = iTemp as IList;
                    }

                    if (iOwner != null)
                    {
                        // if something went wrong, this is null, normally, it should be the last item.
                        SelectItem(iOwner); // make certain it is selected.
                    }
                }
                else
                {
                    SelectItem(iOwner);
                }
            }
        }

        /// <summary>The select item.</summary>
        /// <param name="item">The item.</param>
        private void SelectItem(PatternEditorItem item)
        {
            item.IsSelected = true;
            var iTextItem = item as TextPatternBase;
            if (iTextItem != null)
            {
                iTextItem.RefreshSelectionRange();
            }
        }

        /// <summary>The do text tag.</summary>
        /// <param name="iEditor">The i editor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool DoTextTag(TextPatternEditor iEditor)
        {
            var iItem = Items[0] as DPITextTag;
            if (iItem != null)
            {
                Debug.Assert(iItem != null);
                iEditor.PutFocusOn(iItem.Tag);
                return true;
            }

            return false;
        }

        #region unhanled functions

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item. We are root, so can't
        ///     get anything anymore.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors. We are root, so can't get anything
        ///     anymore.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(IList list)
        {
            throw new NotImplementedException();
        }

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner. Never called, we are a root, for
        ///     textpatterns, not code items.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}