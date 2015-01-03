// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TickGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   delegate used for the tickgenerator when a new tick has past.
//   a tick = 100nano sec
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{

    #region TickGenerator event types

    /// <summary>
    ///     delegate used for the tickgenerator when a new tick has past.
    ///     a tick = 100nano sec
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TickGeneratorHandler(long ticks);

    #endregion

    /// <summary>
    ///     Provides high accuracy timing for the VisemePlayers. This allows all viseme players to share a single high accuracy
    ///     multi media
    ///     timer, allowing us to have an unlimited amount of chatbot channels.
    /// </summary>
    /// <remarks>
    ///     Start and stop the timer as needed. This is incremental.
    /// </remarks>
    public class TickGenerator
    {
        #region ctor

        /// <summary>Prevents a default instance of the <see cref="TickGenerator"/> class from being created. 
        ///     Prevents a default instance of the <see cref="Tick"/> class from being created.</summary>
        private TickGenerator()
        {
        }

        #endregion

        #region prop

        /// <summary>
        ///     Gets the default tick generator.
        /// </summary>
        public static TickGenerator Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new TickGenerator();
                }

                return fDefault;
            }
        }

        #endregion

        /// <summary>
        ///     Raised when the TextSin has got some text it wants to output.
        ///     a single tick lasts 100 nano seconds (1 sec/10000000)
        /// </summary>
        public event TickGeneratorHandler Tick;

        #region internal types

        /// <summary>
        ///     Stores info on a single time observer (usually a viseme player, but can also be something else).
        /// </summary>
        private class TickObserver
        {
            /// <summary>Gets or sets the time left.</summary>
            public int TimeLeft { get; set; }

            /// <summary>Gets or sets the callback.</summary>
            public System.Action Callback { get; set; }
        }

        #endregion

        #region fields

        /// <summary>The f is running.</summary>
        private bool fIsRunning;

        /// <summary>The f timer.</summary>
        private Multimedia.Timer fTimer;

        /// <summary>The f stop watch.</summary>
        private System.Diagnostics.Stopwatch fStopWatch;

        /// <summary>The f default.</summary>
        private static TickGenerator fDefault;

        #endregion

        #region functions

        /// <summary>
        ///     Makes certain that the timer is running. This is incremental
        /// </summary>
        public void Start()
        {
            if (fIsRunning == false)
            {
                fStopWatch = new System.Diagnostics.Stopwatch();
                fTimer = new Multimedia.Timer();
                fTimer.Mode = Multimedia.TimerMode.Periodic;
                fTimer.Resolution = Multimedia.Timer.Capabilities.periodMin;
                fTimer.Period = Multimedia.Timer.Capabilities.periodMin;
                fTimer.Started += fTimer_Started;
                fTimer.Tick += fTimer_Tick;
                fTimer.Start();
                fIsRunning = true;
            }
        }

        /// <summary>
        ///     Stops the timer if incremental count has reached 0.
        /// </summary>
        public void Stop()
        {
            if (fIsRunning)
            {
                fTimer.Stop();
                fTimer.Started -= fTimer_Started;
                fTimer.Tick -= fTimer_Tick;
                fTimer.Dispose();
                fTimer = null;
                fStopWatch.Stop();
                fStopWatch = null;
                fIsRunning = false;
            }
        }

        #endregion

        #region timer callbacks

        /// <summary>Handles the Tick event of the fTimer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fTimer_Tick(object sender, System.EventArgs e)
        {
            if (Tick != null)
            {
                var iEllapsed = fStopWatch.Elapsed.Ticks;
                fStopWatch.Restart();

                // Debug.Print(iEllapsed.ToString());
                Tick(iEllapsed);
            }
        }

        /// <summary>Handles the Started event of the fTimer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void fTimer_Started(object sender, System.EventArgs e)
        {
            fStopWatch.Start();
        }

        #endregion
    }
}