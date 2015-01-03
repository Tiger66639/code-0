// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddLinkInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Creates a link between 3 neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Creates a link between 3 neurons.
    /// </summary>
    /// <remarks>
    ///     Arg 1 = from neuron Arg 2 = to neuron Arg 3 = meaning neuron
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AddLinkInstruction)]
    public class AddLinkInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddLinkInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AddLinkInstruction;
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
                return 3;
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
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(args != null);

            if (args.Count >= 3)
            {
                if (args[0] == null)
                {
                    LogService.Log.LogError("AddLinkInstruction.Execute", "From part is null (first arg).");
                    return;
                }

                if (args[1] == null)
                {
                    LogService.Log.LogError("AddLinkInstruction.Execute", "To part is null (second arg).");
                    return;
                }

                if (args[2] == null)
                {
                    LogService.Log.LogError("AddLinkInstruction.Execute", "meaning is null (third arg).");
                    return;
                }

                var iNew = new Link(args[1], args[0], args[2]);

                    // don't do to much error checking, this is already done in the constructor of the link.
            }
            else
            {
                LogService.Log.LogError("AddLinkInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}