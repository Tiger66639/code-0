// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlStatement.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for CtrlStatement.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for CtrlStatement.xaml
    /// </summary>
    public partial class CtrlStatement : CtrlEditorItem
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlStatement"/> class.</summary>
        public CtrlStatement()
        {
            InitializeComponent();
        }

        #region Order

        /// <summary>The move up_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
            e.CanExecute = iPage != null && iPage.CanMoveUpFor(iContext.Arguments);
        }

        /// <summary>The move down_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
            e.CanExecute = iPage != null && iPage.CanMoveDownFor(iContext.Arguments);
        }

        /// <summary>The move up_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void MoveUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
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
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
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
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
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
            var iContext = DataContext as CodeItemResultStatement;
            var iPage = iContext != null ? iContext.Root as CodeEditorPage : null;
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