// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetRandomChildInstruction.cs" company="">
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
    [NeuronID((ulong)PredefinedNeurons.GetRandomChildInstruction)]
    public class GetRandomChildInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomChildInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetRandomChildInstruction;
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
            if (list.Count >= 1)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.ChildrenIdentifier != null)
                    {
                        ulong iId = 0;
                        var iChildren = iCluster.Children;
                        iChildren.Lock();
                        try
                        {
                            if (iCluster.ChildrenDirect.Count > 0)
                            {
                                var iIndex = Randomizer.Next(iCluster.ChildrenDirect.Count);
                                iId = iCluster.ChildrenDirect[iIndex];
                            }
                        }
                        finally
                        {
                            iChildren.Dispose();
                        }

                        if (iId > 0)
                        {
                            return Brain.Current[iId];
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetRandomChildInstruction.InternalGetValue", 
                        "Invalid first argument, NeuronCluster expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetRandomChildInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}