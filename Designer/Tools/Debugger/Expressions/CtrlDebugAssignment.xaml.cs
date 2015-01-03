// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlDebugAssignment.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlDebugAssignment.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlDebugAssignment.xaml
    /// </summary>
    public partial class CtrlDebugAssignment : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlDebugAssignment"/> class.</summary>
        public CtrlDebugAssignment()
        {
            InitializeComponent();
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
        ///     event is raised on this element. Implement this method to add class
        ///     handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     The event data reports that the left mouse button was pressed.</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
        }
    }
}