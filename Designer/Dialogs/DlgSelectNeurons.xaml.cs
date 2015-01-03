// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectNeurons.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgSelectNeurons.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Interaction logic for DlgSelectNeurons.xaml
    /// </summary>
    public partial class DlgSelectNeurons : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgSelectNeurons"/> class.</summary>
        /// <param name="values">The values.</param>
        /// <param name="selected">The selected.</param>
        public DlgSelectNeurons(System.Collections.Generic.IEnumerable<Neuron> values, System.Collections.Generic.IEnumerable<Neuron> selected)
        {
            var iSelected = Enumerable.ToList(selected);
            Items = (from i in values select new NeuronSelectItem(i, iSelected.Contains(i))).ToList();

                // populate the list.
            InitializeComponent();
        }

        /// <summary>Initializes a new instance of the <see cref="DlgSelectNeurons"/> class.</summary>
        /// <param name="values">The values.</param>
        /// <param name="selected">The selected.</param>
        public DlgSelectNeurons(System.Collections.Generic.IEnumerable<Neuron> values, Neuron selected)
        {
            Items = (from i in values select new NeuronSelectItem(i, i == selected)).ToList(); // populate the list.
            InitializeComponent();
        }

        #region Functions

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

        #region inner types

        /// <summary>
        ///     A simple wrapper which provides a selected property, so we know which
        ///     values to return.
        /// </summary>
        public class NeuronSelectItem : Data.ObservableObject
        {
            #region fields

            /// <summary>The f is selected.</summary>
            private bool fIsSelected;

            #endregion

            #region ctor

            /// <summary>Initializes a new instance of the <see cref="NeuronSelectItem"/> class.</summary>
            /// <param name="value">The value.</param>
            /// <param name="isSelected">The is selected.</param>
            public NeuronSelectItem(Neuron value, bool isSelected)
            {
                Item = value;
                IsSelected = isSelected;
            }

            #endregion

            #region IsSelected

            /// <summary>
            ///     Gets/sets if the item is selected or not.
            /// </summary>
            public bool IsSelected
            {
                get
                {
                    return fIsSelected;
                }

                set
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }

            #endregion

            #region Item

            /// <summary>
            ///     Gets the item that is wrapped.
            /// </summary>
            public Neuron Item { get; internal set; }

            #endregion

            #region NeuronInfo

            /// <summary>
            ///     Gets the info for the neuron.
            /// </summary>
            public NeuronData NeuronInfo
            {
                get
                {
                    return BrainData.Current.NeuronInfo[Item.ID];
                }

                // we uses the id to get the data, this always returns an object if valid neuron, the other getter doesn't.
            }

            #endregion
        }

        #endregion

        #region prop

        #region ItemTemplate

        /// <summary>
        ///     <see cref="ItemTemplate" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemTemplateProperty =
            System.Windows.DependencyProperty.Register(
                "ItemTemplate", 
                typeof(System.Windows.DataTemplate), 
                typeof(DlgSelectNeurons), 
                new System.Windows.FrameworkPropertyMetadata((System.Windows.DataTemplate)null));

        /// <summary>
        ///     Gets or sets the <see cref="ItemTemplate" /> property. This dependency
        ///     property indicates the template that should be used to display each
        ///     neuron that can be selected.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <example>
        ///     <code lang="xml" />
        /// </example>
        public System.Windows.DataTemplate ItemTemplate
        {
            get
            {
                return (System.Windows.DataTemplate)GetValue(ItemTemplateProperty);
            }

            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items to display
        /// </summary>
        public System.Collections.Generic.List<NeuronSelectItem> Items { get; private set; }

        #endregion

        #region SelectedValues

        /// <summary>
        ///     Gets the list of neurons that were selected by the user
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> SelectedValues
        {
            get
            {
                return from i in Items where i.IsSelected select i.Item;
            }
        }

        #endregion

        #endregion
    }
}