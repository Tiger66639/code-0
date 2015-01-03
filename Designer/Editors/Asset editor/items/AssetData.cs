// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetData.cs" company="">
//   
// </copyright>
// <summary>
//   contains all the data for a single data node in an asset (value, where,
//   when, how,...)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     contains all the data for a single data node in an asset (value, where,
    ///     when, how,...)
    /// </summary>
    public class AssetData : Data.OwnedObject<AssetItem>, INeuronInfo, INeuronWrapper
    {
        /// <summary>Initializes a new instance of the <see cref="AssetData"/> class.</summary>
        /// <param name="value">The value.</param>
        /// <param name="col">The link ID.</param>
        public AssetData(Neuron value, AssetColumn col)
        {
            fValue = value;
            Column = col;
            var iVal = fValue as NeuronCluster;
            if (iVal != null)
            {
                if (iVal.Meaning == (ulong)PredefinedNeurons.Or || iVal.Meaning == (ulong)PredefinedNeurons.And
                    || iVal.Meaning == (ulong)PredefinedNeurons.List
                    || iVal.Meaning == (ulong)PredefinedNeurons.ArgumentsList
                    || iVal.Meaning == (ulong)PredefinedNeurons.Arguments)
                {
                    AsGroup = new AssetValueItemCollection(Owner, iVal, this);
                    fGroupMode = Brain.Current[iVal.Meaning];
                    Presentation = "group";
                }
                else if (iVal.Meaning == (ulong)PredefinedNeurons.Asset)
                {
                    Presentation = "asset"; // don't need to build the list, this is done when expanded.
                }
            }
        }

        #region Value

        /// <summary>
        ///     Gets/sets the neuron assigned as value to the asset.
        /// </summary>
        public Neuron Value
        {
            get
            {
                return fValue;
            }

            set
            {
                if (value != fValue)
                {
                    var iItem = Owner.Item;
                    if (iItem != null)
                    {
                        EditorsHelper.SetFirstOutgoingLinkTo(iItem, Column.LinkID, value);
                    }
                }
            }
        }

        #endregion

        #region LinkID

        /// <summary>
        ///     Gets the meaning of the link that references the data.
        /// </summary>
        public ulong LinkID
        {
            get
            {
                return Column.LinkID;
            }
        }

        #endregion

        #region Column

        /// <summary>
        ///     Gets the column at which this data bit should be displayed.
        /// </summary>
        public AssetColumn Column { get; private set; }

        #endregion

        #region Presentation

        /// <summary>
        ///     Gets/sets the way that the value should be presented: as a group,
        ///     asset or normal. This is for the uI, so it can select the correct
        ///     template.
        /// </summary>
        public string Presentation
        {
            get
            {
                return fPresentation;
            }

            set
            {
                fPresentation = value;
                OnPropertyChanged("Presentation");
            }
        }

        #endregion

        #region AsGroup

        /// <summary>
        ///     Gets or sets the value as collection of neurons.
        /// </summary>
        /// <value>
        ///     The value as group.
        /// </value>
        public AssetValueItemCollection AsGroup
        {
            get
            {
                return fAsGroup;
            }

            set
            {
                if (value != fAsGroup)
                {
                    fAsGroup = value;
                    OnPropertyChanged("AsGroup");
                }
            }
        }

        #endregion

        #region GroupMode

        /// <summary>
        ///     Gets/sets the group mode applied to the value cluster.
        /// </summary>
        public Neuron GroupMode
        {
            get
            {
                return fGroupMode;
            }

            set
            {
                if (value != GroupMode)
                {
                    fGroupMode = value;

                    var iUndo = new ClusterMeaningUndoItem();
                    iUndo.Cluster = AsGroup.Cluster;
                    iUndo.Value = Brain.Current[AsGroup.Cluster.Meaning];
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                    if (value != null)
                    {
                        AsGroup.Cluster.Meaning = value.ID;
                    }
                    else
                    {
                        AsGroup.Cluster.Meaning = Neuron.EmptyId;
                    }

                    OnPropertyChanged("GroupMode");
                }
            }
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
        public bool HasChildren
        {
            get
            {
                var iCluster = Value as NeuronCluster;
                return iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Asset;
            }
        }

        #endregion

        /// <summary>
        ///     For the UI, so we can use a single template for all asset children
        ///     that binds to the 'title' property.
        /// </summary>
        public string Title
        {
            get
            {
                return NeuronInfo.DisplayTitle;
            }
        }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                if (Value != null)
                {
                    return BrainData.Current.NeuronInfo[Value];

                        // we return a temp and don't cache it internally cause there is no direct editing for this value, so any bindings wont loose ref to this object.
                }

                return null;
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item
        {
            get
            {
                return Value;
            }
        }

        #endregion

        /// <summary>Sets the value. Used by the monitor. Also makes certain that the child
        ///     assets are available when possible.</summary>
        /// <param name="value">The value.</param>
        internal void SetValue(Neuron value)
        {
            fValue = value;
            OnPropertyChanged("Value");
            var iRoot = Owner.Root;
            if (iRoot != null)
            {
                // if there is no more root, the item is being deleted, so don't try to update anything anymore.
                var iCluster = value as NeuronCluster;
                if (iCluster != null)
                {
                    if (iCluster.Meaning == (ulong)PredefinedNeurons.Asset)
                    {
                        if (iRoot.TreeColumn == Owner.Data.IndexOf(this) + 1)
                        {
                            // +1 cause root's list of columns also includes the attribute, we don't
                            if (Owner.IsExpanded)
                            {
                                // only load the sub items when truely expanded, otherwise simply let the Ui know that children are avaiable.
                                Owner.Assets = new AssetCollection(Owner, iCluster);
                            }
                            else
                            {
                                Owner.HasChildrenChanged();
                            }

                            Presentation = "asset";
                        }
                        else
                        {
                            Presentation = "normal";
                        }

                        AsGroup = null;
                        fGroupMode = null;
                        return;
                    }

                    if (iCluster.Meaning == (ulong)PredefinedNeurons.And
                        || iCluster.Meaning == (ulong)PredefinedNeurons.Or
                        || iCluster.Meaning == (ulong)PredefinedNeurons.List
                        || iCluster.Meaning == (ulong)PredefinedNeurons.Argument)
                    {
                        if (iRoot.TreeColumn == Owner.Data.IndexOf(this) + 1)
                        {
                            // +1 cause root's list of columns also includes the attribute, we don't
                            Owner.Assets = null;
                        }

                        AsGroup = new AssetValueItemCollection(Owner, iCluster, this);
                        fGroupMode = Brain.Current[iCluster.Meaning];

                            // don't set prop cause this generates undo data, but we need value for init.
                        Presentation = "group";
                        return;
                    }
                }

                if (iRoot.TreeColumn == Owner.Data.IndexOf(this) + 1)
                {
                    // +1 cause root's list of columns also includes the attribute, we don't
                    Owner.Assets = null;
                }

                AsGroup = null;
                fGroupMode = null;
                Presentation = "normal";
            }
        }

        #region ctor

        /// <summary>The f as group.</summary>
        private AssetValueItemCollection fAsGroup;

        /// <summary>The f group mode.</summary>
        private Neuron fGroupMode;

        /// <summary>The f presentation.</summary>
        private string fPresentation;

        /// <summary>The f value.</summary>
        private Neuron fValue;

        #endregion
    }
}