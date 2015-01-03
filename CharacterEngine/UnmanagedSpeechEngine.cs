// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnmanagedSpeechEngine.cs" company="">
//   
// </copyright>
// <summary>
//   The unmanaged speech engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>The unmanaged speech engine.</summary>
    public class UnmanagedSpeechEngine : SpeechEngine
    {
        /// <summary>
        ///     The maximum nr of alternatives that we request for an input phrase.
        /// </summary>
        private const int ALTERNATEREQUESTCOUNT = 20;

        /// <summary>The f grammar.</summary>
        private SpeechLib.ISpeechRecoGrammar fGrammar;

        /// <summary>The f recognizer.</summary>
        private SpeechLib.SpSharedRecognizer fRecognizer;

        /// <summary>The f recorder.</summary>
        private SpeechLib.SpSharedRecoContext fRecorder;

        /// <summary>The f voice.</summary>
        private SpeechLib.SpVoice fVoice;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="UnmanagedSpeechEngine"/> class.</summary>
        /// <param name="channel">The channel.</param>
        public UnmanagedSpeechEngine(IChatBotChannel channel)
            : base(channel)
        {
        }

        #endregion

        /// <summary>
        ///     Gets the available voices.
        /// </summary>
        public new Voices AvailableVoices
        {
            get
            {
                return SpeechEngine.AvailableVoices;
            }
        }

        /// <summary>Gets or sets a value indicating whether audio out on.</summary>
        public override bool AudioOutOn
        {
            get
            {
                return fVoice != null;
            }

            set
            {
                if (AudioOutOn != value)
                {
                    if (value)
                    {
                        try
                        {
                            fVoice = new SpeechLib.SpVoice();
                            fVoice.Viseme += Viseme_Reached;
                            fVoice.Bookmark += BookmarkReached;
                            fVoice.EndStream += EndStream;
                            fVoice.StartStream += StartStream;
                            OnPropertyChanged("AvailableVoices"); // reload all the available voices.
                            if (fChannel.SelectedVoice != null)
                            {
                                SelectVoice(fChannel.SelectedVoice.Name);
                            }
                            else
                            {
                                SetSelectedVoice(fVoice.Voice.GetAttribute("Name"));
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogError("SpeechEngine.AudioOut", "Failed to change AudioOut setting: " + e);
                            UnloadVoice();
                        }
                    }
                    else
                    {
                        UnloadVoice();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets/sets wether oudio input is activated or not.
        /// </summary>
        public override bool AudioInOn
        {
            get
            {
                return fRecognizer != null;
            }

            set
            {
                if (AudioInOn != value)
                {
                    try
                    {
                        if (value)
                        {
                            fRecognizer = new SpeechLib.SpSharedRecognizer();
                            fRecorder = fRecognizer.CreateRecoContext() as SpeechLib.SpSharedRecoContext;
                            fRecorder.Recognition += fRecorder_Recognition;
                            fGrammar = fRecorder.CreateGrammar(0);
                            fGrammar.DictationLoad(string.Empty, SpeechLib.SpeechLoadOption.SLOStatic);
                            fGrammar.DictationSetState(SpeechLib.SpeechRuleState.SGDSActive);
                        }
                        else
                        {
                            // fRecorder.FalseRecognition -= new _ISpeechRecoContextEvents_FalseRecognitionEventHandler(fRecorder_FalseRecognition);
                            fGrammar.DictationUnload();
                            fRecorder.Recognition -= fRecorder_Recognition;
                            fGrammar = null;
                            fRecorder = null;
                            fRecognizer = null;
                        }
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError("SpeechEngine.AudioIn", "Failed to change Audio in setting: " + e);
                        try
                        {
                            if (fRecognizer != null)
                            {
                                // fRecorder.FalseRecognition -= new _ISpeechRecoContextEvents_FalseRecognitionEventHandler(fRecorder_FalseRecognition);
                                fRecorder.Recognition -= fRecorder_Recognition;
                                fGrammar = null;
                                fRecorder = null;
                                fRecognizer = null;
                            }
                        }
                        catch
                        {
                            // don't do anything, the catch is to make certain 
                        }
                    }

                    OnPropertyChanged("AudioInOn");
                }
            }
        }

        /// <summary>The load avalailable voices.</summary>
        /// <returns>The <see cref="Voices"/>.</returns>
        internal static Voices LoadAvalailableVoices()
        {
            try
            {
                var iVoice = new SpeechLib.SpVoice();
                var sot = iVoice.GetVoices(string.Empty, string.Empty);

                return Voices.RetrieveAvailableVoices(sot);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogWarning(
                    "SpeechEngine.AvailableVoices", 
                    "Failed to load the list of available voices: " + e);
                return new Voices();
            }
        }

        /// <summary>
        ///     Unloads the voice.
        /// </summary>
        private void UnloadVoice()
        {
            try
            {
                fVoice.Pause(); // to make certain that it doesn't hang.
                fVoice.StartStream -= StartStream;
                fVoice.Viseme -= Viseme_Reached;
                fVoice.Bookmark -= BookmarkReached;
                fVoice.EndStream -= EndStream;
                fVoice = null;
            }
            catch
            {
                fVoice = null;
            }
        }

        /// <summary>The f recorder_ false recognition.</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        /// <param name="Result">The result.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void fRecorder_FalseRecognition(
            int StreamNumber, 
            object StreamPosition, 
            SpeechLib.ISpeechRecoResult Result)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Selects the specified voice in the SAPI engine</summary>
        /// <param name="name">The name.</param>
        public override void SelectVoice(string name)
        {
            if (fVoice != null)
            {
                try
                {
                    var sot = fVoice.GetVoices(string.Empty, string.Empty);
                    var iFound = false;
                    for (var i = 0; i < sot.Count; i++)
                    {
                        var voice = sot.Item(i);
                        if (voice.GetAttribute("Name") == name)
                        {
                            fVoice.Voice = voice;
                            iFound = true;
                            break;
                        }
                    }

                    if (iFound == false)
                    {
                        LogService.Log.LogError("SpeechEngine.SelectVoice", "Failed to select voice: " + name);
                    }
                }
                catch (System.Exception exc)
                {
                    LogService.Log.LogError("SpeechEngine.SelectVoice", exc.ToString());
                }
            }
        }

        /// <summary>Speaks text as SSML async.</summary>
        /// <param name="text"></param>
        public override void SpeakSsmlAsync(string text)
        {
            if (fVoice != null)
            {
                fVoice.Speak(
                    text, 
                    SpeechLib.SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechLib.SpeechVoiceSpeakFlags.SVSFParseSsml);
            }
        }

        /// <summary>Speaks the text (non SSML) async.</summary>
        /// <param name="text">The text.</param>
        public override void SpeakAsync(string text)
        {
            if (fVoice != null)
            {
                fVoice.Speak(
                    text, 
                    SpeechLib.SpeechVoiceSpeakFlags.SVSFDefault | SpeechLib.SpeechVoiceSpeakFlags.SVSFlagsAsync);
            }
        }

        #region functions

        #region event handlers

        /// <summary>The f recorder_ recognition.</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        /// <param name="RecognitionType">The recognition type.</param>
        /// <param name="Result">The result.</param>
        private void fRecorder_Recognition(
            int StreamNumber, 
            object StreamPosition, 
            SpeechLib.SpeechRecognitionType RecognitionType, 
            SpeechLib.ISpeechRecoResult Result)
        {
            if (fChannel != null)
            {
                var iItems = new RecordResultGroup();
                iItems.Text = Result.PhraseInfo.GetText(0, -1, true);
                if (string.IsNullOrEmpty(fChannel.InputText))
                {
                    fChannel.InputText += iItems.Text;
                }
                else
                {
                    fChannel.InputText += " " + iItems.Text;
                }

                var iWords = new System.Collections.Generic.List<RecordedResult>();
                var iRes = GetRecordResult(Result.PhraseInfo);
                iItems.Items.Add(iRes);
                var iList = Result.Alternates(ALTERNATEREQUESTCOUNT, 0, -1);
                if (iList != null)
                {
                    foreach (SpeechLib.ISpeechPhraseAlternate i in iList)
                    {
                        iRes = GetRecordResult(i.PhraseInfo);
                        iItems.Items.Add(iRes);
                        iItems.Inputs.Add(i.PhraseInfo.GetText(0, -1, true));
                    }
                }

                fChannel.RecordedWords.Add(iItems);
            }
        }

        /// <summary>builds a RecordedResult object (single sentence result with all the parts of the sentence and their weights).</summary>
        /// <param name="info">The info.</param>
        /// <returns>The <see cref="RecordedResult"/>.</returns>
        private RecordedResult GetRecordResult(SpeechLib.ISpeechPhraseInfo info)
        {
            var iRes = new RecordedResult { Text = info.GetText(0, -1, true) };
            foreach (SpeechLib.ISpeechPhraseElement i in info.Elements)
            {
                var iItem = new RecordedResult.RecordedResultItem();
                iItem.Word = i.DisplayText;
                iItem.Weight = i.EngineConfidence;
                iRes.Words.Add(iItem);
            }

            return iRes;
        }

        /// <summary>com-event handler for start of stream</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        private void StartStream(int StreamNumber, object StreamPosition)
        {
            SpeakStarted();
        }

        /// <summary>com-event handler for end of stream</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        private void EndStream(int StreamNumber, object StreamPosition)
        {
            SpeakCompleted();
        }

        /// <summary>The bookmark reached.</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        /// <param name="Bookmark">The bookmark.</param>
        /// <param name="BookmarkId">The bookmark id.</param>
        private void BookmarkReached(int StreamNumber, object StreamPosition, string Bookmark, int BookmarkId)
        {
            fChannel.SelectBookmarkAsync(Bookmark);
        }

        /// <summary>com-event handler for when aViseme has been reached.</summary>
        /// <param name="StreamNumber">The stream number.</param>
        /// <param name="StreamPosition">The stream position.</param>
        /// <param name="Duration">The duration.</param>
        /// <param name="NextVisemeId">The next viseme id.</param>
        /// <param name="Feature">The feature.</param>
        /// <param name="CurrentVisemeId">The current viseme id.</param>
        private void Viseme_Reached(
            int StreamNumber, 
            object StreamPosition, 
            int Duration, 
            SpeechLib.SpeechVisemeType NextVisemeId, 
            SpeechLib.SpeechVisemeFeature Feature, 
            SpeechLib.SpeechVisemeType CurrentVisemeId)
        {
            var iFeature = default(System.Speech.Synthesis.SynthesizerEmphasis);
            if ((Feature & SpeechLib.SpeechVisemeFeature.SVF_Emphasis) == SpeechLib.SpeechVisemeFeature.SVF_Emphasis)
            {
                iFeature &= System.Speech.Synthesis.SynthesizerEmphasis.Emphasized;
            }

            if ((Feature & SpeechLib.SpeechVisemeFeature.SVF_Emphasis) == SpeechLib.SpeechVisemeFeature.SVF_Stressed)
            {
                iFeature &= System.Speech.Synthesis.SynthesizerEmphasis.Stressed;
            }

            VisemeReached((int)CurrentVisemeId, (int)NextVisemeId, Duration, iFeature);
        }

        #endregion

        #endregion
    }
}