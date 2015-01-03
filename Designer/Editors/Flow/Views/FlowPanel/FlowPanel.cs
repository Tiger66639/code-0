// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowPanel.cs" company="">
//   
// </copyright>
// <summary>
//   The flow panel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    using System.Linq;

    /// <summary>The flow panel.</summary>
    public class FlowPanel : ScrollablePanel
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlowPanel" /> class.
        /// </summary>
        public FlowPanel()
        {
            fItems = new FlowPanelItemList(null, this);
            fItems.Orientation = System.Windows.Controls.Orientation.Horizontal;
        }

        #endregion

        /// <summary>returns the last UI element on the panel that can be used for editing.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        internal System.Windows.FrameworkElement GetLastUI()
        {
            if (fItems.Children.Count > 0)
            {
                var iLast = fItems.Children[fItems.Children.Count - 1];
                return iLast.GetLastUI();
            }

            return null;
        }

        /// <summary>The get selected ui.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        internal System.Windows.FrameworkElement GetSelectedUI()
        {
            if (fSelected.Count > 0)
            {
                return fSelected[fSelected.Count - 1].GetLastUI();
            }

            return null;
        }

        /// <summary>Removes the <paramref name="item"/> from teh list of selected items.</summary>
        /// <param name="item">The item.</param>
        internal void RemoveSelected(FlowPanelItemBase item)
        {
            fSelected.Remove(item);
        }

        /// <summary>Adds the <paramref name="item"/> to the list of selected.</summary>
        /// <param name="item">The item.</param>
        internal void AddSelected(FlowPanelItemBase item)
        {
            fSelected.Add(item);
        }

        #region Fields

        /// <summary>
        ///     The tree of ui elements, rendered in the same order as the
        ///     itemsCollection. This allows us to add/remove items based on
        ///     collaps/expand actions.
        /// </summary>
        private readonly FlowPanelItemList fItems;

        /// <summary>The f selected.</summary>
        private readonly System.Collections.Generic.List<FlowPanelItemBase> fSelected =
            new System.Collections.Generic.List<FlowPanelItemBase>();

                                                                            // keeps track of all the items that are selected, so we can easely calculate position values.
        #endregion

        #region ElementStyle

        /// <summary>
        ///     <see cref="ElementStyle" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ElementStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ElementStyle", 
                typeof(System.Windows.Style), 
                typeof(FlowPanel), 
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

        #region ConditionalPartBackgroundStyle

        /// <summary>
        ///     <see cref="ConditionalPartBackgroundStyle" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ConditionalPartBackgroundStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ConditionalPartBackgroundStyle", 
                typeof(System.Windows.Style), 
                typeof(FlowPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnConditionalPartBackgroundStyleChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ConditionalPartBackgroundStyle" />
        ///     property. This dependency property indicates the style to apply to the
        ///     backgrounds of the conditional parts. Best to assign this prop before
        ///     the itemsSource.
        /// </summary>
        /// <remarks>
        ///     When changed, we rebuild the ui elements, don't use a binding on the
        ///     border's for this. That would be to costy.
        /// </remarks>
        public System.Windows.Style ConditionalPartBackgroundStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(ConditionalPartBackgroundStyleProperty);
            }

            set
            {
                SetValue(ConditionalPartBackgroundStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ConditionalPartBackgroundStyle"/>
        ///     property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnConditionalPartBackgroundStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowPanel)d).OnConditionalPartBackgroundStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ConditionalPartBackgroundStyle"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnConditionalPartBackgroundStyleChanged(
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iSource = ItemsSource;
            ItemsSource = null;

                // we set to null, so we can rebuild all the ui elements, this re-assigns the style: better to do this for a rare event instead of creating a bunch of bindings that will never be used.
            ItemsSource = iSource;
        }

        #endregion

        #region ConditionalBackgroundStyle

        /// <summary>
        ///     <see cref="ConditionalBackgroundStyle" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ConditionalBackgroundStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ConditionalBackgroundStyle", 
                typeof(System.Windows.Style), 
                typeof(FlowPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnConditionalBackgroundStyleChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ConditionalBackgroundStyle" /> property.
        ///     This dependency property indicates the style to apply to the
        ///     backgrounds of the conditionals. Best to assign this prop before the
        ///     itemsSource.
        /// </summary>
        public System.Windows.Style ConditionalBackgroundStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(ConditionalBackgroundStyleProperty);
            }

            set
            {
                SetValue(ConditionalBackgroundStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ConditionalBackgroundStyle"/>
        ///     property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnConditionalBackgroundStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowPanel)d).OnConditionalBackgroundStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ConditionalBackgroundStyle"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnConditionalBackgroundStyleChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iSource = ItemsSource;
            ItemsSource = null;

                // we set to null, so we can rebuild all the ui elements, this re-assigns the style: better to do this for a rare event instead of creating a bunch of bindings that will never be used.
            ItemsSource = iSource;
        }

        #endregion

        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(Flow), 
                typeof(FlowPanel), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates the itemssource to use by the panel.
        /// </summary>
        public Flow ItemsSource
        {
            get
            {
                return (Flow)GetValue(ItemsSourceProperty);
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
            ((FlowPanel)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ItemsSource"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                fItems.ItemsSource = ((Flow)e.NewValue).Items;
                fItems.IsExpanded = true; // always make certain that it is expanded.
            }
            else
            {
                fItems.ItemsSource = null;
                fItems.IsExpanded = false;
            }
        }

        #endregion

        #region Overrides

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
            var iFlow = ItemsSource;
            if (iFlow != null)
            {
                var iRequired =
                    new System.Windows.Size(
                        (fItems.Size.Width * iFlow.Zoom) + (ScrollAdded.Width * iFlow.Zoom), 
                        (fItems.Size.Height * iFlow.Zoom) + (ScrollAdded.Height * iFlow.Zoom));
                var iHeight = ActualHeight;
                var iWidth = ActualWidth;

                if (iHeight < iRequired.Height)
                {
                    SetVerticalMax(iRequired.Height - iHeight);
                }
                else
                {
                    SetVerticalMax(0);
                }

                if (iWidth < iRequired.Width)
                {
                    iRequired.Width -= iWidth;
                    iRequired.Width /= 2; // we use a centralized scolling for the horizontal bar.
                    SetHorizontalMax(iRequired.Width);
                    SetHorizontalMin(-iRequired.Width);
                }
                else
                {
                    SetHorizontalMax(0);
                    SetHorizontalMin(0);
                }
            }
        }

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iOffset = new System.Windows.Point(-HorizontalOffset, -VerticalOffset);

                // we need to pass along an offset so we can nest items (the ofsset displaces them according to previous items-parents).
            fItems.Arrange(finalSize, iOffset);
            var iPage = ItemsSource;
            if (iPage != null)
            {
                var iSelected = (from FlowItem i in iPage.SelectedItems select i).ToArray();
                iPage.SelectedItems.Clear();

                    // we clear the selected items and reselect them again, cause their size might have changed and we need to update the selectionboxes.
                foreach (var i in iSelected)
                {
                    iPage.SelectedItems.Add(i);

                        // we do an add, not an iSelected=true, since that would check if the ctrl is pressed, which we can't immitate.
                }
            }

            return finalSize;
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
                return new System.Windows.Size(
                    System.Math.Max(iParent.ActualWidth, fItems.Size.Width), 
                    System.Math.Max(iParent.ActualHeight, fItems.Size.Height));

                    // a flowPanel is put in an AdornerDecorator, to give it's own (clipped) adorner layer. This has a strange measure/arrange which we must handle in such a way that we always use all the available space
            }

            return availableSize;
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that
        ///     contains the event data. This event data reports details about the
        ///     mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSource = ItemsSource;
            if (iSource != null)
            {
                iSource.SelectedItems.Clear();
            }

            base.OnMouseDown(e);
            Focus();

            // e.Handled = true;
        }

        #endregion
    }
}