// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveInfoAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes the neuron at the specified index from an info list on a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes the neuron at the specified index from an info list on a link.
    /// </summary>
    /// <remarks>
    ///     Args: 1: The from part of the link 2: the to part of the link 3: the
    ///     meaning of the link 4: the index of the item to remove.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.RemoveInfoAtInstruction)]
    public class RemoveInfoAtInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveInfoAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.RemoveInfoAtInstruction;
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
                return 4;
            }
        }

        #region IExecStatement Members

        /// <summary>The perform.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count >= 2)
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
                    LoadArgsUntil(processor, args, iArgs, 3, ref i);
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

                            if (iArgs.Count >= 1)
                            {
                                var iconv = GetAsInt(iArgs[3]);
                                if (iconv.HasValue)
                                {
                                    iHasInt = true;
                                    iInt = iconv.Value;
                                }
                            }
                        }

                        if (iHasInt)
                        {
                            RemoveInfoAt(iArgs, iInt);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "RemoveInfoAtInstruction.Execute", 
                                string.Format("Can't remove, Invalid index specified: {0}.", args[i]));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "Invalid nr of arguments specified");
                    }

                    return true;
                }
                finally
                {
                    processor.Mem.ArgumentStack.Pop();
                }
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
            if (CheckArgs(args) == false)
            {
                return;
            }

            var iIndex = args[3] as IntNeuron;
            RemoveInfoAt(args, iIndex.Value);
        }

        /// <summary>The remove info at.</summary>
        /// <param name="args">The args.</param>
        /// <param name="index">The index.</param>
        private void RemoveInfoAt(System.Collections.Generic.IList<Neuron> args, int index)
        {
            var iLock = BuildInfoLock(args, true);
            LockManager.Current.RequestLocks(iLock);
            Neuron iChanged = null;
            try
            {
                var iLink = FindLinkUnsafe(args[0], args[1], args[2]);
                if (iLink != null)
                {
                    if (iLink.InfoIdentifier != null)
                    {
                        if (index >= 0 && index < iLink.InfoDirect.Count)
                        {
                            iChanged = Brain.Current[iLink.InfoDirect[index]];
                            iLink.InfoDirect.RemoveAtUnsafe(index);
                        }
                        else
                        {
                            LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "Index out of range.");
                        }
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "RemoveInfoAtInstruction.Execute", 
                        string.Format(
                            "Could not find link from={0}, to={1}, meaning={3}.  Failed to remove info at index={2}", 
                            args[0], 
                            args[1], 
                            args[3], 
                            args[2]));
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
            }

            if (iChanged != null)
            {
                args[0].IsChanged = true;
                args[1].IsChanged = true;
                iChanged.IsChanged = true;
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count < 4)
            {
                LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "Invalid nr of arguments specified");
                return false;
            }

            if (CheckLinkPart(args) == false)
            {
                return false;
            }

            if (args[3] == null)
            {
                LogService.Log.LogError(
                    "RemoveInfoAtInstruction.Execute", 
                    "Index of item to remove is null  (fourth arg).");
                return false;
            }

            if (!(args[3] is IntNeuron))
            {
                LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "Invalid index (fourth arg).");
                return false;
            }

            return true;
        }

        /// <summary>The check link part.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckLinkPart(System.Collections.Generic.IList<Neuron> args)
        {
            if (args[0] == null)
            {
                LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "From part is null (first arg).");
                return false;
            }

            if (args[1] == null)
            {
                LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "To part is null (second arg).");
                return false;
            }

            if (args[2] == null)
            {
                LogService.Log.LogError("RemoveInfoAtInstruction.Execute", "Meaning pat is null (third arg).");
                return false;
            }

            return true;
        }
    }
}