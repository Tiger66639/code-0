// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuspendInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Puts the current <see cref="Processor" /> (thread) to sleep and adds the
//   second argument to the first argument which has to be a cluster. This is
//   an atomic operation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Puts the current <see cref="Processor" /> (thread) to sleep and adds the
    ///     second argument to the first argument which has to be a cluster. This is
    ///     an atomic operation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         After this operation, the second neuron an be used to activate the
    ///         processor again with the Awake instruction.
    ///     </para>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>The cluster to add the item to.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The neuron that should be used to signal it's reactivation again through
    ///                 the <see cref="AwakeInstruction" /> .
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SuspendInstruction)]
    public class SuspendInstruction : Instruction
    {
        #region Fields

        /// <summary>The f suspended processors.</summary>
        private static readonly System.Collections.Generic.Dictionary<Neuron, Processor> fSuspendedProcessors =
            new System.Collections.Generic.Dictionary<Neuron, Processor>();

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SuspendInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SuspendInstruction;
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
                var iIndicator = args[1];
                if (iIndicator == null)
                {
                    LogService.Log.LogError("SuspendInstruction.Execute", "item to add is null (second arg).");
                    return;
                }

                if (fSuspendedProcessors.ContainsKey(iIndicator) == false)
                {
                    var iCluster = args[0] as NeuronCluster;
                    if (iCluster != null)
                    {
                        var iList = iCluster.ChildrenW;
                        iList.Lock(args[1]);
                        try
                        {
                            iList.AddUnsafe(args[1]);
                            Processor.CurrentProcessor.ThreadNeuron = iIndicator;
                        }
                        finally
                        {
                            iList.Unlock(args[1]);
                            iList.Dispose();
                        }

                        lock (fSuspendedProcessors) fSuspendedProcessors[iIndicator] = Processor.CurrentProcessor;
                        ThreadManager.Default.TryFreeThread(processor);

                            // we also need to let the ThreadManager (also manages the threads) know that this thread is sleeping now, so he can start a new one.
                        Processor.CurrentProcessor.ThreadBlocker.WaitOne();
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "SuspendInstruction.Execute", 
                            string.Format("Can't add, Invalid cluster specified: {0}.", args[0]));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "SuspendInstruction.Execute", 
                        "Neuron is already used as the wait handler for another processor.");
                }
            }
            else
            {
                LogService.Log.LogError("SuspendInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Resumes the specified <paramref name="suspended"/> processor.</summary>
        /// <param name="suspended">The neuron that identifies the suspended processor.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool Resume(Neuron suspended)
        {
            Processor iFound;
            if (fSuspendedProcessors.TryGetValue(suspended, out iFound))
            {
                iFound.ThreadNeuron = null; // we remove the neuron cause this no longer represents the item.
                lock (fSuspendedProcessors) fSuspendedProcessors.Remove(suspended);
                ThreadManager.Default.ReserveThread(iFound, ThreadRequestType.Awake);

                    // we use the ThreadManager, which also manages the treads to start the processor, to avoid overtaxing the system (we have a limit on the max concurrent running processors).
                return true;
            }

            return false;
        }
    }
}