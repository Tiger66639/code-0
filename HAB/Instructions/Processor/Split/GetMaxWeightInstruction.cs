// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetMaxWeightInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the neuron(s) with the maximum weight value in the split result
//   list. When there are multiple neurons with the same maximum result, all
//   of them are returned. When the list is empty, an empty list is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the neuron(s) with the maximum weight value in the split result
    ///     list. When there are multiple neurons with the same maximum result, all
    ///     of them are returned. When the list is empty, an empty list is returned.
    /// </summary>
    /// <remarks>
    ///     Arguments: none
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetMaxWeightInstruction)]
    public class GetMaxWeightInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetMaxWeightInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetMaxWeightInstruction;
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
                return 0;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = processor.Mem.ArgumentStack.Peek();
            processor.SplitValues.GetMaxWeights(iRes);
        }
    }
}