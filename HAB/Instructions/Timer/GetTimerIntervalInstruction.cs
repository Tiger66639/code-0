// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetTimerIntervalInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the interval of the specified timer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the interval of the specified timer.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The timer for which to return the interval.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetTimerIntervalInstruction)]
    public class GetTimerIntervalInstruction : SingleResultInstruction, ICalculateDouble
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetTimerIntervalInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetTimerIntervalInstruction;
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

        #region ICalculateDouble Members

        /// <summary>Calculate the <see langword="double"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public double CalculateDouble(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iTimer = list[0] as TimerNeuron;
                if (iTimer != null)
                {
                    return iTimer.Interval;
                }

                LogService.Log.LogError(
                    "GetTimerIntervalInstruction.GetValues", 
                    "First argument should be a TimerNeuron!");
            }
            else
            {
                LogService.Log.LogError("GetTimerIntervalInstruction.GetValues", "No arguments specified");
            }

            return -1;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetDouble(CalculateDouble(processor, list));
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}