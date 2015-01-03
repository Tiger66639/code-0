// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiTrackCache.cs" company="">
//   
// </copyright>
// <summary>
//   a dictionary that internally stores the data in multiple dictionaries, so
//   that multiple threads can potentially access the dictionary at the same
//   time. Distribution between the dictionaries is done by formula: key %
//   NrDicts
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a dictionary that internally stores the data in multiple dictionaries, so
    ///     that multiple threads can potentially access the dictionary at the same
    ///     time. Distribution between the dictionaries is done by formula: key %
    ///     NrDicts
    /// </summary>
    internal class MultiTrackCache : System.Collections.Generic.IDictionary<ulong, object>
    {
        /// <summary>The f count.</summary>
        private long fCount;

        /// <summary>The f dicts.</summary>
        private readonly System.Collections.Generic.List<System.Collections.Generic.Dictionary<ulong, object>> fDicts =
            new System.Collections.Generic.List<System.Collections.Generic.Dictionary<ulong, object>>();

        /// <summary>Initializes a new instance of the <see cref="MultiTrackCache"/> class. 
        ///     Initializes a new instance of the <see cref="MultiTrackCache"/>
        ///     class.</summary>
        public MultiTrackCache()
        {
            CreateTracks();
        }

        #region IEnumerable<KeyValuePair<ulong,object>> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong, object>> GetEnumerator()
        {
            foreach (var i in fDicts)
            {
                foreach (var u in i)
                {
                    yield return u;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be
        ///     used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        ///     Creates the tracks as requested by the settings.
        /// </summary>
        private void CreateTracks()
        {
            fCount = 0;
            fDicts.Clear();
            while (fDicts.Count < Settings.NrCacheTracks)
            {
                var iNew = new System.Collections.Generic.Dictionary<ulong, object>();
                fDicts.Add(iNew);
            }
        }

        /// <summary>
        ///     Deletes the tracks that are to many.
        /// </summary>
        private void DeleteTracks()
        {
            while (fDicts.Count < Settings.NrCacheTracks)
            {
                fDicts.RemoveAt(fDicts.Count - 1);
            }
        }

        /// <summary>Fills a list with the contents of a single track. It is best to lock
        ///     the track (by locking an id that it would store (id % nrTracks) before
        ///     accessing the track. This is used by the neuron's link resolver as a
        ///     speed improvement (if there are many links, it can be faster to
        ///     inverse the operation and walk through all the neurons in the cache
        ///     instead of all the links).</summary>
        /// <param name="index">The index of the track.</param>
        /// <param name="results">The list to put the results in.</param>
        internal void GetTrackData(int index, System.Collections.Generic.List<Neuron> results)
        {
            foreach (var i in fDicts[index])
            {
                Neuron iTo = null;
                if (i.Value is System.WeakReference)
                {
                    if (((System.WeakReference)i.Value).IsAlive)
                    {
                        iTo = ((System.WeakReference)i.Value).Target as Neuron;
                    }
                }
                else
                {
                    iTo = i.Value as Neuron;
                }

                if (iTo != null)
                {
                    results.Add(iTo);
                }
            }
        }

        #region IDictionary<ulong,object> Members

        /// <summary>Adds the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(ulong key, object value)
        {
            var iIndex = (int)(key % (ulong)fDicts.Count);
            fDicts[iIndex].Add(key, value);
            System.Threading.Interlocked.Increment(ref fCount); // do a thread save increment.
        }

        /// <summary>Determines whether the specified <paramref name="key"/> contains key.</summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified <paramref name="key"/> contains key;
        ///     otherwise, <c>false</c> .</returns>
        public bool ContainsKey(ulong key)
        {
            var iIndex = (int)(key % (ulong)fDicts.Count);
            return fDicts[iIndex].ContainsKey(key);
        }

        /// <summary>
        ///     Gets the keys.
        /// </summary>
        public System.Collections.Generic.ICollection<ulong> Keys
        {
            get
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>Removes the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Remove(ulong key)
        {
            var iIndex = (int)(key % (ulong)fDicts.Count);
            if (fDicts[iIndex].Remove(key))
            {
                System.Threading.Interlocked.Decrement(ref fCount); // do a thread save increment.
                return true;
            }

            return false;
        }

        /// <summary>Tries the get value.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool TryGetValue(ulong key, out object value)
        {
            var iIndex = (int)(key % (ulong)fDicts.Count);
            return fDicts[iIndex].TryGetValue(key, out value);
        }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        public System.Collections.Generic.ICollection<object> Values
        {
            get
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>Gets or sets the <see cref="object"/> with the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public object this[ulong key]
        {
            get
            {
                var iIndex = (int)(key % (ulong)fDicts.Count);
                return fDicts[iIndex][key];
            }

            set
            {
                var iIndex = (int)(key % (ulong)fDicts.Count);
                if (fDicts[iIndex].ContainsKey(key) == false)
                {
                    System.Threading.Interlocked.Increment(ref fCount);

                        // do a thread save increment if we are not replacing an item but adding.
                }

                fDicts[iIndex][key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<ulong,object>> Members

        /// <summary>Adds the specified item.</summary>
        /// <param name="item">The item.</param>
        public void Add(System.Collections.Generic.KeyValuePair<ulong, object> item)
        {
            var iIndex = (int)(item.Key % (ulong)fDicts.Count);
            fDicts[iIndex].Add(item.Key, item.Value);
            System.Threading.Interlocked.Increment(ref fCount); // do a thread save increment.
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public void Clear()
        {
            foreach (var i in fDicts)
            {
                i.Clear();
            }

            if (fDicts.Count != Settings.NrCacheTracks)
            {
                DeleteTracks();
                CreateTracks();
                LockManager.Current.RebuildCacheLock();

                    // make certain that the lockManager has rebuild it's locks with the same nr of locks as in the settings.
            }

            fCount = 0;
        }

        /// <summary>Determines whether [contains] [the specified item].</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if [contains] [the specified item]; otherwise,<c>false</c> .</returns>
        public bool Contains(System.Collections.Generic.KeyValuePair<ulong, object> item)
        {
            var iIndex = (int)(item.Key % (ulong)fDicts.Count);
            object iFound;
            if (fDicts[iIndex].TryGetValue(item.Key, out iFound) && iFound == item.Value)
            {
                return true;
            }

            return false;
        }

        /// <summary>Copies to.</summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(System.Collections.Generic.KeyValuePair<ulong, object>[] array, int arrayIndex)
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return (int)fCount;
            }
        }

        /// <summary>
        ///     as this is a multi track cache, it is theoretically possible to
        ///     contain more than int.max values, so there is a long count as well.
        /// </summary>
        public long CountLong
        {
            get
            {
                return fCount;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is read only; otherwise, <c>false</c> .
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>Removes the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Remove(System.Collections.Generic.KeyValuePair<ulong, object> item)
        {
            throw new System.NotSupportedException();
        }

        #endregion
    }
}