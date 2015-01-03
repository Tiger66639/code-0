// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndsWithInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the textvalue of the first argument
//   (which must be a textneuron), ends with the second arg (which also must
//   be textneuron). otherwise, <see langword="false" /> is returned. When no
//   items are specified, <see langword="false" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if the textvalue of the first argument
    ///     (which must be a textneuron), ends with the second arg (which also must
    ///     be textneuron). otherwise, <see langword="false" /> is returned. When no
    ///     items are specified, <see langword="false" /> is returned.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: a textneuron. 2: a textneuron ...neurons to check.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.EndsWithInstruction)]
    public class EndsWithInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.EndsWithInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.EndsWithInstruction;
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
                return 2;
            }
        }

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 2)
            {
                var iText = list[0] as TextNeuron;
                if (iText != null)
                {
                    var iSec = list[1] as TextNeuron;
                    if (iSec != null)
                    {
                        if (iText.Text.EndsWith(iSec.Text))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "EndsWithInstruction.InternalGetValue", 
                            "Invalid second argument, TextNeuron expected.");
                    }
                }
                else if (Settings.LogContainsChildrenNoCluster)
                {
                    LogService.Log.LogError(
                        "EndsWithInstruction.InternalGetValue", 
                        "Invalid first argument, TextNeuron expected.");
                }
            }
            else
            {
                LogService.Log.LogError("EndsWithInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return false;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CalculateBool(processor, list))
            {
                return Brain.Current.TrueNeuron;
            }

            return Brain.Current.FalseNeuron;
        }
    }
}