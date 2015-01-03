// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkOutIndexerItem.cs" company="">
//   
// </copyright>
// <summary>
//   stores the indexes for a single neuron in the
//   <see cref="FirstLinkOutIndexer" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Indexing
{
    /// <summary>
    ///     stores the indexes for a single neuron in the
    ///     <see cref="FirstLinkOutIndexer" /> class.
    /// </summary>
    public class LinkOutIndexerItem
    {
        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.Dictionary<System.Type, object> fItems =
            new System.Collections.Generic.Dictionary<System.Type, object>();

        /// <summary>Initializes a new instance of the <see cref="LinkOutIndexerItem"/> class. Initializes a new instance of the <see cref="LinkOutIndexerItem"/>
        ///     class.</summary>
        /// <param name="toAdd">To add.</param>
        public LinkOutIndexerItem(JaStDev.HAB.Neuron toAdd)
        {
            var iLinks = JaStDev.HAB.Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iOut = toAdd.LinksOut) iLinks.AddRange(iOut);
                foreach (var i in iLinks)
                {
                    TryAddValue(i.Meaning, i.ToID);
                }
            }
            finally
            {
                JaStDev.HAB.Factories.Default.LinkLists.Recycle(iLinks);
            }
        }

        /// <summary>
        ///     keeps track of the nr of times that this item was accessed.
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>Checks if the <paramref name="key"/> is to be indexed and if so, adds
        ///     the <paramref name="value"/> if this is the first<paramref name="value"/> for the specified index position.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void TryAddValue(JaStDev.HAB.Neuron key, ulong value)
        {
            if (key.ID == (ulong)JaStDev.HAB.PredefinedNeurons.True)
            {
                SetValue(true, value);
            }
            else if (key.ID == (ulong)JaStDev.HAB.PredefinedNeurons.False)
            {
                SetValue(false, value);
            }
            else
            {
                var iType = key.GetType();
                if (iType == typeof(JaStDev.HAB.IntNeuron))
                {
                    SetValue(((JaStDev.HAB.IntNeuron)key).Value, value);
                }
                else if (iType == typeof(JaStDev.HAB.DoubleNeuron))
                {
                    SetValue(((JaStDev.HAB.DoubleNeuron)key).Value, value);
                }
                else if (iType == typeof(JaStDev.HAB.TextNeuron))
                {
                    SetValue(((JaStDev.HAB.TextNeuron)key).Text, value);
                }
            }
        }

        /// <summary>looks up the dictionary for the <see langword="bool"/> type and
        ///     returns the hashSet assigned for the value. If none is found yet, one
        ///     is created.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void SetValue<T>(T key, ulong value)
        {
            System.Collections.Generic.Dictionary<T, ulong> iIndex;
            object iFound;
            if (fItems.TryGetValue(typeof(T), out iFound))
            {
                iIndex = iFound as System.Collections.Generic.Dictionary<T, ulong>;
            }
            else
            {
                iIndex = new System.Collections.Generic.Dictionary<T, ulong>();
                fItems.Add(typeof(T), iIndex);
            }

            if (iIndex.ContainsKey(key) == false)
            {
                // we only store the first link value cause we do a 'findFirst' approach.
                iIndex.Add(key, value);
            }
        }

        /// <summary>Looks up the <paramref name="value"/> in the index and return the id
        ///     of the first outgoing referenced neuron.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        public ulong FindFirst<T>(T value)
        {
            object iFound;
            if (fItems.TryGetValue(value.GetType(), out iFound))
            {
                var iIndx = iFound as System.Collections.Generic.Dictionary<T, ulong>;
                if (iIndx != null)
                {
                    ulong iRes;
                    if (iIndx.TryGetValue(value, out iRes))
                    {
                        return iRes;
                    }
                }
            }

            return 0;
        }

        /// <summary>Looks up the <paramref name="key"/> and if it stores the same value,
        ///     the <paramref name="key"/> is removed.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void TryRemoveValue(JaStDev.HAB.Neuron key, ulong value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The try replace value.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="old">The old.</param>
        /// <param name="replacement">The replacement.</param>
        /// <exception cref="NotImplementedException"></exception>
        internal void TryReplaceValue(JaStDev.HAB.Neuron neuron, ulong old, ulong replacement)
        {
            throw new System.NotImplementedException();
        }
    }
}