// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemConditional.cs" company="">
//   
// </copyright>
// <summary>
//   A loop or option.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A loop or option.
    /// </summary>
    public class FlowItemConditional : FlowItemBlock
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FlowItemConditional"/> class. Initializes a new instance of the <see cref="FlowItemConditional"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FlowItemConditional(NeuronCluster toWrap)
            : base(toWrap)
        {
        }

        #endregion

        #region fields

        /// <summary>The f is looped.</summary>
        private bool fIsLooped;

        /// <summary>The f requires floating separator.</summary>
        private bool fRequiresFloatingSeparator;

        /// <summary>The f floating flow splits.</summary>
        private bool fFloatingFlowSplits;

        /// <summary>The f back image.</summary>
        private string fBackImage;

        /// <summary>The f front image.</summary>
        private string fFrontImage;

        /// <summary>The f is selection required.</summary>
        private bool fIsSelectionRequired;

        #endregion

        #region prop

        #region IsLooped

        /// <summary>
        ///     Gets/sets wether the conditional flow item is looped or not.
        /// </summary>
        public bool IsLooped
        {
            get
            {
                return fIsLooped;
            }

            set
            {
                if (value != fIsLooped)
                {
                    InternalSetIsLooped(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.FlowItemIsLoop, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        #endregion

        #region FloatingFlowSplits

        /// <summary>
        ///     Gets/sets wether the flow conditional stops collection parts if a
        ///     floating flow is encountered.
        /// </summary>
        public bool FloatingFlowSplits
        {
            get
            {
                return fFloatingFlowSplits;
            }

            set
            {
                if (value != fFloatingFlowSplits)
                {
                    InternalSetFloatingFlowSplits(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.FloatingFlowSplits, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        #endregion

        #region RequiresFloatingSeparator

        /// <summary>
        ///     Gets/sets wether the conditional flow item is looped or not.
        /// </summary>
        public bool RequiresFloatingSeparator
        {
            get
            {
                return fRequiresFloatingSeparator;
            }

            set
            {
                if (value != fRequiresFloatingSeparator)
                {
                    InternalSetRequiresFloatingSeparator(value);
                    InternalChange = true;
                    try
                    {
                        SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.RequiresFloatingSeparator, value);
                    }
                    finally
                    {
                        InternalChange = false;
                    }
                }
            }
        }

        #endregion

        #region IsSelectionRequired

        /// <summary>
        ///     Gets/sets wether the conditional statement requires a selection or
        ///     not.
        /// </summary>
        public bool IsSelectionRequired
        {
            get
            {
                return fIsSelectionRequired;
            }

            set
            {
                InternalSetIsSelectionRequired(value);
                InternalChange = true;
                try
                {
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.FlowItemRequiresSelection, value);
                }
                finally
                {
                    InternalChange = false;
                }
            }
        }

        #endregion

        #region FrontImage

        /// <summary>
        ///     Gets the string to put in front of the conditional, to indicate it is
        ///     a loop '{' or option '['
        /// </summary>
        public object FrontImage
        {
            get
            {
                if (IsSelected == false)
                {
                    return System.Windows.Application.Current.FindResource(fFrontImage);
                }

                return System.Windows.Application.Current.FindResource("Selected" + fFrontImage);
            }

            internal set
            {
                fFrontImage = (string)value;
                OnPropertyChanged("FrontImage");
            }
        }

        #endregion

        #region BackSign

        /// <summary>
        ///     Gets the string to put in the back of the conditional, to indicate it
        ///     is a loop '}' or option ']'
        /// </summary>
        public object BackImage
        {
            get
            {
                if (IsSelected == false)
                {
                    return System.Windows.Application.Current.FindResource(fBackImage);
                }

                return System.Windows.Application.Current.FindResource("Selected" + fBackImage);
            }

            set
            {
                fBackImage = (string)value;
                OnPropertyChanged("BackImage");
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Updates the root object's<see cref="JaStDev.HAB.Designer.IEditorSelection.SelectedItems"/>
        ///     list so that everything is up to date.</summary>
        /// <remarks>When the selection of this object is changed, we need to update the
        ///     images used for front and backsign.</remarks>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected internal override void SetSelected(bool value)
        {
            base.SetSelected(value);
            OnPropertyChanged("FrontImage");
            OnPropertyChanged("BackImage");
        }

        /// <summary>Stores the actual <paramref name="value"/> for <see cref="IsLooped"/>
        ///     and raises the PropertyChanged.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void InternalSetRequiresFloatingSeparator(bool value)
        {
            fRequiresFloatingSeparator = value;
            OnPropertyChanged("RequiresFloatingSeparator");
        }

        /// <summary>Stores the actual <paramref name="value"/> for <see cref="IsLooped"/>
        ///     and raises the PropertyChanged.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void InternalSetIsLooped(bool value)
        {
            fIsLooped = value;
            OnPropertyChanged("IsLooped");
            if (value)
            {
                FrontImage = "LoopLeft";
                BackImage = "LoopRight";
            }
            else
            {
                FrontImage = "OptionLeft";
                BackImage = "OptionRight";
            }
        }

        /// <summary>Stores the actual <paramref name="value"/> for the<see cref="JaStDev.HAB.Designer.FlowItemConditional.FloatingFlowSplits"/>
        ///     prop.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void InternalSetFloatingFlowSplits(bool value)
        {
            fFloatingFlowSplits = value;
            OnPropertyChanged("FloatingFlowSplits");
        }

        /// <summary>Stores the actual <paramref name="value"/> for<see cref="IsSelectionRequired"/> and raises the PropertyChanged.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private void InternalSetIsSelectionRequired(bool value)
        {
            fIsSelectionRequired = value;
            OnPropertyChanged("IsSelectionRequired");
        }

        /// <summary>called when a <paramref name="link"/> was removed or modified so that
        ///     this <see cref="EditorItem"/> no longer wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.FlowItemIsLoop)
            {
                InternalSetIsLooped(false);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.FlowItemRequiresSelection)
            {
                InternalSetIsSelectionRequired(false);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.RequiresFloatingSeparator)
            {
                InternalSetRequiresFloatingSeparator(false);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.FloatingFlowSplits)
            {
                InternalSetFloatingFlowSplits(false);
            }
            else
            {
                base.OutgoingLinkRemoved(link);
            }
        }

        /// <summary>called when a <paramref name="link"/> was created or modified so that
        ///     this <see cref="EditorItem"/> wraps the From part of the<paramref name="link"/></summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            if (link.MeaningID == (ulong)PredefinedNeurons.FlowItemIsLoop)
            {
                InternalSetIsLooped(link.ToID == (ulong)PredefinedNeurons.True);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.FlowItemRequiresSelection)
            {
                InternalSetIsSelectionRequired(link.ToID == (ulong)PredefinedNeurons.True);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.RequiresFloatingSeparator)
            {
                InternalSetRequiresFloatingSeparator(link.ToID == (ulong)PredefinedNeurons.True);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.FloatingFlowSplits)
            {
                InternalSetFloatingFlowSplits(link.ToID == (ulong)PredefinedNeurons.True);
            }
            else
            {
                base.OutgoingLinkCreated(link);
            }
        }

        /// <summary>Called when the <see cref="JaStDev.HAB.Designer.EditorItem.Item"/>
        ///     has changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.FlowItemIsLoop);
            InternalSetIsLooped(iFound != null && iFound.ID == (ulong)PredefinedNeurons.True);
            iFound = Item.FindFirstOut((ulong)PredefinedNeurons.FlowItemRequiresSelection);
            InternalSetIsSelectionRequired(iFound != null && iFound.ID == (ulong)PredefinedNeurons.True);
            iFound = Item.FindFirstOut((ulong)PredefinedNeurons.RequiresFloatingSeparator);
            InternalSetRequiresFloatingSeparator(iFound != null && iFound.ID == (ulong)PredefinedNeurons.True);
            iFound = Item.FindFirstOut((ulong)PredefinedNeurons.FloatingFlowSplits);
            InternalSetFloatingFlowSplits(iFound != null && iFound.ID == (ulong)PredefinedNeurons.True);
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.FlowPanel"/> object.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="panel">The panel.</param>
        /// <returns>The <see cref="FlowPanelItemBase"/>.</returns>
        protected internal override WPF.Controls.FlowPanelItemBase CreateDefaultUI(
            WPF.Controls.FlowPanelItemBase owner, 
            WPF.Controls.FlowPanel panel)
        {
            var iNew = new WPF.Controls.EnclosedFlowPanelItemList(owner, panel);
            iNew.Data = this;
            iNew.List.IsExpanded = true;
            iNew.List.ItemsSource = Items;
            iNew.List.Orientation = System.Windows.Controls.Orientation.Vertical;
            iNew.List.ListBackground = new System.Windows.Controls.Border();
            iNew.List.ListBackground.Style = panel.ConditionalBackgroundStyle;
            iNew.Front = new WPF.Controls.FlowItemConditionalFrontView();
            iNew.Back = new WPF.Controls.FlowItemConditionalBackView();
            return iNew;
        }

        #endregion
    }
}