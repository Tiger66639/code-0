// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadLockPartFactory.cs" company="">
//   
// </copyright>
// <summary>
//   a very simple class to recycle <see cref="ThreadLockPart" /> objects.
//   This factory doesn't need to take multi threading into account cause this
//   type of object is only used by the locking mechanisme while a mutex is
//   active, so only 1 thread at a time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a very simple class to recycle <see cref="ThreadLockPart" /> objects.
    ///     This factory doesn't need to take multi threading into account cause this
    ///     type of object is only used by the locking mechanisme while a mutex is
    ///     active, so only 1 thread at a time.
    /// </summary>
    internal class ThreadLockPartFactory
    {
        /// <summary>
        ///     A queue of recycled Clusterlists. We keep a queue per thread. When the
        ///     queue is empty, one is requested from the global list.
        /// </summary>
        private readonly System.Collections.Generic.Queue<ThreadLockPart> fRecycledLists =
            new System.Collections.Generic.Queue<ThreadLockPart>();

        /// <summary>gets a regular neuron.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="ThreadLockPart"/>.</returns>
        public ThreadLockPart Get(ThreadLock owner)
        {
            if (fRecycledLists.Count > 0)
            {
                var iRes = fRecycledLists.Dequeue();
                iRes.Section = owner;
                iRes.Clear();
                return iRes;
            }

            return new ThreadLockPart(owner);
        }

        /// <summary>collects the neuron for recycling.</summary>
        /// <param name="toAdd">To add.</param>
        public void Recycle(ThreadLockPart toAdd)
        {
            toAdd.Section = null;
            fRecycledLists.Enqueue(toAdd);
        }
    }
}