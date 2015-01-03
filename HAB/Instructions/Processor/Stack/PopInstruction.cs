// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PopInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   This instruction removes the current <see cref="Neuron" /> from the
//   <see cref="Processor" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This instruction removes the current <see cref="Neuron" /> from the
    ///     <see cref="Processor" /> .
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.PopInstruction)]

    // [XmlRoot("PopInstruction", Namespace = "http://www.jastdev.com", IsNullable = false)]
    public class PopInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PopInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PopInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        public override int ArgCount
        {
            get
            {
                return 0;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            System.Diagnostics.Debug.Assert(processor != null);
            return processor.Pop();
        }
    }
}