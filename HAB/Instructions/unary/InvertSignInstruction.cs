// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvertSignInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the boolean inverse of the argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the boolean inverse of the argument.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.InvertSignInstruction)]
    public class InvertSignInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertSignInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InvertSignInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iRes = processor.Mem.ArgumentStack.Peek();
                foreach (IntNeuron i in list)
                {
                    if (list[0] is IntNeuron)
                    {
                        var iInt = NeuronFactory.GetInt(-((IntNeuron)list[0]).Value);
                        Brain.Current.MakeTemp(iInt);
                        iRes.Add(iInt);
                    }
                    else if (list[0] is DoubleNeuron)
                    {
                        var iDouble = NeuronFactory.GetDouble(-((DoubleNeuron)list[0]).Value);
                        Brain.Current.MakeTemp(iDouble);
                        iRes.Add(iDouble);
                    }
                    else
                    {
                        LogService.Log.LogError("InvertSign", "int or double expected");
                    }
                }
            }
            else
            {
                LogService.Log.LogError("InvertSign", "No arguments specified");
            }
        }
    }
}