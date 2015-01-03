// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExplorerItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that contains <see cref="ExplorerItem" /> objects. It
//   provides syncing with the network through the <see cref="EventManager" />
//   .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection that contains <see cref="ExplorerItem" /> objects. It
    ///     provides syncing with the network through the <see cref="EventManager" />
    ///     .
    /// </summary>
    public class ExplorerItemCollection : System.Collections.ObjectModel.ObservableCollection<ExplorerItem>
    {
        /// <summary>The f event monitor.</summary>
        private readonly ExplorerItemCollectionEventMonitor fEventMonitor;

        /// <summary>Initializes a new instance of the <see cref="ExplorerItemCollection"/> class.</summary>
        public ExplorerItemCollection()
        {
            fEventMonitor = new ExplorerItemCollectionEventMonitor(this);
        }

        /// <summary>Changes the neuron.</summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        internal void ChangeNeuron(Neuron oldValue, Neuron newValue)
        {
            var iFound =
                (from i in this where i is NeuronExplorerItem && ((NeuronExplorerItem)i).Item == oldValue select i)
                    .FirstOrDefault();
            if (iFound != null)
            {
                this[IndexOf(iFound)] = new NeuronExplorerItem(newValue);
            }
        }

        /// <summary>Removes the neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="id">The id.</param>
        internal void RemoveNeuron(Neuron neuron, ulong id)
        {
            var iFound =
                (from i in this
                 where i is NeuronExplorerItem && ((NeuronExplorerItem)i).Item == neuron
                 select IndexOf(i)).ToList();

                // we need to explicitly convert to a list cause otherwise we get an error in the 'set new item' loop that we are modify the list inside a loop.
            foreach (var i in iFound)
            {
                this[i] = new FreeExplorerItem(id);
            }
        }

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ExplorerItem item)
        {
            var iItem = item as NeuronExplorerItem;
            if (iItem != null)
            {
                fEventMonitor.AddItem(iItem.ID);
            }

            base.InsertItem(index, item);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index] as NeuronExplorerItem;
            if (iItem != null)
            {
                fEventMonitor.RemoveItem(iItem.ID);
            }

            base.RemoveItem(index);
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ExplorerItem item)
        {
            var iItem = this[index] as NeuronExplorerItem;
            if (iItem != null)
            {
                fEventMonitor.RemoveItem(iItem.ID);
            }

            iItem = item as NeuronExplorerItem;
            if (iItem != null)
            {
                fEventMonitor.AddItem(iItem.ID);
            }

            base.SetItem(index, item);
        }
    }
}