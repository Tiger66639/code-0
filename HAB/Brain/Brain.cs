// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Brain.cs" company="">
//   
// </copyright>
// <summary>
//   The brain exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    #region Exceptions

    /// <summary>The brain exception.</summary>
    [System.Serializable]
    public class BrainException : System.Exception
    {
        // For guidelines regarding the creation of new exception types, see
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

        /// <summary>Initializes a new instance of the <see cref="BrainException"/> class.</summary>
        public BrainException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BrainException"/> class.</summary>
        /// <param name="message">The message.</param>
        public BrainException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BrainException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public BrainException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BrainException"/> class.</summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected BrainException(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion

    /// <summary>
    ///     Represents the entry point and central data store for a HAB system.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This object provides easy access to brain objects: it provides an indexed property get (
    ///         <see cref="Brain.this" />)
    ///         for retrieving ( - converting an ulong to) a neuron object.
    ///     </para>
    ///     <para>
    ///         There should only be 1 Brain object per application.  You can't create objects of this type, instead
    ///         use the <see cref="Brain.Current" /> static property to access the default object.
    ///         This is because this object manages a large set of data objects, these objects must have easy access
    ///         to the owning object without loosing to much memory though back references.
    ///     </para>
    ///     <para>
    ///         All operations on this object are thread save, so the brain can be used from different threads (important so
    ///         that different Sins can work independently).
    ///     </para>
    ///     <para>
    ///         The objects stored in the short term memory of the brain, are stored with a <see cref="System.WeakReference" />
    ///         so that they can be garbage collected (and streamed to disk) when no longer used anywhere and mem room is
    ///         required.
    ///         When items are loaded and added to the iternal dictionary, they are kept alive delibertly so that
    ///         they remain in memory even if they are not used at the moment.  This is done for all newly registered items. At
    ///         regular
    ///         time intervals, a clean should be performed by releasing the objects
    ///     </para>
    /// </remarks>
    [System.Xml.Serialization.XmlRoot("Network", Namespace = "http://www.janbogaerts.name", IsNullable = false)]
    public class Brain : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>Determines whether the specified ID exists or not.</summary>
        /// <remarks>Specifically checks the lists containing all the existing items instead of the lists containing
        ///     the deleted items (done by <see cref="Brain.IsDeletedID"/>). <see cref="Brain.IsValidId"/> is faster.</remarks>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if the id is assigned to a neuron; Deleted, reserved or out of range values return <c>false</c>.</returns>
        public bool IsExistingID(ulong id)
        {
            var iRes = IsValidID(id);
            if (iRes == false)
            {
                return false;
            }

            LockManager.Current.RequestCacheLock(id, false);
            try
            {
                return fNeuronCash.ContainsKey(id) || Storage.IsExistingID(id);
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(id, false);
            }
        }

        /// <summary>Determines whether the specified ID is was used but has been deleted.</summary>
        /// <remarks>Specifically checks the internal 'free' list instead of the lists containing
        ///     the existing items (done by <see cref="Brain.IsExisting"/>).</remarks>
        /// <param name="id">The id to check.</param>
        /// <returns><c>true</c> if the id is free (no neuron is using it); otherwise, <c>false</c>.</returns>
        public bool IsDeletedID(ulong id)
        {
            return fIDs.IsDeleted(id);
        }

        /// <summary>Determines whether the specified id is valid: within range of the existing items and not deleted: so a neuron
        ///     should
        ///     exist for the id.</summary>
        /// <remarks>This is faster than <see cref="Brain.IsExisting"/></remarks>
        /// <param name="id">The id to verify.</param>
        /// <returns><c>true</c> if the specified id is valid; otherwise, <c>false</c>.</returns>
        public bool IsValidID(ulong id)
        {
            if (id > NextID || id == Neuron.EmptyId)
            {
                return false;
            }

            if (id >= (ulong)PredefinedNeurons.EndOfStatic && id < (ulong)PredefinedNeurons.Dynamic)
            {
                return false;
            }

            return !fIDs.IsDeleted(id);
        }

        /// <summary>
        ///     Loads all the neurons into the cash.
        ///     Also makes certain that the sensory interfaces load all their extra data into memory.
        /// </summary>
        /// <remarks>
        ///     This is done by manually adding every item that was found in the storage, to the cache.
        /// </remarks>
        public void TouchMem()
        {
            LockManager.Current.RequestCacheLock(true);
            try
            {
                for (var i = Neuron.StartId; i < fIDs.NextID; i++)
                {
                    if (IsValidID(i))
                    {
                        Neuron iFound;
                        LinkResolverData iData = null;
                        var iInCach = TryFindInCashUnsafe(i, out iFound);
                        if (iInCach == false)
                        {
                            // sins are already in the cache, make certain we don't create new object that aren't needed.
                            iData = Storage.GetNeuron(i);
                        }

                        if (iData != null)
                        {
                            iFound = iData.ToResolve;
                            if (iInCach == false)
                            {
                                Neuron.LinkResolver.Default.Resolve(iData, fNeuronCash);
                                fNeuronCash[i] = iFound; // always make certain taht it it the neuron and not a weakRef
                                Neuron.LinkResolver.Default.Remove(iData.ToResolve);
                            }

                            iData.Recycle();
                        }

                        if (iFound != null)
                        {
                            // if it's null, there is actually an inconsistency in the db, but this shouldn't prevent us from doing a touchmem.
                            iFound.SetIsChangedDirect();
                        }
                    }
                }

                foreach (var iSin in fSins)
                {
                    // use the general method of asking each Sin to store extra data.
                    iSin.TouchMem();
                }

                if (TextSin.Words.Count != 0)
                {
                    // this statement's purpose is to call 'Words' cause this will load the dict into memory.   
                    TextSin.Words.IsChanged = true; // for touchmem, need to make certain taht the dict is saved again.
                }

                Time.TouchMem();
                Modules.TouchMem();
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(true);
                Neuron.LinkResolver.Default.Clear();
            }
        }

        /// <summary>Makes certain that the specified neuron is stored as a weak reference. Note, this is
        ///     a thread safe function.</summary>
        /// <param name="item">The item.</param>
        internal void MakeWeak(Neuron item)
        {
            LockManager.Current.RequestCacheLock(item.ID, true);
            try
            {
                fNeuronCash[item.ID] = new System.WeakReference(item); // make it unloadable again.
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(item.ID, true);
            }
        }

        /// <summary>This has been added to fix a small bug in the early version that didn't add the static sins
        ///     to the sins list. This fixes those projects.</summary>
        /// <param name="toAdd">To add.</param>
        public void AddSin(Sin toAdd)
        {
            if (fSins.Contains(toAdd) == false)
            {
                fSins.Add(toAdd);
            }
        }

        /// <summary>The dump locks.</summary>
        public void DumpLocks()
        {
            LockManager.Current.Dump();
        }

        /// <summary>The dump locks save.</summary>
        public void DumpLocksSave()
        {
            LockManager.Current.DumpSave();
        }

        #region fields

        /// <summary>The f current.</summary>
        private static Brain fCurrent;

        /// <summary>The f neuron cash.</summary>
        private MultiTrackCache fNeuronCash = new MultiTrackCache();

                                // stores all neurons and weakReferences that are currently loaded in memory. This object is used as a lock for thread safe operations.

        /// <summary>The f flusher.</summary>
        private AutoFlusher fFlusher;

        /// <summary>The f sins.</summary>
        private System.Collections.Generic.List<Sin> fSins = new System.Collections.Generic.List<Sin>();

        /// <summary>The f timers.</summary>
        private System.Collections.Generic.List<ulong> fTimers = new System.Collections.Generic.List<ulong>();

        /// <summary>The f i ds.</summary>
        private IDManager fIDs = new IDManager();

        /// <summary>The f neurons to delete.</summary>
        private System.Collections.Generic.HashSet<ulong> fNeuronsToDelete =
            new System.Collections.Generic.HashSet<ulong>();

                                                          // when Settings.AutoSaveNeurons == false, we can't remove neurons from the db backend untill a save instruction is issued.

        /// <summary>The f is changed.</summary>
        private bool fIsChanged;

        /// <summary>The f is initialized.</summary>
        private bool fIsInitialized;

        /// <summary>The f modules.</summary>
        private Modules fModules = new Modules();

        #endregion

        #region Events

        /// <summary>
        ///     Raised when a link between 2 neurons is changed.
        /// </summary>
        public event LinkChangedEventHandler LinkChanged;

        /// <summary>
        ///     Raised when something in the list of neurons has changed (add, remove or change).
        /// </summary>
        public event NeuronChangedEventHandler NeuronChanged;

        /// <summary>
        ///     Occurs when a neuron got unfrozen again because it was changed after it had been frozen.
        /// </summary>
        public event FreezeEventHandler NeuronUnfrozen;

        /// <summary>
        ///     Raised when a <see cref="NeuronList" /> was changed.
        /// </summary>
        public event NeuronListChangedEventHandler NeuronListChanged;

        /// <summary>
        ///     Occurs when the brain is cleared completely (all neurons are removed).
        /// </summary>
        public event System.EventHandler Cleared;

        /// <summary>
        ///     Occurs when a new data set has been loaded and the brain is ready for operation.
        /// </summary>
        public event System.EventHandler Loaded;

        /// <summary>
        ///     Occurs when the state of the database is changed from saved to changed or visa versa.
        /// </summary>
        /// <remarks>
        ///     Designers can use this to monitor any changes.
        /// </remarks>
        public event System.EventHandler Changed;

        /// <summary>
        ///     called when an auto-flush happened (during automatic streaming). This can be used
        ///     for a designer to also save it's data.
        /// </summary>
        public event System.EventHandler AutoFlushed;

        #endregion

        #region ctor

        /// <summary>Prevents a default instance of the <see cref="Brain"/> class from being created. 
        ///     Privates constructor so that no objects but the current can be used (singleton pattern).</summary>
        private Brain()
        {
            Storage = GetDefaultStorage();
        }

        /// <summary>
        ///     Creates a new storage object, based on the default <see cref="Settings" />.
        /// </summary>
        /// <returns>an empty storage system.</returns>
        private ILongtermMem GetDefaultStorage()
        {
            switch (Settings.DefaultStorageSystem)
            {
                case NeuronStorageSystem.NDB:
                    return new Storage.NDB.NDBStore();
                case NeuronStorageSystem.Xml:
                    return new LongtermMem();
                default:
                    throw new System.InvalidOperationException("Unkown storage system requested.");
            }
        }

        #endregion

        #region prop

        #region this

        /// <summary>gets the neuron object with the specified id (if available).</summary>
        /// <remarks>This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.
        ///     It is also sub processor safe.  That is, if the current thread's processor is a sub processor, it will first
        ///     try to get a local clone of the neuron.</remarks>
        /// <exception cref="System.IndexOutOfRangeException">There is no valid Neuron object using the specified id</exception>
        /// <param name="id">The identifier of the neuron.</param>
        /// <returns>The Neuron object with the specified id.</returns>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron this[ulong id]
        {
            get
            {
                Neuron iRes = null;
                try
                {
                    InternalTryFindNeuron(id, out iRes);
                }
                catch (System.Exception e)
                {
                    throw new System.InvalidOperationException(
                        string.Format("No neuron found with the specified id: {0}.", id), 
                        e);
                }

                if (iRes != null)
                {
                    if (Settings.TrackNeuronAccess && iRes.AccessCounter + 1 > iRes.AccessCounter)
                    {
                        // make certain we don't cause overflow with add.
                        iRes.AccessCounter++; // indicate it has been accessed so we can later optimize for it.
                    }

                    return iRes;
                }

                throw new System.InvalidOperationException(
                    string.Format("No neuron found with the specified id: {0}.", id));
            }
        }

        #endregion

        #region Current

        /// <summary>
        ///     Gets a reference to the currently active Brain.
        /// </summary>
        public static Brain Current
        {
            get
            {
                if (fCurrent == null)
                {
                    fCurrent = new Brain();
                }

                return fCurrent;
            }
        }

        #endregion

        #region HasLinkChangedEvents

        /// <summary>
        ///     Gets if there are delegates registered for the <see cref="Brain.LinkChanged" /> event.
        ///     This can be used as a small optimization so you don't always have to create a
        ///     <see cref="LinkChangedEventArgs" /> object each time you have to call <see cref="Brain.OnLinkChanged" />.
        /// </summary>
        public bool HasLinkChangedEvents
        {
            get
            {
                return LinkChanged != null;
            }
        }

        #endregion

        #region HasNeuronChangedEvents

        /// <summary>
        ///     Gets if there are delegates registered for the <see cref="Brain.NeuronChanged" /> event.
        ///     This can be used as a small optimization so you don't always have to create a
        ///     <see cref="NeuronChangedEventArgs" /> object each time you have to call <see cref="Brain.OnNeuronChanged" />.
        /// </summary>
        public bool HasNeuronChangedEvents
        {
            get
            {
                return NeuronChanged != null;
            }
        }

        #endregion

        #region HasNeuronChangedEvents

        /// <summary>
        ///     Gets if there are delegates registered for the <see cref="Brain.NeuronChanged" /> event.
        ///     This can be used as a small optimization so you don't always have to create a
        ///     <see cref="NeuronChangedEventArgs" /> object each time you have to call <see cref="Brain.OnNeuronChanged" />.
        /// </summary>
        public bool HasNeuronListChangedEvents
        {
            get
            {
                return NeuronListChanged != null;
            }
        }

        #endregion

        #region LastID

        /// <summary>
        ///     Gets the last used ID.
        /// </summary>
        /// <value>The last ID.</value>
        public ulong NextID
        {
            get
            {
                return fIDs.NextID;
            }
        }

        #endregion

        #region NeuronCount

        /// <summary>
        ///     Gets the total number of neurons that the brain currently has.
        /// </summary>
        /// <remarks>
        ///     This is primarely for display purposes.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public ulong NeuronCount
        {
            get
            {
                return fIDs.NeuronCount;
            }
        }

        #endregion

        /// <summary>
        ///     gets the total nr of items currently in the neuron cache.
        /// </summary>
        public long CacheCount
        {
            get
            {
                return fNeuronCash.CountLong;
            }
        }

        #region Storage

        /// <summary>
        ///     Gets the long term storage container for Neuron objects.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is currently always a <see cref="LongtermMem" /> type.
        ///     </para>
        ///     <para>
        ///         The reasonf for using a storage is so that this could be replaced later on with other types of storage.
        ///     </para>
        /// </remarks>
        public ILongtermMem Storage { get; private set; }

        /// <summary>Replaces the storage object with a new one. Warning, when assigning the new storage, no data is copied.
        ///     This needs to be done manually, before this call. It is provided to allow for upgrading to a new storage
        ///     mechanisme (when file formats have changed for instance).</summary>
        /// <param name="storage">The storage.</param>
        public void ReplaceStorage(ILongtermMem storage)
        {
            Storage = storage;
        }

        #endregion

        #region Modules

        /// <summary>
        ///     Gets the object that manages all the modules in the network.
        /// </summary>
        public Modules Modules
        {
            get
            {
                return fModules;
            }
        }

        #endregion

        #region Flusher

        /// <summary>
        ///     Gets/sets the object used to save neurons to storage during normal run, when the network is configured to always
        ///     stream
        /// </summary>
        internal AutoFlusher Flusher
        {
            get
            {
                if (fFlusher == null)
                {
                    fFlusher = new AutoFlusher();
                }

                return fFlusher;
            }

            set
            {
                if (fFlusher != value)
                {
                    if (value == null)
                    {
                        Flusher.Stop();
                    }

                    fFlusher = value;
                }
            }
        }

        #endregion

        #region HasFlusher

        /// <summary>
        ///     Gets the wether there is a flusher object currently loaded or not.
        /// </summary>
        public bool HasFlusher
        {
            get
            {
                return fFlusher != null;
            }
        }

        #endregion

        #region Sins

        /// <summary>
        ///     Gets the list of <see cref="Sin" />s that are used by the brain
        ///     for input/output (interaction).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ReadOnlyCollection<Sin> Sins
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Sin>(fSins);
            }
        }

        #endregion

        #region Timers

        /// <summary>
        ///     Gets the list of <see cref="Sin" />s that are used by the brain
        ///     for input/output (interaction).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ReadOnlyCollection<ulong> Timers
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<ulong>(fTimers);
            }
        }

        #endregion

        #region IsChanged

        /// <summary>
        ///     Gets wether the network has been changed since the last save.
        /// </summary>
        /// <remarks>
        ///     Also triggers the <see cref="Brain.Changed" /> event when this value changes.
        /// </remarks>
        public bool IsChanged
        {
            get
            {
                return fIsChanged;
            }

            internal set
            {
                if (fIsChanged != value)
                {
                    fIsChanged = value;
                    if (Changed != null)
                    {
                        Changed(this, System.EventArgs.Empty);
                    }
                }
            }
        }

        #endregion

        #region IsInitialized

        /// <summary>
        ///     Gets/sets the wether the network has been properly initialized or not.
        ///     An initialization can be done by loading an aleady existing network, or by creating a new one
        ///     through <see cref="Brain.New" />
        /// </summary>
        /// <remarks>
        ///     Whenever the network gets initialized, some default neuron refs are also stored for fast execution.
        /// </remarks>
        public bool IsInitialized
        {
            get
            {
                return fIsInitialized;
            }

            internal set
            {
                fIsInitialized = value;
                if (value)
                {
                    TrueNeuron = Current[(ulong)PredefinedNeurons.True];
                    FalseNeuron = Current[(ulong)PredefinedNeurons.False];
                }
                else
                {
                    TrueNeuron = null;
                    FalseNeuron = null;
                }
            }
        }

        #endregion

        #region IsEditMode

        /// <summary>
        ///     gets/set wether the network is currently in edit mode or run mode.
        ///     This effects how neuron id's are rendered: when in edit mode, id's are rendered
        ///     in sequence, compacting the list as soon as possible and always trying to re-use id's as
        ///     soon as possible.
        ///     In Run mode, id's are rendered in a more thread secure way, so that deleted neurons
        ///     aren't re-used before all the references to the neuron that previously held the id, are removed.
        ///     By default, a network always starts in run mode, but an editor can temporarely switch this off while editing.
        ///     Ideally, the <see cref="ThreadManager.ActivityStarted" /> and <see cref="ThreadManager.ActivityStopped" /> can
        ///     be used to switch between modes.
        /// </summary>
        /// <summary>
        ///     Gets the neuron that represents the 'true' value.
        /// </summary>
        public Neuron TrueNeuron { get; private set; }

        #endregion

        #region FalseNeuron

        /// <summary>
        ///     Gets the neuron that represents the 'false' state.
        /// </summary>
        public Neuron FalseNeuron { get; private set; }

        #endregion

        #endregion

        #region event callers

        /// <summary>Called when a neuron got unfrozen. Triggers the event when needed.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void OnNeuronUnFrozen(Neuron neuron)
        {
            if (NeuronUnfrozen != null)
            {
                NeuronUnfrozen(this, new FreezeEventAgrs { Neuron = neuron });
            }
        }

        /// <summary>Raises the <see cref="Brain.LinkChanged"/> event.</summary>
        /// <remarks>Also see <see cref="Brain.HasLinkChangedEvents"/>.</remarks>
        /// <param name="e">The event arguments.</param>
        internal void OnLinkChanged(LinkChangedEventArgs e)
        {
            if (LinkChanged != null)
            {
                LinkChanged(this, e);
            }
        }

        /// <summary>Raises the <see cref="Brain.NeuronChanged"/> event.</summary>
        /// <remarks>Also see <see cref="Brain.HasNeuronChangedEvents"/>.
        ///     Only events for registered or temp neurons are raised.</remarks>
        /// <param name="e">The event arguments.</param>
        internal void OnNeuronChanged(NeuronChangedEventArgs e)
        {
            if (NeuronChanged != null && e.OriginalSource.ID > Neuron.EmptyId)
            {
                NeuronChanged(this, e);
            }
        }

        /// <summary>Raises the <see cref="Brain.NeuronChanged"/> event.</summary>
        /// <remarks>Also see <see cref="Brain.HasNeuronChangedEvents"/>.
        ///     Only events for registered or temp neurons are raised.</remarks>
        /// <param name="e">The event arguments (the actual list that changed can be found here + more data about what changed..</param>
        internal void OnNeuronListChanged(NeuronListChangedEventArgs e)
        {
            if (NeuronListChanged != null)
            {
                NeuronListChanged(this, e);
            }
        }

        #endregion

        #region functions

        /// <summary>provides a speed optimisation by preloading the code neurons that start at the specified root object (it's
        ///     rules/actions and childrend will be loaded).
        ///     this is done async.</summary>
        /// <param name="start"></param>
        public void LoadCodeAt(ulong start)
        {
            CodeLoader.Start(start);
        }

        /// <summary>Tries to find the neuron with the specified id, if it can't find it, false is returned.</summary>
        /// <remarks>Catches all exceptions and logs them.</remarks>
        /// <param name="id">The id of the neuron to find.</param>
        /// <param name="found">The found.</param>
        /// <returns>True if we found the item, otherwise false.</returns>
        public bool TryFindNeuron(ulong id, out Neuron found)
        {
            try
            {
                return InternalTryFindNeuron(id, out found);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Brain.TryFindNeuron", e.ToString());
            }

            found = null; // in case there was an error.
            return false;
        }

        /// <summary>TryFind without a catch, it lets any exception triple up.</summary>
        /// <param name="id">The id.</param>
        /// <param name="found">The found.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool InternalTryFindNeuron(ulong id, out Neuron found)
        {
            object iRef = null;
            found = null;
            LockManager.Current.RequestCacheLock(id, false);
            try
            {
                if (fNeuronCash.TryGetValue(id, out iRef))
                {
                    if (iRef is Neuron)
                    {
                        found = (Neuron)iRef;
                    }
                    else if (iRef is System.WeakReference && ((System.WeakReference)iRef).IsAlive)
                    {
                        found = (Neuron)((System.WeakReference)iRef).Target;
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(id, false);
            }

            if (found == null && IsValidID(id))
            {
                // we only need to check if it is a valid id when it is not in the cache. Everything in the cache is valid.
                found = GetFromStorage(id);
            }

            return found != null;
        }

        /// <summary>Tries to load all the specified neurons.</summary>
        /// <param name="id">The list of ids to load.</param>
        /// <param name="result">The result list.</param>
        /// <returns>True if all neurons were found, otherwise false.</returns>
        public bool TryFindNeurons(System.Collections.Generic.List<ulong> id, System.Collections.Generic.List<Neuron> result)
        {
            try
            {
                var iAllFound = true;
                LockManager.Current.RequestCacheLock(false);
                try
                {
                    object iRef;
                    foreach (var i in id)
                    {
                        if (fNeuronCash.TryGetValue(i, out iRef))
                        {
                            if (iRef is Neuron)
                            {
                                result.Add((Neuron)iRef);
                            }
                            else if (iRef is System.WeakReference && ((System.WeakReference)iRef).IsAlive)
                            {
                                result.Add((Neuron)((System.WeakReference)iRef).Target);
                            }
                            else
                            {
                                result.Add(null);
                                iAllFound = false;
                            }
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseCacheLock(false);
                }

                if (iAllFound == false)
                {
                    iAllFound = true;
                    for (var i = 0; i < id.Count; i++)
                    {
                        if (result[i] == null)
                        {
                            result[i] = GetFromStorage(id[i]);
                            if (result[i] == null)
                            {
                                iAllFound = false;
                            }
                        }
                    }
                }

                return iAllFound;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Brain.TryFindNeurons", e.ToString());
            }

            return false;
        }

        /// <summary>Gets the id from storage. When another thread was reading it from storage, this function
        ///     waits and gets it out of the cash afterwarths.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetFromStorage(ulong id)
        {
            Neuron iFound = null;
            if (Storage != null && string.IsNullOrEmpty(Storage.DataPath) == false)
            {
                // check for the existence of a storage path, if there is none, can't load from db.
                if (LockManager.Current.RequestStorageReadLock(id))
                {
                    try
                    {
                        iFound = TryGetFromCache(id);
                        if (iFound == null)
                        {
                            var iToResolve = Storage.GetNeuron(id);
                            if (iToResolve != null)
                            {
                                iFound = iToResolve.ToResolve;
                                AddToCashAndResolveLinks(iToResolve);

                                    // has to be inside StorageLock, otherwise the same id can be read multiple times in different objects, causing strange results.
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseStorageReadLock(id);
                    }
                }
                else
                {
                    LockManager.Current.ReleaseStorageReadLock(id);

                        // still need to unleash the lock, cause we have been waiting for it.
                    iFound = TryGetFromCache(id);

                        // the id was still locked, so when we get back, it has just been added to the cache, no need to try and read it again.
#if DEBUG
                    System.Diagnostics.Debug.Assert(iFound != null && iFound.ID == id);
#endif
                }
            }

            return iFound;
        }

        /// <summary>The try get from cache.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryGetFromCache(ulong id)
        {
            Neuron iFound = null;
            object iRef = null;
            LockManager.Current.RequestCacheLock(id, false);

                // before getting from storage, check that a previous thread hasn't done so, just before we got the lock.
            try
            {
                fNeuronCash.TryGetValue(id, out iRef);
                if (iRef is Neuron)
                {
                    iFound = (Neuron)iRef;
                }
                else if (iRef is System.WeakReference && ((System.WeakReference)iRef).IsAlive)
                {
                    iFound = (Neuron)((System.WeakReference)iRef).Target; // this can still have been unloaded.
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(id, false);
            }

            return iFound;
        }

        /// <summary>Checks if the specified id is loaded in the cash, if so, it retuns the neuron that was found.</summary>
        /// <param name="id">The id to search for.</param>
        /// <param name="result">The neuron that was found or null.</param>
        /// <returns>True if there was a neuron in the cash with the specified id.</returns>
        public bool TryFindInCash(ulong id, out Neuron result)
        {
            LockManager.Current.RequestCacheLock(id, false);
            try
            {
                return TryFindInCashUnsafe(id, out result);
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(id, false);
            }
        }

        /// <summary>Checks if the specified id is loaded in the cash, if so, it retuns the neuron that was found.
        ///     doesn't lock the cache.</summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool TryFindInCashUnsafe(ulong id, out Neuron result)
        {
            result = null;
            object iRef = null;
            if (fNeuronCash.TryGetValue(id, out iRef) == false
                || (iRef is System.WeakReference && ((System.WeakReference)iRef).IsAlive == false))
            {
                return false;
            }

            if (iRef is Neuron)
            {
                result = (Neuron)iRef;
            }
            else if (iRef is System.WeakReference)
            {
                result = (Neuron)((System.WeakReference)iRef).Target;
            }

            return result != null;
        }

        /// <summary>Creates a new Current object and loads the values for the properties from the specified stream.</summary>
        /// <remarks>No checks are performed wether it is actually possible to load a new network. Any processors that were still
        ///     running
        ///     should have to be manually stopped before calling this method.
        ///     You should call <see cref="Brain.Clear"/> before loading a new project into a brain that had already items loaded.
        ///     Note: after the project is loaded, there is no automatic call to the network activity started event, this needs
        ///     to be done manually. This allows the caller to deside when the startup code is called exactly.</remarks>
        /// <param name="path">The path.</param>
        /// <param name="readOnly">The read Only.</param>
        public static void Load(string path, bool readOnly = false)
        {
            using (var iStr = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iSettings = new System.Xml.XmlReaderSettings();
                iSettings.IgnoreComments = true;
                iSettings.IgnoreWhitespace = true;
                using (var iReader = System.Xml.XmlReader.Create(iStr, iSettings))
                {
                    iReader.Read();
                    iReader.Read();
                    Current.ReadXml(iReader);
                }
            }

            Current.Storage.MakePathAbsoluteTo(path, readOnly);
            Current.Modules.MakePathAbsoluteTo(path);
            foreach (var i in fCurrent.Storage.GetEntryNeuronsFor(typeof(Brain)))
            {
                if (i is Sin)
                {
                    fCurrent.fSins.Add((Sin)i);
                }
                else if (i is TimerNeuron)
                {
                    fCurrent.fTimers.Add(i.ID);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        string.Format("Sins expected as the entry points for the brain, found: {0}!", i.GetType()));
                }
            }

            Current.OnLoaded();
        }

        /// <summary>The on loaded.</summary>
        private void OnLoaded()
        {
            Current.IsChanged = false; // when just loaded, can't be changed.
            Current.IsInitialized = true;
            if (Current.Loaded != null)
            {
                Current.Loaded(Current, System.EventArgs.Empty);
            }

            // CallNetActivityEvent((ulong)PredefinedNeurons.OnStarted);                                                       //don't do this, let the caller do OnStarted, this way , he can select when this needs to be called.
        }

        /// <summary>Calls the network activity event (OnStarted, OnShutDown, on SinActivity,...), if there is any code and
        ///     if it is appropriate to raise them according to the current engine settings.</summary>
        /// <param name="id">The id.</param>
        public void CallNetActivityEvent(ulong id)
        {
            if (Settings.RaiseNetworkEvents)
            {
                try
                {
                    NeuronCluster iCluster;
                    Neuron iFound;
                    if (Current.TryFindNeuron(id, out iFound))
                    {
                        iCluster = iFound as NeuronCluster;
                        if (iCluster != null)
                        {
                            using (var iList = iCluster.Children)
                                if (iList.Count > 0)
                                {
                                    var iProc = ProcessorFactory.GetProcessor();
                                    iProc.CallSingle(iCluster);
                                }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Brain.CallNetActivityEvent", e.ToString());
                }
            }
        }

        /// <summary>
        ///     Clears all the data from the db and closes all possibly running processors.
        ///     Also clears out all the factories.
        /// </summary>
        /// <remarks>
        ///     Also raises the <see cref="Brain.Cleared" /> event.
        /// </remarks>
        public void Clear()
        {
            StopTimers();
            CallNetActivityEvent((ulong)PredefinedNeurons.OnShutDown);
            fIDs.GetWriteLock();
            LockManager.Current.RequestCacheLock(false);
            try
            {
                foreach (var i in fNeuronCash)
                {
                    // when we clear the data, we need to let each neuron know it is no longer associated with the brain, otherwise the neuron will try to save itself.
                    if (i.Value is System.WeakReference)
                    {
                        var iVal = (System.WeakReference)i.Value;
                        if (iVal.IsAlive)
                        {
                            ((Neuron)iVal.Target).SetId(Neuron.EmptyId);
                        }
                    }
                    else if (i.Value is Neuron)
                    {
                        ((Neuron)i.Value).SetId(Neuron.EmptyId);
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Unknown type found in neuroncash, can't clear.");
                    }
                }

                fNeuronCash.Clear();
                fSins.Clear();
                fTimers.Clear();
                fNeuronsToDelete.Clear();

                    // don't forget to clear the items that are in the deletion queue, otherwise these could give a problem.
                Storage.Dispose(); // make certain that the storage system knows it is being closed.
                CacheBuffer.Default.Clear(); // don't forget the buffer, don't need to keep anything in mem.
                Storage = GetDefaultStorage();

                    // we also create a new mem manager, for the next one. have to do dispose before creating new one, to make certain all previous files are closed.
                fIDs.ClearUnsave();

                    // use the unsave version. The lock doesn't support recursion and we already locked the id manager.
                TextSin.ResetDict(); // we also need to reset the dictionary.
                if (Cleared != null)
                {
                    Cleared(this, System.EventArgs.Empty);
                }

                Time.Reset();
                Factories.Default.Clear();
                NeuronFactory.Clear();
                Flusher = null;

                    // remove any previously loaded flushers, this will also remove any waiting neurons + stop the timer.
                IsInitialized = false;
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(false);
                fIDs.ReleaseWriteLock();
            }
        }

        // makes certain that all the timers are stopped.
        /// <summary>The stop timers.</summary>
        private void StopTimers()
        {
            if (fTimers != null)
            {
                foreach (var i in fTimers)
                {
                    Neuron iFound;
                    if (TryFindNeuron(i, out iFound))
                    {
                        var iTimer = iFound as TimerNeuron;
                        if (iFound != null)
                        {
                            iTimer.IsActive = false;
                        }
                    }
                }
            }
        }

        /// <summary>Saves the property values of the Current object to the stream. It also flushes all the currently cashed content to
        ///     disk.</summary>
        /// <param name="path">The path.</param>
        /// <remarks>This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.</remarks>
        public void Save(string path)
        {
            StorageHelper.VerifyDataPath(System.IO.Path.GetDirectoryName(path));
            LockManager.Current.RequestCacheLock(true);

                // we request a fulll write lock with the lockmanager cause during a save, we need to prevent any changes (reads are still allowed).
            fIDs.Lock();
            try
            {
                RemoveDeletedNeurons();
                Storage.SaveEntryNeuronsFor((from i in fSins select i.ID).Concat(fTimers), GetType());

                    // we save the timers and sins in a single list of entry points. During the load, we filter things again.
                Modules.Flush();
                FlushUnsave();
                SaveState(path);

                    // important: do savestate after the flush, otherwise, some longtermmemories haven't built up all the correct data (like NDBSTore, which needs a dict that maps id's to neurontypes, which is created during flush.
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(true);
                fIDs.ReleaseLock();
            }
        }

        /// <summary>Saves the property values of the Current object to the stream. It also flushes all the currently cashed content to
        ///     disk.
        ///     This implementation provides an argument to specify a callback that needs to be executed when the flush is done,
        ///     but the
        ///     network is still locked. This allows a caller to perform some other taks like saving designer files when it is
        ///     still safe
        ///     to do so.</summary>
        /// <param name="path">The path to store the network to.</param>
        /// <param name="callWhenDone">The function to call when done.</param>
        /// <remarks>This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.</remarks>
        public void Save(string path, System.Action callWhenDone)
        {
            StorageHelper.VerifyDataPath(path);
            LockManager.Current.RequestCacheLock(true);

                // we request a fulll write lock with the lockmanager cause during a save, we need to prevent any changes (reads are still allowed).
            fIDs.Lock();
            try
            {
                RemoveDeletedNeurons();
                Storage.SaveEntryNeuronsFor((from i in fSins select i.ID).Concat(fTimers), GetType());

                    // we save the timers and sins in a single list of entry points. During the load, we filter things again.
                Modules.Flush();
                FlushUnsave();
                SaveState(path);

                    // important: do savestate after the flush, otherwise, some longtermmemories haven't built up all the correct data (like NDBSTore, which needs a dict that maps id's to neurontypes, which is created during flush.
                if (callWhenDone != null)
                {
                    callWhenDone();
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(true);
                fIDs.ReleaseLock();
            }
        }

        /// <summary>
        ///     Removes the neurons that were deleted, but for which this was not yet flushed to the db backend.
        /// </summary>
        private void RemoveDeletedNeurons()
        {
            lock (fNeuronsToDelete)
            {
                foreach (var i in fNeuronsToDelete)
                {
                    Storage.RemoveNeuron(i);
                }

                fNeuronsToDelete.Clear();
            }
        }

        /// <summary>
        ///     Removes the neurons that were deleted, but for which this was not yet flushed to the db backend.
        ///     This version locks the storage for each delete and is better suited for real-time usage.
        /// </summary>
        private void RemoveDeletedNeuronsSave()
        {
            lock (fNeuronsToDelete)
            {
                foreach (var i in fNeuronsToDelete)
                {
                    LockManager.Current.RequestStorageReadLock(i);
                    try
                    {
                        Storage.RemoveNeuron(i);
                        Storage.Flush();

                            // flush before releasing the lock, so that a potential next read gets the correct data.
                    }
                    finally
                    {
                        LockManager.Current.ReleaseStorageReadLock(i);
                    }
                }

                fNeuronsToDelete.Clear();
            }
        }

        /// <summary>Saves the Brain object to disk, but doesn't flush the cashed content to disk.</summary>
        /// <remarks>While saving, the path to the storage path is made relative.
        ///     Any processor should be stopped before calling this method.</remarks>
        /// <param name="path">The path.</param>
        private void SaveState(string path)
        {
            using (var iStr = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
            {
                var iStoragePath = Storage.DataPath;
                var iModulesPath = Modules.Path;
                try
                {
                    Storage.MakePathRelativeTo(path);

                        // need to make it relative for saving so that the project can be moved.
                    Modules.MakePathRelativeTo(path);
                    try
                    {
                        var iSer = new System.Xml.Serialization.XmlSerializer(typeof(Brain));
                        using (System.IO.TextWriter iWriter = new System.IO.StreamWriter(iStr)) iSer.Serialize(iWriter, Current);
                        IsChanged = false;
                    }
                    catch (System.Exception e)
                    {
                        LogService.Log.LogError("Brain.SaveState", e.Message);
                    }
                }
                finally
                {
                    Storage.DataPath = iStoragePath;

                        // need to restore the path so that it can be used again. Don't use MakePathAbsoluteTo, cause this might force a reload from the data files, which we don't want, and it is always slower.
                    Modules.Path = iModulesPath;
                }
            }
        }

        /// <summary>
        ///     Flushes the data in the cash to disk. This is not thread safe.
        /// </summary>
        /// <remarks>
        ///     The cash is stored in <see cref="Brain.fNeuronCash" />. Files are stored at <see cref="Brain.NeuronsPath" />
        ///     <para>
        ///         This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.
        ///     </para>
        /// </remarks>
        private void FlushUnsave()
        {
            try
            {
                foreach (var iSin in fSins)
                {
                    // use the general method of asking each Sin to store extra data.
                    iSin.Flush();
                }

                TextSin.SaveDict(); // and also some custom ways for storing Sin data.
                if (Time.CurrentLoaded)
                {
                    // only try to save current if is is loaded, otherwise not needed + can cause a lock problem: IDManager is locked during flush, but if we need to load data from storage, we need to lock it again, which is not supported.
                    Time.Current.Flush();
                }

                var iToSave = (from i in fNeuronCash
                               let iItem = NeedsSaving(i.Value)
                               where iItem != null
                               orderby iItem.ID

                               // sor the list, this way, the  items are saved in sequence, which should allow for faster reading.
                               select iItem).ToList();

                    // we will be changing the dictionary during the flush (possibly: when hard refs are made into weakreferences)
                foreach (var i in iToSave)
                {
                    Storage.SaveNeuron(i);
                    i.SetIsChangedUnsafeNoUnfreeze(false); // it's been saved, so let the neuron know.
                }

                Storage.Flush();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Brain.Flush", e.ToString());
            }
        }

        /// <summary>Flushes the data in the cash to disk. This is not thread safe, but it locks every neuron before saving,
        ///     making it suitable for usage during operations.</summary>
        /// <param name="toSave">The to Save.</param>
        /// <remarks>The cash is stored in <see cref="Brain.fNeuronCash"/>. Files are stored at <see cref="Brain.NeuronsPath"/>
        /// <para>
        ///         This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.</para>
        /// </remarks>
        private void FlushUnsaveLocked(System.Collections.Generic.List<Neuron> toSave)
        {
            try
            {
                foreach (var i in toSave)
                {
                    var iId = i.ID;
                    LockManager.Current.RequestStorageReadLock(iId); // prevent the deletion of the neuron.
                    try
                    {
                        if (i.IsDeleted == false)
                        {
                            // make certain that the neuron didn't get deleted during the save.
                            i.SetIsChangedNoUnfreeze(false);

                                // we set this value before saving. if we do it after saving, we might get a situation where thread 1 saves, gets blocked before setting iSchanged = false, while another thread changes the neuron again, sets iSchanged = true, stops, and then the original threads sets isChanged = false again -> oeps. So by doing this first, we only risk of having to write the neuron 2 times, less of a problem.
                            LockManager.Current.RequestLock(i, LockLevel.All, true);

                                // we request a writable lock, to prevent others from being able to write to the neuron.
                            try
                            {
                                if (i.IsDeleted == false && i.IsFrozen == false)
                                {
                                    Storage.SaveNeuron(i);
#if DEBUG

                                    // Neuron iRead = Storage.GetNeuron(iId);
                                    // Debug.Assert(iRead.ID == iId);
#endif
                                }
                            }
                            finally
                            {
                                LockManager.Current.ReleaseLock(i, LockLevel.All, true);

                                    // do in 1 call to prevent deadlocks.
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseStorageReadLock(iId);
                    }
                }

                Storage.FlushFast();
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Brain.Flush", e.ToString());
            }
        }

        /// <summary>
        ///     Saves the data in the cash to disk in a thread save way.
        /// </summary>
        internal void FlushAndClean()
        {
            try
            {
                var iToRemove = Factories.Default.IDLists.GetBuffer(IDListFactory.LARGELISTSIZE);

                    // always want a big list for this.
                var iToSave = Factories.Default.NLists.GetBuffer();
                LockManager.Current.RequestCacheLock(true);

                    // we request a fulll write lock with the lockmanager cause during a save, we need to prevent any changes (reads are still allowed).
                fIDs.Lock();
                try
                {
                    RemoveDeletedNeuronsSave();
                    foreach (var iSin in fSins)
                    {
                        // use the general method of asking each Sin to store extra data.
                        iSin.Flush();
                    }

                    TextSin.SaveDict(); // and also some custom ways for storing Sin data.
                    if (Time.CurrentLoaded)
                    {
                        // only try to save current if is is loaded, otherwise not needed + can cause a lock problem: IDManager is locked during flush, but if we need to load data from storage, we need to lock it again, which is not supported.
                        Time.Current.Flush();
                    }

                    foreach (var i in fNeuronCash)
                    {
                        // needs to be done inside the lock, otherwise the cachelock can change during the creation of the list.
                        if (!(i.Value is System.WeakReference))
                        {
                            // this is the most common path, fastet to do it like this.
                            var iItem = NeedsSaving(i.Value);
                            if (iItem != null && iItem.IsFrozen == false)
                            {
                                // don't save frozen neurons, they are probably going to be deleted anyway and don't want to unfreeze them.
                                iToSave.Add(iItem);

                                    // we will be changing the dictionary during the flush (possibly: when hard refs are made into weakreferences)
                            }
                        }
                        else if (((System.WeakReference)i.Value).IsAlive == false)
                        {
                            iToRemove.Add(i.Key);

                                // we will be changing the dictionary during the flush (possibly: when hard refs are made into weakreferences)
                        }
                    }

                    if (iToRemove.Count > 0)
                    {
                        foreach (var i in iToRemove)
                        {
                            fNeuronCash.Remove(i); // do inside cachelock, cause this changes the cachelist.
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseCacheLock(true);
                    fIDs.ReleaseLock();
                }

                FlushUnsaveLocked(iToSave);

                    // we lock every neuron during save, to make certain it can't be changed in between a save and changing IsChanged. This means we have to do the cachelocking together with neurons
                OnFlushed(); // let the designer db also flush, if it is available.
                System.GC.Collect();

                    // after doing a cul, there are possibly neurons waiting in the gc to be finalized, activate this so that these neurons can be recycled asap.
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Brain.Flush", e.ToString());
            }
        }

        /// <summary>
        ///     Called when the system performed an autoflush.
        /// </summary>
        protected void OnFlushed()
        {
            if (AutoFlushed != null)
            {
                AutoFlushed(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Check if a dictionary item needs saving.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The neuron that needs to be saved, or null if there is none.</returns>
        private static Neuron NeedsSaving(object i)
        {
            var iRes = false;
            var iToProcess = i as Neuron;
            if (iToProcess != null)
            {
                // if it's a weakReference, we know that it doesn't need to be saved, cause it can get unloaded at any moment, no need to waste CPU cycles here.
                iRes = iToProcess != null && iToProcess.ID != Neuron.TempId && iToProcess.IsChanged;

                    // temp neurons can't be saved yet.
                if (iRes)
                {
                    return iToProcess;
                }
            }

            return null;
        }

        /// <summary>Makes the specified neuron a temp one that will be registered the first time it is used.</summary>
        /// <param name="toChange"></param>
        public void MakeTemp(Neuron toChange)
        {
            if (toChange.ID == 0)
            {
                toChange.SetId(Neuron.TempId); // use SetID, this is faster to make a neuron temp (no locking involved.
                toChange.SetIsChangedUnsafeNoUnfreeze(true);

                    // doesnt need to be a save access cause the dict is not accessed.
            }
            else
            {
                toChange.ID = Neuron.TempId;
                LogService.Log.LogWarning(
                    "make temp", 
                    "Assigning a temp status to an already registered neuron can create problems");
            }

            if (Settings.LogTempIntOrDouble)
            {
                if (toChange is IntNeuron)
                {
                    LogService.Log.LogWarning("temp int", ((IntNeuron)toChange).Value.ToString());
                }
                else if (toChange is DoubleNeuron)
                {
                    LogService.Log.LogWarning("temp double", ((DoubleNeuron)toChange).Value.ToString());
                }
            }
        }

        /// <summary>
        ///     Creates a new set of initial neurons.
        ///     Before calling new, it is best to <see cref="Brain.Clear" /> any previous db.
        /// </summary>
        /// <remarks>
        ///     Also raises the <see cref="Brain.Loaded" /> event since, when this operation is doen, a new brain has been loaded.
        /// </remarks>
        public static void New()
        {
            TextSin.ResetDict(); // whenever a new brain is loaded, we need to reset the dictionary.
            Current.LoadDefaultNeurons();
            if (Current.Loaded != null)
            {
                Current.Loaded(Current, System.EventArgs.Empty);
            }

            Current.IsChanged = false;
            Current.IsInitialized = true;
        }

        /// <summary>
        ///     Tries to create and store an object of each type in all the loaded assemblies that has
        ///     a <see cref="NeuronIDAttribute" />.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method is thread safe, it is locked at the beginning of the call and only unlocked at the end.
        ///     </para>
        /// </remarks>
        private void LoadDefaultNeurons()
        {
            LockManager.Current.RequestCacheLock(false);
            try
            {
                try
                {
                    var iFailCount = 0; // keeps track of how many constructors failed.
                    var iAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var iAsm in iAssemblies)
                    {
                        var iTypes = iAsm.GetTypes();
                        foreach (var iType in iTypes)
                        {
                            var iAttribs =
                                (NeuronIDAttribute[])iType.GetCustomAttributes(typeof(NeuronIDAttribute), false);
                            if (iAttribs.Length > 0)
                            {
                                foreach (var iAttrib in iAttribs)
                                {
                                    var iNeuronType = iAttrib.Type;
                                    if (iNeuronType == null)
                                    {
                                        iNeuronType = iType;
                                    }

                                    if (typeof(Neuron).IsAssignableFrom(iNeuronType) == false)
                                    {
                                        iFailCount++;
                                        LogService.Log.LogInfo(
                                            "Brain.LoadDefaultNeurons", 
                                            string.Format(
                                                "Unable to create a default neuron for {0} because it is not derived from Neuron.", 
                                                iType));
                                    }
                                    else
                                    {
                                        var iObj = NeuronFactory.Get(iNeuronType);
                                        if (iObj == null)
                                        {
                                            iFailCount++;
                                            LogService.Log.LogInfo(
                                                "Brain.LoadDefaultNeurons", 
                                                string.Format("Failed to load default neuron: {0}.", iType));
                                        }
                                        else
                                        {
                                            iObj.IsChanged = true;

                                                // newly created items, must be stored in cash -> doing it before setting the id speeds up things
                                            iObj.SetId(iAttrib.ID);
                                            if (iObj is Sin)
                                            {
                                                // when we create all new items, we also need to add the newly created sins to the Sins list, so that the network knows about it.
                                                fSins.Add((Sin)iObj);
                                            }
                                            else if (iObj is TimerNeuron)
                                            {
                                                fTimers.Add(iObj.ID);
                                            }

                                            AddToCash(iObj);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (iFailCount == 0)
                    {
                        LogService.Log.LogInfo("Brain.LoadDefaultNeurons", "Succesfully loaded all default neurons.");
                    }
                    else
                    {
                        LogService.Log.LogInfo(
                            "Brain.LoadDefaultNeurons", 
                            string.Format("Found {0} errors while loading all default neurons.", iFailCount));
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "Brain.LoadDefaultNeurons", 
                        string.Format("Failed to load the default neurons with message: {0}", e));
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(false);
            }
        }

        /// <summary>
        ///     Checks if every default neuron is loaded or not, when not, it is automatically created. This is used to keep a
        ///     network
        ///     up to date with the latest default neurons.
        /// </summary>
        public void ReloadLoadDefaultNeurons()
        {
            LockManager.Current.RequestCacheLock(false);
            try
            {
                try
                {
                    var iFailCount = 0; // keeps track of how many constructors failed.
                    var iAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var iAsm in iAssemblies)
                    {
                        var iTypes = iAsm.GetTypes();
                        foreach (var iType in iTypes)
                        {
                            var iAttribs =
                                (NeuronIDAttribute[])iType.GetCustomAttributes(typeof(NeuronIDAttribute), false);
                            if (iAttribs.Length > 0)
                            {
                                foreach (var iAttrib in iAttribs)
                                {
                                    var iNeuronType = iAttrib.Type;
                                    if (iNeuronType == null)
                                    {
                                        iNeuronType = iType;
                                    }

                                    if (typeof(Neuron).IsAssignableFrom(iNeuronType) == false)
                                    {
                                        iFailCount++;
                                        LogService.Log.LogInfo(
                                            "Brain.LoadDefaultNeurons", 
                                            string.Format(
                                                "Unable to create a default neuron for {0} because it is not derived from Neuron.", 
                                                iType));
                                    }
                                    else
                                    {
                                        Neuron iObj;
                                        if (Current.TryFindNeuron(iAttrib.ID, out iObj) == false
                                            || iAttrib.ID != iObj.ID)
                                        {
                                            // we also check for iAttrib.ID != iObj.ID this could happen if the database somehow got corrupt and overrode the one of the static neurons. In this case, we simply discard the previous one.
                                            iObj = NeuronFactory.Get(iNeuronType);
                                            if (iObj == null)
                                            {
                                                iFailCount++;
                                                LogService.Log.LogInfo(
                                                    "Brain.LoadDefaultNeurons", 
                                                    string.Format("Failed to load default neuron: {0}.", iType));
                                            }
                                            else
                                            {
                                                iObj.IsChanged = true; // newly created items, must be stored in cash.
                                                iObj.SetId(iAttrib.ID);
                                                AddToCash(iObj);
                                                if (iObj is Sin)
                                                {
                                                    // when we create all new items, we also need to add the newly created sins to the Sins list, so that the network knows about it.
                                                    fSins.Add((Sin)iObj);
                                                }
                                                else if (iObj is TimerNeuron)
                                                {
                                                    fTimers.Add(iObj.ID);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (iFailCount == 0)
                    {
                        LogService.Log.LogInfo("Brain.LoadDefaultNeurons", "Succesfully loaded all default neurons.");
                    }
                    else
                    {
                        LogService.Log.LogInfo(
                            "Brain.LoadDefaultNeurons", 
                            string.Format("Found {0} errors while loading all default neurons.", iFailCount));
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "Brain.LoadDefaultNeurons", 
                        string.Format("Failed to load the default neurons with message: {0}", e));
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(false);
            }
        }

        /// <summary>Gets the type object, based on the typename of the neuron. This is used to overcome the
        ///     problem that Type.GetType(string name) only searches in the exeuting assembly and no further, while
        ///     neurons can be declared in different assemblies.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        public System.Type GetNeuronType(string name)
        {
            var iRes = System.Type.GetType(name);
            if (iRes != null)
            {
                return iRes;
            }

            var iAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var iAsm in iAssemblies)
            {
                iRes = iAsm.GetType(name);
                if (iRes != null)
                {
                    return iRes;
                }
            }

            if (name == "JaStDev.HAB.SelectInstruction")
            {
                // this is for solving an old namechange.
                return typeof(FilterInstruction);
            }

            return null;
        }

        /// <summary>Adds a new neuron object to the brain and assigns a valid <see cref="Neuron.ID"/> to the object.</summary>
        /// <exception cref="BrainException">The object already has an ID, so it can't be saved.</exception>
        /// <exception cref="BrainException">The brain is full!</exception>
        /// <remarks><para>When the neuron being added is a <see cref="Sin"/> is is also added to the internal list of Sins.</para>
        /// <para>By default, the item is simply added to the working memory.  It is not persisted to the<see cref="Brain.Storage"/>.</para>
        /// <para>This method is thread safe, it is locked at the moment when the internal cash is accessed and unlocked when
        ///         done.</para>
        /// </remarks>
        /// <param name="toAdd">The object to add.</param>
        public void Add(Neuron toAdd)
        {
            if (toAdd.ID == Neuron.EmptyId || toAdd.ID == Neuron.TempId)
            {
                var iProcessAfterAdd = true;
                toAdd.SetIsChangedUnsafeNoUnfreeze(true);

                    // need to make certain that newly added items are always saved. Needs to be done before setting the id, otherwise the cache gets corrupted, since this updates the cache.
                var iId = fIDs.GetId();
                LockManager.Current.RequestCacheLock(iId, true);
                try
                {
                    if (toAdd.ID == Neuron.EmptyId || toAdd.ID == Neuron.TempId)
                    {
                        // do the extra check after the lock to make certain that there aren't 2 threads trying to add the same neuron at exactly the same time.
                        toAdd.SetId(iId);
                        AddToCashUnsave(toAdd);
                    }
                    else
                    {
                        iProcessAfterAdd = false;
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseCacheLock(iId, true);
                }

                if (iProcessAfterAdd)
                {
                    ProcessAfterAdd(toAdd); // bring this outside of the lock, cause it raises an event.
                }
            }
            else
            {
                throw new BrainException("The object already has an ID assigned, can't add it to this brain!");
            }
        }

        /// <summary>Removes the id from the list of items that need to delete. This is to make certain that, when an id
        ///     is reused, it doesn't remain qeueud for deletion.</summary>
        /// <param name="id">The id.</param>
        internal void RemoveFromNeuronsToDelete(ulong id)
        {
            lock (fNeuronsToDelete)
                fNeuronsToDelete.Remove(id);

                    // each time we create a new neuron, we make certain that the ID is no longer in the 'ToDelete' list. This makes certain that the list doesn't get to big.  We do this always (even if we are not in AutoSaveNeurons mode -> the state was determined at the time it was added, if it is in there and the id is reused, the file will be overwrittern anyway.
        }

        /// <summary>Adds the neuron to the cash.</summary>
        /// <remarks>Tries to add the neuron as a weakReference if possible (we have a datastore) so that it gets unloaded when
        ///     not used.</remarks>
        /// <param name="toAdd">To add.</param>
        private void AddToCash(Neuron toAdd)
        {
            LockManager.Current.RequestCacheLock(toAdd.ID, true);
            try
            {
                AddToCashUnsave(toAdd);
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(toAdd.ID, true);
            }
        }

        /// <summary>
        ///     removes all the dead weakref objects.
        /// </summary>
        internal void CulCash()
        {
            var iToRemove = Factories.Default.IDLists.GetBuffer(fNeuronCash.Count / 2);

                // provide some sense of how big the cleanup will be. can be wrong.
            iToRemove.Capacity = Settings.CleanCacheAfter; // this way, we never need to resize this list.
            LockManager.Current.RequestCacheLock(true);
            try
            {
                foreach (var i in fNeuronCash)
                {
                    if (i.Value is System.WeakReference && ((System.WeakReference)i.Value).IsAlive == false)
                    {
                        iToRemove.Add(i.Key);
                    }
                }

                if (iToRemove.Count > 0)
                {
                    foreach (var i in iToRemove)
                    {
                        fNeuronCash.Remove(i);
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(true);
                Factories.Default.IDLists.Recycle(iToRemove);
            }

            System.GC.Collect();

                // after doing a cul, there are possibly neurons waiting in the gc to be finalized, activate this so that these neurons can be recycled asap.
        }

        /// <summary>adds to the cache without locking</summary>
        /// <param name="toAdd"></param>
        private void AddToCashUnsave(Neuron toAdd)
        {
            var iStorage = Storage;
            switch (Settings.StorageMode)
            {
                case NeuronStorageMode.AlwaysInMem:
                    fNeuronCash[toAdd.ID] = toAdd;
                    break;
                case NeuronStorageMode.StreamWhenPossible:
                    if (toAdd.IsChanged == false && iStorage != null && string.IsNullOrEmpty(iStorage.DataPath) == false)
                    {
                        fNeuronCash[toAdd.ID] = new System.WeakReference(toAdd);
                        CacheBuffer.Default.Add(toAdd);

                            // keep in mem for short time, so that we don't need to reload to many times.
                    }
                    else
                    {
                        fNeuronCash[toAdd.ID] = toAdd;
                    }

                    break;
                case NeuronStorageMode.AlwaysStream:
                    if (toAdd.IsChanged == false)
                    {
                        // when a new neuron got added to the network, it is still modified, so we need to flush it, and prevent it from being disposed before it gets flushed.
                        fNeuronCash[toAdd.ID] = new System.WeakReference(toAdd);
                        CacheBuffer.Default.Add(toAdd);

                            // keep in mem for short time, so that we don't need to reload to many times.
                    }
                    else
                    {
                        fNeuronCash[toAdd.ID] = toAdd;
                        Flusher.TryFlush();
                    }

                    break;
                default:
                    throw new System.InvalidOperationException();
            }
        }

        // private void TestPrevVal(Neuron toAdd)
        // {
        // object iPrev;
        // if (fNeuronCash.TryGetValue(toAdd.ID, out iPrev) == true)
        // {
        // Neuron iN = iPrev as Neuron;
        // if ((iN != null && iN.ID == toAdd.ID) || (iPrev is WeakReference && ((WeakReference)iPrev).Target != null && ((Neuron)((WeakReference)iPrev).Target).ID == toAdd.ID))
        // Debug.Print("oeps: overwrite");
        // }
        // }

        /// <summary>The add to cash and resolve links.</summary>
        /// <param name="toAdd">The to add.</param>
        private void AddToCashAndResolveLinks(LinkResolverData toAdd)
        {
            var iToResolve = toAdd.ToResolve; // after the Resolve, 'ToData' is recycled and 'ToResolve' will be empty.
            Neuron.LinkResolver.Default.Resolve(toAdd, fNeuronCash);

                // has to be outside the cachelock, cause this will get a lock on linklists, which, if done inside the cachelock, could lead to deadlocks.
            LockManager.Current.RequestCacheLock(iToResolve.ID, true);
            try
            {
                AddToCashUnsave(iToResolve);
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(iToResolve.ID, true);
            }

            Neuron.LinkResolver.Default.Remove(iToResolve);
            toAdd.Recycle();
        }

        /// <summary>Notifies the brain that the specified neuron has changed.</summary>
        /// <remarks>Important for storage mode (correct updates).</remarks>
        /// <param name="changed">The changed.</param>
        internal void NotifyChanged(Neuron changed)
        {
            IsChanged = true;
            if (Settings.StorageMode == NeuronStorageMode.StreamWhenPossible
                || Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                LockManager.Current.RequestCacheLock(changed.ID, false);
                try
                {
                    object iFound;
                    if (fNeuronCash.TryGetValue(changed.ID, out iFound))
                    {
                        // when we add an item, we first set the ID, which triggeres the changed, but offcourse, the item isn't yet in the dictionary, so check for this situation.
                        if (iFound is System.WeakReference)
                        {
                            // if it is a weakrefrefence, we replace it with a hard reference.
                            LockManager.Current.UpgradeCacheLock(changed.ID);
                            fNeuronCash[changed.ID] = changed;
                            LockManager.Current.DowngradeCacheLock(changed.ID);
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseCacheLock(changed.ID, false);
                }
            }

            if (Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                // when we stream, we need to put the neuron into a buffer that will eventually save the neuron, when the buffer is full or, the time has ellapsed.
                Flusher.TryFlush();
            }
        }

        /// <summary>a fast, unsafer version for updating an  IsChanged state of a neuron.
        ///     The Cache needs to be locked before caling this function. This is to make certain that the entire operation is
        ///     atomic.</summary>
        /// <param name="changed">The changed.</param>
        internal void NotifyChangedUnsave(Neuron changed)
        {
            IsChanged = true;
            if (Settings.StorageMode == NeuronStorageMode.StreamWhenPossible
                || Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                fNeuronCash[changed.ID] = changed; // no lock required, this should already ahve been done.
            }

            if (Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                // when we stream, we need to put the neuron into a buffer that will eventually save the neuron, when the buffer is full or, the time has ellapsed.
                Flusher.TryFlush();
            }
        }

        /// <summary>Notifies the brain that the specified neuron has been saved.</summary>
        /// <remarks>Important for storage mode (correct updates): we need to make hardreference weak again, if required.</remarks>
        /// <param name="changed">The changed.</param>
        internal void NotifySaved(Neuron changed)
        {
            if (Settings.StorageMode == NeuronStorageMode.StreamWhenPossible
                || Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                LockManager.Current.RequestCacheLock(changed.ID, true);
                try
                {
                    var iFound = fNeuronCash[changed.ID];
                    if (iFound is Neuron)
                    {
                        // if it is a weakrefrefence, we replace it with a hard reference.
                        fNeuronCash[changed.ID] = new System.WeakReference(changed);
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseCacheLock(changed.ID, true);
                }
            }
        }

        /// <summary>Notifies the brain that the specified neuron has been saved.</summary>
        /// <remarks>Important for storage mode (correct updates): we need to make hardreference weak again, if required.</remarks>
        /// <param name="changed">The changed.</param>
        internal void NotifySavedUnsafe(Neuron changed)
        {
            if (Settings.StorageMode == NeuronStorageMode.StreamWhenPossible
                || Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                var iFound = fNeuronCash[changed.ID];
                if (iFound is Neuron)
                {
                    // if it is a weakrefrefence, we replace it with a hard reference.
                    fNeuronCash[changed.ID] = new System.WeakReference(changed);
                }
            }
        }

        /// <summary>Adds a new neuron object to the brain and tries to use the specified value as <see cref="Neuron.ID"/> for the
        ///     object.
        ///     This is usefull if you want to restore a neuron.</summary>
        /// <exception cref="BrainException">The object already has an ID, so it can't be saved.</exception>
        /// <exception cref="BrainException">The brain can't store neurons created in sub processors!</exception>
        /// <exception cref="BrainException">The brain is full!</exception>
        /// <remarks><para>If the id is not available, or not accesseable, a new one is created for the item, otherwise, the specified
        ///         id is used.</para>
        /// <para>When the neuron being added is a <see cref="Sin"/> is is also added to the internal list of Sins.</para>
        /// <para>By default, the item is simply added to the working memory.  It is not persisted to the<see cref="Brain.Storage"/>.</para>
        /// <para>This method is thread safe, it is locked at the moment when the internal cash is accessed and unlocked when
        ///         done.</para>
        /// <para>When an add is requested from a sub processor, the request is denied.  This is because sub processors
        ///         are temporary and most of them will create invalid information that needs to be disgarded, so an
        ///         add is not allowed.  This is done by checking a static that has a unique value for each thread.</para>
        /// </remarks>
        /// <param name="toAdd">The object to add.</param>
        /// <param name="id">The id.</param>
        public void Add(Neuron toAdd, ulong id)
        {
            if (toAdd.ID == Neuron.EmptyId || toAdd.ID == Neuron.TempId)
            {
                toAdd.SetId(fIDs.GetId(id));
                AddToCash(toAdd);
                ProcessAfterAdd(toAdd); // bring this outside of the lock, cause it raises an event.
            }
            else
            {
                throw new BrainException("The object already has an ID assigned, can't add it to this brain!");
            }
        }

        /// <summary>Processes the item after it was added to the brain. This includes raising the event and checking if it is a sin.</summary>
        /// <param name="toAdd">Item that was added.</param>
        private void ProcessAfterAdd(Neuron toAdd)
        {
            if (toAdd is Sin)
            {
                var iSin = toAdd as Sin;
                fSins.Add(iSin);
                iSin.CallSinCreateEvent();
            }
            else if (toAdd is TimerNeuron)
            {
                fTimers.Add(toAdd.ID);
            }

            if (HasNeuronChangedEvents)
            {
                var iArgs = new NeuronChangedEventArgs
                                {
                                    Action = BrainAction.Created, 
                                    OriginalSource = toAdd, 
                                    OriginalSourceID = toAdd.ID
                                };
                OnNeuronChanged(iArgs);
            }
        }

        /// <summary>Replaces the object representation of a neuron with a new one.</summary>
        /// <remarks>This function is used for instance, when the type of a neuron is changed to replace
        ///     a physical object with the new representation of the item, or when a neuron is changed from
        ///     one type to another.
        /// <para>
        ///         Thread safe.</para>
        /// </remarks>
        /// <param name="old">The old neuron</param>
        /// <param name="fresh">The new neuron.</param>
        internal void Replace(Neuron old, Neuron fresh)
        {
            if (fresh.ID != old.ID)
            {
                // this needs to be done outside of the cashloc, cause the statement can try to change the cash also which requests a lock.
                fresh.SetId(old.ID);
            }

            LockManager.Current.RequestCacheLock(true);
            try
            {
                fresh.SetChangedUnsave(); // can be done inside lock: it's the cachelock anyway.
                if (fNeuronCash.ContainsKey(old.ID))
                {
                    // if it's cashed, update the cashe.
                    if (fNeuronCash[old.ID] is System.WeakReference)
                    {
                        fNeuronCash[old.ID] = new System.WeakReference(fresh);
                    }
                    else
                    {
                        fNeuronCash[old.ID] = fresh;
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(true);
            }

            if (HasNeuronChangedEvents)
            {
                var iArgs = new NeuronChangedEventArgs
                                {
                                    Action = BrainAction.Changed, 
                                    OriginalSource = old, 
                                    NewValue = fresh, 
                                    OriginalSourceID = old.ID
                                };
                OnNeuronChanged(iArgs);
            }

            old.SetId(Neuron.EmptyId);

                // need to do this before reset IsChanged, otherwise the reset causes the cash reference to be replaced again by the old one, which we don't want.
            old.IsChanged = false;

                // we use this to prevent the old item from being streamed to disk when it is no longer valid, we can't 
        }

        /// <summary>Permanently removes an item from the brain when possible.</summary>
        /// <exception cref="BrainException">The neuron is not stored in the brain!!</exception>
        /// <exception cref="BrainException">The id recycle list is full!</exception>
        /// <remarks><para>The id is recycled.  Because of this recyclying, only int.MaxValue number of items can be deleted at the
        ///         same time (which is much less than the brain can store in total).</para>
        /// <para>When a neuron is removed, all the links to and from that neuron are also destroyed. The neuron is also
        ///         removed from any clusters that contain it.</para>
        /// </remarks>
        /// <param name="toDelete">Item to delete</param>
        /// <returns>True when the item was successfully deleted.  False when the item is blocked and can't be deleted.
        ///     An item can be blocked because:
        ///     - it is used as the value for 1 or more <see cref="Link.Meaning"/> props,
        ///     - it is used as the value for 1 or more <see cref="NeuronCluster.Meaning"/> props,
        ///     - it is used as a value in one or more <see cref="Link.Info"/> lists or,
        ///     - it is a statically created item (id less than <see cref="PredefinedNeurons.Dynamic"/></returns>
        public bool Delete(Neuron toDelete)
        {
            return Delete(toDelete, true);
        }

        /// <summary>Deletes the specified item, allowing the option to unfreeze or not. This is used to delete frozen
        ///     items that need to die with the processor.</summary>
        /// <param name="toDelete">To delete.</param>
        /// <param name="unFreeze">if set to <c>true</c> [un freeze].</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool Delete(Neuron toDelete, bool unFreeze)
        {
            if (toDelete.ID == Neuron.EmptyId)
            {
                throw new BrainException("The neuron is not stored in the brain!");
            }

            if (toDelete.ModuleRefCount > 0)
            {
                throw new BrainException(
                    "Neurons that are referenced by a module can't be deleted. Try unloading the module first.");
            }

            if (toDelete.CanBeDeleted == false)
            {
                return false;
            }

            var iId = toDelete.ID;
            if (iId == Neuron.TempId)
            {
                // temp id's simply reset to empty
                toDelete.SetId(Neuron.EmptyId);
            }
            else if (iId != Neuron.EmptyId)
            {
                // we check that the neuron isn't deleted in another processor, before we were able to get the lock. If this is the case, we don't treat it as an error, we simply skip.
                try
                {
                    LockManager.Current.RequestStorageReadLock(iId);

                        // we need to lock the item for the delete, so that it's not possible for multiple procs to delete the same item.
                    try
                    {
                        if (toDelete.ID != Neuron.EmptyId)
                        {
                            // could be that we only acquired the lock after the item was already deleted, check for this, othewise we release the 0 id.
                            toDelete.IsDeleted = true;

                                // tihs is important: it prevents deadlocks and other strange threading behaviour: when we need to clear the lists from a deleted neuron, we don't want to try and lock lists of other neurons that are being deleted.
                            if (unFreeze)
                            {
                                toDelete.UnFreeze();

                                    // every neuron that gets deleted needs to be unfrozen so that the neuron is removed from each processor that has it frozen.
                            }

                            if (toDelete is Sin)
                            {
                                ProcessSinDelete((Sin)toDelete);
                            }
                            else if (toDelete is TimerNeuron)
                            {
                                fTimers.Remove(iId);
                            }

                            TextSin.RemoveEntryPoint(toDelete);

                                // if it's a textneuron, also remove it from the entry points set.
                            var iDeleter = new Deleter();
                            iDeleter.Delete(toDelete);
                            RemoveFromCash(iId);
                            RemoveFromStorage(toDelete);
                            if (HasNeuronChangedEvents)
                            {
                                var iArgs = new NeuronChangedEventArgs
                                                {
                                                    Action = BrainAction.Removed, 
                                                    OriginalSource = toDelete, 
                                                    OriginalSourceID = iId
                                                };
                                OnNeuronChanged(iArgs);
                            }

                            fIDs.ReleaseId(iId);

                                // only release the id after it has been removed from cache, otherwise it could have already been reused before we delete from cache.
                            toDelete.SetId(Neuron.EmptyId);

                                // we need to reset the id so that other parts of the system now it no longer belongs to a brain.  We do this after all the events so that the event handlers can still see the correct id.
                            toDelete.IsDeleted = false;

                                // we reset this value, so that the actual physical object can be reused, for an undo system for instance. The prop itself still knows it is deleted because id == 0.
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseStorageReadLock(iId);
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Brain.Delete", e.ToString());
                }
            }

            return true;
        }

        /// <summary>called when a sin gets deleted.</summary>
        /// <param name="sin"></param>
        private void ProcessSinDelete(Sin sin)
        {
            System.Diagnostics.Debug.Assert(sin != null);
            sin.CallSinDestroyEvent();
            fSins.Remove(sin);
        }

        ///// <summary>
        ///// This is a delete that will first change the id of the neuron to the specified one before deleting
        ///// it. This is used to delete neuron's that had an invalid id. (can happen when the system craches
        ///// when the index is updated, but the data not yet saved.
        ///// </summary>
        ///// <param name="toDelete">To delete.</param>
        ///// <param name="id">The id.</param>
        ///// <returns></returns>
        // public bool DeleteAfterIDReplace(Neuron toDelete, ulong id)
        // {
        // toDelete.SetId(id);
        // return Delete(toDelete, true);
        // }

        /// <summary>Resolves 2 id's pointing to the same physical storage space.
        ///     The neuron with specified as 'keep' is kept, the bad id is marked as free.</summary>
        /// <param name="toFix">The neuorn to fix.</param>
        /// <param name="keep">The id to keep.</param>
        /// <param name="bad">The id to release.</param>
        public void ResolveCrossRef(Neuron toFix, ulong keep, ulong bad)
        {
            toFix.SetId(keep);
            fNeuronCash.Remove(bad);

                // need to update the cache as well, otherwise the IsChanged isn't registered correctly.
            fNeuronCash[keep] = toFix;
            toFix.IsChanged = true; // need to make it store the change.
            Storage.MarkAsFree(bad);
            fIDs.ReleaseId(bad);
        }

        /// <summary>Removes the neuron from the backend storage.</summary>
        /// <param name="toDelete">Item To remove.</param>
        private void RemoveFromStorage(Neuron toDelete)
        {
            AddToNeuronsToDelete(toDelete.ID);
            if (Settings.StorageMode == NeuronStorageMode.AlwaysStream)
            {
                // even for always-stream, don't remove immediatly, but let the flusher handle it (saver, with locks and all).
                Flusher.TryFlush();
            }
        }

        /// <summary>Adds id to the list of neurons to delete.</summary>
        /// <param name="id">The id.</param>
        private void AddToNeuronsToDelete(ulong id)
        {
            lock (fNeuronsToDelete) fNeuronsToDelete.Add(id);
        }

        /// <summary>Removes the id from the cash.</summary>
        /// <param name="id">The id to remove.</param>
        private void RemoveFromCash(ulong id)
        {
            LockManager.Current.RequestCacheLock(id, true);
            try
            {
                fNeuronCash.Remove(id);
            }
            finally
            {
                LockManager.Current.ReleaseCacheLock(id, true);
            }
        }

        /// <summary>Deletes the neuron with the specified id. If there is no neuron found with this id,
        ///     it is recycled (if it hasn't been yet).</summary>
        /// <param name="id">The id of the neuron to remove.</param>
        public void Delete(ulong id)
        {
            Neuron iNeuron;
            if (TryFindNeuron(id, out iNeuron))
            {
                Delete(iNeuron);
            }
            else if (id < NextID && IsValidID(id))
            {
                // if it's bigger than nextid, don't recycle, will be used as normal. If not valid, it's out of range: a static, possibly not used id, so don't recycle.
                fIDs.ReleaseId(id);
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation. Note: this is not thread safe. Use the <see cref="Brain.Load"/>
        ///     method for a tread safe way to read the entire object.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            reader.ReadStartElement("Count");
            var iVal = reader.ReadString();
            var iConverted = ulong.Parse(iVal);
            fIDs.NextID = iConverted;
            reader.ReadEndElement();

            if (reader.Name == "Memory")
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(LongtermMem));
                Storage = (LongtermMem)valueSerializer.Deserialize(reader);
            }
            else if (reader.Name == "NDBStore")
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Storage.NDB.NDBStore));
                Storage = (Storage.NDB.NDBStore)valueSerializer.Deserialize(reader);
            }

            reader.MoveToContent();

            fIDs.ReadFreeIDs(reader);

            if (reader.Name == "Modules")
            {
                // modules were added in ver 0.4
                var iModSer = new System.Xml.Serialization.XmlSerializer(typeof(Modules));
                fModules = (Modules)iModSer.Deserialize(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.this is not thread safe. Use the <see cref="Brain.Save"/>
        ///     method for a tread safe way to read the entire object.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            fIDs.ShrinkFreeIds();

                // before we save the list of free id's or NextID, remove the top range from the next id, untill the first real neuron.
            writer.WriteStartElement("Count");
            writer.WriteString(fIDs.NextID.ToString());
            writer.WriteEndElement();

            var MemSerializer = new System.Xml.Serialization.XmlSerializer(Storage.GetType());
            MemSerializer.Serialize(writer, Storage);
            fIDs.WriteFreeIDs(writer);

            var ModulesSer = new System.Xml.Serialization.XmlSerializer(typeof(Modules));
            ModulesSer.Serialize(writer, Modules);
        }

        #endregion
    }
}