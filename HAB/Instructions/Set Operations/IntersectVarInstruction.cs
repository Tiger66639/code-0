// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntersectVarInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Performs an intersection of the values of variables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Instructions.Set_Operations
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Performs an intersection of the values of variables.
    /// </summary>
    /// <remarks>
    ///     arg: -1 (at least 2 vars).
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IntersectVarInstruction)]
    public class IntersectVarInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntersectVarInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IntersectVarInstruction;
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
        ///     -1
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
            if (list != null && list.Count >= 2)
            {
                var iRes = processor.Mem.ArgumentStack.Peek();
                var iArgStackCount = 0;
                var a = list[0] as Variable;
                if (a != null)
                {
                    var iLeft = GetValuesFor(processor, a, ref iArgStackCount);
                    try
                    {
                        for (var i = 1; i < list.Count; i++)
                        {
                            var b = list[i] as Variable;
                            if (b != null)
                            {
                                var iRight = GetValuesFor(processor, b, ref iArgStackCount);
                                iLeft = Enumerable.ToList(Enumerable.Intersect(iLeft, iRight));
                                if (iLeft.Count == 0)
                                {
                                    // if there are no more items left to check, we might as well stop now.
                                    break;
                                }
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "IntersectVarInstruction.GetValues", 
                                    "All arguments should be Variables!");
                            }
                        }
                    }
                    finally
                    {
                        processor.Mem.ArgumentStack.Pop(iArgStackCount);
                    }

                    iRes.AddRange(iLeft);
                }
                else
                {
                    LogService.Log.LogError("IntersectVarInstruction.GetValues", "All arguments should be Variables!");
                }
            }
            else
            {
                LogService.Log.LogError("IntersectVarInstruction.GetValues", "No arguments specified");
            }
        }

        /// <summary>The get values for.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="var">The var.</param>
        /// <param name="argStackCount">The arg stack count.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        private static System.Collections.Generic.IList<Neuron> GetValuesFor(
            Processor processor, 
            Variable var, 
            ref int argStackCount)
        {
            System.Collections.Generic.IList<Neuron> iUniverse = SolveResultExpNoStackChange(var, processor);
            argStackCount++;
            return iUniverse;
        }
    }
}