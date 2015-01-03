// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitAccumInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   Splits up the processor into identical sub processors which continue
//   processing from the same point. For each argument after the third one, a
//   new sub processor is created.
//   </para>
//   <para>
//   When the split is finished, all results are added together. If multiple
//   processors have the same result, it's weight will be added together
//   (accumulus). When a processor splits, non of it's results are compied
//   over to the sub processors, they start with a clean slate.
//   </para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     <para>
    ///         Splits up the processor into identical sub processors which continue
    ///         processing from the same point. For each argument after the third one, a
    ///         new sub processor is created.
    ///     </para>
    ///     <para>
    ///         When the split is finished, all results are added together. If multiple
    ///         processors have the same result, it's weight will be added together
    ///         (accumulus). When a processor splits, non of it's results are compied
    ///         over to the sub processors, they start with a clean slate.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: the <see cref="NeuronCluster" /> that should be called
    ///     (executed) when the split is ready. 2: the var that will contain the
    ///     result of the split (must be declared as byref, or as the content of
    ///     another var). 3: a list of neurons on which to perform the split (for
    ///     each item, a new sub processor will be created and in that processor,
    ///     only
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SplitAccumInstruction)]
    public class SplitAccumInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitAccumInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SplitAccumInstruction;
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

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            processor.CheckStopRequested();

                // the split instruction can be very costly, so check if a stop was requested before actually performing this task.
            if (processor.SplitAllowed)
            {
                if (args.Count >= 3)
                {
                    var iCode = args[0] as NeuronCluster;
                    if (iCode != null)
                    {
                        var iVar = args[1] as Variable;
                        if (iVar != null)
                        {
                            var iSplitArgs = new ThreadManager.SplitArgs(processor);
                            iSplitArgs.Callback = iCode;
                            iSplitArgs.Variable = iVar;
                            iSplitArgs.Processor = processor;
                            iSplitArgs.Weight = 0;
                            iSplitArgs.IsAccum = true;
                            if (iSplitArgs.ToSplit.Capacity < args.Count - 2)
                            {
                                iSplitArgs.ToSplit.Capacity = args.Count - 2;
                            }

                            for (var i = 2; i < args.Count; i++)
                            {
                                iSplitArgs.ToSplit.Add(args[i]);
                            }

                            ThreadManager.Default.Split(iSplitArgs);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "SplitAccum.Execute", 
                                "Invalid second argument, Variable expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SplitAccum.Execute", 
                            "Invalid first argument, NeuronCluster (callback code) expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError("SplitAccum.Execute", "Invalid nr of arguments specified");
                }
            }
            else
            {
                LogService.Log.LogError("SplitAccum.Execute", "Split not allowed in current context");
            }
        }
    }
}