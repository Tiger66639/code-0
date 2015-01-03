// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuplicateInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns a duplicate copy (but with unique id) of the item passed in as
//   param.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns a duplicate copy (but with unique id) of the item passed in as
    ///     param.
    /// </summary>
    /// <remarks>
    ///     Only 1 argument allowed, the item that needs to be copied. returns a
    ///     duplicate of the item.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.DuplicateInstruction)]
    public class DuplicateInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DuplicateInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.DuplicateInstruction;
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
                return 1;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns><para>The result of the instruction: a duplicate of the item or<see cref="JaStDev.HAB.PredefinedNeurons.Empty"/></para>
        /// <para>if there is no valid result.</para>
        /// </returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1 && list[0] != null)
            {
                return list[0].Duplicate();
            }

            LogService.Log.LogError("DuplicateInstruction.InternalGetValue", "Invalid nr of arguments specified");
            return null;
        }
    }
}