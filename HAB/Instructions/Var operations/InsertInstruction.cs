// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Adds all the arguments except the first argument, to the first argument,
//   which must the a variable. arg 1: a variable 2: the index to start
//   inserting from. the first item will be at this position, the second item
//   to insert will be after it. 2+: the items that need to be added to the
//   var at the specified pos.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Adds all the arguments except the first argument, to the first argument,
    ///     which must the a variable. arg 1: a variable 2: the index to start
    ///     inserting from. the first item will be at this position, the second item
    ///     to insert will be after it. 2+: the items that need to be added to the
    ///     var at the specified pos.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.InsertInstruction)]
    public class InsertInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InsertInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InsertInstruction;
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
                    int iInt;

                    if (CalculateInt(handler, args[1], out iInt))
                    {
                        var iList = iVar.ExtractValue(handler);
                        if (iList == null)
                        {
                            // could be that the var didn't have any values yet, check and create if needed.
                            iList = Factories.Default.NLists.GetBuffer();
                            iVar.StoreValue(iList, handler);
                        }

                        for (var i = 2; i < args.Count; i++)
                        {
                            if (args[i] is Variable)
                            {
                                // regular var: don't use the arg stack, but get it directly, which is faster, less mem operations.
                                var iTemp = ((Variable)args[i]).ExtractValue(handler);
                                foreach (var u in iTemp)
                                {
                                    iList.Insert(iInt++, u);
                                }
                            }
                            else if (args[i] is ResultExpression)
                            {
                                // it's a calculation, so do this.
                                var iTemp = SolveResultExpNoStackChange(args[i], handler);
                                foreach (var u in iTemp)
                                {
                                    iList.Insert(iInt++, u);
                                }

                                handler.Mem.ArgumentStack.Pop();

                                    // need to remove from the arg stack, we left it on there, so it couldn't have been recycled while collecting the values.
                            }
                            else
                            {
                                iList.Insert(iInt++, args[i]);
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "InsertInstruction.Execute", 
                            string.Format("Can't insert value, Invalid IntNeuron specified: {0}.", args[1]));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "InsertInstruction.Execute", 
                        string.Format("Can't insert, variable expected at pos 0, found: {0}.", args[0]));
                }
            }
            else
            {
                if (args.Count <= 2 && Settings.LogAddChildInvalidArgs == false)
                {
                    // don't logg an error if there were no items to add and the logging for this has been turned off.
                    return true;
                }

                LogService.Log.LogError("InsertInstruction.Execute", "Invalid nr of arguments specified");
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
            throw new System.InvalidOperationException("use perform instead");
        }
    }
}