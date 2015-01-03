// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClusterAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the neuron at the specified index in the list of clusters that
//   owns the neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the neuron at the specified index in the list of clusters that
    ///     owns the neuron.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The neuron to return the parent for.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 2: An IntNeuron, which contains the index position
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetClusterAtInstruction)]
    public class GetClusterAtInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClusterAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetClusterAtInstruction;
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
                var iNeuron = list[0];
                var iInt = GetAsInt(list[1]);
                if (iNeuron != null)
                {
                    if (iInt != null && iInt.HasValue)
                    {
                        ulong iRes = 0;
                        if (iNeuron.ClusteredByIdentifier != null)
                        {
                            var iBuffered = iNeuron.TryGetBufferedClusteredBy();
                            if (iBuffered != null)
                            {
                                return iBuffered[iInt.Value];
                            }

                            IDListAccessor iParents = iNeuron.ClusteredBy;
                            iParents.Lock();
                            try
                            {
                                if (iParents.CountUnsafe > iInt.Value && iInt.Value >= 0)
                                {
                                    iRes = iParents.GetUnsafe(iInt.Value);
                                }
                            }
                            finally
                            {
                                iParents.Unlock();
                                iParents.Dispose();
                            }
                        }

                        if (iRes > 0)
                        {
                            return Brain.Current[iRes];
                        }

                        LogService.Log.LogError(
                            "GetClusterAtInstruction.GetValues", 
                            string.Format("Index out of bounds for neuron {0}, index: {1}!", iNeuron.ID, iInt.Value));
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetClusterAtInstruction.GetValues", 
                            "Second argument should be an IntNeuron!");
                    }
                }
                else
                {
                    LogService.Log.LogError("GetClusterAtInstruction.GetValues", "Invalid first argument");
                }
            }
            else
            {
                LogService.Log.LogError("GetClusterAtInstruction.GetValues", "No arguments specified");
            }

            return null;
        }
    }
}