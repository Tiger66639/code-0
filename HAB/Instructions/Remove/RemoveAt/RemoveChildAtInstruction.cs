// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveChildAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes the neuron at the specified index from a cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes the neuron at the specified index from a cluster.
    /// </summary>
    /// <remarks>
    ///     Args: 1: The neuron cluster 2: the index of the item to remove.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveChildAtInstruction)]
    public class RemoveChildAtInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveChildAtInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveChildAtInstruction;
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
            if (args != null && args.Count >= 2)
            {
                var iArgs = processor.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    NeuronCluster iCluster = null;
                    var iExp = args[0] as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(processor);
                        if (iArgs.Count > 0)
                        {
                            iCluster = iArgs[0] as NeuronCluster;
                        }
                    }
                    else
                    {
                        iCluster = args[0] as NeuronCluster;
                    }

                    if (iCluster != null)
                    {
                        var iHasInt = true;
                        int iInt;
                        if (CalculateInt(processor, args[1], out iInt) == false)
                        {
                            // when can't do direct convert, try to get the value of the xpression (already know it isn't an int or double neuron, cause that would be caught by CalculateInt)
                            iHasInt = false;
                            iExp = args[1] as ResultExpression;
                            if (iExp != null)
                            {
                                iExp.GetValue(processor);
                            }

                            if (iArgs.Count >= 1)
                            {
                                var iconv = GetAsInt(iArgs[1]);
                                if (iconv.HasValue)
                                {
                                    iHasInt = true;
                                    iInt = iconv.Value;
                                }
                            }
                        }

                        if (iHasInt)
                        {
                            if (iInt >= 0)
                            {
                                // we don't check for upper boundery, but leaf that up to the removeAt: cause it has to happen inside the lock.
                                using (var iChildren = iCluster.ChildrenW) iChildren.RemoveAt(iInt);
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "RemoveChildAtInstruction.Execute", 
                                    string.Format("Index out of range: {0}.", args[1]));
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "RemoveChildAtInstruction.Execute", 
                                string.Format("Can't remove, Invalid index specified: {0}.", args[1]));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "RemoveChildAtInstruction.Execute", 
                            string.Format("Can't remove, Invalid cluster specified: {0}.", args[0]));
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
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                if (args[1] == null)
                {
                    LogService.Log.LogError(
                        "RemoveChildAtInstruction.Execute", 
                        "Index should be specified (second arg).");
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                var iIndex = args[1] as IntNeuron;
                if (iCluster != null)
                {
                    if (iIndex != null)
                    {
                        if (iIndex.Value >= 0)
                        {
                            // we don't check for upper boundery, but leaf that up to the removeAt: cause it has to happen inside the lock.
                            using (var iChildren = iCluster.ChildrenW) iChildren.RemoveAt(iIndex.Value);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "RemoveChildAtInstruction.Execute", 
                                string.Format("Index out of range: {0}.", args[1]));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "RemoveChildAtInstruction.Execute", 
                            string.Format("Can't remove, Invalid index specified: {0}.", args[1]));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "RemoveChildAtInstruction.Execute", 
                        string.Format("Can't remove, Invalid cluster specified: {0}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("RemoveChildAtInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}