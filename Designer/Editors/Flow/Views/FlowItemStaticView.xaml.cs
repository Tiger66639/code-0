// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemStaticView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FlowItemStaticView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for FlowItemStaticView.xaml
    /// </summary>
    public partial class FlowItemStaticView : System.Windows.Controls.ContentControl
    {
        /// <summary>Initializes a new instance of the <see cref="FlowItemStaticView"/> class.</summary>
        public FlowItemStaticView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the MouseDoubleClick event of the ContentControl control.</summary>
        /// <remarks>Navigate to the link, if possible.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void ContentControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iStatic = (FlowItemStatic)DataContext;
            if (iStatic != null && iStatic.IsLink)
            {
                var iEditor = iStatic.FindFirstOwner<FlowEditor>();
                System.Diagnostics.Debug.Assert(iEditor != null);
                var iFound = (from i in iEditor.Flows where i.Item == iStatic.Item select i).FirstOrDefault();
                if (iFound == null)
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            string.Format(
                                "The flow '{0}' is not yet included in this editor, do so now?", 
                                iStatic.NeuronInfo.DisplayTitle), 
                            "Go to flow", 
                            System.Windows.MessageBoxButton.OKCancel, 
                            System.Windows.MessageBoxImage.Question, 
                            System.Windows.MessageBoxResult.OK);
                    if (iRes == System.Windows.MessageBoxResult.OK)
                    {
                        iFound = new Flow();
                        iFound.ItemID = iStatic.Item.ID;
                        iEditor.Flows.Add(iFound);
                    }
                }

                if (iFound != null)
                {
                    iEditor.SelectedFlow = iFound;
                }
            }
        }

        /// <summary>Invoked when an unhandled<see cref="System.Windows.Input.Mouse.MouseDown"/> attached event
        ///     reaches an element in its route that is derived from this class.
        ///     Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
        ///     This event data reports details about the mouse button that was
        ///     pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            var iStatic = (FlowItemStatic)DataContext;
            iStatic.IsSelected = System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control
                                     ? !iStatic.IsSelected
                                     : true;
            base.OnMouseDown(e);
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
                var iPanelItem = Tag as WPF.Controls.FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                iPanelItem.MoveLeft();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                var iPanelItem = Tag as WPF.Controls.FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                iPanelItem.MoveRight();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                var iPanelItem = Tag as WPF.Controls.FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.FlowPanel>(this);
                iPanelItem.MoveUp();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                var iPanelItem = Tag as WPF.Controls.FlowPanelItemBase;
                System.Diagnostics.Debug.Assert(iPanelItem != null);
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<WPF.Controls.FlowPanel>(this);
                iPanelItem.MoveDown();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}