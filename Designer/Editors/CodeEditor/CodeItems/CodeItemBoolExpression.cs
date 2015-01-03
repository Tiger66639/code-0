// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemBoolExpression.cs" company="">
//   
// </copyright>
// <summary>
//   The code item bool expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item bool expression.</summary>
    public class CodeItemBoolExpression : CodeItemResult
    {
        #region ctor - dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemBoolExpression"/> class. Initializes a new instance of the<see cref="CodeItemBoolExpression"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemBoolExpression(BoolExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #endregion

        #region fields

        /// <summary>The f left part.</summary>
        private CodeItemResult fLeftPart;

        /// <summary>The f right part.</summary>
        private CodeItemResult fRightPart;

        /// <summary>The f operator.</summary>
        private CodeItemResult fOperator;

        /// <summary>The f orientation.</summary>
        private System.Windows.Controls.Orientation fOrientation = System.Windows.Controls.Orientation.Horizontal;

        #endregion

        #region prop

        #region IsActive

        /// <summary>
        ///     Gets/sets wether this object monitors changes in the database or not.
        ///     When this is false, it uses less resources.
        /// </summary>
        /// <remarks>
        ///     This is <see langword="virtual" /> so that any changes to this property
        ///     can also be propegated to other objects.
        /// </remarks>
        /// <value>
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
                    if (LeftPart != null)
                    {
                        LeftPart.IsActive = value;
                    }

                    if (RightPart != null)
                    {
                        RightPart.IsActive = value;
                    }

                    if (Operator != null)
                    {
                        Operator.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #region Orientation

        /// <summary>
        ///     Gets/sets the orientation to use for displaying the
        ///     <see langword="bool" /> expression.
        /// </summary>
        /// <remarks>
        ///     This is usefull for longer expressions so we can show the right part
        ///     on a new line.
        /// </remarks>
        public System.Windows.Controls.Orientation Orientation
        {
            get
            {
                return fOrientation;
            }

            set
            {
                fOrientation = value;
                OnPropertyChanged("Orientation");
            }
        }

        #endregion

        #region NotHasLeftPart

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasLeftPart
        {
            get
            {
                return ((BoolExpression)Item).LeftPart == null;
            }
        }

        #endregion

        #region LeftPart

        /// <summary>
        ///     Gets/sets the left part of the boolean expression.
        /// </summary>
        public CodeItemResult LeftPart
        {
            get
            {
                return fLeftPart;
            }

            set
            {
                if (fLeftPart != value)
                {
                    InternalSetLeftPart(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LeftPart, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>only stores the new value, warns the undo system + the UI</summary>
        /// <param name="value">The new value.</param>
        private void InternalSetLeftPart(CodeItemResult value)
        {
            if (fLeftPart != null)
            {
                UnRegisterChild(fLeftPart);
            }

            fLeftPart = value;
            if (fLeftPart != null)
            {
                RegisterChild(fLeftPart);
            }

            OnPropertyChanged("LeftPart");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasLeftPart");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
            UpdateOrientation();
        }

        #endregion

        #region NotHasRightPart

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasRightPart
        {
            get
            {
                return ((BoolExpression)Item).RightPart == null;
            }
        }

        #endregion

        #region RightPart

        /// <summary>
        ///     Gets/sets the right part of the <see langword="bool" /> expression
        /// </summary>
        public CodeItemResult RightPart
        {
            get
            {
                return fRightPart;
            }

            set
            {
                if (fRightPart != value)
                {
                    InternalSetRightPart(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.RightPart, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>The internal set right part.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetRightPart(CodeItemResult value)
        {
            if (fRightPart != null)
            {
                UnRegisterChild(fRightPart);
            }

            fRightPart = value;
            if (fRightPart != null)
            {
                RegisterChild(fRightPart);
            }

            OnPropertyChanged("RightPart");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasRightPart");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
            UpdateOrientation();
        }

        #endregion

        #region Operator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> to use for the boolean
        ///     expression
        /// </summary>
        public CodeItemResult Operator
        {
            get
            {
                return fOperator;
            }

            set
            {
                if (fOperator != value)
                {
                    InternalSetOperator(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        /// <summary>The internal set operator.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetOperator(CodeItemResult value)
        {
            if (fOperator != null)
            {
                UnRegisterChild(fOperator);
            }

            fOperator = value;
            if (fOperator != null)
            {
                RegisterChild(fOperator);
            }

            OnPropertyChanged("Operator");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasOperator");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
        }

        #endregion

        #region NotHasOperator

        /// <summary>
        ///     Gets the if the <see langword="operator" /> is present or not.
        /// </summary>
        public bool NotHasOperator
        {
            get
            {
                return ((BoolExpression)Item).Operator == null;
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
            CodeItem iItem = RightPart;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = LeftPart;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = Operator;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }
        }

        /// <summary>
        ///     Automatically updates the orientation based on the type of code items
        ///     found in left and right.
        /// </summary>
        private void UpdateOrientation()
        {
            if (LeftPart is CodeItemBoolExpression || RightPart is CodeItemBoolExpression)
            {
                Orientation = System.Windows.Controls.Orientation.Vertical;
            }
            else
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal;
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as BoolExpression;
            if (iToWrap != null)
            {
                var iFound = iToWrap.LeftPart;
                if (iFound != null)
                {
                    InternalSetLeftPart((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.RightPart;
                if (iFound != null)
                {
                    InternalSetRightPart((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iToWrap.Operator;
                if (iFound != null)
                {
                    InternalSetOperator((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
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
                if (link.MeaningID == (ulong)PredefinedNeurons.LeftPart)
                {
                    InternalSetLeftPart(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.RightPart)
                {
                    InternalSetRightPart(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Operator)
                {
                    InternalSetOperator(null);
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
                if (link.MeaningID == (ulong)PredefinedNeurons.LeftPart)
                {
                    InternalSetLeftPart((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.RightPart)
                {
                    InternalSetRightPart((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Operator)
                {
                    InternalSetOperator((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (LeftPart == child)
            {
                LeftPart = null;
            }
            else if (RightPart == child)
            {
                RightPart = null;
            }
            else if (Operator == child)
            {
                Operator = null;
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
            iNew.Element = new CtrlBoolExpression();
            iNew.Data = this;
            return iNew;
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            if (meaning == (ulong)PredefinedNeurons.LeftPart)
            {
                return LeftPart;
            }

            if (meaning == (ulong)PredefinedNeurons.RightPart)
            {
                return RightPart;
            }

            if (meaning == (ulong)PredefinedNeurons.Operator)
            {
                return Operator;
            }

            return null;
        }

        #endregion
    }
}