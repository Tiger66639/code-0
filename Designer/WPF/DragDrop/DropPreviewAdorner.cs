// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropPreviewAdorner.cs" company="">
//   
// </copyright>
// <summary>
//   The drop preview adorner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace DnD
{
    /// <summary>The drop preview adorner.</summary>
    public class DropPreviewAdorner : System.Windows.Documents.Adorner
    {
        /// <summary>The _left.</summary>
        private double _left;

        /// <summary>The _top.</summary>
        private double _top;

        /// <summary>The _presenter.</summary>
        private readonly System.Windows.Controls.ContentPresenter _presenter;

        /// <summary>Initializes a new instance of the <see cref="DropPreviewAdorner"/> class.</summary>
        /// <param name="feedbackUI">The feedback ui.</param>
        /// <param name="adornedElt">The adorned elt.</param>
        public DropPreviewAdorner(System.Windows.UIElement feedbackUI, System.Windows.UIElement adornedElt)
            : base(adornedElt)
        {
            _presenter = new System.Windows.Controls.ContentPresenter();
            _presenter.Content = feedbackUI;
            _presenter.IsHitTestVisible = false;
            Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>Gets or sets the left.</summary>
        public double Left
        {
            get
            {
                return _left;
            }

            set
            {
                _left = value;
                UpdatePosition();
            }
        }

        /// <summary>Gets or sets the top.</summary>
        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                _top = value;
                UpdatePosition();
            }
        }

        /// <summary>Gets the visual children count.</summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>The update position.</summary>
        private void UpdatePosition()
        {
            var layer = Parent as System.Windows.Documents.AdornerLayer;
            if (layer != null)
            {
                layer.Update(AdornedElement);
            }
        }

        /// <summary>The measure override.</summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            _presenter.Measure(constraint);
            return _presenter.DesiredSize;
        }

        /// <summary>The arrange override.</summary>
        /// <param name="finalSize">The final size.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            _presenter.Arrange(new System.Windows.Rect(finalSize));
            return finalSize;
        }

        /// <summary>The get visual child.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="Visual"/>.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return _presenter;
        }

        /// <summary>The get desired transform.</summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The <see cref="GeneralTransform"/>.</returns>
        public override System.Windows.Media.GeneralTransform GetDesiredTransform(
            System.Windows.Media.GeneralTransform transform)
        {
            var result = new System.Windows.Media.GeneralTransformGroup();
            result.Children.Add(new System.Windows.Media.TranslateTransform(Left, Top));
            if (Left > 0)
            {
                Visibility = System.Windows.Visibility.Visible;
            }

            result.Children.Add(base.GetDesiredTransform(transform));

            return result;
        }
    }
}