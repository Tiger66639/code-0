// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesHelper.cs" company="">
//   
// </copyright>
// <summary>
//   An <see langword="internal" /> class that can be used by the storage
//   systems for managing properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An <see langword="internal" /> class that can be used by the storage
    ///     systems for managing properties.
    /// </summary>
    /// <remarks>
    ///     Handles each entryneurons-type pair as a single binary flat file.
    /// </remarks>
    internal class PropertiesHelper
    {
        /// <summary>The fileextention.</summary>
        private const string FILEEXTENTION = ".xml";

        /// <summary>The typepropsplit.</summary>
        private const string TYPEPROPSPLIT = "_";

        /// <summary><para>Gets all properties that are defined in the storage. This is used for
        ///         transfering from</para>
        /// <list type="number"><item><description>storage system to another one.</description></item>
        /// </list>
        /// </summary>
        /// <param name="DataPath">The Data Path.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        internal static System.Collections.Generic.IList<PropertyValue> GetAllProperties(string DataPath)
        {
            if (DataPath != null)
            {
                var iRes = new System.Collections.Generic.List<PropertyValue>();
                foreach (var iPath in System.IO.Directory.GetFiles(DataPath, "*" + FILEEXTENTION))
                {
                    var iNew = new PropertyValue();
                    var iTemp = System.IO.Path.GetFileNameWithoutExtension(iPath);
                    var iSplit = iTemp.Split(new[] { TYPEPROPSPLIT }, System.StringSplitOptions.None);
                    iNew.SinType = Brain.Current.GetNeuronType(iSplit[0]);
                    iNew.PropertyName = iSplit[1];
                    var iStorageUser = System.Activator.CreateInstance(iNew.SinType) as IStoragePropertiesUser;

                        // we need this to get the exact type of the property.
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                        var valueSerializer =
                            new System.Xml.Serialization.XmlSerializer(iStorageUser.GetTypeForProperty(iSplit[1]));
                        iNew.Value = valueSerializer.Deserialize(iBuffer);
                    }

                    iRes.Add(iNew);
                }

                return iRes;
            }

            LogService.Log.LogError("Storage.GetAllProperties", "No NeuronsPath defined: failed to find properties.");
            return null;
        }

        /// <summary>Gets extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks><para>This is primarely used by sins to retrieve extra information from the
        ///         brain that they require for proper processing.</para>
        /// <para>The data should be stored in xml format.</para>
        /// </remarks>
        /// <typeparam name="T">The <paramref name="type"/> of the property value that should be read.</typeparam>
        /// <param name="DataPath">The Data Path.</param>
        /// <param name="type">The type for which to get the property value.</param>
        /// <param name="id">The id of the property value that should be retrieved.</param>
        /// <returns>The <see cref="T"/>.</returns>
        internal static T GetProperty<T>(string DataPath, System.Type type, string id) where T : class
        {
            if (DataPath != null)
            {
                var iPath = System.IO.Path.Combine(DataPath, type + TYPEPROPSPLIT + id + FILEEXTENTION);
                if (System.IO.File.Exists(iPath))
                {
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var iBuffer = new System.IO.BufferedStream(iStr, Settings.DBBufferSize);
                        var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        return (T)valueSerializer.Deserialize(iBuffer);
                    }
                }

                return null;
            }

            LogService.Log.LogError(
                "Storage.GetProperty", 
                string.Format(
                    "No NeuronsPath defined: failed to find property values for Sin type {0} - {1}.", 
                    type, 
                    id));
            return null;
        }

        /// <summary>Saves extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks>See<see cref="ILongtermMem.GetProperty``1(System.Type,System.String)"/>
        ///     for more info.</remarks>
        /// <param name="DataPath">The Data Path.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The id of the property <paramref name="value"/> that should be saved.</param>
        /// <param name="value">The value that needs to be saved.</param>
        internal static void SaveProperty(string DataPath, System.Type type, string id, object value)
        {
            var iPath = System.IO.Path.Combine(DataPath, type + TYPEPROPSPLIT + id + FILEEXTENTION);
            using (var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(value.GetType());
                valueSerializer.Serialize(iStr, value);
            }
        }

        /// <summary>The get all properties.</summary>
        /// <param name="DataPath">The data path.</param>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        internal static System.Collections.Generic.IList<PropertyValue> GetAllProperties(
            string DataPath, 
            System.Type type)
        {
            if (DataPath != null)
            {
                var iRes = new System.Collections.Generic.List<PropertyValue>();
                var iFilter = System.IO.Path.Combine(DataPath, type + "*." + FILEEXTENTION);
                foreach (var iPath in System.IO.Directory.GetFiles(iFilter))
                {
                    var iNew = new PropertyValue();

                    var iSplit = iPath.Split(new[] { TYPEPROPSPLIT }, System.StringSplitOptions.None);

                    iNew.SinType = Brain.Current.GetNeuronType(iSplit[0]);
                    iNew.PropertyName = iSplit[1];
                    var iStorageUser = System.Activator.CreateInstance(iNew.SinType) as IStoragePropertiesUser;

                        // we need this to get the exact type of the property.
                    using (
                        var iStr = new System.IO.FileStream(iPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var valueSerializer =
                            new System.Xml.Serialization.XmlSerializer(iStorageUser.GetTypeForProperty(iSplit[1]));
                        iNew.Value = valueSerializer.Deserialize(iStr);
                    }

                    iRes.Add(iNew);
                }

                return iRes;
            }

            LogService.Log.LogError("Storage.GetAllProperties", "No NeuronsPath defined: failed to find properties.");
            return null;
        }
    }
}