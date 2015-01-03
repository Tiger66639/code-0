// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatBotChannel.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   A commChannel that wraps the <see cref="IntSin" /> and which can link to
//   another <see cref="JaStDev.HAB.Designer.ChatBotChannel.TextSin" />
//   </para>
//   <para>
//   for performing text input and output. The main sin being wrapped though,
//   is the 'face' <see langword="interface" /> and not the text one (which is
//   handled by text channels).
//   </para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     <para>
    ///         A commChannel that wraps the <see cref="IntSin" /> and which can link to
    ///         another <see cref="JaStDev.HAB.Designer.ChatBotChannel.TextSin" />
    ///     </para>
    ///     <para>
    ///         for performing text input and output. The main sin being wrapped though,
    ///         is the 'face' <see langword="interface" /> and not the text one (which is
    ///         handled by text channels).
    ///     </para>
    /// </summary>
    public class ChatBotChannel : CommChannel, Test.ITesteable, CharacterEngine.IChatBotChannel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChatBotChannel" /> class.
        /// </summary>
        public ChatBotChannel()
        {
            ChangeSpeechEngine(Properties.Settings.Default.SpeechEngineMode);
            fRecordedWords.CollectionChanged += fRecordedWords_CollectionChanged;
        }

        /// <summary>Handles the CollectionChanged event of the<see cref="fRecordedWords"/> control. When this list changes, we need
        ///     to let th ui know that there are items available to select from.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The<see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/>
        ///     instance containing the event data.</param>
        private void fRecordedWords_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasRecordedWords");
            if (RecordedWords.Count > 0)
            {
                // we start or reset a timer when speech input was encountered, so that it gets automatically processed.
                ResetSpeechStartTimer();
            }
        }

        #region const

        /// <summary>The bot.</summary>
        public const string BOT = "bot: ";

        /// <summary>The user.</summary>
        public const string USER = "You: ";

        /// <summary>The chararctersfilter.</summary>
        private const string CHARARCTERSFILTER = "*.char";

        /// <summary>The ccscharfilter.</summary>
        private const string CCSCHARFILTER = "*" + CharacterEngine.Character.CCSFILEEXT;

        /// <summary>The ssmlstring.</summary>
        private const string SSMLSTRING =
            "<?xml version='1.0'?><speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemalocation='http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='en-US'>{0}</speak>";

        #endregion

        #region fields 

        /// <summary>The f conversation log.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<string> fConversationLog =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        /// <summary>The f recorded words.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<CharacterEngine.RecordResultGroup> fRecordedWords =
                new System.Collections.ObjectModel.ObservableCollection<CharacterEngine.RecordResultGroup>();

        /// <summary>The f current inputs.</summary>
        private System.Collections.Generic.List<string> fCurrentInputs;

                                                        // when we are processing speech input, we temporarely store the combined inputs, so that we don't need to recalculate them all the time.

        /// <summary>The f input text.</summary>
        private string fInputText; // the default value. 

        /// <summary>The f selected voice.</summary>
        private CharacterEngine.Voice fSelectedVoice;

        /// <summary>The f text sin.</summary>
        private ulong fTextSin; // stores a ref to the text sin as id, for loading from file

        /// <summary>The f selected character.</summary>
        private CharacterEngine.Character fSelectedCharacter;

        /// <summary>The f characters.</summary>
        private static System.Collections.Generic.List<CharacterEngine.Character> fCharacters;

        /// <summary>The f idle timer.</summary>
        private System.Windows.Threading.DispatcherTimer fIdleTimer;

                                                         // used to keep track off 'idle' times, so that we can start small animations to show 'liveliness'.

        /// <summary>The f speech input.</summary>
        private System.Windows.Threading.DispatcherTimer fSpeechInput;

                                                         // when voice input has happened, we use a timeout to automatically send the text to the engine.

        /// <summary>The f speech engine.</summary>
        private CharacterEngine.SpeechEngine fSpeechEngine;

        /// <summary>The f idle randomizer.</summary>
        private readonly System.Random fIdleRandomizer = new System.Random();

                                       // a randomizer for the idle times and animations.

        /// <summary>The f is speaking.</summary>
        private bool fIsSpeaking;

        /// <summary>The f show floating char.</summary>
        private bool fShowFloatingChar;

        /// <summary>The f show advanced input.</summary>
        private bool fShowAdvancedInput;

        /// <summary>The f audio in on.</summary>
        private bool fAudioInOn;

                     // for reading from xml, we need to buffer these values cause reading is done in another thread, but the unmanaged version (com) can't handle the event registration this way (unregistration causes a hang), so we buffer these values and assign after the app was loaded, from the UI thread.

        /// <summary>The f audio out on.</summary>
        private bool fAudioOutOn;

        /// <summary>The f char window.</summary>
        private CharacterWindow fCharWindow;

        /// <summary>The f zoom value.</summary>
        private double fZoomValue = 1.0;

        /// <summary>The f text to speek.</summary>
        private string fTextToSpeek; // stores the text that needs to be spoken when the idle animation is done.

        /// <summary>The f available voices.</summary>
        private System.Windows.Data.CollectionViewSource fAvailableVoices;

        #endregion

        #region prop

        #region ConversationLog

        /// <summary>
        ///     Gets the conversation log in plain text
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> ConversationLog
        {
            get
            {
                return fConversationLog;
            }
        }

        #endregion

        #region SpeechEngine

        /// <summary>
        ///     Gets the engine responsible for speech/audio and animation/video .
        /// </summary>
        public CharacterEngine.SpeechEngine SpeechEngine
        {
            get
            {
                return fSpeechEngine;
            }

            private set
            {
                if (value != fSpeechEngine)
                {
                    if (fSpeechEngine != null)
                    {
                        value.AudioInOn = fSpeechEngine.AudioInOn; // copy over the settings to the new engine.
                        value.AudioOutOn = fSpeechEngine.AudioOutOn;
                        fSpeechEngine.Free();
                    }

                    fSpeechEngine = value;
                    OnPropertyChanged("SpeechEngine");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets a filtered list for the available Voices.
        /// </summary>
        public object AvailableVoices
        {
            get
            {
                if (fAvailableVoices == null)
                {
                    fAvailableVoices = new System.Windows.Data.CollectionViewSource();
                    fAvailableVoices.Source = CharacterEngine.SpeechEngine.AvailableVoices;
                    fAvailableVoices.Filter += iRes_Filter;
                }

                return fAvailableVoices.View;
            }
        }

        /// <summary>
        ///     Resets the available voices to <see langword="null" /> and raises the
        ///     changes event, so that they can be reloaded. (when
        /// </summary>
        internal void ResetAvailableVoices()
        {
            fAvailableVoices = null;
            OnPropertyChanged("AvailableVoices");
        }

        /// <summary>Handles the Filter event of the <see cref="AvailableVoices"/>
        ///     CollectionViewFilter</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Data.FilterEventArgs"/> instance containing the event data.</param>
        private void iRes_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        {
            e.Accepted = ((CharacterEngine.Voice)e.Item).IsEnabled;
        }

        #region InputText

        /// <summary>
        ///     Gets/sets the text currently typed in, but not yet sent to the
        ///     network. This allows us to remember the input text when the user has
        ///     changed tabs. Default value = 'Type here'.
        /// </summary>
        public string InputText
        {
            get
            {
                return fInputText;
            }

            set
            {
                if (value != fInputText)
                {
                    fInputText = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        RecordedWords.Clear();
                    }

                    OnPropertyChanged("InputText");
                }
            }
        }

        #endregion

        #region RecordedWords

        /// <summary>
        ///     Gets or sets the text that was recorded by SAPI, split up into it's
        ///     recorded words with weight.
        /// </summary>
        /// <value>
        ///     The recorded words.
        /// </value>
        public System.Collections.Generic.IList<CharacterEngine.RecordResultGroup> RecordedWords
        {
            get
            {
                return fRecordedWords;
            }
        }

        #endregion

        #region HasRecordedWords

        /// <summary>
        ///     Gets if the list of recroded words has any items or not.
        /// </summary>
        public bool HasRecordedWords
        {
            get
            {
                return fRecordedWords.Count > 0;
            }
        }

        #endregion

        #region ShowAdvancedInput

        /// <summary>
        ///     Gets/sets the value that indicates if we need to show the advanced
        ///     input or not.
        /// </summary>
        public bool ShowAdvancedInput
        {
            get
            {
                return fShowAdvancedInput;
            }

            set
            {
                fShowAdvancedInput = value;
                OnPropertyChanged("ShowAdvancedInput");
            }
        }

        #endregion

        #region SelectedVoice

        /// <summary>
        ///     Gets/sets the name of the currently selected voice that is used as
        ///     output.
        /// </summary>
        public CharacterEngine.Voice SelectedVoice
        {
            get
            {
                return fSelectedVoice;
            }

            set
            {
                if (fSpeechEngine.AudioOutOn && value != fSelectedVoice)
                {
                    try
                    {
                        if (value != null)
                        {
                            fSpeechEngine.SelectVoice(value.Name);
                        }
                        else
                        {
                            fSpeechEngine.SelectVoice(null);
                        }

                        SetSelectedVoice(value);
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError(
                            "Chatbot", 
                            string.Format("Failed to load voice: {0}, with message: {1}.", value, e.Message));
                        if (fSpeechEngine != null)
                        {
                            fSpeechEngine.AudioOutOn = false; // can't have a speeker, there  is no valid voice.
                        }
                    }
                }
            }
        }

        /// <summary>stores the currently selected voice and raises the event, but doesn't
        ///     change the engine.</summary>
        /// <param name="value">The value.</param>
        public void SetSelectedVoice(CharacterEngine.Voice value)
        {
            if (value.PreferedCharacter != null)
            {
                // if there is a prefered char to assign, check if the user hasn't already changed the char to seomthing.
                if (SelectedVoice != null)
                {
                    if (SelectedVoice.PreferedCharacter != null)
                    {
                        if (SelectedCharacter != null && SelectedVoice.PreferedCharacter == SelectedCharacter.Name)
                        {
                            SelectedCharacter =
                                (from i in Characters where i.Name == value.PreferedCharacter select i).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    SelectedCharacter =
                        (from i in Characters where i.Name == value.PreferedCharacter select i).FirstOrDefault();
                }
            }

            fSelectedVoice = value;
            OnPropertyChanged("SelectedVoice");
        }

        #endregion

        #region TextSin

        /// <summary>
        ///     Gets/sets the sin to use for handling text.
        /// </summary>
        public TextSin TextSin
        {
            get
            {
                if (fTextSin != Neuron.EmptyId)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(fTextSin, out iFound))
                    {
                        return iFound as TextSin;
                    }
                }

                return null;
            }

            internal set
            {
                var iText = TextSin;
                if (iText != null)
                {
                    iText.TextOut -= Sin_TextOut;
                }

                if (value != null)
                {
                    fTextSin = value.ID;
                    value.TextOut += Sin_TextOut;
                }
                else
                {
                    fTextSin = Neuron.EmptyId;
                }

                OnPropertyChanged("TextSin");
            }
        }

        #endregion

        #region Textsin (ITesteable Members)

        /// <summary>
        ///     Gets or sets the textsin that should be used to communicate with
        ///     during testing.
        /// </summary>
        /// <value>
        ///     The textsin.
        /// </value>
        public TextSin Textsin
        {
            get
            {
                return TextSin;
            }
        }

        #endregion

        #region SelectedCharacter

        /// <summary>
        ///     Gets/sets the currently selected character that needs to be displayed.
        /// </summary>
        public CharacterEngine.Character SelectedCharacter
        {
            get
            {
                if (IsVisible)
                {
                    return fSelectedCharacter;
                }

                return null;

                    // when the channel not visible -> no char. This is done so that the ObservingCanvas unregisters it's list of visible Items.If we don't do this, older canvases keep a ref to the visibleList and will register new visible items before the actual list, causing problems.
            }

            set
            {
                if (value != fSelectedCharacter && IsSpeaking == false)
                {
                    // can't change char while speaking.
                    if (fSelectedCharacter != null)
                    {
                        fSelectedCharacter.IsLoaded = false;
                        fSelectedCharacter.IdleAnimationFinished -= fSelectedCharacter_IdleAnimationFinished;
                    }

                    fSelectedCharacter = value;
                    if (fSelectedCharacter != null && value.FileName != null)
                    {
                        // if there is no file name, it's the self generated 'None' character, so set the value to 'null'.
                        InitSelectedChar(true);
                    }
                    else if (fIdleTimer != null)
                    {
                        fIdleTimer.Stop(); // if there is no no char, simply stop the idle timer.
                    }

                    OnPropertyChanged("SelectedCharacter");
                }
            }
        }

        /// <summary>Prepares everything for the selected character, when it just got
        ///     selected.</summary>
        /// <param name="showError">if set to <c>true</c> any errors are shown with a dialog box,
        ///     otherwise they are only logged. This is to prevent error boxes while
        ///     loading the application (default template)</param>
        private void InitSelectedChar(bool showError)
        {
            if (fSelectedCharacter != null && fSelectedCharacter.FileName != null)
            {
                // can only load if there is a file to load (the 'none' character has no file).
                try
                {
                    fSelectedCharacter.IsLoaded = true;
                    fSelectedCharacter.IdleAnimationFinished += fSelectedCharacter_IdleAnimationFinished;
                    if (IsVisible)
                    {
                        CreateIdleTimer();

                            // each time there is a new char, we reset the idle timer to a new time out, so we have variations.
                    }
                }
                catch (System.Exception e)
                {
                    var iErr =
                        string.Format(
                            "There was a problem while loading the character, with message: {0}. Please check the character definitions file for any errors", 
                            e.Message);
                    if (showError)
                    {
                        System.Windows.MessageBox.Show(
                            iErr, 
                            "Load failure", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Error);
                    }

                    LogService.Log.LogError("Chatbot.loadCharacter", iErr);
                    try
                    {
                        fSelectedCharacter.IsLoaded = false;
                        SelectedCharacter = null; // make certain that the char is no longer loaded.
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region Characters

        /// <summary>
        ///     Gets all the available characters.
        /// </summary>
        public static System.Collections.Generic.List<CharacterEngine.Character> Characters
        {
            get
            {
                if (fCharacters == null)
                {
                    fCharacters = new System.Collections.Generic.List<CharacterEngine.Character>();
                    if (System.IO.Directory.Exists(Properties.Settings.Default.CharactersPath))
                    {
                        GetCharsFromDir(Properties.Settings.Default.CharactersPath);
                        var iSubs = System.IO.Directory.GetDirectories(Properties.Settings.Default.CharactersPath);
                        foreach (var i in iSubs)
                        {
                            GetCharsFromDir(i);
                        }
                    }

                    var iEmpty = new CharacterEngine.Character(null);
                    iEmpty.Name = "None";
                    fCharacters.Add(iEmpty);
                }

                return fCharacters;
            }

            internal set
            {
                fCharacters = null; // allow a set so we can reset the value when the prop has changed.
            }
        }

        /// <summary>The get chars from dir.</summary>
        /// <param name="path">The path.</param>
        private static void GetCharsFromDir(string path)
        {
            var iFiles = System.IO.Directory.GetFiles(path, CHARARCTERSFILTER);
            if (iFiles != null)
            {
                foreach (var iFile in iFiles)
                {
                    var iNew = new CharacterEngine.Character(iFile);
                    fCharacters.Add(iNew);
                }
            }

            iFiles = System.IO.Directory.GetFiles(path, CCSCHARFILTER);
            if (iFiles != null)
            {
                foreach (var iFile in iFiles)
                {
                    var iNew = new CharacterEngine.Character(iFile);
                    fCharacters.Add(iNew);
                }
            }
        }

        #endregion

        #region IsSpeaking

        /// <summary>
        ///     Gets/sets if the chatbot is currently speaking or not. This is used to
        ///     determine if new idle events need to be triggered.
        /// </summary>
        public bool IsSpeaking
        {
            get
            {
                return fIsSpeaking;
            }

            set
            {
                fIsSpeaking = value;
                OnPropertyChanged("IsSpeaking");
                OnPropertyChanged("IsNotSpeaking");
            }
        }

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether this instance is not speaking (the
        ///         inverse of the
        ///         <see cref="JaStDev.HAB.Designer.ChatBotChannel.IsSpeaking" />
        ///     </para>
        ///     <para>
        ///         for the UI, so it can easely bind to it for disabling stuff while
        ///         speaking.
        ///     </para>
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is not speaking; otherwise, <c>false</c>
        ///     .
        /// </value>
        public bool IsNotSpeaking
        {
            get
            {
                return !fIsSpeaking;
            }
        }

        #endregion

        #region ShowFloatingChar

        /// <summary>
        ///     Gets/sets wether the character is floating or not
        /// </summary>
        /// <remarks>
        ///     the UI should use
        ///     <see cref="JaStDev.HAB.Designer.ChatBotChannel.ShowFloatingCharUI" />
        ///     cause this will <see langword="switch" /> on/off when the chatbot
        ///     channel is made visible/invisible, while this value remains the same.
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
                {
                    SetShowFloatingChar(value);
                }
            }
        }

        /// <summary>The set show floating char.</summary>
        /// <param name="value">The value.</param>
        private void SetShowFloatingChar(bool value)
        {
            fShowFloatingChar = value;
            OnPropertyChanged("ShowFloatingChar");
        }

        #endregion

        #region ShowFloatingCharUI

        /// <summary>
        ///     Gets/sets wether the character is currently floating or not and also
        ///     visible.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The chatbot channel view could be unloaded, cause the user switched to
        ///         another tab, but if the <see langword="char" /> is floating, it should
        ///         remain visible, so this needs to be controlled seperatly.
        ///     </para>
        ///     <para>
        ///         Note: this proerty is controlled from within the view: when it is
        ///         ready to show/hide the character, it uses this property to actually
        ///         visualise/ hide it.
        ///     </para>
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
                {
                    SetShowFloatingCharUI(value);
                }
            }
        }

        /// <summary>The set show floating char ui.</summary>
        /// <param name="value">The value.</param>
        private void SetShowFloatingCharUI(bool value)
        {
            if (value)
            {
                fCharWindow = new CharacterWindow();
                fCharWindow.DataContext = this;
                fCharWindow.Show();
            }
            else
            {
                CloseCharWindow();
            }
        }

        /// <summary>The close char window.</summary>
        private void CloseCharWindow()
        {
            fCharWindow.DataContext = null;
            fCharWindow.Close();
            fCharWindow = null;
        }

        #endregion

        #region ZoomValue

        /// <summary>
        ///     Gets/sets the zoom value assigned to the character.
        /// </summary>
        /// <remarks>
        ///     This allows us to pass this value over to other windows.
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
        ///     Resets or starts the speech timer so that speech input automatically
        ///     gets send to the engine.
        /// </summary>
        private void ResetSpeechStartTimer()
        {
            if (fSpeechInput == null)
            {
                fSpeechInput = new System.Windows.Threading.DispatcherTimer();
                fSpeechInput.Tick += fSpeechInput_Tick;
                fSpeechInput.Interval = new System.TimeSpan(0, 0, 0, 1, 500); // we wait for 1,5 seconds.
            }
            else
            {
                fSpeechInput.Stop();
            }

            fSpeechInput.Start();
        }

        /// <summary>Handles the Tick event of the <see cref="fSpeechInput"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fSpeechInput_Tick(object sender, System.EventArgs e)
        {
            fSpeechInput.Stop();
            SendRecordedWords();
            InputText = string.Empty; // reset the input so that new text can take it's place.
            fSpeechInput = null;
        }

        /// <summary>
        ///     Rebuilds the <see cref="InputText" /> because an alternate was
        ///     selected.
        /// </summary>
        internal void RebuildInputForAlternates()
        {
            var iRes = new System.Text.StringBuilder();
            foreach (var i in RecordedWords)
            {
                if (iRes.Length > 0)
                {
                    iRes.Append(" ");
                }

                iRes.Append(i.Text);
            }

            fInputText = iRes.ToString();

                // don't set the prop cause this clears the list of recordedWords, which we don't want.
            OnPropertyChanged("InputText");
        }

        /// <summary>Selects the <paramref name="viseme"/> async.</summary>
        /// <param name="viseme">The viseme.</param>
        public void SelectVisemeAsync(int viseme)
        {
            if (SelectedCharacter != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<int>(SelectedCharacter.SelectViseme), 
                    viseme);
            }
        }

        /// <summary>Selects the <paramref name="bookmark"/> async.</summary>
        /// <param name="bookmark">The bookmark.</param>
        public void SelectBookmarkAsync(string bookmark)
        {
            if (SelectedCharacter != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Send, 
                    new System.Action<string>(SelectedCharacter.ActivateAnimation), 
                    bookmark);
            }
        }

        /// <summary>Handles the Tick event of the <see cref="fIdleTimer"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fIdleTimer_Tick(object sender, System.EventArgs e)
        {
            if (SelectedCharacter != null && SelectedCharacter.IsLoaded && IsSpeaking == false)
            {
                // for some reason, after switching char, we still get a timer for the old char, luckily it is already unloaded, so don't try to start it.
                SelectedCharacter.ActivateIdleAnimation(fIdleRandomizer);
            }

            fIdleTimer.Stop();

                // we stop the idle timer for as long as the idle anim is running. we will restart it once it is done.  
        }

        /// <summary>Handles the IdleAnimationFinished event of the<see cref="fSelectedCharacter"/> control. We simply restart a new one
        ///     immidiatly.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fSelectedCharacter_IdleAnimationFinished(object sender, System.EventArgs e)
        {
            if (sender == SelectedCharacter)
            {
                // only respond when the caller  is the current char. It could be the previous char.
                if (IsSpeaking == false)
                {
                    // idles are stopped because speaking is started.
                    ResetIdleTimer();

                    // App.Current.Dispatcher.BeginInvoke(new Action<Random>(SelectedCharacter.ActivateIdleAnimation), DispatcherPriority.Background, fIdleRandomizer);    //do async, so that a previous image has time to unload.
                }
                else if (string.IsNullOrEmpty(fTextToSpeek) == false)
                {
                    try
                    {
                        if (fSpeechEngine.AudioOutOn)
                        {
                            if (SelectedVoice.SSMLEnabled)
                            {
                                fSpeechEngine.SpeakSsmlAsync(fTextToSpeek);
                            }
                            else
                            {
                                fSpeechEngine.SpeakAsync(fTextToSpeek);
                            }
                        }

                        fTextToSpeek = null;
                    }
                    catch (System.Exception ex)
                    {
                        LogService.Log.LogError("SpeechEngine.Speak", "Failed to speak, error: " + ex);
                        IsSpeaking = false;

                            // if something went wrong during the activation of the speech, make certain that we don't get stuck (IsSpeaking gets turned off when speech is done, but since this isn't started, it never gets turned off.
                    }
                }
                else
                {
                    IsSpeaking = false;

                        // something strange going on, but IsSpeaking was turned on, make certain we enable it again.
                }
            }
        }

        /// <summary>
        ///     Resets the idle timer to a new time interval and starts it.
        /// </summary>
        public void ResetIdleTimer()
        {
            if (fIdleTimer != null)
            {
                fIdleTimer.Stop();
                if (SelectedCharacter.IdleLevels != null)
                {
                    var iCurLevel = SelectedCharacter.IdleLevels[SelectedCharacter.CurrentIdleLevel];
                    int iNrSeconds;
                    if (iCurLevel.ElapsedTime == 0)
                    {
                        // if there is no elapsed time yet, it's the first time that this idle level runs, so use the startdelay.
                        iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinStartDelay, iCurLevel.MaxStartDelay);
                    }
                    else if (iCurLevel.ElapsedTime > iCurLevel.MinDuration
                             && SelectedCharacter.CurrentIdleLevel < SelectedCharacter.IdleLevels.Count - 1)
                    {
                        iCurLevel.ElapsedTime = 0; // we reset the time, so we can use it again later on.
                        SelectedCharacter.CurrentIdleLevel++;
                        iCurLevel = SelectedCharacter.IdleLevels[SelectedCharacter.CurrentIdleLevel];
                        iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinStartDelay, iCurLevel.MaxStartDelay);
                    }
                    else
                    {
                        iNrSeconds = fIdleRandomizer.Next(iCurLevel.MinInterval, iCurLevel.MaxInterval);
                    }

                    iCurLevel.ElapsedTime += iNrSeconds;
                    fIdleTimer.Interval = new System.TimeSpan(0, 0, iNrSeconds);
                    fIdleTimer.IsEnabled = true;
                }
            }
        }

        /// <summary>The create idle timer.</summary>
        private void CreateIdleTimer()
        {
            if (fIdleTimer == null)
            {
                fIdleTimer =
                    new System.Windows.Threading.DispatcherTimer(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        System.Windows.Application.Current.Dispatcher);

                    // we are a regular object, don't ahve a dispatcher ref, so supply one.
                fIdleTimer.Tick += fIdleTimer_Tick;
            }

            ResetIdleTimer();
        }

        /// <summary>
        ///     DispatcherTimer object can cause mem leaks when not properly cleaned.
        /// </summary>
        private void ClearIdleTimer()
        {
            if (fIdleTimer != null)
            {
                fIdleTimer.Stop();
                fIdleTimer.Tick -= fIdleTimer_Tick;
                fIdleTimer = null;
            }
        }

        /// <summary>Sets the Sensory <see langword="interface"/> that this object is a
        ///     wrapper of.</summary>
        /// <param name="sin">The sin.</param>
        protected internal override void SetSin(Sin sin)
        {
            if (Sin != null)
            {
                var iSin = Sin as IntSin;
                iSin.IntsOut -= IntSin_IntsOut;
                iSin.IntOut -= iSin_IntOut;
            }

            if (sin != null)
            {
                var iSin = sin as IntSin;
                iSin.IntsOut += IntSin_IntsOut;
                iSin.IntOut += iSin_IntOut;
            }

            base.SetSin(sin);
        }

        /// <summary>
        ///     Called from the UI thread just after the project has been loaded. This
        ///     allows communication channels to perform load tasks that can only be
        ///     done from the UI.
        /// </summary>
        internal override void AfterLoaded()
        {
            base.AfterLoaded();
            if (SpeechEngine != null)
            {
                SpeechEngine.AudioInOn = fAudioInOn;
                SpeechEngine.AudioOutOn = fAudioOutOn;
            }

            var iText = TextSin;
            if (iText != null)
            {
                // need to set the event handler cause we simply read the id from teh xml file, didn't do anything more.
                iText.TextOut += Sin_TextOut;
            }

            InitSelectedChar(false);
        }

        /// <summary>
        ///     Updates the open documents.
        /// </summary>
        /// <remarks>
        ///     When the channel is opened or closed, we also need to adjust the
        ///     visibility of the head.
        /// </remarks>
        protected internal override void UpdateOpenDocuments()
        {
            base.UpdateOpenDocuments();
            if (BrainData.Current != null && BrainData.Current.DesignerData != null)
            {
                // when designerData is not set, not all the data is loaded yet.
                if (IsVisible)
                {
                    if (ShowFloatingChar)
                    {
                        SetShowFloatingChar(true);
                    }

                    if (SelectedCharacter != null)
                    {
                        // can only create and reset the timer if there is a char.
                        CreateIdleTimer();
                    }
                }
                else
                {
                    SetShowFloatingChar(false);
                    fSpeechEngine.Release();
                    ClearIdleTimer();
                }
            }
            else if (fCharWindow != null)
            {
                // when there is a char window but no current project, the project is being cleared and we are getting worned about this, so unload any char window and stop timers.
                CloseCharWindow();
                fSpeechEngine.Release();
                ClearIdleTimer();
            }
            else
            {
                ClearIdleTimer();
                fSpeechEngine.Free();
            }

            OnPropertyChanged("SelectedCharacter");

                // always updated selected char so that observableCanvas can update ref to list.
        }

        /// <summary>The sin_ text out.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void Sin_TextOut(object sender, OutputEventArgs<string> e)
        {
            if (IsVisible)
            {
                // don't try to render anything if we are not visible. this allows for processes to run as fast as possible when the channel is not visible.
                var iXmlText = string.Format(SSMLSTRING, e.Value);

                    // for convertion to text, we always use the official ssml format
                var iOut = HAB.Characters.SSMLParser.ConvertSSMLToText(iXmlText);
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<string, string>(HandleIncommingData), 
                    e.Value, 
                    iOut);
            }
        }

        /// <summary>The handle incomming data.</summary>
        /// <param name="original">The original.</param>
        /// <param name="forLog">The for log.</param>
        private void HandleIncommingData(string original, string forLog)
        {
            fConversationLog.Add(BOT + forLog);
            if (SelectedCharacter != null && fSpeechEngine.AudioOutOn)
            {
                // only try to stop the anims if there is a speaker, otherwise the anims can keep on running
                fTextToSpeek = string.Format(SelectedVoice.SendFormatString, original);

                    // the output formatting can be declared, even if no ssml send.
                IsSpeaking = true;
                SelectedCharacter.StopAnimationsForSpeech(); // when we do something, stop all animations
            }
        }

        /// <summary>The int sin_ ints out.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void IntSin_IntsOut(object sender, OutputEventArgs<System.Collections.Generic.List<int>> e)
        {
            int iIndex;
            if (e.Value.Count == 1)
            {
                // if we get 1 item back, it is to let us know which input that the engine eventually used, so log this value.
                iIndex = e.Value[0];
            }
            else
            {
                var iSelector = new InputSelector();
                var iToSelect = new System.Collections.Generic.List<InputSelectionItem>();
                e.Value.Sort(); // we sort the list so that they are in the order that the STT found them.
                foreach (var i in e.Value)
                {
                    // need to filter the full list to only the items that the engine asks to resolve.
                    var iNew = new InputSelectionItem { Value = fCurrentInputs[i], Index = i };
                    iToSelect.Add(iNew);
                }

                iIndex = iSelector.Request(iToSelect);
                if (iIndex > -1 && iIndex <= e.Value.Count)
                {
                    var iSin = Sin as IntSin;
                    iSin.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.IntSin, e.Data[iIndex]);
                }
            }

            if (iIndex > -1)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new System.Action<string>(ConversationLog.Add), 
                    USER + fCurrentInputs[iIndex]);

                    // let the user see what was eventually used. async cause this is called from a network thread, while this causes a UI update which can only happen from the uithread.
            }

            fCurrentInputs = null;
        }

        /// <summary>The i sin_ int out.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iSin_IntOut(object sender, OutputEventArgs<int> e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new System.Action<string>(ConversationLog.Add), 
                USER + fCurrentInputs[e.Value]);

                // let the user see what was eventually used. async cause this is called from a network thread, while this causes a UI update which can only happen from the uithread.
            fCurrentInputs = null;
        }

        /// <summary>Sends the text to sin.</summary>
        /// <param name="value">The value.</param>
        /// <param name="info">The info.</param>
        internal void SendTextToSin(string value, System.Collections.Generic.List<Neuron> info = null)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                var iSin = TextSin;
                if (iSin != null)
                {
                    iSin.Process(value, ProcessorFactory.GetProcessor(), TextSinProcessMode.ClusterAndDict, info);
                    fConversationLog.Add(USER + value);
                }
            }
        }

        /// <summary>Sends the text to sin. Use this when called from a thread other then
        ///     UI thread.</summary>
        /// <param name="value">The value.</param>
        public void SendTextToSinAsync(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                var iSin = TextSin;
                if (iSin != null)
                {
                    iSin.Process(value, ProcessorFactory.GetProcessor(), TextSinProcessMode.ClusterAndDict);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(fConversationLog.Add), 
                        USER + value); // do async cause UI gets updated cause of this.
                }
            }
        }

        /// <summary>
        ///     sends the recorded audio as text to the sin. This will first combine
        ///     all the recorded fragments into 1 big set of possible inputs and send
        ///     all of them to the sin. This will let us know which one will
        ///     eventually be selected.
        /// </summary>
        internal void SendRecordedWords()
        {
            if (fRecordedWords.Count > 0)
            {
                var iRes = fRecordedWords[0].Inputs;
                for (var i = 1; i < fRecordedWords.Count; i++)
                {
                    var iNew = new System.Collections.Generic.List<string>();
                    foreach (var iFirst in iRes)
                    {
                        foreach (var iSecond in fRecordedWords[i].Inputs)
                        {
                            iNew.Add(iFirst + " " + iSecond);
                        }
                    }

                    iRes = iNew;
                }

                fCurrentInputs = iRes; // so we can show the selected item or a selection box.
                var iSin = TextSin;
                if (iSin != null)
                {
                    iSin.Process(iRes, ProcessorFactory.GetProcessor(), TextSinProcessMode.ClusterAndDict);
                }
            }
        }

        /// <summary>
        ///     Clears all the conversationlog data.
        /// </summary>
        internal void ClearData()
        {
            fConversationLog.Clear();
        }

        /// <summary>The change speech engine.</summary>
        /// <param name="value">The value.</param>
        internal void ChangeSpeechEngine(CharacterEngine.SpeechEngineMode value)
        {
            switch (Properties.Settings.Default.SpeechEngineMode)
            {
                case CharacterEngine.SpeechEngineMode.NonManaged:
                    SpeechEngine = new CharacterEngine.UnmanagedSpeechEngine(this);
                    break;
                case CharacterEngine.SpeechEngineMode.Managed:
                    SpeechEngine = new CharacterEngine.ManagedSpeechEngine(this);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "AudioInOn", fSpeechEngine.AudioInOn);
            XmlStore.WriteElement(writer, "AudioOutOn", fSpeechEngine.AudioOutOn);
            XmlStore.WriteElement(writer, "InputText", InputText);
            if (SelectedCharacter != null)
            {
                var iIndex = Characters.IndexOf(SelectedCharacter);
                XmlStore.WriteElement(writer, "SelectedCharacter", iIndex);
            }
            else
            {
                XmlStore.WriteElement(writer, "SelectedCharacter", -1);
            }

            if (SelectedVoice != null)
            {
                XmlStore.WriteElement(writer, "SelectedVoice", SelectedVoice.Name);
            }
            else
            {
                XmlStore.WriteElement<string>(writer, "SelectedVoice", null);
            }

            XmlStore.WriteElement(writer, "TextSin", fTextSin);
            XmlStore.WriteElement(writer, "ShowFloatingChar", fShowFloatingChar);
            XmlStore.WriteElement(writer, "Zoom", fZoomValue);
            XmlStore.WriteElement(writer, "ShowAdvancedInput", ShowAdvancedInput);
        }

        /// <summary>Reads the content of the XML.</summary>
        /// <param name="reader">The reader.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            fAudioInOn = XmlStore.ReadElement<bool>(reader, "AudioInOn");
            fAudioOutOn = XmlStore.ReadElement<bool>(reader, "AudioOutOn");
            InputText = XmlStore.ReadElement<string>(reader, "InputText");
            var iIndex = XmlStore.ReadElement<int>(reader, "SelectedCharacter");
            if (iIndex > -1 && iIndex < Characters.Count)
            {
                fSelectedCharacter = Characters[iIndex];

                    // set the field, not the prop cause this is called from a different thread. The prop loads the images, which are wpf objects and need to be created in the UI thread, so delay the loading of the image untill the sin gets assigned.
            }
            else if (Characters.Count > 0)
            {
                fSelectedCharacter = Characters[0];
            }

            var iVoice = XmlStore.ReadElement<string>(reader, "SelectedVoice");
            if (iVoice != null)
            {
                SelectedVoice =
                    (from i in CharacterEngine.SpeechEngine.AvailableVoices where i.Name == iVoice select i)
                        .FirstOrDefault();
            }
            else if (CharacterEngine.SpeechEngine.AvailableVoices.Count > 0)
            {
                SelectedVoice = CharacterEngine.SpeechEngine.AvailableVoices[0];
            }
            else
            {
                SelectedVoice = null;
            }

            var iVal = 0;
            XmlStore.TryReadElement(reader, "VoiceAge", ref iVal);
            if (reader.Name == "ChatbotSex")
            {
                // skip some old stuff.
                reader.Read();
            }

            fTextSin = XmlStore.ReadElement<ulong>(reader, "TextSin");
            var iBool = false;
            if (XmlStore.TryReadElement(reader, "ShowFloatingChar", ref iBool))
            {
                ShowFloatingChar = iBool;
            }

            var idouble = 1.0;
            if (XmlStore.TryReadElement(reader, "Zoom", ref idouble))
            {
                ZoomValue = idouble;
            }

            if (XmlStore.TryReadElement(reader, "ShowAdvancedInput", ref iBool))
            {
                ShowAdvancedInput = iBool;
            }
        }

        #endregion
    }
}