// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoolBotPropDecl.cs" company="">
//   
// </copyright>
// <summary>
//   Manages a bot's property that has a boolean value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages a bot's property that has a boolean value.
    /// </summary>
    public class BoolBotPropDecl : BaseBotPropDecl
    {
        #region Value

        /// <summary>
        ///     Gets/sets the value for the property
        /// </summary>
        public bool Value
        {
            get
            {
                return ((IntNeuron)Item).Value == 1;
            }

            set
            {
                if (value != Value)
                {
                    if (value)
                    {
                        NeuronInfo.DisplayTitle = "1";

                            // we use displayTitle cause this will also generate the undo data.
                    }
                    else
                    {
                        NeuronInfo.DisplayTitle = "0";
                    }

                    OnPropertyChanged("Value");
                }
            }
        }

        #endregion

        /// <summary>Writes the property's value to an XML stream.</summary>
        /// <param name="writer">The writer.</param>
        public override void WriteXmlValue(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, XmlElementName, Value);
        }

        /// <summary>Reads the property's value from an XML stream.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXmlValue(System.Xml.XmlReader reader)
        {
            Value = XmlStore.ReadElement<bool>(reader, XmlElementName);
        }
    }
}