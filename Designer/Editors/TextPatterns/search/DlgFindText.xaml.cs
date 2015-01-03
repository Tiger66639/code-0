// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgFindText.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgFindText.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Dialogs
{
    /// <summary>
    ///     Interaction logic for DlgFindText.xaml
    /// </summary>
    public partial class DlgFindText : System.Windows.Window
    {
        /// <summary>The f default.</summary>
        private static DlgFindText fDefault;

        /// <summary>Initializes a new instance of the <see cref="DlgFindText"/> class.</summary>
        public DlgFindText()
        {
            InitializeComponent();
        }

        #region Default

        /// <summary>
        ///     Gets the default search window. if there is none, one will be created
        ///     and shown.
        /// </summary>
        public static DlgFindText Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = new DlgFindText();
                    fDefault.Owner = WindowMain.Current;
                    fDefault.Show();
                }
                else
                {
                    fDefault.Activate();
                }

                return fDefault;
            }
        }

        #endregion

        /// <summary>Handles the Closed event of the Window control. When the window gets
        ///     closed, we must reset the 'default' reference.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            fDefault = null;
        }

        /// <summary>Handles the Executed event of the Close control. Used to close the
        ///     window with the escape key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Close_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Handles the Click event of the FindNext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void FindNext_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = DataContext as FindInTextData;
            if (iData != null)
            {
                iData.FindNext();
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>Handles the Click event of the FindAll control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void FindAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = DataContext as FindInTextData;
            if (iData != null)
            {
                iData.FindAll();
                Close(); // after a find-all it might be more usefull to close the window.
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>The replace all_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReplaceAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = DataContext as FindInTextData;
            if (iData != null)
            {
                iData.ReplaceAll();
                Close(); // after a find-all it might be more usefull to close the window.
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>The replace_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Replace_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iData = DataContext as FindInTextData;
            if (iData != null)
            {
                iData.Replace();
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }
    }
}