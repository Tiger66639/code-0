// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterListFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The cluster list factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The cluster list factory.</summary>
    public class ClusterListFactory
    {
        /// <summary>The maxnrrecycledlists.</summary>
        private const int MAXNRRECYCLEDLISTS = 150;

        /// <summary>
        ///     the max nr of items that a recycled list should be able to hold. When bigger, it should be cut to preserve space.
        /// </summary>
        private const int MAXCHILDLISTSIZE = 100;

        /// <summary>
        ///     contains all the available liss that can be reused. They have already been stored in lists that can be dispatched
        ///     to the different threads.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<ClusterList>> fAvailable = new System.Collections.Generic.Queue<System.Collections.Generic.Queue<ClusterList>>();

        /// <summary>
        ///     this list can be used by the GC to collect data. This provides a way for the GC to use the same list all the time,
        ///     even if it is in different threads.
        /// </summary>
        private static System.Collections.Generic.Queue<ClusterList> fGCList =
            new System.Collections.Generic.Queue<ClusterList>();

        /// <summary>
        ///     A queue of recycled Clusterlists. We keep a queue per thread. When the queue is empty, one is requested from the
        ///     global list.
        /// </summary>
        private System.Collections.Generic.Queue<ClusterList> fRecycledLists;

        /// <summary>gets a regular neuron.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="capacity">The capacity.</param>
        /// <returns>The <see cref="ClusterList"/>.</returns>
        public ClusterList GetList(Neuron owner, int capacity = 0)
        {
            if (fRecycledLists == null || fRecycledLists.Count == 0)
            {
                lock (fAvailable)
                {
                    if (fAvailable.Count > 0)
                    {
                        fRecycledLists = fAvailable.Dequeue();
                    }
                }
            }

            if (fRecycledLists != null && fRecycledLists.Count > 0)
            {
                var iRes = fRecycledLists.Dequeue();
                iRes.Owner = owner;
                iRes.List = Factories.Default.IDLists.GetBuffer(capacity);
                return iRes;
            }

            return new ClusterList(owner, capacity);
        }

        /// <summary>collects the neuron for recycling.</summary>
        /// <param name="toAdd">To add.</param>
        /// <param name="fromGC">The from GC.</param>
        public void Recycle(ClusterList toAdd, bool fromGC)
        {
            toAdd.Owner = null;
            if (toAdd.List != null)
            {
                Factories.Default.IDLists.Recycle(toAdd.List, fromGC);
                toAdd.List = null;
            }
 // make certain that the lists are empty.
            if (fromGC == false)
            {
                if (fRecycledLists == null)
                {
                    fRecycledLists = new System.Collections.Generic.Queue<ClusterList>();
                }
                else if (fRecycledLists.Count >= MAXNRRECYCLEDLISTS)
                {
                    lock (fAvailable) fAvailable.Enqueue(fRecycledLists);
                    fRecycledLists = new System.Collections.Generic.Queue<ClusterList>();
                }

                fRecycledLists.Enqueue(toAdd);
            }
            else
            {
                lock (fGCList)
                {
                    fGCList.Enqueue(toAdd);
                    if (fGCList.Count >= MAXNRRECYCLEDLISTS)
                    {
                        lock (fAvailable)
                        {
                            fAvailable.Enqueue(fGCList);
                            fGCList = new System.Collections.Generic.Queue<ClusterList>();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Clears buffered data.
        /// </summary>
        internal void Clear()
        {
            fRecycledLists = null;
            fGCList.Clear();
            fAvailable.Clear();
        }
    }
}