// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwnedBrainList.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all lists used by the <see cref="Brain" /> that are owned
//   by a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for all lists used by the <see cref="Brain" /> that are owned
    ///     by a neuron.
    /// </summary>
    /// <remarks>
    ///     Makes certain that when an item is added to this list, the owner is
    ///     registered with the brain (if it has a temp id).
    /// </remarks>
    public abstract class OwnedBrainList : BrainList
    {
        /// <summary>Initializes a new instance of the <see cref="OwnedBrainList"/> class. Initializes a new instance of the <see cref="BrainList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        internal OwnedBrainList(Neuron owner)
        {
            if (owner == null)
            {
                throw new System.ArgumentNullException();
            }

            Owner = owner;
        }

        /// <summary>Initializes a new instance of the <see cref="OwnedBrainList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="capacity">The capacity.</param>
        internal OwnedBrainList(Neuron owner, int capacity)
            : base(capacity)
        {
            if (owner == null)
            {
                throw new System.ArgumentNullException();
            }

            Owner = owner;
        }

        #region Owner

        /// <summary>
        ///     Gets the <see cref="Neuron" /> that owns this list.
        /// </summary>
        /// <remarks>
        ///     This is used for <see langword="internal" /> checking to allow modifs
        ///     or to buffer them.
        /// </remarks>
        public Neuron Owner { get;

            // we allow an internal set, cause links use the 'from' neuron as the owner, since any changes reflect on the owner, so it must be editable. But offcourse, a link's from can change, so the owner must also be changeable.
            internal set; }

        #endregion

        /// <summary>Performs the actual insertion.</summary>
        /// <remarks>Makes certain that everything is regisetered + raises the appropriate
        ///     events.</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="BrainException">When there is no owner defined.</exception>
        protected override void InternalInsert(int index, Neuron item)
        {
            if (Owner != null)
            {
                base.InternalInsert(index, item);
            }
            else
            {
                throw new BrainException("Owner not defined.");
            }
        }

        /// <summary>Removes the <paramref name="item"/> at the specified index.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalRemove(Neuron item, int index)
        {
            if (Owner != null)
            {
                return base.InternalRemove(item, index);
            }

            return false;
        }

        /// <summary>Called when a neuron gets repaced by a new one in the list.</summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        protected override void InternalReplace(Neuron value, int index)
        {
            if (Owner != null)
            {
                base.InternalReplace(value, index);
            }
            else
            {
                throw new BrainException("Owner not defined.");
            }
        }
    }
}