// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICalculateDouble.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that should be implemented by instructions
//   that are able to return a <see langword="double" /> value. This is a speed
//   opt + also used by the parser to check the result type of an instruction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that should be implemented by instructions
    ///     that are able to return a <see langword="double" /> value. This is a speed
    ///     opt + also used by the parser to check the result type of an instruction.
    /// </summary>
    public interface ICalculateDouble
    {
        /// <summary>Calculate the <see langword="double"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="double"/>.</returns>
        double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list);
    }
}