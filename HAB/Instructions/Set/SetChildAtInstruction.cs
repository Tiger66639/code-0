// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetChildAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Changes the child of a cluster to a new value at a specific index.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Instructions.Set
{
    /// <summary>
    ///     Changes the child of a cluster to a new value at a specific index.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The who's meaning needs to be changed.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 2: an that specifies the index position of the value that needs to be
    ///                 changed.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>3: the new value.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetChildAtInstruction)]
    public class SetChildAtInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetChildAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetChildAtInstruction;
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
                var iCl = args[0] as NeuronCluster;
                if (iCl == null && args[0] is ResultExpression)
                {
                    iCl = SolveSingleResultExp(args[0], processor) as NeuronCluster;
                }

                var iVal = SolveSingleResultExp(args[2], processor);

                int iInt;
                if (CalculateInt(processor, args[1], out iInt))
                {
                    if (iCl == null)
                    {
                        LogService.Log.LogError(
                            "SetChildAtInstruction.Execute", 
                            string.Format("Can't set value, Invalid var specified: {0}.", args[0]));
                    }
                    else
                    {
                        if (iVal == null)
                        {
                            LogService.Log.LogError(
                                "SetChildAtInstruction.Execute", 
                                string.Format("Can't set value, Invalid value specified: {0}.", args[1]));
                        }
                        else
                        {
                            if (iVal.ID == TempId)
                            {
                                Brain.Current.Add(iVal);
                            }

                            using (IDListAccessor iChildren = iCl.ChildrenW)
                            {
                                iChildren.Lock(iVal);
                                try
                                {
                                    if (iChildren.CountUnsafe > iInt && iInt >= 0)
                                    {
                                        iChildren.SetUnsafe(iInt, iVal.ID);
                                    }
                                    else
                                    {
                                        LogService.Log.LogError(
                                            "SetChildAtInstruction.Execute", 
                                            "index out of bounds: " + iInt);
                                    }
                                }
                                finally
                                {
                                    iChildren.Unlock(iVal);
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "SetChildAtInstruction.Execute", 
                        string.Format("Can't set value, Invalid IntNeuron specified: {0}.", args[1]));
                }

                return true;
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
                var iCl = args[0] as NeuronCluster;
                var iPos = args[1] as IntNeuron;
                var iVal = args[2];
                if (iCl == null)
                {
                    LogService.Log.LogError(
                        "SetChildAtInstruction.Execute", 
                        string.Format("Can't set value, Invalid var specified: {0}.", args[0]));
                }
                else
                {
                    var iInt = GetAsInt(iPos);
                    if (iInt.HasValue == false)
                    {
                        LogService.Log.LogError(
                            "SetChildAtInstruction.Execute", 
                            string.Format("Can't set value, Invalid IntNeuron specified: {0}.", args[1]));
                    }
                    else
                    {
                        if (iVal == null)
                        {
                            LogService.Log.LogError(
                                "SetChildAtInstruction.Execute", 
                                string.Format("Can't set value, Invalid value specified: {0}.", args[1]));
                        }
                        else
                        {
                            if (iVal.ID == TempId)
                            {
                                Brain.Current.Add(iVal);
                            }

                            using (IDListAccessor iChildren = iCl.ChildrenW)
                            {
                                iChildren.Lock(iVal);
                                try
                                {
                                    if (iChildren.CountUnsafe > iInt.Value && iInt.Value >= 0)
                                    {
                                        iChildren.SetUnsafe(iInt.Value, iVal.ID);
                                    }
                                    else
                                    {
                                        LogService.Log.LogError(
                                            "SetChildAtInstruction.Execute", 
                                            "index out of bounds: " + iInt.Value);
                                    }
                                }
                                finally
                                {
                                    iChildren.Unlock(iVal);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                LogService.Log.LogError("SetChildAtInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}