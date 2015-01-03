// --------------------------------------------------------------------------------------------------------------------
// <copyright file="wordnetDataSet.cs" company="">
//   
// </copyright>
// <summary>
//   The wordnet data set.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The wordnet data set.</summary>
    public class wordnetDataSet
    {
        /// <summary>The compund words data table.</summary>
        private class CompundWordsDataTable
        {
        }

        /// <summary>
        ///     Contains all the synsets that are build from multiple words and which
        ///     contain the specified word.
        /// </summary>
        /// <remarks>
        ///     Note: when specifying the word to search for, don't forget to put a %
        ///     in front and in the back.
        /// </remarks>
        private class oldCompundWordsDataTable
        {
        }

        /// <summary>
        ///     Contains all the synonyms of a specific symset.
        /// </summary>
        private class SynonymsDataTable
        {
        }

        /// <summary>
        ///     Performs a lookup for all the related words (from a specific
        ///     relationship) of a specific word sense.
        /// </summary>
        private class RelatedWordsOldDataTable
        {
        }

        /// <summary>
        ///     Performs a lookup for 1 word to find it's different meanings + small
        ///     description.
        /// </summary>
        private class WordInfoDataTable
        {
        }
    }
}

namespace JaStDev.HAB.wordnetDataSetTableAdapters
{
    /// <summary>The check root table adapter.</summary>
    internal class CheckRootTableAdapter
    {
    }

    /// <summary>The related words table adapter.</summary>
    internal class RelatedWordsTableAdapter
    {
    }

    /// <summary>The relationships table adapter.</summary>
    public class RelationshipsTableAdapter
    {
    }
}