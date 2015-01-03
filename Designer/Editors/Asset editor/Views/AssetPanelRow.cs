// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetPanelRow.cs" company="">
//   
// </copyright>
// <summary>
//   The asset panel row.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer.WPF.Controls
{
    using System.Linq;

    /// <summary>The asset panel row.</summary>
    public class AssetPanelRow : System.Windows.FrameworkElement
    {
        /// <summary>The f visuals.</summary>
        private readonly System.Windows.Media.VisualCollection fVisuals;

        /// <summary>Initializes a new instance of the <see cref="AssetPanelRow"/> class.</summary>
        public AssetPanelRow()
        {
            DataContextChanged += AssetPanelRow_DataContextChanged;
            fVisuals = new System.Windows.Media.VisualCollection(this);
        }

        /// <summary>
        ///     Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>
        ///     The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return fVisuals.Count;
            }
        }

        /// <summary>Overrides <see cref="System.Windows.Media.Visual.GetVisualChild"/> , and returns a child
        ///     at the specified <paramref name="index"/> from a collection of child
        ///     elements.</summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element. This should not return null; if the
        ///     provided <paramref name="index"/> is out of range, an exception is
        ///     thrown.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return fVisuals[index];
        }

        /// <summary>The asset panel row_ data context changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AssetPanelRow_DataContextChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                if (e.NewValue == null)
                {
                    ClearItems();
                }
                else
                {
                    ReplaceItems(e.NewValue);
                }
            }
            else if (e.NewValue != null)
            {
                BuildItems((AssetBase)e.NewValue);
            }
        }

        /// <summary>Called to arrange and size the content of a <see cref="System.Windows.Controls.Control"/>
        ///     object.</summary>
        /// <param name="arrangeBounds">The computed size that is used to arrange the content.</param>
        /// <returns>The size of the control.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            var iItem = DataContext as AssetItem;
            if (iItem != null)
            {
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<AssetPanel>(this); // Parent as AssetPanel;
                double iOffset = 0;
                if (iPanel != null)
                {
                    iOffset = Level * iPanel.LevelDepth;

                        // the first column has to be smaller, to compensate for the expand/collaps UI's
                }

                var iSize = new System.Windows.Size(0, double.PositiveInfinity); // height can be determined by the item.
                var iControls = from System.Windows.Controls.ContentControl i in fVisuals
                                orderby ((AssetColumn)i.Tag).Index
                                select i; // sort the list according to the index position of the column.
                foreach (var i in iControls)
                {
                    var iCol = (AssetColumn)i.Tag;
                    var iWidth = iCol.Width - iOffset < 0 ? 0 : iCol.Width - iOffset;
                    var iRect = new System.Windows.Rect(
                        new System.Windows.Point(iSize.Width, 0), 
                        new System.Windows.Size(iWidth, i.DesiredSize.Height));
                    if (iOffset < iCol.Width)
                    {
                        // could be that the offset is grater than 1 col.
                        iOffset = 0;
                    }
                    else
                    {
                        iOffset -= iCol.Width;
                    }

                    iSize.Width += iCol.Width;
                    i.Arrange(iRect);
                }
            }

            return arrangeBounds;
        }

        /// <summary>Called to remeasure a control.</summary>
        /// <param name="constraint">The maximum size that the method can return.</param>
        /// <returns>The size of the control, up to the maximum specified by<paramref name="constraint"/> .</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            var iItem = DataContext as AssetItem;
            var iRes = new System.Windows.Size();
            if (iItem != null)
            {
                var iPanel = ControlFramework.Utility.TreeHelper.FindInTree<AssetPanel>(this); // Parent as AssetPanel;
                double iOffset = 0;
                if (iPanel != null)
                {
                    iOffset = Level * iPanel.LevelDepth;
                }

                var iSize = new System.Windows.Size(0, double.PositiveInfinity); // height can be determined by the item.
                foreach (System.Windows.Controls.ContentControl iContent in fVisuals)
                {
                    var iCol = (AssetColumn)iContent.Tag;
                    iSize.Width = iCol.Width - iOffset < 0 ? 0 : iCol.Width - iOffset;
                    iContent.Measure(iSize);
                    if (iRes.Height < iContent.DesiredSize.Height)
                    {
                        iRes.Height = iContent.DesiredSize.Height;
                    }

                    iRes.Width += iSize.Width;
                }
            }

            return iRes;
        }

        /// <summary>keeps all the previously generated UI elements, but replaces the data.</summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ReplaceItems(object value)
        {
            var iItem = value as AssetItem;
            if (iItem != null && fVisuals.Count == iItem.Data.Count + 1)
            {
                // 1 extra for the attribute
                var iTemplate = AttributeTemplate;
                var iNew = fVisuals[0] as System.Windows.Controls.ContentControl;

                    // the first visual is always the attribute
                iNew.Content = iItem;

                iTemplate = CellTemplate;
                for (var iCount = 0; iCount < iItem.Data.Count; iCount++)
                {
                    iNew = fVisuals[iCount + 1] as System.Windows.Controls.ContentControl;
                    iNew.Content = iItem.Data[iCount];
                }
            }
            else
            {
                throw new System.InvalidOperationException("Only asset items are supported, no groups");
            }
        }

        /// <summary>Builds all the UI items.</summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void BuildItems(AssetBase value)
        {
            var iItem = value as AssetItem;
            if (iItem != null)
            {
                var iCols = iItem.Root.Columns;
                var iTemplate = AttributeTemplate;
                var iNew = new System.Windows.Controls.ContentControl();
                iNew.Content = iItem;
                iNew.ContentTemplate = iTemplate;
                iNew.Tag = iCols[0]; // so we can easily find the associating col def.
                iNew.IsKeyboardFocusWithinChanged += Cell_IsKeyboardFocusWithinChanged;
                fVisuals.Add(iNew);

                iTemplate = CellTemplate;
                var iCount = 1;
                foreach (var i in iItem.Data)
                {
                    iNew = new System.Windows.Controls.ContentControl();
                    iNew.Content = i;
                    iNew.ContentTemplate = iTemplate;
                    iNew.Tag = iCols[iCount++]; // so we can easily find the associating col def.
                    iNew.IsKeyboardFocusWithinChanged += Cell_IsKeyboardFocusWithinChanged;
                    fVisuals.Add(iNew);
                }
            }
            else
            {
                throw new System.InvalidOperationException("Only asset items are supported, no groups");
            }
        }

        /// <summary>The cell_ is keyboard focus within changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Cell_IsKeyboardFocusWithinChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // only try to do this if we get focus, when loosing focus, doens't really matter, but we should better keep previous settings.
                var iSender = sender as System.Windows.Controls.ContentControl;
                var iItem = iSender.DataContext as AssetItem;
                if (iItem != null)
                {
                    iItem.IsSelected = true; // make certain that the row is also selected.
                    var iEditor = iItem.Root;
                    if (iEditor != null)
                    {
                        iEditor.SelectedColumn = fVisuals.IndexOf(iSender);
                    }
                }
            }
        }

        /// <summary>
        ///     Removes any previously created UI items.
        /// </summary>
        /// <exception cref="System.NotImplementedException" />
        private void ClearItems()
        {
            fVisuals.Clear();
        }

        #region AttributeTemplate

        /// <summary>
        ///     <see cref="AttributeTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AttributeTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "AttributeTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(AssetPanelRow), 
                new System.Windows.FrameworkPropertyMetadata(null, OnAttributeTemplateChanged));

        /// <summary>
        ///     Gets or sets the <see cref="AttributeTemplate" /> property. This
        ///     dependency property indicates template to use for the atributeColumn.
        /// </summary>
        public System.Windows.DataTemplate AttributeTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(AttributeTemplateProperty);
            }

            set
            {
                SetValue(AttributeTemplateProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="AttributeTemplate"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnAttributeTemplateChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((AssetPanelRow)d).OnAttributeTemplateChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="AttributeTemplate"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnAttributeTemplateChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fVisuals.Count > 0)
            {
                ((System.Windows.Controls.ContentControl)fVisuals[0]).ContentTemplate =
                    (System.Windows.DataTemplate)e.NewValue;
            }
        }

        #endregion

        #region CellTemplate

        /// <summary>
        ///     <see cref="CellTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CellTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "CellTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(AssetPanelRow), 
                new System.Windows.FrameworkPropertyMetadata(null, OnCellTemplateChanged));

        /// <summary>
        ///     Gets or sets the <see cref="CellTemplate" /> property. This dependency
        ///     property indicates template that should be used by the cells other
        ///     than the attribute.
        /// </summary>
        public System.Windows.DataTemplate CellTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(CellTemplateProperty);
            }

            set
            {
                SetValue(CellTemplateProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="AttributeTemplate"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnCellTemplateChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((AssetPanelRow)d).OnCellTemplateChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="AttributeTemplate"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnCellTemplateChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            for (var i = 1; i < fVisuals.Count; i++)
            {
                ((System.Windows.Controls.ContentControl)fVisuals[i]).ContentTemplate =
                    (System.Windows.DataTemplate)e.NewValue;
            }
        }

        #endregion

        #region Level

        /// <summary>
        ///     <see cref="Level" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LevelProperty =
            System.Windows.DependencyProperty.Register(
                "Level", 
                typeof(int), 
                typeof(AssetPanelRow), 
                new System.Windows.FrameworkPropertyMetadata(
                    0, 
                    System.Windows.FrameworkPropertyMetadataOptions.AffectsMeasure, 
                    OnLevelChanged));

        /// <summary>
        ///     Gets or sets the <see cref="Level" /> property. This dependency
        ///     property indicates the level depth that the tree has been expanded.
        ///     This is used to adjest the width of the first columns.
        /// </summary>
        public int Level
        {
            get
            {
                return (int)GetValue(LevelProperty);
            }

            set
            {
                SetValue(LevelProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="Level"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnLevelChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((AssetPanelRow)d).OnLevelChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="Level"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnLevelChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion
    }
}