// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemToLockList.cs" company="">
//   
// </copyright>
// <summary>
//   a wrapper for a cluster used by the LockExpression. We use this seperate
//   class for the neuronsToLock and LinksToLock lists, so that the
//   <see cref="LockExpression" /> only is the owner of 1 list. This is
//   important for the <see cref="ICodeItemsOwner" /> interface. We imple
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a wrapper for a cluster used by the LockExpression. We use this seperate
    ///     class for the neuronsToLock and LinksToLock lists, so that the
    ///     <see cref="LockExpression" /> only is the owner of 1 list. This is
    ///     important for the <see cref="ICodeItemsOwner" /> interface. We imple
    /// </summary>
    public class CodeItemToLockList : CodeItem, ICodeItemsOwner
    {
        /// <summary>The f lock neurons.</summary>
        private readonly bool fLockNeurons;

        /// <summary>Initializes a new instance of the <see cref="CodeItemToLockList"/> class. Initializes a new instance of the <see cref="CodeItemNeuronsToLock"/>
        ///     class.</summary>
        /// <param name="source">The source.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <param name="lockNeurons">if set to <c>true</c> [lock neurons].</param>
        public CodeItemToLockList(LockExpression source, bool isActive, bool lockNeurons)
            : base(isActive)
        {
            fLockNeurons = lockNeurons;
            Item = source;

                // w can't pass this in the constructor, need to set manually, cause fLockNeurons must be set before this is called.
        }

        /// <summary>
        ///     Gets/sets wether this object monitors changes in the database or not.
        ///     When this is false, it uses less resources.
        /// </summary>
        /// <remarks>
        ///     This is <see langword="virtual" /> so that any changes to this property
        ///     can also be propegated to other objects.
        /// </remarks>
        /// <value>
        /// </value>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }

            set
            {
                var iPrev = IsActive;
                base.IsActive = value;
                if (iPrev != IsActive)
                {
                    Items.IsActive = value;
                }
            }
        }

        #region ICodeItemsOwner Members

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public CodeItemCollection Items { get; private set; }

        #endregion

        #region Functions

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            if (value != null)
            {
                NeuronCluster iToLock;
                if (fLockNeurons)
                {
                    iToLock = ((LockExpression)Item).NeuronsToLockCluster;
                    if (iToLock != null)
                    {
                        Items = new CodeItemCollection(this, iToLock);
                    }
                    else
                    {
                        Items = new CodeItemCollection(this, (ulong)PredefinedNeurons.NeuronsToLock);
                    }
                }
                else
                {
                    iToLock = ((LockExpression)Item).LinksToLockCluster;
                    if (iToLock != null)
                    {
                        Items = new CodeItemCollection(this, iToLock);
                    }
                    else
                    {
                        Items = new CodeItemCollection(this, (ulong)PredefinedNeurons.LinksToLock);
                    }
                }
            }
            else
            {
                Items = null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (InternalChange == false)
            {
                if (fLockNeurons == false && link.MeaningID == (ulong)PredefinedNeurons.LinksToLock)
                {
                    Items = new CodeItemCollection(this, (ulong)PredefinedNeurons.LinksToLock);
                }
                else if (fLockNeurons && link.MeaningID == (ulong)PredefinedNeurons.NeuronsToLock)
                {
                    Items = new CodeItemCollection(this, (ulong)PredefinedNeurons.NeuronsToLock);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (InternalChange == false)
            {
                NeuronCluster iToLock;
                if (fLockNeurons && link.MeaningID == (ulong)PredefinedNeurons.NeuronsToLock)
                {
                    iToLock = ((LockExpression)Item).NeuronsToLockCluster;
                    Items = new CodeItemCollection(this, iToLock);
                }
                else if (fLockNeurons == false && link.MeaningID == (ulong)PredefinedNeurons.LinksToLock)
                {
                    iToLock = ((LockExpression)Item).LinksToLockCluster;
                    Items = new CodeItemCollection(this, iToLock);
                }
            }
        }

        /// <summary>Check if this item or any of it's children wraps the specified neuron,
        ///     if so, the item is made selected.</summary>
        /// <param name="neuron">The neuron.</param>
        public override void Select(Neuron neuron)
        {
            base.Select(neuron);
            foreach (var i in Items)
            {
                i.Select(neuron);
            }
        }

        /// <summary>Returns the object that is currently used to depict the value for the
        ///     other side of the link with the specified meaning.</summary>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetCodeItemFor(ulong meaning)
        {
            return null;
        }

        #endregion
    }
}