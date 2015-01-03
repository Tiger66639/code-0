// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatermarkService.cs" company="">
//   
// </copyright>
// <summary>
//   The watermark service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>The watermark service.</summary>
    public class WatermarkService
    {
        /// <summary>Gets the Watermark property. This dependency property indicates the
        ///     watermark for the control.</summary>
        /// <param name="d"><see cref="System.Windows.DependencyObject"/> to get the property from</param>
        /// <returns>The value of the Watermark property</returns>
        public static object GetWatermark(System.Windows.DependencyObject d)
        {
            return d.GetValue(WatermarkProperty);
        }

        /// <summary>Sets the Watermark property. This dependency property indicates the
        ///     watermark for the control.</summary>
        /// <param name="d"><see cref="System.Windows.DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetWatermark(System.Windows.DependencyObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        /// <summary>Handles changes to the Watermark property.</summary>
        /// <param name="d"><see cref="System.Windows.DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the
        ///     event data.</param>
        private static void OnWatermarkChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var control = (System.Windows.Controls.Control)d;
            control.Loaded += Control_Loaded;

            if (d is System.Windows.Controls.ComboBox || d is System.Windows.Controls.TextBox)
            {
                control.GotKeyboardFocus += Control_GotKeyboardFocus;
                control.LostKeyboardFocus += Control_Loaded;
            }
        }

        /// <summary>
        ///     Watermark Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty WatermarkProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "Watermark", 
                typeof(object), 
                typeof(WatermarkService), 
                new System.Windows.FrameworkPropertyMetadata(null, OnWatermarkChanged));

        #region Event Handlers

        /// <summary>Handle the GotFocus event on the control</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        private static void Control_GotKeyboardFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var c = (System.Windows.Controls.Control)sender;
            if (ShouldShowWatermark(c))
            {
                RemoveWatermark(c);
            }
        }

        /// <summary>Handle the Loaded and LostFocus event on the control</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        private static void Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var control = (System.Windows.Controls.Control)sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>Remove the watermark from the specified element</summary>
        /// <param name="control">Element to remove the watermark from</param>
        private static void RemoveWatermark(System.Windows.UIElement control)
        {
            var layer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                var adorners = layer.GetAdorners(control);
                if (adorners == null)
                {
                    return;
                }

                foreach (var adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = System.Windows.Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>Show the watermark on the specified <paramref name="control"/></summary>
        /// <param name="control">Control to show the watermark on</param>
        private static void ShowWatermark(System.Windows.Controls.Control control)
        {
            var layer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
            }
        }

        /// <summary>Indicates whether or not the watermark should be shown on the
        ///     specified control</summary>
        /// <param name="c"><see cref="System.Windows.Controls.Control"/> to test</param>
        /// <returns><see langword="true"/> if the watermark should be shown;<see langword="false"/> otherwise</returns>
        private static bool ShouldShowWatermark(System.Windows.Controls.Control c)
        {
            if (c is System.Windows.Controls.ComboBox)
            {
                return (c as System.Windows.Controls.ComboBox).Text == string.Empty;
            }

            if (c is System.Windows.Controls.Primitives.TextBoxBase)
            {
                return (c as System.Windows.Controls.TextBox).Text == string.Empty;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    ///     Adorner for the watermark
    /// </summary>
    internal class WatermarkAdorner : System.Windows.Documents.Adorner
    {
        #region Private Fields

        /// <summary>
        ///     <see cref="System.Windows.Controls.ContentPresenter" /> that holds the watermark
        /// </summary>
        private readonly System.Windows.Controls.ContentPresenter contentPresenter;

        #endregion

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="WatermarkAdorner"/> class. Initializes a new instance of the <see cref="WatermarkAdorner"/>
        ///     class</summary>
        /// <param name="adornedElement"><see cref="System.Windows.UIElement"/> to be adorned</param>
        /// <param name="watermark">The watermark</param>
        public WatermarkAdorner(System.Windows.UIElement adornedElement, object watermark)
            : base(adornedElement)
        {
            IsHitTestVisible = false;

            contentPresenter = new System.Windows.Controls.ContentPresenter();
            contentPresenter.Content = watermark;
            contentPresenter.Opacity = 0.5;
            contentPresenter.Margin = new System.Windows.Thickness(
                Control.Margin.Left + Control.Padding.Left, 
                Control.Margin.Top + Control.Padding.Top, 
                0, 
                0);

            // Hide the control adorner when the adorned element is hidden
            var binding = new System.Windows.Data.Binding("IsVisible");
            binding.Source = adornedElement;
            binding.Converter = new System.Windows.Controls.BooleanToVisibilityConverter();
            SetBinding(VisibilityProperty, binding);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        ///     Gets the number of children for the <see cref="System.Windows.Media.ContainerVisual" /> .
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        #endregion

        #region Private Properties

        /// <summary>
        ///     Gets the control that is being adorned
        /// </summary>
        private System.Windows.Controls.Control Control
        {
            get
            {
                return (System.Windows.Controls.Control)AdornedElement;
            }
        }

        #endregion

        #region Protected Overrides

        /// <summary>Returns a specified child <see cref="System.Windows.Media.Visual"/> for the parent<see cref="System.Windows.Media.ContainerVisual"/> .</summary>
        /// <param name="index"><para>A 32-bit signed integer that represents the index value of the child<see cref="System.Windows.Media.Visual"/> . The value of index must be between 0 and<see cref="JaStDev.HAB.Designer.WPF.Controls.WatermarkAdorner.VisualChildrenCount"/></para>
        /// <list type="bullet"><item><description>1.</description></item>
        /// </list>
        /// </param>
        /// <returns>The child <see cref="System.Windows.Media.Visual"/> .</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        /// <summary>Implements any custom measuring behavior for the adorner.</summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>A <see cref="System.Windows.Size"/> object representing the amount of layout space
        ///     needed by the adorner.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        /// <summary>When overridden in a derived class, positions child elements and
        ///     determines a size for a <see cref="System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to
        ///     arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            contentPresenter.Arrange(new System.Windows.Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}