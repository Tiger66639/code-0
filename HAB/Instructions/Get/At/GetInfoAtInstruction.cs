// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInfoAtInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the neuron at the specified index in the list of info neurons of
//   a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the neuron at the specified index in the list of info neurons of
    ///     a link.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The from part of the link</description>
    ///         </item>
    ///         <item>
    ///             <description>2: The To part of the link</description>
    ///         </item>
    ///         <item>
    ///             <description>3: The meaning of the link</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 4: An IntNeuron, which contains the index position
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetInfoAtInstruction)]
    public class GetInfoAtInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoAtInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetInfoAtInstruction;
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
                return 4;
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
                var iFrom = list[0];
                var iTo = list[1];
                var iMeaning = list[2];
                var iInt = GetAsInt(list[3]);
                if (iFrom != null && iTo != null && iMeaning != null)
                {
                    if (iInt != null)
                    {
                        var ilink = Link.Find(iFrom, iTo, iMeaning);
                        if (ilink != null)
                        {
                            ulong iRes = 0;
                            var iInfo = ilink.Info;
                            iInfo.Lock();
                            try
                            {
                                if (iInfo.Count > iInt.Value && iInt.Value >= 0)
                                {
                                    iRes = iInfo[iInt.Value];
                                }
                            }
                            finally
                            {
                                iInfo.Unlock();
                            }

                            if (iRes != 0)
                            {
                                return Brain.Current[iRes];
                            }

                            LogService.Log.LogError(
                                "GetInfoAtInstruction.GetValues", 
                                string.Format("Index out of bounds: {0}!", iInt.Value));
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetInfoAtInstruction.GetValues", 
                                string.Format(
                                    "Could not find link (from: {0}, to: {1}, meaning: {2} !", 
                                    iFrom, 
                                    iTo, 
                                    iMeaning));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetInfoAtInstruction.GetValues", 
                            "4th argument should be an IntNeuron!");
                    }
                }
                else
                {
                    LogService.Log.LogError("GetInfoAtInstruction.GetValues", "Invalid link coordinates!");
                }
            }
            else
            {
                LogService.Log.LogError("GetInfoAtInstruction.GetValues", "No arguments specified");
            }

            return null;
        }
    }
}