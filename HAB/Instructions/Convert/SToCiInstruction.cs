// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SToCiInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Converts a string neuron into a cluster containing <see langword="int" />
//   neurons, where each neuron represents a single ascii
//   <see langword="char" /> of the string. It is possible to convert multiple
//   items at once.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Converts a string neuron into a cluster containing <see langword="int" />
    ///     neurons, where each neuron represents a single ascii
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
    [NeuronID((ulong)PredefinedNeurons.SToCiInstruction)]
    public class SToCiInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToCiInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SToCiInstruction;
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
                        var iRes = NeuronFactory.GetCluster();
                        Brain.Current.Add(iRes);
                        foreach (var i in iPar.Text)
                        {
                            var iItem = NeuronFactory.GetInt(i);
                            Brain.Current.Add(iItem);
                            using (var iChildren = iRes.ChildrenW) iChildren.Add(iItem); // lock 1 by 1 to prevent deadlocks.
                        }

                        iList.Add(iRes);
                    }
                    else
                    {
                        LogService.Log.LogError("SToCiInstruction.GetValues", "arguments must be text neurons!");
                        return;
                    }
                }
            }
            else
            {
                LogService.Log.LogError("SToCiInstruction.GetValues", "Invalid nr of arguments specified!");
            }
        }
    }
}