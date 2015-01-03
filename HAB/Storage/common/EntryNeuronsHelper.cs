// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntryNeuronsHelper.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="internal" /> class that can be used by the storage
//   systems for managing entry point neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An <see langword="internal" /> class that can be used by the storage
    ///     systems for managing entry point neurons.
    /// </summary>
    /// <remarks>
    ///     Handles each entryneurons-type pair as a single binary flat file.
    /// </remarks>
    internal class EntryNeuronsHelper
    {
        /// <summary>The fileprefix.</summary>
        private const string FILEPREFIX = "EntryPointsFor";

        /// <summary>The fileextention.</summary>
        private const string FILEEXTENTION = ".dat";

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified type.</summary>
        /// <param name="path">The path.</param>
        /// <param name="list">The list of neurons that need to be stored as entry points. Note that
        ///     these Neurons are already stored as regular neurons in the brain, so
        ///     you should only store the Id of the neurons.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        internal static void SaveEntryNeuronsFor(
            string path, System.Collections.Generic.IEnumerable<ulong> list, 
            System.Type type)
        {
            var iPath = System.IO.Path.Combine(path, FILEPREFIX + type + FILEEXTENTION);
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (var iWriter = new System.IO.BinaryWriter(iStr))
                {
                    foreach (var i in list)
                    {
                        iWriter.Write(i);
                    }
                }
            }
        }

        /// <summary>Saves the dictionary for the textsin.</summary>
        /// <param name="path"></param>
        /// <param name="dict"></param>
        internal static void SaveTextDict(string path, System.Collections.Generic.Dictionary<string, ulong> dict)
        {
            var iIDPath = System.IO.Path.Combine(path, FILEPREFIX + typeof(TextSin) + FILEEXTENTION);
            var iTextPath = System.IO.Path.Combine(path, WordsDictionary.TEXTFILENAME);
            using (
                var iTextStr = new System.IO.FileStream(
                    iTextPath, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.Write))
            {
                using (var iTextWriter = new System.IO.BinaryWriter(iTextStr))
                {
                    using (
                        var iStr = new System.IO.FileStream(
                            iIDPath, 
                            System.IO.FileMode.Create, 
                            System.IO.FileAccess.Write))
                    {
                        using (var iWriter = new System.IO.BinaryWriter(iStr))
                        {
                            foreach (var i in dict)
                            {
                                iWriter.Write(i.Value);
                                iTextWriter.Write(i.Key);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified type.</summary>
        /// <param name="DataPath">The Data Path.</param>
        /// <param name="list">The list of neurons that need to be stored as entry points. Note that
        ///     these Neurons are already stored as regular neurons in the brain, so
        ///     you should only store the Id of the neurons.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        internal static void SaveEntryNeuronsFor(
            string DataPath, System.Collections.Generic.IEnumerable<Neuron> list, 
            System.Type type)
        {
            var iPath = System.IO.Path.Combine(DataPath, FILEPREFIX + type + FILEEXTENTION);
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (var iWriter = new System.IO.BinaryWriter(iStr))
                {
                    foreach (var i in list)
                    {
                        iWriter.Write(i.ID);
                    }
                }
            }
        }

        /// <summary>Retrieves a list of neurons that form the entry points</summary>
        /// <param name="DataPath">The Data Path.</param>
        /// <param name="type">The type for which to retrieve the data.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        internal static System.Collections.Generic.IList<Neuron> GetEntryNeuronsFor(string DataPath, System.Type type)
        {
            if (DataPath != null)
            {
                var iRes = new System.Collections.Generic.List<Neuron>();
                var iPath = System.IO.Path.Combine(DataPath, FILEPREFIX + type + FILEEXTENTION);
                if (System.IO.File.Exists(iPath))
                {
#if ANDROID // android doesn't return length info, so need to read differntly then in windows.
                  GetEntryNeuronsForAndroid(iPath, iRes, type);
               #else
                    GetEntryNeuronsForWindows(iPath, iRes, type);
#endif
                }

                return iRes;
            }

            LogService.Log.LogError(
                "Storage.GetEntryNeuronsFor", 
                string.Format("No NeuronsPath defined: failed to find entry point for Sin type {0}.", type));
            return null;
        }

        /// <summary>Gets the size of the entries list, so we can set the mem.</summary>
        /// <param name="DataPath"></param>
        /// <param name="type"></param>
        /// <returns>The <see cref="int"/>.</returns>
        internal static int GetCapacityEntryFor(string DataPath, System.Type type)
        {
            if (DataPath != null)
            {
                var iPath = System.IO.Path.Combine(DataPath, FILEPREFIX + type + FILEEXTENTION);
                if (System.IO.File.Exists(iPath))
                {
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
#if ANDROID // android doesn't return length info, so need to read differntly then in windows.
                     return 0;            
#else
                        return (int)(iStr.Length / 8); // prepare the size, for faster loading.
#endif
                    }
                }
            }

            return 0;
        }

        /// <summary>Retrieves a list of ids that form the entry points This is fasterr
        ///     than GetEntryNeuronsFor. It is used by the textsing for loading the
        ///     dict.</summary>
        /// <param name="DataPath">The Data Path.</param>
        /// <param name="type">The type for which to retrieve the data.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        internal static System.Collections.Generic.IEnumerable<ulong> GetEntryIDsFor(string DataPath, System.Type type)
        {
            if (DataPath != null)
            {
                var iRes = new System.Collections.Generic.List<ulong>();
                var iPath = System.IO.Path.Combine(DataPath, FILEPREFIX + type + FILEEXTENTION);
                if (System.IO.File.Exists(iPath))
                {
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                        using (var iReader = new System.IO.BinaryReader(iBuffer))
                        {
#if ANDROID // android doesn't return length info, so need to read differntly then in windows.
                     byte[] iTemp = new byte[8];
                     int iNrRead = iReader.Read(iTemp, 0, 8);
                     while (iNrRead == 8)
                     {
                        yield return BitConverter.ToUInt64(iTemp, 0);
                        iNrRead = iReader.Read(iTemp, 0, 8);
                     }
#else
                            var iToRead = iStr.Length / 8;

                                // length && pos are slow, faster to precalculate. 64bit = 8 bytes.
                            while (iToRead > 0)
                            {
                                yield return iReader.ReadUInt64();
                                iToRead--;
                            }

#endif
                        }
                    }
                }
            }
            else
            {
                LogService.Log.LogError(
                    "Storage.GetEntryNeuronsFor", 
                    string.Format("No NeuronsPath defined: failed to find entry point for Sin type {0}.", type));
            }
        }

        /// <summary>reads the data on a windows system.</summary>
        /// <param name="iPath"></param>
        /// <param name="res"></param>
        /// <param name="type"></param>
        private static void GetEntryNeuronsForWindows(
            string iPath, System.Collections.Generic.List<Neuron> res, 
            System.Type type)
        {
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                res.Capacity = (int)(iStr.Length / 8); // prepare the size, for faster loading.
                using (var iReader = new System.IO.BinaryReader(iStr))
                {
                    var iToRead = iStr.Length / 8;

                        // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                    while (iToRead > 0)
                    {
                        var iId = iReader.ReadUInt64();
                        try
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(iId, out iFound))
                            {
                                res.Add(iFound);
                            }
                            else
                            {
                                LogService.Log.LogWarning(
                                    "Storage.GetEntryNeuronsFor", 
                                    string.Format("{0} not found in brain, skipped as entry point.", iId));
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogWarning(
                                "Storage.GetEntryNeuronsFor", 
                                string.Format(
                                    "Failed to find entry point for Sin type: '{0}' with error: {1}.", 
                                    type, 
                                    e));
                        }

                        iToRead--;
                    }
                }
            }
        }

        /// <summary>The android system doesn't return proper length information for file
        ///     streams, so we need to use a different reading method.</summary>
        /// <param name="iPath"></param>
        /// <param name="res"></param>
        /// <param name="type"></param>
        private static void GetEntryNeuronsForAndroid(
            string iPath, System.Collections.Generic.List<Neuron> res, 
            System.Type type)
        {
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                using (var iReader = new System.IO.BinaryReader(iBuffer))
                {
                    var iTemp = new byte[8];
                    var iNrRead = iReader.Read(iTemp, 0, 8);
                    while (iNrRead == 8)
                    {
                        var iId = System.BitConverter.ToUInt64(iTemp, 0);
                        try
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(iId, out iFound))
                            {
                                res.Add(iFound);
                            }
                            else
                            {
                                LogService.Log.LogWarning(
                                    "Storage.GetEntryNeuronsFor", 
                                    string.Format("{0} not found in brain, skipped as entry point.", iId));
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogWarning(
                                "Storage.GetEntryNeuronsFor", 
                                string.Format(
                                    "Failed to find entry point for Sin type: '{0}' with error: {1}.", 
                                    type, 
                                    e));
                        }

                        iNrRead = iReader.Read(iTemp, 0, 8);
                    }
                }
            }
        }

        /// <summary>Gets all entry neurons for all the types. This is used to convert from
        ///     1 storage type to another.</summary>
        /// <param name="DataPath">The Data Path.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        internal static System.Collections.Generic.IList<EntryPoints> GetAllEntryNeurons(string DataPath)
        {
            if (DataPath != null)
            {
                var iRes = new System.Collections.Generic.List<EntryPoints>();
                foreach (var iPath in System.IO.Directory.GetFiles(DataPath, FILEPREFIX + "*" + FILEEXTENTION))
                {
                    var iNew = new EntryPoints();
                    var iTemp = System.IO.Path.GetFileNameWithoutExtension(iPath);
                    iNew.SinType =
                        Brain.Current.GetNeuronType(
                            iTemp.Substring(FILEPREFIX.Length, iTemp.Length - FILEPREFIX.Length));
                    iNew.EntryNeurons = new System.Collections.Generic.List<Neuron>();
                    iRes.Add(iNew);
#if ANDROID // android doesn't return length info, so need to read differntly then in windows.
                  GetAllEntryNeuronsForAndroid(iPath, iNew);
               #else
                    GetAllEntryNeuronsForWindows(iPath, iNew);
#endif
                }

                return iRes;
            }

            LogService.Log.LogError("Storage.GetAllProperties", "No NeuronsPath defined: failed to find entry points.");
            return null;
        }

        /// <summary>reads the entry points on the android platform.</summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        private static void GetAllEntryNeuronsForAndroid(string path, EntryPoints result)
        {
            using (var iStr = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                using (var iReader = new System.IO.BinaryReader(iBuffer))
                {
                    var iTemp = new byte[8];
                    var iNrRead = iReader.Read(iTemp, 0, 8);
                    while (iNrRead == 8)
                    {
                        var iId = System.BitConverter.ToUInt64(iTemp, 0);
                        try
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(iId, out iFound))
                            {
                                result.EntryNeurons.Add(iFound);
                            }
                            else
                            {
                                LogService.Log.LogWarning(
                                    "Storage.GetEntryNeuronsFor", 
                                    string.Format("{0} not found in brain, skipped as entry point.", iId));
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogWarning(
                                "Storage.GetEntryNeuronsFor", 
                                string.Format(
                                    "Failed to find entry point for Sin type: '{0}' with error: {1}.", 
                                    result.SinType, 
                                    e));
                        }

                        iNrRead = iReader.Read(iTemp, 0, 8);
                    }
                }
            }
        }

        /// <summary>reads entry points on the windows platform.</summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        private static void GetAllEntryNeuronsForWindows(string path, EntryPoints result)
        {
            using (var iStr = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                using (var iReader = new System.IO.BinaryReader(iBuffer))
                {
                    var iToRead = (iStr.Length - iStr.Position) / 8;

                        // position and length are slow, faster to precalculate. 64bit = 8 bytes.
                    while (iToRead > 0)
                    {
                        var iId = iReader.ReadUInt64();
                        try
                        {
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(iId, out iFound))
                            {
                                result.EntryNeurons.Add(iFound);
                            }
                            else
                            {
                                LogService.Log.LogWarning(
                                    "Storage.GetEntryNeuronsFor", 
                                    string.Format("{0} not found in brain, skipped as entry point.", iId));
                            }
                        }
                        catch (System.Exception e)
                        {
                            LogService.Log.LogWarning(
                                "Storage.GetEntryNeuronsFor", 
                                string.Format(
                                    "Failed to find entry point for Sin type: '{0}' with error: {1}.", 
                                    result.SinType, 
                                    e));
                        }

                        iToRead--;
                    }
                }
            }
        }
    }
}