// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesChildItemsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection of thesaurus items that are children of another thesaurus item. These are loaded by wrapping the
//   cluster
//   that contains all the related items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection of thesaurus items that are children of another thesaurus item. These are loaded by wrapping the
    ///     cluster
    ///     that contains all the related items.
    /// </summary>
    public class ThesChildItemsCollection : ClusterCollection<ThesaurusItem>, 
                                            Data.IOnCascadedChanged, 
                                            Data.INotifyCascadedPropertyChanged, 
                                            Data.ICascadedNotifyCollectionChanged
    {
        /// <summary>Checks if there are any thesaurus items in the current list for the specified id and if so,
        ///     asks it to update wether it has any children or not.</summary>
        /// <param name="id">The id.</param>
        internal void CheckHasItems(ulong id)
        {
            var iRel = BrainData.Current.Thesaurus.SelectedRelationship;
            foreach (var i in this)
            {
                if (i.ID == id)
                {
                    i.CheckHasItems(iRel);
                }
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ThesChildItemsCollection"/> class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The NeuronCluster that contains all the code items.</param>
        public ThesChildItemsCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThesChildItemsCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public ThesChildItemsCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning)
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

        #region overrides

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ThesaurusItem"/>.</returns>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        public override ThesaurusItem GetWrapperFor(Neuron toWrap)
        {
            return new ThesaurusItem(toWrap, Brain.Current[MeaningID]);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return linkMeaning;

                // the meanin of the link is also the meaning of hte cluster, this way we know the type of relationship a tthe cluster level.
        }

        /// <summary>
        ///     We don't call base, this is done by the event handler that monitors changes to the list.
        ///     If we don't use this technique, actions performed by the user will be done 2 times: once
        ///     normally, once in response to the list change in the neuron.
        /// </summary>
        protected override void ClearItems()
        {
            var iCount = Count;

            // event raise needs to be done before the clear so that we can reach the list of items. 
            var iArgs =
                new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            var iCArgs = new Data.CascadedCollectionChangedEventArgs(this, iArgs);
            Data.EventEngine.OnCollectionChanged(this, iCArgs);

            base.ClearItems();
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

            var iOwner = Owner as ThesaurusItem;
            iOwner.HasItems = Count > 0;
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