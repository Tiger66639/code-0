// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParserBase.cs" company="">
//   
// </copyright>
// <summary>
//   Stores the line and column number.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     Stores the line and column number.
    /// </summary>
    public struct FilePos
    {
        /// <summary>
        ///     the column on the line.
        /// </summary>
        public int Column;

        /// <summary>
        ///     the line number.
        /// </summary>
        public int Line;

        /// <summary>
        ///     the total nr of character compared to the start of the file.
        /// </summary>
        public int Pos;
    }

    /// <summary>
    ///     a base class for pattern parsers.
    /// </summary>
    public abstract class ParserBase
    {
        #region fields

        /// <summary>The f scanner.</summary>
        private Tokenizer fScanner;

        /// <summary>The f last token pos.</summary>
        private FilePos fLastTokenPos;

        /// <summary>The f cur line.</summary>
        private int fCurLine = 1;

        /// <summary>The f process comments.</summary>
        protected bool fProcessComments = false;

        /// <summary>The f error count.</summary>
        private int fErrorCount; // so we don't render to many errors.

        /// <summary>Initializes static members of the <see cref="ParserBase"/> class.</summary>
        static ParserBase()
        {
            BlockLogErrors = false;
        }

        /// <summary>Initializes a new instance of the <see cref="ParserBase"/> class.</summary>
        public ParserBase()
        {
            HasErrors = false;
        }

        #endregion

        #region prop

        #region Scanner

        /// <summary>Gets or sets the scanner.</summary>
        protected Tokenizer Scanner
        {
            get
            {
                return fScanner;
            }

            set
            {
                if (fScanner != value)
                {
                    fScanner = value;
                }
            }
        }

        #endregion

        #region ParserTitle

        /// <summary>
        ///     Gets the parser title. Used to generate errors.
        /// </summary>
        protected virtual string ParserTitle { get; set; }

        #endregion

        #region Statics

        /// <summary>
        ///     Gets/sets the list of statics words and their mappings to neuron ids
        ///     so that we can convert words to neurons, like 'present particle',
        ///     'user',...
        /// </summary>
        /// <remarks>
        ///     This dictionay should be filled
        /// </remarks>
        public static System.Collections.Generic.Dictionary<string, ulong> Statics { get; set; }

        #endregion

        #region AssetPronouns

        /// <summary>
        ///     Gets/sets the list of statics words and their mappings to neuron ids
        ///     so that we can convert words to neurons, like 'present particle',
        ///     'user',...
        /// </summary>
        /// <remarks>
        ///     This dictionay should be filled
        /// </remarks>
        public static System.Collections.Generic.Dictionary<string, ulong> AssetPronouns { get; set; }

        #endregion

        #region BlockLogErrors

        /// <summary>
        ///     Gets/sets the value that indicates if errors are logged and thrown or
        ///     only thrown. This is used so we can import patterns in batch and
        ///     report the errors when all have been processed, so that intermediate
        ///     errors (due to missing topics) don't report <see langword="false" />
        ///     negatives.
        /// </summary>
        public static bool BlockLogErrors { get; set; }

        #endregion

        #region LastTokenPos

        /// <summary>
        ///     Gets/sets the position of the end of the last token that was read
        ///     before skipping the spaces. This is used to get the end pos for the
        ///     parse nodes.
        /// </summary>
        public FilePos LastTokenPos
        {
            get
            {
                return fLastTokenPos;
            }

            set
            {
                fLastTokenPos = value;
            }
        }

        #endregion

        #region CurLine

        /// <summary>
        ///     Gets the current line number.
        /// </summary>
        public int CurLine
        {
            get
            {
                return fCurLine;
            }

            internal set
            {
                fCurLine = value;
            }
        }

        #endregion

        #region CurLineAt

        /// <summary>
        ///     Gets the character position of the first character in the current
        ///     line. This allows us to calculate the position on the current line.
        /// </summary>
        public int CurLineAt { get; internal set; }

        #endregion

        #region HasErrors

        /// <summary>
        ///     Gets the value that indicates if there were errors during the parse or
        ///     not.
        /// </summary>
        public bool HasErrors { get; internal set; }

        #endregion

        #endregion

        #region functinos

        /// <summary>
        ///     resets the current position.
        /// </summary>
        public void Reset()
        {
            CurLineAt = 0;
            CurLine = 1;
            HasErrors = false;
        }

        /// <summary>Logs the error.</summary>
        /// <param name="error">The error.</param>
        /// <param name="fail">The fail.</param>
        protected void LogError(string error, bool fail = true)
        {
            fErrorCount++;
            if (BlockLogErrors == false)
            {
                if (fErrorCount > 200)
                {
                    // if we render to many errors, make certain that the first remain visible.
                    BlockLogErrors = true;
                    LogService.Log.LogError("Parser", "To many errors");
                }
                else
                {
                    var iLine = Scanner.ToParse.Substring(CurLineAt, GetEndOfCurLine() - CurLineAt);
                    LogService.Log.LogError(ParserTitle, string.Format("{0}: {1}", error, iLine));
                }
            }

            HasErrors = true;
            if (fail)
            {
                throw new System.InvalidOperationException(error);
            }
        }

        /// <summary>Calculates the end position of the current line.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetEndOfCurLine()
        {
            var iRes = CurLineAt;
            while (iRes < Scanner.ToParse.Length && Scanner.ToParse[iRes] != '\n' && Scanner.ToParse[iRes] != '\r')
            {
                iRes++;
            }

            return iRes;
        }

        /// <summary>Logs an <paramref name="error"/> and adds the current pattern and
        ///     position to the <paramref name="error"/> message.</summary>
        /// <param name="error">The error.</param>
        /// <param name="fail">The fail.</param>
        public virtual void LogPosError(string error, bool fail = true)
        {
            LogError(string.Format("L:{0} C:{1} {2}", CurLine, 1 + (Scanner.CurrentStart - CurLineAt), error), fail);
        }

        /// <summary>Logs the warning + position, continues parse.</summary>
        /// <param name="error">The error.</param>
        public void LogPosWarning(string error)
        {
            var iLine = Scanner.ToParse.Substring(CurLineAt, GetEndOfCurLine() - CurLineAt);

            // if (fBlockLogErrors == false)
            LogService.Log.LogWarning(
                ParserTitle, 
                string.Format("L:{0} C:{1} {2}  ({3})", CurLine, 1 + (Scanner.CurrentStart - CurLineAt), error, iLine));
        }

        /// <summary>Logs the warning + position, continues parse.</summary>
        /// <param name="error">The error.</param>
        protected void LogPosInfo(string error)
        {
            var iLine = Scanner.ToParse.Substring(CurLineAt, GetEndOfCurLine() - CurLineAt);

            // if (fBlockLogErrors == false)
            LogService.Log.LogInfo(
                ParserTitle, 
                string.Format("L:{0} C:{1} {2}  ({3})", CurLine, 1 + (Scanner.CurrentStart - CurLineAt), error, iLine));
        }

        /// <summary>
        ///     Moves the cursor position forward till the first non 'space'
        ///     character. If the current is a non space, the cursor position remains
        ///     the same.
        /// </summary>
        protected void SkipSpaces()
        {
            var iPrev = (char)0;
            while (Scanner.CurrentToken == Token.Space || ProcessComments())
            {
                // skip the first space and any comments
                var iCur = Scanner.ToParse[Scanner.CurrentStart];
                AdjustCurLine(iPrev);
                Scanner.GetNext();
                iPrev = iCur;
            }
        }

        /// <summary>checks if the Curline value needs to be increased and does this if
        ///     required.</summary>
        /// <param name="iPrev"></param>
        private void AdjustCurLine(char iPrev)
        {
            if ((Scanner.CurrentValue == "\r") || (Scanner.CurrentValue == "\n" && iPrev != '\r'))
            {
                // need to check for /r /n and /r/n 
                CurLine++;
                CurLineAt = Scanner.NextStart;
            }
            else if (Scanner.CurrentValue == "\n" && iPrev == '\r')
            {
                // when /n/r or /r/n , need add 1 more position to the start of the current line
                CurLineAt++;
            }
        }

        /// <summary>Checks if comments need to be skipped and if the current parser pos is
        ///     a comment. If so, it will set the end at the end of the comment so
        ///     that it can be completely consumed at the next call of GetNext.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessComments()
        {
            if (fProcessComments)
            {
                var iToParse = Scanner.ToParse;
                if (Scanner.NextStart < Scanner.ToParse.Length)
                {
                    if (iToParse[Scanner.CurrentStart] == '/' && iToParse[Scanner.NextStart] == '/')
                    {
                        Scanner.NextStart += 2;
                        while (Scanner.NextStart <= iToParse.Length && iToParse[Scanner.NextStart - 1] != '\n'
                               && iToParse[Scanner.NextStart - 1] != '\r')
                        {
                            // only filter out 1 cuase this type of comment always needs to be interpreted as a newline.
                            Scanner.NextStart++;
                        }

                        CurLine++;
                        CurLineAt = Scanner.NextStart;
                        if (Scanner.NextStart < Scanner.ToParse.Length)
                        {
                            if ((iToParse[Scanner.NextStart - 1] == '\n' && iToParse[Scanner.NextStart] == '\r')
                                || (iToParse[Scanner.NextStart - 1] == '\r' && iToParse[Scanner.NextStart] == '\n'))
                            {
                                // if it's an /n/r or /r/n -> skip 1 extra token cause it's just 1 line.
                                Scanner.NextStart++;
                                CurLineAt++;
                            }
                        }
                    }
                    else if (iToParse[Scanner.CurrentStart] == '/' && iToParse[Scanner.NextStart] == '*')
                    {
                        var iCur = iToParse[Scanner.CurrentStart];
                        Scanner.CurrentStart = Scanner.NextStart;
                        var iPrev = (char)0;
                        Scanner.NextStart += 1;
                        AdjustCurLine(iPrev);
                        iPrev = iCur;
                        while (Scanner.NextStart <= iToParse.Length && iToParse[Scanner.NextStart - 2] == '/'
                               && iToParse[Scanner.NextStart - 1] == '*')
                        {
                            // could have multiple comment blocks after each other, filter them all out at once.
                            Scanner.CurrentStart = Scanner.NextStart;
                            iCur = iToParse[Scanner.CurrentStart];
                            Scanner.NextStart++;
                            AdjustCurLine(iPrev);
                            iPrev = iCur;
                            while (Scanner.NextStart <= iToParse.Length
                                   && (iToParse[Scanner.NextStart - 1] != '*' || iToParse[Scanner.NextStart] != '/'))
                            {
                                // we can have nested comments, need to skip those as well
                                Scanner.CurrentStart = Scanner.NextStart;
                                iCur = iToParse[Scanner.NextStart];
                                Scanner.NextStart++;
                                AdjustCurLine(iPrev);
                                iPrev = iCur;
                            }

                            Scanner.NextStart++; // add 1 final item to get passed the /
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (Scanner.NextStart > iToParse.Length - 1)
                    {
                        // Scanner.CurrentToken = Token.End;                           //we have reached the end, make certain that the rest of the system i aware of this.
                        Scanner.NextStart = iToParse.Length;

                            // we can go passed the end during this check, don't want that, we can get into trobules that way, so fix that.
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Goes to the next item, then moves until until the first non space.
        /// </summary>
        protected void AdvanceWithSpaceSkip()
        {
            fLastTokenPos = new FilePos();
            fLastTokenPos.Pos = Scanner.CurrentStart;
            fLastTokenPos.Line = CurLine;
            fLastTokenPos.Column = 1 + (Scanner.CurrentStart - CurLineAt);
            Scanner.GetNext();
            SkipSpaces();
        }

        /// <summary>
        ///     Goes to the next item, then moves until until the first non space.
        /// </summary>
        protected void Advance()
        {
            fLastTokenPos = new FilePos();
            fLastTokenPos.Pos = Scanner.CurrentStart;
            fLastTokenPos.Line = CurLine;
            fLastTokenPos.Column = 1 + (Scanner.CurrentStart - CurLineAt);
            Scanner.GetNext();
        }

        /// <summary>Reads a word and tries to convert it into a part of speech neuron.
        ///     After the word has been read, the scanner is moved to the next token.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadThesPos()
        {
            Neuron iRes = null;
            if (Scanner.CurrentToken == Token.Word)
            {
                iRes = ReadSingleWordPos();
            }
            else
            {
                return null;
            }

            if (iRes != null)
            {
                // only advance if there was a pos declared, othewise we need to continue searching.
                Scanner.GetNext();
            }

            return iRes;
        }

        /// <summary>Reads a part of speech that consists of a single word.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron ReadSingleWordPos()
        {
            var iVarName = Scanner.CurrentValue;
            return ConvertStringToPos(iVarName);
        }

        /// <summary>tries to convert a string to a pos value.</summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron ConvertStringToPos(string name)
        {
            ulong iFound = 0;
            switch (name.ToLower())
            {
                case "noun":
                    iFound = (ulong)PredefinedNeurons.Noun;
                    break;
                case "verb":
                    iFound = (ulong)PredefinedNeurons.Verb;
                    break;
                case "adjective":
                case "adj":
                    iFound = (ulong)PredefinedNeurons.Adjective;
                    break;
                case "adverb":
                case "adv":
                    iFound = (ulong)PredefinedNeurons.Adverb;
                    break;
                case "article":
                case "art":
                    iFound = (ulong)PredefinedNeurons.Article;
                    break;
                case "pronoun":
                case "pron":
                    iFound = (ulong)PredefinedNeurons.Pronoun;
                    break;
                case "conj":
                case "conjunction":
                    iFound = (ulong)PredefinedNeurons.Conjunction;
                    break;
                case "inter":
                case "interjection":
                    iFound = (ulong)PredefinedNeurons.Interjection;
                    break;
                case "prep":
                case "preposition":
                    iFound = (ulong)PredefinedNeurons.Preposition;
                    break;
                case "comp":
                case "complementizer":
                    iFound = (ulong)PredefinedNeurons.Complementizer;
                    break;
                case "number":
                    iFound = (ulong)PredefinedNeurons.Number;
                    break;
                case "int":
                case "integer":
                    iFound = (ulong)PredefinedNeurons.IntNeuron;
                    break;
                case "double":
                    iFound = (ulong)PredefinedNeurons.DoubleNeuron;
                    break;
                case "any":
                    iFound = (ulong)PredefinedNeurons.Empty;
                    break;
                default:
                    break;
            }

            if (iFound != 0)
            {
                return Brain.Current[iFound];
            }

            return null;
        }

        ///// <summary>
        ///// Tries to remove a list of code.
        ///// </summary>
        ///// <param name="toDel"></param>
        // public static void RemoveCodeIfPos(NeuronCluster toDel)
        // {
        // if (BrainHelper.HasIncommingReferences(toDel) == false)
        // {
        // List<ulong> iToDel = Factories.Default.IDLists.GetBuffer();
        // using (ChildrenAccessor iChildren = toDel.Children)
        // iToDel.AddRange(iChildren);
        // Brain.Current.Delete(toDel);
        // List<Neuron> iNewRetries = new List<Neuron>();
        // foreach (ulong i in iToDel)
        // {
        // Neuron iN = Brain.Current[i];
        // if (BrainHelper.HasIncommingReferences(iN) == false || BrainHelper.OnlyIncommingFrom(iN, iToDel)) //HasIncommingreferences is fast, but also need to verify circular refs.
        // {
        // if (Brain.Current.Delete(iN) == false)                                                             //if for some reason,can't  delete (maybe used as a link info somewhere), try again.
        // iNewRetries.Add(iN);
        // }
        // else
        // iNewRetries.Add(iN);
        // }

        // List<Neuron> iRetries = null;
        // while (iRetries == null || iNewRetries.Count != iRetries.Count)
        // {
        // iRetries = iNewRetries;
        // iNewRetries = new List<Neuron>();
        // for (int i = 0; i < iRetries.Count; i++)
        // {
        // if (BrainHelper.HasIncommingReferences(iRetries[i]) == false || BrainHelper.OnlyIncommingFrom(iRetries[i], iToDel))     //don't need to check anymore if moduleRefCount == 0, this has already happened, simply need to check the incomming refs.
        // {
        // if (Brain.Current.Delete(iRetries[i]) == false)
        // iNewRetries.Add(iRetries[i]);
        // }
        // else
        // iNewRetries.Add(iRetries[i]);
        // }
        // }
        // Factories.Default.IDLists.Recycle(iToDel);
        // }
        // }
        #endregion
    }
}