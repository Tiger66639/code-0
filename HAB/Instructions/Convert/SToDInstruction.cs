// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SToDInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a string neuron into a cluster containing string neurons where
//   each string neuron has a single char. It is possible to convert multiple
//   items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a string neuron into a cluster containing string neurons where
    ///     each string neuron has a single char. It is possible to convert multiple
    ///     items at once.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a list of string neuron.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SToDInstruction)]
    public class SToDInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToDInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SToDInstruction;
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
                var iList = new System.Collections.Generic.List<Neuron>();
                foreach (var iNeuron in list)
                {
                    var iPar = iNeuron as TextNeuron;
                    if (iPar != null)
                    {
                        double iVal;
                        if (double.TryParse(iPar.Text, out iVal))
                        {
                            var iRes = NeuronFactory.GetDouble(iVal);
                            Brain.Current.MakeTemp(iRes);
                            iList.Add(iRes);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "SToDInstruction.GetValues", 
                                string.Format(
                                    "Argument(value {0}) must be a Text neuron that can be converted to a double: {1}", 
                                    iPar, 
                                    iPar.Text));
                            return;
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("SToDInstruction.GetValues", "Arguments must be Text neurons!");
                    }
                }

                processor.Mem.ArgumentStack.Peek().AddRange(iList);
            }
            else
            {
                LogService.Log.LogError("SToDInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }
    }
}