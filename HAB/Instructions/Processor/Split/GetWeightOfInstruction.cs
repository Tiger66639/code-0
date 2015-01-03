// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetWeightOfInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   gets the weight of a single neuron in the context of the current
//   processor by the specified amount.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     gets the weight of a single neuron in the context of the current
    ///     processor by the specified amount.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: the neuron to change the weight of.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetWeightOfInstruction)]
    public class GetWeightOfInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetWeightOfInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetWeightOfInstruction;
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
                return 1;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iVal = processor.SplitValues.GetWeightOf(list[0]);
                var iN = NeuronFactory.GetDouble(iVal);
                Brain.Current.MakeTemp(iN);
                return iN;
            }

            LogService.Log.LogError("GetWeightOf.Execute", "No arguments specified");
            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 1)
            {
                var iVal = handler.SplitValues.GetWeightOf(SolveSingleResultExp(args[0], handler));
                var iN = NeuronFactory.GetDouble(iVal);
                Brain.Current.MakeTemp(iN);
                handler.Mem.ArgumentStack.Peek().Add(iN);
            }
            else
            {
                LogService.Log.LogError("GetWeightOf.Execute", "No arguments specified");
            }

            return true;
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return true;
        }

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 1)
            {
                return handler.SplitValues.GetWeightOf(SolveSingleResultExp(args[0], handler));
            }

            LogService.Log.LogError("GetWeightOf.Execute", "No arguments specified");
            return 0;
        }

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

        /// <summary>The get int.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="int"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}