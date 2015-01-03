// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoveInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Same as <see cref="CopyInfoInstruction" /> , but removes the items from
//   the source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Same as <see cref="CopyInfoInstruction" /> , but removes the items from
    ///     the source.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.MoveInfoInstruction)]
    public class MoveInfoInstruction : CopyInfoInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CopyInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.MoveInfoInstruction;
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
            if (args != null && args.Count >= 9)
            {
                var iLock = LockRequestList.Create();
                BuildLinkLocks(iLock, args[0], args[1], true);
                BuildLinkLocks(iLock, args[5], args[6], true);
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iTarget = GetLink(args[0], args[1], args[2], "target");
                    var iSource = GetLink(args[5], args[6], args[7], "Source");
                    if (iTarget != null && iSource != null)
                    {
                        var iInsertAt = -1;
                        if (args[3] is IntNeuron)
                        {
                            iInsertAt = ((IntNeuron)args[3]).Value;
                            if (iInsertAt >= iTarget.InfoDirect.Count)
                            {
                                // if insertpos over the range, do an add.
                                iInsertAt = -1;
                            }
                        }

                        int iStart;
                        int iEnd;

                        if (GetRange(args, out iStart, out iEnd, iTarget.InfoDirect.Count))
                        {
                            if (iEnd <= iSource.InfoDirect.Count)
                            {
                                MoveInfo(iSource.InfoDirect, iStart, iEnd, iTarget.InfoDirect, iInsertAt);
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "MoveInfoInstruction.Execute", 
                                    string.Format("end pos '{0}' is bigger than list.", iEnd));
                            }
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }

                args[0].IsChanged = true;
                args[1].IsChanged = true;
                args[5].IsChanged = true;
                args[6].IsChanged = true;
            }
            else
            {
                LogService.Log.LogError("MoveInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Performs the actual copy operation.</summary>
        /// <param name="from">copy from.</param>
        /// <param name="start">The start index <paramref name="to"/> begin copying<paramref name="from"/></param>
        /// <param name="end">Copy untill this index</param>
        /// <param name="to">Cop to this link.</param>
        /// <param name="insertAt">Start inserting at this pos (-1 means add).</param>
        protected void MoveInfo(LinkInfoList from, int start, int end, LinkInfoList to, int insertAt)
        {
            if (insertAt == -1)
            {
                while (start < end)
                {
                    to.Add(from[start]);
                    from.RemoveAt(start);
                    end++;
                }
            }
            else
            {
                while (start < end)
                {
                    to.Insert(insertAt++, from[start]);
                    from.RemoveAt(start);
                    end++;
                }
            }
        }
    }
}