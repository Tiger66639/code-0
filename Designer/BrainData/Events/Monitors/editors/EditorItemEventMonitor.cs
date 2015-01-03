// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorItemEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The editor item event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The editor item event monitor.</summary>
    internal class EditorItemEventMonitor : EventMonitor
    {
        /// <summary>The f clustered by count.</summary>
        private int fClusteredByCount; // same as for links.

        /// <summary>The f link count.</summary>
        private int fLinkCount;

                    // we keep track of the link count seperatly, so that we don't need to access the links list each time this needs to be recalculated.

        /// <summary>Initializes a new instance of the <see cref="EditorItemEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public EditorItemEventMonitor(EditorItem toWrap)
            : base(toWrap)
        {
            fLinkCount = 0;
            if (toWrap.Item.LinksInIdentifier != null)
            {
                using (var iList = toWrap.Item.LinksIn) fLinkCount = iList.Count;
            }

            if (toWrap.Item.ClusteredByIdentifier != null)
            {
                using (var iList = toWrap.Item.ClusteredBy) fClusteredByCount = iList.Count;
            }
            else
            {
                fClusteredByCount = 0;
            }
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public EditorItem Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (EditorItem)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>The list changed.</summary>
        /// <param name="e">The e.</param>
        public override void ListChanged(NeuronListChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem != null)
            {
                switch (e.Action)
                {
                    case NeuronListChangeAction.Insert:
                        fClusteredByCount++;
                        break;
                    case NeuronListChangeAction.Remove:
                        fClusteredByCount--;
                        break;
                    default:
                        break;
                }

                iItem.UpdateRefCount(fClusteredByCount, fLinkCount);
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void ToLinkRemoved(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                fLinkCount--;
                iItem.UpdateRefCount(fClusteredByCount, fLinkCount);
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the to part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void ToLinkCreated(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                fLinkCount++;
                iItem.UpdateRefCount(fClusteredByCount, fLinkCount);
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            var iItem = Item;
            if (iItem != null)
            {
                iItem.OutgoingLinkRemoved(link);
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                iItem.OutgoingLinkCreated(link);
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the <paramref name="link"/> (so the other
        ///     side of the <paramref name="link"/> is changed, not the item on this
        ///     side).</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old Val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                iItem.OutgoingLinkCreated(link);
            }
        }
    }
}