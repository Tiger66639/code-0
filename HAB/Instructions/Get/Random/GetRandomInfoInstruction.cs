// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetRandomInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the first child of the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the first child of the cluster.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The cluster for which to retur the first child. if no children,
    ///     <see langword="null" /> is returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetRandomInfoInstruction)]
    public class GetRandomInfoInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInfoInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetRandomInfoInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CheckArgs(list) == false)
            {
                return null;
            }

            var iFrom = list[0];
            var iTo = list[1];
            var iMeaning = list[2];

            var iLock = BuildLinkLock(list, false);
            LockManager.Current.RequestLocks(iLock);
            try
            {
                var iFound = FindLinkUnsafe(iFrom, iTo, iMeaning);
                if (iFound != null)
                {
                    ulong iId = 0;
                    if (iFound.InfoIdentifier != null)
                    {
                        // no need to creat the list if empty.
                        if (iFound.InfoDirect.Count > 0)
                        {
                            var iIndex = Randomizer.Next(iFound.InfoDirect.Count);
                            iId = iFound.InfoDirect[iIndex];
                        }
                    }

                    if (iId > 0)
                    {
                        return Brain.Current[iId];
                    }
                }
                else
                {
                    LogService.Log.LogWarning(
                        "GetRandomInfoInstruction.InternalGetValue", 
                        "Link doesn't exist, can't return random info item.");
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
            }

            return null;
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count < 3)
            {
                LogService.Log.LogError(
                    "GetRandomInfoInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
                return false;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError(
                    "GetRandomInfoInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError(
                    "GetRandomInfoInstruction.InternalGetValue", 
                    "Invalid second argument, Neuron expected, found null.");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError(
                    "GetRandomInfoInstruction.InternalGetValue", 
                    "Invalid third argument, Neuron expected, found null.");
                return false;
            }

            return true;
        }
    }
}