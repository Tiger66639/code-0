// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectLinks.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Allows the user to select all the visible links for a single neuron on a
//   mindmap.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Allows the user to select all the visible links for a single neuron on a
    ///     mindmap.
    /// </summary>
    public partial class DlgSelectLinks : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgSelectLinks"/> class.</summary>
        /// <param name="neuron">The neuron.</param>
        public DlgSelectLinks(MindMapNeuron neuron)
        {
            fNeuron = neuron;
            if (neuron.Item.LinksInIdentifier != null)
            {
                using (var iLinks = neuron.Item.LinksIn)
                    foreach (var i in iLinks)
                    {
                        CreateLinkItem(i, true);
                    }
            }

            if (neuron.Item.LinksOutIdentifier != null)
            {
                using (var iLinks = neuron.Item.LinksOut)
                    foreach (var i in iLinks)
                    {
                        CreateLinkItem(i, false);
                    }
            }

            if (neuron.Item.ClusteredByIdentifier != null)
            {
                using (var iList = neuron.Item.ClusteredBy)
                    foreach (var i in iList)
                    {
                        CreateNeuron(i, true);
                    }
            }

            var iCluster = neuron.Item as NeuronCluster;
            if (iCluster != null)
            {
                Children = new System.Collections.Generic.List<SelectableItem>();
                using (var iList = iCluster.Children)
                    foreach (var i in iList)
                    {
                        CreateNeuron(i, false);
                    }
            }

            Title = Title + neuron.NeuronInfo.DisplayTitle;

            InitializeComponent();
        }

        #region Internal types

        /// <summary>The selectable item.</summary>
        public class SelectableItem : Data.ObservableObject
        {
            #region Fields

            /// <summary>The f item.</summary>
            private Neuron fItem;

            #endregion

            /// <summary>Initializes a new instance of the <see cref="SelectableItem"/> class. Initializes a new instance of the<see cref="DlgSelectLinks.SelectableItem"/> class.</summary>
            /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
            public SelectableItem(bool isVisible)
            {
                IsVisible = isVisible;
                OriginalIsVisible = isVisible;
            }

            /// <summary>Initializes a new instance of the <see cref="SelectableItem"/> class.</summary>
            /// <param name="isVisible">The is visible.</param>
            /// <param name="item">The item.</param>
            public SelectableItem(bool isVisible, Neuron item)
            {
                IsVisible = isVisible;
                OriginalIsVisible = isVisible;
                Item = item;
            }

            #region Item

            /// <summary>
            ///     Gets/sets the neuron that this link points to.
            /// </summary>
            public Neuron Item
            {
                get
                {
                    return fItem;
                }

                set
                {
                    if (fItem != value)
                    {
                        fItem = value;
                        ItemData = BrainData.Current.NeuronInfo[value.ID];
                    }
                }
            }

            #endregion

            #region IsVisible

            /// <summary>
            ///     Gets/sets if this link should be displayed or not.
            /// </summary>
            public bool IsVisible { get; set; }

            #endregion

            #region OriginalIsVisible

            /// <summary>
            ///     Gets if the link is visible on the mindmap before any changes are
            ///     applied. This is used to check if the value actually changed.
            /// </summary>
            public bool OriginalIsVisible { get; internal set; }

            #endregion

            #region ItemData

            /// <summary>
            ///     Gets the extra data for the neuron that the link points to.
            /// </summary>
            public NeuronData ItemData { get; internal set; }

            #endregion
        }

        /// <summary>
        ///     <see cref="Helper" /> class for displaying all the links.
        /// </summary>
        public class LinkItem : SelectableItem
        {
            /// <summary>Initializes a new instance of the <see cref="LinkItem"/> class.</summary>
            /// <param name="isVisible">The is visible.</param>
            /// <param name="link">The link.</param>
            public LinkItem(bool isVisible, Link link)
                : base(isVisible)
            {
                Link = link;
            }

            #region Link

            /// <summary>
            ///     Gets the link that this objects provides a wrapper for.
            /// </summary>
            public Link Link { get; internal set; }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>The f incomming.</summary>
        private readonly System.Collections.Generic.List<LinkItem> fIncomming =
            new System.Collections.Generic.List<LinkItem>();

        /// <summary>The f outgoing.</summary>
        private readonly System.Collections.Generic.List<LinkItem> fOutgoing =
            new System.Collections.Generic.List<LinkItem>();

        /// <summary>The f parents.</summary>
        private readonly System.Collections.Generic.List<SelectableItem> fParents =
            new System.Collections.Generic.List<SelectableItem>();

        /// <summary>The f neuron.</summary>
        private readonly MindMapNeuron fNeuron;

        #endregion

        #region Prop

        #region Incomming

        /// <summary>
        ///     Gets the list of incomming links for the neuron in an appropriate
        ///     editable format for the UI.
        /// </summary>
        public System.Collections.Generic.List<LinkItem> Incomming
        {
            get
            {
                return fIncomming;
            }
        }

        #endregion

        #region Outgoing

        /// <summary>
        ///     Gets the list of outgoing links for the neuron in an appropriate
        ///     editable format for the UI.
        /// </summary>
        public System.Collections.Generic.List<LinkItem> Outgoing
        {
            get
            {
                return fOutgoing;
            }
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of children
        /// </summary>
        public System.Collections.Generic.List<SelectableItem> Children { get; private set; }

        #endregion

        #region ClusteredBy

        /// <summary>
        ///     Gets the list of <see cref="ClusteredBy" />
        /// </summary>
        public System.Collections.Generic.List<SelectableItem> ClusteredBy
        {
            get
            {
                return fParents;
            }
        }

        #endregion

        #region HasChildren

        /// <summary>
        ///     Gets the wether the specified neuron has any children or not (is a
        ///     NeuronCluster).
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return Children != null;
            }
        }

        #endregion

        #endregion

        #region Event handlers

        /// <summary>The window_ closing.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // if the user selected ok, update all the links.
            if (DialogResult == true)
            {
                WindowMain.UndoStore.BeginUndoGroup(true);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    ProcessList(fIncomming);
                    ProcessList(fOutgoing);
                    ProcessItems(ClusteredBy, true);
                    if (Children != null)
                    {
                        ProcessItems(Children, false);
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

        #region Helpers

        /// <summary>Processes the <paramref name="list"/> of links, checks if they are
        ///     selected or not and adds/removes mindmap items accordingly.</summary>
        /// <param name="list">The list.</param>
        private void ProcessList(System.Collections.Generic.List<LinkItem> list)
        {
            var iOffset = 0.0; // stores the offset to use for putting new neurons on the ui.
            foreach (var iLink in list)
            {
                if (iLink.OriginalIsVisible != iLink.IsVisible)
                {
                    if (iLink.IsVisible == false)
                    {
                        var iToRemove = -1;
                        for (var i = 0; i < fNeuron.Owner.Items.Count; i++)
                        {
                            var iItem = fNeuron.Owner.Items[i] as MindMapLink;
                            if (iItem != null
                                && ((list == fIncomming && iItem.To == fNeuron.ItemID && iItem.From == iLink.Item.ID)
                                    || (list == fOutgoing && iItem.From == fNeuron.ItemID && iItem.To == iLink.Item.ID)))
                            {
                                iToRemove = i;
                                break;
                            }
                        }

                        if (iToRemove > -1)
                        {
                            fNeuron.Owner.Items.RemoveAt(iToRemove);
                        }
                    }
                    else
                    {
                        fNeuron.Owner.ShowLinkBetween(fNeuron, iLink.Link, list == fIncomming, ref iOffset);
                    }
                }
            }
        }

        /// <summary>The process items.</summary>
        /// <param name="list">The list.</param>
        /// <param name="atTop">The at top.</param>
        private void ProcessItems(System.Collections.Generic.List<SelectableItem> list, bool atTop)
        {
            const double SPACE = 10;

            var iAdded = new System.Collections.Generic.List<MindMapNeuron>();
            var iTotalWidth = 0.0;
            var iMaxHeight = 0.0;
            foreach (var i in list)
            {
                if (i.OriginalIsVisible != i.IsVisible)
                {
                    if (i.IsVisible == false)
                    {
                        var iFound =
                            (from u in fNeuron.Owner.Items
                             where u is MindMapNeuron && ((MindMapNeuron)u).Item == i.Item
                             select (MindMapNeuron)u).FirstOrDefault();
                        if (iFound != null)
                        {
                            fNeuron.Owner.Items.Remove(iFound);
                        }
                    }
                    else
                    {
                        var iNew = fNeuron.Owner.CreateNewFor(i.Item, fNeuron.X, fNeuron.Y);
                        iAdded.Add(iNew);
                        iMaxHeight = System.Math.Max(iMaxHeight, iNew.Height);
                        iTotalWidth += iNew.Width;
                    }
                }
            }

            // we have found them all, now arrange them properly.
            var iStart = System.Math.Max(fNeuron.X - ((iTotalWidth + (iAdded.Count * SPACE)) / 2), 0);

                // we put the parents side by side, with a little spacing in between and the child neuron in the center.
            foreach (var i in iAdded)
            {
                if (atTop)
                {
                    i.Y -= iMaxHeight + SPACE;
                }
                else
                {
                    i.Y += iMaxHeight + SPACE;
                }

                i.X = iStart;
                iStart += i.Width + SPACE;
            }
        }

        /// <summary>Creates a sinlge link <paramref name="item"/> and adds it to the list.</summary>
        /// <param name="item">the link for which to create a wrapper.</param>
        /// <param name="asIncomming">The as Incomming.</param>
        private void CreateLinkItem(Link item, bool asIncomming)
        {
            LinkItem iNew;
            var iRes =
                (from iItem in fNeuron.Owner.Items
                 where iItem is MindMapLink && ((MindMapLink)iItem).Link == item
                 select iItem).FirstOrDefault();
            iNew = new LinkItem(iRes != null, item);
            if (asIncomming)
            {
                iNew.Item = item.From;
                fIncomming.Add(iNew);
            }
            else
            {
                iNew.Item = item.To;
                fOutgoing.Add(iNew);
            }
        }

        /// <summary>Creates a new selectable item for the neuron with the specified id.
        ///     Makes certain that it is <see langword="checked"/> when the item is
        ///     already on the mindmap.</summary>
        /// <remarks><see cref="fParents"/> and fChildren lists must exist.</remarks>
        /// <param name="id">The id.</param>
        /// <param name="asParent">if set to <c>true</c> [as parent].</param>
        private void CreateNeuron(ulong id, bool asParent)
        {
            SelectableItem iNew;
            var iItem = Brain.Current[id];
            var iRes =
                (from i in fNeuron.Owner.Items where i is MindMapNeuron && ((MindMapNeuron)i).Item == iItem select i)
                    .FirstOrDefault();
            iNew = new SelectableItem(iRes != null);
            iNew.Item = iItem;
            if (asParent)
            {
                fParents.Add(iNew);
            }
            else
            {
                Children.Add(iNew);
            }
        }

        #endregion
    }
}