// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessorsOwner.cs" company="">
//   
// </copyright>
// <summary>
//   The ProcessorsOwner interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The ProcessorsOwner interface.</summary>
    public interface IProcessorsOwner
    {
        /// <summary>
        ///     Gets the list of processors, for <see langword="internal" /> use.
        /// </summary>
        /// <value>
        ///     The processors.
        /// </value>
        Data.ObservedCollection<ProcManItem> Processors { get; }

        /// <summary>
        ///     Gets the list of processors for UI use.
        /// </summary>
        /// <value>
        ///     The UI processors.
        /// </value>
        System.Collections.Generic.List<ProcManItem> UIProcessors { get; }

        /// <summary>
        ///     Called when the ui list of processors needs to be updated.
        /// </summary>
        void ProcessorsChanged();
    }
}