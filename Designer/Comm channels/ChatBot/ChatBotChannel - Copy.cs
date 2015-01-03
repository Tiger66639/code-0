//-----------------------------------------------------------------------
// <copyright file="ChatBotChannel - Copy.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>07/08/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.IO;
using JaStDev.LogService;
using System.Collections;
using System.Windows.Threading;
using System.Xml;
using System.Windows;
using JaStDev.HAB.Designer.Test;
using JaStDev.HAB.Characters;
using System.Diagnostics;

namespace JaStDev.HAB.Designer
{

   /// <summary>
   /// A commChannel that wraps the <see cref="FaceSin"/> and which can link to another <see cref="TextSin"/>
   /// for performing text input and output. The main sin being wrapped though, is the 'face' interface and
   /// not the text one (which is handled by text channels).
   /// </summary>
   public class ChatBotChannel : CommChannel, ITesteable
   {
      #region const

      public const string BOT = "bot: ";
      const string USER = "You: ";
      const string CHARARCTERSFILTER = "*.char";
      const string CCSCHARFILTER = "*" + Character.CCSFILEEXT;
      const string SSMLSTRING = "<?xml version='1.0'?><speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemalocation='http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='en-US'>{0}</speak>";
                                    
      #endregion

      #region fields 
      ObservableCollection<string> fConversationLog = new ObservableCollection<string>();
      SpeechSynthesizer fSpeaker;
      SpeechRecognizer fRecognizer;
      string fInputText;                                           //the default value. 
      Voice fSelectedVoice;
      static Voices fAvailableVoices;
      ulong fTextSin;                                                //stores a ref to the text sin as id, for loading from file
      Character fSelectedCharacter;
      static List<Character> fCharacters;
      DispatcherTimer fIdleTimer;            //used to keep track off 'idle' times, so that we can start small animations to show 'liveliness'.
      VisemeRecorder fPlayer;
      Random fIdleRandomizer = new Random();                         //a randomizer for the idle times and animations.
      bool fIsSpeaking = false;
      bool fShowFloatingChar = false;
      CharacterWindow fCharWindow;
      double fZoomValue = 1.0;
      #endregion


      /// <summary>
      /// Initializes a new instance of the <see cref="ChatBotChannel"/> class.
      /// </summary>
      public ChatBotChannel()
      {
         fPlayer = new VisemeRecorder(this);
      }

      #region prop
      #region ConversationLog

      /// <summary>
      /// Gets the conversation log in plain text
      /// </summary>
      public ObservableCollection<string> ConversationLog
      {
         get { return fConversationLog; }
      }

      #endregion

      #region AudioOutOn

      /// <summary>
      /// Gets/sets wether oudio output is activated or not.
      /// </summary>
      public bool AudioOutOn
      {
         get
         {
            return fSpeaker != null;
         }
         set
         {
            if (AudioOutOn != value)
            {
               if (value == true)
               {
                  fSpeaker = new SpeechSynthesizer();
                  fSpeaker.SpeakStarted += fPlayer.fSpeaker_SpeakStarted;
                  fSpeaker.VisemeReached += fPlayer.fSpeaker_VisemeReached;
                  fSpeaker.BookmarkReached += fPlayer.fSpeaker_BookmarkReached;
                  fSpeaker.SpeakCompleted += fPlayer.fSpeaker_SpeakCompleted;
                  OnPropertyChanged("AvailableVoices");                                //reload all the available voices.
                  if (SelectedVoice != null)
                     fSpeaker.SelectVoice(SelectedVoice.Name);
                  else
                     SetSelectedVoice(fSpeaker.Voice.Name);
               }
               else
               {
                  fSpeaker.SpeakStarted -= fPlayer.fSpeaker_SpeakStarted;
                  fSpeaker.VisemeReached -= fPlayer.fSpeaker_VisemeReached;
                  fSpeaker.BookmarkReached -= fPlayer.fSpeaker_BookmarkReached;
                  fSpeaker.SpeakCompleted -= fPlayer.fSpeaker_SpeakCompleted;
                  fSpeaker = null;
               }
               OnPropertyChanged("AudioOutOn");
            }
         }
      }


      #endregion

      #region AudioInOn

