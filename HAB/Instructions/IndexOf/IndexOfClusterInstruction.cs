// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexOfClusterInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the index number of the second argument into the list of clusters
//   of the first argument,
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the index number of the second argument into the list of clusters
    ///     of the first argument,
    /// </summary>
    /// <remarks>
    ///     <para>arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a neuron</description>
    ///         </item>
    ///         <item>
    ///             <description>a cluster to search for.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IndexOfClusterInstruction)]
    public class IndexOfClusterInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IndexOfClusterInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IndexOfClusterInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var iSearchIn = list[0];
                var iCluster = list[1] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iSearchIn != null)
                    {
                        if (iSearchIn.ClusteredByIdentifier != null)
                        {
                            using (var iList = iSearchIn.ClusteredBy) return iList.IndexOf(iCluster.ID);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "IndexOfClusterInstruction.InternalGetValue", 
                            "Invalid first argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "IndexOfClusterInstruction.InternalGetValue", 
                        "Invalid second argument, NeuronCluster expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "IndexOfClusterInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return -1;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The count of the list.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetInt(CalculateInt(processor, list));
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}