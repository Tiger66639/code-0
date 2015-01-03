// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILongtermMem.cs" company="">
//   
// </copyright>
// <summary>
//   A class used to get all the available entry neurons in a storage,
//   together with the sin type to which they belong. This is used for
//   converting 1 storage format to another.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A class used to get all the available entry neurons in a storage,
    ///     together with the sin type to which they belong. This is used for
    ///     converting 1 storage format to another.
    /// </summary>
    /// <remarks>
    ///     Use class instead of struct, this is saver to use with lists than
    ///     structs.
    /// </remarks>
    public class EntryPoints
    {
        /// <summary>
        ///     Gets or sets the entry neurons.
        /// </summary>
        /// <value>
        ///     The entry neurons.
        /// </value>
        public System.Collections.Generic.IList<Neuron> EntryNeurons { get; set; }

        /// <summary>
        ///     Gets or sets the type of the sin.
        /// </summary>
        /// <value>
        ///     The type of the sin.
        /// </value>
        public System.Type SinType { get; set; }
    }

    /// <summary>
    ///     A class used to get all the available property values in a storage.
    ///     Together with the sin type and property name. This is used for converting
    ///     1 storage format to another.
    /// </summary>
    /// <remarks>
    ///     Use class instead of struct, this is saver to use with lists than
    ///     structs.
    /// </remarks>
    public class PropertyValue
    {
        /// <summary>
        ///     Gets or sets the type of the sin.
        /// </summary>
        /// <value>
        ///     The type of the sin.
        /// </value>
        public System.Type SinType { get; set; }

        /// <summary>
        ///     Gets or sets the name of the property.
        /// </summary>
        /// <value>
        ///     The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        ///     Gets or sets the value for the propery.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public object Value { get; set; }
    }

    /// <summary>
    ///     Provides an <see langword="interface" /> for objects that provide long
    ///     term storage of <see cref="Neuron" /> objects (sreamed to file or to
    ///     database,...).
    /// </summary>
    public interface ILongtermMem : System.IDisposable
    {
        #region Prop

        /// <summary>
        ///     Gets or sets the path where the data should be saved to. Setting this
        ///     property to <see langword="null" /> also closes the db.
        /// </summary>
        /// <remarks>
        ///     You must always call <see cref="ILongTermMem.MakePathAbsoluteTo" /> in
        ///     order to make certain that all the data files are loaded. Simply
        ///     setting the path doesn't do it.
        /// </remarks>
        /// <value>
        ///     The directory as a string.
        /// </value>
        string DataPath { get; set; }

        #endregion

        /// <summary>Instructs the LongTermMem to make the<see cref="ILongTermMem.DataPath"/> relative to the specified<paramref name="path"/> so that the info can be streamed together with
        ///     the project.</summary>
        /// <param name="path">The path.</param>
        void MakePathRelativeTo(string path);

        /// <summary>Instructs the LongTermMem to make the <paramref name="path"/> absolute
        ///     so that the data can be loaded.</summary>
        /// <param name="path">The path to make the <see cref="ILongTermMem.DataPath"/> absolute
        ///     against.</param>
        /// <param name="readOnly">The read Only.</param>
        void MakePathAbsoluteTo(string path, bool readOnly = false);

        /// <summary>Tries to load the neuron with the specified ID</summary>
        /// <remarks>If the <paramref name="id"/> can't be found <see langword="null"/>
        ///     should be returned.</remarks>
        /// <param name="id">The id of the neuron to load.</param>
        /// <returns>The object that was loaded or <see langword="null"/> if not found.</returns>
        LinkResolverData GetNeuron(ulong id);

        /// <summary>Tries to save the neuron to the long time memory, using the id of the
        ///     neuron as a key.</summary>
        /// <remarks>When implementing this function, it is not required to lock the entire
        ///     neuron, nor is it required to change<see cref="JaStDev.HAB.Neuron.IsChanged"/> . This is all done by the
        ///     caller of this function.</remarks>
        /// <param name="toSave">The object to save.</param>
        void SaveNeuron(Neuron toSave);

        /// <summary>Tries to save the neuron to the long time memory, using the specified<paramref name="id"/> as a key.</summary>
        /// <remarks>Deprecated.</remarks>
        /// <param name="toSave">To save.</param>
        /// <param name="id">The id.</param>
        void SaveNeuron(Neuron toSave, ulong id);

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The object to remove</param>
        void RemoveNeuron(Neuron toRemove);

        /// <summary>Removes the specified <see cref="Neuron"/> from the long term memory.</summary>
        /// <param name="toRemove">The id of the neuron to remove.</param>
        void RemoveNeuron(ulong toRemove);

        /// <summary>Copies all the data to the specified location. This operation doesn't
        ///     effect the value of <see cref="JaStDev.HAB.ILongtermMem.DataPath"/> .</summary>
        /// <param name="loc">The location to copy the data to.</param>
        void CopyTo(string loc);

        /// <summary>Retrieves a list of neurons that form the entry points for a<see cref="Sin"/> .</summary>
        /// <remarks>A sin can only have 1 set of entrypoints. If more control over data
        ///     saving is required, use<see cref="ILongtermMem.GetProperty``1(System.Type,System.String)"/></remarks>
        /// <param name="type">The <see cref="Sin"/> type for which to retrieve the data</param>
        /// <returns>The <see cref="IList"/>.</returns>
        System.Collections.Generic.IList<Neuron> GetEntryNeuronsFor(System.Type type);

        /// <summary>Gets all entry neurons for all the types. This is used to convert from
        ///     1 storage type to another.</summary>
        /// <returns>The <see cref="IList"/>.</returns>
        System.Collections.Generic.IList<EntryPoints> GetAllEntryNeurons();

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified type.</summary>
        /// <param name="list">The list of neuron id's that need to be stored as entry points.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<ulong> list, System.Type type);

        /// <summary>Stores the <paramref name="list"/> of neurons as the default entry
        ///     points for the specified <paramref name="type"/> but uses a<paramref name="list"/> of neurons instead of ids.</summary>
        /// <param name="list">The list of neurons that need to be stored as entry points. Note that
        ///     these Neurons are already stored as regular neurons in the brain, so
        ///     you should only store the Id of the neurons.</param>
        /// <param name="type">The <see cref="Sin"/> type for which to perform the store.</param>
        void SaveEntryNeuronsFor(System.Collections.Generic.IEnumerable<Neuron> list, System.Type type);

        /// <summary>Gets extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks><para>Types that use propeerties should also implement<see cref="IStoragePropertiesUser"/> .</para>
        /// <para>This is primarely used by sins to retrieve extra information from the
        ///         brain that they require for proper processing. The<see cref="VerbNet"/> class also uses this during the import of data,
        ///         to find already generated roles for the project.</para>
        /// <para>The data should be stored in xml format.</para>
        /// </remarks>
        /// <typeparam name="T">The <paramref name="type"/> of the property value that should be read.</typeparam>
        /// <param name="type">The type for which to get the property value.</param>
        /// <param name="id">The id of the property value that should be retrieved.</param>
        /// <returns>The <see cref="T"/>.</returns>
        T GetProperty<T>(System.Type type, string id) where T : class;

        /// <summary>Saves extra data associated with the specified <paramref name="type"/>
        ///     and <paramref name="id"/> from the store.</summary>
        /// <remarks><para>Types that use propeerties should also implement<see cref="IStoragePropertiesUser"/> .</para>
        /// <para>See<see cref="ILongtermMem.GetProperty``1(System.Type,System.String)"/>
        ///         for more info.</para>
        /// </remarks>
        /// <param name="type">The type.</param>
        /// <param name="id">The id of the property <paramref name="value"/> that should be saved.</param>
        /// <param name="value">The value that needs to be saved.</param>
        void SaveProperty(System.Type type, string id, object value);

        /// <summary><para>Gets all properties that are defined in the storage. This is used for
        ///         transfering from</para>
        /// <list type="number"><item><description>storage system to another one.</description></item>
        /// </list>
        /// </summary>
        /// <returns>The <see cref="IList"/>.</returns>
        System.Collections.Generic.IList<PropertyValue> GetAllProperties();

        /// <summary>Determines whether the specified <paramref name="id"/> exists.</summary>
        /// <param name="id">The id to look for.</param>
        /// <returns><c>true</c> if the <paramref name="id"/> exists in the db; otherwise,<c>false</c> .</returns>
        bool IsExistingID(ulong id);

        /// <summary>Gets all the properties for a specified type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The list of prop values for the type, or <see langword="null"/> if
        ///     non.</returns>
        System.Collections.Generic.IList<PropertyValue> GetAllProperties(System.Type type);

        /// <summary>Marks the specified id as free but doesn't delete any neurons. This is
        ///     used for cleaning up the database (2 id's returning the same neuron).</summary>
        /// <param name="bad">The bad.</param>
        void MarkAsFree(ulong bad);

        /// <summary>
        ///     Makes certain that everything is properly written to file and nothing
        ///     is left in the buffers anymore.
        /// </summary>
        void Flush();

        /// <summary>Makes certain that everything is properly written to file and nothing
        ///     is left in the buffers anymore after saving a single neuron</summary>
        /// <param name="id">The id.</param>
        void Flush(ulong id);

        /// <summary>
        ///     flushes the data without flushing any index files if this can be
        ///     avoided (when they are buffered in memory).
        /// </summary>
        void FlushFast();
    }
}