// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfoCountInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the number of neurons contained in the info section of a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the number of neurons contained in the info section of a link.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>1: The from part of the link.</description>
    ///         </item>
    ///         <item>
    ///             <description>2: the to part of the link.</description>
    ///         </item>
    ///         <item>
    ///             <description>3: the meaning of the link.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.InfoCountInstruction)]
    public class InfoCountInstruction : SingleResultInstruction, ICalculateInt
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InfoCountInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InfoCountInstruction;
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

        #region ICalculateInt Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public int CalculateInt(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CheckArgs(list) == false)
            {
                return -1;
            }

            var iFrom = list[0];
            var iTo = list[1];
            var iMeaning = list[2];

            var iLock = BuildLinkLock(list, false);
            LockManager.Current.RequestLocks(iLock);
            try
            {
                var iFound = FindLinkUnsafe(iFrom, iTo, iMeaning);
                if (iFound != null)
                {
                    if (iFound.InfoIdentifier != null)
                    {
                        return iFound.InfoDirect.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    LogService.Log.LogWarning(
                        "InfoCountInstruction.InternalGetValue", 
                        "Link doesn't exist, can't return Nr of items.");
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
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
            if (list.Count < 3)
            {
                LogService.Log.LogError("InfoCountInstruction.InternalGetValue", "Invalid nr of arguments specified");
                return false;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError(
                    "InfoCountInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError(
                    "InfoCountInstruction.InternalGetValue", 
                    "Invalid second argument, Neuron expected, found null.");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError(
                    "InfoCountInstruction.InternalGetValue", 
                    "Invalid third argument, Neuron expected, found null.");
                return false;
            }

            return true;
        }
    }
}