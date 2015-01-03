// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiplyInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the multiplication of all the arguments, which must either an
//   <see langword="int" /> or a double. If the first is int, an
//   <see langword="int" /> is returned, otherwise, if it is a double, the
//   result is also a double.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the multiplication of all the arguments, which must either an
    ///     <see langword="int" /> or a double. If the first is int, an
    ///     <see langword="int" /> is returned, otherwise, if it is a double, the
    ///     result is also a double.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>the first argument, either or</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 the second arguement, either or double, the same as argument 1.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.MultiplyInstruction)]
    public class MultiplyInstruction : SingleResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.MultiplyInstruction;
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
            if (list != null && list.Count >= 1)
            {
                var iRes = TryInt(list);
                if (iRes == null)
                {
                    iRes = TryDouble(list);
                }

                if (iRes != null)
                {
                    return iRes;
                }

                LogService.Log.LogError(
                    "MultiplyInstruction.GetValues", 
                    "All arguments must be a value type, either IntNeuron or DoubleNeuron!");
            }
            else
            {
                LogService.Log.LogError("MultiplyInstruction.GetValues", "Invalid nr of arguments specified");
            }

            return null;
        }

        /// <summary>The try double.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryDouble(System.Collections.Generic.IList<Neuron> list)
        {
            var iFirst = list[0] as DoubleNeuron;
            if (iFirst != null)
            {
                var iVal = DoDoubleMulForRest(list, iFirst.Value);

                var iRes = NeuronFactory.GetDouble(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            return null;
        }

        /// <summary>The do double mul for rest.</summary>
        /// <param name="list">The list.</param>
        /// <param name="iVal">The i val.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private double DoDoubleMulForRest(System.Collections.Generic.IList<Neuron> list, double iVal)
        {
            for (var i = 1; i < list.Count; i++)
            {
                var iSecond = list[i] as DoubleNeuron;
                if (iSecond != null)
                {
                    iVal *= iSecond.Value;
                }
                else
                {
                    var iIntSec = list[i] as IntNeuron;
                    if (iIntSec != null)
                    {
                        iVal *= iIntSec.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MultiplyInstruction.GetValues", 
                            "All arguments must be a value type, either IntNeuron or DoubleNeuron!");
                    }
                }
            }

            return iVal;
        }

        /// <summary>The try int.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron TryInt(System.Collections.Generic.IList<Neuron> list)
        {
            var iFirst = list[0] as IntNeuron;
            if (iFirst != null)
            {
                var iVal = DoIntMulForRest(list, iFirst.Value);
                var iRes = NeuronFactory.GetInt(iVal);
                Brain.Current.MakeTemp(iRes);
                return iRes;
            }

            return null;
        }

        /// <summary>The do int mul for rest.</summary>
        /// <param name="list">The list.</param>
        /// <param name="iVal">The i val.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int DoIntMulForRest(System.Collections.Generic.IList<Neuron> list, int iVal)
        {
            for (var i = 1; i < list.Count; i++)
            {
                var iSecond = list[i] as IntNeuron;
                if (iSecond != null)
                {
                    iVal *= iSecond.Value;
                }
                else
                {
                    var iIntSec = list[i] as DoubleNeuron;
                    if (iIntSec != null)
                    {
                        iVal *= (int)iIntSec.Value;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MultiplyInstruction.GetValues", 
                            "All arguments must be a value type, either IntNeuron or DoubleNeuron!");
                    }
                }
            }

            return iVal;
        }

        #region IExecResultStatement Members

        /// <summary>Gets the value.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                int? iInt = null;
                double? iDouble = null;
                var iArgs = handler.Mem.ArgumentStack.Push();

                    // has to be on the stack cause ResultExpressions will calculate their value in this.
                if (iArgs.Capacity < 10)
                {
                    iArgs.Capacity = 10; // reserve a little space for speed improvement
                }

                try
                {
                    var iExp = list[0] as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(handler);
                    }
                    else if (list[0] is IntNeuron)
                    {
                        iArgs.Add((IntNeuron)list[0]);
                    }
                    else
                    {
                        iArgs.Add(list[0] as DoubleNeuron);
                    }

                    if (iArgs.Count > 0)
                    {
                        if (iArgs[0] is IntNeuron)
                        {
                            iInt = GetAsInt(iArgs[0]);
                            iInt = DoIntMulForRest(iArgs, iInt.Value);

                                // in case the first arg rendered multiple neurons.
                        }
                        else
                        {
                            iDouble = GetAsDouble(iArgs[0]);
                            if (iDouble.HasValue)
                            {
                                iDouble = DoDoubleMulForRest(iArgs, iDouble.Value);

                                    // in case the first arg rendered multiple neurons.
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "MultiplyInstruction.GetValues", 
                                    "Argument must be int or double neurons!");
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("MultiplyInstruction.GetValues", "No values found.");
                    }
                }
                finally
                {
                    handler.Mem.ArgumentStack.Pop();
                }

                if (iInt.HasValue)
                {
                    CalcInt(handler, list, iInt.Value);
                }
                else if (iDouble.HasValue)
                {
                    CalcDouble(handler, list, iDouble.Value);
                }
            }
            else
            {
                LogService.Log.LogError("MultiplyInstruction", "Invalid nr of arguments specified");
            }

            return true;
        }

        /// <summary>Trycalcs the double.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="list">The list.</param>
        /// <param name="iRes">The i Res.</param>
        private void CalcDouble(Processor handler, System.Collections.Generic.IList<Neuron> list, double iRes)
        {
            int iInt;
            double iDouble;
            for (var i = 1; i < list.Count; i++)
            {
                if (CalculateDouble(handler, list[i], out iDouble))
                {
                    iRes *= iDouble;
                }
                else if (CalculateInt(handler, list[i], out iInt))
                {
                    iRes *= iInt;
                }
                else
                {
                    var iExp = list[i] as ResultExpression;
                    if (iExp != null)
                    {
                        iRes = MulDoubleValuesForExp(handler, iExp, iRes);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MultiplyInstruction", 
                            "invalid argument: int or double expeced, found: " + list[i].TypeOfNeuron);
                    }
                }
            }

            var iN = NeuronFactory.GetDouble(iRes);
            Brain.Current.MakeTemp(iN);
            handler.Mem.ArgumentStack.Peek().Add(iN); // needs to be added to the result list
        }

        /// <summary>The mul double values for exp.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="iRes">The i res.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private double MulDoubleValuesForExp(Processor handler, ResultExpression exp, double iRes)
        {
            var iArgs = handler.Mem.ArgumentStack.Push();

                // has to be on the stack cause ResultExpressions will calculate their value in this.
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                exp.GetValue(handler);
                foreach (var iNeuron in iArgs)
                {
                    var iSecond = GetAsDouble(iNeuron);
                    if (iSecond.HasValue)
                    {
                        iRes *= iSecond.Value;
                    }
                    else
                    {
                        LogService.Log.LogError("MultiplyInstruction", "Argument must be int or double neurons!");
                    }
                }

                return iRes;
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }
        }

        /// <summary>Tries the calc int.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="list">The list.</param>
        /// <param name="iRes">The i Res.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CalcInt(Processor handler, System.Collections.Generic.IList<Neuron> list, int iRes)
        {
            int iInt;
            double iDouble;
            for (var i = 1; i < list.Count; i++)
            {
                if (CalculateInt(handler, list[i], out iInt))
                {
                    iRes *= iInt;
                }
                else if (CalculateDouble(handler, list[i], out iDouble))
                {
                    iRes *= (int)iDouble;
                }
                else
                {
                    var iExp = list[i] as ResultExpression;
                    if (iExp != null)
                    {
                        iRes = MulIntValuesForExp(handler, iExp, iRes);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MultiplyInstruction", 
                            "invalid argument: int or double expeced, found: " + list[i].TypeOfNeuron);
                    }
                }
            }

            var iN = NeuronFactory.GetInt(iRes);
            Brain.Current.MakeTemp(iN);
            handler.Mem.ArgumentStack.Peek().Add(iN); // needs to be added to the result list
            return true;
        }

        /// <summary>The mul int values for exp.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="iRes">The i res.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int MulIntValuesForExp(Processor handler, ResultExpression exp, int iRes)
        {
            var iArgs = handler.Mem.ArgumentStack.Push();

                // has to be on the stack cause ResultExpressions will calculate their value in this.
            if (iArgs.Capacity < 10)
            {
                iArgs.Capacity = 10; // reserve a little space for speed improvement
            }

            try
            {
                exp.GetValue(handler);
                foreach (var iNeuron in iArgs)
                {
                    var iSecond = GetAsInt(iNeuron);
                    if (iSecond.HasValue)
                    {
                        iRes *= iSecond.Value;
                    }
                    else
                    {
                        LogService.Log.LogError("MultiplyInstruction", "Argument must be int or double neurons!");
                    }
                }

                return iRes;
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }
        }

        /// <summary>checks if the value can be returned as a bool.</summary>
        /// <param name="args"></param>
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

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            var iRes = false;
            if (args.Count >= 1)
            {
                var iExp = args[0] as ResultExpression;
                if (iExp != null)
                {
                    iRes = iExp.CanGetInt();
                }
                else
                {
                    iRes = args[0] is IntNeuron;
                }
            }

            return iRes;
        }

        /// <summary>The get int.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            int iRes;
            int iInt;
            double iDouble;
            if (CalculateInt(handler, args[0], out iRes))
            {
                for (var i = 1; i < args.Count; i++)
                {
                    if (CalculateInt(handler, args[i], out iInt))
                    {
                        iRes *= iInt;
                    }
                    else if (CalculateDouble(handler, args[i], out iDouble))
                    {
                        iRes *= (int)iDouble;
                    }
                    else
                    {
                        var iExp = args[i] as ResultExpression;
                        if (iExp != null)
                        {
                            iRes = MulIntValuesForExp(handler, iExp, iRes);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "MultiplyInstruction", 
                                "invalid argument: int or double expeced, found: " + args[i].TypeOfNeuron);
                        }
                    }
                }

                return iRes;
            }

            LogService.Log.LogError("MultiplyInstruction", "invalid arguments: int or doubles expected");
            return 0;
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            var iRes = false;
            if (args.Count >= 1)
            {
                var iExp = args[0] as ResultExpression;
                if (iExp != null)
                {
                    iRes = iExp.CanGetDouble();
                }
                else
                {
                    iRes = args[0] is DoubleNeuron;
                }
            }

            return iRes;
        }

        /// <summary>The get double.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            double iRes;
            int iInt;
            double iDouble;
            if (CalculateDouble(handler, args[0], out iRes))
            {
                for (var i = 1; i < args.Count; i++)
                {
                    if (CalculateDouble(handler, args[i], out iDouble))
                    {
                        iRes *= iDouble;
                    }
                    else if (CalculateInt(handler, args[i], out iInt))
                    {
                        iRes *= iInt;
                    }
                    else
                    {
                        var iExp = args[i] as ResultExpression;
                        if (iExp != null)
                        {
                            iRes = MulDoubleValuesForExp(handler, iExp, iRes);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "MultiplyInstruction", 
                                "invalid argument: int or double expeced, found: " + args[i].TypeOfNeuron);
                        }
                    }
                }

                return iRes;
            }

            LogService.Log.LogError("MultiplyInstruction", "invalid arguments: int or doubles expected");
            return 0;
        }

        #endregion
    }
}