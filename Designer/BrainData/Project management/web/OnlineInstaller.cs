// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlineInstaller.cs" company="">
//   
// </copyright>
// <summary>
//   An ftp uploader that makes can be used to install an online chatbot. It will prepare the data to upload with the
//   selections made through the desktop version.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An ftp uploader that makes can be used to install an online chatbot. It will prepare the data to upload with the
    ///     selections made through the desktop version.
    /// </summary>
    internal class OnlineInstaller : OnlineUpdater
    {
        /// <summary>The f full.</summary>
        private bool fFull = true; // can also install only the database.

        /// <summary>
        ///     starts the installation process.
        /// </summary>
        public override void Install()
        {
            if (CheckSaveProject(InstallInternal))
            {
                // before closing the project, make certain that it is saved.
                return;
            }

            if (string.IsNullOrEmpty(ProjectManager.Default.DesignerFile))
            {
                System.Windows.MessageBox.Show(
                    "Can only install previously saved projects!", 
                    "Installation", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                LogService.Log.LogError("FTP", "Can only install previously saved projects.");
                return;
            }

            try
            {
                InstallInternal(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                LogService.Log.LogError("FTP", ex.ToString());
                End();
            }
        }

        /// <summary>
        ///     the final function to call when not all things went ok.
        /// </summary>
        protected override void End()
        {
            base.End();
            System.Windows.MessageBox.Show(
                "The operation failed! See the log for more info.", 
                "Operation failed", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }

        /// <summary>
        ///     Will only upload the database after any previous was removed.
        ///     Can be used in place of <see cref="OnlineInstaller.Install" />
        /// </summary>
        public void ReplaceDb()
        {
            fFull = false;
            Install();
        }

        /// <summary>The install internal.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InstallInternal(object sender, System.EventArgs e)
        {
            PrepareForInstall(fFull);
            System.Action iUpload = InternalUpload;
            iUpload.BeginInvoke(null, null);
        }

        /// <summary>copies all the data that needs tob be uploaded to a temp folder so that the uploader can work easily and everything
        ///     is prepared correctly.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool PrepareUploadFolder()
        {
            try
            {
                return PrepareUploadFolderInternal();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("FTP", "Failed to prepare content with error: " + e.Message);
                return false;
            }
        }

        /// <summary>
        ///     copies all the data that needs tob be uploaded to a temp folder so that the uploader can work easily and everything
        ///     is prepared correctly.
        /// </summary>
        /// <returns>True if the operation was succesful, otherwise false</returns>
        protected bool PrepareUploadFolderInternal()
        {
            if (fFull)
            {
                var iAppPath =
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                if (ExtractZip(iAppPath) == false)
                {
                    return false;
                }

                BuildWebConfig();
                CopyTemplateFiles();
            }

            CopyDatabase(fUploadFolder);
            return true;
        }

        /// <summary>extracts the zip content.</summary>
        /// <param name="appPath"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ExtractZip(string appPath)
        {
            var iAspApp = System.IO.Path.Combine(appPath, "online.zip");
            if (System.IO.File.Exists(iAspApp))
            {
                using (var zip = Ionic.Zip.ZipFile.Read(iAspApp))
                {
                    foreach (var e in zip)
                    {
                        e.Extract(fUploadFolder);
                    }
                }
            }
            else
            {
                LogService.Log.LogError("FTP", "Failed to find content that needs to be installed");
                return false;
            }

            // iAspApp = Path.Combine(appPath, "assemblies.zip");
            return true;
        }

        /// <summary>copies over the database to the installation directory so that everything is ready.</summary>
        /// <param name="to">The to.</param>
        private void CopyDatabase(string to)
        {
            var iTo = System.IO.Path.Combine(to, "data");
            System.IO.Directory.CreateDirectory(iTo);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(iTo, "Logs"));
            var iFile = System.IO.Path.Combine(iTo, System.IO.Path.GetFileName(ProjectManager.Default.DesignerFile));
            iTo = System.IO.Path.Combine(
                iTo, 
                System.IO.Path.GetFileNameWithoutExtension(ProjectManager.Default.DesignerFile));
            CopyData(iTo, iFile);
        }
    }
}