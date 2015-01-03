// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkInfoAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   An <see cref="Accessor" /> specicific for <see cref="Link.LinkInfo" />
//   data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An <see cref="Accessor" /> specicific for <see cref="Link.LinkInfo" />
    ///     data.
    /// </summary>
    public class LinkInfoAccessor : IDListAccessor
    {
        #region Fields

        /// <summary>The f link.</summary>
        private Link fLink;

        #endregion

        /// <summary>Initializes a new instance of the <see cref="LinkInfoAccessor"/> class. 
        ///     Initializes a new instance of the <see cref="LinkInfoAccessor"/>
        ///     class.</summary>
        public LinkInfoAccessor()
        {
            Level = LockLevel.LinksOut;
        }

        /// <summary>Initializes a new instance of the <see cref="LinkInfoAccessor"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="writeable">The writeable.</param>
        public LinkInfoAccessor(Link owner, bool writeable)
            : base(owner.InfoDirect, owner.From, LockLevel.LinksOut, writeable)
        {
            fLink = owner;
        }

        /// <summary>lock the current neuron and the specified items.</summary>
        /// <param name="items"></param>
        public override void Lock(System.Collections.Generic.IEnumerable<Neuron> items)
        {
            fLocks = LockRequestList.Create();
            var iLock = LockRequestInfo.Create(Neuron, Level, IsWriteable);
            fLocks.Add(iLock);
            var iDuplicates = Factories.Default.NHashSets.GetBuffer();
            try
            {
                foreach (var iItem in items)
                {
                    if (iDuplicates.Contains(iItem) == false)
                    {
                        // make certain that we don't add the same lock multiple times, this can cause problems.
                        iLock = LockRequestInfo.Create(iItem, LockLevel.Value, IsWriteable);
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

        /// <summary>lock the current neuron and the specified item.</summary>
        /// <param name="item"></param>
        public override void Lock(Neuron item)
        {
            fLocks = LockRequestList.Create();
            var iLock = LockRequestInfo.Create(Neuron, Level, IsWriteable);
            fLocks.Add(iLock);
            iLock = LockRequestInfo.Create(item, LockLevel.Value, IsWriteable);
            fLocks.Add(iLock);
            LockManager.Current.RequestLocks(fLocks);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public override void Clear()
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iItems = LockAll();
            try
            {
                ((LinkInfoList)List).Clear(iItems);
            }
            finally
            {
                Unlock(iItems);
                Factories.Default.NLists.Recycle(iItems);
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Factories.Default.LinkInfoAccFactory.Release(this);
        }
    }
}