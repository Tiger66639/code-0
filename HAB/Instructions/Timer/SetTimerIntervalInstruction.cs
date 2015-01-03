// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetTimerIntervalInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Assigns a new interval to the timer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     Assigns a new interval to the timer.
    /// </summary>
    /// <remarks>
    ///     <para>Args: - a timerNeuron</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a that represens the interva.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetTimerIntervalInstruction)]
    public class SetTimerIntervalInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetTimerIntervalInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetTimerIntervalInstruction;
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

        #region IExecStatement Members

        /// <summary>Performs the specified processor.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                var iTimer = args[0] as TimerNeuron;
                if (iTimer == null && args[0] is ResultExpression)
                {
                    iTimer = SolveSingleResultExp(args[0], processor) as TimerNeuron;
                }

                double iDouble;
                if (CalculateDouble(processor, args[1], out iDouble))
                {
                    if (iTimer == null)
                    {
                        LogService.Log.LogError(
                            "SetTimerIntervalInstruction.Execute", 
                            string.Format("Can't set value, Invalid timer specified: {0}.", args[0]));
                    }
                    else
                    {
                        iTimer.Interval = iDouble;
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "SetTimerIntervalInstruction.Execute", 
                        string.Format("Can't set value, Invalid DoubleNeuron specified: {0}.", args[1]));
                }

                return true;
            }

            return false;

                // can't directly resolve args, let the statement give a try, perhaps some arguments are hidden under a single var.
        }

        #endregion

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                var iTimer = args[0] as TimerNeuron;
                var iInterval = args[1] as DoubleNeuron;
                if (iTimer == null)
                {
                    LogService.Log.LogError(
                        "SetTimerIntervalInstruction.Execute", 
                        string.Format("Can't set value, Invalid var specified: {0}.", args[0]));
                }
                else
                {
                    var iDouble = GetAsDouble(iInterval);
                    if (iDouble.HasValue == false)
                    {
                        LogService.Log.LogError(
                            "SetTimerIntervalInstruction.Execute", 
                            string.Format("Can't set value, Invalid DoubleNeuron specified: {0}.", args[1]));
                    }
                    else
                    {
                        iTimer.Interval = iDouble.Value;
                    }
                }
            }
            else
            {
                LogService.Log.LogError("SetTimerIntervalInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}