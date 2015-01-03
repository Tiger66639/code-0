// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PassFrozenToCallerInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   when the current processor was created through a 'BlockedSolve' or
//   'BlockedCall' instruction, all the neurons that are frozen for the
//   current processor will also be frozen for the calling processor (the one
//   that is blocked). This is used to pass values back to the caller that are
//   frozen. If you don't call this function, all the frozen neurons will be
//   deleted by the time that they get to the caller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     when the current processor was created through a 'BlockedSolve' or
    ///     'BlockedCall' instruction, all the neurons that are frozen for the
    ///     current processor will also be frozen for the calling processor (the one
    ///     that is blocked). This is used to pass values back to the caller that are
    ///     frozen. If you don't call this function, all the frozen neurons will be
    ///     deleted by the time that they get to the caller.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.PassFrozenToCallerInstruction)]
    public class PassFrozenToCallerInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PassFrozenToCallerInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PassFrozenToCallerInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Any length is possible but at least 2 are required.
        /// </summary>
        public override int ArgCount
        {
            get
            {
                return 0;
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
            if (processor.BlockedProcessor != null)
            {
                processor.CopyFrozenTo(processor.BlockedProcessor);
            }
        }
    }
}