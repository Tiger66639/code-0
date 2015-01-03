// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusSubItemCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection that contains all the related objects for another object for
//   a single relationship.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection that contains all the related objects for another object for
    ///     a single relationship.
    /// </summary>
    public class ThesaurusSubItemCollection : ClusterCollection<ThesaurusSubItem>
    {
        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded = true;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f relationship.</summary>
        private Neuron fRelationship;

        /// <summary>The f selected item.</summary>
        private ThesaurusSubItem fSelectedItem;

        /// <summary>Initializes a new instance of the <see cref="ThesaurusSubItemCollection"/> class. Initializes a new instance of the<see cref="ThesaurusSubItemCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="relationship">The relationship.</param>
        public ThesaurusSubItemCollection(INeuronWrapper owner, NeuronCluster cluster, Neuron relationship)
            : base(owner, cluster)
        {
            Relationship = relationship;
        }

        #region Relationship

        /// <summary>
        ///     Gets the relationship that this collection contains related items for.
        /// </summary>
        public Neuron Relationship
        {
            get
            {
                return fRelationship;
            }

            internal set
            {
                fRelationship = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Relationship"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("RelationshipInfo"));

                    // also changes
            }
        }

        #endregion

        #region RelationshipInfo

        /// <summary>
        ///     Gets the extra info for the relationship neuron.
        /// </summary>
        public NeuronData RelationshipInfo
        {
            get
            {
                var iRel = Relationship;
                if (iRel != null)
                {
                    return BrainData.Current.NeuronInfo[iRel.ID];
                }

                return null;
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the content in the ui is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                fIsExpanded = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsExpanded"));
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the current sub relationship is selected or not.
        /// </summary>
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
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsSelected"));
                    var iOwner = Owner as SubRelationshipsCollection;
                    if (iOwner != null)
                    {
                        iOwner.SelectedItem = value ? this : null;
                    }
                }
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     Gets/sets the currently selected sub item of this list.
        /// </summary>
        public ThesaurusSubItem SelectedItem
        {
            get
            {
                return fSelectedItem;
            }

            set
            {
                if (value != fSelectedItem)
                {
                    fSelectedItem = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedItem"));
                }
            }
        }

        #endregion

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="ThesaurusSubItem"/>.</returns>
        public override ThesaurusSubItem GetWrapperFor(Neuron toWrap)
        {
            return new ThesaurusSubItem(toWrap);
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return Relationship.ID;
        }
    }
}