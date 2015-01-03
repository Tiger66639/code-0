// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesaurusSubItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for neurons that are related to another neuron through
//   the thesaurus, but not by a recursive relationship, but a non recursive
//   one (so, they are displayed at the bottom of the thesaurus, only for the
//   selected item).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for neurons that are related to another neuron through
    ///     the thesaurus, but not by a recursive relationship, but a non recursive
    ///     one (so, they are displayed at the bottom of the thesaurus, only for the
    ///     selected item).
    /// </summary>
    /// <remarks>
    ///     We need to make the the Owner the SubRelationshipsCollection, can't make
    ///     it the <see cref="ThesaurusSubItemCollection" /> cause than that would
    ///     need to have it's self as parent, which doesn't work. We should have used
    ///     a sub object that contains the ThesaurusSubitemCollection.
    /// </remarks>
    public class ThesaurusSubItem : Data.OwnedObject<SubRelationshipsCollection>, INeuronWrapper, INeuronInfo
    {
        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>Initializes a new instance of the <see cref="ThesaurusSubItem"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ThesaurusSubItem(Neuron toWrap)
        {
            Item = toWrap;
        }

        /// <summary>
        ///     Gets the parent list.
        /// </summary>
        /// <value>
        ///     The parent list.
        /// </value>
        public ThesaurusSubItemCollection ParentList
        {
            get
            {
                foreach (var iCol in Owner)
                {
                    if (iCol.Contains(this))
                    {
                        return iCol;
                    }
                }

                return null;
            }
        }

        #region DebugItem

        /// <summary>
        ///     Gets the item as a debugneuron, which is better to browse the item.
        /// </summary>
        public DebugNeuron DebugItem
        {
            get
            {
                return new DebugNeuron(Item);
            }
        }

        #endregion

        #region Value

        /// <summary>
        ///     Gets/sets the value neuron to wrap
        /// </summary>
        public Neuron Value
        {
            get
            {
                return Item;
            }

            set
            {
                if (value != Item)
                {
                    WindowMain.UndoStore.BeginUndoGroup();
                    try
                    {
                        // we create a new object and replace it in  the collection cause this prop represents the item directly in the colleciton.
                        var iIndex = ParentList.IndexOf(this); // asGroup must exist cause this item exists.
                        if (value != null)
                        {
                            ParentList[iIndex] = new ThesaurusSubItem(value);
                        }
                        else
                        {
                            ParentList.RemoveAt(iIndex);
                        }
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether this item is currently selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                    var iParent = ParentList;
                    if (iParent != null)
                    {
                        if (value)
                        {
                            iParent.IsSelected = true;

                                // need to make certain that the group is also selected, otherwise the remove command doesn't work.
                            iParent.SelectedItem = this;
                        }
                        else
                        {
                            iParent.SelectedItem = null;
                        }
                    }
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                return BrainData.Current.NeuronInfo[Item.ID];
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #endregion
    }
}