// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Inserts an item into a list
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Inserts an item into a list
    /// </summary>
    /// <remarks>
    ///     <para>Args: - Cluster to insert to</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>item to insert</description>
    ///         </item>
    ///         <item>
    ///             <description>position at which to insert.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.InsertChildInstruction)]
    public class InsertChildInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InsertChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InsertChildInstruction;
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
            if (args != null && args.Count >= 3)
            {
                int iIndex;
                if (CheckArgsForChildren(args, out iIndex) == false)
                {
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iLock = BuildClusterLock(args, true);
                    LockManager.Current.RequestLocks(iLock);
                    try
                    {
                        iCluster.InsertChildUnsafe(iIndex, args[1]);
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iLock);
                        iCluster.IsChanged = true;
                        args[1].IsChanged = true;
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "InsertChildInstruction.Execute", 
                        string.Format("Can't add, Invalid cluster specified: {0}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("InsertChildInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Checks the <paramref name="args"/> for a child insert operation.</summary>
        /// <param name="args">The args.</param>
        /// <param name="index">returns the index at which the insert should be done.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgsForChildren(System.Collections.Generic.IList<Neuron> args, out int index)
        {
            index = -1;
            if (args.Count >= 3)
            {
                var iRes = true;
                if (args[1] == null)
                {
                    LogService.Log.LogError(
                        "InsertChildInstruction.CheckArgsForLink", 
                        "item to insert is null (second arg).");
                    iRes = false;
                }

                if (!(args[2] is IntNeuron))
                {
                    LogService.Log.LogError(
                        "InsertChildInstruction.CheckArgsForLink", 
                        "insert position is invalid (last arg).");
                    iRes = false;
                }

                if (!(args[0] is NeuronCluster))
                {
                    LogService.Log.LogError(
                        "InsertChildInstruction.CheckArgsForLink", 
                        "owner of the list is invalid (null or not a cluster) (first arg).");
                    iRes = false;
                }

                if (iRes)
                {
                    index = ((IntNeuron)args[2]).Value;
                }

                return iRes;
            }

            LogService.Log.LogError("InsertChildInstruction.CheckArgsForChildren", "Invalid nr of arguments specified");
            return false;
        }
    }
}