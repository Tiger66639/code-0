// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionMap.cs" company="">
//   
// </copyright>
// <summary>
//   An id collection that stores a list of neurons which need to be monitored
//   for deletion, and which are used by the do-parser to map identifiers to
//   neurons that can be sent to the reflection sin. This is used for things
//   like 'CmdShell', 'copy',...
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An id collection that stores a list of neurons which need to be monitored
    ///     for deletion, and which are used by the do-parser to map identifiers to
    ///     neurons that can be sent to the reflection sin. This is used for things
    ///     like 'CmdShell', 'copy',...
    /// </summary>
    /// <remarks>
    ///     Note: all strings are stored in lower case, so case insensitive. Auto
    ///     assigns itself to the
    ///     <see cref="JaStDev.HAB.Parsers.DoParser.Functions" />
    /// </remarks>
    public class FunctionMap : MappedSmallIDCollection
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FunctionMap"/> class. 
        ///     Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        public FunctionMap()
        {
            Parsers.DoParser.Functions = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="FunctionMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="owner">The owner of the list.</param>
        public FunctionMap(object owner)
            : base(owner)
        {
            Parsers.DoParser.Functions = Map;
        }

        /// <summary>Initializes a new instance of the <see cref="FunctionMap"/> class. Initializes a new instance of the <see cref="ParserMap"/> class.</summary>
        /// <param name="items">The items.</param>
        public FunctionMap(System.Collections.Generic.List<ulong> items)
            : base(items)
        {
            Parsers.DoParser.Functions = Map;
        }

        #endregion
    }
}