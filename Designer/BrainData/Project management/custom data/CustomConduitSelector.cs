// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConduitSelector.cs" company="">
//   
// </copyright>
// <summary>
//   The custom conduit selector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The custom conduit selector.</summary>
    public class CustomConduitSelector : Data.ObservableObject
    {
        #region EntryPoints

        /// <summary>
        ///     Gets/sets the list of entry points that can be used in the dll.
        /// </summary>
        public System.Collections.Generic.List<string> EntryPoints
        {
            get
            {
                return fEntryPoints;
            }

            set
            {
                fEntryPoints = value;
                OnPropertyChanged("EntryPoints");
            }
        }

        #endregion

        /// <summary>
        ///     the list of types.
        /// </summary>
        public System.Collections.Generic.List<System.Type> EntryPointTypes { get; private set; }

        #region SelectedEntryPoint

        /// <summary>
        ///     Gets/sets the entry point that should be used for processing the data.
        /// </summary>
        public int SelectedEntryPoint
        {
            get
            {
                return fSelectedEntryPoint;
            }

            set
            {
                if (value != fSelectedEntryPoint)
                {
                    fSelectedEntryPoint = value;
                    OnPropertyChanged("SelectedEntryPoint");
                    if (EntryPointTypes != null && value >= 0 && value < EntryPointTypes.Count)
                    {
                        Process =
                            (CustomConduitSupport.ICustomConduit)System.Activator.CreateInstance(EntryPointTypes[value]);
                        if (Process != null)
                        {
                            NeedsDestination = Process.NeedsDestination();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(
                                "Failed to create instance of type: " + fSelectedEntryPoint, 
                                "Custom conduits", 
                                System.Windows.MessageBoxButton.OK, 
                                System.Windows.MessageBoxImage.Error);
                            fSelectedEntryPoint = -1;
                        }
                    }
                    else
                    {
                        fSelectedEntryPoint = -1;
                        Process = null;
                    }
                }
            }
        }

        #endregion

        #region Source

        /// <summary>
        ///     Gets/sets the source where the custom dll should load the data from.
        /// </summary>
        public string Source
        {
            get
            {
                return fSource;
            }

            set
            {
                fSource = value;
                OnPropertyChanged("Source");
            }
        }

        #endregion

        #region Destination

        /// <summary>
        ///     Gets/sets the destination that the custom conduit should generate data
        ///     to. This can be empty, depending on wether the conduit needs this
        ///     info.
        /// </summary>
        public string Destination
        {
            get
            {
                return fDestination;
            }

            set
            {
                fDestination = value;
                OnPropertyChanged("Destination");
            }
        }

        #endregion

        #region NeedsDestination

        /// <summary>
        ///     Gets/sets the value that indicates if a destination is needed or not.
        /// </summary>
        public bool NeedsDestination
        {
            get
            {
                return fNeedsDestination;
            }

            set
            {
                fNeedsDestination = value;
                OnPropertyChanged("NeedsDestination");
            }
        }

        #endregion

        #region Process

        /// <summary>
        ///     Gets or sets the process.
        /// </summary>
        /// <value>
        ///     The process.
        /// </value>
        public CustomConduitSupport.ICustomConduit Process
        {
            get
            {
                return fProcess;
            }

            set
            {
                fProcess = value;
                OnPropertyChanged("Process");
                OnPropertyChanged("HasConduitSettings");
            }
        }

        #endregion

        /// <summary>
        ///     for wpf binding: not all conduits currnetly support extra settings.
        /// </summary>
        public bool HasConduitSettings
        {
            get
            {
                return fProcess is CustomConduitSupport.BaseConduit;
            }
        }

        #region ExtraData

        /// <summary>
        ///     Gets/sets the extra data assigned to the selector. This is used by the
        ///     queries (QueryDataSource), so that it can assign the selected values
        ///     to the pipe when needed (selection has been made). This way, we can
        ///     use the selector object for both a push and pull situation.
        /// </summary>
        public object ExtraData
        {
            get
            {
                return fExtraData;
            }

            set
            {
                fExtraData = value;
                OnPropertyChanged("ExtraData");
            }
        }

        #endregion

        #region fields

        /// <summary>The f custom dll.</summary>
        private string fCustomDll;

        /// <summary>The f source.</summary>
        private string fSource;

        /// <summary>The f entry points.</summary>
        private System.Collections.Generic.List<string> fEntryPoints;

        /// <summary>The f selected entry point.</summary>
        private int fSelectedEntryPoint = -1;

        /// <summary>The f destination.</summary>
        private string fDestination;

        /// <summary>The f needs destination.</summary>
        private bool fNeedsDestination;

        /// <summary>The f process.</summary>
        private CustomConduitSupport.ICustomConduit fProcess;

        /// <summary>The f extra data.</summary>
        private object fExtraData;

        #endregion

        #region CustomDll

        /// <summary>
        ///     Gets/sets the dll to use that will handle the custom load
        ///     functionality.
        /// </summary>
        public string CustomDll
        {
            get
            {
                return fCustomDll;
            }

            set
            {
                if (value != fCustomDll)
                {
                    fCustomDll = value;
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        LoadEntryPoints();
                    }

                    OnPropertyChanged("CustomDll");
                }
            }
        }

        /// <summary>
        ///     Loads the entry points.
        /// </summary>
        private void LoadEntryPoints()
        {
            var iFile = CustomDll;
            if (System.IO.Path.IsPathRooted(iFile) == false)
            {
                iFile = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, iFile);
            }

            if (System.IO.File.Exists(iFile))
            {
                var iTypes = new System.Collections.Generic.List<string>();
                var ass = System.Reflection.Assembly.LoadFrom(iFile);
                if (ass != null)
                {
                    EntryPointTypes = new System.Collections.Generic.List<System.Type>();
                    foreach (var t in ass.GetExportedTypes())
                    {
                        if (t.IsAbstract == false && t.GetInterface("ICustomConduit", true) != null)
                        {
                            // Get all classes implement ICustomConduit
                            iTypes.Add(t.Name);
                            EntryPointTypes.Add(t); // so we can create the process easily.
                        }
                    }
                }

                EntryPoints = iTypes;
            }
            else
            {
                EntryPoints = new System.Collections.Generic.List<string>(); // give an empty list.
            }
        }

        #endregion
    }
}