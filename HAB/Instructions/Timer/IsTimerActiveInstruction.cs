// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsTimerActiveInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the timer (the arg) is active or not.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if the timer (the arg) is active or not.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The timer neuron.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IsTimerActiveInstruction)]
    public class IsTimerActiveInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsTimerActiveInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IsTimerActiveInstruction;
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
                return 1;
            }
        }

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iTimer = list[0] as TimerNeuron;
                if (iTimer != null)
                {
                    return iTimer.IsActive;
                }

                LogService.Log.LogError(
                    "IsTimerActiveInstruction.InternalGetValue", 
                    "Invalid first argument, TimerNeuron expected.");
            }
            else
            {
                LogService.Log.LogError(
                    "IsTimerActiveInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
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