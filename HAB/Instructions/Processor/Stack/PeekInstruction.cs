// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeekInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the top of the <see cref="Neuron" /> execution stack without
//   removing it. If the stack is empty,
//   <see cref="PredefinedNeurons.Emtpy" /> is used.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the top of the <see cref="Neuron" /> execution stack without
    ///     removing it. If the stack is empty,
    ///     <see cref="PredefinedNeurons.Emtpy" /> is used.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.PeekInstruction)]
    public class PeekInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PeekInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PeekInstruction;
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
                return 0;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return processor.Peek();
        }
    }
}