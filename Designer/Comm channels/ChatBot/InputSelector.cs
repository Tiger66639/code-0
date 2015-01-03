// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputSelector.cs" company="">
//   
// </copyright>
// <summary>
//   groups the index nr and text together so that the user can select a line
//   and we can return the corresponding index.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     groups the index nr and text together so that the user can select a line
    ///     and we can return the corresponding index.
    /// </summary>
    public class InputSelectionItem
    {
        /// <summary>
        ///     the index of the text.
        /// </summary>
        public int Index { get; set; }

        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; }
    }

    /// <summary>
    ///     Provides functionality to block a thread while the user is asked which
    ///     input to select
    /// </summary>
    internal class InputSelector
    {
        /// <summary>The f selected.</summary>
        private int fSelected;

        /// <summary>The f selection block.</summary>
        private readonly System.Threading.ManualResetEvent fSelectionBlock = new System.Threading.ManualResetEvent(
            false);

                                                           // if the STT engine can't figure out which input to use, we need to ask the user. While we are doing this, the network needs to be blocked.

        /// <summary>Blocks the calling thread while the user can make a selection.</summary>
        /// <param name="list"></param>
        /// <returns>The <see cref="int"/>.</returns>
        internal int Request(System.Collections.Generic.List<InputSelectionItem> list)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new System.Action<System.Collections.Generic.List<InputSelectionItem>>(ShowDialog), 
                System.Windows.Threading.DispatcherPriority.Normal, 
                list);
            fSelectionBlock.WaitOne();
            return fSelected;
        }

        /// <summary>Shows the dialog box so the user can make a selection. This has to be
        ///     called from the Ui thread.</summary>
        /// <param name="items">The items.</param>
        private void ShowDialog(System.Collections.Generic.List<InputSelectionItem> items)
        {
            var iDlg = new SelectInputDlg(items);
            iDlg.Owner = WindowMain.Current; // so that it shows in the center of the app.
            iDlg.ShowDialog();
            fSelected = iDlg.SelectedIndex;
            fSelectionBlock.Set();
        }
    }
}