// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CSVConduit.cs" company="">
//   
// </copyright>
// <summary>
//   a generic comma sperated values reader(conduit).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>
    ///     a generic comma sperated values reader(conduit).
    /// </summary>
    public class CSVConduit : BaseConduit
    {
        /// <summary>The f comment sign.</summary>
        private char fCommentSign = '#';

        /// <summary>The f escape sign.</summary>
        private char fEscapeSign = '\0';

        /// <summary>The f field count.</summary>
        private int fFieldCount;

        /// <summary>The f field separator.</summary>
        private char fFieldSeparator = ',';

        /// <summary>The f first line is header.</summary>
        private bool fFirstLineIsHeader = true;

        /// <summary>The f quotation sign.</summary>
        private char fQuotationSign = '"';

        /// <summary>The f reader.</summary>
        private LumenWorks.Framework.IO.Csv.CsvReader fReader;

        /// <summary>The f source.</summary>
        private System.IO.StreamReader fSource;

        /// <summary>The f field value mappings.</summary>
        private readonly System.Collections.Generic.List<StringMapItem> fFieldValueMappings =
            new System.Collections.Generic.List<StringMapItem>();

        #region FieldValueMappings

        /// <summary>
        ///     Gets the list of strings that need to be replaced with another value, in each field that is read from the file.
        /// </summary>
        public System.Collections.Generic.List<StringMapItem> FieldValueMappings
        {
            get
            {
                return fFieldValueMappings;
            }
        }

        #endregion

        #region QuotationSign

        /// <summary>
        ///     Gets/sets the char used to surround texts.
        /// </summary>
        public char QuotationSign
        {
            get
            {
                return fQuotationSign;
            }

            set
            {
                fQuotationSign = value;
            }
        }

        #endregion

        #region EscapeSign

        /// <summary>
        ///     Gets/sets the escape sign to use so that the quotation sign can be used within strings.
        ///     set to 0 for faster performacne (according to the csv lib).
        /// </summary>
        public char EscapeSign
        {
            get
            {
                return fEscapeSign;
            }

            set
            {
                fEscapeSign = value;
            }
        }

        #endregion

        #region CommentSign

        /// <summary>
        ///     Gets/sets the char to use for indicating comment values.
        /// </summary>
        public char CommentSign
        {
            get
            {
                return fCommentSign;
            }

            set
            {
                fCommentSign = value;
            }
        }

        #endregion

        #region FirstLineIsHeader

        /// <summary>
        ///     Gets/sets the value taht indicates if the first line of the cvs file contains the column names or is a data row.
        /// </summary>
        public bool FirstLineIsHeader
        {
            get
            {
                return fFirstLineIsHeader;
            }

            set
            {
                fFirstLineIsHeader = value;
            }
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether the conduit is open or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public override bool IsOpen
        {
            get
            {
                return fReader != null;
            }
        }

        /// <summary>provides a way to save any conduit specific data to the db, so the conduit can correcly be loaded from disk again.</summary>
        /// <param name="writer"></param>
        public override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            writer.Write((short)1); // version nr.
            writer.Write(fFirstLineIsHeader);
            writer.Write(FieldSeparator);
            writer.Write(QuotationSign);
            writer.Write(EscapeSign);
            writer.Write(CommentSign);
            writer.Write(fFieldValueMappings.Count);
            foreach (var i in fFieldValueMappings)
            {
                writer.Write(i.Original);
                writer.Write(i.ReplaceWith);
            }
        }

        /// <summary>provides a way to read the settings for the conduit from disk, so that it can be reused again with the previous
        ///     settings.</summary>
        /// <param name="reader"></param>
        public override void ReadV1(System.IO.BinaryReader reader)
        {
            base.ReadV1(reader);
            var iVer = reader.ReadInt16();
            if (iVer == 1)
            {
                fFirstLineIsHeader = reader.ReadBoolean();
                fFieldSeparator = reader.ReadChar();
                fQuotationSign = reader.ReadChar();
                fEscapeSign = reader.ReadChar();
                fCommentSign = reader.ReadChar();

                var iCount = reader.ReadInt32();
                while (iCount > 0)
                {
                    var iNew = new StringMapItem();
                    iNew.Original = reader.ReadString();
                    iNew.ReplaceWith = reader.ReadString();
                    fFieldValueMappings.Add(iNew);
                    iCount--;
                }
            }
        }

        /// <summary>asks the conduit to read it's settings from an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="reader"></param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            var iValue = string.Empty;
            var iBoolVal = false;
            var iCharVal = '\0';
            if (XmlStore.TryReadElement(reader, "FirstLineIsHeader", ref iBoolVal))
            {
                FirstLineIsHeader = iBoolVal;
            }

            if (XmlStore.TryReadElement(reader, "FieldSeparator", ref iCharVal))
            {
                FieldSeparator = iCharVal;
            }

            if (XmlStore.TryReadElement(reader, "QuotationSign", ref iCharVal))
            {
                QuotationSign = iCharVal;
            }

            if (XmlStore.TryReadElement(reader, "EscapeSign", ref iCharVal))
            {
                EscapeSign = iCharVal;
            }

            if (XmlStore.TryReadElement(reader, "CommentSign", ref iCharVal))
            {
                CommentSign = iCharVal;
            }

            while (reader.Name == "FieldValueMapping")
            {
                reader.ReadStartElement();
                var iNew = new StringMapItem();
                if (XmlStore.TryReadElement(reader, "Original", ref iValue))
                {
                    iNew.Original = iValue;
                }

                if (XmlStore.TryReadElement(reader, "ReplaceWith", ref iValue))
                {
                    iNew.ReplaceWith = iValue;
                }

                fFieldValueMappings.Add(iNew);
                reader.ReadEndElement();
            }
        }

        /// <summary>asks the conduit to write it's settings to an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="writer"></param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "FirstLineIsHeader", FirstLineIsHeader);
            XmlStore.WriteElement(writer, "FieldSeparator", FieldSeparator);
            XmlStore.WriteElement(writer, "QuotationSign", QuotationSign);
            XmlStore.WriteElement(writer, "EscapeSign", EscapeSign);
            XmlStore.WriteElement(writer, "CommentSign", CommentSign);

            foreach (var i in fFieldValueMappings)
            {
                writer.WriteStartElement("FieldValueMapping");
                XmlStore.WriteElement(writer, "Original", i.Original);
                XmlStore.WriteElement(writer, "ReplaceWith", i.ReplaceWith);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        ///     Gets a value to indicate if the process needs to be started with a location and destination or only a location
        ///     argument.
        /// </summary>
        /// <returns>
        ///     true or false
        /// </returns>
        public override bool NeedsDestination()
        {
            return false;
        }

        /// <summary>Opens the file at the specified location. So that data can be read by the <see cref="ICustomConduit.ReadLine"/>
        ///     function.
        ///     This is for a pull scenario.</summary>
        /// <param name="location">The location.</param>
        public override void Open(string location)
        {
            var iFile = new System.IO.FileStream(location, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            fSource = new System.IO.StreamReader(iFile);
            fReader = new LumenWorks.Framework.IO.Csv.CsvReader(
                fSource, 
                FirstLineIsHeader, 
                FieldSeparator, 
                QuotationSign, 
                EscapeSign, 
                CommentSign, 
                LumenWorks.Framework.IO.Csv.ValueTrimmingOptions.All);
            fFieldCount = fReader.FieldCount;
        }

        /// <summary>
        ///     Closes this instance.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (fSource != null)
            {
                fSource.Close();
            }

            fSource = null;
            fReader = null;
        }

        /// <summary>reads a single line of data from the file.
        ///     This is used together with <see cref="ICustomConduit.Opern"/> and  <see cref="ICustomConduit.Close"/></summary>
        /// <param name="result"></param>
        /// <returns>false if no more line could be read, otherwise true.</returns>
        public override bool ReadLine(System.Collections.Generic.List<Neuron> result)
        {
            var iProc = Processor.CurrentProcessor;
            if (CancelRequested)
            {
                return false;
            }

            if (fReader.ReadNextRecord())
            {
                for (var i = 0; i < fFieldCount; i++)
                {
                    var iField = GetField(i);
                    if (string.IsNullOrEmpty(iField.Trim()) == false)
                    {
                        result.Add(BrainHelper.GetNeuronFor(iField, Culture, iProc));
                    }
                    else
                    {
                        result.Add(Brain.Current[(ulong)PredefinedNeurons.Empty]);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>reads a field value and makes certain taht the text mappings are applied.</summary>
        /// <param name="index"></param>
        /// <returns>The <see cref="string"/>.</returns>
        protected string GetField(int index)
        {
            var iStr = new System.Text.StringBuilder(fReader[index]);
            foreach (var u in FieldValueMappings)
            {
                iStr.Replace(u.Original, u.ReplaceWith);
            }

            return iStr.ToString();
        }

        #region FieldSeparator

        /// <summary>
        ///     Gets the list of field separators that should be used during the parse of the file.
        /// </summary>
        public char FieldSeparator
        {
            get
            {
                return fFieldSeparator;
            }

            set
            {
                fFieldSeparator = value;
            }
        }

        /// <summary>
        ///     for binding, gets/sets if tab is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsTab
        {
            get
            {
                return fFieldSeparator == '\t';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = '\t';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if SemiColon is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsSemiColon
        {
            get
            {
                return fFieldSeparator == ';';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ';';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Comma is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsComma
        {
            get
            {
                return fFieldSeparator == ',';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ',';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Space is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsSpace
        {
            get
            {
                return fFieldSeparator == ' ';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ' ';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Pipe is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsPipe
        {
            get
            {
                return fFieldSeparator == '|';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = '|';
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     a class that determins which strings need to be replaced with which values in each field that is read from the
    ///     file.
    ///     This is for replacing things like \\t with \t
    /// </summary>
    public class StringMapItem
    {
        /// <summary>
        ///     Gets or sets the original text in the field.
        ///     ex: \\t
        /// </summary>
        /// <value>
        ///     The original.
        /// </value>
        public string Original { get; set; }

        /// <summary>
        ///     Gets or sets the string that the original should be replaced with.
        ///     ex: \t
        /// </summary>
        /// <value>
        ///     The replace with.
        /// </value>
        public string ReplaceWith { get; set; }
    }
}