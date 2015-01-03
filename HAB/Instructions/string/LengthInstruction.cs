// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LengthInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   returns the length of a string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Instructions
{
    /// <summary>
    ///     returns the length of a string.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.LengthInstruction)]
    public class LengthInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LengthInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.LengthInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count > 0)
            {
                var iText = list[0] as TextNeuron;
                if (iText != null)
                {
                    return iText.Text.Length;
                }

                LogService.Log.LogError("length", "TextNeuron expected.");
            }
            else
            {
                LogService.Log.LogError("Length", "no arguments specified.");
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