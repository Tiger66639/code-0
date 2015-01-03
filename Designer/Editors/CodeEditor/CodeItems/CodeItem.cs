// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItem.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a code item in a <see cref="CodeList" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Represents a code item in a <see cref="CodeList" /> .
    /// </summary>
    public class CodeItem : EditorItem, INeuronRemoveable<CodeItem>
    {
        // need to inherit from INeuronRemovable from here, cause 'CodeItem' is what the deleter needs in the U param.
        #region INeuronRemoveable<CodeItem> Members

        /// <summary>Removes the child.</summary>
        /// <param name="child">The child.</param>
        public void RemoveChild(CodeItem child)
        {
            RemoveChild((EditorItem)child); // simply ask the base to do it.
        }

        #endregion

        #region fields

        /// <summary>The f is break point.</summary>
        private bool fIsBreakPoint;

        /// <summary>The f is next statement.</summary>
        private bool fIsNextStatement;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeItem"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItem(Neuron toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeItem"/> class.</summary>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public CodeItem(bool isActive)
            : base(isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CodeItem"/> class. 
        ///     For <see langword="static"/> code item</summary>
        public CodeItem()
        {
        }

        #endregion

        #region prop

        #region IsBreakPoint

        /// <summary>
        ///     Gets/sets if the processor should pause on this line.
        /// </summary>
        public bool IsBreakPoint
        {
            get
            {
                return fIsBreakPoint;
            }

            set
            {
                var iItem = Item as Expression;
                if (iItem != null)
                {
                    // we could wrap a static neuron.
                    OnPropertyChanging("IsBreakPoint", fIsBreakPoint, value);
                    fIsBreakPoint = value;
                    if (value)
                    {
                        BrainData.Current.BreakPoints.Add(iItem);
                    }
                    else
                    {
                        BrainData.Current.BreakPoints.Remove(iItem);
                    }

                    OnPropertyChanged("IsBreakPoint");
                }
            }
        }

        #endregion

        #region IsNextStatement

        /// <summary>
        ///     Gets/sets wether this is the next statement to be executed or not.
        /// </summary>
        public bool IsNextStatement
        {
            get
            {
                return fIsNextStatement;
            }

            internal set
            {
                fIsNextStatement = value;
                OnPropertyChanged("IsNextStatement");

                    // this should be thread save cause property changed works accross threads.
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            if (value is Expression)
            {
                if (BrainData.Current.BreakPoints.Contains((Expression)value))
                {
                    IsBreakPoint = true;
                }
            }

            base.OnItemChanged(value);
        }

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public virtual void Select(Neuron neuron)
        {
            if (Item == neuron)
            {
                var iRoot = Root;
                if (iRoot != null)
                {
                    if (iRoot.SelectedItems.Count < 3)
                    {
                        iRoot.SelectedItems.Add(this);

                            // we use this type of adding to make certain we can add multiple items.
                        Visualize();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "Only the first 3 items have been visualized to prevent stack overflow, more are present in this page through.");
                    }
                }
            }
        }

        /// <summary>
        ///     Makes certain that this item is visually displayed, so non of it's
        ///     owner's are collapsed (through <see cref="ExpandableCodeItem" /> s.
        /// </summary>
        public void Visualize()
        {
            var iOwner = Owner as CodeItem;
            while (iOwner != null)
            {
                var iExpandable = iOwner as ExpandableCodeItem;
                if (iExpandable != null)
                {
                    iExpandable.IsExpanded = true;
                }

                iOwner = iOwner.Owner as CodeItem;
            }
        }

        /// <summary>
        ///     Resets the <see langword="break" /> point on this item.
        /// </summary>
        public virtual void ResetBreakPoint()
        {
            fIsBreakPoint = false;
            OnPropertyChanged("IsBreakPoint");
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.CodePagePanel"/> object.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        /// <returns>The <see cref="CPPItemBase"/>.</returns>
        protected internal virtual WPF.Controls.CPPItemBase CreateDefaultUI(
            WPF.Controls.CPPItemBase owner, 
            WPF.Controls.CodePagePanel panel)
        {
            var iNew = new WPF.Controls.CPPItem(owner, panel);
            iNew.Element = new CtrlStatic();
            iNew.Data = this;
            return iNew;
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public virtual object GetCodeItemFor(ulong meaning)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}