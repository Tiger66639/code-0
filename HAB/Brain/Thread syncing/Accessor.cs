// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Accessor.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all types that manage multi thread access to list data in
//   the network.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for all types that manage multi thread access to list data in
    ///     the network.
    /// </summary>
    public abstract class Accessor
    {
        #region IDisposable Members

        /// <summary>
        ///     Removes the lock that is kept by this instance on the neuron.
        /// </summary>
        public abstract void Unlock();

        /// <summary>
        ///     Locks this instance.
        /// </summary>
        public abstract void Lock();

        #endregion
    }
}