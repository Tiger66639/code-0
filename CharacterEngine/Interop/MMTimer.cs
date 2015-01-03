// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MMTimer.cs" company="">
//   
// </copyright>
// <summary>
//   Defines constants for the multimedia Timer's event types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region License

/* Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */
#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */
#endregion

namespace Multimedia
{
    /// <summary>
    ///     Defines constants for the multimedia Timer's event types.
    /// </summary>
    public enum TimerMode
    {
        /// <summary>
        ///     Timer event occurs once.
        /// </summary>
        OneShot, 

        /// <summary>
        ///     Timer event occurs periodically.
        /// </summary>
        Periodic
    };

    /// <summary>
    ///     Represents information about the multimedia Timer's capabilities.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct TimerCaps
    {
        /// <summary>
        ///     Minimum supported period in milliseconds.
        /// </summary>
        public int periodMin;

        /// <summary>
        ///     Maximum supported period in milliseconds.
        /// </summary>
        public int periodMax;
    }

    /// <summary>
    ///     Represents the Windows multimedia timer.
    /// </summary>
    public sealed class Timer : System.ComponentModel.IComponent
    {
        #region IDisposable Members

        /// <summary>
        ///     Frees timer resources.
        /// </summary>
        public void Dispose()
        {
            

            if (disposed)
            {
                return;
            }

            

            if (IsRunning)
            {
                Stop();
            }

            disposed = true;

            OnDisposed(System.EventArgs.Empty);
        }

        #endregion

        #region Timer Members

        #region Delegates

        // Represents the method that is called by Windows when a timer event occurs.
        /// <summary>The time proc.</summary>
        /// <param name="id">The id.</param>
        /// <param name="msg">The msg.</param>
        /// <param name="user">The user.</param>
        /// <param name="param1">The param 1.</param>
        /// <param name="param2">The param 2.</param>
        private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        // Represents methods that raise events.
        /// <summary>The event raiser.</summary>
        /// <param name="e">The e.</param>
        private delegate void EventRaiser(System.EventArgs e);

        #endregion

        #region Win32 Multimedia Timer Functions

        // Gets timer capabilities.
        /// <summary>The time get dev caps.</summary>
        /// <param name="caps">The caps.</param>
        /// <param name="sizeOfTimerCaps">The size of timer caps.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

        // Creates and starts the timer.
        /// <summary>The time set event.</summary>
        /// <param name="delay">The delay.</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="proc">The proc.</param>
        /// <param name="user">The user.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

        // Stops and destroys the timer.
        /// <summary>The time kill event.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        // Indicates that the operation was successful.
        /// <summary>The timer r_ noerror.</summary>
        private const int TIMERR_NOERROR = 0;

        #endregion

        #region Fields

        // Timer identifier.
        /// <summary>The timer id.</summary>
        private int timerID;

        // Timer mode.
        /// <summary>The mode.</summary>
        private volatile TimerMode mode;

        // Period between timer events in milliseconds.
        /// <summary>The period.</summary>
        private volatile int period;

        // Timer resolution in milliseconds.
        /// <summary>The resolution.</summary>
        private volatile int resolution;

        // Called by Windows when a timer periodic event occurs.
        /// <summary>The time proc periodic.</summary>
        private TimeProc timeProcPeriodic;

        // Called by Windows when a timer one shot event occurs.
        /// <summary>The time proc one shot.</summary>
        private TimeProc timeProcOneShot;

        // Represents the method that raises the Tick event.
        /// <summary>The tick raiser.</summary>
        private EventRaiser tickRaiser;

        // Indicates whether or not the timer is running.

        // Indicates whether or not the timer has been disposed.
        /// <summary>The disposed.</summary>
        private volatile bool disposed;

        // The ISynchronizeInvoke object to use for marshaling events.
        /// <summary>The synchronizing object.</summary>
        private System.ComponentModel.ISynchronizeInvoke synchronizingObject;

        // For implementing IComponent.

