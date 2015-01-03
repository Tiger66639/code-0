// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogHistoryStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   provides streaming to and from xml files for the chatlog-history data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides streaming to and from xml files for the chatlog-history data.
    /// </summary>
    public class LogHistoryStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The f log history.</summary>
        private NeuronCluster fLogHistory;

        #region LogHistory

        /// <summary>
        ///     Gets the neuroncluster that represents the log history. This is a
        ///     quick ref, so we don't overtax the db.
        /// </summary>
        public NeuronCluster LogHistory
        {
            get
            {
                if (fLogHistory == null)
                {
                    fLogHistory = Brain.Current[(ulong)PredefinedNeurons.ConversationLogHistory] as NeuronCluster;
                }

                return fLogHistory;
            }
        }

        #endregion

        #region functions

        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        public static void Import(string filename)
        {
            var iStreamer = new LogHistoryStreamer();
            using (var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // XmlSerializer iSer = new XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iStreamer.ReadXml(iReader);
                    }
                }
            }
        }

        /// <summary>Exports the specified editor to an xml file with the specified name
        ///     and path.</summary>
        /// <param name="filename">The filename.</param>
        public static void Export(string filename)
        {
            var iStreamer = new LogHistoryStreamer();
            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                {
                    iWriter.WriteStartElement("ConversationHistory");
                    iStreamer.WriteXml(iWriter);
                    iWriter.WriteEndElement();
                }
            }
        }

        #endregion

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

            while (reader.Name != "ConversationHistory")
            {
                ReadLog(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read log.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadLog(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iNewLog = NeuronFactory.GetCluster();
            Brain.Current.Add(iNewLog);
            using (var iLog = LogHistory.ChildrenW) iLog.Add(iNewLog);
            var iTime = ReadTime(reader);
            Link.Create(iNewLog, iTime, (ulong)PredefinedNeurons.Time);
            while (reader.Name != "Log")
            {
                ReadLogItem(reader, iNewLog);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read log item.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iNewLog">The i new log.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReadLogItem(System.Xml.XmlReader reader, NeuronCluster iNewLog)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iName = reader.Name;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iLogItem = NeuronFactory.GetCluster();
            Brain.Current.Add(iLogItem);
            using (var iLog = iNewLog.ChildrenW) iLog.Add(iLogItem);
            if (iName == "user")
            {
                iLogItem.Meaning = (ulong)PredefinedNeurons.In;
            }
            else if (iName == "bot")
            {
                iLogItem.Meaning = (ulong)PredefinedNeurons.Out;
            }
            else
            {
                throw new System.InvalidOperationException("unknown log item in log history: " + iName);
            }

            var iContent = new System.Collections.Generic.List<Neuron>();
            while (reader.Name != iName)
            {
                iContent.Add(ReadTextNeuron(reader));
            }

            var iNew = Link.Create(iNewLog, iLogItem, (ulong)PredefinedNeurons.ConversationLogHistory);
            var iInfo = iNew.InfoW;
            iInfo.AddRange(iContent);
            reader.ReadEndElement();
        }

        #endregion

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            System.Collections.Generic.List<NeuronCluster> iToWrite;
            var iRoot = Brain.Current[(ulong)PredefinedNeurons.ConversationLogHistory] as NeuronCluster;
            System.Diagnostics.Debug.Assert(iRoot != null);
            using (var iChildren = iRoot.Children) iToWrite = iChildren.ConvertTo<NeuronCluster>();
            foreach (var i in iToWrite)
            {
                WriteLog(writer, i);
            }

            Factories.Default.CLists.Recycle(iToWrite);
        }

        /// <summary>The write log.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cluster">The cluster.</param>
        private void WriteLog(System.Xml.XmlWriter writer, NeuronCluster cluster)
        {
            writer.WriteStartElement("Log");
            var iTime = cluster.FindFirstOut((ulong)PredefinedNeurons.Time) as NeuronCluster;
            WriteTime(writer, iTime);
            System.Collections.Generic.List<NeuronCluster> iItems;
            using (var iChildren = cluster.Children) iItems = iChildren.ConvertTo<NeuronCluster>();
            foreach (var i in iItems)
            {
                WriteLogItem(writer, cluster, i);
            }

            writer.WriteEndElement();
            Factories.Default.CLists.Recycle(iItems);
        }

        /// <summary>The write log item.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="logItem">The log item.</param>
        private void WriteLogItem(System.Xml.XmlWriter writer, NeuronCluster cluster, NeuronCluster logItem)
        {
            if (logItem.Meaning == (ulong)PredefinedNeurons.In)
            {
                writer.WriteStartElement("user");
            }
            else
            {
                writer.WriteStartElement("bot");
            }

            var iLink = Link.Find(cluster, logItem, LogHistory);
            if (iLink != null)
            {
                System.Collections.Generic.List<Neuron> iItems;
                using (var iChildren = cluster.Children) iItems = iChildren.ConvertTo<Neuron>();
                foreach (var i in iItems)
                {
                    WriteTextNeuron(writer, i);
                }

                Factories.Default.NLists.Recycle(iItems);
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}