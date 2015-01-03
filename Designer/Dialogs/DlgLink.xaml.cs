// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgLink.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgLink.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgLink.xaml
    /// </summary>
    public partial class DlgLink : System.Windows.Window
    {
        /// <summary>Initializes a new instance of the <see cref="DlgLink"/> class.</summary>
        public DlgLink()
        {
            InitializeComponent();
        }

        /// <summary><see cref="MindMapView"/> depends on the selectedItems from cmbFrom,
        ///     cmbTo, cmbMeaning to be valid.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            var iOk = false;
            if (CmbFrom.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please provide a valid 'from' value.");
            }
            else if (CmbTo.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please provide a valid 'to' value.");
            }
            else if (CmbMeaning.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please provide a valid 'meaning' value.");
            }
            else
            {
                iOk = true;
            }

            if (iOk)
            {
                DialogResult = true;
            }
        }

        #region ToList

        /// <summary>
        ///     <see cref="ToList" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ToListProperty =
            System.Windows.DependencyProperty.Register(
                "ToList", 
                typeof(System.Collections.Generic.IList<MindMapNeuron>), 
                typeof(DlgLink), 
                new System.Windows.FrameworkPropertyMetadata((System.Collections.Generic.IList<MindMapNeuron>)null));

        /// <summary>
        ///     Gets or sets the <see cref="ToList" /> property. This dependency
        ///     property indicates the list of mindmapneurons which the user can
        ///     select as to.
        /// </summary>
        public System.Collections.Generic.IList<MindMapNeuron> ToList
        {
            get
            {
                return (System.Collections.Generic.IList<MindMapNeuron>)GetValue(ToListProperty);
            }

            set
            {
                SetValue(ToListProperty, value);
            }
        }

        #endregion

        #region FromList

        /// <summary>
        ///     <see cref="FromList" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty FromListProperty =
            System.Windows.DependencyProperty.Register(
                "FromList", 
                typeof(System.Collections.Generic.IList<MindMapNeuron>), 
                typeof(DlgLink), 
                new System.Windows.FrameworkPropertyMetadata((System.Collections.Generic.IList<MindMapNeuron>)null));

        /// <summary>
        ///     Gets or sets the <see cref="FromList" /> property. This dependency
        ///     property indicates the list of mindmapneurons which the user can
        ///     select as from.
        /// </summary>
        public System.Collections.Generic.IList<MindMapNeuron> FromList
        {
            get
            {
                return (System.Collections.Generic.IList<MindMapNeuron>)GetValue(FromListProperty);
            }

            set
            {
                SetValue(FromListProperty, value);
            }
        }

        #endregion

        #region SelectedTo

        /// <summary>
        ///     <see cref="SelectedTo" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedToProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedTo", 
                typeof(MindMapNeuron), 
                typeof(DlgLink), 
                new System.Windows.FrameworkPropertyMetadata((MindMapNeuron)null));

        /// <summary>
        ///     Gets or sets the <see cref="SelectedTo" /> property. This dependency
        ///     property indicates the initial value, followed by the mind map neuron
        ///     that the user selected for to.
        /// </summary>
        public MindMapNeuron SelectedTo
        {
            get
            {
                return (MindMapNeuron)GetValue(SelectedToProperty);
            }

            set
            {
                SetValue(SelectedToProperty, value);
            }
        }

        #endregion

        #region SelectedFrom

        /// <summary>
        ///     <see cref="SelectedFrom" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedFromProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedFrom", 
                typeof(MindMapNeuron), 
                typeof(DlgLink), 
                new System.Windows.FrameworkPropertyMetadata((MindMapNeuron)null));

        /// <summary>
        ///     Gets or sets the <see cref="SelectedFrom" /> property. This dependency
        ///     property indicates the initial value, followed by the mind map neuron
        ///     that the user selected for from.
        /// </summary>
        public MindMapNeuron SelectedFrom
        {
            get
            {
                return (MindMapNeuron)GetValue(SelectedFromProperty);
            }

            set
            {
                SetValue(SelectedFromProperty, value);
            }
        }

        #endregion

        #region SelectedMeaning

        /// <summary>
        ///     <see cref="SelectedMeaning" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelectedMeaningProperty =
            System.Windows.DependencyProperty.Register(
                "SelectedMeaning", 
                typeof(Neuron), 
                typeof(DlgLink), 
                new System.Windows.FrameworkPropertyMetadata((Neuron)null));

        /// <summary>
        ///     Gets or sets the <see cref="SelectedMeaning" /> property. This
        ///     dependency property indicates the initial value, followed by the mm
        ///     neuron that the user selected as the meaning.
        /// </summary>
        public Neuron SelectedMeaning
        {
            get
            {
                return (Neuron)GetValue(SelectedMeaningProperty);
            }

            set
            {
                SetValue(SelectedMeaningProperty, value);
            }
        }

        #endregion
    }
}