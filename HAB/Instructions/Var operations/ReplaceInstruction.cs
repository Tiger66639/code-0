// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReplaceInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Replaces the second arg with the third arg in the list of the first arg.
//   arg 1: a variable 2: the item to replace 3: the item to replace the
//   previous with.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Replaces the second arg with the third arg in the list of the first arg.
    ///     arg 1: a variable 2: the item to replace 3: the item to replace the
    ///     previous with.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.ReplaceInstruction)]
    public class ReplaceInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReplaceInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReplaceInstruction;
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
                    Neuron iOld, iNew;
                    if (args.Count == 3)
                    {
                        iOld = SolveSingleResultExp(args[1], handler);
                        iNew = SolveSingleResultExp(args[1], handler);
                    }
                    else
                    {
                        var iTemp = SolveResultExp(args[1], handler);
                        if (iTemp.Count >= 2)
                        {
                            iOld = iTemp[0];
                            iNew = iTemp[1];
                        }
                        else
                        {
                            LogService.Log.LogError("Replace.Execute", "Invalid nr of arguments specified");
                            return true;
                        }
                    }

                    var iList = iVar.ExtractValue(handler);
                    if (iList == null)
                    {
                        // could be that the var didn't have any values yet, check and create if needed.
                        iList = Factories.Default.NLists.GetBuffer();
                        iVar.StoreValue(iList, handler);
                    }

                    for (var i = 0; i < iList.Count; i++)
                    {
                        if (iList[i] == iOld)
                        {
                            iList[i] = iNew;
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "Replace.Execute", 
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

                LogService.Log.LogError("Replace.Execute", "Invalid nr of arguments specified");
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
            if (args != null && args.Count >= 3)
            {
                var iVar = args[0] as Variable;
                if (iVar != null)
                {
                    var iList = iVar.ExtractValue(processor);
                    if (iList == null)
                    {
                        // could be that the var didn't have any values yet, check and create if needed.
                        iList = Factories.Default.NLists.GetBuffer();
                        iVar.StoreValue(iList, processor);
                    }

                    for (var i = 0; i < iList.Count; i++)
                    {
                        if (iList[i] == args[1])
                        {
                            iList[i] = args[2];
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "Replace.Execute", 
                        string.Format("Can't replace, variable expected, found: {0}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("Replace.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}