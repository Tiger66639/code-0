// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronExplorerItem.cs" company="">
//   
// </copyright>
// <summary>
//   The neuron explorer item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The neuron explorer item.</summary>
    public class NeuronExplorerItem : ExplorerItem, INeuronInfo, INeuronWrapper
    {
        /// <summary>
        ///     Gets a value indicating whether this instance has any code associated
        ///     with it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has code; otherwise, <c>false</c> .
        /// </value>
        public bool HasCode
        {
            get
            {
                var iItem = Item;
                if (iItem != null)
                {
                    if (iItem.RulesCluster != null || iItem.ActionsCluster != null)
                    {
                        return true;
                    }

                    if (IsClusterCode())
                    {
                        return true;
                    }

                    if (iItem is ExpressionsBlock && ((ExpressionsBlock)iItem).StatementsCluster != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        #region NeuronTypeName

        /// <summary>
        ///     Gets the name of the type of neuron we wrap.
        /// </summary>
        /// <remarks>
        ///     Used as the tooltip of the image
        /// </remarks>
        public string NeuronTypeName
        {
            get
            {
                if (Item != null)
                {
                    return Item.GetType().Name;
                }

                return null;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets wether this item is currently selected or not.
        /// </summary>
        /// <remarks>
        ///     Can't be assigneable from xaml, cause when this prop is updated, the
        ///     selection range of the explorer might also have to be updated (but not
        ///     always, when it's in the children list). That's why the selection is
        ///     controlled from the xaml explorer part.
        /// </remarks>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            internal set
            {
                if (value != fIsSelected)
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>Gets the neuron info.</summary>
        public NeuronData NeuronInfo { get; private set; }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the <see cref="Neuron" /> item that this object wraps.
        /// </summary>
        /// <remarks>
        ///     When null, it indicates there was an error found at the specified
        ///     index.
        /// </remarks>
        public Neuron Item { get; private set; }

        #endregion

        #region IsClusterCode

        /// <summary>
        ///     Determines whether the current item is a cluster code].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [is cluster code]; otherwise, <c>false</c> .
        /// </returns>
        private bool IsClusterCode()
        {
            var iItem = Item as NeuronCluster;
            if (iItem != null)
            {
                using (var iList = iItem.Children) return iList.Count > 0 && iItem.Meaning == (ulong)PredefinedNeurons.Code;
            }

            return false;
        }

        #endregion

        #region fields

        /// <summary>The f is default meaning.</summary>
        private bool fIsDefaultMeaning;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NeuronExplorerItem"/> class.</summary>
        /// <param name="id">The id.</param>
        public NeuronExplorerItem(ulong id)
            : base(id)
        {
            Item = Brain.Current[id];
            InternalCreate();
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronExplorerItem"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public NeuronExplorerItem(Neuron toWrap)
            : base(toWrap.ID)
        {
            Item = toWrap;
            InternalCreate();
        }

        /// <summary>The internal create.</summary>
        private void InternalCreate()
        {
            if (Item != null)
            {
                NeuronInfo = BrainData.Current.NeuronInfo[Item];
                System.Diagnostics.Debug.Assert(NeuronInfo != null);

                    // should be true -> braindata must always return something.
                fIsDefaultMeaning = BrainData.Current.DefaultMeaningIds.Contains(Item.ID);
            }
            else
            {
                fIsDefaultMeaning = false;
            }
        }

        #endregion

        #region IsDefaultMeaning

        /// <summary>
        ///     Gets/sets if this item is in the
        ///     <see cref="JaStDev.HAB.Designer.BrainData.DefaultMeanings" /> list.
        /// </summary>
        public bool IsDefaultMeaning
        {
            get
            {
                return fIsDefaultMeaning;
            }

            set
            {
                if (fIsDefaultMeaning != value && Item != null)
                {
                    if (value)
                    {
                        BrainData.Current.DefaultMeaningIds.Add(Item.ID);
                    }
                    else
                    {
                        BrainData.Current.DefaultMeaningIds.Remove(Item.ID);
                    }
                }
            }
        }

        /// <summary>Raises the events and stores the new value. Isn't done in the setter,
        ///     cause this is called in response to changes of the<see cref="JaStDev.HAB.Designer.BrainData.DefaultMeaningIds"/> list.
        ///     The setter only updates the list.</summary>
        /// <param name="value"></param>
        internal void SetDefaultMeaning(bool value)
        {
            OnPropertyChanging("IsDefaultMeaning", fIsDefaultMeaning, value);
            fIsDefaultMeaning = value;
            OnPropertyChanged("IsDefaultMeaning");
        }

        #endregion
    }
}