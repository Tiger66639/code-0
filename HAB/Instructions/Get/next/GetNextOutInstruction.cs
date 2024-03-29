﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetNextOutInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the neuron to which a link, starting from the first arg, points to.
//   The link should be the next one found in the 'From' argument compared to
//   the last argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the neuron to which a link, starting from the first arg, points to.
    ///     The link should be the next one found in the 'From' argument compared to
    ///     the last argument.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: From part 2: meaning part 3: prev item
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetNextOutInstruction)]
    public class GetNextOutInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextOutInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetNextOutInstruction;
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
                var iFrom = list[0];
                if (iFrom != null)
                {
                    var iMeaning = list[1];
                    if (iMeaning != null)
                    {
                        var iPrev = list[2];
                        if (iPrev != null)
                        {
                            iRes = iFrom.FindNextOut(iMeaning.ID, iPrev);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetNextOutInstruction.InternalGetValue", 
                                "Invalid 3th argument, Neuron expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetNextOutInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetNextOutInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetNextOutInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return iRes;
        }
    }
}