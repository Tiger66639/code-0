// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitWeightedInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   Splits up the processor into identical sub processors which continue
//   processing from the same point. For each argument after the third one, a
//   new sub processor is created. Each processor is assigned an accumulative
//   weight. The accumulus needs to be supplied as an int. The first item in
//   the split always gets a value of 0, the second gets the accumulus, next
//   accumulus * 2,....
//   </para>
//   <para>
//   When the split is done, all results are combined. If multiple processors
//   have the same result, that result will only be included 1 time and
//   receive the maximum weight that it had accross all processors. Whe a
//   split is done while there is already a previous result added to the
//   processor, all sub processors will inherit the same result.
//   </para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     <para>
    ///         Splits up the processor into identical sub processors which continue
    ///         processing from the same point. For each argument after the third one, a
    ///         new sub processor is created. Each processor is assigned an accumulative
    ///         weight. The accumulus needs to be supplied as an int. The first item in
    ///         the split always gets a value of 0, the second gets the accumulus, next
    ///         accumulus * 2,....
    ///     </para>
    ///     <para>
    ///         When the split is done, all results are combined. If multiple processors
    ///         have the same result, that result will only be included 1 time and
    ///         receive the maximum weight that it had accross all processors. Whe a
    ///         split is done while there is already a previous result added to the
    ///         processor, all sub processors will inherit the same result.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: the <see cref="NeuronCluster" /> that should be called
    ///     (executed) when the split is ready. 2: the accumulus. This needs to be an
    ///     int. Specify 1 to have each subsequent processor to have a higher weight
    ///     of 1 . Specify -1 so that each has a smaller weight. 3: the var that will
    ///     contain the result of the split (must be declared as byref, or as the
    ///     content of another var). 4: a list of neurons on which to perform the
    ///     split (for each item, a new sub processor will be created and in that
    ///     processor, only
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SplitWeightedInstruction)]
    public class SplitWeightedInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitWeightedInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SplitWeightedInstruction;
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

        #region IExecStatement Members

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count >= 3)
            {
                var iArgs = processor.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    var i = 0;
                    int iInt;
                    LoadArgsUntil(processor, args, iArgs, 1, ref i);
                    if (args.Count > i)
                    {
                        var iHasInt = true;
                        if (CalculateInt(processor, args[i], out iInt) == false)
                        {
                            // when can't do direct convert, try to get the value of the xpression (already know it isn't an int or double neuron, cause that would be caught by CalculateInt)
                            iHasInt = false;
                            var iExp = args[i] as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }

                            if (iArgs.Count >= 2)
                            {
                                var iconv = GetAsInt(iArgs[1]);
                                if (iconv.HasValue)
                                {
                                    iHasInt = true;
                                    iInt = iconv.Value;
                                    iArgs.RemoveAt(1);

                                        // remove the int from the argument stack, cuase this version doesn't allow it in the list like that (so we have the same method of working as when we can calculate the int directly).
                                }
                            }
                        }

                        i++;
                        LoadArgsFrom(processor, args, iArgs, ref i);
                        if (iHasInt)
                        {
                            DoSplit(processor, iInt, iArgs, 2); // 2 cause the int isn't in the list.
                        }
                        else
                        {
                            LogService.Log.LogError("SplitWeightedInstruction.Execute", "Can't split, Invalid weight.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("SplitWeightedInstruction.Execute", "Invalid nr of arguments specified");
                    }
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }

                return true;
            }

            return false;
        }

        #endregion

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
            if (args.Count >= 4)
            {
                var iInt = args[1] as IntNeuron;
                if (iInt != null)
                {
                    DoSplit(processor, iInt.Value, args, 3);
                }
                else
                {
                    LogService.Log.LogError(
                        "SplitWeightedInstruction.Execute", 
                        "Invalid 2th argument, IntNeuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("SplitWeightedInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The do split.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="args">The args.</param>
        /// <param name="startFrom">The start from.</param>
        private static void DoSplit(
            Processor processor, 
            int weight, System.Collections.Generic.IList<Neuron> args, 
            int startFrom)
        {
            if (processor.SplitAllowed)
            {
                if (args.Count > startFrom)
                {
                    var iCode = args[0] as NeuronCluster;
                    if (iCode != null)
                    {
                        var iVar = args[startFrom - 1] as Variable;
                        if (iVar != null)
                        {
                            var iSplitArgs = new ThreadManager.SplitArgs(processor);
                            iSplitArgs.Callback = iCode;
                            iSplitArgs.Variable = iVar;
                            iSplitArgs.Processor = processor;
                            iSplitArgs.Weight = weight;
                            if (iSplitArgs.ToSplit.Capacity < args.Count - startFrom)
                            {
                                iSplitArgs.ToSplit.Capacity = args.Count - startFrom;
                            }

                            for (var i = startFrom; i < args.Count; i++)
                            {
                                iSplitArgs.ToSplit.Add(args[i]);
                            }

                            ThreadManager.Default.Split(iSplitArgs);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "SplitWeightedInstruction.Execute", 
                                "Invalid 3th argument, Variable expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SplitWeightedInstruction.Execute", 
                            "Invalid first argument, NeuronCluster (callback code) expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError("SplitWeightedInstruction.Execute", "Invalid nr of arguments specified");
                }
            }
            else
            {
                LogService.Log.LogError("SplitWeightedInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}