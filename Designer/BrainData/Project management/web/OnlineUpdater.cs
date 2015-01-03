// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlineUpdater.cs" company="">
//   
// </copyright>
// <summary>
//   updates the settings of the online installation (the form, css, default
//   api,...)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     updates the settings of the online installation (the form, css, default
    ///     api,...)
    /// </summary>
    internal class OnlineUpdater : FtpUploader
    {
        /// <summary>The f upload folder.</summary>
        protected string fUploadFolder;

        /// <summary>
        ///     starts the installation process.
        /// </summary>
        public virtual void Install()
        {
            PrepareForInstall(false); // we don't clear the content, only the files that we want to replace.
            System.Action iUpload = InternalUpload;
            iUpload.BeginInvoke(null, null);
        }

        /// <summary>The prepare for install.</summary>
        /// <param name="clearDest">The clear dest.</param>
        protected void PrepareForInstall(bool clearDest)
        {
            fUploadFolder = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), 
                System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName()));

                // need to get a temp dir to build all the stuff.
            System.IO.Directory.CreateDirectory(fUploadFolder);
            var iLoc = BrainData.Current.DesignerData.OnlineInfo.FTPLocation;
            if (iLoc.StartsWith("ftp://") == false)
            {
                iLoc = "ftp://" + iLoc;
            }

            iLoc = PathUtil.VerifyPathEnd(iLoc, '/'); // make certain that the path ends with a /
            var iUri = new System.Uri(iLoc);
            var iServerLoc = new System.Uri(iLoc);
            Init(fUploadFolder, iUri, iServerLoc, clearDest);
        }

        /// <summary>
        ///     the async version of the upload process.
        /// </summary>
        protected override void InternalUpload()
        {
            try
            {
                if (PrepareUploadFolder())
                {
                    CompressUploadFolder();
                    base.InternalUpload();
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
                LogService.Log.LogError("FTP", ex.Message);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }

        /// <summary>
        ///     Compresses the content of the upload folder and clears it so that only
        ///     the zip file remains. This is done to speed up the upload process.
        /// </summary>
        private void CompressUploadFolder()
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        ///     only when the whole operation has finished ok, can we store the
        ///     installation location. Otherwise the system thinks it is installed
        ///     when it is not yet.
        /// </summary>
        protected override void EndOk()
        {
            base.EndOk();
            BrainData.Current.DesignerData.OnlineInfo.IsInstalled = true;
            ProjectManager.Default.ProjectChanged = true; // need to indicate that things need to be saved.
        }

        /// <summary>copies all the data that needs tob be uploaded to a temp folder so
        ///     that the uploader can work easily and everything is prepared
        ///     correctly.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected virtual bool PrepareUploadFolder()
        {
            try
            {
                var iAppPath =
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                var iAspApp = System.IO.Path.Combine(iAppPath, "online.zip");
                if (System.IO.File.Exists(iAspApp))
                {
                    using (var zip = Ionic.Zip.ZipFile.Read(iAspApp))
                    {
                        var e = zip["web.config"];
                        e.Extract(fUploadFolder, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    }

                    BuildWebConfig();
                    CopyTemplateFiles();
                }
                else
                {
                    LogService.Log.LogError("FTP", "Failed to find online content");
                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("FTP", "Failed to prepare content with error: " + e.Message);
                return false;
            }
        }

        /// <summary>
        ///     copies over the template files (css and html) to the upload dir.
        /// </summary>
        protected void CopyTemplateFiles()
        {
            if (string.IsNullOrEmpty(BrainData.Current.DesignerData.OnlineInfo.CSSFile) == false)
            {
                System.IO.File.Copy(
                    BrainData.Current.DesignerData.OnlineInfo.CSSFile, 
                    System.IO.Path.Combine(fUploadFolder, "Content", "Site.css"), 
                    true);
            }

            if (string.IsNullOrEmpty(BrainData.Current.DesignerData.OnlineInfo.HtmlTemplate) == false)
            {
                System.IO.File.Copy(
                    BrainData.Current.DesignerData.OnlineInfo.HtmlTemplate, 
                    System.IO.Path.Combine(fUploadFolder, "Views", "Shared", "_Layout.cshtml"), 
                    true);
            }
        }

        /// <summary>
        ///     rebuilds the web.config file so that it contains the settings that the
        ///     user selected.
        /// </summary>
        protected void BuildWebConfig()
        {
            var iPath = System.IO.Path.Combine(fUploadFolder, "web.config");
            if (System.IO.File.Exists(iPath))
            {
                var iCustomPage = string.IsNullOrEmpty(BrainData.Current.DesignerData.OnlineInfo.HtmlTemplate) == false;
                var iData = BrainData.Current.DesignerData.OnlineInfo;
                var iConfig = System.IO.File.ReadAllText(iPath);
                var iServerPath = PathUtil.VerifyPathEnd(iData.ServerPath, '\\');
                var iRes = string.Format(
                    iConfig, 
                    iData.DefaultController, 
                    iData.User, 
                    iData.TimeOut, 
                    iData.IPFilter, 
                    iData.HtmlAsPartial, 
                    iCustomPage, 
                    Settings.MaxConcurrentProcessors, 
                    Settings.InitProcessorStackSize, 
                    Settings.MinReservedForBlocked, 
                    System.IO.Path.GetFileNameWithoutExtension(ProjectManager.Default.DesignerFile), 
                    iServerPath, 
                    iData.CTLocation, 
                    iData.CTLocation);
                System.IO.File.WriteAllText(iPath, iRes);
            }
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to
        ///     put common code that always needs to be called at the end. When you
        ///     <see langword="override" /> this function, don't forget to call the
        ///     base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            try
            {
                if (string.IsNullOrEmpty(fUploadFolder) == false)
                {
                    System.IO.Directory.Delete(fUploadFolder, true);
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogWarning(
                    "FTP", 
                    string.Format("Failed to delete temporary directory {0}, error: ", fUploadFolder, e.Message));
            }

            base.PerformOnEnd();
        }
    }
}