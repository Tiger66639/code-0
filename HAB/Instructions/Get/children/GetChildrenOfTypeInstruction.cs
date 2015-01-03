// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetChildrenOfTypeInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the children of the specified type for the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the children of the specified type for the cluster.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The cluster for which to return the children 2: the neuron
    ///     that idenfities the type of children that should be returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetChildrenOfTypeInstruction)]
    public class GetChildrenOfTypeInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IndexOfChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetChildrenOfTypeInstruction;
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
                var iCluster = list[0] as NeuronCluster;
                var iType = list[1];
                if (iCluster != null)
                {
                    if (iType != null)
                    {
                        var iItems = iCluster.GetBufferedChildren<Neuron>();
                        try
                        {
                            foreach (var i in iItems)
                            {
                                if (i is Instruction && iType.ID == (ulong)PredefinedNeurons.Instruction)
                                {
                                    iRes.Add(i);
                                }
                                else if (i.TypeOfNeuron == iType)
                                {
                                    iRes.Add(i);
                                }
                            }
                        }
                        finally
                        {
                            iCluster.ReleaseBufferedChildren((System.Collections.IList)iItems);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetChildrenOfTypeInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetChildrenOfTypeInstruction.InternalGetValue", 
                        "Invalid first argument, NeuronCluster expected, found ." + list[0]);
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetChildrenOfTypeInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }
        }
    }
}