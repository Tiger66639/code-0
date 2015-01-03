// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvertDoubleInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the inverse (-) of the argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Instructions.unary
{
    /// <summary>
    ///     Gets the inverse (-) of the argument.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.InvertDoubleInstruction)]
    public class InvertDoubleInstruction : SingleResultInstruction, ICalculateDouble, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertDoubleInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InvertDoubleInstruction;
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

        #region ICalculateDouble Members

        /// <summary>Calculate the <see langword="double"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return -((DoubleNeuron)list[0]).Value;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iRes = NeuronFactory.GetDouble(-((DoubleNeuron)list[0]).Value);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            LogService.Log.LogError("InvertDouble", "double expected");
            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            return false;
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
            return args.Count >= 1 && args[0].CanGetDouble();
        }

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            double ires;
            if (CalculateDouble(handler, args[0], out ires))
            {
                return -ires;
            }

            LogService.Log.LogError("InvertDouble", "double expected");
            return 0;
        }

        #endregion
    }
}