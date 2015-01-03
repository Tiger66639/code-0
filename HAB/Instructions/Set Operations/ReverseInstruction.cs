// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReverseInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Reverses the order of the items in the argument list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Reverses the order of the items in the argument list.
    /// </summary>
    /// <remarks>
    ///     Arg: -1.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ReverseInstruction)]
    public class ReverseInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReverseInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReverseInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without any
        ///     specific number of values.
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            processor.Mem.ArgumentStack.Peek().AddRange(Enumerable.Reverse(list));
        }
    }
}