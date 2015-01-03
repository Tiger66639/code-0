//-----------------------------------------------------------------------
// <copyright file="AiciActivity.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/05/2012</date>
//-----------------------------------------------------------------------
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using JaStDev.HAB;
using Android.Speech.Tts;
using Java.Util;
using AiciAndroid.Network;
using Android.Speech;
using System.Collections.Generic;
using System.IO;
using os.MonoDroid;
using Android.Preferences;
using AiciAndroid.Acitivities;
using System.Net;
using JaStDev.LogService;
using Android.Graphics;
using System.Threading;
using Android.Database;

namespace AiciAndroid
{
   /// <summary>
   /// The main window.
   /// </summary>
   [Activity(Label = "Aici", MainLauncher = true, Icon = "@drawable/icon")]
   public class AiciActivity : ListActivity, TextToSpeech.IOnInitListener, IServiceConnection, IAiciServerCallbacks, IIntentResultService
   {
      #region internal types

      class StringIntentResult
      {
         /// <summary>
         /// true if the user selected the value, otherwise false.
         /// </summary>
         public bool Result { get; set; }
         /// <summary>
         /// the value that the user selected.
         /// </summary>
         public string Value { get; set; }
      }

      #endregion

      #region fields

      /// <summary>
      /// For StartActivityResult, so that we know which result to process
      /// </summary>
      const int TRYSTARTTTS = 1;
      
      const int CHECK_VOICE_DATA_PASS = 1;
      public const int FILTERINPUTRESULT = 3;                                        //id used by the FilterInputAcitivity to return a result.

      /// <summary>
      /// Identifies the general preferences bag.
      /// </summary>
      public const string AICIPREF = "AiciPreferences";
      public const string AICIDIR = "Aici";
      public const string AICICHARDIR = "AiciChar";
      public const string AICIDEFAULTDIR = "Default";

      bool fLoaded = false;
      EditText fEdit;
      ProgressBar fIsRunningToggle;
      TextToSpeech fTTS;
      LogAdapter fLog;
      StringIntentResult fLastStringResult;
      static AiciServer fServer;

      STT fSTT;
      AutoResetEvent fResultFlag;
      #endregion

      #region prop

      #region STT

      /// <summary>
      /// Gets the STT.
      /// </summary>
      public STT Stt
      {
         get { return fSTT; }
      }

      #endregion

      #region Server
      /// <summary>
      /// Gets the server.
      /// this is made available so that other  parts an easily get to it (maybe not such a good idea)
      /// </summary>
      static public AiciServer Server
      {
         get { return fServer; }
      } 
      #endregion

      #endregion

      #region overrides

     

      /// <summary>
      /// Called when the activity is created.
      /// </summary>
      /// <param name="bundle">The bundle.</param>
      protected override void OnCreate(Bundle bundle)
      {
         try
         {
            base.OnCreate(bundle);
            AiciPrefs.Default = new AiciPrefs(this);                                               //get and store the props. So it only has to be odne 1 time.                                
			
            TryStartTTS();
            fSTT = new STT(this);

			bool iCustomTitleSupported = RequestWindowFeature(WindowFeatures.CustomTitle);			//has to be called before SEtContentView.
			SetContentView(Resource.Layout.MainList);                                            // Set our view from the "main" layout resource	
			if (iCustomTitleSupported == true)
	         { 
		         Window.SetFeatureInt(WindowFeatures.CustomTitle,Resource.Layout.TitleBar);
		         TextView iTitle = (TextView)FindViewById(Resource.Id.Apptitle);
		         fIsRunningToggle = (ProgressBar)FindViewById(Resource.Id.ToggleIsRunning);
		         iTitle.Text = "Aici";
	         }            
			
            LoadMicButton();

            SetupLog();                                                                            //do this before requesting to start the server, always safer cause the server could have to sync some log data.            
            if (AiciPrefs.Default.IsFirstRun == false)                                             //when firstRun -> first need to install stuff, only then can we start the server.
            {
               StartServer();
            }
            else
               CheckFirstRunBeforeDBLoad();
         }
         catch 
         {
            Toast iToast = Toast.MakeText(this, "Something went serriously wrong while trying to start the app!", ToastLength.Long);
            iToast.Show();
         }
      }

