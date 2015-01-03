// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualEditorResources.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for VisualEditorResources.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for VisualEditorResources.xaml
    /// </summary>
    public partial class VisualEditorResources : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="VisualEditorResources"/> class.</summary>
        public VisualEditorResources()
        {
            InitializeComponent();
        }

        /// <summary>The set operator_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SetOperator_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSource = e.OriginalSource as System.Windows.Controls.MenuItem;
            var iMenu = ((System.Windows.Controls.MenuItem)sender).Parent as System.Windows.Controls.ContextMenu;
            var iFrame = ((System.Windows.FrameworkElement)iMenu.PlacementTarget).DataContext as VisualItem;
            iFrame.Operator = iSource.DataContext as Neuron;
        }

        /// <summary>The menu item_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.MenuItem;
            var iTarget =
                (System.Windows.Controls.ComboBox)((System.Windows.Controls.ContextMenu)iSender.Parent).PlacementTarget;
            var iView = ControlFramework.Utility.TreeHelper.FindInTree<Editors.VisualEditorView>(iTarget);
            if (iTarget != null && iView != null)
            {
                var iEditor = iTarget.DataContext as VisualEditor;
                if (iEditor != null)
                {
                    if (iTarget == iView.CmbLowOperator)
                    {
                        iEditor.LowValOperator = null;
                    }
                    else
                    {
                        iEditor.HighValOperator = null;
                    }
                }
            }
        }
    }
}