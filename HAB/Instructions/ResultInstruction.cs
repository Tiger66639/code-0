// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ResultInstruction type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    /// </summary>
    public abstract class ResultInstruction : Instruction
    {
        /// <summary>performs the task and returns it's result. Results are returned by
        ///     adding them to the last item on the<see cref="processor.Mem.ArgumentStack"/> .</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list of arguments</param>
        public abstract void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list);
    }
}