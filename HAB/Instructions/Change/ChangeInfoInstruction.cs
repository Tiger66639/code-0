// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeInfoInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Changes an item on an info list. When the item was not found,
//   <see langword="false" /> is returned, if the replace succeeded,
//   <see langword="true" /> is returned. If the operation failed,
//   <see langword="null" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes an item on an info list. When the item was not found,
    ///     <see langword="false" /> is returned, if the replace succeeded,
    ///     <see langword="true" /> is returned. If the operation failed,
    ///     <see langword="null" /> is returned.
    /// </summary>
    /// <remarks>
    ///     1: From part 2: to part 3: meaning part 4: old info item 5: new info item
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChangeInfoInstruction)]
    public class ChangeInfoInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeInfoInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChangeInfoInstruction;
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

        /// <summary>replaces the item.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(list != null);

            if (CheckArgs(list) == false)
            {
                return null;
            }

            try
            {
                var iLock = BuildInfoLock(list, true);

                    // we need to lock all the items at the same time, otherwise we are not thread save, hence the overlapping lock on all items.
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iLink = FindLinkUnsafe(list[0], list[1], list[2]);
                    if (iLink != null)
                    {
                        var iIndex = iLink.InfoDirect.IndexOf(list[3].ID);
                        if (iIndex > -1)
                        {
                            iLink.InfoDirect.ChangeUnsafe(iIndex, list[3], list[4]);
                            return Brain.Current.TrueNeuron;
                        }
                        else
                        {
                            return Brain.Current.FalseNeuron;
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ChangeInfoInstruction.Execute", 
                            string.Format("Link not found: to={0}, from={1}, meaning={2}.", list[0], list[1], list[2]));
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                }
            }
            finally
            {
                list[0].IsChanged = true;
                list[1].IsChanged = true;
                list[3].IsChanged = true;
                list[4].IsChanged = true;
            }

            return null;
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = true;

            if (list.Count < 5)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "Invalid nr of arguments specified");
                return false;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "From part is null (first arg).");
                iRes = false;
            }

            if (list[1] == null)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "To part is null (second arg).");
                iRes = false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "meaning is null (third arg).");
                iRes = false;
            }

            if (list[3] == null)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "old info item is null (fourth arg).");
                iRes = false;
            }

            if (list[4] == null)
            {
                LogService.Log.LogError("ChangeInfoInstruction.Execute", "new info item is null (fifth arg).");
                iRes = false;
            }

            return iRes;
        }
    }
}