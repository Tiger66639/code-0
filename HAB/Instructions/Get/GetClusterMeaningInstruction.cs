// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClusterMeaningInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the meaning of a cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the meaning of a cluster.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 1: The cluster for which to return the meaning. If it has no meaning,
    ///                 <see cref="JaStDev.HAB.PredefinedNeurons.Empty" /> is returned.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetClusterMeaningInstruction)]
    public class GetClusterMeaningInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ConditionalPart" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetClusterMeaningInstruction;
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
                return 1;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.Meaning != EmptyId)
                    {
                        return Brain.Current[iCluster.Meaning];
                    }

                    return null;
                }

                if (Settings.LogGetClusterMeaningInvalidArgs)
                {
                    LogService.Log.LogError(
                        "GetClusterMeaningInstruction.GetValues", 
                        "First argument should be a NeuronCluster!");
                }
            }
            else if (Settings.LogGetClusterMeaningInvalidArgs)
            {
                LogService.Log.LogError("GetClusterMeaningInstruction.GetValues", "No arguments specified");
            }

            return null;
        }
    }
}