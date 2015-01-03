// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectListFactory.cs" company="">
//   
// </copyright>
// <summary>
//   so we can see where the same list gets recycled 2 times.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{

    #region debug types

    /// <summary>so we can see where the same list gets recycled 2 times.</summary>
    /// <typeparam name="T"></typeparam>
    public class DList<T> : System.Collections.Generic.List<T>
    {
        /// <summary>The recycle count.</summary>
        public int RecycleCount;

        /// <summary>The ref count.</summary>
        public int RefCount;
    }

    #endregion

    /// <summary>a factory that provides faster creation of neuron lists.</summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectListFactory<T>
    {
        /// <summary>Initializes a new instance of the <see cref="ObjectListFactory{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.ObjectListFactory`1"/> class.</summary>
        /// <param name="maxBufSize">the maximum nr of lists that are kept in the buffer when the factory
        ///     gets recycled. This is to prevent all buffers from keeping the max
        ///     amount of lists available..</param>
        /// <param name="maxCapacity">the max capacity for lists that get recycled. This is to prevent from
        ///     slowly moving to the maximum by keeping every list short, but
        ///     reasonable enough.</param>
        public ObjectListFactory(int maxBufSize = 350, int maxCapacity = 50)
        {
            fMaxBufSize = maxBufSize;
            fMaxCapacity = maxCapacity;
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
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<T> GetBuffer()
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

                // #if DEBUG
                // lock (iRes)
                // {
                // if (iRes.RefCount > 0)
                // throw new InvalidOperationException();
                // iRes.RefCount++;
                // iRes.RecycleCount--;
                // if (iRes.RecycleCount > 0)
                // throw new InvalidOperationException();
                // }
                // #endif
                return iRes;
            }
            else
            {
                var iRes = new System.Collections.Generic.List<T>();

                // #if DEBUG
                // iRes.RefCount++;
                // #endif
                return iRes;
            }
        }

        /// <summary>Releases the specified list so it can be reused.</summary>
        /// <param name="toRelease">To release.</param>
        /// <param name="fromGC">The from GC.</param>
        public void Recycle(System.Collections.Generic.List<T> toRelease, bool fromGC = false)
        {
            // DList<T> iToRelease = (DList<T>)toRelease;
            // #if DEBUG
            // lock (toRelease)
            // {
            // iToRelease.RefCount--;
            // if (iToRelease.RefCount > 0)
            // throw new InvalidOperationException();
            // if (iToRelease.RecycleCount > 0)
            // throw new InvalidOperationException();
            // iToRelease.RecycleCount++;
            // }
            // #endif
            toRelease.Clear(); // no need to buffer these objects for a longer period.
            if (toRelease.Capacity > fMaxCapacity)
            {
                toRelease.Capacity = fMaxCapacity;
            }

            if (fromGC == false)
            {
                if (fBuffer == null)
                {
                    fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.List<T>>(fMaxBufSize);
                }
                else if (fBuffer.Count >= fMaxBufSize)
                {
                    lock (fAvailable) fAvailable.Enqueue(fBuffer);
                    fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.List<T>>(fMaxBufSize);
                }

                // #if DEBUG
                // if(fBuffer.Contains(iToRelease) == true)
                // throw new InvalidOperationException();
                // #endif 
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
                                new System.Collections.Generic.Queue<System.Collections.Generic.List<T>>(fMaxBufSize);
                        }
                    }
                }
            }
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fBuffer = null;
            fGCBuffer.Clear();
            fAvailable.Clear();
        }

        #region fields

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<System.Collections.Generic.List<T>> fBuffer;

        /// <summary>
        ///     this list can be used by the GC to collect data. This provides a way
        ///     for the GC to use the same list all the time, even if it is in
        ///     different threads.
        /// </summary>
        private static System.Collections.Generic.Queue<System.Collections.Generic.List<T>> fGCBuffer =
            new System.Collections.Generic.Queue<System.Collections.Generic.List<T>>();

        /// <summary>
        ///     contains the global queues of available items. The threads can
        ///     add/remove items from this as needed.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.List<T>>> fAvailable =
                new System.Collections.Generic.Queue
                    <System.Collections.Generic.Queue<System.Collections.Generic.List<T>>>();

        /// <summary>
        ///     the maximum nr of lists that are kept in the buffer when the factory
        ///     gets recycled. This is to prevent all buffers from keeping the max
        ///     amount of lists available.
        /// </summary>
        private readonly int fMaxBufSize;

        /// <summary>
        ///     the max capacity for lists that get recycled. This is to prevent from
        ///     slowly moving to the maximum by keeping every list short, but
        ///     reasonable enough
        /// </summary>
        private readonly int fMaxCapacity;

        #endregion
    }
}