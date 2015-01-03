// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsClusteredByInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the first argument, is clustered by all
//   the following argumetns, which should be clusters, otherwise,
//   <see langword="false" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if the first argument, is clustered by all
    ///     the following argumetns, which should be clusters, otherwise,
    ///     <see langword="false" /> is returned.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The neuron to check ...clusters to if they contain the neuron.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IsClusteredByInstruction)]
    public class IsClusteredByInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsClusteredByInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IsClusteredByInstruction;
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
                var iToCheck = list[0];
                if (iToCheck != null)
                {
                    if (iToCheck.ClusteredByIdentifier == null)
                    {
                        return false;
                    }

                    IDListAccessor iClustered = iToCheck.ClusteredBy;
                    iClustered.Lock();
                    try
                    {
                        for (var i = 1; i < list.Count; i++)
                        {
                            var iItem = list[i];
                            if (iItem != null)
                            {
                                if (iClustered.ContainsUnsafe(iItem) == false)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    finally
                    {
                        iClustered.Unlock();
                        iClustered.Dispose();
                    }

                    return true;
                }

                LogService.Log.LogError(
                    "IsClusteredByAllInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
            }
            else
            {
                LogService.Log.LogError(
                    "IsClusteredByAllInstruction.InternalGetValue", 
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