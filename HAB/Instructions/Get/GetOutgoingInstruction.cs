// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetOutgoingInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns all the neurons that start from the specified neuron, with the
//   specified meaning(s).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns all the neurons that start from the specified neuron, with the
    ///     specified meaning(s).
    /// </summary>
    /// <remarks>
    ///     <para>Arguments: 2 or more</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>from part of link.</description>
    ///         </item>
    ///         <item>
    ///             <description>the first meaning part of link.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.GetOutgoingInstruction)]
    public class GetOutgoingInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetOutgoingInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GetOutgoingInstruction;
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

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 2)
            {
                var iFrom = list[0];
                if (iFrom != null)
                {
                    if (list[1] != null)
                    {
                        if (iFrom.LinksOutIdentifier != null)
                        {
                            var iMemFac = Factories.Default;
                            System.Collections.Generic.List<ulong> iIds = null;
                            var iRes = processor.Mem.ArgumentStack.Peek();
                            ListAccessor<Link> iLinksOut = iFrom.LinksOut;
                            iLinksOut.Lock();
                            try
                            {
                                var iCount = iLinksOut.CountUnsafe;
                                iIds = iMemFac.IDLists.GetBuffer(iCount);
                                if (iFrom.fSortedLinksOut == null)
                                {
                                    GetOutgoing(iLinksOut, list, iIds);
                                }
                                else
                                {
                                    GetOutgoing(iFrom.fSortedLinksOut, list, iIds);
                                }
                            }
                            finally
                            {
                                iLinksOut.Dispose(); // also unlocks
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
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "GetOutgoingInstruction.InternalGetValue", 
                            "Invalid second argument, Neuron expected, found null.");
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

        /// <summary>The get outgoing.</summary>
        /// <param name="source">The source.</param>
        /// <param name="list">The list.</param>
        /// <param name="result">The result.</param>
        private void GetOutgoing(System.Collections.Generic.Dictionary<ulong, System.Collections.Generic.Dictionary<ulong, Link>> source, System.Collections.Generic.IList<Neuron> list, System.Collections.Generic.List<ulong> result)
        {
            System.Collections.Generic.Dictionary<ulong, Link> iSub;
            for (var i = 1; i < list.Count; i++)
            {
                if (source.TryGetValue(list[i].ID, out iSub))
                {
                    foreach (var u in iSub)
                    {
                        result.Add(u.Key);
                    }
                }
            }
        }

        /// <summary>The get outgoing.</summary>
        /// <param name="source">The source.</param>
        /// <param name="list">The list.</param>
        /// <param name="results">The results.</param>
        private void GetOutgoing(
            ListAccessor<Link> source, System.Collections.Generic.IList<Neuron> list, System.Collections.Generic.List<ulong> results)
        {
            if (list.Count > 2)
            {
                // are there more then 1 meaning specified or not.
                for (var i = 1; i < list.Count; i++)
                {
                    foreach (var u in source.List)
                    {
                        if (u.MeaningID == list[i].ID)
                        {
                            results.Add(u.ToID);
                        }
                    }
                }
            }
            else
            {
                foreach (var i in source.List)
                {
                    if (i.MeaningID == list[1].ID)
                    {
                        results.Add(i.ToID);
                    }
                }
            }
        }
    }
}