      /// <summary>
      /// Gets/sets wether oudio input is activated or not.
      /// </summary>
      public bool AudioInOn
      {
         get
         {
            return fRecognizer != null;
         }
         set
         {
            if (AudioInOn != value)
            {
               if (value == true)
               {
                  fRecognizer = new SpeechRecognizer();
                  fRecognizer.SpeechRecognized += fRecognizer_SpeechRecognized;
               }
               else
               {
                  fRecognizer.SpeechRecognized -= fRecognizer_SpeechRecognized;
                  fRecognizer = null;
               }
               OnPropertyChanged("AudioInOn");
            }
         }
      }

      #endregion

      #region InputText

      /// <summary>
      /// Gets/sets the text currently typed in, but not yet sent to the network. This allows us to remember the input
      /// text when the user has changed tabs. Default value = 'Type here'.
      /// </summary>
      public string InputText
      {
         get
         {
            return fInputText;
         }
         set
         {
            fInputText = value;
            OnPropertyChanged("InputText");
         }
      }

      #endregion 

      #region SelectedVoice

      /// <summary>
      /// Gets/sets the name of the currently selected voice that is used as output.
      /// </summary>
      public Voice SelectedVoice
      {
         get
         {
            return fSelectedVoice;
         }
         set
         {
            if (AudioOutOn == true && value != fSelectedVoice)
            {
               try
               {
                  if (value != null)
                     fSpeaker.SelectVoice(value.Name);
                  else
                     fSpeaker.SelectVoice(null);
                  SetSelectedVoice(value);
               }
               catch (Exception e)
               {
                  Log.LogError("Chatbot", string.Format("Failed to load voice: {0}, with message: {1}.", value, e.Message));
                  fSpeaker = null;                                                              //can't have a speeker, there  is no valid voice.
               }
            }
         }
      }

      /// <summary>
      /// stores the currently selected voice and raises the event, but doesn't change the engine.
      /// </summary>
      /// <param name="value">The value.</param>
      private void SetSelectedVoice(Voice value)
      {
         if (value.PreferedCharacter != null)                                    //if there is a prefered char to assign, check if the user hasn't already changed the char to seomthing.
         {
            if (SelectedVoice != null)
            {
               if (SelectedVoice.PreferedCharacter != null)
               {
                  if (SelectedCharacter != null && SelectedVoice.PreferedCharacter == SelectedCharacter.Name)
                     SelectedCharacter = (from i in Characters where i.Name == value.PreferedCharacter select i).FirstOrDefault();
               }
            }
            else
               SelectedCharacter = (from i in Characters where i.Name == value.PreferedCharacter select i).FirstOrDefault();
         }
         fSelectedVoice = value;
         OnPropertyChanged("SelectedVoice");
      }

      /// <summary>
      /// stores the currently selected voice and raises the event, but doesn't change the engine.
      /// This after converting the string to a voice object.
      /// </summary>
      /// <param name="value">The value.</param>
      private void SetSelectedVoice(string value)
      {
         var iFound = (from i in AvailableVoices where i.Name == value select i).FirstOrDefault();
         if (iFound != null)
            SetSelectedVoice(iFound);
         else
            throw new InvalidOperationException("Unexpected voice: " + value);
      }

      #endregion



      #region AvailableVoices
      /// <summary>
      /// Gets all the available voices for the current speaker..
      /// </summary>
      public static Voices AvailableVoices
      {
         get
         {
            if (fAvailableVoices == null)
            {
               SpeechSynthesizer iSpeaker = new SpeechSynthesizer();
               fAvailableVoices = Voices.RetrieveAvailableVoices(iSpeaker);
            }
            return fAvailableVoices;
         }
      }

      #endregion

      #region TextSin

      /// <summary>
      /// Gets/sets the sin to use for handling text.
      /// </summary>
      public TextSin TextSin
      {
         get
         {
            if (fTextSin != Neuron.EmptyId)
            {
               Neuron iFound;
               if (Brain.Current.TryFindNeuron(fTextSin, out iFound) == true)
                  return iFound as TextSin;
            }
            return null;
         }
         internal set
         {
            TextSin iText = TextSin;
            if (iText != null)
               iText.TextOut -= Sin_TextOut;
            if (value != null)
            {
               fTextSin = value.ID;
               value.TextOut += Sin_TextOut;
            }
            else
               fTextSin = Neuron.EmptyId;
            OnPropertyChanged("TextSin");
         }
      }

