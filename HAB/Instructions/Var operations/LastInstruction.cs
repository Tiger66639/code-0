// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LastInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the last item in the argument list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the last item in the argument list.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.GetLastInstruction)]
    public class GetLastInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetLastInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this
        ///     instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without
        ///     any specific number of values.
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

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                return list[list.Count - 1];
            }

            return null;
        }
    }
}