﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartTimerInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   starts all the timers in the argument.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>
    ///     starts all the timers in the argument.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.StartTimerInstruction)]
    internal class StartTimerInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StartTimerInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.StartTimerInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Any length is possible but at least 2 are required.
        /// </summary>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

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
            if (args != null && args.Count >= 1)
            {
                foreach (var i in args)
                {
                    var iTimer = i as TimerNeuron;
                    if (iTimer != null)
                    {
                        iTimer.IsActive = true;
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "StartTimerInstruction.Execute", 
                            string.Format("Timer neuron expected, found: {0}", i.TypeOfNeuron));
                    }
                }
            }
            else
            {
                LogService.Log.LogError("StartTimerInstruction.Execute", "No arguments specified");
            }
        }
    }
}