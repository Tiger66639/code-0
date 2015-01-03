// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScrollablePanel.cs" company="">
//   
// </copyright>
// <summary>
//   A panel that provides calculated values for scrollbars.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A panel that provides calculated values for scrollbars.
    /// </summary>
    public abstract class ScrollablePanel : System.Windows.Controls.Panel
    {
        #region ScrollAdded

        /// <summary>
        ///     Gets/sets the size that was added by the scrollbars because they went out of the region.
        /// </summary>
        /// <value>The scroll added.</value>
        public System.Windows.Size ScrollAdded
        {
            get
            {
                return fScrollAdded;
            }

            set
            {
                fScrollAdded = value;
            }
        }

        #endregion

        #region fields

        /// <summary>
        ///     The amount of space that is added to <see cref="HorizontalMax" /> and <see cref="VerticalMax" /> after calculating
        ///     these values, to add some empty space. This is also added whenever the edge is reached, so we can have an infinite
        ///     space.
        /// </summary>
        public const double BORDERSIZE = 30.0;

        /// <summary>The f scroll added.</summary>
        private System.Windows.Size fScrollAdded; // the size added by scrolling outside of area.

        #endregion

        #region HorizontalOffset

        /// <summary>
        ///     HorizontalOffset Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty HorizontalOffsetProperty =
            System.Windows.DependencyProperty.Register(
                "HorizontalOffset", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(
                    0.0, 
                    System.Windows.FrameworkPropertyMetadataOptions.AffectsArrange, 
                    OnHorizontalOffsetChanged, 
                    CoerceHorizontalOffsetValue));

        /// <summary>
        ///     Gets or sets the HorizontalOffset property.  This dependency property
        ///     indicates how much the top of the display needs to move down.
        /// </summary>
        public double HorizontalOffset
        {
            get
            {
                return (double)GetValue(HorizontalOffsetProperty);
            }

            set
            {
                SetValue(HorizontalOffsetProperty, value);
            }
        }

        /// <summary>Handles changes to the HorizontalOffset property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnHorizontalOffsetChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ScrollablePanel)d).OnHorizontalOffsetChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the HorizontalOffset property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnHorizontalOffsetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iMax = HorizontalMax;
            var iNew = (double)e.NewValue;
            if (iNew - (double)e.OldValue > 0)
            {
                // if we are going down
                if (iNew >= iMax)
                {
                    fScrollAdded.Width += BORDERSIZE;
                    CalculateScrollValues();
                }
            }
            else
            {
                if (ScrollAdded.Width > 0)
                {
                    var iNewVal = ScrollAdded.Width - (iMax - iNew);
                    fScrollAdded.Width = iNewVal >= 0 ? iNewVal : 0;
                    CalculateScrollValues();
                }
            }
        }

        /// <summary>Coerces the test value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoerceHorizontalOffsetValue(System.Windows.DependencyObject d, object value)
        {
            return ((ScrollablePanel)d).CoerceHorizontalOffsetValue(value);
        }

        /// <summary>Coerces the horizontal offset value. By default, simply returns the value (doesn't check if within min and max),
        ///     so that the scroll area can auto expand.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected virtual object CoerceHorizontalOffsetValue(object value)
        {
            var iVal = (double)value;
            return System.Math.Min(System.Math.Max(iVal, HorizontalMin), HorizontalMax);
        }

        #endregion

        #region VerticalOffset

        /// <summary>
        ///     VerticalOffset Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty VerticalOffsetProperty =
            System.Windows.DependencyProperty.Register(
                "VerticalOffset", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(
                    0.0, 
                    System.Windows.FrameworkPropertyMetadataOptions.AffectsArrange, 
                    OnVerticalOffsetChanged, 
                    CoerceVerticalOffsetValue));

        /// <summary>
        ///     Gets or sets the VerticalOffset property.  This dependency property
        ///     indicates how much  we need to offset to the right.
        /// </summary>
        public double VerticalOffset
        {
            get
            {
                return (double)GetValue(VerticalOffsetProperty);
            }

            set
            {
                SetValue(VerticalOffsetProperty, value);
            }
        }

        /// <summary>Handles changes to the VerticalOffset property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnVerticalOffsetChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ScrollablePanel)d).OnVerticalOffsetChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the VerticalOffset property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnVerticalOffsetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iMax = VerticalMax;
            var iNew = (double)e.NewValue;
            if (iNew - (double)e.OldValue > 0)
            {
                // if we are going down
                if (iNew >= iMax)
                {
                    fScrollAdded.Height += BORDERSIZE;
                    CalculateScrollValues();
                }
            }
            else
            {
                if (ScrollAdded.Height > 0)
                {
                    var iNewVal = ScrollAdded.Height - (iMax - iNew);
                    fScrollAdded.Height = iNewVal >= 0 ? iNewVal : 0;
                    CalculateScrollValues();
                }
            }
        }

        /// <summary>Coerces the test value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoerceVerticalOffsetValue(System.Windows.DependencyObject d, object value)
        {
            return ((ScrollablePanel)d).CoerceVerticalOffsetValue(value);
        }

        /// <summary>Coerces the horizontal offset value. By default, simply returns the value (doesn't check if within min and max),
        ///     so that the scroll area can auto expand.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected virtual object CoerceVerticalOffsetValue(object value)
        {
            var iVal = (double)value;
            return System.Math.Min(System.Math.Max(iVal, VerticalMin), VerticalMax);
        }

        #endregion

        #region HorizontalMax

        /// <summary>
        ///     HorizontalMax Read-Only Dependency Property
        /// </summary>
        protected static readonly System.Windows.DependencyPropertyKey HorizontalMaxPropertyKey =
            System.Windows.DependencyProperty.RegisterReadOnly(
                "HorizontalMax", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0, OnHorizontalMaxChanged, CoerceHorizontalMaxValue));

        /// <summary>The horizontal max property.</summary>
        public static readonly System.Windows.DependencyProperty HorizontalMaxProperty =
            HorizontalMaxPropertyKey.DependencyProperty;

        /// <summary>
        ///     Gets the HorizontalMax property.  This dependency property
        ///     indicates the total size of the mindmap. This can be used by scrollbars..
        /// </summary>
        public double HorizontalMax
        {
            get
            {
                return (double)GetValue(HorizontalMaxProperty);
            }
        }

        /// <summary>Provides a secure method for setting the HorizontalMax property.
        ///     This dependency property indicates the total size of the mindmap. This can be used by scrollbars..</summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetHorizontalMax(double value)
        {
            SetValue(HorizontalMaxPropertyKey, value);
        }

        /// <summary>Handles changes to the HorizontalMax property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnHorizontalMaxChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ScrollablePanel)d).OnHorizontalMaxChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the HorizontalMax property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnHorizontalMaxChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>Coerces the HorizontalMax value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoerceHorizontalMaxValue(System.Windows.DependencyObject d, object value)
        {
            return ((ScrollablePanel)d).CoerceHorizontalMaxValue(value);
        }

        /// <summary>Coerces the horizontal max value. Simply returns the value by default.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected virtual object CoerceHorizontalMaxValue(object value)
        {
            return value;
        }

        #endregion

        #region VerticalMax

        /// <summary>
        ///     VerticalMax Read-Only Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyPropertyKey VerticalMaxPropertyKey =
            System.Windows.DependencyProperty.RegisterReadOnly(
                "VerticalMax", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0, OnVerticalMaxChanged, CoerceVerticalMaxValue));

        /// <summary>The vertical max property.</summary>
        public static readonly System.Windows.DependencyProperty VerticalMaxProperty =
            VerticalMaxPropertyKey.DependencyProperty;

        /// <summary>
        ///     Gets the VerticalMax property.  This dependency property
        ///     indicates the total vertical size of the panel (either in pixels or in data units). Can be used by scrollbars.
        /// </summary>
        public double VerticalMax
        {
            get
            {
                return (double)GetValue(VerticalMaxProperty);
            }
        }

        /// <summary>Provides a secure method for setting the VerticalMax property.
        ///     This dependency property indicates the total vertical size of the panel (either in pixels or in data units). Can be
        ///     used by scrollbars.</summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetVerticalMax(double value)
        {
            SetValue(VerticalMaxPropertyKey, value);
        }

        /// <summary>Handles changes to the VerticalMax property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnVerticalMaxChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ScrollablePanel)d).OnVerticalMaxChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the VerticalMax property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnVerticalMaxChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>Coerces the VerticalMax value.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        private static object CoerceVerticalMaxValue(System.Windows.DependencyObject d, object value)
        {
            return ((ScrollablePanel)d).CoerceVerticalMaxValue(value);
        }

        /// <summary>Coerces the vertical max value. By default, nothing is done.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="object"/>.</returns>
        protected virtual object CoerceVerticalMaxValue(object value)
        {
            return value;
        }

        #endregion

        #region HorizontalMin

        /// <summary>
        ///     HorizontalMin Read-Only Dependency Property
        /// </summary>
        private static readonly System.Windows.DependencyPropertyKey HorizontalMinPropertyKey =
            System.Windows.DependencyProperty.RegisterReadOnly(
                "HorizontalMin", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>The horizontal min property.</summary>
        public static readonly System.Windows.DependencyProperty HorizontalMinProperty =
            HorizontalMinPropertyKey.DependencyProperty;

        /// <summary>
        ///     Gets the HorizontalMin property.  This dependency property
        ///     indicates the minimum horizontal scroll value.
        /// </summary>
        public double HorizontalMin
        {
            get
            {
                return (double)GetValue(HorizontalMinProperty);
            }
        }

        /// <summary>Provides a secure method for setting the HorizontalMin property.
        ///     This dependency property indicates the minimum horizontal scroll value.</summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetHorizontalMin(double value)
        {
            SetValue(HorizontalMinPropertyKey, value);
        }

        #endregion

        #region VerticalMin

        /// <summary>
        ///     VerticalMin Read-Only Dependency Property
        /// </summary>
        private static readonly System.Windows.DependencyPropertyKey VerticalMinPropertyKey =
            System.Windows.DependencyProperty.RegisterReadOnly(
                "VerticalMin", 
                typeof(double), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>The vertical min property.</summary>
        public static readonly System.Windows.DependencyProperty VerticalMinProperty =
            VerticalMinPropertyKey.DependencyProperty;

        /// <summary>
        ///     Gets the VerticalMin property.  This dependency property
        ///     indicates the vertical minimum scroll value.
        /// </summary>
        public double VerticalMin
        {
            get
            {
                return (double)GetValue(VerticalMinProperty);
            }
        }

        /// <summary>Provides a secure method for setting the VerticalMin property.
        ///     This dependency property indicates the vertical minimum scroll value.</summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetVerticalMin(double value)
        {
            SetValue(VerticalMinPropertyKey, value);
        }

        #endregion

        #region HorBarVisibility

        /// <summary>
        ///     HorBarVisibility Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty HorBarVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "HorBarVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Visibility.Visible, 
                    OnHorBarVisibilityChanged));

        /// <summary>
        ///     Gets or sets the HorBarVisibility property.  This dependency property
        ///     indicates wether the horizontal scrollbar should be visible or not.
        /// </summary>
        /// <remarks>
        ///     By Default,the scrollbar is always visible. It's up to the descendent to control this as he wants.
        /// </remarks>
        public System.Windows.Visibility HorBarVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(HorBarVisibilityProperty);
            }

            set
            {
                SetValue(HorBarVisibilityProperty, value);
            }
        }

        /// <summary>Handles changes to the HorBarVisibility property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnHorBarVisibilityChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ScrollablePanel)d).OnHorBarVisibilityChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the HorBarVisibility property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnHorBarVisibilityChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region VerBarVisibility

        /// <summary>
        ///     VerBarVisibility Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty VerBarVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "VerBarVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(ScrollablePanel), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Visible));

        /// <summary>
        ///     Gets or sets the VerBarVisibility property.  This dependency property
        ///     indicates weather the vertical scrollbar should be visible or not.
        /// </summary>
        /// <remarks>
        ///     By Default,the scrollbar is always visible. It's up to the descendent to control this as he wants.
        /// </remarks>
        public System.Windows.Visibility VerBarVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(VerBarVisibilityProperty);
            }

            set
            {
                SetValue(VerBarVisibilityProperty, value);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Calculates the max scroll values and assigns them to the properties. This is abstract cause it depends on
        ///     the implementation.
        /// </summary>
        protected internal abstract void CalculateScrollValues();

        /// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged"/> event, using the specified information as
        ///     part of
        ///     the eventual event data.</summary>
        /// <remarks>When the size has changed, we need to update the horizontal and vertical max scroll values.</remarks>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            CalculateScrollValues();
            base.OnRenderSizeChanged(sizeInfo);
        }

        #endregion
    }
}