// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastNeuronDataEditor.cs" company="">
//   
// </copyright>
// <summary>
//   A fast implementation for rendering a neurondata editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A fast implementation for rendering a neurondata editor.
    /// </summary>
    public class FastNeuronDataEditor : System.Windows.FrameworkElement, System.Windows.IWeakEventListener
    {
        /// <summary>The f converter.</summary>
        private NeuronToImageBrushConverter fConverter;

        /// <summary>The f data.</summary>
        private NeuronData fData; // for the event handler, need to keep this alive for as long as we ref the value. 

        /// <summary>The f data mode.</summary>
        private NeuronDataEditMode fDataMode = NeuronDataEditMode.text;

        /// <summary>The f delete btn.</summary>
        private System.Windows.Controls.Button fDeleteBtn;

        /// <summary>The f image.</summary>
        private System.Windows.Media.DrawingVisual fImage; // fast way for rendering images

        /// <summary>The f image brush.</summary>
        private System.Windows.Media.BitmapCacheBrush fImageBrush;

        /// <summary>The f timer.</summary>
        private System.Windows.Threading.DispatcherTimer fTimer; // for delaying the upate of the value.

        /// <summary>The f toggle.</summary>
        private System.Windows.Controls.Primitives.ToggleButton fToggle;

        /// <summary>The f text.</summary>
        private readonly System.Windows.Controls.TextBox fText;

        /// <summary>The f visuals.</summary>
        private readonly System.Windows.Media.VisualCollection fVisuals;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FastNeuronDataEditor" /> class.
        /// </summary>
        public FastNeuronDataEditor()
        {
            fVisuals = new System.Windows.Media.VisualCollection(this);
            fText = new System.Windows.Controls.TextBox();
            fText.TextChanged += fText_TextChanged;
            fVisuals.Add(fText);
        }

        /// <summary>Initializes static members of the <see cref="FastNeuronDataEditor"/> class.</summary>
        static FastNeuronDataEditor()
        {
            System.Windows.EventManager.RegisterClassHandler(
                typeof(FastNeuronDataEditor), 
                LostKeyboardFocusEvent, 
                new System.Windows.Input.KeyboardFocusChangedEventHandler(DataEditor_LostKeyboardFocus));
        }

        #region visuals

        /// <summary>Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the
        ///     specified index from a collection of child elements.</summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element. This should not return null; if the provided index is out of range, an exception is
        ///     thrown.</returns>
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return fVisuals[index];
        }

        /// <summary>
        ///     Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return fVisuals.Count;
            }
        }

        #endregion

        #region prop

        #region ToggleButtonStyle

        /// <summary>
        ///     ToggleButtonButtonStyle Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ToggleButtonStyleProperty =
            System.Windows.DependencyProperty.Register(
                "ToggleButtonStyle", 
                typeof(System.Windows.Style), 
                typeof(FastNeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(null, OnToggleButtonStyleChanged));

        /// <summary>
        ///     Gets or sets the ToggleButtonButtonStyle property.  This dependency property
        ///     indicates the style to use for the toggle button that shows/hides the dropdown menu.
        /// </summary>
        public System.Windows.Style ToggleButtonStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(ToggleButtonStyleProperty);
            }

            set
            {
                SetValue(ToggleButtonStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the ToggleButtonButtonStyle property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnToggleButtonStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FastNeuronDataEditor)d).OnToggleButtonStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ToggleButtonButtonStyle property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnToggleButtonStyleChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fToggle != null)
            {
                fToggle.Style = (System.Windows.Style)e.NewValue;
            }
        }

        #endregion

        #region TextBoxStyle

        /// <summary>
        ///     TextBoxStyle Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty TextBoxStyleProperty =
            System.Windows.DependencyProperty.Register(
                "TextBoxStyle", 
                typeof(System.Windows.Style), 
                typeof(FastNeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(null, OnTextBoxStyleChanged));

        /// <summary>
        ///     Gets or sets the TextBoxStyle property.  This dependency property
        ///     indicates the style to use for the textbox.
        /// </summary>
        public System.Windows.Style TextBoxStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(TextBoxStyleProperty);
            }

            set
            {
                SetValue(TextBoxStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the TextBoxStyle property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnTextBoxStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FastNeuronDataEditor)d).OnTextBoxStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the TextBoxStyle property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnTextBoxStyleChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fText != null)
            {
                fText.Style = e.NewValue as System.Windows.Style;
            }
        }

        #endregion

        #region DeleteButtonStyle

        /// <summary>
        ///     DeleteButtonStyle Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty DeleteButtonStyleProperty =
            System.Windows.DependencyProperty.Register(
                "DeleteButtonStyle", 
                typeof(System.Windows.Style), 
                typeof(FastNeuronDataEditor), 
                new System.Windows.FrameworkPropertyMetadata(null, OnDeleteButtonStyleChanged));

        /// <summary>
        ///     Gets or sets the DeleteButtonStyle property.  This dependency property
        ///     indicates the style to use for the delete button.
        /// </summary>
        public System.Windows.Style DeleteButtonStyle
        {
            get
            {
                return (System.Windows.Style)GetValue(DeleteButtonStyleProperty);
            }

            set
            {
                SetValue(DeleteButtonStyleProperty, value);
            }
        }

        /// <summary>Handles changes to the DeleteButtonStyle property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnDeleteButtonStyleChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((FastNeuronDataEditor)d).OnDeleteButtonStyleChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the DeleteButtonStyle property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnDeleteButtonStyleChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (fDeleteBtn != null)
            {
                fDeleteBtn.Style = e.NewValue as System.Windows.Style;
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
                typeof(FastNeuronDataEditor), 
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

        #region ShowDropDownOnMouseOver

        /// <summary>
        ///     ShowDropDownOnMouseOver Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ShowDropDownOnMouseOverProperty =
            System.Windows.DependencyProperty.Register(
                "ShowDropDownOnMouseOver", 
                typeof(bool), 
                typeof(FastNeuronDataEditor), 
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
            ((FastNeuronDataEditor)d).OnShowDropDownOnMouseOverChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the ShowDropDownOnMouseOver property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnShowDropDownOnMouseOverChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                DestroyToggle();
            }
            else
            {
                ShowToggle();
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
                typeof(FastNeuronDataEditor), 
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

        #region Converter

        /// <summary>Gets the converter.</summary>
        private NeuronToImageBrushConverter Converter
        {
            get
            {
                if (fConverter == null)
                {
                    fConverter =
                        System.Windows.Application.Current.TryFindResource("FastNeuronToImgConv") as
                        NeuronToImageBrushConverter;
                }

                return fConverter;
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
                typeof(FastNeuronDataEditor), 
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
            ((FastNeuronDataEditor)d).OnIconVisibilityChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the IconVisibility property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnIconVisibilityChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UpdateImage(SelectedNeuron, (System.Windows.Visibility)e.NewValue);
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
                typeof(FastNeuronDataEditor), 
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
            ((FastNeuronDataEditor)d).OnSelectedNeuronChanged(e);
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
            UpdateImage(iNew, IconVisibility);
            RaiseSelectedNeuronChangedEvent((Neuron)e.OldValue, (Neuron)e.NewValue);
        }

        /// <summary>makes certain that, if there is an image required, it is created and has the correct brush.</summary>
        /// <param name="value"></param>
        /// <param name="visibility">The visibility.</param>
        private void UpdateImage(Neuron value, System.Windows.Visibility visibility)
        {
            if (value != null && visibility == System.Windows.Visibility.Visible)
            {
                if (fImage == null)
                {
                    fImage = new System.Windows.Media.DrawingVisual();
                    if (fDeleteBtn != null)
                    {
                        // if ther is a delete button, we need to make certain that the image is placed in front of it so tat the delete button is always on top.
                        var iIndex = fVisuals.IndexOf(fDeleteBtn);
                        fVisuals.Insert(iIndex, fDeleteBtn);
                    }
                    else
                    {
                        fVisuals.Add(fImage);
                    }
                }
                else
                {
                    InvalidateVisual();
                }

                fImageBrush =
                    Converter.Convert(value, value.GetType(), null, null) as System.Windows.Media.BitmapCacheBrush;
            }
            else if (fImage != null)
            {
                fVisuals.Remove(fImage);
                fImage = null;
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
                typeof(FastNeuronDataEditor));

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

        #region CanClearValue

        /// <summary>
        ///     CanClearValue Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CanClearValueProperty =
            System.Windows.DependencyProperty.Register(
                "CanClearValue", 
                typeof(bool), 
                typeof(FastNeuronDataEditor), 
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
            ((FastNeuronDataEditor)d).OnCanClearValueChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the CanClearValue property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnCanClearValueChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && SelectedNeuron != null)
            {
                fDeleteBtn = new System.Windows.Controls.Button();
                fDeleteBtn.Style = DeleteButtonStyle;
                var iBtnImg = new System.Windows.Controls.Image();
                iBtnImg.Source =
                    new System.Windows.Media.Imaging.BitmapImage(
                        new System.Uri("\\Images\\Edit\\DeleteVerySmall_Enabled.png"));
                fDeleteBtn.Content = iBtnImg;
                fDeleteBtn.Click += fDeleteBtn_Click;
                fVisuals.Add(fDeleteBtn);
            }
            else if (fDeleteBtn != null)
            {
                fDeleteBtn.Click -= fDeleteBtn_Click;
                fVisuals.Remove(fDeleteBtn);
                fDeleteBtn = null;
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
                typeof(FastNeuronDataEditor), 
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
                typeof(FastNeuronDataEditor), 
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
                typeof(FastNeuronDataEditor), 
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

        #endregion

        #region functions

        /// <summary>The set mode and text.</summary>
        /// <param name="value">The value.</param>
        private void SetModeAndText(Neuron value)
        {
            try
            {
                if (value != null)
                {
                    fData = BrainData.Current.NeuronInfo[value];
                    System.ComponentModel.PropertyChangedEventManager.AddListener(fData, this, "DisplayTitle");
                    fText.Tag = true; // so we can keep track if internal updates for this field

                    var iCluster = value as NeuronCluster;
                    if (iCluster != null)
                    {
                        if (iCluster.Meaning == (ulong)PredefinedNeurons.Time)
                        {
                            fDataMode = NeuronDataEditMode.Time;
                            var iVal = Time.GetTime(iCluster);
                            if (iVal.HasValue)
                            {
                                fText.Text = iVal.Value.ToString();
                            }
                            else
                            {
                                fText.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                            }
                        }
                        else if (iCluster.Meaning == (ulong)PredefinedNeurons.TimeSpan)
                        {
                            fDataMode = NeuronDataEditMode.TimeSpan;
                            var iVal = Time.GetTimeSpan(iCluster);
                            if (iVal.HasValue)
                            {
                                fText.Text = iVal.Value.ToString();
                            }
                            else
                            {
                                fText.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                            }
                        }
                        else
                        {
                            fDataMode = NeuronDataEditMode.text;
                            fText.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                        }
                    }
                    else if (value is DoubleNeuron)
                    {
                        fDataMode = NeuronDataEditMode.Float;
                        fText.Text = ((DoubleNeuron)value).Value.ToString();
                    }
                    else if (value is IntNeuron)
                    {
                        fDataMode = NeuronDataEditMode.Integer;
                        fText.Text = ((IntNeuron)value).Value.ToString();
                    }
                    else
                    {
                        fDataMode = NeuronDataEditMode.text;
                        fText.Text = BrainData.Current.NeuronInfo[value].DisplayTitle;
                    }
                }
                else
                {
                    fData = null;
                    fText.Text = string.Empty;
                    fDataMode = NeuronDataEditMode.text;
                }
            }
            finally
            {
                fText.Tag = null;
            }

            UpdateImage(value, IconVisibility);
        }

        /// <summary>Handles the LostKeyboardFocus event of the DataEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private static void DataEditor_LostKeyboardFocus(
            object sender, 
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            var iSender = sender as FastNeuronDataEditor;
            if (iSender.fTimer != null)
            {
                iSender.TryProcessInput();
            }
        }

        /// <summary>
        ///     Called when the input should be reparsed and stored in the network. Called when the time is past or the control
        ///     looses keyboard focus.
        /// </summary>
        private void TryProcessInput()
        {
            ChangeNeuron(fText.Text);
            fTimer.Stop();
            fTimer = null; // data has been processed, reset the timer.
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

        /// <summary>
        ///     Hides and destroys the toggle button.
        /// </summary>
        private void DestroyToggle()
        {
            if (fToggle != null)
            {
                fVisuals.Remove(fToggle);
                fToggle.Checked -= fToggle_Checked;
                fToggle = null;
                InvalidateMeasure();
            }
        }

        /// <summary>
        ///     Creates the toggle button and shows it.
        /// </summary>
        private void ShowToggle()
        {
            if (fToggle == null)
            {
                fToggle = new System.Windows.Controls.Primitives.ToggleButton();
                fToggle.Checked += fToggle_Checked;

                // fToggle.Unchecked += fToggle_Unchecked;
                fToggle.Style = ToggleButtonStyle;
                fVisuals.Add(fToggle);
                InvalidateMeasure();
            }
        }

        /// <summary>The f toggle_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fToggle_Checked(object sender, System.Windows.RoutedEventArgs e)
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
            object iOver = System.Windows.Input.Mouse.DirectlyOver;
            if (iOver != fToggle
                || System.Windows.Input.Mouse.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                // the popup is also automatically closed when we press the toggle button again. But in this case, we want to close it manually, so filter on this situation, othwerwise the popup closes and opens again. .
                if (fToggle != null)
                {
                    fToggle.IsChecked = false;
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

                if (iOver != this && iOver != fText && iOver != fImage)
                {
                    // if the mouse isn't the over the editor, remove the toggle
                    DestroyToggle();
                }
            }
        }

        /// <summary>The f text_ text changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (fText.Tag == null)
            {
                // when the tag is not null, we are doing an internal update (setting the textvalue) so, we don't want to do a parse again.
                switch (EditMode)
                {
                    case NeuronDataEditMode.DisplayTitle:
                        var iNeuron = SelectedNeuron;
                        if (iNeuron != null)
                        {
                            BrainData.Current.NeuronInfo[iNeuron].DisplayTitle = fText.Text;
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

        /// <summary>Handles the Tick event of the DelayTimer control.
        ///     When the time has past, we try to update the NeuronData according to the new input text. If
        ///     there was a problem, show an error.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DelayTimer_Tick(object sender, System.EventArgs e)
        {
            TryProcessInput();
        }

        /// <summary>Handles the Click event of the fDeleteBtn control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void fDeleteBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedNeuron = null;
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver)
            {
                ShowToggle();
            }

            base.OnMouseEnter(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/> attached event is raised on this
        ///     element. Implement this method to add class handling for this event.</summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (ShowDropDownOnMouseOver && fToggle != null && fToggle.IsChecked == false)
            {
                DestroyToggle();
            }

            base.OnMouseLeave(e);
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>true if the listener handled the event. It is considered an error by the<see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the
        ///     listener does not handle. Regardless, the method should return false if it receives an event that it does not
        ///     recognize or handle.</returns>
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
            fText.Text = fData.DisplayTitle; // update the text
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

        #region rendering

        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        ///     The rendering instructions for this element are not used directly when this method is invoked, and are instead
        ///     preserved for later asynchronous use by layout and drawing.</summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout
        ///     system.</param>
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(
                System.Windows.Media.Brushes.Transparent, 
                null, 
                new System.Windows.Rect(DesiredSize));

                // draw a transparent rectangle so that the mouse doesn't jump continuously when approaching the dropdown arrow.
            if (fImage != null)
            {
                drawingContext.DrawRectangle(fImageBrush, null, new System.Windows.Rect(0, 0, 16, 16));
            }
        }

        /// <summary>When overridden in a derived class, positions child elements and determines a size for a<see cref="T:System.Windows.FrameworkElement"/> derived class.</summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its
        ///     children.</param>
        /// <returns>The actual size used.</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            var iPos = new System.Windows.Rect();
            if (fDeleteBtn != null)
            {
                iPos.Size = fDeleteBtn.DesiredSize;
                fDeleteBtn.Arrange(iPos);
            }

            System.Windows.Size iTextBoxSize;
            if (fToggle != null)
            {
                iPos.X = finalSize.Width - fToggle.DesiredSize.Width;

                // iPos.Y = (finalSize.Height - fToggle.DesiredSize.Height) / 2;
                iPos.Size = new System.Windows.Size(fToggle.DesiredSize.Width, finalSize.Height);
                fToggle.Arrange(iPos);
                iTextBoxSize = new System.Windows.Size(finalSize.Width - iPos.Size.Width, finalSize.Height);
            }
            else
            {
                iTextBoxSize = finalSize;
            }

            iPos.Y = 0;
            if (fImage != null)
            {
                // calculate the start of the textbox.
                iPos.X = 16;
                iTextBoxSize.Width -= 16;
            }
            else
            {
                iPos.X = 0;
            }

            iPos.Size = iTextBoxSize;
            fText.Arrange(iPos);

            return finalSize;
        }

        /// <summary>When overridden in a derived class, measures the size in layout required for child elements and determines a size
        ///     for the <see cref="T:System.Windows.FrameworkElement"/>-derived class.</summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified
        ///     as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var iUsed = new System.Windows.Size();
            if (fDeleteBtn != null)
            {
                fDeleteBtn.Measure(availableSize);
                iUsed.Width = fDeleteBtn.DesiredSize.Width;

                    // the delete button doesn't consume the space, it's on top of the rest.
                iUsed.Height = fDeleteBtn.DesiredSize.Height;
            }

            if (fImage != null)
            {
                // we draw this on a context.
                availableSize.Width -= 16; // the image is always 16*16, the size is consumed
                if (iUsed.Width < 16)
                {
                    iUsed.Width = 16;
                }

                if (iUsed.Height < 16)
                {
                    iUsed.Height = 16;
                }
            }

            if (fToggle != null)
            {
                fToggle.Measure(availableSize);
                iUsed.Width += fToggle.DesiredSize.Width;
                if (iUsed.Height < fToggle.DesiredSize.Height)
                {
                    iUsed.Height = fToggle.DesiredSize.Height;
                }

                availableSize.Width -= fToggle.DesiredSize.Width;
            }

            fText.Measure(availableSize);
            iUsed.Width += fText.DesiredSize.Width;
            if (iUsed.Height < fText.DesiredSize.Height)
            {
                iUsed.Height = fText.DesiredSize.Height;
            }

            return iUsed;
        }

        #endregion
    }
}