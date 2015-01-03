// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisemeNote.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a viseme event for an <see cref="AudioNote" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>
    ///     Represents a viseme event for an <see cref="AudioNote" />
    /// </summary>
    public class VisemeNote
    {
        #region Viseme

        /// <summary>
        ///     Gets/sets the viseme to play
        /// </summary>
        public int Viseme { get; set; }

        #endregion

        #region NextViseme

        /// <summary>
        ///     Gets/sets the next viseme that should be played.
        /// </summary>
        public int NextViseme { get; set; }

        #endregion

        #region Duration

        /// <summary>
        ///     Gets/sets the duration of the viseme (important for the last.
        /// </summary>
        public long Duration { get; set; }

        #endregion

        #region Emphasis

        /// <summary>
        ///     Gets/sets the emphasis to use.
        /// </summary>
        public System.Speech.Synthesis.SynthesizerEmphasis Emphasis { get; set; }

        #endregion

        #region MilliSeconds

        /// <summary>
        ///     Gets/sets the nr of milliseconds between this and the previous viseme
        /// </summary>
        public long Ticks { get; set; }

        #endregion
    }
}