      private void SetBackground()
      {
         RelativeLayout iScreen = (RelativeLayout)FindViewById(Resource.Id.MainLayout);
         if (iScreen != null)
         {
            iScreen.SetBackgroundColor(new Color(AiciPrefs.Default.BackgroundColor));
            if (string.IsNullOrEmpty(AiciPrefs.Default.BackgroundImage) == false && File.Exists(AiciPrefs.Default.BackgroundImage))
            {
               Bitmap iBitmap = BitmapFactory.DecodeFile(AiciPrefs.Default.BackgroundImage);
               if (iBitmap != null)
               {
                  ImageView iImage = FindViewById<ImageView>(Resource.Id.ImgBackground);
                  iImage.SetImageBitmap(iBitmap);
               }
            }
         }
      }

      private void SetupLog()
      {
         JavaList<UILogItem> iList = LastNonConfigurationInstance as JavaList<UILogItem>;    //it could be that the activity was restarted because of a config change (monitor tilt), in which case we passed along the log data, so restore this.
         if (iList == null)
            fLog = new LogAdapter(this, Resource.Layout.Log_List_Item);                         //set up the log listview.
         else
            fLog = new LogAdapter(this, Resource.Layout.Log_List_Item, iList);
         ListAdapter = fLog;
      }

      /// <summary>
      /// sets up the button that activates microphone input.
      /// </summary>
      private void LoadMicButton()
      {
         ImageButton iMic = FindViewById<ImageButton>(Resource.Id.Mic);                          //set up the 'enter' button
         iMic.Click += new EventHandler(Talk_Click);

         //ImageButton iCall = FindViewById<ImageButton>(Resource.Id.BtnCall);                          //set up the 'enter' button
         //iCall.Click += new EventHandler(Call_Click);

         //Button iFb = FindViewById<Button>(Resource.Id.BtnEvent);                          //set up the 'enter' button
         //iFb.Click += new EventHandler(ScheduleEvent_Click);


         //Button iTwitter = FindViewById<Button>(Resource.Id.BtnTwitter);                          //set up the 'enter' button
         //iTwitter.Click += new EventHandler(Twitter_Click);

         //Button iSms = FindViewById<Button>(Resource.Id.BtnSms);                          //set up the 'enter' button
         //iSms.Click += new EventHandler(Sms_Click);

         //ImageButton iEmail = FindViewById<ImageButton>(Resource.Id.BtnEmail);                          //set up the 'enter' button
         //iEmail.Click += new EventHandler(Email_Click);

         if (fSTT.HasSTT == false)
         {
            #if !DEBUG
            iMic.Enabled = false;
            #endif
            //iEmail.Enabled = false;
            //iCall.Enabled = false;
            //iFb.Enabled = false;
            //iTwitter.Enabled = false;
            //iSms.Enabled = false;
         }
      }

      /// <summary>
      /// Sets up the button that sends the text in the edit box to the engine.
      /// </summary>
      private void LoadSendButtonAndText()
      {
         fEdit = FindViewById<EditText>(Resource.Id.Input);                               //set up the input text
         Button button = FindViewById<Button>(Resource.Id.Send);                          //set up the 'enter' button
         button.Visibility = ViewStates.Visible;
         button.Click += new EventHandler(Send_Click);
         fEdit.Visibility = ViewStates.Visible;
         fEdit.KeyPress += new EventHandler<View.KeyEventArgs>(iEdit_KeyPress);
      }

      /// <summary>
      /// when the prefs are changed, this window doesn't get recreated, but usually "OnStart' is called. So the callbacks
      /// are still registered, we simply need to update the view.
      /// </summary>
      private void ResetSendButtonAndText()
      {
         if (fSTT != null && fSTT.HasSTT == false || AiciPrefs.Default.PreferShowTextInput == true)                                                         //if there is no stt, allow text input
         {
            if (fEdit == null)
               LoadSendButtonAndText();
            else
            {
               fEdit.Visibility = ViewStates.Visible;
               Button button = FindViewById<Button>(Resource.Id.Send);                          //set up the 'enter' button
               button.Visibility = ViewStates.Visible;
            }
         }
         else if (fEdit != null)
         {
            fEdit.Visibility = ViewStates.Gone;
            Button button = FindViewById<Button>(Resource.Id.Send);                          //set up the 'enter' button
            button.Visibility = ViewStates.Gone;
         }
      }

