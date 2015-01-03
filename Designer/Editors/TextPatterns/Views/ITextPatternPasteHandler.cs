// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITextPatternPasteHandler.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="interface" /> for objects that support pasting text
//   patterns (textPatternEditor and ChatbotProperties)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An <see langword="interface" /> for objects that support pasting text
    ///     patterns (textPatternEditor and ChatbotProperties)
    /// </summary>
    public interface ITextPatternPasteHandler
    {
        /// <summary>Pastes on an input pattern.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert At.</param>
        void PasteFromClipboardToList(InputPatternCollection list, InputPattern insertAt = null);

        /// <summary>The paste from clipboard to list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        void PasteFromClipboardToList(PatternOutputsCollection list, PatternEditorItem insertAt = null);

        /// <summary>The paste from clipboard to list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        void PasteFromClipboardToList(InvalidPatternResponseCollection list, PatternEditorItem insertAt = null);

        /// <summary>The can paste from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool CanPasteFromClipboard();
    }
}