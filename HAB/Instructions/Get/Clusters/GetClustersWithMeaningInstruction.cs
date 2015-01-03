// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClustersWithMeaningInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the clusters with the specified meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the clusters with the specified meaning.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The <see cref="Neuron" /> for which to return the owning
    ///     clusters 2: the neuron that idenfities the meaning of the clusters to
    ///     find.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetClustersWithMeaningInstruction)]
    public class GetClustersWithMeaningInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersWithMeaningInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetClustersWithMeaningInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = processor.Mem.ArgumentStack.Peek();
            if (list != null && list.Count >= 2)
            {
                var iChild = list[0];
                var iMeaning = list[1];
                if (iChild != null)
                {
                    if (iMeaning != null)
                    {
                        var iItems = iChild.GetBufferedClusteredBy();
                        try
                        {
                            foreach (var i in iItems)
                            {
                                // we use the buffered list to speed things up.
                                if (i.Meaning == iMeaning.ID)
                                {
                                    iRes.Add(i);
                                }
                            }
                        }
                        finally
                        {
                            iChild.ReleaseBufferedCluseteredBy(iItems);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetClustersWithMeaningInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetClustersWithMeaningInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetClustersWithMeaningInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }
        }
    }
}