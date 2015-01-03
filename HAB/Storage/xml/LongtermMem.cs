// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LongtermMem.cs" company="">
//   
// </copyright>
// <summary>
//   Implements the <see cref="ILongtermMem" /> interface. It saves the
//   neurons to xml files in the path specified by
//   <see cref="LongtermMem.NeuronsPath" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Implements the <see cref="ILongtermMem" /> interface. It saves the
    ///     neurons to xml files in the path specified by
    ///     <see cref="LongtermMem.NeuronsPath" />
    /// </summary>
    /// <remarks>
    ///     Neurons are saved in subdirectories, in groups of 4096 (2^12).
    ///     SubDirectories are also grouped with 4096 dirs together in 1
    ///     parent-subdir (and so on). This is done to keep the file system fast
    ///     enough in retrieving single files from dirs (when there are to many
    ///     files, it gets slow + some file systems have limitations, far lower than
    ///     the ulong.max amount of possible neurons).
    /// </remarks>
    [System.Xml.Serialization.XmlRoot("Memory", Namespace = "http://www.jastdev.com", IsNullable = false)]
    public class LongtermMem : ILongtermMem, System.Xml.Serialization.IXmlSerializable
    {
        #region DataPath

        /// <summary>
        ///     Gets/sets the path to the location where all the neurons are saved as
        ///     files.
        /// </summary>
        /// <remarks>
        ///     the filename is the ID, with extention 'xml'.
        /// </remarks>
        public string DataPath
        {
            get
            {
                return fDataPath;
            }

            set
            {
                if (value != fDataPath)
                {
                    fDataPath = value;
                }
            }
        }

        #endregion

        /// <summary>Instructs the LongTermMem to make the<see cref="ILongTermMem.DataPath"/> relative to the specified<paramref name="path"/> so that the info can be streamed together with
        ///     the project.</summary>
        /// <param name="path">The path.</param>
        public void MakePathRelativeTo(string path)
        {
            DataPath = PathUtil.RelativePathTo(
                PathUtil.VerifyPathEnd(System.IO.Path.GetDirectoryName(path)), 
                PathUtil.VerifyPathEnd(DataPath));

                // need to make it relative for saving so that the project can be moved.
        }

        /// <summary>Instructs the LongTermMem to make the <paramref name="path"/> absolute
        ///     so that the data can be loaded.</summary>
        /// <param name="path">The path to make the <see cref="ILongTermMem.DataPath"/> absolute
        ///     against.</param>
        /// <param name="readOnly">The read Only.</param>
        public void MakePathAbsoluteTo(string path, bool readOnly = false)
        {
            if (System.IO.Path.IsPathRooted(DataPath) == false)
            {
                var iPath = new System.Uri(DataPath, System.UriKind.Relative);
                var iTemp = new System.Uri(new System.Uri(path), iPath);
                DataPath = iTemp.LocalPath;
            }
        }

        /// <summary>Tries to load the neuron with the specified ID</summary>
        /// <remarks><para>If the <paramref name="id"/> can't be found <see langword="null"/> is
        ///         returned.</para>
        /// <para>the filename is the ID, with extention 'xml'. the path is defined in<see cref="LongtermMem.NeuronsPath"/></para>
        /// <para>errors are written to the log.</para>
        /// </remarks>
        /// <param name="id">The id of the neuron to load.</param>
        /// <returns>The object that was loaded or <see langword="null"/> if not found.</returns>
        public LinkResolverData GetNeuron(ulong id)
        {
            if (string.IsNullOrEmpty(DataPath) == false)
            {
                var iPath = GetPath(id);
                if (System.IO.File.Exists(iPath))
                {
                    var iRes = ReadFromFile(iPath);
                    return Neuron.LinkResolver.Default.GetDataFor(iRes);
                }

                if (Settings.LogNeuronNotFoundInLongTermMem)
                {
                    LogService.Log.LogWarning("LongtermMemory.GetNeuron", "File not found: " + iPath);
                }

                return null;
            }

            LogService.Log.LogError(
                "LongtermMemory.GetNeuron", 
                "Can't search for parked neurons, NeuronsPath not yet defined!");
            return null;
        }

        /// <summary>Tries to save the neuron to the long time memory.</summary>
        /// <remarks>This implementation of <see cref="ILongtermMem"/> stores the neuron
        ///     to an xml file.</remarks>
        /// <param name="toSave">The object to save.</param>
        public void SaveNeuron(Neuron toSave)
        {
            SaveNeuron(toSave, toSave.ID);
        }

        /// <summary>Tries to save the neuron to the long time memory, using the specified<paramref name="id"/> as a key.</summary>
        /// <remarks>Used for modules, so a neuron can be saved under a different<paramref name="id"/> to compact the data.</remarks>
        /// <param name="toSave">To save.</param>
        /// <param name="id">The id.</param>
        public void SaveNeuron(Neuron toSave, ulong id)
        {
            if (string.IsNullOrEmpty(DataPath) || System.IO.Directory.Exists(DataPath) == false)
            {
                throw new BrainException(
                    "Can't save the Neuron because there is no valid NeuronPath defined.  Please provide the proper setup info.");
            }

            if (toSave.ID == Neuron.EmptyId)
            {
                throw new BrainException(
                    "Can't save the Neuron because it has no valid ID.  Has it already been added to the Brain?");
            }

            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
            string iDir;
            var iPath = GetPath(DataPath, id, out iDir);

            if (System.IO.Directory.Exists(iDir) == false)
            {
                System.IO.Directory.CreateDirectory(iDir); // need to make certain that the subdir also exists.
            }

            using (System.IO.TextWriter iWriter = new System.IO.StreamWriter(iPath))
            {
                var iTemp = new XmlStore { Data = toSave };
                iSer.Serialize(iWriter, iTemp);
            }
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The object to remove</param>
        public void RemoveNeuron(Neuron toRemove)
        {
            if (toRemove != null)
            {
                RemoveNeuron(toRemove.ID);
            }
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The id of the neuron to remove.</param>
        public void RemoveNeuron(ulong toRemove)
        {
            if (string.IsNullOrEmpty(DataPath) == false)
            {
                var iPath = GetPath(toRemove);
                if (System.IO.File.Exists(iPath))
                {
                    // need to check if the file exists, cause the function throw an exception when the path doesn't exits (if the file only doesn't exist, it's ok, but the entire path needs to be retrievable).
                    System.IO.File.Delete(iPath);
                }
            }
        }

        /// <summary>Marks the specified id as free but doesn't delete any neurons. This is
        ///     used for cleaning up the database (2 id's returning the same neuron).</summary>
        /// <remarks>Don't do anything this is for the dabase version.</remarks>
        /// <param name="bad">The bad.</param>
        public void MarkAsFree(ulong bad)
        {
        }

        /// <summary>
        ///     Makes certain that everything is properly written to file and nothing
        ///     is left in the buffers anymore.
        /// </summary>
        /// <remarks>
        ///     Don't do anything this is for the dabase version.
        /// </remarks>
        public void Flush()
        {
        }

        /// <summary>Makes certain that everything is properly written to file and nothing
        ///     is left in the buffers anymore after saving a single neuron</summary>
        /// <remarks>Don't do anything this is for the dabase version.</remarks>
        /// <param name="id">the id of the neuron that just changed.</param>
        public void Flush(ulong id)
        {
        }

        /// <summary>
        ///     flushes the data without flushing any index files if this can be
        ///     avoided (when they are buffered in memory). Don't do anything.
        /// </summary>
        public void FlushFast()
        {
        }

        /// <summary>Retrieves a list of neurons that form the entry points</summary>
        /// <param name="type">The type for which to retrieve the data.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.Generic.IList<Neuron> GetEntryNeuronsFor(System.Type type)
        {
            return EntryNeuronsHelper.GetEntryNeuronsFor(DataPath, type);
        }

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified type.</summary>
        /// <param name="list">The list of neurons that need to be stored as entry points. Note that
        ///     these Neurons are already stored as regular neurons in the brain, so
        ///     you should only store the Id of the neurons.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        public void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<Neuron> list, System.Type type)
        {
            EntryNeuronsHelper.SaveEntryNeuronsFor(DataPath, list, type);
        }

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified type.</summary>
        /// <param name="list">The list of neurons that need to be stored as entry points. Note that
        ///     these Neurons are already stored as regular neurons in the brain, so
        ///     you should only store the Id of the neurons.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        public void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<ulong> list, System.Type type)
        {
            EntryNeuronsHelper.SaveEntryNeuronsFor(DataPath, list, type);
        }

        /// <summary>Gets all entry neurons for all the types. This is used to convert from
        ///     1 storage type to another.</summary>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.Generic.IList<EntryPoints> GetAllEntryNeurons()
        {
            return EntryNeuronsHelper.GetAllEntryNeurons(DataPath);
        }

        /// <summary><para>Gets all properties that are defined in the storage. This is used for
        ///         transfering from</para>
        /// <list type="number"><item><description>storage system to another one.</description></item>
        /// </list>
        /// </summary>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.Generic.IList<PropertyValue> GetAllProperties()
        {
            return PropertiesHelper.GetAllProperties(DataPath);
        }

        /// <summary>The get all properties.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.Generic.IList<PropertyValue> GetAllProperties(System.Type type)
        {
            return PropertiesHelper.GetAllProperties(DataPath, type);
        }

        /// <summary>Gets extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks><para>This is primarely used by sins to retrieve extra information from the
        ///         brain that they require for proper processing.</para>
        /// <para>The data should be stored in xml format.</para>
        /// </remarks>
        /// <typeparam name="T">The <paramref name="type"/> of the property value that should be read.</typeparam>
        /// <param name="type">The type for which to get the property value.</param>
        /// <param name="id">The id of the property value that should be retrieved.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public T GetProperty<T>(System.Type type, string id) where T : class
        {
            return PropertiesHelper.GetProperty<T>(DataPath, type, id);
        }

        /// <summary>Saves extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks>See<see cref="ILongtermMem.GetProperty``1(System.Type,System.String)"/>
        ///     for more info.</remarks>
        /// <param name="type">The type.</param>
        /// <param name="id">The id of the property <paramref name="value"/> that should be saved.</param>
        /// <param name="value">The value that needs to be saved.</param>
        public void SaveProperty(System.Type type, string id, object value)
        {
            PropertiesHelper.SaveProperty(DataPath, type, id, value);
        }

        /// <summary>Copies all the data to the specified location. This operation doesn't
        ///     effect the value of <see cref="JaStDev.HAB.ILongtermMem.DataPath"/> .</summary>
        /// <param name="loc">The location to copy the data to.</param>
        public void CopyTo(string loc)
        {
            CopyTo(DataPath, loc);
        }

        /// <summary>Determines whether the specified <paramref name="id"/> exists.</summary>
        /// <param name="id">The id to look for.</param>
        /// <returns><c>true</c> if the <paramref name="id"/> exists in the db; otherwise,<c>false</c> .</returns>
        public bool IsExistingID(ulong id)
        {
            if (string.IsNullOrEmpty(DataPath) == false)
            {
                var iPath = GetPath(id);
                return System.IO.File.Exists(iPath);
            }

            return false;
        }

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // don't do anything. Not required for xml. 
        }

        #endregion

        /// <summary>The read from file.</summary>
        /// <param name="iPath">The i path.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadFromFile(string iPath)
        {
            using (var iFile = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iBuffer = new System.IO.BufferedStream(iFile, Settings.DBBufferSize);
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                using (var iReader = new System.IO.StreamReader(iBuffer))
                {
                    var iTemp = iSer.Deserialize(iReader) as XmlStore;
                    if (iTemp != null)
                    {
                        return iTemp.Data;
                    }

                    return null;
                }
            }
        }

        /// <summary>Copies the entire content of <paramref name="source"/> to destination,
        ///     including subdirs.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        private void CopyTo(string source, string dest)
        {
            foreach (var i in System.IO.Directory.GetFiles(source))
            {
                // we copy all the files, don't filter on any file type so we make certain we have everything. (SVN stores in subdir which isn't copied over) -> subdirs of SVN gave problems while deleting the sandbox.
                System.IO.File.Copy(i, System.IO.Path.Combine(dest, System.IO.Path.GetFileName(i)));
            }

            foreach (var i in System.IO.Directory.GetDirectories(source))
            {
                var iNewDir = System.IO.Path.Combine(dest, PathUtil.RelativePathTo(source, i));
                System.IO.Directory.CreateDirectory(iNewDir);
                CopyTo(i, iNewDir);
            }
        }

        /// <summary>Calculates the path for the specified id.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="string"/>.</returns>
        protected string GetPath(ulong id)
        {
            var iPath = DataPath;
            return GetPath(DataPath, id, out iPath);
        }

        /// <summary>Calculates the path for the specified <paramref name="id"/> and also
        ///     returns the directory seperatly.</summary>
        /// <param name="datapath">The datapath.</param>
        /// <param name="id">The id.</param>
        /// <param name="dir">The dir.</param>
        /// <returns>The <see cref="string"/>.</returns>
        protected static string GetPath(string datapath, ulong id, out string dir)
        {
            dir = datapath;
            var iTemp = id & ROOTPATH;
            if (iTemp > 0)
            {
                dir = System.IO.Path.Combine(dir, (iTemp >> (15 * 4)).ToString());

                    // we shift to right 12 times cause there are 12 0's after the last f, 1 digit in hex, represents 4 bits.
            }

            iTemp = id & SUBPATH1;
            if (iTemp > 0)
            {
                dir = System.IO.Path.Combine(dir, (iTemp >> (12 * 4)).ToString());
            }
            else
            {
                dir = System.IO.Path.Combine(dir, "0");
            }

            iTemp = id & SUBPATH2;
            if (iTemp > 0)
            {
                dir = System.IO.Path.Combine(dir, (iTemp >> (9 * 4)).ToString());
            }
            else
            {
                dir = System.IO.Path.Combine(dir, "0");
            }

            iTemp = id & SUBPATH3;
            if (iTemp > 0)
            {
                dir = System.IO.Path.Combine(dir, (iTemp >> (6 * 4)).ToString());
            }
            else
            {
                dir = System.IO.Path.Combine(dir, "0");
            }

            iTemp = id & SUBPATH4;
            if (iTemp > 0)
            {
                dir = System.IO.Path.Combine(dir, (iTemp >> (3 * 4)).ToString());
            }
            else
            {
                dir = System.IO.Path.Combine(dir, "0");
            }

            return System.IO.Path.Combine(dir, id + ".xml");
        }

        #region fields

        /// <summary>The f data path.</summary>
        private string fDataPath;

        /*
       * the folowing consts are used to filter out the values for building the path to the file of the neuron.
       */

        /// <summary>The rootpath.</summary>
        private const ulong ROOTPATH = 0xF000000000000000;

        /// <summary>The subpat h 1.</summary>
        private const ulong SUBPATH1 = 0x0FFF000000000000;

        /// <summary>The subpat h 2.</summary>
        private const ulong SUBPATH2 = 0x0000FFF000000000;

        /// <summary>The subpat h 3.</summary>
        private const ulong SUBPATH3 = 0x0000000FFF000000;

        /// <summary>The subpat h 4.</summary>
        private const ulong SUBPATH4 = 0x0000000000FFF000;

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

            DataPath = XmlStore.ReadElement<string>(reader, "DataPath");
            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "DataPath", fDataPath);
        }

        #endregion
    }
}