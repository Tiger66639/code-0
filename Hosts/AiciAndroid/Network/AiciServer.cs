//-----------------------------------------------------------------------
// <copyright file="AiciServer.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JaStDev.HAB;
using JaStDev.LogService;
using System.IO;
using System.Reflection;
using System.Threading;
using os.MonoDroid;
using System.Diagnostics;
using AiciAndroid.Acitivities;

namespace AiciAndroid.Network
{
   /// <summary>
   /// This is a service that manages the aici database.
   /// </summary>
   [Service]
   public class AiciServer : Service, IAndroidService
   {
      #region fields
      /// <summary>
      /// The name of the project file containing info about the network itself.
      /// </summary>
      public const string NETWORKFILE = "Brain.xml";
      const string MINRESERVEDPROC = "MinReservedBlockedProcessors";
      const string MAXCONCURRENTPROC = "MaxConcurrentProcessors";
      const string INITSTACKSIZE = "InitProcessorStackSize";
      
      
      

      bool fNetworkActive = false;
      bool fIsOpen = false;
      bool fIsSaving = false;
      string fPath;
      TextSin fSin;
      IntSin fIntSin;                                                //when we get a series of ints, we need to let the user make a selection
      WeakReference fContext;
      List<UILogItem> fBuffer = new List<UILogItem>();               //when there is no IAiciServerCallbacks available to provide the data too, we buffer it until it has made a connection.
      List<int> fInputSelectionList;                                 //when the network needs to ask user for selection between possible inputs, but there is no current context, we try to wait until a context is available and ask for it then, this field is used to store the list of possible values. At the end of the selection, the list should only contain 1 item (the result).
      AutoResetEvent fIntSinBlock = new AutoResetEvent(false);   //the IntSin.IntsOut needs to block until the user has made a selection (or timeout has expired).
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
            if (value != fNetworkActive)
            {
               fNetworkActive = value;
               if (fContext != null && fContext.IsAlive == true)
               {
                  IAiciServerCallbacks iCallback = fContext.Target as IAiciServerCallbacks;
                  if (iCallback != null)
                     iCallback.SetNetworkActivity(value);
               }
            }
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

      /// <summary>
      /// Gets the object that we should use to send data back too (when available).
      /// </summary>
      public IAiciServerCallbacks Context
      {
         get
         {
            if (fContext != null && fContext.IsAlive == true)
               return (IAiciServerCallbacks)fContext.Target;
            else
               return null;
         }
      }

      #endregion


      #region Activity(IAndroidService Members)


      /// <summary>
      /// Gets the activity that is currently associtiated with the Android service. This allows us to call sertain functions
      /// from the context of an activity (like StartActivityWithREsult).
      /// </summary>
      public Activity Activity
      {
         get { return Context as Activity; }
      }

      #endregion


      #region overrides

      /// <summary>
      /// we don't support binding.
      ///   <i>Note that unlike other application components, calls on to the
      /// IBinder interface returned here may not happen on the main thread
      /// of the process</i>.
      /// </summary>
      /// <param name="intent">The Intent that was used to bind to this service,
      /// as given to <c><see cref="M:Android.Content.Context.BindService(Android.Content.Intent, Android.Content.IServiceConnection, Android.Content.IServiceConnection)"/></c>.  Note that any extras that were included with
      /// the Intent at that point will <i>not</i> be seen here.</param>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>Return an IBinder through which clients can call on to the
      /// service.
      ///   </term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      public override IBinder OnBind(Intent intent)
      {
         AiciBinder iBind = new AiciBinder();
         iBind.Server = this;
         return iBind;
      }

      /// <summary>
      /// Called by the system to notify a Service that it is no longer used and is being removed.
      /// </summary>
      /// <since version="API Level 1"/>
      public override void OnDestroy()
      {
         try
         {
            IsOpen = false;
         }
         catch (Exception e)
         {
            Log.LogError("Aici server", e.ToString());
            Toast iToast = Toast.MakeText(this, "Failed to stop the network.", ToastLength.Long);
            iToast.Show();
         }
         base.OnDestroy();
      }

      /// <summary>
      /// Called by the system when the service is first created.
      /// </summary>
      /// <since version="API Level 1"/>
      public override void OnCreate()
      {
         base.OnCreate();
         Log.Service = TextLogService.Default;
         Settings.SinAssemblies = CreateAssemblies();
         Settings.StorageMode = NeuronStorageMode.StreamWhenPossible;
         Settings.BufferIndexFiles = true;                                                               //this should speed up disk access, can be dangerous though (when index files are too big).
         Settings.BufferFreeBlocksFiles = true;                                                          //same as buffering index files.
         Settings.LogNeuronNotFoundInLongTermMem = false;
         Settings.TrackNeuronAccess = false;
         LoadSettingsFromPref();
      }

