// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the different ways that a neural network can stream it's cashed
//   neurons to disk.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Defines the different ways that a neural network can stream it's cashed
    ///     neurons to disk.
    /// </summary>
    public enum NeuronStorageMode
    {
        /// <summary>
        ///     In this mode, all neurons will always be kept in memory once they are
        ///     loaded. They are not all automatically loaded at startup, only on an
        ///     'as needed' basis.
        /// </summary>
        AlwaysInMem, 

        /// <summary>
        ///     In this mode, neurons will only remain in memory when they have been
        ///     modified (and need to be saved). This is usefull for editors.
        /// </summary>
        StreamWhenPossible, 

        /// <summary>
        ///     In this mode, they will always be streamed when necessary. When a
        ///     neuron was changed, it is saved first. This mode is the normal run
        ///     mode.
        /// </summary>
        AlwaysStream
    }

    /// <summary>
    ///     to determin how certain things need to be logged (like duplicate
    ///     patterns).
    /// </summary>
    public enum LogMethod
    {
        /// <summary>The none.</summary>
        None, 

        /// <summary>The info.</summary>
        Info, 

        /// <summary>The warning.</summary>
        Warning, 

        /// <summary>The error.</summary>
        Error
    }

    /// <summary>
    ///     Defines the different storage types that can be used for the neurons (the
    ///     backend database).
    /// </summary>
    public enum NeuronStorageSystem
    {
        /// <summary>
        ///     Data is stored in a neural database, using binary flat files.
        /// </summary>
        NDB = 0, 

        /// <summary>
        ///     All data is stored in xml files. (a single xml file per neuron).
        /// </summary>
        Xml
    }

    /// <summary>
    ///     Stores all the settings that are currently applicable for the Brain.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is provided so an application has a single entry point to this
    ///         info (which is ok since there can only be a single brain in an
    ///         application), but doesn't force a specific storage mechanisme (such as
    ///         seperate xml files or application settings.
    ///     </para>
    ///     <para>
    ///         Changing a setting doesn't update all the objects, the new value will
    ///         only be used for new objects. Automatic update is not supported since
    ///         these properties should be set initially at startup and normally don't
    ///         change. This can be refactored later on.
    ///     </para>
    /// </remarks>
    public static class Settings
    {
        /// <summary>The f cache buffer delay.</summary>
        private static int fCacheBufferDelay = 1000 * 10;

        /// <summary>The f cache buffer size.</summary>
        private static int fCacheBufferSize = 9999; // should be 10000, but this value is easier divided by 3.

        /// <summary>The f write buffer size.</summary>
        private static int fWriteBufferSize = 400;

        /// <summary>The f write buffer delay.</summary>
        private static int fWriteBufferDelay = 8 * 1000;

        /// <summary>The f max write buffer size.</summary>
        private static int fMaxWriteBufferSize = 10000;

        /// <summary>The f max concurrent processors.</summary>
        private static int fMaxConcurrentProcessors = 20;

        /// <summary>The f min reserved for blocked.</summary>
        private static int fMinReservedForBlocked = 60;

        /// <summary>The f error on invalid link remove.</summary>
        private static bool fErrorOnInvalidLinkRemove = true;

        /// <summary>The f duplicate pattern log method.</summary>
        private static LogMethod fDuplicatePatternLogMethod = LogMethod.None;

        /// <summary>
        ///     The initial size of the stack used by processors.
        /// </summary>
        public static int InitProcessorStackSize = 100;

        /// <summary>The db block size.</summary>
        public static int DBBlockSize = 130;

        /// <summary>
        ///     determins the amount of info is written to the log. 0, means only the
        ///     critical items.
        /// </summary>
        public static int LogLevel = 0;

        /// <summary>
        ///     This <see langword="switch" /> determins if the network events
        ///     (OnStarted, OnShutDown, OnSinActivity) are automatically triggered or
        ///     not. This allows a designer to turn off this behaviour when not
        ///     required.
        /// </summary>
        public static bool RaiseNetworkEvents = true;

        /// <summary>
        ///     When true, the system will generate a warning when a new
        ///     <see langword="int" /> or <see langword="double" /> temporary neuron is
        ///     created.
        /// </summary>
        public static bool LogTempIntOrDouble = false;

        /// <summary>
        ///     when true, ints and <see langword="double" /> will issue a warning
        ///     whenever they get created.
        /// </summary>
        public static bool LogAddIntOrDouble = false;

        /// <summary>
        ///     when true, the system will generate a warning whenever an
        ///     <see langword="int" /> or <see langword="double" /> is unfrozen.
        /// </summary>
        public static bool LogUnfreezeIntOrDouble = false;

        /// <summary>
        ///     when true, conditional statements are verified during execution that
        ///     there is no invalid empty part. False by default.
        /// </summary>
        public static bool CheckConditional = false;

        /// <summary>
        ///     Gets or sets a value indicating whether to track neuron access (how
        ///     many times a neuron is acessed).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         When this value is true, the <see cref="Brain" /> will keep track how
        ///         many times a neuron is retrieved from the longterm memory. This info
        ///         can be used to optimize neuron load.
        ///     </para>
        ///     <para>
        ///         While editing a neural network with an editor however, it is desirable
        ///         to turn this off since this will change the state of the project,
        ///         while the user actually only viewed the value of a neuron.
        ///     </para>
        /// </remarks>
        /// <value>
        ///     <c>true</c> if tracking should be turned on; otherwise, <c>false</c> .
        /// </value>
        public static bool TrackNeuronAccess = false;

        /// <summary>
        ///     defines the minimum nr of links that a list must contain before an
        ///     index is applied to it (in the form of a dictionary).
        /// </summary>
        public static int MinNrOfLinksForIndex = 15;

        /// <summary>
        ///     The nr of dictionaries that the cache uses internally. The more
        ///     dictionaries, the less likely threads get locked while waiting for the
        ///     cache to become available.
        /// </summary>
        public static int NrCacheTracks = 16;

        /// <summary>
        ///     The number of adds before a neuron-cache clean starts.
        /// </summary>
        public static int CleanCacheAfter = 1000;

        /// <summary>
        ///     Determins the ndb's index files are buffered or not.
        /// </summary>
        public static bool BufferIndexFiles = false;

        /// <summary>
        ///     Determins if the ndb's freeblocks files are buffered or not.
        /// </summary>
        public static bool BufferFreeBlocksFiles = false;

        /// <summary>The db buffer size.</summary>
        public static int DBBufferSize = 1024 * 64;

                          // 64 kb buffersize by default per file. Should equal the same size as a disk buffer      

        /// <summary>Initializes static members of the <see cref="Settings"/> class.</summary>
        static Settings()
        {
            LogSplitToOtherCallBack = false;
            LogContainsChildrenNoCluster = false;
            LogCallSaveVarNotFound = false;
            LogAddChildInvalidArgs = false;
            LogGetClusterMeaningInvalidArgs = false;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to log if a neuron was not
        ///     found in long the term mem.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [log neuron not found in long term mem]; otherwise,
        ///     <c>false</c> .
        /// </value>
        public static bool LogNeuronNotFoundInLongTermMem { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to log the GetClusterMeaning
        ///     instruction's invalid argument errors or not.
        /// </summary>
        /// <remarks>
        ///     These don't have to be logged since the instruction also returns
        ///     'null' when there is no cluster meaning, so it is a valid state to
        ///     return <see langword="null" /> when there is something wrong with the
        ///     arguments. Supressing this error allows us to combine some
        ///     instructions into 1: we don't have to check if the argument is valid.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if [log get cluster meaning invalid args]; otherwise,
        ///     <c>false</c> .
        /// </value>
        public static bool LogGetClusterMeaningInvalidArgs { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating wether to log the GetClusteMeaning
        ///     instruction's invalid argument errors or not.
        /// </summary>
        /// <remarks>
        ///     These don't have to be logged since adding 0 items can also be valid
        ///     (shortcut in case there are no items to add). But can sometimes
        ///     indicate an error, so allow the logging to be switched on or off.
        /// </remarks>
        public static bool LogAddChildInvalidArgs { get; set; }

        /// <summary>
        ///     gets/sets a value indicating wether to log the CallSave instruction's
        ///     invalid argument errors or not.
        /// </summary>
        public static bool LogCallSaveVarNotFound { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether an error should be generated
        ///     on invalid link removal. By default, the system throws an exception
        ///     when you try to delete a link that isn't correctly hooked up
        ///     (possibly because of some error). This prevents you from cleaning up
        ///     the network. So to allow this, set this field to
        ///     <see langword="false" /> while cleaning up.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [error on invalid link remove]; otherwise,
        ///     <c>false</c> .
        /// </value>
        public static bool ErrorOnInvalidLinkRemove
        {
            get
            {
                return fErrorOnInvalidLinkRemove;
            }

            set
            {
                fErrorOnInvalidLinkRemove = value;
            }
        }

        #region DuplicatePatternLogMethod

        /// <summary>
        ///     Gets/sets the way duplicate patterns are treated.
        /// </summary>
        public static LogMethod DuplicatePatternLogMethod
        {
            get
            {
                return fDuplicatePatternLogMethod;
            }

            set
            {
                fDuplicatePatternLogMethod = value;
            }
        }

        #endregion

        /// <summary>
        ///     When true, the <see cref="ContainsChildrenInsetruction" /> will log an
        ///     error when the first argument wasn't a cluster. Otherwise, it will
        ///     simply return <see langword="false" /> without complaining.
        /// </summary>
        public static bool LogContainsChildrenNoCluster { get; set; }

        /// <summary>
        ///     When true, the system will log a warning when a split is performed
        ///     with a callback that differs from the callback that was used in the
        ///     previous split (a situation that can possibly cause problems, but can
        ///     also be used legally). This <see langword="switch" /> is for debugging
        ///     purposes.
        /// </summary>
        public static bool LogSplitToOtherCallBack { get; set; }

        /// <summary>
        ///     Gets or sets the list containing assemblies with externally defined
        ///     sensory interfaces.
        /// </summary>
        /// <remarks>
        ///     This property is used to find types that correspond to type names. By
        ///     using an external list, we don't create a hard dependency on
        ///     assemblies that are not always loaded. For instance, there could be a
        ///     wpf implementation and a forms implementation of the Image sin, or a
        ///     device might already provide fft data from an audio stream, which also
        ///     requires a different audio sin.
        /// </remarks>
        /// <value>
        ///     The list of assemblies containing external sins.
        /// </value>
        public static System.Collections.Generic.List<System.Reflection.Assembly> SinAssemblies { get; set; }

        /// <summary>
        ///     Gets or sets the storage mode used by the <see cref="Brain" /> .
        /// </summary>
        /// <value>
        ///     The storage mode.
        /// </value>
        public static NeuronStorageMode StorageMode { get; set; }

        /// <summary>
        ///     Gets or sets the default type of storage system that is used. This can
        ///     be xml or NDB (the default).
        /// </summary>
        /// <value>
        ///     The storage system.
        /// </value>
        public static NeuronStorageSystem DefaultStorageSystem { get; set; }

        #region WriteBufferSize

        /// <summary>
        ///     Gets or sets the size of the buffer used to store the neurons that
        ///     have been changed and which need to be streamed to disk. When the list
        ///     is full (or the time has elapsed),the neurons are flushed to storage
        /// </summary>
        /// <value>
        ///     The size of the buffer.
        /// </value>
        public static int WriteBufferSize
        {
            get
            {
                return fWriteBufferSize;
            }

            set
            {
                if (fWriteBufferSize != value)
                {
                    fWriteBufferSize = value;
                }
            }
        }

        #endregion

        #region WriteBufferDelay

        /// <summary>
        ///     Gets/sets the maximum delay time after which the buffer must be
        ///     flushed, expressed in milli seconds.
        /// </summary>
        public static int WriteBufferDelay
        {
            get
            {
                return fWriteBufferDelay;
            }

            set
            {
                fWriteBufferDelay = value;
            }
        }

        #endregion

        /// <summary>
        ///     gets/sets the nr of neurons that the write buffer has to contain
        ///     before the save gets activated.
        /// </summary>
        public static int MaxWriteBufferSize
        {
            get
            {
                return fMaxWriteBufferSize;
            }

            set
            {
                fMaxWriteBufferSize = value;
            }
        }

        /// <summary>
        ///     Gets or sets the maximum number of concurrently running processors.
        /// </summary>
        /// <remarks>
        ///     This property determins the maximum nr of threads that are used at any
        ///     time by the network. Hardware puts a limit to the prefereble number of
        ///     running threads for a system. A network can use as many as possible,
        ///     but as not to tax the OS or hosting application (through callbacks) to
        ///     much, it is possible to limit how many are allowed to run at the same
        ///     time. That's controlled through this value.
        /// </remarks>
        /// <value>
        ///     The max concurrent processors.
        /// </value>
        public static int MaxConcurrentProcessors
        {
            get
            {
                return fMaxConcurrentProcessors;
            }

            set
            {
                if (fMaxConcurrentProcessors != value)
                {
                    int iMinWorker, iMinIOC;
                    System.Threading.ThreadPool.GetMinThreads(out iMinWorker, out iMinIOC);

                        // Get the current settings so that we can copy the IOC value.
                    if (System.Threading.ThreadPool.SetMinThreads(value + MinReservedForBlocked, iMinIOC))
                    {
                        fMaxConcurrentProcessors = value;
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            string.Format(
                                "{0} is an invalid value for the maximum number of concurrent processors!", 
                                value));
                    }

                    WaitHandleManager.Current.NrOfPreloadedHandles = value * 3;

                        // we do * 3 cause 1 thread can lock 3 neurons at the same time for a single instruction, so when all threads are waiting for locks, there is a max of threads * 3 locks used.
                }
            }
        }

        #region CacheBufferDelay

        /// <summary>
        ///     Gets/sets the amount of time in milliseconds, neurons should be kept
        ///     in memory after being added to the cache.
        /// </summary>
        public static int CacheBufferDelay
        {
            get
            {
                return fCacheBufferDelay;
            }

            set
            {
                if (fCacheBufferDelay != value)
                {
                    fCacheBufferDelay = value;
                    CacheBuffer.Default.SetDelay(value);
                }
            }
        }

        #endregion

        #region CacheBufferSize

        /// <summary>
        ///     Gets/sets the size of the buffer used to keep neurons temporarely in
        ///     memory. When this is full or the <see cref="CacheBufferDelay" /> time
        ///     has passed, a clean is done.
        /// </summary>
        public static int CacheBufferSize
        {
            get
            {
                return fCacheBufferSize;
            }

            set
            {
                if (fCacheBufferSize != value)
                {
                    fCacheBufferSize = value;
                    CacheBuffer.Default.SetBufferSize(value);
                }
            }
        }

        #endregion

        #region MinReservedForBlocked

        /// <summary>
        ///     Gets/sets the minimum nr of threads that are reserved fo blocked
        ///     threads.
        /// </summary>
        public static int MinReservedForBlocked
        {
            get
            {
                return fMinReservedForBlocked;
            }

            set
            {
                if (fMinReservedForBlocked != value)
                {
                    int iMinWorker, iMinIOC;
                    System.Threading.ThreadPool.GetMinThreads(out iMinWorker, out iMinIOC);

                        // Get the current settings so that we can copy the IOC value.
                    if (System.Threading.ThreadPool.SetMinThreads(value + MaxConcurrentProcessors, iMinIOC))
                    {
                        fMinReservedForBlocked = value;
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            string.Format(
                                "{0} is an invalid value for the minimum number of reserved threads for blocked processors!", 
                                value));
                    }
                }
            }
        }

        /// <summary>The set min max proc.</summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetMinMaxProc(int min, int max)
        {
            int iMinWorker, iMinIOC;
            System.Threading.ThreadPool.GetMinThreads(out iMinWorker, out iMinIOC);

                // Get the current settings so that we can copy the IOC value.
            if (System.Threading.ThreadPool.SetMinThreads(min + max, iMinIOC))
            {
                fMaxConcurrentProcessors = max;
                fMinReservedForBlocked = min;
            }
            else
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "{0} is an invalid value for the minimum and max number of reserved threads for processors!", 
                        min + max));
            }
        }

        #endregion
    }
}