// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceReference.cs" company="">
//   
// </copyright>
// <summary>
//   Stores the path and name of a file that can be used as training data for
//   the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Stores the path and name of a file that can be used as training data for
    ///     the brain.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(ImageReference))]
    [System.Xml.Serialization.XmlInclude(typeof(AudioReference))]
    public class ResourceReference : Data.OwnedObject
    {
        /// <summary>The f file name.</summary>
        private string fFileName;

        #region FileName

        /// <summary>
        ///     Gets/sets the name and path of the file.
        /// </summary>
        public string FileName
        {
            get
            {
                return fFileName;
            }

            set
            {
                fFileName = value;
                OnPropertyChanged("FileName");
            }
        }

        #endregion
    }
}