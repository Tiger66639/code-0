// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpeechEngine.cs" company="">
//   
// </copyright>
// <summary>
//   determins the mode of the speechEngine
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    using System.Linq;

    /// <summary>
    ///     determins the mode of the speechEngine
    /// </summary>
    public enum SpeechEngineMode
    {
        /// <summary>The non managed.</summary>
        NonManaged, 

        /// <summary>The managed.</summary>
        Managed, 

        /// <summary>The synth.</summary>
        Synth
    }

    /// <summary>
    ///     base class for speech engines. Inheriters provide the actual speech
    ///     implementation. At this level, the anaimation is handled.
    /// </summary>
    public abstract class SpeechEngine : Data.ObservableObject
    {
        /// <summary>The f engine mode.</summary>
        private static SpeechEngineMode fEngineMode = SpeechEngineMode.NonManaged;

                                        // the default mode is non managed cause this has a better quality, but doesn't work in silverlight.

        /// <summary>The f timer count.</summary>
        private static volatile int fTimerCount;

                                    // so we can keep track of the nr of tracks that have started the tickgenerator,allowing us to determine when to close the generator

        /// <summary>The f available voices.</summary>
        private static Voices fAvailableVoices;

        /// <summary>The f channel.</summary>
        protected IChatBotChannel fChannel;

        /// <summary>The f is speaking.</summary>
        private bool fIsSpeaking;

                     // stores if we are speaking, in case there were callbacks still on the timer after speaking stopped.

        // bool fStart = true;
        /// <summary>The f need advance.</summary>
        private bool fNeedAdvance;

                     // in case there are still buffered items that need to be spit out cause of new events.

        /// <summary>The f prev duration.</summary>
        private long fPrevDuration; // so we know the amount of adjustement we need to do.

        /// <summary>The f prev time.</summary>
        private System.TimeSpan fPrevTime; // so we can calculate the time difference between 2 events.

        /// <summary>The f queue lock.</summary>
        private readonly object fQueueLock = new object();

        /// <summary>The f timer count lock.</summary>
        private readonly object fTimerCountLock = new object();

                                // the timercount is accessed accross multiple threads, so we need to secure it's access.

        /// <summary>The f visemes to play.</summary>
        private readonly System.Collections.Generic.Queue<VisemeNote> fVisemesToPlay =
            new System.Collections.Generic.Queue<VisemeNote>();

                                                                      // viseme events are received out of sync, so we need to use a timer to sync them again.

        // Stopwatch fTester = new Stopwatch();

        /// <summary>Initializes a new instance of the <see cref="SpeechEngine"/> class.</summary>
        /// <param name="channel">The channel.</param>
        public SpeechEngine(IChatBotChannel channel)
        {
            fChannel = channel;
        }

        #region AudioOutOn

        /// <summary>
        ///     Gets/sets wether oudio output is activated or not.
        /// </summary>
        public abstract bool AudioOutOn { get; set; }

        #endregion

        #region AudioInOn

        /// <summary>
        ///     Gets/sets wether oudio input is activated or not.
        /// </summary>
        public abstract bool AudioInOn { get; set; }

        #endregion

        #region EngineMode

        /// <summary>
        ///     Gets/sets the mode of the speechEngine
        /// </summary>
        public static SpeechEngineMode EngineMode
        {
            get
            {
                return fEngineMode;
            }

            set
            {
                fEngineMode = value;
            }
        }

        #endregion

        /// <summary>
        ///     HAS TO BE CALLED IF YOU WANT TO CHANGE OR UNLOAD A SPEECHENGINE will
        ///     make certain that resources are cleaned up.
        /// </summary>
        /// <remarks>
        ///     Internally makes certain that audio is turned off.
        /// </remarks>
        public void Free()
        {
            AudioInOn = false;
            AudioOutOn = false;
        }

        /// <summary>Selects the specified voice.</summary>
        /// <param name="name">The name.</param>
        public abstract void SelectVoice(string name);

        /// <summary>stores the currently selected voice and raises the event, but doesn't
        ///     change the engine. This after converting the string to a voice object.</summary>
        /// <param name="value">The value.</param>
        protected void SetSelectedVoice(string value)
        {
            if (fChannel != null)
            {
                var iFound = (from i in AvailableVoices where i.Name == value select i).FirstOrDefault();
                if (iFound != null)
                {
                    fChannel.SetSelectedVoice(iFound);
                }
                else
                {
                    throw new System.InvalidOperationException("Unexpected voice: " + value);
                }
            }
        }

        /// <summary>
        ///     Need to start/create the viseme player.
        /// </summary>
        protected void SpeakStarted()
        {
            fIsSpeaking = true;

            // fStart = true;
            if (fChannel.SelectedCharacter != null)
            {
                fPrevTime = new System.TimeSpan();
                fPrevDuration = 0;

                // fTester.Start();
                lock (fTimerCountLock)
                {
                    fTimerCount++;
                    if (EngineMode != SpeechEngineMode.NonManaged)
                    {
                        // non-managed engine is accurate enough to do direct sends.
                        TickGenerator.Default.Tick += Default_Tick;
                        if (fTimerCount == 1)
                        {
                            TickGenerator.Default.Start();
                        }
                    }
                }
            }
        }

        /// <summary>Handles the <see cref="VisemeReached"/> event of the fSpeaker control.</summary>
        /// <param name="e">The <see cref="System.Speech.Synthesis.VisemeReachedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void VisemeReached(System.Speech.Synthesis.VisemeReachedEventArgs e)
        {
            // Debug.Print("V:{0}     AudioPos:{1}      Duration:{2}       Next:{3}", e.Viseme, e.AudioPosition.Ticks, e.Duration.Ticks, e.NextViseme);
            if (fChannel.SelectedCharacter != null)
            {
                var iNew = new VisemeNote();
                if (fPrevTime == e.AudioPosition)
                {
                    // this is the adjustement: if the time was the same as prev, do a short delay equal to the duration of the previous note.
                    iNew.Ticks = fPrevDuration;
                }
                else
                {
                    iNew.Ticks = 0;
                }

                iNew.Viseme = e.Viseme;
                iNew.NextViseme = e.NextViseme;
                iNew.Emphasis = e.Emphasis;
                iNew.Duration = e.Duration.Ticks;
                fPrevDuration = e.Duration.Ticks;
                fPrevTime = e.AudioPosition;
                lock (fQueueLock)
                {
                    if (iNew.Ticks == 0)
                    {
                        fNeedAdvance = fVisemesToPlay.Count > 0;
                    }

                    fVisemesToPlay.Enqueue(iNew);
                }
            }
        }

        /// <summary>Plays a <paramref name="viseme"/> now, bypassing the tickgenerator.
        ///     This is for the unmanaged system, which has accurate enough timing.</summary>
        /// <param name="viseme">The viseme.</param>
        /// <param name="nextViseme">The next viseme.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="feature">The feature.</param>
        protected void VisemeReached(
            int viseme, 
            int nextViseme, 
            long duration, 
            System.Speech.Synthesis.SynthesizerEmphasis feature)
        {
            // Debug.Print("V:{0}     AudioPos:{1}      Duration:{2}       Next:{3}", e.Viseme, e.AudioPosition.Ticks, e.Duration.Ticks, e.NextViseme);
            if (fChannel.SelectedCharacter != null)
            {
                var iNew = new VisemeNote();
                iNew.Ticks = 0; // this is a direct form of activating visemes, for the unmanaged SAPI.
                iNew.Viseme = viseme;
                iNew.NextViseme = nextViseme;
                iNew.Emphasis = feature;
                iNew.Duration = duration;
                PlayEvent(iNew);
            }
        }

        /// <summary>
        ///     Handles the <see cref="SpeakCompleted" /> event of the fSpeaker
        ///     control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="System.Speech.Synthesis.SpeakCompletedEventArgs" /> instance containing the
        ///     event data.
        /// </param>
        protected void SpeakCompleted()
        {
            if (Release())
            {
                // it could be that speach has already been stopped because the window was closed.
                fChannel.IsSpeaking = false;
                if (fChannel.SelectedCharacter != null)
                {
                    fChannel.SelectVisemeAsync(-1);
                    fChannel.ResetIdleTimer();

                        // after an output is done, we need to reset the idle timer so a new animation can start if needed.
                }

                lock (fQueueLock) fVisemesToPlay.Clear(); // clear any items that were still queued for some reason.

                // fTester.Stop();
            }
        }

        /// <summary>
        ///     Releases all the resources used for this viseme player without
        ///     updating the channel specifically. Thi is used when the channel closes
        ///     and when speach is done.
        /// </summary>
        /// <returns>
        ///     True if the player was actually speaking, otherwise
        ///     <see langword="false" />
        /// </returns>
        public bool Release()
        {
            lock (fTimerCountLock)
            {
                if (fIsSpeaking)
                {
                    fTimerCount--;
                    if (EngineMode != SpeechEngineMode.NonManaged)
                    {
                        // non-managed engine is accurate enough to do direct sends.
                        TickGenerator.Default.Tick -= Default_Tick;
                        if (fTimerCount == 0)
                        {
                            TickGenerator.Default.Stop();
                        }
                    }

                    fIsSpeaking = false;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The default_ tick.</summary>
        /// <param name="ticks">The ticks.</param>
        private void Default_Tick(long ticks)
        {
            if (fChannel.SelectedCharacter != null)
            {
                VisemeNote iEvent = null;
                lock (fQueueLock)
                {
                    // keep locks as small as possible.
                    if (fVisemesToPlay.Count > 0)
                    {
                        iEvent = fVisemesToPlay.Peek();
                    }
                }

                while (iEvent != null && (iEvent.Ticks <= ticks || fNeedAdvance))
                {
                    if (fNeedAdvance == false)
                    {
                        ticks -= iEvent.Ticks;
                    }

                    PlayEvent(iEvent);

                    // Debug.Print(fTester.Elapsed.Ticks.ToString());
                    // fTester.Restart();
                    lock (fQueueLock)
                    {
                        if (fVisemesToPlay.Count > 0)
                        {
                            // the clear could already have happened.
                            fVisemesToPlay.Dequeue();
                            if (fVisemesToPlay.Count > 0)
                            {
                                iEvent = fVisemesToPlay.Peek();
                            }
                        }
                        else
                        {
                            iEvent = null;
                        }
                    }
                }

                if (fNeedAdvance)
                {
                    fNeedAdvance = false;
                }
                else if (iEvent != null)
                {
                    iEvent.Ticks -= ticks;

                        // if not all the ticks have been consumed, they will eat up on the time of the next event, otherwise we get stuck.
                }
            }
        }

        /// <summary>Plays the event viseme note.</summary>
        /// <param name="toPlay">The to Play.</param>
        private void PlayEvent(VisemeNote toPlay)
        {
            switch (toPlay.Emphasis)
            {
                case System.Speech.Synthesis.SynthesizerEmphasis.Emphasized:
                    fChannel.SelectedCharacter.EmphasisStyle = CharacterEmphasis.Emphasized;
                    break;
                case System.Speech.Synthesis.SynthesizerEmphasis.Stressed:
                    fChannel.SelectedCharacter.EmphasisStyle = CharacterEmphasis.Stressed;
                    break;
                default:
                    fChannel.SelectedCharacter.EmphasisStyle = CharacterEmphasis.Normal;
                    break;
            }

            fChannel.SelectVisemeAsync(toPlay.Viseme);
        }

        /// <summary>Speaks <paramref name="text"/> as SSML async.</summary>
        /// <param name="text">The text.</param>
        public abstract void SpeakSsmlAsync(string text);

        /// <summary>Speaks the <paramref name="text"/> (non SSML) async.</summary>
        /// <param name="text">The text.</param>
        public abstract void SpeakAsync(string text);

        #region AvailableVoices

        /// <summary>
        ///     Gets all the available voices for the current speaker..
        /// </summary>
        public static Voices AvailableVoices
        {
            get
            {
                if (fAvailableVoices == null)
                {
                    switch (EngineMode)
                    {
                        case SpeechEngineMode.NonManaged:
                            fAvailableVoices = UnmanagedSpeechEngine.LoadAvalailableVoices();
                            break;
                        case SpeechEngineMode.Managed:
                            fAvailableVoices = ManagedSpeechEngine.LoadAvalailableVoices();
                            break;
                        case SpeechEngineMode.Synth:
                            fAvailableVoices = SynthSpeechEngine.LoadAvalailableVoices();
                            break;
                        default:
                            throw new System.InvalidOperationException("unknown engine mode");
                    }
                }

                return fAvailableVoices;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [available voices loaded].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [available voices loaded]; otherwise, <c>false</c> .
        /// </value>
        public static bool AvailableVoicesLoaded
        {
            get
            {
                return fAvailableVoices != null;
            }
        }

        #endregion
    }
}