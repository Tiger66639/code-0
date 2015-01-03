// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveLinkInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Destroys a link between 2 neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Destroys a link between 2 neurons.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: from part 2: to part 3: meaning
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveLinkInstruction)]
    public class RemoveLinkInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveLinkInstruction;
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
                if (args[0] == null || args[0].ID == EmptyId)
                {
                    LogService.Log.LogError("RemoveLinkInstruction.Execute", "From part is null (first arg).");
                    return;
                }

                if (args[1] == null || args[1].ID == EmptyId)
                {
                    LogService.Log.LogError("RemoveLinkInstruction.Execute", "To part is null (second arg).");
                    return;
                }

                if (args[2] == null || args[2].ID == EmptyId)
                {
                    LogService.Log.LogError("RemoveLinkInstruction.Execute", "meaning is null (third arg).");
                    return;
                }

                var iFound = Link.Find(args[0], args[1], args[2]);
                if (iFound != null)
                {
                    iFound.Destroy();
                }
                else
                {
                    LogService.Log.LogWarning("RemoveLinkInstruction.Execute", "Can't remove link: couldn't find it!");
                }
            }
            else
            {
                LogService.Log.LogError("RemoveLinkInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}