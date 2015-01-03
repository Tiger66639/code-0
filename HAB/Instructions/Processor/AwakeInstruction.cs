// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwakeInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Lets a suspended processor (through <see cref="SuspendInstruction" /> )
//   continue processing and removes the neuron from the cluster. This is an
//   atomic instruction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Lets a suspended processor (through <see cref="SuspendInstruction" /> )
    ///     continue processing and removes the neuron from the cluster. This is an
    ///     atomic instruction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This instruction behaves very similar to the
    ///         <see cref="RemoveChildInstruction" /> in that it takes the same
    ///         arguments: a cluster and a neuron + it also tries to remove the neuron
    ///         from the cluster. This is done to provide an atomic remove operation with
    ///         the wakeup instruction. This way, we can be certain that, once the
    ///         processor has been woken up, the item is removed from the cluster.
    ///     </para>
    ///     <para>
    ///         <para>Arguments:</para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     the cluster fromt where the neuron needs to be removed.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     the neuron that represents the <see cref="Processor" /> . This was
    ///                     previously returned by the <see cref="SuspendInstruction" /> .
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AwakeInstruction)]
    public class AwakeInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AwakeInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AwakeInstruction;
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
                    LogService.Log.LogError("AwakeInstruction.Execute", "item to remove is null (second arg).");
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iList = iCluster.ChildrenW;
                    iList.Lock(iIndicator);
                    try
                    {
                        iList.RemoveUnsafe(iIndicator);
                        SuspendInstruction.Resume(iIndicator);
                    }
                    finally
                    {
                        iList.Unlock(iIndicator);
                        iList.Dispose();
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "AwakeInstruction.Execute", 
                        string.Format("Can't remove, Invalid cluster specified: {}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("AwakeInstruction.Execute", "No arguments specified");
            }
        }
    }
}