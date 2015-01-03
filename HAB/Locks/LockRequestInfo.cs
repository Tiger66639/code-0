// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockRequestInfo.cs" company="">
//   
// </copyright>
// <summary>
//   A simple class to pass along multiple lock requests at once (for thread
//   safety) to the To create an object use the create function. This is to
//   release pressure on the GC. <see cref="LockManager" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A simple class to pass along multiple lock requests at once (for thread
    ///     safety) to the To create an object use the create function. This is to
    ///     release pressure on the GC. <see cref="LockManager" /> .
    /// </summary>
    public class LockRequestInfo
    {
        /// <summary>Initializes a new instance of the <see cref="LockRequestInfo"/> class. 
        ///     Prevents a default instance of the <see cref="LockRequestInfo"/>
        ///     class from being created.</summary>
        internal LockRequestInfo()
        {
        }

        /// <summary>
        ///     Gets or sets the neuron to lock. Important: this has to be a neuron,
        ///     can't be an ulong, cause otherwise we can have deadlocks while trying
        ///     to load the neuron: the id is locked, but the neuron needs to be
        ///     loaded, so the cache could be accessed,...
        /// </summary>
        /// <value>
        ///     The neuron.
        /// </value>
        public Neuron Neuron { get; set; }

        /// <summary>
        ///     Gets or sets the level of the lock to request for the neuron.
        /// </summary>
        /// <value>
        ///     The level.
        /// </value>
        public LockLevel Level { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the lock should be writeable.
        /// </summary>
        /// <value>
        ///     <c>true</c> if writeable; otherwise, <c>false</c> .
        /// </value>
        public bool Writeable { get; set; }

        /// <summary>Creates a new lock request object.</summary>
        /// <returns>The <see cref="LockRequestInfo"/>.</returns>
        public static LockRequestInfo Create()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetLock();
            }

            return new LockRequestInfo();
        }

        /// <summary>Creates a new lock request object.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        /// <returns>The <see cref="LockRequestInfo"/>.</returns>
        public static LockRequestInfo Create(Neuron neuron, LockLevel level, bool writeable)
        {
            LockRequestInfo iRes;
            if (Processor.CurrentProcessor != null)
            {
                iRes = Processor.CurrentProcessor.Mem.LocksFactory.GetLock();
            }
            else
            {
                iRes = new LockRequestInfo();
            }

            iRes.Neuron = neuron;
            iRes.Level = level;
            iRes.Writeable = writeable;
            return iRes;
        }
    }
}