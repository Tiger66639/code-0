// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReturnInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Stops the current function without returns any possible result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Stops the current function without returns any possible result.
    /// </summary>
    /// <remarks>
    ///     Arguments: any result that needs to be returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ReturnInstruction)]
    public class ReturnInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReturnInstruction;
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

        #region IExecStatement Members

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            handler.ExitFramesUntillReturn();
            return true;
        }

        #endregion

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
            processor.ExitFramesUntillReturn();
        }
    }
}