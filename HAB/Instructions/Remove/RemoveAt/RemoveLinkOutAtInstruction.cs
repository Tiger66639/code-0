// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveLinkOutAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes the incomming link at the specified index
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes the incomming link at the specified index
    /// </summary>
    /// <remarks>
    ///     Args: 1: The to part of the link 3: the index of the item to remove.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveLinkOutAtInstruction)]
    public class RemoveLinkOutAtInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkOutAtInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveLinkOutAtInstruction;
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

            if (args.Count >= 2)
            {
                IntNeuron iIndex = null;
                if (args[0] == null)
                {
                    LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "To part is null (first arg).");
                    return;
                }

                if (args[1] == null)
                {
                    LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "Index is null (second arg).");
                    return;
                }

                iIndex = args[3] as IntNeuron;
                if (iIndex == null)
                {
                    LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "Invalid index (fourth arg).");
                    return;
                }

                Link iFound = null;
                if (args[0].LinksInIdentifier != null)
                {
                    ListAccessor<Link> iLinks = args[0].LinksOut;
                    iLinks.Lock();
                    try
                    {
                        if (iIndex.Value >= 0 && iIndex.Value < iLinks.CountUnsafe)
                        {
                            iFound = iLinks.GetUnsafe(iIndex.Value);
                        }
                        else
                        {
                            LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "Index out of range.");
                        }
                    }
                    finally
                    {
                        iLinks.Dispose(); // also unlocks.
                    }
                }
                else
                {
                    LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "Index out of range.");
                }

                if (iFound != null)
                {
                    iFound.Destroy();
                }
                else
                {
                    LogService.Log.LogWarning(
                        "RemoveLinkOutAtInstruction.Execute", 
                        "Can't remove link: couldn't find it!");
                }
            }
            else
            {
                LogService.Log.LogError("RemoveLinkOutAtInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}