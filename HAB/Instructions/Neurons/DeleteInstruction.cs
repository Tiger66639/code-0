// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes the Neurons defined in the arguments list from the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes the Neurons defined in the arguments list from the brain.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.DeleteInstruction)]
    public class DeleteInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DeleteInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.DeleteInstruction;
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
            foreach (var i in args)
            {
                if (i != null)
                {
                    // need to make certain we don't try to delete items that have already been delted. if this is the case, report it.
                    if (i.ID != EmptyId)
                    {
                        Brain.Current.Delete(i);

                        // Debug.Print(iId.ToString());
                    }
                    else
                    {
                        LogService.Log.LogWarning(
                            "DeleteInstruction.Execute", 
                            "Trying to delete a neuron that is not registered with the network, possibly it has already been deleted?");
                    }
                }
            }
        }
    }
}