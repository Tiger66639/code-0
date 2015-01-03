// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetLastOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the neuron to which a link points from the first argument. The link
//   should be the last found in the 'To' part.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the neuron to which a link points from the first argument. The link
    ///     should be the last found in the 'To' part.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: From part 2: meaning part
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetLastOutInstruction)]
    public class GetLastOutInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastOutInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetLastOutInstruction;
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
                return 2;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            Neuron iRes = null;
            if (list.Count >= 2)
            {
                var iFrom = list[0];
                if (iFrom != null)
                {
                    var iMeaning = list[1];
                    if (iMeaning != null)
                    {
                        iRes = iFrom.FindLastOut(iMeaning.ID);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetLastOutInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetLastOutInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetLastOutInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}