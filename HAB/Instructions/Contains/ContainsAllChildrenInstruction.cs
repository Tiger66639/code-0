// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainsAllChildrenInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the first argument (which must be a
//   cluster), contains all the following argumetns, otherwise,
//   <see langword="false" /> is returned. When no items are specified,
//   <see langword="false" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if the first argument (which must be a
    ///     cluster), contains all the following argumetns, otherwise,
    ///     <see langword="false" /> is returned. When no items are specified,
    ///     <see langword="false" /> is returned.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The cluster ...neurons to check.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ContainsAllChildrenInstruction)]
    public class ContainsAllChildrenInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsAllChildrenInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ContainsAllChildrenInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this
        ///     instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without
        ///     any specific number of values.
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

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iCluster = list[0] as NeuronCluster;
                if (iCluster != null)
                {
                    if (list.Count > 1)
                    {
                        var iChildren = iCluster.Children;
                        iChildren.Lock();
                        try
                        {
                            for (var i = 1; i < list.Count; i++)
                            {
                                var iItem = list[i];
                                if (iItem != null)
                                {
                                    if (iChildren.ContainsUnsafe(iItem) == false)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            iChildren.Dispose();
                        }

                        return true;
                    }

                    return false;
                }

                if (Settings.LogContainsChildrenNoCluster)
                {
                    LogService.Log.LogError(
                        "ContainsAllChildrenInstruction.InternalGetValue", 
                        "Invalid first argument, NeuronCluster expected.");
                }
            }
            else
            {
                LogService.Log.LogError(
                    "ContainsAllChildrenInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return false;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CalculateBool(processor, list))
            {
                return Brain.Current.TrueNeuron;
            }

            return Brain.Current.FalseNeuron;
        }
    }
}