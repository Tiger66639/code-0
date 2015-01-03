// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemAssignment.cs" company="">
//   
// </copyright>
// <summary>
//   The code item assignment.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item assignment.</summary>
    public class CodeItemAssignment : CodeItem
    {
        #region ctor - dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemAssignment"/> class. Initializes a new instance of the <see cref="CodeItemAssignment"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemAssignment(Assignment toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #endregion

        #region fields

        /// <summary>The f left part.</summary>
        private CodeItemResult fLeftPart;

        /// <summary>The f right part.</summary>
        private CodeItemResult fRightPart;

        #endregion

        #region prop

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
                }
            }
        }

        #region NotHasLeftPart

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasLeftPart
        {
            get
            {
                return ((Assignment)Item).LeftPart == null;
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
                return ((Assignment)Item).RightPart == null;
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
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as Assignment;
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
            iNew.Element = new CtrlAssignment();
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

            return null;
        }

        #endregion
    }
}