// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimatedImageFrame.cs" company="">
//   
// </copyright>
// <summary>
//   Descibes a single frame in an animated image. Can declare the duration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     Descibes a single frame in an animated image. Can declare the duration.
    /// </summary>
    public class AnimatedImageFrame : System.Windows.DependencyObject
    {
        #region Duration

        /// <summary>
        ///     Gets/sets the duration of the frame in milliseconds.
        /// </summary>
        public int Duration { get; set; }

        #endregion

        #region Bitmap

        /// <summary>
        ///     Gets/sets the bitmap to display.
        /// </summary>
        public System.Windows.Media.Imaging.BitmapSource Bitmap { get; set; }

        #endregion
    }
}