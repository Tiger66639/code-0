// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetGroup.cs" company="">
//   
// </copyright>
// <summary>
//   Groups assets together in an And/Or group (all parts are linked together
//   with an AND relationship)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Groups assets together in an And/Or group (all parts are linked together
    ///     with an AND relationship)
    /// </summary>
    public class AssetGroup : AssetBase, IAssetOwner
    {
        /// <summary>The f assets.</summary>
        private readonly AssetCollection fAssets;

        /// <summary>Initializes a new instance of the <see cref="AssetGroup"/> class. Initializes a new instance of the <see cref="AssetAndGroup"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public AssetGroup(NeuronCluster toWrap)
            : base(toWrap)
        {
            fAssets = new AssetCollection(this, toWrap);
            fAssets.CollectionChanged += fAssets_CollectionChanged;
        }

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
                return fAssets;
            }
        }

        #endregion

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
                return fAssets.Count > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the group mode (or/and).
        /// </summary>
        /// <value>
        ///     The group mode.
        /// </value>
        public Neuron GroupMode
        {
            get
            {
                return Brain.Current[fAssets.Cluster.Meaning];
            }

            set
            {
                fAssets.Cluster.Meaning = value.ID;
            }
        }

        #region Assets

        /// <summary>
        ///     Gets the assets connected to the object.
        /// </summary>
        public System.Collections.Generic.IList<AssetBase> Assets
        {
            get
            {
                return fAssets;
            }
        }

        #endregion

        /// <summary>The f assets_ collection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fAssets_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (fAssets.Count - e.NewItems.Count == 0)
                    {
                        OnPropertyChanged("HasChildren");
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (fAssets.Count == 0)
                    {
                        OnPropertyChanged("HasChildren");
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged("HasChildren");
                    break;
                default:
                    break;
            }
        }
    }
}