// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowPanelItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper for normal flow items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A wrapper for normal flow items.
    /// </summary>
    public class FlowPanelItem : FlowPanelItemBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FlowPanelItem"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public FlowPanelItem(FlowPanelItemBase owner, FlowPanel panel)
            : base(owner, panel)
        {
        }

        #endregion

        #region Element

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public virtual System.Windows.Controls.ContentControl Element
        {
            get
            {
                return fElement;
            }

            set
            {
                if (value != fElement)
                {
                    if (fElement != null)
                    {
                        Panel.Children.Remove(fElement);
                        fElement.Tag = null;
                    }

                    fElement = value;
                    if (fElement != null)
                    {
                        fElement.Style = Panel.ElementStyle;
                        fElement.DataContext = Data;
                        Panel.Children.Add(fElement);
                        fElement.Tag = this;

                            // the UI control needs to know about the flowPanelItem, so we can move with the cursor.
                    }
                }
            }
        }

        #endregion

        #region Data

        /// <summary>
        ///     Gets/sets the code item that needs to be displayed.
        /// </summary>
        public virtual FlowItem Data
        {
            get
            {
                return fData;
            }

            set
            {
                if (fData != null)
                {
                    System.Windows.Data.BindingOperations.ClearBinding(this, IsSelectedProperty);
                }

                fData = value;
                if (fElement != null)
                {
                    fElement.DataContext = fData;
                }

                if (fData != null)
                {
                    var iBind = new System.Windows.Data.Binding("IsSelected")
                                    {
                                        Source = fData, 
                                        Mode =
                                            System.Windows.Data.BindingMode
                                            .OneWay
                                    };
                    System.Windows.Data.BindingOperations.SetBinding(this, IsSelectedProperty, iBind);

                        // this has to be done from code since we are binding to this object, which is unreacheable from xaml.
                }
            }
        }

        #endregion

        #region fields

        /// <summary>The f element.</summary>
        private System.Windows.Controls.ContentControl fElement;

        /// <summary>The f data.</summary>
        private FlowItem fData;

        #endregion

        #region Functions

        /// <summary>Arranges the items.</summary>
        /// <remarks>Doesn't advance the <paramref name="offset"/> itself. This is done by
        ///     the parent list.</remarks>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public override void Arrange(System.Windows.Size size, System.Windows.Point offset)
        {
            // the rect puts the element horizontally, left
            var iRect = new System.Windows.Rect(
                offset.X, 
                offset.Y + ((size.Height - Size.Height) / 2), 

                // we need to center it vertically.
                Element.DesiredSize.Width, 
                Element.DesiredSize.Height);
            Element.Arrange(iRect);
        }

        /// <summary>Measures this instance.</summary>
        /// <param name="available"></param>
        public override void Measure(System.Windows.Size available)
        {
            Element.Measure(available);
            Size = Element.DesiredSize;
            ZIndex = Owner.ZIndex + 1;
            System.Windows.Controls.Panel.SetZIndex(Element, ZIndex);
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            Element = null; // we need to release the element, nothing else.
        }

        /// <summary>Gets the last UI element of the <see cref="FlowPanel"/> item, to be
        ///     used for calculating the position when displaying the drop down.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        protected internal override System.Windows.FrameworkElement GetLastUI()
        {
            return Element;
        }

        /// <summary>
        ///     Moves focuses to the ui element appropriate for this item.
        /// </summary>
        protected internal override void Focus()
        {
            Data.IsSelected = true;
            Element.Focus();
        }

        /// <summary>
        ///     Moves focus down.
        /// </summary>
        protected internal override void MoveDown()
        {
            var iList = Owner as FlowPanelItemList;
            var iListOwner = iList.Owner as EnclosedFlowPanelItemList;
            if (iListOwner != null)
            {
                var iIndex = iListOwner.List.Children.IndexOf(iList);
                if (iIndex < iListOwner.List.Children.Count - 1)
                {
                    var iSource = iListOwner.List.Children[iIndex + 1] as FlowPanelItemList;
                    iSource.FocusChildAt(iList.GetRangeOf(this));
                }
                else
                {
                    iListOwner.Focus();
                }
            }
        }

        /// <summary>
        ///     Moves focus to the left.
        /// </summary>
        protected internal override void MoveLeft()
        {
            var iList = Owner as FlowPanelItemList;
            var iIndex = iList.Children.IndexOf(this);
            if (iIndex > 0)
            {
                iList.Children[iIndex - 1].Focus();
            }
            else
            {
                iList.Focus();
            }
        }

        /// <summary>
        ///     Moves focus to the right.
        /// </summary>
        protected internal override void MoveRight()
        {
            var iList = Owner as FlowPanelItemList;
            var iIndex = iList.Children.IndexOf(this);
            if (iIndex < iList.Children.Count - 1)
            {
                iList.Children[iIndex + 1].Focus();
            }
            else
            {
                var iListOwner = ((FlowPanelItemList)iList.Owner).Owner as EnclosedFlowPanelItemList;
                if (iListOwner != null)
                {
                    iListOwner.FocusToRight();
                }
                else
                {
                    iList.Focus();
                }
            }
        }

        /// <summary>The move up.</summary>
        protected internal override void MoveUp()
        {
            var iList = Owner as FlowPanelItemList;
            var iListOwner = iList.Owner as EnclosedFlowPanelItemList;
            if (iListOwner != null)
            {
                var iIndex = iListOwner.List.Children.IndexOf(iList);
                if (iIndex > 0)
                {
                    var iSource = iListOwner.List.Children[iIndex - 1] as FlowPanelItemList;
                    iSource.FocusChildAt(iList.GetRangeOf(this));
                }
                else
                {
                    iListOwner.Focus();
                }
            }
        }

        #endregion
    }
}