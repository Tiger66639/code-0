// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlResourceManager.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   A control that is able to manage a list of
//   <see cref="ResourceReference" /> objects. It is able to remove and add
//   items to the list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A control that is able to manage a list of
    ///     <see cref="ResourceReference" /> objects. It is able to remove and add
    ///     items to the list.
    /// </summary>
    public partial class CtrlResourceManager : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlResourceManager"/> class.</summary>
        public CtrlResourceManager()
        {
            InitializeComponent();
        }

        /// <summary>Handles the SelectionChanged event of the LstItems control.</summary>
        /// <remarks>Update if there is a selection or not.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void LstItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            HasSelection = LstItems.SelectedItems.Count > 0;
        }

        /// <summary>Handles the Executed event of the AddResource control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddResource_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.OpenFileDialog();
            iDlg.Multiselect = true;
            iDlg.Filter = DialogFilter;
            var iRes = iDlg.ShowDialog(System.Windows.Window.GetWindow(this));
            if (iRes.HasValue && iRes.Value)
            {
                var iItems = Items;
                System.Diagnostics.Debug.Assert(iItems != null);
                foreach (var i in iDlg.FileNames)
                {
                    var iNew = (ResourceReference)System.Activator.CreateInstance(ResourceType);
                    iNew.FileName = i;
                    iItems.Add(iNew);
                }
            }
        }

        /// <summary>The delete_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LstItems.SelectedItems.Count > 0;
        }

        /// <summary>The delete_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iItems = Items;
            foreach (var i in Enumerable.ToArray(Enumerable.OfType<ResourceReference>(LstItems.SelectedItems)))
            {
                // make a duplicate of the list cause we are about the modify it, which doesn't work in a foreach otherwise.
                iItems.Remove(i);
            }
        }

        /// <summary>The send resource_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SendResource_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iChannel = DataContext as CommChannel;
            if (iChannel != null)
            {
                var iRef = LstItems.SelectedItem as ResourceReference;
                iChannel.SendResource(iRef.FileName);
            }
        }

        /// <summary>The send all resources_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SendAllResources_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iChannel = DataContext as CommChannel;
            if (iChannel != null)
            {
                foreach (ResourceReference iRef in Items)
                {
                    iChannel.SendResource(iRef.FileName);
                }
            }
        }

        #region Items

        /// <summary>
        ///     <see cref="Items" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsProperty =
            System.Windows.DependencyProperty.Register(
                "Items", 
                typeof(System.Collections.IList), 
                typeof(CtrlResourceManager), 
                new System.Windows.FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the <see cref="Items" /> property. This dependency
        ///     property indicates the resource values to display.
        /// </summary>
        /// <remarks>
        ///     For best results, the list assigned should implement
        ///     <see cref="ICollectionChanged" /> .
        /// </remarks>
        public System.Collections.IList Items
        {
            get
            {
                return (System.Collections.IList)GetValue(ItemsProperty);
            }

            set
            {
                SetValue(ItemsProperty, value);
            }
        }

        #endregion

        #region ResourceType

        /// <summary>
        ///     <see cref="ResourceType" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ResourceTypeProperty =
            System.Windows.DependencyProperty.Register(
                "ResourceType", 
                typeof(System.Type), 
                typeof(CtrlResourceManager), 
                new System.Windows.FrameworkPropertyMetadata((System.Type)null));

        /// <summary>
        ///     Gets or sets the <see cref="ResourceType" /> property. This dependency
        ///     property indicates the type of resource that should be created. This
        ///     should be a descendent of <see cref="ResourceReference" /> .
        /// </summary>
        public System.Type ResourceType
        {
            get
            {
                return (System.Type)GetValue(ResourceTypeProperty);
            }

            set
            {
                SetValue(ResourceTypeProperty, value);
            }
        }

        #endregion

        #region HasSelection

        /// <summary>
        ///     <see cref="HasSelection" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty HasSelectionProperty =
            System.Windows.DependencyProperty.Register(
                "HasSelection", 
                typeof(bool), 
                typeof(CtrlResourceManager), 
                new System.Windows.FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets the <see cref="HasSelection" /> property. This dependency
        ///     property indicates wether there are items selected or not.
        /// </summary>
        public bool HasSelection
        {
            get
            {
                return (bool)GetValue(HasSelectionProperty);
            }

            set
            {
                SetValue(HasSelectionProperty, value);
            }
        }

        #endregion

        #region DialogFilter

        /// <summary>
        ///     <see cref="DialogFilter" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty DialogFilterProperty =
            System.Windows.DependencyProperty.Register(
                "DialogFilter", 
                typeof(string), 
                typeof(CtrlResourceManager), 
                new System.Windows.FrameworkPropertyMetadata((string)null));

        /// <summary>
        ///     Gets or sets the <see cref="DialogFilter" /> property. This dependency
        ///     property indicates the filter string to use by the OpenFileDialog for
        ///     adding new resources.
        /// </summary>
        public string DialogFilter
        {
            get
            {
                return (string)GetValue(DialogFilterProperty);
            }

            set
            {
                SetValue(DialogFilterProperty, value);
            }
        }

        #endregion
    }
}