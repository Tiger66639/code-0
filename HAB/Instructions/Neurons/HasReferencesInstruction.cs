// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HasReferencesInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if any of the neurons in the arguremnt
//   list has any references (incomming, outgoing links or parents).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if any of the neurons in the arguremnt
    ///     list has any references (incomming, outgoing links or parents).
    /// </summary>
    /// <remarks>
    ///     <para>arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>endless number of neurons.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.HasReferencesInstruction)]
    public class HasReferencesInstruction : MultiResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.HasReferencesInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.HasReferencesInstruction;
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

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            foreach (var i in list)
            {
                if (BrainHelper.HasReferences(i))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = processor.Mem.ArgumentStack.Peek();
            if (CalculateBool(processor, list))
            {
                iRes.Add(Brain.Current.TrueNeuron);
            }
            else
            {
                iRes.Add(Brain.Current.FalseNeuron);
            }
        }
    }
}