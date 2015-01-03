// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedItemAdorner.cs" company="">
//   
// </copyright>
// <summary>
//   An adorner to indicate that the item is selected in some way. You can
//   assign the stroke properties through the reccangle property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An adorner to indicate that the item is selected in some way. You can
    ///     assign the stroke properties through the reccangle property.
    /// </summary>
    public class SelectedItemAdorner : System.Windows.Documents.Adorner
    {
        #region fields

        /// <summary>The f back side.</summary>
        private readonly System.Windows.Shapes.Rectangle fBackSide;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="SelectedItemAdorner"/> class.</summary>
        /// <param name="adornedElt">The adorned elt.</param>
        public SelectedItemAdorner(System.Windows.UIElement adornedElt)
            : base(adornedElt)
        {
            fBackSide = new System.Windows.Shapes.Rectangle();
            fBackSide.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            fBackSide.SnapsToDevicePixels = true;
            AddLogicalChild(fBackSide);
            AddVisualChild(fBackSide);

            Rectangle = new System.Windows.Shapes.Rectangle();
            Rectangle.SnapsToDevicePixels = true;
            AddLogicalChild(Rectangle);
            AddVisualChild(Rectangle);

            SnapsToDevicePixels = true;
        }

        #region Rectangle

        /// <summary>
        ///     Gets the rectangle that is displayed.
        /// </summary>
        public System.Windows.Shapes.Rectangle Rectangle { get; private set; }

        #endregion

        #region Rect

        /// <summary>
        ///     Gets/sets the rectangle that should be drawn instead of the boundery
        ///     of the object. When null, the size of the object is used.
        /// </summary>
        public System.Windows.Rect? Rect { get; set; }

        #endregion

        /// <summary>
        ///     Gets or sets the offset that should be applied (negatively) to the
        ///     rect for rendering. This is important since we normally draw the left
        ///     upper corner at the location of the adorned element, but if we are
        ///     using a rect, this needs to be relative to the panel, so we need the
        ///     offset (can't adorn panel, cause than the remove and add algorithms
        ///     become much more complicated).s
        /// </summary>
        /// <value>
        ///     The offset.
        /// </value>
        public System.Windows.Point Offset { get; set; }

        /// <summary>
        ///     Gets the number of visual child elements within this element.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return 2;
            }
        }

        /// <summary>Implements any custom measuring behavior for the adorner.</summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>A <see cref="System.Windows.Size"/> object representing the amount of layout space
        ///     needed by the adorner.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            if (Rect.HasValue == false)
            {
                var iSize = AdornedElement.DesiredSize;
                Rectangle.Measure(iSize);
                fBackSide.Measure(iSize);
                return iSize;
            }

            Rectangle.Measure(Rect.Value.Size);
            fBackSide.Measure(Rect.Value.Size);
            return Rect.Value.Size;
        }

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            System.Windows.Rect iRect;
            if (Rect.HasValue)
            {
                iRect =
                    new System.Windows.Rect(
                        new System.Windows.Point(-Offset.X + Rect.Value.X, -Offset.Y + Rect.Value.Y), 
                        finalSize);
            }
            else
            {
                iRect = new System.Windows.Rect(finalSize);
            }

            fBackSide.Arrange(iRect);
            Rectangle.Arrange(iRect);
            return finalSize;
        }

        /// <summary>Overrides <see cref="System.Windows.Media.Visual.GetVisualChild"/> , and returns a child
        ///     at the specified <paramref name="index"/> from a collection of child
        ///     elements.</summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element. This should not return null; if the
        ///     provided <paramref name="index"/> is out of range, an exception is
        ///     thrown.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return fBackSide;
            }

            return Rectangle;
        }

        #region StrokeThickness

        /// <summary>
        ///     <see cref="StrokeThickness" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty StrokeThicknessProperty =
            System.Windows.DependencyProperty.Register(
                "StrokeThickness", 
                typeof(double), 
                typeof(SelectedItemAdorner), 
                new System.Windows.FrameworkPropertyMetadata((double)0, OnStrokeThicknessChanged));

        /// <summary>
        ///     Gets or sets the <see cref="StrokeThickness" /> property. This
        ///     dependency property indicates the strokethickness that should be
        ///     assigned to the rectangles.
        /// </summary>
        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }

            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="StrokeThickness"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnStrokeThicknessChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((SelectedItemAdorner)d).OnStrokeThicknessChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="StrokeThickness"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnStrokeThicknessChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iVal = (double)e.NewValue;
            Rectangle.StrokeThickness = iVal;
            fBackSide.StrokeThickness = iVal;
        }

        #endregion
    }
}