// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainList.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all lists in the <see cref="Brain" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for all lists in the <see cref="Brain" /> .
    /// </summary>
    public abstract class BrainList : System.Collections.Generic.IList<ulong>
    {
        #region prop

        /// <summary>
        ///     Gets the <see langword="internal" /> list of data
        /// </summary>
        /// <value>
        ///     The list.
        /// </value>
        public System.Collections.Generic.List<ulong> List { get; set; }

        #endregion

        #region IEnumerable Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<ulong> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        #endregion

        #region Contains

        /// <summary>Checks if every item id in the specified <paramref name="list"/> is
        ///     also in this list.</summary>
        /// <remarks>If <paramref name="list"/> doesn't contain any elements, we always
        ///     return true.</remarks>
        /// <param name="list">an enumerable containing neuron id's.</param>
        /// <returns>True if all the specified id's are also in this list.</returns>
        public bool Contains(System.Collections.Generic.IEnumerable<ulong> list)
        {
            var toSearch = new System.Collections.Generic.List<ulong>(list);

                // we make a copy, so we can remove all the items we have found, this should be faster: only have to run over each list 1 time + a search on the smallest for each from the biggest.
            if (toSearch.Count > 0)
            {
                foreach (var i in this)
                {
                    toSearch.Remove(i);
                }

                return toSearch.Count == 0;
            }

            return true;
        }

        #endregion

        /// <summary>moves the items <paramref name="from"/> position. Doesn't raise an
        ///     event.</summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Move(int from, int to)
        {
            var iVal = List[from];
            List.RemoveAt(from);
            List.Insert(to, iVal);
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrainList" /> class.
        /// </summary>
        internal BrainList()
        {
            List = Factories.Default.IDLists.GetBuffer();
        }

        /// <summary>Initializes a new instance of the <see cref="BrainList"/> class.</summary>
        /// <param name="capacity">The capacity.</param>
        internal BrainList(int capacity)
        {
            List = Factories.Default.IDLists.GetBuffer(capacity);
        }

        #endregion

        #region abstract

        /// <summary>
        ///     Gets the accessor for the list.
        /// </summary>
        /// <returns>
        ///     By default, this returns a <see cref="NeuronsAccessor" /> , but can be
        ///     changed in any descendent.
        /// </returns>
        protected internal abstract Accessor GetAccessor();

        /// <summary>clears the list of items, doesn't have to get the each neuron from
        ///     cache. Use this instead of <see cref="BrainList.Clear"/></summary>
        /// <param name="items">The items.</param>
        public abstract void Clear(System.Collections.Generic.List<Neuron> items);

        #endregion

        #region virtual

        /// <summary>Performs the actual insertion.</summary>
        /// <remarks>Makes certain that everything is regisetered + raises the appropriate
        ///     events.</remarks>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected virtual void InternalInsert(int index, Neuron item)
        {
            System.Diagnostics.Debug.Assert(item.ID != Neuron.TempId && item.ID != 0);
            List.Insert(index, item.ID);
            if (Brain.Current.HasNeuronListChangedEvents)
            {
                var iArgs = new NeuronListChangedEventArgs();
                iArgs.Action = NeuronListChangeAction.Insert;
                iArgs.Index = index;
                iArgs.Item = item;
                iArgs.InternalList = this;
                Brain.Current.OnNeuronListChanged(iArgs);
            }
        }

        /// <summary>Removes the <paramref name="item"/> at the specified index.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected virtual bool InternalRemove(Neuron item, int index)
        {
            if (index != -1)
            {
                if (Brain.Current.HasNeuronListChangedEvents)
                {
                    var iArgs = new NeuronListChangedEventArgs();
                    iArgs.Action = NeuronListChangeAction.Remove;
                    iArgs.Index = index;
                    iArgs.Item = item;
                    iArgs.InternalList = this;
                    Brain.Current.OnNeuronListChanged(iArgs);
                }

                List.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>Called when a neuron gets repaced by a new one in the list.</summary>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        protected virtual void InternalReplace(Neuron value, int index)
        {
            if (Brain.Current.HasNeuronListChangedEvents)
            {
                var iArgs = new NeuronListChangedEventArgs();
                iArgs.Action = NeuronListChangeAction.Remove;
                iArgs.Index = index;
                iArgs.Item = Brain.Current[List[index]];
                iArgs.InternalList = this;
                Brain.Current.OnNeuronListChanged(iArgs);
            }

            System.Diagnostics.Debug.Assert(value.ID != Neuron.TempId && value.ID != 0);
            List[index] = value.ID;
            if (Brain.Current.HasNeuronListChangedEvents)
            {
                var iArgs = new NeuronListChangedEventArgs();
                iArgs.Action = NeuronListChangeAction.Insert;
                iArgs.Index = index;
                iArgs.Item = value;
                iArgs.InternalList = this;
                Brain.Current.OnNeuronListChanged(iArgs);
            }
        }

        #endregion

        #region IList<ulong> Members

        /// <summary>Determines the index of a specific <paramref name="item"/> in the<see cref="System.Collections.Generic.IList`1"/> .</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.IList`1"/> .</param>
        /// <returns>The index of <paramref name="item"/> if found in the list; otherwise,
        ///     -1.</returns>
        public int IndexOf(ulong item)
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
        public void Insert(int index, ulong item)
        {
            InternalInsert(index, Brain.Current[item]);
        }

        /// <summary>Performs an insert using a <see cref="Neuron"/> instead of it's id.
        ///     This is a bit faster if the <see cref="Neuron"/> is known and we are
        ///     not in a subprocessor with a duplication of the owner.</summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Neuron item)
        {
            InternalInsert(index, item);
        }

        /// <summary>Removes the <see cref="System.Collections.Generic.IList`1"/> item at
        ///     the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.Generic.IList`1"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            InternalRemove(Brain.Current[List[index]], index);
        }

        /// <summary>Removes the <see cref="System.Collections.Generic.IList`1"/> item at
        ///     the specified index. No cache access required.</summary>
        /// <param name="toRemove">The to Remove.</param>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.Generic.IList`1"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Remove(Neuron toRemove, int index)
        {
            InternalRemove(toRemove, index);
        }

        /// <summary>Gets or sets the <see cref="ulong"/> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <value></value>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong this[int index]
        {
            get
            {
                return List[index];
            }

            set
            {
                InternalReplace(Brain.Current[value], index);
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>Adds a <paramref name="range"/> of identifiefs to the list.</summary>
        /// <param name="range">The range.</param>
        internal void AddRange(System.Collections.Generic.IEnumerable<ulong> range)
        {
            foreach (var i in range)
            {
                InternalInsert(List.Count, Brain.Current[i]);
            }
        }

        /// <summary>Adds a <paramref name="range"/> of identifiefs to the list.</summary>
        /// <param name="range">The range.</param>
        internal void AddRange(System.Collections.Generic.IEnumerable<Neuron> range)
        {
            foreach (var i in range)
            {
                if (i.IsDeleted == false)
                {
                    InternalInsert(List.Count, i);
                }
            }
        }

        /// <summary>Adds an <paramref name="item"/> to the<see cref="System.Collections.Generic.ICollection`1"/> .</summary>
        /// <param name="item">The object to add to the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     read-only.</exception>
        public void Add(ulong item)
        {
            InternalInsert(List.Count, Brain.Current[item]);
        }

        /// <summary><see langword="internal"/> function that should only be used while
        ///     loading from xml. it simply adds the <paramref name="item"/> to the
        ///     list without updating any references or checking for current processor
        ///     state.</summary>
        /// <param name="item">The value to add.</param>
        internal void AddFromLoad(ulong item)
        {
            List.Add(item);
        }

        /// <summary>Adds the id of the specified neuron to the list. This is a little
        ///     faster if the neuron is known compared to useing the index.</summary>
        /// <param name="item"></param>
        public void Add(Neuron item)
        {
            InternalInsert(List.Count, item);
        }

        /// <summary>
        ///     Removes all items from the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> . Don't use
        ///     this directly, instead use the one with parameters, which is more
        ///     secure.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     The <see cref="System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public virtual void Clear()
        {
            List.Clear();
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.ICollection`1"/> contains a
        ///     specific value.</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false.</returns>
        public bool Contains(ulong item)
        {
            return List.Contains(item);
        }

        /// <summary>The contains.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Contains(Neuron item)
        {
            return List.Contains(item.ID);
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
        public void CopyTo(ulong[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
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
                return false;
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the<see cref="System.Collections.Generic.ICollection`1"/> .</summary>
        /// <param name="item">The object to remove from the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     read-only.</exception>
        /// <returns><see langword="true"/> if <paramref name="item"/> was successfully
        ///     removed from the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false. This method also returns <see langword="false"/> if<paramref name="item"/> is not found in the original<see cref="System.Collections.Generic.ICollection`1"/> .</returns>
        public bool Remove(ulong item)
        {
            var iIndex = List.IndexOf(item);
            if (iIndex > -1)
            {
                Neuron iFound;
                Brain.Current.TryFindNeuron(item, out iFound);

                    // we use this type of retrieval, othewise we get an exception, which we don't wan't cause we also want to be able to remove invalid references so we can fix faulty data.
                return InternalRemove(iFound, iIndex);
            }

            return false;
        }

        /// <summary>Removes the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Remove(Neuron item)
        {
            if (item == null)
            {
                throw new BrainException("Can't remove null from ID list.");
            }

            var iIndex = List.IndexOf(item.ID);
            if (iIndex > -1)
            {
                return InternalRemove(item, iIndex);
            }

            return false;
        }

        #endregion
    }
}