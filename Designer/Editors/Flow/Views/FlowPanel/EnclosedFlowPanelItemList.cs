// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnclosedFlowPanelItemList.cs" company="">
//   
// </copyright>
// <summary>
//   Manages a list of controls that are enclosed by a start and end path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Manages a list of controls that are enclosed by a start and end path.
    /// </summary>
    internal class EnclosedFlowPanelItemList : FlowPanelItemBase
    {
        /// <summary>Initializes a new instance of the <see cref="EnclosedFlowPanelItemList"/> class. Initializes a new instance of the<see cref="EnclosedFlowPanelItemList"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public EnclosedFlowPanelItemList(FlowPanelItemBase owner, FlowPanel panel)
            : base(owner, panel)
        {
            List = new FlowPanelItemList(this, panel);
        }

        /// <summary>Measures this instance.</summary>
        /// <param name="available"></param>
        public override void Measure(System.Windows.Size available)
        {
            if (Owner != null)
            {
                ZIndex = Owner.ZIndex + 1;
            }
            else
            {
                ZIndex = 0;
            }

            List.Measure(available);

            var iSize = new System.Windows.Size(
                double.PositiveInfinity, 
                System.Math.Max(List.Size.Height, System.Math.Max(Front.MinHeight, Back.MinHeight)));
            Front.Measure(iSize);
            System.Windows.Controls.Panel.SetZIndex(fFront, ZIndex - 1);

            Back.Measure(iSize);
            System.Windows.Controls.Panel.SetZIndex(fBack, ZIndex - 1);
            Size = new System.Windows.Size(
                List.Size.Width + Front.DesiredSize.Width + Back.DesiredSize.Width, 
                iSize.Height);
        }

        /// <summary>Arranges the items.</summary>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public override void Arrange(System.Windows.Size size, System.Windows.Point offset)
        {
            var iTotalY = offset.Y + ((size.Height - Size.Height) / 2);
            var iOffset = new System.Windows.Point(offset.X, iTotalY);
            var iNew = new System.Windows.Rect(iOffset, Front.DesiredSize);
            Front.Arrange(iNew);
            iOffset = new System.Windows.Point(
                offset.X + Front.DesiredSize.Width, 
                offset.Y + ((size.Height - Size.Height) / 2));
            List.Arrange(size, iOffset);
            iNew = new System.Windows.Rect(
                offset.X + Front.DesiredSize.Width + List.Size.Width, 
                iTotalY, 
                Back.DesiredSize.Width, 
                Back.DesiredSize.Height);
            Back.Arrange(iNew);
        }

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            List.Release();
            if (Front != null)
            {
                Panel.Children.Remove(Front);
            }

            if (Back != null)
            {
                Panel.Children.Remove(Back);
            }
        }

        /// <summary>Gets the last UI element of the <see cref="FlowPanel"/> item, to be
        ///     used for calculating the position when displaying the drop down.</summary>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        protected internal override System.Windows.FrameworkElement GetLastUI()
        {
            if (fBack != null)
            {
                return fBack;
            }

            return fFront;
        }

        /// <summary>
        ///     Moves focuses to the ui element appropriate for this item.
        /// </summary>
        protected internal override void Focus()
        {
            Data.IsSelected = true;
            System.Diagnostics.Debug.Assert(Front != null);
            Front.Focus();
        }

        /// <summary>The focus to right.</summary>
        internal void FocusToRight()
        {
            Data.IsSelected = true;
            System.Diagnostics.Debug.Assert(Back != null);
            Back.Focus();
        }

        /// <summary>The move down.</summary>
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

        /// <summary>The move left.</summary>
        protected internal override void MoveLeft()
        {
            if (Back.IsFocused && List.Children.Count > 0)
            {
                List.Children[List.Children.Count - 1].Focus();
            }
            else
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
        }

        /// <summary>The move right.</summary>
        protected internal override void MoveRight()
        {
            if (Front.IsFocused && List.Children.Count > 0)
            {
                List.Children[0].Focus();
            }
            else
            {
                var iList = Owner as FlowPanelItemList;
                var iIndex = iList.Children.IndexOf(this);
                if (iIndex < iList.Children.Count - 1)
                {
                    iList.Children[iIndex + 1].Focus();
                }
                else if (iList.Owner != null)
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
                else
                {
                    iList.Focus();
                }
            }
        }

        #region Fields

        /// <summary>The f back.</summary>
        private System.Windows.FrameworkElement fBack;

        /// <summary>The f front.</summary>
        private System.Windows.FrameworkElement fFront;

        /// <summary>The f data.</summary>
        private FlowItem fData;

        #endregion

        #region Prop

        #region Front

        /// <summary>
        ///     Gets or sets the ui element to put in front of the list.
        /// </summary>
        /// <value>
        ///     The front.
        /// </value>
        public System.Windows.FrameworkElement Front
        {
            get
            {
                return fFront;
            }

            set
            {
                if (value != fFront)
                {
                    if (fFront != null)
                    {
                        Panel.Children.Remove(fFront);
                        fFront.Tag = null;
                    }

                    fFront = value;
                    if (fFront != null)
                    {
                        fFront.DataContext = fData;
                        Panel.Children.Add(fFront);
                        fFront.Tag = this;

                            // the UI control needs to know about the flowPanelItem, so we can move with the cursor.
                    }
                }
            }
        }

        #endregion

        #region Back

        /// <summary>
        ///     Gets or sets the ui element to put after the list.
        /// </summary>
        /// <value>
        ///     The back.
        /// </value>
        public System.Windows.FrameworkElement Back
        {
            get
            {
                return fBack;
            }

            set
            {
                if (value != fBack)
                {
                    if (fBack != null)
                    {
                        Panel.Children.Remove(fBack);
                        fBack.Tag = null;
                    }

                    fBack = value;
                    if (fBack != null)
                    {
                        fBack.DataContext = fData;
                        Panel.Children.Add(fBack);
                        fBack.Tag = this;

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
                if (value != fData)
                {
                    if (fData != null)
                    {
                        System.Windows.Data.BindingOperations.ClearBinding(this, IsSelectedProperty);
                    }

                    fData = value;
                    if (fBack != null)
                    {
                        fBack.DataContext = fData;
                    }

                    if (fFront != null)
                    {
                        fFront.DataContext = fData;
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
                    }
                }
            }
        }

        #endregion

        #region List

        /// <summary>
        ///     Gets the list.
        /// </summary>
        /// <value>
        ///     The list.
        /// </value>
        public FlowPanelItemList List { get; private set; }

        #endregion

        #endregion
    }
}