// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectConverter.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a project from the current storage storage mechanisme to a new
//   one.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Converts a project from the current storage storage mechanisme to a new
    ///     one.
    /// </summary>
    internal class ProjectConverter : ProjectOperation
    {
        /// <summary>The f path.</summary>
        private string fPath;

        /// <summary>The f loaded test cases.</summary>
        private readonly System.Collections.Generic.List<TestCaseBackup> fLoadedTestCases =
            new System.Collections.Generic.List<TestCaseBackup>();

        /// <summary>
        ///     Converts the storage system from the brain into the new type as
        ///     defined in <see cref="ProjectConverter.NewStore" /> .
        /// </summary>
        public void Convert()
        {
            DisableUI();
            System.Action iConv = InternalConvert;
            iConv.BeginInvoke(null, null);
        }

        /// <summary>The internal convert.</summary>
        private void InternalConvert()
        {
            try
            {
                ExportProject();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(CreateNewProject));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectConverter.Convert", e.ToString());
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }

        /// <summary>The create new project.</summary>
        private void CreateNewProject()
        {
            try
            {
                var iTemplateName =
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Properties.Settings.Default.DefaultTemplatePath), 
                        "Rebuildtemplate." + ProjectManager.PROJECT_EXTENTION);
                ProjectManager.Default.CreateOrLoadFromTemplate(iTemplateName);
                System.Action iImport = ImportXmlProject;
                iImport.BeginInvoke(null, null);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectConverter.Convert", e.ToString());
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }

        /// <summary>The import xml project.</summary>
        private void ImportXmlProject()
        {
            try
            {
                ProjectXmlStreamer.Import(fPath);
                System.IO.File.Delete(fPath); // don't need to store the temp file
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(RestoreTestCases));
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectConverter.Convert", e.ToString());
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action(End));
            }
        }

        /// <summary>
        ///     Exports the entire project in xml format, to a temp file Before we
        ///     begin the export, we check if we first need to do a rebuild of the
        ///     <see langword="static" /> neurons.
        /// </summary>
        private void ExportProject()
        {
            Neuron iFound;
            if (Brain.Current.TryFindNeuron((ulong)PredefinedNeurons.EndOfStatic - 1, out iFound) == false)
            {
                // if the neuron doesn't exist, rebuild the default neurons, otherwise we might get errors while exporting, cause some properties can't be read.
                Brain.Current.ReloadLoadDefaultNeurons();
            }

            fPath = System.IO.Path.GetTempFileName();
            ProjectXmlStreamer.Export(fPath);
            BackupTestCases();
        }

        /// <summary>The backup test cases.</summary>
        private void BackupTestCases()
        {
            foreach (
                var iFile in
                    System.IO.Directory.GetFiles(BrainData.Current.NeuronInfo.StoragePath, "*." + BrainData.TESTCASEEXT)
                )
            {
                var iNew = new TestCaseBackup();
                iNew.Content = System.IO.File.ReadAllText(iFile);
                iNew.Name = System.IO.Path.GetFileNameWithoutExtension(iFile);
                fLoadedTestCases.Add(iNew);

                    // we read the data as text, so that the xml data is local to the app during conversion (multiple apps can convert a project at the same time), We don't read as a testcase cause then we loose the ref to the channel.
            }
        }

        /// <summary>
        ///     Restores the test cases. Needs to be done from ui thread cause adding
        ///     to an observableCollection.
        /// </summary>
        private void RestoreTestCases()
        {
            try
            {
                foreach (var i in fLoadedTestCases)
                {
                    var iFileName = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllText(iFileName, i.Content);
                    var iNew = new Test.TestCase();
                    iNew.Name = i.Name;
                    iNew.LoadDataFrom(iFileName);
                    iNew.IsChanged = true; // need to make certain that it will be saved.
                    BrainData.Current.TestCases.Add(iNew);
                }

                EndOk();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("ProjectConverter", e.ToString());
                End();
            }
        }

        #region inner types

        /// <summary>
        ///     so we can keep a memory copy of all the testcases together with the
        ///     name for during the conversion, so we don't loose them when creating a
        ///     new project.
        /// </summary>
        private class TestCaseBackup
        {
            /// <summary>Gets or sets the name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the content.</summary>
            public string Content { get; set; }
        }

        #endregion
    }
}