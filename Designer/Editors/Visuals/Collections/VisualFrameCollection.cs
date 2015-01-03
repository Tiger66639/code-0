// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualFrameCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of visual frames.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection of visual frames.
    /// </summary>
    public class VisualFrameCollection : Data.ObservedCollection<VisualFrame>
    {
        /// <summary>Initializes a new instance of the <see cref="VisualFrameCollection"/> class. Initializes a new instance of the <see cref="VisualFrameCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        public VisualFrameCollection(object owner)
            : base(owner)
        {
        }

        /// <summary>Provides a direct Add, without locking or assigning any namespace.
        ///     Used for loading from stream, in which case, these things can't be
        ///     done untill when the data gets registered with the designer.</summary>
        /// <param name="toAdd">To add.</param>
        public void AddFromStream(VisualFrame toAdd)
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
        protected override void InsertItem(int index, VisualFrame item)
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
        protected override void InsertItemDirect(int index, VisualFrame item)
        {
            if (item != null)
            {
                var iData = item.NeuronInfo;
                iData.IsLocked = true;
            }

            base.InsertItemDirect(index, item);
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            if (this[index] != null)
            {
                var iData = this[index].NeuronInfo;
                iData.IsLocked = false;
            }

            base.RemoveItem(index);
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

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, VisualFrame item)
        {
            NeuronData iData;
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
        protected override void SetItemDirect(int index, VisualFrame item)
        {
            NeuronData iData;
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