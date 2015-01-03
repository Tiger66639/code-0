// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DToIInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a <see langword="double" /> neuron into an <see langword="int" />
//   neuron. It is possible to convert multiple items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a <see langword="double" /> neuron into an <see langword="int" />
    ///     neuron. It is possible to convert multiple items at once.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a list of neurons.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.DToIInstruction)]
    public class DToIInstruction : MultiResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DToIInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.DToIInstruction;
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

        /// <summary>The get values.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iList = processor.Mem.ArgumentStack.Peek();
                foreach (var iNeuron in list)
                {
                    var iPar = iNeuron as DoubleNeuron;
                    if (iPar != null)
                    {
                        var iRes = NeuronFactory.GetInt((int)iPar.Value);
                        Brain.Current.MakeTemp(iRes);
                        iList.Add(iRes);
                    }
                    else
                    {
                        LogService.Log.LogError("DToIInstruction.GetValues", "Argument must be double neurons!");
                    }
                }
            }
            else
            {
                LogService.Log.LogError("DToIInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }

        #region IExecResultStatement Members

        /// <summary>The get value.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetValue(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 1)
            {
                var iList = handler.Mem.ArgumentStack.Peek();
                double iVal;
                foreach (var iNeuron in args)
                {
                    if (CalculateDouble(handler, iNeuron, out iVal))
                    {
                        var iRes = NeuronFactory.GetInt((int)iVal);
                        Brain.Current.MakeTemp(iRes);
                        iList.Add(iRes);
                    }
                    else
                    {
                        var iExp = iNeuron as ResultExpression;
                        if (iExp != null)
                        {
                            GetValuesForExp(handler, iExp, iList);
                        }
                        else
                        {
                            LogService.Log.LogError("DToIInstruction.GetValues", "Arguments must be double neurons!");
                        }
                    }
                }
            }
            else
            {
                LogService.Log.LogError("DToIInstruction.GetValues", "Invalid nr of arguments specified!");
            }

            return true;
        }

        /// <summary>The get values for exp.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="result">The result.</param>
        private void GetValuesForExp(
            Processor handler, 
            ResultExpression exp, System.Collections.Generic.List<Neuron> result)
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
                    var iPar = iNeuron as DoubleNeuron;
                    if (iPar != null)
                    {
                        var iRes = NeuronFactory.GetInt((int)iPar.Value);
                        Brain.Current.MakeTemp(iRes);
                        result.Add(iRes);
                    }
                    else
                    {
                        LogService.Log.LogError("DToIInstruction.GetValues", "Argument must be double neurons!");
                    }
                }
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }
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

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            if (args.Count == 1)
            {
                var iStat = args[0] as ResultStatement;
                return args[0] is DoubleNeuron
                       || (iStat != null && iStat.WorkData.Instruction is SingleResultInstruction);
            }

            return false;
        }

        /// <summary>gets the result as an <see langword="int"/> (argumnets still need to
        ///     be calculated)</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            double iVal;
            if (CalculateDouble(handler, args[0], out iVal))
            {
                return (int)iVal;
            }

            LogService.Log.LogError("DToIInstruction.GetValues", "Argument must be double neurons!");
            return 0;
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