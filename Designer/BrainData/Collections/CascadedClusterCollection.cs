// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CascadedClusterCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A cluster collection that provides support for the cascadedCollectionChanged events, which are  used by the<see cref="TreeViewPanel" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>A cluster collection that provides support for the cascadedCollectionChanged events, which are  used by the<see cref="TreeViewPanel"/></summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CascadedClusterCollection<T> : ClusterCollection<T>, 
                                                         Data.IOnCascadedChanged, 
                                                         Data.INotifyCascadedPropertyChanged, 
                                                         Data.ICascadedNotifyCollectionChanged
        where T : INeuronWrapper
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CascadedClusterCollection{T}"/> class. Default constructor to use with a <see cref="NeuronCluster"/> that is already registered with the owner,
        ///     and therefor possibly already has code.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list. </param>
        /// <param name="childList">The NeuronCluster that contains all the code items.</param>
        public CascadedClusterCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CascadedClusterCollection{T}"/> class. Default constructor to use with a <see cref="NeuronCluster"/> that is already registered with the owner,
        ///     and therefor possibly already has code.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The NeuronCluster that contains all the code items.</param>
        /// <param name="isActive">if set to <c>true</c> the collection should be active, so it should monitor any changes.
        ///     Otherwise, the list wont monitor any changes.</param>
        public CascadedClusterCollection(INeuronWrapper owner, NeuronCluster childList, bool isActive)
            : base(owner, childList, isActive)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CascadedClusterCollection{T}"/> class. Default constructor to use for a <see cref="NeuronCluster"/> that is not yet declared (empty,
        ///     and therefor still needs to be created, specifically for code or argument lists (will automatically
        ///     generate a cluster meaning depending on the linkmeaning).</summary>
        /// <remarks>This cluster will only be registered if data is added.  This prevents us from creating clusters that only
        ///     to view the code.</remarks>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list. </param>
        /// <param name="linkMeaning">The id that should be used as meaning if data is added to the list.</param>
        public CascadedClusterCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CascadedClusterCollection{T}"/> class. Default constructor to use for a <see cref="NeuronCluster"/> that is not yet declared (empty,
        ///     and therefor still needs to be created.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="linkMeaning">The id that should be used as meaning if data is added to the list.</param>
        /// <param name="clusterMeaning">The id that should be used as the meaning for the cluster.</param>
        /// <remarks>This cluster will only be registered if data is added.  This prevents us from creating clusters that only
        ///     to view the code.</remarks>
        public CascadedClusterCollection(INeuronWrapper owner, ulong linkMeaning, ulong clusterMeaning)
            : base(owner, linkMeaning, clusterMeaning)
        {
        }

        #endregion

        #region events

        /// <summary>
        ///     Occurs when [cascaded collection changed].
        /// </summary>
        public event Data.NotifyCascadedCollectionChangedEventHandler CascadedCollectionChanged;

        /// <summary>
        ///     Occurs when [cascaded property changed].
        /// </summary>
        public event Data.CascadedPropertyChangedEventHandler CascadedPropertyChanged;

        #endregion

        #region Overrides

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            ThesaurusItemCollectionEventMonitor.Default.Clear();
            base.ClearItems();

            // event raise needs to be done before the clear so that we can reach the list of items. 
            var iArgs =
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            var iCArgs = new Data.CascadedCollectionChangedEventArgs(this, iArgs);
            Data.EventEngine.OnCollectionChanged(this, iCArgs);
        }

        /// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the
        ///     provided arguments.</summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Data.EventEngine.OnCollectionChanged(this, new Data.CascadedCollectionChangedEventArgs(this, e));
            }
        }

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