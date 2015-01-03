// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInfoFilteredInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the info neurons on the specified link that pass the filter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the info neurons on the specified link that pass the filter.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 5</para>
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
    ///         <item>
    ///             <description>
    ///                 a reference to the variable that will store the info item that needs to
    ///                 by the filter.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a reference to a result statement that is used as the filter expression.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetInfoFilteredInstruction)]
    public class GetInfoFilteredInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoFilteredInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetInfoFilteredInstruction;
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
                return 5;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 3)
            {
                if (CheckArgs(list) == false)
                {
                    return;
                }

                var iFrom = list[0];
                var iTo = list[1];
                var iMeaning = list[2];
                var iVar = list[3] as Variable;
                var iEx = list[4] as ResultExpression;

                var iRes = processor.Mem.ArgumentStack.Peek();
                var iMemFac = Factories.Default;
                System.Collections.Generic.List<ulong> iIds = null;
                var iLock = BuildLinkLock(list, false);
                LockManager.Current.RequestLocks(iLock);

                    // we need to keep the link the same between finding and getting the info.
                try
                {
                    var iFound = FindLinkUnsafe(iFrom, iTo, iMeaning);
                    if (iFound != null)
                    {
                        using (IDListAccessor iInfo = iFound.Info)
                        {
                            iIds = iMemFac.IDLists.GetBuffer(iInfo.CountUnsafe);
                            iIds.AddRange(iInfo);
                        }
                    }
                    else
                    {
                        LogService.Log.LogWarning(
                            "GetInfoFilteredInstruction.InternalGetValue", 
                            "Link doesn't exist, can't return first info item.");
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }

                if (iIds != null)
                {
                    foreach (var i in iIds)
                    {
                        // has to be done outside of lock cause we are accesing the cache.
                        var iItem = Brain.Current[i];
                        iVar.StoreValue(iItem, processor);
                        var iResOk = false;

                        // init to false, so we can handle the case in which there is no result.
                        try
                        {
                            foreach (var iResItem in SolveResultExpNoStackChange(iEx, processor))
                            {
                                if (iResItem.ID != (ulong)PredefinedNeurons.True)
                                {
                                    iResOk = false; // if there is is anything but a true, we're in trouble.
                                    break;
                                }

                                iResOk = true; // if there is a true, we have an ok.
                            }
                        }
                        finally
                        {
                            processor.Mem.ArgumentStack.Pop();

                                // we used the stack to get the result, so don't forget to free it again so that it can be reused.
                        }

                        if (iResOk)
                        {
                            iRes.Add(iItem);
                        }
                    }

                    iMemFac.IDLists.Recycle(iIds);
                }
            }
            else
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list[0] == null)
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid first argument, Neuron expected, found null.");
                return false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid second argument, Neuron expected, found null.");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid third argument, Neuron expected, found null.");
                return false;
            }

            if (!(list[3] is Variable))
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid fourth argument, variable expected.");
                return false;
            }

            if (!(list[4] is ResultExpression))
            {
                LogService.Log.LogError(
                    "GetInfoFilteredInstruction.InternalGetValue", 
                    "Invalid fifth argument, result statement expected.");
                return false;
            }

            return true;
        }
    }
}