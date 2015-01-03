// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for accessors that provide access to lists of neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for accessors that provide access to lists of neurons.
    /// </summary>
    public class NeuronAccessor : Accessor, System.IDisposable
    {
        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Unlock();
        }

        #endregion

        /// <summary>
        ///     Removes the lock that is kept by this instance on the neuron.
        /// </summary>
        public override void Unlock()
        {
            if (fLocks != null)
            {
                LockManager.Current.ReleaseLocks(fLocks); // this will also recycle the lock objects.
                fLocks = null;
                if (IsWriteable)
                {
                    Neuron.SetIsChangedNoClearBuffers(true);

                        // don't clear the buffers, this is already done during the action itself. = true;
                }
            }
        }

        // when  the isFrozen is changed, don't want to udpate the isChanged value.
        /// <summary>The unlock no is changed.</summary>
        protected void UnlockNoIsChanged()
        {
            if (fLocks != null)
            {
                LockManager.Current.ReleaseLocks(fLocks); // this will also recycle the lock objects.
                fLocks = null;
            }
        }

        /// <summary>
        ///     Locks this instance.
        /// </summary>
        public override void Lock()
        {
            System.Diagnostics.Debug.Assert(fLocks == null);

                // important, allows us to test in debug mode that nothing illegal is done.
            if (Neuron.ID == Neuron.TempId)
            {
                // Can't add a child to a temp cluster, both need to be registered.
                Brain.Current.Add(Neuron);
            }

            fLocks = LockRequestList.Create();
            var iLock = LockRequestInfo.Create(Neuron, Level, IsWriteable);
            fLocks.Add(iLock);
            LockManager.Current.RequestLocks(fLocks);
        }

        #region Fields

        /// <summary>The f locks.</summary>
        protected LockRequestList fLocks;

        /// <summary>The f is writeable.</summary>
        private bool fIsWriteable;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronAccessor"/> class.</summary>
        internal NeuronAccessor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronAccessor"/> class.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        protected NeuronAccessor(Neuron neuron, LockLevel level, bool writeable)
        {
            Neuron = neuron;
            Level = level;
            fIsWriteable = writeable;
        }

        #endregion

        #region Prop

        #region Neuron

        /// <summary>
        ///     Gets the neuron being <see langword="protected" /> by this accessor.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Neuron { get; internal set; }

        #endregion

        #region Level

        /// <summary>
        ///     Gets the level of the lock on the neuron.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public LockLevel Level { get; internal set; }

        #endregion

        #region IsWriteable

        /// <summary>
        ///     Gets/sets the wether the accessor allows writing to the data or not.
        /// </summary>
        public bool IsWriteable
        {
            get
            {
                return fIsWriteable;
            }

            set
            {
                if (value != fIsWriteable)
                {
                    fIsWriteable = value;
                }
            }
        }

        #endregion

        #endregion
    }
}