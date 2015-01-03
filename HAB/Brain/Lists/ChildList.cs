// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildList.cs" company="">
//   
// </copyright>
// <summary>
//   A list used by <see cref="NeuronCluster" /> s to store the children.
//   Note: always put an operation on this list in a lock on the list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A list used by <see cref="NeuronCluster" /> s to store the children.
    ///     Note: always put an operation on this list in a lock on the list.
    /// </summary>
    /// <remarks>
    ///     This list is not thread save, so ANY operation made on this list
    ///     (including foreach), should be performed within a lock statement. It is
    ///     not done on the list itself since it is impossible to reach all
    ///     situations (for instance indexOf is thread save, replace as well, but if
    ///     those 2 statements are done in sequence as in get the index of someting
    ///     and replace it, the operation is not thread save. So lock it.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="Neuron" /> .</typeparam>
    public class ChildList : OwnedBrainList
    {
        /// <summary>The change unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        internal void ChangeUnsafe(int index, Neuron old, Neuron value)
        {
            var iCluster = (NeuronCluster)Owner;
            System.Diagnostics.Debug.Assert(iCluster != null);
            if (value != null && old != null)
            {
                value.AddClusterUnsafe(iCluster);
                old.RemoveClusterUnsafe(iCluster); // succeed or not, don't care, replacing the item,
                base.InternalReplace(value, index);
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ChildList"/> class. Initializes a new instance of the <see cref="NeuronList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        internal ChildList(Neuron owner)
            : base(owner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ChildList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="capacity">The capacity.</param>
        internal ChildList(Neuron owner, int capacity)
            : base(owner, capacity)
        {
        }

        #endregion

        #region overrides

        /// <summary>
        ///     Gets the accessor for the list.
        /// </summary>
        /// <returns>
        ///     By default, this returns a <see cref="NeuronsAccessor" /> , but can be
        ///     changed in any descendent.
        /// </returns>
        protected internal override Accessor GetAccessor()
        {
            return new ChildrenAccessor(this, Owner, false);

                // always need read access, even if we only want to know a count.
        }

        /// <summary>Performs the actual insertion.</summary>
        /// <remarks>Makes certain that everything is regisetered + raises the appropriate
        ///     events.</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InternalInsert(int index, Neuron item)
        {
            var iCluster = (NeuronCluster)Owner;
            System.Diagnostics.Debug.Assert(iCluster != null);

            item.AddClusterUnsafe(iCluster);
            base.InternalInsert(index, item);
        }

        /// <summary>Removes the <paramref name="item"/> at the specified index.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalRemove(Neuron item, int index)
        {
            var iCluster = (NeuronCluster)Owner;
            System.Diagnostics.Debug.Assert(iCluster != null);
            if (item != null)
            {
                item.RemoveClusterUnsafe(iCluster);
            }

            return base.InternalRemove(item, index);
        }

        /// <summary>Called when a neuron gets repaced by a new one in the list.</summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        protected override void InternalReplace(Neuron value, int index)
        {
            var iCluster = (NeuronCluster)Owner;
            System.Diagnostics.Debug.Assert(iCluster != null);
            var iOld = Brain.Current[this[index]];
            if (value != null && iOld != null)
            {
                value.AddClusterUnsafe(iCluster);
                iOld.RemoveClusterUnsafe(iCluster); // succeed or not, don't care, replacing the item,
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

        /// <summary>Performs a remove and raises the events but doesn't change anything in
        ///     the neuron being added.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool RemoveDirect(Neuron item)
        {
            if (item != null)
            {
                var iIndex = IndexOf(item.ID);
                if (iIndex > -1)
                {
                    // we do a consistency check to avoid errors, this is an internal function anyway, simply to keep things in sync, so if it wasn't there, the remove is still ok, cause we are in sync again.
                    return base.InternalRemove(item, iIndex);
                }
            }

            return false;
        }

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
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(i, out iFound))
                {
                    iFound.RemoveClusterUnsafe((NeuronCluster)Owner);
                }
            }

            base.Clear();
        }

        /// <summary>
        ///     clears the list without registering this with the children. Only used
        ///     by the <see cref="Deleter" /> object.
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
            foreach (var i in items)
            {
                i.RemoveClusterUnsafe((NeuronCluster)Owner);
            }

            base.Clear();
        }

        #endregion
    }
}