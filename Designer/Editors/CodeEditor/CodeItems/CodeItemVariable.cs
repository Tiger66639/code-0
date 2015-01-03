// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemVariable.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for the <see cref="Variable" /> neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for the <see cref="Variable" /> neuron.
    /// </summary>
    public class CodeItemVariable : CodeItemResult
    {
        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemVariable"/> class. Initializes a new instance of the <see cref="CodeItemVariable"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public CodeItemVariable(Variable toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        ///// <summary>
        ///// Tries to register this variable with the root CodeEditorPage, if there is any.
        ///// </summary>
        // private void TryRegisterVar()
        // {
        // CodeEditorPage iRoot = Root as CodeEditorPage;
        // if (iRoot != null && iRoot.RegisteredVariables.Contains((Variable)Item) == false && !(Item is SystemVariable))               //we don't want a system var cause this is already placed on the toolbox.
        // iRoot.RegisteredVariables.Add((Variable)Item);
        // }

        ///// <summary>
        ///// Releases unmanaged resources and performs other cleanup operations before the
        ///// <see cref="CodeItemVariable"/> is reclaimed by garbage collection.
        ///// </summary>
        // ~CodeItemVariable()
        // {
        // if (Environment.HasShutdownStarted == false)                                     //don't try to remove the item if we are trying to quit the app, this causes a problem.
        // {
        // CodeEditorPage iRoot = Root as CodeEditorPage;
        // if (iRoot != null)
        // iRoot.RegisteredVariables.Remove((Variable)Item);
        // }
        // } 
        #endregion

        #region Fields

        /// <summary>The f value.</summary>
        private CodeItemResult fValue;

        /// <summary>The f split reaction.</summary>
        private CodeItemResult fSplitReaction;

        #endregion

        #region Prop

        #region IsActive

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c> .
        /// </value>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }

            set
            {
                if (value != base.IsActive)
                {
                    base.IsActive = value;
                    if (Value != null)
                    {
                        Value.IsActive = value;
                    }

                    if (SplitReaction != null)
                    {
                        SplitReaction.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #region SplitReaction

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> to use for the boolean
        ///     expression
        /// </summary>
        public CodeItemResult SplitReaction
        {
            get
            {
                return fSplitReaction;
            }

            set
            {
                if (fSplitReaction != value)
                {
                    InternalSetSplitReaction(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.SplitReaction, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>The internal set split reaction.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetSplitReaction(CodeItemResult value)
        {
            if (fSplitReaction != null)
            {
                UnRegisterChild(fSplitReaction);
            }

            fSplitReaction = value;
            if (fSplitReaction != null)
            {
                RegisterChild(fSplitReaction);
            }

            OnPropertyChanged("SplitReaction");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasSplitReaction");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
        }

        #endregion

        #region NotHasSplitReaction

        /// <summary>
        ///     Gets the if the <see langword="operator" /> is present or not.
        /// </summary>
        public bool NotHasNotHasSplitReaction
        {
            get
            {
                return ((Variable)Item).SplitReaction == null;
            }
        }

        #endregion

        #region Value

        /// <summary>
        ///     Gets/sets the object who's list should be searched.
        /// </summary>
        public CodeItemResult Value
        {
            get
            {
                return fValue;
            }

            set
            {
                InternalSetValue(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set value.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetValue(CodeItemResult value)
        {
            if (fValue != null)
            {
                UnRegisterChild(fValue);
            }

            fValue = value;
            if (fValue != null)
            {
                RegisterChild(fValue);
            }

            OnPropertyChanged("Value");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasValue");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region NotHasValue

        /// <summary>
        ///     Gets if there is an ToSearch item.
        /// </summary>
        public bool NotHasValue
        {
            get
            {
                return fValue == null;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            CodeItem iItem = Value;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = SplitReaction;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Value)
                {
                    InternalSetValue(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.SplitReaction)
                {
                    InternalSetSplitReaction(null);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Value)
                {
                    InternalSetValue((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.SplitReaction)
                {
                    InternalSetSplitReaction((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as Variable;
            if (iToWrap != null)
            {
                var iFound = iToWrap.Value;
                if (iFound != null)
                {
                    InternalSetValue((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.SplitReaction;
                if (iFound != null)
                {
                    InternalSetSplitReaction((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (Value == child)
            {
                Value = null;
            }
            else if (SplitReaction == child)
            {
                SplitReaction = null;
            }
            else
            {
                base.RemoveChild(child);
            }
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.CodePagePanel"/> object.</summary>
        /// <param name="owner"></param>
        /// <param name="panel"></param>
        /// <returns>The <see cref="CPPItemBase"/>.</returns>
        protected internal override WPF.Controls.CPPItemBase CreateDefaultUI(
            WPF.Controls.CPPItemBase owner, 
            WPF.Controls.CodePagePanel panel)
        {
            var iNew = new WPF.Controls.CPPItem(owner, panel);
            iNew.Element = new CtrlVariable();
            iNew.Data = this;
            return iNew;
        }

        #endregion
    }
}