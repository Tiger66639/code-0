// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JmpIfInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   A specialized instruction, used by the NNL compiler to jump to a specific
//   location in the execution list of the current frame, if a condition is
//   met. (used to put function calls in boolean statements).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A specialized instruction, used by the NNL compiler to jump to a specific
    ///     location in the execution list of the current frame, if a condition is
    ///     met. (used to put function calls in boolean statements).
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: something that evaluates to <see langword="true" /> or
    ///     <see langword="false" /> (an expression or a static). 2: an intneuron that
    ///     defines the index positio to jump to (within the current frame).
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.JmpIfInstruction)]
    public class JmpIfInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.JmpIfInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.JmpIfInstruction;
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
                return 2;
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
            int iTo;
            if (CalculateInt(handler, args[1], out iTo) == false)
            {
                LogService.Log.LogError("JmpIf", "second argument should be an int");
            }
            else
            {
                bool iRes;
                if (CalculateBool(handler, args[0], out iRes) == false)
                {
                    LogService.Log.LogError("JmpIf", "first argument should be a bool");
                }
                else if (iRes)
                {
                    var iFrame = handler.CallFrameStack.Peek();
                    iFrame.NextExp = iTo - 1;

                        // we do -1 cause just after exeucting this instruction, we incrment the position.
                }
            }

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
            var iInt = args[1] as IntNeuron;
            if (iInt == null)
            {
                LogService.Log.LogError("JmpIf", "second argument should be an int");
                return;
            }

            if (args[0] == Brain.Current.TrueNeuron)
            {
                var iFrame = processor.CallFrameStack.Peek();
                iFrame.NextExp = iInt.Value - 1;

                    // we do -1 cause just after exeucting this instruction, we incrment the position.
            }
        }
    }
}