// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemResultStatement.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for result statent neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for result statent neurons.
    /// </summary>
    public class CodeItemResultStatement : CodeItemResult, ICodeItemsOwner
    {
        /// <summary>The f arguments.</summary>
        private CodeItemCollection fArguments;

        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemResultStatement"/> class. Initializes a new instance of the<see cref="CodeItemResultStatement"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemResultStatement(ResultStatement toWrap, bool isActive)
            : base(toWrap, isActive)
        {
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
                if (value != base.IsActive)
                {
                    base.IsActive = value;
                    if (Arguments != null)
                    {
                        Arguments.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #region Instruction

        /// <summary>
        ///     Gets/sets the instruction to use.
        /// </summary>
        /// <remarks>
        ///     Wrapper for <see cref="JaStDev.HAB.Statement.Instruction" /> to
        ///     provide undo info and ui updating.
        /// </remarks>
        public ResultInstruction Instruction
        {
            get
            {
                return ((ResultStatement)Item).Instruction;
            }

            set
            {
                OnPropertyChanging("Instruction", ((ResultStatement)Item).Instruction, value);
                ((ResultStatement)Item).Instruction = value;

                // PropertyChanged event is raised by the Link_Changed event handler, making certain that all updates are correctly done.
            }
        }

        #endregion

        #region Arguments

        /// <summary>
        ///     Gets the list of arguments for this statement.
        /// </summary>
        public CodeItemCollection Arguments
        {
            get
            {
                return fArguments;
            }

            internal set
            {
                if (fArguments != value)
                {
                    fArguments = value;
                    OnPropertyChanged("Arguments");
                }
            }
        }

        #endregion

        #region ICodeItemsOwner Members

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        CodeItemCollection ICodeItemsOwner.Items
        {
            get
            {
                return Arguments;
            }
        }

        #endregion

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            return null;
        }

        #region Functions

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            fArguments = null;
            LoadChildren();
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Instruction)
                {
                    OnPropertyChanged("Instruction");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Arguments)
                {
                    LoadChildren();
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
                if (link.MeaningID == (ulong)PredefinedNeurons.Instruction)
                {
                    OnPropertyChanged("Instruction");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Arguments)
                {
                    LoadChildren();
                }
            }
        }

        /// <summary>
        ///     Loads the children.
        /// </summary>
        private void LoadChildren()
        {
            var iArgCluster = ((ResultStatement)Item).ArgumentsCluster;
            if (fArguments == null || iArgCluster != fArguments.Cluster)
            {
                if (iArgCluster != null)
                {
                    Arguments = new CodeItemCollection(this, iArgCluster);
                }
                else
                {
                    Arguments = new CodeItemCollection(this, (ulong)PredefinedNeurons.Arguments);
                }
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iChild = (CodeItem)child;
            if (Arguments.Remove(iChild) == false)
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
            foreach (var i in Arguments)
            {
                i.Select(neuron);
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
            iNew.Element = new CtrlResultStatement();
            iNew.Data = this;
            return iNew;
        }

        #endregion
    }
}