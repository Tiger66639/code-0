// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedSmallIDCollection.cs" company="">
//   
// </copyright>
// <summary>
//   An id collection that stores a list of neurons which need to be monitored
//   for deletion, and which are used by the parsers to map
//   <see langword="static" /> words/identifiers to neurons. This is used for
//   things like 'present particle', 'user', 'bot',...
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An id collection that stores a list of neurons which need to be monitored
    ///     for deletion, and which are used by the parsers to map
    ///     <see langword="static" /> words/identifiers to neurons. This is used for
    ///     things like 'present particle', 'user', 'bot',...
    /// </summary>
    /// <remarks>
    ///     Note: all strings are stored in lower case, so case insensitive. Auto
    ///     assigns itself to the <see cref="ParserBase.Statistics" />
    /// </remarks>
    public class MappedSmallIDCollection : SmallIDCollection
    {
        /// <summary>The f map.</summary>
        private readonly System.Collections.Generic.Dictionary<string, ulong> fMap =
            new System.Collections.Generic.Dictionary<string, ulong>();

        #region Map

        /// <summary>
        ///     Gets the map as a dictionary, for fast access by parsers.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, ulong> Map
        {
            get
            {
                return fMap;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the owner.
        /// </summary>
        /// <remarks>
        ///     When we assign the owner, we can also build the dictionary. When an
        ///     already existing list gets loaded, the names for the neurons aren't
        ///     known yet, so we need to wait untill everything gets registered.
        /// </remarks>
        /// <value>
        ///     The owner.
        /// </value>
        public override object Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                base.Owner = value;
                if (Owner != null && Count != fMap.Count && BrainData.Current.NeuronInfo != null)
                {
                    fMap.Clear();
                    foreach (var i in this)
                    {
                        var iData = BrainData.Current.NeuronInfo[i];
                        var iKey = iData.DisplayTitle.ToLower();
                        if (fMap.ContainsKey(iKey) == false)
                        {
                            // make certain that there is no problem with duplicates.
                            fMap.Add(iKey, i);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            fMap.Clear();
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            base.ClearItemsDirect();
            fMap.Clear();
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, ulong item)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                var iData = BrainData.Current.NeuronInfo[item];
                fMap.Add(iData.DisplayTitle.ToLower(), item);
            }

            base.InsertItem(index, item);

                // do this after trying to add to the map, so we avoid duplicates being stored in the list.
        }

        /// <summary>Inserts the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItemDirect(int index, ulong item)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                var iData = BrainData.Current.NeuronInfo[item];
                fMap.Add(iData.DisplayTitle.ToLower(), item);
            }

            base.InsertItemDirect(index, item);

                // do this after trying to add to the map, so we avoid duplicates being stored in the list.
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                NeuronData iData = null;
                if (Brain.Current.IsValidID(this[index]))
                {
                    iData = BrainData.Current.NeuronInfo[this[index]];
                }

                base.RemoveItem(index);
                if (iData != null)
                {
                    // check if the object wasn't deleted already, if so, we need to remove the mapping in the slow way.
                    fMap.Remove(iData.DisplayTitle.ToLower());
                }
                else
                {
                    RemoveMappingFor(this[index]);
                }
            }
            else
            {
                base.RemoveItem(index);
            }
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                var iId = this[index];
                string iData = null;
                if (Brain.Current.IsValidID(iId))
                {
                    iData = BrainData.Current.NeuronInfo.GetDisplayTitleFor(iId);

                        // only need the display title, so this can de done quick.
                }

                base.RemoveItemDirect(index);
                if (iData != null)
                {
                    // check if the object wasn't deleted already, if so, we need to remove the mapping in the slow way.
                    fMap.Remove(iData.ToLower());
                }
                else
                {
                    RemoveMappingFor(iId);
                }
            }
            else
            {
                base.RemoveItemDirect(index);
            }
        }

        /// <summary>Sets the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, ulong item)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                NeuronData iData = null;
                if (Brain.Current.IsValidID(this[index]))
                {
                    iData = BrainData.Current.NeuronInfo[this[index]];
                }

                var iKey = iData.DisplayTitle.ToLower();
                if (iData != null)
                {
                    // check if the object wasn't deleted already, if so, we need to remove the mapping in the slow way.
                    fMap.Remove(iKey);
                }
                else
                {
                    RemoveMappingFor(this[index]);
                }

                iData = BrainData.Current.NeuronInfo[item];
                fMap.Add(iKey, item);
                base.SetItem(index, item);
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        /// <summary>Sets the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItemDirect(int index, ulong item)
        {
            if (BrainData.Current.NeuronInfo != null)
            {
                // when the project gets loaded, we do inserts, but the neuron info isn't known yet, so we must defer building the map until later.
                NeuronData iData = null;
                if (Brain.Current.IsValidID(this[index]))
                {
                    iData = BrainData.Current.NeuronInfo[this[index]];
                }

                if (iData != null)
                {
                    // check if the object wasn't deleted already, if so, we need to remove the mapping in the slow way.
                    fMap.Remove(iData.DisplayTitle.ToLower());
                }
                else
                {
                    RemoveMappingFor(this[index]);
                }

                iData = BrainData.Current.NeuronInfo[item];
                fMap.Add(iData.DisplayTitle.ToLower(), item);
                base.SetItemDirect(index, item);
            }
            else
            {
                base.SetItemDirect(index, item);
            }
        }

        /// <summary>The remove mapping for.</summary>
        /// <param name="item">The item.</param>
        private void RemoveMappingFor(ulong item)
        {
            string iVal = null;
            foreach (var i in fMap)
            {
                if (i.Value == item)
                {
                    iVal = i.Key;
                }
            }

            if (iVal != null)
            {
                fMap.Remove(iVal);
            }
        }

        /// <summary>Walks through all the keys untill it can build a name with the
        ///     specified string and a nr appended to it, that forms a unique value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetUnique(string value)
        {
            var i = 1;
            var iBuild = new System.Text.StringBuilder(value);
            ulong iTemp;
            var iRes = value;
            while (Map.TryGetValue(iRes.ToLower(), out iTemp))
            {
                iBuild.Clear();
                iBuild.Append(value);
                iBuild.Append(i);
                iRes = iBuild.ToString();
                i++;
            }

            return iRes;
        }

        /// <summary>Determines whether the specified <paramref name="value"/> is unique in
        ///     the dict.</summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified <paramref name="value"/> is unique;
        ///     otherwise, <c>false</c> .</returns>
        public bool IsUnique(string value)
        {
            return !Map.ContainsKey(value.ToLower());
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MappedSmallIDCollection"/> class. 
        ///     Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        public MappedSmallIDCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MappedSmallIDCollection"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public MappedSmallIDCollection(object owner)
            : base(owner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MappedSmallIDCollection"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="items">The items.</param>
        public MappedSmallIDCollection(System.Collections.Generic.List<ulong> items)
            : base(items)
        {
        }

        #endregion
    }
}