// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AndroidManager.cs" company="">
//   
// </copyright>
// <summary>
//   provides support to manage the projects on an android device.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides support to manage the projects on an android device.
    /// </summary>
    internal class AndroidManager : ProjectOperation
    {
        /// <summary>The adb.</summary>
        private const string ADB = "adb.exe";

        /// <summary>The aici package.</summary>
        private const string AiciPackage = "com.bragisoft.aici.apk";

        /// <summary>The f project data.</summary>
        private ProjectData fProjectData; // for uploading the current project.

        /// <summary>
        ///     gets the path and file name to the android debugger, which provides comm services.
        /// </summary>
        public static string ADBExeFile
        {
            get
            {
                return
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                        ADB);
            }
        }

        /// <summary>Gets the android app file.</summary>
        public static string AndroidAppFile
        {
            get
            {
                return
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                        AiciPackage);
            }
        }

        /// <summary>
        ///     called when the processing is done.
        /// </summary>
        public event System.EventHandler Finished;

        /// <summary>Starts the server that communicates with the devices. This needs to be done before
        ///     calling any other function, otherwise deadlocks are possible.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<string> StartServer()
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iArgs = new System.Diagnostics.ProcessStartInfo();
            iArgs.FileName = ADBExeFile;
            iArgs.Arguments = "start-server"; // lists all the devices.
            iArgs.UseShellExecute = false;
            iArgs.CreateNoWindow = true;
            iArgs.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            LogService.Log.LogInfo("Android manager", "Starting android communication server.");
            using (var process = System.Diagnostics.Process.Start(iArgs)) process.WaitForExit(10000);
            return iRes;
        }

        /// <summary>The stop server.</summary>
        public static void StopServer()
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iArgs = new System.Diagnostics.ProcessStartInfo();
            iArgs.FileName = ADBExeFile;
            iArgs.Arguments = "kill-server"; // lists all the devices.
            iArgs.UseShellExecute = false;
            iArgs.CreateNoWindow = true;
            iArgs.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            using (var process = System.Diagnostics.Process.Start(iArgs)) process.WaitForExit(10000);
        }

        /// <summary>gets the list of available devices. Before you call this, make certain that you have started the server, otherwise,
        ///     you can get a deadlock.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public static System.Collections.Generic.List<string> GetDevices()
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iArgs = new System.Diagnostics.ProcessStartInfo();
            iArgs.FileName = ADBExeFile;
            iArgs.Arguments = "devices"; // lists all the devices.
            iArgs.UseShellExecute = false;
            iArgs.CreateNoWindow = true;
            iArgs.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            iArgs.RedirectStandardOutput = true;
            iArgs.RedirectStandardError = true;
            using (var process = System.Diagnostics.Process.Start(iArgs))
            {
                using (var reader = process.StandardOutput)
                {
                    // Read in all the text from the process with the StreamReader.
                    while (reader.EndOfStream == false)
                    {
                        var iLine = reader.ReadLine();
                        if (iLine.EndsWith("device"))
                        {
                            // when a line ends with device, it means that the device is connected and available, otherwise it's off line.
                            iRes.Add(iLine.Substring(0, iLine.Length - 6).Trim());
                        }
                    }
                }
            }

            return iRes;
        }

        /// <summary>The get projects.</summary>
        /// <param name="device">The device.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<string> GetProjects(string device)
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iArgs = new System.Diagnostics.ProcessStartInfo();
            iArgs.FileName = ADBExeFile;
            iArgs.Arguments = string.Format("-s {0} shell ls sdcard/aici", device);

                // list the sub-dirs in the aici dir which are all the projects.
            iArgs.UseShellExecute = false;
            iArgs.CreateNoWindow = true;
            iArgs.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            iArgs.RedirectStandardOutput = true;
            using (var process = System.Diagnostics.Process.Start(iArgs))
            {
                using (var reader = process.StandardOutput)
                {
                    // Read in all the text from the process with the StreamReader.
                    while (reader.EndOfStream == false)
                    {
                        var iLine = reader.ReadLine().Trim();
                        if (string.IsNullOrEmpty(iLine) == false && System.IO.Path.HasExtension(iLine) == false
                            && iLine.Contains("daemon started successfully") == false)
                        {
                            // don't need to add empty lines and the project files, which is returned at the end of 'ls'.
                            iRes.Add(iLine);
                        }
                    }
                }
            }

            return iRes;
        }

        /// <summary>
        ///     installs the app to a device. If there is only 1 device attached, this is used, otherwise a selection box is shown.
        /// </summary>
        internal static void InstallApp()
        {
            string idev = null;
            StartServer();
            var iDevices = GetDevices();
            if (iDevices.Count == 0)
            {
                var iRes = System.Windows.MessageBox.Show(
                    "Please connect an android device to you computer.", 
                    "Install to android", 
                    System.Windows.MessageBoxButton.OKCancel, 
                    System.Windows.MessageBoxImage.Question, 
                    System.Windows.MessageBoxResult.OK);
                if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }

                iDevices = GetDevices();
                if (iDevices.Count == 0)
                {
                    System.Windows.MessageBox.Show(
                        "No devices found", 
                        "Install on android", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Warning);
                }
            }

            if (iDevices.Count > 0)
            {
                if (iDevices.Count > 1)
                {
                    var iDlg = new DlgSelectDevice(iDevices);
                    iDlg.ShowDialog();
                    idev = iDlg.SelectedDevice;
                }
                else
                {
                    idev = iDevices[0];
                }

                if (idev != null)
                {
                    var iArgs = new System.Diagnostics.ProcessStartInfo();
                    iArgs.FileName = ADBExeFile;
                    iArgs.Arguments = string.Format("-s {0} install {1}", idev, AndroidAppFile); // install the package.
                    iArgs.UseShellExecute = false;
                    iArgs.CreateNoWindow = true;
                    iArgs.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    var iProc = System.Diagnostics.Process.Start(iArgs);
                    LogService.Log.LogInfo("Android manager", "Installing Aici on device " + idev);
                    iProc.WaitForExit(120000); // wait untill all is installed, but with a timeout.
                    StopServer();
                    System.Windows.MessageBox.Show(
                        "Installation completed", 
                        "Install on android", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    LogService.Log.LogInfo("Android manager", "Installation canceled" + idev);
                }
            }
        }

        /// <summary>The download project.</summary>
        /// <param name="device">The device.</param>
        /// <param name="project">The project.</param>
        internal static void DownloadProject(string device, string project)
        {
            var iDialog = new System.Windows.Forms.FolderBrowserDialog();

            iDialog.Description = "Download to";
            iDialog.SelectedPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var iRes = iDialog.ShowDialog();
            if (iRes == System.Windows.Forms.DialogResult.OK)
            {
                var iDest = iDialog.SelectedPath;
                var iMgr = new AndroidManager();
                WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                    // make certain that the log is visible so that the user can follow what's happening. use this way instead of direct call, to make certain that the item is visible.
                iMgr.DisableUI();
                System.Action<string, string, string> iMovDataTo = iMgr.DownloadProjectInternal;
                iMovDataTo.BeginInvoke(device, project, iDest, null, null);
            }
        }

        /// <summary>deletes a project from the specified android device.</summary>
        /// <param name="device"></param>
        /// <param name="location"></param>
        internal void DeleteProject(string device, string location)
        {
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // make certain that the log is visible so that the user can follow what's happening. use this way instead of direct call, to make certain that the item is visible.
            DisableUI();
            System.Action<string, string> iDelete = DeleteProjectInternal;
            iDelete.BeginInvoke(device, location, null, null);
        }

        /// <summary>uploads a project to the specified android device.</summary>
        /// <param name="device"></param>
        /// <param name="location"></param>
        internal void UploadProject(string device, string location)
        {
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // make certain that the log is visible so that the user can follow what's happening. use this way instead of direct call, to make certain that the item is visible.
            DisableUI();
            System.Action<string, string, string> iUpload = UploadProjectInternal;
            var iPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(location), 
                System.IO.Path.GetFileNameWithoutExtension(location));
            iUpload.BeginInvoke(device, iPath, location, null, null);
        }

        /// <summary>
        ///     uploads the current project (after saving). When there is 1 device connected, this is
        ///     automatically used, otehrwise a selection dialog is shown.
        /// </summary>
        internal static void UploadCurrentProject()
        {
            var iMgr = new AndroidManager();
            if (CheckSaveProject(iMgr.CheckSave_EndedOk) == false)
            {
                iMgr.CheckSave_EndedOk(iMgr, System.EventArgs.Empty);
            }
        }

        /// <summary>The check save_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CheckSave_EndedOk(object sender, System.EventArgs e)
        {
            WindowMain.Current.ActivateTool(ToolsList.Default.LogTool);

                // make certain that the log is visible so that the user can follow what's happening. use this way instead of direct call, to make certain that the item is visible.
            DisableUI();
            System.Action iMovDataTo = UploadCurrentProjectAsync;
            iMovDataTo.BeginInvoke(null, null);
        }

        /// <summary>The upload current project async.</summary>
        private void UploadCurrentProjectAsync()
        {
            string idev = null;
            StartServer();
            var iDevices = GetDevices();
            if (iDevices.Count > 1)
            {
                var iDlg = new DlgSelectDevice(iDevices);
                iDlg.ShowDialog();
                idev = iDlg.SelectedDevice;
            }
            else if (iDevices.Count > 0)
            {
                idev = iDevices[0];
            }

            if (idev != null)
            {
                var iStorage = Brain.Current.Storage;
                System.Diagnostics.Debug.Assert(iStorage != null);
                fProjectData = new ProjectData(iStorage);
                BrainData.Current.NeuronInfo.Store.Dispose(); // this closes all the files
                iStorage.Dispose(); // closes all the files
                EndedOk += UploadCurrentProject_EndedOk;
                UploadProjectInternal(idev, fProjectData.iPath, fProjectData.iFile);
            }
            else
            {
                LogService.Log.LogInfo("Android manager", "Upload canceled" + idev);
            }
        }

        /// <summary>The upload current project_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UploadCurrentProject_EndedOk(object sender, System.EventArgs e)
        {
            fProjectData.Restore();
            fProjectData = null;
            StopServer();
            EndedOk -= UploadCurrentProject_EndedOk;
        }

        /// <summary>Uploads the project to the specified device.</summary>
        /// <param name="dev">The dev.</param>
        /// <param name="path">The path.</param>
        /// <param name="file">The file.</param>
        private void UploadProjectInternal(string dev, string path, string file)
        {
            LogService.Log.LogInfo("Android manager", string.Format("Uploading {0} to device {1}", file, dev));
            var iProc = new System.Diagnostics.Process();
            iProc.StartInfo.FileName = ADBExeFile;
            iProc.StartInfo.Arguments = string.Format(
                "-s {0} push \"{1}\" \"/sdcard/aici/{2}\"", 
                dev, 
                path.Replace('\\', '/'), 
                System.IO.Path.GetFileNameWithoutExtension(file));

                // install the package. android tools appear to want linux characters in paths so chagne \ to /
            iProc.StartInfo.UseShellExecute = false;
            iProc.StartInfo.CreateNoWindow = true;
            iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            iProc.EnableRaisingEvents = true;
            iProc.Exited += delegate
                {
                    iProc = new System.Diagnostics.Process();
                    iProc.StartInfo.FileName = ADBExeFile;
                    iProc.StartInfo.UseShellExecute = false;
                    iProc.StartInfo.CreateNoWindow = true;
                    iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    iProc.StartInfo.Arguments = string.Format(
                        "-s {0} push \"{1}\" \"/sdcard/aici/{2}\"", 
                        dev, 
                        file, 
                        System.IO.Path.GetFileName(file));

                        // also copy the project file, so we can retrieve it during a download.
                    iProc.EnableRaisingEvents = true;
                    iProc.Exited += delegate
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action(EndOk));
                            LogService.Log.LogInfo(
                                "Android manager", 
                                string.Format("Finished uploading '{0}' to device {1}", file, dev));
                        };
                    iProc.Start();
                };
            iProc.Start();
        }

        /// <summary>Deletes the project from the specified device.</summary>
        /// <param name="dev"></param>
        /// <param name="project">The project.</param>
        private void DeleteProjectInternal(string dev, string project)
        {
            LogService.Log.LogInfo("Android manager", string.Format("Deleting '{0}' from device {1}", project, dev));
            var iProc = new System.Diagnostics.Process();
            iProc.StartInfo.FileName = ADBExeFile;
            iProc.StartInfo.Arguments = string.Format("-s {0} shell rm -r  \"sdcard/aici/{1}\"", dev, project);

                // install the package.
            iProc.StartInfo.UseShellExecute = false;
            iProc.StartInfo.CreateNoWindow = true;
            iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            iProc.EnableRaisingEvents = true;
            iProc.Exited += delegate
                {
                    iProc = new System.Diagnostics.Process();
                    iProc.StartInfo.FileName = ADBExeFile;
                    iProc.StartInfo.Arguments = string.Format("-s {0} shell rm \"sdcard/aici/{1}.dpl\"", dev, project);

                        // also copy the project file, so we can retrieve it during a download.
                    iProc.StartInfo.UseShellExecute = false;
                    iProc.StartInfo.CreateNoWindow = true;
                    iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    iProc.EnableRaisingEvents = true;
                    iProc.Exited += delegate
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action(EndOk));
                            LogService.Log.LogInfo(
                                "Android manager", 
                                string.Format("Finished deleting '{0}' from device {1}", project, dev));
                        };
                    iProc.Start();
                };
            iProc.Start();
        }

        /// <summary>The download project internal.</summary>
        /// <param name="dev">The dev.</param>
        /// <param name="project">The project.</param>
        /// <param name="location">The location.</param>
        private void DownloadProjectInternal(string dev, string project, string location)
        {
            LogService.Log.LogInfo("Android manager", string.Format("downloading '{0}' from device {1}", project, dev));
            var iProc = new System.Diagnostics.Process();
            iProc.StartInfo.FileName = ADBExeFile;
            iProc.StartInfo.Arguments = string.Format(
                "-s {0} pull \"sdcard/aici/{1}\" \"{2}\" ", 
                dev, 
                project, 
                System.IO.Path.Combine(location, project)); // install the package.
            iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            iProc.StartInfo.CreateNoWindow = true;
            iProc.StartInfo.UseShellExecute = false;
            iProc.EnableRaisingEvents = true;
            iProc.Exited += delegate
                {
                    iProc = new System.Diagnostics.Process();
                    iProc.StartInfo.FileName = ADBExeFile;
                    iProc.StartInfo.Arguments = string.Format(
                        "-s {0} pull \"sdcard/aici/{1}.dpl\" \"{2}\"", 
                        dev, 
                        project, 
                        location); // also copy the project file, so we can retrieve it during a download.
                    iProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    iProc.StartInfo.CreateNoWindow = true;
                    iProc.StartInfo.UseShellExecute = false;
                    iProc.EnableRaisingEvents = true;
                    iProc.Exited += delegate
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action(EndOk));
                            LogService.Log.LogInfo(
                                "Android manager", 
                                string.Format("Finished downloading '{0}' from device {1}", project, dev));
                        };
                    iProc.Start();
                };
            iProc.Start();
        }

        #region internal types

        /// <summary>
        ///     stores all the paths and files while the project is closed so that it can be uploaded.
        /// </summary>
        private class ProjectData
        {
            /// <summary>The i data path.</summary>
            public string iDataPath;

            /// <summary>The i file.</summary>
            public readonly string iFile;

            /// <summary>The i modules path.</summary>
            public readonly string iModulesPath;

            /// <summary>The i path.</summary>
            public readonly string iPath;

            /// <summary>Initializes a new instance of the <see cref="ProjectData"/> class.</summary>
            /// <param name="iStorage">The i storage.</param>
            public ProjectData(ILongtermMem iStorage)
            {
                iPath = ProjectManager.Default.Location;
                iFile = ProjectManager.Default.DesignerFile;
                iDataPath = iStorage.DataPath;
                iModulesPath = Brain.Current.Modules.Path;
            }

            /// <summary>The restore.</summary>
            public void Restore()
            {
                var iStorage = Brain.Current.Storage;
                System.Diagnostics.Debug.Assert(iStorage != null);
                iStorage.DataPath = iDataPath; // new path, opens the files also
                Brain.Current.Modules.Path = iModulesPath;
                ProjectManager.Default.Location = iPath;
                ProjectManager.Default.DesignerFile = iFile;
                iDataPath = System.IO.Path.Combine(iPath, NeuronDataDictionary.DATAPATH);
                BrainData.Current.NeuronInfo.Store.LoadFiles(iDataPath);
            }
        }

        #endregion
    }
}