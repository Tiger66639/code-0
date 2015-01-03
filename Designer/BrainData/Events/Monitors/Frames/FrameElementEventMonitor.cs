// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameElementEventMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   The frame element event monitor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame element event monitor.</summary>
    internal class FrameElementEventMonitor : EventMonitor
    {
        /// <summary>Initializes a new instance of the <see cref="FrameElementEventMonitor"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FrameElementEventMonitor(FrameElement toWrap)
            : base(toWrap)
        {
            EventManager.Current.RegisterForLinksOut(this, toWrap.Item.ID);
        }

        /// <summary>
        ///     Gets the item being wrapped.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public FrameElement Item
        {
            get
            {
                if (Reference.IsAlive)
                {
                    return (FrameElement)Reference.Target;
                }

                return null;
            }
        }

        /// <summary>called when a <paramref name="link"/> was removed where this monitor
        ///     wraps the from part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        /// <param name="oldValue">The old Value.</param>
        public override void FromLinkRemoved(Link link, ulong oldValue)
        {
            UpdateChange(link);
        }

        /// <summary>called when a <paramref name="link"/> was created where this monitor
        ///     wraps the From part of the <paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        public override void FromLinkCreated(Link link)
        {
            UpdateChange(link);
        }

        /// <summary>The to changed.</summary>
        /// <param name="link">The link.</param>
        /// <param name="oldVal">The old val.</param>
        public override void ToChanged(Link link, ulong oldVal)
        {
            UpdateChange(link);
        }

        /// <summary>The update change.</summary>
        /// <param name="link">The link.</param>
        private void UpdateChange(Link link)
        {
            var iItem = Item;
            if (iItem != null)
            {
                if (link.MeaningID == (ulong)PredefinedNeurons.FrameImportance)
                {
                    iItem.CallPropertyChangedChanged("Importance");

                        // can be done from the sub thread, is a simple prop update.
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.FrameElementResultType)
                {
                    iItem.CallPropertyChangedChanged("ElementResultType");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRole)
                {
                    if (iItem.NeuronInfo != null && string.IsNullOrEmpty(iItem.NeuronInfo.Title))
                    {
                        // if the element doesn't have a title yet, give it the same one as that of the role.
                        iItem.NeuronInfo.DisplayTitle = BrainData.Current.NeuronInfo[link.ToID].Title;
                    }

                    iItem.CallPropertyChangedChanged("VerbNetRole");

                        // can be done from the sub thread, is a simple prop update.
                    iItem.CallPropertyChangedChanged("VerbNetRoleVisual");
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.VerbNetRestrictions)
                {
                    var iResrictionsCluster =
                        iItem.Item.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions) as NeuronCluster;
                    if (iResrictionsCluster != null && iItem.RestrictionsRoot.Items.Cluster != iResrictionsCluster)
                    {
                        iItem.RestrictionsRoot.Items = new RestrictionsCollection(iItem, iResrictionsCluster);
                    }
                    else if (iResrictionsCluster == null)
                    {
                        iItem.RestrictionsRoot.Items = null;
                    }
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.IsFrameEvoker)
                {
                    iItem.ResetEvoker();
                }
                else if (link.MeaningID == (ulong)PredefinedNeurons.FrameElementAllowMulti)
                {
                    iItem.ResetAllowMulti();
                }
            }
        }
    }
}