// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionAdorner.cs" company="">
//   
// </copyright>
// <summary>
//   Used to select a set of neurons on the mind map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to select a set of neurons on the mind map.
    /// </summary>
    public class SelectionAdorner : System.Windows.Documents.Adorner
    {
        /// <summary>The f end point.</summary>
        private System.Windows.Point fEndPoint;

        #region fields

        /// <summary>The f rect.</summary>
        private readonly System.Windows.Shapes.Rectangle fRect;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="SelectionAdorner"/> class. Initializes a new instance of the <see cref="SelectionAdorner"/>
        ///     class.</summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="adornedElt">The element to adorn.</param>
        /// <param name="inverseZoom">The inverse of the zoom, which becomes the stroke thikeness (must be 1
        ///     base, not 100).</param>
        public SelectionAdorner(
            System.Windows.Point startPoint, 
            System.Windows.UIElement adornedElt, 
            double inverseZoom)
            : base(adornedElt)
        {
            StartPoint = startPoint;
            fEndPoint = startPoint;
            fRect = new System.Windows.Shapes.Rectangle();
            fRect.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.AliceBlue);
            fRect.Opacity = 0.4;
            fRect.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.SteelBlue);
            fRect.StrokeThickness = inverseZoom;
        }

        #region EndPoint

        /// <summary>
        ///     Gets/sets the end point of the selection, can be smaller than
        ///     startpoint for inverse selection.
        /// </summary>
        public System.Windows.Point EndPoint
        {
            get
            {
                return fEndPoint;
            }

            set
            {
                fEndPoint = value;
                var layer = Parent as System.Windows.Documents.AdornerLayer;
                if (layer != null)
                {
                    layer.Update(AdornedElement);
                }
            }
        }

        #endregion

        #region StartPoint

        /// <summary>
        ///     Gets the startpoint
        /// </summary>
        public System.Windows.Point StartPoint { get; private set; }

        #endregion

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
                return 1;
            }
        }

        /// <summary>Implements any custom measuring behavior for the adorner.</summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>A <see cref="System.Windows.Size"/> object representing the amount of layout space
        ///     needed by the adorner.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            var iSize = new System.Windows.Size();
            if (StartPoint.X < fEndPoint.X)
            {
                iSize.Width = fEndPoint.X - StartPoint.X;
            }
            else
            {
                iSize.Width = StartPoint.X - fEndPoint.X;
            }

            if (StartPoint.Y < fEndPoint.Y)
            {
                iSize.Height = fEndPoint.Y - StartPoint.Y;
            }
            else
            {
                iSize.Height = StartPoint.Y - fEndPoint.Y;
            }

            fRect.Measure(iSize);
            return fRect.DesiredSize;
        }

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iRect = GetSelectArea();
            fRect.Arrange(iRect);
            return iRect.Size;
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
            return fRect;
        }

        /// <summary>The get select area.</summary>
        /// <returns>The <see cref="Rect"/>.</returns>
        internal System.Windows.Rect GetSelectArea()
        {
            var iSize = new System.Windows.Size();
            var iStart = new System.Windows.Point();
            if (StartPoint.X < fEndPoint.X)
            {
                iStart.X = StartPoint.X;
                iSize.Width = fEndPoint.X - StartPoint.X;
            }
            else
            {
                iStart.X = fEndPoint.X;
                iSize.Width = StartPoint.X - fEndPoint.X;
            }

            if (StartPoint.Y < fEndPoint.Y)
            {
                iStart.Y = StartPoint.Y;
                iSize.Height = fEndPoint.Y - StartPoint.Y;
            }
            else
            {
                iStart.Y = fEndPoint.Y;
                iSize.Height = StartPoint.Y - fEndPoint.Y;
            }

            return new System.Windows.Rect(iStart, iSize);
        }
    }
}