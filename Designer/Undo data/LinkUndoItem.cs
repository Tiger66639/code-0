// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   An undo data item that is used to handle creation and destruction of
//   links (changes are handled differently).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An undo data item that is used to handle creation and destruction of
    ///     links (changes are handled differently).
    /// </summary>
    public class LinkUndoItem : BrainUndoItem
    {
        #region fields

        /// <summary>The f item.</summary>
        private Link fItem;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="LinkUndoItem"/> class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="action">The action.</param>
        public LinkUndoItem(Link item, BrainAction action)
        {
            Item = item;
            Action = action;
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets or sets the actual link.
        /// </summary>
        /// <remarks>
        ///     When the link is created, we can store a reference to the exact link.
        ///     When the link was destroyed, this property has become invalid.
        /// </remarks>
        /// <value>
        ///     The item.
        /// </value>
        public Link Item
        {
            get
            {
                return fItem;
            }

            set
            {
                fItem = value;
                if (value != null)
                {
                    // we always store the internals of the link in case it was a delete, in that case, the link object looses it's info.
                    From = value.From;
                    To = value.To;
                    Meaning = value.Meaning;
                    Info = value.Info.ConvertTo<Neuron>();
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the <see cref="From" /> part of the link
        /// </summary>
        /// <value>
        ///     From.
        /// </value>
        private Neuron From { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="To" /> part of the link.
        /// </summary>
        /// <value>
        ///     To.
        /// </value>
        private Neuron To { get; set; }

        /// <summary>
        ///     Gets or sets the meaning part of the link.
        /// </summary>
        /// <value>
        ///     The meaning.
        /// </value>
        private Neuron Meaning { get; set; }

        /// <summary>
        ///     Gets or sets the info part of the link.
        /// </summary>
        /// <value>
        ///     The info.
        /// </value>
        private System.Collections.Generic.List<Neuron> Info { get; set; }

        #region overrides

        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            switch (Action)
            {
                case BrainAction.Created:
                    var iUndoData = new LinkUndoItem(Item, BrainAction.Removed);
                    
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    Item.Destroy();
                    break;
                case BrainAction.Removed:
                    if (To.ID != Neuron.EmptyId && From.ID != Neuron.EmptyId && Meaning.ID != Neuron.EmptyId)
                    {
                        Item.Recreate(From, To, Meaning);

                            // we re-use the same link object, this will allow the UI object to remain the same (works better with the undo system). 
                        if (Info != null)
                        {
                            var iList = Item.InfoW;
                            iList.AddRange(Info);
                        }

                        iUndoData = new LinkUndoItem(Item, BrainAction.Created);
                        WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    }

                    break;
                case BrainAction.Changed:
                    iUndoData = new LinkUndoItem(Item, BrainAction.Changed);
                    Item.From = From;
                    Item.To = To;
                    Item.Meaning = Meaning;
                    WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                    break;
                default:
                    throw new System.InvalidOperationException(
                        string.Format("Unsuported BrainAction type: {0}.", Action));
            }
        }

        #endregion
    }
}