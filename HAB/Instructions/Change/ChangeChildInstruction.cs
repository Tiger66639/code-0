// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeChildInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Changes a child in a cluster list. When the item was not found,
//   <see langword="false" /> is returned, if the replace succeeded,
//   <see langword="true" /> is returned. If the operation failed,
//   <see langword="null" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Changes a child in a cluster list. When the item was not found,
    ///     <see langword="false" /> is returned, if the replace succeeded,
    ///     <see langword="true" /> is returned. If the operation failed,
    ///     <see langword="null" /> is returned.
    /// </summary>
    /// <remarks>
    ///     1: the cluster. 2: The child to replace 3: the value to replace the
    ///     previous item with.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ChangeChildInstruction)]
    public class ChangeChildInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeChildInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ChangeChildInstruction;
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
                var iCluster = CheckArgs(list);
                if (iCluster == null)
                {
                    return null;
                }

                if (list[2].ID == TempId)
                {
                    // make certain that the new item is no longer a temp when we add it to the cluster.
                    Brain.Current.Add(list[2]);
                }

                var iLock = BuildClusterLock(list, true);

                    // we need to lock all the items at the same time, otherwise we are not thread save, hence the overlapping lock on all items.
                LockManager.Current.RequestLocks(iLock);
                try
                {
                    var iList = iCluster.ChildrenDirect;
                    var iIndex = iList.IndexOf(list[1].ID); // we can call directly cause everything is locked anyway.
                    if (iIndex > -1)
                    {
                        iList.ChangeUnsafe(iIndex, list[1], list[2]);
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
                    iCluster.IsChanged = true;
                    list[1].IsChanged = true;
                    list[2].IsChanged = true;
                }
            }

            LogService.Log.LogError("ChangeChildInstruction.Execute", "Invalid nr of arguments specified");
            return null;
        }

        /// <summary>The check args.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster CheckArgs(System.Collections.Generic.IList<Neuron> list)
        {
            NeuronCluster iRes = null;
            if (list[1] == null)
            {
                LogService.Log.LogError("ChangeChildInstruction.Execute", "old child is null (second arg).");
                return null;
            }

            if (list[2] == null)
            {
                LogService.Log.LogError("ChangeChildInstruction.Execute", "new child is null (third arg).");
                return null;
            }

            if (list[0] == null)
            {
                LogService.Log.LogError("ChangeChildInstruction.Execute", "cluster is null (first arg).");
            }
            else
            {
                iRes = list[0] as NeuronCluster;
                if (iRes == null)
                {
                    LogService.Log.LogError("ChangeChildInstruction.Execute", "cluster expected (first arg).");
                }
            }

            return iRes;
        }

        /// <summary>The get lock.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private System.Collections.Generic.IEnumerable<LockRequestInfo> GetLock(System.Collections.Generic.IList<Neuron> list)
        {
            var iRes = LockRequestList.Create();
            var iItem = LockRequestInfo.Create();
            iItem.Neuron = list[0];
            iItem.Level = LockLevel.Children;
            iItem.Writeable = true;
            iRes.Add(iItem);

            for (var i = 1; i < list.Count; i++)
            {
                iItem = LockRequestInfo.Create();
                iItem.Neuron = list[i];
                iItem.Level = LockLevel.Parents;
                iItem.Writeable = true;
                iRes.Add(iItem);
            }

            return iRes;
        }
    }
}