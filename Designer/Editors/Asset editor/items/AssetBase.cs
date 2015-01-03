// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetBase.cs" company="">
//   
// </copyright>
// <summary>
//   base class for all asset objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for all asset objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AssetBase : Data.OwnedObject<IAssetOwner>, 
                             Data.IOnCascadedChanged, 
                             Data.INotifyCascadedPropertyChanged, 
                             Data.ICascadedNotifyCollectionChanged, 
                             INeuronWrapper, 
                             INeuronInfo, 
                             WPF.Controls.ITreeViewPanelItem
    {
        /// <summary>Initializes a new instance of the <see cref="AssetBase"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public AssetBase(Neuron toWrap)
        {
            Item = toWrap;
        }

        #region fields

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f needs bring into view.</summary>
        private bool fNeedsBringIntoView;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        #endregion

        #region events

        /// <summary>
        ///     Occurs when [cascaded property changed].
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        /// <summary>
        ///     Occurs when [cascaded collection changed].
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        #endregion

        #region prop

        #region Root

        /// <summary>
        ///     Gets the root object.
        /// </summary>
        /// <value>The root.</value>
        public ObjectEditor Root
        {
            get
            {
                var iOwner = Owner;
                while (iOwner != null)
                {
                    var iRes = iOwner as ObjectEditor;
                    if (iRes != null)
                    {
                        return iRes;
                    }

                    iOwner = ((Data.OwnedObject<IAssetOwner>)iOwner).Owner;
                }

                return null;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets or sets a value indicating whether this tree item is selected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (fIsSelected != value)
                {
                    var iRoot = Root;
                    if (iRoot != null)
                    {
                        iRoot.SetSelected(this, value);
                    }

                    SetSelected(value);
                }
            }
        }

        /// <summary>Sets the IsSlected value without updating the root.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected internal void SetSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether this item is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                if (fIsExpanded != value)
                {
                    SetExpanded(value);
                }
            }
        }

        /// <summary>Sets the expanded value. Allows descendents to do something extra while expanding/collapsing.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected virtual void SetExpanded(bool value)
        {
            fIsExpanded = value;
            OnPropertyChanged("IsExpanded");
            var iArgs = new Data.CascadedPropertyChangedEventArgs(
                this, 
                new System.ComponentModel.PropertyChangedEventArgs("IsExpanded"));
            Data.EventEngine.OnPropertyChanged(this, iArgs);
        }

        #endregion

        #region ParentTreeItem

        /// <summary>
        ///     Gets the parent tree item.
        /// </summary>
        /// <value>The parent tree item.</value>
        public WPF.Controls.ITreeViewPanelItem ParentTreeItem
        {
            get
            {
                return Owner as WPF.Controls.ITreeViewPanelItem;
            }
        }

        #endregion

        #region NeedsBringIntoView

        /// <summary>
        ///     Gets or sets a value indicating whether this object needs to be brought into view.
        /// </summary>
        /// <value><c>true</c> if [needs bring into view]; otherwise, <c>false</c>.</value>
        public bool NeedsBringIntoView
        {
            get
            {
                return fNeedsBringIntoView;
            }

            set
            {
                fNeedsBringIntoView = value;
                OnPropertyChanged("NeedsBringIntoView");
                var iArgs = new Data.CascadedPropertyChangedEventArgs(
                    this, 
                    new System.ComponentModel.PropertyChangedEventArgs("NeedsBringIntoView"));
                Data.EventEngine.OnPropertyChanged(this, iArgs);
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public Neuron Item { get; private set; }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron.  Can be null.
        /// </summary>
        /// <value></value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item];
            }
        }

        #endregion

        #region TreeItems

        /// <summary>
        ///     Gets a list to all the children of this tree item.
        /// </summary>
        /// <value>The tree items.</value>
        public virtual System.Collections.IList TreeItems
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children or not. When the
        ///     list of children changes (becomes empty or gets the first item), this should
        ///     be raised when appropriate through a propertyChanged event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasChildren
        {
            get
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region IOnCascadedChanged Members

        /// <summary>Raises the <see cref="E:CascadedCollectionChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedCollectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public void OnCascadedCollectionChanged(Data.CascadedCollectionChangedEventArgs args)
        {
            if (CascadedCollectionChanged != null)
            {
                CascadedCollectionChanged(this, args);
            }

            var iSource = args.OriginalSource as System.Collections.IList;
            if (iSource != null)
            {
                if (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                        && iSource.Count == 1)
                    || (args.Args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                        && iSource.Count == 0))
                {
                    OnPropertyChanged("HasChildren");
                }
            }
            else
            {
                OnPropertyChanged("HasChildren");

                    // if we can't check agains the nr of existing children, always let it update.
            }
        }

        /// <summary>Raises the <see cref="E:CascadedPropertyChanged"/> event.</summary>
        /// <param name="args">The <see cref="JaStDev.Data.CascadedPropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnCascadedPropertyChanged(Data.CascadedPropertyChangedEventArgs args)
        {
            if (CascadedPropertyChanged != null)
            {
                CascadedPropertyChanged(this, args);
            }
        }

        #endregion
    }
}