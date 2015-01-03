// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISearchDataProvider.cs" company="">
//   
// </copyright>
// <summary>
//   This <see langword="interface" /> should be implemented by any part that
//   wants to provide advanced search capabilities that are integrated into
//   the designer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     This <see langword="interface" /> should be implemented by any part that
    ///     wants to provide advanced search capabilities that are integrated into
    ///     the designer.
    /// </summary>
    public interface ISearchDataProvider
    {
        /// <summary>
        ///     Gets/sets the process that performs the actual search, async.
        /// </summary>
        SearcherProcess Process { get; set; }

        /// <summary>Gets the data that needs to be searched in the form of neuron id's.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        System.Collections.Generic.IEnumerable<ulong> GetData();

        /// <summary>Continues to get the data from when we stopped at the previous run.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        System.Collections.Generic.IEnumerable<ulong> ContinueData();

        /// <summary>
        ///     This function is called when a valid search result is found and the
        ///     object at the current pointer position should be selected.
        /// </summary>
        void SelectCurrent();

        /// <summary>
        ///     Called when the search process has reached the end.
        /// </summary>
        void Finished();

        /// <summary>Continues to get the data from when we stopped at the previous run
        ///     (while searching id's and strings).</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        System.Collections.Generic.IEnumerable<object> ContinueObjectData();

        /// <summary>Gets the data that needs to be searched in the form of neuron id's and
        ///     strings.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        System.Collections.Generic.IEnumerable<object> GetObjectData();
    }
}