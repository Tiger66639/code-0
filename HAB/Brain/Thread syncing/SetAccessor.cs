// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   The set accessor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The set accessor.</summary>
    /// <typeparam name="T"></typeparam>
    public class SetAccessor<T> : NeuronAccessor
    {
        /// <summary>Initializes a new instance of the <see cref="SetAccessor{T}"/> class.</summary>
        internal SetAccessor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SetAccessor{T}"/> class.</summary>
        /// <param name="set">The set.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        public SetAccessor(System.Collections.Generic.HashSet<T> set, Neuron neuron, LockLevel level, bool writeable)
            : base(neuron, level, writeable)
        {
            Set = set;
        }

        #region Set

        /// <summary>
        ///     Gets the HashSet <see langword="protected" /> by this lock. For
        ///     <see langword="internal" /> use only by descendenets.
        /// </summary>
        public System.Collections.Generic.HashSet<T> Set { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether the hashSet is empty.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is empty; otherwise, <c>false</c> .
        /// </value>
        public bool IsEmptyUnsafe
        {
            get
            {
                return Set.Count == 0;
            }
        }
    }
}