      /// <summary>
      ///   <i>Derived classes must call through to the super class's
      /// implementation of this method.  If they do not, an exception will be
      /// thrown.</i>
      /// </summary>
      /// <since version="API Level 1"/>
      protected override void OnDestroy()
      {
         if (IsFinishing == true)                                                //OnDestroy can be called when the config is changed (activiity is restarted) or the app is permantly finished. in the latter case, stop the service.
         {
            Intent iIntent = new Intent(this, typeof(AiciServer));
            StopService(iIntent);
         }
         base.OnDestroy();
      }

      /// <summary>
      ///   <i>Derived classes must call through to the super class's
      /// implementation of this method.  If they do not, an exception will be
      /// thrown.</i>
      /// </summary>
      /// <since version="API Level 1"/>
      protected override void OnStart()
      {
         base.OnStart();
         if(AiciPrefs.Default.IsFirstRun == false)
         {
         	SetBackground();                                                                 //whenever we start, reset the background. We could ahve been started after a pref change
         	BindToAici();
         }
         ResetSendButtonAndText();
      }
      
      void BindToAici()
      {
         Intent intent = new Intent(this, typeof(AiciServer));                            //need to bind to the service so that we can communicate with it.
         BindService(intent, this, Bind.AutoCreate);
      }

      /// <summary>
      ///   <i>Derived classes must call through to the super class's
      /// implementation of this method.  If they do not, an exception will be
      /// thrown.</i>
      /// </summary>
      /// <since version="API Level 1"/>
      protected override void OnStop()
      {
         base.OnStop();
         if (fServer != null)
            UnbindService(this);
      }

      /// <summary>
      /// The guarantee of no message handling during the switch to the next
      /// activity simplifies use with active objects.
      /// </summary>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>Return any Object holding the desired state to propagate to the
      /// next activity instance.</term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      public override Java.Lang.Object OnRetainNonConfigurationInstance()
      {
         if (fLog != null)
         {
            JavaList<UILogItem> iList = new JavaList<UILogItem>();  //needs to be a java object. can't return the adapter itself cause that causes mem leaks (would take along all the ui elements)
            for (int i = 0; i < fLog.Count;i++)
               iList.Add(fLog.GetItem(i));
            return iList;
         }
         else
            return null;
      }

      /// <summary>
      /// You will receive this call immediately before onResume() when your
      /// activity is re-starting.
      /// </summary>
      /// <param name="requestCode">The integer request code originally supplied to
      /// startActivityForResult(), allowing you to identify who this
      /// result came from.</param>
      /// <param name="resultCode">The integer result code returned by the child activity
      /// through its setResult().</param>
      /// <param name="data">An Intent, which can return result data to the caller
      /// (various data can be attached to Intent "extras").</param>
      /// <since version="API Level 1"/>
      protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
      {
         if (requestCode == TRYSTARTTTS)
         {
            if ((int)resultCode == CHECK_VOICE_DATA_PASS)
               fTTS = new TextToSpeech(this, this);                                                // success, create the TTS instance
            else
            {
               TryInstallTTS();
            }
         }
         else if (requestCode == STT.HANDLESTTINPUT)        //got some input text from the STT
         {
            if (resultCode == Result.Ok && fServer != null)
               fSTT.ProcessInput(data.GetStringArrayListExtra(RecognizerIntent.ExtraResults));             //get the results of the STT and send them to the network.
         }
         else if (requestCode == FILTERINPUTRESULT)                                                   //the user made a selection from the list of possible voice inputs.
         {
            if (resultCode == Result.Ok && fServer != null)
               fSTT.SelectInput(data.GetIntExtra("result", -1));
         }
         else if (requestCode == PhoneManagement.STRINGRESULT)
         {
            fLastStringResult = new StringIntentResult();
            fLastStringResult.Result = resultCode == Result.Ok;
            Android.Net.Uri iRes = data.Data;
            fLastStringResult.Value = ExtractStringResFromUri(iRes);
            ResultFlag.Set();                                                                //let any monitors know that there is a result value.
         }

         base.OnActivityResult(requestCode, resultCode, data);
      }

