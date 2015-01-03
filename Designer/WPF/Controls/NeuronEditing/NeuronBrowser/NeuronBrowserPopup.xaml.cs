// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronBrowserPopup.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for NeuronBrowserPopup.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Interaction logic for NeuronBrowserPopup.xaml
    /// </summary>
    public partial class NeuronBrowserPopup : System.Windows.Controls.Primitives.Popup
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronBrowserPopup"/> class.</summary>
        public NeuronBrowserPopup()
        {
            SelectedNeuron = null;
            ItemsSources = new BrowserDataSourceList(this);
            InitializeComponent();
        }

        #region ItemsSources

        /// <summary>
        ///     Gets the list of itemsSources to use for the drop down box.
        /// </summary>
        public BrowserDataSourceList ItemsSources { get; private set; }

        #endregion

        #region SelectedNeuron

        /// <summary>
        ///     Gets the selected neuron. Can't be assigned to, cause can't calculate
        ///     the position of the selected value.
        /// </summary>
        public Neuron SelectedNeuron { get; internal set; }

        #endregion

        /// <summary>Handles the SelectionChanged event of the ItemBrowser control. When
        ///     the user made a selection, we close the box again.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ItemBrowser_SelectionChanged(object sender, System.EventArgs e)
        {
            SelectedNeuron = ItemBrowser.SelectedNeuron;
            IsOpen = false;

                // important: close first, then set value. This is for the undo system: otherwise we have the wrong object focused when the undo system requests item in focus (it would return the drop-down, not the backing control).
        }

        /// <summary>Responds to the condition in which the value of the<see cref="System.Windows.Controls.Primitives.Popup.IsOpen"/>
        ///     property changes from <see langword="false"/> to true.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
            ItemBrowser.ItemsSources = ItemsSources;
            ItemBrowser.AttachItemsSources();
        }

        /// <summary>Responds when the value of the<see cref="System.Windows.Controls.Primitives.Popup.IsOpen"/>
        ///     property changes from to <see langword="true"/> to false.</summary>
        /// <param name="e">The event data.</param>
        protected override void OnClosed(System.EventArgs e)
        {
            ItemBrowser.DetachItemsSources();
            ItemBrowser.ItemsSources = null;
            base.OnClosed(e);
        }

        /// <summary>Handles the SelectionCanceled event of the ItemBrowser control. When
        ///     canceled, simply close the combobox.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ItemBrowser_SelectionCanceled(object sender, System.EventArgs e)
        {
            SelectedNeuron = null;
            IsOpen = false;
        }

        #region inner types

        /// <summary>
        ///     <para>
        ///         Has to be inline from a component cause it needs access to
        ///         RemoveLogicalChild and AddLogicalChild. Makes certain that all the
        ///         children are added/removed to the logical tree of the control. This
        ///         has to be an inline class, cause otherwise we can't call the
        ///         <see cref="System.Windows.FrameworkElement.AddLogicalChild" />
        ///     </para>
        ///     <para>and remove counterpart.</para>
        /// </summary>
        public class BrowserDataSourceList : System.Collections.ObjectModel.ObservableCollection<BrowserDataSource>
        {
            /// <summary>The f owner.</summary>
            private readonly NeuronBrowserPopup fOwner;

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class. Initializes a new instance of the<see cref="NeuronBrowserPopup.BrowserDataSourceList"/> class.</summary>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(NeuronBrowserPopup owner)
            {
                fOwner = owner;
            }

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class. Initializes a new instance of the<see cref="NeuronBrowserPopup.BrowserDataSourceList"/> class.</summary>
            /// <param name="source">The source.</param>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(System.Collections.Generic.IEnumerable<BrowserDataSource> source, 
                NeuronBrowserPopup owner)
                : base(source)
            {
                fOwner = owner;
            }

            /// <summary>Initializes a new instance of the <see cref="BrowserDataSourceList"/> class. Initializes a new instance of the<see cref="NeuronBrowserPopup.BrowserDataSourceList"/> class.</summary>
            /// <param name="source">The source.</param>
            /// <param name="owner">The owner.</param>
            public BrowserDataSourceList(System.Collections.Generic.List<BrowserDataSource> source, 
                NeuronBrowserPopup owner)
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

        #endregion
    }
}