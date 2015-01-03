// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetNextInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the next neuron found on the <see cref="JaStDev.HAB.Link.Info" />
//   section found by the specified arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the next neuron found on the <see cref="JaStDev.HAB.Link.Info" />
    ///     section found by the specified arguments.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: From part 2: To part 3: meaning part 4: prev item
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetNextInfoInstruction)]
    public class GetNextInfoInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetNextInfoInstruction;
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
            if (list.Count >= 4)
            {
                var iFrom = list[0];
                if (iFrom != null)
                {
                    var iTo = list[1];
                    if (iTo != null)
                    {
                        var iMeaning = list[2];
                        if (iMeaning != null)
                        {
                            ulong iId = 0;
                            var iLock = LockRequestList.Create();
                            BuildLinkLocks(iLock, list[0], list[1], false);
                            LockManager.Current.RequestLocks(iLock);
                            try
                            {
                                var iFound = Link.Find(iFrom, iTo, iMeaning);
                                if (iFound != null)
                                {
                                    if (iFound.InfoIdentifier != null)
                                    {
                                        // don't try to search if there is no list, don't need to create it.
                                        var iPrev = list[3];
                                        if (iPrev != null)
                                        {
                                            var iIndex = iFound.InfoDirect.IndexOf(iPrev.ID);
                                            if (iIndex > -1 && iIndex < iFound.InfoDirect.Count - 1)
                                            {
                                                iId = iFound.InfoDirect[iIndex + 1];
                                            }
                                        }
                                        else
                                        {
                                            LogService.Log.LogError(
                                                "GetNextInfoInstruction.InternalGetValue", 
                                                "Invalid 4th argument, Neuron expected, found null.");
                                        }
                                    }
                                }
                                else
                                {
                                    LogService.Log.LogWarning(
                                        "GetNextInfoInstruction.InternalGetValue", 
                                        "Link doesn't exist, can't return first info item.");
                                }
                            }
                            finally
                            {
                                LockManager.Current.ReleaseLocks(iLock);
                            }

                            if (iId > 0)
                            {
                                return Brain.Current[iId];
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "GetNextInfoInstruction.InternalGetValue", 
                                "Invalid third argument, Neuron expected, found null.");
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetNextInfoInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "GetNextInfoInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetNextInfoInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }

            return null;
        }
    }
}