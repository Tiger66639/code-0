// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAllOutgoingInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the neurons that start from the specified neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the neurons that start from the specified neuron.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 1</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>from part of link.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetAllOutgoingInstruction)]
    public class GetAllOutgoingInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetAllOutgoingInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetAllOutgoingInstruction;
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
                return 1;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iFrom = list[0];
                if (iFrom != null)
                {
                    if (iFrom.LinksOutIdentifier != null)
                    {
                        var iMemFac = Factories.Default;
                        System.Collections.Generic.List<ulong> iIds;
                        var iRes = processor.Mem.ArgumentStack.Peek();
                        LockManager.Current.RequestLock(iFrom, LockLevel.LinksOut, false);
                        try
                        {
                            var iLinksOut = iFrom.LinksOutIdentifier;
                            iIds = iMemFac.IDLists.GetBuffer(iLinksOut.Count);
                            foreach (var i in iLinksOut)
                            {
                                iIds.Add(i.ToID);
                            }
                        }
                        finally
                        {
                            LockManager.Current.ReleaseLock(iFrom, LockLevel.LinksOut, false);
                        }

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
                else
                {
                    LogService.Log.LogError(
                        "GetOutgoingInstruction.InternalGetValue", 
                        "Invalid first argument, Neuron expected, found null.");
                }
            }
            else
            {
                LogService.Log.LogError("GetOutgoingInstruction.InternalGetValue", "Invalid nr of arguments specified");
            }
        }
    }
}