// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterList.cs" company="">
//   
// </copyright>
// <summary>
//   A List that contains all the ID's of the <see cref="NeuronCluster" /> s
//   that own a <see cref="Neuron" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A List that contains all the ID's of the <see cref="NeuronCluster" /> s
    ///     that own a <see cref="Neuron" /> .
    /// </summary>
    /// <remarks>
    ///     Provides extra functionality that makes certain that the
    ///     <see cref="JaStDev.HAB.NeuronCluster.Children" /> lists are also kept in
    ///     sync.
    /// </remarks>
    public class ClusterList : OwnedBrainList
    {
        /// <summary>Initializes a new instance of the <see cref="ClusterList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        internal ClusterList(Neuron owner)
            : base(owner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ClusterList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="capacity">The capacity.</param>
        internal ClusterList(Neuron owner, int capacity)
            : base(owner, capacity)
        {
        }

        /// <summary>changes a parentref from <paramref name="old"/> to the new value. Both<paramref name="old"/> and new are internally updated so that they
        ///     point to the new objects.</summary>
        /// <param name="index"></param>
        /// <param name="old"></param>
        /// <param name="value"></param>
        internal void ChangeUnsafe(int index, NeuronCluster old, NeuronCluster value)
        {
            if (value != null && old != null)
            {
                value.AddChildDirect(Owner);
                old.RemoveChildUnsafe(Owner); // succeed or not, don't care, replacing the item,
                base.InternalReplace(value, index);
            }
        }

        #region overrides

        /// <summary>
        ///     Removes all items from the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> . Preferebly
        ///     use Clear(list) instead, which is saver
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     The <see cref="System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public override void Clear()
        {
            foreach (var i in this)
            {
                if (i != Neuron.EmptyId)
                {
                    // could be if the neuron got deleted in another thread.
                    var iCluster = (NeuronCluster)Brain.Current[i];
                    iCluster.RemoveChildUnsafe(Owner);
                }
                else
                {
                    LogService.Log.LogWarning(
                        "ClusterList.Clear", 
                        string.Format(
                            "Neuron {0} appears to be clustered by an invalid cluster (id=0), the database might be corrupted.", 
                            i));
                }
            }

            base.Clear();
        }

        /// <summary>
        ///     Clears the list in an unsafe way, without locking the clusters and
        ///     without updating the reference to the cluster. When this is called, it
        ///     is presumed that the objects are already locked and therefor loaded
        ///     into mem.
        /// </summary>
        internal void ClearDirect()
        {
            base.Clear();
        }

        /// <summary>clears the list of items, doesn't have to get the each neuron from
        ///     cache. Use this instead of <see cref="BrainList.Clear"/></summary>
        /// <param name="items"></param>
        public override void Clear(System.Collections.Generic.List<Neuron> items)
        {
            foreach (NeuronCluster i in items)
            {
                i.RemoveChildUnsafe(Owner);
            }

            base.Clear();
        }

        /// <summary>
        ///     Gets the accessor for the list.
        /// </summary>
        /// <returns>
        ///     By default, this returns a <see cref="NeuronsAccessor" /> , but can be
        ///     changed in any descendent.
        /// </returns>
        protected internal override Accessor GetAccessor()
        {
            return new ClustersAccessor(this, Owner, false);
        }

        /// <summary>Performs the actual insertion.</summary>
        /// <remarks>Makes certain that everything is regisetered + raises the appropriate
        ///     events.</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InternalInsert(int index, Neuron item)
        {
            var iItem = (NeuronCluster)item; // this will raise an exception if not of correct type.

            iItem.AddChildDirect(Owner);
            base.InternalInsert(index, item);
        }

        /// <summary>Removes the <paramref name="item"/> at the specified index.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalRemove(Neuron item, int index)
        {
            var iItem = item as NeuronCluster;

                // dont' raise exception, while removing, we need maximum flexibility here.
            if (iItem != null)
            {
                iItem.RemoveChildUnsafe(Owner);
            }

            return base.InternalRemove(item, index);
            
        }

        /// <summary>Called when a neuron gets repaced by a new one in the list.</summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        protected override void InternalReplace(Neuron value, int index)
        {
            var iOld = Brain.Current[this[index]] as NeuronCluster;
            var iItem = value as NeuronCluster;
            if (iItem != null && iOld != null)
            {
                iItem.AddChildDirect(Owner);
                iOld.RemoveChildUnsafe(Owner); // succeed or not, don't care, replacing the item,
                base.InternalReplace(value, index);
            }
        }

        /// <summary>performs an insert and raises the event but doesn't change anything in
        ///     the neuron being added.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        internal void InsertDirect(int index, Neuron item)
        {
            base.InternalInsert(index, item);
        }

        /// <summary>performs a remove and raises the event but doesn't change anything in
        ///     the neuron being added.</summary>
        /// <param name="toRemove">The to Remove.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool RemoveDirect(NeuronCluster toRemove)
        {
            var iIndex = IndexOf(toRemove.ID);
            if (iIndex > -1)
            {
                return base.InternalRemove(toRemove, iIndex);
            }

            return false;
        }

        #endregion
    }
}