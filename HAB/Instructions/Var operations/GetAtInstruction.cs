// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the item at the specified index in the argument list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the item at the specified index in the argument list.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.GetAtInstruction)]
    public class GetAtInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetAtInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iInt = GetAsInt(list[0]);
                if (iInt.HasValue)
                {
                    if (iInt.Value >= 0 && iInt.Value + 1 < list.Count)
                    {
                        return list[iInt.Value + 1];

                            // we add 1 cause the first was the index int. only after that comes the list into which we need to digg.
                    }

                    LogService.Log.LogError("GetAtInstruction.InternalGetValue", "Index out of range.");
                }
                else
                {
                    LogService.Log.LogError("GetAtInstruction.InternalGetValue", "First argument should be an Int.");
                }
            }
            else
            {
                LogService.Log.LogError("GetAtInstruction.InternalGetValue", "Invalid nr of arguments.");
            }

            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.
        ///     small speed up:the first arg is a number (int or double), the rest
        ///     produces a list of items to retiieve the result from.</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            var iStackPushed = false;
            int iIndex;
            Neuron iRes = null;
            System.Collections.Generic.List<Neuron> iArgs = null;
            if (args.Count > 1)
            {
                try
                {
                    if (CalculateInt(handler, args[0], out iIndex) == false)
                    {
                        iStackPushed = true;
                        iArgs = handler.Mem.ArgumentStack.Push();

                            // has to be on the stack cause ResultExpressions will calculate their value in this.
                        GetExpResult(handler, args[0], iArgs);
                        var iIntRes = GetAsInt(iArgs[0]);
                        if (iIntRes.HasValue == false)
                        {
                            LogService.Log.LogError(
                                "GetAtInstruction.InternalGetValue", 
                                "First argument should be an Int.");
                            return true;
                        }
                        else
                        {
                            iIndex = iIntRes.Value;
                        }
                    }

                    if (args.Count == 2 && args[1] is Variable)
                    {
                        // small improvement: if the arg is a var-> faster calculation.
                        iArgs = ((Variable)args[1]).ExtractValue(handler);
                        System.Diagnostics.Debug.Assert(iArgs != null);
                        if (iArgs.Count > iIndex)
                        {
                            iRes = iArgs[iIndex];
                        }
                    }
                    else
                    {
                        if (iStackPushed == false)
                        {
                            iArgs = handler.Mem.ArgumentStack.Push();

                                // has to be on the stack cause ResultExpressions will calculate their value in this.
                        }
                        else
                        {
                            iArgs.Clear();

                                // we clear the result list cause we might be adding some extra items to it, don't want to have the index value interfere with it.
                        }

                        for (var i = 1; i < args.Count; i++)
                        {
                            GetExpResult(handler, args[i], iArgs);
                            if (iArgs.Count > iIndex)
                            {
                                iRes = iArgs[i];
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if (iStackPushed)
                    {
                        handler.Mem.ArgumentStack.Pop();
                    }
                }

                if (iRes != null)
                {
                    handler.Mem.ArgumentStack.Peek().Add(iRes); // needs to be added to the result list
                }
                else
                {
                    LogService.Log.LogError("GetAtInstruction.InternalGetValue", "Index out of range.");
                }

                return true;
            }

            return false; // let the system try to first calculate the arguments, see if we get an int then.
        }

        /// <summary>The can get bool.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetBool(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as a <see langword="bool"/> value (argumnets still
        ///     need to be calculated).</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetBool(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as an <see langword="int"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}