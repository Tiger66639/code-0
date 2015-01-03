// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FreezeInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Attaches 1 or more neurons to the currently running processor so that
//   when it dies out, the neuron will be removed if it hasn't changed in any
//   way since it got frozen. Note: a processor can die out because it is done
//   and not part of a split, or when the entire split is finished.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Attaches 1 or more neurons to the currently running processor so that
    ///     when it dies out, the neuron will be removed if it hasn't changed in any
    ///     way since it got frozen. Note: a processor can die out because it is done
    ///     and not part of a split, or when the entire split is finished.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.FreezeInstruction)]
    public class FreezeInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.FreezeInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.FreezeInstruction;
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
                    if (IsEmpty(i.ID) == false)
                    {
                        if (i.ModuleRefCount == 0)
                        {
                            i.IsFrozen = true; // simply let the item know it needs to be frozen.
                        }
                        else
                        {
                            LogService.Log.LogWarning("Freeze", "Can't freeze neurons that belong to a module: " + i.ID);
                        }
                    }
                }
            }
        }
    }
}