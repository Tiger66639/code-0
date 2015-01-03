// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetLastChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the last child of the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the last child of the cluster.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The cluster for which to retur the first child. if no children,
    ///     <see langword="null" /> is returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetLastChildInstruction)]
    public class GetLastChildInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstChildInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetLastChildInstruction;
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
            Neuron iRes = null;
            if (list.Count >= 1)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.ChildrenIdentifier != null)
                    {
                        ulong iLast = 0;
                        var iChildren = iCluster.Children;
                        iChildren.Lock();
                        try
                        {
                            iLast = iCluster.ChildrenDirect[iCluster.ChildrenDirect.Count - 1];
                        }
                        finally
                        {
                            iChildren.Dispose();
                        }

                        if (iLast > 0)
                        {
                            return Brain.Current[iLast];
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetLastChildInstruction.InternalGetValue", 
                        "Invalid first argument, NeuronCluster expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetLastChildInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}