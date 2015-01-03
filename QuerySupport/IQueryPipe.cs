// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueryPipe.cs" company="">
//   
// </copyright>
// <summary>
//   this <see langword="interface" /> should be implemented by data providers
//   that are able to feed data to a query.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     this <see langword="interface" /> should be implemented by data providers
    ///     that are able to feed data to a query.
    /// </summary>
    public interface IQueryPipe : System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<Neuron>>
    {
        /// <summary>
        ///     makes certain that everything is loaded into memory.
        /// </summary>
        void TouchMem();

        /// <summary>
        ///     called when extra data needs to be saved to disk in seperate files.
        /// </summary>
        void Flush();

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        void WriteV1(System.IO.BinaryWriter writer);

        /// <summary>read the settings from a stream.</summary>
        /// <param name="iTempReader"></param>
        void ReadV1(System.IO.BinaryReader iTempReader);

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a
        ///     duplicate, its' ok to return a new <see langword="enum"/> that goes
        ///     back to the start (a warning should be rendered that splits are not
        ///     supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum);

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The <see langword="enum"/> to move passed the end.</param>
        void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum);
    }
}