      /// <summary>
      /// extracts the string value from
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      String ExtractStringResFromUri(Android.Net.Uri uri)
      {
         String[] projection = { "_data" };
         ICursor cursor = ManagedQuery(uri, projection, null, null, null);
         int column_index = cursor.GetColumnIndexOrThrow("_data");
         cursor.MoveToFirst();
         return cursor.GetString(column_index);
      }

      private void TryInstallTTS()
      {
         AlertDialog.Builder iAlert = new AlertDialog.Builder(this);
         iAlert.SetTitle("No TTS");
         iAlert.SetMessage("Could not find a Text-to-speech engine. Would you like to install one from google play?");
         iAlert.SetPositiveButton("Ok", delegate
            {
               Intent installIntent = new Intent(TextToSpeech.Engine.ActionInstallTtsData);     // missing data, install it
               StartActivity(installIntent);
            });
         iAlert.SetNegativeButton("Cancel", delegate
            {
               AiciPrefs.Default.SetTTSActivated(false, this);
            });
         iAlert.Show();
      }

      /// <summary>
      /// check if this is the first run, if so, ask if the user want's to sync the data.
      /// </summary>
      /// <returns></returns>
      private void CheckFirstRunBeforeDBLoad()
      {
         try
         {
            NetworkManager.InstallDefaultWithDlg(this, new Action(DoAfterInstallDB));
         }
         catch (Exception e)
         {
            Log.LogError("Check first run", e.ToString());
         }
      }

      /// <summary>
      /// the stuff that needs to be done after everything is installed for the first run.
      /// </summary>
      void DoAfterInstallDB()
      {
         StartServer();
         SetBackground();
         BindToAici();
      }

      /// <summary>
      /// check if this is the first run, if so, ask if the user want's to sync the data.
      /// </summary>
      /// <returns></returns>
      private void CheckFirstRunAfterDBLoad()
      {
         try
         {
            if (AiciPrefs.Default.IsFirstRun == true)
            {
               AiciPrefs.Default.ResetFirstRun(this);
               ContactsLoader.Load(this);                                                            //we try to load the contact info at first run so that this is imported.
            }
         }
         catch (Exception e)
         {
            Log.LogError("Check first run", e.ToString());
         }
      }
      
      #region option menu

      /// <summary>
      /// When you add items to the menu, you can implement the Activity's
      /// <c><see cref="M:Android.App.Activity.OnOptionsItemSelected(Android.Views.IMenuItem)"/></c> method to handle them there.
      /// </summary>
      /// <param name="menu">The options menu in which you place your items.</param>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>You must return true for the menu to be displayed;
      /// if you return false it will not be shown.</term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      public override bool OnCreateOptionsMenu(IMenu menu)
      {
         MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
         return true;
      }

      /// <summary>
      /// Derived classes should call through to the base class for it to
      /// perform the default menu handling.
      /// </summary>
      /// <param name="item">The menu item that was selected.</param>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>boolean Return false to allow normal menu processing to
      /// proceed, true to consume it here.</term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      public override bool OnOptionsItemSelected(IMenuItem item)
      {
         switch (item.ItemId)
         {
            case Resource.Id.Menu_SyncContacts:
               ContactsLoader.Load(this, "Sync contacts");                                                            //we try to load the contact info at first run so that this is imported.
               return true;
            //case Resource.Id.Menu_Panic:
            //   fServer.Panic();
            //   return true;
            //case Resource.Id.Menu_Save:
             //  fServer.Save();
             //  return true;
            case Resource.Id.Menu_Preferences:
               ShowPrefs();
               return true;
            case Resource.Id.Menu_Info:
               ShowInfo();
               return true;
            default:
               return base.OnOptionsItemSelected(item);
         }
      }


