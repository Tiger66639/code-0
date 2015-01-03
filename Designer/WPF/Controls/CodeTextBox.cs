// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeTextBox.cs" company="">
//   
// </copyright>
// <summary>
//   a specialized textbox that handles tabs correctly and inserts spaces in
//   the front of a newline, for easy code editing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a specialized textbox that handles tabs correctly and inserts spaces in
    ///     the front of a newline, for easy code editing.
    /// </summary>
    public class CodeTextBox : System.Windows.Controls.TextBox
    {
        /// <summary>Raises the <see cref="PreviewKeyDown"/> event.</summary>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab
                && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control)
                != System.Windows.Input.ModifierKeys.Control)
            {
                // a tab is replaced with spaces, for correct formatting accross editors. ctrl tab is skipped.
                var caretPosition = CaretIndex;
                if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Shift)
                {
                    // when shift is pressed, we simply want to jump back.
                    CaretIndex = caretPosition - Properties.Settings.Default.TabSize;
                }
                else
                {
                    var iLine = GetLineIndexFromCharacterIndex(caretPosition);
                    var iSpaceCount = Properties.Settings.Default.TabSize
                                      - ((caretPosition - GetCharacterIndexFromLineIndex(iLine))
                                         % Properties.Settings.Default.TabSize);

                        // we want a tab to always align at the same boundery, nomatter where we started the tab from: so a tab pos is always at 0,3,6,9,.. not 1,4,7,.. if started from 1.
                    var tab = new System.String(' ', iSpaceCount);
                    Text = Text.Insert(caretPosition, tab);
                    CaretIndex = caretPosition + iSpaceCount;
                }

                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                // when we press enter, insert spaces in the front of the newline. The nr of spaces is determined by the current line (before the enter was pressed).
                var caretPosition = CaretIndex;
                var iLine = GetLineIndexFromCharacterIndex(caretPosition);
                var iCurLine = GetLineText(iLine);
                var iSpaceCount = 0;
                var iToInsert = new System.Text.StringBuilder("\n");
                while (iCurLine.Length > iSpaceCount && char.IsWhiteSpace(iCurLine[iSpaceCount]))
                {
                    iSpaceCount++; // count the nr of leading spaces.
                    iToInsert.Append(" ");
                }

                Text = Text.Insert(caretPosition, iToInsert.ToString());
                CaretIndex = caretPosition + iToInsert.Length;
                e.Handled = true;
            }
        }
    }
}