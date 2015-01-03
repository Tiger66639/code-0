// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformResultInstruction.cs" company="">
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
    [NeuronID((ulong)PredefinedNeurons.PerformResultInstruction)]
    public class PerformResultInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PerformResultInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PerformResultInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iInst = list[0] as ResultInstruction;
                if (iInst != null)
                {
                    list.RemoveAt(0);

                        // remove the instruction, so that the list can be re-used as arguments for the instruction.
                    iInst.GetValues(processor, list);
                }
                else
                {
                    LogService.Log.LogError("PerformResult.GetValues", "First argument should be a result-instruction!");
                }
            }
            else
            {
                LogService.Log.LogError("PerformResult.GetValues", "No arguments specified");
            }
        }
    }
}