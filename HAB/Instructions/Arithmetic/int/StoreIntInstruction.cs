// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoreIntInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   a faster instruction to store an <see langword="int" /> value into another
//   int. arg 1: the <see langword="int" /> to store the value to 2: the value
//   to store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a faster instruction to store an <see langword="int" /> value into another
    ///     int. arg 1: the <see langword="int" /> to store the value to 2: the value
    ///     to store.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.StoreIntInstruction)]
    public class StoreIntInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoreIntInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.StoreIntInstruction;
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

        /// <summary>called by the statement when the instruction can calculate it's own
        ///     arguments.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        /// <returns>True if the function was performed succesful, <see langword="false"/>
        ///     if the statement should try again (usually cause the arguments
        ///     couldn't be calculated).</returns>
        public bool Perform(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 2)
            {
                var iRes = args[0] as IntNeuron;
                if (iRes == null)
                {
                    iRes = SolveSingleResultExp(args[0], processor) as IntNeuron;
                }

                int iInt;
                if (iRes != null)
                {
                    LockManager.Current.RequestLock(iRes, LockLevel.Value, true);

                        // the whole calculation needs to be thread save.
                    try
                    {
                        if (CalculateInt(processor, args[1], out iInt))
                        {
                            iRes.SetValueDirect(iInt);
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "StoreInt", 
                                string.Format("Can't store int, Invalid second arg specified: {0}.", args[1]));
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(iRes, LockLevel.Value, true);
                    }

                    iRes.SetIsChangedNoUnfreeze(true);

                        // don't unfreeze, otherwise changing the value would cause to many mem leaks.
                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", iRes));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "StoreInt", 
                        string.Format("Can't store int, Invalid int specified: {0}.", args[0]));
                }

                return true;
            }

            return false;
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
            ((IntNeuron)args[0]).Value = ((IntNeuron)args[1]).Value;
        }
    }
}