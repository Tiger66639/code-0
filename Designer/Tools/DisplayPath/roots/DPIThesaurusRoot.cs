// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPIThesaurusRoot.cs" company="">
//   
// </copyright>
// <summary>
//   a display path root item for thesaurus search results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    using System.Linq;

    /// <summary>
    ///     a display path root item for thesaurus search results.
    /// </summary>
    public class DPIThesaurusRoot : DPIRoot
    {
        /// <summary>The f data source.</summary>
        private Thesaurus fDataSource;

        #region Relationship

        /// <summary>
        ///     Gets/sets the relationship which was selected.
        /// </summary>
        public ulong Relationship { get; set; }

        #endregion

        #region POS

        /// <summary>
        ///     Gets/sets the pos that was selected.
        /// </summary>
        public int POS { get; set; }

        #endregion

        #region DataSource

        /// <summary>
        ///     Gets/sets the thesaurus to make the selection in.
        /// </summary>
        public Thesaurus DataSource
        {
            get
            {
                return fDataSource;
            }

            set
            {
                if (value != fDataSource)
                {
                    fDataSource = value;
                    if (fDataSource != null)
                    {
                        POS = fDataSource.SelectedPosFilterIndex;
                    }
                }
            }
        }

        #endregion

        /// <summary>Duplicates this instance.</summary>
        /// <returns>The <see cref="DPIRoot"/>.</returns>
        internal override DPIRoot Duplicate()
        {
            var iRes = new DPIThesaurusRoot();
            iRes.Item = Item;
            iRes.Items.AddRange(Items);
            iRes.DataSource = DataSource;
            iRes.POS = POS;
            iRes.Relationship = Relationship;
            return iRes;
        }

        /// <summary>
        ///     Selects the path result.
        /// </summary>
        internal override void SelectPathResult()
        {
            System.Diagnostics.Debug.Assert(DataSource != null);
            var iRel = (from i in DataSource.Relationships where i.Item.ID == Relationship select i).FirstOrDefault();
            if (iRel != null)
            {
                DataSource.SelectedRelationshipIndex = DataSource.Relationships.IndexOf(iRel);
                DataSource.SelectedPosFilterIndex = POS;

                if (DataSource.Items != null)
                {
                    SelectPath();
                }
                else
                {
                    DataSource.ItemsChanged += DataSource_ItemsChanged;
                }
            }
        }

        /// <summary>The select path.</summary>
        private void SelectPath()
        {
            object iCurrent = (from i in DataSource.Items where i.Item == Item select i).FirstOrDefault();
            if (iCurrent != null)
            {
                foreach (ISelectDisplayPathForThes i in (from i in Items select i).Reverse())
                {
                    // we walk through the list in reverse cause the items are added in reverse.
                    if (iCurrent is ThesaurusItem)
                    {
                        ((ThesaurusItem)iCurrent).IsExpanded = true;
                    }

                    iCurrent = i.SelectFrom(DataSource, iCurrent);
                    if (iCurrent == null)
                    {
                        break;
                    }
                }

                if (iCurrent is ThesaurusItem)
                {
                    ((ThesaurusItem)iCurrent).NeedsBringIntoView = true;
                    ((ThesaurusItem)iCurrent).IsSelected = true;
                }
                else if (iCurrent is ThesaurusSubItem)
                {
                    ((ThesaurusSubItem)iCurrent).IsSelected = true;
                }
            }
        }

        /// <summary>Handles the ItemsChanged event of the <see cref="DataSource"/>
        ///     control. called when the data is loaded and we are ready to select the
        ///     path.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DataSource_ItemsChanged(object sender, System.EventArgs e)
        {
            DataSource.ItemsChanged -= DataSource_ItemsChanged;

            // async call, to make certain that the ui is loaded properly.
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new System.Action(SelectPathResult), 
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                // we call this again to make certain that this time, the correct relationship and pos have been loaded. if this is notthe case, a new query will be launched.
        }

        #region unhanled functions

        /// <summary>Returns a PatterEditorItem, basedon the path selection method ofthis
        ///     item, applied to the owning pattern Editor item. We are root, so can't
        ///     get anything anymore.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns an object from the specified list. This is usually only
        ///     implemented by indexed accessors. We are root, so can't get anything
        ///     anymore.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>returns a code item, based on the path selection method of this item,
        ///     applied to an ICodeItemsOwner. Never called, we are a root, for
        ///     textpatterns, not code items.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}