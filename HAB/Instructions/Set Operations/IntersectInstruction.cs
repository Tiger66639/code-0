// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntersectInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Performs an intersection of the list of clusters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Performs an intersection of the list of clusters.
    /// </summary>
    /// <remarks>
    ///     arg: -1 (at least 2 clusters).
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IntersectInstruction)]
    public class IntersectInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntersectInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IntersectInstruction;
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
                var a = list[0] as NeuronCluster;
                if (a != null)
                {
                    var iLeft = a.GetBufferedChildren<Neuron>();
                    try
                    {
                        System.Collections.Generic.IList<Neuron> iRight = null;
                        for (var i = 1; i < list.Count; i++)
                        {
                            var b = list[i] as NeuronCluster;
                            if (b != null)
                            {
                                iRight = b.GetBufferedChildren<Neuron>();
                                try
                                {
                                    iLeft = Enumerable.ToList(Enumerable.Intersect(iLeft, iRight));
                                }
                                finally
                                {
                                    b.ReleaseBufferedChildren((System.Collections.IList)iRight);
                                }

                                if (iLeft.Count == 0)
                                {
                                    // if there are no more items left to check, we might as well stop now.
                                    break;
                                }
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "IntersectInstruction.GetValues", 
                                    "All arguments should be clusters!");
                            }
                        }

                        processor.Mem.ArgumentStack.Peek().AddRange(iLeft);
                    }
                    finally
                    {
                        a.ReleaseBufferedChildren((System.Collections.IList)iLeft);
                    }
                }
                else
                {
                    LogService.Log.LogError("IntersectInstruction.GetValues", "All arguments should be clusters!");
                }
            }
            else
            {
                LogService.Log.LogError("IntersectInstruction.GetValues", "No arguments specified");
            }
        }
    }
}