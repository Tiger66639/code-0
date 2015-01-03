// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetPrevChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the prev child of the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the prev child of the cluster.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The cluster for which to return the next child. if no children,
    ///     <see langword="null" /> is returned. 2: the next child
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetPrevChildInstruction)]
    public class GetPrevChildInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetPrevChildInstruction;
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
            if (list.Count >= 2)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.ChildrenIdentifier != null)
                    {
                        var iPrev = list[1];
                        if (iPrev != null)
                        {
                            ulong iRes = 0;
                            var iChildren = iCluster.Children;
                            iChildren.Lock();
                            try
                            {
                                for (var i = iCluster.ChildrenDirect.Count - 1; i >= 0; i--)
                                {
                                    if (iCluster.ChildrenDirect[i] == iPrev.ID)
                                    {
                                        if (i > 0)
                                        {
                                            iRes = iCluster.ChildrenDirect[i - 1];
                                            break;
                                        }

                                        return null;
                                    }
                                }
                            }
                            finally
                            {
                                iChildren.Dispose();
                            }

                            if (iRes > 0)
                            {
                                return Brain.Current[iRes];
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetPrevChildInstruction.InternalGetValue", 
                                "Invalid second argument, Neuron expected, found null.");
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetPrevChildInstruction.InternalGetValue", 
                        "Invalid first argument, NeuronCluster expected.");
                }
            }
            else
            {
                LogService.Log.LogError("GetPrevChildInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}