// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlineDataSynchronizer.cs" company="">
//   
// </copyright>
// <summary>
//   bundles a download, sync and upload operation for the data part of a
//   chatbot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     bundles a download, sync and upload operation for the data part of a
    ///     chatbot.
    /// </summary>
    internal class OnlineDataSynchronizer : ProjectOperation
    {
        /// <summary>The f local.</summary>
        private string fLocal;

        /// <summary>The f online.</summary>
        private System.Uri fOnline;

        /// <summary>
        ///     starts the installation process.
        /// </summary>
        public void Sync()
        {
            if (CheckSaveProject(StartDownload))
            {
                // before closing the project, make certain that it is saved.
                return;
            }

            StartDownload(this, System.EventArgs.Empty);
        }

        /// <summary>The start download.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void StartDownload(object sender, System.EventArgs e)
        {
            var iSettings = new SyncSettings();
            var iDlg = new DlgOnlineSync();
            iDlg.DataContext = iSettings;
            iDlg.Owner = WindowMain.Current;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                fLocal = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), 
                    System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName()));

                    // need to get a temp dir to build all the stuff.
                System.IO.Directory.CreateDirectory(fLocal);
                var iLocation = new System.Uri(BrainData.Current.DesignerData.OnlineInfo.FTPLocation);
                fOnline = new System.Uri(iLocation, "Data");
                var iDownload = new FtpDownloader();
                iDownload.EndedOk += StartSync;
                iDownload.Download(fOnline, fLocal, BrainData.Current.DesignerData.OnlineInfo.FTPLocation);
            }
        }

        /// <summary>starts the synchronization process.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSync(object sender, System.EventArgs e)
        {
            var iSync = new ProjectSynchronizer();
            iSync.EndedOk += StartUpload;
            iSync.SynchronizeWith(fLocal);
        }

        /// <summary>starts the upload.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartUpload(object sender, System.EventArgs e)
        {
            var iUploader = new FtpUploader();
            iUploader.EndedOk += ProcessFinished;
            iUploader.Upload(fLocal, fOnline, new System.Uri(BrainData.Current.DesignerData.OnlineInfo.FTPLocation));
        }

        /// <summary>The process finished.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ProcessFinished(object sender, System.EventArgs e)
        {
            System.Windows.MessageBox.Show("Synchronization finished.");
        }
    }
}