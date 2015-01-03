// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetLastClusterInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the last cluster that owns the specified neuron and has the
//   specified meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the last cluster that owns the specified neuron and has the
    ///     specified meaning.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a shortcut instruction that combines a few statements
    ///         (getClusters, loop, check meaning).
    ///     </para>
    ///     <para>
    ///         Arguments: 1: The neuron for which to find a cluster that it belongs to.
    ///         2: the meaning that should be assigned to the cluster that needs to be
    ///         returned.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetLastClusterInstruction)]
    public class GetLastClusterInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastClusterInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetLastClusterInstruction;
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
                        var iClusters = iChild.GetBufferedClusteredBy();
                        try
                        {
                            for (var i = iClusters.Count - 1; i >= 0; i--)
                            {
                                if (iClusters[i].Meaning == iMeaning.ID)
                                {
                                    return iClusters[i];
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
                            "GetLastClusterInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetLastClusterInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetLastClusterInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}