      #endregion

      #region Textsin (ITesteable Members)

      /// <summary>
      /// Gets or sets the textsin that should be used to communicate with during testing.
      /// </summary>
      /// <value>
      /// The textsin.
      /// </value>
      public TextSin Textsin
      {
         get { return TextSin; }
      }


      #endregion

      #region SelectedCharacter

      /// <summary>
      /// Gets/sets the currently selected character that needs to be displayed.
      /// </summary>
      public Character SelectedCharacter
      {
         get
         {
            return fSelectedCharacter;
         }
         set
         {
            if (value != fSelectedCharacter)
            {
               if (fSelectedCharacter != null)
               {
                  fSelectedCharacter.IsLoaded = false;
                  fSelectedCharacter.IdleAnimationFinished -= fSelectedCharacter_IdleAnimationFinished;
               }
               fSelectedCharacter = value;
               if (fSelectedCharacter != null)
                  InitSelectedChar();
               else if (fIdleTimer != null)
                  fIdleTimer.Stop();                                               //if there is no no char, simply stop the idle timer.
               OnPropertyChanged("SelectedCharacter");
            }
         }
      }

      /// <summary>
      /// Prepares everything for the selected character, when it just got selected.
      /// </summary>
      private void InitSelectedChar()
      {
         if (fSelectedCharacter != null)
         {
            fSelectedCharacter.IsLoaded = true;
            fSelectedCharacter.IdleAnimationFinished += fSelectedCharacter_IdleAnimationFinished;
            if (fIdleTimer != null)                                              //when a chatbotchannel is first created, it has no idle timer, so create one. When read from xml file, this setter isn't called, isntead the 'AfterLoaded' is called. (multi threading issues othersise)
               ResetIdleTimer();                                                 //each time there is a new char, we reset the idle timer to a new time out, so we have variations.
            else
               CreateIdleTimer();
         }
      }


      #endregion

      #region Characters
      /// <summary>
      /// Gets all the available characters.
      /// </summary>
      static public List<Character> Characters
      {
         get
         {
            if (fCharacters == null)
            {
               fCharacters = new List<Character>();
               if (Directory.Exists(Properties.Settings.Default.CharactersPath) == true)
               {
                  GetCharsFromDir(Properties.Settings.Default.CharactersPath);
                  string[] iSubs = Directory.GetDirectories(Properties.Settings.Default.CharactersPath);
                  foreach (string i in iSubs)
                     GetCharsFromDir(i);
               }
            }
            return fCharacters;
         }
      }

      static private void GetCharsFromDir(string path)
      {
         string[] iFiles = Directory.GetFiles(path, CHARARCTERSFILTER);
         if (iFiles != null)
         {
            foreach (string iFile in iFiles)
            {
               Character iNew = new Character(iFile);
               fCharacters.Add(iNew);
            }
         }

         iFiles = Directory.GetFiles(path, CCSCHARFILTER);
         if (iFiles != null)
         {
            foreach (string iFile in iFiles)
            {
               Character iNew = new Character(iFile);
               fCharacters.Add(iNew);
            }
         }
      } 
      #endregion
      
      #region IsSpeaking

      /// <summary>
      /// Gets/sets if the chatbot is currently speaking or not. This is used to determine if new idle events need to be triggered.
      /// </summary>
      public bool IsSpeaking
      {
         get
         {
            return fIsSpeaking;
         }
         internal set
         {
            fIsSpeaking = value;
            OnPropertyChanged("IsSpeaking");
         }
      }

      #endregion

      #region ShowFloatingChar

      /// <summary>
      /// Gets/sets wether the character is floating or not
      /// </summary>
      /// <remarks>
      /// the UI should use <see cref="ShowFloatingCharUI"/> cause this will switch on/off when the 
      /// chatbot channel is made visible/invisible, while this value remains the same.
      /// </remarks>
      public bool ShowFloatingChar
      {
         get
         {
            return fShowFloatingChar;
         }
         set
         {
            if (value != fShowFloatingChar)
               SetShowFloatingChar(value);
         }
      }

      private void SetShowFloatingChar(bool value)
      {
         fShowFloatingChar = value;
         OnPropertyChanged("ShowFloatingChar");
      }