        // Multimedia timer capabilities.
        /// <summary>The caps.</summary>
        private static readonly TimerCaps caps;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the Timer has started;
        /// </summary>
        public event System.EventHandler Started;

        /// <summary>
        ///     Occurs when the Timer has stopped;
        /// </summary>
        public event System.EventHandler Stopped;

        /// <summary>
        ///     Occurs when the time period has elapsed.
        /// </summary>
        public event System.EventHandler Tick;

        #endregion

        #region Construction

        /// <summary>Initializes static members of the <see cref="Timer"/> class. 
        ///     Initialize class.</summary>
        static Timer()
        {
            // Get multimedia timer capabilities.
            timeGetDevCaps(ref caps, System.Runtime.InteropServices.Marshal.SizeOf(caps));
        }

        /// <summary>Initializes a new instance of the Timer class with the specified IContainer.</summary>
        /// <param name="container">The IContainer to which the Timer will add itself.</param>
        public Timer(System.ComponentModel.IContainer container)
        {
            Site = null;
            IsRunning = false;

            // 
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);

            Initialize();
        }

        /// <summary>
        ///     Initializes a new instance of the Timer class.
        /// </summary>
        public Timer()
        {
            Site = null;
            IsRunning = false;
            Initialize();
        }

        /// <summary>Finalizes an instance of the <see cref="Timer"/> class. </summary>
        ~Timer()
        {
            if (IsRunning)
            {
                // Stop and destroy timer.
                timeKillEvent(timerID);
            }
        }

        // Initialize timer with default values.
        /// <summary>The initialize.</summary>
        private void Initialize()
        {
            mode = TimerMode.Periodic;
            period = Capabilities.periodMin;
            resolution = 1;

            IsRunning = false;

            timeProcPeriodic = TimerPeriodicEventCallback;
            timeProcOneShot = TimerOneShotEventCallback;
            tickRaiser = OnTick;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        ///     The timer failed to start.
        /// </exception>
        public void Start()
        {
            

            if (disposed)
            {
                throw new System.ObjectDisposedException("Timer");
            }

            

            #region Guard

            if (IsRunning)
            {
                return;
            }

            #endregion

            // If the periodic event callback should be used.
            if (Mode == TimerMode.Periodic)
            {
                // Create and start timer.
                timerID = timeSetEvent(Period, Resolution, timeProcPeriodic, 0, (int)Mode);
            }

            // Else the one shot event callback should be used.
            else
            {
                // Create and start timer.
                timerID = timeSetEvent(Period, Resolution, timeProcOneShot, 0, (int)Mode);
            }

            // If the timer was created successfully.
            if (timerID != 0)
            {
                IsRunning = true;

                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                {
                    SynchronizingObject.BeginInvoke(new EventRaiser(OnStarted), new object[] { System.EventArgs.Empty });
                }
                else
                {
                    OnStarted(System.EventArgs.Empty);
                }
            }
            else
            {
                throw new TimerStartException("Unable to start multimedia Timer.");
            }
        }

        /// <summary>
        ///     Stops timer.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     If the timer has already been disposed.
        /// </exception>
        public void Stop()
        {
            

            if (disposed)
            {
                throw new System.ObjectDisposedException("Timer");
            }

            

            #region Guard

            if (!IsRunning)
            {
                return;
            }

            #endregion

            // Stop and destroy timer.
            var result = timeKillEvent(timerID);

            System.Diagnostics.Debug.Assert(result == TIMERR_NOERROR);

            IsRunning = false;

            if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
            {
                SynchronizingObject.BeginInvoke(new EventRaiser(OnStopped), new object[] { System.EventArgs.Empty });
            }
            else
            {
                OnStopped(System.EventArgs.Empty);
            }
        }

        #region Callbacks

        // Callback method called by the Win32 multimedia timer when a timer
        // periodic event occurs.
        /// <summary>The timer periodic event callback.</summary>
        /// <param name="id">The id.</param>
        /// <param name="msg">The msg.</param>
        /// <param name="user">The user.</param>
        /// <param name="param1">The param 1.</param>
        /// <param name="param2">The param 2.</param>
        private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (synchronizingObject != null)
            {
                synchronizingObject.BeginInvoke(tickRaiser, new object[] { System.EventArgs.Empty });
            }
            else
            {
                OnTick(System.EventArgs.Empty);
            }
        }

