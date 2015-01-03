// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgOnlineSync.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgOnlineSync.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgOnlineSync.xaml
    /// </summary>
    public partial class DlgOnlineSync : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgOnlineSync"/> class.</summary>
        public DlgOnlineSync()
        {
            InitializeComponent();
        }

        /// <summary>Need to close the dialog.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }

    /// <summary>
    ///     stores the settings used for a project synchronizatio process.
    /// </summary>
    public class SyncSettings
    {
        /// <summary>Initializes a new instance of the <see cref="SyncSettings"/> class. 
        ///     set up the defaults</summary>
        public SyncSettings()
        {
            UploadTopics = true;
            UploadAssets = false;
            DownloadAsets = true;
            UploadThes = true;
            DownloadThes = true;
            DownloadLogs = true;
        }

        /// <summary>Gets or sets a value indicating whether upload topics.</summary>
        public bool UploadTopics { get; set; }

        /// <summary>Gets or sets a value indicating whether upload assets.</summary>
        public bool UploadAssets { get; set; }

        /// <summary>Gets or sets a value indicating whether download asets.</summary>
        public bool DownloadAsets { get; set; }

        /// <summary>Gets or sets a value indicating whether upload thes.</summary>
        public bool UploadThes { get; set; }

        /// <summary>Gets or sets a value indicating whether download thes.</summary>
        public bool DownloadThes { get; set; }

        /// <summary>Gets or sets a value indicating whether download logs.</summary>
        public bool DownloadLogs { get; set; }
    }
}