// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildrenAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   <see cref="Accessor" /> for the
//   <see cref="JaStDev.HAB.NeuronCluster.Children" /> list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     <see cref="Accessor" /> for the
    ///     <see cref="JaStDev.HAB.NeuronCluster.Children" /> list.
    /// </summary>
    public class ChildrenAccessor : IDListAccessor
    {
        /// <summary>Initializes a new instance of the <see cref="ChildrenAccessor"/> class.</summary>
        public ChildrenAccessor()
        {
            Level = LockLevel.Children;
        }

        /// <summary>Initializes a new instance of the <see cref="ChildrenAccessor"/> class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        public ChildrenAccessor(ChildList list, Neuron neuron, bool writeable)
            : base(list, neuron, LockLevel.Children, writeable)
        {
        }

        /// <summary>The remove direct.</summary>
        /// <param name="item">The item.</param>
        internal void RemoveDirect(Neuron item)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iList = (ChildList)List;
            iList.RemoveDirect(item);
        }

        /// <summary>The insert direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        internal void InsertDirect(int index, Neuron item)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iList = (ChildList)List;
            iList.InsertDirect(index, item);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ((NeuronCluster)Neuron).ClearBufferedChildren();

                // this needs to be done manually cause it isn't done when the lock is released.
        }

        /// <summary>lock the current neuron and the specified item.</summary>
        /// <param name="item"></param>
        public override void Lock(Neuron item)
        {
            System.Diagnostics.Debug.Assert(fLocks == null);
            if (Neuron.ID == Neuron.TempId)
            {
                // Can't add a child to a temp cluster, both need to be registered.
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
            iLock = LockRequestInfo.Create(item, LockLevel.Parents, IsWriteable);
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
                // Can't add a child to a temp cluster, both need to be registered.
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
                        iLock = LockRequestInfo.Create(iItem, LockLevel.Parents, IsWriteable);
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
            Factories.Default.ChildrenAccFactory.Release(this);
        }
    }
}