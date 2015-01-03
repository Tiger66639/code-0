// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterleafInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Interleafs the content of arg 1 (cluster or var) with the content of arg2
//   (cluster or var). Possibly a third argument can be supplied to close the
//   final leaf.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Interleafs the content of arg 1 (cluster or var) with the content of arg2
    ///     (cluster or var). Possibly a third argument can be supplied to close the
    ///     final leaf.
    /// </summary>
    /// <remarks>
    ///     If cluster1 = aaaa and cluster2 = b than result = abababa or var 1 = aaaa
    ///     var 2 = b var 3 = c -> ababaca
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.InterleafInstruction)]
    public class InterleafInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InterleafInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InterleafInstruction;
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
            var iStackUsageCount = 0;
            try
            {
                if (list != null && list.Count >= 2)
                {
                    var iRes = processor.Mem.ArgumentStack.Peek();
                    var iList = GetVarContent(processor, list[0], ref iStackUsageCount);
                    var iToInsert = GetVarContent(processor, list[1], ref iStackUsageCount);
                    System.Collections.Generic.IList<Neuron> iCloser;
                    Neuron iLast = null;
                    if (list.Count >= 3)
                    {
                        iCloser = GetVarContent(processor, list[2], ref iStackUsageCount);
                        if (iList.Count > 1)
                        {
                            iLast = iList[iList.Count - 1];
                            iList.RemoveAt(iList.Count - 1);

                                // remove the last item cause this will be used with the closer.
                        }
                    }
                    else
                    {
                        iCloser = null;
                    }

                    if (iList != null)
                    {
                        if (iToInsert != null)
                        {
                            if (iList.Count > 0)
                            {
                                for (var i = 0; i < iList.Count - 1; i++)
                                {
                                    iRes.Add(iList[i]);
                                    iRes.AddRange(iToInsert);
                                }

                                iRes.Add(iList[iList.Count - 1]);
                                if (iLast != null)
                                {
                                    iRes.AddRange(iCloser);
                                    iRes.Add(iLast);
                                }
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "Interleaf.GetValues", 
                                    "Nothing to interleaf: the first var or cluster has no values or children!");
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "Interleaf.GetValues", 
                                "Second argument should be a NeuronCluster or variable!");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Interleaf.GetValues", 
                            "First argument should be a NeuronCluster or variable!");
                    }
                }
                else
                {
                    LogService.Log.LogError("Interleaf.GetValues", "No arguments specified");
                }
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop(iStackUsageCount);
            }
        }

        /// <summary>The get var content.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="stackUsageCount">The stack usage count.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        private static System.Collections.Generic.IList<Neuron> GetVarContent(
            Processor processor, 
            Neuron neuron, 
            ref int stackUsageCount)
        {
            var iCluster = neuron as NeuronCluster;
            if (iCluster != null)
            {
                var iTemp = iCluster.GetBufferedChildren<Neuron>();

                    // we copy the buffered list in case that it gets reset and recycled: we don't want race conditions.
                try
                {
                    var iRes = processor.Mem.ArgumentStack.Push();
                    iRes.AddRange(iTemp);
                    stackUsageCount++;
                    return iRes;
                }
                finally
                {
                    iCluster.ReleaseBufferedChildren((System.Collections.IList)iTemp);
                }
            }

            var iVar = neuron as Variable;
            if (iVar != null)
            {
                stackUsageCount++;
                return SolveResultExpNoStackChange(iVar, processor);
            }

            return null;
        }
    }
}