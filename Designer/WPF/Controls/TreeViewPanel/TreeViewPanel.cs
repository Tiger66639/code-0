// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeViewPanel.cs" company="">
//   
// </copyright>
// <summary>
//   A panel that dislays it's data using a Treeview paradigm. Children are wrapped using
//   <see cref="TreeViewPanelItem" /> objects. Scrolling is provided through the base
//   <see cref="ScrollablePanel" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A panel that dislays it's data using a Treeview paradigm. Children are wrapped using
    ///     <see cref="TreeViewPanelItem" /> objects. Scrolling is provided through the base
    ///     <see cref="ScrollablePanel" />.
    /// </summary>
    /// <remarks>
    ///     All children displayed in this panel should implement <see cref="ITreeViewPanelItem" /> so that
    ///     the panel can manipulate (or retreive info from) the data when required.
    /// </remarks>
    public class TreeViewPanel : ScrollablePanel, System.Windows.IWeakEventListener
    {
        /// <summary>
        ///     Goes to the first data item and displays this at the bottom of the treeview.
        /// </summary>
        public void GotoFirst()
        {
            VerticalOffset = 0;
        }

        /// <summary>
        ///     Goes to the last data item and displays this at the bottom of the treeview.
        /// </summary>
        public void GotoLast()
        {
            VerticalOffset = VerticalMax;
        }

        /// <summary>
        ///     Goes to the first data item, selects it and displays this at the bottom of the treeview.
        /// </summary>
        public void SelectFirst()
        {
            GotoFirst();
            var iFirst = GetItemAt(fTopIndex);
            if (iFirst != null)
            {
                iFirst.IsSelected = true;
            }
        }

        /// <summary>
        ///     Goes to the last data item, selects it and displays this at the bottom of the treeview.
        /// </summary>
        public void SelectLast()
        {
            GotoLast();
            var iLast = GetItemAt(fBottomIndex);
            if (iLast != null)
            {
                iLast.IsSelected = true;
            }
        }

        /// <summary>Selects the next item that follows the specified data item in the tree. When it is not in
        ///     the viewable area, it is scrolled into view.</summary>
        /// <param name="data">The data.</param>
        public void SelectNext(ITreeViewPanelItem data)
        {
            ITreeViewPanelItem iNext;
            TreeViewPanelItem iWrapper;
            if (fDict.TryGetValue(data, out iWrapper))
            {
                var iIndex = InternalChildren.IndexOf(iWrapper);
                if (iIndex < InternalChildren.Count - 1)
                {
                    iWrapper = InternalChildren[iIndex + 1] as TreeViewPanelItem;
                    System.Diagnostics.Debug.Assert(iWrapper != null);
                    iNext = iWrapper.Content as ITreeViewPanelItem;
                    iNext.IsSelected = true;
                    iWrapper.Focus();
                    return;
                }
            }

            fRenderingData = true;

                // we need to prevent the scrollbar from rendering data when we update it, we want to control the rendering ourselves.
            try
            {
                fBottomIndex = GetIndex(data);
                iNext = GetNextData(data);
                if (iNext != null)
                {
                    VerticalOffset++;

                        // we are rendering new data from a new bottom, so move the scrollpos to the new top value to keep it all in sync.
                    RenderChildrenUntil(iNext);
                    if (IsLast(fBottomIndex))
                    {
                        fLastIsVisible = true;
                    }

                    iNext.IsSelected = true;
                    iWrapper = fDict[iNext];
                    iWrapper.Focus();
                }
                else
                {
                    SelectFirst();
                }
            }
            finally
            {
                fRenderingData = false;
            }
        }

        /// <summary>Selects the item that is before the specified data item in the tree. When it is not in
        ///     the viewable area, it is scrolled into view.</summary>
        /// <param name="data">The data.</param>
        public void SelectPrev(ITreeViewPanelItem data)
        {
            ITreeViewPanelItem iPrev;
            TreeViewPanelItem iWrapper;

            if (fDict.TryGetValue(data, out iWrapper))
            {
                var iIndex = InternalChildren.IndexOf(iWrapper);
                if (iIndex > 0)
                {
                    iWrapper = InternalChildren[iIndex - 1] as TreeViewPanelItem;
                    System.Diagnostics.Debug.Assert(iWrapper != null);
                    iPrev = iWrapper.Content as ITreeViewPanelItem;
                    iPrev.IsSelected = true;
                    iWrapper.Focus();
                    return;
                }
            }

            RenderPrev(data);
        }

        /// <summary>The render prev.</summary>
        /// <param name="data">The data.</param>
        private void RenderPrev(ITreeViewPanelItem data)
        {
            ITreeViewPanelItem iPrev;
            TreeViewPanelItem iWrapper;

            fRenderingData = true;

                // we need to prevent the scrollbar from rendering data when we update it, we want to control the rendering ourselves.
            try
            {
                VerticalOffset--;

                    // we are rendering new data from a new top, so move the scrollpos to the new top value to keep it all in sync.
                fTopIndex = GetIndex(data);
                iPrev = GetPrevData();
                if (iPrev != null)
                {
                    fTopIndex = GetIndex(iPrev);
                    RenderChildrenFromTopIndex();
                }
                else
                {
                    ClearChildren(); // we need to go to the top, so render again.
                    RenderChildren(ItemsSource.TreeItems);
                    if (ItemsSource.TreeItems.Count > 0)
                    {
                        iPrev = ItemsSource.TreeItems[0] as ITreeViewPanelItem;
                    }
                    else
                    {
                        iPrev = null;
                    }
                }

                if (iPrev != null)
                {
                    iPrev.IsSelected = true;
                    iWrapper = fDict[iPrev];
                    iWrapper.Focus();
                }
            }
            finally
            {
                fRenderingData = false;
            }
        }

        /// <summary>Gets the last data item in the data list, keeping count of expanded tree nodes.</summary>
        /// <returns>The <see cref="ITreeViewPanelItem"/>.</returns>
        private ITreeViewPanelItem GetLast()
        {
            fBottomIndex.Clear();
            if (ItemsSource != null && ItemsSource.TreeItems != null)
            {
                fBottomIndex.Add(ItemsSource.TreeItems.Count - 1);
                var iRes = ItemsSource.TreeItems[fBottomIndex[0]] as ITreeViewPanelItem;
                while (iRes.HasChildren && iRes.IsExpanded)
                {
                    var iCount = iRes.TreeItems.Count - 1;
                    fBottomIndex.Add(iCount);
                    iRes = iRes.TreeItems[iCount] as ITreeViewPanelItem;
                }

                return iRes;
            }

            return null;
        }

        #region Fields

        /// <summary>The f top index.</summary>
        private System.Collections.Generic.List<int> fTopIndex = new System.Collections.Generic.List<int>();

        /// <summary>The f bottom index.</summary>
        private System.Collections.Generic.List<int> fBottomIndex = new System.Collections.Generic.List<int>();

        /// <summary>The f fallback value.</summary>
        private System.Windows.Controls.TextBlock fFallbackValue;

        /// <summary>The f width.</summary>
        private double fWidth; // the width, required by the objects.

        /// <summary>The f height.</summary>
        private double fHeight; // the height required by the objects.

        /// <summary>The f buffered items.</summary>
        private readonly System.Collections.Generic.Queue<TreeViewPanelItem> fBufferedItems =
            new System.Collections.Generic.Queue<TreeViewPanelItem>();

                                                                             // so we don't have to recreate new objects all the time.

        /// <summary>The f dict.</summary>
        private readonly System.Collections.Generic.Dictionary<ITreeViewPanelItem, TreeViewPanelItem> fDict =
            new System.Collections.Generic.Dictionary<ITreeViewPanelItem, TreeViewPanelItem>();

                                                                                                      // used to quickly find the container for a data item, to check if it is loaded or not.

        /// <summary>The f rendering data.</summary>
        private bool fRenderingData; // to keep accurate count of the nr of items in the tree.

        /// <summary>The f last is visible.</summary>
        private bool fLastIsVisible; // indicates if the first or the last needs to spil over. By default, it's the last

        /// <summary>
        ///     The amount that is added to the MaxWidth, as a visual indicator that it is the last text item.
        /// </summary>
        private const short HORBORDER = 2;

        #endregion

        #region Prop

        #region ItemsSource

        /// <summary>
        ///     ItemsSource Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(ITreeViewRoot), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the ItemsSource property.  This dependency property
        ///     indicates the list to retrieve the data from.
        /// </summary>
        public ITreeViewRoot ItemsSource
        {
            get
            {
                return (ITreeViewRoot)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>Handles changes to the ItemsSource property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemsSourceChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPanel)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ItemsSource property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ClearItemsSource();
            if (e.OldValue != null)
            {
                if (e.NewValue == null && fFallbackValue != null)
                {
                    InternalChildren.Add(fFallbackValue);
                }

                var iColChanged = e.OldValue as ITreeViewRoot;
                System.Diagnostics.Debug.Assert(iColChanged != null);
                Data.EventManagers.CascadedCollectionChangedEventManager.RemoveListener(iColChanged, this);
                Data.EventManagers.CascadedPropertyChangedEventManager.RemoveListener(iColChanged, this);
            }

            if (e.NewValue != null)
            {
                var iColChanged = e.NewValue as ITreeViewRoot;
                System.Diagnostics.Debug.Assert(iColChanged != null);
                Data.EventManagers.CascadedCollectionChangedEventManager.AddListener(iColChanged, this);
                Data.EventManagers.CascadedPropertyChangedEventManager.AddListener(iColChanged, this);
                if (iColChanged.TreeItems != null)
                {
                    RenderChildren(iColChanged.TreeItems);
                }
                else if (fFallbackValue != null)
                {
                    InternalChildren.Add(fFallbackValue); // was removed at the beginning (ClearItems).
                }
            }
        }

        #endregion

        #region FallbackValue

        /// <summary>
        ///     FallbackValue Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty FallbackValueProperty =
            System.Windows.DependencyProperty.Register(
                "FallbackValue", 
                typeof(string), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnFallbackValueChanged));

        /// <summary>
        ///     Gets or sets the FallbackValue property.  This dependency property
        ///     indicates the text to display for when the itemsSource data isn't yet loaded.
        /// </summary>
        public string FallbackValue
        {
            get
            {
                return (string)GetValue(FallbackValueProperty);
            }

            set
            {
                SetValue(FallbackValueProperty, value);
            }
        }

        /// <summary>Handles changes to the FallbackValue property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnFallbackValueChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPanel)d).OnFallbackValueChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the FallbackValue property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnFallbackValueChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && fFallbackValue != null)
            {
                InternalChildren.Remove(fFallbackValue);
                fFallbackValue = null;
            }

            if (InternalChildren.Count == 0 && e.NewValue != null)
            {
                fFallbackValue = new System.Windows.Controls.TextBlock();
                fFallbackValue.Text = (string)e.NewValue;
                InternalChildren.Add(fFallbackValue);
            }
        }

        #endregion

        #region LevelDepth

        /// <summary>
        ///     LevelDepth Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LevelDepthProperty =
            System.Windows.DependencyProperty.Register(
                "LevelDepth", 
                typeof(double), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(
                    19.0, 
                    System.Windows.FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        ///     Gets or sets the LevelDepth property.  This dependency property
        ///     indicates the nr of pixels that each level should move to the right.
        /// </summary>
        public double LevelDepth
        {
            get
            {
                return (double)GetValue(LevelDepthProperty);
            }

            set
            {
                SetValue(LevelDepthProperty, value);
            }
        }

        #endregion

        #region VerViewportSize

        /// <summary>
        ///     VerViewPortSize Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty VerViewportSizeProperty =
            System.Windows.DependencyProperty.Register(
                "VerViewportSize", 
                typeof(double), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0, OnVerViewportSizeChanged));

        /// <summary>
        ///     Gets or sets the VerViewPortSize property.  This dependency property
        ///     indicates the vertical viewportsize, which is equal to the amount of visible items..
        /// </summary>
        public double VerViewportSize
        {
            get
            {
                return (double)GetValue(VerViewportSizeProperty);
            }

            set
            {
                SetValue(VerViewportSizeProperty, value);
            }
        }

        /// <summary>Handles changes to the VerViewPortSize property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnVerViewportSizeChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPanel)d).OnVerViewportSizeChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the VerViewPortSize property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnVerViewportSizeChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            VerLargeChange = System.Math.Max((double)e.NewValue - 2, 2);

            // CoerceValue(VerticalMaxProperty);                  //need to update this, since VerticalMax depends on the value of viewport value.
        }

        #endregion

        #region VerLargeChange

        /// <summary>
        ///     VerLargeChange Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty VerLargeChangeProperty =
            System.Windows.DependencyProperty.Register(
                "VerLargeChange", 
                typeof(double), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the VerLargeChange property.  This dependency property
        ///     indicates the amount to change the vertical scroll value with for a large change.
        /// </summary>
        public double VerLargeChange
        {
            get
            {
                return (double)GetValue(VerLargeChangeProperty);
            }

            set
            {
                SetValue(VerLargeChangeProperty, value);
            }
        }

        #endregion

        #region ItemContainerStyle

        /// <summary>
        ///     ItemContainerStyle Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemContainerStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ItemContainerStyle", 
                typeof(System.Windows.Style), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemContainerStyleChanged));

        /// <summary>
        ///     Gets or sets the ItemContainerStyle property.  This dependency property
        ///     indicates the style to apply to all the wrapper objects.
        /// </summary>
        public System.Windows.Style ItemContainerStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(ItemContainerStyleProperty);
            }

            set
            {
                SetValue(ItemContainerStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the ItemContainerStyle property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemContainerStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPanel)d).OnItemContainerStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ItemContainerStyle property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemContainerStyleChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            foreach (System.Windows.UIElement i in InternalChildren)
            {
                var iWrapper = i as TreeViewPanelItem;
                if (iWrapper != null)
                {
                    iWrapper.Style = (System.Windows.Style)e.NewValue;
                }
            }
        }

        #endregion

        #region ItemTemplate

        /// <summary>
        ///     ItemTemplate Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "ItemTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, System.Windows.FrameworkPropertyMetadataOptions.None));

        /// <summary>
        ///     Gets or sets the ItemTemplate property.  This dependency property
        ///     indicates the template to use for the data items.
        /// </summary>
        public System.Windows.DataTemplate ItemTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(ItemTemplateProperty);
            }

            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }

        #endregion

        #region ItemTemplateSelector

        /// <summary>
        ///     ItemTemplateSelector Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemTemplateSelectorProperty =
            System.Windows.DependencyProperty.Register(
                "ItemTemplateSelector", 
                typeof(System.Windows.Controls.DataTemplateSelector), 
                typeof(TreeViewPanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.Controls.DataTemplateSelector)null));

        /// <summary>
        ///     Gets or sets the ItemTemplateSelector property.  This dependency property
        ///     indicates the template selector to use for the children.
        /// </summary>
        public System.Windows.Controls.DataTemplateSelector ItemTemplateSelector
        {
            get
            {
                return (System.Windows.Controls.DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
            }

            set
            {
                SetValue(ItemTemplateSelectorProperty, value);
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown"/> attached event reaches an element
        ///     in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event
        ///     data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource == this)
            {
                Focus();
            }

            base.OnMouseDown(e);
        }

        /// <summary>Coerces the horizontal max value. Simply returns the value by default.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected override object CoerceHorizontalMaxValue(object value)
        {
            if ((double)value != 0)
            {
                return (double)value + HORBORDER;

                    // we add a little space so that the biggest item isn't exactly at the border, but there is some extra space available to indicate it's the end.
            }

            return (double)0;
        }

        /// <summary>Coerces the horizontal offset value. By default, simply returns the value (doesn't check if within min and max),
        ///     so that the scroll area can auto expand.</summary>
        /// <remarks>We need to make certain that it is a round number (not a decimal), otherwise, we might loose values (the part after
        ///     the comma), which might cause
        ///     strange behaviour.</remarks>
        /// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged"/> event, using the specified information as
        ///     part of
        ///     the eventual event data.</summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        /// <remarks>When the size has changed, we need to update the horizontal and vertical max scroll values.</remarks>
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            if (ItemsSource != null && sizeInfo.HeightChanged)
            {
                if (sizeInfo.PreviousSize.Height == 0 && sizeInfo.PreviousSize.Width == 0 && InternalChildren.Count == 0)
                {
                    // first time draw, render children.
                    RenderChildren(ItemsSource.TreeItems);
                }
                else
                {
                    var iHeightChange = sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height;
                    if (iHeightChange > 0)
                    {
                        // we only try to repaint if we have more room available. otherwise it's not worth the redraw.
                        if (InternalChildren.Count != 1 || InternalChildren[0] != fFallbackValue)
                        {
                            // when we are displaying the fallback value, we don't want to clear the list of children.
                            RenderChildrenFromTopIndex(); // when the size has changed, we need to recreate the objects.
                        }
                    }
                }
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseWheel"/> attached event reaches an element
        ///     in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
        /// <remarks>When control is pressed-&gt; zoom, otherwise scroll.</remarks>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            VerticalOffset -= e.Delta / 60.0;
            base.OnMouseWheel(e);
        }

        /// <summary>When overridden in a derived class, measures the size in layout required for child elements and determines a size
        ///     for the <see cref="T:System.Windows.FrameworkElement"/>-derived class.</summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified
        ///     as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var iLevelDepth = LevelDepth; // local copy is faster in loop.
            fWidth = 0;
            var iMax = new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (System.Windows.FrameworkElement i in InternalChildren)
            {
                i.Measure(iMax);
                var iItem = i as TreeViewPanelItem;
                if (iItem != null)
                {
                    fWidth = System.Math.Max(fWidth, i.DesiredSize.Width + (iItem.Level * iLevelDepth));
                }
                else
                {
                    fWidth = System.Math.Max(fWidth, i.DesiredSize.Width);
                }
            }

            CalculateScrollValues();

            // FrameworkElement iParent = (FrameworkElement)Parent;                          //if we are the child of an adornerDecorator, we don't get the correct size constraint. So we  make certain that we always get this from the parent.
            // if (iParent != null)                                                          //a codePAgePanel is put in an AdornerDecorator, to give it's own (clipped) adorner layer. This has a strange measure/arrange which we must handle in such a way that we always use all the available space.
            // return new Size(iParent.ActualWidth, iParent.ActualHeight);
            // else
            return availableSize;
        }

        /// <summary>When overridden in a derived class, positions child elements and determines a size for a<see cref="T:System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its
        ///     children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iLevelDepth = LevelDepth; // local copy is faster in loop.
            System.Windows.Point iLoc;
            if (fLastIsVisible == false)
            {
                iLoc = new System.Windows.Point(-HorizontalOffset, 0);
            }
            else
            {
                var iVal = finalSize.Height - fHeight;
                if (iVal < 0)
                {
                    iLoc = new System.Windows.Point(-HorizontalOffset, finalSize.Height - fHeight);

                        // this is to display the last item which is a bit outside of the range, so we move the whole render area a bit up. We don't wan't it to be bigger then 0, cause then we don't render at the top.
                }
                else
                {
                    iLoc = new System.Windows.Point(-HorizontalOffset, 0);
                }
            }

            foreach (System.Windows.FrameworkElement i in InternalChildren)
            {
                double iLevelOffset = 0;
                var iItem = i as TreeViewPanelItem;
                if (iItem != null)
                {
                    iLevelOffset = iItem.Level * iLevelDepth;
                    iLoc.Offset(iLevelOffset, 0);
                }

                var iRect = new System.Windows.Rect(iLoc, i.DesiredSize);
                i.Arrange(iRect);
                iLoc.Offset(-iLevelOffset, iRect.Height);

                    // we need to reset the level offset cause each item sets it's own level, height needs to be accumulated.
            }

            return finalSize;
        }

        /// <summary>
        ///     Calculates the max scroll values and assigns them to the properties. This is abstract cause it depends on
        ///     the implementation.
        /// </summary>
        protected internal override void CalculateScrollValues()
        {
            var iVal = fWidth - ActualWidth;
            if ((iVal + HORBORDER) != HorizontalMax)
            {
                if (iVal < 0)
                {
                    iVal = 0;
                }

                SetHorizontalMax(iVal);
            }

            VerViewportSize = InternalChildren.Count;
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the HorizontalOffset property.</summary>
        /// <param name="e"></param>
        protected override void OnHorizontalOffsetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // don't do anything: we only allow to scroll horizontally over the already existing content.
        }

        /// <summary>The f scroll rest.</summary>
        private double fScrollRest;

        /// <summary>Provides derived classes an opportunity to handle changes to the VerticalOffset property.</summary>
        /// <remarks>When we scroll, we need to update the list of visual items.</remarks>
        /// <param name="e"></param>
        protected override void OnVerticalOffsetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fRenderingData == false)
            {
                // sometimes, when we are rendering data, we need to update the scrollpos, which souldn't change a new data rendering stage, hence the switch.
                var iNew = (double)e.NewValue;
                var iOld = (double)e.OldValue;

                var iAmount = iNew - iOld + fScrollRest;
                var iRoundAmount = (long)iAmount;
                fScrollRest = iAmount - iRoundAmount;
                iAmount = iRoundAmount;

                if (iNew < iOld)
                {
                    if (iNew == 0)
                    {
                        // we go to start, take shortcut.
                        ClearChildren();
                        if (ItemsSource != null)
                        {
                            RenderChildren(ItemsSource.TreeItems);
                        }
                    }
                    else
                    {
                        ScrollBack(System.Math.Abs(iAmount));
                    }
                }
                else if (iNew == VerticalMax)
                {
                    fLastIsVisible = true;
                    var iData = GetLast();
                    RenderChildrenUntil(iData);
                }
                else
                {
                    ScrollForward(iAmount);
                }
            }
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>true if the listener handled the event. It is considered an error by the<see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the
        ///     listener does not handle. Regardless, the method should return false if it receives an event that it does not
        ///     recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(Data.EventManagers.CascadedCollectionChangedEventManager))
            {
                CollectionChanged((Data.CascadedCollectionChangedEventArgs)e);
                return true;
            }

            if (managerType == typeof(Data.EventManagers.CascadedPropertyChangedEventManager))
            {
                PropertyChanged((Data.CascadedPropertyChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Called when a property is about to change on a treenode.
        ///     Used to check if any of the valid (interesting for treeview) properties is changed.</summary>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.PropertyChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void PropertyChanged(Data.CascadedPropertyChangedEventArgs e)
        {
            var iData = e.OriginalSource as ITreeViewPanelItem;
            if (iData != null)
            {
                if (e.Args.PropertyName == "IsExpanded")
                {
                    // update itemcount 
                    UpdateAferExpand(iData);
                }
                else if (e.Args.PropertyName == "NeedsBringIntoView")
                {
                    // need to bring it into view.
                    if (iData.NeedsBringIntoView)
                    {
                        iData.NeedsBringIntoView = false;
                        var iFlatIndex = GetFlatIndex(iData);
                        if (iFlatIndex < InternalChildren.Count - 1)
                        {
                            GotoFirst();
                        }
                        else if (iFlatIndex >= VerticalMax - VerViewportSize)
                        {
                            GotoLast();
                        }
                        else
                        {
                            GotoPos(iFlatIndex + 1, iData);
                        }
                    }
                }
                else if (e.Args.PropertyName == "TreeItems")
                {
                    // a sub list was added or removed.
                    Dispatcher.BeginInvoke(new System.Action(RenderChildrenFromTopIndex));

                        // try to rerender the visual items to make certain that the UI is updated and the removed items are no longer visible.
                }
            }
            else if (e.Args.PropertyName == "TreeItems")
            {
                // it's comming from the root.
                Dispatcher.BeginInvoke(new System.Action(TreeItemsChanged));

                    // needs to be done async cause the items might be loaded from a different thread and we need to update the ui when responding.
            }
        }

        /// <summary>The tree items changed.</summary>
        private void TreeItemsChanged()
        {
            var iRoot = ItemsSource;
            if (iRoot != null)
            {
                // could be null if the project has been unloaded just after the treeItems list was reset to null.
                ClearItemsSource();
                if (iRoot.TreeItems == null)
                {
                    if (fFallbackValue != null)
                    {
                        InternalChildren.Add(fFallbackValue);
                    }
                }
                else
                {
                    RenderChildren(iRoot.TreeItems);
                }
            }
        }

        /// <summary>The goto pos.</summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        private void GotoPos(double index, ITreeViewPanelItem data)
        {
            var iScollVal = System.Math.Max(index - InternalChildren.Count + 1, 0);

                // we + 1, to compensate: index (0 based) versus count (1 based)
            fRenderingData = true;
            try
            {
                VerticalOffset = iScollVal;
                fBottomIndex = GetIndex(data);
                data = GetNextData(data);

                    // we go 1 further, so that we make certain that the selected item is completly visible (the last is used as filler for the space that can't take a whole item )
                if (data != null)
                {
                    RenderChildrenUntil(data);
                    if (IsLast(fBottomIndex))
                    {
                        fLastIsVisible = true;
                    }
                }
            }
            finally
            {
                fRenderingData = false;
            }
        }

        /// <summary>Calculates the index position of the object as it would appear on the scrollbar.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private double GetFlatIndex(ITreeViewPanelItem data)
        {
            double iRes = 0;
            int iIndex;
            while (data.ParentTreeItem != null)
            {
                iIndex = data.ParentTreeItem.TreeItems.IndexOf(data);
                iRes += iIndex + 1;
                data = data.ParentTreeItem;
                for (var i = 0; i < iIndex; i++)
                {
                    var iPrev = data.TreeItems[i] as ITreeViewPanelItem;
                    if (iPrev.IsExpanded && iPrev.HasChildren)
                    {
                        // all prev items that were expanded: we also need to add their chid count to the index.
                        iRes += CountNrOfTreeItems(iPrev.TreeItems) + iPrev.TreeItems.Count;
                    }
                }
            }

            iIndex = ItemsSource.TreeItems.IndexOf(data);
            iRes += iIndex;
            for (var i = 0; i < iIndex; i++)
            {
                var iPrev = ItemsSource.TreeItems[i] as ITreeViewPanelItem;
                if (iPrev.IsExpanded && iPrev.HasChildren)
                {
                    // all prev items that were expanded: we also need to add their chid count to the index.
                    iRes += CountNrOfTreeItems(iPrev.TreeItems) + iPrev.TreeItems.Count;
                }
            }

            return iRes;
        }

        /// <summary>Updates the UI elements afer an expand.</summary>
        /// <remarks>Need the treeITems list as extra argument, cause for a collaps, it is already removed when this
        ///     function is processed.</remarks>
        /// <param name="data">The data.</param>
        private void UpdateAferExpand(ITreeViewPanelItem data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            var iTreeItems = data.TreeItems;
            double iTotalChange = 0;
            if (data.HasChildren && data.IsExpanded)
            {
                iTotalChange += GetExpandCount(data);
            }
            else
            {
                iTotalChange -= CountNrOfTreeItems(iTreeItems) + iTreeItems.Count;

                    // can't use GetExpandCount, cause items have already been removed from the list after the collaps, so the iData object no longer has correct references.
            }

            if (fDict.ContainsKey(data))
            {
                // when expanding/collapsing a visual item, rerender all UI items.
                RenderChildrenFromTopIndex(); // do before setting visibility, saves us a bunch of visible/hide switches.
            }

            double iVal;
            if (iTotalChange > 0)
            {
                if (VerticalMax > 0)
                {
                    iVal = VerticalMax + iTotalChange;
                }
                else
                {
                    iVal = ItemsSource.TreeItems.Count + CountNrOfTreeItems(ItemsSource.TreeItems)
                           - InternalChildren.Count;
                }
            }
            else
            {
                if (VerticalMax > 0)
                {
                    iVal = ItemsSource.TreeItems.Count + CountNrOfTreeItems(ItemsSource.TreeItems)
                           - InternalChildren.Count;
                }
                else
                {
                    iVal = 0;
                }
            }

            SetVerticalMax(iVal);
            var iFlatIndex = GetFlatIndex(data);
            if (iFlatIndex < VerticalOffset)
            {
                // if the expand happened before the current screen, we need to update hte current scrollpos.
                VerticalOffset += iTotalChange;
            }
        }

        /// <summary>Get the total nr of children that are expanded in the data item. The direct children of the data item
        ///     are always included.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong GetExpandCount(ITreeViewPanelItem data)
        {
            if (data.TreeItems != null && data.HasChildren && data.IsExpanded)
            {
                var iCount = (ulong)data.TreeItems.Count;
                foreach (ITreeViewPanelItem i in data.TreeItems)
                {
                    System.Diagnostics.Debug.Assert(i != null);
                    iCount += GetExpandCount(i);
                }

                return iCount;
            }

            return 0;
        }

        /// <summary>Called when a chid collection is about to change. We use this to update the scroll count. We also use this
        ///     to check if any of the loaded items was involved, or currently within the scrollview.</summary>
        /// <param name="e">The <see cref="JaStDev.UndoSystem.Interfaces.CollectionChangingEventArgs"/> instance containing the
        ///     event data.</param>
        private void CollectionChanged(Data.CascadedCollectionChangedEventArgs e)
        {
            switch (e.Args.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    CheckIncVerticalMax(e);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    CheckVisualMove(e);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    CheckDecVerticalMax(e);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    CheckVisualReplace(e);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    CheckResetVerticalMax(e);
                    foreach (ITreeViewPanelItem i in (System.Collections.IList)e.OriginalSource)
                    {
                        if (CheckVisualRemove(i))
                        {
                            // only need to try and repaint the screen 1 time.
                            break;
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>The check reset vertical max.</summary>
        /// <param name="e">The e.</param>
        private void CheckResetVerticalMax(Data.CascadedCollectionChangedEventArgs e)
        {
            if (fRenderingData == false)
            {
                var iOrSource = e.OriginalSource as Data.IOwnedObject;
                if (iOrSource != null)
                {
                    var iParent = iOrSource.Owner as ITreeViewPanelItem;
                    if (iParent == null && InternalChildren.Count > 0 && InternalChildren[0] != fFallbackValue)
                    {
                        // when no parent, we found root, when root reset: check that we aren't already displaying the fallback.
                        if (System.Threading.Thread.CurrentThread != Dispatcher.Thread)
                        {
                            Dispatcher.BeginInvoke(new System.Action(ClearChildren)); // wpf: needs to be in UI thread.
                        }
                        else
                        {
                            ClearChildren();
                        }
                    }
                    else if (iParent != null && iParent.HasChildren && iParent.IsExpanded)
                    {
                        var iOrList = e.OriginalSource as System.Collections.IList;
                        if (System.Threading.Thread.CurrentThread != Dispatcher.Thread)
                        {
                            Dispatcher.BeginInvoke(
                                new System.Action<int>(RemoveVerticalMax), 
                                iOrList.Count + CountNrOfTreeItems(iOrList)); // wpf: needs to be in UI thread.
                        }
                        else
                        {
                            RemoveVerticalMax(iOrList.Count + (int)CountNrOfTreeItems(iOrList));
                        }
                    }
                }
            }
        }

        /// <summary>The check dec vertical max.</summary>
        /// <param name="e">The e.</param>
        private void CheckDecVerticalMax(Data.CascadedCollectionChangedEventArgs e)
        {
            if (fRenderingData == false)
            {
                var iOrSource = e.OriginalSource as Data.IOwnedObject;
                if (iOrSource != null)
                {
                    var iParent = iOrSource.Owner as ITreeViewPanelItem;
                    if (iParent == null || iParent.HasChildren == false || iParent.IsExpanded)
                    {
                        // we only increment for child lists that belong to expanded items, otherwise, we can't allow an increase. The root list always needs to monitor changes.
                        var iOld = e.Args.OldItems[0] as ITreeViewPanelItem;
                        if (iOld != null)
                        {
                            if (iOld.HasChildren == false || iOld.IsExpanded == false)
                            {
                                Dispatcher.BeginInvoke(new System.Action<int>(RemoveVerticalMax), 1);

                                    // wpf: needs to be in UI thread.
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(
                                    new System.Action<int>(RemoveVerticalMax), 
                                    1 + CountNrOfTreeItems(iOld.TreeItems));
                            }
                        }

                        CheckVisualRemove(e.Args.OldItems[0] as ITreeViewPanelItem);
                    }
                }
            }
        }

        /// <summary>The check inc vertical max.</summary>
        /// <param name="e">The e.</param>
        private void CheckIncVerticalMax(Data.CascadedCollectionChangedEventArgs e)
        {
            if (fRenderingData == false)
            {
                var iOrSource = e.OriginalSource as Data.IOwnedObject;
                if (iOrSource != null && ItemsSource != null && ItemsSource.TreeItems != null)
                {
                    var iParent = iOrSource.Owner as ITreeViewPanelItem;
                    if (iParent == null || (iParent.HasChildren && iParent.IsExpanded))
                    {
                        // we only increment for child lists that belong to expanded items, otherwise, we can't allow an increase. The root list always needs to monitor changes.
                        var iVal = ItemsSource.TreeItems.Count + CountNrOfTreeItems(ItemsSource.TreeItems)
                                   - InternalChildren.Count;

                            // this needs to be done in the same thread, otherwise we can get Enumeration exceptions due to changing list.
                        Dispatcher.BeginInvoke(
                            new System.Action<Data.CascadedCollectionChangedEventArgs, double>(CheckVisualInsert), 
                            e, 
                            iVal);
                    }
                }
            }
        }

        /// <summary>The remove vertical max.</summary>
        /// <param name="value">The value.</param>
        private void RemoveVerticalMax(int value)
        {
            var iRoot = ItemsSource;
            double iVal;
            if (iRoot != null)
            {
                iVal = VerticalMax;
                if (iVal == 0)
                {
                    iVal = iRoot.TreeItems.Count + CountNrOfTreeItems(iRoot.TreeItems) - InternalChildren.Count - value;
                }
                else
                {
                    iVal -= value;
                }
            }
            else
            {
                iVal = 0;
            }

            SetVerticalMax(iVal);
        }

        /// <summary>The check visual replace.</summary>
        /// <param name="e">The e.</param>
        private void CheckVisualReplace(Data.CascadedCollectionChangedEventArgs e)
        {
            var iData = e.Args.OldItems[0] as ITreeViewPanelItem;
            TreeViewPanelItem iWrapper;
            if (iData != null && fDict.TryGetValue(iData, out iWrapper))
            {
                Dispatcher.BeginInvoke(
                    new System.Action<TreeViewPanelItem, object>(ReplaceContent), 
                    iWrapper, 
                    e.Args.NewItems[0]);
            }
        }

        /// <summary>The replace content.</summary>
        /// <param name="wrapper">The wrapper.</param>
        /// <param name="data">The data.</param>
        private void ReplaceContent(TreeViewPanelItem wrapper, object data)
        {
            wrapper.Content = data;
        }

        /// <summary>The check visual remove.</summary>
        /// <param name="iData">The i data.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckVisualRemove(ITreeViewPanelItem iData)
        {
            TreeViewPanelItem iWrapper;
            if (iData != null && (fDict.TryGetValue(iData, out iWrapper) || (iData.HasChildren && iData.IsExpanded)))
            {
                // when the item is expanded, we don't know how many items get removed, so always do a refresh.
                Dispatcher.BeginInvoke(new System.Action(RenderChildrenFromTopIndex));
                return true;
            }

            return false;
        }

        /// <summary>The check visual move.</summary>
        /// <param name="e">The e.</param>
        private void CheckVisualMove(Data.CascadedCollectionChangedEventArgs e)
        {
            var iItem = e.Args.OldItems[0] as ITreeViewPanelItem;

            // if the item or it's parent are visible, redraw the treeview.
            if (fDict.ContainsKey(iItem))
            {
                Dispatcher.BeginInvoke(new System.Action(RenderChildrenFromTopIndex));

                    // if the moved item is a child of a visual item , we simply draw from the top again. It could be that it's not included, but we don't yet know this.
            }
            else
            {
                var iIndex = GetIndex(e.Args.NewItems[0] as ITreeViewPanelItem);

                    // if the new position is within viewing range, also render from top.
                if (IsIndexInRange(iIndex))
                {
                    Dispatcher.BeginInvoke(new System.Action(RenderChildrenFromTopIndex));
                }
            }
        }

        /// <summary>The check visual insert.</summary>
        /// <param name="e">The e.</param>
        /// <param name="val">The val.</param>
        private void CheckVisualInsert(Data.CascadedCollectionChangedEventArgs e, double val)
        {
            var iNew = e.Args.NewItems[0] as ITreeViewPanelItem;
            if (iNew != null)
            {
                var iIndex = GetIndex(iNew);
                if (IsIndexInRange(iIndex))
                {
                    RenderChildrenFromTopIndex();

                        // if the newly inserted item is a child of a visual item (that is expanded), we simply draw from the top again. It could be that it's not included, but we don't yet know this.
                }

                SetVerticalMax(val);
            }
        }

        /// <summary>The is index in range.</summary>
        /// <param name="toCheck">The to check.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsIndexInRange(System.Collections.Generic.List<int> toCheck)
        {
            long iTotalIndex = 0;
            for (var i = 0; i < toCheck.Count; i++)
            {
                var iToCheck = toCheck[i];
                if (i >= fTopIndex.Count)
                {
                    var iRemainer = 0;
                    for (var u = i; u < toCheck.Count; u++)
                    {
                        iRemainer += toCheck[u];
                    }

                    return iTotalIndex + iRemainer <= ((long)VerticalOffset + InternalChildren.Count);
                }

                if (i >= fBottomIndex.Count)
                {
                    return fBottomIndex.Count == 0;

                        // if the bottom index doesn't exand as far, then it is below it, but if there is no bottom index, there is not data yet visible, so anything inserted should be made visible.
                }

                if (iToCheck < fTopIndex[i] || iToCheck > fBottomIndex[i])
                {
                    return false;
                }

                iTotalIndex += toCheck[i];
            }

            return true;
        }

        /// <summary>Gets the list of indexes of the specified item in the tree.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<int> GetIndex(ITreeViewPanelItem item)
        {
            var iRes = new System.Collections.Generic.List<int>();
            var iOwner = item.ParentTreeItem;
            while (iOwner != null && iOwner.TreeItems != null)
            {
                // treeItems could be null if the whole thing already was collapsed. This can happen during import of a thesaurus from xml.
                iRes.Insert(0, iOwner.TreeItems.IndexOf(item));
                item = iOwner;
                iOwner = item.ParentTreeItem;
            }

            iRes.Insert(0, ItemsSource.TreeItems.IndexOf(item));
            return iRes;
        }

        #endregion

        #region data rendering

        /// <summary>The render children.</summary>
        /// <param name="list">The list.</param>
        private void RenderChildren(System.Collections.IList list)
        {
            if (IsLoaded && IsVisible)
            {
                // if not yet loaded, can't render items yet since don't yet know the exact size.
                if (list != null)
                {
                    fRenderingData = true;
                    try
                    {
                        VerViewportSize = 0;
                        fTopIndex.Clear();
                        fBottomIndex.Clear();
                        fTopIndex.Add(0);
                        double iHeight = 0;
                        RenderNodes(list, 0, ref iHeight);
                        var iVal = ItemsSource.TreeItems.Count + CountNrOfTreeItems(ItemsSource.TreeItems)
                                   - InternalChildren.Count;
                        SetVerticalMax(iVal);
                    }
                    finally
                    {
                        fRenderingData = false;
                    }
                }
            }
            else
            {
                Loaded += TreeViewPanel_Loaded;
            }
        }

        /// <summary>The count nr of tree items.</summary>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private double CountNrOfTreeItems(System.Collections.IEnumerable items)
        {
            double iVal = 0;
            if (items != null)
            {
                foreach (ITreeViewPanelItem i in items)
                {
                    if (i.IsExpanded && i.HasChildren && i.TreeItems != null)
                    {
                        // i.TreeItems != null is an extra check, there is nothing requiring it to not be null, so check this.
                        iVal += i.TreeItems.Count + CountNrOfTreeItems(i.TreeItems);
                    }
                }
            }

            return iVal;
        }

        /// <summary>The render nodes.</summary>
        /// <param name="list">The list.</param>
        /// <param name="level">The level.</param>
        /// <param name="usedHeight">The used height.</param>
        private void RenderNodes(System.Collections.IList list, int level, ref double usedHeight)
        {
            var iActualHeight = ActualHeight;
            for (var counter = 0; counter < list.Count; counter++)
            {
                var i = list[counter] as ITreeViewPanelItem;
                System.Diagnostics.Debug.Assert(i != null);
                var iWrapper = GetWrapper(i, level);
                InternalChildren.Add(iWrapper);
                iWrapper.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

                    // we measuse, so we can see if any other will still fit.
                usedHeight += iWrapper.DesiredSize.Height;
                if (usedHeight > iActualHeight)
                {
                    fBottomIndex.Insert(0, counter);

                        // we need to reverse the insert, cause the first index needs to be of the top list, but we are called in reverse order, so insert at the top to get the last item first.
                    break;
                }

                if (i.HasChildren && i.IsExpanded)
                {
                    RenderNodes(i.TreeItems, level + 1, ref usedHeight);
                    if (usedHeight > iActualHeight)
                    {
                        fBottomIndex.Insert(0, counter);
                        break;
                    }
                }
            }
        }

        /// <summary>Generates a wrapper ovject for the  specified data. Doesn't yet add or measure the item.</summary>
        /// <remarks>When a wrapper is requested, the viewportsize is increased.</remarks>
        /// <param name="data">The data.</param>
        /// <param name="level">The level.</param>
        /// <returns>The <see cref="TreeViewPanelItem"/>.</returns>
        private TreeViewPanelItem GetWrapper(ITreeViewPanelItem data, int level)
        {
            TreeViewPanelItem iWrapper = null;
            if (data != null)
            {
                if (fBufferedItems.Count == 0)
                {
                    iWrapper = CreateContainer(data);
                }
                else
                {
                    iWrapper = fBufferedItems.Dequeue();
                    if (iWrapper.DataContext != data)
                    {
                        iWrapper.Style = null;

                            // if we reuse the item, we reset the style. We do this, cause otherwise, the 'togglebutton' doesn't get reset properly.
                    }
                }

                iWrapper.Level = level;
                iWrapper.DataContext = data;
                iWrapper.Content = data;
                var iTemplate = ItemTemplate;
                if (ItemTemplate != null)
                {
                    iWrapper.ContentTemplate = ItemTemplate;
                }
                else
                {
                    var iSelector = ItemTemplateSelector;
                    if (iSelector != null)
                    {
                        iWrapper.ContentTemplateSelector = iSelector;
                    }
                }

                iWrapper.Style = ItemContainerStyle;
                fDict[data] = iWrapper; // so we can find it quickly in event handlers.
            }

            return iWrapper;
        }

        /// <summary>Creates a new container. Overwite this if you want custom containers for the objects.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="TreeViewPanelItem"/>.</returns>
        protected virtual TreeViewPanelItem CreateContainer(ITreeViewPanelItem data)
        {
            return new TreeViewPanelItem();
        }

        /// <summary>Scrolls forward by the specified amount by removing this amount of items from the front and adding them
        ///     to the back.</summary>
        /// <param name="amount">The amount.</param>
        private void ScrollForward(double amount)
        {
            fRenderingData = true;
            try
            {
                var iData = GoForward(amount);
                if (iData != null)
                {
                    RenderChildrenUntil(iData);
                    if (IsLast(fBottomIndex))
                    {
                        fLastIsVisible = true;
                    }
                }
                else
                {
                    fLastIsVisible = true;
                    iData = GetLast();
                    RenderChildrenUntil(iData);
                }
            }
            finally
            {
                fRenderingData = false;
            }
        }

        /// <summary>Determines whether the specified index references the last item in the tree.</summary>
        /// <param name="list">The list.</param>
        /// <returns><c>true</c> if the specified index is last; otherwise, <c>false</c>.</returns>
        private bool IsLast(System.Collections.Generic.List<int> list)
        {
            var iSource = ItemsSource.TreeItems;
            foreach (var i in list)
            {
                if (iSource == null || i < iSource.Count - 1)
                {
                    return false;
                }

                var iItem = iSource[i] as ITreeViewPanelItem;
                iSource = iItem.TreeItems;
            }

            return true;
        }

        /// <summary>Scrolls back by the specified amount by removing items from the back and adding them to the front
        ///     by the specified amount.</summary>
        /// <param name="amount">The p.</param>
        private void ScrollBack(double amount)
        {
            if (fLastIsVisible == false)
            {
                fRenderingData = true;
                try
                {
                    if (GoBack(amount))
                    {
                        RenderChildrenFromTopIndex();
                    }
                    else
                    {
                        ClearChildren(); // we need to go to the top, so render again.
                        RenderChildren(ItemsSource.TreeItems);
                    }
                }
                finally
                {
                    fRenderingData = false;
                }
            }
            else
            {
                fLastIsVisible = false;
                InvalidateArrange();
            }
        }

        /// <summary>Gets the item (seen from the top of the treeview) that lies before the current topmost item by the specified
        ///     amount.</summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GoBack(double amount)
        {
            var i = fTopIndex.Count - 1;
            var iFirst = GetItemAt(fTopIndex);

            if (iFirst != null)
            {
                while (amount > 0 && fTopIndex.Count > 0)
                {
                    var iParent = iFirst.ParentTreeItem;
                    var iParentList = (iParent != null) ? iParent.TreeItems : ItemsSource.TreeItems;
                    if (fTopIndex[i] > 0)
                    {
                        fTopIndex[i]--;
                        amount -= 1;
                        iFirst = iParentList[fTopIndex[i]] as ITreeViewPanelItem;
                        while (iFirst.HasChildren && iFirst.IsExpanded)
                        {
                            fTopIndex.Add(iFirst.TreeItems.Count - 1);
                            iFirst = iFirst.TreeItems[iFirst.TreeItems.Count - 1] as ITreeViewPanelItem;
                            i++;
                        }
                    }
                    else
                    {
                        fTopIndex.RemoveAt(i);
                        i--;
                        amount -= 1;
                        iFirst = iFirst.ParentTreeItem;
                    }
                }
            }

            return fTopIndex.Count > 0;
        }

        /// <summary>The go forward.</summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The <see cref="ITreeViewPanelItem"/>.</returns>
        private ITreeViewPanelItem GoForward(double amount)
        {
            var iLast = GetItemAt(fBottomIndex);
            var i = fBottomIndex.Count - 1;
            while (amount > 0 && fBottomIndex.Count > 0 && iLast != null)
            {
                var iParent = iLast.ParentTreeItem;
                var iList = iParent != null ? iParent.TreeItems : ItemsSource.TreeItems;
                if (fBottomIndex[i] < iList.Count - 1
                    || (fBottomIndex[i] == iList.Count - 1 && iLast.IsExpanded && iLast.HasChildren))
                {
                    // ITreeViewPanelItem iChild = iList[fBottomIndex[i]] as ITreeViewPanelItem;
                    if (iLast.IsExpanded && iLast.HasChildren)
                    {
                        fBottomIndex.Add(0);
                        iLast = iLast.TreeItems[0] as ITreeViewPanelItem;
                        i++;
                    }
                    else
                    {
                        fBottomIndex[i]++;
                        iLast = iList[fBottomIndex[i]] as ITreeViewPanelItem;
                    }
                }
                else
                {
                    while (fBottomIndex.Count > 0 && fBottomIndex[i] >= iList.Count - 1)
                    {
                        fBottomIndex.RemoveAt(i);
                        i--;
                        iLast = iParent;
                        if (iLast != null)
                        {
                            // reached the end.
                            iParent = iLast.ParentTreeItem;
                            iList = iParent != null ? iParent.TreeItems : ItemsSource.TreeItems;
                        }
                        else
                        {
                            iList = ItemsSource.TreeItems;
                        }
                    }

                    if (i >= 0)
                    {
                        // we have found an item that is not the very last in the list, so we can advance.
                        fBottomIndex[i]++;
                        iLast = iList[fBottomIndex[i]] as ITreeViewPanelItem;
                    }
                }

                amount -= 1;
            }

            if (fBottomIndex.Count > 0)
            {
                return iLast;
            }

            return null;
        }

        /// <summary>Renders all the visual children in such a way that the specified item is the last, so the top index
        ///     gets calculated by getting the previous item until the screen is full or there are no more items left.</summary>
        /// <param name="last">The last.</param>
        private void RenderChildrenUntil(ITreeViewPanelItem last)
        {
            ClearChildren();

            if (ItemsSource != null)
            {
                var iWrapper = GetWrapper(last, fBottomIndex.Count - 1);
                InternalChildren.Add(iWrapper);
                iWrapper.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

                    // we measuse, so we can see if any other will still fit.
                fHeight = iWrapper.DesiredSize.Height;
                var iActualHeight = ActualHeight;
                fTopIndex = new System.Collections.Generic.List<int>(fBottomIndex);

                    // when we start, the top is the same as the bottom, we bring this top further above until panel area is filled.
                var iWidth = ActualWidth;
                while (fHeight < iActualHeight)
                {
                    var iPrev = GetPrevData();
                    if (iPrev != null)
                    {
                        // we use this 'if' instead of moving the condition in the loop, this way, the top cursor remains correct, otherewise, we move 1 to far to the top.
                        iWrapper = GetWrapper(iPrev, fTopIndex.Count - 1);
                        InternalChildren.Insert(0, iWrapper);
                        iWrapper.Measure(new System.Windows.Size(double.PositiveInfinity, iWidth));

                            // we measuse, so we can see if any other will still fit.
                        fHeight += iWrapper.DesiredSize.Height;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>Moves the top cursor on back and returns the data objects.</summary>
        /// <returns>The <see cref="ITreeViewPanelItem"/>.</returns>
        private ITreeViewPanelItem GetPrevData()
        {
            var iTop = fTopIndex.Count - 1;
            if (fTopIndex.Count > 0 && fTopIndex[iTop] == 0)
            {
                // when the last index is 0, remove it and return the previous item on the stack, if there still is any. we do this cause an exanded item alwasy first returns it's children, so when all of it's children are handled, we can handle the item itself.
                iTop--;
                if (iTop >= 0)
                {
                    fTopIndex.RemoveAt(iTop + 1);

                        // only remove the top index if there still is one, otherwise we screw up for the next time (if we insert an item and there is no top index anymore, it will render incorrectly), 
                    return GetItemAt(fTopIndex);
                }

                return null;
            }

            if (fTopIndex.Count > 0)
            {
                fTopIndex[iTop]--;
                var iData = GetItemAt(fTopIndex);
                while (iData.IsExpanded && iData.HasChildren)
                {
                    var iIndex = iData.TreeItems.Count - 1;
                    fTopIndex.Add(iIndex);
                    iData = iData.TreeItems[iIndex] as ITreeViewPanelItem;
                }

                return iData;
            }

            return null;
        }

        /// <summary>
        ///     Renders the children from the specified item, which becomes the item at the top.
        /// </summary>
        /// <param name="data">The data.</param>
        private void RenderChildrenFromTopIndex()
        {
            ClearChildren();
            if (ItemsSource != null && ItemsSource.TreeItems != null)
            {
                ITreeViewPanelItem first;
                if (fTopIndex.Count > 0 && fTopIndex[0] < ItemsSource.TreeItems.Count)
                {
                    first = GetItemAt(fTopIndex);
                }
                else
                {
                    first = null;
                }

                if (first != null)
                {
                    var iWrapper = GetWrapper(first, fTopIndex.Count - 1);
                    InternalChildren.Add(iWrapper);
                    iWrapper.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

                        // we measuse, so we can see if any other will still fit.
                    fHeight = iWrapper.DesiredSize.Height;
                    fBottomIndex = new System.Collections.Generic.List<int>(fTopIndex);

                        // when we start, the bottom is the same as the top, we bring this bottom further down until panel area is filled.
                    FillTilEnd();
                }
            }
        }

        /// <summary>The fill til end.</summary>
        private void FillTilEnd()
        {
            var iActualHeight = ActualHeight;
            ITreeViewPanelItem iNext = null;
            while (fHeight < iActualHeight)
            {
                iNext = GetNextData(iNext);
                if (iNext != null)
                {
                    // we use this 'if' instead of moving the condition in the loop, this way, the top cursor remains correct, otherewise, we move 1 to far to the top.
                    var iWrapper = GetWrapper(iNext, fBottomIndex.Count - 1);
                    InternalChildren.Add(iWrapper);
                    iWrapper.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

                        // we measuse, so we can see if any other will still fit.
                    fHeight += iWrapper.DesiredSize.Height;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Clears the children and puts the UI items in a buffer for reuse.
        /// </summary>
        private void ClearChildren()
        {
            VerViewportSize = 0;

            // VerBarVisibility = Visibility.Collapsed;
            // HorBarVisibility = Visibility.Collapsed;
            // ClearValue(ScrollablePanel.HorizontalMaxPropertyKey);                                                              //we need to clear this value cause we reset the horBar Visibility. If we don't reset, it doesn't get recalculated properly cause the object would think nothing has changed.
            foreach (System.Windows.UIElement i in InternalChildren)
            {
                var iItem = i as TreeViewPanelItem;
                if (iItem != null)
                {
                    // could be the fallback or something the user put on the panel (not allowed)
                    var iPropChanged = iItem.Content as UndoSystem.Interfaces.INotifyPropertyChanging;

                        // remove any monitoring.
                    if (iPropChanged != null)
                    {
                        Data.EventManagers.PropertyChangingEventManager.RemoveListener(iPropChanged, this);
                    }

                    fBufferedItems.Enqueue(iItem);

                    // don't remove data ref, could be that we redraw the same item, in which case we can leave as is.
                }
            }

            InternalChildren.Clear();
            fDict.Clear();
        }

        /// <summary>
        ///     Clears all the data when the itemsSource (list of tree items) was changed.
        /// </summary>
        private void ClearItemsSource()
        {
            fRenderingData = true;

                // when changing the vertical Offset, it normally rerenders the data, we dont' want this, cause we already are rendering the data again.
            try
            {
                ClearChildren();

                    // when the itemssource has changed, don't try to recouperate the UI elements. Usually, it's the fallback value anyway.
                VerticalOffset = 0.0; // need to reset the scroll pos when the data has changed.
                HorizontalOffset = 0.0;
                fLastIsVisible = false; // when new data, always render from top.
            }
            finally
            {
                fRenderingData = false;
            }
        }

        /// <summary>The get next data.</summary>
        /// <param name="curLast">The cur last.</param>
        /// <returns>The <see cref="ITreeViewPanelItem"/>.</returns>
        private ITreeViewPanelItem GetNextData(ITreeViewPanelItem curLast)
        {
            System.Collections.IList iPrevList; // required to find out how far the index may reach.

            var iCurBottom = curLast ?? GetItemAt(fBottomIndex);

            if (iCurBottom != null)
            {
                iPrevList = (iCurBottom.ParentTreeItem != null)
                                ? iCurBottom.ParentTreeItem.TreeItems
                                : ItemsSource.TreeItems;
                if (iCurBottom.HasChildren && iCurBottom.IsExpanded && iCurBottom.TreeItems != null
                    && iCurBottom.TreeItems.Count > 0)
                {
                    fBottomIndex.Add(0);
                    return iCurBottom.TreeItems[0] as ITreeViewPanelItem;
                }

                var iBottom = fBottomIndex.Count - 1;
                fBottomIndex[iBottom]++;
                while (iBottom > 0 && fBottomIndex[iBottom] >= iPrevList.Count)
                {
                    fBottomIndex.RemoveAt(iBottom);
                    iBottom--;
                    iCurBottom = iCurBottom.ParentTreeItem;
                    if (iCurBottom != null && iCurBottom.ParentTreeItem != null)
                    {
                        iPrevList = iCurBottom.ParentTreeItem.TreeItems;
                    }
                    else
                    {
                        iPrevList = ItemsSource.TreeItems;
                    }

                    fBottomIndex[iBottom]++;
                }

                if (iBottom >= 0 && fBottomIndex[iBottom] < iPrevList.Count)
                {
                    return iPrevList[fBottomIndex[iBottom]] as ITreeViewPanelItem;
                }
            }

            return null;
        }

        /// <summary>Retrieves the item at the specfied index position.</summary>
        /// <param name="indexList">The index list.</param>
        /// <returns>The <see cref="ITreeViewPanelItem"/>.</returns>
        private ITreeViewPanelItem GetItemAt(System.Collections.Generic.List<int> indexList)
        {
            var iPrevList = ItemsSource != null ? ItemsSource.TreeItems : null;
            ITreeViewPanelItem iRes = null;
            if (iPrevList != null)
            {
                for (var i = 0; i < indexList.Count; i++)
                {
                    if (indexList[i] < iPrevList.Count)
                    {
                        iRes = iPrevList[indexList[i]] as ITreeViewPanelItem;
                        iPrevList = iRes.TreeItems;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return iRes;
        }

        /// <summary>Handles the Loaded event of the TreeViewPanel control.
        ///     When loaded, we need to render the children if they were alaready assigned.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TreeViewPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= TreeViewPanel_Loaded;
            if (InternalChildren.Count == 0)
            {
                // only try to rerender when not yet done. Sometimes, Onloaded is raised to many times, causing double data.
                RenderChildren(ItemsSource.TreeItems);
            }
        }

        #endregion
    }
}