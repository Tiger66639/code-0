// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MinusIntInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the - of all the arguments, which must either be an
//   <see langword="int" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the - of all the arguments, which must either be an
    ///     <see langword="int" />
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
    [NeuronID((ulong)PredefinedNeurons.MinusIntInstruction)]
    public class MinusIntInstruction : SingleResultInstruction, ICalculateInt, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusIntInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.MinusIntInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return ((IntNeuron)list[0]).Value - ((IntNeuron)list[1]).Value;
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
                var iVal = ((IntNeuron)list[0]).Value - ((IntNeuron)list[1]).Value;
                var iRes = NeuronFactory.GetInt(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            LogService.Log.LogError("MinusInt.GetValues", "Invalid nr of arguments specified");
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
                var iRes = GetInt(handler, args);
                var iN = NeuronFactory.GetInt(iRes);
                Brain.Current.MakeTemp(iN);
                handler.Mem.ArgumentStack.Peek().Add(iN); // needs to be added to the result list
                return true;
            }

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

        /// <summary>The can get int.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            return args.Count == 2; // need 2 args
        }

        /// <summary>The get int.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            int iFirst;
            int iSecond;
            if (CalculateInt(handler, args[0], out iFirst) == false)
            {
                var iLeft = SolveSingleResultExp(args[0], handler);
                var iLeftVal = GetAsInt(iLeft);
                if (iLeftVal.HasValue)
                {
                    iFirst = iLeftVal.Value;
                }
                else
                {
                    LogService.Log.LogError("MinusInt.GetValues", "Invalid nr of arguments specified");
                    return 0;
                }
            }

            if (CalculateInt(handler, args[1], out iSecond))
            {
                return iFirst - iSecond;
            }

            var iRight = SolveSingleResultExp(args[1], handler);
            var iRightVal = GetAsInt(iRight);
            if (iRightVal.HasValue)
            {
                return iFirst - iRightVal.Value;
            }

            LogService.Log.LogError("MinusInt.GetValues", "second value should be an int.");
            return 0;
        }

        /// <summary>The can get double.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>The get double.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="double"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}