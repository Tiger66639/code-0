// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StackCountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of neurons that are left on the stack (excluding the
//   currently executing one).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of neurons that are left on the stack (excluding the
    ///     currently executing one).
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.StackCountInstruction)]
    public class StackCountInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StackCountInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.StackCountInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return processor.Count;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetInt(processor.Count);
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}