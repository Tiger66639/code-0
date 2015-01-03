// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   An accessor that provides read/write capabilities to access list types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>An accessor that provides read/write capabilities to access list types.</summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ListAccessor<T> : NeuronAccessor, System.Collections.Generic.IList<T>
    {
        /// <summary>Initializes a new instance of the <see cref="ListAccessor{T}"/> class.</summary>
        internal ListAccessor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ListAccessor{T}"/> class. Initializes a new instance of the<see cref="JaStDev.HAB.ListAccessor`1"/> class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        public ListAccessor(System.Collections.Generic.IList<T> list, Neuron neuron, LockLevel level, bool writeable)
            : base(neuron, level, writeable)
        {
            List = list;
        }

        /// <summary>
        ///     Provides direct access to the list. Only use when already locked. Best
        ///     not to write to the list unless everything is properly locked.
        /// </summary>
        public System.Collections.Generic.IList<T> List { get; internal set; }

        #region IEnumerable<T> Members

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.Collections.Generic.IEnumerator`1" /> that can be
        ///     used to iterate through the collection.
        /// </returns>
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            Lock();
            try
            {
                foreach (var i in List)
                {
                    yield return i;
                }
            }
            finally
            {
                Unlock();
            }

            // return List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be
        ///     used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        /// <summary>Determines the index of a specific <paramref name="item"/> in the<see cref="System.Collections.Generic.IList`1"/> .</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.IList`1"/> .</param>
        /// <returns>The index of <paramref name="item"/> if found in the list; otherwise,
        ///     -1.</returns>
        public int IndexOf(T item)
        {
            Lock();
            try
            {
                return List.IndexOf(item);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>Determines the index of a specific <paramref name="item"/> in the<see cref="System.Collections.Generic.IList`1"/> .</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.IList`1"/> .</param>
        /// <returns>The index of <paramref name="item"/> if found in the list; otherwise,
        ///     -1.</returns>
        public int IndexOfUnsafe(T item)
        {
            return List.IndexOf(item);
        }

        /// <summary>Inserts an <paramref name="item"/> to the<see cref="System.Collections.Generic.IList`1"/> at the specified
        ///     index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be
        ///     inserted.</param>
        /// <param name="item">The object to insert into the<see cref="System.Collections.Generic.IList`1"/> .</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.Generic.IList`1"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Insert(int index, T item)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iUnlock = Lock(item);
            try
            {
                List.Insert(index, item);
            }
            finally
            {
                Unlock(iUnlock);
            }
        }

        /// <summary>Removes the <see cref="System.Collections.Generic.IList`1"/> item at
        ///     the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.Generic.IList`1"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iItem = this[index];
            var iAlsoLocked = Lock(iItem);
            try
            {
                List.RemoveAt(index);
            }
            finally
            {
                Unlock(iAlsoLocked);
            }
        }

        /// <summary>The get unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public T GetUnsafe(int index)
        {
            return List[index];
        }

        /// <summary>The set unsafe.</summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetUnsafe(int index, T value)
        {
            List[index] = value;
        }

        /// <summary>Gets or sets the <see cref="T"/> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <value></value>
        /// <returns>The <see cref="T"/>.</returns>
        public T this[int index]
        {
            get
            {
                Lock(); // need to make certain that a get is save.
                try
                {
                    return List[index];
                }
                finally
                {
                    Unlock();
                }
            }

            set
            {
                System.Diagnostics.Debug.Assert(IsWriteable);
                var iItems = Lock(value, this[index]);
                try
                {
                    List[index] = value;
                }
                finally
                {
                    Unlock(iItems);
                    Factories.Default.NLists.Recycle(iItems);
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>Adds an <paramref name="item"/> to the<see cref="System.Collections.Generic.ICollection`1"/> .</summary>
        /// <param name="item">The object to add to the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     read-only.</exception>
        public void Add(T item)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iUnlock = Lock(item);
            try
            {
                List.Add(item);
            }
            finally
            {
                Unlock(iUnlock);
            }
        }

        /// <summary>adds the <paramref name="item"/> without locking anything.</summary>
        /// <param name="item"></param>
        internal void AddUnsave(T item)
        {
            List.Add(item);
        }

        /// <summary>
        ///     Removes all items from the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     The <see cref="System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public virtual void Clear()
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iItems = LockAll();
            try
            {
                ((BrainList)List).Clear(iItems);

                    // always pass the items along, this way they don't need to be retrieved from teh cache again.
            }
            finally
            {
                Unlock(iItems);
                Factories.Default.NLists.Recycle(iItems);
            }
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.ICollection`1"/> contains a
        ///     specific value.</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false.</returns>
        public bool Contains(T item)
        {
            Lock();
            try
            {
                return List.Contains(item);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>Copies the elements of the<see cref="System.Collections.Generic.ICollection`1"/> to an<see cref="System.Array"/> , starting at a particular <see cref="System.Array"/>
        ///     index.</summary>
        /// <param name="array">The one-dimensional <see cref="System.Array"/> that is the destination of
        ///     the elements copied from<see cref="System.Collections.Generic.ICollection`1"/> . The<see cref="System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying
        ///     begins.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="array"/> is multidimensional. -or-<paramref name="arrayIndex"/> is equal to or greater than the length
        ///     of <paramref name="array"/> . -or- The number of elements in the
        ///     source <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     greater than the available space from <paramref name="arrayIndex"/>
        ///     to the end of the destination <paramref name="array"/> . -or- Type<paramref name="T"/> cannot be cast automatically to the type of the
        ///     destination <paramref name="array"/> .</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Lock();
            try
            {
                List.CopyTo(array, arrayIndex);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>
        ///     Gets the number of elements contained in the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     The number of elements contained in the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </returns>
        public int Count
        {
            get
            {
                Lock();
                try
                {
                    return List.Count;
                }
                finally
                {
                    Unlock();
                }
            }
        }

        /// <summary>
        ///     Gets the number of elements contained in the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     The number of elements contained in the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </returns>
        public int CountUnsafe
        {
            get
            {
                return List.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> is read-only;
        ///     otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return !IsWriteable;
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the<see cref="System.Collections.Generic.ICollection`1"/> .</summary>
        /// <param name="item">The object to remove from the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     read-only.</exception>
        /// <returns><see langword="true"/> if <paramref name="item"/> was successfully
        ///     removed from the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false. This method also returns <see langword="false"/> if<paramref name="item"/> is not found in the original<see cref="System.Collections.Generic.ICollection`1"/> .</returns>
        public bool Remove(T item)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iUnlock = Lock(item);
            try
            {
                return List.Remove(item);
            }
            finally
            {
                Unlock(iUnlock);
            }
        }

        /// <summary>A remove that can be used when the <paramref name="index"/> and the<paramref name="item"/> are known (a little faster.</summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public void Remove(T item, int index)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iUnlock = Lock(item);
            try
            {
                List.RemoveAt(index);
            }
            finally
            {
                Unlock(iUnlock);
            }
        }

        #endregion

        #region locking

        /// <summary>unlocks the previously created lock + makes certain that if the lock
        ///     was writable, both the wrapped neuron as the argument are set as
        ///     changed.</summary>
        /// <param name="item"></param>
        public virtual void Unlock(Neuron item)
        {
            Unlock();
            if (IsWriteable)
            {
                item.SetIsChangedNoClearBuffers(true);

                    // don't clear the buffers, this is already done during the action itself.
            }
        }

        /// <summary>Unlocks the previously created lock and makes certain taht everyone' s
        ///     isChanged is correctly updated.</summary>
        /// <param name="items">The items.</param>
        public virtual void Unlock(System.Collections.Generic.IEnumerable<Neuron> items)
        {
            Unlock();
            if (IsWriteable)
            {
                foreach (var i in items)
                {
                    i.SetIsChangedNoClearBuffers(true);

                        // don't clear the buffers, this is already done during the action itself.
                }
            }
        }

        /// <summary>The lock all.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public abstract System.Collections.Generic.List<Neuron> LockAll();

        /// <summary>locks the current neuron and the specified item.</summary>
        /// <param name="item"></param>
        /// <returns>The other itme that was locked.</returns>
        public abstract Neuron Lock(T item);

        /// <summary>The lock.</summary>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public abstract System.Collections.Generic.List<Neuron> Lock(T a, T b);

        /// <summary>Locks this item and all the specified items.</summary>
        /// <param name="array"></param>
        /// <returns>The <see cref="List"/>.</returns>
        public abstract System.Collections.Generic.List<Neuron> Lock(System.Collections.Generic.IEnumerable<T> array);

        #endregion
    }
}