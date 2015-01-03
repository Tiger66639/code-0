// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolBoxItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that manages toolbox items. We have put this in a seperate
//   class so that it can track changes to the network (and delete
//   neuronToolboxItems as needed).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection that manages toolbox items. We have put this in a seperate
    ///     class so that it can track changes to the network (and delete
    ///     neuronToolboxItems as needed).
    /// </summary>
    public class ToolBoxItemCollection : Data.ObservedCollection<ToolBoxItem>
    {
        /// <summary>The f event monitor.</summary>
        private readonly ToolBoxItemCollectionEventMonitor fEventMonitor;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ToolBoxItemCollection"/> class. 
        ///     Initializes a new instance of the <see cref="ToolBoxItemCollection"/>
        ///     class.</summary>
        public ToolBoxItemCollection()
        {
            fEventMonitor = new ToolBoxItemCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="ToolBoxItemCollection"/> class. Initializes a new instance of the <see cref="ToolBoxItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public ToolBoxItemCollection(object owner)
            : base(owner)
        {
            fEventMonitor = new ToolBoxItemCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="ToolBoxItemCollection"/> class. Initializes a new instance of the <see cref="ToolBoxItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="toCopyFrom">To copy from.</param>
        public ToolBoxItemCollection(object owner, System.Collections.Generic.List<ToolBoxItem> toCopyFrom)
            : base(owner, toCopyFrom)
        {
            fEventMonitor = new ToolBoxItemCollectionEventMonitor(this);
        }

        #endregion

        #region Event Monitors

        /// <summary>Called (by the monitor) when a neuron represented through a toolbox
        ///     item, is removed from the network.</summary>
        /// <param name="item">The item.</param>
        public void RemoveNeuron(Neuron item)
        {
            var iFound = (from i in this

                          // get all the toolbox items related to the item.
                          where i is NeuronToolBoxItem && ((NeuronToolBoxItem)i).Item == item
                          select i).ToList();
            foreach (var i in iFound)
            {
                RemoveItemDirect(IndexOf(i));
            }
        }

        /// <summary>Called (by the monitor) when a neuron object has changed (so same id,
        ///     new object).</summary>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        public void ReplaceNeuron(Neuron old, Neuron value)
        {
            var iFound = from i in this

                         // get all the toolbox items related to the item.
                         where i is NeuronToolBoxItem && ((NeuronToolBoxItem)i).Item == old
                         select i;
            foreach (NeuronToolBoxItem i in iFound)
            {
                i.Item = value;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            var iFound = from i in this where i is NeuronToolBoxItem select ((NeuronToolBoxItem)i).ItemID;
            EventManager.Current.RemoveNeuronChangedMonitor(fEventMonitor, iFound);
            base.ClearItems();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ToolBoxItem item)
        {
            base.InsertItem(index, item);
            var iItem = item as NeuronToolBoxItem;
            if (iItem != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(fEventMonitor, iItem.ItemID);
            }
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index] as NeuronToolBoxItem;
            if (iItem != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(fEventMonitor, iItem.ItemID);
            }

            base.RemoveItem(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ToolBoxItem item)
        {
            var iItem = this[index] as NeuronToolBoxItem;
            if (iItem != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(fEventMonitor, iItem.ItemID);
            }

            base.SetItem(index, item);
            iItem = item as NeuronToolBoxItem;
            if (iItem != null)
            {
                EventManager.Current.AddNeuronChangedMonitor(fEventMonitor, iItem.ItemID);
            }
        }

        #endregion
    }
}