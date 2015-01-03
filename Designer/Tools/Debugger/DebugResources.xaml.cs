// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugResources.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DebugResources.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DebugResources.xaml
    /// </summary>
    public partial class DebugResources : System.Windows.ResourceDictionary
    {
        /// <summary>Initializes a new instance of the <see cref="DebugResources"/> class.</summary>
        public DebugResources()
        {
            InitializeComponent();
        }

        /// <summary>Handles the MouseDown event of the Grid control.</summary>
        /// <remarks>need to make the <paramref name="sender"/> focused.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                iSender.Focus();
            }
        }

        /// <summary>So the commands work properly when an item gets selected.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void BD_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;
            if (iSender != null)
            {
                iSender.Focus();
                e.Handled = true;
            }
        }
    }
}