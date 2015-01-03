// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPIChatbotPropsRoot.cs" company="">
//   
// </copyright>
// <summary>
//   The chatbot props page contains some textpattern editor items, so they
//   need to be selectedalbe with a Display path. This class provides a root
//   for the prop page.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     The chatbot props page contains some textpattern editor items, so they
    ///     need to be selectedalbe with a Display path. This class provides a root
    ///     for the prop page.
    /// </summary>
    public class DPIChatbotPropsRoot : DPIRoot
    {
        /// <summary>The f selected ui.</summary>
        private readonly ChatbotProperties.SelectedUI fSelectedUI;

        /// <summary>Initializes a new instance of the <see cref="DPIChatbotPropsRoot"/> class.</summary>
        /// <param name="selectedUI">The selected ui.</param>
        public DPIChatbotPropsRoot(ChatbotProperties.SelectedUI selectedUI)
        {
            fSelectedUI = selectedUI;
        }

        /// <summary>Duplicates this instance.</summary>
        /// <returns>The <see cref="DPIRoot"/>.</returns>
        internal override DPIRoot Duplicate()
        {
            var iRes = new DPIChatbotPropsRoot(fSelectedUI);
            iRes.Item = Item;
            iRes.Items.AddRange(Items);
            return iRes;
        }

        /// <summary>
        ///     Selects the path result.
        /// </summary>
        internal override void SelectPathResult()
        {
            WindowMain.Current.ShowChatbotProps();
            BrainData.Current.ChatbotProps.SetSectedTab(fSelectedUI);
            var iTag = Items[0] as DPITextTag;
            if (iTag != null)
            {
                DoTagFocus(iTag);
            }
            else
            {
                var iRange = Items[0] as DPITextRange; // the do patterns only need a range.
                if (iRange != null)
                {
                    DoRangeFocus(iRange);
                }
                else
                {
                    DoChildFocus();
                }
            }
        }

        /// <summary>The do child focus.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void DoChildFocus()
        {
            PatternEditorItem iOwner;
            System.Diagnostics.Debug.Assert(Items.Count > 0);
            var iChild = Items[0] as DPIChild;
            System.Diagnostics.Debug.Assert(iChild != null);

            if (fSelectedUI == ChatbotProperties.SelectedUI.IsOpeningStatSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.ConversationStarts[iChild.Index];
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsFallbackSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.FallBacks[iChild.Index];
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsDoAfterSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.DoAfterStatement[iChild.Index];
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsRepetetitionSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.ResponsesOnRepeat[iChild.Index];
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsDoOnStartupSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.DoOnStartup[iChild.Index];
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsContextSelected)
            {
                iOwner = BrainData.Current.ChatbotProps.Context[iChild.Index];
            }
            else
            {
                throw new System.InvalidOperationException("index out of range");
            }

            System.Collections.IList iSubList = null;

                // some operators will return a collection, like link out, this needs to be handled differently.
            if (Items.Count > 1)
            {
                object iTemp;
                for (var i = 1; i < Items.Count && (iOwner != null || iSubList != null); i++)
                {
                    if (iOwner != null)
                    {
                        var iOut = iOwner as OutputPattern;
                        if (iOut != null)
                        {
                            // need to expand the output if required
                            iOut.IsExpanded = true;
                        }

                        iTemp = Items[i].GetFrom(iOwner);
                    }
                    else
                    {
                        iTemp = Items[i].GetFrom(iSubList);
                    }

                    if (iTemp is DoPatternCollection && iOwner is PatternRuleOutput)
                    {
                        // if we are going to a do pattern, make the do section visible.
                        ((PatternRuleOutput)iOwner).IsDoPatternVisible = true;
                    }

                    iOwner = iTemp as PatternEditorItem;
                    iSubList = iTemp as System.Collections.IList;
                }

                if (iOwner != null)
                {
                    // if something went wrong, this is null, normally, it should be the last item.
                    iOwner.IsSelected = true; // make certain it is selected.
                }
            }
            else
            {
                iOwner.IsSelected = true;
            }
        }

        /// <summary>The do tag focus.</summary>
        /// <param name="iTag">The i tag.</param>
        private void DoTagFocus(DPITextTag iTag)
        {
            BrainData.Current.ChatbotProps.FocusOn(iTag.Tag);
            if (Items.Count > 1)
            {
                var iRange = Items[1] as DPITextRange;
                System.Diagnostics.Debug.Assert(iRange != null);
                var iText = System.Windows.Input.Keyboard.FocusedElement as System.Windows.Controls.TextBox;
                if (iText != null)
                {
                    iText.SelectionStart = iRange.Start;
                    iText.SelectionLength = iRange.Length;
                }
            }
        }

        /// <summary>The do range focus.</summary>
        /// <param name="iRange">The i range.</param>
        private void DoRangeFocus(DPITextRange iRange)
        {
            if (fSelectedUI == ChatbotProperties.SelectedUI.IsDoAfterSelected
                && BrainData.Current.ChatbotProps.DoAfter != null)
            {
                iRange.GetFrom(BrainData.Current.ChatbotProps.DoAfter);
            }
            else if (fSelectedUI == ChatbotProperties.SelectedUI.IsDoOnStartupSelected
                     && BrainData.Current.ChatbotProps.DoStartup != null)
            {
                iRange.GetFrom(BrainData.Current.ChatbotProps.DoStartup);
            }
        }

        #region unhanled functions

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item. We are root, so can't
        ///     get anything anymore.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors. We are root, so can't get anything
        ///     anymore.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner. Never called, we are a root, for
        ///     textpatterns, not code items.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}