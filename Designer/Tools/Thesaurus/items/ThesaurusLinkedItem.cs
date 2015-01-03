// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusLinkedItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for a relationship between a thesaurus item and a related
//   part of speech item or conjugated item (any item that is linked).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for a relationship between a thesaurus item and a related
    ///     part of speech item or conjugated item (any item that is linked).
    /// </summary>
    public class ThesaurusLinkedItem : Data.ObservableObject
    {
        /// <summary>The f related.</summary>
        private Neuron fRelated;

        /// <summary>The f relationship.</summary>
        private Neuron fRelationship;

        /// <summary>Initializes a new instance of the <see cref="ThesaurusLinkedItem"/> class. Initializes a new instance of the <see cref="ThesaurusLinkedItem"/>
        ///     class, with everything initialized.</summary>
        /// <param name="item">The item.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="related">The related.</param>
        public ThesaurusLinkedItem(Neuron item, Neuron relationship, Neuron related)
        {
            Item = item;
            fRelated = related;
            fRelationship = relationship;
        }

        /// <summary>Initializes a new instance of the <see cref="ThesaurusLinkedItem"/> class. Initializes a new instance of the <see cref="ThesaurusLinkedItem"/>
        ///     class, without an initial relationship.</summary>
        /// <param name="item">The item.</param>
        public ThesaurusLinkedItem(Neuron item)
        {
            Item = item;
        }

        #region Relationship

        /// <summary>
        ///     Gets/sets the meaning part of the link
        /// </summary>
        public Neuron Relationship
        {
            get
            {
                return fRelationship;
            }

            set
            {
                if (fRelationship != value)
                {
                    var iLink = GetLink();
                    OnPropertyChanging("Relationship", fRelationship, value);
                    fRelationship = value;
                    if (iLink != null)
                    {
                        UpdateLink(iLink);
                    }
                    else
                    {
                        TryCreateLink();
                    }

                    OnPropertyChanged("Relationship");
                }
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the item that is wrapped.
        /// </summary>
        public Neuron Item { get; internal set; }

        #endregion

        #region Related

        /// <summary>
        ///     Gets/sets the to part of the link.
        /// </summary>
        public Neuron Related
        {
            get
            {
                return fRelated;
            }

            set
            {
                if (fRelated != value)
                {
                    var iLink = GetLink();
                    OnPropertyChanging("Related", fRelated, value);
                    fRelated = value;
                    if (iLink != null)
                    {
                        UpdateLink(iLink);
                    }
                    else
                    {
                        TryCreateLink();
                    }

                    OnPropertyChanged("Related");
                    OnPropertyChanged("RelatedObjectWord");
                }
            }
        }

        #endregion

        #region RelatedObjectWord

        /// <summary>
        ///     Gets or sets the value as a string for the related neuron. It does
        ///     this by presuming that the 'related' neuron is an object. It modifies
        ///     the DisplayTitle of the object and upates the unerlying textNeuron.
        ///     This textneuron is recreated each time with a change.
        /// </summary>
        /// <value>
        ///     The related object word.
        /// </value>
        public string RelatedObjectWord
        {
            get
            {
                if (fRelated != null)
                {
                    return BrainData.Current.NeuronInfo[fRelated].DisplayTitle;
                }

                return null;
            }

            set
            {
                if (value != RelatedObjectWord)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        if (fRelated == null)
                        {
                            Related = EditorsHelper.CreateObject(value);
                        }
                        else
                        {
                            EditorsHelper.ChangeTextForObject((NeuronCluster)fRelated, value);
                        }

                        OnPropertyChanged("RelatedObjectWord");
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Deletes this instance by removing the link from the item to the
        ///     related item (with relationship as meaning).
        /// </summary>
        /// <returns>
        ///     True if we succeeded in deleting the link. No if there was no link
        ///     found (item was not yet completed?)
        /// </returns>
        internal bool Delete()
        {
            var iLink = GetLink();
            if (iLink != null)
            {
                var iUndo = new LinkUndoItem(iLink, BrainAction.Removed);
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                iLink.Destroy();
                return true;
            }

            return false;
        }

        /// <summary>The update link.</summary>
        /// <param name="link">The link.</param>
        private void UpdateLink(Link link)
        {
            if (Relationship != null && Related != null)
            {
                link.Meaning = Relationship;
                link.To = Related;
            }
            else
            {
                link.Destroy();
            }
        }

        /// <summary>The get link.</summary>
        /// <returns>The <see cref="Link"/>.</returns>
        private Link GetLink()
        {
            if (Relationship != null && Related != null)
            {
                // only try to delete the link if there is one.
                var iLink = Link.Find(Item, Related, Relationship);
                System.Diagnostics.Debug.Assert(iLink != null);
                return iLink;
            }

            return null;
        }

        /// <summary>The try create link.</summary>
        private void TryCreateLink()
        {
            if (Relationship != null && Related != null)
            {
                Link.Create(Item, Related, Relationship);
            }
        }
    }
}