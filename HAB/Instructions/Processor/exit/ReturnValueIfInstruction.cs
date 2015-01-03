// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReturnValueIfInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   A customized version of the <see cref="ReturnValueInstruction" /> . Stops
//   the current function and returns a possible result (including an empty
//   result), if a condition is met, which must be specified in the first
//   argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A customized version of the <see cref="ReturnValueInstruction" /> . Stops
    ///     the current function and returns a possible result (including an empty
    ///     result), if a condition is met, which must be specified in the first
    ///     argument.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: an expression that evaluates to <see langword="true" /> or
    ///     <see langword="false" /> 2: any result that needs to be returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ReturnValueIfInstruction)]
    public class ReturnValueIfInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnValueIfInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReturnValueIfInstruction;
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
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count > 0)
            {
                bool iRes;
                if (CalculateBool(handler, args[0], out iRes) == false)
                {
                    LogService.Log.LogError("ReturnValueIf", "first argument should be a bool");
                }
                else if (iRes)
                {
                    var iArgs = handler.Mem.ArgumentStack.Push();

                        // has to be on the stack, so that it can store the result of the expressions.
                    if (iArgs.Capacity < args.Count)
                    {
                        // reserve enough space, when -1, we don't know the required space, so take default of 10.
                        iArgs.Capacity = args.Count;
                    }

                    try
                    {
                        for (var i = 1; i < args.Count; i++)
                        {
                            var iExp = args[i] as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(handler);

                                    // the expression uses the processor.ArgumentlistStack to put it's result on.
                            }
                            else
                            {
                                iArgs.Add(args[i]);
                            }
                        }

                        handler.Mem.LastReturnValues.Push(iArgs);
                    }
                    finally
                    {
                        handler.Mem.ArgumentStack.Release(); // we release the list cause this became the result
                    }

                    handler.ExitFramesUntillReturn();
                }
            }
            else
            {
                LogService.Log.LogError("ReturnValueIf instruction", "No arguments specified");
            }

            return true;
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
            if (args.Count > 0)
            {
                if (args[0] == Brain.Current.TrueNeuron)
                {
                    processor.ExitFramesUntillReturn();
                    var iValues = Factories.Default.NLists.GetBuffer();
                    for (var i = 1; i < args.Count; i++)
                    {
                        iValues.Add(args[i]);
                    }

                    processor.Mem.LastReturnValues.Push(iValues);
                }
            }
            else
            {
                LogService.Log.LogError("ReturnValueIf instruction", "No arguments specified");
            }
        }
    }
}