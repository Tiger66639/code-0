// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Searches for the link between the first 2 neurons in the arg list and
//   adds all the following arguments to the info section of the link, if
//   found.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Searches for the link between the first 2 neurons in the arg list and
    ///     adds all the following arguments to the info section of the link, if
    ///     found.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a special implementation of the <see cref="AddInstruction" /> .
    ///         It takes less arguments.
    ///     </para>
    ///     <para>
    ///         arguments: 1: from part of link 2: to part of link 3: meaning part of
    ///         link. 4: info to set. If the link is not found, nothing is done.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AddInfoInstruction)]
    public class AddInfoInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AddInfoInstruction;
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

        /// <summary>The execute.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count >= 4)
            {
                if (VerifyArgs(args) == false)
                {
                    return;
                }

                for (var i = 3; i < args.Count; i++)
                {
                    if (args[i].ID == TempId)
                    {
                        Brain.Current.Add(args[i]);

                            // add temp items outside of the lock, this is required to make it save (no writable cache lock inside a neuron lock).
                    }
                }

                var iLock = BuildInfoLock(args, true);
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iLink = FindLinkUnsafe(args[0], args[1], args[2]);
                    if (iLink != null)
                    {
                        for (var i = 3; i < args.Count; i++)
                        {
                            iLink.InfoDirect.AddItemUnsafe(args[i]); // no need to lock anymore, is already done.
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "AddInfoInstruction.Execute", 
                            string.Format(
                                "Could not find link from={0}, to={1}, meaning={2}.  Failed to set info={3}", 
                                args[0], 
                                args[1], 
                                args[2], 
                                args[3]));
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
                LogService.Log.LogError("AddInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The verify args.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool VerifyArgs(System.Collections.Generic.IList<Neuron> args)
        {
            if (args[0] == null)
            {
                LogService.Log.LogError("AddInfoInstruction.Execute", "From part is null (first arg).");
                return false;
            }

            if (args[1] == null)
            {
                LogService.Log.LogError("AddInfoInstruction.Execute", "To part is null (second arg).");
                return false;
            }

            if (args[2] == null)
            {
                LogService.Log.LogError("AddInfoInstruction.Execute", "Meaning pat is null (third arg).");
                return false;
            }

            if (args[3] == null)
            {
                LogService.Log.LogError("AddInfoInstruction.Execute", "Item to add is null  (fourth arg).");
                return false;
            }

            return true;
        }
    }
}