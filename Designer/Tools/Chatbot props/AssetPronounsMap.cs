// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetPronounsMap.cs" company="">
//   
// </copyright>
// <summary>
//   provides mapping to the parser for the neurons that repperesent the asset
//   pronouns (what, where, when,...)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides mapping to the parser for the neurons that repperesent the asset
    ///     pronouns (what, where, when,...)
    /// </summary>
    public class AssetPronounsMap : MappedSmallIDCollection
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="AssetPronounsMap"/> class. 
        ///     Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        public AssetPronounsMap()
        {
            Parsers.ParserBase.AssetPronouns = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="AssetPronounsMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public AssetPronounsMap(object owner)
            : base(owner)
        {
            Parsers.ParserBase.AssetPronouns = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="AssetPronounsMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="items">The items.</param>
        public AssetPronounsMap(System.Collections.Generic.List<ulong> items)
            : base(items)
        {
            Parsers.ParserBase.AssetPronouns = Map;
        }

        #endregion
    }
}