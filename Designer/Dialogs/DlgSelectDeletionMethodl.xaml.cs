// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectDeletionMethodl.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgDeleteSpecial.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DlgDeleteSpecial.xaml
    /// </summary>
    public partial class DlgSelectDeletionMethod : System.Windows.Window
    {
        /// <summary>The f branch handling.</summary>
        private DeletionMethod fBranchHandling = DeletionMethod.Nothing;

        /// <summary>The f neuron deletion method.</summary>
        private DeletionMethod fNeuronDeletionMethod = DeletionMethod.Delete;

        /// <summary>Initializes a new instance of the <see cref="DlgSelectDeletionMethod"/> class.</summary>
        public DlgSelectDeletionMethod()
        {
            InitializeComponent();
        }

        #region NeuronDeletionMethod

        /// <summary>
        ///     Gets the deletion method that should be applied to the selected items
        ///     (Default is delete permantly).
        /// </summary>
        public DeletionMethod NeuronDeletionMethod
        {
            get
            {
                return fNeuronDeletionMethod;
            }

            internal set
            {
                fNeuronDeletionMethod = value;
            }
        }

        #endregion

        #region BranchHandling

        /// <summary>
        ///     Gets the way that the branches of the deleted neurons should be
        ///     handled (default is nothing).
        /// </summary>
        public DeletionMethod BranchHandling
        {
            get
            {
                return fBranchHandling;
            }

            internal set
            {
                fBranchHandling = value;
            }
        }

        #endregion

        /// <summary>The sub group_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SubGroup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == BtnNothing)
            {
                BranchHandling = DeletionMethod.Nothing;
            }
            else if (sender == BtnSubDeleteNoRef)
            {
                BranchHandling = DeletionMethod.DeleteIfNoRef;
            }
            else if (sender == BtnSubDelete)
            {
                BranchHandling = DeletionMethod.Delete;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>The delete group_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void DeleteGroup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (GrpSub != null)
            {
                GrpSub.IsEnabled = true;
            }

            if (sender == BtnRemove)
            {
                GrpSub.IsEnabled = false;
                NeuronDeletionMethod = DeletionMethod.Remove;
            }
            else if (sender == BtnDeleteNoRef)
            {
                NeuronDeletionMethod = DeletionMethod.DeleteIfNoRef;
            }
            else if (sender == BtnDelete)
            {
                NeuronDeletionMethod = DeletionMethod.Delete;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>The on click ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}