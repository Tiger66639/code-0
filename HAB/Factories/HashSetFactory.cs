// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HashSetFactory.cs" company="">
//   
// </copyright>
// <summary>
//   a factory for hashSets.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>a factory for hashSets.</summary>
    /// <typeparam name="T"></typeparam>
    public class HashSetFactory<T>
    {
        /// <summary>Initializes a new instance of the <see cref="HashSetFactory{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.ObjectListFactory`1"/> class.</summary>
        /// <param name="maxBufSize">the maximum nr of lists that are kept in the buffer when the factory
        ///     gets recycled. This is to prevent all buffers from keeping the max
        ///     amount of lists available..</param>
        public HashSetFactory(int maxBufSize = 350)
        {
            fMaxBufSize = maxBufSize;
        }

        /// <summary>
        ///     returns the nr of nlists that are currently buffered.
        /// </summary>
        public int BufferCount
        {
            get
            {
                return fBuffer.Count;
            }
        }

        /// <summary>gets a buffered list when possible or creates a new one oetherwise.</summary>
        /// <returns>The <see cref="HashSet"/>.</returns>
        internal System.Collections.Generic.HashSet<T> GetBuffer()
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
                var iRes = fBuffer.Dequeue();
                iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                return iRes;
            }

            return new System.Collections.Generic.HashSet<T>();
        }

        /// <summary>Releases the specified list so it can be reused.</summary>
        /// <param name="toRelease">To release.</param>
        /// <param name="fromGC">The from GC.</param>
        public void Recycle(System.Collections.Generic.HashSet<T> toRelease, bool fromGC)
        {
            toRelease.Clear(); // no need to buffer these objects for a longer period.
            if (fromGC == false)
            {
                if (fBuffer == null)
                {
                    fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>(fMaxBufSize);
                }
                else if (fBuffer.Count >= fMaxBufSize)
                {
                    lock (fAvailable) fAvailable.Enqueue(fBuffer);
                    fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>(fMaxBufSize);
                }

                fBuffer.Enqueue(toRelease);
            }
            else
            {
                lock (fGCBuffer)
                {
                    fGCBuffer.Enqueue(toRelease);
                    if (fGCBuffer.Count >= fMaxBufSize)
                    {
                        lock (fAvailable)
                        {
                            fAvailable.Enqueue(fGCBuffer);
                            fGCBuffer =
                                new System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>(fMaxBufSize);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        internal void Clear()
        {
            fBuffer = null;
            fGCBuffer.Clear();
            fAvailable.Clear();
        }

        #region fields

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>> fBuffer;

        /// <summary>
        ///     this list can be used by the GC to collect data. This provides a way
        ///     for the GC to use the same list all the time, even if it is in
        ///     different threads.
        /// </summary>
        private static System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>> fGCBuffer =
            new System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>();

        /// <summary>
        ///     contains the global queues of available items. The threads can
        ///     add/remove items from this as needed.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>> fAvailable =
                new System.Collections.Generic.Queue
                    <System.Collections.Generic.Queue<System.Collections.Generic.HashSet<T>>>();

        /// <summary>
        ///     the maximum nr of lists that are kept in the buffer when the factory
        ///     gets recycled. This is to prevent all buffers from keeping the max
        ///     amount of lists available.
        /// </summary>
        private readonly int fMaxBufSize;

        #endregion
    }
}