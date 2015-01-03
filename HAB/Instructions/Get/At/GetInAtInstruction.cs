// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the incomming link at the specified index.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the incomming link at the specified index.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The neuron to search the link for.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 2: An IntNeuron, which contains the index position
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetInAtInstruction)]
    public class GetInAtInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetInAtInstruction;
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
                return 2;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var iNeuron = list[0];
                var iInt = GetAsInt(list[1]);
                if (iNeuron != null)
                {
                    if (iInt != null && iInt.HasValue)
                    {
                        Link iRes = null;
                        if (iNeuron.LinksInIdentifier != null)
                        {
                            ListAccessor<Link> iLinks = iNeuron.LinksIn;
                            iLinks.Lock();
                            try
                            {
                                if (iLinks.CountUnsafe > iInt.Value && iInt.Value >= 0)
                                {
                                    iRes = iLinks.GetUnsafe(iInt.Value);
                                }
                            }
                            finally
                            {
                                iLinks.Unlock();
                                iLinks.Dispose();
                            }
                        }

                        if (iRes != null)
                        {
                            return iRes.From;
                        }

                        LogService.Log.LogError(
                            "GetInAtInstruction.GetValues", 
                            string.Format("Index out of bounds for neuron {0}, index: {1}!", iNeuron.ID, iInt.Value));
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetInAtInstruction.GetValues", 
                            "Second argument should be an IntNeuron!");
                    }
                }
                else
                {
                    LogService.Log.LogError("GetInAtInstruction.GetValues", "Invalid first argument");
                }
            }
            else
            {
                LogService.Log.LogError("GetInAtInstruction.GetValues", "No arguments specified");
            }

            return null;
        }
    }
}