// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExecStatement.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that should be implemented by instructions
//   that can calculate their arguments themsevles and which don't return a
//   value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that should be implemented by instructions
    ///     that can calculate their arguments themsevles and which don't return a
    ///     value.
    /// </summary>
    public interface IExecStatement
    {
        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args);
    }
}