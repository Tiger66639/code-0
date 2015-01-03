// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemByRef.cs" company="">
//   
// </copyright>
// <summary>
//   The code item by ref.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item by ref.</summary>
    public class CodeItemByRef : CodeItemResult
    {
        #region Fields

        /// <summary>The f argument.</summary>
        private CodeItem fArgument;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="CodeItemByRef"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemByRef(ByRefExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #region NotHasArgument

        /// <summary>
        ///     Gets if there is an ToSearch item.
        /// </summary>
        public bool NotHasArgument
        {
            get
            {
                return fArgument == null;
            }
        }

        #endregion

        #region Argument

        /// <summary>
        ///     Gets/sets the object who's list should be searched.
        /// </summary>
        public CodeItem Argument
        {
            get
            {
                return fArgument;
            }

            set
            {
                InternalSetArgument(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Argument, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set argument.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetArgument(CodeItem value)
        {
            if (fArgument != null)
            {
                UnRegisterChild(fArgument);
            }

            fArgument = value;
            if (fArgument != null)
            {
                RegisterChild(fArgument);
            }

            OnPropertyChanged("Argument");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasArgument");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region Functions

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Argument)
                {
                    InternalSetArgument(null);
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
                if (link.MeaningID == (ulong)PredefinedNeurons.Argument)
                {
                    InternalSetArgument(EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iToWrap = value as ByRefExpression;
            if (iToWrap != null)
            {
                var iFound = iToWrap.Argument;
                if (iFound != null)
                {
                    InternalSetArgument(EditorsHelper.CreateCodeItemFor(iFound));
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            if (Argument == child)
            {
                Argument = null;
            }
            else
            {
                base.RemoveChild(child);
            }
        }

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            var iItem = Argument;
            if (iItem != null)
            {
                iItem.Select(neuron);
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
            iNew.Element = new CtrlByRef();
            iNew.Data = this;
            return iNew;
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            if (meaning == (ulong)PredefinedNeurons.Argument)
            {
                return Argument;
            }

            return null;
        }

        #endregion
    }
}