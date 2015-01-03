// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowPanelItemList.cs" company="">
//   
// </copyright>
// <summary>
//   Manages a list of controls that are displayed on a
//   <see cref="FlowPanel" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Manages a list of controls that are displayed on a
    ///     <see cref="FlowPanel" /> .
    /// </summary>
    public class FlowPanelItemList : FlowPanelItemBase, System.Windows.IWeakEventListener
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FlowPanelItemList"/> class. Initializes a new instance of the <see cref="FlowPanelItemList"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public FlowPanelItemList(FlowPanelItemBase owner, FlowPanel panel)
            : base(owner, panel)
        {
        }

        #endregion

        /// <summary>Gets the X positions that the argument uses in this list.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Point"/>.</returns>
        internal System.Windows.Point GetRangeOf(FlowPanelItemBase item)
        {
            double iX = 0;
            double iWidth = 0;
            foreach (var i in Children)
            {
                if (i == item)
                {
                    iWidth = item.Size.Width;
                    break;
                }

                iX += i.Size.Width;
            }

            return new System.Windows.Point(iX, iX + iWidth);
        }

        /// <summary>Moves the focus to the child at the specified x position (taken from
        ///     the left).</summary>
        /// <param name="point">The point.</param>
        internal void FocusChildAt(System.Windows.Point point)
        {
            double iX = 0;
            var iFound = false;
            foreach (var i in Children)
            {
                iX += i.Size.Width;
                if (iX >= point.X && iX <= point.Y)
                {
                    i.Focus();
                    iFound = true;
                    break;
                }
            }

            if (iFound == false && Children.Count > 0)
            {
                Children[Children.Count - 1].Focus();
            }
        }

        /// <summary>
        ///     Moves focuses to the ui element appropriate for this item.
        /// </summary>
        protected internal override void Focus()
        {
            if (ListBackground != null)
            {
                var iItem = ListBackground.DataContext as FlowItem;
                iItem.IsSelected = true;
                ListBackground.Focus();
            }
        }

        /// <summary>The move down.</summary>
        protected internal override void MoveDown()
        {
            var iList = ((FlowPanelItemList)Owner).Owner as EnclosedFlowPanelItemList;
            if (iList != null)
            {
                var iIndex = iList.List.Children.IndexOf(iList);
                if (iIndex < iList.List.Children.Count - 1)
                {
                    iList.List.Children[iIndex + 1].Focus();
                }
                else
                {
                    iList.Focus();
                }
            }
        }

        /// <summary>The move left.</summary>
        protected internal override void MoveLeft()
        {
            var iList = ((FlowPanelItemList)Owner).Owner as EnclosedFlowPanelItemList;
            if (iList != null)
            {
                iList.Focus();
            }
        }

        /// <summary>The move right.</summary>
        protected internal override void MoveRight()
        {
            if (Children.Count > 0)
            {
                Children[0].Focus();
            }
            else
            {
                var iList = ((FlowPanelItemList)Owner).Owner as EnclosedFlowPanelItemList;
                if (iList != null)
                {
                    iList.FocusToRight();
                }
            }
        }

        /// <summary>The move up.</summary>
        protected internal override void MoveUp()
        {
            var iList = ((FlowPanelItemList)Owner).Owner as EnclosedFlowPanelItemList;
            if (iList != null)
            {
                var iIndex = iList.List.Children.IndexOf(iList);
                if (iIndex > 0)
                {
                    iList.List.Children[iIndex - 1].Focus();
                }
                else
                {
                    iList.Focus();
                }
            }
        }

        #region fields

        /// <summary>The droptargetmargin.</summary>
        protected const double DROPTARGETMARGIN = 3.0;

        /// <summary>The itemspace.</summary>
        protected const double ITEMSPACE = 2.0; // the space between 2 children.

        /// <summary>The f items source.</summary>
        private FlowItemCollection fItemsSource;

        /// <summary>The f children.</summary>
        private readonly System.Collections.Generic.List<FlowPanelItemBase> fChildren =
            new System.Collections.Generic.List<FlowPanelItemBase>();

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f orientation.</summary>
        private System.Windows.Controls.Orientation fOrientation = System.Windows.Controls.Orientation.Horizontal;

        /// <summary>The f list background.</summary>
        private System.Windows.FrameworkElement fListBackground;

        #endregion

        #region Prop

        #region ItemsSource

        /// <summary>
        ///     Gets/sets the list of items to display.
        /// </summary>
        public FlowItemCollection ItemsSource
        {
            get
            {
                return fItemsSource;
            }

            set
            {
                if (value != fItemsSource)
                {
                    if (fItemsSource != null)
                    {
                        ClearItems();
                    }

                    fItemsSource = value;
                    if (fItemsSource != null && IsExpanded)
                    {
                        BuildItems();
                    }
                }
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children that are generated based on the itemsSource.
        /// </summary>
        public System.Collections.Generic.IList<FlowPanelItemBase> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the value that indicates if this list is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (value != fIsExpanded)
                {
                    fIsExpanded = value;
                    if (fIsExpanded)
                    {
                        if (ItemsSource != null)
                        {
                            BuildItems();
                        }
                    }
                    else
                    {
                        ClearItems();
                    }
                }
            }
        }

        #endregion

        #region Orientation

        /// <summary>
        ///     Gets or sets the orientation that the list is depicted.
        /// </summary>
        /// <value>
        ///     The orientation.
        /// </value>
        public System.Windows.Controls.Orientation Orientation
        {
            get
            {
                return fOrientation;
            }

            set
            {
                if (value != fOrientation)
                {
                    fOrientation = value;
                }
            }
        }

        #endregion

        #region ListBackground

        /// <summary>
        ///     Allows you to assign a custom list background. Conditionals use this
        ///     to display the type of looping and code blocks use it to group
        ///     together their code.
        /// </summary>
        /// <value>
        ///     The list background.
        /// </value>
        public System.Windows.FrameworkElement ListBackground
        {
            get
            {
                return fListBackground;
            }

            set
            {
                if (value != fListBackground)
                {
                    if (fListBackground != null)
                    {
                        Panel.Children.Remove(fListBackground);
                        fListBackground.Tag = null;
                    }

                    fListBackground = value;
                    if (fListBackground != null && IsExpanded)
                    {
                        // if we are not expanded, we will add the listbackground again when expanding, we don't want that.
                        Panel.Children.Add(fListBackground);
                        fListBackground.Tag = this;

                            // the UI control needs to know about the flowPanelItem, so we can move with the cursor.
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Creates the user controls for all the code items in the list.
        /// </summary>
        /// <param name="list">
        ///     The list of values to build ui elements for.
        /// </param>
        private void BuildItems()
        {
            foreach (var i in ItemsSource)
            {
                var iNew = i.CreateDefaultUI(this, Panel);
                System.Diagnostics.Debug.Assert(iNew != null);
                fChildren.Add(iNew);
            }

            if (ListBackground != null)
            {
                Panel.Children.Add(ListBackground);
            }

            System.Collections.Specialized.CollectionChangedEventManager.AddListener(ItemsSource, this);
            UpdateMeasure();
        }

        /// <summary>
        ///     Clears all the items and makes certain that this list is rearranged.
        ///     This is used to collaps the list, but not completely remove it.
        /// </summary>
        private void ClearItems()
        {
            System.Collections.Specialized.CollectionChangedEventManager.RemoveListener(ItemsSource, this);
            Release();
            fChildren.Clear();
            UpdateMeasure();
        }

        #endregion

        #region Overrides

        /// <summary>Measures this instance.</summary>
        /// <param name="available">The available.</param>
        /// <remarks>The list doesn't have to calculate it's zindex, it gets assigned by
        ///     the header object, and the first has a default of 0.</remarks>
        public override void Measure(System.Windows.Size available)
        {
            SetZIndex();
            var iSize = new System.Windows.Size(0, 0);
            MeasureChildren(ref iSize, available);
            AddMargins(ref iSize);
            MeasureBackground(ref iSize);
            Size = iSize;
        }

        /// <summary>The measure background.</summary>
        /// <param name="iSize">The i size.</param>
        protected void MeasureBackground(ref System.Windows.Size iSize)
        {
            if (ListBackground != null && IsExpanded)
            {
                iSize = new System.Windows.Size(
                    System.Math.Max(ListBackground.MinWidth, iSize.Width), 
                    System.Math.Max(ListBackground.MinHeight, iSize.Height));
                ListBackground.Measure(iSize);

                    // the background always gets the same size as the list, except if it's minimum size is bigger than what we try to render.
                System.Windows.Controls.Panel.SetZIndex(ListBackground, ZIndex - 1);

                    // the background needs to be below everything.
            }
        }

        /// <summary>The add margins.</summary>
        /// <param name="iSize">The i size.</param>
        protected void AddMargins(ref System.Windows.Size iSize)
        {
            iSize.Height += DROPTARGETMARGIN * 2;
            iSize.Width += DROPTARGETMARGIN * 2;
            if (Children.Count > 0)
            {
                if (Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    iSize.Width += ITEMSPACE * (Children.Count - 1);
                }
                else
                {
                    iSize.Height += ITEMSPACE * (Children.Count - 1);
                }
            }
        }

        /// <summary>The measure children.</summary>
        /// <param name="iSize">The i size.</param>
        /// <param name="available">The available.</param>
        protected void MeasureChildren(ref System.Windows.Size iSize, System.Windows.Size available)
        {
            foreach (var i in fChildren)
            {
                i.Measure(available);
                UpdateSize(ref iSize, i.Size);
            }
        }

        /// <summary>The set z index.</summary>
        protected void SetZIndex()
        {
            if (Owner != null)
            {
                ZIndex = Owner.ZIndex + 1;
            }
            else
            {
                ZIndex = 0;
            }
        }

        /// <summary>Updates the <paramref name="size"/> field, with the<paramref name="size"/> of the new object, according to the
        ///     orientation rules.</summary>
        /// <param name="size">The size.</param>
        /// <param name="sizeOfNew">The <paramref name="size"/> of new.</param>
        protected void UpdateSize(ref System.Windows.Size size, System.Windows.Size sizeOfNew)
        {
            if (Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                size.Width += sizeOfNew.Width;
                size.Height = System.Math.Max(size.Height, sizeOfNew.Height);
            }
            else
            {
                size.Width = System.Math.Max(size.Width, sizeOfNew.Width);
                size.Height += sizeOfNew.Height;
            }
        }

        /// <summary>Arranges verticallly.</summary>
        /// <param name="size">The size.</param>
        /// <param name="offset">The offset.</param>
        public override void Arrange(System.Windows.Size size, System.Windows.Point offset)
        {
            var iSubOffset = offset;
            Arrange(size, offset, iSubOffset);
        }

        /// <summary>The arrange.</summary>
        /// <param name="size">The size.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="iSubOffset">The i sub offset.</param>
        protected void Arrange(System.Windows.Size size, System.Windows.Point offset, System.Windows.Point iSubOffset)
        {
            iSubOffset.Offset(DROPTARGETMARGIN, DROPTARGETMARGIN); // upper corner border needs to be taken into account
            if (Orientation == System.Windows.Controls.Orientation.Vertical)
            {
                foreach (var i in Children)
                {
                    var iNewSize = new System.Windows.Size(size.Width - (DROPTARGETMARGIN * 2), i.Size.Height);

                        // vertical or: the height is restricted to that of the child. Not so important in  this algorithm, since we usually center horizontally. But drop targets might use the vertical alignment.
                    i.Arrange(iNewSize, iSubOffset); // don't need to change the offset, the chidlren do this.
                    iSubOffset = new System.Windows.Point(
                        offset.X + DROPTARGETMARGIN, 
                        iSubOffset.Y + i.Size.Height + ITEMSPACE);
                }
            }
            else
            {
                foreach (var i in Children)
                {
                    var iNewSize = new System.Windows.Size(i.Size.Width, Size.Height - (DROPTARGETMARGIN * 2));

                        // horizontal orientation: each item gets the desired width, their heights is the same: all the available. This is used to center the objects.
                    i.Arrange(iNewSize, iSubOffset);
                    iSubOffset = new System.Windows.Point(
                        iSubOffset.X + i.Size.Width + ITEMSPACE, 
                        offset.Y + DROPTARGETMARGIN);
                }
            }

            if (ListBackground != null && IsExpanded)
            {
                var iRect = new System.Windows.Rect(offset.X, offset.Y, Size.Width, Size.Height);
                ListBackground.Arrange(iRect);
            }
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            if (ListBackground != null)
            {
                Panel.Children.Remove(ListBackground);

                    // don't set to null, than we can't add it anymore since this is not a generated object, but assigned.
            }

            foreach (var i in Children)
            {
                i.Release();
            }
        }

        /// <summary>Gets the last UI element of the <see cref="FlowPanel"/> item, to be
        ///     used for calculating the position when displaying the drop down.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        protected internal override System.Windows.FrameworkElement GetLastUI()
        {
            if (Children.Count > 0)
            {
                return Children[Children.Count - 1].GetLastUI();
            }

            if (ListBackground != null)
            {
                return ListBackground;
            }

            return null;
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.Collections.Specialized.CollectionChangedEventManager))
            {
                ItemsSource_CollectionChanged((System.Collections.Specialized.NotifyCollectionChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Handles the CollectionChanged event of the Items control.</summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void ItemsSource_CollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Children.Insert(e.NewStartingIndex, ((FlowItem)e.NewItems[0]).CreateDefaultUI(this, Panel));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var iToMove = fChildren[e.OldStartingIndex];
                    fChildren.RemoveAt(e.OldStartingIndex);
                    fChildren.Insert(e.NewStartingIndex, iToMove);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Children[e.OldStartingIndex].Release();
                    Children.RemoveAt(e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    Children[e.NewStartingIndex].Release();
                    Children[e.NewStartingIndex] = ((FlowItem)e.NewItems[0]).CreateDefaultUI(this, Panel);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (var i in Children)
                    {
                        i.Release();
                    }

                    fChildren.Clear();
                    break;
                default:
                    break;
            }

            UpdateMeasure();
        }

        #endregion
    }
}