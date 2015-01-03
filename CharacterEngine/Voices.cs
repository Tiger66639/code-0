// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Voices.cs" company="">
//   
// </copyright>
// <summary>
//   manages a list of voices.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     manages a list of voices.
    /// </summary>
    public class Voices : System.Collections.Generic.List<Voice>, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     Saves the voices to the settings applicationsData file (voices.xml)
        /// </summary>
        public void Save()
        {
            var iPath =
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), 
                    "NND");
            if (System.IO.Directory.Exists(iPath) == false)
            {
                System.IO.Directory.CreateDirectory(iPath);
            }
            {
                iPath = System.IO.Path.Combine(iPath, "Voices.xml");
                using (
                    var iFile = new System.IO.FileStream(
                        iPath, 
                        System.IO.FileMode.Create, 
                        System.IO.FileAccess.ReadWrite))
                {
                    var iSettings = CreateWriterSettings();
                    using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings)) WriteXml(iWriter);
                }
            }
        }

        /// <summary>The create writer settings.</summary>
        /// <returns>The <see cref="XmlWriterSettings"/>.</returns>
        private static System.Xml.XmlWriterSettings CreateWriterSettings()
        {
            var iSettings = new System.Xml.XmlWriterSettings();

            // iSettings.Indent = false;
            iSettings.Indent = true;
            iSettings.NewLineHandling = System.Xml.NewLineHandling.None;
            return iSettings;
        }

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
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

            while (reader.Name != "Voices" && reader.EOF == false)
            {
                ReadVoice(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read voice.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadVoice(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iNew = new Voice();
            Add(iNew);
            iNew.Name = ReadElement<string>(reader, "Name");
            iNew.DisplayName = ReadElement<string>(reader, "DisplayName");
            iNew.PreferedCharacter = ReadElement<string>(reader, "PreferedCharacter");
            iNew.SSMLEnabled = ReadElement<bool>(reader, "SSMLEnabled");
            iNew.SendFormatString = ReadElement<string>(reader, "SendFormatString");
            if (reader.Name == "Enabled")
            {
                // newly added.
                iNew.IsEnabled = ReadElement<bool>(reader, "Enabled");
            }

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Voices");
            foreach (var i in this)
            {
                WriteVoice(writer, i);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write voice.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="i">The i.</param>
        private void WriteVoice(System.Xml.XmlWriter writer, Voice i)
        {
            writer.WriteStartElement("Voice");
            WriteElement(writer, "Name", i.Name);
            WriteElement(writer, "DisplayName", i.DisplayName);
            WriteElement(writer, "PreferedCharacter", i.PreferedCharacter);
            WriteElement(writer, "SSMLEnabled", i.SSMLEnabled);
            WriteElement(writer, "SendFormatString", i.SendFormatString);
            WriteElement(writer, "Enabled", i.IsEnabled);
            writer.WriteEndElement();
        }

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

        #endregion

        #region build data

        /// <summary>Retrieves the available voices from unmanaged sapi</summary>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="Voices"/>.</returns>
        public static Voices RetrieveAvailableVoices(SpeechLib.ISpeechObjectTokens items)
        {
            var iRes = GetFromSettings();
            if (iRes == null)
            {
                iRes = new Voices();
            }

            try
            {
                var iNewNames = new System.Collections.Generic.List<string>();
                for (var i = 0; i < items.Count; i++)
                {
                    // make a copy of all the new names
                    iNewNames.Add(items.Item(i).GetAttribute("Name"));
                }

                RemoveOldVoices(iRes, iNewNames);
                AddNewVoices(iRes, iNewNames);
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    "Failed to load the available voices with error message: " + e, 
                    "Voices", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            return iRes;
        }

        /// <summary>The retrieve available voices.</summary>
        /// <param name="synth">The synth.</param>
        /// <returns>The <see cref="Voices"/>.</returns>
        internal static Voices RetrieveAvailableVoices(SpeechSynth.SpeechSynth synth)
        {
            var iRes = GetFromSettings();
            if (iRes == null)
            {
                iRes = new Voices();
            }

            try
            {
                var iNewNames = new System.Collections.Generic.List<string>();

                // IList<SynthVoice> iVoices = synth.GetVoices();
                // for (int i = 0; i < iVoices.Count; i++)                                //make a copy of all the new names
                // iNewNames.Add(iVoices[i].Name);
                RemoveOldVoices(iRes, iNewNames);
                AddNewVoices(iRes, iNewNames);
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    "Failed to load the available voices with error message: " + e, 
                    "Voices", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            return iRes;
        }

        /// <summary>Retrieves the available voices from managed sapi</summary>
        /// <param name="speaker">The speaker.</param>
        /// <returns>The <see cref="Voices"/>.</returns>
        public static Voices RetrieveAvailableVoices(System.Speech.Synthesis.SpeechSynthesizer speaker)
        {
            var iRes = GetFromSettings();
            if (iRes == null)
            {
                iRes = new Voices();
            }

            try
            {
                System.Collections.IList iVoices = Enumerable.ToList(speaker.GetInstalledVoices());
                RemoveOldVoices(iRes, iVoices);
                AddNewVoices(iRes, iVoices);
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    "Failed to load the available voices with error message: " + e, 
                    "Voices", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            return iRes;
        }

        /// <summary>Adds the new voices.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="iVoices">The i voices.</param>
        private static void AddNewVoices(Voices iRes, System.Collections.IList iVoices)
        {
            foreach (System.Speech.Synthesis.InstalledVoice i in iVoices)
            {
                if (i != null)
                {
                    var iNew = new Voice { Name = i.VoiceInfo.Name, DisplayName = i.VoiceInfo.Name };
                    iNew.SSMLEnabled = true;
                    iNew.SendFormatString = Voice.AvailableFormatStrings[0];
                    iNew.IsEnabled = true;
                    iRes.Add(iNew);
                }
            }
        }

        /// <summary>The add new voices.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="items">The items.</param>
        private static void AddNewVoices(Voices iRes, System.Collections.Generic.List<string> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var iNew = new Voice { Name = items[i], DisplayName = items[i] };
                iNew.SSMLEnabled = true;
                iNew.SendFormatString = Voice.AvailableFormatStrings[0];
                iNew.IsEnabled = true;
                iRes.Add(iNew);
            }
        }

        /// <summary>The remove old voices.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="items">The items.</param>
        private static void RemoveOldVoices(Voices iRes, System.Collections.Generic.List<string> items)
        {
            var i = 0;
            while (i < iRes.Count)
            {
                if (items.Remove(iRes[i].Name) == false)
                {
                    iRes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>The remove old voices.</summary>
        /// <param name="iRes">The i res.</param>
        /// <param name="iVoices">The i voices.</param>
        private static void RemoveOldVoices(Voices iRes, System.Collections.IList iVoices)
        {
            var i = 0;
            while (i < iRes.Count)
            {
                var iOld = iRes[i];
                var iFound = false;
                for (var u = 0; u < iVoices.Count; u++)
                {
                    var iNew = iVoices[u] as System.Speech.Synthesis.InstalledVoice;
                    if (iNew != null && iNew.VoiceInfo.Name == iOld.Name)
                    {
                        iFound = true;
                        iVoices[u] = null; // set to null, so we know it doesn't have to be added again.
                        break;
                    }
                }

                if (iFound == false)
                {
                    iRes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>The get from settings.</summary>
        /// <returns>The <see cref="Voices"/>.</returns>
        private static Voices GetFromSettings()
        {
            Voices iRes = null;
            var iPath =
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), 
                    "NND");
            if (System.IO.Directory.Exists(iPath))
            {
                iPath = System.IO.Path.Combine(iPath, "Voices.xml");
                if (System.IO.File.Exists(iPath))
                {
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var iSettings = new System.Xml.XmlReaderSettings();
                        iSettings.IgnoreComments = true;
                        iSettings.IgnoreWhitespace = true;
                        using (var iReader = System.Xml.XmlReader.Create(iStr, iSettings))
                        {
                            iRes = new Voices();
                            if (iReader.IsStartElement())
                            {
                                iRes.ReadXml(iReader);
                            }
                        }
                    }
                }
            }

            return iRes;
        }

        #endregion
    }
}