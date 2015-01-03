// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetRandomInInstruction.cs" company="">
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
    [NeuronID((ulong)PredefinedNeurons.GetRandomInInstruction)]
    public class GetRandomInInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetRandomInInstruction;
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
            Neuron iRes = null;
            if (list.Count >= 2)
            {
                var iTo = list[0];
                if (iTo != null)
                {
                    var iMeaning = list[1];
                    if (iMeaning != null)
                    {
                        if (iTo.LinksInIdentifier != null)
                        {
                            ulong iId;
                            ListAccessor<Link> iLinksOut = iTo.LinksIn;
                            iLinksOut.Lock();
                            try
                            {
                                var iIndex = Randomizer.Next(iLinksOut.CountUnsafe);
                                iId = iLinksOut.GetUnsafe(iIndex).ToID;
                            }
                            finally
                            {
                                iLinksOut.Unlock();
                                iLinksOut.Dispose();
                            }

                            iRes = Brain.Current[iId];
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetRandomInInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetRandomInInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetRandomInInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}