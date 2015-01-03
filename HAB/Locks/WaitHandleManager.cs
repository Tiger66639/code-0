// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitHandleManager.cs" company="">
//   
// </copyright>
// <summary>
//   The wait handle manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The wait handle manager.</summary>
    internal class WaitHandleManager
    {
        /// <summary>The f current.</summary>
        private static readonly WaitHandleManager fCurrent = new WaitHandleManager();

        /// <summary>The f nr of preloaded handles.</summary>
        private volatile int fNrOfPreloadedHandles;

        /// <summary>The f nr of used handles.</summary>
        private volatile int fNrOfUsedHandles;

                             // stores the amount of handles that are currently being used. This allows us to create new handles as needed when the value for NrOfPreloadedhandles changes.

        /// <summary>The f active handles.</summary>
        private readonly System.Collections.Generic.List<System.Threading.ManualResetEventSlim> fActiveHandles =
            new System.Collections.Generic.List<System.Threading.ManualResetEventSlim>();

                                                                                                // so we can release them in case of locks.

        /// <summary>The f wait handles.</summary>
        private readonly System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim> fWaitHandles =
            new System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>();

        /// <summary>Gets the current.</summary>
        public static WaitHandleManager Current
        {
            get
            {
                return fCurrent;
            }
        }

        #region NrOfPreloadedHandles

        /// <summary>
        ///     Gets/sets the number of preloaded waithandles that the LockMananager
        ///     creates that the <see cref="neuronLock" /> can use.
        /// </summary>
        public int NrOfPreloadedHandles
        {
            get
            {
                return fNrOfPreloadedHandles;
            }

            set
            {
                if (value != fNrOfPreloadedHandles)
                {
                    fNrOfPreloadedHandles = value;
                    if (fNrOfPreloadedHandles < value)
                    {
                        lock (fWaitHandles)
                        {
                            while (fWaitHandles.Count + fNrOfUsedHandles < fNrOfPreloadedHandles)
                            {
                                fWaitHandles.Enqueue(new System.Threading.ManualResetEventSlim(false));
                            }
                        }
                    }
                    else
                    {
                        lock (fWaitHandles)
                        {
                            while (fWaitHandles.Count + fNrOfUsedHandles > fNrOfPreloadedHandles
                                   && fWaitHandles.Count > 0)
                            {
                                // stop dequeuing when no more spares.
                                fWaitHandles.Dequeue();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Wait handle

        /// <summary>Gets a waithandle that is on stock, or a new one if there aren't any
        ///     more.</summary>
        /// <returns>The <see cref="ManualResetEventSlim"/>.</returns>
        internal System.Threading.ManualResetEventSlim GetWaitHandle()
        {
            lock (fWaitHandles)
            {
                System.Threading.ManualResetEventSlim iRes;
                fNrOfUsedHandles++;
                if (fWaitHandles.Count > 0)
                {
                    iRes = fWaitHandles.Dequeue();
                    iRes.Reset(); // need to make certain that the event is blocking
                }
                else
                {
                    iRes = new System.Threading.ManualResetEventSlim(false);
                }

                fActiveHandles.Add(iRes); // so we can unlock again if needed.
                return iRes;
            }
        }

        /// <summary>The release wait handle.</summary>
        /// <param name="toRelease">The to release.</param>
        internal void ReleaseWaitHandle(System.Threading.ManualResetEventSlim toRelease)
        {
            lock (fWaitHandles)
            {
                fNrOfUsedHandles--;
                fWaitHandles.Enqueue(toRelease);
                fActiveHandles.Remove(toRelease); // so we can unlock again if needed.
            }
        }

        /// <summary>
        ///     Releases all wait-handles that are still locking threads. This is used
        ///     to kill all the processors. We use this in case there were any
        ///     deadlocks. This can lift them.
        /// </summary>
        internal void ReleaseAllHandles()
        {
            while (fActiveHandles.Count > 0 && ThreadManager.Default.HasRunningProcessors)
            {
                foreach (var i in fActiveHandles.ToArray())
                {
                    // local copy cause the 'set' could change the list.
                    if (i != null)
                    {
                        i.Set();
                    }
                }

                foreach (var i in fWaitHandles.ToArray())
                {
                    // simply walk over every handle and let it unlock.
                    if (i != null)
                    {
                        // this happened 1 time, better to check for this.
                        i.Set();
                    }
                }
            }

            while (fActiveHandles.Count > 0)
            {
                // when we get here and there are stil active handles, something strange happened. The system is done but there are still some handles active, which shouldn't, so remove them from here.
                ReleaseWaitHandle(fActiveHandles[0]);
            }
        }

        #endregion
    }
}