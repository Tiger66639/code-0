// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetItemBase.cs" company="">
//   
// </copyright>
// <summary>
//   base class for wordnetitems: goups and normal items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for wordnetitems: goups and normal items.
    /// </summary>
    public class WordNetItemBase : Data.OwnedObject, IWordNetItemOwner, INeuronWrapper
    {
        /// <summary>The f children.</summary>
        private readonly Data.ObservedCollection<WordNetItem> fChildren;

        /// <summary>Initializes a new instance of the <see cref="WordNetItemBase"/> class. 
        ///     Initializes a new instance of the <see cref="WordNetItemBase"/>
        ///     class.</summary>
        public WordNetItemBase()
        {
            fChildren = new Data.ObservedCollection<WordNetItem>(this);
        }

        #region Root

        /// <summary>
        ///     Gets the root of the tree.
        /// </summary>
        /// <value>
        ///     The root.
        /// </value>
        public WordNetChannel Root
        {
            get
            {
                var iGroup = Group;
                if (iGroup != null)
                {
                    return iGroup.Owner as WordNetChannel;
                }

                return null;
            }
        }

        #endregion

        #region Group

        /// <summary>
        ///     Gets the group that this .
        /// </summary>
        /// <value>
        ///     The group.
        /// </value>
        public WordNetItemGroup Group
        {
            get
            {
                IWordNetItemOwner iCur = this;
                while (iCur != null && !(iCur is WordNetItemGroup))
                {
                    iCur = ((WordNetItemBase)iCur).Owner as IWordNetItemOwner;
                }

                return iCur as WordNetItemGroup;
            }
        }

        #endregion

        #region ID

        /// <summary>
        ///     Gets the <see cref="ID" /> of the item being wrapped.
        /// </summary>
        /// <value>
        ///     The ID.
        /// </value>
        public ulong ID { get; internal set; }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item
        {
            get
            {
                if (ID != Neuron.EmptyId)
                {
                    return Brain.Current[ID];
                }

                return null;
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children for this item.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<WordNetItem> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion
    }
}