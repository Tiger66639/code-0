// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitObject.cs" company="">
//   
// </copyright>
// <summary>
//   A replacement for WaitHandle.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A replacement for WaitHandle.
    /// </summary>
    internal class WaitObject
    {
        /// <summary>The f recycled.</summary>
        [System.ThreadStatic]
        private static System.Collections.Generic.Queue<WaitObject> fRecycled; // stores the recycled objects per thread.

        /// <summary>
        ///     keeps track of the total
        /// </summary>
        /// <summary>
        ///     Keeps track of how many threads the object still needs to wait for.
        /// </summary>
        private int fCount;

        /// <summary>The f handle.</summary>
        private System.Threading.ManualResetEventSlim fHandle;

        /// <summary>
        ///     Prevents a default instance of the <see cref="WaitObject" /> class from being created.
        /// </summary>
        private WaitObject()
        {
        }

        /// <summary>creates a new object, if possible a recycled one is used.</summary>
        /// <returns>The <see cref="WaitObject"/>.</returns>
        public static WaitObject Create()
        {
            if (fRecycled == null)
            {
                fRecycled = new System.Collections.Generic.Queue<WaitObject>(); // need to initialize per thread
                return new WaitObject();
            }

            if (fRecycled.Count > 0)
            {
                var iRes = fRecycled.Dequeue();
                iRes.fHandle = null;
                iRes.fCount = 0;
                return iRes;
            }

            return new WaitObject();
        }

        /// <summary>adds the specified object back to the list so it can be recycled/reused.</summary>
        /// <param name="toAdd"></param>
        public static void Recycle(WaitObject toAdd)
        {
            fRecycled.Enqueue(toAdd);
        }

        /// <summary>
        ///     Sets the object as signalled to indicate that a thread that this object was waiting on, has stopped.
        /// </summary>
        public void Set()
        {
            lock (this)
            {
                // this needs to be made save cause multiple threads might do this at the same time (local locks are used very often)
                fCount--;
                if (fCount == 0 && fHandle != null)
                {
                    // when we get to 0, all threads have ended and the thread that was waiting, can continue again.
                    fHandle.Set();
                }
            }
        }

        /// <summary>
        ///     Waits until all the threads are done, if required.
        /// </summary>
        public void Wait()
        {
            if (fHandle != null)
            {
                fHandle.Wait();
                WaitHandleManager.Current.ReleaseWaitHandle(fHandle);
                fHandle = null;
            }
        }

        /// <summary>Registers a thread to the object so that it will wait for it. The thread should call <see cref="WaitObject.Set"/>
        ///     to release
        ///     the wait object.</summary>
        /// <remarks>We return the same object, so taht the call can be used at the moment that the wait object gets used.</remarks>
        /// <returns>The <see cref="WaitObject"/>.</returns>
        public WaitObject Register()
        {
            lock (this)
            {
                if (fCount == 0)
                {
                    if (fHandle != null)
                    {
                        // just to be save.
                        WaitHandleManager.Current.ReleaseWaitHandle(fHandle);
                    }

                    fHandle = WaitHandleManager.Current.GetWaitHandle();
                }

                fCount++;
            }

            return this;
        }
    }
}