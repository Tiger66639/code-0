// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemCondionalPartBorder.cs" company="">
//   
// </copyright>
// <summary>
//   A border that implements some <see langword="interface" /> actions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A border that implements some <see langword="interface" /> actions.
    /// </summary>
    public class FlowItemCondionalPartBorder : System.Windows.Controls.Border
    {
        /// <summary>The on mouse down.</summary>
        /// <param name="e">The e.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            var iItem = DataContext as FlowItem;
            iItem.IsSelected = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control
                                   ? !iItem.IsSelected
                                   : true;
            base.OnMouseDown(e);
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