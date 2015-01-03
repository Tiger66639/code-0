// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection containing frames, used by the frame editor. Assigns to and
//   removes from the <see langword="namespace" /> automatically.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection containing frames, used by the frame editor. Assigns to and
    ///     removes from the <see langword="namespace" /> automatically.
    /// </summary>
    public class FrameCollection : Data.ObservedCollection<Frame>
    {
        /// <summary>Initializes a new instance of the <see cref="FrameCollection"/> class. Initializes a new instance of the <see cref="FrameCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public FrameCollection(object owner)
            : base(owner)
        {
        }

        /// <summary>Provides a direct Add, without locking or assigning any namespace.
        ///     Used for loading from stream, in which case, these things can't be
        ///     done untill when the data gets registered with the designer.</summary>
        /// <param name="toAdd">To add.</param>
        public void AddFromStream(Frame toAdd)
        {
            base.InsertItem(Count, toAdd);
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                var iData = i.NeuronInfo;
                iData.IsLocked = false;
            }

            base.ClearItems();
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            foreach (var i in this)
            {
                if (i != null)
                {
                    var iData = i.NeuronInfo;
                    iData.IsLocked = false;
                }
            }

            base.ClearItemsDirect();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, Frame item)
        {
            if (item != null)
            {
                var iData = item.NeuronInfo;
                iData.IsLocked = true;
            }

            base.InsertItem(index, item);
        }

        /// <summary>Inserts the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItemDirect(int index, Frame item)
        {
            if (item != null)
            {
                var iData = item.NeuronInfo;
                iData.IsLocked = true;
            }

            base.InsertItemDirect(index, item);
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            if (this[index] != null)
            {
                var iData = this[index].NeuronInfo;
                iData.IsLocked = false;
            }

            base.RemoveItemDirect(index);
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            if (this[index].Item != null)
            {
                // if the neuron got deleted, the neurondata is probably already deleted, and so is the namespace already cleared.
                var iData = BrainData.Current.NeuronInfo[this[index].Item];

                    // this is the savest way of getting the neurondata: if it is already deleted, this will simply return nul.
                if (iData != null)
                {
                    iData.IsLocked = false;
                }
            }

            base.RemoveItem(index);
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, Frame item)
        {
            NeuronData iData;
            var iOwner = (FrameEditor)Owner;
            if (this[index] != null)
            {
                iData = this[index].NeuronInfo;
                iData.IsLocked = false;
            }

            if (item != null)
            {
                iData = item.NeuronInfo;
                iData.IsLocked = true;
            }

            base.SetItem(index, item);
        }

        /// <summary>Sets the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItemDirect(int index, Frame item)
        {
            NeuronData iData;
            var iOwner = (FrameEditor)Owner;
            if (this[index] != null)
            {
                iData = this[index].NeuronInfo;
                iData.IsLocked = false;
            }

            if (item != null)
            {
                iData = item.NeuronInfo;
                iData.IsLocked = true;
            }

            base.SetItemDirect(index, item);
        }
    }
}