// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetClusterMeaningInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Changes the meaning of a <see cref="NeuronCluster" /> to the new value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes the meaning of a <see cref="NeuronCluster" /> to the new value.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The who's meaning needs to be changed.</description>
    ///         </item>
    ///         <item>
    ///             <description>2: The new meaning of the cluster.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SetClusterMeaningInstruction)]
    public class SetClusterMeaningInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetClusterMeaningInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SetClusterMeaningInstruction;
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
                if (args[1] == null)
                {
                    LogService.Log.LogError(
                        "SetClusterMeaningInstruction.Execute", 
                        "Meaning argument can't be null (second arg).");
                    return;
                }

                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    iCluster.SetMeaning(args[1]);
                }
                else
                {
                    LogService.Log.LogError(
                        "SetClusterMeaningInstruction.Execute", 
                        string.Format("Can't change meaning, Invalid cluster specified: {0}.", args[0]));
                }
            }
            else
            {
                LogService.Log.LogError("SetClusterMeaningInstruction.Execute", "Invalid nr of arguments specified");
            }
        }
    }
}