// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageReference.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="ResourceReference" /> for the <see cref="ImageChannel" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A <see cref="ResourceReference" /> for the <see cref="ImageChannel" /> .
    /// </summary>
    public class ImageReference : ResourceReference
    {
        #region Image

        /// <summary>
        ///     Gets the image as a bitmapresource.
        /// </summary>
        public System.Windows.Media.Imaging.BitmapSource Image
        {
            get
            {
                var iPath = FileName;
                if (string.IsNullOrEmpty(iPath) == false)
                {
                    return new System.Windows.Media.Imaging.BitmapImage(new System.Uri(iPath));
                }

                return null;
            }
        }

        #endregion
    }
}