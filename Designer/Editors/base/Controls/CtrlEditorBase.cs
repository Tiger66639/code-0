// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CtrlEditorBase.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for user controls that represent editors. It provides
//   default copy/paste/delete actions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A base class for user controls that represent editors. It provides
    ///     default copy/paste/delete actions.
    /// </summary>
    public class CtrlEditorBase : System.Windows.Controls.UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CtrlEditorBase"/> class.</summary>
        public CtrlEditorBase()
        {
            var iNew = new System.Windows.Input.CommandBinding(
                System.Windows.Input.ApplicationCommands.Copy, 
                Copy_Executed, 
                Copy_CanExecute);
            CommandBindings.Add(iNew);
            iNew = new System.Windows.Input.CommandBinding(
                System.Windows.Input.ApplicationCommands.Paste, 
                Paste_Executed, 
                Paste_CanExecute);
            CommandBindings.Add(iNew);
            iNew = new System.Windows.Input.CommandBinding(
                System.Windows.Input.ApplicationCommands.Cut, 
                Cut_Executed, 
                Cut_CanExecute);
            CommandBindings.Add(iNew);

            iNew = new System.Windows.Input.CommandBinding(
                App.PasteSpecialCmd, 
                PasteSpecial_Executed, 
                PasteSpecial_CanExecute);
            CommandBindings.Add(iNew);

            iNew = new System.Windows.Input.CommandBinding(
                System.Windows.Input.ApplicationCommands.Delete, 
                Delete_Executed, 
                Delete_CanExecute);
            CommandBindings.Add(iNew);

            iNew = new System.Windows.Input.CommandBinding(
                App.DeleteSpecialCmd, 
                DeleteSpecial_Executed, 
                DeleteSpecial_CanExecute);
            CommandBindings.Add(iNew);

            var iKey = new System.Windows.Input.KeyBinding(
                App.DeleteSpecialCmd, 
                System.Windows.Input.Key.Delete, 
                System.Windows.Input.ModifierKeys.Control);
            InputBindings.Add(iKey);
        }

        #region Copy

        /// <summary>Handles the Executed event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Copy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.CopyToClipboard();
        }

        /// <summary>Handles the CanExecute event of the Copy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Copy_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanCopyToClipboard();
        }

        #endregion

        #region Paste

        /// <summary>Handles the Executed event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Paste_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.PasteFromClipboard();
        }

        /// <summary>Handles the CanExecute event of the Paste control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Paste_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanPasteFromClipboard();
        }

        #endregion

        #region Cut

        /// <summary>Handles the Executed event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Cut_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.CutToClipboard();
        }

        /// <summary>Handles the CanExecute event of the Cut control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        protected void Cut_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanCutToClipboard();
        }

        #endregion

        #region PasteSpecial

        /// <summary>Handles the Executed event of the PasteSpecial control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteSpecial_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.PasteSpecialFromClipboard();
        }

        /// <summary>Handles the CanExecute event of the PasteSpecial control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void PasteSpecial_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanPasteSpecialFromClipboard();
        }

        #endregion

        #region Delete

        /// <summary>Handles the Executed event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.Delete();
        }

        /// <summary>Handles the CanExecute event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanDelete();
            e.Handled = true;
        }

        #endregion

        #region DeleteSpecial

        /// <summary>Handles the Executed event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteSpecial_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            System.Diagnostics.Debug.Assert(iEditor != null);
            iEditor.DeleteSpecial();
        }

        /// <summary>Handles the CanExecute event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void DeleteSpecial_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iEditor = (EditorBase)DataContext;
            e.CanExecute = iEditor != null && iEditor.CanDeleteSpecial();
        }

        #endregion
    }
}