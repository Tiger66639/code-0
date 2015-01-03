// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IThesaurusData.cs" company="">
//   
// </copyright>
// <summary>
//   The ThesaurusData interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The ThesaurusData interface.</summary>
    public interface IThesaurusData
    {
        /// <summary>
        ///     Gets the list of items
        /// </summary>
        System.Collections.Generic.IList<ThesaurusItem> Items { get; }

        /// <summary>
        ///     Gets the root of the thesaurus
        /// </summary>
        /// <value>
        ///     The root.
        /// </value>
        Thesaurus Root { get; }
    }
}