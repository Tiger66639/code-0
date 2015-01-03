// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoLiInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a string neuron into a list of <see langword="int" /> neurons,
//   where each <see langword="int" /> represents a single ascii
//   <see langword="char" /> of the string. It is possible to convert multiple
//   items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a string neuron into a list of <see langword="int" /> neurons,
    ///     where each <see langword="int" /> represents a single ascii
    ///     <see langword="char" /> of the string. It is possible to convert multiple
    ///     items at once.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1 or more string neurons</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.StoLiInstruction)]
    public class StoLiInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoLiInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.StoLiInstruction;
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
                var iList = processor.Mem.ArgumentStack.Peek();
                foreach (var iNeuron in list)
                {
                    var iPar = iNeuron as TextNeuron;
                    if (iPar != null)
                    {
                        foreach (var i in iPar.Text)
                        {
                            var iItem = NeuronFactory.GetInt(i);
                            Brain.Current.Add(iItem);
                            iList.Add(iItem);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError("StoLiInstruction.GetValues", "arguments must be text neurons!");
                        return;
                    }
                }
            }
            else
            {
                LogService.Log.LogError("StoLiInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }
    }
}