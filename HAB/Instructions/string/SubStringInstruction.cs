// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubStringInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   returns a sub section of a string. arg: 1: the string 2: the start of the
//   substring 3: optionally the length of the substring.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     returns a sub section of a string. arg: 1: the string 2: the start of the
    ///     substring 3: optionally the length of the substring.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.SubStringInstruction)]
    public class SubStringInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SubStringInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SubStringInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 2)
            {
                var iText = list[0] as TextNeuron;
                var iStart = GetAsInt(list[1]);
                int? iLength;
                if (list.Count >= 3)
                {
                    iLength = GetAsInt(list[2]);
                }
                else
                {
                    iLength = null;
                }

                if (iText != null)
                {
                    if (iStart.HasValue)
                    {
                        var iVal = iText.Text;
                        if (iLength.HasValue)
                        {
                            return TextNeuron.GetFor(iVal.Substring(iStart.Value, iLength.Value), processor);
                        }

                        return TextNeuron.GetFor(iVal.Substring(iStart.Value), processor);
                    }

                    LogService.Log.LogError(
                        "SubStringInstruction.InternalGetValue", 
                        "second argument should be an int (start of substring).");
                }
                else
                {
                    LogService.Log.LogError(
                        "SubStringInstruction.InternalGetValue", 
                        "First argument should be a textneuron.");
                }
            }
            else
            {
                LogService.Log.LogError("SubStringInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count == 3)
            {
                var iText = SolveSingleResultExp(args[0], handler) as TextNeuron;
                var iStart = 0;
                if (CalculateInt(handler, args[1], out iStart) == false)
                {
                    var iTemp = GetAsInt(SolveSingleResultExp(args[1], handler));
                    if (iTemp.HasValue)
                    {
                        iStart = iTemp.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SubStringInstruction.InternalGetValue", 
                            "second argument should be an int (start of substring).");
                    }
                }

                var iLength = 0;
                if (CalculateInt(handler, args[2], out iLength) == false)
                {
                    var iTemp = GetAsInt(SolveSingleResultExp(args[2], handler));
                    if (iTemp.HasValue)
                    {
                        iLength = iTemp.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SubStringInstruction.InternalGetValue", 
                            "third argument should be an int (length of substring).");
                    }
                }

                var iVal = iText.Text.Substring(iStart, iLength);
                if (string.IsNullOrEmpty(iVal) == false)
                {
                    handler.Mem.ArgumentStack.Peek().Add(TextNeuron.GetFor(iVal, handler));
                }

                return true;
            }

            return false;
        }

        #region notused

        /// <summary>checks if the value can be returned as a bool.</summary>
        /// <param name="args"></param>
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
        /// <param name="args">The args, not calculated.</param>
        /// <returns><c>true</c> if this instance [can get int] the specified args;
        ///     otherwise, <c>false</c> .</returns>
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

        #endregion
    }
}