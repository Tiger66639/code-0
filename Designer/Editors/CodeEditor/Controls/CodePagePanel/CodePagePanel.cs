// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodePagePanel.cs" company="">
//   
// </copyright>
// <summary>
//   Inherits from SelectedItemAdornder so that we can easely see a difference
//   between the 2 types of adorners, for removing them.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Inherits from SelectedItemAdornder so that we can easely see a difference
    ///     between the 2 types of adorners, for removing them.
    /// </summary>
    public class NextStatementAdorner : SelectedItemAdorner
    {
        /// <summary>Initializes a new instance of the <see cref="NextStatementAdorner"/> class. Initializes a new instance of the <see cref="NextStatementAdorner"/>
        ///     class.</summary>
        /// <param name="adornedEl">The adorned el.</param>
        public NextStatementAdorner(System.Windows.UIElement adornedEl)
            : base(adornedEl)
        {
        }
    }

    /// <summary>
    ///     A panel for a code page.
    /// </summary>
    public class CodePagePanel : ScrollablePanel
    {
        #region Fields

        /// <summary>
        ///     The tree of ui elements, rendered in the same order as the
        ///     itemsCollection. This allows us to add/remove items based on
        ///     collaps/expand actions.
        /// </summary>
        private readonly CPPItemList fItems;

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodePagePanel" /> class.
        /// </summary>
        public CodePagePanel()
        {
            fItems = new CPPItemList(null, this);
            fItems.Orientation = System.Windows.Controls.Orientation.Vertical;
        }

        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(CodeEditorPage), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates the code editor page that needs to be displayed.
        /// </summary>
        public CodeEditorPage ItemsSource
        {
            get
            {
                return (CodeEditorPage)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ItemsSource"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemsSourceChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((CodePagePanel)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ItemsSource"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                fItems.ItemsSource = ((CodeEditorPage)e.NewValue).Items;
                fItems.IsExpanded = true; // always make certain that it is expanded.
            }
            else
            {
                fItems.ItemsSource = null;
                fItems.IsExpanded = false;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     IsSelected Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsSelectedProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "IsSelected", 
                typeof(bool), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        /// <summary>Gets the IsSelected property. This attached property indicates if the
        ///     usercontrol has a selection adorner or not.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool GetIsSelected(System.Windows.DependencyObject d)
        {
            return (bool)d.GetValue(IsSelectedProperty);
        }

        /// <summary>Sets the IsSelected property. This attached property indicates if the
        ///     usercontrol has a selection adorner or not.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetIsSelected(System.Windows.DependencyObject d, bool value)
        {
            d.SetValue(IsSelectedProperty, value);
        }

        /// <summary>Handles changes to the IsSelected property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsSelectedChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<CodePagePanel>(d);
            if (iPanel != null)
            {
                var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(iPanel);
                System.Diagnostics.Debug.Assert(iLayer != null);
                var iControl = d as System.Windows.UIElement;
                System.Diagnostics.Debug.Assert(iControl != null);

                if ((bool)e.NewValue == false)
                {
                    var iAdorners = iLayer.GetAdorners(iControl);
                    if (iAdorners == null)
                    {
                        return;
                    }

                    foreach (var i in iAdorners)
                    {
                        if (i is SelectedItemAdorner)
                        {
                            iLayer.Remove(i);
                            break;
                        }
                    }
                }
                else
                {
                    var iNew = new SelectedItemAdorner(iControl);
                    var iFound = iControl.ReadLocalValue(SelectionRectProperty);
                    if (iFound != System.Windows.DependencyProperty.UnsetValue)
                    {
                        iNew.Rect = (System.Windows.Rect)iFound;
                        iNew.Offset = iControl.TranslatePoint(new System.Windows.Point(), iPanel);
                    }

                    iNew.Rectangle.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                    iNew.Rectangle.StrokeDashArray.Add(1.0);
                    iNew.Rectangle.StrokeDashArray.Add(2.0);
                    var iBinding = new System.Windows.Data.Binding("ZoomInverse");
                    iBinding.Source = iPanel.ItemsSource;
                    iNew.SetBinding(SelectedItemAdorner.StrokeThicknessProperty, iBinding);

                        // we need bind to the inverse zoom so that the selection boxes always have the same size at every zoom value.
                    iLayer.Add(iNew);
                }
            }
        }

        #endregion

        #region IsNextStatement

        /// <summary>
        ///     IsNextStatement Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsNextStatementProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "IsNextStatement", 
                typeof(bool), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsNextStatementChanged));

        /// <summary>Gets the IsNextStatement property. This attached property indicates if
        ///     the control (displays the next statement or not) is displayed with the
        ///     nextStatement adorner.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool GetIsNextStatement(System.Windows.DependencyObject d)
        {
            return (bool)d.GetValue(IsNextStatementProperty);
        }

        /// <summary>Sets the IsNextStatement property. This attached property indicates if
        ///     the control (displays the next statement or not) is displayed with the
        ///     nextStatement adorner.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetIsNextStatement(System.Windows.DependencyObject d, bool value)
        {
            d.SetValue(IsNextStatementProperty, value);
        }

        /// <summary>Handles changes to the IsNextStatement property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsNextStatementChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<CodePagePanel>(d);
            if (iPanel != null)
            {
                var iLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(iPanel);
                System.Diagnostics.Debug.Assert(iLayer != null);
                var iControl = d as System.Windows.UIElement;
                System.Diagnostics.Debug.Assert(iControl != null);

                if ((bool)e.NewValue == false)
                {
                    var iAdorners = iLayer.GetAdorners(iControl);
                    if (iAdorners == null)
                    {
                        return;
                    }

                    foreach (var i in iAdorners)
                    {
                        if (i is NextStatementAdorner)
                        {
                            iLayer.Remove(i);
                            break;
                        }
                    }
                }
                else
                {
                    var iNew = new NextStatementAdorner(iControl);
                    var iFound = iControl.ReadLocalValue(SelectionRectProperty);
                    if (iFound != System.Windows.DependencyProperty.UnsetValue)
                    {
                        iNew.Rect = (System.Windows.Rect)iFound;
                        iNew.Offset = iControl.TranslatePoint(new System.Windows.Point(), iPanel);
                    }

                    iNew.Rectangle.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    var iBinding = new System.Windows.Data.Binding("ZoomInverse");
                    iBinding.Source = iPanel.ItemsSource;
                    iNew.SetBinding(SelectedItemAdorner.StrokeThicknessProperty, iBinding);

                        // we need bind to the inverse zoom so that the selection boxes always have the same size at every zoom value.
                    iLayer.Add(iNew);
                }
            }
        }

        #endregion

        #region ItemContainerStyle

        /// <summary>
        ///     <see cref="ItemContainerStyle" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemContainerStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ItemContainerStyle", 
                typeof(System.Windows.Style), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.Style)null));

        /// <summary>
        ///     Gets or sets the <see cref="ItemContainerStyle" /> property. This
        ///     dependency property indicates the style that should be applied to the
        ///     containers of all the items on panel.
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

        #endregion

        #region ElementStyle

        /// <summary>
        ///     <see cref="ElementStyle" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ElementStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ElementStyle", 
                typeof(System.Windows.Style), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.Style)null));

        /// <summary>
        ///     Gets or sets the <see cref="ElementStyle" /> property. This dependency
        ///     property indicates the style that wil be applied to the code element
        ///     ui's themselves (not the wrappers).
        /// </summary>
        public System.Windows.Style ElementStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(ElementStyleProperty);
            }

            set
            {
                SetValue(ElementStyleProperty, value);
            }
        }

        #endregion

        #region SelectionRect

        /// <summary>
        ///     SelectionRect Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectionRectProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "SelectionRect", 
                typeof(System.Windows.Rect), 
                typeof(CodePagePanel), 
                new System.Windows.FrameworkPropertyMetadata(new System.Windows.Rect()));

        /// <summary>Gets the SelectionRect property. This dependency property indicates
        ///     the region to put a selection rectangle over instead of the bounder of
        ///     the object that this property is attached to. This is used to allow a
        ///     group of objects to be dragged and selected in 1 action, like the
        ///     conditional expression, which is dragged with the expander, but should
        ///     include other objects as well.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="Rect"/>.</returns>
        public static System.Windows.Rect GetSelectionRect(System.Windows.DependencyObject d)
        {
            return (System.Windows.Rect)d.GetValue(SelectionRectProperty);
        }

        /// <summary>Sets the SelectionRect property. This dependency property indicates
        ///     the region to put a selection rectangle over instead of the bounder of
        ///     the object that this property is attached to. This is used to allow a
        ///     group of objects to be dragged and selected in 1 action, like the
        ///     conditional expression, which is dragged with the expander, but should
        ///     include other objects as well.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetSelectionRect(System.Windows.DependencyObject d, System.Windows.Rect value)
        {
            d.SetValue(SelectionRectProperty, value);
        }

        #endregion

        #region overrides

        /// <summary>Raises the <see cref="System.Windows.FrameworkElement.SizeChanged"/>
        ///     event, using the specified information as part of the eventual event
        ///     data.</summary>
        /// <remarks>When the size has changed, we need to update the horizontal and
        ///     vertical max scroll values.</remarks>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CalculateScrollValues();
        }

        /// <summary>
        ///     Calculates the max scroll values and assigns them to the properties.
        ///     This is <see langword="abstract" /> cause it depends on the
        ///     implementation.
        /// </summary>
        protected internal override void CalculateScrollValues()
        {
            var iPage = ItemsSource;
            if (iPage != null)
            {
                var iRequired =
                    new System.Windows.Size(
                        (fItems.Size.Width * iPage.Zoom) + (ScrollAdded.Width * iPage.Zoom), 
                        (fItems.Size.Height * iPage.Zoom) + (ScrollAdded.Height * iPage.Zoom));

                // double iHeight = ActualHeight;
                // double iWidth = ActualWidth;
                SetVerticalMax(System.Math.Max(iPage.VerScrollPos, iRequired.Height));

                    // if the current scroll value is bigger than what we want to assign, we use the value, so that we at least can scroll back.
                iRequired.Width /= 2; // we use a centralized scolling for the horizontal bar.
                SetHorizontalMax(System.Math.Max(iPage.HorScrollPos, iRequired.Width));
                SetHorizontalMin(System.Math.Min(iPage.HorScrollPos, -iRequired.Width));

                // if (iHeight < iRequired.Height)

                // else
                // SetVerticalMax(Math.Max(iPage.VerScrollPos, iRequired.Height));
                // if (iWidth < iRequired.Width)
                // {
                // iRequired.Width -= iWidth;

                // }
                // else
                // {
                // iWidth /= 2;
                // SetHorizontalMax(Math.Max(iPage.HorScrollPos, iWidth));
                // SetHorizontalMin(Math.Min(iPage.HorScrollPos, -iWidth));
                // }
            }
        }

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iPage = ItemsSource;
            if (iPage != null)
            {
                var iOffset = new System.Windows.Point(-iPage.HorScrollPos, -iPage.VerScrollPos);

                    // we need to pass along an offset so we can nest items (the ofsset displaces them according to previous items-parents).
                fItems.Arrange(finalSize, ref iOffset);
                var iSelected = Enumerable.ToArray(iPage.SelectedItems);
                iPage.SelectedItems.Clear();

                    // we clear the selected items and reselect them again, cause their size might have changed and we need to update the selectionboxes.
                foreach (var i in iSelected)
                {
                    i.IsSelected = true;
                }

                UpdateIsNexts();
            }

            return finalSize;
        }

        /// <summary>The update is nexts.</summary>
        private void UpdateIsNexts()
        {
            var iProc = ProcessorManager.Current.SelectedProcessor;
            if (iProc != null && iProc.Processor != null)
            {
                var iNext = iProc.Processor.NextStatement;
                if (iNext != null)
                {
                    var iData = BrainData.Current.NeuronInfo[iNext.ID];
                    iData.IsNextStatement = false; // we toggle this on/off to update it, so that it is redrawn.
                    iData.IsNextStatement = true;
                }
            }
        }

        /// <summary>When overridden in a derived class, measures the size in layout
        ///     required for child elements and determines a size for the<see cref="System.Windows.FrameworkElement"/> -derived class.</summary>
        /// <param name="availableSize">The available size that this element can give to child elements.
        ///     Infinity can be specified as a value to indicate that the element will
        ///     size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on
        ///     its calculations of child element sizes.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var iMax = new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity);
            fItems.Measure(iMax);
            var iParent = (System.Windows.FrameworkElement)Parent;

                // if we are the child of an adornerDecorator, we don't get the correct size constraint. So we  make certain that we always get this from the parent.
            CalculateScrollValues();
            if (iParent != null)
            {
                // a codePAgePanel is put in an AdornerDecorator, to give it's own (clipped) adorner layer. This has a strange measure/arrange which we must handle in such a way that we always use all the available space.
                return new System.Windows.Size(iParent.ActualWidth, iParent.ActualHeight);
            }

            return availableSize;
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseWheel"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <remarks>When control is pressed-&gt; zoom, otherwise scroll.</remarks>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var iPage = ItemsSource;
            System.Diagnostics.Debug.Assert(iPage != null);
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                // we do a zoom.
                if (e.Delta > 0)
                {
                    iPage.Zoom += iPage.Zoom * 0.1; // we make the zoom relative to the current value: 10 %
                }
                else
                {
                    iPage.Zoom -= iPage.Zoom * 0.1;
                }
            }
            else
            {
                iPage.VerScrollPos -= e.Delta / 3.0;
                CalculateScrollValues();

                    // recalculate these cause we did a scroll, maybe the scrollbars weren't visible yet
            }

            base.OnMouseWheel(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the
        ///     HorizontalOffset property.</summary>
        /// <param name="e"></param>
        protected override void OnHorizontalOffsetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // don't do anything: we only allow to scroll horizontally over the already existing content.
        }

        /// <summary>Coerces the horizontal offset value. We make certain that the<paramref name="value"/> remains within the min and max values. We
        ///     also make certain that, if the new value, crosses 0, we first stop
        ///     there, to create a little snap, for centering.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected override object CoerceHorizontalOffsetValue(object value)
        {
            var iVal = (double)value;
            if (HorizontalOffset < 0 && iVal > 0)
            {
                return (double)0;
            }

            return System.Math.Min(System.Math.Max(iVal, HorizontalMin), HorizontalMax);
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
        ///     event is raised on this element. Implement this method to add class
        ///     handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that the left mouse button was pressed.</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.OriginalSource == this)
            {
                // we make certain that the user clicked on the panel itself and not one of the children.
                Focus();
                var iPage = ItemsSource;
                if (iPage != null)
                {
                    iPage.SelectedItems.Clear();
                }
            }
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.UIElement.MouseRightButtonDown"/> routed
        ///     event reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that the right mouse button was pressed.</param>
        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (e.OriginalSource == this)
            {
                // we make certain that the user clicked on the panel itself and not one of the children.
                Focus();
                var iPage = ItemsSource;
                if (iPage != null)
                {
                    iPage.SelectedItems.Clear();
                }
            }
        }

        #endregion
    }
}