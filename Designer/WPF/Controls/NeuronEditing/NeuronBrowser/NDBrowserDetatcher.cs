// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBrowserDetatcher.cs" company="">
//   
// </copyright>
// <summary>
//   A helper class that detatches a popup from the routed event system so
//   that it can be used in a treeview item and still allows ListBox selection
//   inside the popup.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A helper class that detatches a popup from the routed event system so
    ///     that it can be used in a treeview item and still allows ListBox selection
    ///     inside the popup.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class NDBrowserDetatcher : System.Windows.FrameworkElement
    {
        #region Content

        /// <summary>
        ///     <see cref="Content" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ContentProperty =
            System.Windows.DependencyProperty.Register(
                "Content", 
                typeof(System.Windows.Controls.Primitives.Popup), 
                typeof(NDBrowserDetatcher), 
                new System.Windows.FrameworkPropertyMetadata(null, OnContentChanged));

        /// <summary>
        ///     Gets or sets the <see cref="Content" /> property. This dependency
        ///     property indicates ....
        /// </summary>
        public System.Windows.Controls.Primitives.Popup Content
        {
            get
            {
                return (System.Windows.Controls.Primitives.Popup)GetValue(ContentProperty);
            }

            set
            {
                SetValue(ContentProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="Content"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnContentChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NDBrowserDetatcher)d).OnContentChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="Content"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnContentChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iNew = (System.Windows.Controls.Primitives.Popup)e.NewValue;
            if (iNew != null)
            {
                var iTarget = PlacementTarget;
                if (iTarget == null)
                {
                    iTarget = this;
                }

                iNew.PlacementTarget = iTarget;
            }
        }

        #endregion

        #region PlacementTarget

        /// <summary>
        ///     <see cref="PlacementTarget" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PlacementTargetProperty =
            System.Windows.DependencyProperty.Register(
                "PlacementTarget", 
                typeof(System.Windows.UIElement), 
                typeof(NDBrowserDetatcher), 
                new System.Windows.FrameworkPropertyMetadata(null, OnPlacementTargetChanged));

        /// <summary>
        ///     Gets or sets the <see cref="PlacementTarget" /> property. This
        ///     dependency property indicates the object to put the popup relative to.
        /// </summary>
        public System.Windows.UIElement PlacementTarget
        {
            get
            {
                return (System.Windows.UIElement)GetValue(PlacementTargetProperty);
            }

            set
            {
                SetValue(PlacementTargetProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="PlacementTarget"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnPlacementTargetChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NDBrowserDetatcher)d).OnPlacementTargetChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="PlacementTarget"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPlacementTargetChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iContent = Content;
            if (iContent != null)
            {
                iContent.PlacementTarget = (System.Windows.UIElement)e.NewValue;
            }
        }

        #endregion

        #region IsOpen

        /// <summary>
        ///     <see cref="IsOpen" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsOpenProperty =
            System.Windows.DependencyProperty.Register(
                "IsOpen", 
                typeof(bool), 
                typeof(NDBrowserDetatcher), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IsOpen" /> property. This dependency
        ///     property indicates if the item is open or not.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }

            set
            {
                SetValue(IsOpenProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="IsOpen"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsOpenChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NDBrowserDetatcher)d).OnIsOpenChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="IsOpen"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIsOpenChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iPopup = Content;
            if (iPopup != null)
            {
                iPopup.IsOpen = (bool)e.NewValue;
            }
        }

        #endregion
    }
}