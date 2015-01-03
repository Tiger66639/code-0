// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameBase.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for frame data elements that have an ID, name, description,
//   date and SemTypes
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     Base class for frame data elements that have an ID, name, description,
    ///     date and SemTypes
    /// </summary>
    public class FrameBase : FrameCore
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FrameBase" /> class.
        /// </summary>
        public FrameBase()
        {
            SemTypes = new Data.ObservedCollection<SemType>(this);
        }

        #endregion

        #region Description (Definition)

        /// <summary>
        ///     Gets/sets the description of the item.
        /// </summary>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, 
            ElementName = "definition")]
        public string Description
        {
            get
            {
                return fDescription;
            }

            set
            {
                OnPropertyChanging("Description", fDescription, value);
                fDescription = value;
                OnPropertyChanged("Description");
            }
        }

        #endregion

        #region SemTypes

        /// <summary>
        ///     Gets the list of semantic types associated with this frame.
        /// </summary>
        [System.Xml.Serialization.XmlElement("semTypes")]
        public Data.ObservedCollection<SemType> SemTypes { get; set; }

        #endregion

        #region ID

        /// <summary>
        ///     Gets/sets the id of the frame.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
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
        ///     Gets/sets the name of the frame
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("name")]
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

        #region Date

        /// <summary>
        ///     Gets/sets the date that the frame was created
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("cDate")]
        public string Date
        {
            get
            {
                return fDate;
            }

            set
            {
                OnPropertyChanging("Date", fDate, value);
                fDate = value;
                OnPropertyChanged("Date");
            }
        }

        #endregion

        #region Fields

        /// <summary>The f id.</summary>
        private int fID;

        /// <summary>The f name.</summary>
        private string fName;

        /// <summary>The f description.</summary>
        private string fDescription;

        /// <summary>The f date.</summary>
        private string fDate;

        #endregion
    }
}