      /// <summary>
      /// shows the prefs menu and passes the available voices along (if there are any), so that the prefs menu can include them.
      /// Can't pass along pointers, only values.
      /// </summary>
      private void ShowPrefs()
      {
         Intent iIntent = new Intent(ApplicationContext, typeof(AiciPreferencesActivity));
         if (fTTS != null)
         {
            List<string> iText = new List<string>();
            List<string> iValues = new List<string>();
            iText.Add("Default");
            iValues.Add("");
            foreach (Locale i in Locale.GetAvailableLocales())
            {
               if (fTTS.IsLanguageAvailable(i) == LanguageAvailableResult.Available)
               {
                  iText.Add(i.DisplayName);
                  iValues.Add(i.Language);
               }
            }
            iIntent.PutExtra("Lang_text", iText.ToArray());
            iIntent.PutExtra("Lang_value", iValues.ToArray());
         }
         StartActivity(iIntent);
      }

      public void ShowInfo()
      {
         Intent iIntent = new Intent(ApplicationContext, typeof(InfoActivity));
         StartActivity(iIntent);
      }

      #endregion
      
      #endregion

      


      #region server


      //Connects the <see cref="AiciAcitivity"/> with the AiciService. It functions as a callback:
      // when the service gets created or destroyed, this is the callback object.
      #region IServiceConnection Members

      /// <summary>
      /// Called when a connection to the Service has been established, with
      /// the <c><see cref="T:Android.OS.BinderConsts"/></c> of the communication channel to the
      /// Service.
      /// </summary>
      /// <param name="name">The concrete component name of the service that has
      /// been connected.</param>
      /// <param name="service">The IBinder of the Service's communication channel,
      /// which you can now make calls on.</param>
      /// <since version="API Level 1"/>
      public void OnServiceConnected(ComponentName name, IBinder service)
      {
         AiciBinder iBinder = service as AiciBinder;
         fServer = iBinder.Server;
         if (fServer != null)
         {
            try
            {
               fServer.Sync(this);
               RunOnUiThread(new Action(CheckFirstRunAfterDBLoad));
            }
            catch(Exception e)
            {
               Log.LogError("Aici activity", e.ToString());
               Toast iToast = Toast.MakeText(this, "Failed to connect to the network, try selecting a different project.", ToastLength.Long);
               iToast.Show();
            }
         }
      }

      /// <summary>
      /// Called when a connection to the Service has been lost.
      /// </summary>
      /// <param name="name">The concrete component name of the service whose
      /// connection has been lost.</param>
      /// <since version="API Level 1"/>
      public void OnServiceDisconnected(ComponentName name)
      {
         fServer = null;
      }

      #endregion

      /// <summary>
      /// Makes certain that the Aici server is running. This doens't open the network itself, just the server
      /// </summary>
      private void StartServer()
      {
         Intent iIntent = new Intent(this, typeof(AiciServer));
         StartService(iIntent);
      } 
      #endregion

      #region event handlers
      /// <summary>
      /// Handles the KeyPress event of the iEdit control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="Android.Views.View.KeyEventArgs"/> instance containing the event data.</param>
      void iEdit_KeyPress(object sender, View.KeyEventArgs e)
      {
         if (e.KeyCode == Keycode.Enter)
            Send_Click(sender, e);
         else
            e.Handled = false;                        //if it's not an enter, let the system deal with it.
      }


      /// <summary>
      /// Handles the Click event of the Send control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Send_Click(object sender, EventArgs e)
      {
         if (fServer != null)
         {
            if (fServer.IsOpen == true)
            {
               if (string.IsNullOrEmpty(fEdit.Text) == false)                                               //somehow, we get the event 2 times if an enter is pressed.
               {

                  UILogItem iNew = new UILogItem() { Source = SourceType.User, Text = fEdit.Text };
                  LogItem(iNew);
                  fEdit.Text = "";
                  fServer.Process(iNew.Text);
               }
            }
            else
               Toast.MakeText(this, "Please select a project to work with.", ToastLength.Short).Show();
         }
         else
            Toast.MakeText(this, "The Aicie service isn't running", ToastLength.Short).Show();
      }

