// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgCustomConduit.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgCustomImport.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgCustomImport.xaml
    /// </summary>
    public partial class DlgCustomConduit : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgCustomConduit"/> class.</summary>
        public DlgCustomConduit()
        {
            InitializeComponent();
        }

        /// <summary>Handles the Click event of the Button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iContext = (CustomConduit)DataContext;
            System.Diagnostics.Debug.Assert(iContext != null);
            if (string.IsNullOrEmpty(iContext.Selector.CustomDll)
                || System.IO.File.Exists(iContext.Selector.CustomDll) == false)
            {
                System.Windows.MessageBox.Show(
                    "Please provide a valid dll", 
                    "Missing data", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            if (iContext.Selector.SelectedEntryPoint < 0)
            {
                System.Windows.MessageBox.Show(
                    "Please select an entry point", 
                    "Missing data", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            DialogResult = true;
        }
    }
}