// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VarDict.cs" company="">
//   
// </copyright>
// <summary>
//   A variable dictionaty which tries to preserve as much mem as possible by
//   reusing the data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A variable dictionaty which tries to preserve as much mem as possible by
    ///     reusing the data.
    /// </summary>
    public class VarDict :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Variable, VarValuesList>>
    {
        /// <summary>Initializes a new instance of the <see cref="VarDict"/> class.</summary>
        /// <param name="notUsed">The not used.</param>
        public VarDict(System.Collections.Generic.Stack<VarValuesList> notUsed)
        {
            fNotUsed = notUsed;
        }

        /// <summary>The this.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        public VarValuesList this[Variable index]
        {
            get
            {
                return fDict[index];
            }
        }

        #region IEnumerable<KeyValuePair<ulong,List<Neuron>>> Members

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<Variable, VarValuesList>> GetEnumerator()
        {
            return fDict.GetEnumerator();
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
            return fDict.GetEnumerator();
        }

        #endregion

        /// <summary>Adds a new list and returns it for the specified index. If the<paramref name="index"/> is already stored, it will be reset.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        public VarValuesList Add(Variable index)
        {
            var iRes = GetList(index);
            iRes.Data = Factories.Default.NLists.GetBuffer();
            return iRes;
        }

        /// <summary>The get list.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        private VarValuesList GetList(Variable index)
        {
            VarValuesList iRes;
            if (fNotUsed.Count > 0)
            {
                iRes = fNotUsed.Pop();
            }
            else
            {
                iRes = new VarValuesList();
            }

            iRes.Owner = this;
            fDict[index] = iRes;

                // original fDict.Add(index, iRes) -> if it already exists=problem. Can be if we want to clear the list
            return iRes;
        }

        /// <summary>Adds a <paramref name="list"/> that can't be recycled, but which needs
        ///     to be removed when done. This is provided for the 'callSave'
        ///     instruction, which allows some variables to be shared into the next
        ///     dict.</summary>
        /// <param name="index"></param>
        /// <param name="list"></param>
        internal void AddShared(Variable index, VarValuesList list)
        {
            fDict.Add(index, list);
        }

        /// <summary>stores the specified <paramref name="value"/> in the variable,
        ///     clearing any potential previous values.</summary>
        /// <param name="var"></param>
        /// <param name="value"></param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        internal VarValuesList Set(Variable var, Neuron value)
        {
            VarValuesList iList;
            if (fDict.TryGetValue(var, out iList) == false)
            {
                iList = Add(var);
            }
            else
            {
                iList.Data.Clear();
            }

            iList.Data.Add(value);
            return iList;
        }

        /// <summary>STores the <paramref name="value"/> in the variable. The list is
        ///     consumed.</summary>
        /// <param name="var">The var.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        internal VarValuesList Set(Variable var, System.Collections.Generic.List<Neuron> value)
        {
            VarValuesList iList;
            if (fDict.TryGetValue(var, out iList) == false)
            {
                iList = GetList(var);
            }
            else if (var.SplitReactionID != (ulong)PredefinedNeurons.shared)
            {
                // shared data doesn't get recycled, but GC'ed.
                Factories.Default.NLists.Recycle(iList.Data);

                    // release the previous data before we assign the new one, otherwise we have a leak. This is also done for 'shared' vars. Its' the programmer's responsibility to manage shared vars, if they need a new value, this is allowed.
            }

            iList.Data = value;
            return iList;
        }

        /// <summary>Removes the specified index.</summary>
        /// <param name="index">The index.</param>
        public void Remove(Variable index)
        {
            VarValuesList iList;
            if (fDict.TryGetValue(index, out iList))
            {
                // could be that the key is not stored.
                fDict.Remove(index);
                if (iList.Owner == this)
                {
                    if (Processor.CurrentProcessor != null && iList.Data != null)
                    {
                        if (index.SplitReactionID != (ulong)PredefinedNeurons.shared)
                        {
                            // don't ever recycle a list from a shared var: that could cause memory problems.
                            Factories.Default.NLists.Recycle(iList.Data);
                        }

                        iList.Data = null;
                    }

                    fNotUsed.Push(iList);
                }
            }
        }

        /// <summary>
        ///     Clears the data used by this instance so taht it can be recycled.
        /// </summary>
        internal void Clear()
        {
            var iMemFac = Factories.Default;
            foreach (var i in fDict)
            {
                if (i.Value.Owner == this)
                {
                    if (i.Key.SplitReactionID != (ulong)PredefinedNeurons.shared)
                    {
                        // don't recycle the content of shared vars, that could cause problems, simply let the gc handle this.
                        iMemFac.NLists.Recycle(i.Value.Data);
                    }

                    i.Value.Data = null;
                    fNotUsed.Push(i.Value);
                }
            }

            fDict.Clear();
        }

        /// <summary>Tries the get value.</summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool TryGetValue(Variable index, out VarValuesList value)
        {
            return fDict.TryGetValue(index, out value);
        }

        /// <summary>The contains key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool ContainsKey(Variable key)
        {
            return fDict.ContainsKey(key);
        }

        #region fields

        /// <summary>The f not used.</summary>
        private readonly System.Collections.Generic.Stack<VarValuesList> fNotUsed;

                                                                         // stores al the lists that have already been created but which have been recycled. Supplied by the stack that owns this dict, so it can be shared accross many dicts.

        /// <summary>The f dict.</summary>
        private readonly System.Collections.Generic.Dictionary<Variable, VarValuesList> fDict =
            new System.Collections.Generic.Dictionary<Variable, VarValuesList>();

        #endregion
    }
}