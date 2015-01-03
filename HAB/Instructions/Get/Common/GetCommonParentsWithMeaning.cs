// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCommonParentsWithMeaning.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the clusters that own the specified neurons, where all the
//   returned clusters have the specified type. Each clustered that is
//   returned, must own all the arguments as children (order is free, but can
//   further be specified in the filter part).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the clusters that own the specified neurons, where all the
    ///     returned clusters have the specified type. Each clustered that is
    ///     returned, must own all the arguments as children (order is free, but can
    ///     further be specified in the filter part).
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The meaning that all clusters must have. 2: The first
    ///     neuron for which to return the owning clusters, followed by the others.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetCommonParentsWithMeaningInstruction)]
    public class GetCommonParentsWithMeaning : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonParentsWithMeaningInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetCommonParentsWithMeaningInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iFilterVal = list[0];
                System.Collections.Generic.List<ulong> iCommmons = null;
                list.RemoveAt(0);

                    // we remove the first item from the list, cause this is the meaning we want to filter on, don't want this in the corresponding calculation
                if (iFilterVal != null)
                {
                    var iLock = BuildParentsLockFrom(list, 0, false);
                    LockManager.Current.RequestLocks(iLock);
                    try
                    {
                        iCommmons = GetCommonParentsUnsafeUlong(list);
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iLock);
                    }

                    if (iCommmons != null)
                    {
                        ProcessResults(iCommmons, processor, iFilterVal);

                            // needs to be done outside of iLock, cause otherwise we can get a deadlock while trying to get the parent neurons from the cache.
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetCommonParentsWithMeaning.GetValues", 
                        "Invalid first argument, Neuron expected.");
                }
            }
            else
            {
                LogService.Log.LogError("GetCommonParentsWithMeaning.GetValues", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The process results.</summary>
        /// <param name="list">The list.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="filter">The filter.</param>
        private static void ProcessResults(System.Collections.Generic.List<ulong> list, 
            Processor processor, 
            Neuron filter)
        {
            try
            {
                var iRes = processor.Mem.ArgumentStack.Peek();
                foreach (var i in list)
                {
                    if (i != EmptyId)
                    {
                        Neuron iFound;
                        if (Brain.Current.TryFindNeuron(i, out iFound))
                        {
                            // could already have been deleted cause we are outside of the lock.
                            var iToAdd = iFound as NeuronCluster;
                            if (iToAdd != null && iToAdd.Meaning == filter.ID)
                            {
                                iRes.Add(iToAdd);
                            }
                        }
                    }
                }
            }
            finally
            {
                Factories.Default.IDLists.Recycle(list);
            }
        }
    }
}