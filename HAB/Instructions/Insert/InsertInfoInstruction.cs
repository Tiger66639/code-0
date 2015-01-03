// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Inserts an item into an info list of a link.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Inserts an item into an info list of a link.
    /// </summary>
    /// <remarks>
    ///     Args: 1: from part of link 2: to part of link 3: meaning part of link 4:
    ///     item to insert 5: position at which item should be inserted.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.InsertInfoInstruction)]
    public class InsertInfoInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InsertInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.InsertInfoInstruction;
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
            if (args != null && args.Count >= 3)
            {
                int iIndex;
                if (CheckArgs(args, out iIndex) == false)
                {
                    return;
                }

                var iLock = BuildLock(args);
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iLink = FindLinkUnsafe(args[0], args[1], args[2]);
                    if (iLink != null)
                    {
                        if (iIndex != -1)
                        {
                            iLink.InfoDirect.InsertInfoUnsafe(iIndex, args[3]);
                        }
                        else
                        {
                            iLink.InfoDirect.AddItemUnsafe(args[3]);
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "AddInfoInstruction.Execute", 
                            string.Format(
                                "Could not find link from={0}, to={1}, meaning={2}.  Failed to set info={3}", 
                                args[0], 
                                args[1], 
                                args[2], 
                                args[3]));
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }

                args[0].IsChanged = true;
                args[1].IsChanged = true;
                for (var i = 3; i < args.Count; i++)
                {
                    args[i].IsChanged = true;
                }
            }
            else
            {
                LogService.Log.LogError("InsertInfoInstruction.Execute", "Invalid nr of arguments specified");
            }
        }

        /// <summary>The build lock.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildLock(System.Collections.Generic.IList<Neuron> args)
        {
            var iRes = LockRequestList.Create();

            var iReq = LockRequestInfo.Create();
            iReq.Neuron = args[0];
            iReq.Level = LockLevel.LinksOut;
            iReq.Writeable = false;
            iRes.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = args[1];
            iReq.Level = LockLevel.LinksIn;
            iReq.Writeable = false;
            iRes.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = args[2];
            iReq.Level = LockLevel.Value;
            iReq.Writeable = false;
            iRes.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = args[3];
            iReq.Level = LockLevel.Value;
            iReq.Writeable = true;
            iRes.Add(iReq);

            return iRes;
        }

        /// <summary>The check args.</summary>
        /// <param name="args">The args.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> args, out int index)
        {
            index = -1;
            if (args.Count >= 5)
            {
                var iRes = true;
                if (args[0] == null)
                {
                    LogService.Log.LogError("InsertInfoInstruction.CheckArgsForLink", "From part is null (first arg).");
                    iRes = false;
                }

                if (args[1] == null)
                {
                    LogService.Log.LogError("InsertInfoInstruction.CheckArgsForLink", "To part is null (second arg).");
                    iRes = false;
                }

                if (args[2] == null)
                {
                    LogService.Log.LogError(
                        "InsertInfoInstruction.CheckArgsForLink", 
                        "meaning part is null (third arg).");
                    iRes = false;
                }

                if (args[3] == null)
                {
                    LogService.Log.LogError(
                        "InsertInfoInstruction.CheckArgsForLink", 
                        "item to insert is null (fourth arg).");
                    iRes = false;
                }

                if (!(args[4] is IntNeuron))
                {
                    LogService.Log.LogError(
                        "InsertInfoInstruction.CheckArgsForLink", 
                        "insert position is invalid (fifth arg).");
                    iRes = false;
                }

                if (iRes)
                {
                    index = ((IntNeuron)args[4]).Value;
                }

                return iRes;
            }

            LogService.Log.LogError("InsertInfoInstruction.CheckArgsForChildren", "Invalid nr of arguments specified");
            return false;
        }
    }
}