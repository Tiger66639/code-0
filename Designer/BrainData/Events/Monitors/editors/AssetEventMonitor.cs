// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   event monitor for asset items
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     event monitor for asset items
    /// </summary>
    internal class AssetEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="AssetEventMonitor"/> class. Initializes a new instance of the <see cref="AssetEventMonitor"/>
        ///     class.</summary>
        /// <param name="toWarp">To warp.</param>
        public AssetEventMonitor(AssetItem toWarp)
            : base(toWarp)
        {
            EventManager.Current.RegisterForLinksOut(this, toWarp.Item.ID);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public AssetItem Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (AssetItem)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>
        ///     Removes all the callbacks from the event manager. After this call, the
        ///     object won't be notified anymore from any changes.
        /// </summary>
        public void Unregister()
        {
            EventManager.Current.UnRegisterForLinksOut(this, Item.Item.ID);
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old value of the from part in the link.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.Attribute)
            {
                Item.SetAttribute(null);
            }
            else
            {
                var iFound = (from i in Item.Data where i.LinkID == link.MeaningID select i).FirstOrDefault();
                if (iFound != null)
                {
                    iFound.SetValue(null);
                }
            }
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            HandleLink(link);
        }

        /// <summary>Handles the link.</summary>
        /// <param name="link">The link.</param>
        private void HandleLink(Link link)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.Attribute)
            {
                Item.SetAttribute(link.To);
            }
            else
            {
                var iFound = (from i in Item.Data where i.LinkID == link.MeaningID select i).FirstOrDefault();
                if (iFound != null)
                {
                    iFound.SetValue(link.To);
                }
            }
        }

        /// <summary>Called when a <paramref name="link"/> was changed where this monitor
        ///     wraps the 'From' part of the link.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The neuron that previously was the 'to' value.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            HandleLink(link);
        }
    }
}