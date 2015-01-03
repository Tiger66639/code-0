// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParserMap.cs" company="">
//   
// </copyright>
// <summary>
//   A parsermap of general statics.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A parsermap of general statics.
    /// </summary>
    public class ParserMap : MappedSmallIDCollection
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParserMap" /> class.
        /// </summary>
        public ParserMap()
        {
            Parsers.ParserBase.Statics = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public ParserMap(object owner)
            : base(owner)
        {
            Parsers.ParserBase.Statics = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="items">The items.</param>
        public ParserMap(System.Collections.Generic.List<ulong> items)
            : base(items)
        {
            Parsers.ParserBase.Statics = Map;
        }

        #endregion
    }
}