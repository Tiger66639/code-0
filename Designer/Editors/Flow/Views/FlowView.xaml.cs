// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FlowView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for FlowView.xaml
    /// </summary>
    public partial class FlowView : System.Windows.Controls.UserControl
    {
        /// <summary>
        ///     used to go up and down through items, stores the minimum width of all items, so that we have an offset from the
        ///     border.
        /// </summary>
        private const double StaticMinWidth = 20;

        /// <summary>
        ///     Keeps track of the UI element that was focused before we opened the popup, so we can move focus again to there.
        /// </summary>
        private System.Windows.UIElement fPopuptarget;

        /// <summary>Initializes a new instance of the <see cref="FlowView"/> class.</summary>
        public FlowView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the ContextMenuOpening event of the LstItems control.</summary>
        /// <remarks>When we open a context menu, we make certain that the underlying item is selected.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.ContextMenuEventArgs"/> instance containing the event data.</param>
        private void LstItems_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            var iEl =
                ControlFramework.Utility.TreeHelper.FindInTree<System.Windows.FrameworkElement>(
                    (System.Windows.DependencyObject)e.OriginalSource);
            if (iEl != null)
            {
                var iItem = iEl.DataContext as FlowItem;
                if (iItem != null)
                {
                    iItem.IsSelected = true; // automatically takes care of the shift and ctrl.
                }
            }
        }

        #region PopupIsOpen

        /// <summary>
        ///     PopupIsOpen Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PopupIsOpenProperty =
            System.Windows.DependencyProperty.Register(
                "PopupIsOpen", 
                typeof(bool), 
                typeof(FlowView), 
                new System.Windows.FrameworkPropertyMetadata(false, OnPopupIsOpenChanged));

        /// <summary>
        ///     Gets or sets the PopupIsOpen property.  This dependency property
        ///     indicates wether the popup is currently open or closed.
        /// </summary>
        public bool PopupIsOpen
        {
            get
            {
                return (bool)GetValue(PopupIsOpenProperty);
            }

            set
            {
                SetValue(PopupIsOpenProperty, value);
            }
        }

        /// <summary>Handles changes to the PopupIsOpen property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnPopupIsOpenChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowView)d).OnPopupIsOpenChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the PopupIsOpen property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPopupIsOpenChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var iFlow = (Flow)DataContext;
                var iPoint = new System.Windows.Point(0, 0);

                    // is struct, so init from start like this, can't set to null.
                System.Windows.FrameworkElement iLast;
                if (iFlow.SelectedItems.Count > 0)
                {
                    iLast = LstItems.GetSelectedUI();
                    if (iLast != null)
                    {
                        if (iFlow.PopupDoesInsert)
                        {
                            iPoint = iLast.TranslatePoint(new System.Windows.Point(0, iLast.ActualHeight), LstItems);
                        }
                        else
                        {
                            iPoint =
                                iLast.TranslatePoint(
                                    new System.Windows.Point(iLast.ActualWidth, iLast.ActualHeight), 
                                    LstItems);
                        }
                    }
                }
                else
                {
                    iLast = LstItems.GetLastUI();
                    if (iLast != null)
                    {
                        iPoint = iLast.TranslatePoint(
                            new System.Windows.Point(iLast.ActualWidth, iLast.ActualHeight), 
                            LstItems);
                    }
                }

                fPopuptarget = iLast;
                PopupSelectItem.HorizontalOffset = iPoint.X;
                PopupSelectItem.VerticalOffset = iPoint.Y;
            }
        }

        #endregion

        #region Popup

        /// <summary>Creates a new flow item when the popup was closed..</summary>
        /// <param name="neuron">The neuron.</param>
        private void CreateFlowItemFromPopup(Neuron neuron)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)DataContext;
                var iNew = EditorsHelper.CreateFlowItemFor(neuron);
                if (iFlow.PopupDoesInsert)
                {
                    EditorsHelper.InsertFlowItem(iNew, iFlow.SelectedItem);
                }
                else
                {
                    EditorsHelper.AddFlowItem(iNew, iFlow.SelectedItem, iFlow);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Handles the SelectionCanceled event of the LstDictItems control. Simply close the popup.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LstDictItems_SelectionCanceled(object sender, System.EventArgs e)
        {
            PopupIsOpen = false;
        }

        /// <summary>Handles the SelectionChanged event of the LstDictItems control. When the selection was changed,
        ///     need to create a new flow item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LstDictItems_SelectionChanged(object sender, System.EventArgs e)
        {
            PopupIsOpen = false;
            var iFlow = (Flow)DataContext;
            var iPrevSelected = iFlow.SelectedItem;
            var iSelected = LstDictItems.SelectedNeuron;
            if (iSelected != null)
            {
                CreateFlowItemFromPopup(iSelected);
            }

            if (fPopuptarget != null)
            {
                fPopuptarget.Focus();

                    // move back to the item that requested the operation, if we don't do this, it is difficult to continue editing with the keyboard.
            }
            else
            {
                LstItems.Focus();
            }
        }

        /// <summary>Handles the Opened event of the PopupSelectItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void PopupSelectItem_Opened(object sender, System.EventArgs e)
        {
            LstDictItems.AttachItemsSources();
        }

        /// <summary>Handles the Closed event of the PopupSelectItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void PopupSelectItem_Closed(object sender, System.EventArgs e)
        {
            LstDictItems.DetachItemsSources();
        }

        #endregion

        #region NeedsFocus

        /// <summary>
        ///     NeedsFocus Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty NeedsFocusProperty =
            System.Windows.DependencyProperty.Register(
                "NeedsFocus", 
                typeof(bool), 
                typeof(FlowView), 
                new System.Windows.FrameworkPropertyMetadata(false, OnNeedsFocusChanged));

        /// <summary>
        ///     Gets or sets the NeedsFocus property.  This dependency property
        ///     indicates wether this object needs to get focus or not.
        /// </summary>
        /// <remarks>
        ///     This is a fix for wpf's shortcommings in the area of focusing to the correct object.
        ///     this is simply the fastest way to link it up.
        /// </remarks>
        public bool NeedsFocus
        {
            get
            {
                return (bool)GetValue(NeedsFocusProperty);
            }

            set
            {
                SetValue(NeedsFocusProperty, value);
            }
        }

        /// <summary>Handles changes to the NeedsFocus property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnNeedsFocusChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowView)d).OnNeedsFocusChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the NeedsFocus property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnNeedsFocusChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && IsKeyboardFocusWithin == false)
            {
                LstItems.Focus();
            }
        }

        /// <summary>The on is keyboard focus within changed.</summary>
        /// <param name="e">The e.</param>
        protected override void OnIsKeyboardFocusWithinChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            NeedsFocus = false;
            base.OnIsKeyboardFocusWithinChanged(e);
        }

        #endregion

        #region Deletespecial

        /// <summary>The delete special_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSpecial_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            e.CanExecute = iFlow != null && iFlow.CanDeleteSpecial();
        }

        /// <summary>The delete special_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DeleteSpecial_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            iFlow.DeleteSpecial();
        }

        #endregion

        #region Delete

        /// <summary>The delete_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            e.CanExecute = iFlow != null && iFlow.CanDelete();
        }

        /// <summary>The delete_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.Delete();
        }

        #endregion

        #region Paste

        /// <summary>The paste_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Paste_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            e.CanExecute = iFlow != null && iFlow.CanPasteFromClipboard();
        }

        /// <summary>The paste_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Paste_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.PasteFromClipboard();
        }

        #endregion

        #region Copy

        /// <summary>The copy_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Copy_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            e.CanExecute = iFlow != null && iFlow.CanCopyToClipboard();
        }

        /// <summary>The copy_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Copy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.CopyToClipboard();
        }

        #endregion

        #region Cut

        /// <summary>The cut_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Cut_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            e.CanExecute = iFlow != null && iFlow.CanCutToClipboard();
        }

        /// <summary>The cut_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Cut_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)DataContext;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.CutToClipboard();
        }

        #endregion

        #region Rename

        /// <summary>The rename_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iItem = e.Parameter as FlowItem;
            var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
            e.CanExecute = iItem != null || (iFocused != null && iFocused.DataContext is FlowItem);
        }

        /// <summary>The rename_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Rename_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItem = e.Parameter as FlowItem;
            if (iItem == null)
            {
                var iFocused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.FrameworkElement;
                if (iFocused != null && iFocused.DataContext is FlowItem)
                {
                    iItem = (FlowItem)iFocused.DataContext;
                }
            }

            if (iItem != null)
            {
                EditorsHelper.RenameItem(iItem.Item, "Change name of flow item");
            }
        }

        #endregion
    }
}