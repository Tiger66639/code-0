// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCommonOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the neurons that have an incomming link from the specified
//   neurons. Each clustered that is returned, must own all the arguments as
//   children (order is free, but can further be specified in the filter
//   part).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the neurons that have an incomming link from the specified
    ///     neurons. Each clustered that is returned, must own all the arguments as
    ///     children (order is free, but can further be specified in the filter
    ///     part).
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The first neuron for which to return the owning clusters,
    ///     followed by the others.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetCommonOutInstruction)]
    public class GetCommonOutInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonOutInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetCommonOutInstruction;
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
            if (list != null)
            {
                System.Collections.Generic.List<ulong> iCommmons = null;
                var iLock = BuildLinksOutLock(list, false);
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    iCommmons = GetCommonOutUnsafeUlong(list);
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }

                if (iCommmons != null)
                {
                    try
                    {
                        var iRes = processor.Mem.ArgumentStack.Peek();
                        foreach (var i in iCommmons)
                        {
                            if (i != EmptyId)
                            {
                                Neuron iFound;
                                if (Brain.Current.TryFindNeuron(i, out iFound))
                                {
                                    // could already have been deleted cause we are outside of the lock.
                                    iRes.Add(iFound);
                                }
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.IDLists.Recycle(iCommmons);
                    }
                }
            }
            else
            {
                LogService.Log.LogError("GetCommonOutInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }
        }
    }
}