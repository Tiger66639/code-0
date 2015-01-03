// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetItem.cs" company="">
//   
// </copyright>
// <summary>
//   A single value in an asset.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A single value in an asset.
    /// </summary>
    public class AssetItem : AssetBase, IAssetOwner
    {
        /// <summary>Initializes a new instance of the <see cref="AssetItem"/> class. Initializes a new instance of the <see cref="AsettItem"/> class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        public AssetItem(Neuron toWrap)
            : base(toWrap)
        {
        }

        #region Assets

        /// <summary>
        ///     Gets the assets connected to the object.
        /// </summary>
        public AssetCollection Assets
        {
            get
            {
                return fAssets;
            }

            internal set
            {
                if (value != fAssets)
                {
                    if (fAssets != null)
                    {
                        fAssets.IsActive = false;

                            // need to make certain that the previous list is detached from the event system , cuae it is no longer used.
                    }

                    fAssets = value;
                    OnPropertyChanged("Assets");
                    HasChildrenChanged();
                    OnPropertyChanged("TreeItems");
                    var iArgs = new Data.CascadedPropertyChangedEventArgs(
                        this, 
                        new System.ComponentModel.PropertyChangedEventArgs("TreeItems"));

                        // need to let the treeViewPanel know that the TreeItems list was changed, otherwise we don't get a refresh when removed.
                    JaStDev.Data.EventEngine.OnPropertyChanged(this, iArgs);
                }
            }
        }

        #endregion

        #region TreeItems

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>
        ///     The tree items.
        /// </value>
        public override System.Collections.IList TreeItems
        {
            get
            {
                return Assets;
            }
        }

        #endregion

        #region Data

        /// <summary>
        ///     gets the list of data items: value, where, when,...
        /// </summary>
        public System.Collections.Generic.List<AssetData> Data
        {
            get
            {
                return fData;
            }
        }

        #endregion

        /// <summary>Gets or sets the owner.</summary>
        public override IAssetOwner Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                base.Owner = value;
                if (value != null)
                {
                    fEventMonitor = new AssetEventMonitor(this);
                    LoadLinkedNeurons();
                }
                else if (fEventMonitor != null)
                {
                    fEventMonitor.Unregister();
                    fEventMonitor = null;
                }
            }
        }

        #region IAssetOwner Assets

        /// <summary>
        ///     Gets the list of items
        /// </summary>
        /// <value>
        /// </value>
        System.Collections.Generic.IList<AssetBase> IAssetOwner.Assets
        {
            get
            {
                return fAssets;
            }
        }

        #endregion

        /// <summary>
        ///     Loads the linked neurons (the data). This is done when the item is
        ///     added to the list of the asset cause we need to get the root for
        ///     getting to the columns that need to be loaded.
        /// </summary>
        private void LoadLinkedNeurons()
        {
            fAttribute = Item.FindFirstOut((ulong)PredefinedNeurons.Attribute);

            var iRoot = Root;
            if (iRoot != null)
            {
                // while importing asset data through xml this value can be null, so check for this. shouldn't be a problem cause this is only while loading the asset object. 
                for (var iCount = 1; iCount < iRoot.Columns.Count; iCount++)
                {
                    var i = iRoot.Columns[iCount];
                    var iFound = Item.FindFirstOut(i.LinkID);
                    if (i.LinkID != (ulong)PredefinedNeurons.Attribute)
                    {
                        // amount is stored in the asset itself.
                        var iNew = new AssetData(iFound, i);
                        iNew.Owner = this;
                        fData.Add(iNew);
                    }
                }
            }
        }

        /// <summary>Sets the expanded value. Allows descendents to do something extra
        ///     while expanding/collapsing.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected override void SetExpanded(bool value)
        {
            if (value)
            {
                var iValue = Data[Root.TreeColumn - 1].Value;
                if (iValue is NeuronCluster && iValue != null)
                {
                    // if there is no value, we can not expand, so don't try to.
                    Assets = new AssetCollection(this, (NeuronCluster)iValue);
                    base.SetExpanded(value);
                }
            }
            else
            {
                base.SetExpanded(value);
                Assets = null;
            }
        }

        /// <summary>Updates the 'assets' list for the specified column index. When this
        ///     column has sub assets, they are made active. The previous assets list
        ///     is removed.</summary>
        /// <param name="value"></param>
        internal void UpdateAssetsForCol(int value)
        {
            if (Data[value].Value is NeuronCluster
                && ((NeuronCluster)Data[value].Value).Meaning == (ulong)PredefinedNeurons.Asset)
            {
                Assets = new AssetCollection(this, (NeuronCluster)Data[value].Value);
                Data[value].Presentation = "asset";
            }
            else
            {
                Assets = null;
            }
        }

        #region fields

        /// <summary>The f attribute.</summary>
        private Neuron fAttribute;

        /// <summary>The f assets.</summary>
        private AssetCollection fAssets;

        /// <summary>The f event monitor.</summary>
        private AssetEventMonitor fEventMonitor;

        /// <summary>The f data.</summary>
        private readonly System.Collections.Generic.List<AssetData> fData =
            new System.Collections.Generic.List<AssetData>();

        #endregion

        #region Attribute

        /// <summary>
        ///     Gets/sets the neuron assigned as attribute to the asset.
        /// </summary>
        public Neuron Attribute
        {
            get
            {
                return fAttribute;
            }

            set
            {
                if (value != fAttribute)
                {
                    if (Item != null)
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.Attribute, value);
                    }

                    // no prop changed call, this is done by the monitor.
                }
            }
        }

        /// <summary>Sets the attribute and raises the event, can be used by the monitor.</summary>
        /// <param name="value">The value.</param>
        internal void SetAttribute(Neuron value)
        {
            fAttribute = value;
            OnPropertyChanged("Attribute");
        }

        #endregion

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not.
        ///     When the list of children changes (becomes empty or gets the first
        ///     item), this should be raised when appropriate through a
        ///     propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public override bool HasChildren
        {
            get
            {
                if (Root != null)
                {
                    var iData = Data[Root.TreeColumn - 1];
                    if (iData != null)
                    {
                        return iData.HasChildren;
                    }

                    return false;
                }

                return false;
            }
        }

        /// <summary>
        ///     Raises the event so that the ui knowns of the change.
        /// </summary>
        internal void HasChildrenChanged()
        {
            OnPropertyChanged("HasChildren");
        }

        #endregion
    }
}