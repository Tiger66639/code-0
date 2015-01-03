// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FtpUploader.cs" company="">
//   
// </copyright>
// <summary>
//   provides a method for uploading an entire directory structure to a remote
//   ftp site. can delete the previous data structure.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides a method for uploading an entire directory structure to a remote
    ///     ftp site. can delete the previous data structure.
    /// </summary>
    internal class FtpUploader : FtpOperation
    {
        /// <summary>The f async op count.</summary>
        private int fAsyncOpCount;

                    // need to keep track when all async operations are done, so we unblock the app at the correct moment.

        /// <summary>The f clear dest.</summary>
        private bool fClearDest;

        /// <summary>The f dest.</summary>
        private System.Uri fDest;

        /// <summary>The f source.</summary>
        private string fSource;

        /// <summary>The f uploading files.</summary>
        private bool fUploadingFiles; // so we can check if we are still uploading or all is done.

        /// <summary>The f counter lock.</summary>
        private readonly object fCounterLock = new object();

                                // so we can savely increment/decrement the nr of files that are still being processed async.

        /// <summary>The i all done signal.</summary>
        private readonly System.Threading.ManualResetEvent iAllDoneSignal = new System.Threading.ManualResetEvent(false);

                                                           // files are uploaded async, so we need to wait untill all are done.

        /// <summary>Uploads the entire directory content to the specified destination.
        ///     Before it uploads the content, it makes certain that the server is
        ///     down.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="serverLoc">The server loc.</param>
        /// <param name="clearDest">if set to <c>true</c> the destination on the server will completely
        ///     cleared, otherwise only the files being uploaded will be deleted (no
        ///     dirs).</param>
        internal void Upload(string source, System.Uri dest, System.Uri serverLoc, bool clearDest = true)
        {
            Init(source, dest, serverLoc, clearDest);
            System.Action iUpload = InternalUpload;
            iUpload.BeginInvoke(null, null);
        }

        /// <summary>The init.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="serverLoc">The server loc.</param>
        /// <param name="clearDest">The clear dest.</param>
        protected virtual void Init(string source, System.Uri dest, System.Uri serverLoc, bool clearDest = true)
        {
            fServerLoc = serverLoc;
            fDest = dest;
            fSource = source;
            fClearDest = clearDest;
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // make certain that the log is visible so that the user can follow what's happening. use this way instead of direct call, to make certain that the item is visible.
            LogService.Log.LogInfo("FTP", "===========  Upload started  ===========");
            DisableUI();
            fCredentials = new System.Net.NetworkCredential(
                BrainData.Current.DesignerData.OnlineInfo.User, 
                BrainData.Current.DesignerData.OnlineInfo.Pwd);
        }

        /// <summary>
        ///     the async version of the upload process.
        /// </summary>
        protected virtual void InternalUpload()
        {
            try
            {
                StopServer();
                if (fEndOk)
                {
                    DeleteServerContent();
                    if (fEndOk)
                    {
                        LogService.Log.LogInfo("FTP", "Uploading data");
                        UploadData();
                        if (fEndOk)
                        {
                            LogService.Log.LogInfo("FTP", "Restarting server");
                            StartServer();
                        }
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
                    string.Format("Failed to upload to '{0}'\n error: {1}.", fDest, ex.Message));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End)); // call the end when the designer data has been loaded 
            }
        }

        /// <summary>
        ///     uploads all the data.
        /// </summary>
        private void UploadData()
        {
            fUploadingFiles = true;

                // need to keep track of this, so we don't signal to soon that everything is finished downloaidng.
            try
            {
                UploadContentFrom(fSource, fDest);
            }
            finally
            {
                fUploadingFiles = false;
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
        ///     Gets all the files and directories on the remote ftp site and deletes
        ///     them. so that new content can be uploaded.
        /// </summary>
        private void DeleteServerContent()
        {
            LogService.Log.LogInfo("FTP", "Deleting previous content");
            if (fClearDest)
            {
                DelContentAt(fDest, true);
            }
            else
            {
                DeleteFilesAt(fDest, fSource);
            }
        }

        /// <summary>deletes all the files at the server taht are also found in the source.
        ///     This is used for performing updates: leave what's possible</summary>
        /// <param name="dest">The dest.</param>
        /// <param name="source">The source.</param>
        private void DeleteFilesAt(System.Uri dest, string source)
        {
            foreach (var iDir in System.IO.Directory.EnumerateDirectories(source))
            {
                // create the sub dirs.
                var iDirShort = iDir.Substring(source.Length + 1);
                var iDest = new System.Uri(dest, PathUtil.VerifyPathEnd(iDirShort));

                    // we need to get the substring of the path, otherwise we get the full path. + 1 to get rid of the \\
                DeleteFilesAt(iDest, iDir);
            }

            foreach (var iFile in System.IO.Directory.EnumerateFiles(source))
            {
                var iDest = new System.Uri(dest, System.IO.Path.GetFileName(iFile));
                if (FileExists(iDest))
                {
                    // is a little slower, but more saver.
                    DeleteFile(iDest);
                }
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
            LogService.Log.LogInfo("FTP", "===========  Upload finished  ===========");
        }

        #region helpers

        /// <summary>recursively copies the content from the specified location to the
        ///     specified loc.</summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private void UploadContentFrom(string source, System.Uri dest)
        {
            foreach (var iDir in System.IO.Directory.EnumerateDirectories(source))
            {
                // create the sub dirs.
                var iDirShort = iDir.Substring(source.Length + 1);
                var iDestShort = new System.Uri(dest, iDirShort);
                var iDest = new System.Uri(dest, PathUtil.VerifyPathEnd(iDirShort));

                    // we need to get the substring of the path, otherwise we get the full path. + 1 to get rid of the \\
                if (fClearDest)
                {
                    CreateDir(iDestShort);
                }
                else
                {
                    TryCreateDir(iDestShort);
                }

                UploadContentFrom(System.IO.Path.Combine(source, iDir), iDest);
            }

            foreach (var iFile in System.IO.Directory.EnumerateFiles(source))
            {
                var iDest = new System.Uri(dest, System.IO.Path.GetFileName(iFile));
                CopyFile(iFile, iDest);
            }
        }

        /// <summary>recursively delets the content at the sepecified location.</summary>
        /// <param name="dest"></param>
        /// <param name="delWebConfig">The del Web Config.</param>
        private void DelContentAt(System.Uri dest, bool delWebConfig = false)
        {
            var iContent = ListFtpContent(dest, System.Net.WebRequestMethods.Ftp.ListDirectory); // delete the sub dirs
            if (iContent != null)
            {
                foreach (var iPath in iContent)
                {
                    if (System.IO.Path.HasExtension(iPath) == false)
                    {
                        var iDest = new System.Uri(dest, PathUtil.VerifyPathEnd(iPath));

                            // we need to make certain that the path ends with a \ owtherwise we get into trouble.
                        DelContentAt(iDest);
                        DeleteDir(new System.Uri(dest, iPath));

                            // create new uri cause deleting the dir can't have a \ at end.
                    }
                    else if (iPath.EndsWith("App_Offline.htm") == false
                             && (delWebConfig == false || iPath.EndsWith("Web.config") == false))
                    {
                        // don't want to delete the app stopper + web.config cause otherwise the web-stopper doesn't function ok. web.config should be overwritten.
                        var iDest = new System.Uri(dest, iPath);
                        DeleteFile(iDest);
                    }
                }
            }
        }

        /// <summary>The copy file.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        private void CopyFile(string source, System.Uri dest)
        {
#if(DEBUG)
            LogService.Log.LogInfo("FTP", "Uploading " + source);
#endif
            var iRequest = GetUploadRequest(source, dest);
            lock (fCounterLock)

                // need to keep track of the nr of async operations still going on, so we know when the operation is done.
                fAsyncOpCount++;
            iRequest.BeginGetResponse(FileUploaded, iRequest);

                // we do an async upload so that everything happens faster.
        }

        /// <summary>callback: when file is uploaded, we need to close up everything and
        ///     check the <paramref name="result"/> state.</summary>
        /// <param name="result"></param>
        private void FileUploaded(System.IAsyncResult result)
        {
            var iRequest = result.AsyncState as System.Net.FtpWebRequest;

            System.Net.FtpWebResponse iResponse = null;
            try
            {
                iResponse = (System.Net.FtpWebResponse)iRequest.EndGetResponse(result);
                iResponse.Close();
                if (iResponse.StatusCode != System.Net.FtpStatusCode.ClosingData)
                {
                    LogService.Log.LogError("FTP", iResponse.StatusDescription);
                    fEndOk = false;
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("FTP", e.ToString());
                fEndOk = false;
            }

            lock (fCounterLock)
            {
                fAsyncOpCount--;
                if (fAsyncOpCount == 0 && fUploadingFiles == false)
                {
                    // when all async operations are done and we are no longer uploading, signal a finish.
                    iAllDoneSignal.Set();
                }
            }
        }

        #endregion
    }
}