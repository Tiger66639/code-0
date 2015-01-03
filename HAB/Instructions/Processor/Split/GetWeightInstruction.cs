// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetWeightInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the weight value that was assigned to the processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the weight value that was assigned to the processor.
    /// </summary>
    /// <remarks>
    ///     Arguments: 0
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetWeightInstruction)]
    public class GetWeightInstruction : SingleResultInstruction, ICalculateDouble
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetWeightInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetWeightInstruction;
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

        #region ICalculateDouble Members

        /// <summary>The calculate double.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            return processor.SplitValues.CurrentWeight;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetDouble(processor.SplitValues.CurrentWeight);
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}