// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection containing flows. Provides monitoring capabilities for the
//   flows.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection containing flows. Provides monitoring capabilities for the
    ///     flows.
    /// </summary>
    public class FlowCollection : Data.ObservedCollection<Flow>
    {
        /// <summary>The f event monitor.</summary>
        private readonly FlowCollectionEventMonitor fEventMonitor;

        /// <summary>
        ///     Keeps track of the previously selected flows.
        /// </summary>
        /// <remarks>
        ///     This is maintained in the flow list (and not in the flow editor),
        ///     since this is closest to where the list is changed.
        /// </remarks>
        private readonly Tools.Navigation.NavigationStore<Flow> fSelectionHistory =
            new Tools.Navigation.NavigationStore<Flow>();

        /// <summary>
        ///     Keeps track of the previously selected flows.
        /// </summary>
        /// <remarks>
        ///     This is maintained in the flow list (and not in the flow editor),
        ///     since this is closest to where the list is changed.
        /// </remarks>
        public Tools.Navigation.NavigationStore<Flow> SelectionHistory
        {
            get
            {
                return fSelectionHistory;
            }
        }

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            fSelectionHistory.Clear();
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, Flow item)
        {
            base.InsertItem(index, item);
            fEventMonitor.AddItem(item.Item.ID);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iToRemove = this[index];
            fSelectionHistory.Remove(iToRemove);
            fEventMonitor.RemoveItem(iToRemove.Item.ID);
            base.RemoveItem(index);
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, Flow item)
        {
            var iOld = this[index];
            fSelectionHistory.Remove(iOld);
            fEventMonitor.RemoveItem(iOld.Item.ID);
            base.SetItem(index, item);
            fEventMonitor.AddItem(item.Item.ID);
        }

        /// <summary>The remove all.</summary>
        /// <param name="toRemove">The to remove.</param>
        internal void RemoveAll(Neuron toRemove)
        {
            var iFound = (from i in this where i.Item == toRemove select i).ToArray();
            foreach (var i in iFound)
            {
                RemoveItemDirect(IndexOf(i));
                fSelectionHistory.Remove(i);
            }
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlowCollection" /> class.
        /// </summary>
        public FlowCollection()
        {
            fEventMonitor = new FlowCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="FlowCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public FlowCollection(object owner)
            : base(owner)
        {
            fEventMonitor = new FlowCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="FlowCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toCopyFrom">To copy from.</param>
        public FlowCollection(object owner, System.Collections.Generic.List<Flow> toCopyFrom)
            : base(owner, toCopyFrom)
        {
            fEventMonitor = new FlowCollectionEventMonitor(this);
        }

        #endregion
    }
}