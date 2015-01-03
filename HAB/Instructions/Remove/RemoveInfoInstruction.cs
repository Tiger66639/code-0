// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes 1 or more info items from a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes 1 or more info items from a link.
    /// </summary>
    /// <remarks>
    ///     Args: 1: from part 2: to part 3: meaning part 4: first neuron to remove
    ///     from list.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveInfoInstruction)]
    public class RemoveInfoInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveInfoInstruction;
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
            if (args.Count >= 4)
            {
                if (CheckArgs(args) == false)
                {
                    return;
                }

                var iLock = BuildInfoLock(args, true);
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iLink = FindLinkUnsafe(args[0], args[1], args[2]);
                    if (iLink != null)
                    {
                        if (iLink.InfoIdentifier != null)
                        {
                            // check if there is info data, otherwise we can save some processing time + mem (don't need to create the list for a delete)
                            for (var i = 3; i < args.Count; i++)
                            {
                                iLink.InfoDirect.Remove(args[i]);
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "RemoveInfoInstruction.Execute", 
                            string.Format(
                                "Could not find link from={0}, to={1}, meaning={3}.  Failed to set info={2}", 
                                args[0], 
                                args[1], 
                                args[3], 
                                args[2]));
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }

                args[0].IsChanged = true;
                args[1].IsChanged = true;
                for (var i = 3; i < args.Count; i++)
                {
                    args[i].IsChanged = true;
                }
            }
            else
            {
                LogService.Log.LogError("RemoveInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> args)
        {
            if (args[0] == null)
            {
                LogService.Log.LogError("RemoveInfoInstruction.Execute", "From part is null (first arg).");
                return false;
            }

            if (args[1] == null)
            {
                LogService.Log.LogError("RemoveInfoInstruction.Execute", "To part is null (second arg).");
                return false;
            }

            if (args[2] == null)
            {
                LogService.Log.LogError("RemoveInfoInstruction.Execute", "Meaning pat is null (third arg).");
                return false;
            }

            if (args[3] == null)
            {
                LogService.Log.LogError("RemoveInfoInstruction.Execute", "Item to remove is null  (fourth arg).");
                return false;
            }

            return true;
        }
    }
}