      #endregion

      #region ShowFloatingCharUI

      /// <summary>
      /// Gets/sets wether the character is currently floating or not and also visible.
      /// </summary>
      /// <remarks>
      /// The chatbot channel view could be unloaded, cause the user switched to another tab, but
      /// if the char is floating, it should remain visible, so this needs to be controlled seperatly.
      /// 
      /// Note: this proerty is controlled from within the view: when it is ready to show/hide the
      /// character, it uses this property to actually visualise/ hide it.
      /// </remarks>
      public bool ShowFloatingCharUI
      {
         get
         {
            return fCharWindow != null;
         }
         set
         {
            if (value != ShowFloatingCharUI)
               SetShowFloatingCharUI(value);
         }
      }

      private void SetShowFloatingCharUI(bool value)
      {
         if (value == true)
         {
            fCharWindow = new CharacterWindow();
            fCharWindow.DataContext = this;
            fCharWindow.Show();
         }
         else
            CloseCharWindow();
      }

      private void CloseCharWindow()
      {
         fCharWindow.DataContext = null;
         fCharWindow.Close();
         fCharWindow = null;
      }

      #endregion


      #region ZoomValue

      /// <summary>
      /// Gets/sets the zoom value assigned to the character.
      /// </summary>
      /// <remarks>
      /// This allows us to pass this value over to other windows.
      /// </remarks>
      public double ZoomValue
      {
         get
         {
            return fZoomValue;
         }
         set
         {
            if (value != fZoomValue)
            {
               fZoomValue = value;
               OnPropertyChanged("ZoomValue");
            }
         }
      }

      #endregion

      #endregion

      #region functions

      /// <summary>
      /// Handles the Tick event of the fIdleTimer control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void fIdleTimer_Tick(object sender, EventArgs e)
      {
         if (SelectedCharacter != null && SelectedCharacter.IsLoaded == true)       //for some reason, after switching char, we still get a timer for the old char, luckily it is already unloaded, so don't try to start it.
            SelectedCharacter.ActivateIdleAnimation(fIdleRandomizer);
         fIdleTimer.Stop();                                                         //we stop the idle timer for as long as the idle anim is running. we will restart it once it is done.  
      }

      /// <summary>
      /// Handles the IdleAnimationFinished event of the fSelectedCharacter control.
      /// We simply restart a new one immidiatly.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void fSelectedCharacter_IdleAnimationFinished(object sender, EventArgs e)
      {
         if (sender == SelectedCharacter)                                                                //only respond when the caller  is the current char. It could be the previous char.
         {
            if (fPlayer.IsRecording == false)                                                                     //idles are stopped because speaking is started.
               ResetIdleTimer();
            else
               fPlayer.CanPlay();                                                                        //only let the player know it can start speaking, cause recording might not yet be fully ready.
         }
      }