      /// <summary>
      /// stores the log item and checks if it needs to be send to the server for logging.
      /// </summary>
      /// <param name="iNew"></param>
      private void LogItem(UILogItem iNew)
      {
         fLog.AddLogItem(iNew);
         if (AiciPrefs.Default.LogToServer == true)
         {
            string iBy = AiciPrefs.Default.AppId;
            string address = string.Format("http://androidserver.bragisoft.com/collect/store?mode={0}&val={1}&by={2}", Uri.EscapeDataString(iNew.Source.ToString()), Uri.EscapeDataString(iNew.Text), Uri.EscapeDataString(iBy));
            using (WebClient client = new WebClient())
               client.DownloadStringAsync(new Uri(address));               //we do async, we don't need to wait for a result anyway
         }
      }

      /// <summary>
      /// Handles the Click event of the Send control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Talk_Click(object sender, EventArgs e)
      {
         try
         {
            //string iID = AiciContacts.TestGetID();
            //if (iID != null)
            //{
            //   string iFound = AiciContacts.GetContactEmail(iID);
            //   Communication.SendEmail(new string[] { iFound }, "subject test", "subject content");
            //}

            //fServer.IsOpen = false;

            fSTT.TryStartSTT(this);
         }
         catch (Exception ex)
         {
            Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
         }
      }


      /// <summary>
      /// Handles the Click event of the Call control.
      /// sets the engine up  so that it will do a call after the next voice input
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Call_Click(object sender, EventArgs e)
      {
         fServer.Process("call");
         fSTT.TryStartSTT(this);
      } 

      /// <summary>
      /// Handles the Click event of the FacebookBtn control.
      /// sets the engine up  so that it will do a facebook post after the next voice input
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void ScheduleEvent_Click(object sender, EventArgs e)
      {
         fServer.Process("schedule");
         fSTT.TryStartSTT(this);
      } 


      /// <summary>
      /// Handles the Click event of the TwitterBtn control.
      /// sets the engine up  so that it will do a Twitter post after the next voice input
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Twitter_Click(object sender, EventArgs e)
      {
         fServer.Process("twitter");
         fSTT.TryStartSTT(this);
      } 

      /// <summary>
      /// Handles the Click event of the SmsBtn control.
      /// sets the engine up  so that it will send an sms after the next voice input
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Sms_Click(object sender, EventArgs e)
      {
         fServer.Process("sms");
         fSTT.TryStartSTT(this);
      } 

      /// <summary>
      /// Handles the Click event of the EmailBtn control.
      /// sets the engine up  so that it will send an email after the next voice input
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Email_Click(object sender, EventArgs e)
      {
         fServer.Process("email");
         fSTT.TryStartSTT(this);
      } 

      #endregion

      #region STT

     

      #endregion

      #region TTS
      /// <summary>
      /// Tries to start the TTS engine if available.
      /// </summary>
      private void TryStartTTS()
      {
         Intent checkIntent = new Intent(TextToSpeech.Engine.ActionCheckTtsData);
         StartActivityForResult(checkIntent, TRYSTARTTTS);
         VolumeControlStream = Android.Media.Stream.Music; //this binds the TTS volume to the volume buttons.  in android == AudioManager.Stream_music 
      }

      #region IOnInitListener Members

      /// <summary>
      /// Called when the TTS engine is done initializing.
      /// </summary>
      /// <param name="status">The status.</param>
      public void OnInit(OperationResult status)
      {
         Locale iPrefered = AiciPrefs.Default.PreferedLanguage;
         if (iPrefered != null && iPrefered != fTTS.Language)
         {
            LanguageAvailableResult iRes =  fTTS.IsLanguageAvailable(iPrefered);
            if (iRes == LanguageAvailableResult.MissingData || iRes == LanguageAvailableResult.NotSupported)
            {
               Toast toast = Toast.MakeText(ApplicationContext, string.Format("The prefered language {0} was not available, reverting to {1}.", iPrefered.DisplayName, fTTS.Language.DisplayName), ToastLength.Short);
               toast.Show();
            }
            else
               fTTS.SetLanguage(iPrefered);
         }
      }


