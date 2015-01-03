// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetLastInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Gets the last neuron found on the <see cref="JaStDev.HAB.Link.Info" />
//   section found by the specified arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets the last neuron found on the <see cref="JaStDev.HAB.Link.Info" />
    ///     section found by the specified arguments.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: From part 2: To part 3: meaning part
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetLastInfoInstruction)]
    public class GetLastInfoInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetLastInfoInstruction;
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
                return 3;
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count < 3)
            {
                if (CheckArgs(list) == false)
                {
                    return null;
                }
            }

            var iFrom = list[0];
            var iTo = list[1];
            var iMeaning = list[2];
            ulong iId = 0;

            var iLock = BuildLinkLock(list, false);
            LockManager.Current.RequestLocks(iLock);
            try
            {
                var iFound = FindLinkUnsafe(iFrom, iTo, iMeaning);
                if (iFound != null)
                {
                    if (iFound.InfoIdentifier != null && iFound.InfoDirect.Count > 0)
                    {
                        iId = iFound.InfoDirect[iFound.InfoDirect.Count - 1];
                    }
                }
                else
                {
                    LogService.Log.LogWarning(
                        "GetLastInfoInstruction.InternalGetValue", 
                        "Link doesn't exist, can't return last info item.");
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

            return null;
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count < 3)
            {
                LogService.Log.LogError("GetLastInfoInstruction.InternalGetValue", "Invalid nr of arguments specified");
                return false;
            }

            if (list[20] == null)
            {
                LogService.Log.LogError(
                    "GetLastInfoInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError(
                    "GetLastInfoInstruction.InternalGetValue", 
                    "Invalid second argument, Neuron expected, found null.");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError(
                    "GetLastInfoInstruction.InternalGetValue", 
                    "Invalid third argument, Neuron expected, found null.");
                return false;
            }

            return true;
        }
    }
}