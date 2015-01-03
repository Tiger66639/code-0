// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatchCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that contains <see cref="Watch" /> objects. Makes certain
//   that any watch being removed, is also unregistered by the
//   <see cref="AttachedNeuronsDict" /> so that we stop keeping an eye on the
//   content of the var.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A collection that contains <see cref="Watch" /> objects. Makes certain
    ///     that any watch being removed, is also unregistered by the
    ///     <see cref="AttachedNeuronsDict" /> so that we stop keeping an eye on the
    ///     content of the var.
    /// </summary>
    public class WatchCollection : Data.ObservedCollection<Watch>
    {
        /// <summary>The f event monitor.</summary>
        private readonly WatchCollectionEventMonitor fEventMonitor;

        /// <summary>The clear items.</summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                if (i.AttachValuesToProcessor)
                {
                    ProcessorManager.Current.AtttachedDict.RemoveWatch(i);
                }
            }

            fEventMonitor.Clear();
            base.ClearItems();
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, Watch item)
        {
            var iPrev = this[index];
            if (iPrev.AttachValuesToProcessor)
            {
                ProcessorManager.Current.AtttachedDict.RemoveWatch(iPrev);
            }

            fEventMonitor.RemoveItem(iPrev.ID);
            base.SetItem(index, item);
            fEventMonitor.AddItem(item.ID);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iPrev = this[index];
            if (iPrev.AttachValuesToProcessor)
            {
                ProcessorManager.Current.AtttachedDict.RemoveWatch(iPrev);
            }

            fEventMonitor.RemoveItem(iPrev.ID);
            base.RemoveItem(index);
        }

        /// <summary>The remove item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            var iPrev = this[index];
            if (iPrev.AttachValuesToProcessor)
            {
                ProcessorManager.Current.AtttachedDict.RemoveWatch(iPrev);
            }

            // note:don't need to call fEventMonitor.RemoveItem ,cause this function gets called from the event monitor through removeAll
            base.RemoveItemDirect(index);
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, Watch item)
        {
            fEventMonitor.AddItem(item.ID);
            base.InsertItem(index, item);
        }

        /// <summary>The remove all.</summary>
        /// <param name="toRemove">The to remove.</param>
        internal void RemoveAll(Neuron toRemove)
        {
            var iFound = from i in this where i.Item == toRemove select i;
            foreach (var i in iFound)
            {
                RemoveItemDirect(IndexOf(i));
            }
        }

        /// <summary>The replace all.</summary>
        /// <param name="toReplace">The to replace.</param>
        internal void ReplaceAll(Neuron toReplace)
        {
            var iFound = (from i in this where i.Item == toReplace select i).ToList();
            foreach (var i in iFound)
            {
                var iAttach = i.AttachValuesToProcessor;
                var iNew = new Watch(i.ID); // simply recreate the watch, this is easiest
                this[IndexOf(i)] = iNew;
                iNew.AttachValuesToProcessor = iAttach;
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="WatchCollection"/> class.</summary>
        public WatchCollection()
        {
            fEventMonitor = new WatchCollectionEventMonitor(this);
        }

        /// <summary>Initializes a new instance of the <see cref="WatchCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public WatchCollection(object owner)
            : base(owner)
        {
            fEventMonitor = new WatchCollectionEventMonitor(this);
        }

        #endregion
    }
}