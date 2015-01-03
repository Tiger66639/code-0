// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncreaseWeightInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Increases the current weight of the procesor by the specified amount.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Increases the current weight of the procesor by the specified amount.
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
    [NeuronID((ulong)PredefinedNeurons.IncreaseWeightInstruction)]
    public class IncreaseWeightInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IncreaseWeightInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IncreaseWeightInstruction;
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
                LogService.Log.LogError("IncreaseWeightInstruction.Execute", "No arguments specified");
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

            LogService.Log.LogError("IncreaseWeightInstruction.Execute", "IntNeuron or DoubleNeuron expected as arg.");
            result = double.MinValue;
            return false;
        }

        #region IExecStatement Members

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
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
                LogService.Log.LogError("IncreaseWeightInstruction.Execute", "No arguments specified");
            }

            return true;
        }

        /// <summary>The set value.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="iVal">The i val.</param>
        private static void SetValue(Processor handler, double iVal)
        {
            var iPrev = handler.SplitValues.CurrentWeight;
            if (iVal >= 0)
            {
                if (iPrev <= (iPrev + iVal))
                {
                    handler.SplitValues.CurrentWeight += iVal;
                }
                else
                {
                    handler.SplitValues.CurrentWeight = double.MaxValue;
                    LogService.Log.LogWarning(
                        "IncreaseWeightInstruction.Execute", 
                        string.Format("double overflow encountered while increasing weight of the processor."));
                }
            }
            else
            {
                if (iPrev + iVal <= iPrev)
                {
                    handler.SplitValues.CurrentWeight += iVal;
                }
                else
                {
                    handler.SplitValues.CurrentWeight = double.MinValue;
                    LogService.Log.LogWarning(
                        "IncreaseWeightInstruction.Execute", 
                        string.Format("double overflow encountered while increasing the weight of the processor."));
                }
            }
        }

        #endregion
    }
}