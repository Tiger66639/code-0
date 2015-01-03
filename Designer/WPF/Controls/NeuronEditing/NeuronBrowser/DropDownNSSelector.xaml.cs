// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropDownNSSelector.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DropDownNSSelector.xaml. Used to display a treeview in a combobox for selecting a
//   NeuronData object from a list (usually the namespaces). Only neuronData can be selected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for DropDownNSSelector.xaml. Used to display a treeview in a combobox for selecting a
    ///     NeuronData object from a list (usually the namespaces). Only neuronData can be selected.
    /// </summary>
    public partial class DropDownNSSelector : System.Windows.Controls.UserControl
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DropDownNSSelector" /> class.
        /// </summary>
        public DropDownNSSelector()
        {
            ItemsSources = new BrowserDataSourceList(this);
            InitializeComponent();
        }

        #endregion

        /// <summary>Handles the IsKeyboardFocusWithinChanged event of the ThePopup control.</summary>
        /// <remarks>If we don't do this, the popup remains open when the user clicks on something else without selecting something or
        ///     explicitly
        ///     canceling the operation.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void ThePopup_IsKeyboardFocusWithinChanged(
            object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ContextIdle, 
                    new System.Action(CheckFocus)); // do async, cause when we change tab, it wants to close sometimes.
            }
        }

        /// <summary>The check focus.</summary>
        private void CheckFocus()
        {
            if (IsKeyboardFocusWithin == false)
            {
                IsDropDownOpen = false;
            }
        }

        /// <summary>The btn reset_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnReset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedNeuron = null;
        }

        /// <summary>Handles the KeyDown event of the This control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void This_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                IsDropDownOpen = !IsDropDownOpen;
            }
        }

        /// <summary>
        ///     Has to be inline from a component cause it needs access to RemoveLogicalChild and AddLogicalChild.
        ///     Makes certain that all the children are added/removed to the logical tree of the control.
        ///     This has to be an inline class, cause otherwise we can't call the
        ///     <see cref="System.Windows.FrameworkElement.AddLogicalChild" />
        ///     and remove counterpart.
        /// </summary>
        public class BrowserDataSourceList : System.Collections.ObjectModel.ObservableCollection<BrowserDataSource>
        {
            /// <summary>The f owner.</summary>
            private readonly DropDownNSSelector fOwner;

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class.</summary>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(DropDownNSSelector owner)
            {
                fOwner = owner;
            }

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class.</summary>
            /// <param name="source">The source.</param>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(System.Collections.Generic.IEnumerable<BrowserDataSource> source, 
                DropDownNSSelector owner)
                : base(source)
            {
                fOwner = owner;
            }

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class.</summary>
            /// <param name="source">The source.</param>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(System.Collections.Generic.List<BrowserDataSource> source, 
                DropDownNSSelector owner)
                : base(source)
            {
                fOwner = owner;
            }

            /// <summary>
            ///     Clears the items.
            /// </summary>
            protected override void ClearItems()
            {
                foreach (var i in this)
                {
                    fOwner.RemoveLogicalChild(i);
                }

                base.ClearItems();
            }

            /// <summary>Inserts the item.</summary>
            /// <param name="index">The index.</param>
            /// <param name="item">The item.</param>
            protected override void InsertItem(int index, BrowserDataSource item)
            {
                fOwner.AddLogicalChild(item);
                base.InsertItem(index, item);
            }

            /// <summary>Removes the item.</summary>
            /// <param name="index">The index.</param>
            protected override void RemoveItem(int index)
            {
                fOwner.RemoveLogicalChild(this[index]);
                base.RemoveItem(index);
            }

            /// <summary>Sets the item.</summary>
            /// <param name="index">The index.</param>
            /// <param name="item">The item.</param>
            protected override void SetItem(int index, BrowserDataSource item)
            {
                fOwner.RemoveLogicalChild(this[index]);
                fOwner.AddLogicalChild(item);
                base.SetItem(index, item);
            }
        }

        #region Prop

        #region IsDropDownOpen

        /// <summary>
        ///     IsDropDownOpen Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IsDropDownOpenProperty =
            System.Windows.DependencyProperty.Register(
                "IsDropDownOpen", 
                typeof(bool), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata(false, OnIsDropDownChanged));

        /// <summary>
        ///     Gets or sets the IsDropDownOpen property.  This dependency property
        ///     indicates wether the dropdown part is open or not.
        /// </summary>
        public bool IsDropDownOpen
        {
            get
            {
                return (bool)GetValue(IsDropDownOpenProperty);
            }

            set
            {
                SetValue(IsDropDownOpenProperty, value);
            }
        }

        /// <summary>The on is drop down changed.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIsDropDownChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DropDownNSSelector)d).OnIsDropDownChanged(e);
        }

        /// <summary>The on is drop down changed.</summary>
        /// <param name="e">The e.</param>
        private void OnIsDropDownChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (ShowDropDownOnMouseOver && IsMouseOver == false)
                {
                    BtnToggle.Visibility = System.Windows.Visibility.Collapsed;
                    System.Windows.Controls.Grid.SetColumnSpan(ThePresenter, 2);
                }
            }
        }

        #endregion

        #region SelectedNeuronChanged

        /// <summary>
        ///     SelectedNeuronChanged Routed Event
        /// </summary>
        public static readonly System.Windows.RoutedEvent SelectedNeuronChangedEvent =
            System.Windows.EventManager.RegisterRoutedEvent(
                "SelectedNeuronChanged", 
                System.Windows.RoutingStrategy.Direct, 
                typeof(System.Windows.RoutedPropertyChangedEventHandler<Neuron>), 
                typeof(DropDownNSSelector));

        /// <summary>
        ///     Occurs when the selected neuron has changed.
        /// </summary>
        public event System.Windows.RoutedPropertyChangedEventHandler<Neuron> SelectedNeuronChanged
        {
            add
            {
                AddHandler(SelectedNeuronChangedEvent, value);
            }

            remove
            {
                RemoveHandler(SelectedNeuronChangedEvent, value);
            }
        }

        /// <summary>A helper method to raise the SelectedNeuronChanged event.</summary>
        /// <param name="oldVal">the previous value</param>
        /// <param name="newVal">The new value</param>
        /// <returns>The <see cref="RoutedPropertyChangedEventArgs"/>.</returns>
        protected System.Windows.RoutedPropertyChangedEventArgs<Neuron> RaiseSelectedNeuronChangedEvent(
            Neuron oldVal, 
            Neuron newVal)
        {
            var args = new System.Windows.RoutedPropertyChangedEventArgs<Neuron>(oldVal, newVal);
            args.RoutedEvent = SelectedNeuronChangedEvent;
            RaiseEvent(this, args);
            return args;
        }

        #endregion

        #region RoutedEvent Helper Methods

        /// <summary>A static helper method to raise a routed event on a target UIElement or ContentElement.</summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        /// <param name="args">RoutedEventArgs to use when raising the event</param>
        private static void RaiseEvent(System.Windows.DependencyObject target, System.Windows.RoutedEventArgs args)
        {
            if (target is System.Windows.UIElement)
            {
                (target as System.Windows.UIElement).RaiseEvent(args);
            }
            else if (target is System.Windows.ContentElement)
            {
                (target as System.Windows.ContentElement).RaiseEvent(args);
            }
        }

        /// <summary>A static helper method that adds a handler for a routed event
        ///     to a target UIElement or ContentElement.</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will be handled</param>
        /// <param name="handler">Event handler to be added</param>
        private static void AddHandler(
            System.Windows.DependencyObject element, 
            System.Windows.RoutedEvent routedEvent, 
            System.Delegate handler)
        {
            var uie = element as System.Windows.UIElement;
            if (uie != null)
            {
                uie.AddHandler(routedEvent, handler);
            }
            else
            {
                var ce = element as System.Windows.ContentElement;
                if (ce != null)
                {
                    ce.AddHandler(routedEvent, handler);
                }
            }
        }

        /// <summary>A static helper method that removes a handler for a routed event
        ///     from a target UIElement or ContentElement.</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will no longer be handled</param>
        /// <param name="handler">Event handler to be removed</param>
        private static void RemoveHandler(
            System.Windows.DependencyObject element, 
            System.Windows.RoutedEvent routedEvent, 
            System.Delegate handler)
        {
            var uie = element as System.Windows.UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(routedEvent, handler);
            }
            else
            {
                var ce = element as System.Windows.ContentElement;
                if (ce != null)
                {
                    ce.RemoveHandler(routedEvent, handler);
                }
            }
        }

        #endregion

        #region ContentFocusable

        /// <summary>
        ///     ContentFocusable Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ContentFocusableProperty =
            System.Windows.DependencyProperty.Register(
                "ContentFocusable", 
                typeof(bool), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets the ContentFocusable property.  This dependency property
        ///     indicates if the content (the selected item) should be focusable or not. True by default.
        /// </summary>
        public bool ContentFocusable
        {
            get
            {
                return (bool)GetValue(ContentFocusableProperty);
            }

            set
            {
                SetValue(ContentFocusableProperty, value);
            }
        }

        #endregion

        #region SelectedNeuron

        /// <summary>
        ///     SelectedNeuron Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedNeuronProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedNeuron", 
                typeof(Neuron), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata(null, OnSelectedNeuronChanged));

        /// <summary>
        ///     Gets or sets the SelectedNeuron property.  This dependency property
        ///     indicates which neuron is selected.
        /// </summary>
        public Neuron SelectedNeuron
        {
            get
            {
                return (Neuron)GetValue(SelectedNeuronProperty);
            }

            set
            {
                SetValue(SelectedNeuronProperty, value);
            }
        }

        /// <summary>Handles changes to the SelectedNeuron property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnSelectedNeuronChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DropDownNSSelector)d).OnSelectedNeuronChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the SelectedNeuron property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnSelectedNeuronChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iNew = e.NewValue as Neuron;
            if (iNew != null)
            {
                SetSelectionBoxItem(BrainData.Current.NeuronInfo[iNew.ID]);
                if (CanClearValue)
                {
                    BtnReset.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                SetSelectionBoxItem(null);
                BtnReset.Visibility = System.Windows.Visibility.Collapsed;
            }

            ItemBrowser.SelectedNeuron = iNew; // need to make certain that the ItemBrowser is also updated.
            RaiseSelectedNeuronChangedEvent((Neuron)e.OldValue, (Neuron)e.NewValue);
        }

        #endregion

        #region SelectionBoxItem

        /// <summary>
        ///     SelectionBoxItem Read-Only Dependency Property
        /// </summary>
        private static readonly System.Windows.DependencyPropertyKey SelectionBoxItemPropertyKey =
            System.Windows.DependencyProperty.RegisterReadOnly(
                "SelectionBoxItem", 
                typeof(object), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata((object)null));

        /// <summary>The selection box item property.</summary>
        public static readonly System.Windows.DependencyProperty SelectionBoxItemProperty =
            SelectionBoxItemPropertyKey.DependencyProperty;

        /// <summary>
        ///     Gets or sets the SelectionBoxItem property.  This dependency property
        ///     indicates which value to display in the content presenter. This is used to display the currently selected value in
        ///     the
        ///     xaml of the object, don't bind to it.
        /// </summary>
        public object SelectionBoxItem
        {
            get
            {
                return GetValue(SelectionBoxItemProperty);
            }
        }

        /// <summary>Provides a secure method for setting the SelectionBoxItem property.
        ///     This dependency property indicates ....</summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetSelectionBoxItem(object value)
        {
            SetValue(SelectionBoxItemPropertyKey, value);
        }

        #endregion

        #region ItemsSources

        /// <summary>
        ///     Gets the list of itemsSources to use for the drop down box.
        /// </summary>
        public BrowserDataSourceList ItemsSources { get; private set; }

        #endregion

        #region ShowDropDownOnMouseOver

        /// <summary>
        ///     ShowDropDownOnMouseOver Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ShowDropDownOnMouseOverProperty =
            System.Windows.DependencyProperty.Register(
                "ShowDropDownOnMouseOver", 
                typeof(bool), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata(false, OnShowDropDownOnMouseOverChanged));

        /// <summary>
        ///     Gets or sets the ShowDropDownOnMouseOver property.  This dependency property
        ///     indicates if the drop down button should only be displayed when the mouse is over
        ///     it (value =true) or all the time (value = falue = default).
        /// </summary>
        public bool ShowDropDownOnMouseOver
        {
            get
            {
                return (bool)GetValue(ShowDropDownOnMouseOverProperty);
            }

            set
            {
                SetValue(ShowDropDownOnMouseOverProperty, value);
            }
        }

        /// <summary>Handles changes to the ShowDropDownOnMouseOver property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnShowDropDownOnMouseOverChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DropDownNSSelector)d).OnShowDropDownOnMouseOverChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ShowDropDownOnMouseOver property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnShowDropDownOnMouseOverChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                BtnToggle.Visibility = System.Windows.Visibility.Collapsed;
                System.Windows.Controls.Grid.SetColumnSpan(ThePresenter, 2);
            }
            else
            {
                BtnToggle.Visibility = System.Windows.Visibility.Visible;
                System.Windows.Controls.Grid.SetColumnSpan(ThePresenter, 1);
            }
        }

        #endregion

        #region CanClearValue

        /// <summary>
        ///     CanClearValue Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CanClearValueProperty =
            System.Windows.DependencyProperty.Register(
                "CanClearValue", 
                typeof(bool), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata(true, OnCanClearValueChanged));

        /// <summary>
        ///     Gets or sets the CanClearValue property.  This dependency property
        ///     indicates wether the user can clear the value again or not. When true, an extra button is displayed.
        /// </summary>
        public bool CanClearValue
        {
            get
            {
                return (bool)GetValue(CanClearValueProperty);
            }

            set
            {
                SetValue(CanClearValueProperty, value);
            }
        }

        /// <summary>Handles changes to the CanClearValue property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnCanClearValueChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((DropDownNSSelector)d).OnCanClearValueChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the CanClearValue property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnCanClearValueChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && SelectedNeuron != null)
            {
                BtnReset.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                BtnReset.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        #endregion

        #region SelectedNeuronTemplate

        /// <summary>
        ///     SelectedNeuronTemplate Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedNeuronTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedNeuronTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(DropDownNSSelector), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the SelectedNeuronTemplate property.  This dependency property
        ///     indicates the template to use for the selected neuron.
        /// </summary>
        public System.Windows.DataTemplate SelectedNeuronTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(SelectedNeuronTemplateProperty);
            }

            set
            {
                SetValue(SelectedNeuronTemplateProperty, value);
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>
        ///     Gets an enumerator to the content control's logical child elements.
        /// </summary>
        /// <returns>An enumerator. The default value is null.</returns>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                yield return base.LogicalChildren;
                foreach (object i in ItemsSources)
                {
                    yield return i;
                }
            }
        }

        /// <summary>Handles the SelectionChanged event of the ItemBrowser control. When the user made
        ///     a selection, we close the box again.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ItemBrowser_SelectionChanged(object sender, System.EventArgs e)
        {
            IsDropDownOpen = false;

                // important: close first, then set value. This is for the undo system: otherwise we have the wrong object focused when the undo system requests item in focus (it would return the drop-down, not the backing control).
            Focus();

                // we move focus, so the undo get's the correct path to the item that was selected during the change.
            SelectedNeuron = ItemBrowser.SelectedNeuron;
        }

        /// <summary>Handles the SelectionCanceled event of the ItemBrowser control. When canceled, simply close the
        ///     combobox.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ItemBrowser_SelectionCanceled(object sender, System.EventArgs e)
        {
            IsDropDownOpen = false;
        }

        /// <summary>Handles the Opened event of the Popup control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Popup_Opened(object sender, System.EventArgs e)
        {
            ItemBrowser.ItemsSources = ItemsSources;
            ItemBrowser.AttachItemsSources();
            ItemBrowser.Focus();
        }

        /// <summary>Handles the Closed event of the Popup control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Popup_Closed(object sender, System.EventArgs e)
        {
            ItemBrowser.DetachItemsSources();
            ItemBrowser.ItemsSources = null;
            Focus();
        }

        #endregion

        #region overrides

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver)
            {
                BtnToggle.Visibility = System.Windows.Visibility.Visible;
                System.Windows.Controls.Grid.SetColumnSpan(ThePresenter, 1);
            }

            base.OnMouseEnter(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver && IsDropDownOpen == false)
            {
                BtnToggle.Visibility = System.Windows.Visibility.Collapsed;
                System.Windows.Controls.Grid.SetColumnSpan(ThePresenter, 2);
            }

            base.OnMouseLeave(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown"/> attached event reaches an element
        ///     in its route that is derived from this class. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event
        ///     data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource != BtnToggle)
            {
                Focus();
                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        #endregion

        #region Commands

        #region sync

        /// <summary>Handles the CanExecute event of the Sync control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Sync_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedNeuron != null;
        }

        /// <summary>Handles the Executed event of the Sync control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Sync_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.Current.SyncExplorerToNeuron(SelectedNeuron);
        }

        #endregion

        #region Cut/copy/paste

        /// <summary>Handles the CanExecute event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Copy_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedNeuron != null;
        }

        /// <summary>Handles the Executed event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Copy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.NeuronIDFormat, SelectedNeuron.ID, false);
            System.Windows.Clipboard.SetDataObject(iData, false);
        }

        /// <summary>Handles the CanExecute event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Cut_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedNeuron != null && CanClearValue;
        }

        /// <summary>Handles the Executed event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Cut_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iData = EditorsHelper.GetDataObject();
            iData.SetData(Properties.Resources.NeuronIDFormat, SelectedNeuron.ID, false);
            System.Windows.Clipboard.SetDataObject(iData, false);
            BtnReset_Click(sender, e);
        }

        /// <summary>Handles the CanExecute event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void Paste_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat);
        }

        /// <summary>Handles the Executed event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Paste_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat);
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                SelectedNeuron = Brain.Current[iId];
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #endregion
    }
}