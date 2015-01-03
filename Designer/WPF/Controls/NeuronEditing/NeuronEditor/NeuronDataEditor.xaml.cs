// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronDataEditor.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   determins the different editor modes that a <see cref="NeuronDataEditor" /> can be in.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     determins the different editor modes that a <see cref="NeuronDataEditor" /> can be in.
    /// </summary>
    public enum NeuronDataEditMode
    {
        /// <summary>The display title.</summary>
        DisplayTitle, // emmidiate assign

        /// <summary>The time span.</summary>
        TimeSpan, // cash changes, use timer to delay parsing of the data.

        /// <summary>The time.</summary>
        Time, // cash changes, use timer to delay parsing of the data.

        /// <summary>The integer.</summary>
        Integer, // cash changes, use timer to delay parsing of the data.

        /// <summary>The float.</summary>
        Float, // cash changes, use timer to delay parsing of the data.

        /// <summary>The text.</summary>
        text // cash changes, use timer to delay parsing of the data.
    }

    /// <summary>
    ///     Interaction logic for NeuronDataEditor.xaml
    /// </summary>
    public partial class NeuronDataEditor : System.Windows.Controls.UserControl, System.Windows.IWeakEventListener
    {
        #region DataMode

        /// <summary>
        ///     Gets/sets the mode of the editor determined by the data itself (calculated value).
        /// </summary>
        public NeuronDataEditMode DataMode
        {
            get
            {
                return fDataMode;
            }

            set
            {
                fDataMode = value;
            }
        }

        #endregion

        /// <summary>The txt value_ text changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TxtValue.Tag == null)
            {
                // when the tag is not null, we are doing an internal update (setting the textvalue) so, we don't want to do a parse again.
                switch (EditMode)
                {
                    case NeuronDataEditMode.DisplayTitle:
                        var iNeuron = SelectedNeuron;
                        if (iNeuron != null)
                        {
                            BrainData.Current.NeuronInfo[iNeuron].DisplayTitle = TxtValue.Text;
                        }

                        break;
                    case NeuronDataEditMode.TimeSpan:
                    case NeuronDataEditMode.Time:
                    case NeuronDataEditMode.Float:
                    case NeuronDataEditMode.Integer:
                    case NeuronDataEditMode.text:
                        ResetTimer();

                            // for all text, numbers, time/timespan, we give the user a little more time to input some more data so that we don't overtax the system with continuous reparses.
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        ///     Makes certain that the timer exists and is reset so that it runs the full amount of time again.
        ///     This is called whenever the input text has changed.
        /// </summary>
        private void ResetTimer()
        {
            if (fTimer == null)
            {
                fTimer = new System.Windows.Threading.DispatcherTimer();
                fTimer.Interval = new System.TimeSpan(0, 0, 2);

                    // we delay the update for 2 seconds, should give the user enough time to think and continue typing before displaying a format error.
                fTimer.Tick += DelayTimer_Tick;
            }
            else
            {
                fTimer.Stop(); // stop the timer so we can restart it at a fresh time slot.
            }

            fTimer.Start();
        }

        /// <summary>Handles the LostKeyboardFocus event of the DataEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private static void DataEditor_LostKeyboardFocus(
            object sender, 
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            var iSender = sender as NeuronDataEditor;
            if (iSender.fTimer != null)
            {
                iSender.TryProcessInput();
            }
        }

        /// <summary>Handles the Tick event of the DelayTimer control.
        ///     When the time has past, we try to update the NeuronData according to the new input text. If
        ///     there was a problem, show an error.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DelayTimer_Tick(object sender, System.EventArgs e)
        {
            TryProcessInput();
        }

        /// <summary>
        ///     Called when the input should be reparsed and stored in the network. Called when the time is past or the control
        ///     looses keyboard focus.
        /// </summary>
        private void TryProcessInput()
        {
            ChangeNeuron(TxtValue.Text);
            fTimer.Stop();
            fTimer = null; // data has been processed, reset the timer.
        }

        /// <summary>Handles the Click event of the BtnReset control.
        ///     resets the value to null.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnReset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedNeuron = null;
        }

        #region fields

        /// <summary>The f data mode.</summary>
        private NeuronDataEditMode fDataMode = NeuronDataEditMode.text;

        /// <summary>The f timer.</summary>
        private System.Windows.Threading.DispatcherTimer fTimer; // for delaying the upate of the value.

        /// <summary>The f data.</summary>
        private NeuronData fData; // for the event handler, need to keep this alive for as long as we ref the value. 

        #endregion

        #region ctor

        /// <summary>Initializes static members of the <see cref="NeuronDataEditor"/> class. </summary>
        static NeuronDataEditor()
        {
            System.Windows.EventManager.RegisterClassHandler(
                typeof(NeuronDataEditor), 
                LostKeyboardFocusEvent, 
                new System.Windows.Input.KeyboardFocusChangedEventHandler(DataEditor_LostKeyboardFocus));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NeuronDataEditor" /> class.
        /// </summary>
        public NeuronDataEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region ShowDropDownOnMouseOver

        /// <summary>
        ///     ShowDropDownOnMouseOver Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ShowDropDownOnMouseOverProperty =
            System.Windows.DependencyProperty.Register(
                "ShowDropDownOnMouseOver", 
                typeof(bool), 
                typeof(NeuronDataEditor), 
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
            ((NeuronDataEditor)d).OnShowDropDownOnMouseOverChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ShowDropDownOnMouseOver property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnShowDropDownOnMouseOverChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                PART_TOGGLE.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                PART_TOGGLE.Visibility = System.Windows.Visibility.Visible;
            }
        }

        #endregion

        #region ImageVisibility

        /// <summary>
        ///     ImageVisibility Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ImageVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "ImageVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Visibility.Visible, 
                    OnImageVisibilityChanged));

        /// <summary>
        ///     Gets or sets the ImageVisibility property.  This dependency property
        ///     indicates if the image in front of the text should be visilbe or not.
        /// </summary>
        public System.Windows.Visibility ImageVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(ImageVisibilityProperty);
            }

            set
            {
                SetValue(ImageVisibilityProperty, value);
            }
        }

        /// <summary>Handles changes to the ImageVisibility property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnImageVisibilityChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NeuronDataEditor)d).OnImageVisibilityChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ImageVisibility property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnImageVisibilityChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (PART_SelectedNeuronImage != null)
            {
                PART_SelectedNeuronImage.Visibility = (System.Windows.Visibility)e.NewValue;
            }
        }

        #endregion

        #region Popup

        /// <summary>
        ///     Popup Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty PopupProperty =
            System.Windows.DependencyProperty.Register(
                "Popup", 
                typeof(NeuronBrowserPopup), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata((NeuronBrowserPopup)null));

        /// <summary>
        ///     Gets or sets the Popup property.  This dependency property
        ///     indicates the popup to use for displaying a selection list.
        /// </summary>
        public NeuronBrowserPopup Popup
        {
            get
            {
                return (NeuronBrowserPopup)GetValue(PopupProperty);
            }

            set
            {
                SetValue(PopupProperty, value);
            }
        }

        #endregion

        #region EditMode

        /// <summary>
        ///     EditMode Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty EditModeProperty =
            System.Windows.DependencyProperty.Register(
                "EditMode", 
                typeof(NeuronDataEditMode), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(NeuronDataEditMode.text));

        /// <summary>
        ///     Gets or sets the EditMode property.  This dependency property
        ///     indicates the edit mode to use for text input. This allows us to switch between editing the DisplayTitle
        ///     of a neuron or creating new neurons while editing.
        /// </summary>
        public NeuronDataEditMode EditMode
        {
            get
            {
                return (NeuronDataEditMode)GetValue(EditModeProperty);
            }

            set
            {
                SetValue(EditModeProperty, value);
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
                typeof(NeuronDataEditor));

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

        #region SelectedNeuron

        /// <summary>
        ///     SelectedNeuron Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedNeuronProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedNeuron", 
                typeof(Neuron), 
                typeof(NeuronDataEditor), 
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
            ((NeuronDataEditor)d).OnSelectedNeuronChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the SelectedNeuron property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnSelectedNeuronChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fData != null)
            {
                System.ComponentModel.PropertyChangedEventManager.RemoveListener(fData, this, "DisplayTitle");
            }

            var iNew = e.NewValue as Neuron;
            SetModeAndText(iNew);
            RaiseSelectedNeuronChangedEvent((Neuron)e.OldValue, (Neuron)e.NewValue);
        }

        /// <summary>The set mode and text.</summary>
        /// <param name="value">The value.</param>
        private void SetModeAndText(Neuron value)
        {
            TxtValue.Tag = true; // so we can keep track if internal updates for this field
            try
            {
                if (value != null)
                {
                    if (PART_SelectedNeuronImage != null)
                    {
                        PART_SelectedNeuronImage.Visibility = IconVisibility;

                            // when there is data, use the default setting for making the icon visible or not.
                    }

                    fData = BrainData.Current.NeuronInfo[value];
                    System.ComponentModel.PropertyChangedEventManager.AddListener(fData, this, "DisplayTitle");

                    var iCluster = value as NeuronCluster;
                    if (iCluster != null)
                    {
                        if (iCluster.Meaning == (ulong)PredefinedNeurons.Time)
                        {
                            fDataMode = NeuronDataEditMode.Time;
                            var iVal = Time.GetTime(iCluster);
                            if (iVal.HasValue)
                            {
                                TxtValue.Text = iVal.Value.ToString();
                            }
                            else
                            {
                                TxtValue.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                            }
                        }
                        else if (iCluster.Meaning == (ulong)PredefinedNeurons.TimeSpan)
                        {
                            fDataMode = NeuronDataEditMode.TimeSpan;
                            var iVal = Time.GetTimeSpan(iCluster);
                            if (iVal.HasValue)
                            {
                                TxtValue.Text = iVal.Value.ToString();
                            }
                            else
                            {
                                TxtValue.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                            }
                        }
                        else
                        {
                            fDataMode = NeuronDataEditMode.text;
                            TxtValue.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                        }
                    }
                    else if (value is DoubleNeuron)
                    {
                        fDataMode = NeuronDataEditMode.Float;
                        TxtValue.Text = ((DoubleNeuron)value).Value.ToString();
                    }
                    else if (value is IntNeuron)
                    {
                        fDataMode = NeuronDataEditMode.Integer;
                        TxtValue.Text = ((IntNeuron)value).Value.ToString();
                    }
                    else
                    {
                        fDataMode = NeuronDataEditMode.text;
                        TxtValue.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                    }
                }
                else
                {
                    fData = null;
                    TxtValue.Text = string.Empty;
                    fDataMode = NeuronDataEditMode.text;
                    if (PART_SelectedNeuronImage != null)
                    {
                        PART_SelectedNeuronImage.Visibility = System.Windows.Visibility.Collapsed;

                            // when there is no data, always hide the icon
                    }
                }
            }
            finally
            {
                TxtValue.Tag = null;
            }
        }

        #endregion

        #region LinkFrom

        /// <summary>
        ///     LinkFrom Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LinkFromProperty =
            System.Windows.DependencyProperty.Register(
                "LinkFrom", 
                typeof(Neuron), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata((Neuron)null));

        /// <summary>
        ///     Gets or sets the LinkFrom property.  This dependency property
        ///     indicates the from neuron to use in case a change in the SElectedNeuron should also modify a link in the same undo
        ///     group.
        /// </summary>
        public Neuron LinkFrom
        {
            get
            {
                return (Neuron)GetValue(LinkFromProperty);
            }

            set
            {
                SetValue(LinkFromProperty, value);
            }
        }

        #endregion

        #region LinkMeaning

        /// <summary>
        ///     LinkMeaning Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty LinkMeaningProperty =
            System.Windows.DependencyProperty.Register(
                "LinkMeaning", 
                typeof(Neuron), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata((Neuron)null));

        /// <summary>
        ///     Gets or sets the LinkMeaning property.  This dependency property
        ///     indicates the meaning part of the link that needs to be maintained when the SelectedNeuron is changed (so that the
        ///     change remains in the same undo group).
        /// </summary>
        public Neuron LinkMeaning
        {
            get
            {
                return (Neuron)GetValue(LinkMeaningProperty);
            }

            set
            {
                SetValue(LinkMeaningProperty, value);
            }
        }

        #endregion

        #region Cluster

        /// <summary>
        ///     Cluster Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ClusterProperty =
            System.Windows.DependencyProperty.Register(
                "Cluster", 
                typeof(Neuron), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata((Neuron)null));

        /// <summary>
        ///     Gets or sets the Cluster property.  This dependency property
        ///     indicates the cluster that should contain the  selectedNeuron and which should be updated (added) in the same
        ///     undogroup.
        /// </summary>
        public Neuron Cluster
        {
            get
            {
                return (Neuron)GetValue(ClusterProperty);
            }

            set
            {
                SetValue(ClusterProperty, value);
            }
        }

        #endregion

        #region IconVisibility

        /// <summary>
        ///     IconVisibility Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty IconVisibilityProperty =
            System.Windows.DependencyProperty.Register(
                "IconVisibility", 
                typeof(System.Windows.Visibility), 
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(System.Windows.Visibility.Visible, OnIconVisibilityChanged));

        /// <summary>
        ///     Gets or sets the IconVisibility property.  This dependency property
        ///     indicates the visisibility of the icon.
        /// </summary>
        public System.Windows.Visibility IconVisibility
        {
            get
            {
                return (System.Windows.Visibility)GetValue(IconVisibilityProperty);
            }

            set
            {
                SetValue(IconVisibilityProperty, value);
            }
        }

        /// <summary>Handles changes to the IconVisibility property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnIconVisibilityChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((NeuronDataEditor)d).OnIconVisibilityChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the IconVisibility property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIconVisibilityChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (PART_SelectedNeuronImage != null && SelectedNeuron != null)
            {
                PART_SelectedNeuronImage.Visibility = (System.Windows.Visibility)e.NewValue;
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
                typeof(NeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(false, OnCanClearValueChanged));

        /// <summary>
        ///     Gets or sets the CanClearValue property.  This dependency property
        ///     indicates wether the editor displays a delete button or not.
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
            ((NeuronDataEditor)d).OnCanClearValueChanged(e);
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

        #region functions

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver)
            {
                PART_TOGGLE.Visibility = System.Windows.Visibility.Visible;
            }

            base.OnMouseEnter(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver && PART_TOGGLE.IsChecked == false)
            {
                PART_TOGGLE.Visibility = System.Windows.Visibility.Collapsed;
            }

            base.OnMouseLeave(e);
        }

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

        /// <summary>Handles the Checked event of the PART_TOGGLE control.
        ///     open the popup.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PART_TOGGLE_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.Popup iPopup = Popup;
            if (iPopup != null)
            {
                ((NeuronBrowserPopup)iPopup).SelectedNeuron = SelectedNeuron;
                iPopup.PlacementTarget = this;
                iPopup.Closed += Popup_Closed;
                iPopup.IsOpen = true;
            }
        }

        /// <summary>Handles the Closed event of the Popup control.
        ///     cleanup when popup is closed</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Popup_Closed(object sender, System.EventArgs e)
        {
            if (System.Windows.Input.Mouse.DirectlyOver != PART_TOGGLE
                || System.Windows.Input.Mouse.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                // the popup is also automatically closed when we press the toggle button again. But in this case, we want to close it manually, so filter on this situation, othwerwise the popup closes and opens again. .
                if (PART_TOGGLE != null)
                {
                    PART_TOGGLE.IsChecked = false;
                }

                var iSender = sender as NeuronBrowserPopup;
                if (iSender != null)
                {
                    if (iSender.SelectedNeuron != null)
                    {
                        // only get the value if the user didn't cancel the operation.
                        SelectedNeuron = iSender.SelectedNeuron;
                    }

                    iSender.Closed -= Popup_Closed;
                }
            }
        }

        /// <summary>Handles the Unchecked event of the PART_TOGGLE control.
        ///     close the popup</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PART_TOGGLE_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            // NeuronBrowserPopup iPopup = Popup;
            // if (iPopup != null)
            // iPopup.IsOpen = false;
        }

        /// <summary>The change neuron.</summary>
        /// <param name="value">The value.</param>
        private void ChangeNeuron(string value)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                RemovePrevValue(SelectedNeuron);
                if (string.IsNullOrEmpty(value) == false)
                {
                    var iSelected = GetNeuron(value);
                    SelectedNeuron = iSelected;
                    if (LinkFrom != null && LinkMeaning != null)
                    {
                        Link.Create(LinkFrom, iSelected, LinkMeaning);
                    }
                    else if (Cluster != null)
                    {
                        var iCluster = (NeuronCluster)Cluster;
                        using (var iList = iCluster.ChildrenW) iList.Add(iSelected);
                    }
                }
                else
                {
                    SelectedNeuron = null;
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>The get neuron.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetNeuron(string value)
        {
            int iVal;
            if (int.TryParse(value, out iVal))
            {
                var iIntRes = NeuronFactory.GetInt(iVal);
                WindowMain.AddItemToBrain(iIntRes);
                return iIntRes;
            }

            double iDouble;
            if (double.TryParse(value, out iDouble))
            {
                var iDoubleRes = NeuronFactory.GetDouble(iDouble);
                WindowMain.AddItemToBrain(iDoubleRes);
                return iDoubleRes;
            }

            return EditorsHelper.ConvertStringToNeurons(value);
        }

        /// <summary>The remove prev value.</summary>
        /// <param name="value">The value.</param>
        private void RemovePrevValue(Neuron value)
        {
            if (value != null)
            {
                if (LinkFrom != null && LinkMeaning != null)
                {
                    var iFound = Link.Find(LinkFrom, value, LinkMeaning);
                    if (iFound != null)
                    {
                        iFound.Destroy();
                    }
                }
                else if (Cluster != null)
                {
                    var iCluster = (NeuronCluster)Cluster;
                    using (var iList = iCluster.ChildrenW) iList.Remove(value);
                }

                var iDel = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
                iDel.Start(value);
            }
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>The receive weak event.</summary>
        /// <param name="managerType">The manager type.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.ComponentModel.PropertyChangedEventManager))
            {
                fData_PropertyChanged(sender, (System.ComponentModel.PropertyChangedEventArgs)e);
                return true;
            }

            return false;
        }

        /// <summary>Handles the PropertyChanged event of the fData control.</summary>
        /// <remarks>Need to update the title of the code Editor if appropriate.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void fData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TxtValue.Text = fData.DisplayTitle; // update the text
        }

        #endregion
    }
}