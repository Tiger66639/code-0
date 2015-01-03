// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBStore.cs" company="">
//   
// </copyright>
// <summary>
//   This is an <see cref="ILongTermMem" /> implementation that streams neurons in a binary format to a set of files
//   which store neurons in blocks.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Storage.NDB
{
    /// <summary>
    ///     This is an <see cref="ILongTermMem" /> implementation that streams neurons in a binary format to a set of files
    ///     which store neurons in blocks.
    /// </summary>
    [System.Xml.Serialization.XmlRoot("NDBStore", Namespace = "http://www.janbogaerts.name", IsNullable = false)]
    public class NDBStore : ILongtermMem, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The request storage clean data.</summary>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        internal object RequestStorageCleanData()
        {
            throw new System.NotImplementedException();
        }

        #region const

        /// <summary>
        ///     The default nr of tables that is used if there is no value defined when the files need to be loaded/created (for
        ///     init)
        /// </summary>
        public const int DEFAULTNROFTABLES = 8;

        /// <summary>
        ///     The start of all the file names that manage the entry points.
        /// </summary>
        private const string ENTRIESFILENAME = "Entries";

        #endregion

        #region fields

        /// <summary>The f nr of tables.</summary>
        private int? fNrOfTables;

        /// <summary>The f data path.</summary>
        private string fDataPath;

        /// <summary>The f relative path.</summary>
        private string fRelativePath;

                       // we store the path to which DataPath must be made relative to during storing, so that we don't unnecessarely close and open all the files by changing the datapath

        /// <summary>The f neurons ser.</summary>
        private NDBSerializer fNeuronsSer; // so we don't have to destroy and recreate this object continuasly

        /// <summary>The f type names.</summary>
        private System.Collections.Generic.Dictionary<ulong, System.Type> fTypeNames =
            new System.Collections.Generic.Dictionary<ulong, System.Type>();

        /// <summary>The f unknown type names.</summary>
        private System.Collections.Generic.Dictionary<ulong, string> fUnknownTypeNames;

        /// <summary>The f tables.</summary>
        private System.Collections.Generic.List<NDBTable> fTables = new System.Collections.Generic.List<NDBTable>();

                                                          // contains all the file groups that store the neurons. We need multiple tables since 1 file can only be lon.maxValue in size, while we can have ulong.maxvalue nr of neurons. 
        #endregion

        #region ctor/~

        /// <summary>Initializes a new instance of the <see cref="NDBStore"/> class. 
        ///     Initializes a new instance of the <see cref="NDB"/> class.</summary>
        public NDBStore()
        {
            fNeuronsSer = new NDBSerializer(fTypeNames);
            NDBTable.BlockSize = Settings.DBBlockSize; // provide an initial blocksize in case of a new db.
        }

        /// <summary>Finalizes an instance of the <see cref="NDBStore"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations before the<see cref="NDB"/> is reclaimed by garbage collection.</summary>
        ~NDBStore()
        {
            CloseFiles();
        }

        #endregion

        #region Prop

        #region TypeNames

        /// <summary>
        ///     Gets the dictionary that contains the mappings between ulongs (used by the NDB) and neurontypes, which they
        ///     represent.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.Generic.Dictionary<ulong, System.Type> TypeNames
        {
            get
            {
                return fTypeNames;
            }
        }

        #endregion

        #region DataPath

        /// <summary>
        ///     Gets or sets the path where the data should be saved to.
        /// </summary>
        /// <remarks>
        ///     To reopen files that were closed using <see cref="NDBStore.Dispose" />, simply reassign the same
        ///     path.
        /// </remarks>
        /// <value>The directory as a string.</value>
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
                    if (System.IO.Path.IsPathRooted(value))
                    {
                        // if this path is rooted, we try to open the files.
                        LoadFiles();
                    }
                    else
                    {
                        CloseFiles();
                        NDBTable.BlockSize = Settings.DBBlockSize;

                            // after closing, make certain that the blocksize is reset. This is important for working with templates: a project gets loaded into mem, then the db is closed so it can be reopened in a different location. when this happens, the db blockszie also needs to be reset.
                    }
                }
                else if (System.IO.Path.IsPathRooted(value) && fTables.Count == 0)
                {
                    // if we reassign the project path, but the files are not open (closed for copying), than we need to reopen the files.
                    LoadFiles();
                }
            }
        }

        #endregion

        #region NrOfTables

        /// <summary>
        ///     Gets/sets the number of data files that are used. This property can only be set 1 time.
        /// </summary>
        /// <remarks>
        ///     We only allow to set 1 time since, if there are neurons stored, and we add a file, we can't find
        ///     the correct neurons anymore, cause the algorithm to select the correct table to request a neuron
        ///     from uses the nr of tables to calculate this.
        /// </remarks>
        public int NrOfTables
        {
            get
            {
                if (fNrOfTables.HasValue)
                {
                    return fNrOfTables.Value;
                }

                return 0;
            }

            set
            {
                if (fNrOfTables.HasValue == false)
                {
                    fNrOfTables = value;
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "Can't change the nr of tables once this has been assigned.");
                }
            }
        }

        #endregion

        #endregion

        #region ILongtermMem Members

        /// <summary>Instructs the LongTermMem to make the <see cref="ILongTermMem.DataPath"/> relative to the specified path
        ///     so that the info can be streamed together with the project.</summary>
        /// <param name="path">The path.</param>
        public void MakePathRelativeTo(string path)
        {
            fRelativePath = path;
        }

        /// <summary>Instructs the LongTermMem to make the path absolute so that the data can be loaded.</summary>
        /// <param name="path">The path to make the <see cref="ILongTermMem.DataPath"/> absolute against.</param>
        /// <param name="readOnly">The read Only.</param>
        public void MakePathAbsoluteTo(string path, bool readOnly = false)
        {
            if (System.IO.Path.IsPathRooted(DataPath) == false)
            {
                var iPath = new System.Uri(DataPath, System.UriKind.Relative);
                var iTemp = new System.Uri(new System.Uri(path), iPath);

                var iStr = iTemp.LocalPath;
#if ANDROID
               if (iStr.Contains("\\") == true)                  // the android platform is unix based and uses / instead of \, so we need to switch this if needed.
                  iStr = iStr.Replace('\\', Path.DirectorySeparatorChar);
            #endif
                DataPath = iStr;
                LoadFiles(readOnly);
            }
        }

        /// <summary>Loads all the files so they can be accessed.</summary>
        /// <param name="readOnly">The read Only.</param>
        private void LoadFiles(bool readOnly = false)
        {
            CloseFiles();
            try
            {
                if (System.IO.Directory.Exists(DataPath))
                {
                    if (fNrOfTables.HasValue == false)
                    {
                        fNrOfTables = DEFAULTNROFTABLES;
                    }

                    string iTableName;
                    for (var i = 1; i <= fNrOfTables.Value; i++)
                    {
                        iTableName = System.IO.Path.Combine(DataPath, "Neurons" + i);
                        var iTable = new NDBTable(iTableName, fNeuronsSer, readOnly);
                        fTables.Add(iTable);
                    }
                }
                else
                {
                    throw new System.InvalidOperationException(string.Format("Invalid directory: {0}", DataPath));
                }
            }
            catch (System.Exception e)
            {
                CloseFiles();
                throw new System.InvalidOperationException("Failed to load database files", e);
            }
        }

