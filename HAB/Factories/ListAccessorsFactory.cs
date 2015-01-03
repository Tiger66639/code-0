// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListAccessorsFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The list accessors factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The list accessors factory.</summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    public class ListAccessorsFactory<T, Y>
        where T : ListAccessor<Y>, new()
    {
        /// <summary>The maxnrrecycledlists.</summary>
        private const int MAXNRRECYCLEDLISTS = 10;

        /// <summary>
        ///     contains all the available Objects that can be reused. They have
        ///     already been stored in lists that can be dispatched to the different
        ///     threads.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<T>> fAvailable =
            new System.Collections.Generic.Queue<System.Collections.Generic.Queue<T>>();

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.Queue<T> fBuffer;

        /// <summary>Initializes a new instance of the <see cref="ListAccessorsFactory{T,Y}"/> class.</summary>
        /// <param name="lockLevel">The lock level.</param>
        public ListAccessorsFactory(LockLevel? lockLevel = null)
        {
            LockLevel = lockLevel;
        }

        #region LockLevel

        /// <summary>
        ///     Gets/sets the default lockLevel that should be assigned to every
        ///     accessor. Can be <see langword="null" /> if the accessor assigns it
        ///     himself. LinkListAccessors need to know if they are for incomming or
        ///     outgoing links.
        /// </summary>
        public LockLevel? LockLevel { get; set; }

        #endregion

        /// <summary>Gets the call frame.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public T Get(System.Collections.Generic.IList<Y> list, Neuron neuron, bool writeable)
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

            T iRes;
            if (fBuffer != null && fBuffer.Count > 0)
            {
                iRes = fBuffer.Dequeue();
            }
            else
            {
                iRes = new T();
            }

            iRes.List = list;
            iRes.Neuron = neuron;
            if (LockLevel.HasValue)
            {
                iRes.Level = LockLevel.Value;
            }

            iRes.IsWriteable = writeable;
            return iRes;
        }

        /// <summary>Releases the specified to callframe.</summary>
        /// <param name="toRelease">To release.</param>
        public void Release(T toRelease)
        {
            if (fBuffer == null)
            {
                fBuffer = new System.Collections.Generic.Queue<T>(MAXNRRECYCLEDLISTS);
            }
            else if (fBuffer.Count >= MAXNRRECYCLEDLISTS)
            {
                lock (fAvailable) fAvailable.Enqueue(fBuffer);
                fBuffer = new System.Collections.Generic.Queue<T>(MAXNRRECYCLEDLISTS);
            }

            fBuffer.Enqueue(toRelease);
        }

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            fBuffer = null;
            fAvailable.Clear();
        }
    }
}