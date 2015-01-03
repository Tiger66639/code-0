// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecreaseWeightInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Decreases the weight of the processor by the specified amount. This
//   weight will be assigned to the split results of the processor when it is
//   done.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Decreases the weight of the processor by the specified amount. This
    ///     weight will be assigned to the split results of the processor when it is
    ///     done.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 A that defines how much the weight should be changed.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.DecreaseWeightInstruction)]
    public class DecreaseWeightInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DecreaseWeightInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.DecreaseWeightInstruction;
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
            if (args != null && args.Count >= 1)
            {
                double iVal;
                if (GetVal(args[0], out iVal))
                {
                    SetValue(processor, iVal);
                }
            }
            else
            {
                LogService.Log.LogError("DecreaseWeightInstruction.Execute", "No arguments specified");
            }
        }

        /// <summary>The get val.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="result">The result.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetVal(Neuron neuron, out double result)
        {
            var iInt = neuron as IntNeuron;
            if (iInt != null)
            {
                result = iInt.Value;
                return true;
            }

            var iDouble = neuron as DoubleNeuron;
            if (iDouble != null)
            {
                result = iDouble.Value;
                return true;
            }

            LogService.Log.LogError("DecreaseWeightInstruction.Execute", "IntNeuron or DoubleNeuron expected as arg.");
            result = double.MinValue;
            return false;
        }

        #region IExecStatement Members

        /// <summary>The perform.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 1)
            {
                double iVal;
                if (CalculateDouble(handler, args[0], out iVal))
                {
                    SetValue(handler, iVal);
                }
                else
                {
                    var iExp = args[0] as ResultExpression;
                    if (iExp != null)
                    {
                        ExecuteForExp(handler, iExp);
                    }
                }
            }
            else
            {
                LogService.Log.LogError("DecreaseWeight", "No arguments specified");
            }

            return true;
        }

        /// <summary>The set value.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="val">The val.</param>
        private void SetValue(Processor processor, double val)
        {
            var iPrev = processor.SplitValues.CurrentWeight;
            if (val >= 0)
            {
                if (iPrev >= (iPrev - val))
                {
                    processor.SplitValues.CurrentWeight -= val;
                }
                else
                {
                    processor.SplitValues.CurrentWeight = double.MinValue;
                    LogService.Log.LogWarning(
                        "DecreaseWeightInstruction.Execute", 
                        string.Format("double overflow encountered while decreasing the weight of the processor."));
                }
            }
            else
            {
                if (iPrev >= (iPrev - val))
                {
                    processor.SplitValues.CurrentWeight -= val;
                }
                else
                {
                    processor.SplitValues.CurrentWeight = double.MaxValue;
                    LogService.Log.LogWarning(
                        "DecreaseWeightInstruction.Execute", 
                        string.Format("double overflow encountered while decreasing the weight of the processor."));
                }
            }
        }

        #endregion
    }
}