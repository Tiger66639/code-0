// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemCodeBlock.cs" company="">
//   
// </copyright>
// <summary>
//   Wraps an ExpressionsBlock.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Wraps an ExpressionsBlock.
    /// </summary>
    public class CodeItemCodeBlock : ExpandableCodeItem, ICodeItemsOwner
    {
        #region fields

        /// <summary>The f children.</summary>
        private CodeItemCollection fChildren;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="CodeItemCodeBlock"/> class. Initializes a new instance of the <see cref="CodeItemCodeBlock"/>
        ///     class.</summary>
        /// <remarks>We don't use the base constructor with same arguments, but build the
        ///     data ourselves, cause we need to make certain that IsExpanded is
        ///     false, otherwise we will load the <see cref="Children"/> when the Item
        ///     is set. This can cause an ethernal loop if one of the children is the
        ///     owner (recursive call).</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemCodeBlock(ExpressionsBlock toWrap, bool isActive)
        {
            IsExpanded = false;
            Item = toWrap;
            IsActive = isActive;
        }

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
                    if (Children != null)
                    {
                        Children.IsActive = value;
                    }
                }
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of statements for this conditional expression.
        /// </summary>
        public CodeItemCollection Children
        {
            get
            {
                return fChildren;
            }

            private set
            {
                if (fChildren != value)
                {
                    fChildren = value;
                    OnPropertyChanged("Children");
                }
            }
        }

        #endregion

        #region ICodeItemsOwner Members

        /// <summary>Gets the items.</summary>
        CodeItemCollection ICodeItemsOwner.Items
        {
            get
            {
                return Children;
            }
        }

        #endregion

        /// <summary>
        ///     Resets the <see langword="break" /> point on this item.
        /// </summary>
        public override void ResetBreakPoint()
        {
            base.ResetBreakPoint();
            if (Children != null)
            {
                foreach (var i in Children)
                {
                    i.ResetBreakPoint();
                }
            }
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            return null;
        }

        #region Functions

        /// <summary>
        ///     Called when the code item get expanded or contracted again. Allows
        ///     descendents to dynamically load/unload data if needed. Doesn't do
        ///     anything by default.
        /// </summary>
        protected override void OnExpandedChanged()
        {
            LoadChildren(); // will also unload when needed.
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            fChildren = null; // item changed, so children always need to be reloaded.
            LoadChildren();
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.Statements)
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
                if (link.MeaningID == (ulong)PredefinedNeurons.Statements)
                {
                    LoadChildren();
                }
            }
        }

        /// <summary>
        ///     Loads the children.
        /// </summary>
        protected void LoadChildren()
        {
            if (IsExpanded)
            {
                // it's important to only load the children when expanded, otherwise, a Code block that calls itself (recursive code block), would cause the developper to get stuck into a loop and we get a stack overflow.
                var iStatements = ((ExpressionsBlock)Item).StatementsCluster;
                if (fChildren == null || iStatements != fChildren.Cluster)
                {
                    if (iStatements != null)
                    {
                        Children = new CodeItemCollection(this, iStatements);
                    }
                    else
                    {
                        Children = new CodeItemCollection(this, (ulong)PredefinedNeurons.Statements);
                    }
                }
            }
            else
            {
                Children = null; // when not expanded, there are no children.
            }
        }

        /// <summary>Removes the current code item from the code list, but not the actual
        ///     neuron that represents the code item, this stays in the brain, it is
        ///     simply no longer used in this code list.</summary>
        /// <param name="child"></param>
        public override void RemoveChild(EditorItem child)
        {
            var iChild = (CodeItem)child;
            if (Children.Remove(iChild) == false)
            {
                base.RemoveChild(iChild);
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
            iNew.Element = new WPF.Controls.CtrlCodeItemBlock();

            // iNew.Element.Content = this;
            iNew.Data = this;
            iNew.List.Orientation = System.Windows.Controls.Orientation.Vertical;
            iNew.List.ItemsSource = Children;
            return iNew;
        }

        #endregion
    }
}