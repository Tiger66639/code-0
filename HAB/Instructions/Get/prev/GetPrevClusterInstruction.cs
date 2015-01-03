// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetPrevClusterInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the next cluster that owns the specified neuron and has the
//   specified meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the next cluster that owns the specified neuron and has the
    ///     specified meaning.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The neuron for which to find a cluster that it belongs to.
    ///     2: the meaning that should be assigned to the cluster that needs to be
    ///     returned. 3: the cluster that should be just in front of the result.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetPrevClusterInstruction)]
    public class GetPrevClusterInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevClusterInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetPrevClusterInstruction;
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
            if (list != null && list.Count >= 3)
            {
                var iChild = list[0];
                var iMeaning = list[1];
                var iNext = list[2] as NeuronCluster;
                if (iChild != null)
                {
                    if (iMeaning != null)
                    {
                        if (iNext != null)
                        {
                            var iClusters = iChild.GetBufferedClusteredBy();
                            try
                            {
                                var iFound = false;
                                foreach (var i in iClusters)
                                {
                                    if (iFound == false)
                                    {
                                        if (i == iNext)
                                        {
                                            iFound = true;
                                        }
                                    }
                                    else if (i.Meaning == iMeaning.ID)
                                    {
                                        return i;
                                    }
                                }
                            }
                            finally
                            {
                                iChild.ReleaseBufferedCluseteredBy(iClusters);
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetNextClusterInstruction.InternalGetValue", 
                                "Invalid third argument, NeuronCluster expected.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetNextClusterInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetNextClusterInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetNextClusterInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}