      #endregion

      #region functions


      /// <summary>
      /// Starts the network: sets all the global values, loads the data.
      /// </summary>
      internal void StartNetwork()
      {
         try
         {
            if (fIsSaving == false)
            {
               LoadNetwork();
               TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network started");
               ThreadManager.Default.ActivityStarted += new EventHandler(Default_ActivityStarted);
               ThreadManager.Default.ActivityStopped += new EventHandler(Default_ActivityStopped);
               Brain.Current.CallNetActivityEvent((ulong)PredefinedNeurons.OnStarted);                         //we need to d this manually when the brain is loaded.
               Brain.Current.LoadCodeAt((ulong)PredefinedNeurons.ContainsWord);                                //a little speed optimisation: we preload the code so that the first salvo also runs fast.
               Brain.Current.LoadCodeAt(1365);                                                                 //1365 = patternmatch finished, so we have some more code preloaded.
            }
            else
               Toast.MakeText(this, "The network is still being saved.", ToastLength.Long).Show();
         }
         catch (Exception e)
         {
            if (TextLogService.IsInit == true)                                                              //don't need to create extra errors if the logservice didn't initialize yet.
               Log.LogError("Aici.StartNetwork", e.ToString());
            throw;
         }
      }

      /// <summary>
      /// loads all the settings that are stored in the preferences data.
      /// </summary>
      private void LoadSettingsFromPref()
      {
         ISharedPreferences iSettings = GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
         Settings.SetMinMaxProc(iSettings.GetInt(MINRESERVEDPROC, 30), iSettings.GetInt(MAXCONCURRENTPROC, 25));
         Settings.InitProcessorStackSize = iSettings.GetInt(INITSTACKSIZE, 100);
         fPath = iSettings.GetString(Resources.GetString(Resource.String.BrainPrefKey), "");
         if (string.IsNullOrEmpty(fPath) == false && Directory.Exists(fPath) == false)
         {
            Toast iToast = Toast.MakeText(this, string.Format("Failed to find {0} on sd-card, reverting to default.", Path.GetFileName(fPath)), ToastLength.Long);
            iToast.Show();
            fPath = null;
            ISharedPreferencesEditor iEdit = iSettings.Edit();
            iEdit.PutString(Resources.GetString(Resource.String.BrainPrefKey), fPath);
         }
         else if(string.IsNullOrEmpty(fPath) == true)																	//build path to default network.
            fPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR,AiciActivity.AICIDEFAULTDIR);
      }


      /// <summary>
      /// Loads the brain.
      /// </summary>
      /// <param name="from">From.</param>
      private void LoadNetwork()
      {
         if (Android.OS.Environment.ExternalStorageState == (Android.OS.Environment.MediaMounted) == false)
            throw new InvalidOperationException("This application requires an external storage (SD-card).");
         string iPath = Path.Combine(fPath, NETWORKFILE);
         if (File.Exists(iPath) == true)
         {
            Brain.Load(iPath);
            ReflectionSin.Context = this;                                                                      //so that the functions have a context to work with.
            BatteryReceiver.Default.Register(this);
            fSin = (from i in Brain.Current.Sins where i is TextSin select (TextSin)i).FirstOrDefault();
            if (fSin != null)
               fSin.TextOut += fSin_TextOut;
            fIntSin = (from i in Brain.Current.Sins where i is IntSin select (IntSin)i).FirstOrDefault();
            if (fIntSin != null)
            {
               fIntSin.IntsOut += fIntSin_IntsOut;
               fIntSin.IntOut += fIntSin_IntOut;
            }
            ThreadPool.QueueUserWorkItem(LoadWords);                                                     //when a new network loaded-> try to load the text neurons asap, so that the dict is loaded and cached (otherwise we can have some bottle necks).
         }
         else
            throw new InvalidOperationException(string.Format("{0} not found!", NETWORKFILE));
      }

      /// <summary>
      /// loads the words dictionary. this is called async.
      /// </summary>
      /// <param name="toStart"></param>
      void LoadWords(object toStart)
      {
         if (TextSin.Words.Count > 0)                       //simply access the words is enough for loading them.
         {
         }
      }

      /// <summary>
      /// Handles the TextOut event of the fSin control.
      /// called when the network has something to say.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
      void fSin_TextOut(object sender, OutputEventArgs<string> e)
      {
         UILogItem iRes = new UILogItem();
         iRes.Source = SourceType.Bot;
         iRes.Text = e.Value;
         IAiciServerCallbacks iCallback = Context;
         if (iCallback != null)
            iCallback.TextOut(iRes);
         else
         {
            lock (fBuffer)
               fBuffer.Add(iRes);
         }
#if ANDROITRACE
         Debug.StopMethodTracing();
#endif
      }

