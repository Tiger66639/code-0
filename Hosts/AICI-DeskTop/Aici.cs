//-----------------------------------------------------------------------
// <copyright file="Aici.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>15/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;
using JaStDev.HAB;
using JaStDev.Data;
using System.IO;
using System.Windows;
using JaStDev.LogService;
using JaStDev.WPFLog;
using System.Threading;

namespace AICI_DeskTop
{


   /// <summary>
   /// The entry point to the network, which manages all the communication.
   /// </summary>
   class Aici : ObservableObject, IProcessorFactory
   {
      #region fields
      ObservableCollection<TextChannel> fChannels;
      /// <summary>
      /// The name of the project file containing info about the network itself.
      /// </summary>
      public const string NETWORKFILE = "Brain.xml";

      /// <summary>
      /// The path of the network that was loaded.
      /// </summary>
      string fPath;
      bool fNetworkActive = false;
      #endregion


      #region Prop

      #region Channels

      /// <summary>
      /// Gets the list of channels that provide access to the TextSins.
      /// </summary>
      public ObservableCollection<TextChannel> Channels
      {
         get { return fChannels; }
         internal set
         {
            fChannels = value;
            OnPropertyChanged("Channels");
         }
      }

      #endregion

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
            OnPropertyChanged("NetworkActive");
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
         Log.Service = WPFLog.Default;
         Settings.SinAssemblies = CreateAssemblies();
         Settings.StorageMode = NeuronStorageMode.StreamWhenPossible;
         Settings.BufferIndexFiles = true;                                                               //this should speed up disk access, can be dangerous though (when index files are too big).
         Settings.BufferFreeBlocksFiles = true;                                                          //same as buffering index files.
         Settings.LogNeuronNotFoundInLongTermMem = false;
         Settings.TrackNeuronAccess = false;
         Settings.SetMinMaxProc(30, Properties.Settings.Default.MaxConcurrentProcessors);
         Settings.InitProcessorStackSize = Properties.Settings.Default.InitProcessorStackSize;
         ProcessorFactory.Factory = this;
         string[] iArgs = Environment.GetCommandLineArgs();
         if (iArgs.Length > 1)
         {
            if (LoadNetwork(iArgs[1]) == true)
            {
               fPath = iArgs[1];
               LoadChannels();
               ThreadManager.Default.ActivityStarted += new EventHandler(Default_ActivityStarted);
               ThreadManager.Default.ActivityStopped += new EventHandler(Default_ActivityStopped);
               ThreadPool.QueueUserWorkItem(LoadWords);
               Brain.Current.CallNetActivityEvent((ulong)PredefinedNeurons.OnStarted);                         //we need to d this manually when the brain is loaded.
            }
         }
         else
            MessageBox.Show("Please provide a valid path to a network that should be run as argument!");
      }

      /// <summary>
      /// Looks up the first textsin in the network and attaches to it.
      /// </summary>
      private void LoadChannels()
      {
         int iCount = 0;
         Channels = new ObservableCollection<TextChannel>(from i in Brain.Current.Sins
                                                          where i is TextSin
                                                          select new TextChannel((TextSin)i) { DisplayTitle = "Channel " + (++iCount).ToString() });
         if (Channels.Count == 0)
            MessageBox.Show("Couldn't find a TextSin in the network to use as an interface!");
      }

      /// <summary>
      /// Loads the brain.
      /// </summary>
      /// <param name="from">From.</param>
      private static bool LoadNetwork(string from)
      {
         string iPath = Path.Combine(from, NETWORKFILE);
         if (File.Exists(iPath) == true)
         {
            Brain.Load(iPath);
            return true;
         }
         else
            MessageBox.Show(string.Format("{0} not found!", iPath));
         return false;
      }

      /// <summary>
      /// loads the words dictionary. this is called async.
      /// Not really required, but it gives a speed up at the start.
      /// </summary>
      /// <param name="toStart"></param>
      void LoadWords(object toStart)
      {
         if (TextSin.Words.Count > 0)                       //simply access the words is enough for loading them.
         {
         }
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
      /// Gets the processor. Not required to implement, but provides a way to overload the processer class.
      /// </summary>
      /// <returns></returns>
      public Processor CreateProcessor()
      {
         return new Processor();
      }

      /// <summary>
      /// Called when a processor is about to be started. This is always called after a <see cref="IProcessorFactory.CreateProcessor"/>
      /// was called, but can also be called at other times, when a processor gets reused.
      /// This can be left empty. It's primarily to set up debuggers.
      /// </summary>
      /// <param name="proc"></param>
      public void ActivateProc(Processor proc)
      {
      }

      #endregion

      internal void CloseNetwork()
      {
         if (fPath != null)
         {
            string iPath = Path.Combine(fPath, NETWORKFILE);
            Brain.Current.Save(iPath);
         }
      }

   }
}