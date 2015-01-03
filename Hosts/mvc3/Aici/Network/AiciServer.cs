//-----------------------------------------------------------------------
// <copyright file="AiciServer.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>04/09/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JaStDev.HAB;
using JaStDev.LogService;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Aici.Network
{
    public class AiciServer 
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
        /// Checks if there are still any textsins left from a previous invalid close. If so, they are all
        /// deleted, except the 1st sin, which is that of the editor.
        /// </summary>
        internal void CleanChannels()
        {
            List<TextSin> iSins = Channels.ToList();
            if (iSins.Count > 1)
            {
                for (int i = 1; i < iSins.Count - 1; i++)
                    Brain.Current.Delete(iSins[i]);                                 //delete the sin so that there is no clutter.
            }
        }

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
                Settings.SetMinMaxProc(Properties.Settings.Default.MinReservedBlockedProcessors, Properties.Settings.Default.MaxConcurrentProcessors);
                Settings.InitProcessorStackSize = Properties.Settings.Default.InitProcessorStackSize;
                string iDBLoc = Properties.Settings.Default.DBLocation;
                if (iDBLoc.Length > 1)
                {
                    LoadNetwork(iDBLoc);
                    fPath = iDBLoc;
                    ThreadManager.Default.ActivityStarted += new EventHandler(Default_ActivityStarted);
                    ThreadManager.Default.ActivityStopped += new EventHandler(Default_ActivityStopped);
                    Brain.Current.CallNetActivityEvent((ulong)PredefinedNeurons.OnStarted);                         //we need to d this manually when the brain is loaded.
                    Brain.Current.LoadCodeAt((ulong)PredefinedNeurons.ContainsWord);                                //a little speed optimisation: we preload the code so that the first salvo also runs fast.
                    //Brain.Current.LoadCodeAt(1365);                                                                 //1365 = patternmatch finished, so we have some more code preloaded.
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


        static public bool CheckAllowConnection(string ip)
        {
           if (string.IsNullOrEmpty(Properties.Settings.Default.IPFilter) == false)
           {
              Match iRes = Regex.Match(ip, Properties.Settings.Default.IPFilter);
              if (iRes.Success == false)
              {
                 Log.LogError("AllowConnection", "invalid request from :" + ip);
                 return false;
              }
           }
           return true;
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


        internal void CloseNetwork()
        {
            if (NetworkActive == true)
            {
                if (ThreadManager.Default.HasRunningProcessors == true)
                {
                    ThreadManager.Default.EndDeadLock();                                    //if the network was stil active when a close is requested (all sessions have terminated), simply do an end deadlock. This will still delete any frozen data.
                    TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network forcefully closed");
                }
                else
                   TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network indicated still active, but there were no more running processors? Network closed");
                NetworkActive = false;
            }else
                TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network closed");
            if (fPath != null)
            {
                string iPath = Path.Combine(fPath, NETWORKFILE);
                Brain.Current.Save(iPath);
            }
            Brain.Current.Clear();                                                          //we clear the network, so that all the files will be closed will raise appropriate events.
        }

    }
}