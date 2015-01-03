// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the info neurons on the specified link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the info neurons on the specified link.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 3</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>from part of link</description>
    ///         </item>
    ///         <item>
    ///             <description>to part of link</description>
    ///         </item>
    ///         <item>
    ///             <description>meaning part of link.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetInfoInstruction)]
    public class GetInfoInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetInfoInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CheckArgs(list) == false)
            {
                return;
            }

            var iFrom = list[0];
            var iTo = list[1];
            var iMeaning = list[2];

            var iRes = processor.Mem.ArgumentStack.Peek();
            var iMemFac = Factories.Default;
            System.Collections.Generic.List<ulong> iIds = null;
            var iLock = BuildLinkLock(list, false);

            // we need to keep the link the same between finding and getting the info.
            LockManager.Current.RequestLocks(iLock);
            try
            {
                var iFound = FindLinkUnsafe(iFrom, iTo, iMeaning);
                if (iFound != null)
                {
                    if (iFound.InfoIdentifier != null)
                    {
                        // don't need to create the list if empty
                        iIds = iMemFac.IDLists.GetBuffer(iFound.InfoDirect.Count);
                        iIds.AddRange(iFound.InfoDirect);
                    }
                }
                else
                {
                    LogService.Log.LogWarning(
                        "GetInfoInstruction.InternalGetValue", 
                        "Link doesn't exist, can't return first info item.");
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
            }

            if (iIds != null)
            {
                if (iRes.Capacity < iIds.Count)
                {
                    iRes.Capacity = iIds.Count;
                }

                foreach (var i in iIds)
                {
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        // could have been deleted?
                        iRes.Add(iFound);
                    }
                }

                iMemFac.IDLists.Recycle(iIds);
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count < 3)
            {
                LogService.Log.LogError("GetInfoInstruction.InternalGetValue", "Invalid nr of arguments specified");
                return false;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError(
                    "GetInfoInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError(
                    "GetInfoInstruction.InternalGetValue", 
                    "Invalid second argument, Neuron expected, found null.");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError(
                    "GetInfoInstruction.InternalGetValue", 
                    "Invalid third argument, Neuron expected, found null.");
                return false;
            }

            return true;
        }
    }
}