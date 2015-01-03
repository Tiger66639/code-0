// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Sends a neuroncluster's content to the processor for execution. The
//   cluster should contain expressions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sends a neuroncluster's content to the processor for execution. The
    ///     cluster should contain expressions.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CallInstruction)]
    public class CallInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CallInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CallInstruction;
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
                return 1;
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
                var iCluster = args[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iFrame = CallInstCallFrame.CreateCallInst(iCluster);
                    processor.PushFrame(iFrame);
                }
                else
                {
                    LogService.Log.LogError("CallInstruction.Execute", "First argument should be a neuron cluster.");
                }
            }
            else
            {
                LogService.Log.LogError("CallInstruction.Execute", "No arguments specified");
            }
        }
    }
}