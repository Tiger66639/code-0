// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeOfInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the exact type (as a neuron) of one or more neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the exact type (as a neuron) of one or more neurons.
    /// </summary>
    /// <remarks>
    ///     <para>arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>endless number of neurons.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.TypeOfInstruction)]
    public class TypeOfInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TypeOfInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.TypeOfInstruction;
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
            var iRes = processor.Mem.ArgumentStack.Peek();
            foreach (var i in list)
            {
                if (i is Instruction)
                {
                    iRes.Add(Brain.Current[(ulong)PredefinedNeurons.Instruction]);
                }
                else
                {
                    iRes.Add(i.TypeOfNeuron);
                }
            }
        }
    }
}