// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlinePreparer.cs" company="">
//   
// </copyright>
// <summary>
//   a specialized implementation of the <see cref="OnlineInstaller" /> class
//   which will only prepare the project and copy it to a specified directory
//   but doesn't upload it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a specialized implementation of the <see cref="OnlineInstaller" /> class
    ///     which will only prepare the project and copy it to a specified directory
    ///     but doesn't upload it.
    /// </summary>
    internal class OnlinePreparer : OnlineInstaller
    {
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
                    "Can only prepare previously saved projects!", 
                    "Installation", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
                LogService.Log.LogError("Prepare online", "Can only prepare previously saved projects.");
                return;
            }

            try
            {
                InstallInternal(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                LogService.Log.LogError("Prepare online", ex.ToString());
                End();
            }
        }

        /// <summary>The install internal.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InstallInternal(object sender, System.EventArgs e)
        {
            fUploadFolder = GetDestination();
            if (string.IsNullOrEmpty(fUploadFolder) == false)
            {
                System.Action iUpload = InternalUpload;
                iUpload.BeginInvoke(null, null);
            }
        }

        /// <summary>ask the user for the location to copy the website too.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetDestination()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var iRes = System.IO.Path.Combine(dialog.SelectedPath, BrainData.Current.Name);
                if (System.IO.Directory.Exists(iRes))
                {
                    if (
                        System.Windows.MessageBox.Show(
                            string.Format(
                                "The destination directory {0} already exists. Its current content will be lost. Do you want to continue?", 
                                iRes), 
                            "Destination already exists", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.No)
                    {
                        return null;
                    }

                    System.IO.Directory.Delete(iRes, true);
                }

                return iRes;
            }

            return null;
        }

        /// <summary>
        ///     the async version of the prepare process.
        /// </summary>
        protected override void InternalUpload()
        {
            try
            {
                var iRes = PrepareUploadFolder();
                fUploadFolder = null;

                    // reset the directory, if we don't do this, the base class will delete the freshly prepared directly, don't want that.
                if (iRes)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(EndOk));
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(End));
                }
            }
            catch (System.Exception ex)
            {
                LogService.Log.LogError("Prepare online", ex.Message);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }

        /// <summary>copies all the data that needs tob be uploaded to a temp folder so
        ///     that the uploader can work easily and everything is prepared
        ///     correctly.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool PrepareUploadFolder()
        {
            try
            {
                return PrepareUploadFolderInternal();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Prepare online", "Failed to prepare content with error: " + e.Message);
                return false;
            }
        }
    }
}