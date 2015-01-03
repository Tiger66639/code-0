// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionArgument.cs" company="">
//   
// </copyright>
// <summary>
//   Stores information about a single function argument value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     Stores information about a single function argument value.
    /// </summary>
    public class FunctionArgument
    {
        /// <summary>Initializes a new instance of the <see cref="FunctionArgument"/> class.</summary>
        /// <param name="tokenType">The token type.</param>
        /// <param name="content">The content.</param>
        public FunctionArgument(Token tokenType, string content)
        {
            Content = content;
            TokenType = tokenType;
        }

        /// <summary>
        ///     Gets or sets the type of the token (so we know it is a static, thes
        ///     var, var,...)
        /// </summary>
        /// <value>
        ///     The type of the token.
        /// </value>
        public Token TokenType { get; set; }

        /// <summary>
        ///     Gets or sets the content of the argumet (the string value)
        /// </summary>
        /// <value>
        ///     The content.
        /// </value>
        public string Content { get; set; }
    }
}