// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClustersFilteredInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the clusters that own the specified neuron, filtered using
//   the specified filter code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the clusters that own the specified neuron, filtered using
    ///     the specified filter code.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: The <see cref="Neuron" /> for which to return the owning
    ///     clusters 2: a reference to a variable that is used in the next argument.
    ///     It temporarely stores every (non filtered) cluster. this is usually
    ///     passed along using a <see cref="ByRefExpression" /> . 3: a reference to a
    ///     result statement that determins if a cluster should be included in the
    ///     result or not. This is usually a <see cref="BoolExpression" /> , using
    ///     the previously specified var to compare things against. this is usually
    ///     passed along using a <see cref="ByRefExpression" /> .
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetClustersFilteredInstruction)]
    public class GetClustersFilteredInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersFilteredInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetClustersFilteredInstruction;
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
                return 3;
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
            if (list != null && list.Count >= 3)
            {
                var iChild = list[0];
                var iVar = list[1] as Variable;
                var iExp = list[2] as ResultExpression;
                if (iChild != null)
                {
                    if (iVar != null)
                    {
                        if (iExp != null)
                        {
                            var iClusters = iChild.GetBufferedClusteredBy();
                            try
                            {
                                foreach (var i in iClusters)
                                {
                                    iVar.StoreValue(i, processor);

                                        // we assign the current cluster to the var so the expression can evaluate it correctly.
                                    var iResOk = false;

                                        // init to false, so we can handle the case in which there is no result.
                                    try
                                    {
                                        foreach (var iResItem in SolveResultExpNoStackChange(iExp, processor))
                                        {
                                            if (iResItem.ID != (ulong)PredefinedNeurons.True)
                                            {
                                                iResOk = false; // if there is is anything but a true, we're in trouble.
                                                break;
                                            }

                                            iResOk = true; // if there is a true, we have an ok.
                                        }
                                    }
                                    finally
                                    {
                                        processor.Mem.ArgumentStack.Pop();

                                            // we used the stack to get the result, so don't forget to free it again so that it can be reused.
                                    }

                                    if (iResOk)
                                    {
                                        iRes.Add(i);
                                    }
                                }
                            }
                            finally
                            {
                                iChild.ReleaseBufferedCluseteredBy(iClusters);
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetClustersFilteredInstruction.InternalGetValue", 
                                "Invalid third argument, ResultExpression expected.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetClustersFilteredInstruction.InternalGetValue", 
                            "Invalid second argument, Variable expected.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetClustersFilteredInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetClustersFilteredInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }
        }
    }
}