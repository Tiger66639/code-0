// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexOfLinkInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the index position of a link in one of the sides of the link
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the index position of a link in one of the sides of the link
    /// </summary>
    /// <remarks>
    ///     arg: 1: from part of link 2: to part of link 3: meaning part of link 4:
    ///     either from or to part, to indicate at which side the index should be
    ///     returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IndexOfLinkInstruction)]
    public class IndexOfLinkInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IndexOfLinkInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IndexOfLinkInstruction;
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
                return 4;
            }
        }

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 4)
            {
                if (list[0] == null)
                {
                    LogService.Log.LogError("IndexOfLinkInstruction.GetLink", string.Format("From part of is null"));
                    return -1;
                }

                if (list[1] == null)
                {
                    LogService.Log.LogError("IndexOfLinkInstruction.GetLink", string.Format("To part is null"));
                    return -1;
                }

                if (list[2] == null)
                {
                    LogService.Log.LogError("IndexOfLinkInstruction.GetLink", string.Format("Meaning part is null"));
                    return -1;
                }

                if (list[3] == null)
                {
                    LogService.Log.LogError("IndexOfLinkInstruction.GetLink", string.Format("Meaning part is null"));
                    return -1;
                }

                return Link.FindIndex(list[0], list[1], list[2], list[3]);
            }

            LogService.Log.LogError("IndexOfLinkInstruction.InternalGetValue", "No arguments specified");
            return -1; // we always return a value
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