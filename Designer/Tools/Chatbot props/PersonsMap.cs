// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonsMap.cs" company="">
//   
// </copyright>
// <summary>
//   a parsermap for I, you, he/she, we, they, it, me, mine, your, yours,
//   his/hers,....
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a parsermap for I, you, he/she, we, they, it, me, mine, your, yours,
    ///     his/hers,....
    /// </summary>
    public class PersonsMap : MappedSmallIDCollection
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="PersonsMap"/> class. 
        ///     Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        public PersonsMap()
        {
            BaseXmlStreamer.PersonMap = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="PersonsMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public PersonsMap(object owner)
            : base(owner)
        {
            BaseXmlStreamer.PersonMap = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="PersonsMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="items">The items.</param>
        public PersonsMap(System.Collections.Generic.List<ulong> items)
            : base(items)
        {
            BaseXmlStreamer.PersonMap = Map;
        }

        #endregion
    }
}