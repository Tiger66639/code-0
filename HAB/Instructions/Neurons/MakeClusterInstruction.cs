// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MakeClusterInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Creates a cluster of the specified items. The first argument identifies
//   the meaning of the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Creates a cluster of the specified items. The first argument identifies
    ///     the meaning of the cluster.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.MakeClusterInstruction)]
    public class MakeClusterInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MakeClusterInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.MakeClusterInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            if (list.Count > 0)
            {
                iRes.SetMeaning(list[0]);
                if (list.Count > 1)
                {
                    list.RemoveAt(0); // remove the item, so we can do an addrange with the entire list.
                    using (var iChildren = iRes.ChildrenW) iChildren.AddRange(list);
                }
            }

            return iRes;
        }
    }
}