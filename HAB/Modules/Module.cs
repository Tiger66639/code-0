// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Module.cs" company="">
//   
// </copyright>
// <summary>
//   Defines an id of a module,which consists out of a name, major and minor
//   version.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Defines an id of a module,which consists out of a name, major and minor
    ///     version.
    /// </summary>
    public class ModuleID
    {
        #region Name

        /// <summary>
        ///     Gets or sets the name of the module.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        #endregion

        #region MajorVer

        /// <summary>
        ///     Gets or sets the major version.
        /// </summary>
        /// <value>
        ///     The major version.
        /// </value>
        public int MajorVersion { get; set; }

        #endregion

        #region MinorVer

        /// <summary>
        ///     Gets or sets the minor version.
        /// </summary>
        /// <value>
        ///     The minor version.
        /// </value>
        public int MinorVersion { get; set; }

        #endregion

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return Name + "(Version " + MajorVersion + "." + MinorVersion + ")";
        }
    }

    /// <summary>
    ///     Represents a single module that has been loaded into or exported from the
    ///     network.
    /// </summary>
    public class Module : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The externalreffile.</summary>
        private const string EXTERNALREFFILE = "ExternalRefs.dat";

        /// <summary>The neuronsfile.</summary>
        private const string NEURONSFILE = "neurons.dat";

        /// <summary>The registeredbind.</summary>
        private const string REGISTEREDBIND = "bindings.dat";

        /// <summary>The compileddata.</summary>
        private const string COMPILEDDATA = "data.obj";

        /// <summary>The libreffile.</summary>
        private const string LIBREFFILE = "librefs.dat";

        /// <summary>
        ///     Initializes a new instance of the <see cref="Module" /> class.
        /// </summary>
        public Module()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Module"/> class. use this contructor if you want to create a module that is used by a
        ///     query, to store the compiled data from the query.</summary>
        /// <param name="asQuery"></param>
        public Module(bool asQuery)
        {
            fAsQuery = asQuery;
        }

        #region ID

        /// <summary>
        ///     Gets the id of the module. The setter is for reading from xml.
        /// </summary>
        public ModuleID ID
        {
            get
            {
                return fID;
            }

            set
            {
                fID = value;
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the UID. Queries don't save the file based on the id (==
        ///     the name of the query), but on the UID, which is uniquely generated
        ///     for each query. This allows us to change the name of hte query without
        ///     concering about the compiled data files.
        /// </summary>
        /// <value>
        ///     The UID.
        /// </value>
        public string UID { get; set; }

        #region FileNames

        /// <summary>
        ///     Gets the list of file names (including relative paths to the path of
        ///     this file).
        /// </summary>
        public System.Collections.Generic.List<string> FileNames
        {
            get
            {
                return fFileNames;
            }
        }

        #endregion

        #region DependsOn

        /// <summary>
        ///     Gets the list of modules names that should be loaded before this one
        ///     can be because it has dependencies/references to those modules.
        /// </summary>
        public System.Collections.Generic.List<string> DependsOn
        {
            get
            {
                return fDependsOn;
            }
        }

        #endregion

        #region ExtensionFiles

        /// <summary>
        ///     Gets the list of filenames that were added to this module by the user
        ///     (extention functions). These are seperate so that there is an easy
        ///     upgrade path.
        /// </summary>
        public System.Collections.Generic.List<string> ExtensionFiles
        {
            get
            {
                return fExtensionFiles;
            }
        }

        #endregion

        #region FullPath

        /// <summary>
        ///     Gets the full path and filename of the table.
        /// </summary>
        /// <value>
        ///     The full path.
        /// </value>
        public string FullPath
        {
            get
            {
                string iFile;
                if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                {
                    iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, FileName);
                }
                else
                {
                    iFile = null;
                }

                return iFile;
            }
        }

        #endregion

        #region Neurons

        /// <summary>
        ///     Gets all the neuron ids in the module. this is loaded as needed. Newly
        ///     added items are buffered. Warning: not thread save.
        /// </summary>
        /// <value>
        ///     The neurons.
        /// </value>
        public System.Collections.Generic.IList<ulong> Neurons
        {
            get
            {
                if (fNeurons == null)
                {
                    fNeurons = new System.Collections.Generic.List<ulong>();
                    string iFile;
                    if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                    {
                        iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(NEURONSFILE));
                    }
                    else
                    {
                        iFile = null;
                    }

                    if (iFile != null && System.IO.File.Exists(iFile))
                    {
                        using (
                            var iStream = new System.IO.FileStream(
                                iFile, 
                                System.IO.FileMode.Open, 
                                System.IO.FileAccess.Read))
                        {
                            var iBuffer = new System.IO.BufferedStream(iStream, Settings.DBBufferSize);
                            var iReader = new System.IO.BinaryReader(iBuffer);
                            var iToRead = (iStream.Length - iStream.Position) / 8;

                                // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                            while (iToRead > 0)
                            {
                                fNeurons.Add(iReader.ReadUInt64());
                                iToRead--;
                            }
                        }
                    }
                }

                return fNeurons;
            }
        }

        #endregion

        #region ExternalRefs

        /// <summary>
        ///     Gets all the cluster ids in the module that were referenced (not part
        ///     of the module, but the module has rendered children into the cluster).
        ///     this is loaded as needed. Newly added items are buffered. Warning: not
        ///     thread save.
        /// </summary>
        /// <value>
        ///     The neurons.
        /// </value>
        public System.Collections.Generic.IList<ulong> ExternalRefs
        {
            get
            {
                if (fExternalRefs == null)
                {
                    fExternalRefs = new System.Collections.Generic.List<ulong>();
                    string iFile;
                    if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                    {
                        iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(EXTERNALREFFILE));
                    }
                    else
                    {
                        iFile = null;
                    }

                    if (iFile != null && System.IO.File.Exists(iFile))
                    {
                        using (
                            var iStream = new System.IO.FileStream(
                                iFile, 
                                System.IO.FileMode.Open, 
                                System.IO.FileAccess.Read))
                        {
                            var iBuffer = new System.IO.BufferedStream(iStream, Settings.DBBufferSize);
                            var iReader = new System.IO.BinaryReader(iBuffer);
                            var iToRead = (iStream.Length - iStream.Position) / 8;

                                // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                            while (iToRead > 0)
                            {
                                fExternalRefs.Add(iReader.ReadUInt64());
                                iToRead--;
                            }
                        }
                    }
                }

                return fExternalRefs;
            }
        }

        #endregion

        #region LibRefs

        /// <summary>
        ///     Gets all the cluster ids in the module that were referenced (not part
        ///     of the module, but the module has rendered children into the cluster).
        ///     this is loaded as needed. Newly added items are buffered. Warning: not
        ///     thread save.
        /// </summary>
        /// <value>
        ///     The neurons.
        /// </value>
        public System.Collections.Generic.IList<ulong> LibRefs
        {
            get
            {
                if (fLibRefs == null)
                {
                    fLibRefs = new System.Collections.Generic.List<ulong>();
                    string iFile;
                    if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                    {
                        iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(LIBREFFILE));
                    }
                    else
                    {
                        iFile = null;
                    }

                    if (iFile != null && System.IO.File.Exists(iFile))
                    {
                        using (
                            var iStream = new System.IO.FileStream(
                                iFile, 
                                System.IO.FileMode.Open, 
                                System.IO.FileAccess.Read))
                        {
                            var iBuffer = new System.IO.BufferedStream(iStream, Settings.DBBufferSize);
                            var iReader = new System.IO.BinaryReader(iBuffer);
                            var iToRead = (iStream.Length - iStream.Position) / 8;

                                // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                            while (iToRead > 0)
                            {
                                fLibRefs.Add(iReader.ReadUInt64());
                                iToRead--;
                            }
                        }
                    }
                }

                return fLibRefs;
            }
        }

        #endregion

        #region RegisteredBindings

        /// <summary>
        ///     Gets/sets the stream that contains the data for the registered
        ///     bindings. This allows the module compiler to quickly reload the
        ///     bindings, when needed.
        /// </summary>
        /// <remarks>
        ///     When you get this value, it returns a file stream. When you set the
        ///     value, it's best to use a memory Stream. No value is buffered when not
        ///     needed, so every time you do a get, a new filestream is created.
        /// </remarks>
        public System.IO.Stream RegisteredBindings
        {
            get
            {
                if (fRegisteredBindings != null)
                {
                    return fRegisteredBindings;
                }

                string iFile;
                if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                {
                    iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(REGISTEREDBIND));
                    if (System.IO.File.Exists(iFile))
                    {
                        return new System.IO.FileStream(iFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    }

                    return null;
                }

                return null;
            }

            set
            {
                fNeedsSave = true;
                if (value == null)
                {
                    fRegisteredBindings = new System.IO.MemoryStream();

                        // create an empty memory stream. this lets the saver know that something about the registered binding has cahnged, when it sees the the stream is empty, it simply deletes the file (if there was any).
                }
                else
                {
                    fRegisteredBindings = value;
                }
            }
        }

        #endregion

        #region RegisteredBindings

        /// <summary>
        ///     Gets/sets the stream that contains the data for the registered
        ///     bindings. This allows the module compiler to quickly reload the
        ///     bindings, when needed.
        /// </summary>
        /// <remarks>
        ///     When you get this value, it returns a file stream. When you set the
        ///     value, it's best to use a memory Stream. No value is buffered when not
        ///     needed, so every time you do a get, a new filestream is created.
        /// </remarks>
        public System.IO.Stream CompiledData
        {
            get
            {
                if (fCompiledData != null)
                {
                    return fCompiledData;
                }

                string iFile;
                if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
                {
                    iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(COMPILEDDATA));
                    if (System.IO.File.Exists(iFile))
                    {
                        return new System.IO.FileStream(iFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    }

                    return null;
                }

                return null;
            }

            set
            {
                fNeedsSave = true;
                if (value == null)
                {
                    fCompiledData = new System.IO.MemoryStream();

                        // create an empty memory stream. this lets the saver know that something about the registered binding has cahnged, when it sees the the stream is empty, it simply deletes the file (if there was any).
                }
                else
                {
                    fCompiledData = value;
                }
            }
        }

        #endregion

        /// <summary>adds the specified neuron to the module, increments the moduleCount on
        ///     the <paramref name="item"/> and if the name is specified, adds this to
        ///     the dictionary.</summary>
        /// <param name="item"></param>
        public void Add(Neuron item)
        {
            Neurons.Add(item.ID);
            item.ModuleRefCount++;
            fNeedsSave = true;
        }

        /// <summary>removes the specified neuron from this module.</summary>
        /// <param name="item"></param>
        public void Remove(Neuron item)
        {
            Neurons.Remove(item.ID);
            item.ModuleRefCount--;
            fNeedsSave = true;
        }

        /// <summary>Adds the neuron to a list that keeps trakc of objects that aren't part
        ///     of the module (defined somewhere else), but were linked in through
        ///     there id. These neurons can contain other items that did get generated
        ///     by the module. This needs to be remembered, so that the references can
        ///     be broken.</summary>
        /// <param name="item"></param>
        public void AddExternalRef(Neuron item)
        {
            if (ExternalRefs.Contains(item.ID) == false)
            {
                // if the item is already stored as something we generated previously, don't store as external ref.
                ExternalRefs.Add(item.ID);
                item.ModuleRefCount++;
                fNeedsSave = true;
            }
        }

        /// <summary>The add lib ref.</summary>
        /// <param name="item">The item.</param>
        public void AddLibRef(Neuron item)
        {
            if (LibRefs.Contains(item.ID) == false)
            {
                // if the item is already stored as something we generated previously, don't store as external ref.
                LibRefs.Add(item.ID);
                item.ModuleRefCount++;
                fNeedsSave = true;
            }
        }

        /// <summary>
        ///     removes all the neurons as being referenced from the module.
        /// </summary>
        internal void Release()
        {
            foreach (var i in Neurons)
            {
                var iN = Brain.Current[i];
                iN.ModuleRefCount--;
            }

            foreach (var i in ExternalRefs)
            {
                var iN = Brain.Current[i];
                iN.ModuleRefCount--;
            }

            foreach (var i in LibRefs)
            {
                var iN = Brain.Current[i];
                iN.ModuleRefCount--;
            }

            fNeurons = null; // no more neurons required
            fExternalRefs = null;
            fLibRefs = null;
            fRegisteredBindings = new System.IO.MemoryStream();

                // create an empty memory stream to indicate that the stream needs to be saved, but since it's empty, it will simply delete the previous content without creating a new file.
            fCompiledData = new System.IO.MemoryStream();
            fNeedsSave = true;
        }

        /// <summary>
        ///     saves the data to disk.
        /// </summary>
        public void Flush()
        {
            if (fNeedsSave)
            {
                if (fNeurons != null || fExternalRefs != null)
                {
                    // when we need to do a change: the neurons list is either loaded (module added), or null (the module was released). for a release, we need to make certain all the files are gone.
                    SaveToFiles();
                }
                else
                {
                    DeleteFiles();
                }

                SaveRegisteredBindings();
                SaveCompiledData();
                fNeedsSave = false;
            }
        }

        /// <summary>The save registered bindings.</summary>
        private void SaveRegisteredBindings()
        {
            if (fRegisteredBindings != null)
            {
                var iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(REGISTEREDBIND));
                if (fRegisteredBindings.Length == 0)
                {
                    if (System.IO.File.Exists(iFile))
                    {
                        System.IO.File.Delete(iFile);
                    }
                }
                else if (fRegisteredBindings is System.IO.FileStream
                         && ((System.IO.FileStream)fRegisteredBindings).Name == iFile)
                {
                    fRegisteredBindings.Flush(); // save the data and close the stream.
                    fRegisteredBindings.Close();
                }
                else
                {
                    fRegisteredBindings.Position = 0;
                    using (
                        var iStream = new System.IO.FileStream(
                            iFile, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.Write)) fRegisteredBindings.CopyTo(iStream);
                }

                fRegisteredBindings = null;
            }
        }

        /// <summary>The save compiled data.</summary>
        private void SaveCompiledData()
        {
            if (fCompiledData != null)
            {
                var iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(COMPILEDDATA));
                if (fCompiledData.Length == 0)
                {
                    if (System.IO.File.Exists(iFile))
                    {
                        System.IO.File.Delete(iFile);
                    }
                }
                else if (fCompiledData is System.IO.FileStream && ((System.IO.FileStream)fCompiledData).Name == iFile)
                {
                    fCompiledData.Flush(); // save the data and close the stream.
                    fCompiledData.Close();
                }
                else
                {
                    fCompiledData.Position = 0;
                    using (
                        var iStream = new System.IO.FileStream(
                            iFile, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.Write)) fCompiledData.CopyTo(iStream);
                }

                fCompiledData = null;
            }
        }

        /// <summary>
        ///     Deletes the files.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void DeleteFiles()
        {
            string iFile;
            if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
            {
                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(NEURONSFILE));
            }
            else
            {
                iFile = null;
            }

            if (iFile != null)
            {
                if (System.IO.File.Exists(iFile))
                {
                    System.IO.File.Delete(iFile);
                }

                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(EXTERNALREFFILE));
                if (System.IO.File.Exists(iFile))
                {
                    System.IO.File.Delete(iFile);
                }

                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(REGISTEREDBIND));
                if (System.IO.File.Exists(iFile))
                {
                    System.IO.File.Delete(iFile);
                }

                if (System.IO.File.Exists(FullPath))
                {
                    // file doesn't have to exist: queries don't.
                    System.IO.File.Delete(FullPath);
                }

                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(LIBREFFILE));
                if (System.IO.File.Exists(iFile))
                {
                    System.IO.File.Delete(iFile);
                }
            }
        }

        /// <summary>
        ///     saves all the data to the files.
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveToFiles()
        {
            string iFile;
            if (string.IsNullOrEmpty(Brain.Current.Modules.Path) == false)
            {
                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(NEURONSFILE));
            }
            else
            {
                iFile = null;
            }

            if (iFile != null)
            {
                SaveListContent(fNeurons, iFile);
                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(EXTERNALREFFILE));
                SaveListContent(fExternalRefs, iFile);
                iFile = System.IO.Path.Combine(Brain.Current.Modules.Path, GetFileNameFor(LIBREFFILE));
                SaveListContent(fLibRefs, iFile);
                if (fAsQuery == false)
                {
                    // queries don't need to save a .mod file, nothing in them anyway.
                    using (
                        var iStr = new System.IO.FileStream(
                            FullPath, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.ReadWrite))
                    {
                        try
                        {
                            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(Module));
                            using (System.IO.TextWriter iWriter = new System.IO.StreamWriter(iStr)) iSer.Serialize(iWriter, this);
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogError("Module.flush", e.Message);
                        }
                    }
                }
            }
        }

        /// <summary>The save list content.</summary>
        /// <param name="list">The list.</param>
        /// <param name="file">The file.</param>
        private void SaveListContent(System.Collections.Generic.List<ulong> list, string file)
        {
            if (list != null)
            {
                if (list.Count > 0)
                {
                    using (
                        var iStream = new System.IO.FileStream(
                            file, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.Write))
                    {
                        iStream.SetLength(list.Count * sizeof(ulong)); // init the size of the file for fast writing.
                        using (var iWriter = new System.IO.BinaryWriter(iStream))
                        {
                            foreach (var i in list)
                            {
                                iWriter.Write(i);
                            }
                        }
                    }
                }
                else
                {
                    if (System.IO.File.Exists(file))
                    {
                        // the list is empty, so delete the file if it still exists.
                        System.IO.File.Delete(file);
                    }
                }
            }
        }

        /// <summary>reads the module as an xml file from the specified location and
        ///     returns a module object.</summary>
        /// <param name="fileName"></param>
        /// <param name="asQuery">The as Query.</param>
        /// <returns>The <see cref="Module"/>.</returns>
        public static Module LoadXml(string fileName, bool asQuery = false)
        {
            var iSettings = new System.Xml.XmlReaderSettings();
            iSettings.IgnoreComments = true;
            iSettings.IgnoreWhitespace = true;
            iSettings.IgnoreProcessingInstructions = true;
            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(Module));
            var iMod = new Module(asQuery);
            using (var iReader = System.Xml.XmlReader.Create(fileName, iSettings))
            {
                iReader.ReadStartElement();
                iMod.ReadXmlContent(iReader);
            }

            return iMod;
        }

        /// <summary>
        ///     makes certain that all the data for this module is loaded and set as
        ///     changed (so it gets saved again).
        /// </summary>
        public void TouchMem()
        {
            if (Neurons != null)
            {
                // this makes certain that the neurons are loaded..
                fNeedsSave = true; // so we save the data again.
            }

            if (ExternalRefs != null)
            {
                // this makes certain that the neurons are loaded. need an && to load boath.
                fNeedsSave = true; // so we save the data again.
            }

            if (LibRefs != null)
            {
                // this makes certain that the neurons are loaded. need an && to load boath.
                fNeedsSave = true;
            }

            var iRegistered = RegisteredBindings;
            if (iRegistered != null && iRegistered is System.IO.FileStream)
            {
                fRegisteredBindings = new System.IO.MemoryStream();
                iRegistered.CopyTo(fRegisteredBindings);

                    // simply copy over the values to a local stream, when saving time comes, we can save the data.
                fNeedsSave = true;
            }

            iRegistered = CompiledData;
            if (iRegistered != null && iRegistered is System.IO.FileStream)
            {
                fCompiledData = new System.IO.MemoryStream();
                iRegistered.CopyTo(CompiledData);

                    // simply copy over the values to a local stream, when saving time comes, we can save the data.
                fNeedsSave = true;
            }
        }

        #region fields

        /// <summary>The f id.</summary>
        private ModuleID fID = new ModuleID();

        /// <summary>The f neurons.</summary>
        private System.Collections.Generic.List<ulong> fNeurons; // the list of neuron ids managed by this module. 

        /// <summary>The f external refs.</summary>
        private System.Collections.Generic.List<ulong> fExternalRefs;

        /// <summary>The f lib refs.</summary>
        private System.Collections.Generic.List<ulong> fLibRefs;

        /// <summary>The f file names.</summary>
        private readonly System.Collections.Generic.List<string> fFileNames =
            new System.Collections.Generic.List<string>();

        /// <summary>The f extension files.</summary>
        private readonly System.Collections.Generic.List<string> fExtensionFiles =
            new System.Collections.Generic.List<string>();

        /// <summary>The f depends on.</summary>
        private readonly System.Collections.Generic.List<string> fDependsOn =
            new System.Collections.Generic.List<string>();

        /// <summary>The f needs save.</summary>
        private bool fNeedsSave;

        /// <summary>The f as query.</summary>
        private readonly bool fAsQuery;

        /// <summary>The f registered bindings.</summary>
        private System.IO.Stream fRegisteredBindings;

        /// <summary>The f compiled data.</summary>
        private System.IO.Stream fCompiledData;

        #endregion

        #region FileName

        /// <summary>
        ///     Gets the name of the file without path.
        /// </summary>
        /// <value>
        ///     The name of the file.
        /// </value>
        [System.Xml.Serialization.XmlIgnore] // android trips over this for the moment
        public string FileName
        {
            get
            {
                if (fAsQuery == false)
                {
                    return string.Format("{0}_{1}_{2}{3}", ID.Name, ID.MajorVersion, ID.MinorVersion, ".mod");
                }

                return string.Format("{0}_{1}_{2}{3}", UID, ID.MajorVersion, ID.MinorVersion, ".query");
            }
        }

        /// <summary>gets the name of the file for the speicified sub item.</summary>
        /// <param name="ext"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetFileNameFor(string ext)
        {
            if (fAsQuery == false)
            {
                return string.Format("{0}_{1}_{2}{3}", ID.Name, ID.MajorVersion, ID.MinorVersion, ext);
            }

            return string.Format("{0}_{1}_{2}{3}", UID, ID.MajorVersion, ID.MinorVersion, ext);
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

            ReadXmlContent(reader);
        }

        /// <summary>The read xml content.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadXmlContent(System.Xml.XmlReader reader)
        {
            ID.Name = XmlStore.ReadElNoCase<string>(reader, "Name");
            ID.MajorVersion = XmlStore.ReadElNoCase<int>(reader, "MajorVer");
            ID.MinorVersion = XmlStore.ReadElNoCase<int>(reader, "MinorVer");
            var iCur = reader.Name.ToLower();
            while (iCur == "file" || iCur == "dependson")
            {
                if (iCur == "file")
                {
                    FileNames.Add(XmlStore.ReadElNoCase<string>(reader, "file"));
                }
                else
                {
                    DependsOn.Add(XmlStore.ReadElNoCase<string>(reader, "dependson"));
                }

                iCur = reader.Name.ToLower();
            }

            while (reader.Name == "Extension")
            {
                ExtensionFiles.Add(XmlStore.ReadElNoCase<string>(reader, "Extension"));
            }

            reader.ReadEndElement();
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "Name", ID.Name);
            XmlStore.WriteElement(writer, "MajorVer", ID.MajorVersion);
            XmlStore.WriteElement(writer, "MinorVer", ID.MinorVersion);
            foreach (var i in DependsOn)
            {
                XmlStore.WriteElement(writer, "DependsOn", i);
            }

            foreach (var i in FileNames)
            {
                XmlStore.WriteElement(writer, "File", i);
            }

            foreach (var i in ExtensionFiles)
            {
                XmlStore.WriteElement(writer, "Extension", i);
            }
        }

        #endregion
    }
}