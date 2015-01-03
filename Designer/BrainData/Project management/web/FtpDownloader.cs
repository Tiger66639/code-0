// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FtpDownloader.cs" company="">
//   
// </copyright>
// <summary>
//   The ftp downloader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The ftp downloader.</summary>
    internal class FtpDownloader : FtpOperation
    {
        /// <summary>The f async op count.</summary>
        private int fAsyncOpCount;

                    // need to keep track when all async operations are done, so we unblock the app at the correct moment.

        /// <summary>The f dest.</summary>
        private string fDest;

        /// <summary>The f downloading files.</summary>
        private bool fDownloadingFiles; // so we can check if we are still downloading or all is done.

        /// <summary>The f source.</summary>
        private System.Uri fSource;

        /// <summary>The f start when done.</summary>
        private bool fStartWhenDone; // when downloading the data, we start it after the download.

        /// <summary>The f counter lock.</summary>
        private readonly object fCounterLock = new object();

                                // so we can savely increment/decrement the nr of files that are still being processed async.

        /// <summary>The i all done signal.</summary>
        private readonly System.Threading.ManualResetEvent iAllDoneSignal = new System.Threading.ManualResetEvent(false);

                                                           // files are downloaded async, so we need to wait untill all are done.

        /// <summary>downloads the entire directory content to the specified destination.
        ///     Before it downloads the content, it makes certain that the server is
        ///     down.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="serverLoc">The server Loc.</param>
        internal void Download(System.Uri source, string dest, string serverLoc)
        {
            fDest = dest;
            fSource = source;
            fServerLoc = new System.Uri(serverLoc);
            InternalStart();
        }

        /// <summary>The internal start.</summary>
        private void InternalStart()
        {
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // make certain that the log is visible so that the user can follow what's happening.
            LogService.Log.LogInfo("FTP", "===========  Download started  ===========");
            DisableUI();
            fCredentials = new System.Net.NetworkCredential(
                BrainData.Current.DesignerData.OnlineInfo.User, 
                BrainData.Current.DesignerData.OnlineInfo.Pwd);
            System.Action iUpload = InternalDownload;
            iUpload.BeginInvoke(null, null);
        }

        /// <summary>Opens a dialog box so that the user can select where to download the
        ///     items to.</summary>
        /// <param name="source"></param>
        /// <param name="serverLoc"></param>
        internal void DownloadTo(System.Uri source, string serverLoc)
        {
            var iDlg = new System.Windows.Forms.FolderBrowserDialog();
            iDlg.ShowNewFolderButton = true;
            iDlg.Description = "Select download destination";
            var iRes = iDlg.ShowDialog();
            if (iRes == System.Windows.Forms.DialogResult.OK)
            {
                if (iDlg.SelectedPath == ProjectManager.Default.Location)
                {
                    System.Windows.MessageBox.Show(
                        "Can't download to the same location as the current project.", 
                        "Download", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                    return;
                }

                fStartWhenDone = true;
                fDest = iDlg.SelectedPath;
                fSource = source;
                if (serverLoc.StartsWith("ftp://") == false)
                {
                    serverLoc = "ftp://" + serverLoc;
                }

                fServerLoc = new System.Uri(serverLoc);
                InternalStart();
            }
        }

        /// <summary>
        ///     The final function to call when the whole process succeeded. It calls
        ///     the <see cref="JaStDev.HAB.Designer.ProjectOperation.EndedOk" />
        ///     event.
        /// </summary>
        protected override void EndOk()
        {
            base.EndOk();
            if (fStartWhenDone)
            {
                var fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace(".vshost", string.Empty);
                var info = new System.Diagnostics.ProcessStartInfo(fileName);
                info.Arguments = "\""
                                 + System.IO.Path.Combine(
                                     fDest, 
                                     System.IO.Path.GetFileName(ProjectManager.Default.DesignerFile)) + "\"";
                info.UseShellExecute = false;
                var processChild = System.Diagnostics.Process.Start(info);
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
            base.PerformOnEnd();
            LogService.Log.LogInfo("FTP", "===========  Download finished  ===========");
        }

        /// <summary>The internal download.</summary>
        private void InternalDownload()
        {
            try
            {
                StopServer();
                if (fEndOk)
                {
                    LogService.Log.LogInfo("FTP", "downloading content");
                    DownloadData();
                    if (fEndOk)
                    {
                        LogService.Log.LogInfo("FTP", "Restarting server");
                        StartServer();
                    }
                }

                if (fEndOk)
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
                LogService.Log.LogError(
                    "FTP", 
                    string.Format("Failed to download '{0}', error: {1}.", fServerLoc.LocalPath, ex));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End)); // call the end when the designer data has been loaded 
            }
        }

        /// <summary>
        ///     Downloads the data.
        /// </summary>
        private void DownloadData()
        {
            PrepareDest();
            fDownloadingFiles = true;
            try
            {
                DownloadContentFrom(fSource, fDest);
            }
            finally
            {
                fDownloadingFiles = false;
                lock (fCounterLock)
                {
                    if (fAsyncOpCount == 0)
                    {
                        // could be that the upload was really fast and all is finished before we get to this point, so make certain that we don't get stuck accidentally.
                        iAllDoneSignal.Set();
                    }
                }
            }

            iAllDoneSignal.WaitOne(new System.TimeSpan(0, 5, 0));

                // wait till all async files are uploaded. we allow for a few minutes waiting untill we timeout.
        }

        /// <summary>
        ///     Prepares the destination on the harddisk.
        /// </summary>
        private void PrepareDest()
        {
            fDest = System.IO.Path.Combine(
                fDest, 
                System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));

                // we make a temp location within the download destination, to make certain we don't accidentally delete or overwrite somehting important of the user.
            if (System.IO.Directory.Exists(fDest))
            {
                System.IO.Directory.Delete(fDest, true);
            }

            System.IO.Directory.CreateDirectory(fDest);
        }

        /// <summary>The download content from.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        private void DownloadContentFrom(System.Uri source, string dest)
        {
            var iContent = ListFtpContent(source, System.Net.WebRequestMethods.Ftp.ListDirectory); // get the sub dirs
            if (iContent != null)
            {
                foreach (var iPath in iContent)
                {
                    if (System.IO.Path.HasExtension(iPath) == false)
                    {
                        var idest = System.IO.Path.Combine(dest, iPath);
                        System.IO.Directory.CreateDirectory(idest);
                        var iSource = new System.Uri(source, PathUtil.VerifyPathEnd(iPath));

                            // need to make certain that the path ends with a \ owtherwise it wont work.
                        DownloadContentFrom(iSource, idest);
                    }
                    else
                    {
                        var iSource = new System.Uri(source, iPath);
                        CopyFile(iSource, System.IO.Path.Combine(dest, iPath));
                    }
                }
            }
        }

        /// <summary>The copy file.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        private void CopyFile(System.Uri source, string dest)
        {
#if(DEBUG)
            LogService.Log.LogInfo("FTP", "Downloading " + source.AbsoluteUri);
#endif
            var iRequest = GetDownloadRequest(source);
            lock (fCounterLock)

                // need to keep track of the nr of async operations still going on, so we know when the operation is done.
                fAsyncOpCount++;
            var iOp = new DownloadOp { Request = iRequest, Destination = dest };

                // we need to pass along the destination of the downloaded content.
            iRequest.BeginGetResponse(FileDownloaded, iOp); // we do an async upload so that everything happens faster.
        }

        /// <summary>callback: when file is uploaded, we need to close up everything and
        ///     check the <paramref name="result"/> state.</summary>
        /// <param name="result"></param>
        private void FileDownloaded(System.IAsyncResult result)
        {
            var iOp = result.AsyncState as DownloadOp;
            System.Net.FtpWebResponse iResponse = null;
            try
            {
                using (iResponse = (System.Net.FtpWebResponse)iOp.Request.EndGetResponse(result))
                {
                    using (var iSourceStr = iResponse.GetResponseStream())
                    {
                        using (var iDestStr = new System.IO.FileStream(iOp.Destination, System.IO.FileMode.Create))
                        {
                            var Length = 2048;
                            var buffer = new byte[Length];
                            var bytesRead = iSourceStr.Read(buffer, 0, Length);
                            while (bytesRead > 0)
                            {
                                iDestStr.Write(buffer, 0, bytesRead);
                                bytesRead = iSourceStr.Read(buffer, 0, Length);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException exWeb)
            {
                var iError = (System.Net.FtpWebResponse)exWeb.Response;
                LogService.Log.LogError(
                    "FTP", 
                    string.Format(
                        "Failed to download file {0} with error: {1}", 
                        iOp.Destination, 
                        iError.StatusDescription));
                fEndOk = false;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("FTP", e.ToString());
                fEndOk = false;
            }

            lock (fCounterLock)
            {
                fAsyncOpCount--;
                if (fAsyncOpCount == 0 && fDownloadingFiles == false)
                {
                    // when all async operations are done and we are no longer uploading, signal a finish.
                    iAllDoneSignal.Set();
                }
            }
        }

        #region internal types

        /// <summary>
        ///     Keeps track of a download operation, so we know where to copy the file
        ///     too.
        /// </summary>
        private class DownloadOp
        {
            /// <summary>Gets or sets the request.</summary>
            public System.Net.FtpWebRequest Request { get; set; }

            /// <summary>Gets or sets the destination.</summary>
            public string Destination { get; set; }
        }

        #endregion
    }
}