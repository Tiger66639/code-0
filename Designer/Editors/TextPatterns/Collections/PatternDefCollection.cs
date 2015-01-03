// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternDefCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for patterngroups.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper for patterngroups.
    /// </summary>
    public class PatternDefCollection : PatternEditorCollection<PatternRule>
    {
        /// <summary>
        ///     determins the max nr of items in the list before the UI needs to switch to virtualizing mode for displaying.
        /// </summary>
        public const int MAXBEFOREVIRTUALIZATION = 80;

        /// <summary>Gets the wrapper for.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="PatternRule"/>.</returns>
        public override PatternRule GetWrapperFor(Neuron toWrap)
        {
            var iNew = new PatternRule((NeuronCluster)toWrap);
            return iNew;
        }

        /// <summary>Gets the list meaning.</summary>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.TextPatternTopic;
        }

        /// <summary>Removes the item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                iItem.IsSelected = false;

                    // when we get removed from the list, make certain it is no longer in the selection list.
            }

            base.RemoveItem(index);
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // in master-detail view, never virtualisation.
                iOwner.ListRequiresVertualization = Count > MAXBEFOREVIRTUALIZATION;
            }
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            var iItem = this[index];
            if (iItem != null)
            {
                iItem.IsSelected = false;

                    // when we get removed from the list, make certain it is no longer in the selection list.
            }

            base.RemoveItemDirect(index);
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // in master-detail view, never virtualisation.
                iOwner.ListRequiresVertualization = Count > MAXBEFOREVIRTUALIZATION;
            }
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                i.IsSelected = false;
            }

            base.ClearItems();
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null)
            {
                iOwner.ListRequiresVertualization = false;
            }
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            foreach (var i in this)
            {
                i.IsSelected = false;
            }

            base.ClearItemsDirect();
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null)
            {
                iOwner.ListRequiresVertualization = false;
            }
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, PatternRule item)
        {
            base.InsertItem(index, item);
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // in master-detail view, never virtualisation.
                iOwner.ListRequiresVertualization = Count > MAXBEFOREVIRTUALIZATION;
            }
        }

        /// <summary>Inserts the item direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItemDirect(int index, PatternRule item)
        {
            base.InsertItemDirect(index, item);
            var iOwner = Owner as TextPatternEditor;
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // in master-detail view, never virtualisation.
                iOwner.ListRequiresVertualization = Count > MAXBEFOREVIRTUALIZATION;
            }
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="PatternDefCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childList">The child list.</param>
        public PatternDefCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
            InitVirtualization();
        }

        /// <summary>The init virtualization.</summary>
        private void InitVirtualization()
        {
            var iOwner = Owner as TextPatternEditor;

                // when first loaded, make certain that we adjust the virtualization requiremenents as needed.
            if (iOwner != null && iOwner.IsMasterDetailView == false)
            {
                // in master-detail view, never virtualisation.
                iOwner.ListRequiresVertualization = Count > MAXBEFOREVIRTUALIZATION;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="PatternDefCollection"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public PatternDefCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
            InitVirtualization();
        }

        #endregion
    }
}