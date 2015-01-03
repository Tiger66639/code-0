// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestrictionsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   A collection containing frame element restrictions. Note that this class
//   doesn't provide a property for the logical <see langword="operator" />
//   that is assigned to the wrapped cluster. This is because his class
//   doesn't have PropertyChanged/-ing support. This should be done by other
//   wrappers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A collection containing frame element restrictions. Note that this class
    ///     doesn't provide a property for the logical <see langword="operator" />
    ///     that is assigned to the wrapped cluster. This is because his class
    ///     doesn't have PropertyChanged/-ing support. This should be done by other
    ///     wrappers.
    /// </summary>
    public class RestrictionsCollection : CascadedClusterCollection<FERestrictionBase>
    {
        #region inner types

        /// <summary>
        ///     when this collection wraps a temp cluster, we need some extra data to
        ///     create the link when initially populated.
        /// </summary>
        private class RestrictionsMeaningInfo : MeaningInfo
        {
            /// <summary>
            ///     Gets or sets the owner that should be used to attach the cluster
            ///     to..
            /// </summary>
            /// <value>
            ///     The owner.
            /// </value>
            public Neuron Owner { get; set; }
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="RestrictionsCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The <see cref="CodeEditor"/> that contains this code list.</param>
        /// <param name="childList">The <see cref="NeuronCluster"/> that contains all the code items.</param>
        public RestrictionsCollection(INeuronWrapper owner, NeuronCluster childList)
            : base(owner, childList)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RestrictionsCollection"/> class. Initializes a new instance of the <see cref="CodeItemCollection"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        public RestrictionsCollection(INeuronWrapper owner, ulong linkMeaning)
            : base(owner, linkMeaning, linkMeaning)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RestrictionsCollection"/> class. Initializes a new instance of the<see cref="RestrictionsCollection"/> class. A constructor for temp
        ///     clusters. We need this cause the cluster needs to be accessed before
        ///     the collection can be made.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="childlist">The childlist.</param>
        /// <param name="linkMeaning">The link meaning.</param>
        /// <param name="listOwner">The list Owner.</param>
        public RestrictionsCollection(
            INeuronWrapper owner, 
            NeuronCluster childlist, 
            ulong linkMeaning, 
            Neuron listOwner)
            : base(owner, childlist)
        {
            fLinkMeaning = new RestrictionsMeaningInfo { LinkMeaning = linkMeaning, Owner = listOwner };
            fLinkMeaning.ClusterMeaning = linkMeaning;
        }

        #endregion

        #region overides

        /// <summary>Called when a new wrapper object needs to be created for a neuron.</summary>
        /// <remarks>CodeEditors do: return EditorsHelper.CreateCodeItemFor(toWrap)</remarks>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FERestrictionBase"/>.</returns>
        public override FERestrictionBase GetWrapperFor(Neuron toWrap)
        {
            var iToWrap = toWrap as NeuronCluster;
            if (toWrap is NeuronCluster)
            {
                if (iToWrap.Meaning == (ulong)PredefinedNeurons.VerbNetRestrictions)
                {
                    return new FERestrictionGroup(toWrap);
                }

                if (iToWrap.Meaning == (ulong)PredefinedNeurons.Code)
                {
                    return new FECustomRestriction(iToWrap);
                }

                return new FERestriction(toWrap);
            }

            if (toWrap is BoolExpression)
            {
                return new FERestrictionBool(toWrap);
            }

            throw new System.InvalidOperationException();
        }

        /// <summary>Returns the meaning that should be assigned to the cluster when it is
        ///     newly created.</summary>
        /// <param name="linkMeaning">The meaning of the link between the wrapped cluster and the owner of
        ///     this collection.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        protected override ulong GetListMeaning(ulong linkMeaning)
        {
            return (ulong)PredefinedNeurons.VerbNetRestrictions;
        }

        /// <summary>
        ///     Creates the link between the owner and the cluster. This is a
        ///     <see langword="virtual" /> function so that descendents can change the
        ///     behaviour.
        /// </summary>
        protected override void CreateOwnerLink()
        {
            var iLink = new Link(Cluster, ((RestrictionsMeaningInfo)fLinkMeaning).Owner, fLinkMeaning.LinkMeaning);

                // simply create link, don't need to check if already created, cause this is caught by the event monitor.
            Cluster.Meaning = fLinkMeaning.ClusterMeaning;
            fLinkMeaning = null; // need to indicate that the action is completed.
        }

        /// <summary>
        ///     Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            var iOwner = Owner as FERestrictionBase;
            if (iOwner != null)
            {
                iOwner.CallPropertyChangedChanged("HasChildren");
            }
        }

        /// <summary>
        ///     Clears the items direct.
        /// </summary>
        protected override void ClearItemsDirect()
        {
            base.ClearItemsDirect();
            var iOwner = Owner as FERestrictionBase;
            if (iOwner != null)
            {
                iOwner.CallPropertyChangedChanged("HasChildren");
            }
        }

        /// <summary>Inserts the item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, FERestrictionBase item)
        {
            base.InsertItem(index, item);
            if (Count == 1)
            {
                var iOwner = Owner as FERestrictionBase;
                if (iOwner != null)
                {
                    iOwner.CallPropertyChangedChanged("HasChildren");
                }
            }
        }

        /// <summary>Inserts the <paramref name="item"/> direct.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItemDirect(int index, FERestrictionBase item)
        {
            base.InsertItemDirect(index, item);
            if (Count == 1)
            {
                var iOwner = Owner as FERestrictionBase;
                if (iOwner != null)
                {
                    iOwner.CallPropertyChangedChanged("HasChildren");
                }
            }
        }

        /// <summary>Removes the item.</summary>
        /// <remarks>Always raises the event, so that undo works correctly (through the
        ///     CodeItemDropAdvisor).</remarks>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (Count == 0)
            {
                var iOwner = Owner as FERestrictionBase;
                if (iOwner != null)
                {
                    iOwner.CallPropertyChangedChanged("HasChildren");
                }
            }
        }

        /// <summary>Removes the item direct.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItemDirect(int index)
        {
            base.RemoveItemDirect(index);
            if (Count == 0)
            {
                var iOwner = Owner as FERestrictionBase;
                if (iOwner != null)
                {
                    iOwner.CallPropertyChangedChanged("HasChildren");
                }
            }
        }

        #endregion
    }
}