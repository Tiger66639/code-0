// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetSplitCountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of processors that are involved in the current split
//   path ( all the splits that originated from the same source split, with
//   the same callback). This can be used to check if there are any 'overruns'
//   or is 'to much echo' going on. so that the parse can be stopped.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of processors that are involved in the current split
    ///     path ( all the splits that originated from the same source split, with
    ///     the same callback). This can be used to check if there are any 'overruns'
    ///     or is 'to much echo' going on. so that the parse can be stopped.
    /// </summary>
    /// <remarks>
    ///     Arguments: 0
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetSplitCountInstruction)]
    public class GetSplitCountInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetSplitCountInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetSplitCountInstruction;
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
            if (processor.SplitData != null)
            {
                return processor.SplitData.Head.SubProcessors.Count;
            }

            return 0;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetInt(CalculateInt(processor, list));
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}