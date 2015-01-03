// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitResultsDict.cs" company="">
//   
// </copyright>
// <summary>
//   A dictionary that is thread safe, used by the procesor to store all the
//   split results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     A dictionary that is thread safe, used by the procesor to store all the
    ///     split results.
    /// </summary>
    public class SplitResultsDict
    {
        #region fields

        /// <summary>The f dict.</summary>
        private System.Collections.Generic.Dictionary<Neuron, double> fDict =
            new System.Collections.Generic.Dictionary<Neuron, double>();

                                                                      // we use neurons, not id's. So we can keep track of deleted items (id ==0), without having to monitor the brain for a delete (and have a lock on all the processors for each delete, which would be costly). Since these values are only accessed at specific times, we can check for 0 id's.
        #endregion

        #region CurrentWeight

        /// <summary>
        ///     Gets the current weight that is assigned to the processor and which
        ///     will be applied to all the items when the processor is done.
        /// </summary>
        /// <remarks>
        ///     We allow for an <see langword="internal" /> setter. This is done from
        ///     the appropriate instructions, done on the processor itself, in the
        ///     same thread. The weight is only changed in another thread when the
        ///     proc is done, so don't worry about thread safety.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public double CurrentWeight { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets the nr of items currently stored in the results list.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        public int Count
        {
            get
            {
                return fDict.Count;
            }
        }

        /// <summary>
        ///     Applies the current weight to all the neurons that are stores as split
        ///     results. This allows us to correctly get the values of the final
        ///     processor as well.
        /// </summary>
        internal void ApplyWeight()
        {
            lock (fDict)
            {
                foreach (var i in fDict.Keys.ToArray())
                {
                    // we need to a make a local copy otherwise we get a foreach enum error, bugger.
                    fDict[i] += CurrentWeight;
                }
            }
        }

        /// <summary>Gets the weight of the specified neuron. If there is no weight
        ///     assigned, 0 is returned.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="double"/>.</returns>
        internal double GetWeightOf(Neuron value)
        {
            lock (fDict)
            {
                double iVal;
                if (fDict.TryGetValue(value, out iVal))
                {
                    return iVal;
                }

                return 0;
            }
        }

        /// <summary>increases the weight of a single neuron.</summary>
        /// <param name="toSet"></param>
        /// <param name="value"></param>
        internal void IncreaseWeightOf(Neuron toSet, double value)
        {
            lock (fDict)
            {
                double iVal;
                if (fDict.TryGetValue(toSet, out iVal))
                {
                    fDict[toSet] = iVal + value;
                }
                else
                {
                    fDict.Add(toSet, value);
                }
            }
        }

        /// <summary>decreases the weight of a single neuron.</summary>
        /// <param name="toSet"></param>
        /// <param name="value"></param>
        internal void DecreaseWeightOf(Neuron toSet, double value)
        {
            lock (fDict)
            {
                double iVal;
                if (fDict.TryGetValue(toSet, out iVal))
                {
                    fDict[toSet] = iVal - value;
                }
                else
                {
                    fDict.Add(toSet, -value);
                }
            }
        }

        #region functions

        /// <summary>Provides a quick, thread safe way to init the list with the<paramref name="source"/> data. A new splitResultValue is created for
        ///     each entry in the source. Value and weight are both copied to the new
        ///     objects. A copy is made so the new list can have original weight
        ///     values. The <see cref="CurrentWeight"/> of the<paramref name="source"/> processor is also taken over, so that splits
        ///     keep incrementing their own results.</summary>
        /// <param name="source">The source.</param>
        public void InitFrom(SplitResultsDict source)
        {
            lock (fDict)
            {
                fDict.Clear();
                lock (source.fDict)
                {
                    foreach (var i in source.fDict)
                    {
                        if (i.Key.ID != Neuron.EmptyId)
                        {
                            // remove deleted items.
                            fDict.Add(i.Key, i.Value);
                        }
                    }

                    CurrentWeight = source.CurrentWeight;
                }
            }
        }

        /// <summary>Sets the weight if it has not yet been set before (ContainsKey +
        ///     Setter in 1 lock).</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddSplitResult(Neuron key, double value)
        {
            lock (fDict)
            {
                if (fDict.ContainsKey(key) == false)
                {
                    fDict[key] = value;
                }
            }
        }

        /// <summary>Determines whether the neuron with the specified id, is stored in the
        ///     split-results dict.</summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if the specified <paramref name="id"/> is stored;
        ///     otherwise, <c>false</c> .</returns>
        public bool Contains(Neuron id)
        {
            lock (fDict) return fDict.ContainsKey(id);
        }

        /// <summary>
        ///     Clears all the items from the results dict. Also resets the Weight
        ///     value.
        /// </summary>
        internal void Clear()
        {
            lock (fDict)
            {
                CurrentWeight = 0; // because it is within the lock, it is garanteed thread save.
                fDict = new System.Collections.Generic.Dictionary<Neuron, double>();

                    // recreate field: dictionary is internally not thread save (volatile not used), cpu-core's cache can give problems
            }
        }

        /// <summary>returns all the values that are currently stored in the dict, as a new
        ///     dictionary. This is thread safe.</summary>
        /// <value>The get all values.</value>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        public System.Collections.Generic.Dictionary<Neuron, double> GetAllValues()
        {
            var iRes = new System.Collections.Generic.Dictionary<Neuron, double>();
            lock (fDict)
            {
                foreach (var i in fDict)
                {
                    if (i.Key.ID != Neuron.EmptyId)
                    {
                        iRes.Add(i.Key, i.Value);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Gets all the neurons that have a weight assigned in this dict.</summary>
        /// <param name="dest">The dest.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<Neuron> GetNeurons(System.Collections.Generic.List<Neuron> dest = null)
        {
            var iRes = dest;
            if (iRes == null)
            {
                iRes = new System.Collections.Generic.List<Neuron>();
            }

            lock (fDict)
            {
                var iSorted = from i in fDict orderby i.Value descending select i;
                foreach (var i in iSorted)
                {
                    if (i.Key.ID != Neuron.EmptyId)
                    {
                        iRes.Add(i.Key);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Gets the neurons with the maximum weights.</summary>
        /// <param name="dest">The dest.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        internal System.Collections.Generic.IEnumerable<Neuron> GetMaxWeights(System.Collections.Generic.List<Neuron> dest)
        {
            var iRes = dest;
            if (iRes == null)
            {
                iRes = new System.Collections.Generic.List<Neuron>();
            }

            lock (fDict)
            {
                var iSorted = from i in fDict orderby i.Value descending select i;
                var iWeight = double.MinValue;
                foreach (var i in iSorted)
                {
                    if (i.Key.ID != Neuron.EmptyId)
                    {
                        if (i.Value < iWeight)
                        {
                            // the first time, iWeight = 0, so always smaller or equal to the weight of the first element, so this will always fail, and the value can be init. The next time, iWeight has the correct value.
                            break;
                        }

                        iWeight = i.Value;
                        iRes.Add(i.Key);
                    }
                }
            }

            return iRes;
        }

        /// <summary>Removes the neuron with specified id from the results dict.</summary>
        /// <param name="key">The key.</param>
        internal void Remove(Neuron key)
        {
            lock (fDict) fDict.Remove(key);
        }

        /// <summary>Copies the entire content of this results dict <paramref name="to"/>
        ///     the specified dict, while incrementing the weight of each result with
        ///     that of the processor.</summary>
        /// <param name="to">The to.</param>
        /// <param name="isAccum">The is Accum.</param>
        internal void CopyTo(SplitResultsDict to, bool isAccum)
        {
            double iItem;
            lock (fDict)
            {
                lock (to.fDict)
                {
                    if (isAccum == false)
                    {
                        foreach (var i in fDict)
                        {
                            if (i.Key.ID != Neuron.EmptyId)
                            {
                                var iVal = i.Value + CurrentWeight;
                                if (to.fDict.TryGetValue(i.Key, out iItem))
                                {
                                    to.fDict[i.Key] = System.Math.Max(iItem, iVal);
                                }
                                else
                                {
                                    to.fDict.Add(i.Key, iVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var i in fDict)
                        {
                            if (i.Key.ID != Neuron.EmptyId)
                            {
                                var iVal = i.Value + CurrentWeight;
                                if (to.fDict.TryGetValue(i.Key, out iItem))
                                {
                                    to.fDict[i.Key] = iItem + iVal;
                                }
                                else
                                {
                                    to.fDict.Add(i.Key, iVal);
                                }
                            }
                        }
                    }

                    if (CurrentWeight > to.CurrentWeight)
                    {
                        to.CurrentWeight = CurrentWeight;
                    }
                }
            }
        }

        #endregion
    }
}