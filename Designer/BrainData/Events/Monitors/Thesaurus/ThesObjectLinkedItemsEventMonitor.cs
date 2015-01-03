﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesObjectLinkedItemsEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The thes object linked items event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The thes object linked items event monitor.</summary>
    internal class ThesObjectLinkedItemsEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="ThesObjectLinkedItemsEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public ThesObjectLinkedItemsEventMonitor(ThesObjectLinkedItems toWrap)
            : base(toWrap)
        {
            EventManager.Current.RegisterForLinksOut(this, toWrap.Item.ID);
        }

        /// <summary>Gets the item.</summary>
        public ThesObjectLinkedItems Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (ThesObjectLinkedItems)Reference.Target;
                }

                return null;
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
                var iMeaning = link.Meaning as NeuronCluster;
                if (iMeaning != null && iMeaning.Meaning == (ulong)PredefinedNeurons.Object)
                {
                    if (iItem.Items.Count > 0)
                    {
                        // when we do an UI 'add' operation which gets commited, we don't want to create the ThesaurusLinkedItem object again cause then the user would see the same line 2 times.
                        var iLast = iItem.Items[iItem.Items.Count - 1];
                        if (iLast.Related.ID == link.ToID && iLast.Relationship.ID == link.MeaningID)
                        {
                            return;
                        }
                    }

                    var iNew = new ThesaurusLinkedItem(iItem.Item, iMeaning, link.To);
                    iItem.Items.Add(iNew);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value of the from part in the link.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            var iItem = Item;
            if (iItem != null)
            {
                var iFound = (from i in iItem.Items
                              where
                                  (i.Related == null && i.Relationship.ID == link.MeaningID)
                                  || i.Related.ID == link.ToID
                              select i).FirstOrDefault();
                if (iFound != null)
                {
                    iItem.Items.Remove(iFound);
                }
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'to' value.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            var iItem = Item;
            if (iItem != null)
            {
                var iFound = (from i in iItem.Items where i.Related.ID == oldVal select i).FirstOrDefault();
                if (iFound != null)
                {
                    iFound.Related = link.To;
                }
                else
                {
                    iFound = (from i in iItem.Items where i.Relationship.ID == oldVal select i).FirstOrDefault();
                    if (iFound != null)
                    {
                        var iMeaning = link.Meaning as NeuronCluster;
                        if (iMeaning != null && iMeaning.Meaning == (ulong)PredefinedNeurons.Object)
                        {
                            iFound.Relationship = iMeaning;
                        }
                        else
                        {
                            iItem.Items.Remove(iFound);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Unregisters this instance with the event monitor.
        /// </summary>
        internal void Unregister()
        {
            EventManager.Current.UnRegisterForLinksOut(this, Item.Item.ID);
        }
    }
}