// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnionInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Combines the arguments and returns them as a single list (not a new
//   cluster). This is useful to combine a number of neurons into a single
//   variable.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Combines the arguments and returns them as a single list (not a new
    ///     cluster). This is useful to combine a number of neurons into a single
    ///     variable.
    /// </summary>
    /// <remarks>
    ///     Arg: -1.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.UnionInstruction)]
    public class UnionInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.UnionInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.UnionInstruction;
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
            processor.Mem.ArgumentStack.Peek().AddRange(list);
        }
    }
}