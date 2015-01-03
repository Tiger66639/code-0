// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecreaseWeightOfInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Increases the weight of a single neuron in the context of the current
//   processor by the specified amount.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Increases the weight of a single neuron in the context of the current
    ///     processor by the specified amount.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 1: the neuron to change the weight of.</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 A that defines how much the weight should be changed.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.DecreaseWeightOfInstruction)]
    public class DecreaseWeightOfInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DecreaseWeightOfInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.DecreaseWeightOfInstruction;
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

        #region IExecStatement Members

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                var iFirst = SolveSingleResultExp(args[0], handler);
                double iVal;
                if (CalculateDouble(handler, args[1], out iVal))
                {
                    handler.SplitValues.DecreaseWeightOf(iFirst, iVal);
                }
                else
                {
                    var iSecond = SolveSingleResultExp(args[1], handler);
                    var iTryVal = GetAsDouble(iSecond);
                    if (iTryVal.HasValue)
                    {
                        handler.SplitValues.DecreaseWeightOf(iFirst, iTryVal.Value);
                    }
                    else
                    {
                        LogService.Log.LogError("DecreaseWeightOf.Execute", "Second argument should be a double.");
                    }
                }
            }
            else
            {
                LogService.Log.LogError("DecreaseWeightOf.Execute", "No arguments specified");
            }

            return true;
        }

        #endregion

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
            if (args != null && args.Count >= 2)
            {
                double iVal;
                if (GetVal(args[1], out iVal))
                {
                    processor.SplitValues.DecreaseWeightOf(args[0], iVal);
                }
            }
            else
            {
                LogService.Log.LogError("DecreaseWeightOf.Execute", "No arguments specified");
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

            LogService.Log.LogError("DecreaseWeightOf.Execute", "IntNeuron or DoubleNeuron expected as arg.");
            result = double.MinValue;
            return false;
        }
    }
}