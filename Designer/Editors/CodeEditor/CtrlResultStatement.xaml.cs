// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlResultStatement.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlResultStatement.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlResultStatement.xaml
    /// </summary>
    public partial class CtrlResultStatement : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlResultStatement"/> class.</summary>
        public CtrlResultStatement()
        {
            InitializeComponent();
        }

        #region Order

        /// <summary>The move up_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            e.CanExecute = iPage != null && iPage.CanMoveUpFor(iContext.Arguments);
        }

        /// <summary>The move down_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            e.CanExecute = iPage != null && iPage.CanMoveDownFor(iContext.Arguments);
        }

        /// <summary>The move up_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MoveUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            if (iPage != null)
            {
                iPage.MoveUpFor(iContext.Arguments);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>The move down_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MoveDown_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            if (iPage != null)
            {
                iPage.MoveDownFor(iContext.Arguments);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>The move to end_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MoveToEnd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            if (iPage != null)
            {
                iPage.MoveToEndFor(iContext.Arguments);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>The move to home_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MoveToHome_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iContext = (CodeItemResultStatement)DataContext;
            var iPage = iContext.Root as CodeEditorPage;
            if (iPage != null)
            {
                iPage.MoveToStartFor(iContext.Arguments);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        #endregion
    }
}