// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPICodeEditorRoot.cs" company="">
//   
// </copyright>
// <summary>
//   A display root item that starts with a code editor and contains logic to
//   do the selection for a code item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A display root item that starts with a code editor and contains logic to
    ///     do the selection for a code item.
    /// </summary>
    public class DPICodeEditorRoot : DPIRoot
    {
        #region SecondaryItem

        /// <summary>
        ///     Gets the cluster that actually contained the code that should be used
        ///     as starting point. This is provided in case that the Item was deleted,
        ///     but the the code list was not (usually this can happen for the
        ///     'Actions' link. This will only be used to start the display, in case
        ///     the Item no longer exits.
        /// </summary>
        public Neuron SecondaryItem { get; internal set; }

        #endregion

        #region CalledFrom

        /// <summary>
        ///     Gets the path to the location that triggered this new root object.
        ///     This happens when a 'call' instruction is performed.
        /// </summary>
        public DPICodeEditorRoot CalledFrom { get; internal set; }

        #endregion

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The select path result.</summary>
        internal override void SelectPathResult()
        {
            CodeEditorPage iPage = null;
            var iFirst = 0;
            var iEditor = SelectEditorAndPage(ref iPage, ref iFirst);
            if (iPage != null && Items.Count > iFirst)
            {
                // if we found the page and there is something to select, select the item.
                ICodeItemsOwner iOwner = iPage;
                CodeItem iResult = null;
                for (var i = iFirst; i < Items.Count && iOwner != null; i++)
                {
                    var iNext = Items[i].GetFrom(iOwner);

                        // conditional statements have 2 lists, to make it work for the second list, it has a custom owner, which is not a code item, but a wrapper for the actual owner. It does implement ICodeItemsOwner, so it can be used.
                    iResult = iNext as CodeItem;
                    iOwner = iNext as ICodeItemsOwner;
                    var iBlock = iOwner as ExpandableCodeItem;
                    if (iBlock != null)
                    {
                        iBlock.IsExpanded = true; // need to make certain that the exact path is visualized.
                        if ((i < Items.Count - 1) && (NextIsGetSubList(i, iOwner) || NextIsConditional(i, iOwner)))
                        {
                            // depends on how the list is build: sometimes there is the link-out also include, but when built from a processor stack, this info got lost.
                            i++;

                                // for an expressionblock, we increment 1 more, when displaying in a code editor, cause the expressionBlock needs to select the cluster that contains the code (outlink), this is done automatically for codeEditorItem objects.
                        }
                    }
                    else if (iOwner is CodeItemStatement || iOwner is CodeItemResultStatement)
                    {
                        // same as for block: a statement has arguments, which are stored in a cluster, that the codeItemStatement hides, so advance 1.
                        if (NextIsGetSubList(i, iOwner))
                        {
                            // depends on how the list is build: sometimes there is the link-out also include, but when built from a processor stack, this info got lost.
                            i++;
                        }
                    }
                }

                if (iResult != null)
                {
                    iResult.IsSelected = true;
                }
            }
        }

        /// <summary>Checks if the <paramref name="owner"/> is a conditional and the next
        ///     item selects the 'conditional cluster' (which contains all the
        ///     conditions). This allows us to advance by 1 cause the link isn't
        ///     represented in the code UI.</summary>
        /// <param name="i">The i.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool NextIsConditional(int i, ICodeItemsOwner owner)
        {
            var iOwner = owner as CodeItemConditionalStatement;
            if (iOwner != null)
            {
                var iOut = Items[i + 1] as DPILinkOut;
                if (iOut != null)
                {
                    return iOut.Meaning == (ulong)PredefinedNeurons.Condition;
                }

                var iIndexOut = Items[i + 1] as DPILinkIndexOut;
                if (iIndexOut != null)
                {
                    throw new System.NotImplementedException();
                }
            }

            return false;
        }

        /// <summary>Checks if the item at the next index as specified references the
        ///     'arguments' list of a statement or the 'statements' list of a
        ///     conditional part, or the 'parts' of a conditional.</summary>
        /// <param name="i">The i.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool NextIsGetSubList(int i, ICodeItemsOwner owner)
        {
            if (i < Items.Count - 1)
            {
                var iOut = Items[i + 1] as DPILinkOut;
                if (iOut != null)
                {
                    return iOut.Meaning == (ulong)PredefinedNeurons.Arguments
                           || (!(owner is CodeItemConditionalStatement)
                               && iOut.Meaning == (ulong)PredefinedNeurons.Statements);

                        // a conditional statemnet can also be followed with a 'statements' linkOut, but this has to be handled like normal.
                }

                var iIndexOut = Items[i + 1] as DPILinkIndexOut;
                if (iIndexOut != null)
                {
                    throw new System.NotImplementedException();
                }
            }

            return false;
        }

        /// <summary>The select editor and page.</summary>
        /// <param name="iPage">The i page.</param>
        /// <param name="first">The first.</param>
        /// <returns>The <see cref="CodeEditor"/>.</returns>
        private CodeEditor SelectEditorAndPage(ref CodeEditorPage iPage, ref int first)
        {
            if (WindowMain.Current != null)
            {
                // can be null when closing the app.
                CodeEditor iEditor = null;
                if (Item != null && Item.IsDeleted == false)
                {
                    iEditor = WindowMain.Current.ViewCodeForNeuron(Item); // show the code editor for the root neuron.
                    if (Items.Count > 0)
                    {
                        var iLinkOut = Items[0] as DPILinkOut; // select the correct page on the editor.
                        if (iLinkOut != null)
                        {
                            for (var i = 0; i < iEditor.EntryPoints.Count; i++)
                            {
                                iPage = iEditor.EntryPoints[i];
                                if (iPage.LinkMeaning == iLinkOut.Meaning)
                                {
                                    iEditor.SelectedIndex = i;
                                    first = 1;
                                    return iEditor;
                                }
                            }
                        }
                        else
                        {
                            iPage = iEditor.EntryPoints[0];

                                // could be that first item simply  references an item in a list.
                        }
                    }
                }
                else if (SecondaryItem != null)
                {
                    iEditor = WindowMain.Current.ViewCodeForNeuron(SecondaryItem);

                        // show the code editor for the root neuron.
                    if (iEditor.EntryPoints.Count > 0)
                    {
                        // take the first, this should be the statements.
                        iPage = iEditor.EntryPoints[0];
                    }

                    var iLinkOut = Items[0] as DPILinkOut;

                        // if the first item is a link out, skip it, cause we couldn't find the correct start. Any first item that is a link is to 'rules' or 'actions', otherwise we need a child ref.
                    if (iLinkOut != null)
                    {
                        first = 1;
                    }
                }

                return iEditor;
            }

            return null;
        }

        /// <summary>Builds from.</summary>
        /// <param name="source">The source.</param>
        /// <returns>This object or the new root to use (in case there were calls to
        ///     clusters done, which generate new root objects, to display the new
        ///     code cluster.</returns>
        internal DPIRoot BuildFrom(DebugProcessor source)
        {
            var iRes = this;
            ulong iPageMeaning;
            if (source.CurrentMeaning != null)
            {
                Item = source.CurrentMeaning;
                iPageMeaning = (ulong)PredefinedNeurons.Rules;
            }
            else if (source.NeuronToSolve != null)
            {
                Item = source.NeuronToSolve;
                iPageMeaning = (ulong)PredefinedNeurons.Actions;
            }
            else
            {
                // we have a direct 'call'. The only way to figure out the start, is by using the first frame's cluster as the source.
                if (source.ExecutionFramesStack.Count > 0)
                {
                    Item = Enumerable.First(Enumerable.Reverse(source.ExecutionFramesStack)).Frame.ExecSource;
                }

                iPageMeaning = Neuron.EmptyId;
            }

            if (iPageMeaning != Neuron.EmptyId)
            {
                SecondaryItem = source.CodeCluster;
                var iPageOut = new DPILinkOut(iPageMeaning);
                Items.Add(iPageOut);
            }

            var iTotal = source.ExecutionFramesStack.Count; // we need to know if it is the last item or not, 
            foreach (var i in Enumerable.Reverse(source.ExecutionFramesStack))
            {
                // this is a stack, so can't use for(;;)
                iTotal--;
                if (i.Frame is ConditionalFrame)
                {
                    BuildForConditionalFrame(i, iRes, iTotal == 0);

                        // the last frameSTack item can have a ref to a condition, otherwise, it's always a child, so if there are still frame items after the current, we can't ref a conditional.
                }
                else if (i.Frame is ExpressionBlockFrame)
                {
                    BuildForExpressionBlock(i, iRes);
                }
                else if (i.Frame is CallInstCallFrame)
                {
                    var iNew = new DPICodeEditorRoot();
                    iNew.SecondaryItem = i.Frame.ExecSource;
                    iNew.CalledFrom = iRes;
                    iRes = iNew;
                    var iChild = new DPIChild(i.CurrentIndex);
                    iRes.Items.Add(iChild);
                }
                else
                {
                    var iChild = new DPIChild(i.CurrentIndex);
                    iRes.Items.Add(iChild);
                }
            }

            return iRes;
        }

        /// <summary>The build for expression block.</summary>
        /// <param name="i">The i.</param>
        /// <param name="res">The res.</param>
        private void BuildForExpressionBlock(ExecutionFrame i, DPICodeEditorRoot res)
        {
            var iExpCode = new DPILinkOut((ulong)PredefinedNeurons.Statements);
            res.Items.Add(iExpCode);
            var iChild = new DPIChild(i.CurrentIndex);
            res.Items.Add(iChild);
        }

        /// <summary>The build for conditional frame.</summary>
        /// <param name="i">The i.</param>
        /// <param name="res">The res.</param>
        /// <param name="refToCondition">The ref to condition.</param>
        private void BuildForConditionalFrame(ExecutionFrame i, DPICodeEditorRoot res, bool refToCondition)
        {
            int iIndex;
            var iCondFrame = i.Frame as ConditionsFrame;
            if (iCondFrame != null)
            {
                if (i.NextIndex <= -1)
                {
                    iIndex = 0;
                }
                else if (i.NextIndex < iCondFrame.Conditions.Count)
                {
                    iIndex = i.NextIndex;
                }
                else
                {
                    iIndex = iCondFrame.Conditions.Count - 1;
                }
            }
            else if (i.Frame is ForEachCallFrame)
            {
                // a foreach index must also have it's first condition selected, since this is skipped by this type of frame.
                var iPart = new DPIChild(0);
                res.Items.Add(iPart);
                var iPartCode = new DPILinkOut((ulong)PredefinedNeurons.Statements);
                res.Items.Add(iPartCode);
                var iCodeChild = new DPIChild(i.CurrentIndex);
                res.Items.Add(iCodeChild);
                return; // a foreach does it all self, cause no link out at end needed.
            }
            else
            {
                var iFrame = (ConditionalFrame)i.Frame;
                if (i.NextIndex <= -1)
                {
                    iIndex = 0;
                }
                else if (i.NextIndex < iFrame.Code.Count)
                {
                    iIndex = i.NextIndex;
                }
                else
                {
                    iIndex = iFrame.Code.Count - 1;
                }
            }

            var iChild = new DPIChild(iIndex);
            res.Items.Add(iChild);
            if (refToCondition)
            {
                // if this is the last frame item, we are evaluating the conditions, so create a link for them.
                var iExpCode = new DPILinkOut((ulong)PredefinedNeurons.Condition);
                res.Items.Add(iExpCode);
            }
        }

        /// <summary>Duplicates this instance.</summary>
        /// <returns>The <see cref="DPIRoot"/>.</returns>
        internal override DPIRoot Duplicate()
        {
            var iRes = new DPICodeEditorRoot();
            iRes.Items.AddRange(Items);
            iRes.CalledFrom = CalledFrom;
            iRes.Item = Item;
            iRes.SecondaryItem = SecondaryItem;

            return iRes;
        }

        /// <summary>invalid for here, we are a root.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>invalid for here, we are a root.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }
    }
}