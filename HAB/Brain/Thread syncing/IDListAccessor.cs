// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDListAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   An accessor for lists of a neuron that contains ids (children or
//   clusteredBy lists)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An accessor for lists of a neuron that contains ids (children or
    ///     clusteredBy lists)
    /// </summary>
    public abstract class IDListAccessor : ListAccessor<ulong>
    {
        /// <summary>Initializes a new instance of the <see cref="IDListAccessor"/> class.</summary>
        internal IDListAccessor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="IDListAccessor"/> class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        public IDListAccessor(System.Collections.Generic.IList<ulong> list, 
            Neuron neuron, 
            LockLevel level, 
            bool writeable)
            : base(list, neuron, level, writeable)
        {
        }

        #region Convertion

        /// <summary>
        ///     Converts a list of ulongs that represent
        ///     <see cref="JaStDev.HAB.Neuron.ID" /> s into neurons.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <exception cref="!:">
        ///     Brain.Current may <see langword="throw" /> an exception, which is
        ///     caught.
        /// </exception>
        /// <returns>
        ///     <see langword="null" /> if the convertion failed, otherwise a list with
        ///     expressions.
        /// </returns>
        public System.Collections.Generic.List<T> ConvertTo<T>() where T : Neuron
        {
            try
            {
                System.Collections.Generic.List<T> iRes;
                if (typeof(T) == typeof(NeuronCluster))
                {
                    iRes = Factories.Default.CLists.GetBuffer() as System.Collections.Generic.List<T>;
                }
                else if (typeof(T) == typeof(Neuron))
                {
                    iRes = Factories.Default.NLists.GetBuffer() as System.Collections.Generic.List<T>;
                }
                else
                {
                    iRes = new System.Collections.Generic.List<T>(List.Count);
                }

                var iIds = Factories.Default.IDLists.GetBuffer(List.Count);
                try
                {
                    Lock();
                    try
                    {
                        iIds.AddRange(List);
                    }
                    finally
                    {
                        Unlock();
                    }

                    foreach (var i in iIds)
                    {
                        var iFound = Brain.Current[i] as T;
                        if (iFound != null)
                        {
                            iRes.Add(iFound);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return iRes;
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iIds);
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("IDListAccessor.ConvertTo<>", e.ToString());
                return null;
            }
        }

        #endregion

        /// <summary>Adds a range of id's to the list in a thread safe way.</summary>
        /// <param name="items">The items.</param>
        public void AddRange(System.Collections.Generic.IEnumerable<Neuron> items)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            Lock(items);
            try
            {
                ((BrainList)List).AddRange(items);
            }
            finally
            {
                Unlock(items);
            }
        }

        /// <summary>Adds a range of ids.</summary>
        /// <param name="items">The items.</param>
        public void AddRange(System.Collections.Generic.IEnumerable<ulong> items)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            var iItems = Lock(items);
            try
            {
                ((BrainList)List).AddRange(items);
            }
            finally
            {
                Unlock(iItems);
                Factories.Default.NLists.Recycle(iItems);
            }
        }

        /// <summary>Removes the specified neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        public void Remove(Neuron neuron)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            Lock(neuron);
            try
            {
                ((BrainList)List).Remove(neuron);
            }
            finally
            {
                Unlock(neuron);
            }
        }

        /// <summary>Removes the specified neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="index">The index.</param>
        public void Remove(Neuron neuron, int index)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            Lock(neuron);
            try
            {
                ((BrainList)List).Remove(neuron, index);
            }
            finally
            {
                Unlock(neuron);
            }
        }

        /// <summary>remvoes the <paramref name="item"/> without locking anything.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool RemoveUnsafe(Neuron item)
        {
            var iList = (BrainList)List;
            return iList.Remove(item);
        }

        /// <summary>remvoes the <paramref name="item"/> without locking anything.</summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool RemoveUnsafe(Neuron item, int index)
        {
            var iList = (BrainList)List;
            if (index >= 0 && iList.Count > index)
            {
                iList.Remove(item, index);
                return true;
            }

            return false;
        }

        /// <summary>Adds the specified neuron.</summary>
        /// <param name="neuron">The neuron.</param>
        public void Add(Neuron neuron)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            Lock(neuron);
            try
            {
                ((BrainList)List).Add(neuron);
            }
            finally
            {
                Unlock(neuron);
            }
        }

        /// <summary>Adds the <paramref name="item"/> without locking anything.</summary>
        /// <param name="item">The item.</param>
        public void AddUnsafe(Neuron item)
        {
            var iList = (BrainList)List;
            iList.Add(item);
        }

        /// <summary>Inserts at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <param name="neuron">The neuron.</param>
        public void Insert(int index, Neuron neuron)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            Lock(neuron);
            try
            {
                ((BrainList)List).Insert(index, neuron);
            }
            finally
            {
                Unlock(neuron);
            }
        }

        /// <summary>Inserts at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <param name="neuron">The neuron.</param>
        public void InsertUnsafe(int index, Neuron neuron)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            ((BrainList)List).Insert(index, neuron);
        }

        /// <summary>Moves the item <paramref name="from"/> position.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public void Move(int from, int to)
        {
            Lock(); // the children don't need to be locked, nothing is changed for them.
            try
            {
                System.Diagnostics.Debug.Assert(IsWriteable);
                ((BrainList)List).Move(from, to);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>Moves the items without locking.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public void MoveUnsave(int from, int to)
        {
            System.Diagnostics.Debug.Assert(IsWriteable);
            ((BrainList)List).Move(from, to);
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.ICollection`1"/> contains a
        ///     specific value.</summary>
        /// <param name="toSearch">The object to locate in the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <returns><see langword="true"/> if <paramref name="toSearch"/> is found in the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false.</returns>
        public bool Contains(Neuron toSearch)
        {
            Lock(); // the children don't need to be locked, nothing is changed for them.
            try
            {
                return ((BrainList)List).Contains(toSearch);
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>Clears the list without locking anything. Also makes certain that the
        ///     list of <paramref name="items"/> is correctly updated. The list is
        ///     provided so that they don't have to be retrieved from teh cache during
        ///     the lock (which can cause deadlocks).</summary>
        /// <param name="items">The items.</param>
        public void ClearUnsafe(System.Collections.Generic.List<Neuron> items)
        {
            ((BrainList)List).Clear(items);
        }

        /// <summary>Determines whether the<see cref="System.Collections.Generic.ICollection`1"/> contains a
        ///     specific value. Thread unsave: no locking is done.</summary>
        /// <param name="toSearch">The object to locate in the<see cref="System.Collections.Generic.ICollection`1"/> .</param>
        /// <returns><see langword="true"/> if <paramref name="toSearch"/> is found in the<see cref="System.Collections.Generic.ICollection`1"/> ; otherwise,
        ///     false.</returns>
        public bool ContainsUnsafe(Neuron toSearch)
        {
            return ((BrainList)List).Contains(toSearch);
        }

        /// <summary>locks the current neuron and the specified item.</summary>
        /// <param name="item"></param>
        /// <returns>The other itme that was locked.</returns>
        public override Neuron Lock(ulong item)
        {
            var iItem = Brain.Current[item];
            Lock(iItem);
            return iItem;
        }

        /// <summary>The lock.</summary>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> Lock(ulong a, ulong b)
        {
            var iList = Factories.Default.NLists.GetBuffer();
            iList.Add(Brain.Current[a]);
            iList.Add(Brain.Current[b]);
            Lock(iList);
            return iList;
        }

        /// <summary>Locks the specified items.</summary>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> Lock(System.Collections.Generic.IEnumerable<ulong> items)
        {
            var iList = Factories.Default.NLists.GetBuffer();
            foreach (var i in items)
            {
                iList.Add(Brain.Current[i]);
            }

            Lock(iList);
            return iList;
        }

        /// <summary>The lock.</summary>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public virtual System.Collections.Generic.List<Neuron> Lock(Neuron a, Neuron b)
        {
            if (a.ID == Neuron.TempId)
            {
                // make certain that all temps are added to the cache before locking
                Brain.Current.Add(a);
            }

            if (b.ID == Neuron.TempId)
            {
                // make certain that all temps are added to the cache before locking
                Brain.Current.Add(b);
            }

            var iList = Factories.Default.NLists.GetBuffer();
            iList.Add(a);
            iList.Add(b);
            Lock(iList);
            return iList;
        }

        /// <summary>Locks all.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> LockAll()
        {
            var iTemp = Factories.Default.IDLists.GetBuffer(List.Count);
            Lock();
            iTemp.AddRange(List);
            Unlock();
            var iRes = Lock(iTemp);

                // when done inside current lock, nothing can get in between, but we create a possible deadlock: getting the neurons can cause a cache write, which is dangerous inside another lock.
            Factories.Default.IDLists.Recycle(iTemp);
            return iRes;
        }

        /// <summary>lock the current neuron and the specified items.</summary>
        /// <param name="items">The items.</param>
        public abstract void Lock(System.Collections.Generic.IEnumerable<Neuron> items);

        /// <summary>lock the current neuron and the specified item.</summary>
        /// <param name="item">The item.</param>
        public abstract void Lock(Neuron item);
    }
}