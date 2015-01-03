// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoveChildrenInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Same as <see cref="CopyChildrenInstruction" /> but removes the children
//   in the from list after the copy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Same as <see cref="CopyChildrenInstruction" /> but removes the children
    ///     in the from list after the copy.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.MoveChildrenInstruction)]
    public class MoveInstruction : CopyChildrenInstruction
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
                return (ulong)PredefinedNeurons.MoveChildrenInstruction;
            }
        }

        #endregion

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
                        if (iFrom != iTo)
                        {
                            int iCount;
                            
                            using (var iChildren = iFrom.Children) iCount = iChildren.Count;
                            if (GetRange(args, out iStartIndex, out iEndIndex, iCount))
                            {
                                if (iCount <= iEndIndex)
                                {
                                    MoveChildren(iFrom, iStartIndex, iEndIndex, iTo, iInsertAt);
                                }
                                else
                                {
                                    LogService.Log.LogError(
                                        "MoveInstruction.Execute", 
                                        string.Format("end pos '{0}' is bigger than list.", iEndIndex));
                                }
                            }
                        }
                        else
                        {
                            using (var iChildren = iFrom.ChildrenW)
                            {
                                if (GetRange(args, out iStartIndex, out iEndIndex, iChildren.Count))
                                {
                                    iChildren.Move(iStartIndex, iEndIndex);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "MoveInstruction.CheckFirstArgsForClusters", 
                            "Invalid 'move from' argument (second arg)");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "MoveInstruction.CheckFirstArgsForClusters", 
                        "Invalid 'move to' argument (first arg)");
                }
            }
            else
            {
                LogService.Log.LogError("MoveInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Performs the actual copy operation.</summary>
        /// <remarks>Removes the child after each copy.</remarks>
        /// <param name="from">From.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="to">To.</param>
        /// <param name="insertAt">The insert at.</param>
        protected void MoveChildren(NeuronCluster from, int start, int end, NeuronCluster to, int insertAt)
        {
            using (IDListAccessor iFrom = from.ChildrenW)
            {
                var iToRelease = iFrom.LockAll();
                try
                {
                    var iStart = start;
                    while (iStart < end)
                    {
                        from.ChildrenDirect.RemoveAt(start);
                        iStart++;
                    }
                }
                finally
                {
                    iFrom.Unlock(iToRelease);
                }

                if (insertAt == -1)
                {
                    using (var iChildren = to.ChildrenW) iChildren.AddRange(iToRelease.GetRange(start, end - start));
                }
                else
                {
                    using (var iChildren = to.ChildrenW)
                        for (var i = start; i < end; i++)
                        {
                            iChildren.Insert(insertAt++, iToRelease[i]);
                        }
                }

                Factories.Default.NLists.Recycle(iToRelease);
            }
        }
    }
}