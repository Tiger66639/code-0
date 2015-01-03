// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordsDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   A dictionary for the words used by the <see cref="TextSin" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A dictionary for the words used by the <see cref="TextSin" /> .
    /// </summary>
    public class WordsDictionary : System.Collections.Generic.IEnumerable<ulong>
    {
        /// <summary>Initializes a new instance of the <see cref="WordsDictionary"/> class. 
        ///     Initializes a new instance of the <see cref="WordsDictionary"/>
        ///     class.</summary>
        public WordsDictionary()
        {
            IsChanged = false;
            fDict = new System.Collections.Generic.Dictionary<string, ulong>();
        }

        /// <summary>Initializes a new instance of the <see cref="WordsDictionary"/> class. Initializes a new instance of the <see cref="WordsDictionary"/>
        ///     class.</summary>
        /// <param name="capacity">The capacity.</param>
        public WordsDictionary(int capacity)
        {
            IsChanged = false;
            fDict = new System.Collections.Generic.Dictionary<string, ulong>(capacity);
        }

        /// <summary>
        ///     gets all the id's of the textneurons that are stored in the
        ///     dictionary.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public System.Collections.Generic.IEnumerable<ulong> Values
        {
            get
            {
                return fDict.Values;
            }
        }

        /// <summary>
        ///     Gets the total nr of items stored in the dict.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        public int Count
        {
            get
            {
                return fDict.Count;
            }
        }

        #region IsChanged

        /// <summary>
        ///     Gets the value that indicates if this dict was changed or not. This
        ///     allows us to speed up the save process.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsChanged { get; internal set; }

        #endregion

        /// <summary>Gets or sets the <see cref="ulong"/> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <value></value>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong this[string index]
        {
            get
            {
                fLock.EnterReadLock();
                try
                {
                    return fDict[index];
                }
                finally
                {
                    fLock.ExitReadLock();
                }
            }

            set
            {
                fLock.EnterWriteLock();
                try
                {
                    IsChanged = true;
                    fDict[index] = value;
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        #region IEnumerable<ulong> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<ulong> GetEnumerator()
        {
            return new WordsDictionaryEnum(Values.GetEnumerator(), fLock);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new WordsDictionaryEnum(Values.GetEnumerator(), fLock);
        }

        #endregion

        /// <summary>Loads the words dictionary fromt the specified path.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="WordsDictionary"/>.</returns>
        internal static WordsDictionary Load(string path)
        {
            WordsDictionary iRes;
            var iTextFile = System.IO.Path.Combine(path, TEXTFILENAME);
            if (System.IO.File.Exists(iTextFile))
            {
                // older systems don't store a seperate index file for the strings.
                iRes = LoadFromTextFile(path, iTextFile);
            }
            else
            {
                iRes = LoadFromDB(path);
            }

            iRes.IsChanged = false;
            return iRes;
        }

        /// <summary>The load from db.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="WordsDictionary"/>.</returns>
        private static WordsDictionary LoadFromDB(string path)
        {
            var iWords = new WordsDictionary(EntryNeuronsHelper.GetCapacityEntryFor(path, typeof(TextSin)));

                // first create a temp, than assign to fWords, this way, the 'SaveDict' won't try to save anything until after it is fully loaded.
            foreach (var iId in EntryNeuronsHelper.GetEntryIDsFor(path, typeof(TextSin)))
            {
                if (Brain.Current.IsInitialized == false)
                {
                    // the database got closed or isn't open, don't try any further.
                    return iWords;
                }

                iWords.AddIdFromDB(iId);
            }

            return iWords;
        }

        /// <summary>The add id from db.</summary>
        /// <param name="iId">The i id.</param>
        private void AddIdFromDB(ulong iId)
        {
            Neuron iTemp;
            if (Brain.Current.TryFindNeuron(iId, out iTemp))
            {
                var iText = iTemp as TextNeuron;
                if (iText != null)
                {
                    if (ContainsKey(iText.Text) == false)
                    {
                        fDict.Add(iText.Text.ToLower(), iText.ID);
                    }
                    else
                    {
                        fDict[iText.Text] = iText.ID; // we store the new value, so we use the last in the list.
                        LogService.Log.LogError(
                            "Words dictionary", 
                            string.Format("Found duplicate neuron in entry points: {0}.", iText));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "Words dictionary", 
                        string.Format("Found invalid neuron in entry points: {0} is not a TextNeuron.", iId));
                }
            }
            else
            {
                LogService.Log.LogError(
                    "Words dictionary", 
                    string.Format("Neuron was not found in the database: {0}.", iId));
            }
        }

        /// <summary>Loads the strings from a text file.</summary>
        /// <param name="path">The path.</param>
        /// <param name="iTextFile">The i text file.</param>
        /// <returns>The <see cref="WordsDictionary"/>.</returns>
        private static WordsDictionary LoadFromTextFile(string path, string iTextFile)
        {
            var iWords = new WordsDictionary(EntryNeuronsHelper.GetCapacityEntryFor(path, typeof(TextSin)));

                // first create a temp, than assign to fWords, this way, the 'SaveDict' won't try to save anything until after it is fully loaded.
            using (
                var iTextStr = new System.IO.FileStream(iTextFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iBuffer = new System.IO.BufferedStream(iTextStr, Settings.DBBufferSize);
                using (var iTextReader = new System.IO.BinaryReader(iBuffer))
                {
                    foreach (var iId in EntryNeuronsHelper.GetEntryIDsFor(path, typeof(TextSin)))
                    {
                        if (Brain.Current.IsInitialized == false)
                        {
                            // the database got closed or isn't open, don't try any further.
                            return iWords;
                        }

                        var iLine = iTextReader.ReadString();
                        if (iLine != null)
                        {
                            iWords.fDict.Add(iLine, iId);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "Words dictionary", 
                                string.Format("Text file inconsistency found at id: ", iId));
                            iWords.AddIdFromDB(iId);
                        }
                    }
                }
            }

            return iWords;
        }

        /// <summary>Saves the dict to the specified path.</summary>
        /// <param name="path">The path.</param>
        internal void Save(string path)
        {
            fLock.EnterReadLock();

                // the save operation can happen during execution. need to make certain that it is thread save.
            try
            {
                EntryNeuronsHelper.SaveTextDict(path, fDict);
                IsChanged = false;
            }
            finally
            {
                fLock.ExitReadLock();
            }
        }

        /// <summary>adds the item if there isn't an item already stored witht he same
        ///     text.</summary>
        /// <param name="toAdd"></param>
        internal void AddWhenPossible(TextNeuron toAdd)
        {
            fLock.EnterWriteLock();
            try
            {
                var iVal = toAdd.Text.ToLower();
                if (fDict.ContainsKey(iVal) == false)
                {
                    // don't add if it already is in there.
                    fDict.Add(iVal, toAdd.ID);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #region fields

        /// <summary>The textfilename.</summary>
        public const string TEXTFILENAME = "strings.txt";

        /// <summary>The f dict.</summary>
        private readonly System.Collections.Generic.Dictionary<string, ulong> fDict =
            new System.Collections.Generic.Dictionary<string, ulong>();

        /// <summary>The f lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fLock = new System.Threading.ReaderWriterLockSlim();

        #endregion

        #region Functions

        /// <summary>Removes the element with the specified <paramref name="key"/> from the
        ///     Dict.</summary>
        /// <param name="key">The key.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IDictionary`2"/> is
        ///     read-only.</exception>
        /// <returns><see langword="true"/> if the element is successfully removed;
        ///     otherwise, false. This method also returns <see langword="false"/> if<paramref name="key"/> was not found in the original<see cref="System.Collections.Generic.IDictionary`2"/> .</returns>
        public bool Remove(TextNeuron key)
        {
            fLock.EnterWriteLock();
            try
            {
                var iKey = key.Text.ToLower();
                ulong iId;
                if (fDict.TryGetValue(iKey, out iId) && iId == key.ID)
                {
                    IsChanged = true;
                    return fDict.Remove(iKey);
                }

                return false;
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Adds an element with the provided <paramref name="key"/> and<paramref name="value"/> to the dict</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same <paramref name="key"/> already exists in the<see cref="System.Collections.Generic.IDictionary`2"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IDictionary`2"/> is
        ///     read-only.</exception>
        public void Add(string key, ulong value)
        {
            fLock.EnterWriteLock();
            try
            {
                IsChanged = true;
                fDict.Add(key.ToLower(), value);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.IDictionary`2"/> contains an
        ///     element with the specified key.</summary>
        /// <param name="key">The key to locate in the<see cref="System.Collections.Generic.IDictionary`2"/> .</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <returns><see langword="true"/> if the<see cref="System.Collections.Generic.IDictionary`2"/> contains an
        ///     element with the key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            fLock.EnterReadLock();
            try
            {
                return fDict.ContainsKey(key.ToLower());
            }
            finally
            {
                fLock.ExitReadLock();
            }
        }

        /// <summary>Gets the neuron that corresponds to the specified string. If it didn't
        ///     yet exist, it is created. This is thread safe.</summary>
        /// <param name="value">The value.</param>
        /// <param name="freezeOn">The processor to freeze any newly created neurons on.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        public TextNeuron GetNeuronFor(string value, Processor freezeOn = null)
        {
            var iVal = value.ToLower();
            TextNeuron iRes = null;
            fLock.EnterUpgradeableReadLock();
            try
            {
                ulong iTextId;
                if (fDict.TryGetValue(iVal, out iTextId))
                {
                    // could be that this word already exists, if so, we reuse it, cause an item can only be once in the dict.
                    iRes = Brain.Current[iTextId] as TextNeuron;
#if DEBUG
                    if (iRes == null)
                    {
                        iRes = Brain.Current[iTextId] as TextNeuron;
                    }

#endif
                }
                else
                {
                    iRes = NeuronFactory.GetText(iVal);
                    Brain.Current.Add(iRes);
                    if (freezeOn != null)
                    {
                        iRes.SetIsFrozen(true, freezeOn);
                    }

                    fLock.EnterWriteLock();
                    try
                    {
                        IsChanged = true;
                        fDict.Add(iVal, iRes.ID);
                    }
                    finally
                    {
                        fLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                fLock.ExitUpgradeableReadLock();
            }

            return iRes;
        }

        /// <summary>Gets the ID of the textNeuron that corresponds to the specified
        ///     string. This is thread safe. if the neuron doesn't exist, it is
        ///     created.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong GetIDFor(string value)
        {
            var iVal = value.ToLower();
            ulong iTextId;
            fLock.EnterUpgradeableReadLock();
            try
            {
                if (fDict.TryGetValue(iVal, out iTextId) == false)
                {
                    // could be that this word already exists, if so, we reuse it, cause an item can only be once in the dict.
                    var iRes = NeuronFactory.GetText(iVal);
                    Brain.Current.Add(iRes);
                    fLock.EnterWriteLock();
                    try
                    {
                        IsChanged = true;
                        fDict.Add(iVal, iRes.ID);
                        return iRes.ID;
                    }
                    finally
                    {
                        fLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                fLock.ExitUpgradeableReadLock();
            }

            return iTextId;
        }

        /// <summary>Tries to get the neuron with the specified string key. if it doesn't
        ///     exist, <see langword="false"/> is returned.</summary>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the item was found, otherwise false.</returns>
        public bool TryGetNeuron(string key, out TextNeuron result)
        {
            ulong iFound;
            fLock.EnterReadLock();
            try
            {
                if (fDict.TryGetValue(key.ToLower(), out iFound))
                {
                    result = Brain.Current[iFound] as TextNeuron;
                    if (result != null)
                    {
                        return true;
                    }
                }
                else
                {
                    result = null;
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }

            return false;
        }

        /// <summary>The try get id.</summary>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool TryGetID(string key, out ulong result)
        {
            fLock.EnterReadLock();
            try
            {
                return fDict.TryGetValue(key.ToLower(), out result);
            }
            finally
            {
                fLock.ExitReadLock();
            }
        }

        #endregion
    }

    /// <summary>
    ///     an <see langword="enum" /> that provides thread save walk through the
    ///     words dict.
    /// </summary>
    public class WordsDictionaryEnum : System.Collections.Generic.IEnumerator<ulong>
    {
        /// <summary>The f source.</summary>
        private readonly System.Collections.Generic.IEnumerator<ulong> fSource;

        /// <summary>The f source lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fSourceLock;

        /// <summary>Initializes a new instance of the <see cref="WordsDictionaryEnum"/> class. Prevents a default instance of the <see cref="WordsDictionaryEnum"/>
        ///     class from being created.</summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceLock">The source Lock.</param>
        internal WordsDictionaryEnum(System.Collections.Generic.IEnumerator<ulong> source, 
            System.Threading.ReaderWriterLockSlim sourceLock)
        {
            System.Diagnostics.Debug.Assert(source != null && sourceLock != null);
            fSource = source;
            fSourceLock = sourceLock;
        }

        #region IEnumerator<ulong> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public ulong Current
        {
            get
            {
                return fSource.Current;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            fSource.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>Gets the current.</summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return fSource.Current;
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            fSourceLock.EnterReadLock();
            try
            {
                return fSource.MoveNext();
            }
            finally
            {
                fSourceLock.ExitReadLock();
            }
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first
        ///     element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            fSourceLock.EnterReadLock();
            try
            {
                fSource.Reset();
            }
            finally
            {
                fSourceLock.ExitReadLock();
            }
        }

        #endregion
    }
}