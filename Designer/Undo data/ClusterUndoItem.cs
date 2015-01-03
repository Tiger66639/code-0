// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   to undo a cluster meaning change.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     to undo a cluster meaning change.
    /// </summary>
    public class ClusterMeaningUndoItem : UndoSystem.UndoItem
    {
        /// <summary>Gets or sets the cluster.</summary>
        public NeuronCluster Cluster { get; set; }

        /// <summary>Gets or sets the value.</summary>
        public Neuron Value { get; set; }

        /// <summary>The stores data from.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false;

                // can never group multiple changes together that are executed faster after each other (like text editor).
        }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            return false;

                // we don't need to delete cluster undo data because ui elements are invalid, this is always valid data.
        }

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new ClusterMeaningUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Value = Brain.Current[Cluster.Meaning];
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            if (Value != null)
            {
                Cluster.Meaning = Value.ID;
            }
            else
            {
                Cluster.Meaning = Neuron.EmptyId;
            }
        }
    }

    /// <summary>
    ///     A custom undo item that stores changes in a <see cref="NeuronCluster" />
    ///     . This is done seperatly, outside the undo event system because this uses
    ///     object references to wrappers, while we need neuron references because
    ///     the wrappers for the neurons get created and destroyed.
    /// </summary>
    public abstract class ClusterUndoItem : UndoSystem.UndoItem
    {
        /// <summary>Gets the action.</summary>
        public System.Collections.Specialized.NotifyCollectionChangedAction Action { get; internal set; }

        /// <summary>Gets or sets the cluster.</summary>
        public NeuronCluster Cluster { get; set; }

        /// <summary>The stores data from.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool StoresDataFrom(object source)
        {
            return false;

                // can never group multiple changes together that are executed faster after each other (like text editor).
        }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            return false;

                // we don't need to delete cluster undo data because ui elements are invalid, this is always valid data.
        }
    }

    /// <summary>The cluster items undo item.</summary>
    public abstract class ClusterItemsUndoItem : ClusterUndoItem
    {
        /// <summary>
        ///     Gets or sets the items that were removed during the reset.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public System.Collections.Generic.IList<Neuron> Items { get; set; }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            var iRes = base.HasSameTarget(toCompare);
            if (iRes)
            {
                var iItem = toCompare as ClusterItemsUndoItem;
                if (iItem != null && iItem.Items.Count == Items.Count)
                {
                    foreach (var i in iItem.Items)
                    {
                        if (Items.Contains(i) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>The reset cluster undo item.</summary>
    public class ResetClusterUndoItem : ClusterItemsUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="ResetClusterUndoItem"/> class.</summary>
        public ResetClusterUndoItem()
        {
            Action = System.Collections.Specialized.NotifyCollectionChangedAction.Reset;
        }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new AddClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Items = Items;
            iUndo.FromReset = true;
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            using (var iList = Cluster.ChildrenW) iList.AddRange(Items);
        }
    }

    /// <summary>The add cluster undo item.</summary>
    public class AddClusterUndoItem : ClusterItemsUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="AddClusterUndoItem"/> class.</summary>
        public AddClusterUndoItem()
        {
            FromReset = false;
            Action = System.Collections.Specialized.NotifyCollectionChangedAction.Add;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the undo came from a reversed
        ///     reset, in which case we can do a reset again. False by default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [from reset]; otherwise, <c>false</c> .
        /// </value>
        public bool FromReset { get; set; }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            if (FromReset)
            {
                DoReset(caller);
            }
            else
            {
                DoRemove(caller);
            }
        }

        /// <summary>The do remove.</summary>
        /// <param name="caller">The caller.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void DoRemove(UndoSystem.UndoStore caller)
        {
            var iUndo = new RemoveClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Items = Items;
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            var iChildren = Cluster.ChildrenW;
            iChildren.Lock(Items);
            try
            {
                iUndo.Index = iChildren.IndexOfUnsafe(Items[0].ID);

                    // when the action gets reversed, we need to know the index of the first object (usually there is only 1 item, except during a reset).
                if (iUndo.Index != -1)
                {
                    foreach (var i in Items)
                    {
                        iChildren.RemoveUnsafe(i);
                    }
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "Item not found in cluster, can't execute the undo item.");
                }
            }
            finally
            {
                iChildren.Unlock(Items);
                iChildren.Dispose();
            }
        }

        /// <summary>The do reset.</summary>
        /// <param name="caller">The caller.</param>
        private void DoReset(UndoSystem.UndoStore caller)
        {
            var iUndo = new ResetClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Items = Items;
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            using (var iList = Cluster.ChildrenW) iList.Clear();
        }
    }

    /// <summary>The remove cluster undo item.</summary>
    public class RemoveClusterUndoItem : ClusterItemsUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="RemoveClusterUndoItem"/> class.</summary>
        public RemoveClusterUndoItem()
        {
            Action = System.Collections.Specialized.NotifyCollectionChangedAction.Remove;
        }

        /// <summary>
        ///     Gets or sets the index at which the first item was located when the
        ///     remove happened. -1 indicates to do an add instead of an insert when
        ///     reversing the action.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new AddClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Items = Items;
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            if (Index == -1)
            {
                using (var iList = Cluster.ChildrenW) iList.AddRange(Items);
            }
            else
            {
                var iChildren = Cluster.ChildrenW;
                iChildren.Lock(Items);
                try
                {
                    foreach (var i in Items)
                    {
                        iChildren.InsertUnsafe(Index++, i);
                    }
                }
                finally
                {
                    iChildren.Unlock(Items);
                    iChildren.Dispose();
                }
            }
        }
    }

    /// <summary>The move cluster undo item.</summary>
    public class MoveClusterUndoItem : ClusterUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="MoveClusterUndoItem"/> class.</summary>
        public MoveClusterUndoItem()
        {
            Action = System.Collections.Specialized.NotifyCollectionChangedAction.Move;
        }

        /// <summary>
        ///     Gets or sets the item that was moved.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; set; }

        /// <summary>
        ///     Gets or sets the index that the item had.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new MoveClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Item = Item;
            iUndo.FocusedElement = FocusedElement;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            var iChildren = Cluster.ChildrenW;
            iChildren.Lock();
            try
            {
                var iIndex = iChildren.IndexOfUnsafe(Item.ID);
                iUndo.Index = iIndex;
                iChildren.MoveUnsave(iIndex, Index);

                // iChildren.RemoveAt(iIndex);
                // iChildren.Insert(Index, Item);
            }
            finally
            {
                iChildren.Unlock();
            }
        }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            if (base.HasSameTarget(toCompare))
            {
                var iToCompare = toCompare as MoveClusterUndoItem;
                if (iToCompare != null)
                {
                    return iToCompare.Index == Index && iToCompare.Item == Item;
                }
            }

            return false;
        }
    }

    /// <summary>The replace cluster undo item.</summary>
    public class ReplaceClusterUndoItem : ClusterUndoItem
    {
        /// <summary>Initializes a new instance of the <see cref="ReplaceClusterUndoItem"/> class.</summary>
        public ReplaceClusterUndoItem()
        {
            Action = System.Collections.Specialized.NotifyCollectionChangedAction.Replace;
        }

        /// <summary>
        ///     Gets or sets the item that was replaced.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; set; }

        /// <summary>
        ///     Gets or sets the index that the item had.
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>The execute.</summary>
        /// <param name="caller">The caller.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new ReplaceClusterUndoItem();
            iUndo.Cluster = Cluster;
            iUndo.Index = Index;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            using (var iList = Cluster.Children) iUndo.Item = Brain.Current[iList[Index]];
            var iChildren = Cluster.ChildrenW;
            var iToRelease = iChildren.Lock(iUndo.Item, Item);
            try
            {
                iChildren.RemoveUnsafe(iUndo.Item, Index);
                iChildren.InsertUnsafe(Index, Item);
            }
            finally
            {
                iChildren.Unlock(iToRelease);
                Factories.Default.NLists.Recycle(iToRelease);
                iChildren.Dispose();
            }
        }

        /// <summary>The has same target.</summary>
        /// <param name="toCompare">The to compare.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool HasSameTarget(UndoSystem.UndoItem toCompare)
        {
            if (base.HasSameTarget(toCompare))
            {
                var iToCompare = toCompare as MoveClusterUndoItem;
                if (iToCompare != null)
                {
                    return iToCompare.Index == Index && iToCompare.Item == Item;
                }
            }

            return false;
        }
    }
}