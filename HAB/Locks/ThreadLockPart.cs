// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadLockPart.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the info required for a single thread in a single
//   <see cref="LockSection" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Contains all the info required for a single thread in a single
    ///     <see cref="LockSection" /> .
    /// </summary>
    internal class ThreadLockPart
    {
        /// <summary>Initializes a new instance of the <see cref="ThreadLockPart"/> class. Initializes a new instance of the <see cref="ThreadLockInfo"/> class.</summary>
        /// <param name="section">The section.</param>
        public ThreadLockPart(ThreadLock section)
        {
            Section = section;
        }

        /// <summary>
        ///     Gets or sets the <see cref="LockSection" /> to which this record
        ///     belongs.
        /// </summary>
        /// <value>
        ///     The section.
        /// </value>
        public ThreadLock Section { get; internal set; }

        /// <summary>
        ///     Gets or sets the nr of reads for this thread.
        /// </summary>
        /// <value>
        ///     The read count.
        /// </value>
        public int ReadCount { get; set; }

        /// <summary>
        ///     Gets or sets the nr of writes for this thread.
        /// </summary>
        /// <value>
        ///     The read count.
        /// </value>
        public int WriteCount { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is active (reading or
        ///     writing) .
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c> .
        /// </value>
        public bool IsActive
        {
            get
            {
                return WriteCount > 0 || ReadCount > 0;
            }
        }

        /// <summary>
        ///     Makes certain that all values are at 0
        /// </summary>
        internal void Clear()
        {
            WriteCount = ReadCount = 0;
        }
    }
}