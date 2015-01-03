using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JaStDev.HAB;
using System.IO;
using System.Reflection;
using JaStDev.LogService;

namespace AiciWeb.Web
{
    public class Aici : IProcessorFactory
    {
        #region fields
        /// <summary>
        /// The name of the project file containing info about the network itself.
        /// </summary>
        public const string NETWORKFILE = "Brain.xml";

        /// <summary>
        /// The path of the network that was loaded.
        /// </summary>
        string fPath;
        bool fNetworkActive = false;
        bool fIsOpen = false;
        #endregion


        #region Prop

        #region NetworkActive

        /// <summary>
        /// Gets the wether the network is currently active or not
        /// </summary>
        public bool NetworkActive
        {
            get
            {
                return fNetworkActive;
            }
            private set
            {
                fNetworkActive = value;
            }
        }

        #endregion

        #region IsOpen
        /// <summary>
        /// Opens and closes the network.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return fIsOpen;
            }
            set
            {
                if (fIsOpen != value)
                {
                    fIsOpen = value;
                    if (value == true)
                        StartNetwork();
                    else
                        CloseNetwork();
                }
            }
        } 
        #endregion

        #region Channels
        /// <summary>
        /// Gets all the available text sin's in the network.
        /// </summary>
        public IEnumerable<TextSin> Channels
        {
            get
            {
                return from i in Brain.Current.Sins where i is TextSin select (TextSin)i;
            }
        }
        #endregion

        #endregion


        #region functions


        /// <summary>
        /// Starts the network: sets all the global values, loads the data.
        /// </summary>
        internal void StartNetwork()
        {
            try
            {
                Log.Service = TextLogService.Default;
                TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network started");
                Settings.SinAssemblies = CreateAssemblies();
                Settings.StorageMode = NeuronStorageMode.StreamWhenPossible;
                Settings.LogNeuronNotFoundInLongTermMem = false;
                Settings.TrackNeuronAccess = false;
                Settings.MaxConcurrentProcessors = Properties.Settings.Default.MaxConcurrentProcessors;
                Settings.InitProcessorStackSize = Properties.Settings.Default.InitProcessorStackSize;
                ProcessorFactory.Factory = this;
                string iDBLoc = Properties.Settings.Default.DBLocation;
                if (iDBLoc.Length > 1)
                {
                    LoadNetwork(iDBLoc);
                    fPath = iDBLoc;
                    LoadChannels();
                    ThreadManager.Default.ActivityStarted += new EventHandler(Default_ActivityStarted);
                    ThreadManager.Default.ActivityStopped += new EventHandler(Default_ActivityStopped);
                }
                else
                    throw new InvalidOperationException("Please provide a valid path to a network that should be run as argument!");
            }
            catch (Exception e)
            {
                Log.LogError("Aici.StartNetwork", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Looks up the first textsin in the network and attaches to it.
        /// </summary>
        private void LoadChannels()
        {
            //int iCount = 0;
            //Channels = new ObservableCollection<TextChannel>(from i in Brain.Current.Sins
            //                                                 where i is TextSin
            //                                                 select new TextChannel((TextSin)i) { DisplayTitle = "Channel " + (++iCount).ToString() });
            //if (Channels.Count == 0)
            //   MessageBox.Show("Couldn't find a TextSin in the network to use as an interface!");
        }

        /// <summary>
        /// Loads the brain.
        /// </summary>
        /// <param name="from">From.</param>
        private static void LoadNetwork(string from)
        {
            string iPath = Path.Combine(from, NETWORKFILE);
            if (File.Exists(iPath) == true)
                Brain.Load(iPath);
            else
                throw new InvalidOperationException(string.Format("{0} not found!", iPath));
        }

        /// <summary>
        /// Handles the ActivityStopped event of the Default control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Default_ActivityStopped(object sender, EventArgs e)
        {
            NetworkActive = false;
        }

        /// <summary>
        /// Handles the ActivityStarted event of the Default control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Default_ActivityStarted(object sender, EventArgs e)
        {
            NetworkActive = true;
        }

        /// <summary>
        /// Creates the list of assemblies that contain extra sins used by the designer.
        /// </summary>
        /// <returns>A list of assebmlies, currently this list only contains the 'Sensory Interfaces' and wordnet assembly.</returns>
        private List<Assembly> CreateAssemblies()
        {
            List<Assembly> iRes = new List<Assembly>();
            iRes.Add(Assembly.GetAssembly(typeof(WordNetSin)));
            iRes.Add(Assembly.GetAssembly(typeof(ReflectionSin)));
            return iRes;
        }

        #endregion


        #region IProcessorFactory Members

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <returns></returns>
        public Processor GetProcessor()
        {
            return new Processor();
        }

        #endregion

        internal void CloseNetwork()
        {
            TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network closed");
            if (fPath != null)
            {
                string iPath = Path.Combine(fPath, NETWORKFILE);
                Brain.Current.Save(iPath);
            }
            Brain.Current.Clear();                                                          //we clear the network, so that all the files will be closed

        }
    }
}