      /// <summary>
      /// Handles the IntOut event of the fIntSin control.
      /// when there is 1 index returned, it is to let us know which input was selected.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.Int32&gt;"/> instance containing the event data.</param>
      void fIntSin_IntOut(object sender, OutputEventArgs<int> e)
      {
         IAiciServerCallbacks iCallback = Context;
         if (iCallback != null)                                                              //if there is no context buffer yet, we collect untill there is a context assigned.
            iCallback.SelectInputAsync(e.Value);
         else
            lock (fBuffer)
               fInputSelectionList = new List<int>() { e.Value };
      }


      /// <summary>
      /// Logs the incomming text. When possible, it is immediatly passed on to the main window, otherwise it is buffered.
      /// </summary>
      /// <param name="value">The value.</param>
      public void LogTextIn(string value)
      {
         lock (fBuffer)
         {
            IAiciServerCallbacks iCallback = null;
            if (fContext != null && fContext.IsAlive == true)
               iCallback = (IAiciServerCallbacks)fContext.Target;
            UILogItem iNew = new UILogItem() { Source = SourceType.User, Text = value };
            if (iCallback != null)
               iCallback.TextIn(iNew);
            else
               fBuffer.Add(iNew);
         }
      }

      /// <summary>
      /// Handles the IntsOut event of the fIntSin control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.Collections.Generic.List&lt;System.Int32&gt;&gt;"/> instance containing the event data.</param>
      void fIntSin_IntsOut(object sender, OutputEventArgs<List<int>> e)
      {
         IAiciServerCallbacks iCallback = Context;


         int iIndex;
         if (e.Value.Count == 1)                                                       //if we get 1 item back, it is to let us know which input that the engine eventually used, so log this value.
         {
            iIndex = e.Value[0];
            if (iCallback != null)                                                              //if there is no context buffer yet, we collect untill there is a context assigned.
               iCallback.FilterInput(e.Value);
            else
               lock (fBuffer)
                  fInputSelectionList = e.Value;
         }
         else
         {
            if (iCallback != null)                                                              //if there is no context buffer yet, we collect untill there is a context assigned.
               iCallback.FilterInput(e.Value);
            else
               lock (fBuffer)
                  fInputSelectionList = e.Value;
            //also wait when the context wasn't assigned yet, this will happen in another thread and when it does happen, the user is asked for some input.
            if (fIntSinBlock.WaitOne(new TimeSpan(0, 2, 0)) == true)                                       //we wait for a few minutes  for the user to respond, then we timeout.
            {
               if (e.Value.Count == 1)
               {
                  IntNeuron iInt = e.Data[e.Value[0]] as IntNeuron;                                                 //return the result of the selection through a link on the intSin (the int itself will be destroyed later on).
                  fIntSin.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.IntSin, iInt);
               }
               else
                  fIntSin.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.IntSin, null);
            }
            else
               Log.LogError("AiciServer", "Timeout reached while waiting for user choice");
         }
      }

      /// <summary>
      /// Processes the specified message.
      /// </summary>
      /// <param name="message">The message.</param>
      internal void Process(string message)
      {
#if ANDROITRACE
         Debug.StartMethodTracing("aici");
#endif
         Processor iProc = ProcessorFactory.GetProcessor ();
         fSin.Process(message, iProc, TextSinProcessMode.ClusterAndDict);
      }

      internal void Process(IList<string> messages)
      {
#if ANDROITRACE
         Debug.StartMethodTracing("aici");
#endif
         Processor iProc = new Processor();
         fSin.Process(messages, iProc, TextSinProcessMode.ClusterAndDict);
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
            ThreadManager.Default.EndDeadLock();                                    //if the network was stil active when a close is requested (all sessions have terminated), simply do an end deadlock. This will still delete any frozen data.
            TextLogService.Default.WriteToLog(string.Format("{0} SYS", DateTime.Now), "Network forcefully closed");
         }
         string iPath = Path.Combine(fPath, NETWORKFILE);
         fIsSaving = true;
         try
         {
            Brain.Current.Save(iPath);
            Brain.Current.Clear();                                                          //we clear the network, so that all the files will be closed will raise appropriate events.
            if (fSin != null)
            {
               fSin.TextOut -= fSin_TextOut;
               fSin = null;
            }
            if (fIntSin != null)
            {
               fIntSin.IntOut -= fIntSin_IntOut;
               fIntSin.IntsOut -= fIntSin_IntsOut;
               fIntSin = null;
            }
            BatteryReceiver.Default.Unregister(this);
         }
         finally
         {
            fIsSaving = false;
         }
      }

      /// <summary>
      /// request a deadlock resolvement.
      /// </summary>
      internal void Panic()
      {
         ThreadManager.Default.EndDeadLock();
      }

      /// <summary>
      /// Checks if the user changed the path to the network that should be used, if so, the path is changed
      /// and the network is reloaded if it was already loaded.
      /// </summary>
      internal void CheckPath()
      {
         ISharedPreferences iSettings = GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
         string iNewPath = iSettings.GetString(Resources.GetString(Resource.String.BrainPrefKey), fPath);		//important to return the previous path, otherwise we close the network at an incorrect time.
         if (fPath != iNewPath)
         {
            bool fIsOpen = IsOpen;
            IsOpen = false;
            fPath = iNewPath;
            IsOpen = fIsOpen;
         }
      }

      /// <summary>
      /// Called when the <see cref="IAiciServerCallbacks"/> object that is making use of this service, has been
      /// restarted and the data needs to be resynced.
      /// </summary>
      /// <param name="callbacks"></param>
      internal void Sync(IAiciServerCallbacks callbacks)
      {
         lock (fBuffer)                                                       //sync all the buffered data.
         {
            foreach (UILogItem i in fBuffer)
            {
               if (i.Source == SourceType.Bot)
                  callbacks.TextOut(i);
               else
                  callbacks.TextIn(i);
            }
            fBuffer.Clear();
            if (fInputSelectionList != null)
            {
               if (fInputSelectionList.Count == 1)
               {
                  callbacks.SelectInput(fInputSelectionList[0]);
                  fInputSelectionList.Clear();                                      //don't need to do anything anymore with this.
               }
               else
                  callbacks.FilterInput(fInputSelectionList);
            }
            callbacks.SetNetworkActivity(NetworkActive);
         }
         fContext = new WeakReference(callbacks);                                //store for future ref in a weakref, so we don't keep a lock on it.
         CheckPath();                                                          //whenever we reconnect, make certain that the path has been reloaded cause this can happen after showing the preferences dialog (where the user can select a different db).
         IsOpen = true;                                                          //after a sync, we need to make certain that the network is running, cause there is a client available.
      }

      /// <summary>
      /// called when the <see cref="IAiciServerCallbacks"/> callback is done filtering the input
      /// and knows which result to use.
      /// </summary>
      /// <param name="result"> when -1, the selection was canceled and there is no valid result.</param>
      internal void PutFilteredInputResult(int result)
      {
         if (result >= 0 && fInputSelectionList != null && fInputSelectionList.Count > result)
         {
            fInputSelectionList.Clear();
            fInputSelectionList.Add(result);
            fIntSinBlock.Set();
         }
      }

      #region IAndroidService Members

      /// <summary>
      /// Instructs the service to display the main activity again.
      /// </summary>
      public void ShowMainActivity()
      {
         Intent iIntent = new Intent(this, typeof(AiciActivity));
         iIntent.SetFlags(ActivityFlags.NoHistory | ActivityFlags.NewTask | ActivityFlags.ClearTop);                             //if we don't do newTask android complains that we are starting an activity outside of another activity. If we don't to cleartopa an noHistory, we get 2 activities on the history list which causes the back button to go to aici again.
         StartActivity(iIntent);
      }

      /// <summary>
      /// Shows the add calendar activity with the specified data object.
      /// </summary>
      /// <param name="item">The item.</param>
      public void ShowAddCalendarItem(CalendarItem item)
      {
         Intent iIntent = new Intent(this, typeof(AddCalendarItemActivity));
         iIntent.SetFlags(ActivityFlags.NewTask );                             //if we don't do newTask android complains that we are starting an activity outside of another activity. If we don't to cleartopa an noHistory, we get 2 activities on the history list which causes the back button to go to aici again.
         //Bundle iExtras = iIntent.Extras;
         //iExtras.PutString("Title", item.Title);
         //iExtras.PutString("Location", item.Location);
         //iExtras.PutString("Description", item.Description);
         //iExtras.PutLong("StartDate", item.StartDate.ToBinary());
         //iExtras.PutLong("EndtDate", item.EndDate.ToBinary());
         //iExtras.PutBoolean("Recurring", item.Recurring);

         StartActivity(iIntent);
      }

      /// <summary>
      /// Shows the help.
      /// </summary>
      public void ShowHelp()
      {
         Intent iIntent = new Intent(ApplicationContext, typeof(InfoActivity));
         StartActivity(iIntent);
      }

      #endregion


      internal void Save()
      {
         string iPath = Path.Combine(fPath, NETWORKFILE);
         fIsSaving = true;
         try
         {
            Brain.Current.Save(iPath);
         }
         finally
         {
            fIsSaving = false;
         }
      }
   }
}