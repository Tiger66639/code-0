// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModStoreDoubleInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   a faster instruction to perform a <see langword="double" /> addition and
//   store the result in a double.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a faster instruction to perform a <see langword="double" /> addition and
    ///     store the result in a double.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.ModStoreDoubleInstruction)]
    public class ModStoreDoubleInstruction : Instruction, IExecStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModStoreDoubleInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ModStoreDoubleInstruction;
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

        #region IExecStatement Members

        /// <summary>Performs the specified handler.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Perform(Processor handler, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count == 3)
            {
                var iRes = SolveSingleResultExp(list[0], handler) as DoubleNeuron;
                double iSecond;
                double iThird;
                if (iRes == null)
                {
                    LogService.Log.LogWarning("ModStoreDouble", "first arg should be a double");
                    return true;
                }

                CodeLoader.LoadFor(list[1] as Expression);

                    // make certain that code is loaded before lock, to prevent deadlocks.
                CodeLoader.LoadFor(list[2] as Expression);

                    // make certain that code is loaded before lock, to prevent deadlocks.
                LockManager.Current.RequestLock(iRes, LockLevel.Value, true);

                    // the whole calculation needs to be thread save.
                try
                {
                    if (CalculateDouble(handler, list[1], out iSecond) == false)
                    {
                        var iConv = GetAsDouble(SolveSingleResultExp(list[1], handler));
                        if (iConv.HasValue)
                        {
                            iSecond = iConv.Value;
                        }
                        else
                        {
                            LogService.Log.LogWarning("ModStoreDouble", "second arg should be an int or double");
                            return true;
                        }
                    }

                    if (CalculateDouble(handler, list[2], out iThird) == false)
                    {
                        var iConv = GetAsDouble(SolveSingleResultExp(list[2], handler));
                        if (iConv.HasValue)
                        {
                            iThird = iConv.Value;
                        }
                        else
                        {
                            LogService.Log.LogWarning("ModStoreDouble", "third arg should be an int or double");
                            return true;
                        }
                    }

                    iRes.SetValueDirect(iSecond % iThird);
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
            LockManager.Current.RequestLock(args[0], LockLevel.Value, true);

                // the whole calculation needs to be thread save.
            try
            {
                ((DoubleNeuron)args[0]).SetValueDirect(((DoubleNeuron)args[1]).Value % ((DoubleNeuron)args[2]).Value);
            }
            finally
            {
                LockManager.Current.ReleaseLock(args[0], LockLevel.Value, true);
            }

            args[0].SetIsChangedNoUnfreeze(true);

                // don't unfreeze, otherwise changing the value would cause to many mem leaks.
            if (Brain.Current.HasNeuronChangedEvents)
            {
                Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", args[0]));
            }
        }
    }
}