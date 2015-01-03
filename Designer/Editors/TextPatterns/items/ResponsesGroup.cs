// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponsesGroup.cs" company="">
//   
// </copyright>
// <summary>
//   contains a list of conditionals and a reference to an output statetement
//   for which the contitionals are valid possible outputs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     contains a list of conditionals and a reference to an output statetement
    ///     for which the contitionals are valid possible outputs.
    /// </summary>
    public class ResponsesForGroup : PatternEditorItem, IResponseForOutputOwner, IConditionalOutputsCollectionOwner
    {
        /// <summary>The f conditionals.</summary>
        private ConditionalOutputsCollection fConditionals;

        /// <summary>The f response for.</summary>
        private ResponsesForCollection fResponseFor;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ResponsesForGroup"/> class. Initializes a new instance of the <see cref="ResponsesForGroup"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ResponsesForGroup(NeuronCluster toWrap)
            : base(toWrap)
        {
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance has any children or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is empty; otherwise, <c>false</c> .
        /// </value>
        public bool IsEmpty
        {
            get
            {
                if (fConditionals != null)
                {
                    foreach (var iCond in fConditionals)
                    {
                        if (iCond.IsEmpty == false)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets the rule to which this object belongs.
        /// </summary>
        public override PatternRule Rule
        {
            get
            {
                return Owner as PatternRule;
            }
        }

        #region Conditionals

        /// <summary>
        ///     Gets the list of conditional output sets.
        /// </summary>
        public ConditionalOutputsCollection Conditionals
        {
            get
            {
                return fConditionals;
            }

            internal set
            {
                fConditionals = value;
                OnPropertyChanged("Conditionals");
            }
        }

        #endregion

        #region ResponseFor

        /// <summary>
        ///     Gets/sets the list of outputs for which this can be a response.
        /// </summary>
        public ResponsesForCollection ResponseFor
        {
            get
            {
                return fResponseFor;
            }

            set
            {
                fResponseFor = value;
                OnPropertyChanged("ResponseFor");
            }
        }

        #endregion

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            if (value == null || Neuron.IsEmpty(value.ID))
            {
                Conditionals = null;
                ResponseFor = null;
            }
            else
            {
                Conditionals = new ConditionalOutputsCollection(this, (NeuronCluster)value);
                var iCl = value.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
                SetResponseFor(iCl);
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.ResponseForOutputs)
                {
                    SetResponseFor(link.To as NeuronCluster);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.ResponseForOutputs)
                {
                    SetResponseFor(null);
                }
            }
        }

        /// <summary>tores the new 'response for' + raises the event.</summary>
        /// <param name="value">The value.</param>
        private void SetResponseFor(NeuronCluster value)
        {
            if (value != null)
            {
                fResponseFor = new ResponsesForCollection(this, value);
            }
            else
            {
                fResponseFor = new ResponsesForCollection(this, (ulong)PredefinedNeurons.ResponseForOutputs);
            }

            OnPropertyChanged("ResponseFor");
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iChild = child as PatternRuleOutput;
            if (iChild != null && fConditionals != null)
            {
                if (fConditionals.Remove(iChild))
                {
                    return;
                }
            }
            else if (fResponseFor != null)
            {
                var iResponse = child as ResponseForOutput;
                if (iChild != null && fResponseFor.Remove(iResponse))
                {
                    return;
                }
            }

            base.RemoveChild(child);
        }

        /// <summary>Gets the display path that points to the current object. This is for
        ///     the <see cref="ResponseFor"/> value.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public override Search.DisplayPath GetDisplayPathFromThis()
        {
            Search.DPIRoot iRoot = null;
            FillDisplayPathForThis(ref iRoot);
            if (iRoot != null)
            {
                var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.ResponseForOutputs);

                    // the link to the output statement.
                iRoot.Items.Add(iOut);
                return new Search.DisplayPath(iRoot);
            }

            return null;
        }

        /// <summary>Fills the display path for this.</summary>
        /// <param name="list">The list.</param>
        internal override void FillDisplayPathForThis(ref Search.DPIRoot list)
        {
            var iOwner = Owner as PatternRule;
            System.Diagnostics.Debug.Assert(iOwner != null);
            iOwner.FillDisplayPathForThis(ref list); // this will create the list as well, when it finds the first item.

            var iOut = new Search.DPILinkOut((ulong)PredefinedNeurons.ResponseForOutputs);

                // get the list to the responseFor conditionals
            list.Items.Add(iOut);

            var iChild = new Search.DPIChild(iOwner.ResponsesFor.IndexOf(this));
            list.Items.Add(iChild);
        }

        /// <summary>
        ///     Called by the editor when the user has requested to remove the
        ///     selected items from the list. The PatternDef should remove from the
        ///     TextPatternEditor, other items can aks their owner to remove the
        ///     child.
        /// </summary>
        internal override void RemoveFromOwner()
        {
            ((EditorItem)Owner).RemoveChild(this);
        }

        /// <summary>
        ///     Deletes this instance and all of it's children.
        /// </summary>
        internal override void Delete()
        {
            EditorsHelper.DeleteResponsesGroup((NeuronCluster)Item);
        }

        /// <summary>Rebuilds all the items.</summary>
        /// <param name="errors">The errors.</param>
        internal void Rebuild(System.Collections.Generic.List<string> errors)
        {
            if (Conditionals != null)
            {
                foreach (var i in Conditionals)
                {
                    i.Rebuild(errors);
                }
            }
        }

        /// <summary>
        ///     releases all the output patterns
        /// </summary>
        internal void ReleaseAll()
        {
            if (Conditionals != null)
            {
                foreach (var i in Conditionals)
                {
                    i.ReleaseOutputs();
                }
            }
        }
    }
}