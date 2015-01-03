// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorItemSelectionList.cs" company="">
//   
// </copyright>
// <summary>
//   The editor item selection list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The editor item selection list.</summary>
    /// <typeparam name="T"></typeparam>
    internal class EditorItemSelectionList<T> : System.Collections.Generic.IList<T>, System.Collections.IList
        where T : EditorItem
    {
        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<T> fItems = new System.Collections.Generic.List<T>();

        #region IEnumerable<CodeItem> Members

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.Collections.Generic.IEnumerator`1" /> that can be
        ///     used to iterate through the collection.
        /// </returns>
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return fItems.GetEnumerator();
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
            return fItems.GetEnumerator();
        }

        #endregion

        #region IList<EditorItem> Members

        /// <summary>Determines the index of a specific <paramref name="item"/> in the<see cref="System.Collections.Generic.IList`1"/> .</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.IList`1"/> .</param>
        /// <returns>The index of <paramref name="item"/> if found in the list; otherwise,
        ///     -1.</returns>
        public int IndexOf(T item)
        {
            return fItems.IndexOf(item);
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
            if (item != null)
            {
                item.SetSelected(true);
            }

            fItems.Insert(index, item);
        }

        /// <summary>Removes the <see cref="System.Collections.Generic.IList`1"/> item at
        ///     the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.Generic.IList`1"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            if (index > -1 && index > fItems.Count)
            {
                if (fItems[index] != null)
                {
                    fItems[index].SetSelected(false);
                }

                fItems.RemoveAt(index);
            }
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        /// <summary>Gets or sets the <see cref="T"/> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <value></value>
        /// <returns>The <see cref="T"/>.</returns>
        public T this[int index]
        {
            get
            {
                return fItems[index];
            }

            set
            {
                if (fItems[index] != null)
                {
                    fItems[index].SetSelected(false);
                }

                fItems[index] = value;
                if (fItems[index] != null)
                {
                    fItems[index].SetSelected(true);
                }
            }
        }

        #endregion

        #region ICollection<CodeItem> Members

        /// <summary>Adds an <paramref name="item"/> to the<see cref="System.Collections.Generic.ICollection`1"/> .</summary>
        /// <param name="item">The object to add to the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection`1"/> is
        ///     read-only.</exception>
        public void Add(T item)
        {
            if (item != null)
            {
                item.SetSelected(true);
            }

            fItems.Add(item);
        }

        /// <summary>
        ///     Removes all items from the
        ///     <see cref="System.Collections.Generic.ICollection`1" /> .
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     The <see cref="System.Collections.Generic.ICollection`1" /> is
        ///     read-only.
        /// </exception>
        public void Clear()
        {
            foreach (var i in this)
            {
                if (i != null)
                {
                    i.SetSelected(false);
                }
            }

            fItems.Clear();
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.ICollection`1"/> contains a
        ///     specific value.</summary>
        /// <param name="item">The object to locate in the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false.</returns>
        public bool Contains(T item)
        {
            return fItems.Contains(item);
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
            fItems.CopyTo(array, arrayIndex);
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
                return fItems.Count;
            }
        }

        /// <summary>Gets a value indicating whether is read only.</summary>
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
        public bool Remove(T item)
        {
            var iIndex = fItems.IndexOf(item);
            if (iIndex > -1 && item != null)
            {
                fItems[iIndex].SetSelected(false);
                fItems.RemoveAt(iIndex);
                return true;
            }

            return false;
        }

        #endregion

        #region IList Members

        /// <summary>Adds an item to the <see cref="System.Collections.IList"/> .</summary>
        /// <param name="value">The <see cref="object"/> to add to the <see cref="System.Collections.IList"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"/> is read-only. -or- The <see cref="System.Collections.IList"/>
        ///     has a fixed size.</exception>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            var iItem = value as T;
            if (iItem != null)
            {
                Add(iItem);
                return fItems.Count;
            }

            throw new System.NotSupportedException();
        }

        /// <summary>Determines whether the <see cref="System.Collections.IList"/> contains a specific value.</summary>
        /// <param name="value">The <see cref="object"/> to locate in the <see cref="System.Collections.IList"/> .</param>
        /// <returns><see langword="true"/> if the <see cref="object"/> is found in the<see cref="System.Collections.IList"/> ; otherwise, false.</returns>
        public bool Contains(object value)
        {
            var iItem = value as T;
            if (iItem != null)
            {
                return Contains(iItem);
            }

            throw new System.NotSupportedException();
        }

        /// <summary>Determines the index of a specific item in the <see cref="System.Collections.IList"/> .</summary>
        /// <param name="value">The <see cref="object"/> to locate in the <see cref="System.Collections.IList"/> .</param>
        /// <returns>The index of <paramref name="value"/> if found in the list;
        ///     otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            var iItem = value as T;
            if (iItem != null)
            {
                return IndexOf(iItem);
            }

            throw new System.NotSupportedException();
        }

        /// <summary>Inserts an item to the <see cref="System.Collections.IList"/> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be
        ///     inserted.</param>
        /// <param name="value">The <see cref="object"/> to insert into the <see cref="System.Collections.IList"/> .</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid <paramref name="index"/> in
        ///     the <see cref="System.Collections.IList"/> .</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"/> is read-only. -or- The <see cref="System.Collections.IList"/>
        ///     has a fixed size.</exception>
        /// <exception cref="System.NullReferenceException"><paramref name="value"/> is <see langword="null"/> reference in the<see cref="System.Collections.IList"/> .</exception>
        public void Insert(int index, object value)
        {
            var iItem = value as T;
            if (iItem != null)
            {
                Insert(index, iItem);
            }
            else
            {
                throw new System.NullReferenceException();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="System.Collections.IList" /> has a fixed
        ///     size.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="System.Collections.IList" /> has a fixed size;
        ///     otherwise, false.
        /// </returns>
        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the<see cref="System.Collections.IList"/> .</summary>
        /// <param name="value">The <see cref="object"/> to remove from the <see cref="System.Collections.IList"/> .</param>
        /// <exception cref="System.NotSupportedException">The <see cref="System.Collections.IList"/> is read-only. -or- The <see cref="System.Collections.IList"/>
        ///     has a fixed size.</exception>
        public void Remove(object value)
        {
            var iItem = value as T;
            if (iItem != null)
            {
                Remove(iItem);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>Gets or sets the <see cref="object"/> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <value></value>
        /// <returns>The <see cref="object"/>.</returns>
        object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = value as T;
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>Copies the elements of the <see cref="System.Collections.ICollection"/> to an<see cref="System.Array"/> , starting at a particular <see cref="System.Array"/>
        ///     index.</summary>
        /// <param name="array">The one-dimensional <see cref="System.Array"/> that is the destination of
        ///     the elements copied from <see cref="System.Collections.ICollection"/> . The<see cref="System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying
        ///     begins.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="array"/> is multidimensional. -or-<paramref name="index"/> is equal to or greater than the length of<paramref name="array"/> . -or- The number of elements in the source<see cref="System.Collections.ICollection"/> is greater than the available space from<paramref name="index"/> to the end of the destination<paramref name="array"/> .</exception>
        /// <exception cref="System.ArgumentException">The type of the source <see cref="System.Collections.ICollection"/> cannot be cast
        ///     automatically to the type of the destination <paramref name="array"/>
        ///     .</exception>
        public void CopyTo(System.Array array, int index)
        {
            var iArray = array as T[];
            if (iArray != null)
            {
                fItems.CopyTo(iArray, index);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether access to the
        ///     <see cref="System.Collections.ICollection" /> is synchronized (thread safe).
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     <see langword="true" /> if access to the <see cref="System.Collections.ICollection" /> is
        ///     synchronized (thread safe); otherwise, false.
        /// </returns>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the
        ///     <see cref="System.Collections.ICollection" /> .
        /// </summary>
        /// <value>
        /// </value>
        /// <returns>
        ///     An object that can be used to synchronize access to the
        ///     <see cref="System.Collections.ICollection" /> .
        /// </returns>
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}