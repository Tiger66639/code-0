// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDataBrowser.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for NeuronDataBrowser.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for NeuronDataBrowser.xaml
    /// </summary>
    public partial class NeuronDataBrowser : System.Windows.Controls.UserControl
    {
        /// <summary>The f items sources.</summary>
        private System.Collections.Generic.IList<BrowserDataSource> fItemsSources =
            new System.Collections.Generic.List<BrowserDataSource>();

        /// <summary>The f switch.</summary>
        private bool fSwitch;

                     // so we can init the treeview with the selected item whitout immidiatly closing the dropdown list|Also used for changing date/neuron value.

        /// <summary>Initializes a new instance of the <see cref="NeuronDataBrowser"/> class.</summary>
        public NeuronDataBrowser()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Removes all the datasources from the sub controls, so that the data can be released. This is important
        ///     cause this control is often used in a popup, which doesn't unload its children when closed, so the data remains
        ///     in memory. Off course, this object represents a snapshot of the neuornData dictionary, so we need to reload when
        ///     reopened. This shows the most up to date data and doesn't tax the mem to much.
        /// </summary>
        public void DetachItemsSources()
        {
            foreach (var i in fItemsSources)
            {
                i.IsOpened = false;
            }

            TabMain.ItemsSource = null; // do this after closing the datasources, otherwise the ui's don't unload.
        }

        /// <summary>
        ///     Attaches all the datasources to the sub controls, so that the data can be shown. This is important
        ///     cause this control is often used in a popup, which doesn't unload its children when closed, so the data remains
        ///     in memory. Off course, this object represents a snapshot of the neuornData dictionary, so we need to reload when
        ///     reopened. This shows the most up to date data and doesn't tax the mem to much.
        /// </summary>
        public void AttachItemsSources()
        {
            foreach (var i in fItemsSources)
            {
                i.IsOpened = true;
            }

            TabMain.ItemsSource = fItemsSources;
            TabMain.SelectedIndex = 0;
        }

        /// <summary>The focus found.</summary>
        /// <param name="found">The found.</param>
        private void FocusFound(System.Windows.FrameworkElement found)
        {
            found.Focus();
        }

        /// <summary>The tab main_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TabMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender == e.OriginalSource)
            {
                // when the tabcontrol changes tabItem, we quickly move the focus back to the tabcontrontrol. This allows the owning dropDownBox to prevent itself from closing (cause it closes when focus is lost). This prevents undesired closing of the box. but we should only do this for the tabcontrol, not the listboxes it contains.
                TabMain.Focus();
            }
        }

        #region Events

        /// <summary>
        ///     Occurs when the selection was changed.
        /// </summary>
        public event System.EventHandler SelectionChanged;

        /// <summary>
        ///     Occurs when the selection was canceled by the user (by pressing escape).
        /// </summary>
        public event System.EventHandler SelectionCanceled;

        #endregion

        #region Prop

        #region SelectedNeuron

        /// <summary>
        ///     SelectedNeuron Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedNeuronProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedNeuron", 
                typeof(Neuron), 
                typeof(NeuronDataBrowser), 
                new System.Windows.FrameworkPropertyMetadata(null, OnSelectedNeuronChanged));

        /// <summary>
        ///     Gets or sets the SelectedNeuron property.  This dependency property
        ///     indicates currently selected value as a neuron.
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
            ((NeuronDataBrowser)d).OnSelectedNeuronChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the SelectedNeuron property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnSelectedNeuronChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fSwitch == false)
            {
                var iCluster = e.NewValue as NeuronCluster;
                if (iCluster != null)
                {
                    var iVal = Time.GetTime(iCluster);
                    if (iVal.HasValue)
                    {
                        SelectedDate = iVal.Value;
                    }
                }
            }

            if (SelectionChanged != null)
            {
                SelectionChanged(this, System.EventArgs.Empty);
            }
        }

        #endregion

        #region SelectedDate

        /// <summary>
        ///     SelectedDate Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedDateProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedDate", 
                typeof(System.DateTime), 
                typeof(NeuronDataBrowser), 
                new System.Windows.FrameworkPropertyMetadata(System.DateTime.Now, OnSelectedDateChanged));

        /// <summary>
        ///     Gets or sets the SelectedDate property.  This dependency property
        ///     indicates the datetimve value that is selected. This is only valid when the neuron represents a dateTime value.
        /// </summary>
        public System.DateTime SelectedDate
        {
            get
            {
                return (System.DateTime)GetValue(SelectedDateProperty);
            }

            set
            {
                SetValue(SelectedDateProperty, value);
            }
        }

        /// <summary>Handles changes to the SelectedDate property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnSelectedDateChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NeuronDataBrowser)d).OnSelectedDateChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the SelectedDate property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnSelectedDateChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            fSwitch = true;
            try
            {
                SelectedNeuron = Time.Current.GetTimeCluster((System.DateTime)e.NewValue);
            }
            finally
            {
                fSwitch = false;
            }
        }

        #endregion

        #region ItemsSources

        /// <summary>
        ///     Gets the list of itemsSources to use for the drop down box.
        /// </summary>
        public System.Collections.Generic.IList<BrowserDataSource> ItemsSources
        {
            get
            {
                return fItemsSources;
            }

            set
            {
                fItemsSources = value;
            }
        }

        #endregion

        #region AllowSelection

        /// <summary>
        ///     AllowSelection Attached Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty AllowSelectionProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "AllowSelection", 
                typeof(bool), 
                typeof(NeuronDataBrowser), 
                new System.Windows.FrameworkPropertyMetadata(true));

        /// <summary>Gets the AllowSelection property.  This dependency property
        ///     indicates wether the tree-item can be selected or not.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool GetAllowSelection(System.Windows.DependencyObject d)
        {
            return (bool)d.GetValue(AllowSelectionProperty);
        }

        /// <summary>Sets the AllowSelection property.  This dependency property
        ///     indicates wether the tree-item can be selected or not.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetAllowSelection(System.Windows.DependencyObject d, bool value)
        {
            d.SetValue(AllowSelectionProperty, value);
        }

        #endregion

        #endregion

        #region load

        /// <summary>Handles the DataContextChanged event of the TrvItem control. We use this to check if the item needs to be selected
        ///     or not. This
        ///     way, we don't need to create yet another type of wrapper. Seperate for treeviewItem and listboxITem since different
        ///     parents.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void TrvItem_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.TreeViewItem;
            var iSelected = SelectedNeuron;
            var iInfo = iSender.DataContext as INeuronInfo;
            if (iInfo != null)
            {
                var iSenderData = iInfo.NeuronInfo;
                if (iSenderData != null && iSenderData.Neuron == iSelected)
                {
                    fSwitch = true;

                        // this is to prevent the dropdown from closing while we are initializing the treeview item.
                    try
                    {
                        System.Diagnostics.Debug.Assert(iSender != null);
                        iSender.IsSelected = true;
                    }
                    finally
                    {
                        fSwitch = false;
                    }
                }
            }

            // e.Handled = true;
        }

        /// <summary>Handles the Loaded event of the LstItem control. We use this to check if the item needs to be selected or not. This
        ///     way, we don't need to create yet another type of wrapper. Seperate for treeviewItem and listboxITem since different
        ///     parents.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void LstItem_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.ListBoxItem;
            var iSelected = SelectedNeuron;
            var iInfo = iSender.DataContext as INeuronInfo;
            if (iInfo != null)
            {
                var iSenderData = iInfo.NeuronInfo;
                if (iSenderData != null && iSenderData.Neuron == iSelected)
                {
                    fSwitch = true;

                        // this is to prevent the dropdown from closing while we are initializing the treeview item.
                    try
                    {
                        System.Diagnostics.Debug.Assert(iSender != null);
                        iSender.IsSelected = true;
                    }
                    finally
                    {
                        fSwitch = false;
                    }
                }
            }

            // e.Handled = true;
        }

        /// <summary>When the calendar gets shown, make certain that the selected date is shown.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Part_Calendar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.Calendar;
            if (iSender != null)
            {
                iSender.SelectedDate = SelectedDate;
            }
        }

        #endregion

        #region select

        /// <summary>Handles the MouseDoubleClick event of the  listBoxItems. When doubleclick: user made the selection.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;

            if (GetAllowSelection(iSender))
            {
                var iInfo = iSender.DataContext as INeuronInfo;

                NeuronData iNew;
                if (iInfo != null)
                {
                    iNew = iInfo.NeuronInfo;
                }
                else
                {
                    iNew = iSender.DataContext as NeuronData;
                }

                if (iNew != null && fSwitch == false)
                {
                    SelectedNeuron = iNew.Neuron;
                }
            }

            // e.Handled = true;
        }

        /// <summary>Handles the MouseDoubleClick event of the listBoxItems. When doubleclick: user made the selection.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TreeItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var iSender = sender as System.Windows.FrameworkElement;

            if (GetAllowSelection(iSender))
            {
                var iInfo = iSender.DataContext as INeuronInfo;

                NeuronData iNew;
                if (iInfo != null)
                {
                    iNew = iInfo.NeuronInfo;
                }
                else
                {
                    iNew = iSender.DataContext as NeuronData;
                }

                if (iNew != null && fSwitch == false)
                {
                    SelectedNeuron = iNew.Neuron;
                }
            }

            e.Handled = true;
        }

        /// <summary>Handles the KeyDown event of the Item control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void Item_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return || e.Key == System.Windows.Input.Key.Enter)
            {
                var iSender = sender as System.Windows.FrameworkElement;
                var iInfo = iSender.DataContext as INeuronInfo;
                if (iInfo != null)
                {
                    var iNew = iInfo.NeuronInfo;
                    if (iNew != null)
                    {
                        SelectedNeuron = iNew.Neuron;
                    }
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (SelectionCanceled != null)
                {
                    SelectionCanceled(this, System.EventArgs.Empty);
                }
            }
        }

        /// <summary>Handles the SelectedDatesChanged event of the Part_Calendar control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void Part_Calendar_SelectedDatesChanged(
            object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var iSender = sender as System.Windows.Controls.Calendar;
            var iValue = iSender.SelectedDate;
            if (iValue.HasValue)
            {
                SelectedDate = iValue.Value;
            }
        }

        #endregion
    }
}