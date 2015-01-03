// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectSaver.cs" company="">
//   
// </copyright>
// <summary>
//   a helper class to save projects to stream mostly in an async way.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a helper class to save projects to stream mostly in an async way.
    /// </summary>
    internal class ProjectSaver : ProjectOperation
    {
        /// <summary>The f d file.</summary>
        private string fDFile;

        /// <summary>The f path.</summary>
        private string fPath;

        /// <summary>
        ///     Tries to save the project to the current <see cref="ProjectManager.Location" />. If there is no current location,
        ///     a SaveAs is performed.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(ProjectManager.Default.Location))
            {
                // if there is no path defined, do a save as.
                SaveAs(); // we don't check data error before SaveAs, cause this does the check also.
            }
            else if (ProjectManager.Default.CheckProjectStateForSave("Save project") == false)
            {
                Start();
            }
        }

        /// <summary>
        ///     Performs a Saves as. When the user canceled the operation, no <see cref="ProjectOperation.EndOk" /> is called.
        /// </summary>
        public void SaveAs()
        {
            if (ProjectManager.Default.CheckProjectStateForSave("Save project as"))
            {
                return;
            }

            var iDlg = GetSaveDlg();
            if (string.IsNullOrEmpty(ProjectManager.Default.DesignerFile) == false)
            {
                iDlg.InitialDirectory = System.IO.Path.GetDirectoryName(ProjectManager.Default.DesignerFile);
                iDlg.FileName = System.IO.Path.GetFileName(ProjectManager.Default.DesignerFile);
            }

            if (iDlg.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(ProjectManager.Default.DesignerFile) == false)
                {
                    // if there was a previous path defined, we need to copy all the data to the new location.
                    if (ProjectManager.Default.DesignerFile != iDlg.FileName)
                    {
                        // only need to move if there is a new path: important, cause moving cleans out the new location first.
                        var iMover = new ProjectMover();
                        iMover.EndedOk += SaveAsMover_EndedOk;
                        iMover.Start(iDlg.FileName);
                    }
                }
                else
                {
                    ProjectManager.Default.DesignerFile = iDlg.FileName; // so we know the path next time.
                    fDFile = ProjectManager.Default.DesignerFile;
                    fPath = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(fDFile), 
                        System.IO.Path.GetFileNameWithoutExtension(fDFile));
                    ProjectManager.Default.Location = fPath;
                    System.Action iAsyncSetPath = delegate
                        {
                            ProjectManager.PreparePathForProject(fPath);
                            ProjectManager.AssignPathToProject(fPath);
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(InternalStart));

                                // we still need to actually save the project.
                        };
                    DisableUI();

                        // the preparePathForProject can take a long time if it needs to delete a lot of files, so we need to make certain that the ui is disabled before we begin doing this.
                    try
                    {
                        iAsyncSetPath.BeginInvoke(null, null);
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError("ProjectManager.SaveProject", e.ToString());
                        ProjectManager.Default.DataError = true;
                        End();
                    }
                }
            }
        }

        /// <summary>The get save dlg.</summary>
        /// <returns>The <see cref="SaveFileDialog"/>.</returns>
        public static Microsoft.Win32.SaveFileDialog GetSaveDlg()
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.Title = "Save project";
            iDlg.Filter = ProjectManager.FILE_DIALOG_FILTER;
            iDlg.FilterIndex = 0;
            iDlg.DefaultExt = ProjectManager.PROJECT_EXTENTION;
            iDlg.AddExtension = true;
            return iDlg;
        }

        /// <summary>Handles the EndedOk event of the SaveAsMover control, called when it's operation has ended ok.
        ///     Starts the actual operation of this class.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SaveAsMover_EndedOk(object sender, System.EventArgs e)
        {
            Start();
        }

        /// <summary>
        ///     Starts the process of saving the current project to the specified path without actually asking the
        ///     user anything. Other entry points are <see cref="ProjectSaver.Save" /> and <see cref="ProjectSaver.SaveAs" />
        /// </summary>
        /// <param name="path">The path.</param>
        private void Start()
        {
            fPath = ProjectManager.Default.Location;
            fDFile = ProjectManager.Default.DesignerFile;
            DisableUI();
            try
            {
                InternalStart();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectManager.SaveProject", e.ToString());
                ProjectManager.Default.DataError = true;
                End();
            }
        }

        /// <summary>The internal start.</summary>
        private void InternalStart()
        {
            BrainData.Current.OnBeforeSave();

            System.Action iSaveNetwork = PerformSave;
            iSaveNetwork.BeginInvoke(null, null);
        }

        /// <summary>
        ///     This function is called by End and EndOk. So it provides a location to put common code that always needs to be
        ///     called
        ///     at the end. When you override this function, don't forget to call the base.s
        /// </summary>
        protected override void PerformOnEnd()
        {
            BrainData.Current.OnAfterSave(); // this must always be done, because the OnBeforeSave is also always done.
            base.PerformOnEnd();
        }

        /// <summary>
        ///     The final function to call when the whole process succeeded. It calls
        ///     the <see cref="ProjectOperation.EndedOk" /> event.
        /// </summary>
        protected override void EndOk()
        {
            LogService.Log.LogInfo(
                "ProjectManager.SaveProject", 
                string.Format("Project '{0}' succesfully saved.", fDFile));
            ProjectManager.Default.OnProjectSaved();
            base.EndOk();
        }

        /// <summary>The perform save.</summary>
        private void PerformSave()
        {
            try
            {
                ProcessorManager.Current.StopAndUnDeadLock();
                var iPath = System.IO.Path.Combine(fPath, ProjectManager.NETWORKFILE);

                // BrainData.Current.NeuronInfo.Lock.EnterUpgradeableReadLock();
                // try
                // {
                Brain.Current.Save(iPath, PerformSaveDesigner);

                    // this allows us to save the designer data while the network is still locked.

                // }
                // finally
                // {
                // BrainData.Current.NeuronInfo.Lock.ExitUpgradeableReadLock();
                // }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogWarning(
                    "ProjectManager.SaveProject", 
                    string.Format("Failed to save Project '{0}', message: {1}.", fPath, e.Message));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End)); // call the end when the designer data has been loaded
            }
        }

        /// <summary>The perform save designer.</summary>
        private void PerformSaveDesigner()
        {
            using (var iStr = new System.IO.MemoryStream())
            {
                // first write to memory stream. This way, if something goes wrong, the file won't be partly overwritten.
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(DesignerDataFile));
                using (System.IO.TextWriter iWriter = new System.IO.StreamWriter(iStr))
                {
                    iSer.Serialize(iWriter, BrainData.Current.DesignerData);
                    using (
                        var iFile = new System.IO.FileStream(
                            fDFile, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.ReadWrite)) // when we managed to write the xml to mem, dump to file.
                        iStr.WriteTo(iFile);
                }
            }

            SaveDesignerData(BrainData.Current.DesignerData, fPath);
            BrainData.Current.AttachedTopics.SaveData(
                System.IO.Path.Combine(
                    fPath, 
                    NeuronDataDictionary.DATAPATH, 
                    Properties.Resources.AttachedTopicsFileName));
            SaveTestCases();
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action(EndOk)); // call the end when the designer data has been loaded
        }

        /// <summary>
        ///     Saves all the test cases that were modified/are still open.
        /// </summary>
        /// <param name="path">The path.</param>
        private void SaveTestCases()
        {
            var iPath = System.IO.Path.Combine(fPath, NeuronDataDictionary.DATAPATH);
            foreach (var i in BrainData.Current.TestCases)
            {
                if (i.IsOpen || i.IsChanged)
                {
                    i.Save(iPath);
                }
            }
        }
    }
}