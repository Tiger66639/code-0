// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The dictionary factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The dictionary factory.</summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    public class DictionaryFactory<T, Y>
    {
        /// <summary>gets a buffered list when possible or creates a new one oetherwise.</summary>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        internal System.Collections.Generic.Dictionary<T, Y> GetBuffer()
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

            return new System.Collections.Generic.Dictionary<T, Y>();
        }

        /// <summary>Releases the specified list so it can be reused.</summary>
        /// <param name="toRelease">To release.</param>
        public void Recycle(System.Collections.Generic.Dictionary<T, Y> toRelease)
        {
            if (fBuffer == null)
            {
                fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<T, Y>>();
            }
            else if (fBuffer.Count >= fMaxBufSize)
            {
                lock (fAvailable) fAvailable.Enqueue(fBuffer);
                fBuffer = new System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<T, Y>>();
            }

            toRelease.Clear(); // no need to buffer these objects for a longer period.

            fBuffer.Enqueue(toRelease);
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fBuffer = null;
            fAvailable.Clear();
        }

        #region fields

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<T, Y>> fBuffer;

        /// <summary>
        ///     contains the global queues of available items. The threads can
        ///     add/remove items from this as needed.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<T, Y>>> fAvailable =
                    new System.Collections.Generic.Queue
                        <System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<T, Y>>>();

        /// <summary>
        ///     the maximum nr of lists that are kept in the buffer when the factory
        ///     gets recycled. This is to prevent all buffers from keeping the max
        ///     amount of lists available.
        /// </summary>
        private readonly int fMaxBufSize = 20;

        #endregion
    }
}