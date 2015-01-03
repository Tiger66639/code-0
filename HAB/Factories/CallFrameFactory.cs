// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallFrameFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The call frame factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The call frame factory.</summary>
    /// <typeparam name="T"></typeparam>
    internal class CallFrameFactory<T>
        where T : CallFrame, new()
    {
        /// <summary>The maxnrrecycledlists.</summary>
        private const int MAXNRRECYCLEDLISTS = 300;

        /// <summary>
        ///     contains all the available Objects that can be reused. They have
        ///     already been stored in lists that can be dispatched to the different
        ///     threads.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<T>> fAvailable =
            new System.Collections.Generic.Queue<System.Collections.Generic.Queue<T>>();

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<T> fBuffer;

        /// <summary>Gets the call frame.</summary>
        /// <returns>The <see cref="T"/>.</returns>
        public T GetCallFrame()
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
                iRes.Reset();
                return iRes;
            }

            return new T();
        }

        /// <summary>Releases the specified to callframe.</summary>
        /// <param name="toRelease">To release.</param>
        public void ReleaseCallFrame(T toRelease)
        {
            if (fBuffer == null)
            {
                fBuffer = new System.Collections.Generic.Queue<T>();
            }
            else if (fBuffer.Count >= MAXNRRECYCLEDLISTS)
            {
                lock (fAvailable) fAvailable.Enqueue(fBuffer);
                fBuffer = new System.Collections.Generic.Queue<T>();
            }

            fBuffer.Enqueue(toRelease);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        internal void Clear()
        {
            fBuffer = null;
            fAvailable.Clear();
        }
    }
}