        // Callback method called by the Win32 multimedia timer when a timer
        // one shot event occurs.
        /// <summary>The timer one shot event callback.</summary>
        /// <param name="id">The id.</param>
        /// <param name="msg">The msg.</param>
        /// <param name="user">The user.</param>
        /// <param name="param1">The param 1.</param>
        /// <param name="param2">The param 2.</param>
        private void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (synchronizingObject != null)
            {
                synchronizingObject.BeginInvoke(tickRaiser, new object[] { System.EventArgs.Empty });
                Stop();
            }
            else
            {
                OnTick(System.EventArgs.Empty);
                Stop();
            }
        }

        #endregion

        #region Event Raiser Methods

        // Raises the Disposed event.
        /// <summary>The on disposed.</summary>
        /// <param name="e">The e.</param>
        private void OnDisposed(System.EventArgs e)
        {
            var handler = Disposed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Raises the Started event.
        /// <summary>The on started.</summary>
        /// <param name="e">The e.</param>
        private void OnStarted(System.EventArgs e)
        {
            var handler = Started;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Raises the Stopped event.
        /// <summary>The on stopped.</summary>
        /// <param name="e">The e.</param>
        private void OnStopped(System.EventArgs e)
        {
            var handler = Stopped;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Raises the Tick event.
        /// <summary>The on tick.</summary>
        /// <param name="e">The e.</param>
        private void OnTick(System.EventArgs e)
        {
            var handler = Tick;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the object used to marshal event-handler calls.
        /// </summary>
        public System.ComponentModel.ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                return synchronizingObject;
            }

            set
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                synchronizingObject = value;
            }
        }

        /// <summary>
        ///     Gets or sets the time between Tick events.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     If the timer has already been disposed.
        /// </exception>
        public int Period
        {
            get
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                return period;
            }

            set
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                if (value < Capabilities.periodMin || value > Capabilities.periodMax)
                {
                    throw new System.ArgumentOutOfRangeException(
                        "Period", 
                        value, 
                        "Multimedia Timer period out of range.");
                }

                

                period = value;

                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the timer resolution.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     If the timer has already been disposed.
        /// </exception>
        /// <remarks>
        ///     The resolution is in milliseconds. The resolution increases
        ///     with smaller values; a resolution of 0 indicates periodic events
        ///     should occur with the greatest possible accuracy. To reduce system
        ///     overhead, however, you should use the maximum value appropriate
        ///     for your application.
        /// </remarks>
        public int Resolution
        {
            get
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                return resolution;
            }

            set
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(
                        "Resolution", 
                        value, 
                        "Multimedia timer resolution out of range.");
                }

                

                resolution = value;

                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        ///     Gets the timer mode.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     If the timer has already been disposed.
        /// </exception>
        public TimerMode Mode
        {
            get
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                return mode;
            }

            set
            {
                

                if (disposed)
                {
                    throw new System.ObjectDisposedException("Timer");
                }

                

                mode = value;

                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the Timer is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Gets the timer capabilities.
        /// </summary>
        public static TimerCaps Capabilities
        {
            get
            {
                return caps;
            }
        }

        #endregion

        #endregion

        #region IComponent Members

        /// <summary>The disposed.</summary>
        public event System.EventHandler Disposed;

        /// <summary>Gets or sets the site.</summary>
        public System.ComponentModel.ISite Site { get; set; }

        #endregion
    }

    /// <summary>
    ///     The exception that is thrown when a timer fails to start.
    /// </summary>
    public class TimerStartException : System.ApplicationException
    {
        /// <summary>Initializes a new instance of the TimerStartException class.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TimerStartException(string message)
            : base(message)
        {
        }
    }
}