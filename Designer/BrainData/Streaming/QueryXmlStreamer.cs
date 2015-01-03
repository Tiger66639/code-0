// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   Provides steaming support to and from xml files for queries
//   (query-editors).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Provides steaming support to and from xml files for queries
    ///     (query-editors).
    /// </summary>
    public class QueryXmlStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f data source.</summary>
        private QueryEditor fDataSource;

        /// <summary>The f reader.</summary>
        private System.Xml.XmlReader fReader;

        /// <summary>The fwriter.</summary>
        private System.Xml.XmlWriter fwriter;

        /// <summary>Imports the specified file name as a queryEditor and returns the just
        ///     imported object. This is not yet added to the projectManager. The
        ///     query will be compiled when possible.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="QueryEditor"/>.</returns>
        public static QueryEditor Import(string fileName)
        {
            var iStreamer = new QueryXmlStreamer();
            using (var iFile = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iStreamer.ReadXml(iReader);
                    }
                }
            }

            return iStreamer.fDataSource;
        }

        /// <summary>exports the <paramref name="editor"/> to the specified filename.</summary>
        /// <param name="editor"></param>
        /// <param name="fileName"></param>
        public static void Export(QueryEditor editor, string fileName)
        {
            var iStreamer = new QueryXmlStreamer();
            iStreamer.fDataSource = editor;
            using (
                var iFile = new System.IO.FileStream(
                    fileName, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                {
                    iWriter.WriteStartElement("Query");
                    iStreamer.WriteXml(iWriter);
                    iWriter.WriteEndElement();
                }
            }
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

        #region read

        /// <summary>reads and returns a query editor from inside another xml file.</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="QueryEditor"/>.</returns>
        public QueryEditor ReadQuery(System.Xml.XmlReader reader)
        {
            ReadXml(reader);
            return fDataSource;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            WindowMain.UndoStore.UndoStateStack.Push(UndoSystem.UndoState.Blocked);

                // we block the undo system cause we might have to delete some pattern editors/assets from objects that already existed.
            try
            {
                fReader = reader;
                var iQuery = new Queries.Query();
                Brain.Current.Add(iQuery);
                fDataSource = new QueryEditor(iQuery);
                fDataSource.IsOpen = true;
                try
                {
                    fDataSource.Name = XmlStore.ReadElement<string>(reader, "Name");
                    if (string.IsNullOrEmpty(fDataSource.Name))
                    {
                        fDataSource.Name = "query";
                    }

                    ReadInfo(reader, fDataSource.NeuronInfo);
                    fDataSource.Source = XmlStore.ReadElement<string>(reader, "Source");

                        // this field is written as cdata, but we should be able to read it as a regular element.
                    fDataSource.IsRunningResult = XmlStore.ReadElement<bool>(reader, "RunningResult");
                    ReadAttachedFiles();
                    ReadDataSource();
                    ReadRenderTarget();
                    ReadColumns();
                    reader.ReadEndElement();
                }
                catch
                {
                    // if there is an error at this stage, make certain that the query object is destroyed, cause there will be no editor added to the designer. this is the only db created object up untill this point, so this isok
                    Brain.Current.Delete(iQuery);
                    throw;
                }

                fDataSource.Compile(); // try to compile the source.
            }
            finally
            {
                WindowMain.UndoStore.UndoStateStack.Pop();
            }
        }

        /// <summary>The read render target.</summary>
        private void ReadRenderTarget()
        {
            if (fReader.Name == "RenderTarget")
            {
                var iName = XmlStore.ReadElement<string>(fReader, "RenderTarget").ToLower();
                fDataSource.SelectedRenderTarget =
                    (from i in QueryEditor.RenderTargets where i.Name.ToLower() == iName select i).FirstOrDefault();

                    // get the datasource.
                if (fDataSource.SelectedRenderTarget != null)
                {
                    var wasEmpty = fReader.IsEmptyElement;
                    fReader.ReadStartElement("RenderTargetSettings");
                    if (wasEmpty == false)
                    {
                        fDataSource.SelectedRenderTarget.ReadXml(fReader, fDataSource); // read the extra data.
                        fReader.ReadEndElement();
                    }
                }
                else
                {
                    fReader.ReadOuterXml(); // skipt the entire settings node
                }
            }
        }

        /// <summary>
        ///     reads all the columns if there were any.
        /// </summary>
        private void ReadColumns()
        {
            if (fReader.Name == "Columns")
            {
                var wasEmpty = fReader.IsEmptyElement;
                if (wasEmpty)
                {
                    return;
                }

                fReader.ReadStartElement();

                while (fReader.Name != "Columns")
                {
                    var iNew = new QueryColumn();
                    fDataSource.Columns.Add(iNew);
                    iNew.Name = fReader.GetAttribute("Name");
                    var iVal = fReader.GetAttribute("Index");
                    int iIndex;
                    if (int.TryParse(iVal, out iIndex))
                    {
                        iNew.Index = iIndex;
                    }
                    else
                    {
                        iNew.Index = fDataSource.ColumnCount;

                            // try to assign a value if we can't parse the index to an int.
                    }

                    double iWidth;
                    iVal = fReader.GetAttribute("Width");
                    if (double.TryParse(iVal, out iWidth))
                    {
                        iNew.Width = iWidth;
                    }

                    fReader.Read(); // a column doesn't havae any children, so go to next line.
                }

                fReader.ReadEndElement();
            }
        }

        /// <summary>
        ///     reads the data source (if ther is any) and it's extra info.
        /// </summary>
        private void ReadDataSource()
        {
            if (fReader.Name == "DataSource")
            {
                var iName = XmlStore.ReadElement<string>(fReader, "DataSource").ToLower();
                fDataSource.SelectedDataSource =
                    (from i in QueryEditor.DataSources where i.Name.ToLower() == iName select i).FirstOrDefault();

                    // get the datasource.
                if (fDataSource.SelectedDataSource != null)
                {
                    var wasEmpty = fReader.IsEmptyElement;
                    fReader.ReadStartElement("DataSourceSettings");
                    if (wasEmpty == false)
                    {
                        fDataSource.SelectedDataSource.ReadXml(fReader, fDataSource); // read the extra data.
                        fReader.ReadEndElement();
                    }
                }
                else
                {
                    fReader.ReadOuterXml(); // skipt the entire settings node
                }
            }
        }

        /// <summary>The read attached files.</summary>
        private void ReadAttachedFiles()
        {
            while (fReader.Name == "ExternalSource")
            {
                var iFile = XmlStore.ReadElement<string>(fReader, "ExternalSource");
                fDataSource.AdditionalFiles.Add(iFile);
            }
        }

        #endregion

        #region write

        /// <summary>Can be used to stream a query inside another xml file.</summary>
        /// <param name="writer"></param>
        /// <param name="editor"></param>
        public void WriteQuery(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            fDataSource = editor;
            fwriter = writer;
            writer.WriteStartElement("Query");
            WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            fwriter = writer;
            XmlStore.WriteElement(writer, "Name", fDataSource.Name);
            WriteInfo(writer, fDataSource.NeuronInfo);
            writer.WriteStartElement("Source");
            if (fDataSource.Source != null)
            {
                writer.WriteCData(fDataSource.Source); // the code is written as cdata, for best formatting perservation.
            }
            else
            {
                writer.WriteCData(string.Empty);
            }

            writer.WriteEndElement();
            XmlStore.WriteElement(fwriter, "RunningResult", fDataSource.IsRunningResult);
            WriteAttachedFiles();
            WriteDataSource();
            WriteRenderTarget();
            WriteColumns();
        }

        /// <summary>The write render target.</summary>
        private void WriteRenderTarget()
        {
            if (fDataSource.SelectedRenderTarget != null)
            {
                XmlStore.WriteElement(fwriter, "RenderTarget", fDataSource.SelectedRenderTarget.Name);
                fwriter.WriteStartElement("RenderTargetSettings");

                    // always wrap the extra data in a block, so we can skip it if something went wrong with reading the datasoruce.
                fDataSource.SelectedRenderTarget.WriteXml(fwriter, fDataSource);
                fwriter.WriteEndElement();
            }
        }

        /// <summary>The write data source.</summary>
        private void WriteDataSource()
        {
            if (fDataSource.SelectedDataSource != null)
            {
                XmlStore.WriteElement(fwriter, "DataSource", fDataSource.SelectedDataSource.Name);
                fwriter.WriteStartElement("DataSourceSettings");

                    // always wrap the extra data in a block, so we can skip it if something went wrong with reading the datasoruce.
                fDataSource.SelectedDataSource.WriteXml(fwriter, fDataSource);
                fwriter.WriteEndElement();
            }
        }

        /// <summary>
        ///     writes all the column defs, if there are any.
        /// </summary>
        private void WriteColumns()
        {
            if (fDataSource.Columns.Count > 0)
            {
                fwriter.WriteStartElement("Columns");
                foreach (var i in fDataSource.Columns)
                {
                    fwriter.WriteStartElement("Column");
                    fwriter.WriteAttributeString("Name", i.Name);
                    fwriter.WriteAttributeString("Index", i.Index.ToString());
                    fwriter.WriteAttributeString("Width", i.Width.ToString());
                    fwriter.WriteEndElement();
                }

                fwriter.WriteEndElement();
            }
        }

        /// <summary>
        ///     writes the list of external files that are also used by the query.
        ///     This is a potential loss of data.
        /// </summary>
        private void WriteAttachedFiles()
        {
            foreach (var i in fDataSource.AdditionalFiles)
            {
                XmlStore.WriteElement(fwriter, "ExternalSource", i);
            }
        }

        #endregion

        #endregion
    }
}