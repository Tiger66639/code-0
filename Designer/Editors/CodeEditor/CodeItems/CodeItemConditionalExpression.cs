// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemConditionalExpression.cs" company="">
//   
// </copyright>
// <summary>
//   The code item conditional expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item conditional expression.</summary>
    public class CodeItemConditionalExpression : CodeItemCodeBlock
    {
        #region fields

        /// <summary>The f condition.</summary>
        private CodeItemResult fCondition;

        #endregion

        #region ctor - dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemConditionalExpression"/> class. Initializes a new instance of the<see cref="CodeItemConditionalExpression"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemConditionalExpression(ConditionalExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #endregion

        #region Prop

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
                    if (Condition != null)
                    {
                        Condition.IsActive = value;
                    }
                }
            }
        }

        #region Condition

        /// <summary>
        ///     Gets/sets the <see cref="Condition" /> that needs to be evaluated to
        ///     <see langword="true" /> for executing all the statements.
        /// </summary>
        /// <remarks>
        ///     This is usually a <see cref="CodeItemBoolExpression" /> but could also
        ///     be a variable or static.
        /// </remarks>
        public CodeItemResult Condition
        {
            get
            {
                return fCondition;
            }

            set
            {
                if (fCondition != value)
                {
                    InternalSetCondition(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Condition, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>The internal set condition.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetCondition(CodeItemResult value)
        {
            if (fCondition != null)
            {
                UnRegisterChild(fCondition); // needs to be thread safe!!!!!.
            }

            fCondition = value;
            if (fCondition != null)
            {
                RegisterChild(fCondition);
            }

            OnPropertyChanged("Condition");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasCondition");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
        }

        #endregion

        #region NotHasCondition

        /// <summary>
        ///     Gets the if there is a condition or not (true when not).
        /// </summary>
        public bool NotHasCondition
        {
            get
            {
                return fCondition == null;
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
            CodeItem iItem = Condition;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as ConditionalExpression;
            if (iToWrap != null)
            {
                var iFound = iToWrap.Condition;
                if (iFound != null)
                {
                    InternalSetCondition((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            base.OutgoingLinkRemoved(link);
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    InternalSetCondition(null);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            base.OutgoingLinkCreated(link);
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    InternalSetCondition(
                        (CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID] as Expression));
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (Condition == child)
            {
                Condition = null;
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
            var iNew = new WPF.Controls.CPPHeaderedItemList(owner, panel);
            iNew.Element = new WPF.Controls.CtrlCondExpression();
            iNew.Data = this;
            iNew.List.ItemsSource = Children;
            iNew.List.Orientation = System.Windows.Controls.Orientation.Vertical;
            return iNew;
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            if (meaning == (ulong)PredefinedNeurons.Condition)
            {
                return Condition;
            }

            return null;
        }

        #endregion
    }
}