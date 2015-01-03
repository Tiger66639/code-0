// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDManager.cs" company="">
//   
// </copyright>
// <summary>
//   This class is responsible for managing the Neuron ids.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     This class is responsible for managing the Neuron ids.
    /// </summary>
    /// <remarks>
    ///     A timer is used together with a double buffers, to make certain that recycled id's are kept as deleted for at least
    ///     x amount of time. this
    ///     is to make certain that there are no conflicts while retrieving neurons from ids: locks need to be released before
    ///     neurons can be retrieved (otherwise
    ///     it's possible to have deadlocks), so it's best that an id can't be reused to soon after deletion, otherwise some
    ///     processes get screwed up.
    ///     Another possible solution would be to lock all the id's while being retrieved from the neuron (as parents,
    ///     children,..) and released again after the
    ///     corresponding neuron was retrieved. In that case, you don't need timers and double buffers. But it is probably
    ///     slower.
    /// </remarks>
    internal class IDManager
    {
        /// <summary>The timerperiod.</summary>
        private const int TIMERPERIOD = 200;

        /// <summary>Initializes a new instance of the <see cref="IDManager"/> class.</summary>
        public IDManager()
        {
            IsEditMode = false;
            fTimer.Elapsed += fTimer_Elapsed;
        }

        /// <summary>Determines whether the specified id is part of the Free id lists.</summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if [contains] [the specified id]; otherwise, <c>false</c>.</returns>
        internal bool IsDeleted(ulong id)
        {
            fFreeIdsLock.EnterReadLock();
            try
            {
                return fFreeIds.Contains(id) || fFreeIds2.Contains(id) || fFreeIds3.Contains(id);
            }
            finally
            {
                fFreeIdsLock.ExitReadLock();
            }
        }

        /// <summary>when the timer has elapsed, we switch the lists.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            fFreeIdsLock.EnterWriteLock();
            try
            {
                if (fFreeIds.Count == 0)
                {
                    var iTemp = fFreeIds;
                    fFreeIds = fFreeIds3;
                    fFreeIds3 = fFreeIds2;
                    fFreeIds2 = iTemp;
                    if (fFreeIds2.Count == 0 && fFreeIds3.Count == 0)
                    {
                        // if there are no more new id's to free, stop the timer.
                        fTimer.Stop();
                    }
                }
            }
            finally
            {
                fFreeIdsLock.ExitWriteLock();
            }
        }

        #region Fields

        /// <summary>The f free ids.</summary>
        private System.Collections.Generic.HashSet<ulong> fFreeIds = new System.Collections.Generic.HashSet<ulong>();

                                                          // the list to start providing id's from.

        /// <summary>The f free ids 2.</summary>
        private System.Collections.Generic.HashSet<ulong> fFreeIds2 = new System.Collections.Generic.HashSet<ulong>();

                                                          // the list that will collect new free id's for as long the timer is not done.

        /// <summary>The f free ids 3.</summary>
        private System.Collections.Generic.HashSet<ulong> fFreeIds3 = new System.Collections.Generic.HashSet<ulong>();

                                                          // the list currently on hold, it will become the first list on the next timer. It provides the delay.

        /// <summary>The f timer.</summary>
        private readonly System.Timers.Timer fTimer = new System.Timers.Timer(TIMERPERIOD);

        /// <summary>The f free ids lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fFreeIdsLock =
            new System.Threading.ReaderWriterLockSlim();

        /// <summary>The f next id.</summary>
        private ulong fNextID = (ulong)PredefinedNeurons.Dynamic;

        #endregion

        #region prop

        #region NrOfFreeIds

        /// <summary>
        ///     Gets the amount of free id's currently available.
        /// </summary>
        public ulong NrOfFreeIds
        {
            get
            {
                fFreeIdsLock.EnterReadLock();
                try
                {
                    return (ulong)fFreeIds.Count + (ulong)fFreeIds2.Count + (ulong)fFreeIds3.Count;
                }
                finally
                {
                    fFreeIdsLock.ExitReadLock();
                }
            }
        }

        /// <summary>Gets the neuron count.</summary>
        public ulong NeuronCount
        {
            get
            {
                fFreeIdsLock.EnterReadLock();
                try
                {
                    return fNextID - ((ulong)fFreeIds.Count + (ulong)fFreeIds2.Count + (ulong)fFreeIds3.Count);
                }
                finally
                {
                    fFreeIdsLock.ExitReadLock();
                }
            }
        }

        #endregion

        #region NextID

        /// <summary>
        ///     Gets the next id that is available for use.
        /// </summary>
        /// <remarks>
        ///     we allow the set, so we can easely read it from xml (older code)
        /// </remarks>
        public ulong NextID
        {
            get
            {
                return fNextID;
            }

            internal set
            {
                fNextID = value;
            }
        }

        #endregion

        #region IsEditMode

        /// <summary>
        ///     determins how ids are recycled: when in edit mode, they ar re-used as fast as possible,
        ///     but when in run mode, recycled id's are buffered for a short while, to compensate for
        ///     buffered objects in variables.
        /// </summary>
        public bool IsEditMode { get; set; }

        #endregion

        #endregion

        #region Functions

        /// <summary>Gets a new id that can be used for a freshly created neuron.</summary>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong GetId()
        {
            ulong iRes;
            fFreeIdsLock.EnterWriteLock();
            try
            {
                if (fFreeIds.Count > 0)
                {
                    iRes = Enumerable.Last(fFreeIds);

                        // need to take last, so that the recycled ids remain in the list as long as possible. If we don't do this, and always return the id as fast as possible, there can sometimes be a neuron leak, don't know why yet.
                    fFreeIds.Remove(iRes);
                    Brain.Current.RemoveFromNeuronsToDelete(iRes);

                        // we need to make certain that the brain doesn't think anymore that it needs to delete this record, it is simply reused.

                    // return iRes;
                }
                else
                {
                    if (fNextID == ulong.MaxValue)
                    {
                        throw new BrainException("The brain is full!");
                    }

                    iRes = fNextID;
                    fNextID++;

                        // this is always a new id, it hasn't been recycled yet, so we don't need to call RemoveFromNeuronsToDelete
                }
            }
            finally
            {
                fFreeIdsLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>Gets a new id that can be used for a freshly created neuron, preferebly the
        ///     specified id, if this is possible.</summary>
        /// <param name="prefered">The prefered.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong GetId(ulong prefered)
        {
            ulong iRes;
            fFreeIdsLock.EnterWriteLock();

                // we do a write lock from the start cause need to a write in to many different locations.
            try
            {
                if (fFreeIds.Contains(prefered))
                {
                    fFreeIds.Remove(prefered);
                    iRes = prefered;
                }
                else if (fNextID < prefered)
                {
                    while (fNextID < prefered - 1)
                    {
                        fFreeIds.Add(fNextID);
                        fNextID++;
                    }

                    iRes = prefered;
                }
                else
                {
                    iRes = fNextID;
                    fNextID++;
                }

                Brain.Current.RemoveFromNeuronsToDelete(iRes);

                    // we need to make certain that the brain doesn't think anymore that it needs to delete this record, it is simply reused.
            }
            finally
            {
                fFreeIdsLock.ExitWriteLock();
            }

            return iRes;
        }

        /// <summary>Releases the specified id so that it can be assigned again to a new neuron.</summary>
        /// <param name="toRelease">To release.</param>
        public void ReleaseId(ulong toRelease)
        {
            fFreeIdsLock.EnterWriteLock();
            try
            {
                // if (fIsEditMode == false)
                // {
                if (fFreeIds2.Count == int.MaxValue)
                {
                    throw new BrainException("The id recycle list is full!");
                }

                fFreeIds2.Add(toRelease);
                if (fTimer.Enabled == false)
                {
                    fTimer.Start();
                }

                // }
                // else
                // fFreeIds.Add(toRelease);                                 //when in edit mode, simply add the new item to be used asap.
            }
            finally
            {
                fFreeIdsLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     locks the id manager in a write state. This is used to clear out the network.
        /// </summary>
        internal void GetWriteLock()
        {
            fFreeIdsLock.EnterWriteLock();
        }

        /// <summary>
        ///     Locks this instance so that no new id's can be registered or generated for
        ///     as long as the instance is locked. It's still possible to check for id existence, since the lock is a read lock,
        ///     not write.
        ///     This function should be called before any thread unsafe operations are
        ///     done like <see cref="IDManager.Stream" /> or <see cref="IDManager.Clear" />.
        /// </summary>
        public void Lock()
        {
            fFreeIdsLock.EnterReadLock();
        }

        /// <summary>
        ///     Releases the previously created lock on this instance.
        /// </summary>
        public void ReleaseLock()
        {
            fFreeIdsLock.ExitReadLock();
        }

        /// <summary>
        ///     releases a previously optained write lock. This is used to clear out the network.
        /// </summary>
        internal void ReleaseWriteLock()
        {
            fFreeIdsLock.ExitWriteLock();
        }

        /// <summary>
        ///     Clears all the free ids.
        /// </summary>
        public void Clear()
        {
            fTimer.Stop();
            fFreeIdsLock.EnterWriteLock(); // so that no timer callback can interfere with this call.
            try
            {
                fFreeIds.Clear();
                fFreeIds2.Clear();
                fFreeIds3.Clear();
                fNextID = (ulong)PredefinedNeurons.Dynamic; // also need to reset the next available id.
            }
            finally
            {
                fFreeIdsLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     Clears all the free ids. This is not thread safe. Before calling
        ///     this function, you need to call <see cref="IDManager.Lock" /> or <see cref="IDManager.LockForWrite" />.When
        ///     done, call <see cref="IDManager.ReleaseLock" /> or <see cref="IDManager.ReleaseWriteLock" />.
        /// </summary>
        public void ClearUnsave()
        {
            fTimer.Stop();
            fFreeIds.Clear();
            fFreeIds2.Clear();
            fFreeIds3.Clear();
            fNextID = (ulong)PredefinedNeurons.Dynamic; // also need to reset the next available id.
        }

        /// <summary>Writes the free ids to XML. This is not thread safe. Before calling
        ///     this function, you need to call <see cref="IDManager.Lock"/>.When
        ///     done, call <see cref="IDManager.Release"/>.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteFreeIDs(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("FreeIds");
            WriteFreeIDs(writer, fFreeIds);
            WriteFreeIDs(writer, fFreeIds2);
            WriteFreeIDs(writer, fFreeIds3);
            writer.WriteEndElement();
        }

        /// <summary>
        ///     removes all the free ids that border the next id (so no neuron between next empty and free id).
        /// </summary>
        internal void ShrinkFreeIds()
        {
            var iTest = NextID - 1;
            while (fFreeIds.Remove(iTest) || fFreeIds2.Remove(iTest) || fFreeIds3.Remove(iTest))
            {
                NextID--;
                iTest = NextID - 1;
            }
        }

        /// <summary>The write free i ds.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="list">The list.</param>
        private void WriteFreeIDs(System.Xml.XmlWriter writer, System.Collections.Generic.HashSet<ulong> list)
        {
            foreach (var i in list)
            {
                writer.WriteStartElement("ID");
                writer.WriteString(i.ToString());
                writer.WriteEndElement();
            }
        }

        /// <summary>Reads the free ids from xml.This is not thread safe. Before calling
        ///     this function, you need to call <see cref="IDManager.Lock"/>.When
        ///     done, call <see cref="IDManager.Release"/>.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadFreeIDs(System.Xml.XmlReader reader)
        {
            var iWasEmpty = reader.IsEmptyElement;
            reader.ReadStartElement("FreeIds");
            if (iWasEmpty == false)
            {
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("ID");
                    var iVal = reader.ReadString();
                    var iConverted = ulong.Parse(iVal);
                    fFreeIds.Add(iConverted); // don't need thread safe access while loading the network.
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        #endregion
    }
}