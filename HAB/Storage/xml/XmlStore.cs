// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlStore.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for <see cref="Neuron" /> s. This class allows us to load
//   neurons from disk using xml without knowing the exact root type (crapp
//   work around).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A wrapper class for <see cref="Neuron" /> s. This class allows us to load
    ///     neurons from disk using xml without knowing the exact root type (crapp
    ///     work around).
    /// </summary>
    /// <remarks>
    ///     .net doesn't handle inheritence very well while streaming from xml. You
    ///     can use the xmlElement attribute on 'Data', but that would mean we have
    ///     to know every possible neuron type in advance, which we don't. To solve
    ///     this, we first write the type of the class, than the neuron itself.
    /// </remarks>
    public class XmlStore : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     The value.
        /// </summary>
        public Neuron Data { get; set; }

        /// <summary>Reads an xml block from the specified reader. The block should be for
        ///     the specified element name. The result (content of the element), is
        ///     returned</summary>
        /// <remarks>this function can only be used for simple types</remarks>
        /// <typeparam name="T">The type of value that should be read.</typeparam>
        /// <param name="reader">The xmlreader to read the data from.</param>
        /// <param name="name">The name of the element to read.</param>
        /// <returns>The content of the element that was found, cast to the specified type.</returns>
        public static T ReadElement<T>(System.Xml.XmlReader reader, string name) where T : System.IConvertible
        {
            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement(name);
                var iVal = reader.ReadString();
                var iRes =
                    (T)System.Convert.ChangeType(iVal, typeof(T), System.Globalization.CultureInfo.InvariantCulture);

                    // the cultureInfo is important for correctly reading doubles from xml files.
                reader.ReadEndElement();
                return iRes;
            }

            reader.ReadStartElement(name);
            return default(T);
        }

        /// <summary>The read el no case.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/>.</returns>
        /// <exception cref="XmlException"></exception>
        public static T ReadElNoCase<T>(System.Xml.XmlReader reader, string name) where T : System.IConvertible
        {
            if (reader.Name.ToLower() == name.ToLower())
            {
                if (reader.IsEmptyElement == false)
                {
                    reader.ReadStartElement();
                    var iVal = reader.ReadString();
                    var iRes =
                        (T)System.Convert.ChangeType(iVal, typeof(T), System.Globalization.CultureInfo.InvariantCulture);

                        // the cultureInfo is important for correctly reading doubles from xml files.
                    reader.ReadEndElement();
                    return iRes;
                }

                reader.ReadStartElement(name);
                return default(T);
            }

            throw new System.Xml.XmlException(name + "expected");
        }

        /// <summary>Tries to read an xml block from the specified reader. The block
        ///     should be for the specified element name. The result (content of the
        ///     element), is returned. If the block is not found, nothing is done.</summary>
        /// <remarks>this function can only be used for simple types</remarks>
        /// <typeparam name="T">The type of value that should be read.</typeparam>
        /// <param name="reader">The xmlreader to read the data from.</param>
        /// <param name="name">The name of the element to read.</param>
        /// <param name="val">The value that was read. This is a return value.</param>
        /// <returns><see langword="true"/> when there was an element of the specified
        ///     name, otherwise false.</returns>
        public static bool TryReadElement<T>(System.Xml.XmlReader reader, string name, ref T val)
            where T : System.IConvertible
        {
            if (reader.Name == name)
            {
                if (reader.IsEmptyElement == false)
                {
                    reader.ReadStartElement(name);
                    var iVal = reader.ReadString();
                    var iRes =
                        (T)System.Convert.ChangeType(iVal, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                    reader.ReadEndElement();
                    val = iRes;
                }
                else
                {
                    reader.ReadStartElement(name);
                    val = default(T);
                }

                return true;
            }

            return false;
        }

        /// <summary>Tries to read an xml block from the specified<paramref name="reader"/> where the element content is an<see langword="enum"/> value. The block should be for the specified
        ///     element name. The result (content of the element), is returned. If
        ///     the block is not found, nothing is done.</summary>
        /// <remarks>this function can only be used for simple types</remarks>
        /// <typeparam name="T">The type of value that should be read.</typeparam>
        /// <param name="reader">The xmlreader to read the data from.</param>
        /// <param name="name">The name of the element to read.</param>
        /// <param name="val">The value that was read. This is a return value.</param>
        /// <returns><see langword="true"/> when there was an element of the specified
        ///     name, otherwise false.</returns>
        public static bool TryReadEnum<T>(System.Xml.XmlReader reader, string name, ref T val)
            where T : System.IConvertible
        {
            if (reader.Name == name)
            {
                if (reader.IsEmptyElement == false)
                {
                    reader.ReadStartElement(name);
                    var iVal = reader.ReadString();
                    var iRes = (T)System.Enum.Parse(typeof(T), iVal);
                    reader.ReadEndElement();
                    val = iRes;
                }
                else
                {
                    reader.ReadStartElement(name);
                    val = default(T);
                }

                return true;
            }

            return false;
        }

        /// <summary>Writes the specified <paramref name="value"/> to the xml<paramref name="writer"/> using the specified element to surround it.</summary>
        /// <typeparam name="T">The type of the <paramref name="value"/> that needs to be saved.</typeparam>
        /// <param name="writer">The xml writer to save the <paramref name="value"/> to.</param>
        /// <param name="name">The name of the xml element that surrounds the value.</param>
        /// <param name="value">The value to save.</param>
        public static void WriteElement<T>(System.Xml.XmlWriter writer, string name, T value)
            where T : System.IConvertible
        {
            writer.WriteStartElement(name);
            if (value != null && value.Equals(default(T)) == false)
            {
                // important for strings: if null: exception.
                writer.WriteString(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    // the culture info is important to write doubles correctly.
            }

            writer.WriteEndElement();
        }

        /// <summary>Reads a list of sub elements surroundeed by the specified element
        ///     name. Each sub element is read using the callback, that has an extra<see langword="bool"/> arg.</summary>
        /// <param name="reader">The xmlreader to use.</param>
        /// <param name="name">The name of the element that surrounds the list.</param>
        /// <param name="action">The action to call each time a sub element is found.</param>
        /// <param name="extraArg">An extra argument, passed to the <paramref name="action"/></param>
        /// <param name="extrArg2">The extr Arg 2.</param>
        public static void ReadList(
            System.Xml.XmlReader reader, 
            string name, System.Action<System.Xml.XmlReader, System.Collections.Generic.List<Link>, bool> action, System.Collections.Generic.List<Link> extraArg, 
            bool extrArg2)
        {
            var iIsEmpty = reader.IsEmptyElement;
            reader.ReadStartElement(name);
            if (iIsEmpty == false)
            {
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    action(reader, extraArg, extrArg2);
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read list.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        public static void ReadList(
            System.Xml.XmlReader reader, 
            string name, System.Action<System.Xml.XmlReader> action)
        {
            var iIsEmpty = reader.IsEmptyElement;
            reader.ReadStartElement(name);
            if (iIsEmpty == false)
            {
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    action(reader);
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Reads a <paramref name="list"/> of <see langword="ulong"/> id's where
        ///     the <paramref name="list"/> is surrounded by the specified element<paramref name="name"/> and each item in the <paramref name="list"/>
        ///     is called 'ID'.</summary>
        /// <param name="reader">The xml reader that should be used.</param>
        /// <param name="name">The name of the element that surrounds the list.</param>
        /// <param name="list">The list that stores the result ulongs.</param>
        public static void ReadIDList(
            System.Xml.XmlReader reader, 
            string name, System.Collections.Generic.IList<ulong> list)
        {
            // extra info of link.
            var iIsEmpty = reader.IsEmptyElement;
            reader.ReadStartElement(name);
            if (iIsEmpty == false)
            {
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("ID");
                    var iVal = reader.ReadString();
                    reader.ReadEndElement();
                    var iID = ulong.Parse(iVal);
                    list.Add(iID);
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Writes the <paramref name="list"/> to the stream using the specified
        ///     name, each <see langword="ulong"/> is written using the 'ID' tag.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name.</param>
        /// <param name="list">The list.</param>
        public static void WriteIDList(
            System.Xml.XmlWriter writer, 
            string name, System.Collections.Generic.IList<ulong> list)
        {
            writer.WriteStartElement(name);
            if (list != null)
            {
                foreach (var i in list)
                {
                    writer.WriteStartElement("ID");
                    writer.WriteString(i.ToString());
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes the id of a <paramref name="neuron"/> to the xml file, using
        ///     the specified <paramref name="name"/> as the element.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name.</param>
        /// <param name="neuron">The neuron.</param>
        public static void WriteNeuron(System.Xml.XmlWriter writer, string name, Neuron neuron)
        {
            writer.WriteStartElement(name);
            if (neuron != null)
            {
                writer.WriteString(neuron.ID.ToString());
            }
            else
            {
                writer.WriteString(Neuron.EmptyId.ToString());
            }

            writer.WriteEndElement();
        }

        /// <summary>Reads a neuron from the xml file as an element with the specified
        ///     name.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron ReadNeuron(System.Xml.XmlReader reader, string name)
        {
            var iId = ReadElement<ulong>(reader, name);
            if (iId != Neuron.EmptyId)
            {
                return Brain.Current[iId];
            }

            return null;
        }

        /// <summary>skips any white space in the xml reader.</summary>
        /// <param name="reader"></param>
        public static void SkipSpaces(System.Xml.XmlReader reader)
        {
            while (reader.NodeType == System.Xml.XmlNodeType.Whitespace)
            {
                reader.Read();
            }
        }

        #region IXmlSerializable Members

        /// <summary>The get schema.</summary>
        /// <returns>The <see cref="XmlSchema"/>.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            reader.ReadStartElement("Type");
            var iFound = reader.ReadString();
            var iType = Brain.Current.GetNeuronType(iFound);
            if (iType == null)
            {
                foreach (var i in Settings.SinAssemblies)
                {
                    iType = i.GetType(iFound, false);
                    if (iType != null)
                    {
                        break;
                    }
                }
            }

            reader.ReadEndElement();

            var valueSerializer = new System.Xml.Serialization.XmlSerializer(iType);
            Data = (Neuron)valueSerializer.Deserialize(reader);

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Type");
            writer.WriteString(Data.GetType().ToString());
            writer.WriteEndElement();

            var valueSerializer = new System.Xml.Serialization.XmlSerializer(Data.GetType());
            valueSerializer.Serialize(writer, Data);
        }

        #endregion
    }
}