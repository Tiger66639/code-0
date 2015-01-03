// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildCountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of children contained in the specified cluster
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of children contained in the specified cluster
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 1: The cluster for which to return the nr of children.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChildCountInstruction)]
    public class ChildCountInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChildCountInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChildCountInstruction;
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

        #region ICalculateInt Members

        /// <summary>The calculate int.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    var iTemp = iCluster.TryGetBufferedChildren();
                    if (iTemp != null)
                    {
                        try
                        {
                            return iTemp.Count;
                        }
                        finally
                        {
                            iCluster.ReleaseBufferedChildren(iTemp);
                        }
                    }

                    if (iCluster.ChildrenIdentifier != null)
                    {
                        LockManager.Current.RequestLock(iCluster, LockLevel.Children, false);
                        try
                        {
                            return iCluster.ChildrenIdentifier.Count;
                        }
                        finally
                        {
                            LockManager.Current.ReleaseLock(iCluster, LockLevel.Children, false);
                        }
                    }

                    return 0;
                }

                LogService.Log.LogError("ChildCountInstruction.GetValues", "First argument should be a NeuronCluster!");
            }
            else
            {
                LogService.Log.LogError("ChildCountInstruction.GetValues", "No arguments specified");
            }

            return -1;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = NeuronFactory.GetInt(CalculateInt(processor, list));
            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}