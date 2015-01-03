// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynthSpeechEngine.cs" company="">
//   
// </copyright>
// <summary>
//   The synth speech engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>The synth speech engine.</summary>
    public class SynthSpeechEngine : SpeechEngine
    {
        /// <summary>The buffersize.</summary>
        private const int BUFFERSIZE = 16384;

        /// <summary>The wavefreqrange.</summary>
        private const int WAVEFREQRANGE = 44100;

        /// <summary>The f recognizer.</summary>
        private System.Speech.Recognition.SpeechRecognizer fRecognizer;

        /// <summary>The f speaker.</summary>
        private SpeechSynth.SpeechSynth fSpeaker;

        /// <summary>The f wave player.</summary>
        private WaveLib.WaveOutPlayer fWavePlayer;

        /// <summary>The f player queue.</summary>
        private readonly System.Collections.Generic.Queue<byte[]> fPlayerQueue =
            new System.Collections.Generic.Queue<byte[]>();

                                                                  // stores the queu of blocks, received from the synth, that need to be played.

        /// <summary>Initializes a new instance of the <see cref="SynthSpeechEngine"/> class. Initializes a new instance of the <see cref="SynthSpeechEngine"/>
        ///     class.</summary>
        /// <param name="channel">The channel.</param>
        public SynthSpeechEngine(IChatBotChannel channel)
            : base(channel)
        {
        }

        #region AvailableVoices

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

        #endregion

        /// <summary>
        ///     Gets/sets wether oudio output is activated or not.
        /// </summary>
        public override bool AudioOutOn
        {
            get
            {
                return fSpeaker != null;
            }

            set
            {
                if (value)
                {
                    // fSpeaker = new SpeechSynth.SpeechSynth(BUFFERSIZE);
                    fWavePlayer = new WaveLib.WaveOutPlayer(
                        -1, 
                        new WaveLib.WaveFormat(WAVEFREQRANGE, 16, 1), 
                        BUFFERSIZE * 2, 
                        3, 
                        FillWaveBuffer); // doing buffersize *2 cause 16 bit samples

                    // fSpeaker.Bookmark += new EventHandler(fSpeaker_Bookmark);
                    // fSpeaker.SpeakStarted += fSpeaker_SpeakStarted;
                    // fSpeaker.VisemeReached += fSpeaker_VisemeReached;
                    // fSpeaker.BookmarkReached += fSpeaker_BookmarkReached;
                    // fSpeaker.SpeakCompleted += fSpeaker_SpeakCompleted;
                    OnPropertyChanged("AvailableVoices"); // reload all the available voices.
                    if (fChannel.SelectedVoice != null)
                    {
                        fSpeaker.SelectVoice(fChannel.SelectedVoice.Name);
                    }
                    else
                    {
                        SetSelectedVoice(fSpeaker.CurrentVoice.Name);
                    }
                }
                else
                {
                    // fSpeaker.SpeakStarted -= fSpeaker_SpeakStarted;
                    // fSpeaker.VisemeReached -= fSpeaker_VisemeReached;
                    // fSpeaker.BookmarkReached -= fSpeaker_BookmarkReached;
                    // fSpeaker.SpeakCompleted -= fSpeaker_SpeakCompleted;
                    // fSpeaker.Dispose();
                    fWavePlayer.Dispose();
                    fWavePlayer = null;
                    fSpeaker = null;
                }
            }
        }

        #region AudioInOn

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
                            fRecognizer = new System.Speech.Recognition.SpeechRecognizer();
                            fRecognizer.Enabled = true;
                            fRecognizer.SpeechRecognized += fRecognizer_SpeechRecognized;

                            // fRecognizer.SpeechHypothesized += fRecognizer_SpeechHypothesized;
                        }
                        else
                        {
                            fRecognizer.Enabled = false;
                            fRecognizer.SpeechRecognized -= fRecognizer_SpeechRecognized;

                            // fRecognizer.SpeechHypothesized -= fRecognizer_SpeechHypothesized;
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
                                fRecognizer.Enabled = false;
                                fRecognizer.SpeechRecognized -= fRecognizer_SpeechRecognized;

                                // fRecognizer.SpeechHypothesized -= fRecognizer_SpeechHypothesized;
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

        #endregion

        /// <summary>The fill wave buffer.</summary>
        /// <param name="data">The data.</param>
        /// <param name="size">The size.</param>
        private void FillWaveBuffer(System.IntPtr data, int size)
        {
            byte[] b;
            lock (fPlayerQueue)
            {
                if (fPlayerQueue.Count > 0)
                {
                    b = fPlayerQueue.Dequeue();
                }
                else
                {
                    b = null;
                }
            }

            if (b == null)
            {
                b = new byte[size];
                for (var i = 0; i < b.Length; i++)
                {
                    b[i] = 0;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(b, 0, data, size);
        }

        /// <summary>Handles the SpeechRecognized event of the <see cref="fRecognizer"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the
        ///     event data.</param>
        private void fRecognizer_SpeechRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            if (fChannel != null)
            {
                var iItems = new RecordResultGroup();
                iItems.Text = e.Result.Text;
                if (string.IsNullOrEmpty(fChannel.InputText))
                {
                    fChannel.InputText += e.Result.Text;
                }
                else
                {
                    fChannel.InputText += " " + e.Result.Text;
                }

                var iWords = new System.Collections.Generic.List<RecordedResult>();
                var iRes = GetRecordResult(e.Result.Text, e.Result.Words);
                iItems.Items.Add(iRes);
                foreach (var i in e.Result.Alternates)
                {
                    iItems.Items.Add(GetRecordResult(i.Text, i.Words));
                }

                fChannel.RecordedWords.Add(iItems);
            }
        }

        /// <summary>The get record result.</summary>
        /// <param name="text">The text.</param>
        /// <param name="words">The words.</param>
        /// <returns>The <see cref="RecordedResult"/>.</returns>
        private RecordedResult GetRecordResult(
            string text, System.Collections.Generic.IEnumerable<System.Speech.Recognition.RecognizedWordUnit> words)
        {
            var iRes = new RecordedResult();
            iRes.Text = text;
            foreach (var i in words)
            {
                var iItem = new RecordedResult.RecordedResultItem();
                iItem.Word = i.Text;
                iItem.Weight = i.Confidence;
                iRes.Words.Add(iItem);
            }

            return iRes;
        }

        /// <summary>Selects the specified voice.</summary>
        /// <param name="name">The name.</param>
        public override void SelectVoice(string name)
        {
            if (fSpeaker != null)
            {
                fSpeaker.SelectVoice(name);
            }
        }

        /// <summary>The speak ssml async.</summary>
        /// <param name="text">The text.</param>
        public override void SpeakSsmlAsync(string text)
        {
            fSpeaker.Speak(text);
        }

        /// <summary>Speaks the <paramref name="text"/> (non SSML) async.</summary>
        /// <param name="text">The text.</param>
        public override void SpeakAsync(string text)
        {
            fSpeaker.Speak(text);
        }

        /// <summary>Loads the avalailable voices.</summary>
        /// <returns>The <see cref="Voices"/>.</returns>
        internal static Voices LoadAvalailableVoices()
        {
            try
            {
                var iSpeaker = new SpeechSynth.SpeechSynth(string.Empty);
                return Voices.RetrieveAvailableVoices(iSpeaker);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogWarning(
                    "SpeechEngine.AvailableVoices", 
                    "Failed to load the list of available voices: " + e);
                return new Voices();
            }
        }
    }
}