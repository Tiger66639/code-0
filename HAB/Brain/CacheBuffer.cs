// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheBuffer.cs" company="">
//   
// </copyright>
// <summary>
//   Buffers neurons that were added to the cache through a weakreference, for
//   a short amount of time so that they remain in mem a bit longer, in case
//   they get reused shortly, which is often the case (espesically for
//   executable neurons). Also makes certain that the cache gets cleaned out
//   if the system isn't set to auto save the data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Buffers neurons that were added to the cache through a weakreference, for
    ///     a short amount of time so that they remain in mem a bit longer, in case
    ///     they get reused shortly, which is often the case (espesically for
    ///     executable neurons). Also makes certain that the cache gets cleaned out
    ///     if the system isn't set to auto save the data.
    /// </summary>
    internal class CacheBuffer
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="CacheBuffer" /> class.
        /// </summary>
        public CacheBuffer()
        {
            fActiveBuffer = new System.Collections.Generic.List<Neuron>(Settings.CacheBufferSize / 3);

                // /3 cause we have 3 lists.
            var iTime = Settings.CacheBufferDelay / 3;

                // we do /3 cause we have 3 lists. A list needs to move through 3 timer clocks before it is disgarded
            fTimer = new System.Threading.Timer(OnTimeSlot, null, iTime, iTime);
        }

        #endregion

        #region prop

        /// <summary>Gets the default.</summary>
        public static CacheBuffer Default
        {
            get
            {
                return fDefault;
            }
        }

        #endregion

        /// <summary>Sets the minimum amount of time (in milliseconds), that the neurons
        ///     should be kept in memory</summary>
        /// <param name="value">The value.</param>
        internal void SetDelay(int value)
        {
            fTimer.Change(value / 3, value / 3);

                // we do /3 cause we have 3 lists. A list needs to move through 3 timer clocks before it is disgarded
        }

        /// <summary>Checks if the new maximum size is smaller than our current buffer, if
        ///     so, this is collected. Also adjusts the capacity.</summary>
        /// <param name="value"></param>
        internal void SetBufferSize(int value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Adds the specified to neuron to the cachebuffer so that it remains in
        ///     mem for a short amount of time.</summary>
        /// <param name="toAdd">To add.</param>
        internal void Add(Neuron toAdd)
        {
            lock (fBufferLock)
            {
                // this need to be done so that the timer function doesn't screw things up.
                fActiveBuffer.Add(toAdd);
                if (fActiveBuffer.Count == Settings.CacheBufferSize / 3)
                {
                    // if the buffer list is full, switch buffers.
                    fPrevBuffers.Enqueue(fActiveBuffer);
                    if (fPrevBuffers.Count > 2)
                    {
                        // first couple of buffers need to be added.
                        fActiveBuffer = fPrevBuffers.Dequeue();
                        fActiveBuffer.Clear();
                    }
                    else
                    {
                        fActiveBuffer = new System.Collections.Generic.List<Neuron>(Settings.CacheBufferSize / 3);
                    }
                }
            }
        }

        #region Fields

        /// <summary>
        ///     Stores the last 2 lists, that were replaced by the timer. This creates
        ///     the delay.
        /// </summary>
        private readonly System.Collections.Generic.Queue<System.Collections.Generic.List<Neuron>> fPrevBuffers =
            new System.Collections.Generic.Queue<System.Collections.Generic.List<Neuron>>(3);

        /// <summary>
        ///     Stores the neurons that were added to the cache during this time slot.
        /// </summary>
        private System.Collections.Generic.List<Neuron> fActiveBuffer;

        /// <summary>The f buffer lock.</summary>
        private readonly object fBufferLock = new object();

        /// <summary>The f timer.</summary>
        private readonly System.Threading.Timer fTimer;

        /// <summary>The f default.</summary>
        private static readonly CacheBuffer fDefault = new CacheBuffer();

        /// <summary>The f running.</summary>
        private volatile bool fRunning; // used to make the callback reentrent.

        /// <summary>The f cul count.</summary>
        private int fCulCount;

                    // we keep track of the amount of items that were added, when this reaches a threash-hold, we clean the cache.
        #endregion

        #region functions

        /// <summary>Called when the timer ticks.</summary>
        /// <param name="stateInfo">The state info.</param>
        private void OnTimeSlot(object stateInfo)
        {
            var iContinue = false;
            lock (fTimer)
            {
                // we need to make it reentrent: when already running, simply skip.
                if (fRunning == false)
                {
                    fRunning = true;
                    iContinue = true;
                }
            }

            if (iContinue)
            {
                try
                {
                    lock (fBufferLock)
                        fCulCount += fActiveBuffer.Count;

                            // only after a block has been released, some items might be unloaded. needs a lock for fACtiveBuffer
                    if (fCulCount >= Settings.CleanCacheAfter)
                    {
                        // don't need to do any locking for the fCulCount, the start of this function garantees that only 1 thread at a time executes this part
                        fCulCount = 0;
                        if (Settings.StorageMode != NeuronStorageMode.AlwaysStream)
                        {
                            Brain.Current.CulCash();
                        }
                        else
                        {
                            Brain.Current.Flusher.ForceFlush();
                        }
                    }

                    lock (fBufferLock)
                    {
                        // needs to be locked, cause the clear and add operations can also change the fPrevBuffers list.
                        fPrevBuffers.Enqueue(fActiveBuffer);
                        if (fPrevBuffers.Count > 2)
                        {
                            // first couple of buffers need to be added.
                            fActiveBuffer = fPrevBuffers.Dequeue();
                            fActiveBuffer.Clear();
                        }
                        else
                        {
                            fActiveBuffer = new System.Collections.Generic.List<Neuron>(Settings.CacheBufferSize / 3);
                        }
                    }
                }
                finally
                {
                    fRunning = false;
                }
            }
        }

        /// <summary>
        ///     Clears the cache buffer.
        /// </summary>
        public void Clear()
        {
            lock (fBufferLock)
            {
                // lock the timer, when doing a clear, this is the object that can interfear with the current thread.
                fPrevBuffers.Clear();
                fActiveBuffer.Clear();
            }
        }

        #endregion
    }
}