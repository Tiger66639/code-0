// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListBotPropDecl.cs" company="">
//   
// </copyright>
// <summary>
//   a bot property that wraps round a cluster and which can contain a list of
//   items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a bot property that wraps round a cluster and which can contain a list of
    ///     items.
    /// </summary>
    public class ListBotPropDecl : BaseBotPropDecl
    {
        /// <summary>The f values.</summary>
        private ListBotPropValues fValues;

        #region Values

        /// <summary>
        ///     Gets the list of values assigned to the bot prop.
        /// </summary>
        public ListBotPropValues Values
        {
            get
            {
                if (fValues == null)
                {
                    fValues = new ListBotPropValues(this, (NeuronCluster)Item);
                }

                return fValues;
            }
        }

        #endregion

        /// <summary>Writes the property's value to an XML stream.</summary>
        /// <param name="writer">The writer.</param>
        public override void WriteXmlValue(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(XmlElementName);
            if (Values.Count > 0)
            {
                foreach (var i in Values)
                {
                    if (i.Item.ID < (ulong)PredefinedNeurons.EndOfStatic)
                    {
                        XmlStore.WriteElement(writer, "static", i.Item.ID);
                    }
                    else
                    {
                        XmlStore.WriteElement(
                            writer, 
                            GetTypeName(i.Item), 
                            FixWhiteSpaceForWriting(BrainHelper.GetTextFrom(i.Item)));
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>checks the type of <paramref name="neuron"/> and returns a value that
        ///     can be used to 'rebuild' the value as correct as possible.</summary>
        /// <param name="neuron"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetTypeName(Neuron neuron)
        {
            if (neuron is TextNeuron)
            {
                // textneurons always need to remain text, othrerwise we can use 'value' . This is required for things liks spaces and enters.
                return "text";
            }

            return "value"; // this will use GetNeuronFor (and works on bools, compounds and text)
        }

        /// <summary>Reads the property's value from an XML stream.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXmlValue(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            Values.Clear(); // make certain that all the previous values were removed.
            var iElName = XmlElementName.ToLower();
            while (reader.EOF == false && reader.Name.ToLower() != iElName)
            {
                Neuron iItem;
                if (reader.Name == "static")
                {
                    iItem = Brain.Current[XmlStore.ReadElNoCase<ulong>(reader, "static")];
                }
                else if (reader.Name == "text")
                {
                    iItem = TextNeuron.GetFor(FixWhiteSpaceForReading(XmlStore.ReadElNoCase<string>(reader, "text")));
                }
                else
                {
                    iItem = BrainHelper.GetNeuronFor(XmlStore.ReadElNoCase<string>(reader, "value"));
                }

                var iNew = new ListBotPropValue(iItem);
                Values.Add(iNew);
            }

            reader.ReadEndElement();
        }

        /// <summary>replaces certain characters in the string with escape signs, so that
        ///     the xml streamer doesn't screw up the value. This is important for
        ///     whitespaces.</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string FixWhiteSpaceForWriting(string value)
        {
            return value.Replace("\n", "\\n").Replace(" ", "\\s").Replace("\t", "\\t").Replace("\r", "\\r");
        }

        /// <summary>Fixes the white space for reading.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string FixWhiteSpaceForReading(string value)
        {
            return value.Replace("\\n", "\n").Replace("\\s", " ").Replace("\\t", "\t").Replace("\\r", "\r");
        }
    }
}