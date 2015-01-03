// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadLock.cs" company="">
//   
// </copyright>
// <summary>
//   Defines all the different states that a thread lock can be in.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Defines all the different states that a thread lock can be in.
    /// </summary>
    public enum ThreadLockState
    {
        /// <summary>
        ///     <para>All</para>
        ///     <para>No threads are doing anything.</para>
        /// </summary>
        Dormant, 

        /// <summary>
        ///     There are 1 or more threads performing a read.
        /// </summary>
        Reading, 

        /// <summary>
        ///     a single thread is writing.
        /// </summary>
        Writing
    }

    /// <summary>
    ///     Contains and manages all information for a specific lock and all the
    ///     threads that make use of the info wrapped with this lock.
    /// </summary>
    internal class ThreadLock
    {
        #region Internal types

        /// <summary>The task type.</summary>
        private enum TaskType
        {
            /// <summary>The request read.</summary>
            RequestRead, 

            /// <summary>The request write.</summary>
            RequestWrite, 

            /// <summary>The upgrade.</summary>
            Upgrade
        }

        /// <summary>
        ///     Contains all the information required to relaunch a task (requestRead,
        ///     RequestWrite, ReleaseRead, Upgrad,...) in case it couldn't be
        ///     performed because of the current state of the lock (allowing 1 thread
        ///     to write).
        /// </summary>
        private class TaskInfo
        {
            /// <summary>
            ///     Gets or sets the thread that requested the action.
            /// </summary>
            /// <value>
            ///     The thread.
            /// </value>
            public System.Threading.Thread Thread { get; set; }

            /// <summary>Gets or sets the info.</summary>
            public ThreadLockPart Info { get; set; }

            /// <summary>Gets or sets the task.</summary>
            public TaskType Task { get; set; }

            /// <summary>Gets or sets the wait handle.</summary>
            public WaitObject WaitHandle { get; set; }
        }

        #endregion

        #region fields

        /// <summary>
        ///     stores all the recycled threadlock objects. Access to this list should
        ///     not be locked, since all ThreadLock objects are created and recycled
        ///     within a lock.
        /// </summary>
        private static readonly System.Collections.Generic.Queue<ThreadLock> fRecycled =
            new System.Collections.Generic.Queue<ThreadLock>();

        /// <summary>The f thread lock parts.</summary>
        private readonly ThreadLockPartFactory fThreadLockParts = new ThreadLockPartFactory();

        /// <summary>The f tasks.</summary>
        private readonly System.Collections.Generic.Queue<TaskInfo> fTasks =
            new System.Collections.Generic.Queue<TaskInfo>();

        /// <summary>The f threads.</summary>
        private readonly System.Collections.Generic.Dictionary<System.Threading.Thread, ThreadLockPart> fThreads =
            new System.Collections.Generic.Dictionary<System.Threading.Thread, ThreadLockPart>();

        /// <summary>The f read count.</summary>
        private volatile int fReadCount;

        /// <summary>The f state.</summary>
        private volatile ThreadLockState fState;

        /// <summary>The f write count.</summary>
        private volatile int fWriteCount;

        /// <summary>The f writing thread.</summary>
        private volatile System.Threading.Thread fWritingThread;

        #endregion

        #region ctor

        /// <summary>Prevents a default instance of the <see cref="ThreadLock"/> class from being created. 
        ///     Prevents a default instance of the <see cref="ThreadLock"/> class
        ///     from being created.</summary>
        private ThreadLock()
        {
        }

        /// <summary>Creates an instance. This is nt thread save, it should be done within
        ///     a lock.</summary>
        /// <returns>The <see cref="ThreadLock"/>.</returns>
        public static ThreadLock Create()
        {
            if (fRecycled.Count > 0)
            {
                return fRecycled.Dequeue();
            }

            return new ThreadLock();
        }

        /// <summary>
        ///     adds the item to the <see langword="internal" /> list so it can be
        ///     re-used. This is not thread save. It should be done within a lock.
        /// </summary>
        /// <param name="toAdd"></param>
        public void Recycle()
        {
            fRecycled.Enqueue(this);
        }

        #endregion

        #region Prop

        #region Tasks

        /// <summary>
        ///     Gets the <see cref="ThreadLockInfo" /> objects that have a task that
        ///     hasn't been processed yet because the lock is processing a write for
        ///     another thread.
        /// </summary>
        /// <value>
        ///     The requests.
        /// </value>
        private System.Collections.Generic.Queue<TaskInfo> Tasks
        {
            get
            {
                return fTasks;
            }
        }

        #endregion

        /// <summary>Gets the task count.</summary>
        public int TaskCount
        {
            get
            {
                return fTasks.Count;
            }
        }

        #region Threads

        /// <summary>
        ///     Gets the threads and there lock parts that are currently using the
        ///     information <see langword="protected" /> by this lock.
        /// </summary>
        public System.Collections.Generic.Dictionary<System.Threading.Thread, ThreadLockPart> Threads
        {
            get
            {
                return fThreads;
            }
        }

        #endregion

        #region ReadCount

        /// <summary>
        ///     Gets/sets the number of reads are in action for this lock by all the
        ///     threads together.
        /// </summary>
        /// <remarks>
        ///     This is used to quickly find out if a write is alowed or if it should
        ///     be
        /// </remarks>
        public int ReadCount
        {
            get
            {
                return fReadCount;
            }

            set
            {
                fReadCount = value;
            }
        }

        #endregion

        #region WriteCount

        /// <summary>
        ///     Gets/sets the number of writes are in action for this lock by all the
        ///     threads together.
        /// </summary>
        /// <remarks>
        ///     This is used to quickly find out if a write is alowed or if it should
        ///     be
        /// </remarks>
        public int WriteCount
        {
            get
            {
                return fWriteCount;
            }

            set
            {
                fWriteCount = value;
            }
        }

        #endregion

        #region State

        /// <summary>
        ///     Gets the state of the lock.
        /// </summary>
        public ThreadLockState State
        {
            get
            {
                return fState;
            }

            internal set
            {
                fState = value;
            }
        }

        #endregion

        #region WritingThread

        /// <summary>
        ///     Gets the thread that is currently writing to the locked data (if any).
        /// </summary>
        public System.Threading.Thread WritingThread
        {
            get
            {
                return fWritingThread;
            }

            internal set
            {
                fWritingThread = value;
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Requests the read.</summary>
        /// <param name="handle">The handle.</param>
        /// <remarks>Reads are allowed when there are no other threads writing, or the
        ///     requestor is the same as the writer.</remarks>
        public void RequestRead(WaitObject handle)
        {
            var iPart = GetLockPart();
            if ((fTasks.Count == 0 && State < ThreadLockState.Writing) || iPart.IsActive)
            {
                // when there is only 1 thread active, we allows read during writes.
                ActivateRead(iPart);
            }
            else
            {
                var iTask = new TaskInfo
                                {
                                    Task = TaskType.RequestRead, 
                                    Thread = System.Threading.Thread.CurrentThread, 
                                    Info = iPart, 
                                    WaitHandle = handle.Register()
                                };
                fTasks.Enqueue(iTask);
                ReadCount -= iPart.ReadCount;

                    // we remove all of this thread's read locks from the section, so that any reads from the requesting thread don't block the section from activating it (a write is only activated when all reads are done, but the reads of the requesting thread can't offcourse be done yet).
            }
        }

        /// <summary>Requests the Write.</summary>
        /// <param name="handle">The handle.</param>
        public void RequestWrite(WaitObject handle)
        {
            var iPart = GetLockPart();
            if ((fTasks.Count == 0 && State == ThreadLockState.Dormant)
                || System.Threading.Thread.CurrentThread == WritingThread || fThreads.Count == 1)
            {
                ActivateWrite(iPart, System.Threading.Thread.CurrentThread);
            }
            else
            {
                var iTask = new TaskInfo
                                {
                                    Task = TaskType.RequestWrite, 
                                    Thread = System.Threading.Thread.CurrentThread, 
                                    Info = iPart, 
                                    WaitHandle = handle.Register()
                                };
                fTasks.Enqueue(iTask);
                ReadCount -= iPart.ReadCount;

                    // we remove all of this thread's read locks from the section, so that any reads from the requesting thread don't block the section from activating it (a write is only activated when all reads are done, but the reads of the requesting thread can't offcourse be done yet).
            }
        }

        /// <summary>Upgrades the current thread to writing from reading. Note: no check is
        ///     done wether the thread is currently in a read state, only that it has
        ///     any read locks.</summary>
        /// <param name="handle">The handle.</param>
        public void UpgradeForWriting(WaitObject handle)
        {
            var iPart = GetLockPart();
            if (iPart.ReadCount <= 0)
            {
                throw new System.InvalidOperationException("Read lock required to allow for an upgrade.");
            }

            if ((fTasks.Count == 0 && State == ThreadLockState.Reading && ReadCount == iPart.ReadCount)
                || System.Threading.Thread.CurrentThread == WritingThread || fThreads.Count == 1)
            {
                ActivateUpgradeForWriting(iPart, System.Threading.Thread.CurrentThread);
            }
            else
            {
                var iTask = new TaskInfo
                                {
                                    Task = TaskType.Upgrade, 
                                    Thread = System.Threading.Thread.CurrentThread, 
                                    Info = iPart, 
                                    WaitHandle = handle.Register()
                                };
                fTasks.Enqueue(iTask);
                ReadCount -= iPart.ReadCount;

                    // we remove all of this thread's read locks from the section, so that any reads from the requesting thread don't block the section from activating it (a write is only activated when all reads are done, but the reads of the requesting thread can't offcourse be done yet).
                ProcessQueue();
            }
        }

        /// <summary>
        ///     Releases a previously requested and activated write block.
        /// </summary>
        public void ReleaseWrite()
        {
            var iPart = GetLockPart();
            if (iPart.WriteCount > 0)
            {
                iPart.WriteCount--;
                WriteCount--;
            }
            else
            {
                iPart.WriteCount = 0;
                LogService.Log.LogError("ThreadLock.ReleaseWrite", "Write lock required to allow for a write release.");

                    // don't throw an exception, try to release the write properly, to try and recover.
            }

            if (WriteCount == 0)
            {
                WritingThread = null;
                if (ReadCount == 0)
                {
                    State = ThreadLockState.Dormant;
                }
                else
                {
                    State = ThreadLockState.Reading;
                }
            }

            if (iPart.WriteCount == 0 && iPart.ReadCount == 0)
            {
                fThreads.Remove(System.Threading.Thread.CurrentThread);
                fThreadLockParts.Recycle(iPart);
            }

            ProcessQueue();
        }

        /// <summary>
        ///     Releases a read lock. When there is a write request queued and there
        ///     are no more reads, this is activated
        /// </summary>
        public void ReleaseRead()
        {
            var iPart = GetLockPart();
            if (iPart.ReadCount <= 0)
            {
                throw new System.InvalidOperationException("Read lock required to allow for a read release.");
            }

            iPart.ReadCount--;
            ReadCount--;
            if (iPart.WriteCount == 0 && iPart.ReadCount == 0)
            {
                fThreads.Remove(System.Threading.Thread.CurrentThread);
                fThreadLockParts.Recycle(iPart);
                if (ReadCount == 0 && WriteCount == 0)
                {
                    State = ThreadLockState.Dormant;
                }
            }

            ProcessQueue();
        }

        /// <summary>Downgrades the current thread write mode to a read state. When there
        ///     are read's waiting, these are started. Note, no check is done if the
        ///     thread is in a write mode, only that it has a write lock.</summary>
        public void Downgrade()
        {
            var iPart = GetLockPart();
            if (iPart.WriteCount <= 0)
            {
                throw new System.InvalidOperationException("Write lock required to allow for a downgrade.");
            }

            iPart.ReadCount++;
            iPart.WriteCount--;
            ReadCount++;
            WriteCount--;
            if (WriteCount == 0)
            {
                WritingThread = null;
                State = ThreadLockState.Reading;

                    // there are no more writes + we are downgrading, so going back to a read, so we are in read state.
                ProcessQueue();
            }
        }

        #endregion

        #region Helpers

        /// <summary>Gets the <see cref="ThreadLockPart"/> for the current thread, or
        ///     creates a new one if it doesn't exist already.</summary>
        /// <returns>The <see cref="ThreadLockPart"/>.</returns>
        private ThreadLockPart GetLockPart()
        {
            ThreadLockPart iRes;
            if (fThreads.TryGetValue(System.Threading.Thread.CurrentThread, out iRes) == false)
            {
                iRes = fThreadLockParts.Get(this);
                fThreads.Add(System.Threading.Thread.CurrentThread, iRes);
            }

            return iRes;
        }

        /// <summary>Performs all the actions to active a reead request for the specified
        ///     thread lock part.</summary>
        /// <param name="part">The part.</param>
        private void ActivateRead(ThreadLockPart part)
        {
            part.ReadCount++;
            ReadCount++;

                // in this version, we do a simple increment, cause it is called from a normal state (not sleeping), so the part.ReadCount is already included in the readCount (which is not the case if the part was put on hold).
            if (State == ThreadLockState.Dormant)
            {
                // when we are already writing, we don't change it back to reading (possible when a thread that is writing, also requests a read access again).
                State = ThreadLockState.Reading;
            }
        }

        /// <summary>Performs all the actions to active a reead request for the specified<paramref name="thread"/> lock part.</summary>
        /// <param name="part">The part.</param>
        /// <param name="thread">The thread.</param>
        private void ActivateRead(ThreadLockPart part, System.Threading.Thread thread)
        {
            part.ReadCount++;
            ReadCount += part.ReadCount;

                // important that we add all the readcount of part, since it might have had more readlocks before?
            if (State == ThreadLockState.Dormant)
            {
                // when we are already writing, we don't change it back to reading (possible when a thread that is writing, also requests a read access again).
                State = ThreadLockState.Reading;
            }
        }

        /// <summary>Performs all the actions to active a write request for the specified<paramref name="thread"/> lock part.</summary>
        /// <param name="part">The part.</param>
        /// <param name="thread">The thread.</param>
        private void ActivateWrite(ThreadLockPart part, System.Threading.Thread thread)
        {
            WritingThread = thread;
            part.WriteCount++;
            WriteCount++;
            State = ThreadLockState.Writing;
        }

        /// <summary>Performs all the actions to active an apgrade request for the
        ///     specified <paramref name="thread"/> lock part.</summary>
        /// <param name="part">The part.</param>
        /// <param name="thread">The thread.</param>
        private void ActivateUpgradeForWriting(ThreadLockPart part, System.Threading.Thread thread)
        {
            WritingThread = thread;
            part.WriteCount++;
            part.ReadCount--;
            WriteCount++;
            ReadCount--;
            State = ThreadLockState.Writing;
        }

        /// <summary>
        ///     Activates as many locks as possible that are stored on the
        ///     <see cref="fTasks" /> queue.
        /// </summary>
        /// <remarks>
        ///     -When the next item is a read (so we are in a write lock): activate
        ///     when current mode is dormant or read + activate all consecutive read
        ///     requests. -when the next item is a write: acivate when there are no
        ///     more reads waiting in this
        /// </remarks>
        private void ProcessQueue()
        {
            while (fTasks.Count > 0)
            {
                var iProcessed = false;
                var iTask = fTasks.Peek();
                switch (iTask.Task)
                {
                    case TaskType.RequestRead:
                        if (State == ThreadLockState.Dormant || State == ThreadLockState.Reading)
                        {
                            iProcessed = true;
                            ActivateRead(iTask.Info, iTask.Thread);
                        }

                        break;
                    case TaskType.RequestWrite:
                        if (State == ThreadLockState.Dormant || (ReadCount == 0 && State == ThreadLockState.Reading))
                        {
                            // when there were upgrades that are blocked, we are not dormant, but there comes a point when readcount hits 0 cause all threads are in upgrade or released.
                            iProcessed = true;
                            ReadCount += iTask.Info.ReadCount;

                                // we need to restore the read locks that were removed for this thread, these were removed so that the readCount could get to 0, we now need to add them again cause the thread has become active and they need to be taken into acount again.
                            ActivateWrite(iTask.Info, iTask.Thread);
                        }

                        break;
                    case TaskType.Upgrade:
                        if (State == ThreadLockState.Dormant || (ReadCount == 0 && State == ThreadLockState.Reading))
                        {
                            // when there were upgrades that are blocked, we are not dormant, but there comes a point when readcount hits 0 cause all threads are in upgrade or released.
                            ReadCount += iTask.Info.ReadCount;

                                // we need to restore the read locks that were removed for this thread, these were removed so that the readCount could get to 0, we now need to add them again cause the thread has become active and they need to be taken into acount again.
                            ActivateUpgradeForWriting(iTask.Info, iTask.Thread);
                            iProcessed = true;
                        }

                        break;
                    default:
                        break;
                }

                if (iProcessed)
                {
                    fTasks.Dequeue();
                    iTask.WaitHandle.Set(); // off course we need to let the other thread know it can continue.
                }
                else
                {
                    break;
                }
            }

            // if (fTasks.Count == 0 && State == ThreadLockState.Dormant && fThreads.Count == 0)         //check if we are done. We are only done when all threads have been removed from the fThreads list.
            // State = ThreadLockState.Finished;
        }

        #endregion
    }
}