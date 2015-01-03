// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Annotation.cs" company="">
//   
// </copyright>
// <summary>
//   The annotation info of a lexical unit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     The annotation info of a lexical unit.
    /// </summary>
    public class Annotation : Data.ObservableObject
    {
        /// <summary>The f annotated.</summary>
        private int fAnnotated;

        /// <summary>The f total.</summary>
        private int fTotal;

        #region Annotated

        /// <summary>
        ///     Gets/sets the nr of annotated items.
        /// </summary>
        [System.Xml.Serialization.XmlElement("annotated", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Annotated
        {
            get
            {
                return fAnnotated;
            }

            set
            {
                OnPropertyChanging("Annotated", fAnnotated, value);
                fAnnotated = value;
                OnPropertyChanged("Annotated");
            }
        }

        #endregion

        #region Total

        /// <summary>
        ///     Gets/sets the <see cref="Total" /> nr of annotated items.
        /// </summary>
        [System.Xml.Serialization.XmlElement("total", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Total
        {
            get
            {
                return fTotal;
            }

            set
            {
                OnPropertyChanging("Total", fTotal, value);
                fTotal = value;
                OnPropertyChanged("Total");
            }
        }

        #endregion
    }
}