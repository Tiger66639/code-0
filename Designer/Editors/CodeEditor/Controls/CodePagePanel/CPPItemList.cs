// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CPPItemList.cs" company="">
//   
// </copyright>
// <summary>
//   Manages a list of controls that are displayed on a
//   <see cref="CodePagePanel" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Manages a list of controls that are displayed on a
    ///     <see cref="CodePagePanel" /> .
    /// </summary>
    public class CPPItemList : CPPItemBase, System.Windows.IWeakEventListener
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CPPItemList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public CPPItemList(CPPItemBase owner, CodePagePanel panel)
            : base(owner, panel)
        {
        }

        #endregion

        #region Fields

        /// <summary>The droptargetmargin.</summary>
        private const double DROPTARGETMARGIN = 4.0;

                             // we use this to offset the droptarget a little bit programmically, depending on the orientation. The codeLine also uses this to account for this offset.

        /// <summary>The backgroundmargin.</summary>
        private const double BACKGROUNDMARGIN = 8.0;

        /// <summary>The f items source.</summary>
        private CodeItemCollection fItemsSource;

        /// <summary>The f children.</summary>
        private readonly System.Collections.Generic.List<CPPItemBase> fChildren =
            new System.Collections.Generic.List<CPPItemBase>();

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f orientation.</summary>
        private System.Windows.Controls.Orientation fOrientation = System.Windows.Controls.Orientation.Horizontal;

        /// <summary>The f drop target.</summary>
        private CtrlDropTarget fDropTarget;

        /// <summary>The f list background.</summary>
        private System.Windows.FrameworkElement fListBackground;

        #endregion

        #region Prop

        #region ItemsSource

        /// <summary>
        ///     Gets/sets the list of items to display.
        /// </summary>
        public CodeItemCollection ItemsSource
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
                    if (fItemsSource != null && fIsExpanded)
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
        public System.Collections.Generic.IList<CPPItemBase> Children
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
                    if (fItemsSource != null)
                    {
                        if (fIsExpanded)
                        {
                            BuildItems();
                        }
                        else
                        {
                            ClearItems();
                        }
                    }

                    if (SubList != null)
                    {
                        SubList.IsExpanded = value;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     a <see langword="ref" /> to a possible sublist (headered items can have
        ///     2 lists). This is provided so that when this list is
        ///     expanded/collapsed the sublist can also be expanded/collapsed.
        /// </summary>
        public CPPItemList SubList { get; set; }

        /// <summary>
        ///     Gets/sets if this list is used as a sub list (for conditional
        ///     statemnets). when this is true, some margins are drawn differently.
        /// </summary>
        public bool IsSubList { get; set; }

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
                    UpdateCodeLine();
                }
            }
        }

        /// <summary>The update code line.</summary>
        private void UpdateCodeLine()
        {
            if (Orientation == System.Windows.Controls.Orientation.Vertical && IsExpanded)
            {
                CodeLine = new System.Windows.Shapes.Line();
                CodeLine.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                CodeLine.StrokeThickness = 1;
                CodeLine.IsHitTestVisible = false; // need to make certain it doesn't interupt with the drop code.
                Panel.Children.Add(CodeLine);
            }
            else
            {
                Panel.Children.Remove(CodeLine);
                CodeLine = null;
            }
        }

        #endregion

        /// <summary>
        ///     Used to draw a vertical line that connects the code items.
        /// </summary>
        /// <value>
        ///     The code line.
        /// </value>
        public System.Windows.Shapes.Line CodeLine { get; private set; }

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
                    }

                    fListBackground = value;
                    if (fListBackground != null && IsExpanded)
                    {
                        // if we are not expanded, we will add the listbackground again when expanding, we don't want that.
                        Panel.Children.Add(fListBackground);
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

            if (Owner != null)
            {
                // if we are owned by an item, we also need a drop advisor. (if we are not owned by an item, we are the direct root on the panel, which will handle the drop).
                fDropTarget = new CtrlDropTarget();
                fDropTarget.DataContext = ItemsSource;
                Panel.Children.Add(fDropTarget);
            }

            UpdateCodeLine();
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
            var iSize = new System.Windows.Size(0, 0);
            foreach (var i in fChildren)
            {
                i.Measure(available);
                UpdateSize(ref iSize, i.Size);
            }

            var iDropTargetMargin = IsSubList == false ? DROPTARGETMARGIN : 0;
            if (fDropTarget != null)
            {
                fDropTarget.Measure(available);
                System.Windows.Controls.Panel.SetZIndex(fDropTarget, ZIndex + 1);

                    // droptarget needs to be at same level as children.
                UpdateSize(ref iSize, fDropTarget.DesiredSize);
                if (Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    // the droptarget is drawn with a small margin, which we need to include in our size
                    iSize.Width += iDropTargetMargin;
                }
                else
                {
                    iSize.Height += iDropTargetMargin;
                }
            }

            if (CodeLine != null)
            {
                // we know our required size, so we can assign this to the line that connects all the code items, if there is one (we are vertically)
                CodeLine.X1 = iSize.Width / 2;
                CodeLine.X2 = iSize.Width / 2;
                CodeLine.Y1 = 0;
                CodeLine.Y2 = iSize.Height;
                CodeLine.Measure(iSize);
                System.Windows.Controls.Panel.SetZIndex(CodeLine, ZIndex);

                    // we need to make certain that code lines are always drawn in the background. if we don't do this, it can overlap the droptarget or be to low in the zindex.
            }

            if (ListBackground != null && IsExpanded)
            {
                ListBackground.Measure(iSize); // the background always gets the same size as the list.
                System.Windows.Controls.Panel.SetZIndex(ListBackground, ZIndex - 1);

                    // the background needs to be below everything.
            }

            Size = iSize;
        }

        /// <summary>Updates the <paramref name="size"/> field, with the<paramref name="size"/> of the new object, according to the
        ///     orientation rules.</summary>
        /// <param name="size">The size.</param>
        /// <param name="sizeOfNew">The <paramref name="size"/> of new.</param>
        private void UpdateSize(ref System.Windows.Size size, System.Windows.Size sizeOfNew)
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

        /// <summary>Arranges the items.</summary>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public override void Arrange(System.Windows.Size size, ref System.Windows.Point offset)
        {
            if (Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                ArrangeHor(size, ref offset);
            }
            else
            {
                ArrangeVertical(size, ref offset);
            }
        }

        /// <summary>Arranges verticallly.</summary>
        /// <param name="size">The size.</param>
        /// <param name="offset">The offset.</param>
        private void ArrangeVertical(System.Windows.Size size, ref System.Windows.Point offset)
        {
            var iBackgroundMargin = IsSubList == false ? BACKGROUNDMARGIN : 0;
            if (CodeLine != null)
            {
                CodeLine.X1 = size.Width / 2;
                CodeLine.X2 = size.Width / 2;
                CodeLine.Y1 = 0;
                CodeLine.Y2 = CodeLine.DesiredSize.Height;
                CodeLine.Arrange(
                    new System.Windows.Rect(
                        offset.X, 
                        offset.Y, 
                        size.Width, 
                        System.Math.Max(CodeLine.DesiredSize.Height, 0)));

                    // before we draw the code items, we draw the central line that connects them.
            }

            foreach (var i in Children)
            {
                var iNewSize = new System.Windows.Size(size.Width, i.Size.Height);

                    // vertical or: the height is restricted to that of the child. Not so important in  this algorithm, since we usually center horizontally. But drop targets might use the vertical alignment.
                i.Arrange(iNewSize, ref offset); // don't need to change the offset, the chidlren do this.
            }

            if (fDropTarget != null)
            {
                var iRect = new System.Windows.Rect(
                    ((size.Width - fDropTarget.DesiredSize.Width) / 2) + offset.X, 
                    offset.Y + iBackgroundMargin, 
                    fDropTarget.DesiredSize.Width, 
                    fDropTarget.DesiredSize.Height);
                fDropTarget.Arrange(iRect);
                offset.Offset(0, fDropTarget.DesiredSize.Height + iBackgroundMargin);
            }

            if (ListBackground != null && IsExpanded)
            {
                var iRect = new System.Windows.Rect(
                    ((size.Width - Size.Width) / 2) + offset.X, 
                    offset.Y + iBackgroundMargin, 

                    // we add a little margin, since all code items add this at their top, so we need to add this, to correctly put the background round everything.
                    Size.Width, 
                    Size.Height + iBackgroundMargin); // to compensate of the Y offset.
                ListBackground.Arrange(iRect);
            }
        }

        /// <summary>Arranges horizontally.</summary>
        /// <param name="size">The size.</param>
        /// <param name="offset">The offset.</param>
        private void ArrangeHor(System.Windows.Size size, ref System.Windows.Point offset)
        {
            var iSubOffset = new System.Windows.Point(((size.Width - Size.Width) / 2) + offset.X, offset.Y);

                // hor or: children need to be in the center of the sceen, and added to the right. When we are done, we don't need to let following siblings do this, they only need to go down, so make a copy for all the children.
            foreach (var i in Children)
            {
                var iNewSize = new System.Windows.Size(i.Size.Width, size.Height);

                    // horizontal orientation: each item gets the desired width, their heights is the same: all the available. This is used to center the objects.
                var iChildOffset = iSubOffset;
                i.Arrange(iNewSize, ref iChildOffset);
                iSubOffset.Offset(i.Size.Width, 0);

                    // we have consumed a collumn, so let the next children put their children next to this.
            }

            if (fDropTarget != null)
            {
                var iRect = new System.Windows.Rect(
                    iSubOffset.X + DROPTARGETMARGIN, 
                    offset.Y + DROPTARGETMARGIN, 
                    fDropTarget.DesiredSize.Width, 
                    fDropTarget.DesiredSize.Height);
                fDropTarget.Arrange(iRect);
            }

            if (ListBackground != null && IsExpanded)
            {
                var iRect = new System.Windows.Rect(
                    ((size.Width - Size.Width) / 2) + offset.X, 
                    offset.Y + BACKGROUNDMARGIN, 
                    Size.Width, 
                    Size.Height);
                ListBackground.Arrange(iRect);
                offset.Offset(0, Size.Height + BACKGROUNDMARGIN); // subsequent items need to be put below this list.
            }
            else
            {
                offset.Offset(0, Size.Height);
            }
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            if (CodeLine != null)
            {
                Panel.Children.Remove(CodeLine);
                CodeLine = null;
            }

            if (fDropTarget != null)
            {
                Panel.Children.Remove(fDropTarget);
                fDropTarget = null;
            }

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
                    Children.Insert(e.NewStartingIndex, ((CodeItem)e.NewItems[0]).CreateDefaultUI(this, Panel));
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
                    Children[e.NewStartingIndex] = ((CodeItem)e.NewItems[0]).CreateDefaultUI(this, Panel);
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