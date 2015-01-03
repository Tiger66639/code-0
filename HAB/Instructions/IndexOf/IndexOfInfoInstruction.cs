// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexOfInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the index position of a neuron in an info list of a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the index position of a neuron in an info list of a link.
    /// </summary>
    /// <remarks>
    ///     arg: 1: from part of link 2: to part of link 3: meaning part of link 4:
    ///     neuron to search for.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IndexOfInfoInstruction)]
    public class IndexOfInfoInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IndexOfInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IndexOfInfoInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 4)
            {
                if (CheckArgs(list))
                {
                    var iLock = BuildLinkLock(list, false);
                    LockManager.Current.RequestLocks(iLock);
                    try
                    {
                        var iLink = FindLinkUnsafe(list[0], list[1], list[2]);
                        if (iLink != null)
                        {
                            if (iLink.InfoIdentifier != null)
                            {
                                if (list[3] != null)
                                {
                                    return iLink.InfoDirect.IndexOf(list[3].ID);
                                }
                                else
                                {
                                    LogService.Log.LogError(
                                        "IndexOfInfoInstruction.InternalGetValue", 
                                        string.Format("neuron to search is null"));
                                }
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "IndexOfInfoInstruction.GetLink", 
                                string.Format(
                                    "Failed to find link from={0}, to={1}, meanin={2}.", 
                                    list[0], 
                                    list[1], 
                                    list[2]));
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iLock);
                    }
                }
            }
            else
            {
                LogService.Log.LogError("IndexOfInfoInstruction.InternalGetValue", "No arguments specified");
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

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list[0] == null)
            {
                LogService.Log.LogError("IndexOfInfoInstruction.GetLink", string.Format("From part of is null"));
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError("IndexOfInfoInstruction.GetLink", string.Format("To part is null"));
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError("IndexOfInfoInstruction.GetLink", string.Format("Meaning part is null"));
                return false;
            }

            return true;
        }
    }
}