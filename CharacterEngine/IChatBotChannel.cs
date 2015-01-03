// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChatBotChannel.cs" company="">
//   
// </copyright>
// <summary>
//   This <see langword="interface" /> should be implemented by objects that
//   require animated speech.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>
    ///     This <see langword="interface" /> should be implemented by objects that
    ///     require animated speech.
    /// </summary>
    public interface IChatBotChannel
    {
        /// <summary>
        ///     Gets/sets the currently selected character that needs to be displayed.
        /// </summary>
        Character SelectedCharacter { get; set; }

        /// <summary>
        ///     Gets/sets if the chatbot is currently speaking or not. This is used to
        ///     determine if new idle events need to be triggered. note: don't use the
        ///     setter from the outside.
        /// </summary>
        bool IsSpeaking { get; set; }

        /// <summary>
        ///     Gets/sets the name of the currently selected voice that is used as
        ///     output.
        /// </summary>
        Voice SelectedVoice { get; set; }

        /// <summary>
        ///     Gets/sets the text currently typed in, but not yet sent to the
        ///     network. This allows us to remember the input text when the user has
        ///     changed tabs. Default value = 'Type here'.
        /// </summary>
        string InputText { get; set; }

        /// <summary>
        ///     Gets or sets the text that was recorded by SAPI, split up into it's
        ///     recorded words with weight.
        /// </summary>
        /// <value>
        ///     The recorded words.
        /// </value>
        System.Collections.Generic.IList<RecordResultGroup> RecordedWords { get; }

        /// <summary>stores the currently selected voice and raises the event, but doesn't
        ///     change the engine.</summary>
        /// <param name="value">The value.</param>
        void SetSelectedVoice(Voice value);

        /// <summary>
        ///     Resets the idle timer to a new time interval and starts it.
        /// </summary>
        void ResetIdleTimer();

        /// <summary>Selects the <paramref name="viseme"/> async.</summary>
        /// <param name="viseme">The viseme.</param>
        void SelectVisemeAsync(int viseme);

        /// <summary>Selects the bookmark. async.</summary>
        /// <param name="bookmark">The bookmark.</param>
        void SelectBookmarkAsync(string bookmark);
    }
}