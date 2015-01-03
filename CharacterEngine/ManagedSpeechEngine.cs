// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManagedSpeechEngine.cs" company="">
//   
// </copyright>
// <summary>
//   A speech engine that relies on the managed SAPI lib to provide speech.
//   Warning: has problems with loading voices on some systems.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>
    ///     A speech engine that relies on the managed SAPI lib to provide speech.
    ///     Warning: has problems with loading voices on some systems.
    /// </summary>
    public class ManagedSpeechEngine : SpeechEngine
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ManagedSpeechEngine"/> class.</summary>
        /// <param name="channel">The channel.</param>
        public ManagedSpeechEngine(IChatBotChannel channel)
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

        #region AudioOutOn

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
                if (AudioOutOn != value)
                {
                    try
                    {
                        if (value)
                        {
                            fSpeaker = new System.Speech.Synthesis.SpeechSynthesizer();
                            fSpeaker.SpeakStarted += fSpeaker_SpeakStarted;
                            fSpeaker.VisemeReached += fSpeaker_VisemeReached;
                            fSpeaker.BookmarkReached += fSpeaker_BookmarkReached;
                            fSpeaker.SpeakCompleted += fSpeaker_SpeakCompleted;
                            OnPropertyChanged("AvailableVoices"); // reload all the available voices.
                            if (fChannel.SelectedVoice != null)
                            {
                                fSpeaker.SelectVoice(fChannel.SelectedVoice.Name);
                            }
                            else
                            {
                                SetSelectedVoice(fSpeaker.Voice.Name);
                            }
                        }
                        else
                        {
                            fSpeaker.SpeakStarted -= fSpeaker_SpeakStarted;
                            fSpeaker.VisemeReached -= fSpeaker_VisemeReached;
                            fSpeaker.BookmarkReached -= fSpeaker_BookmarkReached;
                            fSpeaker.SpeakCompleted -= fSpeaker_SpeakCompleted;
                            fSpeaker.Dispose();
                            fSpeaker = null;
                        }
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError("SpeechEngine.AudioOut", "Failed to change AudioOut setting: " + e);
                        try
                        {
                            if (fSpeaker != null)
                            {
                                fSpeaker.Dispose();
                                fSpeaker = null;
                            }
                        }
                        catch
                        {
                        }
                    }

                    OnPropertyChanged("AudioOutOn");
                }
            }
        }

        #endregion

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

        /// <summary>Loads the avalailable voices.</summary>
        /// <returns>The <see cref="Voices"/>.</returns>
        internal static Voices LoadAvalailableVoices()
        {
            try
            {
                using (var iSpeaker = new System.Speech.Synthesis.SpeechSynthesizer()) return Voices.RetrieveAvailableVoices(iSpeaker);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogWarning(
                    "SpeechEngine.AvailableVoices", 
                    "Failed to load the list of available voices: " + e);
                return new Voices();
            }
        }

        #region fields

        /// <summary>The f speaker.</summary>
        private System.Speech.Synthesis.SpeechSynthesizer fSpeaker;

        /// <summary>The f recognizer.</summary>
        private System.Speech.Recognition.SpeechRecognizer fRecognizer;

        #endregion

        #region event handlers

        /// <summary>Handles the SpeechRecognized event of the <see cref="fRecognizer"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the
        ///     event data.</param>
        private void fRecognizer_SpeechRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            if (fChannel != null)
            {
                var iItems = new RecordResultGroup(); // contains all the data returned by the engine
                iItems.Text = e.Result.Text;
                var iWords = new System.Collections.Generic.List<RecordedResult>();
                var iRes = GetRecordResult(e.Result.Text, e.Result.Words);
                iItems.Items.Add(iRes);
                foreach (var i in e.Result.Alternates)
                {
                    iItems.Items.Add(GetRecordResult(i.Text, i.Words));
                    iItems.Inputs.Add(i.Text);
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

        /// <summary>Handles the SpeakStarted event of the <see cref="fSpeaker"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Synthesis.SpeakStartedEventArgs"/> instance containing the event
        ///     data.</param>
        private void fSpeaker_SpeakStarted(object sender, System.Speech.Synthesis.SpeakStartedEventArgs e)
        {
            SpeakStarted();
        }

        /// <summary>Handles the VisemeReached event of the <see cref="fSpeaker"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Synthesis.VisemeReachedEventArgs"/> instance containing the
        ///     event data.</param>
        private void fSpeaker_VisemeReached(object sender, System.Speech.Synthesis.VisemeReachedEventArgs e)
        {
            VisemeReached(e);
        }

        /// <summary>Handles the SpeakCompleted event of the <see cref="fSpeaker"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Synthesis.SpeakCompletedEventArgs"/> instance containing the
        ///     event data.</param>
        private void fSpeaker_SpeakCompleted(object sender, System.Speech.Synthesis.SpeakCompletedEventArgs e)
        {
            SpeakCompleted();
        }

        /// <summary>Handles the BookmarkReached event of the <see cref="fSpeaker"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Synthesis.BookmarkReachedEventArgs"/> instance containing the
        ///     event data.</param>
        private void fSpeaker_BookmarkReached(object sender, System.Speech.Synthesis.BookmarkReachedEventArgs e)
        {
            fChannel.SelectBookmarkAsync(e.Bookmark);
        }

        #endregion

        #region overrides

        /// <summary>Selects the specified voice.</summary>
        /// <param name="name">The name.</param>
        public override void SelectVoice(string name)
        {
            if (fSpeaker != null)
            {
                fSpeaker.SelectVoice(name);
            }
        }

        /// <summary>Speaks the <paramref name="text"/> (non SSML) async.</summary>
        /// <param name="text">The text.</param>
        public override void SpeakAsync(string text)
        {
            if (fSpeaker != null)
            {
                fSpeaker.SpeakAsync(text);
            }
        }

        /// <summary>Speaks <paramref name="text"/> as SSML async.</summary>
        /// <param name="text"></param>
        public override void SpeakSsmlAsync(string text)
        {
            if (fSpeaker != null)
            {
                fSpeaker.SpeakSsmlAsync(text);
            }
        }

        #endregion
    }
}