#if DEBUG

        // Stopwatch fTest = new Stopwatch();
        // long fBiggestTime;
        // ulong fBiggestID;
#endif

        /// <summary>Tries to load the neuron with the specified ID</summary>
        /// <param name="id">The id of the neuron to load.</param>
        /// <returns>The object that was loaded or null if not found.</returns>
        /// <remarks>If the id can't be found null should be returned.</remarks>
        public LinkResolverData GetNeuron(ulong id)
        {
#if DEBUG

            // fTest.Restart();
            // try
            // {
#endif
            var iTableIdx = (int)(id % (ulong)NrOfTables);
            if (fTables.Count > iTableIdx)
            {
                // could be that the tables aren't loaded yet (new project).
                var iTable = fTables[iTableIdx];
                System.Diagnostics.Debug.Assert(iTable != null);
                var iNeuronIndex = id / (ulong)NrOfTables;
                System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                lock (iTable)

                    // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                    return iTable.GetNeuron((long)iNeuronIndex);
            }

            return null;
#if DEBUG

            // }
            // finally
            // {
            // fTest.Stop();
            // if (fBiggestTime < fTest.ElapsedTicks)
            // {
            // fBiggestTime = fTest.ElapsedTicks;
            // fBiggestID = id;
            // }
            // }
#endif
        }

        /// <summary>Tries to save the neuron to the long time memory.</summary>
        /// <param name="toSave">The object to save.</param>
        /// <remarks>When implementing this function, it is not required to lock the entire neuron, nor is it required to
        ///     change <see cref="Neuron.IsChanged"/>. This is all done by the caller of this function.</remarks>
        public void SaveNeuron(Neuron toSave)
        {
            SaveNeuron(toSave, toSave.ID);
        }

        /// <summary>Tries to save the neuron to the long time memory, using the specified id as a key.</summary>
        /// <param name="toSave">To save.</param>
        /// <param name="id">The id.</param>
        /// <remarks>Used for modules, so a neuron can be saved under a different id to compact the data.</remarks>
        public void SaveNeuron(Neuron toSave, ulong id)
        {
            var iTableIdx = (int)(id % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = id / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.SaveNeuron((long)iNeuronIndex, toSave);
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The object to remove</param>
        public void RemoveNeuron(Neuron toRemove)
        {
            var iId = toRemove.ID;
            var iTableIdx = (int)(iId % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = iId / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.RemoveNeuron((long)iNeuronIndex);
        }

        /// <summary>Cleans the db for the specified id. All data will be lost.</summary>
        /// <param name="id">The id.</param>
        internal void CleanFor(ulong id)
        {
            try
            {
                var iTableIdx = (int)(id % (ulong)NrOfTables);
                System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
                var iTable = fTables[iTableIdx];
                System.Diagnostics.Debug.Assert(iTable != null);
                var iNeuronIndex = id / (ulong)NrOfTables;
                System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                lock (iTable)

                    // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                    iTable.CleanData((long)iNeuronIndex);
            }
            catch
            {
                LogService.Log.LogWarning(
                    "NDBStore.CleanFor", 
                    string.Format("Failed to clean the database for bad neuron (id: {0}).", id));
            }
        }

        /// <summary>Marks the specified id as free but doesn't delete any neurons. This
        ///     is used for cleaning up the database (2 id's returning the same neuron).</summary>
        /// <param name="bad">The bad.</param>
        public void MarkAsFree(ulong bad)
        {
            var iTableIdx = (int)(bad % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = bad / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.MarkAsFree((long)iNeuronIndex);
        }

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The id of the neuron to remove.</param>
        public void RemoveNeuron(ulong toRemove)
        {
            var iTableIdx = (int)(toRemove % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            var iNeuronIndex = toRemove / (ulong)NrOfTables;
            System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.RemoveNeuron((long)iNeuronIndex);
        }

        /// <summary>
        ///     Makes certain that everything is properly written to file and nothing is left in the buffers anymore.
        /// </summary>
        public void Flush()
        {
            foreach (var i in fTables)
            {
                lock (i)

                    // lock the table during hte flush, if this is done during processing, we need to make certain nothing goes wrong with the threads.
                    i.Flush();
            }
        }

        /// <summary>
        ///     flushes the data without flushing any index files if this can be avoided (when they are buffered in memory).
        /// </summary>
        public void FlushFast()
        {
            foreach (var i in fTables)
            {
                lock (i)

                    // lock the table during hte flush, if this is done during processing, we need to make certain nothing goes wrong with the threads.
                    i.FlushData();
            }
        }

        /// <summary>Makes certain that everything is properly written to file and nothing is left in the buffers anymore after
        ///     processing a single item
        ///     This flush will not save the index tables if they are loaded in mem, but only the data files, to make the flush go
        ///     faster.</summary>
        /// <param name="id">The id.</param>
        public void Flush(ulong id)
        {
            var iTableIdx = (int)(id % (ulong)NrOfTables);
            System.Diagnostics.Debug.Assert(fTables.Count > iTableIdx);
            var iTable = fTables[iTableIdx];
            System.Diagnostics.Debug.Assert(iTable != null);
            lock (iTable)

                // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                iTable.FlushData();
        }

        /// <summary>Copies all the data to the specified location.  This operation doesn't effect the value of<see cref="ILongtermMem.DataPath"/>.</summary>
        /// <param name="loc">The location to copy the data to.</param>
        public void CopyTo(string loc)
        {
            foreach (var i in System.IO.Directory.GetFiles(DataPath))
            {
                // we copy all the files, don't filter on any file type so we make certain we have everything. (SVN stores in subdir which isn't copied over) -> subdirs of SVN gave problems while deleting the sandbox.
                System.IO.File.Copy(i, System.IO.Path.Combine(loc, System.IO.Path.GetFileName(i)));
            }
        }

        /// <summary>Retrieves a list of neurons that form the entry points for a <see cref="Sin"/>.</summary>
        /// <param name="type">The Sin type for which to retrieve the data</param>
        /// <returns>The <see cref="IList"/>.</returns>
        /// <remarks>A sin can only have 1 set of entrypoints.  If more control over data saving is required,
        ///     use <see cref="ILongtermMem.GetProperty"/></remarks>
        public System.Collections.Generic.IList<Neuron> GetEntryNeuronsFor(System.Type type)
        {
            return EntryNeuronsHelper.GetEntryNeuronsFor(DataPath, type);
        }

        /// <summary>Stores the list of neurons as the default entry points for the specified type.</summary>
        /// <param name="list">The list of neuron id's that need to be stored as entry points.</param>
        /// <param name="type">The Sin type for which to perform the store.</param>
        public void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<ulong> list, System.Type type)
        {
            EntryNeuronsHelper.SaveEntryNeuronsFor(DataPath, list, type);
        }

        /// <summary>Stores the list of neurons as the default entry points for the specified type but uses a list of neurons instead of
        ///     ids.</summary>
        /// <param name="list">The list of neurons that need to be stored as entry points.  Note that these Neurons are already
        ///     stored as regular neurons in the brain, so you should only store the Id of the neurons.</param>
        /// <param name="type">The Sin type for which to perform the store.</param>
        public void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<Neuron> list, System.Type type)
        {
            EntryNeuronsHelper.SaveEntryNeuronsFor(DataPath, list, type);
        }

        /// <summary>The get all entry neurons.</summary>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.Generic.IList<EntryPoints> GetAllEntryNeurons()
        {
            return EntryNeuronsHelper.GetAllEntryNeurons(DataPath);
        }

        /// <summary>The get all properties.</summary>
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

        /// <summary>Gets extra data associated with the specified type and id from the store.</summary>
        /// <typeparam name="T">The type of the property value that should be read.</typeparam>
        /// <param name="type">The type for which to get the property value.</param>
        /// <param name="id">The id of the property value that should be retrieved.</param>
        /// <returns>The <see cref="T"/>.</returns>
        /// <remarks><para>This is primarely used by sins to retrieve extra information from the brain that they require for proper
        ///         processing.
        ///         The <see cref="VerbNet"/> class also uses this during the import of data, to find already generated roles for
        ///         the project.</para>
        /// <para>The data should be stored in xml format.</para>
        /// </remarks>
        public T GetProperty<T>(System.Type type, string id) where T : class
        {
            return PropertiesHelper.GetProperty<T>(DataPath, type, id);
        }

        /// <summary>Saves extra data associated with the specified type and id from the store.</summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The id of the property value that should be saved.</param>
        /// <param name="value">The value that needs to be saved.</param>
        /// <remarks>See <see cref="ILongtermMem.GetProperty"/> for more info.</remarks>
        public void SaveProperty(System.Type type, string id, object value)
        {
            PropertiesHelper.SaveProperty(DataPath, type, id, value);
        }

        /// <summary>The is existing id.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsExistingID(ulong id)
        {
            if (NrOfTables > 0)
            {
                var iTableIdx = (int)(id % (ulong)NrOfTables);
                if (fTables.Count > iTableIdx)
                {
                    var iTable = fTables[iTableIdx];
                    System.Diagnostics.Debug.Assert(iTable != null);
                    var iNeuronIndex = id / (ulong)NrOfTables;
                    System.Diagnostics.Debug.Assert(iNeuronIndex <= long.MaxValue);
                    lock (iTable)

                        // we lock the entire table cause we can only access a file group from 1 thread, otherwise the file pos jumps all around, and the reads fail.
                        return iTable.IsExistingID((long)iNeuronIndex);
                }

                return false;
            }

            return false;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            CloseFiles();
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Closes the files.
        /// </summary>
        private void CloseFiles()
        {
            foreach (var i in fTables)
            {
                i.Dispose();
            }

            fTables.Clear();
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            DataPath = XmlStore.ReadElement<string>(reader, "DataPath");
            NrOfTables = XmlStore.ReadElement<int>(reader, "NrOfTables");
            var iBlockSize = 0;
            if (XmlStore.TryReadElement(reader, "BlockSize", ref iBlockSize))
            {
                NDBTable.BlockSize = iBlockSize;
            }
            else
            {
                NDBTable.BlockSize = 101; // old versions didn't store the info, but used a const.
            }

            XmlStore.ReadList(reader, "NeuronTypes", ReadTypeName);
            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var iPath = PathUtil.RelativePathTo(
                PathUtil.VerifyPathEnd(System.IO.Path.GetDirectoryName(fRelativePath)), 
                PathUtil.VerifyPathEnd(DataPath));

                // need to make it relative for saving so that the project can be moved.
            XmlStore.WriteElement(writer, "DataPath", iPath);
            XmlStore.WriteElement(writer, "NrOfTables", NrOfTables);
            XmlStore.WriteElement(writer, "BlockSize", NDBTable.BlockSize);
            writer.WriteStartElement("NeuronTypes");
            foreach (var i in fTypeNames)
            {
                WriteTypeName(writer, i);
            }

            if (fUnknownTypeNames != null)
            {
                // can't let these simply drop, the project would get corrupted.
                foreach (var i in fUnknownTypeNames)
                {
                    WriteTypeName(writer, i);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>The write type name.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteTypeName(
            System.Xml.XmlWriter writer, 
            System.Collections.Generic.KeyValuePair<ulong, string> item)
        {
            writer.WriteStartElement("Map");
            XmlStore.WriteElement(writer, "ID", item.Key);
            XmlStore.WriteElement(writer, "Type", item.Value);
            writer.WriteEndElement();
        }

        /// <summary>Writes the name and id of a single neuron type mapping.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        private void WriteTypeName(
            System.Xml.XmlWriter writer, 
            System.Collections.Generic.KeyValuePair<ulong, System.Type> item)
        {
            writer.WriteStartElement("Map");
            XmlStore.WriteElement(writer, "ID", item.Key);
            XmlStore.WriteElement(writer, "Type", item.Value.AssemblyQualifiedName);
            writer.WriteEndElement();
        }

        /// <summary>Reads the name and id of a single neuron type mapping.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTypeName(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement("Map");
            var iId = XmlStore.ReadElement<ulong>(reader, "ID");
            var iTypeName = XmlStore.ReadElement<string>(reader, "Type");
            var iType = Brain.Current.GetNeuronType(iTypeName);
            if (iType != null)
            {
                fTypeNames.Add(iId, iType);
            }
            else
            {
                LogService.Log.LogWarning(
                    "NDBStore", 
                    string.Format("Failed to map type name {0} to a type.", iTypeName));

                    // this doens't always have to be a disaster, not all projects need a wordsin for instance.
                if (fUnknownTypeNames == null)
                {
                    fUnknownTypeNames = new System.Collections.Generic.Dictionary<ulong, string>();
                }

                fUnknownTypeNames.Add(iId, iTypeName);

                    // we can't loose this info, so store it in another list, otherwise the project would get corrupted (when wordsin not defined, the designer wont load).
            }

            reader.ReadEndElement();
        }

        #endregion
    }
}