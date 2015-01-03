// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tokenizer.cs" company="">
//   
// </copyright>
// <summary>
//   all the tokens that the tokenizer recognises.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     all the tokens that the tokenizer recognises.
    /// </summary>
    public enum Token
    {
        /// <summary>The word.</summary>
        Word, 

        /// <summary>The length spec.</summary>
        LengthSpec, 

        /// <summary>The space.</summary>
        Space, 

        /// <summary>The variable.</summary>
        Variable, 

        /// <summary>The thes variable.</summary>
        ThesVariable, 

        /// <summary>The option start.</summary>
        OptionStart, 

        /// <summary>The option end.</summary>
        OptionEnd, 

        /// <summary>The loop start.</summary>
        LoopStart, 

        /// <summary>The loop end.</summary>
        LoopEnd, 

        /// <summary>The group start.</summary>
        GroupStart, 

        /// <summary>The group end.</summary>
        GroupEnd, 

        /// <summary>The sign.</summary>
        Sign, 

        /// <summary>The choice.</summary>
        Choice, // also serves as 'or'

        /// <summary>The add.</summary>
        Add, 

        /// <summary>The minus.</summary>
        Minus, 

        /// <summary>The multiply.</summary>
        Multiply, 

        /// <summary>The divide.</summary>
        Divide, 

        /// <summary>The modulus.</summary>
        Modulus, 

        /// <summary>The assign.</summary>
        Assign, 

        /// <summary>The not.</summary>
        Not, 

        /// <summary>The and.</summary>
        And, 

        /// <summary>The double and.</summary>
        DoubleAnd, 

        /// <summary>The dot.</summary>
        Dot, 

        /// <summary>The asset variable.</summary>
        AssetVariable, 

        /// <summary>The end.</summary>
        End, 

        /// <summary>The or.</summary>
        Or, 

        /// <summary>The not assign.</summary>
        NotAssign, 

        /// <summary>The not contains.</summary>
        NotContains, 

        /// <summary>The contains.</summary>
        Contains, // not recognized by the scanner, but used to store the operator in the NNLParser.

        /// <summary>The add assign.</summary>
        AddAssign, 

        /// <summary>The add not assign.</summary>
        AddNotAssign, 

        /// <summary>The minus assign.</summary>
        MinusAssign, 

        /// <summary>The and assign.</summary>
        AndAssign, 

        /// <summary>The or assign.</summary>
        OrAssign, 

        /// <summary>The list assign.</summary>
        ListAssign, 

        /// <summary>The multiply assign.</summary>
        MultiplyAssign, 

        /// <summary>The div assign.</summary>
        DivAssign, 

        /// <summary>The modulus assign.</summary>
        ModulusAssign, 

        /// <summary>The is equal.</summary>
        IsEqual, 

        /// <summary>The is bigger then.</summary>
        IsBiggerThen, 

        /// <summary>The is bigger then or equal.</summary>
        IsBiggerThenOrEqual, 

        /// <summary>The is smaller then.</summary>
        IsSmallerThen, 

        /// <summary>The is smaller then or equal.</summary>
        IsSmallerThenOrEqual, 

        /// <summary>The start of input.</summary>
        StartOfInput, 

        /// <summary>The end of input.</summary>
        EndOfInput, 

        /// <summary>The quote.</summary>
        Quote, 

        /// <summary>The single quote.</summary>
        SingleQuote, 

        /// <summary>The arrow right.</summary>
        ArrowRight, 

        /// <summary>The arrow left.</summary>
        ArrowLeft, 

        /// <summary>The topic ref.</summary>
        TopicRef, 

        /// <summary>The question mark.</summary>
        QuestionMark, 

        /// <summary>The point comma.</summary>
        PointComma, 

        /// <summary>The comma.</summary>
        Comma, 

        /// <summary>The by ref.</summary>
        ByRef, // this is used for storing the operator, it is never found byt he parser, cause it's a word.

        /// <summary>The add add.</summary>
        AddAdd, 

        /// <summary>The minus minus.</summary>
        MinusMinus, 

        /// <summary>The at.</summary>
        At
    }

    /// <summary>
    ///     so we can make a distinction between the word type: normal or some word.
    ///     Usefull for parsing the conditions.
    /// </summary>
    public enum WordTokenType
    {
        /// <summary>The word.</summary>
        Word, 

        /// <summary>The integer.</summary>
        Integer, 

        /// <summary>The decimal.</summary>
        Decimal
    }

    /// <summary>
    ///     Converts a stream into a series of tokens that can be consumed by a
    ///     parser. Escape <see langword="char" /> = '\'
    /// </summary>
    public class Tokenizer
    {
        /// <summary>Initializes a new instance of the <see cref="Tokenizer"/> class.</summary>
        /// <param name="value">The value.</param>
        public Tokenizer(string value)
        {
            NextStart = 0;
            CurrentStart = 0;
            ToParse = value;
            GetNext();
        }

        /// <summary>
        ///     Gets the current token as a string.
        /// </summary>
        public string CurrentValue
        {
            get
            {
                if (CurrentToken != Token.End)
                {
                    return ToParse.Substring(CurrentStart, NextStart - CurrentStart).ToLower();

                        // always convert to lower cause this hasn't happened yet and if we don't, we could store something wrong in the textneurons.
                }

                return string.Empty;
            }
        }

        #region ToParse

        /// <summary>
        ///     Gets the name of the object
        /// </summary>
        public string ToParse { get; private set; }

        #endregion

        #region CurrentToken

        /// <summary>
        ///     Gets the currently active token type.
        /// </summary>
        public Token CurrentToken { get; internal set; }

        #endregion

        #region IsEscaped

        /// <summary>
        ///     Gets the value that indicates wether the current word value was
        ///     escaped or not.
        /// </summary>
        public bool IsEscaped { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets or sets a value indicating whether the tokenizer should treat the
        ///     / sign as an escape character, or as a regular sign. By default, this
        ///     is true.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow escaping]; otherwise, <c>false</c> .
        /// </value>
        public bool AllowEscaping
        {
            get
            {
                return fAllowEscaping;
            }

            set
            {
                fAllowEscaping = value;
            }
        }

        #region WordType

        /// <summary>
        ///     Gets the type of the word: integer, <see langword="float" /> or normal.
        ///     Default is word.
        /// </summary>
        /// <remarks>
        ///     This is useful for parsing, so we can convert to numbers instead of
        ///     textneurons.
        /// </remarks>
        public WordTokenType WordType
        {
            get
            {
                return fWordType;
            }

            internal set
            {
                fWordType = value;
            }
        }

        #endregion

        #region CurrentCapitals

        /// <summary>
        ///     Gets the map of all the capital letters for the current word (if any).
        /// </summary>
        public CapMap CurrentCapitals { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets/sets the start of the current value.
        /// </summary>
        public int CurrentStart { get; set; }

        /// <summary>
        ///     Gets the start fo the next token.
        /// </summary>
        public int NextStart { get; set; }

        /// <summary>Gets the symbol string for the token.</summary>
        /// <param name="token">The token.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetSymbolForToken(Token token)
        {
            switch (token)
            {
                case Token.LengthSpec:
                    return ":";
                case Token.Space:
                    return " ";
                case Token.Variable:
                    return "$";
                case Token.ThesVariable:
                    return "^";
                case Token.OptionStart:
                    return "[";
                case Token.OptionEnd:
                    return "]";
                case Token.LoopStart:
                    return "{";
                case Token.LoopEnd:
                    return "}";
                case Token.GroupStart:
                    return "(";
                case Token.GroupEnd:
                    return ")";
                case Token.Choice:
                    return "|";
                case Token.Add:
                    return "+";
                case Token.Minus:
                    return "-";
                case Token.Multiply:
                    return "*";
                case Token.Divide:
                    return "/";
                case Token.Assign:
                    return "=";
                case Token.Not:
                    return "!";
                case Token.DoubleAnd:
                    return "&&";
                case Token.And:
                    return "&";
                case Token.Dot:
                    return ".";
                case Token.Modulus:
                    return "%";
                case Token.AssetVariable:
                    return "#";
                case Token.Or:
                    return "||";
                case Token.NotAssign:
                    return "!=";
                case Token.NotContains:
                    return "!contains";
                case Token.Contains:
                    return "contains";
                case Token.TopicRef:
                    return "~";
                case Token.AddNotAssign:
                    return "!+=";
                case Token.AddAssign:
                    return "+=";
                case Token.MinusAssign:
                    return "-=";
                case Token.AndAssign:
                    return "&=";
                case Token.ListAssign:
                    return ";=";
                case Token.MultiplyAssign:
                    return "*=";
                case Token.DivAssign:
                    return "/=";
                case Token.ModulusAssign:
                    return "%=";
                case Token.IsEqual:
                    return "==";
                case Token.IsBiggerThen:
                    return ">";
                case Token.IsBiggerThenOrEqual:
                    return ">=";
                case Token.IsSmallerThen:
                    return "<";
                case Token.IsSmallerThenOrEqual:
                    return "<=";
                case Token.OrAssign:
                    return "|=";
                case Token.StartOfInput:
                    return "|<";
                case Token.EndOfInput:
                    return ">|";
                case Token.SingleQuote:
                    return "'";
                case Token.Quote:
                    return "\"";
                case Token.ArrowRight:
                    return "->";
                case Token.ArrowLeft:
                    return "<-";
                case Token.QuestionMark:
                    return "?";
                case Token.PointComma:
                    return ";";
                case Token.Comma:
                    return ",";
                case Token.ByRef:
                    return "Ref";
                case Token.AddAdd:
                    return "++";
                case Token.MinusMinus:
                    return "--";
                case Token.At:
                    return "@";
                default:
                    break;
            }

            return null;
        }

        /// <summary>
        ///     Goes to the next item in the stream and determins what it is. When the
        ///     escape <see langword="char" /> '\' is found, any subsequent text is
        ///     treated as a word.
        /// </summary>
        public void GetNext()
        {
            IsEscaped = false;
            fWordType = WordTokenType.Word;

                // make certain that we reset the wordtype so that nothing gets interpreted incorrectly after a number was found.
            CurrentCapitals = null; // we always reset the capmap, so we can build a new one.
            CurrentStart = NextStart;
            NextStart++;
            if (ToParse != null && CurrentStart < ToParse.Length)
            {
                if (ToParse[CurrentStart] == '\\' && AllowEscaping)
                {
                    IsEscaped = true;
                    CurrentStart++; // the actual escape token doesn't need to be inlcuded with the output.
                    NextStart++;
                    if (CurrentStart > ToParse.Length - 1)
                    {
                        throw new System.InvalidOperationException(
                            "An escape character needs to be followed by at least 1 more character.");
                    }

                    GetNextToken();

                        // this advances the cursor as if it was a normal token, once this is done, we can convert it to a string. this way, we can use the same parser for the escape value.
                    CurrentToken = Token.Word;
                }
                else
                {
                    GetNextToken();
                }
            }
            else
            {
                CurrentToken = Token.End;
            }
        }

        /// <summary>
        ///     Gets the next token without taking any escape <see langword="char" />
        ///     into account.
        /// </summary>
        private void GetNextToken()
        {
            if (ToParse[CurrentStart] == ':')
            {
                // we check start and not end, cause we are checking the first char of the item + the end points to the next new item.
                CurrentToken = Token.LengthSpec;

                    // always return this val even if prev was not a var: this is up to the parser to handle correctly.
            }
            else if (char.IsSeparator(ToParse, CurrentStart) || char.IsControl(ToParse, CurrentStart))
            {
                // while (fPartEnd < fToParse.Length && char.IsSeparator(fToParse, fPartEnd) == true)      //we count a space for 1 char, so that it easely converts into the same neuron, and not in different neurons for different lengths.
                // fPartEnd++;
                CurrentToken = Token.Space;
            }
            else if (ToParse[CurrentStart] == '$')
            {
                CurrentToken = Token.Variable;
            }
            else if (ToParse[CurrentStart] == '^')
            {
                CurrentToken = Token.ThesVariable;
            }
            else if (ToParse[CurrentStart] == '~')
            {
                CurrentToken = Token.TopicRef;
            }
            else if (ToParse[CurrentStart] == '[')
            {
                CurrentToken = Token.OptionStart;
            }
            else if (ToParse[CurrentStart] == ']')
            {
                CurrentToken = Token.OptionEnd;
            }
            else if (ToParse[CurrentStart] == '{')
            {
                CurrentToken = Token.LoopStart;
            }
            else if (ToParse[CurrentStart] == '}')
            {
                CurrentToken = Token.LoopEnd;
            }
            else if (ToParse[CurrentStart] == '"')
            {
                CurrentToken = Token.Quote;
            }
            else if (ToParse[CurrentStart] == '\'')
            {
                CurrentToken = Token.SingleQuote;
            }
            else if (ToParse[CurrentStart] == '(')
            {
                CurrentToken = Token.GroupStart;
            }
            else if (ToParse[CurrentStart] == ')')
            {
                CurrentToken = Token.GroupEnd;
            }
            else if (ToParse[CurrentStart] == ',')
            {
                CurrentToken = Token.Comma;
            }
            else if (ToParse[CurrentStart] == '@')
            {
                CurrentToken = Token.At;
            }
            else if (ToParse[CurrentStart] == '*')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.MultiplyAssign;
                }
                else
                {
                    CurrentToken = Token.Multiply;
                }
            }
            else if (ToParse[CurrentStart] == '/')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.DivAssign;
                }
                else
                {
                    CurrentToken = Token.Divide;
                }
            }
            else if (ToParse[CurrentStart] == '%')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.ModulusAssign;
                }
                else
                {
                    CurrentToken = Token.Modulus;
                }
            }
            else if (ToParse[CurrentStart] == '+')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.AddAssign;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '+')
                {
                    NextStart++;
                    CurrentToken = Token.AddAdd;
                }
                else
                {
                    CurrentToken = Token.Add;
                }
            }
            else if (ToParse[CurrentStart] == '<')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.IsSmallerThenOrEqual;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '-')
                {
                    NextStart++;
                    CurrentToken = Token.ArrowLeft;
                }
                else
                {
                    CurrentToken = Token.IsSmallerThen;
                }
            }
            else if (ToParse[CurrentStart] == '>')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.IsBiggerThenOrEqual;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '|')
                {
                    NextStart++;
                    CurrentToken = Token.EndOfInput;
                }
                else
                {
                    CurrentToken = Token.IsBiggerThen;
                }
            }
            else if (ToParse[CurrentStart] == '-')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.MinusAssign;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '>')
                {
                    NextStart++;
                    CurrentToken = Token.ArrowRight;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '-')
                {
                    NextStart++;
                    CurrentToken = Token.MinusMinus;
                }
                else
                {
                    CurrentToken = Token.Minus;
                }
            }
            else if (ToParse[CurrentStart] == '=')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.IsEqual;
                }
                else
                {
                    CurrentToken = Token.Assign;
                }
            }
            else if (ToParse[CurrentStart] == '!')
            {
                if (NextStart + 2 < ToParse.Length && ToParse.Substring(CurrentStart, 3).ToLower() == "!+=")
                {
                    NextStart += 2;
                    CurrentToken = Token.AddNotAssign;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.NotAssign;
                }
                else if (CurrentStart + 9 < ToParse.Length && ToParse.Substring(CurrentStart, 9).ToLower() == "!contains")
                {
                    NextStart += 9;
                    CurrentToken = Token.NotContains;
                }
                else
                {
                    CurrentToken = Token.Not;
                }
            }
            else if (ToParse[CurrentStart] == '|')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '|')
                {
                    NextStart++;
                    CurrentToken = Token.Or;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.OrAssign;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '<')
                {
                    NextStart++;
                    CurrentToken = Token.StartOfInput;
                }
                else
                {
                    CurrentToken = Token.Choice;
                }
            }
            else if (ToParse[CurrentStart] == '.')
            {
                CurrentToken = Token.Dot;
            }
            else if (ToParse[CurrentStart] == '#')
            {
                CurrentToken = Token.AssetVariable;
            }
            else if (ToParse[CurrentStart] == '&')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '&')
                {
                    NextStart++;
                    CurrentToken = Token.DoubleAnd;
                }
                else if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.AndAssign;
                }
                else
                {
                    CurrentToken = Token.And;
                }
            }
            else if (ToParse[CurrentStart] == ';')
            {
                if (NextStart < ToParse.Length && ToParse[NextStart] == '=')
                {
                    NextStart++;
                    CurrentToken = Token.ListAssign;
                }
                else
                {
                    CurrentToken = Token.PointComma;
                }
            }
            else if (ToParse[CurrentStart] == '?')
            {
                CurrentToken = Token.QuestionMark;
            }
            else if (char.IsSymbol(ToParse, CurrentStart) || char.IsPunctuation(ToParse, CurrentStart))
            {
                CurrentToken = Token.Sign;
            }
            else if (char.IsLetterOrDigit(ToParse, CurrentStart))
            {
                ReadWord();
                CurrentToken = Token.Word;
            }
            else
            {
                CurrentToken = Token.Sign;
            }

            // throw new InvalidOperationException(string.Format("Invaid character ('{0}') in pattern definition '{1}' at position {2}", fToParse[fPartStart], fToParse, fPartEnd)); //we use partEnd cause this is 1 based.
        }

        /// <summary>
        ///     Reads a word, which can be either an integer, <see langword="double" />
        ///     or normal word. When it's a number, an extra sub val is stored so that
        ///     it's easely recognized, but the main tag remains on word for easy
        ///     parsing (so that not much needs to be changed, default behaviour).
        /// </summary>
        private void ReadWord()
        {
            if (char.IsDigit(ToParse, CurrentStart))
            {
                WordType = WordTokenType.Integer;
            }
            else
            {
                WordType = WordTokenType.Word;
                SetCapmap(char.IsUpper(ToParse, CurrentStart), 0);
            }

            while (NextStart < ToParse.Length && char.IsLetterOrDigit(ToParse, NextStart))
            {
                if (char.IsDigit(ToParse, NextStart) == false)
                {
                    WordType = WordTokenType.Word;
                    SetCapmap(char.IsUpper(ToParse, NextStart), NextStart - CurrentStart);
                }

                NextStart++;
            }

            if (NextStart + 1 < ToParse.Length && WordType == WordTokenType.Integer && ToParse[NextStart] == '.')
            {
                // it could be a float, check if the next is also a nr, then we are in business.
                if (char.IsDigit(ToParse, NextStart + 1))
                {
                    NextStart += 2;
                    WordType = WordTokenType.Decimal;
                    while (NextStart < ToParse.Length && char.IsDigit(ToParse, NextStart))
                    {
                        NextStart++;
                    }
                }
            }
        }

        /// <summary>Sets the capmap + makes certain that there is one.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="index">The index.</param>
        private void SetCapmap(bool value, int index)
        {
            if (CurrentCapitals == null)
            {
                CurrentCapitals = new CapMap();
            }

            CurrentCapitals.AddCap(value, index);
        }

        /// <summary>Gets if the specified word is at the curent position.</summary>
        /// <param name="toCompare"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsWord(string toCompare)
        {
            return CurrentToken == Token.Word && IsEscaped == false && CurrentValue == toCompare;
        }

        /// <summary>Determines whether the current token is a word and matches one of hte
        ///     words in the argument list.</summary>
        /// <param name="toCompare">To compare.</param>
        /// <returns><c>true</c> if the specified to compare is word; otherwise,<c>false</c> .</returns>
        public bool IsWord(System.Collections.Generic.IEnumerable<string> toCompare)
        {
            return CurrentToken == Token.Word && IsEscaped == false && Enumerable.Contains(toCompare, CurrentValue);
        }

        /// <summary>Splits the specified <paramref name="text"/> into it's parts. Spaces
        ///     and newlines are ommited.</summary>
        /// <param name="text">The text to split.</param>
        /// <param name="includeSpace">if set to <c>true</c> spaces are included, otherwise they are skipped.</param>
        /// <returns>A list of strings, which together form the input string.</returns>
        public static string[] Split(string text, bool includeSpace = false)
        {
            var iSplitter = new Tokenizer(text);
            iSplitter.AllowEscaping = false;
            var iRes = new System.Collections.Generic.List<string>();
            while (iSplitter.CurrentToken != Token.End)
            {
                if (includeSpace || iSplitter.CurrentToken != Token.Space)
                {
                    iRes.Add(iSplitter.CurrentValue);
                }

                iSplitter.GetNext();
            }

            return iRes.ToArray();
        }

        #region fields

        /// <summary>The f word type.</summary>
        private WordTokenType fWordType = WordTokenType.Word;

        /// <summary>The f allow escaping.</summary>
        private bool fAllowEscaping = true;

        #endregion
    }
}