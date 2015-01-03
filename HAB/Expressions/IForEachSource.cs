// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IForEachSource.cs" company="">
//   
// </copyright>
// <summary>
//   this <see langword="interface" /> needs to be implemented by objects that
//   can provide a data source for the <see langword="foreach" /> statement's
//   list of items to loop through.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     this <see langword="interface" /> needs to be implemented by objects that
    ///     can provide a data source for the <see langword="foreach" /> statement's
    ///     list of items to loop through.
    /// </summary>
    public interface IForEachSource
    {
        /// <summary>Gets the enumerator that can be used to get the data source items.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator();

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