// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes 1 or more neurons from a cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes 1 or more neurons from a cluster.
    /// </summary>
    /// <remarks>
    ///     Args: 1: The neuron cluster 2: the first item to remove from the cluster
    ///     (more may follow).
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveChildInstruction)]
    public class RemoveChildInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveChildInstruction;
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
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                if (args[1] == null || args[1].ID == EmptyId)
                {
                    LogService.Log.LogError("RemoveChildInstruction.Execute", "item to remove is null (second arg).");
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iLock = BuildClusterLock(args, true);
                    LockManager.Current.RequestLocks(iLock);
                    try
                    {
                        for (var i = 1; i < args.Count; i++)
                        {
                            if (args[i].IsDeleted == false)
                            {
                                args[i].RemoveClusterUnsafe(iCluster);
                                iCluster.RemoveChildUnsafe(args[i]);
                            }
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iLock);
                        iCluster.IsChanged = true; // do outside of lock, to prevent deadlocks.
                        for (var i = 1; i < args.Count; i++)
                        {
                            if (args[i].IsDeleted == false)
                            {
                                args[i].IsChanged = true;
                            }
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "RemoveChildInstruction.Execute", 
                        string.Format("Can't remove, Invalid cluster specified: {}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("RemoveChildInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}