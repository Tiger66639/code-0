// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeParentInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Changes a a parent of an item to a new parent. When the item was not
//   found, <see langword="false" /> is returned, if the replace succeeded,
//   <see langword="true" /> is returned. If the operation failed,
//   <see langword="null" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes a a parent of an item to a new parent. When the item was not
    ///     found, <see langword="false" /> is returned, if the replace succeeded,
    ///     <see langword="true" /> is returned. If the operation failed,
    ///     <see langword="null" /> is returned.
    /// </summary>
    /// <remarks>
    ///     1: the cluster. 2: The child to replace 3: the value to replace the
    ///     previous item with.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChangeParentInstruction)]
    public class ChangeParentInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeParentInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChangeParentInstruction;
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

        /// <summary>replaces the item.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(list != null);

            if (list.Count >= 3)
            {
                if (CheckArgs(list) == false)
                {
                    return null;
                }

                var iLock = GetLock(list);

                    // we need to lock all the items at the same time, otherwise we are not thread save, hence the overlapping lock on all items.
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iParents = (ClusterList)list[0].ClusteredByIdentifier;
                    var iIndex = iParents.IndexOf(list[1].ID);
                    if (iIndex > -1)
                    {
                        iParents.ChangeUnsafe(iIndex, (NeuronCluster)list[1], (NeuronCluster)list[2]);
                        return Brain.Current.TrueNeuron;
                    }
                    else
                    {
                        return Brain.Current.FalseNeuron;
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iLock);
                    list[0].IsChanged = true;
                    list[1].IsChanged = true;
                    list[2].IsChanged = true;
                }
            }

            LogService.Log.LogError("ChangeParentInstruction.Execute", "Invalid nr of arguments specified");
            return null;
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            if (list[1] == null)
            {
                LogService.Log.LogError("ChangeParentInstruction.Execute", "old parent is null (second arg).");
                return false;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError("ChangeParentInstruction.Execute", "new parent is null (third arg).");
                return false;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError("ChangeParentInstruction.Execute", "child is null (first arg).");
            }

            return true;
        }

        /// <summary>The get lock.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList GetLock(System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = LockRequestList.Create();
            var iItem = LockRequestInfo.Create();
            iItem.Neuron = list[0];
            iItem.Level = LockLevel.Parents;
            iItem.Writeable = true;
            iRes.Add(iItem);

            for (var i = 1; i < list.Count; i++)
            {
                iItem = LockRequestInfo.Create();
                iItem.Neuron = list[i];
                iItem.Level = LockLevel.Children;
                iItem.Writeable = true;
                iRes.Add(iItem);
            }

            return iRes;
        }
    }
}