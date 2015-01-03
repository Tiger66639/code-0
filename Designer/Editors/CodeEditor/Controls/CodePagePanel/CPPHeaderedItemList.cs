// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CPPHeaderedItemList.cs" company="">
//   
// </copyright>
// <summary>
//   An code page panel item that also has a list, which can be expanded or
//   collapsed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     An code page panel item that also has a list, which can be expanded or
    ///     collapsed.
    /// </summary>
    public class CPPHeaderedItemList : CPPItem
    {
        /// <summary>Initializes a new instance of the <see cref="CPPHeaderedItemList"/> class. Initializes a new instance of the <see cref="CPPHeaderedItemList"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public CPPHeaderedItemList(CPPItemBase owner, CodePagePanel panel)
            : base(owner, panel)
        {
            List = new CPPItemList(this, panel);
            Expander = new CtrlExpander();
            Expander.Header = this;
            panel.Children.Add(Expander);
        }

        #region Data

        /// <summary>
        ///     Gets/sets the code item that needs to be displayed.
        /// </summary>
        /// <value>
        /// </value>
        public override CodeItem Data
        {
            get
            {
                return base.Data;
            }

            set
            {
                base.Data = value;
                Expander.DataContext = value;
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        /// <value>
        /// </value>
        /// <example>
        ///     When the element changes, we need to make certain that the expander is
        ///     removed from the previous and added to the new wrapper as one of it's
        ///     visuals during dragging.
        /// </example>
        public override System.Windows.Controls.UserControl Element
        {
            get
            {
                return base.Element;
            }

            set
            {
                if (Element != null)
                {
                    Expander.ClearValue(DnD.DragDropManager.VisualsProperty);
                    Wrapper.ClearValue(DnD.DragDropManager.VisualsProperty);
                }

                base.Element = value;
                if (Element != null)
                {
                    var iList = new System.Collections.Generic.List<System.WeakReference>();
                    iList.Add(new System.WeakReference(Wrapper));
                    iList.Add(new System.WeakReference(Expander));
                    DnD.DragDropManager.SetVisuals(Wrapper, iList);

                        // when the wrapper or expander are dragged, we display both
                    DnD.DragDropManager.SetVisuals(Expander, iList);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the list part of the item.
        /// </summary>
        /// <value>
        ///     The list.
        /// </value>
        public CPPItemList List
        {
            get
            {
                return fList;
            }

            private set
            {
                fList = value;
                if (ExtraList != null)
                {
                    fList.SubList = ExtraList;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the list normally used for the code items that go in
        ///     front of the conditional parts. this is normally null, but the
        ///     conditional statement assigns an object to this.
        /// </summary>
        /// <value>
        ///     The list.
        /// </value>
        public CPPItemList ExtraList
        {
            get
            {
                return fExtraList;
            }

            set
            {
                fExtraList = value;
                if (List != null)
                {
                    List.SubList = value;
                }

                if (value != null)
                {
                    value.IsSubList = true;
                }
            }
        }

        /// <summary>
        ///     Gets the expander control, used to expand/collaps the objects.
        /// </summary>
        /// <value>
        ///     The expander.
        /// </value>
        public CtrlExpander Expander { get; private set; }

        /// <summary>
        ///     Gets the size of the element itself, without the list.
        /// </summary>
        /// <value>
        ///     The size of the element.
        /// </value>
        public System.Windows.Size ElementSize { get; private set; }

        /// <summary>Measures this instance.</summary>
        /// <remarks><para>levels:</para>
        /// <list type="number"><item><description>children/droptarget</description></item>
        /// <item><description>Codeline -&gt; same as list</description></item>
        /// <item><description>background</description></item>
        /// <item><description>header -&gt; this</description></item>
        /// <item><description>parent</description></item>
        /// </list>
        /// </remarks>
        /// <param name="available"></param>
        public override void Measure(System.Windows.Size available)
        {
            base.Measure(available);
            ElementSize = Size;
            Expander.Measure(available);
            System.Windows.Controls.Panel.SetZIndex(Expander, ZIndex + 3); // expander at same level as children of list.
            List.ZIndex = ZIndex + 2;
            List.Measure(available);
            var iDesiredExp = Expander.DesiredSize;
            if (ExtraList != null)
            {
                ExtraList.ZIndex = ZIndex + 2;
                ExtraList.Measure(available);
                if (List.IsExpanded)
                {
                    Size =
                        new System.Windows.Size(
                            System.Math.Max(
                                Size.Width + iDesiredExp.Width + (EXPANDERMARGIN * 2), 
                                System.Math.Max(List.Size.Width, ExtraList.Size.Width)), 
                            Size.Height + List.Size.Height + ExtraList.Size.Height + EXPANDERMARGIN);

                        // ExpanderMarging * 2 -> a little border on both sides, same for below.
                }
                else
                {
                    Size =
                        new System.Windows.Size(
                            System.Math.Max(
                                Size.Width + iDesiredExp.Width + (EXPANDERMARGIN * 2), 
                                System.Math.Max(List.Size.Width, ExtraList.Size.Width)), 
                            Size.Height + List.Size.Height + ExtraList.Size.Height);

                        // when not expanded, we don't need to add some space at the bottom
                }
            }
            else if (List.IsExpanded)
            {
                Size =
                    new System.Windows.Size(
                        System.Math.Max(Size.Width + iDesiredExp.Width + (EXPANDERMARGIN * 2), List.Size.Width), 
                        Size.Height + List.Size.Height + EXPANDERMARGIN);

                    // ExpanderMarging * 2 -> a little border on both sides, same for below.
            }
            else
            {
                Size =
                    new System.Windows.Size(
                        System.Math.Max(Size.Width + iDesiredExp.Width + (EXPANDERMARGIN * 2), List.Size.Width), 
                        Size.Height + List.Size.Height);

                    // when not expanded, we don't need to add some space at the bottom
            }
        }

        /// <summary>Arranges the items.</summary>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public override void Arrange(System.Windows.Size size, ref System.Windows.Point offset)
        {
            var iListRect = new System.Windows.Rect(
                ((size.Width - Size.Width) / 2) + offset.X, 
                offset.Y, 
                Size.Width, 
                Size.Height);

                // the rectangle of the total list and header, so we can show the selection box correctly. Needs to be calculated before we modify the offset values.
            var iRect =
                new System.Windows.Rect(
                    ((size.Width - Expander.DesiredSize.Width - ElementSize.Width) / 2) + offset.X - EXPANDERMARGIN, 
                    offset.Y, 
                    Expander.DesiredSize.Width, 
                    ElementSize.Height); // we give the expander the same height as the element, for dragging.
            Expander.Arrange(iRect);

            // Point iOffset = new Point(offset.X + Expander.DesiredSize.Width, offset.Y);
            base.Arrange(size, ref offset);
            if (ExtraList != null)
            {
                ExtraList.Arrange(size, ref offset);
            }

            List.Arrange(size, ref offset);
            CodePagePanel.SetSelectionRect(Element, iListRect);
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            base.Release();
            Panel.Children.Remove(Expander);
            List.Release();
            if (ExtraList != null)
            {
                ExtraList.Release();
            }
        }

        #region Fields

        /// <summary>The expandermargin.</summary>
        protected const double EXPANDERMARGIN = 8.0;

        /// <summary>The f list.</summary>
        private CPPItemList fList;

        /// <summary>The f extra list.</summary>
        private CPPItemList fExtraList;

        #endregion
    }
}