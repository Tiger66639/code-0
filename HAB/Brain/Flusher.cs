// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flusher.cs" company="">
//   
// </copyright>
// <summary>
//   used to save neurons to the storage when they have been changed and the
//   network is set up to always stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     used to save neurons to the storage when they have been changed and the
    ///     network is set up to always stream.
    /// </summary>
    internal class AutoFlusher
    {
        /// <summary>The f count.</summary>
        private int fCount;

        /// <summary>The f flushing.</summary>
        private volatile bool fFlushing; // to prevent from trying to do multiple  flushes at the same time. 

        /// <summary>The f timer.</summary>
        private System.Threading.Timer fTimer;

        /// <summary>The f lock.</summary>
        private readonly object fLock = new object();

        /// <summary>The stop.</summary>
        public void Stop()
        {
            if (fTimer != null)
            {
                fTimer.Dispose();
                fTimer = null;
            }
        }

        /// <summary>
        ///     Instructs the system that the db has been changed again and that it's
        ///     time again to evaluate wether a flush is required or not.
        /// </summary>
        /// <param name="changed">The changed.</param>
        internal void TryFlush()
        {
            lock (fLock)
            {
                fCount++;
                if (fTimer == null)
                {
                    fTimer = new System.Threading.Timer(
                        TimeCallback, 
                        null, 
                        Settings.WriteBufferDelay, 
                        System.Threading.Timeout.Infinite);
                }
                else if (fCount == 1)
                {
                    // if it's the first write, but the timer already exists, than it is in flush mode, and the time may still be a little long, so change it to the write delay
                    fTimer.Change(Settings.WriteBufferDelay, System.Threading.Timeout.Infinite);
                }

                if (fCount > Settings.MaxWriteBufferSize)
                {
                    // if we have reached the maximum nr of writes, do a flush even if the time hasn't passed yet.
                    System.Action<object> iFlush = TimeCallback;
                    iFlush.BeginInvoke(null, null, null);
                    fCount = 0; // we reset here, otherwise the callback gets called to many times.
                }
            }
        }

        /// <summary>
        ///     starts a save and clean operation immediatly asynchronically.
        /// </summary>
        internal void ForceFlush()
        {
            System.Action<object> iFlush = TimeCallback;
            iFlush.BeginInvoke(null, null, null);
        }

        /// <summary>The callback for the timer, executed on a seperate thread, so we can
        ///     save the neurons the same thread.</summary>
        /// <param name="value">The value.</param>
        private void TimeCallback(object value)
        {
            var iDoFlush = false;
            lock (fLock)
            {
                if (fTimer != null)
                {
                    // if the timer has already been destroyed, another thread has flushed.
                    fTimer.Dispose();
                    fTimer = null;
                    if (fFlushing == false)
                    {
                        iDoFlush = true;
                        fFlushing = true;
                    }
                    else
                    {
                        iDoFlush = false;
                    }
                }
            }

            if (iDoFlush)
            {
                // don't try to flush multiple times.
                try
                {
                    Brain.Current.FlushAndClean();
                }
                finally
                {
                    fFlushing = false;
                }
            }
        }
    }
}