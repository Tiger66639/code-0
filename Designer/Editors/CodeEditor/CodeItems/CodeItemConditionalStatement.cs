// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemConditionalStatement.cs" company="">
//   
// </copyright>
// <summary>
//   The code item conditional statement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The code item conditional statement.</summary>
    public class CodeItemConditionalStatement : ExpandableCodeItem, ICodeItemsOwner
    {
        /// <summary>
        ///     The default thickness to use for loops
        /// </summary>
        private static readonly System.Windows.Thickness LOOPTHICKNESS = new System.Windows.Thickness(1, 1, 1, 1);

        /// <summary>
        ///     The default thickness to use when it is not a loop
        /// </summary>
        private static readonly System.Windows.Thickness NOLOOPTHICKNESS = new System.Windows.Thickness(0, 1, 0, 1);

        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="CodeItemConditionalStatement"/> class. Initializes a new instance of the<see cref="CodeItemConditionalStatement"/> class.</summary>
        /// <param name="towrap">The towrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemConditionalStatement(ConditionalStatement towrap, bool isActive)
            : base(towrap, isActive)
        {
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

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            if (meaning == (ulong)PredefinedNeurons.LoopStyle)
            {
                return LoopStyle;
            }

            if (meaning == (ulong)PredefinedNeurons.CaseItem)
            {
                return CaseItem;
            }

            if (meaning == (ulong)PredefinedNeurons.LoopItem)
            {
                return LoopItem;
            }

            if (meaning == (ulong)PredefinedNeurons.Statements)
            {
                return Statements.Owner;

                    // this is a wrapper, so we have a custom owner. this allows us to work with 2 lists.
            }

            return null;
        }

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

        #region internal types

        /// <summary>
        ///     this is a helper class so that we can properly work with the
        ///     <see cref="JaStDev.HAB.Designer.CodeItemConditionalStatement.Statements" />
        ///     in conjunction with the <see cref="ICodeItemsOwner" /> interface. This
        ///     is used to find the 'items' list of a code item. So a code item can
        ///     only have 1 items list, but the conditional statement has 2, so we
        ///     create an extra owner between the conditional statement and this one.
        /// </summary>
        public class CodeItemConditionalStatements : Data.OwnedObject<CodeItemConditionalStatement>, 
                                                     ICodeItemsOwner, 
                                                     INeuronWrapper
        {
            /// <summary>Initializes a new instance of the <see cref="CodeItemConditionalStatements"/> class.</summary>
            /// <param name="owner">The owner.</param>
            public CodeItemConditionalStatements(CodeItemConditionalStatement owner)
            {
                Owner = owner;
            }

            #region ICodeItemsOwner Members

            /// <summary>
            ///     Gets the items.
            /// </summary>
            public CodeItemCollection Items
            {
                get
                {
                    return Owner.Statements;
                }
            }

            #endregion

            #region INeuronWrapper Members

            /// <summary>
            ///     Gets the item.
            /// </summary>
            public Neuron Item
            {
                get
                {
                    return Owner.Item;
                }
            }

            #endregion
        }

        #endregion

        #region fields

        /// <summary>The f children.</summary>
        private CodeItemCollection fChildren;

        /// <summary>The f statements.</summary>
        private CodeItemCollection fStatements;

        /// <summary>The f loop style.</summary>
        private CodeItemResult fLoopStyle;

        /// <summary>The f case item.</summary>
        private CodeItemResult fCaseItem;

        /// <summary>The f loop item.</summary>
        private CodeItemResult fLoopItem;

        /// <summary>The f loop line thickness.</summary>
        private System.Windows.Thickness fLoopLineThickness = LOOPTHICKNESS;

        /// <summary>The f case item visibility.</summary>
        private System.Windows.Visibility fCaseItemVisibility = System.Windows.Visibility.Collapsed;

        /// <summary>The f loop item visibility.</summary>
        private System.Windows.Visibility fLoopItemVisibility = System.Windows.Visibility.Collapsed;

        #endregion

        #region Prop

        /// <summary>Gets or sets a value indicating whether is active.</summary>
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

                    if (LoopStyle != null)
                    {
                        LoopStyle.IsActive = value;
                    }

                    if (CaseItem != null)
                    {
                        CaseItem.IsActive = value;
                    }

                    if (LoopItem != null)
                    {
                        LoopItem.IsActive = value;
                    }
                }
            }
        }

        #region LoopStyle

        /// <summary>
        ///     Gets/sets the style of looping that should be used.
        /// </summary>
        public CodeItemResult LoopStyle
        {
            get
            {
                return fLoopStyle;
            }

            set
            {
                InternalSetLoopStyle(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LoopStyle, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        /// <summary>The internal set loop style.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetLoopStyle(CodeItemResult value)
        {
            if (fLoopStyle != null)
            {
                UnRegisterChild(fLoopStyle);
            }

            fLoopStyle = value;
            if (fLoopStyle != null)
            {
                RegisterChild(fLoopStyle);
                if (fLoopStyle.Item.ID == (ulong)PredefinedNeurons.Looped
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.CaseLooped
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.ForEach
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoop
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopChildren
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopClusters
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopIn
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopOut
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.Until || fLoopStyle is CodeItemVariable)
                {
                    LoopLineThickness = LOOPTHICKNESS;
                }
                else
                {
                    LoopLineThickness = NOLOOPTHICKNESS;
                }

                if (fLoopStyle.Item.ID == (ulong)PredefinedNeurons.Case
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.CaseLooped || fLoopStyle is CodeItemVariable)
                {
                    CaseItemVisibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    CaseItemVisibility = System.Windows.Visibility.Collapsed;
                }

                if (fLoopStyle.Item.ID == (ulong)PredefinedNeurons.ForEach
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoop
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopChildren
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopClusters
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopIn
                    || fLoopStyle.Item.ID == (ulong)PredefinedNeurons.QueryLoopOut || fLoopStyle is CodeItemVariable)
                {
                    LoopItemVisibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    LoopItemVisibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                LoopLineThickness = NOLOOPTHICKNESS;
            }

            OnPropertyChanged("LoopStyle");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasLoopStyle");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
        }

        #endregion

        #region NotHasLoopItem

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasLoopStyle
        {
            get
            {
                return ((ConditionalStatement)Item).LoopStyle == null;
            }
        }

        #endregion

        #region LoopLineThickness

        /// <summary>
        ///     Gets how the loop lines should be displayed.
        /// </summary>
        public System.Windows.Thickness LoopLineThickness
        {
            get
            {
                return fLoopLineThickness;
            }

            internal set
            {
                fLoopLineThickness = value;
                OnPropertyChanged("LoopLineThickness");
            }
        }

        #endregion

        #region CaseItemVisibility

        /// <summary>
        ///     Gets the if the case item should be displayed.
        /// </summary>
        public System.Windows.Visibility CaseItemVisibility
        {
            get
            {
                return fCaseItemVisibility;
            }

            internal set
            {
                fCaseItemVisibility = value;
                OnPropertyChanged("CaseItemVisibility");
            }
        }

        #endregion

        #region LoopItemVisibility

        /// <summary>
        ///     Gets if the loop item should be displayed.
        /// </summary>
        public System.Windows.Visibility LoopItemVisibility
        {
            get
            {
                return fLoopItemVisibility;
            }

            internal set
            {
                fLoopItemVisibility = value;
                OnPropertyChanged("LoopItemVisibility");
            }
        }

        #endregion

        #region CaseItem

        /// <summary>
        ///     Gets/sets the case value to check.
        /// </summary>
        public CodeItemResult CaseItem
        {
            get
            {
                return fCaseItem;
            }

            set
            {
                if (fCaseItem != value)
                {
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.CaseItem, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    InternalSetCaseItem(value);
                }
            }
        }

        /// <summary>The internal set case item.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetCaseItem(CodeItemResult value)
        {
            if (fCaseItem != null)
            {
                UnRegisterChild(fCaseItem);
            }

            fCaseItem = value;
            if (fCaseItem != null)
            {
                RegisterChild(fCaseItem);
            }

            OnPropertyChanged("CaseItem");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasCaseItem");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event.
        }

        #endregion

        #region NotHasCaseItem

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasCaseItem
        {
            get
            {
                return ((ConditionalStatement)Item).CaseItem == null;
            }
        }

        #endregion

        #region LoopItem

        /// <summary>
        ///     Gets/sets the info to search for.
        /// </summary>
        /// <remarks>
        ///     Should only be visible if we are searching an info list.
        /// </remarks>
        public CodeItemResult LoopItem
        {
            get
            {
                return fLoopItem;
            }

            set
            {
                if (fLoopItem != value)
                {
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.LoopItem, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }

                    InternalSetLoopItem(value);
                }
            }
        }

        /// <summary>The internal set loop item.</summary>
        /// <param name="value">The value.</param>
        private void InternalSetLoopItem(CodeItemResult value)
        {
            if (fLoopItem != null)
            {
                UnRegisterChild(fLoopItem);
            }

            fLoopItem = value;
            if (fLoopItem != null)
            {
                RegisterChild(fLoopItem);
            }

            OnPropertyChanged("LoopItem");
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<string>(OnPropertyChanged), 
                "NotHasLoopItem");

                // we call async cause when called by the prop setter, the value has not yet been assigned to the neuron, which would cause an invalid value for this event
        }

        #endregion

        #region NotHasLoopItem

        /// <summary>
        ///     Gets the if the loopitem is present or not.
        /// </summary>
        public bool NotHasLoopItem
        {
            get
            {
                return ((ConditionalStatement)Item).LoopItem == null;
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of child code items.
        /// </summary>
        /// <remarks>
        ///     If <see cref="CodeItem.HasChildren" /> is false, this reference is
        ///     invalid. The list is only filled if <see cref="ChildrenLoaded" /> is
        ///     true.
        /// </remarks>
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

        #region Statements

        /// <summary>
        ///     Gets the list of statements that should be executed just before the
        ///     conditionals.
        /// </summary>
        /// <remarks>
        ///     If <see cref="CodeItem.HasStatements" /> is false, this reference is
        ///     invalid.
        /// </remarks>
        public CodeItemCollection Statements
        {
            get
            {
                return fStatements;
            }

            private set
            {
                if (fStatements != value)
                {
                    fStatements = value;
                    OnPropertyChanged("Statements");
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iTowrap = value as ConditionalStatement;
            if (iTowrap != null)
            {
                var iFound = iTowrap.LoopStyle;
                if (iFound != null)
                {
                    InternalSetLoopStyle((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iTowrap.CaseItem;
                if (iFound != null)
                {
                    InternalSetCaseItem((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }

                iFound = iTowrap.LoopItem;
                if (iFound != null)
                {
                    InternalSetLoopItem((CodeItemResult)EditorsHelper.CreateCodeItemFor(iFound));
                }
            }

            fChildren = null;
            fStatements = null;
            LoadChildren();
            LoadStataements();
        }

        /// <summary>The outgoing link removed.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.LoopStyle)
                {
                    InternalSetLoopStyle(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.CaseItem)
                {
                    InternalSetCaseItem(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.LoopItem)
                {
                    InternalSetLoopItem(null);
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    LoadChildren();
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Statements)
                {
                    LoadStataements();
                }
            }
        }

        /// <summary>The outgoing link created.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.LoopStyle)
                {
                    InternalSetLoopStyle((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.CaseItem)
                {
                    InternalSetCaseItem((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.LoopItem)
                {
                    InternalSetLoopItem((CodeItemResult)EditorsHelper.CreateCodeItemFor(Brain.Current[link.ToID]));
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Condition)
                {
                    LoadChildren();
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.Statements)
                {
                    LoadStataements();
                }
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
            iNew.Element = new WPF.Controls.CtrlCondStatement();
            iNew.Data = this;
            iNew.List.Orientation = System.Windows.Controls.Orientation.Horizontal;
            iNew.List.ItemsSource = Children;
            var iBackground = new System.Windows.Controls.Border();

                // for the background, we simply create a border from code.
            iBackground.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            iBackground.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            var iBind = new System.Windows.Data.Binding("LoopLineThickness");

                // we bind the borderthickness to our LoopLineThickness prop, to give a visual queue about our loop state.
            iBind.Source = this;
            iBackground.SetBinding(System.Windows.Controls.Border.BorderThicknessProperty, iBind);
            iNew.List.ListBackground = iBackground;

            iNew.ExtraList = new WPF.Controls.CPPItemList(iNew, panel);

                // create the items list for the extra code items in front of the conditionals.
            iNew.ExtraList.Orientation = System.Windows.Controls.Orientation.Vertical;
            iNew.ExtraList.IsExpanded = true; // make certain that the data is expanded, otherwise nothing gets loaded.
            iNew.ExtraList.ItemsSource = Statements;

            // iBackground = new Border();                                                  //for the background, we simply create a border from code.
            // iBackground.Background = new SolidColorBrush(Colors.White);
            // iNew.ExtraList.ListBackground = iBackground;
            return iNew;
        }

        /// <summary>The load children.</summary>
        private void LoadChildren()
        {
            var iConditions = ((ConditionalStatement)Item).ConditionsCluster;
            if (fChildren == null || iConditions != fChildren.Cluster)
            {
                if (iConditions != null)
                {
                    Children = new CodeItemCollection(this, iConditions);
                }
                else
                {
                    Children = new CodeItemCollection(this, (ulong)PredefinedNeurons.Condition);
                }
            }
        }

        /// <summary>The load stataements.</summary>
        private void LoadStataements()
        {
            var iStatements = ((ConditionalStatement)Item).StatementsCluster;
            if (fStatements == null || iStatements != fStatements.Cluster)
            {
                var iOwner = new CodeItemConditionalStatements(this);

                    // we need an extra layer so we can work with 2 lists in 1 code item and still use the ICodeItemsOwner interface
                if (iStatements != null)
                {
                    Statements = new CodeItemCollection(iOwner, iStatements);
                }
                else
                {
                    Statements = new CodeItemCollection(iOwner, (ulong)PredefinedNeurons.Statements);
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
            if (LoopStyle == iChild)
            {
                LoopStyle = null;
            }
            else if (CaseItem == iChild)
            {
                CaseItem = null;
            }
            else if (LoopItem == iChild)
            {
                LoopItem = null;
            }
            else if (Statements == null || Statements.Remove(iChild) == false)
            {
                if (Children.Remove(iChild) == false)
                {
                    base.RemoveChild(iChild);
                }
            }
        }

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            foreach (var i in Children)
            {
                i.Select(neuron);
            }

            if (Statements != null)
            {
                foreach (var i in Statements)
                {
                    i.Select(neuron);
                }
            }

            CodeItem iItem = LoopStyle;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = LoopItem;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }

            iItem = CaseItem;
            if (iItem != null)
            {
                iItem.Select(neuron);
            }
        }

        #endregion
    }
}