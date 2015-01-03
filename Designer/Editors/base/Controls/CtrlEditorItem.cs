// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlEditorItem.cs" company="">
//   
// </copyright>
// <summary>
//   The ctrl editor item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The ctrl editor item.</summary>
    public class CtrlEditorItem : System.Windows.Controls.UserControl
    {
        /// <summary>Invoked when an unhandled<see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
        ///     event is raised on this element. Implement this method to add class
        ///     handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that the left mouse button was pressed.</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var iOrSource = e.OriginalSource as System.Windows.FrameworkElement;
            if (iOrSource != null && iOrSource.DataContext == DataContext)
            {
                // whe check on similar data context between this item and the original source to see if the click was done on this user control, and not one on top of this one.
                IsSelected = true;
                Focus(); // disabled because we otherwise can't handle paste correctly in code items.

                // e.Handled = true;                                                           //don't do handled, otherwise we can't init a drag.
            }
        }

        /// <summary>The on mouse right button down.</summary>
        /// <param name="e">The e.</param>
        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            var iOrSource = e.OriginalSource as System.Windows.FrameworkElement;
            if (iOrSource != null && iOrSource.DataContext == DataContext)
            {
                // whe check on similar data context between this item and the original source to see if the click was done on this user control, and not one on top of this one.
                IsSelected = true;
                Focus(); // disabled because we otherwise can't handle paste correctly in code items.

                // e.Handled = true;                                                           //don't do handled, otherwise we can't init a drag.
            }
        }

        #region IsSelected

        /// <summary>
        ///     <see cref="IsSelected" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsSelectedProperty =
            System.Windows.DependencyProperty.Register(
                "IsSelected", 
                typeof(bool), 
                typeof(CtrlEditorItem), 
                new System.Windows.FrameworkPropertyMetadata(
                    false, 
                    System.Windows.FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     Gets or sets the <see cref="IsSelected" /> property. This dependency
        ///     property indicates wether this code item is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }

            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        #endregion
    }
}