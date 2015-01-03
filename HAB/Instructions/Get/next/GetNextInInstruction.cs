// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetNextInInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the neuron from which a link starts to the first argument. The link
//   should be the next one found in the 'To' argument compared to the last
//   argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the neuron from which a link starts to the first argument. The link
    ///     should be the next one found in the 'To' argument compared to the last
    ///     argument.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: To part 2: meaning part 3: previous from
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetNextInInstruction)]
    public class GetNextInInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextInInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetNextInInstruction;
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
                return 3;
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
                var iTo = list[0];
                if (iTo != null)
                {
                    var iMeaning = list[1];
                    if (iMeaning != null)
                    {
                        var iPrev = list[2];
                        if (iPrev != null)
                        {
                            iRes = iTo.FindNextIn(iMeaning.ID, iPrev);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetNextInInstruction.InternalGetValue", 
                                "Invalid 3th argument, Neuron expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetNextInInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetNextInInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetNextInInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}