// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetRandomClusterInstruction.cs" company="">
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
    [NeuronID((ulong)PredefinedNeurons.GetRandomClusterInstruction)]
    public class GetRandomClusterInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomClusterInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetRandomClusterInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var iChild = list[0];
                var iMeaning = list[1];
                if (iChild != null)
                {
                    if (iMeaning != null)
                    {
                        if (iChild.ClusteredByIdentifier != null)
                        {
                            ulong iTemp;
                            ListAccessor<ulong> iClusteredBy = iChild.ClusteredBy;
                            iClusteredBy.Lock();
                            try
                            {
                                var iIndex = Randomizer.Next(iClusteredBy.CountUnsafe);
                                iTemp = iClusteredBy.GetUnsafe(iIndex);
                            }
                            finally
                            {
                                iClusteredBy.Unlock();
                                iClusteredBy.Dispose();
                            }

                            return Brain.Current[iTemp];
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetRandomClusterInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetRandomClusterInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetRandomClusterInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}