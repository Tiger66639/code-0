// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgOverlays.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgOverlays.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgOverlays.xaml
    /// </summary>
    public partial class DlgOverlays : System.Windows.Window
    {
        /// <summary>The f data.</summary>
        private System.Collections.ObjectModel.ObservableCollection<OverlayText> fData;

                                                                                 // we need an observable list so we can easely add/remove items to the list from a datagrid.

        /// <summary>Initializes a new instance of the <see cref="DlgOverlays"/> class.</summary>
        public DlgOverlays()
        {
            InitializeComponent();
            LoadData();
        }

        /// <summary>
        ///     Creates a copy of all the already defined overlay items and Loads this
        ///     into the datacontext.
        /// </summary>
        private void LoadData()
        {
            fData = new System.Collections.ObjectModel.ObservableCollection<OverlayText>();
            foreach (var iOr in BrainData.Current.Overlays)
            {
                var iNew = new OverlayText
                               {
                                   ItemID = iOr.ItemID, 
                                   OverlayColor = iOr.OverlayColor, 
                                   Text = iOr.Text, 
                                   Tooltip = iOr.Tooltip
                               };
                fData.Add(iNew);
            }

            DataContext = fData;
        }

        /// <summary>Called when [click ok].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            BrainData.Current.Overlays = new System.Collections.Generic.List<OverlayText>(fData); // we copy them back.
            DialogResult = true;
        }
    }
}