// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddSplitResultInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Adds a neuron to the <see cref="JaStDev.HAB.Processor.SplitValues" />
//   list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Adds a neuron to the <see cref="JaStDev.HAB.Processor.SplitValues" />
    ///     list.
    /// </summary>
    /// <remarks>
    ///     When the neuron is already in the list, it is not stored 2 times, so each
    ///     neuron is always stored uniquely. Arguments: a list of neurons.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AddSplitResultInstruction)]
    public class AddSplitResultInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddSplitResultInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AddSplitResultInstruction;
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

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null)
            {
                var iDict = processor.SplitValues;
                foreach (var i in args)
                {
                    iDict.AddSplitResult(i, 0); // only set the value if it hasn't already.
                }
            }
            else
            {
                LogService.Log.LogError("AddSplitResultInstruction.Execute", "No arguments specified");
            }
        }
    }
}