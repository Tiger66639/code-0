// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetClustersInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the clusters to which the argument belongs.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the clusters to which the argument belongs.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: the neuron for which to return all the clusters it belongs
    ///     to.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetClustersInstruction)]
    public class GetClustersInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetClustersInstruction;
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
                return 1;
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
                var iNeuron = list[0];
                var iParents = iNeuron.GetBufferedClusteredBy();
                try
                {
                    var iRes = processor.Mem.ArgumentStack.Peek();
                    foreach (var i in iParents)
                    {
                        iRes.Add(i);
                    }
                }
                finally
                {
                    iNeuron.ReleaseBufferedCluseteredBy(iParents);
                }

                // using (ListAccessor<ulong> iClusteredBy = iNeuron.ClusteredBy)
                // {

                // for (int i = 0; i < iClusteredBy.Count; i++)                                        //for loop is faster than linq statement.
                // {
                // Neuron iFound = null;
                // if (Brain.Current.TryFindNeuron(iClusteredBy[i], out iFound) == true)            //we use tryfind cause there is a mysterious bug in the system that allows items to be removed from the network, while still in this list during the lock?.
                // iRes.Add(iFound);
                // }
                // }
            }
            else
            {
                LogService.Log.LogError("GetClustersInstruction.GetValues", "No arguments specified");
            }
        }
    }
}