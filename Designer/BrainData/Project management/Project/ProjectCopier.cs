// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectCopier.cs" company="">
//   
// </copyright>
// <summary>
//   Copies the project to a new location. Note that the new location should
//   already have been prepared with ProjectManager.PreparePathForProject
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Copies the project to a new location. Note that the new location should
    ///     already have been prepared with ProjectManager.PreparePathForProject
    /// </summary>
    internal class ProjectCopier : ProjectOperation
    {
        /// <summary>The f data path.</summary>
        private string fDataPath;

        /// <summary>The f path.</summary>
        private string fPath; // root path for designer files

        /// <summary>The f project path.</summary>
        private string fProjectPath;

        /// <summary><see cref="Start"/> the copy operation to the specified path.</summary>
        /// <param name="path">The path.</param>
        /// <param name="projectPath">The project Path.</param>
        public void Start(string path, string projectPath)
        {
            fPath = path;
            fProjectPath = projectPath;
            fDataPath = StorageHelper.GetDataPath(projectPath);
            DisableUI();
            System.Action iCopy = Copy;
            iCopy.BeginInvoke(null, null);
        }

        /// <summary>
        ///     does the actaul copying.
        /// </summary>
        private void Copy()
        {
            try
            {
                var iDataPath = Brain.Current.Storage.DataPath; // store this so we can assig again later on.
                BrainData.Current.NeuronInfo.Store.Dispose(); // this closes all the file
                Brain.Current.Storage.Dispose(); // need to close the files
                try
                {
                    var iFile = System.IO.Path.Combine(
                        fPath, 
                        System.IO.Path.GetFileName(ProjectManager.Default.DesignerFile));
                    System.IO.File.Copy(ProjectManager.Default.DesignerFile, iFile, true);
                    System.IO.File.Copy(
                        System.IO.Path.Combine(ProjectManager.Default.Location, ProjectManager.NETWORKFILE), 
                        System.IO.Path.Combine(fProjectPath, ProjectManager.NETWORKFILE));
                    CopyDesignerData(fProjectPath);
                    CopyModules(fProjectPath);
                    Brain.Current.Storage.CopyTo(fDataPath);
                }
                finally
                {
                    Brain.Current.Storage.DataPath = iDataPath; // need to reset the datapath, so the files load again.
                    iDataPath = System.IO.Path.Combine(ProjectManager.Default.Location, NeuronDataDictionary.DATAPATH);

                        // reload the files again after done.
                    BrainData.Current.NeuronInfo.Store.LoadFiles(iDataPath);
                }

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(EndOk));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "ProjectCopier.Copy", 
                    string.Format("Failed to copy project to '{0}': {1}.", fPath, e));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End)); // call the end when the designer data has been loaded 
            }
        }
    }
}