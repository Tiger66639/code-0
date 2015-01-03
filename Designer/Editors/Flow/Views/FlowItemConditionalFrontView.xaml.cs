// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemConditionalFrontView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FlowItemConditionalFront.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for FlowItemConditionalFront.xaml
    /// </summary>
    public partial class FlowItemConditionalFrontView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItemConditionalFrontView"/> class.</summary>
        public FlowItemConditionalFrontView()
        {
            InitializeComponent();
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     This event data reports details about the mouse button that was
        ///     pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var iItem = DataContext as FlowItem;
            if (iItem != null)
            {
                iItem.IsSelected = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control
                                       ? !iItem.IsSelected
                                       : true;
            }

            Focus();
            e.Handled = true;
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Keyboard.KeyDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Left)
            {
                var iPanelItem = Tag as FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                iPanelItem.MoveLeft();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                var iPanelItem = Tag as FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                iPanelItem.MoveRight();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                var iPanelItem = Tag as FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<FlowPanel>(this);
                iPanelItem.MoveUp();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                var iPanelItem = Tag as FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<FlowPanel>(this);
                iPanelItem.MoveDown();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}