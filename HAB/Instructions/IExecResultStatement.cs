// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExecResultStatement.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that should be implemented by instructions
//   that can have a result and who can calculate their arguments themsevles.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that should be implemented by instructions
    ///     that can have a result and who can calculate their arguments themsevles.
    /// </summary>
    public interface IExecResultStatement
    {
        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args);

        /// <summary>checks if the value can be returned as a bool.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanGetBool(System.Collections.Generic.IList<Neuron> args);

        /// <summary>gets the result as a <see langword="bool"/> value (argumnets still
        ///     need to be calculated).</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool GetBool(Processor handler, System.Collections.Generic.IList<Neuron> args);

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args">The args, not calculated.</param>
        /// <returns><c>true</c> if this instance [can get int] the specified args;
        ///     otherwise, <c>false</c> .</returns>
        bool CanGetInt(System.Collections.Generic.IList<Neuron> args);

        /// <summary>gets the result as an <see langword="int"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="int"/>.</returns>
        int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args);

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanGetDouble(System.Collections.Generic.IList<Neuron> args);

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args);
    }
}