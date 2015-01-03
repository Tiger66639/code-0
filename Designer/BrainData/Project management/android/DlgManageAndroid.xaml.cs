// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgManageAndroid.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgManageAndroid.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgManageAndroid.xaml
    /// </summary>
    public partial class DlgManageAndroid : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgManageAndroid"/> class.</summary>
        public DlgManageAndroid()
        {
            InitializeComponent();
            AndroidManager.StartServer();
            RetrieveAndroidDevices();
        }

        /// <summary>The retrieve android devices.</summary>
        private void RetrieveAndroidDevices()
        {
            CmbDevices.ItemsSource = AndroidManager.GetDevices();
            CmbDevices.SelectedIndex = 0;
        }

        /// <summary>The retrieve android projects.</summary>
        private void RetrieveAndroidProjects()
        {
            var iDev = CmbDevices.SelectedItem as string;
            if (iDev != null)
            {
                LstItems.ItemsSource = AndroidManager.GetProjects(iDev);
            }
            else
            {
                LstItems.ItemsSource = null;
            }
        }

        /// <summary>Handles the SelectionChanged event of the ListBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BtnDownload.IsEnabled = BtnDelete.IsEnabled = LstItems.SelectedItem != null;
        }

        /// <summary>The btn download_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AndroidManager.DownloadProject((string)CmbDevices.SelectedItem, (string)LstItems.SelectedItem);
        }

        /// <summary>The btn delete_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iMgr = new AndroidManager();
            iMgr.EndedOk += delegate { RetrieveAndroidProjects(); // when we have uploaded an item, refresh the list.
            };
            iMgr.DeleteProject((string)CmbDevices.SelectedItem, (string)LstItems.SelectedItem);
        }

        /// <summary>The btn upload_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnUpload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = ProjectLoader.GetOpenDlg();
            if (iDlg.ShowDialog() == true)
            {
                var iMgr = new AndroidManager();
                iMgr.EndedOk += delegate { RetrieveAndroidProjects(); // when we have uploaded an item, refresh the list.
                };
                iMgr.UploadProject((string)CmbDevices.SelectedItem, iDlg.FileName);
            }
        }

        /// <summary>The cmb devices_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CmbDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RetrieveAndroidProjects();
        }

        /// <summary>The window_ closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            AndroidManager.StopServer();
        }
    }
}