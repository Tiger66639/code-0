// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Adds all the arguments except the first argument, to the first argument,
//   which must the a variable. arg 1: a variable (usually supplied with a
//   ByRef 2+: the items that need to be added to the var.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Adds all the arguments except the first argument, to the first argument,
    ///     which must the a variable. arg 1: a variable (usually supplied with a
    ///     ByRef 2+: the items that need to be added to the var.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.AddInstruction)]
    public class AddInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AddInstruction;
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

        /// <summary>The perform.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                var iVar = args[0] as Variable;
                if (iVar == null)
                {
                    iVar = SolveSingleResultExp(args[0], handler) as Variable;
                }

                if (iVar != null)
                {
                    var iList = iVar.ExtractValue(handler);
                    if (iList == null)
                    {
                        // could be that the var didn't have any values yet, check and create if needed.
                        iList = Factories.Default.NLists.GetBuffer();
                        iVar.StoreValue(iList, handler);
                    }

                    for (var i = 1; i < args.Count; i++)
                    {
                        if (args[i] is Variable)
                        {
                            // regular var: don't use the arg stack, but get it directly, which is faster, less mem operations.
                            iList.AddRange(((Variable)args[i]).ExtractValue(handler));
                        }
                        else if (args[i] is ResultExpression)
                        {
                            // it's a calculation, so do this.
                            iList.AddRange(SolveResultExpNoStackChange(args[i], handler));
                            handler.Mem.ArgumentStack.Pop();

                                // need to remove from the arg stack, we left it on there, so it couldn't have been recycled while collecting the values.
                        }
                        else
                        {
                            iList.Add(args[i]);
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "AddInstruction.Execute", 
                        string.Format("Can't add, variable expected, found: {0}.", args[0]));
                    return true;
                }
            }
            else
            {
                if (args.Count == 1 && Settings.LogAddChildInvalidArgs == false)
                {
                    // don't logg an error if there were no items to add and the logging for this has been turned off.
                    return true;
                }

                LogService.Log.LogError("AddInstruction.Execute", "Invalid nr of arguments specified");
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
            if (args != null && args.Count >= 2)
            {
                var iVar = args[0] as Variable;
                if (iVar == null)
                {
                    iVar = SolveSingleResultExp(args[0], processor) as Variable;
                }

                if (iVar != null)
                {
                    args.RemoveAt(0);

                        // remove the variable from the list, so we can use the full list as input to add to the var.
                    var iList = iVar.ExtractValue(processor);
                    if (iList == null)
                    {
                        // could be that the var didn't have any values yet, check and create if needed.
                        iList = Factories.Default.NLists.GetBuffer();
                        iVar.StoreValue(iList, processor);
                    }

                    iList.AddRange(args);
                }
                else
                {
                    LogService.Log.LogError(
                        "AddInstruction.Execute", 
                        string.Format("Can't add, variable expected, found: {0}.", args[0]));
                }
            }
            else
            {
                if (args.Count == 1 && Settings.LogAddChildInvalidArgs == false)
                {
                    // don't logg an error if there were no items to add and the logging for this has been turned off.
                    return;
                }

                LogService.Log.LogError("AddInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}