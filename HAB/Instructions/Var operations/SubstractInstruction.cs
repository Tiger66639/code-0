// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubstractInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Removes the arguments from the first argument, which must be a var
//   (usually passed along with a byref).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Removes the arguments from the first argument, which must be a var
    ///     (usually passed along with a byref).
    /// </summary>
    /// <remarks>
    ///     Arg: -1. 1: a reference to a var (usually passed along with a byref). 2:
    ///     the first item that needs to be removed from the set contained by var 1.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SubstractInstruction)]
    public class SubstractInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SubstractInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SubstractInstruction;
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
            System.Collections.Generic.List<Neuron> iRes = null;
            if (list != null && list.Count >= 1)
            {
                var iVar = list[0] as Variable;
                if (iVar != null)
                {
                    var iArgs = processor.Mem.ArgumentStack.Peek();
                    iRes = SolveResultExpNoStackChange(iVar, processor);

                        // we copy over the content of the var, so we can remove all the other items.
                    try
                    {
                        foreach (var i in iRes)
                        {
                            if (list.Contains(i) == false)
                            {
                                iArgs.Add(i);
                            }
                        }
                    }
                    finally
                    {
                        processor.Mem.ArgumentStack.Pop();
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "SubstractInstruction.InternalGetValue", 
                        "Invalid first argument, Variable expected.");
                }
            }
            else
            {
                LogService.Log.LogError("SubstractInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }
        }
    }
}