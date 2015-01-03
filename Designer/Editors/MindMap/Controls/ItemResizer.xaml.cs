// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemResizer.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CanvasResizer.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CanvasResizer.xaml
    /// </summary>
    public partial class ItemResizer : System.Windows.Controls.UserControl
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemResizer" /> class.
        /// </summary>
        public ItemResizer()
        {
            InitializeComponent();
        }

        /// <summary>LTs the dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event
        ///     data.</param>
        private void LTDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                var iPrevVal = iToControl.X;

                    // when we set iToControl.X, it might cut the value to what is allowed, if this happens, we need to compensate for this when we adjust the width.
                iToControl.X += e.HorizontalChange / iToControl.Owner.Zoom;
                iToControl.Width -= iToControl.X - iPrevVal;

                iPrevVal = iToControl.Y;
                iToControl.Y += e.VerticalChange / iToControl.Owner.Zoom;
                iToControl.Height -= iToControl.Y - iPrevVal;
            }
        }

        /// <summary>The t dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                var iPrevVal = iToControl.Y;
                iToControl.Y += e.VerticalChange / iToControl.Owner.Zoom;
                iToControl.Height -= iToControl.Y - iPrevVal;
            }
        }

        /// <summary>The rt dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RTDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                var iPrevVal = iToControl.Y;
                iToControl.Y += e.VerticalChange / iToControl.Owner.Zoom;
                iToControl.Width += e.HorizontalChange / iToControl.Owner.Zoom;
                iToControl.Height -= iToControl.Y - iPrevVal;
            }
        }

        /// <summary>The l dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void LDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                var iPrevVal = iToControl.X;

                    // when we set iToControl.X, it might cut the value to what is allowed, if this happens, we need to compensate for this when we adjust the width.
                iToControl.X += e.HorizontalChange / iToControl.Owner.Zoom;
                iToControl.Width -= iToControl.X - iPrevVal;
            }
        }

        /// <summary>The r dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                iToControl.Width += e.HorizontalChange / iToControl.Owner.Zoom;
            }
        }

        /// <summary>The lb dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void LBDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                var iPrevVal = iToControl.X;

                    // when we set iToControl.X, it might cut the value to what is allowed, if this happens, we need to compensate for this when we adjust the width.
                iToControl.X += e.HorizontalChange / iToControl.Owner.Zoom;
                iToControl.Width -= iToControl.X - iPrevVal;
                iToControl.Height += e.VerticalChange / iToControl.Owner.Zoom;
            }
        }

        /// <summary>The b dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                iToControl.Height += e.VerticalChange / iToControl.Owner.Zoom;
            }
        }

        /// <summary>The rb dragged.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RBDragged(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var iToControl = ToControl;
            if (iToControl != null)
            {
                iToControl.Width += e.HorizontalChange / iToControl.Owner.Zoom;
                iToControl.Height += e.VerticalChange / iToControl.Owner.Zoom;
            }
        }

        /// <summary>Handles the DragStarted event of the Thumb control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragStartedEventArgs"/> instance containing the event
        ///     data.</param>
        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we put a drag in an undo group so cause a drag is a single maneuvre.
        }

        /// <summary>Handles the DragCompleted event of the Thumb control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragCompletedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            WindowMain.UndoStore.EndUndoGroup();
        }

        #region ToControl

        /// <summary>
        ///     <see cref="ToControl" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ToControlProperty =
            System.Windows.DependencyProperty.Register(
                "ToControl", 
                typeof(PositionedMindMapItem), 
                typeof(ItemResizer), 
                new System.Windows.FrameworkPropertyMetadata((PositionedMindMapItem)null));

        /// <summary>
        ///     Gets or sets the <see cref="ToControl" /> property. This dependency
        ///     property indicates the object that will be resized by this resizer
        ///     object.
        /// </summary>
        public PositionedMindMapItem ToControl
        {
            get
            {
                return (PositionedMindMapItem)GetValue(ToControlProperty);
            }

            set
            {
                SetValue(ToControlProperty, value);
            }
        }

        #endregion
    }

    /// <summary>The item resize adorner.</summary>
    public class ItemResizeAdorner : System.Windows.Documents.Adorner
    {
        /// <summary>The f content.</summary>
        private readonly ItemResizer fContent;

        /// <summary>The visual children.</summary>
        private readonly System.Windows.Media.VisualCollection visualChildren;

        /// <summary>Initializes a new instance of the <see cref="ItemResizeAdorner"/> class.</summary>
        /// <param name="toControl">The to control.</param>
        /// <param name="adornedElt">The adorned elt.</param>
        public ItemResizeAdorner(PositionedMindMapItem toControl, System.Windows.UIElement adornedElt)
            : base(adornedElt)
        {
            visualChildren = new System.Windows.Media.VisualCollection(this);

            fContent = new ItemResizer();
            fContent.ToControl = toControl;
            visualChildren.Add(fContent);
        }

        /// <summary>Gets the visual children count.</summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>The measure override.</summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            fContent.Measure(constraint);
            return fContent.DesiredSize;
        }

        /// <summary>The arrange override.</summary>
        /// <param name="finalSize">The final size.</param>
        /// <returns>The <see cref="Size"/>.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            fContent.Arrange(new System.Windows.Rect(finalSize));
            return finalSize;
        }

        /// <summary>The get visual child.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="Visual"/>.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return fContent;
        }
    }
}