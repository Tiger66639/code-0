// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   stores the last arg at the position specified in the second arg for the
//   variable specified in the first arg.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     stores the last arg at the position specified in the second arg for the
    ///     variable specified in the first arg.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 The variable for which to change the content. This should be passed along
    ///                 through another variable or a byref expression.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 an int-neuron representing the index (0 based) of the item to replace
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>the new value.</description>
    ///         </item>
    ///     </list>
    ///     <para>warning: index must be wihtin range.</para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetAtInstruction)]
    public class SetAtInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetAtInstruction;
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

        /// <summary>Performs the specified processor.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 3)
            {
                var iArgs = processor.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    var iVar = args[0] as Variable;
                    Neuron iVal;
                    ResultExpression iExp;
                    var iArgsIndex = 0;
                    if (iVar == null)
                    {
                        iExp = args[0] as ResultExpression;
                        if (iExp != null)
                        {
                            iExp.GetValue(processor);
                        }

                        iVar = iArgs[iArgsIndex] as Variable;
                        iArgsIndex++;
                    }

                    iExp = args[2] as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(processor);
                        iVal = iArgs[iArgsIndex];
                        iArgsIndex++;
                    }
                    else
                    {
                        iVal = args[iArgsIndex];
                    }

                    int iInt;
                    if (CalculateInt(processor, args[1], out iInt))
                    {
                        if (iVar == null)
                        {
                            LogService.Log.LogError(
                                "SetAtInstruction.Execute", 
                                string.Format("Can't set value, Invalid var specified: {0}.", args[0]));
                        }
                        else
                        {
                            if (iVal == null)
                            {
                                LogService.Log.LogError(
                                    "SetAtInstruction.Execute", 
                                    string.Format("Can't set value, Invalid value specified: {0}.", args[1]));
                            }
                            else
                            {
                                var iValues = iVar.ExtractValue(processor);

                                if (iValues != null)
                                {
                                    iValues[iInt] = iVal;
                                }
                                else
                                {
                                    LogService.Log.LogError(
                                        "SetAtInstruction.Execute", 
                                        string.Format("Variable has no values: {0}.", args[0]));
                                }
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SetAtInstruction.Execute", 
                            string.Format("Can't set value, Invalid IntNeuron specified: {0}.", args[1]));
                    }

                    return true;
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }
            }

            return false;

                // can't directly resolve args, let the statement give a try, perhaps some arguments are hidden under a single var.
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
                var iPos = args[1] as IntNeuron;
                var iVal = args[2];
                if (iVar == null)
                {
                    LogService.Log.LogError(
                        "SetAtInstruction.Execute", 
                        string.Format("Can't set value, Invalid var specified: {0}.", args[0]));
                }
                else
                {
                    var iInt = GetAsInt(iPos);
                    if (iInt.HasValue == false)
                    {
                        LogService.Log.LogError(
                            "SetAtInstruction.Execute", 
                            string.Format("Can't set value, Invalid IntNeuron specified: {0}.", args[1]));
                    }
                    else
                    {
                        if (iVal == null)
                        {
                            LogService.Log.LogError(
                                "SetAtInstruction.Execute", 
                                string.Format("Can't set value, Invalid value specified: {0}.", args[1]));
                        }
                        else
                        {
                            var iValues = iVar.ExtractValue(processor);
                            if (iValues != null)
                            {
                                iValues[iInt.Value] = iVal;
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "SetAtInstruction.Execute", 
                                    string.Format("Variable has no values: {0}.", args[0]));
                            }
                        }
                    }
                }
            }
            else
            {
                LogService.Log.LogError("SetAtInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}