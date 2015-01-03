// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModDoubleInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the modulus of all the arguments, which must either be doubles.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the modulus of all the arguments, which must either be doubles.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>the first argument</description>
    ///         </item>
    ///         <item>
    ///             <description>the second arguement</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ModDoubleInstruction)]
    public class ModDoubleInstruction : SingleResultInstruction, ICalculateDouble, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModDoubleInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ModDoubleInstruction;
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

        #region ICalculateDouble Members

        /// <summary>Calculate the <see langword="double"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return ((DoubleNeuron)list[0]).Value % ((DoubleNeuron)list[1]).Value;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iVal = ((DoubleNeuron)list[0]).Value % ((DoubleNeuron)list[1]).Value;
                var iRes = NeuronFactory.GetDouble(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            LogService.Log.LogError("ModDouble.GetValues", "Invalid nr of arguments specified");
            return null;
        }

        #region IExecResultStatement Members

        /// <summary>calculates the result and puts it in the list at the top of the stack.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>True when the operation succeeded, otherwise false.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count == 2)
            {
                double iRes;
                if (CalculateDouble(handler, args[0], out iRes)
                    || TryGetAsDouble(SolveSingleResultExp(args[0], handler), out iRes) == false)
                {
                    double iSecond;
                    if (CalculateDouble(handler, args[1], out iSecond)
                        || TryGetAsDouble(SolveSingleResultExp(args[1], handler), out iSecond))
                    {
                        iRes = iRes % iSecond;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ModDouble", 
                            "invalid second arg: int or double expeced, found: " + args[1].TypeOfNeuron);
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ModDouble", 
                        "invalid first arg: double expeced, found: " + args[0].TypeOfNeuron);
                }

                var iN = NeuronFactory.GetDouble(iRes);
                Brain.Current.MakeTemp(iN);
                handler.Mem.ArgumentStack.Peek().Add(iN); // needs to be added to the result list
                return true;
            }

            return false;
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return args.Count == 2; // need 2 args
        }

        /// <summary>gets the result as a <see langword="double"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            double iRes;
            if (CalculateDouble(handler, args[0], out iRes)
                || TryGetAsDouble(SolveSingleResultExp(args[0], handler), out iRes) == false)
            {
                double iSecond;
                if (CalculateDouble(handler, args[1], out iSecond)
                    || TryGetAsDouble(SolveSingleResultExp(args[1], handler), out iSecond))
                {
                    iRes = iRes % iSecond;
                }
                else
                {
                    LogService.Log.LogError(
                        "ModDouble", 
                        "invalid second arg: int or double expeced, found: " + args[1].TypeOfNeuron);
                }
            }
            else
            {
                LogService.Log.LogError(
                    "ModDouble", 
                    "invalid first arg: double expeced, found: " + args[0].TypeOfNeuron);
            }

            return iRes;
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