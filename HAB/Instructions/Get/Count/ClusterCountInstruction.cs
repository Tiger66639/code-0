// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterCountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of children contained in the specified cluster
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of children contained in the specified cluster
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 1: The cluster for which to return the nr of children.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ClusterCountInstruction)]
    public class ClusterCountInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClusterCountInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ClusterCountInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iN = list[0];
                if (iN != null)
                {
                    System.Collections.IList iTemp = iN.TryGetBufferedClusteredBy();
                    if (iTemp != null)
                    {
                        return iTemp.Count;
                    }

                    if (iN.ClusteredByIdentifier == null)
                    {
                        return -1;
                    }

                    LockManager.Current.RequestLock(iN, LockLevel.Parents, false);
                    try
                    {
                        return ((ClusterList)iN.ClusteredByIdentifier).Count;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(iN, LockLevel.Parents, false);
                    }
                }

                LogService.Log.LogError("ClusterCountInstruction.GetValues", "First argument should be a Neuron!");
            }
            else
            {
                LogService.Log.LogError("ClusterCountInstruction.GetValues", "No arguments specified");
            }

            return -1;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetInt(CalculateInt(processor, list));
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}