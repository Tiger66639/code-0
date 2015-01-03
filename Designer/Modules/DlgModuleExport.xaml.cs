// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgModuleExport.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgModuleExport.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Dialogs
{
    /// <summary>
    ///     Interaction logic for DlgModuleExport.xaml
    /// </summary>
    public partial class DlgModuleExport : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgModuleExport"/> class.</summary>
        public DlgModuleExport()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the path.
        /// </summary>
        /// <remarks>
        ///     is normal prop, so we can easely reach the value (without having to
        ///     write to much code and stuff in wpf)
        /// </remarks>
        /// <value>
        ///     The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>Gets or sets the module name.</summary>
        public string ModuleName { get; set; }

        /// <summary>Gets or sets the major version.</summary>
        public int MajorVersion { get; set; }

        /// <summary>Gets or sets the minor version.</summary>
        public int MinorVersion { get; set; }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ModuleName))
            {
                System.Windows.MessageBox.Show(
                    "Please provide a name for the module", 
                    "Export module", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
            else if (string.IsNullOrEmpty(Path) || System.IO.Directory.Exists(Path) == false)
            {
                System.Windows.MessageBox.Show(
                    "Please provide valid path for the module", 
                    "Export module", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
            else
            {
                DialogResult = true;
            }
        }

        /// <summary>Handles the Click event of the BtnName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnName_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Ooops, Still to do");
        }

        /// <summary>The btn path_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnPath_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
            {
                iDialog.SelectedPath = ProjectManager.Default.Location;
            }

            iDialog.Description = "Please select the export directory.";
            iDialog.ShowNewFolderButton = true;
            var iRes = iDialog.ShowDialog();
            if (iRes == System.Windows.Forms.DialogResult.OK)
            {
                TxtPath.Text = iDialog.SelectedPath;
            }
        }
    }
}