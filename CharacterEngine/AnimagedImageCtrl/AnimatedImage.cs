// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimatedImage.cs" company="">
//   
// </copyright>
// <summary>
//   An object that is able to play an animation through a series of images (bitmaps).
//   Each bitmap is defined together with the amount of time it should remain visible.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     An object that is able to play an animation through a series of images (bitmaps).
    ///     Each bitmap is defined together with the amount of time it should remain visible.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Frames")]
    public class AnimatedImage : System.Windows.Controls.Image
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimatedImage" /> class.
        /// </summary>
        public AnimatedImage()
        {
            UseSoftStop = false;
            Loaded += AnimatedImage_Loaded;
        }

        /// <summary>
        ///     Gets the randomizer that can be used by the engine.
        /// </summary>
        internal System.Random Randomizer
        {
            get
            {
                if (fRandom == null)
                {
                    fRandom = new System.Random();
                }

                return fRandom;
            }
        }

        #region Frames

        /// <summary>
        ///     Gets the list of frames that should be displayed for this  animated image.
        /// </summary>
        public System.Collections.Generic.List<AnimatedImageFrame> Frames
        {
            get
            {
                return fFrames;
            }
        }

        #endregion

        #region ShowBackground

        /// <summary>
        ///     Gets/sets if the background, still, head is shown or not for this animation.
        /// </summary>
        public bool ShowBackground { get; set; }

        #endregion

        #region UseSoftStop

        /// <summary>
        ///     Gets/sets wether to use a hard stop (default) or a soft stop of the animation, in which it is fast forwarded to the
        ///     closest end (are we playing reverse or not).
        /// </summary>
        public bool UseSoftStop { get; set; }

        #endregion

        #region AllowSpeech

        /// <summary>
        ///     Gets/sets if the animation allows speech to occur or wether it should be terminated when speech occurs.
        /// </summary>
        public bool AllowSpeech { get; set; }

        #endregion

        #region BackgroundSuppress

        /// <summary>
        ///     Gets or sets the list of indexes into the background grid's children list to indicate
        ///     which sections should be suppressed when the animation is activated.
        /// </summary>
        /// <value>
        ///     The background suppress.
        /// </value>
        public System.Collections.Generic.List<int> BackgroundSuppress { get; set; }

        #endregion

        #region MaxLowerDeviation

        /// <summary>
        ///     Gets or sets the max lower value that the speed can go down. This has to be a positive nr.
        /// </summary>
        /// <value>
        ///     The max lower deviation.
        /// </value>
        public int MaxLowerDeviation { get; set; }

        #endregion

        #region MaxUpperDeviation

        /// <summary>
        ///     Gets or sets the maximum value that the speed can go up. This has to be a positive nr.
        /// </summary>
        /// <value>
        ///     The max upper deviation.
        /// </value>
        public int MaxUpperDeviation { get; set; }

        #endregion

        #region LoopStyle

        /// <summary>
        ///     Gets or sets the loop style that should be applied to the animation.
        /// </summary>
        /// <value>
        ///     The loop style.
        /// </value>
        public Characters.AnimationLoopStyle LoopStyle
        {
            get
            {
                return fLoopStyle;
            }

            set
            {
                fLoopStyle = value;
                switch (value)
                {
                    case Characters.AnimationLoopStyle.None:
                        fEngine = new SimpleAnimationEngine();
                        break;
                    case Characters.AnimationLoopStyle.Jojo:
                        fEngine = new JojoAnimationEngine { VariableSpeed = false };
                        break;
                    case Characters.AnimationLoopStyle.VariableJojo:
                        fEngine = new JojoAnimationEngine { VariableSpeed = true };
                        break;
                    case Characters.AnimationLoopStyle.FrontToBack:
                        fEngine = new ForwardLoopAnimationEngine();
                        break;
                    case Characters.AnimationLoopStyle.VarTimer:
                        fEngine = new VarTimerAnimationEngine();
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the timer used by the animation. This allows the engine to set the timer frequency.
        /// </summary>
        internal System.Windows.Threading.DispatcherTimer Timer { get; private set; }

        /// <summary>The animated image_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AnimatedImage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            StartPlay();
        }

        /// <summary>
        ///     Starts the timer for playing the animation.
        /// </summary>
        public void StartPlay()
        {
            if (fEngine != null && fFrames.Count > 0 && Timer == null)
            {
                // it could be that the user switched page, in which case we get a new load event, but we don't want to restart an already running timer, simply let that run.
                fCurAnimPos = 0;
                Timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Send);
                fEngine.Init(this);

                Timer.Tick += fTimer_Tick;
                Timer.Start();
            }
        }

        #region events

        /// <summary>
        ///     Occurs when the animation has finished running and
        /// </summary>
        public event System.EventHandler AnimationFinished;

        #endregion

        #region fields

        /// <summary>The f frames.</summary>
        private readonly System.Collections.Generic.List<AnimatedImageFrame> fFrames =
            new System.Collections.Generic.List<AnimatedImageFrame>();

        /// <summary>The f cur anim pos.</summary>
        private int fCurAnimPos;

        // bool fIsplayingReverse = false;                                                           //inidcates if we currently are playing in reverse or not. 
        // bool fIsStopping = false;                                                                 //indicates if we are in the stopping phase of the animation or not.
        /// <summary>The f time distort.</summary>
        private double fTimeDistort = 1.0;

                       // stores the quotient that needs to be applied on the time value during the stop animation period, so that the animation is sped up to 1 second max

        /// <summary>The f random.</summary>
        private System.Random fRandom;

                              // a randomizer, for if we are variable Jojo or need to vary the speed of the animation.

        /// <summary>The f loop style.</summary>
        private Characters.AnimationLoopStyle fLoopStyle;

        /// <summary>The f engine.</summary>
        private AnimationEngineBase fEngine;

        #endregion

        #region Delay

        /// <summary>
        ///     Gets or sets the min time delay that should be used between 2 animation runs when the loopstyle is vartimer.
        /// </summary>
        /// <value>
        ///     The min start delay.
        /// </value>
        public int MinStartDelay { get; set; }

        /// <summary>
        ///     Gets or sets the max delay that can be used when the loopstyle is VarTimer.
        /// </summary>
        /// <value>
        ///     The max start delay.
        /// </value>
        public int MaxStartDelay { get; set; }

        #endregion

        #region functions

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (Timer != null)
            {
                if (UseSoftStop == false)
                {
                    HardStop();
                }
                else
                {
                    CalculateTotalTimeLeft();
                }
            }
        }

        /// <summary>
        ///     Imidiatly stops the timer from playing.
        /// </summary>
        public void HardStop()
        {
            if (Timer != null)
            {
                // happens sometimes, not certain why, possibly out of sync events while reloading char.
                Timer.Stop();
                Timer.Tick -= fTimer_Tick;
                Timer = null;
            }

            fTimeDistort = 1.0;

            // fIsplayingReverse = false;
            if (AnimationFinished != null)
            {
                AnimationFinished(this, System.EventArgs.Empty);
            }

            fCurAnimPos = 0;
        }

        /// <summary>
        ///     Calculates the total time left to play in the animition to get to the start point. This is by the shortest
        ///     route. It is used during the 'stop' part of the animation, so we can fast forward the animation
        ///     Also stops and starts the timer again so that the animation can be sped up.
        /// </summary>
        private void CalculateTotalTimeLeft()
        {
            Timer.Stop(); // stop before calculating so taht this value can't change.
            long iTick = 0;
            for (var i = fCurAnimPos - 2; i >= 0; i--)
            {
                // we do -2 cause current fCurAnimPos points to the next image to play, so -1 would give the current, but we want the prev.
                iTick += fFrames[i].Duration;
            }

            if (iTick > 500)
            {
                // if the current duration of the animation that's left takes longer then 0.8 second, speed it up.
                fTimeDistort = 500.0 / iTick;
            }
            else
            {
                fTimeDistort = 1;
            }

            fCurAnimPos -= 2;
            if (fCurAnimPos >= 0)
            {
                var iDuration = fFrames[fCurAnimPos].Duration * fTimeDistort; // calculate in doubles, most accuratie.
                Timer.Interval = new System.TimeSpan(0, 0, 0, 0, (int)iDuration);

                    // reset the timer, adjusted for the speed up
                fCurAnimPos--;
                Timer.IsEnabled = true;
            }
            else
            {
                HardStop();
            }
        }

        /// <summary>The f timer_ tick.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fTimer_Tick(object sender, System.EventArgs e)
        {
            if (fEngine != null)
            {
                fEngine.DoTimerTick(this);
            }
        }

        #endregion
    }
}