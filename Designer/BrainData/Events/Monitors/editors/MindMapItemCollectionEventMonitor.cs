// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapItemCollectionEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   event monitor for mindmap item collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     event monitor for mindmap item collection.
    /// </summary>
    internal class MindMapItemCollectionEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="MindMapItemCollectionEventMonitor"/> class. Initializes a new instance of the<see cref="MindMapItemCollectionEventMonitor"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public MindMapItemCollectionEventMonitor(MindMapItemCollection toWrap)
            : base(toWrap)
        {
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public MindMapItemCollection Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (MindMapItemCollection)Reference.Target;
                }

                return null;
            }
        }

        #region overrides

        /// <summary>Called when a neuron was changed.</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        public override void NeuronChanged(NeuronChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (e.Action == BrainAction.Removed)
                {
                    iItem.RemoveNeuron(e.OriginalSource);
                }
                else if (e.Action == BrainAction.Changed && !(e is NeuronPropChangedEventArgs))
                {
                    iItem.UpdateItemsForNeuronChange(e.OriginalSource, e.NewValue);
                }
            }
        }

        /// <summary>Called when a link was changed.</summary>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        public override void LinkChanged(LinkChangedEventArgs e)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    // if the change is comming from another thread do async calls cause collectionChanges arent' handled correctly otherwise.
                    if (e.Action == BrainAction.Removed)
                    {
                        iItem.RemoveLink(e.OriginalSource);
                    }
                    else if (e.Action == BrainAction.Changed)
                    {
                        if (e.NewFrom != e.OldFrom)
                        {
                            iItem.UpdateFromPart(e);
                        }
                        else if (e.NewTo != e.OldTo)
                        {
                            iItem.UpdateToPart(e);
                        }
                    }
                }
                else
                {
                    var iDispatcher = System.Windows.Application.Current.Dispatcher;
                    if (e.Action == BrainAction.Removed)
                    {
                        iDispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            new System.Action<Link>(iItem.RemoveLink), 
                            e.OriginalSource);
                    }
                    else if (e.Action == BrainAction.Changed)
                    {
                        if (e.NewFrom != e.OldFrom)
                        {
                            iDispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<LinkChangedEventArgs>(iItem.UpdateFromPart), 
                                e);
                        }
                        else if (e.NewTo != e.OldTo)
                        {
                            iDispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal, 
                                new System.Action<LinkChangedEventArgs>(iItem.UpdateToPart), 
                                e);
                        }
                    }
                }
            }
        }

        #endregion

        #region helpers

        /// <summary>The clear.</summary>
        internal void Clear()
        {
            var iNeurons = from i in Item where i is MindMapNeuron select ((MindMapNeuron)i).ItemID;
            EventManager.Current.RemoveNeuronChangedMonitor(this, iNeurons);
            var iLinks = from i in Item where i is MindMapLink select ((MindMapLink)i).Link;
            EventManager.Current.UnRegisterForLinkChange(iLinks, this);
        }

        /// <summary>Adds the item.</summary>
        /// <param name="id">The id.</param>
        internal void AddItem(ulong id)
        {
            EventManager.Current.AddNeuronChangedMonitor(this, id);
        }

        /// <summary>Removes the item.</summary>
        /// <param name="id">The id.</param>
        internal void RemoveItem(ulong id)
        {
            EventManager.Current.RemoveNeuronChangedMonitor(this, id);
        }

        /// <summary>Adds the link.</summary>
        /// <param name="link">The link.</param>
        internal void AddLink(Link link)
        {
            if (link != null)
            {
                // this somehow happens sometimes
                EventManager.Current.RegisterForLinkChange(new System.Collections.Generic.List<Link> { link }, this);
            }
        }

        /// <summary>Removes the link.</summary>
        /// <param name="link">The link.</param>
        internal void RemoveLink(Link link)
        {
            if (link != null)
            {
                // this somehow happens sometimes
                EventManager.Current.UnRegisterForLinkChange(new System.Collections.Generic.List<Link> { link }, this);
            }
        }

        #endregion
    }
}