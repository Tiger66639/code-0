// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkInfoList.cs" company="">
//   
// </copyright>
// <summary>
//   A specific implementation of <see cref="NeuronList" /> that will
//   increment/decrement the <see cref="Neuron.InfoUsageCounter" /> of the
//   items being added/removed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A specific implementation of <see cref="NeuronList" /> that will
    ///     increment/decrement the <see cref="Neuron.InfoUsageCounter" /> of the
    ///     items being added/removed.
    /// </summary>
    public class LinkInfoList : BrainList
    {
        /// <summary>The f onwer.</summary>
        private readonly Link fOnwer;

        /// <summary>Initializes a new instance of the <see cref="LinkInfoList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public LinkInfoList(Link owner)
        {
            fOnwer = owner;
        }

        /// <summary>clears the list of items, doesn't have to get the each neuron from
        ///     cache. Use this instead of <see cref="BrainList.Clear"/></summary>
        /// <param name="items"></param>
        public override void Clear(System.Collections.Generic.List<Neuron> items)
        {
            base.Clear();
            foreach (var i in items)
            {
                i.DecInfoUnsafe();
            }
        }

        /// <summary>Performs the actual insertion.</summary>
        /// <remarks>Makes certain that everything is regisetered + raises the appropriate
        ///     events.</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InternalInsert(int index, Neuron item)
        {
            base.InternalInsert(index, item);
            item.IncInfoUnsafe();
        }

        /// <summary>Removes the <paramref name="item"/> at the specified index.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalRemove(Neuron item, int index)
        {
            if (base.InternalRemove(item, index))
            {
                item.DecInfoUnsafe();
                return true;
            }

            return false;
        }

        /// <summary>Called when a neuron gets repaced by a new one in the list.</summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        protected override void InternalReplace(Neuron value, int index)
        {
            var iOld = this[index];
            base.InternalReplace(value, index);
            Neuron iOldNeuron;
            if (Brain.Current.TryFindNeuron(iOld, out iOldNeuron))
            {
                iOldNeuron.DecInfoUnsafe();
            }

            if (value != null)
            {
                value.IncInfoUnsafe();
            }
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
            return new LinkInfoAccessor(fOnwer, false);
        }

        /// <summary>Adds the <paramref name="neuron"/> to the list in an unsafe manner
        ///     (without locking anything). This is used during an AddInfoInstruction.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void AddItemUnsafe(Neuron neuron)
        {
            base.InternalInsert(Count, neuron);
            neuron.IncInfoUnsafe();
        }

        /// <summary>The remove at unsafe.</summary>
        /// <param name="index">The index.</param>
        internal void RemoveAtUnsafe(int index)
        {
            if (index > -1)
            {
                var neuron = Brain.Current[this[index]];
                base.InternalRemove(neuron, index);
                neuron.DecInfoUnsafe();
            }
        }

        /// <summary>The insert info unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <param name="neuron">The neuron.</param>
        internal void InsertInfoUnsafe(int index, Neuron neuron)
        {
            base.InternalInsert(index, neuron);
            neuron.IncInfoUnsafe();
        }

        /// <summary>The change unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <param name="old">The old.</param>
        /// <param name="value">The value.</param>
        internal void ChangeUnsafe(int index, Neuron old, Neuron value)
        {
            base.InternalReplace(value, index);
            old.DecInfoUnsafe();
            if (value != null)
            {
                value.IncInfoUnsafe();
            }
        }
    }
}