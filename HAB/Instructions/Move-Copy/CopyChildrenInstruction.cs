// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyChildrenInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Copies the contents of a list from one neuron to another neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Copies the contents of a list from one neuron to another neuron.
    /// </summary>
    /// <remarks>
    ///     Arguments: 1: the neuron cluster to copy to 2: the index at which to
    ///     start copying (Empty can be used) 3: the neuron cluster to copy from 4:
    ///     start index copy from when not an <see langword="int" /> -> 0 is used (so
    ///     Predefined.Empty can be used) 5: end index copy from when not an
    ///     <see langword="int" /> -> Childrend.count is used (so Predefined.Empty can
    ///     be used)
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CopyChildrenInstruction)]
    public class CopyChildrenInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CopyChildrenInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CopyChildrenInstruction;
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

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            if (args != null && args.Count >= 5)
            {
                int iStartIndex;
                int iEndIndex;
                var iInsertAt = -1; // stores where the insert should start at the target.

                var iTo = args[0] as NeuronCluster;
                if (iTo != null)
                {
                    var iTempInt = args[1] as IntNeuron;
                    if (iTempInt != null)
                    {
                        iInsertAt = iTempInt.Value;
                    }

                    var iFrom = args[2] as NeuronCluster;
                    if (iFrom != null)
                    {
                        System.Collections.Generic.IList<ulong> iToList = null;
                        using (IDListAccessor iChildren = iFrom.ChildrenW)
                        {
                            iChildren.Lock();
                            try
                            {
                                if (iFrom == iTo)
                                {
                                    iToList = iFrom.ChildrenDirect;
                                }
                                else
                                {
                                    iToList = Factories.Default.IDLists.GetBuffer();

                                        // if we copy to another list, we firest copy to a temp, this is to prevent deadlocks.
                                }

                                if (GetRange(args, out iStartIndex, out iEndIndex, iFrom.ChildrenDirect.Count))
                                {
                                    if (iFrom.ChildrenDirect.Count <= iEndIndex)
                                    {
                                        CopyChildren(iFrom.ChildrenDirect, iStartIndex, iEndIndex, iToList, iInsertAt);
                                    }
                                    else
                                    {
                                        LogService.Log.LogError(
                                            "CopyChildrenInstruction.Execute", 
                                            string.Format("end pos '{0}' is bigger than list.", iEndIndex));
                                    }
                                }
                            }
                            finally
                            {
                                iChildren.Unlock();
                            }
                        }

                        if (iFrom != iTo)
                        {
                            using (var iChildren = iTo.ChildrenW)
                            {
                                var iToRelease = iChildren.Lock(iToList);
                                try
                                {
                                    if (GetRange(args, out iStartIndex, out iEndIndex, iFrom.ChildrenDirect.Count))
                                    {
                                        if (iFrom.ChildrenDirect.Count <= iEndIndex)
                                        {
                                            CopyChildren(iToList, iStartIndex, iEndIndex, iTo.ChildrenDirect, iInsertAt);
                                        }
                                        else
                                        {
                                            LogService.Log.LogError(
                                                "CopyChildrenInstruction.Execute", 
                                                string.Format("end pos '{0}' is bigger than list.", iEndIndex));
                                        }
                                    }
                                }
                                finally
                                {
                                    iChildren.Unlock(iToRelease);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "CopyChildrenInstruction.CheckFirstArgsForClusters", 
                            "Invalid 'Copy from' argument (second arg)");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "CopyChildrenInstruction.CheckFirstArgsForClusters", 
                        "Invalid 'Copy to' argument (first arg)");
                }
            }
            else
            {
                LogService.Log.LogError("CopyChildrenInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Performs the actual copy operation.</summary>
        /// <remarks><see langword="virtual"/> so that descendents can do other things,
        ///     like deleting the items.</remarks>
        /// <param name="from">From.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="to">To.</param>
        /// <param name="insertAt">The insert at.</param>
        protected void CopyChildren(System.Collections.Generic.IList<ulong> from, 
            int start, 
            int end, System.Collections.Generic.IList<ulong> to, 
            int insertAt)
        {
            if (insertAt == -1)
            {
                while (start < end)
                {
                    to.Add(from[start]);
                    start++;
                }
            }
            else
            {
                while (start < end)
                {
                    to.Insert(insertAt++, from[start]);
                    start++;
                }
            }
        }

        /// <summary>Checks the <paramref name="args"/> and returns the<paramref name="start"/> and <paramref name="end"/> index to use.</summary>
        /// <param name="args">The args passed along with execute.</param>
        /// <param name="start">The start index of the list at which copying should start.</param>
        /// <param name="end">The end index before wich copying should stop (so if this is 5, up
        ///     untill 4 will be copied).</param>
        /// <param name="max">The max.</param>
        /// <returns>True if everything is ok.</returns>
        protected bool GetRange(System.Collections.Generic.IList<Neuron> args, out int start, out int end, int max)
        {
            start = 0;
            end = max;
            var iTemp = args[3] as IntNeuron;
            if (iTemp != null)
            {
                start = iTemp.Value;
            }

            iTemp = args[4] as IntNeuron;
            if (iTemp != null)
            {
                end = iTemp.Value;
            }

            if (end < start)
            {
                LogService.Log.LogWarning(
                    TypeOfNeuron + ".CheckArgs", 
                    string.Format("end pos '{0}' is smaller than start '{1}'.", end, start));
            }

            return true;
        }
    }
}