// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICalculateInt.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that should be implemented by instructions
//   that are able to return an <see langword="int" /> value. This is a speed
//   opt + also used by the parser to check the result type of an instruction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     an <see langword="interface" /> that should be implemented by instructions
    ///     that are able to return an <see langword="int" /> value. This is a speed
    ///     opt + also used by the parser to check the result type of an instruction.
    /// </summary>
    public interface ICalculateInt
    {
        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list);
    }
}