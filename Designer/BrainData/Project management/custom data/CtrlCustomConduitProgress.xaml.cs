// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlCustomConduitProgress.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlCustomImportProgress.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlCustomImportProgress.xaml
    /// </summary>
    public partial class CtrlCustomConduitProgress : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlCustomConduitProgress"/> class.</summary>
        public CtrlCustomConduitProgress()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the BtnCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = DataContext as CustomConduit;
            if (iData != null && iData.Selector.Process != null)
            {
                iData.Selector.Process.Cancel();
            }
        }
    }
}