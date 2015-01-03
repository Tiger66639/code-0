// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Copies info information from one link to another. links are defined using
//   from/to/meaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Copies info information from one link to another. links are defined using
    ///     from/to/meaning.
    /// </summary>
    /// <remarks>
    ///     args: 1: from target 2: to target 3: meaning target 4: index to insert at
    ///     target. (when <see langword="null" /> or not an <see langword="int" /> ->
    ///     adds at the end). 5: from source 6: to source 7: meaning source 8: start
    ///     pos in source (copy from) when not an <see langword="int" /> -> 0 is used
    ///     (so Predefined.Empty can be used) 9: end pos in source (copy untill) when
    ///     not an <see langword="int" /> -> Info.count is used (so Predefined.Empty
    ///     can be used)
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CopyInfoInstruction)]
    public class CopyInfoInstruction : Instruction
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
                return (ulong)PredefinedNeurons.CopyInfoInstruction;
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
                return 9;
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
            if (args != null && args.Count >= 9)
            {
                var iLock = LockRequestList.Create();
                BuildLinkLocks(iLock, args[0], args[1], true);
                BuildLinkLocks(iLock, args[5], args[6], false);
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
                                CopyInfo(iSource.InfoDirect, iStart, iEnd, iTarget.InfoDirect, iInsertAt);
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "CopyInfoInstruction.Execute", 
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
            }
            else
            {
                LogService.Log.LogError("CopyInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>Performs the actual copy operation.</summary>
        /// <param name="from">copy from.</param>
        /// <param name="start">The start index <paramref name="to"/> begin copying<paramref name="from"/></param>
        /// <param name="end">Copy untill this index</param>
        /// <param name="to">Cop to this link.</param>
        /// <param name="insertAt">Start inserting at this pos (-1 means add).</param>
        protected void CopyInfo(LinkInfoList from, int start, int end, LinkInfoList to, int insertAt)
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

        /// <summary>Gets the range to copy from the arguments.</summary>
        /// <param name="args">The args.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="max">The max.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected bool GetRange(System.Collections.Generic.IList<Neuron> args, out int start, out int end, int max)
        {
            start = 0;
            end = max;
            var iTemp = args[6] as IntNeuron;
            if (iTemp != null)
            {
                start = iTemp.Value;
                iTemp = args[7] as IntNeuron;
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
            }

            return true;
        }

        /// <summary>Gets the link including proper checking and error handling.</summary>
        /// <param name="from">From part of link</param>
        /// <param name="to">To part of link</param>
        /// <param name="meaning">The meaning of the link</param>
        /// <param name="name">The text <paramref name="to"/> use for error reporting.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        protected Link GetLink(Neuron from, Neuron to, Neuron meaning, string name)
        {
            if (from == null)
            {
                LogService.Log.LogError(TypeOfNeuron + ".GetLink", string.Format("From part of {0} is null", name));
                return null;
            }

            if (to == null)
            {
                LogService.Log.LogError(TypeOfNeuron + ".GetLink", string.Format("To part of {0} is null", name));
                return null;
            }

            if (meaning == null)
            {
                LogService.Log.LogError(TypeOfNeuron + ".GetLink", string.Format("Meaning part of {0} is null", name));
                return null;
            }

            return Link.Find(from, to, meaning);
        }
    }
}