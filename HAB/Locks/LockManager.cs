// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockManager.cs" company="">
//   
// </copyright>
// <summary>
//   Enum that specifies the different levels of locking that can be requested
//   for a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Enum that specifies the different levels of locking that can be requested
    ///     for a neuron.
    /// </summary>
    public enum LockLevel
    {
        /// <summary>
        ///     Nothing on the neuron is locked.
        /// </summary>
        None, 

        /// <summary>
        ///     The list of incomming links.
        /// </summary>
        LinksIn, 

        /// <summary>
        ///     The list of outgoing links.
        /// </summary>
        LinksOut, 

        /// <summary>
        ///     The children list of a neuron.
        /// </summary>
        Children, 

        /// <summary>
        ///     The ClusteredBy list of a neuron.
        /// </summary>
        Parents, 

        /// <summary>
        ///     for value neurons and the meaning of a cluster.
        /// </summary>
        Value, 

        /// <summary>
        ///     the set of processors to which the neuron is attached.
        /// </summary>
        Processors, 

        /// <summary>
        ///     a special level that is usualy automtically added to a lockList, when
        ///     the cache also needs to be locked for the operation.
        /// </summary>
        Cache, 

        /// <summary>
        ///     when the entire neuron needs to be/is locked.
        /// </summary>
        All
    }

    /// <summary>
    ///     A manager class for all the locks on the neurons to make everything
    ///     thread safe.
    /// </summary>
    public class LockManager
    {
        #region ctor

        /// <summary>Prevents a default instance of the <see cref="LockManager"/> class from being created. 
        ///     Initializes a new instance of the <see cref="LockManager"/> class.
        ///     Private so that it can' be created and everyone needs to use<see cref="JaStDev.HAB.LockManager.Current"/> .</summary>
        private LockManager()
        {
            for (var i = 0; i < Settings.NrCacheTracks; i++)
            {
                fCachelock[i] = ThreadLock.Create();
            }
        }

        #endregion

        #region Prop

        #region Current

        /// <summary>
        ///     Gets the lockmanager object to use.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public static LockManager Current
        {
            get
            {
                return fCurrent;
            }
        }

        #endregion

        #endregion

        /// <summary>
        ///     Locks the entire lockManager. Any requests done when the lockManager
        ///     is locked, result in an exception. This is to allow for 'Flush' write
        ///     operations.
        /// </summary>
        internal void LockAll()
        {
            fLocked = true;
        }

        /// <summary>
        ///     Releases the lock on the entire LockManager.
        /// </summary>
        internal void ReleaseLockAll()
        {
            fLocked = false;
        }

        /// <summary>
        ///     Releases all the locks that are still open. Warning: only use when
        ///     something went wrong.
        /// </summary>
        internal void ReleaseAllLocks()
        {
            fLocked = true; // lock the entire lock manager, so we can clean stuff out.
            lock (fLock)
            {
                fLinksIn.Clear();
                fLinksOut.Clear();
                fParents.Clear();
                fChildren.Clear();
                fValue.Clear();
                fProcessors.Clear();
                fLinkInfos.Clear();
            }

            fLocked = false;
            lock (fStoragelocks) fStoragelocks.Clear();
        }

        /// <summary>The dump.</summary>
        internal void Dump()
        {
            System.Diagnostics.Debug.WriteLine("Links in");
            DumpDict(fLinksIn);
            System.Diagnostics.Debug.WriteLine("Links out");
            DumpDict(fLinksOut);
            System.Diagnostics.Debug.WriteLine("Values");
            DumpDict(fValue);
            System.Diagnostics.Debug.WriteLine("Parent");
            DumpDict(fParents);
            System.Diagnostics.Debug.WriteLine("Children");
            DumpDict(fChildren);
            System.Diagnostics.Debug.WriteLine("Processors");
            DumpDict(fProcessors);
            System.Diagnostics.Debug.WriteLine("Storage locks");
            DumpDict(fStoragelocks);
        }

        /// <summary>The dump save.</summary>
        internal void DumpSave()
        {
            lock (fLock) Dump();
        }

        /// <summary>The dump dict.</summary>
        /// <param name="dict">The dict.</param>
        private void DumpDict(System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict)
        {
            foreach (var i in dict)
            {
                System.Diagnostics.Debug.WriteLine(
                    "ID: {0} ReadCount: {1},  WriteCount: {2}, Waiting: {3}", 
                    i.Key.ID, 
                    i.Value.ReadCount, 
                    i.Value.WriteCount, 
                    i.Value.TaskCount);
            }
        }

        /// <summary>The dump dict.</summary>
        /// <param name="dict">The dict.</param>
        private void DumpDict(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>> dict)
        {
            foreach (var i in dict)
            {
                System.Diagnostics.Debug.WriteLine("ID: {0} Count: {1}", i.Key, i.Value.Count);
            }
        }

        #region Fields

        /// <summary>The f current.</summary>
        private static readonly LockManager fCurrent = new LockManager();

        /// <summary>The f links in.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fLinksIn =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f links out.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fLinksOut =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f parents.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fParents =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f children.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fChildren =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f value.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fValue =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f processors.</summary>
        private readonly System.Collections.Generic.Dictionary<Neuron, ThreadLock> fProcessors =
            new System.Collections.Generic.Dictionary<Neuron, ThreadLock>();

        /// <summary>The f storagelocks.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>> fStoragelocks =
                    new System.Collections.Generic.Dictionary
                        <ulong, System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>>();

        /// <summary>The f link infos.</summary>
        private readonly System.Collections.Generic.Dictionary<LinkInfoList, ThreadLock> fLinkInfos =
            new System.Collections.Generic.Dictionary<LinkInfoList, ThreadLock>();

        /// <summary>The f lock.</summary>
        private readonly object fLock = new object();

        /// <summary>The f locked.</summary>
        private volatile bool fLocked;

                              // when true, the entire lockManager is locked, any requests for locks are illegal at this point. 

        /// <summary>The f cachelock.</summary>
        private ThreadLock[] fCachelock = new ThreadLock[Settings.NrCacheTracks];

        // ThreadLock fCachelock = new ThreadLock();
        /// <summary>The f reset event queue factory.</summary>
        private readonly System.Collections.Generic.Queue<System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>> fResetEventQueueFactory =
                new System.Collections.Generic.Queue
                    <System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>>(); // the factory 

        #endregion

        #region functions

        #region mem manamgent

        /// <summary>get a queue</summary>
        /// <returns>The <see cref="Queue"/>.</returns>
        private System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim> GetResetEventQueue()
        {
            if (fResetEventQueueFactory.Count > 0)
            {
                var iRes = fResetEventQueueFactory.Dequeue();
                iRes.Clear();
                return iRes;
            }

            return new System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim>();
        }

        /// <summary>recycle the queue</summary>
        /// <param name="toRelease"></param>
        private void Recycle(System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim> toRelease)
        {
            fResetEventQueueFactory.Enqueue(toRelease);
        }

        #endregion

        /// <summary>
        ///     Makes certain that the cachelock reflects the same nr of locks as in
        ///     the settings. This is called whenever the brain gets cleared. Dont'
        ///     call this during the lifetime of a project cause it can screw things
        ///     up.
        /// </summary>
        internal void RebuildCacheLock()
        {
            fCachelock = new ThreadLock[Settings.NrCacheTracks];
            for (var i = 0; i < Settings.NrCacheTracks; i++)
            {
                fCachelock[i] = ThreadLock.Create();
            }
        }

        #region RequestLock

        /// <summary>Requests a lock for the specified neuron, <paramref name="level"/> and
        ///     wether it needs to be writeable.</summary>
        /// <remarks>Internally, it will create the lock, if it doesn't exist yet, prepare
        ///     it for this processor</remarks>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        public void RequestLock(Neuron neuron, LockLevel level, bool writeable)
        {
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            if (Neuron.IsEmpty(neuron.ID))
            {
                // don't try to lock temps.
                return;
            }

            var iRes = WaitObject.Create();
            lock (fLock)
            {
                switch (level)
                {
                    case LockLevel.None:
                        break;
                    case LockLevel.LinksIn:
                        internalRequestLockFor(neuron, fLinksIn, writeable, iRes);
                        break;
                    case LockLevel.LinksOut:
                        internalRequestLockFor(neuron, fLinksOut, writeable, iRes);
                        break;
                    case LockLevel.Children:
                        internalRequestLockFor(neuron, fChildren, writeable, iRes);
                        break;
                    case LockLevel.Parents:
                        internalRequestLockFor(neuron, fParents, writeable, iRes);
                        break;
                    case LockLevel.Value:
                        internalRequestLockFor(neuron, fValue, writeable, iRes);
                        break;
                    case LockLevel.Processors:
                        internalRequestLockFor(neuron, fProcessors, writeable, iRes);
                        break;
                    case LockLevel.All:
                        InternalCreateCompleteLock(neuron, writeable, iRes);
                        break;
                    default:
                        throw new System.InvalidOperationException("Unkown level requested");
                }
            }

            iRes.Wait();
            WaitObject.Recycle(iRes);
        }

        /// <summary>Requests a lock for the specified neuron, <paramref name="level"/> and
        ///     wether it needs to be writeable. If the <paramref name="neuron"/> is
        ///     being deleted (but not yet completely), the lock request wont wait but
        ///     will fail immediatly. This is a thread save way for trying to lock
        ///     something that could cause a deadlock in case the other item is being
        ///     deleted.</summary>
        /// <remarks>Internally, it will create the lock, if it doesn't exist yet, prepare
        ///     it for this processor</remarks>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool TryRequestLock(Neuron neuron, LockLevel level, bool writeable)
        {
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            var iRes = WaitObject.Create();
            lock (fLock)
            {
                if (neuron.IsDeleted)
                {
                    return false;
                }

                switch (level)
                {
                    case LockLevel.None:
                        break;
                    case LockLevel.LinksIn:
                        internalRequestLockFor(neuron, fLinksIn, writeable, iRes);
                        break;
                    case LockLevel.LinksOut:
                        internalRequestLockFor(neuron, fLinksOut, writeable, iRes);
                        break;
                    case LockLevel.Children:
                        internalRequestLockFor(neuron, fChildren, writeable, iRes);
                        break;
                    case LockLevel.Parents:
                        internalRequestLockFor(neuron, fParents, writeable, iRes);
                        break;
                    case LockLevel.Value:
                        internalRequestLockFor(neuron, fValue, writeable, iRes);
                        break;
                    case LockLevel.Processors:
                        internalRequestLockFor(neuron, fProcessors, writeable, iRes);
                        break;
                    case LockLevel.All:
                        InternalCreateCompleteLock(neuron, writeable, iRes);
                        break;
                    default:
                        throw new System.InvalidOperationException("Unkown level requested");
                }
            }

            iRes.Wait();
            WaitObject.Recycle(iRes);
            return true;
        }

        /// <summary>Requests a lock for multiple neurons at the same time. This allows you
        ///     to lock multiple neurons, used in a single operations, at once, in 1
        ///     thread safe call.</summary>
        /// <param name="requests">The request info: each record contains the info for 1 neuron. Upon
        ///     returning, each <see cref="LockRequestInfo"/> item will contain the
        ///     key that can later be used for unlocking or upgrading.</param>
        public void RequestLocks(LockRequestList requests)
        {
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            var iRes = WaitObject.Create();
            lock (fLock)
            {
                foreach (var i in requests)
                {
                    if (i.Level == LockLevel.All)
                    {
                        // we need to do this outside the switch, cause the switch is only for functions that return a single lock, when we lock all,we get multiple lock objects back.
                        InternalCreateCompleteLock(i.Neuron, i.Writeable, iRes);
                    }
                    else if (i.Level == LockLevel.Cache)
                    {
                        throw new System.InvalidOperationException(
                            "can't request cachelock together with neurons lock, this can cause deadlocks.");
                    }
                    else
                    {
                        ProcessLockRequest(i, iRes);
                    }
                }
            }

            iRes.Wait();
            WaitObject.Recycle(iRes);
        }

        /// <summary>The process lock request.</summary>
        /// <param name="i">The i.</param>
        /// <param name="handle">The handle.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ProcessLockRequest(LockRequestInfo i, WaitObject handle)
        {
            switch (i.Level)
            {
                case LockLevel.None:
                    break;
                case LockLevel.LinksIn:
                    internalRequestLockFor(i.Neuron, fLinksIn, i.Writeable, handle);
                    break;
                case LockLevel.LinksOut:
                    internalRequestLockFor(i.Neuron, fLinksOut, i.Writeable, handle);
                    break;
                case LockLevel.Children:
                    internalRequestLockFor(i.Neuron, fChildren, i.Writeable, handle);
                    break;
                case LockLevel.Parents:
                    internalRequestLockFor(i.Neuron, fParents, i.Writeable, handle);
                    break;
                case LockLevel.Processors:
                    internalRequestLockFor(i.Neuron, fProcessors, i.Writeable, handle);
                    break;
                case LockLevel.Value:
                    internalRequestLockFor(i.Neuron, fValue, i.Writeable, handle);
                    break;

                // no need for LockLevel.Cache: this is handled by REquestLocks
                default:
                    throw new System.InvalidOperationException("Unkown level requested");
            }
        }

        ///// <summary>
        ///// Requests a lock for a <see cref="LinkInfoList"/>, which is not connected with a specific
        ///// neuron.
        ///// </summary>
        ///// <param name="list">The list.</param>
        ///// <param name="writeable">if set to <c>true</c> [writeable].</param>
        ///// <returns></returns>
        // public void RequestLock(LinkInfoList list, bool writeable)
        // {
        // if (fLocked == true)
        // throw new InvalidOperationException("No lock can be acquired during a general lockdown.");
        // WaitObject iHandle = WaitObject.Create();
        // ThreadLock iFound;
        // lock(fLock)
        // {
        // if (fLinkInfos.TryGetValue(list, out iFound) == false)
        // {
        // iFound = ThreadLock.Create();                                //don't init writeable, if this is true, the merge will also create a new record, which we don't want.
        // fLinkInfos.Add(list, iFound);
        // }
        // if (writeable == true)
        // iFound.RequestWrite(iHandle);
        // else
        // iFound.RequestRead(iHandle);
        // }
        // iHandle.Wait();
        // WaitObject.Recycle(iHandle);
        // }

        /// <summary>Requests a lock for the neuron cache.</summary>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        public void RequestCacheLock(bool writeable)
        {
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            var iWaitHandle = WaitObject.Create();
            foreach (var i in fCachelock)
            {
                lock (i)
                {
                    // lock the entire object so no other thread can change it while performing the action.
                    if (writeable)
                    {
                        i.RequestWrite(iWaitHandle);
                    }
                    else
                    {
                        i.RequestRead(iWaitHandle);
                    }
                }
            }

            iWaitHandle.Wait();
            WaitObject.Recycle(iWaitHandle);
        }

        /// <summary>Requests a lock for a single channel in the neuron cache.</summary>
        /// <param name="id">The id of the neuron (used to calclate the channel.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        public void RequestCacheLock(ulong id, bool writeable)
        {
            var iChannel = (int)(id % (ulong)fCachelock.Length);
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            var iWaitHandle = WaitObject.Create();
            lock (fCachelock[iChannel])
            {
                // lock the entire object so no other thread can change it while performing the action.
                if (writeable)
                {
                    fCachelock[iChannel].RequestWrite(iWaitHandle);
                }
                else
                {
                    fCachelock[iChannel].RequestRead(iWaitHandle);
                }
            }

            iWaitHandle.Wait();
            WaitObject.Recycle(iWaitHandle);
        }

        /// <summary>Requests the storage lock so that only 1 thread can read the same
        ///     neuron from disk, through the id.</summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if the lock was successul (there was no previous lock).</returns>
        public bool RequestStorageReadLock(ulong id)
        {
            System.Threading.ManualResetEventSlim iHandle;
            System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim> iList;
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            lock (fStoragelocks)
            {
                if (fStoragelocks.TryGetValue(id, out iList) == false)
                {
                    iList = GetResetEventQueue();
                    iHandle = WaitHandleManager.Current.GetWaitHandle();
                    iList.Enqueue(iHandle);
                    fStoragelocks.Add(id, iList);
                    return true;
                }

                iHandle = iList.Peek();
                iList.Enqueue(WaitHandleManager.Current.GetWaitHandle());

                    // we also need to release something when we are done.
            }

            if (iHandle != null)
            {
                // make certain this is outside of the lock, otherwise we can have a deadlock.
                iHandle.Wait();
            }

            return false; // when we get here, something went wrong.
        }

        #endregion

        #region upgrade lock

        /// <summary>Upgrades the lock for the current processor and neuron, so that it is
        ///     possible to write to it.</summary>
        /// <remarks>We check if the lock is currently only used by 1 processor, if so,
        ///     simply allow for writes. If there are multiple processors still
        ///     reading the neuron, remove the current processor from the list and
        ///     create a new key, which is inserted just after the curent one, so it
        ///     becomes the next one to let run, which will block the current
        ///     processor untill all others are done reading. When this happens, a new
        ///     key is returned after the lock has been acquired.</remarks>
        /// <param name="neuron">The neuron to lock</param>
        /// <param name="level">The level.</param>
        /// <exception cref="System.InvalidOperationException">If the key is not for the first in the list of requested locks</exception>
        /// <exception cref="System.InvalidOperationException">No lock found for the specified neuron.</exception>
        public void UpgradeLockForWriting(Neuron neuron, LockLevel level)
        {
            WaitObject iHandle = null;
            switch (level)
            {
                case LockLevel.None:
                    return;
                case LockLevel.LinksIn:
                    UpgradeToWritingFor(neuron, fLinksIn, iHandle);
                    break;
                case LockLevel.LinksOut:
                    UpgradeToWritingFor(neuron, fLinksOut, iHandle);
                    break;
                case LockLevel.Children:
                    UpgradeToWritingFor(neuron, fChildren, iHandle);
                    break;
                case LockLevel.Parents:
                    UpgradeToWritingFor(neuron, fParents, iHandle);
                    break;
                case LockLevel.Processors:
                    UpgradeToWritingFor(neuron, fProcessors, iHandle);
                    break;
                case LockLevel.Value:
                    UpgradeToWritingFor(neuron, fValue, iHandle);
                    break;
                case LockLevel.All:
                    UpgradeCompleteLock(neuron);
                    break;
                default:
                    throw new System.InvalidOperationException("Unkown level requested");
            }

            iHandle.Wait();
        }

        /// <summary>upgraades the lock with the specified key for the specified list.</summary>
        /// <param name="list">The list.</param>
        internal void UpgradeLockForWriting(LinkInfoList list)
        {
            var iHandle = WaitObject.Create();
            lock (fLock)
            {
                ThreadLock iFound = null;
                if (fLinkInfos.TryGetValue(list, out iFound))
                {
                    iFound.UpgradeForWriting(iHandle);
                }
                else
                {
                    throw new System.InvalidOperationException("No lock found for the specified list.");
                }
            }

            iHandle.Wait();
            WaitObject.Recycle(iHandle);
        }

        /// <summary>Upgrades a list of locks for the current processor at the same time in
        ///     a thread safe way, so that it is possible to write to them.</summary>
        /// <remarks>For more info, see<see cref="LockManager.UpgradeLockForWriting(JaStDev.HAB.Neuron,JaStDev.HAB.LockLevel)"/></remarks>
        /// <param name="requests">The requests.</param>
        public void UpgradeLocksForWriting(System.Collections.Generic.IEnumerable<LockRequestInfo> requests)
        {
            var iHandle = WaitObject.Create();
            lock (fLock)
            {
                foreach (var i in requests)
                {
                    if (i.Level == LockLevel.All)
                    {
                        // we need to do this outside the switch, cause the switch is only for functions that return a single lock, when we lock all,we get multiple lock objects back.
                        InternalUpgradeCompleteLock(i.Neuron, iHandle);
                    }
                    else
                    {
                        switch (i.Level)
                        {
                            case LockLevel.None:
                                break;
                            case LockLevel.LinksIn:
                                InternalUpgradeToWritingFor(i.Neuron, fLinksIn, iHandle);
                                break;
                            case LockLevel.LinksOut:
                                InternalUpgradeToWritingFor(i.Neuron, fLinksOut, iHandle);
                                break;
                            case LockLevel.Children:
                                InternalUpgradeToWritingFor(i.Neuron, fChildren, iHandle);
                                break;
                            case LockLevel.Parents:
                                InternalUpgradeToWritingFor(i.Neuron, fParents, iHandle);
                                break;
                            case LockLevel.Processors:
                                InternalUpgradeToWritingFor(i.Neuron, fProcessors, iHandle);
                                break;
                            case LockLevel.Value:
                                InternalUpgradeToWritingFor(i.Neuron, fValue, iHandle);
                                break;
                            default:
                                throw new System.InvalidOperationException("Unkown level requested");
                        }
                    }
                }
            }

            iHandle.Wait();
            WaitObject.Recycle(iHandle);
        }

        /// <summary>The upgrade cache lock.</summary>
        /// <param name="id">The id.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpgradeCacheLock(ulong id)
        {
            var iChannel = (int)(id % (ulong)fCachelock.Length);
            if (fLocked)
            {
                throw new System.InvalidOperationException("No lock can be acquired during a general lockdown.");
            }

            var iWaitHandle = WaitObject.Create();
            lock (fCachelock[iChannel]) fCachelock[iChannel].UpgradeForWriting(iWaitHandle);
            iWaitHandle.Wait();
            WaitObject.Recycle(iWaitHandle);
        }

        #endregion

        #region Release lock

        /// <summary>Releases the lock with the specified key for the specified neuron. If
        ///     there are any other locks for the neuron, they are activated now.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        /// <param name="cacheAlso">The cache Also.</param>
        public void ReleaseLock(Neuron neuron, LockLevel level, bool writeable, bool cacheAlso)
        {
            if (Neuron.IsEmpty(neuron.ID))
            {
                // don't try to lock temps.
                return;
            }

            switch (level)
            {
                case LockLevel.None:
                    break;
                case LockLevel.LinksIn:
                    ReleaseLockFor(neuron, fLinksIn, writeable);
                    break;
                case LockLevel.LinksOut:
                    ReleaseLockFor(neuron, fLinksOut, writeable);
                    break;
                case LockLevel.Children:
                    ReleaseLockFor(neuron, fChildren, writeable);
                    break;
                case LockLevel.Parents:
                    ReleaseLockFor(neuron, fParents, writeable);
                    break;
                case LockLevel.Processors:
                    ReleaseLockFor(neuron, fProcessors, writeable);
                    break;
                case LockLevel.Value:
                    ReleaseLockFor(neuron, fValue, writeable);
                    break;
                case LockLevel.All:
                    ReleaseCompleteLock(neuron, writeable);
                    break;
                default:
                    break;
            }

            if (cacheAlso)
            {
                ReleaseCacheLock(writeable); // in this type of situation, the entire cache was locked out of precaution.
            }
        }

        /// <summary>Releases the lock with the specified key for the specified neuron. If
        ///     there are any other locks for the neuron, they are activated now.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        public void ReleaseLock(Neuron neuron, LockLevel level, bool writeable)
        {
            if (Neuron.IsEmpty(neuron.ID))
            {
                // don't try to lock temps.
                return;
            }

            switch (level)
            {
                case LockLevel.None:
                    break;
                case LockLevel.LinksIn:
                    ReleaseLockFor(neuron, fLinksIn, writeable);
                    break;
                case LockLevel.LinksOut:
                    ReleaseLockFor(neuron, fLinksOut, writeable);
                    break;
                case LockLevel.Children:
                    ReleaseLockFor(neuron, fChildren, writeable);
                    break;
                case LockLevel.Parents:
                    ReleaseLockFor(neuron, fParents, writeable);
                    break;
                case LockLevel.Processors:
                    ReleaseLockFor(neuron, fProcessors, writeable);
                    break;
                case LockLevel.Value:
                    ReleaseLockFor(neuron, fValue, writeable);
                    break;
                case LockLevel.All:
                    ReleaseCompleteLock(neuron, writeable);
                    break;
                default:
                    break;
            }
        }

        /// <summary>Releases all the locks, stored in the specified<see cref="LockRequestInfo"/> objects together in a single thread
        ///     safe action.</summary>
        /// <param name="requests">The requests.</param>
        /// <param name="asBig">The as Big.</param>
        public void ReleaseLocks(LockRequestList requests, bool asBig = false)
        {
            var iProc = Processor.CurrentProcessor;
            lock (fLock)
            {
                foreach (var i in requests)
                {
                    switch (i.Level)
                    {
                        case LockLevel.None:
                            break;
                        case LockLevel.LinksIn:
                            InternalReleaseLockFor(i.Neuron, fLinksIn, i.Writeable);
                            break;
                        case LockLevel.LinksOut:
                            InternalReleaseLockFor(i.Neuron, fLinksOut, i.Writeable);
                            break;
                        case LockLevel.Children:
                            InternalReleaseLockFor(i.Neuron, fChildren, i.Writeable);
                            break;
                        case LockLevel.Parents:
                            InternalReleaseLockFor(i.Neuron, fParents, i.Writeable);
                            break;
                        case LockLevel.Processors:
                            InternalReleaseLockFor(i.Neuron, fProcessors, i.Writeable);
                            break;
                        case LockLevel.Value:
                            InternalReleaseLockFor(i.Neuron, fValue, i.Writeable);
                            break;
                        case LockLevel.Cache:
                            throw new System.InvalidOperationException();
                        case LockLevel.All:
                            InternalReleaseCompleteLock(i.Neuron, i.Writeable);
                            break;
                        default:
                            break;
                    }

                    if (iProc != null)
                    {
                        iProc.Mem.LocksFactory.ReleaseLock(i);
                    }
                }

                if (iProc != null)
                {
                    if (asBig)
                    {
                        iProc.Mem.LocksFactory.ReleaseBigList(requests);
                    }
                    else
                    {
                        iProc.Mem.LocksFactory.ReleaseList(requests);
                    }
                }
            }
        }

        /// <summary>Releases the entire cache lock.</summary>
        /// <param name="writeable">if set to <c>true</c> , release a writeable lock, otherwise a read
        ///     lock.</param>
        public void ReleaseCacheLock(bool writeable)
        {
            foreach (var i in fCachelock)
            {
                lock (i)
                {
                    // lock the entire object so no other thread can change it while performing the action.
                    if (writeable)
                    {
                        i.ReleaseWrite();
                    }
                    else
                    {
                        i.ReleaseRead();
                    }
                }
            }
        }

        /// <summary>Releases 1 channel in the cache lock.</summary>
        /// <param name="id">The id.</param>
        /// <param name="writeable">if set to <c>true</c> , release a writeable lock, otherwise a read
        ///     lock.</param>
        public void ReleaseCacheLock(ulong id, bool writeable)
        {
            var iChannel = (int)(id % (ulong)fCachelock.Length);
            lock (fCachelock[iChannel])
            {
                // lock the entire object so no other thread can change it while performing the action.
                if (writeable)
                {
                    fCachelock[iChannel].ReleaseWrite();
                }
                else
                {
                    fCachelock[iChannel].ReleaseRead();
                }
            }
        }

        /// <summary>Releases the storage lock, so that only 1 thread is accessing the same
        ///     neuron data from storage.</summary>
        /// <param name="id">The id.</param>
        public void ReleaseStorageReadLock(ulong id)
        {
            System.Threading.ManualResetEventSlim iFound;
            lock (fStoragelocks)
            {
                System.Collections.Generic.Queue<System.Threading.ManualResetEventSlim> iList;
                if (fStoragelocks.TryGetValue(id, out iList))
                {
                    iFound = iList.Dequeue();
                    if (iList.Count == 0)
                    {
                        fStoragelocks.Remove(id);
                        Recycle(iList);
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("No lock found for the specified id.");
                }
            }

            iFound.Set();
            WaitHandleManager.Current.ReleaseWaitHandle(iFound);

                // don't need to reset the handle, this is done when it get reused again, but the GetWaitHandle.
        }

        ///// <summary>
        ///// Releases the lock with the specified key for the specified neuron. If there are any other locks
        ///// for the neuron, they are activated now.
        ///// </summary>
        ///// <param name="list">The list.</param>
        ///// <param name="key">The key.</param>
        // internal void ReleaseLock(LinkInfoList list, bool writeable)
        // {
        // ThreadLock iFound = null;
        // lock(fLock)
        // {
        // if (fLinkInfos.TryGetValue(list, out iFound) == false)
        // throw new InvalidOperationException("No lock found for the specified neuron.");
        // if (iFound != null)
        // {
        // if (writeable == true)
        // iFound.ReleaseWrite();
        // else
        // iFound.ReleaseRead();
        // if (iFound.State == ThreadLockState.Dormant)
        // {
        // fLinkInfos.Remove(list);
        // iFound.Recycle();
        // }
        // }
        // }
        // }
        #endregion

        #region Downgrade

        /// <summary>Downgrades the lock back to reading mode.</summary>
        /// <param name="list">The list.</param>
        internal void DowngradeLock(LinkInfoList list)
        {
            lock (fLock)
            {
                ThreadLock iFound = null;
                if (fLinkInfos.TryGetValue(list, out iFound))
                {
                    iFound.Downgrade();
                }
                else
                {
                    throw new System.InvalidOperationException("No lock found for the specified list.");
                }
            }
        }

        /// <summary>The downgrade lock.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void DowngradeLock(Neuron neuron, LockLevel level)
        {
            System.Collections.Generic.Dictionary<Neuron, ThreadLock> iDict = null;
            lock (fLock)
            {
                switch (level)
                {
                    case LockLevel.None:
                        return;
                    case LockLevel.LinksIn:
                        iDict = fLinksIn;
                        break;
                    case LockLevel.LinksOut:
                        iDict = fLinksOut;
                        break;
                    case LockLevel.Children:
                        iDict = fChildren;
                        break;
                    case LockLevel.Parents:
                        iDict = fParents;
                        break;
                    case LockLevel.Processors:
                        iDict = fProcessors;
                        break;
                    case LockLevel.Value:
                        iDict = fValue;
                        break;
                    case LockLevel.All:
                        iDict = null;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unkown level requested");
                }

                if (iDict != null)
                {
                    InternalDowngradeFor(neuron, iDict);
                }
                else
                {
                    InternalDowngradeCompleteLock(neuron);
                }
            }
        }

        /// <summary>Downgrades the cache lock back to readable, so that other threads can
        ///     again also access the cache for reading.</summary>
        /// <param name="id">The id.</param>
        internal void DowngradeCacheLock(ulong id)
        {
            var iChannel = (int)(id % (ulong)fCachelock.Length);
            lock (fCachelock[iChannel])

                // lock the entire object so no other thread can change it while performing the action.
                fCachelock[iChannel].Downgrade();
        }

        #endregion

        #endregion

        #region Helpers

        #region Release lock

        /// <summary>Releases the lock on every part of the neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        private void ReleaseCompleteLock(Neuron neuron, bool writeable)
        {
            lock (fLock) InternalReleaseCompleteLock(neuron, writeable);
        }

        /// <summary>Releases the lock on every part of the <paramref name="neuron"/> in a
        ///     thread unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        private void InternalReleaseCompleteLock(Neuron neuron, bool writeable)
        {
            InternalReleaseLockFor(neuron, fParents, writeable);
            InternalReleaseLockFor(neuron, fLinksIn, writeable);
            InternalReleaseLockFor(neuron, fLinksOut, writeable);

            /*
          * Don't lock the processor section cause that can cause deadlocks.
          * ex, during a link created: 1.the output list gets updated, 
          *                            2. another thread asks a complete lock, needs to wait for the links out, but gets processors since not yet locked.
          *                            3. after the output list update, an unfreeze is done, which requieres the processors lis. -> deadlock
          * not locking this list is ok, since it is only used AFTER a linkin,out, child, parent  or value. All of these others are locked.
          */
            // InternalReleaseLockFor(neuron, fProcessors, writeable);
            if (neuron is NeuronCluster)
            {
                InternalReleaseLockFor(neuron, fChildren, writeable);
            }

            InternalReleaseLockFor(neuron, fValue, writeable);
        }

        /// <summary>Releases the lock for a specific list.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The f links in.</param>
        /// <param name="writeable">The writeable.</param>
        private void ReleaseLockFor(
            Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict, 
            bool writeable)
        {
            lock (fLock) InternalReleaseLockFor(neuron, dict, writeable);
        }

        /// <summary>Releases the lock for a specific list in a thread unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The dict.</param>
        /// <param name="writeable">The writeable.</param>
        private void InternalReleaseLockFor(
            Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict, 
            bool writeable)
        {
            ThreadLock iFound;
            if (dict.TryGetValue(neuron, out iFound))
            {
                if (writeable)
                {
                    iFound.ReleaseWrite();
                }
                else
                {
                    iFound.ReleaseRead();
                }

                if (iFound.State == ThreadLockState.Dormant)
                {
                    // if, after a release, the threadlock turned dormand, remove the reference so that we free everything up.
                    dict.Remove(neuron);
                    iFound.Recycle();
                }
            }
            else
            {
                throw new System.InvalidOperationException("No lock found for the specified neuron.");
            }
        }

        #endregion

        #region RequestLock

        /// <summary>Creates a new ore reuses an already existing lock for a single list in
        ///     a semi thread unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The dict.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <param name="handle">The handle.</param>
        private void internalRequestLockFor(
            Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict, 
            bool writeable, 
            WaitObject handle)
        {
            ThreadLock iFound;
            if (dict.TryGetValue(neuron, out iFound) == false)
            {
                iFound = ThreadLock.Create();

                    // don't init the writeable, if this is true, we get a false result cause the merge would fail
                dict.Add(neuron, iFound);
            }

            if (writeable)
            {
                iFound.RequestWrite(handle);
            }
            else
            {
                iFound.RequestRead(handle);
            }
        }

        /// <summary>The internal create complete lock.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="writeable">The writeable.</param>
        /// <param name="handle">The handle.</param>
        private void InternalCreateCompleteLock(Neuron neuron, bool writeable, WaitObject handle)
        {
            internalRequestLockFor(neuron, fParents, writeable, handle);
            internalRequestLockFor(neuron, fLinksIn, writeable, handle);
            internalRequestLockFor(neuron, fLinksOut, writeable, handle);

            /*
          * Don't lock the processor section cause that can cause deadlocks.
          * ex, during a link created: 1.the output list gets updated, 
          *                            2. another thread asks a complete lock, needs to wait for the links out, but gets processors since not yet locked.
          *                            3. after the output list update, an unfreeze is done, which requieres the processors lis. -> deadlock
          * not locking this list is ok, since it is only used AFTER a linkin,out, child, parent  or value. All of these others are locked.
          */
            // iTemp = internalRequestLockFor(neuron, fProcessors, writeable);
            // if (iTemp != null) iRes.Add(iTemp);
            if (neuron is NeuronCluster)
            {
                internalRequestLockFor(neuron, fChildren, writeable, handle);
            }

            internalRequestLockFor(neuron, fValue, writeable, handle);

                // the value lock needs to be requested for every item, since this is also used to lock changes to 'meaningcount' and similar, which are in neuron.
        }

        #endregion

        #region Upgrade

        /// <summary>Upgrades a lock that wraps the whole neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        private void UpgradeCompleteLock(Neuron neuron)
        {
            var iToWaitFor = WaitObject.Create();
            lock (fLock) InternalUpgradeCompleteLock(neuron, iToWaitFor);
            iToWaitFor.Wait();
            WaitObject.Recycle(iToWaitFor);
        }

        /// <summary>Upgrades a lock that wraps the whole <paramref name="neuron"/> in a
        ///     thread unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="handle">The handle.</param>
        private void InternalUpgradeCompleteLock(Neuron neuron, WaitObject handle)
        {
            InternalUpgradeToWritingFor(neuron, fParents, handle);
            InternalUpgradeToWritingFor(neuron, fLinksIn, handle);
            InternalUpgradeToWritingFor(neuron, fLinksOut, handle);

            /*
          * Don't lock the processor section cause that can cause deadlocks.
          * ex, during a link created: 1.the output list gets updated, 
          *                            2. another thread asks a complete lock, needs to wait for the links out, but gets processors since not yet locked.
          *                            3. after the output list update, an unfreeze is done, which requieres the processors lis. -> deadlock
          * not locking this list is ok, since it is only used AFTER a linkin,out, child, parent  or value. All of these others are locked.
          */
            // iTemp = InternalUpgradeToWritingFor(neuron, fProcessors);
            // if (iTemp != null) iRes.Add(iTemp);
            if (neuron is NeuronCluster)
            {
                InternalUpgradeToWritingFor(neuron, fChildren, handle);
            }

            InternalUpgradeToWritingFor(neuron, fValue, handle);
        }

        /// <summary>Upgrades a lock, found in the specified dictionary (so a specific part
        ///     of the <paramref name="neuron"/> is freed for writing).</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The dict.</param>
        /// <param name="handle">The handle.</param>
        private void UpgradeToWritingFor(
            Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict, 
            WaitObject handle)
        {
            lock (fLock) InternalUpgradeToWritingFor(neuron, dict, handle);
        }

        /// <summary>Upgrades a lock, found in the specified dictionary (so a specific part
        ///     of the <paramref name="neuron"/> is freed for writing) in a thread
        ///     unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The dict.</param>
        /// <param name="handle">The handle.</param>
        private void InternalUpgradeToWritingFor(
            Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict, 
            WaitObject handle)
        {
            ThreadLock iFound = null;
            if (dict.TryGetValue(neuron, out iFound))
            {
                iFound.UpgradeForWriting(handle);
            }
            else
            {
                throw new System.InvalidOperationException("No lock found for the specified neuron.");
            }
        }

        /// <summary>downgrades a lock, found in the specified dictionary (so a specific
        ///     part of the <paramref name="neuron"/> is freed for writing) in a
        ///     thread unsafe way.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="dict">The dict.</param>
        private void InternalDowngradeFor(Neuron neuron, System.Collections.Generic.Dictionary<Neuron, ThreadLock> dict)
        {
            ThreadLock iFound = null;
            if (dict.TryGetValue(neuron, out iFound))
            {
                iFound.Downgrade();
            }
            else
            {
                throw new System.InvalidOperationException("No lock found for the specified neuron.");
            }
        }

        /// <summary>The internal downgrade complete lock.</summary>
        /// <param name="neuron">The neuron.</param>
        private void InternalDowngradeCompleteLock(Neuron neuron)
        {
            InternalDowngradeFor(neuron, fParents);
            InternalDowngradeFor(neuron, fLinksIn);
            InternalDowngradeFor(neuron, fLinksOut);

            /*
          * Don't lock the processor section cause that can cause deadlocks.
          * ex, during a link created: 1.the output list gets updated, 
          *                            2. another thread asks a complete lock, needs to wait for the links out, but gets processors since not yet locked.
          *                            3. after the output list update, an unfreeze is done, which requieres the processors lis. -> deadlock
          * not locking this list is ok, since it is only used AFTER a linkin,out, child, parent  or value. All of these others are locked.
          */
            // InternalDowngradeFor(neuron, fProcessors);
            if (neuron is NeuronCluster)
            {
                InternalDowngradeFor(neuron, fChildren);
            }

            InternalDowngradeFor(neuron, fValue);
        }

        #endregion

        #endregion
    }
}