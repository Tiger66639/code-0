// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetFirstOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the first link out to a new value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the first link out to a new value.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: From part 2: meaning part
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetFirstOutInstruction)]
    public class GetFirstOutInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstOutInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetFirstOutInstruction;
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
                        iRes = iFrom.FindFirstOut(iMeaning.ID);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetFirstOutInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetFirstOutInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetFirstOutInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}