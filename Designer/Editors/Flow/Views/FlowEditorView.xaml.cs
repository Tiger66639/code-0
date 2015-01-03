// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowEditorView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FlowEditorView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for FlowEditorView.xaml
    /// </summary>
    public partial class FlowEditorView : CtrlEditorBase
    {
        /// <summary>The f searcher.</summary>
        private FlowEditorSearcher fSearcher;

        /// <summary>Initializes a new instance of the <see cref="FlowEditorView"/> class.</summary>
        public FlowEditorView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the SelectionChanged event of the CmbStaticItemDisplayMode
        ///     control.</summary>
        /// <remarks>We invalidate the intire editor so that all the items will be redrawn
        ///     with the new correct value.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CmbStaticItemDisplayMode_SelectionChanged(
            object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            if (iEditor != null)
            {
                iEditor.ReDrawFlows();
            }
        }

        /// <summary>Handles the Loaded event of the LstFrames control. We need to make
        ///     certain that the selected items is visible. If it isn't, we don't
        ///     display the flow data itself.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LstItems_Loaded(object sender, System.Windows.RoutedEventArgs args)
        {
            var iEditor = (FlowEditor)DataContext;
            if (iEditor != null)
            {
                object iSelected = iEditor.SelectedFlow;
                if (iSelected != null)
                {
                    LstItems.ScrollIntoView(iSelected);
                }
            }
        }

        /// <summary>Handles the SelectionChanged event of the LstItems control. We need to
        ///     do this manually, can't use the link to update the selected item,
        ///     since this will set it to <see langword="null"/> when unloaded. This
        ///     is not desired.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void LstItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs args)
        {
            var iEditor = (FlowEditor)DataContext;
            if (iEditor != null)
            {
                if (args.AddedItems.Count > 0)
                {
                    iEditor.SelectedFlow = args.AddedItems[0] as Flow;
                }

                // else if (((FrameworkElement)args.OriginalSource).IsLoaded == true)
                // iEditor.SelectedFlow = null;
            }
        }

        #region Prop

        #region IsEditing

        /// <summary>
        ///     <see cref="IsEditing" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsEditingProperty =
            System.Windows.DependencyProperty.Register(
                "IsEditing", 
                typeof(bool), 
                typeof(FlowEditorView), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsEditingChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IsEditing" /> property. This dependency
        ///     property indicates wether the currently selected item in the list is
        ///     in edit mode or not.
        /// </summary>
        public bool IsEditing
        {
            get
            {
                return (bool)GetValue(IsEditingProperty);
            }

            set
            {
                SetValue(IsEditingProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="IsEditing"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsEditingChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowEditorView)d).OnIsEditingChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="IsEditing"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIsEditingChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // if ((bool)e.NewValue == false)
            // {
            // if (fSelected != null)
            // Keyboard.Focus(fSelected);                                               //need to focus to the container, and not the list so that nav keys keep working.
            // else
            // LstItems.Focus();
            // }
        }

        #endregion

        #region OverlayVisibility

        /// <summary>
        ///     <see cref="OverlayVisibility" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty OverlayVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "OverlayVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(FlowEditorView), 
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Visibility.Collapsed, 
                    OnOverlayVisibilityChanged));

        /// <summary>
        ///     Gets or sets the <see cref="OverlayVisibility" /> property. This
        ///     dependency property indicates if the overlays should be displayed or
        ///     not.
        /// </summary>
        public System.Windows.Visibility OverlayVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(OverlayVisibilityProperty);
            }

            set
            {
                SetValue(OverlayVisibilityProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="OverlayVisibility"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnOverlayVisibilityChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FlowEditorView)d).OnOverlayVisibilityChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="OverlayVisibility"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnOverlayVisibilityChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iEditor = DataContext as FlowEditor;
            if (iEditor != null)
            {
                iEditor.OverlayVisibility = (System.Windows.Visibility)e.NewValue;
            }
        }

        #endregion

        #endregion

        #region Readonly textboxes

        /// <summary>Handles the Click event of the EditTextBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void EditTextBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsEditing = true;
        }

        /// <summary>Handles the LostFocus event of the TxtTitle control.</summary>
        /// <remarks>When looses focus, need to make certain that is editing is turned off.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_LostKeybFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            IsEditing = false;
        }

        /// <summary>Handles the LostFocus event of the TxtTitle control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            IsEditing = false;
        }

        /// <summary>Handles the PrvKeyDown event of the TxtTitle control.</summary>
        /// <remarks>when enter is pressed, need to stop editing.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TxtTitle_PrvKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                IsEditing = false;
            }
        }

        #endregion

        #region Commands

        #region Rename

        /// <summary>Handles the Executed event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            IsEditing = true;
        }

        /// <summary>Handles the CanExecute event of the Rename control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Rename_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstItems != null && LstItems.SelectedItem != null;
        }

        #endregion

        #region CanExecute

        /// <summary>Handles the CanExecute event of the InsertFlowItem Command.</summary>
        /// <remarks>We can add a flow item when the editor has a flow assigned.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertFlowItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstItems != null)
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null)
                {
                    e.CanExecute = iFlow.SelectedItem != null;
                    return;
                }
            }

            e.CanExecute = false;
        }

        /// <summary>The add flow item_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddFlowItem_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            e.CanExecute = iEditor != null && iEditor.SelectedFlow != null;
        }

        #endregion

        #region Option

        /// <summary>Handles the Executed event of the InsertOption Command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertOption_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iNew = EditorsHelper.MakeFlowOption();
                EditorsHelper.InsertFlowItem(iNew, iFlow.SelectedItem);
                iFlow.SelectedItems.Clear();
                iNew.IsSelected = true;

                    // we do this, cause the insert is done with ctrl pressed, which does an 'add selected', so the previous is not removed from the list, which we don't want.
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The add option_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddOption_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iNew = EditorsHelper.MakeFlowOption();
                EditorsHelper.AddFlowItem(iNew, iFlow.SelectedItem, iFlow);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region Loop

        /// <summary>Handles the Executed event of the InsertLoop Command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertLoop_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iNew = EditorsHelper.MakeFlowLoop();
                EditorsHelper.InsertFlowItem(iNew, iFlow.SelectedItem);
                iFlow.SelectedItems.Clear();
                iNew.IsSelected = true;

                    // we do this, cause the insert is done with ctrl pressed, which does an 'add selected', so the previous is not removed from the list, which we don't want.
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The add loop_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddLoop_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iNew = EditorsHelper.MakeFlowLoop();
                EditorsHelper.AddFlowItem(iNew, iFlow.SelectedItem, iFlow);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region static

        /// <summary>Handles the Executed event of the InsertStatic Command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertStatic_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)LstItems.SelectedItem;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.PopupDoesInsert = true;
            iFlow.PopupIsOpen = true;
        }

        /// <summary>The add static_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddStatic_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iFlow = (Flow)LstItems.SelectedItem;
            System.Diagnostics.Debug.Assert(iFlow != null);
            iFlow.PopupDoesInsert = false;
            iFlow.PopupIsOpen = true;
        }

        #endregion

        #region New object

        /// <summary>The add new object_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddNewObject_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iCluster = EditorsHelper.CreateNewObject(false, null);
                if (iCluster != null)
                {
                    var iStatic = new FlowItemStatic(iCluster);
                    EditorsHelper.AddFlowItem(iStatic, iFlow.SelectedItem, iFlow);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The insert new object_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InsertNewObject_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iCluster = EditorsHelper.CreateNewObject(false, null);
                if (iCluster != null)
                {
                    var iStatic = new FlowItemStatic(iCluster);
                    EditorsHelper.InsertFlowItem(iStatic, iFlow.SelectedItem);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region New neuron

        /// <summary>The insert new neuron_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InsertNewNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "Neuron name:";
            iIn.Answer = "New neuron";
            iIn.Title = "New neuron";
            if ((bool)iIn.ShowDialog())
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will trigger multiple undo items
                try
                {
                    var iFlow = (Flow)LstItems.SelectedItem;
                    var iNew = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNew);
                    var iStatic = new FlowItemStatic(iNew);
                    iStatic.NeuronInfo.DisplayTitle = iIn.Answer;
                    EditorsHelper.InsertFlowItem(iStatic, iFlow.SelectedItem);
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The add new neuron_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddNewNeuron_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "Neuron name:";
            iIn.Answer = "New neuron";
            iIn.Title = "New neuron";
            if ((bool)iIn.ShowDialog())
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will trigger multiple undo items
                try
                {
                    var iFlow = (Flow)LstItems.SelectedItem;
                    var iNew = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNew);
                    var iStatic = new FlowItemStatic(iNew);
                    iStatic.NeuronInfo.DisplayTitle = iIn.Answer;
                    EditorsHelper.AddFlowItem(iStatic, iFlow.SelectedItem, iFlow);
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        #endregion

        #region Conditional part

        /// <summary>Handles the CanExecute event of the InsertCondPart control.</summary>
        /// <remarks>Almost similar as for the other inserts, except when a listbox item is
        ///     focused, in that case we need to check if the flow item is contained
        ///     in a Flow-item-Conditional.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void InsertCondPart_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstItems != null)
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null && iFlow.SelectedItem != null)
                {
                    e.CanExecute = iFlow.SelectedItem is FlowItemBlock | iFlow.SelectedItem.Owner is FlowItemBlock;
                    return;
                }
            }

            e.CanExecute = false;
        }

        /// <summary>The insert cond part_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void InsertCondPart_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iSelected = iFlow.SelectedItem;
                iFlow.SelectedItems.Clear();

                    // we do this manually for the | cause this is inserted/added using 'shift + \' -> this means that the shift is presssed & SetIsSelected interprets this as if we want to add to the selection, which we don't want.
                var iNew = EditorsHelper.MakeFlowCondPart();
                EditorsHelper.InsertFlowItem(iNew, iSelected);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The add cond part_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddCondPart_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = (Flow)LstItems.SelectedItem;
                var iSelected = iFlow.SelectedItem;
                iFlow.SelectedItems.Clear();

                    // we do this manually for the | cause this is inserted/added using 'shift + \' -> this means that the shift is presssed & SetIsSelected interprets this as if we want to add to the selection, which we don't want.
                var iNew = EditorsHelper.MakeFlowCondPart();
                EditorsHelper.AddFlowItem(iNew, iSelected, iFlow);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region Change option/loop

        /// <summary>The change option to loop_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChangeOptionToLoop_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null && iFlow.SelectedItem != null)
                {
                    FlowItemConditional iCond;
                    if (iFlow.SelectedItem is FlowItemConditional)
                    {
                        iCond = (FlowItemConditional)iFlow.SelectedItem;
                    }
                    else
                    {
                        iCond = iFlow.SelectedItem.FindFirstOwner<FlowItemConditional>();
                    }

                    if (iCond != null)
                    {
                        iCond.IsLooped = true;
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The change loop to option_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChangeLoopToOption_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null && iFlow.SelectedItem != null)
                {
                    FlowItemConditional iCond;
                    if (iFlow.SelectedItem is FlowItemConditional)
                    {
                        iCond = (FlowItemConditional)iFlow.SelectedItem;
                    }
                    else
                    {
                        iCond = iFlow.SelectedItem.FindFirstOwner<FlowItemConditional>();
                    }

                    if (iCond != null)
                    {
                        iCond.IsLooped = false;
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region ToggleFlowLoopSelectionRequirement

        /// <summary>The toggle flow loop selection requirement_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ToggleFlowLoopSelectionRequirement_CanExecute(
            object sender, 
            System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (LstItems != null)
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null && iFlow.SelectedItem != null)
                {
                    FlowItemConditional iCond;
                    if (iFlow.SelectedItem is FlowItemConditional)
                    {
                        iCond = (FlowItemConditional)iFlow.SelectedItem;
                    }
                    else
                    {
                        iCond = iFlow.SelectedItem.FindFirstOwner<FlowItemConditional>();
                    }

                    e.CanExecute = iCond != null;
                    if (e.CanExecute)
                    {
                        BtnToggleSelection.IsChecked = iCond.IsSelectionRequired;
                    }
                    else
                    {
                        BtnToggleSelection.IsChecked = false;
                    }

                    return;
                }
            }

            e.CanExecute = false;
            BtnToggleSelection.IsChecked = false;
        }

        /// <summary>Handles the Executed event of the ToggleFlowLoopSelectionRequirement
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ToggleFlowLoopSelectionRequirement_Executed(
            object sender, 
            System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iFlow = LstItems.SelectedItem as Flow;
                if (iFlow != null && iFlow.SelectedItem != null)
                {
                    FlowItemConditional iCond;
                    if (iFlow.SelectedItem is FlowItemConditional)
                    {
                        iCond = (FlowItemConditional)iFlow.SelectedItem;
                    }
                    else
                    {
                        iCond = iFlow.SelectedItem.FindFirstOwner<FlowItemConditional>();
                    }

                    if (iCond != null)
                    {
                        iCond.IsSelectionRequired = !iCond.IsSelectionRequired;
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        /// <summary>Handles the Executed event of the AddFlow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddFlow_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);

                // we begin a group because this action will trigger multiple undo items
            try
            {
                var iEditor = (FlowEditor)DataContext;
                var iFlow = EditorsHelper.MakeFlow();
                if (iFlow != null)
                {
                    iEditor.Flows.Add(iFlow);
                    iFlow.IsOpen = true;
                    iEditor.SelectedFlow = iFlow;
                    iFlow.IsFocused = true;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #region NextPage

        /// <summary>The next page_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NextPage_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            if (iEditor.SelectedFlow != null)
            {
                var iIndex = iEditor.Flows.IndexOf(iEditor.SelectedFlow);
                e.CanExecute = iIndex < iEditor.Flows.Count - 1;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The next page_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NextPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            var iIndex = iEditor.Flows.IndexOf(iEditor.SelectedFlow);
            iEditor.SelectedFlow = iEditor.Flows[iIndex + 1];
        }

        #endregion

        #region PrevPage

        /// <summary>The prev page_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PrevPage_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            if (iEditor.SelectedFlow != null)
            {
                var iIndex = iEditor.Flows.IndexOf(iEditor.SelectedFlow);
                e.CanExecute = iIndex > 0;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The prev page_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void PrevPage_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            var iIndex = iEditor.Flows.IndexOf(iEditor.SelectedFlow);
            iEditor.SelectedFlow = iEditor.Flows[iIndex - 1];
        }

        #endregion

        #region BrowseBack

        /// <summary>The browse back_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BrowseBack_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            e.CanExecute = iEditor != null && iEditor.Flows.SelectionHistory.CanNavigateBackward;
        }

        /// <summary>The browse back_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BrowseBack_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.SetSelectedFlowFromNavigation(iEditor.Flows.SelectionHistory.NavigateBackward());
        }

        #endregion

        #region BrowseForward

        /// <summary>Handles the CanExecute event of the BrowseForward control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void BrowseForward_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            e.CanExecute = iEditor != null && iEditor.Flows.SelectionHistory.CanNavigateForward;
        }

        /// <summary>Handles the Executed event of the BrowseForward control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void BrowseForward_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (FlowEditor)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.SelectedFlow = iEditor.Flows.SelectionHistory.NavigateForward();
        }

        #endregion

        #endregion

        #region Chec/uncheck OverlayVisibility

        /// <summary>Handles the Checked event of the BtnOverlayVisibility control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnOverlayVisibility_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            OverlayVisibility = System.Windows.Visibility.Visible;
        }

        /// <summary>Handles the UnChecked event of the BtnOverlayVisibility control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnOverlayVisibility_UnChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            OverlayVisibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>The editor_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Editor_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iEditor = DataContext as FlowEditor;
            if (iEditor != null)
            {
                OverlayVisibility = iEditor.OverlayVisibility;
                if (iEditor.OverlayVisibility == System.Windows.Visibility.Visible)
                {
                    BtnOverlayVisibility.IsChecked = true;
                }
            }
        }

        #endregion

        #region Search

        /// <summary>Handles the Executed event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (fSearcher == null)
            {
                var iEditor = (FlowEditor)DataContext;
                fSearcher = new FlowEditorSearcher(iEditor);
                fSearcher.Start(TxtToSearch.Text.ToLower());
            }
            else
            {
                fSearcher.Continue();
            }

            // Flow iFlow = iEditor.SelectedFlow;
            // int iIndex = 0;
            // int iStartIndex = 0;
            // if (iFlow != null)
            // iStartIndex = iIndex = iEditor.Flows.IndexOf(iFlow);
            // else if (iFlow == null && iEditor.Flows.Count > 0)
            // iFlow = iEditor.Flows[0];
            // FlowItem iSelected = null;
            // if (iFlow != null)
            // {
            // iSelected = iFlow.SelectedItem ?? iFlow.Items[0];
            // FlowItem iStart = null;
            // while (iFlow != null)
            // {
            // if (iFlow.Items.Count > 0)                                                                      //only try to search if there is something to search.
            // {
            // if (iSelected == null)
            // iSelected = iFlow.Items[0];
            // FlowItem iNext = iFlow.GetNext(iToSearch, iSelected);
            // if (iStart == null)                                                                       //the first time we get here, start is not yet found, so we set it here, this way, the next time we get to the first found item, we know we have found all items and need to quite.
            // iStart = iNext;
            // else if (iNext == iStart)
            // {
            // MessageBox.Show("End of search reached!");
            // return;
            // }
            // if (iNext != null)
            // {
            // if (fFirstFound == null)
            // fFirstFound = iNext;
            // else if (fFirstFound == iNext)
            // {
            // MessageBox.Show("End of search reached!");
            // fFirstFound = null;
            // return;
            // }
            // iFlow.IsSelected = true;
            // iNext.IsSelected = true;
            // return;
            // }
            // }
            // iIndex++;
            // if (iIndex < iEditor.Flows.Count)
            // iFlow = iEditor.Flows[iIndex];
            // else
            // {
            // iFlow = iEditor.Flows[0];
            // iIndex = 0;
            // }
            // iSelected = null;
            // if (iIndex == iStartIndex)
            // {
            // MessageBox.Show("End of search reached!");
            // break;
            // }
            // }
            // }
            // MessageBox.Show("No items found!");                                                                //when we get here, nothing was found.
        }

        /// <summary>Handles the CanExecute event of the FindNextCmd control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void FindNextCmd_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.IsNullOrEmpty(TxtToSearch.Text) == false;
        }

        /// <summary>The txt to search_ key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtToSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                FindNextCmd_Executed(sender, null);
                e.Handled = true;
            }
            else if (e.Key != System.Windows.Input.Key.F3)
            {
                // when the textbox is focused and we press f3 to go to the next item, we don't want to reset the search.
                fSearcher = null;
            }
        }

        #endregion
    }
}