      /// <summary>
      /// Resets the idle timer to a new time interval and starts it.
      /// </summary>
      internal void ResetIdleTimer()
      {
         fIdleTimer.Stop();
         IdleLevel iCurLevel = SelectedCharacter.IdleLevels[SelectedCharacter.CurrentIdleLevel];
         int iNrSeconds;
         if (iCurLevel.ElapsedTime == 0)                                                              //if there is no elapsed time yet, it's the first time that this idle level runs, so use the startdelay.
            iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinStartDelay, iCurLevel.MaxStartDelay);
         else if (iCurLevel.ElapsedTime > iCurLevel.MinDuration && SelectedCharacter.CurrentIdleLevel < SelectedCharacter.IdleLevels.Count -1)
         {
            iCurLevel.ElapsedTime = 0;                                                                //we reset the time, so we can use it again later on.
            SelectedCharacter.CurrentIdleLevel++;
            iCurLevel = SelectedCharacter.IdleLevels[SelectedCharacter.CurrentIdleLevel];
            iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinStartDelay, iCurLevel.MaxStartDelay);
         }
         else
            iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinInterval, iCurLevel.MaxInterval);
         iCurLevel.ElapsedTime += iNrSeconds;
         fIdleTimer.Interval = new TimeSpan(0, 0, iNrSeconds);
         fIdleTimer.IsEnabled = true;
      }

      private void CreateIdleTimer()
      {
         fIdleTimer = new DispatcherTimer(DispatcherPriority.Send, App.Current.Dispatcher);           //we are a regular object, don't ahve a dispatcher ref, so supply one.
         fIdleTimer.Tick += fIdleTimer_Tick;
         ResetIdleTimer();
      }

      /// <summary>
      /// Sets the Sensory interface that this object is a wrapper of.
      /// </summary>
      /// <param name="sin">The sin.</param>
      internal protected override void SetSin(Sin sin)
      {
         if (Sin != null)
         {
            FaceSin iSin = Sin as FaceSin;
            iSin.IntOut -= FaceSin_IntOut;
         }

         if (sin != null)
         {
            FaceSin iSin = sin as FaceSin;
            iSin.IntOut += FaceSin_IntOut;
         }
         base.SetSin(sin);
      }

      /// <summary>
      /// Called from the UI thread just after the project has been loaded. This allows communication channels to perform load tasks
      /// that can only be done from the UI.
      /// </summary>
      internal override void AfterLoaded()
      {
         base.AfterLoaded();
         TextSin iText = TextSin;
         if (iText != null)                                                            //need to set the event handler cause we simply read the id from teh xml file, didn't do anything more.
            iText.TextOut += Sin_TextOut;
         InitSelectedChar();
      }

      /// <summary>
      /// Updates the open documents.
      /// </summary>
      /// <remarks>
      /// When the channel is opened or closed, we also need to adjust the visibility of the head.
      /// </remarks>
      protected internal override void UpdateOpenDocuments()
      {
         base.UpdateOpenDocuments();
         if (BrainData.Current != null && BrainData.Current.DesignerData != null)                                  //when designerData is not set, not all the data is loaded yet.
         {
            if (IsVisible == true && ShowFloatingChar == true)
               SetShowFloatingChar(true);
            else if (IsVisible == false)
               SetShowFloatingChar(false);
         }
         else if (fCharWindow != null)                                                    //when there is a char window but no current project, the project is being cleared and we are getting worned about this, so unload any char window.
            CloseCharWindow();
      }

      /// <summary>
      /// Handles the SpeechRecognized event of the fRecognizer control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the event data.</param>
      public void fRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
      {
         InputText += e.Result.Text;
      }

      /// <summary>
      /// Handles the TextOut event of the Sin control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
      public void Sin_TextOut(object sender, OutputEventArgs<string> e)
      {
         App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<OutputEventArgs<string>>(HandleIncommingData), e);
      }

      void HandleIncommingData(OutputEventArgs<string> e)
      {
         string iXmlText = string.Format(SSMLSTRING, e.Value);                                                          //for convertion to text, we always use the official ssml format
         string iOut = SSMLParser.ConvertSSMLToText(iXmlText);
         fConversationLog.Add(BOT + iOut);
         if (SelectedCharacter != null && fSpeaker != null)                                                             //only try to stop the anims if there is a speaker, otherwise the anims can keep on running
         {
            string iTextToSpeek = string.Format(SelectedVoice.SendFormatString, e.Value);                                   //the output formatting can be declared, even if no ssml send.
            AudioNote iNote = fPlayer.StartRecording();
            if (string.IsNullOrEmpty(iTextToSpeek) == false && fSpeaker != null)
            {
               fSpeaker.SetOutputToWaveStream(iNote.Audio);                                                                //we need to render the audio to the mem stream, so we can play it later.
               if (SelectedVoice.SSMLEnabled == true)
                  fSpeaker.SpeakSsmlAsync(iTextToSpeek);
               else
                  fSpeaker.SpeakAsync(iTextToSpeek);
            }
            SelectedCharacter.StopAnimationsForSpeech();                                                                  //when we do something, stop all animations
         }
      }

      /// <summary>
      /// Called by the player/recorder when recording is complete and we need to release the memory.
      /// </summary>
      /// <param name="fRecordTo">The f record to.</param>
      internal void SpeechCompleted(AudioNote fRecordTo)
      {
         fSpeaker.SetOutputToNull();                                                                                             //remove the output, to release the mem.
      }

      /// <summary>
      /// Handles the IntOut event of the FaceSin control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.Int32&gt;"/> instance containing the event data.</param>
      void FaceSin_IntOut(object sender, OutputEventArgs<int> e)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Sends the text to sin.
      /// </summary>
      /// <param name="value">The value.</param>
      internal void SendTextToSin(string value)
      {
         if (string.IsNullOrEmpty(value) == false)
         {
            TextSin iSin = TextSin;
            if (iSin != null)
            {
               iSin.Process(value, ProcessorManager.Current.GetProcessor(), TextSinProcessMode.ClusterAndDict);
               fConversationLog.Add(USER + value);
            }
         }
      }

      /// <summary>
      /// Sends the text to sin. Use this when called from a thread other then UI thread.
      /// </summary>
      /// <param name="value">The value.</param>
      public void SendTextToSinAsync(string value)
      {
         if (string.IsNullOrEmpty(value) == false)
         {
            TextSin iSin = TextSin;
            if (iSin != null)
            {
               iSin.Process(value, ProcessorManager.Current.GetProcessor(), TextSinProcessMode.ClusterAndDict);
               App.Current.Dispatcher.BeginInvoke(new Action<string>(fConversationLog.Add), USER + value); //do async cause UI gets updated cause of this.
            }
         }
      }

      #endregion

      #region xml

      /// <summary>
      /// Converts an object into its XML representation.
      /// </summary>
      /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
      public override void WriteXml(System.Xml.XmlWriter writer)
      {
         base.WriteXml(writer);
         XmlStore.WriteElement<bool>(writer, "AudioInOn", AudioInOn);
         XmlStore.WriteElement<bool>(writer, "AudioOutOn", AudioOutOn);
         XmlStore.WriteElement<string>(writer, "InputText", InputText);
         if (SelectedCharacter != null)
         {
            int iIndex = Characters.IndexOf(SelectedCharacter);
            XmlStore.WriteElement<int>(writer, "SelectedCharacter", iIndex);
         }
         else
            XmlStore.WriteElement<int>(writer, "SelectedCharacter", -1);
         if (SelectedVoice != null)
            XmlStore.WriteElement<string>(writer, "SelectedVoice", SelectedVoice.Name);
         else
            XmlStore.WriteElement<string>(writer, "SelectedVoice", null);
         XmlStore.WriteElement<ulong>(writer, "TextSin", fTextSin);
         XmlStore.WriteElement<bool>(writer, "ShowFloatingChar", fShowFloatingChar);
         XmlStore.WriteElement<double>(writer, "Zoom", fZoomValue);
 
      }

      /// <summary>
      /// Reads the content of the XML.
      /// </summary>
      /// <param name="reader">The reader.</param>
      protected override void ReadXmlContent(System.Xml.XmlReader reader)
      {
         base.ReadXmlContent(reader);
         AudioInOn = XmlStore.ReadElement<bool>(reader, "AudioInOn");
         AudioOutOn = XmlStore.ReadElement<bool>(reader, "AudioOutOn");
         InputText = XmlStore.ReadElement<string>(reader, "InputText");
         int iIndex = XmlStore.ReadElement<int>(reader, "SelectedCharacter");
         if (iIndex > -1 && iIndex < Characters.Count)
            fSelectedCharacter = Characters[iIndex];                                            //set the field, not the prop cause this is called from a different thread. The prop loads the images, which are wpf objects and need to be created in the UI thread, so delay the loading of the image untill the sin gets assigned.
         string iVoice = XmlStore.ReadElement<string>(reader, "SelectedVoice");
         if (iVoice != null)
            SelectedVoice = (from i in AvailableVoices where i.Name == iVoice select i).FirstOrDefault();
         else
            SelectedVoice = null;
         int iVal = 0;
         XmlStore.TryReadElement<int>(reader, "VoiceAge", ref iVal);
         if (reader.Name == "ChatbotSex")                                                    //skip some old stuff.
            reader.Read();
         fTextSin = XmlStore.ReadElement<ulong>(reader, "TextSin");
         bool iBool = false;
         if (XmlStore.TryReadElement<bool>(reader, "ShowFloatingChar", ref iBool) == true)
            ShowFloatingChar = iBool;
         double idouble = 1.0;
         if (XmlStore.TryReadElement<double>(reader, "Zoom", ref idouble) == true)
            ZoomValue = idouble;
      }

      #endregion

      internal void ClearData()
      {
         fConversationLog.Clear();
      }

   }
}