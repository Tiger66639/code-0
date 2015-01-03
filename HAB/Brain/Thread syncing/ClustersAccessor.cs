// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClustersAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   provides specialized access to a <see cref="ClusterList" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides specialized access to a <see cref="ClusterList" /> .
    /// </summary>
    public class ClustersAccessor : IDListAccessor
    {
        /// <summary>Initializes a new instance of the <see cref="ClustersAccessor"/> class.</summary>
        public ClustersAccessor()
        {
            Level = LockLevel.Parents;
        }

        /// <summary>Initializes a new instance of the <see cref="ClustersAccessor"/> class. Initializes a new instance of the <see cref="ClustersAccessor"/>
        ///     class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        public ClustersAccessor(ClusterList list, Neuron neuron, bool writeable)
            : base(list, neuron, LockLevel.Parents, writeable)
        {
        }

        /// <summary>Adds the item directly without raising events in a thread safe way.</summary>
        /// <param name="toAdd">To add.</param>
        internal void AddDirect(NeuronCluster toAdd)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iList = (ClusterList)List;
            iList.InsertDirect(iList.Count, toAdd);
        }

        /// <summary>The remove direct.</summary>
        /// <param name="toRemove">The to remove.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool RemoveDirect(NeuronCluster toRemove)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iList = (ClusterList)List;
            return iList.RemoveDirect(toRemove);
        }

        /// <summary>lock the current neuron and the specified item.</summary>
        /// <param name="item"></param>
        public override void Lock(Neuron item)
        {
            System.Diagnostics.Debug.Assert(fLocks == null);
            if (Neuron.ID == Neuron.TempId)
            {
                // make certain that all temps are added to the cache before locking
                Brain.Current.Add(Neuron);
            }

            if (item.ID == Neuron.TempId)
            {
                // make certain that all temps are added to the cache before locking
                Brain.Current.Add(item);
            }

            fLocks = LockRequestList.Create();
            var iLock = LockRequestInfo.Create(Neuron, Level, IsWriteable);
            fLocks.Add(iLock);
            iLock = LockRequestInfo.Create(item, LockLevel.Children, IsWriteable);
            fLocks.Add(iLock);
            LockManager.Current.RequestLocks(fLocks);
        }

        /// <summary>lock the current neuron and the specified items.</summary>
        /// <param name="items"></param>
        public override void Lock(System.Collections.Generic.IEnumerable<Neuron> items)
        {
            System.Diagnostics.Debug.Assert(fLocks == null);
            if (Neuron.ID == Neuron.TempId)
            {
                // make certain that all temps are added to the cache before locking
                Brain.Current.Add(Neuron);
            }

            fLocks = LockRequestList.Create();
            var iLock = LockRequestInfo.Create(Neuron, Level, IsWriteable);
            fLocks.Add(iLock);
            var iDuplicates = Factories.Default.NHashSets.GetBuffer();
            try
            {
                foreach (var iItem in items)
                {
                    if (iItem.ID == Neuron.TempId)
                    {
                        // make certain that all temps are added to the cache before locking
                        Brain.Current.Add(iItem);
                    }

                    if (iDuplicates.Contains(iItem) == false)
                    {
                        // make certain that we don't add the same lock multiple times, this can cause problems.
                        iLock = LockRequestInfo.Create(iItem, LockLevel.Children, IsWriteable);
                        fLocks.Add(iLock);
                        iDuplicates.Add(iItem);
                    }
                }
            }
            finally
            {
                Factories.Default.NHashSets.Recycle(iDuplicates, false);
            }

            LockManager.Current.RequestLocks(fLocks);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Factories.Default.ClustersAccFactory.Release(this);
        }
    }
}