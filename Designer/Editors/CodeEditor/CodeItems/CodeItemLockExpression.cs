// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemLockExpression.cs" company="">
//   
// </copyright>
// <summary>
//   A codeItem wrapper for a <see cref="LockExpression" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A codeItem wrapper for a <see cref="LockExpression" /> .
    /// </summary>
    public class CodeItemLockExpression : CodeItemCodeBlock
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeItemLockExpression"/> class. Initializes a new instance of the<see cref="CodeItemLockExpression"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public CodeItemLockExpression(LockExpression toWrap, bool isActive)
            : base(toWrap, isActive)
        {
            NeuronsToLock = new CodeItemToLockList(toWrap, isActive, true);
            LinksToLock = new CodeItemToLockList(toWrap, isActive, false);
            RegisterChild(NeuronsToLock);
            RegisterChild(LinksToLock);
        }

        #endregion

        #region fields

        /// <summary>The f neurons to lock.</summary>
        private CodeItemToLockList fNeuronsToLock;

        /// <summary>The f links to lock.</summary>
        private CodeItemToLockList fLinksToLock;

        #endregion

        #region Prop

        #region NeuronsToLock

        /// <summary>
        ///     Gets the list of statements for this conditional expression.
        /// </summary>
        public CodeItemToLockList NeuronsToLock
        {
            get
            {
                return fNeuronsToLock;
            }

            private set
            {
                if (fNeuronsToLock != value)
                {
                    fNeuronsToLock = value;
                    OnPropertyChanged("NeuronsToLock");
                }
            }
        }

        #endregion

        #region NeuronsToLock

        /// <summary>
        ///     Gets the list of statements for this conditional expression.
        /// </summary>
        public CodeItemToLockList LinksToLock
        {
            get
            {
                return fLinksToLock;
            }

            private set
            {
                if (fLinksToLock != value)
                {
                    fLinksToLock = value;
                    OnPropertyChanged("LinksToLock");
                }
            }
        }

        #endregion

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
                var iPrev = IsActive;
                base.IsActive = value;
                if (iPrev != IsActive)
                {
                    if (NeuronsToLock != null)
                    {
                        NeuronsToLock.IsActive = value;
                    }

                    if (LinksToLock != null)
                    {
                        LinksToLock.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iChild = (CodeItem)child;
            if (Children.Remove(iChild) == false)
            {
                if (NeuronsToLock.Items.Remove(iChild) == false)
                {
                    if (LinksToLock.Items.Remove(iChild) == false)
                    {
                        base.RemoveChild(iChild);
                    }
                }
            }
        }

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            IsExpanded = true; // always need to expand, otherwise the children aren't loaded.
            foreach (var i in Children)
            {
                i.Select(neuron);
            }

            NeuronsToLock.Select(neuron);
            LinksToLock.Select(neuron);
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
            iNew.Element = new WPF.Controls.CtrlCodeItemLockExp();
            iNew.Data = this;
            iNew.List.Orientation = System.Windows.Controls.Orientation.Vertical;
            iNew.List.ItemsSource = Children;
            return iNew;
        }

        #endregion
    }
}