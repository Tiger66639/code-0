// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDListFactory.cs" company="">
//   
// </copyright>
// <summary>
//   generates and buffers Lists of ulongs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     generates and buffers Lists of ulongs.
    /// </summary>
    public class IDListFactory
    {
        /// <summary>The maxlistsize.</summary>
        private const int MAXLISTSIZE = 1200;

                          // max list size of large list, anything above gets cut, so that there isnt' to much mem usage.

        /// <summary>The maxnrrecycledlists.</summary>
        private const int MAXNRRECYCLEDLISTS = 100;

                          // max nr of items in a single list before a new list is used (reddistribute accross threads)

        /// <summary>The largelistsize.</summary>
        public const int LARGELISTSIZE = 250;

                         // the min size considered to be a 'large' list. (optimization between smaller and big lists).

        /// <summary>
        ///     contains all the available liss that can be reused. They have already
        ///     been stored in lists that can be dispatched to the different threads.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>> fAvailable =
                new System.Collections.Generic.Queue
                    <System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>>();

        /// <summary>The f large available.</summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>> fLargeAvailable =
                new System.Collections.Generic.Queue
                    <System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>>();

        /// <summary>
        ///     this list can be used by the GC to collect data. This provides a way
        ///     for the GC to use the same list all the time, even if it is in
        ///     different threads.
        /// </summary>
        private static System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>> fGCBuffer =
            new System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>();

        /// <summary>The f large gc buffer.</summary>
        private static System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>> fLargeGCBuffer =
            new System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>();

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>> fBuffer;

        /// <summary>The f large buffer.</summary>
        private System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>> fLargeBuffer;

        /// <summary>gets a new buffer with the specified <paramref name="capacity"/></summary>
        /// <param name="capacity">Used for speedups/mem optimization (big lists). Value can be left
        ///     blank</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<ulong> GetBuffer(int capacity = 0)
        {
            System.Collections.Generic.List<ulong> iRes;
            if (capacity >= LARGELISTSIZE)
            {
                if (fBuffer == null || fBuffer.Count == 0)
                {
                    lock (fAvailable)
                    {
                        if (fAvailable.Count > 0)
                        {
                            fBuffer = fAvailable.Dequeue();
                        }
                    }
                }

                if (fBuffer != null && fBuffer.Count > 0)
                {
                    iRes = fBuffer.Dequeue();
                    iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                    if (iRes.Capacity < capacity)
                    {
                        iRes.Capacity = capacity;
                    }

                    return iRes;
                }
            }
            else
            {
                if (fLargeBuffer == null || fLargeBuffer.Count == 0)
                {
                    lock (fLargeAvailable)
                    {
                        if (fLargeAvailable.Count > 0)
                        {
                            fLargeBuffer = fLargeAvailable.Dequeue();
                        }
                    }
                }

                if (fLargeBuffer != null && fLargeBuffer.Count > 0)
                {
                    iRes = fLargeBuffer.Dequeue();
                    iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                    if (iRes.Capacity < capacity)
                    {
                        iRes.Capacity = capacity;
                    }

                    return iRes;
                }
            }

            iRes = new System.Collections.Generic.List<ulong>();
            if (iRes.Capacity < capacity)
            {
                iRes.Capacity = capacity;
            }

            return iRes;
        }

        /// <summary>Releases the specified list so it can be reused.</summary>
        /// <param name="toRelease">To release.</param>
        /// <param name="fromGC">The from GC.</param>
        public void Recycle(System.Collections.Generic.List<ulong> toRelease, bool fromGC = false)
        {
            toRelease.Clear();
            if (toRelease.Capacity > MAXLISTSIZE)
            {
                // every so often, an idlist can be really big. Don't need to buffer all of them in such a big size.
                toRelease.Capacity = MAXLISTSIZE;
            }

            if (fromGC == false)
            {
                if (toRelease.Capacity >= LARGELISTSIZE)
                {
                    Recycle(toRelease, fLargeAvailable, ref fLargeBuffer);
                }
                else
                {
                    Recycle(toRelease, fAvailable, ref fBuffer);
                }
            }
            else
            {
                lock (this)
                {
                    // can be multiple garbage threads, make certain only 1 can process this block at a time.
                    if (toRelease.Capacity >= LARGELISTSIZE)
                    {
                        Recycle(toRelease, fLargeAvailable, ref fLargeGCBuffer);
                    }
                    else
                    {
                        Recycle(toRelease, fAvailable, ref fGCBuffer);
                    }
                }
            }
        }

        /// <summary>The recycle.</summary>
        /// <param name="toRelease">The to release.</param>
        /// <param name="available">The available.</param>
        /// <param name="buffer">The buffer.</param>
        private void Recycle(System.Collections.Generic.List<ulong> toRelease, System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>> available, 
            ref System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>> buffer)
        {
            if (buffer == null)
            {
                buffer = new System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>();
            }

            buffer.Enqueue(toRelease);
            if (buffer.Count >= MAXNRRECYCLEDLISTS)
            {
                lock (available)
                {
                    if (available.Count < MAXNRRECYCLEDLISTS * 2)
                    {
                        // a small protection: don't need to buffer everything, make certain some room remains..
                        available.Enqueue(fBuffer);
                    }
                }

                buffer = new System.Collections.Generic.Queue<System.Collections.Generic.List<ulong>>();
            }
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        internal void Clear()
        {
            fBuffer = null;
            fLargeBuffer = null;
            fLargeGCBuffer.Clear();
            fGCBuffer.Clear();
            fAvailable.Clear();
            fLargeAvailable.Clear();
        }
    }
}