      #endregion



      #endregion


      /// <summary>
      /// Adds specified item to the log and if there is an STT, it is also spoken. This needs to be called from the UI thread.
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      internal void AddOutput(UILogItem item)
      {
         fSTT.CurrentInputs = null;                                                                             //input has been processed, so remove any possible buffered inputs cause they are no longer valid.
         LogItem(item);
         try
         {
            if (fTTS != null)
               fTTS.Speak(item.Text, QueueMode.Add, null);
         }
         catch (Exception e)
         {
            Toast iToast = Toast.MakeText(this, "Something is wrong with the Text to speech engine: " + e.Message, ToastLength.Long);
            iToast.Show();
         }
      }


      #region IAiciServerCallbacks Members

      /// <summary>
      /// Called when there is output text that needs to be processed. This is not called from the UI thread.
      /// </summary>
      /// <param name="item"></param>
      public void TextOut(UILogItem item)
      {
         RunOnUiThread(() => AddOutput(item));
      }

      /// <summary>
      /// called when the server has some buffered incomming text that still needs to be synced with the UI. This happens when there was voice input
      /// from a subwindow.
      /// </summary>
      /// <param name="item"></param>
      public void TextIn(UILogItem item)
      {
         RunOnUiThread(() => LogItem(item));
      }

      /// <summary>
      /// Called when the actity needs to ask the user to select 1 item from the list. When the user has made his selection,
      /// the index of the selected item is returned as a child of the list, all the other values should be removed.
      /// </summary>
      /// <param name="list">The list.</param>
      public void FilterInput(List<int> list)
      {
         if (fSTT.CurrentInputs != null)
         {
            List<string> iValues = new List<string>();
            foreach (int i in list)
               iValues.Add(fSTT.CurrentInputs[i]);
            fSTT.FilterInput(iValues,list);
         }
         else
         {
            Toast.MakeText(this, "The system is trying to filter multiple inputs that it can't find the source for", ToastLength.Long).Show();
            list.Clear();
         }
      }

      /// <summary>
      /// called when a list of possible inputs was sent to the engine and 1 final result got found. This can be used to update
      /// the ui and or train the STT.
      /// </summary>
      /// <param name="index"></param>
      public void SelectInput(int index)
      {
         UILogItem iNew = new UILogItem() { Source = SourceType.User, Text = fSTT.CurrentInputs[index] };
         LogItem(iNew);
         fEdit.Text = "";
      }

      /// <summary>
      /// called when a list of possible inputs was sent to the engine and 1 final result got found. This can be used to update
      /// the ui and or train the STT.
      /// For calls from threads other than the ui.
      /// </summary>
      /// <param name="index"></param>
      public void SelectInputAsync(int index)
      {
         RunOnUiThread(() => SelectInput(index));
      }

      /// <summary>
      /// Sets the network activity.
      /// </summary>
      /// <param name="NetworkActive">if set to <c>true</c> [network active].</param>
      public void SetNetworkActivity(bool isActive)
      {
         if (fIsRunningToggle != null)
         {
            RunOnUiThread(() => 
            {
               if (isActive)
                  fIsRunningToggle.Visibility = ViewStates.Visible;
               else
                  fIsRunningToggle.Visibility = ViewStates.Gone;
            });
         }
      }

      #endregion


      
      #region IIntentResultService Members

      /// <summary>
      /// the event to signal when there is a result found (or canceled). This allows the calling thread to block untill the user is done.
      /// </summary>
      public AutoResetEvent ResultFlag
      {
         get 
         {
            if (fResultFlag == null)
               fResultFlag = new AutoResetEvent(false);
            return fResultFlag; 
         }
      }

      /// <summary>
      /// gets the last string result (if not canceled.
      /// </summary>
      /// <param name="value"></param>
      /// <returns>
      /// true if the user selected the value, false if the user canceled the action.
      /// </returns>
      public bool GetStringResult(out string value)
      {
         if (fLastStringResult != null)
         {
            value = fLastStringResult.Value;
            return fLastStringResult.Result;
         }
         else
         {
            value = null;
            return false;
         }
      }

      #endregion

   }
}