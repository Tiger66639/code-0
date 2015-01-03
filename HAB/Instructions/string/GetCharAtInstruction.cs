// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCharAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   gets the character at the specified
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     gets the character at the specified
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.GetCharAtInstruction)]
    public class GetCharAtInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LengthInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.LengthInstruction;
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 2)
            {
                var iText = list[0] as TextNeuron;
                var iPos = GetAsInt(list[1]);
                if (iText != null)
                {
                    if (iPos.HasValue)
                    {
                        var iRes = NeuronFactory.GetInt(iText.Text[iPos.Value]);
                        Brain.Current.MakeTemp(iRes);
                        return iRes;
                    }

                    LogService.Log.LogError("GetCharAtInstruction", "second argument should be an int.");
                }
                else
                {
                    LogService.Log.LogError("GetCharAtInstruction", "first argument should be a textneuron.");
                }
            }
            else
            {
                LogService.Log.LogError("GetCharAtInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return null;
        }

        #region IExecResultStatement Members

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args">The args, not calculated.</param>
        /// <returns><c>true</c> if this instance [can get int] the specified args;
        ///     otherwise, <c>false</c> .</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            return args.Count >= 2;
        }

        /// <summary>gets the result as an <see langword="int"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            var iText = SolveSingleResultExp(args[0], handler) as TextNeuron;
            var iPos = 0;
            if (CalculateInt(handler, args[1], out iPos) == false)
            {
                var iTemp = GetAsInt(SolveSingleResultExp(args[1], handler));
                if (iTemp.HasValue)
                {
                    iPos = iTemp.Value;
                }
                else
                {
                    LogService.Log.LogError(
                        "GetCharAtInstruction.InternalGetValue", 
                        "second argument should be an int (start of substring).");
                }
            }

            var iVal = iText.Text;
            if (iVal.Length > iPos)
            {
                return iVal[iPos];
            }

            LogService.Log.LogError("GetCharAtInstruction.InternalGetValue", "Index out of range");
            return 0;
        }

        #region not used

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>The can get bool.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetBool(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>The get bool.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool GetBool(Processor handler, System.Collections.Generic.IList<Neuron> args)
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