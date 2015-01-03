// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SemType.cs" company="">
//   
// </copyright>
// <summary>
//   The sem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>The sem type.</summary>
    [System.Xml.Serialization.XmlType(TypeName = "semType", Namespace = "")]
    public class SemType : Data.ObservableObject
    {
        #region ID

        /// <summary>
        ///     Gets/sets the id of the semtype
        /// </summary>
        public int ID
        {
            get
            {
                return fID;
            }

            set
            {
                OnPropertyChanging("ID", fID, value);
                fID = value;
                OnPropertyChanged("ID");
            }
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the semtype
        /// </summary>
        public string Name
        {
            get
            {
                return fName;
            }

            set
            {
                OnPropertyChanging("Name", fName, value);
                fName = value;
                OnPropertyChanged("Name");
            }
        }

        #endregion

        #region fields

        /// <summary>The f id.</summary>
        private int fID;

        /// <summary>The f name.</summary>
        private string fName;

        #endregion
    }
}