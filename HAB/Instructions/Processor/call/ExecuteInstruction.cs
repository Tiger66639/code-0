// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecuteInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   An instruction that takes 1 or more expressions as argument which are
//   executed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An instruction that takes 1 or more expressions as argument which are
    ///     executed.
    /// </summary>
    /// <remarks>
    ///     Arg: - first expression returns: Any possible results of these
    ///     expressions is returned.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ExecuteInstruction)]
    public class ExecuteInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExecuteInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ExecuteInstruction;
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

        /// <summary>performs the task and returns it's result (on the argument<paramref name="list"/> of the processor).</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var iExp = list[i] as Expression;
                    if (iExp != null)
                    {
                        if (iExp is ResultExpression)
                        {
                            ((ResultExpression)iExp).GetValue(processor);

                                // this automatically puts the result on the argument list of the processor.
                        }
                        else if (iExp is Expression)
                        {
                            iExp.Execute(processor);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ExecuteInstruction.GetValues", 
                            string.Format("arg {0} should be an expression!", i));
                    }
                }
            }
            else
            {
                LogService.Log.LogError("ExecuteInstruction.GetValues", "No arguments specified");
            }
        }
    }
}