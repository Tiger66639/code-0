// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FERestrictionsView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for FERestrictionsView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for FERestrictionsView.xaml
    /// </summary>
    public partial class FERestrictionsView : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="FERestrictionsView"/> class.</summary>
        public FERestrictionsView()
        {
            InitializeComponent();
        }

        /// <summary>Handles the TiltWheel event of the ThesPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The<see cref="JaStDev.HAB.Designer.WPF.Controls.MouseTiltEventArgs"/>
        ///     instance containing the event data.</param>
        private void ThesPanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            var iThes = (FrameElement)DataContext;
            iThes.HorScrollPos += e.Tilt;
        }

        /// <summary>Handles the PreviewMouseDown event of the TreeItems control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event
        ///     data.</param>
        private void TreeItems_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource == ThesPanel)
            {
                // if we don't filter on this, each time the contextmenu opens, we loose the selected item, which we don't want.
                var iThes = (FrameElement)DataContext;
                if (iThes != null && iThes.SelectedRestriction != null)
                {
                    iThes.SelectedRestriction = null;
                }

                // iThes.SelectedItem = null;
                // if (fSelectedTrvItem != null)
                // fSelectedTrvItem.IsSelected = false;            //when the user clicks on the empty space of the treeview, we must unselect the item so that the commands work correctly.
            }
        }
    }
}