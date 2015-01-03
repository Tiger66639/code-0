// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDataDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   a Container for <see cref="BrainData" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     a Container for <see cref="BrainData" /> objects.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Makes certain that only <see cref="NeuronData" /> objects that contain
    ///         data are stored + that empty objects are available and commited when
    ///         needed.
    ///     </para>
    ///     <para>
    ///         When an item is requested that does not yet exist, it is created and
    ///         stored in a temp dict. The first time that one of the properties of the
    ///         item is changed, it is commited and moved to the actual list.
    ///     </para>
    ///     <para>
    ///         For this to work properly, <see cref="NeuronData" /> objects must be
    ///         cleaned up after usage, so they must implement iDisposable + this dict
    ///         uses a weakreference.
    ///     </para>
    ///     <para>
    ///         This is done so that we only store objects that have data since there can
    ///         be far more neurons than there can be nodes stored in a dictionary. With
    ///         this technique, we always provide an object, but don't need to have
    ///         ulong.max nr of objects.
    ///     </para>
    ///     <para>
    ///         Access to this object is made thread safe because
    ///         <see cref="NeuronData" /> can be created/accessed from multiple threads by
    ///         the debugger (when it creates <see cref="DebugNeuron" /> s.
    ///     </para>
    /// </remarks>
    public class NeuronDataDictionary : Data.ObservableObject, 
                                        System.Xml.Serialization.IXmlSerializable, 
                                        System.IDisposable
    {
        /// <summary>The datapath.</summary>
        public const string DATAPATH = "DesignerData";

        /// <summary>The cleanupdelay.</summary>
        public const int CLEANUPDELAY = 30000; // try to clean dead weakreferences  every so often.

        /// <summary>The notepadapifile.</summary>
        private const string NOTEPADAPIFILE =
            "<?xml version='1.0' ?><NotepadPlus><AutoComplete language='NNL'> <Environment ignoreCase='yes' startFunc='(' stopFunc=')' paramSeparator=',' terminal=';' additionalWordChar=''/>{0} </AutoComplete></NotepadPlus>";

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
            Free();
        }

        #endregion

        /// <summary>Gets the display title for the specified id. This is a shortcut: when
        ///     the neuronData object is not cached yet, it doesn't get cached by this
        ///     call.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetDisplayTitleFor(ulong id)
        {
            fLock.EnterUpgradeableReadLock();
            try
            {
                NeuronData iRes = null;
                object iFound;
                var iInCash = fItems.TryGetValue(id, out iFound);
                if (iInCash == false
                    || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
                {
                    if (fDeleted.Contains(id) == false)
                    {
                        iRes = Store.GetData(id);
                    }
                }

                if (iRes == null)
                {
                    if (iFound is NeuronData)
                    {
                        // if it's a strong ref, simply return this.
                        iRes = (NeuronData)iFound;
                    }
                    else if (iFound is System.WeakReference)
                    {
                        // a weak ref needs some more checking: could already have been unloaded since the last bit of code, so recheck and reload if needed.
                        iRes = (NeuronData)((System.WeakReference)iFound).Target;
                        if (iRes == null)
                        {
                            // this check is done in case it went dead in between the first check and this one. 
                            iRes = Store.GetData(id);
                        }
                    }
                    else
                    {
                        Neuron iTemp;
                        if (Brain.Current.TryFindNeuron(id, out iTemp))
                        {
                            var iText = iTemp as TextNeuron;

                                // it could be a textneuron that hasn't been loaded yet in the neuronDataCash, in which case we need to get the text value directly.
                            if (iText != null)
                            {
                                return iText.Text;
                            }
                        }
                    }
                }

                if (iRes != null)
                {
                    return iRes.DisplayTitle;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                fLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>a very fast routine for getting the 'Title' of a neuron.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetTitleFor(ulong id)
        {
            fLock.EnterReadLock();
            try
            {
                NeuronData iRes = null;
                object iFound;
                var iInCash = fItems.TryGetValue(id, out iFound);
                if (iInCash == false
                    || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
                {
                    if (fDeleted.Contains(id) == false)
                    {
                        return Store.GetDataTitle(id);
                    }
                }

                if (iRes == null)
                {
                    if (iFound is NeuronData)
                    {
                        // if it's a strong ref, simply return this.
                        iRes = (NeuronData)iFound;
                    }
                    else if (iFound is System.WeakReference)
                    {
                        // a weak ref needs some more checking: could already have been unloaded since the last bit of code, so recheck and reload if needed.
                        iRes = (NeuronData)((System.WeakReference)iFound).Target;
                        if (iRes == null)
                        {
                            // this check is done in case it went dead in between the first check and this one. 
                            iRes = Store.GetData(id);
                        }
                    }
                    else
                    {
                        Neuron iTemp;
                        if (Brain.Current.TryFindNeuron(id, out iTemp))
                        {
                            var iText = iTemp as TextNeuron;

                                // it could be a textneuron that hasn't been loaded yet in the neuronDataCash, in which case we need to get the text value directly.
                            if (iText != null)
                            {
                                return iText.Text;
                            }
                        }
                    }
                }

                if (iRes != null)
                {
                    return iRes.Title;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }
        }

        /// <summary>Cleans the database for the specified neuron: each<paramref name="data"/> block in the db is <see langword="checked"/>
        ///     to see if it is used by another neuron <paramref name="data"/> object.
        ///     If so, the datablock is removed from <paramref name="data"/> object
        ///     being cleaned.</summary>
        /// <param name="data">The data.</param>
        public void CleanDBFor(NeuronData data)
        {
            fStore.CleanFor(data);
        }

        /// <summary>
        ///     Makes ceratin that everything is loaded into memory. This is used for
        ///     loading templates.
        /// </summary>
        internal void TouchMem()
        {
            fLock.EnterWriteLock();
            try
            {
                for (var i = Neuron.StartId; i < Brain.Current.NextID; i++)
                {
                    if (Brain.Current.IsValidID(i))
                    {
                        var iRes = InternalTryGetNeuronData(i);
                        if (iRes != null)
                        {
                            fItems[i] = iRes;
                            iRes.SetChanged(true); // we simply set changed so it will be saved again.
                        }
                    }
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #region fields

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, object> fItems =
            new System.Collections.Generic.Dictionary<ulong, object>();

                                                                              // the buffer for neurondata that is not yet saved, but has been changed.

        /// <summary>The f deleted.</summary>
        private readonly System.Collections.Generic.HashSet<ulong> fDeleted =
            new System.Collections.Generic.HashSet<ulong>();

        /// <summary>The f lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fLock =
            new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

                                                               // lock used to make access thread safe (for debugger mostly)

        /// <summary>The f event monitor.</summary>
        private NeuronInfoEventMonitor fEventMonitor;

        /// <summary>The f store.</summary>
        private DesignerStore fStore;

        /// <summary>The f cleanup timer.</summary>
        private System.Threading.Timer fCleanupTimer;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronDataDictionary"/> class. 
        ///     Default constructor.</summary>
        public NeuronDataDictionary()
        {
            fEventMonitor = new NeuronInfoEventMonitor(this);
            fCleanupTimer = new System.Threading.Timer(CleanupTimer_Callback, null, CLEANUPDELAY, CLEANUPDELAY);
            Brain.Current.AutoFlushed += Current_AutoFlushed;
        }

        /// <summary>Finalizes an instance of the <see cref="NeuronDataDictionary"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="NeuronDataDictionary"/> is reclaimed by garbage
        ///     collection.</summary>
        ~NeuronDataDictionary()
        {
            Free();
        }

        /// <summary>
        ///     The store keeps file open which need closing + also a timer.
        /// </summary>
        private void Free()
        {
            if (fStore != null)
            {
                fStore.Dispose();
            }

            fStore = null;
            if (fCleanupTimer != null)
            {
                fCleanupTimer.Dispose();
            }

            fCleanupTimer = null;
            Brain.Current.AutoFlushed -= Current_AutoFlushed;
        }

        #endregion

        #region prop

        #region This

        /// <summary>Gets the extra info for the specified neuron, if it doesn't exist, one
        ///     is created and added to the temp list.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronData this[ulong id]
        {
            get
            {
                fLock.EnterUpgradeableReadLock();
                try
                {
                    return InternalGetNeuronData(id);
                }
                finally
                {
                    fLock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>Gets the <see cref="NeuronData"/> for the specified item. This is
        ///     more thread save than retrieving through id.</summary>
        /// <param name="item">The item.</param>
        /// <value></value>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        public NeuronData this[Neuron item]
        {
            get
            {
                if (item != null)
                {
                    fLock.EnterUpgradeableReadLock();
                    try
                    {
                        var iRes = InternalTryGetNeuronData(item);
                        if (iRes == null)
                        {
                            // if the item wasn't stored yet.
                            iRes = new NeuronData(item);
                            if (item.ID > (ulong)PredefinedNeurons.EndOfStatic)
                            {
                                // only store if hasn't been deleted already.
                                SetWeak(iRes); // this is a thread save add.
                            }
                            else if (item.ID > Neuron.EmptyId)
                            {
                                Commit(iRes);
                            }
                        }

                        return iRes;
                    }
                    finally
                    {
                        fLock.ExitUpgradeableReadLock();
                    }
                }

                return null;
            }
        }

        /// <summary>Thread unsafe way to get a neuronData object based on id. Only use for
        ///     module Export, sicne this locks the whole dict anyway.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        internal NeuronData InternalGetNeuronData(ulong id)
        {
            NeuronData iRes = null;
            object iFound;
            var iInCash = fItems.TryGetValue(id, out iFound);
            if (iInCash == false || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
            {
                if (fDeleted.Contains(id) == false)
                {
                    iRes = Store.GetData(id);
                }

                if (iRes == null)
                {
                    // if the item wasn't stored yet.
                    iRes = new NeuronData(id);
                }

                if (id > (ulong)PredefinedNeurons.EndOfStatic)
                {
                    SetWeak(iRes); // this is a thread save add.
                }
                else
                {
                    Commit(iRes); // the statics need to stay in mem, for speed.
                }
            }

            if (iRes == null)
            {
                if (iFound is NeuronData)
                {
                    // if it's a strong ref, simply return this.
                    iRes = (NeuronData)iFound;
                }
                else if (iFound is System.WeakReference)
                {
                    // a weak ref needs some more checking: could already have been unloaded since the last bit of code, so recheck and reload if needed.
                    iRes = (NeuronData)((System.WeakReference)iFound).Target;
                    if (iRes == null)
                    {
                        // this check is done in case it went dead in between the first check and this one. 
                        iRes = Store.GetData(id);
                        SetWeak(iRes); // this is a thread save add.
                    }
                }
            }

            return iRes;
        }

        #endregion

        #region Store

        /// <summary>
        ///     Gets the object that manages all the designer data that is stored on
        ///     the disk.
        /// </summary>
        public DesignerStore Store
        {
            get
            {
                if (fStore == null)
                {
                    fStore = new DesignerStore(Storage.NDB.NDBStore.DEFAULTNROFTABLES);
                }

                return fStore;
            }
        }

        #endregion

        #region StoragePath

        /// <summary>
        ///     Gets the path where all the neuronData is stored.
        /// </summary>
        /// <value>
        ///     The storage path.
        /// </value>
        public string StoragePath
        {
            get
            {
                if (string.IsNullOrEmpty(ProjectManager.Default.Location) == false)
                {
                    return System.IO.Path.Combine(ProjectManager.Default.Location, DATAPATH);
                }

                return null;
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     used by the undo system to (re)store the <see cref="NeuronData" />
        ///     associated with a neuron and by the montirong system.
        /// </summary>
        /// <remarks>
        /// </remarks>
        internal System.Collections.Generic.Dictionary<ulong, object> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region OverlayedItems

        /// <summary>
        ///     Gets all the <see cref="NeuronData" /> objects that have overlays
        ///     defined.
        /// </summary>
        /// <remarks>
        ///     This is used in the UI to present neurons that have known links which
        ///     can be reused, like in the flow editor, to share the same code
        ///     clusters.
        /// </remarks>
        /// <value>
        ///     The overlayed items.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.IEnumerable<NeuronData> OverlayedItems
        {
            get
            {
                var iRes = new System.Collections.Generic.List<NeuronData>();
                fLock.EnterReadLock();
                try
                {
                    foreach (var i in fItems)
                    {
                        var iData = i.Value as NeuronData;
                        if (iData == null)
                        {
                            var iRef = i.Value as System.WeakReference;
                            if (iRef.IsAlive)
                            {
                                iData = iRef.Target as NeuronData;
                            }
                        }

                        if (iData != null && iData.HasOverlays)
                        {
                            iRes.Add(iData);
                        }

                        // yield return iData;
                    }
                }
                finally
                {
                    fLock.ExitReadLock();
                }

                iRes.Sort((i, x) => i.DisplayTitle.CompareTo(x.DisplayTitle));

                    // we sort on display title for easy finding.
                return iRes;
            }
        }

        #endregion

        #region Locals

        /// <summary>
        ///     Gets all the currently loaded <see cref="NeuronData" /> objects,
        ///     sorted.
        /// </summary>
        /// <value>
        ///     The locals.
        /// </value>
        public System.Collections.Generic.IEnumerable<NeuronData> Locals
        {
            get
            {
                var iList = new System.Collections.Generic.List<NeuronData>();
                foreach (var i in fItems)
                {
                    var iData = i.Value as NeuronData;
                    if (iData == null)
                    {
                        var iRef = i.Value as System.WeakReference;
                        if (iRef.IsAlive)
                        {
                            iData = iRef.Target as NeuronData;
                        }
                    }

                    if (iData != null)
                    {
                        iList.Add(iData);
                    }
                }

                return Enumerable.OrderBy(iList, item => item.DisplayTitle);
            }
        }

        #endregion

        #region DefaultDataFile

        /// <summary>
        ///     Gets the filename and path to the dictionary containing the dict for
        ///     the data of the predefined neurons.
        /// </summary>
        public static string DefaultDataFile
        {
            get
            {
                return System.IO.Path.Combine(DefaultDataPath, "DefaultData.xml");
            }
        }

        #endregion

        #region DefaultDataPath

        /// <summary>
        ///     Gets the path to the dictionary containing the default data dict.
        /// </summary>
        public static string DefaultDataPath
        {
            get
            {
                return
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), 
                        "DefaultData");
            }
        }

        #endregion

        #region Lock

        /// <summary>
        ///     Gets the lock that protects the data. This is exposed so we can lock
        ///     the entire dict while saving the data, which is required to make
        ///     certain we don't get in a deadlock while saving the data (when a
        ///     neurondata object gets saved, it makes itself a weakref again, which
        ///     requires a write lock, but there could already be a an upgradable read
        ///     lock in another thread, which was trying to create a new neurondata
        ///     object and got stuck during the save process, in the brain object
        ///     itself.
        /// </summary>
        /// <value>
        ///     The lock.
        /// </value>
        internal System.Threading.ReaderWriterLockSlim Lock
        {
            get
            {
                return fLock;
            }
        }

        #endregion

        #endregion

        #region functions

        #region Temp management

        /// <summary>Called by the CleanupTimer, to remove all the weakreferences from the
        ///     dict that are no longer valid.</summary>
        /// <param name="extra">The extra.</param>
        private void CleanupTimer_Callback(object extra)
        {
            if (fLock.TryEnterUpgradeableReadLock(1000))
            {
                // this is a cleaner callback, if we can't clean up, try next time, but don't wait.
                try
                {
                    var iToRemove =
                        (from i in fItems
                         where i.Value is System.WeakReference && ((System.WeakReference)i.Value).IsAlive == false
                         select i.Key).ToList();
                    if (iToRemove.Count > 0)
                    {
                        fLock.EnterWriteLock();
                        try
                        {
                            foreach (var i in iToRemove)
                            {
                                fItems.Remove(i);
                            }
                        }
                        finally
                        {
                            fLock.ExitWriteLock();
                        }

                        OnPropertyChanged("OverlayedItems");

                            // in upgradeable lock, a read must be alowed. this prop triggers a read when ui elements update the list.
                    }
                }
                finally
                {
                    fLock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>moves a temp data neuron to the full list.</summary>
        /// <param name="item"></param>
        internal void Commit(NeuronData item)
        {
            fLock.EnterWriteLock();
            try
            {
                try
                {
                    fItems[item.ID] = item;
                }
                catch (System.Exception e)
                {
                    throw new System.ArgumentOutOfRangeException("Failed to store extra neuron data!", e);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            OnPropertyChanged("OverlayedItems");

                // must be outside write block. this prop triggers a read when ui elements update the list.
        }

        /// <summary>moves a permanent data neuron to the temp list again (because the
        ///     title, description,... were empty/released).</summary>
        /// <param name="item"></param>
        internal void SetWeak(NeuronData item)
        {
            fLock.EnterWriteLock();
            try
            {
                try
                {
                    fItems[item.ID] = new System.WeakReference(item);
                }
                catch (System.Exception e)
                {
                    throw new System.ArgumentOutOfRangeException("Failed to release extra neuron data!", e);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            OnPropertyChanged("OverlayedItems");

                // must be outside write block. this prop triggers a read when ui elements update the list.
        }

        #endregion

        #region Items

        /// <summary>Removes the <see cref="NeuronData"/> object from the<see cref="JaStDev.HAB.Designer.NeuronDataDictionary.Items"/> dict.
        ///     This is called when the corresponding neuron got deleted.</summary>
        /// <param name="id">The id.</param>
        internal void DeleteItem(ulong id)
        {
            fLock.EnterWriteLock();
            try
            {
                if (ContainsID(id))
                {
                    // don't  need to get the actual object, just check if it is there.
                    fItems.Remove(id);
                    if (fDeleted.Contains(id) == false)
                    {
                        // could already be in there from a previous unsaved delete.
                        fDeleted.Add(id);

                            // buffer for delete during save, don't do it now cause we might not want to save the project.
                    }
                }

                // if (iToRemove.OverlaysLoaded == true && fOverlayedItems != null)
                // fOverlayedItems.Remove(iToRemove);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            OnPropertyChanged("OverlayedItems");

                // must be outside write block. this prop triggers a read when ui elements update the list.
        }

        /// <summary>tries to find the NueornData object for the specified id.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        public NeuronData TryGetneuronData(ulong id)
        {
            fLock.EnterUpgradeableReadLock();
            try
            {
                var iRes = InternalTryGetNeuronData(id);
                return iRes;
            }
            finally
            {
                fLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>Tries to retrieve a neuron data object from cache or storage, but
        ///     doesn't create a new one or tries to change the cache. This is used
        ///     for removed neurondata object in response to deleted neurons.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        internal NeuronData InternalTryGetNeuronData(ulong id)
        {
            NeuronData iRes = null;
            object iFound;
            var iInCash = fItems.TryGetValue(id, out iFound);
            if (iInCash == false || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
            {
                if (fDeleted.Contains(id) == false)
                {
                    iRes = Store.GetData(id);
                }
            }

            if (iRes == null && iInCash)
            {
                if (iFound is NeuronData)
                {
                    // if it's a strong ref, simply return this.
                    iRes = (NeuronData)iFound;
                }
                else if (iFound is System.WeakReference)
                {
                    // a weak ref needs some more checking: could already have been unloaded since the last bit of code, so recheck and reload if needed.
                    iRes = (NeuronData)((System.WeakReference)iFound).Target;
                    if (iRes == null)
                    {
                        // this check is done in case it went dead in between the first check and this one. 
                        iRes = Store.GetData(id);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Tries to retrieve a neuron data object from cache or storage, but
        ///     doesn't create a new one or tries to change the cache. This is used to
        ///     retrieve</summary>
        /// <param name="item">The neuron to load designer data for.</param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        private NeuronData InternalTryGetNeuronData(Neuron item)
        {
            NeuronData iRes = null;
            object iFound;
            var iInCash = fItems.TryGetValue(item.ID, out iFound);
            if (iInCash == false || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
            {
                if (fDeleted.Contains(item.ID) == false)
                {
                    iRes = Store.GetData(item);
                }
            }

            if (iRes == null && iInCash)
            {
                if (iFound is NeuronData)
                {
                    // if it's a strong ref, simply return this.
                    iRes = (NeuronData)iFound;
                }
                else if (iFound is System.WeakReference)
                {
                    // a weak ref needs some more checking: could already have been unloaded since the last bit of code, so recheck and reload if needed.
                    iRes = (NeuronData)((System.WeakReference)iFound).Target;
                    if (iRes == null)
                    {
                        // this check is done in case it went dead in between the first check and this one. 
                        iRes = Store.GetData(item);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Determines whether the db contains the specified <paramref name="id"/>
        ///     without opening it.</summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if the specified <paramref name="id"/> contains ID;
        ///     otherwise, <c>false</c> .</returns>
        private bool ContainsID(ulong id)
        {
            object iFound;
            var iInCash = fItems.TryGetValue(id, out iFound);
            if (iInCash == false || (iFound is System.WeakReference && ((System.WeakReference)iFound).IsAlive == false))
            {
                if (fDeleted.Contains(id) == false)
                {
                    return Store.ContainsID(id);
                }
            }

            return iInCash;
        }

        /// <summary>Gets the <see cref="NeuronData"/> that is currently stored in the
        ///     cache for the specified id. If there is no cached item,<see langword="null"/> is returned.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="NeuronData"/>.</returns>
        internal NeuronData GetCached(ulong id)
        {
            object iFound;
            fLock.EnterReadLock();
            try
            {
                if (fItems.TryGetValue(id, out iFound))
                {
                    var iData = iFound as NeuronData;
                    if (iData == null)
                    {
                        var iRef = iFound as System.WeakReference;
                        if (iRef.IsAlive)
                        {
                            iData = iRef.Target as NeuronData;
                        }
                    }

                    return iData;
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }

            return null;
        }

        /// <summary>Adds a neuron <paramref name="data"/> item to the list and makes
        ///     certain that the <see cref="NeuronInfoDictionay.OverlayedItems"/>
        ///     list is updated. Also adds to the <see langword="namespace"/> if there
        ///     was any.</summary>
        /// <param name="id">The id.</param>
        /// <param name="data">The data.</param>
        internal void AddItem(ulong id, NeuronData data)
        {
            fLock.EnterWriteLock();
            try
            {
                Items[id] = data; // direct ref, so it can't be unloaded
                fDeleted.Remove(id);

                    // recycled objects shouldn't be deleted, this is just overhead during the save cause it will be overwritten anyway.

                // if (data.OverlaysLoaded == true && fOverlayedItems != null)                                                 //when we add an item, the overlay list has changed.
                // fOverlayedItems.Add(data);
            }
            finally
            {
                fLock.ExitWriteLock();
            }

            data.IsChanged = true;

                // we add, so it needs to be saved. Not really required anymore, cause it probably wasn't deleted in the first place because this gets called from the unod, but can't be certain, so always instruct to store the data. Needs to be outside write lock, otherwise we get recursive lock.
        }

        #endregion

        /// <summary>Finds the next item.</summary>
        /// <remarks>Does a case insensitive search.</remarks>
        /// <param name="id">The id to start searching from.</param>
        /// <param name="value">The value to search in the display title.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        internal ulong FindNext(ulong id, string value)
        {
            value = value.ToLower();
            NeuronData iFound = null;
            while (++id < Brain.Current.NextID)
            {
                Neuron iNeuron;
                if (Brain.Current.TryFindNeuron(id, out iNeuron))
                {
                    iFound = this[iNeuron];

                        // we use this getter since it will only create temp neurondata objects for non stored data, which is what we need cause we only want to do a compare anyway and throw the object away afterwards.
                    if (iFound != null && iFound.DisplayTitle != null && iFound.DisplayTitle.ToLower().Contains(value))
                    {
                        break;
                    }
                }
            }

            if (iFound != null)
            {
                return iFound.ID;
            }

            return Neuron.EmptyId;
        }

        /// <summary>
        ///     <para>
        ///         Called when a project is loaded, so that all the fixed items are able
        ///         to register the neurons that they wrap properly. This is required for
        ///         any <see cref="NeuronData" /> that was read from xml (default neurons,
        ///         old projects).
        ///     </para>
        ///     <para>Also used to open the data files.</para>
        /// </summary>
        /// <remarks>
        ///     doesn't need to be thread safe since it is called just after load.
        /// </remarks>
        internal void RegisterNeurons()
        {
            foreach (var i in fItems)
            {
                var iData = i.Value as NeuronData;
                if (iData == null)
                {
                    var iRef = i.Value as System.WeakReference;
                    if (iRef.IsAlive)
                    {
                        iData = iRef.Target as NeuronData;
                    }
                }

                if (iData != null)
                {
                    iData.RegisterNeuron();
                }
            }
        }

        /// <summary>
        ///     Saves the <see cref="NeuronData" /> for the
        ///     <see cref="PredefinedNeurons" /> to a file, so it can be auto loaded
        ///     in the future (only way to store the values for the statics). Will
        ///     also export all the instructions to an xml file that can be used by
        ///     notepad++ as an automcomplete list.
        /// </summary>
        public void SaveDefaultInfo()
        {
            var iToExport = new System.Collections.Generic.List<NeuronData>(); // so we can sort the list.
            var iKeyWords = new System.Text.StringBuilder();
            var iData = new Data.SerializableDictionary<ulong, NeuronData>();
            var iRtfDoc = new System.Windows.Documents.FlowDocument();

            for (var i = (ulong)PredefinedNeurons.PopInstruction; i < (ulong)PredefinedNeurons.EndOfStatic; i++)
            {
                iToExport.Add(this[i]);
            }

            iToExport.Sort((a, b) => a.DisplayTitle.CompareTo(b.DisplayTitle));

                // sort the list on display title so we can render all items in correctl sequence.
            foreach (var iFound in iToExport)
            {
                System.Diagnostics.Debug.Assert(iFound.ID != 0);
                if (iFound != null && iFound.NeedsPersisting)
                {
                    iData.Add(iFound.ID, iFound);
                }

                RenderKeyWord(iFound, iKeyWords, iRtfDoc);
            }

            var iPath = System.IO.Path.Combine(DefaultDataPath, "nnl.rtf"); // build the name of the file.
            using (var iStream = new System.IO.FileStream(iPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var iText = new System.Windows.Documents.TextRange(iRtfDoc.ContentStart, iRtfDoc.ContentEnd);
                iText.Save(iStream, System.Windows.DataFormats.Rtf);
            }

            using (
                var iStr = new System.IO.FileStream(
                    DefaultDataFile, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.Write))
            {
                var iSer =
                    new System.Xml.Serialization.XmlSerializer(typeof(Data.SerializableDictionary<ulong, NeuronData>));
                iSer.Serialize(iStr, iData);
            }

            iPath = System.IO.Path.Combine(DefaultDataPath, "nnl.xml"); // build the name of the file.
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var iWriter = new System.IO.StreamWriter(iStr);
                iWriter.Write(NOTEPADAPIFILE, iKeyWords);
                iWriter.Flush(); // need to do this, otherwise the endo fhte file isn't written.
            }
        }

        /// <summary>checks if the neuron in <paramref name="info"/> is an instruction, if
        ///     so, a keyword is generated in the <paramref name="result"/> string.</summary>
        /// <param name="info"></param>
        /// <param name="result"></param>
        /// <param name="iRtfDoc">The i Rtf Doc.</param>
        private void RenderKeyWord(
            NeuronData info, 
            System.Text.StringBuilder result, 
            System.Windows.Documents.FlowDocument iRtfDoc)
        {
            if (info.Neuron is Instruction)
            {
                var iName = ((PredefinedNeurons)info.ID).ToString();
                if (iName != null)
                {
                    var iL = iName.ToLower();
                    if (iL.EndsWith("instruction") && iL != "instruction")
                    {
                        iName = iName.Remove(iName.Length - 11);
                    }

                    result.AppendLine(string.Format("<KeyWord name='{0}' func='yes'></KeyWord>", iName));

                    var iSec = new System.Windows.Documents.Section();
                    var iPar = new System.Windows.Documents.Paragraph();
                    iPar.Inlines.Add(new System.Windows.Documents.Underline(new System.Windows.Documents.Run(iName)));
                    iSec.Blocks.Add(iPar);
                    iSec.Blocks.AddRange(info.Description.Blocks.ToList());
                    iPar = new System.Windows.Documents.Paragraph();
                    iSec.Blocks.Add(iPar);
                    iRtfDoc.Blocks.Add(iSec);
                }
            }
        }

        #region overlays

        /// <summary>
        ///     Called when the list of ovelays has changed and they all need to be
        ///     reloaded in each neuron data item.
        /// </summary>
        internal void ReloadOverlays()
        {
            fLock.EnterReadLock();
            try
            {
                foreach (var i in fItems)
                {
                    var iData = i.Value as NeuronData;
                    if (iData == null)
                    {
                        var iRef = i.Value as System.WeakReference;
                        if (iRef.IsAlive)
                        {
                            iData = iRef.Target as NeuronData;
                        }
                    }

                    if (iData != null)
                    {
                        iData.ReloadOverlays();
                    }
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }
        }

        ///// <summary>
        ///// Called when an item just build it's list of overlays.
        ///// </summary>
        ///// <param name="item">The neuron data.</param>
        // internal void SetItemHasOverlays(NeuronData item)
        // {
        // if (fOverlayedItems != null && fItems.ContainsKey(item.ID) == true && fOverlayedItems.Contains(item) == false)       //we only add data objects that have a fixed label (so in the items list), so we can provide a nice label, and don't let the list grow to big.
        // fOverlayedItems.Add(item);
        // }

        ///// <summary>
        ///// Called when an item has removed all the overlays.
        ///// </summary>
        ///// <param name="item">The neuron data.</param>
        // internal void RemoveItemHasOverlays(NeuronData item)
        // {
        // if (fOverlayedItems != null)
        // fOverlayedItems.Remove(item);
        // } 
        #endregion

        #endregion

        #region streaming

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            try
            {
                var iNrTables = Storage.NDB.NDBStore.DEFAULTNROFTABLES;
                if (XmlStore.TryReadElement(reader, "NrOfTables", ref iNrTables) == false)
                {
                    var iData = new Data.SerializableDictionary<ulong, NeuronData>();

                        // if no NrOfTables field, we are still in old format: a Serializable dictionary, so read and cnvert.

                    // reader.ReadStartElement("Items");
                    iData.ReadXml(reader);
                    fStore = new DesignerStore(iNrTables);
                    foreach (var i in iData)
                    {
                        i.Value.SetChanged(true);

                            // we indicate that the item was changed, so we will store it in the new format.  
                        fItems.Add(i.Key, i.Value);

                            // don't need to raise event handlers, simply add. This is during a load, so not required to raise events.
                    }
                }
                else
                {
                    fStore = new DesignerStore(iNrTables);
                    if (reader.Name == "NameSpaces")
                    {
                        // we simply skip the namespace data, this is no longe used.
                        reader.ReadOuterXml();
                    }
                }

                LoadDefaults();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("NeuronDataDictionary.ReadXml", e.ToString());
            }

            reader.ReadEndElement();
        }

        /// <summary>
        ///     Loads the defaults.
        /// </summary>
        internal void LoadDefaults()
        {
            var iPath = DefaultDataFile;
            if (System.IO.File.Exists(iPath))
            {
                Data.SerializableDictionary<ulong, NeuronData> iData;
                using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var iSer =
                        new System.Xml.Serialization.XmlSerializer(
                            typeof(Data.SerializableDictionary<ulong, NeuronData>));
                    iData = (Data.SerializableDictionary<ulong, NeuronData>)iSer.Deserialize(iStr);
                }

                foreach (var i in iData)
                {
                    if (fItems.ContainsKey(i.Key) == false && i.Value != null)
                    {
                        // we do this so we can convert existing projects easely from containing the default data locally, to externally.
                        fItems.Add(i.Key, i.Value);

                            // we add as a strong ref, the first items are never streamed but always kept in mem. Simply add, no event handlers, not required.
                    }
                }
            }
            else
            {
                LogService.Log.LogWarning(
                    "NeuronDataDictionary.LoadDefaults", 
                    string.Format("{0} not found, couldn't load info for default neurons!", iPath));
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "NrOfTables", Store.NrOfTables);
        }

        /// <summary>Saves the neuron data to the storage and makes the references weak
        ///     again.</summary>
        /// <param name="path">The path.</param>
        internal void SaveNeuronData(string path)
        {
            try
            {
                if (Store.IsLoaded == false)
                {
                    // if the project had not yet been saved, the descriptionTable isn't loaded yet (it has been buffering the data), we now know the path, so let it know.
                    Store.LoadFiles(path);
                }

                InternalSaveData();
            }
            catch (System.Exception e)
            {
                ProjectManager.Default.DataError = true;
                LogService.Log.LogError(
                    "NeuronDataDictionary.SaveNeuronData", 
                    string.Format("Failed to save extra neuron data with error: {0})", e));
            }
        }

        /// <summary>The internal save data.</summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void InternalSaveData()
        {
            foreach (var i in fDeleted)
            {
                Store.RemoveData(i);
            }

            fDeleted.Clear();

            System.Collections.Generic.List<NeuronData> iToSave;
            fLock.EnterReadLock();
            try
            {
                try
                {
                    iToSave = (from i in fItems
                               let iItem = i.Value as NeuronData
                               where iItem != null && i.Key >= (ulong)PredefinedNeurons.Dynamic && iItem.IsChanged
                               orderby iItem.ID
                               select iItem).ToList();

                        // first get a seperate list, so we can release all the saved items, if apprpropriate.
                }
                catch (System.Exception e)
                {
                    throw new System.ArgumentOutOfRangeException("Failed to store extra neuron data!", e);
                }
            }
            finally
            {
                fLock.ExitReadLock();
            }

            foreach (var i in iToSave)
            {
                if (i.ID > Neuron.EmptyId && i.NeedsPersisting && i.IsChanged)
                {
                    // this is an extra check. For some reason, a bug crawled in the dict that causes a save from new template to misbehave and have duplicate links to neurons. The first gets saved and unloaded, the second neuron also gets unloaded, but not the neurondata. this is caused by a data inconsistency (crasch)
                    Store.SaveData(i);

                        // we don't need to make weak again, this is done by the NeuronData object itself when it saves itself.
                }
                else
                {
                    Store.RemoveData(i);

                        // if the item is chagned, but NeedsPersisting == false, there is nothing to store anymore for the item, so remove the neurondata itself.
                }
            }

            Store.Flush();
        }

        /// <summary>Handles the AutoFlushed event of the Current control. Called when
        ///     there was an autosave. Will also stream all the data to disk</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Current_AutoFlushed(object sender, System.EventArgs e)
        {
            try
            {
                if (Store.IsLoaded)
                {
                    InternalSaveData();
                }
            }
            catch (System.Exception ex)
            {
                ProjectManager.Default.DataError = true;
                LogService.Log.LogError(
                    "NeuronDataDictionary.SaveNeuronData", 
                    string.Format("Failed to save extra neuron data with error: {0})", ex));
            }
        }

        #endregion
    }
}