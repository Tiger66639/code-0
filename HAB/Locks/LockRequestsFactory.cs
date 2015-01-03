// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockRequestsFactory.cs" company="">
//   
// </copyright>
// <summary>
//   manages <see cref="LockRequestInfo" /> and lists of LockRequestInfo's so
//   that the objects can be re-used. This is done to take presure of the GC.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     manages <see cref="LockRequestInfo" /> and lists of LockRequestInfo's so
    ///     that the objects can be re-used. This is done to take presure of the GC.
    /// </summary>
    internal class LockRequestsFactory
    {
        /// <summary>The f big lists.</summary>
        private readonly System.Collections.Generic.Stack<LockRequestList> fBigLists =
            new System.Collections.Generic.Stack<LockRequestList>(); // for big locks, like duplicates/deletes.

        /// <summary>The f dict buffer.</summary>
        private readonly
            System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Neuron, LockRequestList>> fDictBuffer
                = new System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Neuron, LockRequestList>>();

                                                                                                             // the dict that contains the lists for comparing duplicates.

        /// <summary>The f info dict buffer.</summary>
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Neuron, LockRequestInfo>> fInfoDictBuffer =
                new System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Neuron, LockRequestInfo>>();

        /// <summary>The f link data.</summary>
        private readonly System.Collections.Generic.Stack<LinkData> fLinkData =
            new System.Collections.Generic.Stack<LinkData>();

        /// <summary>The f link data lists.</summary>
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.List<LinkData>> fLinkDataLists =
            new System.Collections.Generic.Stack<System.Collections.Generic.List<LinkData>>();

        /// <summary>The f lists.</summary>
        private readonly System.Collections.Generic.Stack<LockRequestList> fLists =
            new System.Collections.Generic.Stack<LockRequestList>(); // regular lists, like for link locks.

        /// <summary>The f locks.</summary>
        private readonly System.Collections.Generic.Stack<LockRequestInfo> fLocks =
            new System.Collections.Generic.Stack<LockRequestInfo>(); // the locks themselves.

        /// <summary>Gets a new <see cref="LockRequestInfo"/> object. Doesn't need to be
        ///     released seperatly, can be done when the list is released.</summary>
        /// <returns>The <see cref="LockRequestInfo"/>.</returns>
        public LockRequestInfo GetLock()
        {
            if (fLocks.Count > 0)
            {
                return fLocks.Pop();
            }

            return new LockRequestInfo();
        }

        /// <summary>Gets a regular list, for link locks and the likes (not big lists).</summary>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public LockRequestList GetList()
        {
            if (fLists.Count > 0)
            {
                var iRes = fLists.Pop();
                iRes.Clear();
                return iRes;
            }

            return new LockRequestList();
        }

        /// <summary>Gets a regular list, for link locks and the likes (not big lists).</summary>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public LockRequestList GetBigList()
        {
            if (fBigLists.Count > 0)
            {
                var iRes = fBigLists.Pop();
                iRes.Clear();
                return iRes;
            }

            return new LockRequestList();
        }

        /// <summary>Gets the buffer.</summary>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        public System.Collections.Generic.Dictionary<Neuron, LockRequestList> GetDictBuffer()
        {
            if (fDictBuffer.Count > 0)
            {
                var iRes = fDictBuffer.Pop();
                iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                return iRes;
            }

            return new System.Collections.Generic.Dictionary<Neuron, LockRequestList>();
        }

        /// <summary>The get info dict buffer.</summary>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        public System.Collections.Generic.Dictionary<Neuron, LockRequestInfo> GetInfoDictBuffer()
        {
            if (fInfoDictBuffer.Count > 0)
            {
                var iRes = fInfoDictBuffer.Pop();
                iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                return iRes;
            }

            return new System.Collections.Generic.Dictionary<Neuron, LockRequestInfo>();
        }

        /// <summary>The get link data list.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<LinkData> GetLinkDataList()
        {
            if (fLinkDataLists.Count > 0)
            {
                var iRes = fLinkDataLists.Pop();
                iRes.Clear(); // clear just before using, so taht a CPU cache is always up to date.
                return iRes;
            }

            return new System.Collections.Generic.List<LinkData>();
        }

        /// <summary>gets a link data object from the buffer when possible. doesn't need
        ///     to be released explicitily, this is done automatically when the<see cref="LinkData"/> list is released.</summary>
        /// <returns>The <see cref="LinkData"/>.</returns>
        internal LinkData GetLinkData()
        {
            if (fLinkData.Count > 0)
            {
                var iRes = fLinkData.Pop();
                iRes.Info = null;

                    // need to make certain that the list is gone, otherwise we try to recycle the same list 2 times.
                return iRes;
            }

            return new LinkData();
        }

        /// <summary>Releases the specified to release.</summary>
        /// <param name="toRelease">To release.</param>
        internal void Release(System.Collections.Generic.List<LinkData> toRelease)
        {
            var iMemFac = Factories.Default;
            foreach (var i in toRelease)
            {
                fLinkData.Push(i);
                if (i.Info != null)
                {
                    iMemFac.NLists.Recycle(i.Info);
                }
            }

            toRelease.Clear();
            fLinkDataLists.Push(toRelease);
        }

        /// <summary>Releases the specified list so it can be reused.</summary>
        /// <param name="toRelease">To release.</param>
        public void Release(System.Collections.Generic.Dictionary<Neuron, LockRequestList> toRelease)
        {
            toRelease.Clear(); // no need to buffer these objects for a longer period.
            fDictBuffer.Push(toRelease);
        }

        /// <summary>The release.</summary>
        /// <param name="toRelease">The to release.</param>
        public void Release(System.Collections.Generic.Dictionary<Neuron, LockRequestInfo> toRelease)
        {
            toRelease.Clear();
            fInfoDictBuffer.Push(toRelease);
        }

        /// <summary>Releases a regular list.</summary>
        /// <param name="list">The list.</param>
        public void ReleaseList(LockRequestList list)
        {
            list.Clear();
            fLists.Push(list);
        }

        /// <summary>Releases a regular list.</summary>
        /// <param name="list">The list.</param>
        public void ReleaseBigList(LockRequestList list)
        {
            list.Clear(); // clear the lists just to be save.
            fBigLists.Push(list);
        }

        /// <summary>The release lock.</summary>
        /// <param name="toRelease">The to release.</param>
        public void ReleaseLock(LockRequestInfo toRelease)
        {
            toRelease.Neuron = null;

                // we release the memory as soon as possible so that ghe gc can garbage collect asap.
            fLocks.Push(toRelease);
        }

        /// <summary>The create.</summary>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        internal static System.Collections.Generic.Dictionary<Neuron, LockRequestList> Create()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetDictBuffer();
            }

            return new System.Collections.Generic.Dictionary<Neuron, LockRequestList>();
        }

        /// <summary>The create info dict.</summary>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        internal static System.Collections.Generic.Dictionary<Neuron, LockRequestInfo> CreateInfoDict()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetInfoDictBuffer();
            }

            return new System.Collections.Generic.Dictionary<Neuron, LockRequestInfo>();
        }

        /// <summary>The create link data list.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<LinkData> CreateLinkDataList()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetLinkDataList();
            }

            return new System.Collections.Generic.List<LinkData>();
        }

        /// <summary>The create link data.</summary>
        /// <returns>The <see cref="LinkData"/>.</returns>
        internal static LinkData CreateLinkData()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetLinkData();
            }

            return new LinkData();
        }
    }
}