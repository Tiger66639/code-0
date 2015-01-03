// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CPPItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for a single control in a <see cref="CodePagePanel" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A wrapper class for a single control in a <see cref="CodePagePanel" />
    /// </summary>
    public class CPPItem : CPPItemBase
    {
        /// <summary>The f data.</summary>
        private CodeItem fData;

        /// <summary>The f element.</summary>
        private System.Windows.Controls.UserControl fElement;

        /// <summary>Initializes a new instance of the <see cref="CPPItem"/> class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        public CPPItem(CPPItemBase owner, CodePagePanel panel)
            : base(owner, panel)
        {
        }

        #region Data

        /// <summary>
        ///     Gets/sets the code item that needs to be displayed.
        /// </summary>
        public virtual CodeItem Data
        {
            get
            {
                return fData;
            }

            set
            {
                fData = value;
                if (fElement != null)
                {
                    fElement.DataContext = fData;
                    Wrapper.DataContext = fData;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Called when the item is removed from the code page panel. It should
        ///     release all the resources, like remove usercontrols from the panel.
        /// </summary>
        protected internal override void Release()
        {
            Element = null; // we need to release the element, nothing else.
        }

        /// <summary>Measures this instance.</summary>
        /// <param name="available"></param>
        public override void Measure(System.Windows.Size available)
        {
            Wrapper.Measure(available);
            Size = Wrapper.DesiredSize;
            ZIndex = Owner.ZIndex + 1;
            System.Windows.Controls.Panel.SetZIndex(Wrapper, ZIndex);
        }

        /// <summary>Arranges the items.</summary>
        /// <param name="size">The total available size to this element. This is used to center the
        ///     object.</param>
        /// <param name="offset">The offset that should be applied to all items on the x and y points.
        ///     This is for nested items, to adjust correctly according to previous
        ///     items in the parents.</param>
        public override void Arrange(System.Windows.Size size, ref System.Windows.Point offset)
        {
            // the rect centers the element horizontally along the total available size and puts it at the top.
            var iRect = new System.Windows.Rect(
                ((size.Width - Wrapper.DesiredSize.Width) / 2) + offset.X, 

                // use element.desired size, not our size, cause this can include a list, if we are CPPHeaderedItemList.
                offset.Y, 
                Wrapper.DesiredSize.Width, 
                Wrapper.DesiredSize.Height);
            Wrapper.Arrange(iRect);
            offset.Offset(0, Wrapper.DesiredSize.Height);

                // we need to let the others know that the top has been used. Don't use the entire size, this can contain parts of the list as well (if we are a headeredlist), in which case, the values were added by the list itself.
        }

        #region Element

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public virtual System.Windows.Controls.UserControl Element
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
                        Panel.Children.Remove(Wrapper);
                        Wrapper = null;
                    }

                    fElement = value;
                    if (fElement != null)
                    {
                        fElement.Style = Panel.ElementStyle;
                        Wrapper = new System.Windows.Controls.ContentControl();
                        Wrapper.Content = fElement;
                        Wrapper.Style = Panel.ItemContainerStyle;
                        fElement.DataContext = Data;
                        Wrapper.DataContext = Data;
                        Panel.Children.Add(Wrapper);
                    }
                }
            }
        }

        /// <summary>Gets the wrapper.</summary>
        public System.Windows.Controls.ContentControl Wrapper { get; private set; }

        #endregion
    }
}