// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Adds 1 or more items to a <see cref="Neuron" /> cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Adds 1 or more items to a <see cref="Neuron" /> cluster.
    /// </summary>
    /// <remarks>
    ///     <para>Args: - Cluster</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>first item to add</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AddChildInstruction)]
    public class AddChildInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AddChildInstruction;
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
            if (args != null && args.Count >= 2)
            {
                if (args[1] == null)
                {
                    LogService.Log.LogError("AddChildInstruction.Execute", "item to add is null (second arg).");
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.ID == TempId)
                    {
                        Brain.Current.Add(iCluster);

                            // make certain that the cluster exists before trying to lock anything.
                    }

                    var iLock = BuildClusterLock(args, true);
                    LockManager.Current.RequestLocks(iLock);
                    try
                    {
                        if (iCluster.IsDeleted == false)
                        {
                            // make certain that the cluster isn't being deleted. Coudl be in the process of being deleted (the deletion list is ready, but not yet the actual delete operation).
                            for (var i = 1; i < args.Count; i++)
                            {
                                if (args[i].IsDeleted == false)
                                {
                                    // make certain that the item isn't being deleted. Coudl be in the process of being deleted (the deletion list is ready, but not yet the actual delete operation).
                                    if (args[i].ID == TempId)
                                    {
                                        Brain.Current.Add(args[i]);

                                            // make certain that the cluster exists before trying to lock anything.
                                    }

                                    iCluster.AddChildUnsafe(args[i]);
                                }
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iLock);
                        if (iCluster.IsDeleted == false)
                        {
                            iCluster.IsChanged = true;
                        }

                        for (var i = 1; i < args.Count; i++)
                        {
                            if (args[i].IsDeleted == false)
                            {
                                // make certain that the item isn't being deleted. Coudl be in the process of being deleted (the deletion list is ready, but not yet the actual delete operation).
                                args[i].IsChanged = true;
                            }
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "AddChildInstruction.Execute", 
                        string.Format("Can't add, Invalid cluster specified: {0}.", args[0]));
                }
            }
            else
            {
                if (args.Count == 1 && Settings.LogAddChildInvalidArgs == false)
                {
                    // don't logg an error if there were no items to add and the logging for this has been turned off.
                    return;
                }

                LogService.Log.LogError("AddChildInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}