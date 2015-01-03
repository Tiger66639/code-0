// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Exexutes an instruction and returns it's results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Exexutes an instruction and returns it's results.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>The instruction to execute.</description>
    ///         </item>
    ///         <item>
    ///             <description>The remaining arguments for the instruction.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.PerformInstruction)]
    public class PerformInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PerformInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PerformInstruction;
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
            if (args != null && args.Count >= 1)
            {
                var iInst = args[0] as Instruction;
                if (iInst != null)
                {
                    args.RemoveAt(0);

                        // remove the instruction, so that the list can be re-used as arguments for the instruction.
                    iInst.Execute(processor, args);
                }
                else
                {
                    LogService.Log.LogError("Perform.Execute", "First argument should be an instruction!");
                }
            }
            else
            {
                LogService.Log.LogError("Perform.Execute", "No arguments specified");
            }
        }
    }
}