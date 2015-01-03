// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConduit.cs" company="">
//   
// </copyright>
// <summary>
//   provides support for importing and/or exporting custom data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides support for importing and/or exporting custom data.
    /// </summary>
    public class CustomConduit : Data.ObservableObject, IDocumentInfo
    {
        /// <summary>The f is running.</summary>
        private bool fIsRunning;

        /// <summary>The f wait for save.</summary>
        private System.Threading.AutoResetEvent fWaitForSave;

                                                // if we need to save the project, need to wait until the save is completed (it's an async process).

        /// <summary>The f selector.</summary>
        private readonly CustomConduitSelector fSelector = new CustomConduitSelector();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomConduit" /> class.
        /// </summary>
        public CustomConduit()
        {
            fSelector.CustomDll = Properties.Settings.Default.LastCustomDLL;

                // assign to prop so that the entrypoints get loaded.
            fSelector.SelectedEntryPoint = Properties.Settings.Default.LastSelectedEntryPoint;

                // also needs to be assigned to prop, so that it is correctly loaded.
            fSelector.Source = Properties.Settings.Default.LastCustomSource;
            fSelector.Destination = Properties.Settings.Default.LastCustomDestination;
        }

        /// <summary>
        ///     initializes a new custom import process by showing a userdialog.
        /// </summary>
        public static void StartImport()
        {
            var iStart = new CustomConduit();
            var iImport = new DlgCustomConduit();
            iImport.DataContext = iStart;
            iImport.Owner = System.Windows.Application.Current.MainWindow;
            var iRes = iImport.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                iStart.Import();
            }
        }

        /// <summary>
        ///     displays the custom data importer dialog, loads the dll and starts the
        ///     import process.
        /// </summary>
        private void Import()
        {
            Properties.Settings.Default.LastCustomDLL = fSelector.CustomDll;

                // store the data so it will be available next time.
            Properties.Settings.Default.LastCustomSource = fSelector.Source;
            Properties.Settings.Default.LastCustomDestination = fSelector.Destination;
            Properties.Settings.Default.LastSelectedEntryPoint = fSelector.SelectedEntryPoint;

            WindowMain.Current.AddItemToOpenDocuments(this);

                // show the progress control in the editor so that the user can debug while the import is happening.
            if (fSelector.Process != null)
            {
                if (fSelector.Process.NeedsSaving())
                {
                    var iRes =
                        System.Windows.MessageBox.Show(
                            "This process will generate a lot of data, automatic mid-process project saves might be performed to minimize memory impact. Do you want to continue?", 
                            "Large data set", 
                            System.Windows.MessageBoxButton.YesNo, 
                            System.Windows.MessageBoxImage.Warning, 
                            System.Windows.MessageBoxResult.Yes);
                    if (iRes == System.Windows.MessageBoxResult.No)
                    {
                        return;
                    }

                    fSelector.Process.SaveRequested += Process_SaveRequested;
                }

                fSelector.Process.Finished += Process_Finished;
                fWaitForSave = new System.Threading.AutoResetEvent(false);
                IsRunning = true;
                System.Action iStart = ProcessAsync;
                iStart.BeginInvoke(null, null);
            }
            else
            {
                throw new System.InvalidOperationException("No valid entry point selected.");
            }
        }

        /// <summary>
        ///     wraps the call to process in a try/catch. to make it all save and log
        ///     things correctly.
        /// </summary>
        private void ProcessAsync()
        {
            try
            {
                if (fSelector.Process.NeedsDestination() == false)
                {
                    fSelector.Process.Process(fSelector.Source);
                }
                else
                {
                    fSelector.Process.Process(fSelector.Source, fSelector.Destination);
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Custom conduit", e.ToString());
            }
        }

        /// <summary>called when the entire process is done.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Finished(object sender, System.EventArgs e)
        {
            if (fSelector.Process.NeedsSaving())
            {
                fSelector.Process.SaveRequested -= Process_SaveRequested;
            }

            fSelector.Process.Finished -= Process_Finished;
            LogService.Log.LogInfo("Custom conduit", "process has finished.");
            System.Windows.MessageBox.Show("The conduit process is finished.");
            IsRunning = false;
        }

        /// <summary>called when the project needs to be saved and everything needs to be
        ///     cleaned to make certain that the memory usage isn't to much. Note:
        ///     this is called from a different thread.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_SaveRequested(object sender, System.EventArgs e)
        {
            var iSave = new ProjectSaver();
            iSave.EndedOk += iSave_EndedOk;
            iSave.EndedNok += iSave_EndedNok;
            iSave.Save();
            fWaitForSave.WaitOne();

                // wait untill the save completed. We call this from a different thread then the UI, so the save can still call the UI thread while we are blocked.
        }

        /// <summary>The i save_ ended nok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iSave_EndedNok(object sender, System.EventArgs e)
        {
            LogService.Log.LogInfo("Custom conduit", "Failed to save project.");
            fWaitForSave.Set();
        }

        /// <summary>The i save_ ended ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iSave_EndedOk(object sender, System.EventArgs e)
        {
            LogService.Log.LogInfo("Custom conduit", "project succesfully saved.");
            fWaitForSave.Set();
        }

        #region Prop

        #region Selector

        /// <summary>
        ///     Gets the object responsible for selecting the conduit.
        /// </summary>
        public CustomConduitSelector Selector
        {
            get
            {
                return fSelector;
            }
        }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets/sets the value that indicates if this process is still running or
        ///     not.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return fIsRunning;
            }

            internal set
            {
                fIsRunning = value;
                OnPropertyChanged("IsRunning");
            }
        }

        #endregion

        #region IDocumentInfo Members

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        public string DocumentTitle
        {
            get
            {
                if (string.IsNullOrEmpty(Selector.CustomDll) == false)
                {
                    return System.IO.Path.GetFileNameWithoutExtension(Selector.CustomDll);
                }

                return "Custom conduit";
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public string DocumentInfo
        {
            get
            {
                if (string.IsNullOrEmpty(Selector.CustomDll) == false)
                {
                    return string.Format(
                        "Provides custom import/export of data through the {0} interface", 
                        System.IO.Path.GetFileNameWithoutExtension(Selector.CustomDll));
                }

                return "Provides custom import/export of data";
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public string DocumentType
        {
            get
            {
                return "CustomConduit";
            }
        }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        public object DocumentIcon
        {
            get
            {
                return null;
            }
        }

        #endregion

        #endregion
    }
}