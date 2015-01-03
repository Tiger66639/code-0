// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Sends the specified arguments to a <see cref="Sin" /> for output. This is
//   done directly (so without solving first) in a synchronous manner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sends the specified arguments to a <see cref="Sin" /> for output. This is
    ///     done directly (so without solving first) in a synchronous manner.
    /// </summary>
    /// <remarks>
    ///     <para>At least 2 parameters are required:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>first the to use for output.</description>
    ///         </item>
    ///         <item>
    ///             <description>followed by the first neuron to send as output</description>
    ///         </item>
    ///         <item>
    ///             <description>and any others may come after that.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.OutputInstruction)]
    public class OutputInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="PredefinedNeurons.DirectOutputInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.OutputInstruction;
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
            if (args != null && args.Count >= 2)
            {
                var iSin = args[0] as Sin;
                if (iSin != null)
                {
                    args.RemoveAt(0); // remove the sin, don't need to render it's name.
                    iSin.Output(args);
                }
                else
                {
                    LogService.Log.LogError(
                        "OutInstruction.Execute", 
                        "Invalid first argument, NeuronCluster expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("OutInstruction.Execute", "No arguments specified");
            }
        }
    }
}