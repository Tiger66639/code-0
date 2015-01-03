// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FtpOperation.cs" company="">
//   
// </copyright>
// <summary>
//   base class for ftp operations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for ftp operations.
    /// </summary>
    internal class FtpOperation : ProjectOperation
    {
        /// <summary>The connectiongroup.</summary>
        private const string CONNECTIONGROUP = "FTPOPERATION1";

                             // when all the requests use the same connectionGroup, things are faster.

        /// <summary>The f credentials.</summary>
        protected System.Net.NetworkCredential fCredentials;

                                               // so we can  reuse these for all the connections: speeds up the upload.

        /// <summary>The f end ok.</summary>
        protected bool fEndOk = true;

        /// <summary>The f server loc.</summary>
        protected System.Uri fServerLoc;

        /// <summary>
        ///     restarts a previously stopped server by removing the App_Offline.htm
        ///     file so that asp.net can load the server again.
        /// </summary>
        protected void StartServer()
        {
            var iHtmDest = new System.Uri(fServerLoc, "App_Offline.htm");

                // the App_Offline.htm file is recognzied by asp.net. when it is copied in to the dir of the service: IIS will stop the server.
            try
            {
                var iRequest = GetDeleteRequest(iHtmDest);
                using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
            }
            catch (System.Net.WebException e)
            {
                fEndOk = false;
                var iResponse = (System.Net.FtpWebResponse)e.Response;
                throw new System.InvalidOperationException(
                    string.Format("Failed to restart the server with error: ", iResponse.StatusDescription));
            }
        }

        /// <summary>
        ///     Stops the server by sending an App_Offline.htm to the root of the
        ///     server so tha the asp.net server stops.
        /// </summary>
        protected void StopServer()
        {
            LogService.Log.LogInfo("FTP", "Stopping server");
            var iAppPath =
                System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var iHtmStopPath = System.IO.Path.Combine(iAppPath, "DefaultData", "App_Offline.htm");

                // the App_Offline.htm file is recognzied by asp.net. when it is copied in to the dir of the service: IIS will stop the server.
            var iDest = new System.Uri(fServerLoc, "App_Offline.htm");
            if (FileExists(iDest) == false)
            {
                try
                {
                    var iRequest = GetUploadRequest(iHtmStopPath, iDest);
                    using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
                }
                catch (System.Net.WebException e)
                {
                    fEndOk = false;
                    var iResponse = (System.Net.FtpWebResponse)e.Response;
                    throw new System.InvalidOperationException(
                        string.Format("Failed to stop server with error: ", iResponse.StatusDescription));
                }
            }
            else
            {
                LogService.Log.LogInfo("FTP", "server was already stopped.");
            }
        }

        #region helpers

        /// <summary>delets the fiel from the server.</summary>
        /// <param name="path"></param>
        protected void DeleteFile(System.Uri path)
        {
            try
            {
                var iRequest = GetDeleteRequest(path);
                using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
            }
            catch (System.Net.WebException e)
            {
                fEndOk = false;
                var iResponse = (System.Net.FtpWebResponse)e.Response;
                throw new System.InvalidOperationException(
                    string.Format("Failed to delete '{0}' from server. Error: ", path, iResponse.StatusDescription));
            }
        }

        /// <summary>creates a new dir on the specified path. This is synchronous, so when
        ///     the call returns, the directory is created.</summary>
        /// <param name="path"></param>
        protected void CreateDir(System.Uri path)
        {
            try
            {
                var iRequest = GetDirRequest(path, System.Net.WebRequestMethods.Ftp.MakeDirectory);
                using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
            }
            catch (System.Net.WebException e)
            {
                fEndOk = false;
                var iResponse = (System.Net.FtpWebResponse)e.Response;
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to create directory '{0}' on server. Error: ", 
                        path, 
                        iResponse.StatusDescription));
            }
        }

        /// <summary>creates a new dir on the specified <paramref name="path"/> if it
        ///     doesn't exist yet. This is synchronous, so when the call returns, the
        ///     directory is created.</summary>
        /// <param name="path"></param>
        protected void TryCreateDir(System.Uri path)
        {
            if (DirExists(path) == false)
            {
                try
                {
                    var iRequest = GetDirRequest(path, System.Net.WebRequestMethods.Ftp.MakeDirectory);
                    using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
                }
                catch (System.Net.WebException e)
                {
                    fEndOk = false;
                    var iResponse = (System.Net.FtpWebResponse)e.Response;
                    throw new System.InvalidOperationException(
                        string.Format(
                            "Failed to create directory '{0}' on server. Error: ", 
                            path, 
                            iResponse.StatusDescription));
                }
            }
        }

        /// <summary>checks if a directory exists on the server.</summary>
        /// <param name="path"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool DirExists(System.Uri path)
        {
            var IsExists = true;
            try
            {
                var iRequest = GetDirRequest(path, System.Net.WebRequestMethods.Ftp.PrintWorkingDirectory);
                var response = (System.Net.FtpWebResponse)iRequest.GetResponse();
            }
            catch (System.Net.WebException)
            {
                IsExists = false;
            }

            return IsExists;
        }

        /// <summary>checks if a file exists on the server.</summary>
        /// <param name="path"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool FileExists(System.Uri path)
        {
            var IsExists = true;
            try
            {
                var iRequest = GetDirRequest(path, System.Net.WebRequestMethods.Ftp.GetDateTimestamp);
                var response = (System.Net.FtpWebResponse)iRequest.GetResponse();
            }
            catch (System.Net.WebException ex)
            {
                var response = (System.Net.FtpWebResponse)ex.Response;
                if (response.StatusCode == System.Net.FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    IsExists = false;
                }
                else
                {
                    throw new System.InvalidOperationException(
                        string.Format(
                            "Failed to check if {0} exists, with error: {1}", 
                            path, 
                            response.StatusDescription), 
                        ex);
                }
            }

            return IsExists;
        }

        /// <summary>deletes the specified derictory on the server.</summary>
        /// <param name="path"></param>
        protected void DeleteDir(System.Uri path)
        {
            try
            {
                var iRequest = GetDirRequest(path, System.Net.WebRequestMethods.Ftp.RemoveDirectory);
                using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse()) LogService.Log.LogInfo("FTP", iResponse.StatusDescription.Trim());
            }
            catch (System.Net.WebException e)
            {
                fEndOk = false;
                var iResponse = (System.Net.FtpWebResponse)e.Response;
                throw new System.InvalidOperationException(
                    string.Format(
                        "Failed to remove directory '{0}' on server. Error: ", 
                        path, 
                        iResponse.StatusDescription));
            }
        }

        /// <summary>Gets a webrequest that can be used for listing the content of a
        ///     directory.</summary>
        /// <param name="loc"></param>
        /// <param name="method"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected System.Collections.Generic.List<string> ListFtpContent(System.Uri loc, string method)
        {
            try
            {
                var iRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(loc);
                iRequest.Method = method;
                iRequest.Credentials = fCredentials;
                iRequest.KeepAlive = true;
                iRequest.ConnectionGroupName = CONNECTIONGROUP;
                using (var iResponse = (System.Net.FtpWebResponse)iRequest.GetResponse())
                {
                    // if the getREsponse doesn't throw an error, the operation succeeded.
                    var iRes = new System.Collections.Generic.List<string>();
                    var iContent = new System.IO.StreamReader(iRequest.GetResponse().GetResponseStream());
                    var iPath = iContent.ReadLine();
                    while (iPath != null)
                    {
                        iRes.Add(iPath);
                        iPath = iContent.ReadLine();
                    }

                    return iRes;
                }
            }
            catch (System.Exception e)
            {
                fEndOk = false;
                throw new System.InvalidOperationException(
                    string.Format("Failed to list directory content '{0}' on server. Error: ", loc, e.Message));
            }
        }

        /// <summary>builds an ftp web request object that can be used to upload content.</summary>
        /// <param name="source"></param>
        /// <param name="dest">The dest.</param>
        /// <returns>The <see cref="FtpWebRequest"/>.</returns>
        protected System.Net.FtpWebRequest GetUploadRequest(string source, System.Uri dest)
        {
            var iRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(dest);
            iRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            iRequest.Credentials = fCredentials;
            iRequest.KeepAlive = true;
            iRequest.UseBinary = true;
            iRequest.ConnectionGroupName = CONNECTIONGROUP;
            var iContent = System.IO.File.ReadAllBytes(source);
            iRequest.ContentLength = iContent.Length;
            using (var iStream = iRequest.GetRequestStream()) iStream.Write(iContent, 0, iContent.Length);
            return iRequest;
        }

        /// <summary>The get download request.</summary>
        /// <param name="dest">The dest.</param>
        /// <returns>The <see cref="FtpWebRequest"/>.</returns>
        protected System.Net.FtpWebRequest GetDownloadRequest(System.Uri dest)
        {
            var iRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(dest);
            iRequest.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
            iRequest.Credentials = fCredentials;
            iRequest.KeepAlive = true;
            iRequest.UseBinary = true;
            iRequest.ConnectionGroupName = CONNECTIONGROUP;
            return iRequest;
        }

        /// <summary>gets a request that can be used for deleting a file at the specified
        ///     location.</summary>
        /// <param name="dest"></param>
        /// <returns>The <see cref="FtpWebRequest"/>.</returns>
        protected System.Net.FtpWebRequest GetDeleteRequest(System.Uri dest)
        {
            var iRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(dest);
            iRequest.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
            iRequest.Credentials = fCredentials;
            iRequest.KeepAlive = true;
            iRequest.UseBinary = true;
            iRequest.ConnectionGroupName = CONNECTIONGROUP;
            return iRequest;
        }

        /// <summary>The get dir request.</summary>
        /// <param name="dest">The dest.</param>
        /// <param name="method">The method.</param>
        /// <returns>The <see cref="FtpWebRequest"/>.</returns>
        protected System.Net.FtpWebRequest GetDirRequest(System.Uri dest, string method)
        {
            var iRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(dest);
            iRequest.Method = method;
            iRequest.Credentials = fCredentials;
            iRequest.KeepAlive = true;
            iRequest.UseBinary = true;
            iRequest.ConnectionGroupName = CONNECTIONGROUP;
            return iRequest;
        }

        #endregion
    }
}