// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgWarningsAndExceptions.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   for some reasong, the xaml wont bind to the settins class, so we use an
//   extra wrapper. Bummer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     for some reasong, the xaml wont bind to the settins class, so we use an
    ///     extra wrapper. Bummer.
    /// </summary>
    public class WarningsAndExceptions
    {
        /// <summary>Gets or sets a value indicating whether log neuron not found in long term mem.</summary>
        public bool LogNeuronNotFoundInLongTermMem
        {
            get
            {
                return Settings.LogNeuronNotFoundInLongTermMem;
            }

            set
            {
                Settings.LogNeuronNotFoundInLongTermMem = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log get cluster meaning invalid args.</summary>
        public bool LogGetClusterMeaningInvalidArgs
        {
            get
            {
                return Settings.LogGetClusterMeaningInvalidArgs;
            }

            set
            {
                Settings.LogGetClusterMeaningInvalidArgs = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log add child invalid args.</summary>
        public bool LogAddChildInvalidArgs
        {
            get
            {
                return Settings.LogAddChildInvalidArgs;
            }

            set
            {
                Settings.LogAddChildInvalidArgs = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log call save var not found.</summary>
        public bool LogCallSaveVarNotFound
        {
            get
            {
                return Settings.LogCallSaveVarNotFound;
            }

            set
            {
                Settings.LogCallSaveVarNotFound = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether error on invalid link remove.</summary>
        public bool ErrorOnInvalidLinkRemove
        {
            get
            {
                return Settings.ErrorOnInvalidLinkRemove;
            }

            set
            {
                Settings.ErrorOnInvalidLinkRemove = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log contains children no cluster.</summary>
        public bool LogContainsChildrenNoCluster
        {
            get
            {
                return Settings.LogContainsChildrenNoCluster;
            }

            set
            {
                Settings.LogContainsChildrenNoCluster = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log split to other call back.</summary>
        public bool LogSplitToOtherCallBack
        {
            get
            {
                return Settings.LogSplitToOtherCallBack;
            }

            set
            {
                Settings.LogSplitToOtherCallBack = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether log temp int or double.</summary>
        public bool LogTempIntOrDouble
        {
            get
            {
                return Settings.LogTempIntOrDouble;
            }

            set
            {
                Settings.LogTempIntOrDouble = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether check conditional.</summary>
        public bool CheckConditional
        {
            get
            {
                return Settings.CheckConditional;
            }

            set
            {
                Settings.CheckConditional = value;
            }
        }

        /// <summary>Gets or sets the duplicate pattern log method.</summary>
        public LogMethod DuplicatePatternLogMethod
        {
            get
            {
                return Settings.DuplicatePatternLogMethod;
            }

            set
            {
                Settings.DuplicatePatternLogMethod = value;
                Properties.Settings.Default.DuplicatePatternLogMethod = value;
            }
        }
    }

    /// <summary>
    ///     Interaction logic for DlgWarningsAndExceptions.xaml
    /// </summary>
    public partial class DlgWarningsAndExceptions : System.Windows.Window
    {
        /// <summary>The f data.</summary>
        private readonly WarningsAndExceptions fData = new WarningsAndExceptions();

        /// <summary>Initializes a new instance of the <see cref="DlgWarningsAndExceptions"/> class.</summary>
        public DlgWarningsAndExceptions()
        {
            DataContext = fData;
            InitializeComponent();
        }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateBindingSources(
                DataPanel, 
                System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, 
                System.Windows.Controls.Primitives.Selector.SelectedValueProperty);
            DialogResult = true;
        }

        /// <summary>Recursively processes a given dependency object and all its children,
        ///     and updates sources of all objects that use a binding expression on a
        ///     given property.</summary>
        /// <param name="obj">The dependency object that marks a starting point. This could be a
        ///     dialog window or a panel control that hosts bound controls.</param>
        /// <param name="properties">The properties to be updated if <paramref name="obj"/> or one of its
        ///     childs provide it along with a binding expression.</param>
        public static void UpdateBindingSources(
            System.Windows.DependencyObject obj, 
            params System.Windows.DependencyProperty[] properties)
        {
            foreach (var depProperty in properties)
            {
                // check whether the submitted object provides a bound property
                // that matches the property parameters
                var be = System.Windows.Data.BindingOperations.GetBindingExpression(obj, depProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }

            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                // process child items recursively
                var childObject = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                UpdateBindingSources(childObject, properties);
            }
        }
    }
}