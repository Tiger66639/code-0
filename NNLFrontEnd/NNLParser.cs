// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLParser.cs" company="">
//   
// </copyright>
// <summary>
//   a parser for the neural network language.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     a parser for the neural network language.
    /// </summary>
    public class NNLParser : ParserBase
    {
        /// <summary>The f code sync points.</summary>
        private static readonly Token[] fCodeSyncPoints = { Token.End, Token.LoopEnd, Token.PointComma };

                                        // sync points used for code lines.

        /// <summary>The f overloadable.</summary>
        private static readonly Token[] fOverloadable =
            {
                Token.Add, Token.AddAssign, Token.AddNotAssign, Token.Minus, 
                Token.MinusAssign, Token.Assign, Token.NotAssign, 
                Token.Multiply, Token.MultiplyAssign, Token.Divide, 
                Token.DivAssign, Token.Modulus, Token.ModulusAssign, 
                Token.IsEqual, Token.IsBiggerThen, Token.IsBiggerThenOrEqual, 
                Token.IsSmallerThen, Token.IsSmallerThenOrEqual, 
                Token.ListAssign
            };

                                        // all the operators that a binding item can overload.

        /// <summary>The clusternames.</summary>
        private static readonly string[] CLUSTERNAMES = { "cluster", "neuroncluster" };

        /// <summary>The neuronnames.</summary>
        private static readonly string[] NEURONNAMES =
            {
                "neuron", "statement", "resultstatement", "assignment", 
                "boolexpression", "conditionalexpression", 
                "conditionalstatement", "lockexpression", "lock", 
                "byrefexpression", "byref", "var", "variable", "global", 
                "textsin", "audiosin", "imagesin", "intsin", "reflectionsin", 
                "timersin"
            };

        /// <summary>The vardecltypes.</summary>
        private static readonly string[] VARDECLTYPES = { "string", "int", "var", "double", "bool" };

        /// <summary>The classsyncpoints.</summary>
        private static string[] CLASSSYNCPOINTS =
            {
                "cluster", "neuroncluster", "neuron", "statement", "resultstatement", 
                "assignment", "boolexpression", "conditionalexpression", 
                "conditionalstatement", "lockexpression", "lock", "byrefexpression", 
                "byref", "var", "variable", "global", "textsin", "audiosin", 
                "imagesin", "intsin", "reflectionsin", "timersin", "string", "int", 
                "var", "double", "bool", "static", "bind", "class", "using", 
                "expressionsblock"
            };

        /// <summary>The varnames.</summary>
        private static readonly string[] VARNAMES = { "var", "variable" };

        /// <summary>The selectmodifiers.</summary>
        private static readonly string[] SELECTMODIFIERS = { "in", "out", "children", "parents" };

        /// <summary>The compareops.</summary>
        private static readonly Token[] COMPAREOPS =
            {
                Token.IsEqual, Token.IsBiggerThen, Token.IsBiggerThenOrEqual, 
                Token.IsSmallerThen, Token.IsSmallerThenOrEqual, 
                Token.NotContains, Token.Contains, Token.NotAssign
            };

        /// <summary>The addops.</summary>
        private static readonly Token[] ADDOPS = { Token.Add, Token.Minus };

        /// <summary>The multiplyops.</summary>
        private static readonly Token[] MULTIPLYOPS = { Token.Multiply, Token.Divide, Token.Modulus };

        /// <summary>The assignops.</summary>
        private static readonly Token[] ASSIGNOPS =
            {
                Token.Assign, Token.AddAssign, Token.AddNotAssign, Token.AndAssign, 
                Token.ListAssign, Token.MinusAssign, Token.DivAssign, 
                Token.OrAssign, Token.ModulusAssign
            };

        /// <summary>The unaryops.</summary>
        private static readonly Token[] UNARYOPS =
            {
                Token.Variable, Token.ThesVariable, Token.AssetVariable, 
                Token.TopicRef, Token.Minus, Token.At
            };

        /// <summary>The pathops.</summary>
        private static readonly Token[] PATHOPS = { Token.Dot, Token.LengthSpec, Token.ArrowRight, Token.ArrowLeft };

        /// <summary>The f create new double.</summary>
        private NNLStatementNode fCreateNewDouble;

                                 // to save some code, we can reuse these nodes whenever creating new vars (int or doubles).

        /// <summary>The f create new int.</summary>
        private NNLStatementNode fCreateNewInt;

        /// <summary>The f attributes.</summary>
        private readonly System.Collections.Generic.List<System.Collections.Generic.List<string>> fAttributes =
            new System.Collections.Generic.List<System.Collections.Generic.List<string>>();

                                                                                                  // stores a list of attributes which were just read and which still need to be assigned to the next object that will be read.

        /// <summary>The f code.</summary>
        private readonly System.Collections.Generic.Stack<NNLNodesList> fCode =
            new System.Collections.Generic.Stack<NNLNodesList>(); // the top item should collect the code.

        /// <summary>The f compiler.</summary>
        private readonly NNLModuleCompiler fCompiler; // so we have access to the compiler object from within the parser.

        /// <summary>The f ns.</summary>
        private readonly System.Collections.Generic.Stack<NNLNode> fNS = new System.Collections.Generic.Stack<NNLNode>();

                                                                   // stores the stack of namespace nodes, which is used to resolve names. static so that all the parsers can find it.

        /// <summary>The f root.</summary>
        private readonly NNLClassNode fRoot; // the root class node, so we can find things quickly.

        /// <summary>Initializes a new instance of the <see cref="NNLParser"/> class.</summary>
        /// <param name="root">The root.</param>
        /// <param name="compiler">The compiler.</param>
        internal NNLParser(NNLClassNode root, NNLModuleCompiler compiler)
        {
            fProcessComments = true;
            fRoot = root;
            fCompiler = compiler;
            fNS.Push(fRoot); // provide a root class node.
        }

        /// <summary>Initializes a new instance of the <see cref="NNLParser"/> class. For
        ///     parsing stand alone files.</summary>
        /// <param name="compiler">The compiler.</param>
        public NNLParser(NNLModuleCompiler compiler)
        {
            fProcessComments = true;
            fRoot = new NNLClassNode();
            fNS.Push(fRoot);
            fCompiler = compiler;
        }

        /// <summary>
        ///     provides access to the <see langword="namespace" /> stack of the
        ///     parser. This allows a caller to add/remove items just before and after
        ///     compiling a section of code.
        /// </summary>
        internal System.Collections.Generic.Stack<NNLNode> NS
        {
            get
            {
                return fNS;
            }
        }

        /// <summary>Logs an <paramref name="error"/> and adds the current pattern and
        ///     position to the <paramref name="error"/> message.</summary>
        /// <param name="error"></param>
        /// <param name="fail"></param>
        public override void LogPosError(string error, bool fail = true)
        {
            fCompiler.Errors.Add(
                string.Format("L:{0} C:{1} {2}", CurLine, 1 + (Scanner.CurrentStart - CurLineAt), error));
            base.LogPosError(error, fail);
        }

        /// <summary>Parses the specified to parse.</summary>
        /// <param name="file">The file, so we know for which file to generate errors.</param>
        /// <param name="toParse">The content to parse.</param>
        public void Parse(string file, string toParse)
        {
            var iToParse = toParse;
            if (iToParse == null)
            {
                iToParse = string.Empty;
            }

            Scanner = new Tokenizer(iToParse);
            ParserTitle = file;
            Parse(Token.End);
        }

        /// <summary>parses a block of code. This presumes that the<see cref="NNLClusterNode"/> that will contain the code block has
        ///     already been created, stored and placed on the <see cref="fNS"/>
        ///     (namespacesstack) of the parser.</summary>
        /// <param name="name"></param>
        /// <param name="toParse"></param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        internal NNLFunctionNode ParseCodeBlock(string name, string toParse)
        {
            var iToParse = toParse;
            if (iToParse == null)
            {
                iToParse = string.Empty;
            }

            Scanner = new Tokenizer(iToParse);
            ParserTitle = name;

            var iCode = new NNLFunctionNode();
            SetNodeStart(iCode);
            iCode.IsInline = false;

            fCode.Push(iCode);
            try
            {
                ReadCodeLines();
            }
            finally
            {
                fCode.Pop();
            }

            iCode.End = LastTokenPos;
            return iCode;
        }

        /// <summary>prepares this parser so it uses the specified tokenizer as source.
        ///     This is used by other parser that which to use this parser to handle
        ///     sub sections. Does not initiate a parse itself.</summary>
        /// <param name="title">The title.</param>
        /// <param name="source"></param>
        public void PrepareForSectionParse(string title, Tokenizer source)
        {
            ParserTitle = title;
            Scanner = source;
        }

        /// <summary>reads all the statements in the file.</summary>
        /// <param name="endToken">The end Token.</param>
        private void Parse(Token endToken)
        {
            while (Scanner.CurrentToken != endToken && Scanner.CurrentToken != Token.End)
            {
                // in case endtoken isn't 'end', we need to make certain that we don't get caught in an ehternal loop.
                SkipSpaces(); // always skip any spaces before we try a new item.
                if (Scanner.IsWord(CLUSTERNAMES))
                {
                    ReadCluster();
                }
                else if (Scanner.IsWord(NEURONNAMES))
                {
                    ReadNeuron();
                }
                else if (Scanner.IsWord("static"))
                {
                    ReadNeuron(true);
                }
                else if (Scanner.IsWord("bind"))
                {
                    ReadBinding();
                }
                else if (Scanner.IsWord("class"))
                {
                    ReadClass();
                }
                else if (Scanner.IsWord("using"))
                {
                    ReadUsing();
                }
                else if (Scanner.IsWord(VARDECLTYPES))
                {
                    SaveReadVarDecl(VarScope.Global);
                }
                else if (Scanner.IsWord("expressionsblock"))
                {
                    ReadExpressionBlock();
                }
                else if (Scanner.CurrentToken == Token.OptionStart)
                {
                    ReadAttribute();
                }
                else
                {
                    ReadFunction();

                    // LogPosError("Unexpected token", false);
                    // AdvanceToNextSyncPos(CLASSSYNCPOINTS);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         reads a single attribute that is declared just above an object
        ///         decleration (not above code) the contents of the attribute will be
        ///         further analyzed by the parser, once it has been attached to an object
        ///         (the next one that is read.
        ///     </para>
        ///     <para>
        ///         '[' {any} ']' currently supported for any: callos //the codeblock with
        ///         this attribute is used as the callback for executing os functions
        ///         external xmlElment //loads and binds an os function that is defined in
        ///         the xml element.
        ///     </para>
        /// </summary>
        private void ReadAttribute()
        {
            AdvanceWithSpaceSkip();
            var iRes = new System.Collections.Generic.List<string>();
            string iVal;
            while (Scanner.CurrentToken != Token.OptionEnd && Scanner.CurrentToken != Token.End)
            {
                iVal = Scanner.ToParse.Substring(Scanner.CurrentStart, Scanner.NextStart - Scanner.CurrentStart);

                    // copy like this so we preserve casing (important for xml elements)
                if (string.IsNullOrEmpty(iVal) == false)
                {
                    // skip empty spaces
                    iRes.Add(iVal);
                }

                Advance(); // don't skip spaces,they might be important for many attribs.
            }

            fAttributes.Add(iRes);
            Expect(Token.OptionEnd);
        }

        /// <summary>
        ///     reads a function (declared at the level of a class). for EBNF, see
        ///     <see cref="ReadLinkToCode" /> (and LinkToCodeOrLink).
        /// </summary>
        private void ReadFunction()
        {
            var iNode = new NNLClusterNode();
            SetNodeStart(iNode);

            var iName = GetIdentifier();
            CollectNS(iName, iNode);
            try
            {
                iNode.Content = ReadFunctionDefContent(string.IsNullOrEmpty(iNode.ExternalDef) == false);
                iNode.End = LastTokenPos;
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>
        ///     reads an expression block 'expressionsblock' identifier '{'
        ///     ['statements''('')' '{' code '}' ] { linkCode | VarDecl } '}'
        /// </summary>
        private void ReadExpressionBlock()
        {
            var iNode = new NNLExpBlockNode();
            SetNodeStart(iNode);
            AdvanceWithSpaceSkip();

            var iName = GetIdentifier();
            CollectNS(iName, iNode);
            try
            {
                Expect(Token.LoopStart);
                while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
                {
                    if (Scanner.IsWord("statements"))
                    {
                        AdvanceWithSpaceSkip();
                        iNode.Content = ReadFunctionDefContent(false); // external def not allowed here.
                    }
                    else if (Scanner.IsWord(VARDECLTYPES))
                    {
                        SaveReadVarDecl(VarScope.Var);
                    }
                    else if (Scanner.IsWord(CLUSTERNAMES))
                    {
                        ReadCluster();
                    }
                    else if (Scanner.IsWord(NEURONNAMES))
                    {
                        ReadNeuron();
                    }
                    else if (Scanner.IsWord("static"))
                    {
                        ReadNeuron(true);
                    }
                    else if (Scanner.IsWord("bind"))
                    {
                        ReadBinding();
                    }
                    else if (Scanner.IsWord("expressionsblock"))
                    {
                        ReadExpressionBlock();
                    }
                    else if (Scanner.CurrentToken == Token.OptionStart)
                    {
                        ReadAttribute();
                    }
                    else
                    {
                        ReadCodeOrLink(iNode);
                    }
                }

                Expect(Token.LoopEnd);
                iNode.End = LastTokenPos;
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>
        ///     ebnf: using = 'using' nsPath { ',' nsPath } ';' . nsPath = identifier
        ///     {'.' identifier} .
        /// </summary>
        private void ReadUsing()
        {
            AdvanceWithSpaceSkip();

            var iList = GetSimpleNSPath(true);
            var iNS = fNS.Peek();
            iList.Parent = iNS;
            iNS.Usings.Add(iList);
            while (Scanner.CurrentToken == Token.Comma)
            {
                AdvanceWithSpaceSkip();
                iList = GetSimpleNSPath(true);
                iList.Parent = iNS;
                iNS.Usings.Add(iList);
            }

            Expect(Token.PointComma);
        }

        /// <summary>reads a <see langword="namespace"/> path that does not allow for
        ///     functions or brackets.</summary>
        /// <param name="asUsing">if set to <c>true</c> an <see cref="NNLUsingPathNode"/> is created,
        ///     which is used in using clauses, so that the compiler knows it is
        ///     solving a path for a using clause.</param>
        /// <returns>The <see cref="NNLPathNode"/>.</returns>
        private NNLPathNode GetSimpleNSPath(bool asUsing = false)
        {
            NNLPathNode iList;
            if (asUsing == false)
            {
                iList = new NNLPathNode();
            }
            else
            {
                iList = new NNLUsingPathNode();
            }

            SetNodeStart(iList);
            var iRef = new NNLReferenceNode();
            iRef.Reference = GetIdentifier();
            iList.Items.Add(iRef);
            while (Scanner.CurrentToken == Token.Dot)
            {
                iRef = new NNLReferenceNode();
                iRef.Reference = GetIdentifier();
                iList.Items.Add(iRef);
            }

            iList.End = LastTokenPos;
            return iList;
        }

        /// <summary>
        ///     reads a class (contains other blocks, binds, neurons, clusters
        /// </summary>
        /// <remarks>
        ///     'class' identifier {'.' identifier } '{' {objects} '}'
        /// </remarks>
        private void ReadClass()
        {
            var iItem = new NNLClassNode();
            SetNodeStart(iItem);
            AdvanceWithSpaceSkip();
            var iNames = new System.Collections.Generic.List<string>(); // a class can have a path as a name.
            iNames.Add(GetIdentifier());
            while (Scanner.CurrentToken == Token.Dot)
            {
                AdvanceWithSpaceSkip();
                iNames.Add(GetIdentifier());
            }

            Expect(Token.LoopStart);
            iItem = GetClass(iNames, iItem);
            try
            {
                Parse(Token.LoopEnd);
            }
            finally
            {
                fNS.Pop();
            }

            Expect(Token.LoopEnd);
            iItem.End = LastTokenPos;
        }

        /// <summary>reads a binding between the parser/generator and network. For some
        ///     statements, like asset assignments, some extra code is required to
        ///     link the things together. These are declared using bindings.</summary>
        /// <remarks>'bind' ('$' | '^' | '#' | '~' | '@' ) identifier ['register']
        ///     ['UseStatics'] '{' { operatorBinding | 'this' '(' {param} ')' [':'
        ///     returnValue ] '{' code '}' | linkCode | varDecl } '}'</remarks>
        /// <returns>The <see cref="NNLBinding"/>.</returns>
        private NNLBinding ReadBinding()
        {
            var iNode = new NNLBinding();
            SetNodeStart(iNode);
            AdvanceWithSpaceSkip();

            if (Scanner.CurrentToken != Token.Variable && Scanner.CurrentToken != Token.ThesVariable
                && Scanner.CurrentToken == Token.AssetVariable && Scanner.CurrentToken == Token.TopicRef
                && Scanner.CurrentToken != Token.At)
            {
                LogPosError("$ ~ ^ # or @ expected after bind", false);
            }

            iNode.Operator = Scanner.CurrentToken;
            AdvanceWithSpaceSkip();
            iNode.Name = GetIdentifier();
            iNode = CollectBinding(iNode);
            CheckBindingArguments(iNode);
            Expect(Token.LoopStart);
            try
            {
                try
                {
                    while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
                    {
                        if (Scanner.IsWord(VARDECLTYPES))
                        {
                            SaveReadVarDecl(VarScope.Var);
                        }
                        else if (Scanner.CurrentToken == Token.Word)
                        {
                            // it's a function definition, could also be a 'this' function.
                            var iName = GetIdentifier();
                            var iFunction = ReadFunctionDefContent(false); // external def not allowed here.
                            iFunction.Name = iName;
                            if (iName.ToLower() == "this")
                            {
                                // if this is not a 'this' function, collect
                                iName = iFunction.BuildNameFromParams(iName);

                                    // the 'this' function needs to be parameter type specific.
                            }

                            CollectLeaf(iName, iFunction);

                                // a function is reachable through a name (it's a namespace for lcoals, but this has already been handled when the function content was read).
                            iNode.Functions.Add(iFunction);
                        }
                        else
                        {
                            ReadOperatorBinding(iNode);
                        }
                    }
                }
                catch
                {
                    while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
                    {
                        // if there was an exception, sync to the next }
                        Advance();
                    }
                }

                Expect(Token.LoopEnd);
                iNode.End = LastTokenPos;
                return iNode;
            }
            finally
            {
                fNS.Pop(); // need to remove the binding's namespace.
            }
        }

        /// <summary>The check binding arguments.</summary>
        /// <param name="iNode">The i node.</param>
        private void CheckBindingArguments(NNLBinding iNode)
        {
            if (Scanner.IsWord("register"))
            {
                iNode.Register = true;
                AdvanceWithSpaceSkip();
                if (Scanner.IsWord("usestatics"))
                {
                    iNode.UseStatics = true;
                    AdvanceWithSpaceSkip();
                }
            }
            else if (Scanner.IsWord("usestatics"))
            {
                iNode.UseStatics = true;
                AdvanceWithSpaceSkip();
                if (Scanner.IsWord("register"))
                {
                    iNode.Register = true;
                    AdvanceWithSpaceSkip();
                }
            }
        }

        /// <summary>Reads the binding of a single <see langword="operator"/> in a 'bind'.
        ///     operatorBinding = ( [ ('.' | '-&gt;' | 'arrowleft' '[]' ) identifier ]
        ///     BindingItemContent //the first binding <see langword="operator"/>
        ///     determins what happens with the root and therefor the<see langword="operator"/> is empty | ':' identifier
        ///     FunctionBindingContent //function section is declared a little
        ///     different, it doesn't have get/set parts but a list of functions. |
        ///     BoolOp FunctionDecl //global <see langword="operator"/> overloads for
        ///     things like == &gt;= ... );</summary>
        /// <param name="parent">The parent.</param>
        private void ReadOperatorBinding(NNLBinding parent)
        {
            if (Enumerable.Contains(COMPAREOPS, Scanner.CurrentToken))
            {
                ReadBindOperatorOverload(parent);
            }
            else
            {
                NNLBindItemBase iItem;
                if (Enumerable.Contains(PATHOPS, Scanner.CurrentToken))
                {
                    iItem = ReadBindingItemContentPathOp(parent.BindingItems, parent);
                }
                else if (Scanner.CurrentToken == Token.OptionStart)
                {
                    iItem = ReadBindingItemContentIndex(parent.BindingItems, parent);
                }
                else
                {
                    iItem = new NNLBindItem();
                    iItem.Parent = parent;
                    SetNodeStart(iItem);
                    ReadBindItemContent(iItem); // it's the root item
                    parent.Root = (NNLBindItem)iItem;

                        // this is no Path operator (like a '.' or ':'), so it defines all the possible start-of-path values and how to process them.
                }

                iItem.End = LastTokenPos;
            }
        }

        /// <summary>reads a binding's global <see langword="operator"/> overload.</summary>
        /// <param name="item"></param>
        private void ReadBindOperatorOverload(NNLBinding item)
        {
            var iToken = Scanner.CurrentToken;
            if (Enumerable.Contains(fOverloadable, iToken) == false)
            {
                LogPosError(
                    string.Format(
                        "invalid operator: '{0}' can't be overloaded by a binding", 
                        Tokenizer.GetSymbolForToken(iToken)), 
                    false);
            }

            var iCode = ReadBindingCode(item);
            var iName = iCode.BuildNameFromParams(Tokenizer.GetSymbolForToken(iToken));
            iCode.Name = iName;
            item.OperatorOverloads.Add(iName, iCode);
        }

        /// <summary>The add binding item to.</summary>
        /// <param name="addTo">The add to.</param>
        /// <param name="name">The name.</param>
        /// <param name="toAdd">The to add.</param>
        private void AddBindingItemTo(System.Collections.Generic.Dictionary<Token, NNLBindItemBase> addTo, 
            string name, 
            NNLBindItemBase toAdd)
        {
            toAdd.Name = name;
            if (addTo.ContainsKey(toAdd.Operator) == false)
            {
                addTo.Add(toAdd.Operator, toAdd);
                if (toAdd.RootBinding.AllBindItems.ContainsKey(name) == false)
                {
                    toAdd.RootBinding.AllBindItems.Add(name, toAdd);
                }
                else
                {
                    LogPosError(
                        string.Format("{0} is already defined in the current scope of the binding", name), 
                        false);
                }
            }
            else
            {
                LogPosError(
                    string.Format(
                        "{0} is already defined in the current scope of the binding", 
                        Tokenizer.GetSymbolForToken(toAdd.Operator)), 
                    false);
            }
        }

        /// <summary>The read bind item content.</summary>
        /// <param name="item">The item.</param>
        /// BindingItemContent = '{'  
        /// { 'get' FunctionDecl                                           //standard getter
        /// | 'set' FunctionDecl                                           //standard assignment.
        /// | operatorBinding                                              //for defining operators that can come after this one
        /// | '(' BindingRef ')'                                           //for defining  operators that can come after this one, and that have been declared somewehere else in this binding (a reference).
        /// | identifier BindingItemContent                                //this is for static keywords
        /// | (  '+' | '-' | '=' '+=' |...) functionDecl                   //different setter operators
        /// } 
        /// '}' ;
        ///                   
        /// BindingRef = identifier;
        /// 
        /// functionDecl =  '(' [Type paramName {, type paramnName } ] ')' [':'Type ] '{'  {code}  '}' ;
        private void ReadBindItemContent(NNLBindItemBase item)
        {
            Expect(Token.LoopStart);
            try
            {
                if (item.Operator == Token.LengthSpec)
                {
                    ReadBindingItemContentForFunction((NNLBindItemFunctions)item);
                }
                else
                {
                    ReadBindingItemContentRegular((NNLBindItemIndex)item);
                }
            }
            catch
            {
                AdvanceToNextSyncPos(fObjectSyncPoints);
            }

            ExpectOrSync(Token.LoopEnd, fObjectSyncPoints);
        }

        /// <summary>The read binding item content regular.</summary>
        /// <param name="item">The item.</param>
        private void ReadBindingItemContentRegular(NNLBindItemIndex item)
        {
            while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
            {
                if (Scanner.IsWord("get"))
                {
                    item.AddGetter(ReadBindingCode(item), this);
                }
                else if (Scanner.IsWord("set"))
                {
                    item.AddSetter(ReadBindingCode(item), this);
                }
                else if (Enumerable.Contains(PATHOPS, Scanner.CurrentToken))
                {
                    ReadBindingItemContentPathOp(item.BindingItems, item);
                }
                else if (Scanner.CurrentToken == Token.OptionStart)
                {
                    ReadBindingItemContentIndex(item.BindingItems, item);
                }
                else if (Scanner.CurrentToken == Token.GroupStart)
                {
                    AdvanceWithSpaceSkip();
                    var iRef = GetIdentifier();
                    Expect(Token.GroupEnd);
                    item.SubItemsToResolve.Add(iRef);
                }
                else if (Scanner.CurrentToken == Token.Word || Scanner.CurrentValue == "*")
                {
                    // the '*' operator is a special token that can be used just after a path operator (. : ) without spaces, example: ^noun.test.*
                    ReadbindStatic(item as NNLBindItem);
                }
                else
                {
                    ReadBindOperatorOverload(item);
                }
            }
        }

        /// <summary>The read binding item content path op.</summary>
        /// <param name="addTo">The add to.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        private NNLBindItemBase ReadBindingItemContentPathOp(System.Collections.Generic.Dictionary<Token, NNLBindItemBase> addTo, 
            NNLStatementNode parent)
        {
            NNLBindItemBase iItem;
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                // check the type of path part we are trying to specify. the ':' needs special treatement.
                iItem = new NNLBindItemFunctions();
            }
            else
            {
                iItem = new NNLBindItem();
            }

            iItem.Parent = parent;
            SetNodeStart(iItem);
            iItem.Operator = Scanner.CurrentToken;
            AdvanceWithSpaceSkip();
            var iName = GetIdentifier();
            ReadBindItemContent(iItem);
            AddBindingItemTo(addTo, iName, iItem);
            return iItem;
        }

        /// <summary>FunctionBindingContent = '{' { OperatorBinding | method } '}'</summary>
        /// <param name="item"></param>
        private void ReadBindingItemContentForFunction(NNLBindItemFunctions item)
        {
            while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
            {
                if (Enumerable.Contains(PATHOPS, Scanner.CurrentToken))
                {
                    ReadBindingItemContentPathOp(item.BindingItems, item);
                }
                else if (Scanner.CurrentToken == Token.OptionStart)
                {
                    ReadBindingItemContentIndex(item.BindingItems, item);
                }
                else if (Scanner.CurrentToken == Token.GroupStart)
                {
                    AdvanceWithSpaceSkip();
                    var iRef = GetIdentifier();
                    Expect(Token.GroupEnd);
                    item.SubItemsToResolve.Add(iRef);
                }
                else
                {
                    ReadbindFunctionCallback(item);
                }
            }
        }

        /// <summary>The read binding item content index.</summary>
        /// <param name="addTo">The add to.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        private NNLBindItemBase ReadBindingItemContentIndex(System.Collections.Generic.Dictionary<Token, NNLBindItemBase> addTo, 
            NNLStatementNode parent)
        {
            AdvanceWithSpaceSkip();
            Expect(Token.OptionEnd);
            var iItem = new NNLBindItemIndex();
            iItem.Parent = parent;
            iItem.Operator = Token.OptionStart;
            var iName = GetIdentifier();
            ReadBindItemContent(iItem);
            AddBindingItemTo(addTo, iName, iItem);
            return iItem;
        }

        /// <summary>reads a function decleraition for a ':' binding section.</summary>
        /// <param name="item"></param>
        private void ReadbindFunctionCallback(NNLBindItemFunctions item)
        {
            var iName = GetIdentifier();
            var iRes = ReadFunctionDefContent(false); // externaldef not allowed ehre.
            iRes.Parent = item;
            iRes.Name = iName;
            iName = iRes.FullName;
            if (item.Functions.ContainsKey(iName) == false)
            {
                item.Functions.Add(iName, iRes);
            }
            else
            {
                LogPosError(string.Format("name already used as function: {0}", iName), false);
            }
        }

        /// <summary>The read bind operator overload.</summary>
        /// <param name="item">The item.</param>
        private void ReadBindOperatorOverload(NNLBindItemIndex item)
        {
            var iToken = Scanner.CurrentToken;
            if (Enumerable.Contains(fOverloadable, iToken) == false)
            {
                LogPosError(
                    string.Format(
                        "invalid operator: '{0}' can't be overloaded by a binding", 
                        Tokenizer.GetSymbolForToken(iToken)), 
                    false);
            }

            var iCode = ReadBindingCode(item);
            var iName = iCode.BuildNameFromParams(Tokenizer.GetSymbolForToken(iToken));
            iCode.Name = iName;
            item.OperatorOverloads.Add(iName, iCode);
        }

        /// <summary>The readbind static.</summary>
        /// <param name="parent">The parent.</param>
        private void ReadbindStatic(NNLBindItem parent)
        {
            if (parent == null)
            {
                LogPosError("statics not allwed here", false);
            }

            var iNew = new NNLBindItem();
            SetNodeStart(iNew);
            string iName;
            if (Scanner.CurrentValue != "*")
            {
                iName = GetIdentifier();
            }
            else
            {
                // note: the '*' operator can only be used in . -> and <- just like statics
                iName = "*";

                    // we treat "*" as a static, but the 'GetIdentifier' doesn't treat it like that, so take care of this.
                AdvanceWithSpaceSkip();
            }

            iNew.Name = iName;
            if (parent != null)
            {
                if (parent.Statics.ContainsKey(iName) == false)
                {
                    iNew.Parent = parent;
                    parent.Statics.Add(iName, iNew); // we add it so that operator overloads can quickly be found.
                }
                else
                {
                    LogPosError(
                        string.Format("'{0}' is already defined as a static value in this binding", iName), 
                        false);
                }
            }
            else
            {
                LogPosError(string.Format("statics not allowed as child items here.", iName), false);
            }

            ReadBindItemContent(iNew);
            iNew.End = LastTokenPos;
        }

        /// <summary>the link-name and code syntax: '(' [Type paramName {, type paramnName
        ///     } ] ')' [':'Type ] '{' {code} '}' ParamName = identifier type = 'var'
        ///     | 'variable' | 'int' | 'string' | 'double' | 'int[]' | 'double[]' |
        ///     'string[]' | 'var[]' ex: Find(var a, <see langword="int"/> b): var c,
        ///     var e { ... }</summary>
        /// <param name="parent">The parent.</param>
        /// <returns>the name of the function.</returns>
        private NNLFunctionNode ReadBindingCode(NNLStatementNode parent)
        {
            AdvanceWithSpaceSkip();
            var iRes = ReadFunctionDefContent(false); // external def not allowed here.
            iRes.Parent = parent;
            return iRes;
        }

        /// <summary>The read function def content.</summary>
        /// <param name="hasExternalDef">The has external def.</param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        private NNLFunctionNode ReadFunctionDefContent(bool hasExternalDef)
        {
            var iCode = new NNLFunctionNode();
            SetNodeStart(iCode);
            iCode.Params = ReadParameters();
            iCode.ReturnValues = ReadReturnValue();
            iCode.IsInline = TryReadIsInline(iCode.Params, iCode.ReturnValues);
            bool iHasBody;
            if (Scanner.CurrentToken == Token.PointComma)
            {
                // the function has no body, which is possible if there is an external decleration.
                AdvanceWithSpaceSkip();
                iHasBody = false;
            }
            else
            {
                iHasBody = true;
                ReadCodeBlock(iCode);
            }

            iCode.End = LastTokenPos;
            CheckBodyCode(hasExternalDef, iHasBody);
            return iCode;
        }

        /// <summary>provides a way for other parsers to read a code block and get a
        ///     reference to the parsed data.</summary>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        internal NNLFunctionNode ReadCodeForOtherParser()
        {
            var iCode = new NNLFunctionNode();
            SetNodeStart(iCode);
            iCode.IsInline = false;
            ReadCodeBlockForOtherParser(iCode, true, false);

                // for externally referenced code, a single line doesn't require a ';' at the end.
            iCode.End = LastTokenPos;
            return iCode;
        }

        /// <summary>The try read is inline.</summary>
        /// <param name="theParams">The the params.</param>
        /// <param name="returns">The returns.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryReadIsInline(System.Collections.Generic.List<NNLLocalDeclNode> theParams, System.Collections.Generic.List<NNLLocalDeclNode> returns)
        {
            if (Scanner.IsWord("inline"))
            {
                AdvanceWithSpaceSkip();
                if (theParams != null && theParams.Count > 0)
                {
                    LogPosWarning("functions with parameters can't yet be inlined");
                }

                if (returns != null && returns.Count > 0)
                {
                    LogPosWarning("functions with a return value can't yet be inlined");
                }

                if ((returns == null || returns.Count == 0) && (theParams == null || theParams.Count == 0))
                {
                    return true; // don't log this when not allowed.
                }
            }

            return false;
        }

        /// <summary>reads a neuron (all the code clusters that are attached to a neuron).</summary>
        /// <param name="asConst">The as Const.</param>
        /// <remarks>('neuron'| 'var' | 'global' |
        ///     "statement"|"resultstatement"|"assignment"|"boolexpression"|"conditionalexpression"|"conditionalstatement"|"lockexpression"|"lock"|"byrefexpression"|"byref"
        ///     | 'static' ('int' | 'double' | 'string' ) identifier ['(' ('duplicate'
        ///     | 'copy' | 'clear' | 'shared') ')'] [':' id] [=
        ///     Intvalue|doubleValue|stringValue ] '{' { linkCode | VarDecl } '}'</remarks>
        private void ReadNeuron(bool asConst = false)
        {
            if (asConst)
            {
                // ints, doubles and vars need to start with a 'const' to make a difference between vars.
                AdvanceWithSpaceSkip();
            }

            var iType = GetNodeType();
            var iNode = new NNLNeuronNode(iType);
            SetNodeStart(iNode);
            AdvanceWithSpaceSkip();

            var iName = GetIdentifier();
            CollectNS(iName, iNode);
            try
            {
                TryGetVarSplitMode(iNode);
                GetNeuronId(iNode);
                GetValue(iNode);
                if (Scanner.CurrentToken == Token.LoopStart)
                {
                    AdvanceWithSpaceSkip();
                    while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
                    {
                        if (Scanner.IsWord(VARDECLTYPES))
                        {
                            SaveReadVarDecl(VarScope.Var);
                        }
                        else if (Scanner.IsWord(CLUSTERNAMES))
                        {
                            ReadCluster();
                        }
                        else if (Scanner.IsWord(NEURONNAMES))
                        {
                            ReadNeuron();
                        }
                        else if (Scanner.IsWord("static"))
                        {
                            ReadNeuron(true);
                        }
                        else if (Scanner.IsWord("bind"))
                        {
                            ReadBinding();
                        }
                        else if (Scanner.IsWord("expressionsblock"))
                        {
                            ReadExpressionBlock();
                        }
                        else if (Scanner.CurrentToken == Token.OptionStart)
                        {
                            ReadAttribute();
                        }
                        else
                        {
                            ReadCodeOrLink(iNode);
                        }
                    }

                    ExpectOrSync(Token.LoopEnd, fObjectSyncPoints);
                }
                else
                {
                    Expect(Token.PointComma);
                }

                iNode.End = LastTokenPos;
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>The try get var split mode.</summary>
        /// <param name="node">The node.</param>
        private void TryGetVarSplitMode(NNLNeuronNode node)
        {
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                AdvanceWithSpaceSkip();
                if (node.Neurontype != NeuronType.Variable && node.Neurontype != NeuronType.Global)
                {
                    LogPosError("only variables and globals can have a split decleration section.", false);
                }

                node.SplitReaction = GetSplitReation();
                Expect(Token.GroupEnd);
            }
        }

        /// <summary>reads a var decl (ended with a comma) and adds it as a leaf tot he
        ///     current scope. This performs syncing when an error occured.</summary>
        /// <param name="varScope"></param>
        private void SaveReadVarDecl(VarScope varScope)
        {
            NNLLocalDeclNode iRes = null;
            try
            {
                iRes = ReadVarDecl(varScope);
                GetNeuronId(iRes);
                if (iRes != null)
                {
                    ConvertInitOfVarDecl(iRes);
                    CollectLeaf(iRes.Name, iRes);
                }

                while (Scanner.CurrentToken == Token.Comma)
                {
                    AdvanceWithSpaceSkip();
                    var iNew = new NNLLocalDeclNode();
                    SetNodeStart(iNew);
                    iNew.Scope = varScope;
                    iNew.TypeDecl = iRes.TypeDecl;
                    ReadVarDeclContent(iNew);
                    ConvertInitOfVarDecl(iNew);
                    CollectLeaf(iNew.Name, iNew);

                        // when the var has no init value, only render the var (as a child), don't need to render code for it.
                    iRes.SubDecls.Add(iNew); // let the first var decl render 1 Preperation instrunction
                }
            }
            catch
            {
                AdvanceToNextSyncPos();
                while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                       && Scanner.CurrentToken != Token.LoopEnd && Scanner.CurrentToken != Token.GroupEnd)
                {
                    // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                    Advance();
                }
            }

            if (varScope != VarScope.Local)
            {
                Expect(Token.PointComma); // var decl inside of neuron has to end with a ';'
            }
        }

        /// <summary>a non-local variable, that is an <see langword="int"/> or double, has
        ///     to be initialized to a new value. If there is no default values
        ///     specified, make certain that one is created.</summary>
        /// <param name="decl"></param>
        private void ConvertInitOfVarDecl(NNLLocalDeclNode decl)
        {
            if (decl.InitValue == null)
            {
                if (decl.TypeDecl == DeclType.Int)
                {
                    decl.InitValue = GetCreateNewInt();
                }
                else if (decl.TypeDecl == DeclType.Double)
                {
                    decl.InitValue = GetCreateNewDouble();
                }
            }
        }

        /// <summary>checks if there was a value specified and reads it when possible.</summary>
        /// <param name="node">The node.</param>
        private void GetValue(NNLNeuronNode node)
        {
            if (Scanner.CurrentToken == Token.Assign)
            {
                AdvanceWithSpaceSkip();
                if (node.Neurontype == NeuronType.Int)
                {
                    node.Value = GetStaticInt();
                }
                else if (node.Neurontype == NeuronType.Double)
                {
                    node.Value = GetStaticDouble();
                }
                else if (node.Neurontype == NeuronType.String)
                {
                    if (Scanner.CurrentToken == Token.SingleQuote)
                    {
                        node.Value = GetStaticString(Token.SingleQuote);
                    }
                    else
                    {
                        node.Value = GetStaticString(Token.Quote);
                    }
                }
                else if (node.Neurontype == NeuronType.Variable || node.Neurontype == NeuronType.Global)
                {
                    node.Value = ReadExpression();
                }
                else
                {
                    LogPosError("Neuron type can't have a static value.", false);
                    AdvanceWithSpaceSkip();
                }
            }
        }

        /// <summary>checks if there is an id declared for an object.</summary>
        /// <param name="node"></param>
        private void GetNeuronId(NNLStatementNode node)
        {
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Integer)
                {
                    ulong iId;
                    if (ulong.TryParse(Scanner.CurrentValue, out iId))
                    {
                        if (node != null)
                        {
                            node.ID = iId;
                        }
                    }
                    else
                    {
                        LogPosError("Integer expected", false);
                    }

                    AdvanceWithSpaceSkip();
                }
                else
                {
                    LogPosError("id number of neuron expected", false);
                }
            }
        }

        /// <summary>The get node type.</summary>
        /// <returns>The <see cref="NeuronType"/>.</returns>
        private NeuronType GetNodeType()
        {
            switch (Scanner.CurrentValue.ToLower())
            {
                case "neuron":
                    return NeuronType.Neuron;
                case "statement":
                    return NeuronType.Statement;
                case "resultstatement":
                    return NeuronType.ResultStatement;
                case "assignment":
                    return NeuronType.Assignment;
                case "boolexpression":
                    return NeuronType.BoolExpression;
                case "conditionalexpression":
                    return NeuronType.ConditionalExpression;
                case "conditionalstatement":
                    return NeuronType.ConditionalStatement;
                case "lockexpression":
                    return NeuronType.Lock;
                case "lock":
                    return NeuronType.Lock;
                case "byrefexpression":
                    return NeuronType.ByRef;
                case "byref":
                    return NeuronType.ByRef;
                case "int":
                    return NeuronType.Int;
                case "intneuron":
                    return NeuronType.Int;
                case "double":
                    return NeuronType.Double;
                case "doubleneuron":
                    return NeuronType.Double;
                case "var":
                    return NeuronType.Variable;
                case "variable":
                    return NeuronType.Variable;
                case "global":
                    return NeuronType.Global;
                case "textsin":
                    return NeuronType.TextSin;
                case "audiosin":
                    return NeuronType.AudioSin;
                case "imagesin":
                    return NeuronType.ImageSin;
                case "intsin":
                    return NeuronType.IntSin;
                case "reflectionsin":
                    return NeuronType.ReflectionSin;
                case "timersin":
                    return NeuronType.TimerSin;
                default:
                    return NeuronType.Neuron;
            }
        }

        /// <summary>
        ///     reads all the code in a cluster (if any) and all the other code lists
        ///     attached to the cluster. a possible meaning can be supplied, when
        ///     dropped, and the cluster needs to be created, 'code' is used.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         'cluster' identifier [(meaning)] [':' id] '{' ['this' '(' {param} ')'
        ///         [':' returnValue ] '{' code '}' ] { linkCode | VarDecl } '}'
        ///     </para>
        ///     <para>meaning = identifier {.identifier}</para>
        /// </remarks>
        private void ReadCluster()
        {
            var iNode = new NNLClusterNode();
            SetNodeStart(iNode);
            AdvanceWithSpaceSkip();

            var iName = GetIdentifier();
            CollectNS(iName, iNode);
            try
            {
                if (Scanner.CurrentToken == Token.GroupStart)
                {
                    AdvanceWithSpaceSkip();
                    iNode.Meaning = GetNSPath();
                    var iRef = iNode.Meaning as NNLPathNode;
                    if (iRef == null)
                    {
                        LogPosError("expecting a reference to a neuron (namespace path expected)", false);
                    }
                    else
                    {
                        var iLast = iRef.Items[iRef.Items.Count - 1] as NNLReferenceNode;
                        if (iLast != null && iLast.IsFunctionCall)
                        {
                            LogPosError("No function calls allowed as the meaning for a cluster", false);
                        }
                    }

                    Expect(Token.GroupEnd);
                }

                GetNeuronId(iNode);
                Expect(Token.LoopStart);
                while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
                {
                    if (Scanner.IsWord("this"))
                    {
                        AdvanceWithSpaceSkip();
                        iNode.Content = ReadFunctionDefContent(string.IsNullOrEmpty(iNode.ExternalDef) == false);
                    }
                    else if (Scanner.IsWord(VARDECLTYPES))
                    {
                        SaveReadVarDecl(VarScope.Var);
                    }
                    else if (Scanner.IsWord(CLUSTERNAMES))
                    {
                        ReadCluster();
                    }
                    else if (Scanner.IsWord(NEURONNAMES))
                    {
                        ReadNeuron();
                    }
                    else if (Scanner.IsWord("static"))
                    {
                        ReadNeuron(true);
                    }
                    else if (Scanner.IsWord("bind"))
                    {
                        ReadBinding();
                    }
                    else if (Scanner.IsWord("expressionsblock"))
                    {
                        ReadExpressionBlock();
                    }
                    else if (Scanner.CurrentToken == Token.OptionStart)
                    {
                        ReadAttribute();
                    }
                    else
                    {
                        ReadCodeOrLink(iNode);
                    }
                }

                ExpectOrSync(Token.LoopEnd, fObjectSyncPoints);
                iNode.End = LastTokenPos;
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>the link-name and code syntax: functionName '(' [Type paramName {,
        ///     type paramnName } ] ')' [':' Type ] [inline] FunctionName = ParamName
        ///     = identifier type = 'var' | 'variable' | 'int' | 'string' | 'double' |
        ///     'int[]' | 'double[]' | 'string[]' | 'var[]' ex: Find(var a,<see langword="int"/> b): var c, var e { ... }</summary>
        /// <param name="target">The target.</param>
        private void ReadCodeOrLink(NNLNode target)
        {
            var iName = GetIdentifier(); // this is the function name, but it becomes the name of the link.
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                ReadLink(target, iName);
            }
            else
            {
                bool iHasBody;
                var iNode = ReadLinkToCode(iName, out iHasBody);
                target.Functions.Add(iNode);
                ProcessAttributes(iNode);
                CheckBodyCode(string.IsNullOrEmpty(iNode.ExternalDef) == false, iHasBody);
            }
        }

        /// <summary>The check body code.</summary>
        /// <param name="hasExternalDef">The has external def.</param>
        /// <param name="iHasBody">The i has body.</param>
        private void CheckBodyCode(bool hasExternalDef, bool iHasBody)
        {
            if (iHasBody == false)
            {
                if (hasExternalDef == false)
                {
                    LogPosError("Functions that have no body need to be declared with the 'external' attribute.");
                }
            }
            else if (hasExternalDef)
            {
                LogPosError("When a function is declared as external, it can't have a body.");
            }
        }

        /// <summary>The read link to code.</summary>
        /// <param name="name">The name.</param>
        /// <param name="hasBody">The has body.</param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        private NNLFunctionNode ReadLinkToCode(string name, out bool hasBody)
        {
            var iCode = new NNLFunctionNode();
            iCode.Params = ReadParameters();
            iCode.ReturnValues = ReadReturnValue();
            iCode.IsInline = TryReadIsInline(iCode.Params, iCode.ReturnValues);
            if (Scanner.CurrentToken == Token.PointComma)
            {
                // the function has no body, which is possible if there is an external decleration.
                AdvanceWithSpaceSkip();
                hasBody = false;
            }
            else
            {
                hasBody = true;
                ReadClusterContent(name, iCode);

                    // this collects the function as a child of the current namespace (the neuron).
            }

            return iCode;
        }

        /// <summary>The read link.</summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        private void ReadLink(NNLNode target, string name)
        {
            AdvanceWithSpaceSkip();
            try
            {
                var iNode = new NNLLinkNode();
                iNode.Name = name;
                iNode.To = ReadExpression();
                target.Links.Add(iNode);
                iNode.Parent = target;
                ClearAttribs();
            }
            catch
            {
                while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.LoopStart
                       && Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd
                       && Scanner.CurrentToken != Token.GroupEnd)
                {
                    // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                    Advance();
                }
            }

            Expect(Token.PointComma);
        }

        /// <summary>The read return value.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<NNLLocalDeclNode> ReadReturnValue()
        {
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                var iRes = new System.Collections.Generic.List<NNLLocalDeclNode>();
                AdvanceWithSpaceSkip();
                var iType = new NNLLocalDeclNode();
                try
                {
                    ReadTypeDecl(iType);
                }
                catch
                {
                    while (Scanner.CurrentToken != Token.LoopStart && Scanner.CurrentToken != Token.End
                           && Scanner.CurrentToken != Token.LoopEnd && Scanner.CurrentToken != Token.GroupEnd)
                    {
                        // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                        Advance();
                    }
                }

                iRes.Add(iType);
                return iRes;
            }

            return null;
        }

        /// <summary>reads the parameters declarations of a cluster.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<NNLLocalDeclNode> ReadParameters()
        {
            var iRes = new System.Collections.Generic.List<NNLLocalDeclNode>();
            Expect(Token.GroupStart);
            if (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.PointComma
                && Scanner.CurrentToken != Token.LoopEnd && Scanner.CurrentToken != Token.GroupEnd)
            {
                iRes.Add(ReadParameter());
                while (Scanner.CurrentToken == Token.Comma)
                {
                    AdvanceWithSpaceSkip();
                    iRes.Add(ReadParameter());
                }
            }

            Expect(Token.GroupEnd);
            return iRes;
        }

        /// <summary>reads a single parameter.</summary>
        /// <returns>The <see cref="NNLLocalDeclNode"/>.</returns>
        private NNLLocalDeclNode ReadParameter()
        {
            NNLLocalDeclNode iRes = null;
            try
            {
                iRes = ReadVarDecl();
                iRes.IsParam = true; // so we know it doesn't need to rende some extra code for variables.
            }
            catch
            {
                while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                       && Scanner.CurrentToken != Token.LoopEnd && Scanner.CurrentToken != Token.GroupEnd)
                {
                    // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                    Advance();
                }
            }

            return iRes;
        }

        /// <summary><para>reads a local type decleration. ('var' | 'int' | 'int[]' | 'double' |
        ///         'double[]' | 'string' | 'string[]' | <see langword="bool"/> )
        ///         VarIdentifier {',' VarIdentifier }</para>
        /// <para>varIdentifier = identifier [ '(' 'duplicate' | 'copy' | 'clear' ')']
        ///         ['=' ResultStatement]</para>
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>The <see cref="NNLLocalDeclNode"/>.</returns>
        private NNLLocalDeclNode ReadVarDecl(VarScope scope = VarScope.Local)
        {
            var iRes = new NNLLocalDeclNode();
            SetNodeStart(iRes);
            iRes.Scope = scope;
            ReadTypeDecl(iRes);
            ReadVarDeclContent(iRes);
            return iRes;
        }

        /// <summary>The read var decl content.</summary>
        /// <param name="iRes">The i res.</param>
        private void ReadVarDeclContent(NNLLocalDeclNode iRes)
        {
            iRes.Name = GetIdentifier();

            if (Scanner.CurrentToken == Token.GroupStart)
            {
                AdvanceWithSpaceSkip();
                iRes.SplitReaction = GetSplitReation();
                Expect(Token.GroupEnd);
            }

            if (Scanner.CurrentToken == Token.Assign)
            {
                AdvanceWithSpaceSkip();
                iRes.InitValue = ReadExpression();
            }

            iRes.End = LastTokenPos;
        }

        /// <summary>The read type decl.</summary>
        /// <param name="local">The local.</param>
        private void ReadTypeDecl(NNLLocalDeclNode local)
        {
            if (Scanner.IsWord(VARNAMES))
            {
                local.TypeDecl = DeclType.Var;
                AdvanceWithSpaceSkip();
            }
            else if (Scanner.IsWord("int"))
            {
                Advance();
                if (Scanner.CurrentToken == Token.OptionStart)
                {
                    AdvanceWithSpaceSkip();
                    Expect(Token.OptionEnd);
                    local.TypeDecl = DeclType.IntAr;
                }
                else
                {
                    SkipSpaces();
                    local.TypeDecl = DeclType.Int;
                }
            }
            else if (Scanner.IsWord("bool"))
            {
                AdvanceWithSpaceSkip();
                local.TypeDecl = DeclType.Bool;
            }
            else if (Scanner.IsWord("double"))
            {
                Advance();
                if (Scanner.CurrentToken == Token.OptionStart)
                {
                    AdvanceWithSpaceSkip();
                    Expect(Token.OptionEnd);
                    local.TypeDecl = DeclType.DoubleAr;
                }
                else
                {
                    SkipSpaces();
                    local.TypeDecl = DeclType.Double;
                }
            }
            else if (Scanner.IsWord("string"))
            {
                Advance();
                if (Scanner.CurrentToken == Token.OptionStart)
                {
                    AdvanceWithSpaceSkip();
                    Expect(Token.OptionEnd);
                    local.TypeDecl = DeclType.StringAr;
                }
                else
                {
                    SkipSpaces();
                    local.TypeDecl = DeclType.String;
                }
            }
            else
            {
                LogPosError("Type decleration expected");
            }
        }

        /// <summary>The get split reation.</summary>
        /// <returns>The <see cref="ulong"/>.</returns>
        private ulong GetSplitReation()
        {
            if (Scanner.IsWord("duplicate"))
            {
                AdvanceWithSpaceSkip();
                return (ulong)PredefinedNeurons.Duplicate;
            }

            if (Scanner.IsWord("copy"))
            {
                AdvanceWithSpaceSkip();
                return (ulong)PredefinedNeurons.Copy;
            }

            if (Scanner.IsWord("shared"))
            {
                AdvanceWithSpaceSkip();
                return (ulong)PredefinedNeurons.shared;
            }

            if (Scanner.IsWord("clear"))
            {
                AdvanceWithSpaceSkip();
                return (ulong)PredefinedNeurons.Empty;
            }

            return 0;
        }

        /// <summary>'{' {code} '}'</summary>
        /// <param name="name">The name.</param>
        /// <param name="iNode">The i Node.</param>
        private void ReadClusterContent(string name, NNLFunctionNode iNode)
        {
            SetNodeStart(iNode);
            CollectNS(name, iNode); // a function is reachable through a name and is a namespace for lcoals.
            try
            {
                ReadCodeBlockSimple(iNode);
                iNode.End = LastTokenPos;
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>The read code block for other parser.</summary>
        /// <param name="list">The list.</param>
        /// <param name="allowSingleLine">The allow single line.</param>
        /// <param name="singleLineREquiresPointComma">The single line r equires point comma.</param>
        private void ReadCodeBlockForOtherParser(
            NNLFunctionNode list, 
            bool allowSingleLine, 
            bool singleLineREquiresPointComma)
        {
            fNS.Push(list); // a codeblock is a namespace for locals, but isn't reachable through a name.
            try
            {
                fCode.Push(list);
                try
                {
                    ReadCodeLines();
                }
                finally
                {
                    fCode.Pop();
                }
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>The read code block.</summary>
        /// <param name="list">The list.</param>
        /// <param name="allowSingleLine">The allow single line.</param>
        /// <param name="singleLineRequiresPointComma">The single line requires point comma.</param>
        private void ReadCodeBlock(
            NNLNodesList list, 
            bool allowSingleLine = false, 
            bool singleLineRequiresPointComma = true)
        {
            fNS.Push(list); // a codeblock is a namespace for locals, but isn't reachable through a name.
            try
            {
                ReadCodeBlockSimple(list, allowSingleLine, singleLineRequiresPointComma);
            }
            finally
            {
                fNS.Pop();
            }
        }

        /// <summary>The read code block simple.</summary>
        /// <param name="list">The list.</param>
        /// <param name="allowSingleLine">The allow single line.</param>
        /// <param name="singleLineRequiresPointComma">The single line requires point comma.</param>
        private void ReadCodeBlockSimple(
            NNLNodesList list, 
            bool allowSingleLine = false, 
            bool singleLineRequiresPointComma = true)
        {
            var iExpectPointComma = true;
            fCode.Push(list);
            try
            {
                if (allowSingleLine == false || Scanner.CurrentToken == Token.LoopStart)
                {
                    Expect(Token.LoopStart);
                    ReadCodeLines();
                    Expect(Token.LoopEnd);
                }
                else
                {
                    if (allowSingleLine == false)
                    {
                        LogPosError("{ expected");
                    }

                    try
                    {
                        iExpectPointComma = ReadCode();
                    }
                    catch
                    {
                        while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                               && Scanner.CurrentToken != Token.LoopEnd)
                        {
                            // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                            Advance();
                        }
                    }

                    if (iExpectPointComma && singleLineRequiresPointComma)
                    {
                        Expect(Token.PointComma);
                    }
                }
            }
            finally
            {
                fCode.Pop();
            }
        }

        /// <summary>The read code lines.</summary>
        private void ReadCodeLines()
        {
            var iExpectPointComma = true;
            while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != Token.LoopEnd)
            {
                try
                {
                    SkipSpaces(); // always skip any spaces before we try a new item.
                    iExpectPointComma = ReadCode();
                }
                catch
                {
                    while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                           && Scanner.CurrentToken != Token.LoopEnd)
                    {
                        // something went wrong during the parse, so advance the parser to the next sync point, which is the ';' token.
                        Advance();
                    }
                }

                if (iExpectPointComma)
                {
                    Expect(Token.PointComma);
                }
            }
        }

        /// <summary>If the specified <paramref name="token"/> is not found, an error is
        ///     written. OTherwise, the cursor is advanced with space skip.</summary>
        /// <param name="token"></param>
        private void Expect(Token token)
        {
            if (Scanner.CurrentToken == token)
            {
                AdvanceWithSpaceSkip();
            }
            else
            {
                LogPosError(Tokenizer.GetSymbolForToken(token) + " expected", false);
            }
        }

        /// <summary>checks if the specified <paramref name="token"/> can be found at the
        ///     current position. If not, the scanner advances to the next sync point.</summary>
        /// <param name="token"></param>
        /// <param name="toSync"></param>
        private void ExpectOrSync(Token token, Token[] toSync = null)
        {
            if (Scanner.CurrentToken == token)
            {
                AdvanceWithSpaceSkip();
            }
            else
            {
                LogPosError(Tokenizer.GetSymbolForToken(token) + " expected", false);
                AdvanceToNextSyncPos(toSync);
            }
        }

        /// <summary>The expect.</summary>
        /// <param name="value">The value.</param>
        private void Expect(string value)
        {
            if (Scanner.IsWord(value))
            {
                AdvanceWithSpaceSkip();
            }
            else
            {
                LogPosError(value + " expected", false);
            }
        }

        /// <summary>checks if the specified <paramref name="token"/> is at the curren
        ///     tposition. if not, an error is logged, otherwise, the scanner is
        ///     advanced to the nex <paramref name="token"/> (no space skip).</summary>
        /// <param name="token"></param>
        private void Consume(Token token)
        {
            if (Scanner.CurrentToken == token)
            {
                Advance();
            }
            else
            {
                LogPosError(Tokenizer.GetSymbolForToken(token) + " expected", false);
            }
        }

        /// <summary>if the specified <paramref name="token"/> is found, the scanner is
        ///     advanced (by default, no space skip) and <see langword="true"/> is
        ///     returned. Otherwise false.</summary>
        /// <param name="token"></param>
        /// <param name="skipSpaces">The skip Spaces.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ConsumeCurToken(Token token, bool skipSpaces = false)
        {
            if (Scanner.CurrentToken == token)
            {
                if (skipSpaces == false)
                {
                    Advance();
                }
                else
                {
                    AdvanceWithSpaceSkip();
                }

                return true;
            }

            return false;
        }

        /// <summary>reads an <see langword="int"/> nr and returns it after advancing. If
        ///     there is no int, an error is logged.</summary>
        /// <param name="spaceSkip">The space Skip.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetStaticInt(bool spaceSkip = true)
        {
            if (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Integer)
            {
                var iRes = Scanner.CurrentValue;
                if (spaceSkip)
                {
                    AdvanceWithSpaceSkip();
                }
                else
                {
                    Advance();
                }

                int iVal;

                if (int.TryParse(iRes, out iVal))
                {
                    return iVal;
                }

                LogPosError("Static int expected", false);
            }
            else
            {
                LogPosError("Static int expected", false);
            }

            return -1;
        }

        /// <summary>The get static double.</summary>
        /// <param name="spaceSkip">The space skip.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private double GetStaticDouble(bool spaceSkip = true)
        {
            if (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Decimal)
            {
                var iRes = Scanner.CurrentValue;
                if (spaceSkip)
                {
                    AdvanceWithSpaceSkip();
                }
                else
                {
                    Advance();
                }

                double iVal;

                if (double.TryParse(
                    iRes, 
                    System.Globalization.NumberStyles.AllowDecimalPoint, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out iVal))
                {
                    return iVal;
                }

                LogPosError("Static double expected", false);
            }
            else
            {
                LogPosError("Static double expected", false);
            }

            return double.NaN;
        }

        /// <summary>gets a <see langword="static"/> string value (no quotes included)</summary>
        /// <param name="token">The token.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetStaticString(Token token)
        {
            Consume(token);
            var iIsEscaped = Scanner.IsEscaped;
            var iStart = Scanner.CurrentStart;
            while (Scanner.CurrentToken != Token.End && Scanner.CurrentToken != token && Scanner.CurrentValue != "\n"
                   && Scanner.CurrentValue != "\r")
            {
                Advance();
            }

            var iEnd = Scanner.CurrentStart;
            Expect(token);
            var iRes = Scanner.ToParse.Substring(iStart, iEnd - iStart);
            if (iIsEscaped)
            {
                iRes = CheckEscapeSigns(iRes);
            }

            return iRes;
        }

        /// <summary>called when an escape sign was used '\'. Checks if the token that was
        ///     found, is one of the known escape characters, like \r, \n, \t, \s</summary>
        /// <param name="toCheck">The to Check.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string CheckEscapeSigns(string toCheck)
        {
            if (toCheck == "a")
            {
                return "\a";
            }

            if (toCheck == "b")
            {
                return "\b";
            }

            if (toCheck == "f")
            {
                return "\f";
            }

            if (toCheck == "n")
            {
                return "\n";
            }

            if (toCheck == "r")
            {
                return "\r";
            }

            if (toCheck == "t")
            {
                return "\t";
            }

            if (toCheck == "v")
            {
                return "\v";
            }

            return toCheck;
        }

        /// <summary>checks if the next token is a <see langword="static"/> int. if so,
        ///     this <paramref name="value"/> is returned, the cursor pos is advanced
        ///     with space-skip and <see langword="true"/> is returned, otherwise, no
        ///     advance is done and <see langword="false"/> is returned.</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool TryGetStaticInt(ref int value)
        {
            if (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Integer)
            {
                var iRes = Scanner.CurrentValue;
                int iVal;

                if (int.TryParse(iRes, out iVal))
                {
                    AdvanceWithSpaceSkip();
                    value = iVal;
                    return true;
                }
            }

            return false;
        }

        /// <summary>reads out an identifier if there is one and advances, otherwise an
        ///     error is logged.</summary>
        /// <param name="fail">if set to <c>true</c> an error will cause an exception.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetIdentifier()
        {
            if (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Word)
            {
                // an identifier can start with a letter or _
                var iRes = new System.Text.StringBuilder(Scanner.CurrentValue);
                Advance();
                while (Scanner.CurrentValue == "_")
                {
                    // the scanner treats _ as a seperate sign (for the other parser), but we don't, so solve this here.
                    iRes.Append("_");
                    Advance();
                    if (Scanner.CurrentToken == Token.Word)
                    {
                        iRes.Append(Scanner.CurrentValue);
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                SkipSpaces();
                return iRes.ToString();
            }

            if (Scanner.CurrentValue == "_")
            {
                var iRes = new System.Text.StringBuilder(Scanner.CurrentValue);
                Advance();
                while (Scanner.CurrentToken == Token.Word && Scanner.WordType == WordTokenType.Word)
                {
                    // the scanner treats _ as a seperate sign (for the other parser), but we don't, so solve this here.
                    iRes.Append(Scanner.CurrentValue);
                    Advance();
                    if (Scanner.CurrentValue == "_")
                    {
                        iRes.Append(Scanner.CurrentValue);
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                SkipSpaces();
                return iRes.ToString();
            }

            LogPosError("Identifier expected", false);
            AdvanceToNextSyncPos();
            return string.Empty;
        }

        /// <summary>advances the position to the next synchronisation point.</summary>
        /// <param name="syncPoints">The sync Points.</param>
        private void AdvanceToNextSyncPos(Token[] syncPoints = null)
        {
            if (syncPoints == null)
            {
                syncPoints = fCurSyncPoints;
            }

            while (Enumerable.Contains(syncPoints, Scanner.CurrentToken) == false)
            {
                Advance();
            }
        }

        /// <summary>advances the position to the next synchronisation point.</summary>
        /// <param name="syncPoints">The sync Points.</param>
        private void AdvanceToNextSyncPos(string[] syncPoints)
        {
            while (Scanner.IsWord(syncPoints) == false && Scanner.CurrentToken != Token.OptionStart
                   && Scanner.CurrentToken != Token.End)
            {
                // string sync points are for classes, so the attribute start should also be included in the sync points.
                Advance();
            }
        }

        /// <summary>
        ///     reads a single code line. Returns if a pointComma is expected or not
        ///     at the end of the line.
        /// </summary>
        /// <param name="inConditional"></param>
        /// <returns>
        ///     <see langword="true" /> if ; is expected after the statement. Otherwise
        ///     false.
        /// </returns>
        private bool ReadCode()
        {
            fCurSyncPoints = fCodeSyncPoints;
            try
            {
                var iIdentifier = TryGetCodeIdentifier();
                var iRes = false;
                if (Scanner.IsWord("lock"))
                {
                    ReadLock(iIdentifier);
                }
                else if (Scanner.IsWord("if"))
                {
                    ReadCond(NodeType.If, iIdentifier, "if");
                }
                else if (Scanner.IsWord("for"))
                {
                    ReadFor(iIdentifier);
                }
                else if (Scanner.IsWord("while"))
                {
                    ReadCond(NodeType.While, iIdentifier, "while");
                }
                else if (Scanner.IsWord("foreach"))
                {
                    ReadForEach(iIdentifier);
                }
                else if (Scanner.IsWord("switch"))
                {
                    ReadSwitch(iIdentifier);
                }
                else if (Scanner.IsWord("select"))
                {
                    ReadSelect(iIdentifier);
                }
                else
                {
                    iRes = true;
                    if (Scanner.IsWord("do"))
                    {
                        // from do...while
                        ReadDo(iIdentifier);
                    }
                    else if (Scanner.IsWord("else"))
                    {
                        // this is for invalid/orphaned 'else' statements, so we can continue the parse.
                        ReadElse();
                    }
                    else if (Scanner.IsWord("return"))
                    {
                        ReadReturn(iIdentifier);
                    }
                    else if (Scanner.IsWord(VARDECLTYPES))
                    {
                        ReadLocalDecl(iIdentifier);
                    }
                    else
                    {
                        CollectCode(ReadExpression(), iIdentifier);
                    }
                }

                return iRes;
            }
            finally
            {
                fCurSyncPoints = fObjectSyncPoints;
            }
        }

        /// <summary>select statements on queries, like <see langword="foreach"/> loops.</summary>
        /// <remarks>'select' '(' (varDecl| <see cref="Expression"/> ) { ',' varDecl|<see cref="Expression"/> } from [ 'in' | 'out' | 'child' | 'cluster' ]
        ///     identifier ')' codeblock</remarks>
        /// <param name="id">The id.</param>
        private void ReadSelect(string id)
        {
            var iNode = new NNLSelectNode();
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();
            Expect(Token.GroupStart);
            var iLoopVars = new System.Collections.Generic.List<NNLStatementNode>();
            NNLLocalDeclNode iPrevDecl = null;

                // so we can coppy the type of the previous decl, in case the 'var' word was ommited.
            if (Scanner.IsWord(VARDECLTYPES))
            {
                var iDecl = ReadVarDecl();
                ConvertInitOfLocalDecl(iDecl);
                CollectLeaf(iDecl.Name, iDecl);
                iLoopVars.Add(iDecl);
                iPrevDecl = iDecl;
            }
            else
            {
                iLoopVars.Add(ReadExpression());
            }

            while (Scanner.CurrentToken == Token.Comma)
            {
                AdvanceWithSpaceSkip();
                NNLLocalDeclNode iDecl = null;
                if (Scanner.IsWord(VARDECLTYPES))
                {
                    iDecl = ReadVarDecl();
                    iPrevDecl = iDecl;
                }
                else if (iPrevDecl != null)
                {
                    iDecl = new NNLLocalDeclNode();
                    SetNodeStart(iDecl);
                    iDecl.Scope = VarScope.Local;
                    if (iPrevDecl != null)
                    {
                        iDecl.TypeDecl = iPrevDecl.TypeDecl;
                    }

                    ReadVarDeclContent(iDecl);
                }
                else
                {
                    iLoopVars.Add(ReadExpression());
                }

                if (iDecl != null)
                {
                    ConvertInitOfLocalDecl(iDecl);
                    CollectLeaf(iDecl.Name, iDecl);
                    iLoopVars.Add(iDecl);
                }
            }

            iNode.Loopvars = iLoopVars;
            Expect("from");
            if (Scanner.IsWord(SELECTMODIFIERS))
            {
                iNode.Modifier = Scanner.CurrentValue;
                AdvanceWithSpaceSkip();
            }

            iNode.Condition = ReadExpression();
            Expect(Token.GroupEnd);
            ReadCodeBlock(iNode, true);
            iNode.End = LastTokenPos;
        }

        /// <summary>The read local decl.</summary>
        /// <param name="iIdentifier">The i identifier.</param>
        private void ReadLocalDecl(string iIdentifier)
        {
            var iDecl = ReadVarDecl();
            if (string.IsNullOrEmpty(iIdentifier) == false)
            {
                LogPosError("can't label a var decleration, it already has a name", false);
            }

            CollectCode(iDecl, null); // an init for the var always needs to be rendered.
            CollectLeaf(iDecl.Name, iDecl); // need to find it in the dict.                              
            ConvertInitOfLocalDecl(iDecl);
            while (Scanner.CurrentToken == Token.Comma)
            {
                AdvanceWithSpaceSkip();
                var iNew = new NNLLocalDeclNode();
                SetNodeStart(iNew);
                iNew.Scope = VarScope.Local;
                iNew.TypeDecl = iDecl.TypeDecl;
                ReadVarDeclContent(iNew);
                CollectLeaf(iNew.Name, iNew);

                    // need to find it in the dict. Don't collect in the code list, cause we will add it to the first decl as a subDecl, so it will handle the rendering.
                ConvertInitOfLocalDecl(iNew);
                iDecl.SubDecls.Add(iNew); // let the first var decl render 1 Preperation instrunction
            }
        }

        /// <summary>var declerations that have an init value and which are declared inside
        ///     code blocks, need to have their initializer moved to a seperate
        ///     assignment. this is because the initializer could call a function,
        ///     which would scew up certain things. Also, if you have something like:<see langword="int"/> a = x + 4; then, the init = x + 4, which gets
        ///     optimized to an <see langword="int"/> calculation that assigns it's
        ///     value to an already exisint intneuron. for this reason, the original
        ///     initializer is moved to an assign and replaced with a 'new'
        ///     instruction.</summary>
        /// <param name="decl"></param>
        private void ConvertInitOfLocalDecl(NNLLocalDeclNode decl)
        {
            if (decl.InitValue != null)
            {
                var iAssign = new NNLBinaryStatement(NodeType.Assign);
                iAssign.Operator = Token.Assign;
                iAssign.Start = decl.Start;
                iAssign.FileName = decl.FileName;
                iAssign.End = decl.End;
                iAssign.LeftPart = decl;
                iAssign.RightPart = decl.InitValue;
                CollectCode(iAssign, null);

                    // the assign doesn't need to be stored as the ref to the var, that needs to be the varDecl
            }

            if (decl.TypeDecl == DeclType.Int)
            {
                decl.InitValue = GetCreateNewInt();
            }
            else if (decl.TypeDecl == DeclType.Double)
            {
                decl.InitValue = GetCreateNewDouble();
            }
            else
            {
                decl.InitValue = null;
            }
        }

        /// <summary>The get create new double.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode GetCreateNewDouble()
        {
            if (fCreateNewDouble == null)
            {
                var iRes = new NNLPathNode();
                var iCall = new NNLReferenceNode();
                iCall.IsFunctionCall = true;
                iCall.Reference = "new";
                var iParam = new NNLNeuronNode(NeuronType.Neuron);
                iParam.ID = (ulong)PredefinedNeurons.DoubleNeuron;
                iCall.ParamValues = iParam;
                iCall.Parent = iRes;
                iRes.Items.Add(iCall);
                fCreateNewDouble = iRes;
            }

            return fCreateNewDouble;
        }

        /// <summary>The get create new int.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode GetCreateNewInt()
        {
            if (fCreateNewInt == null)
            {
                var iRes = new NNLPathNode();
                var iCall = new NNLReferenceNode();
                iCall.IsFunctionCall = true;
                iCall.Reference = "new";
                var iParam = new NNLNeuronNode(NeuronType.Neuron);
                iParam.ID = (ulong)PredefinedNeurons.IntNeuron;
                iCall.ParamValues = iParam;
                iRes.Items.Add(iCall);
                iCall.Parent = iRes;
                fCreateNewInt = iRes;
            }

            return fCreateNewInt;
        }

        /// <summary>'return' [expression]</summary>
        /// <param name="id">The id.</param>
        private void ReadReturn(string id)
        {
            var iRes = new NNLPathNode(); // we need a path to get it to render properly
            var iRef = new NNLReferenceNode();
            SetNodeStart(iRef);
            AdvanceWithSpaceSkip();
            iRef.IsFunctionCall = true;
            if (Scanner.CurrentToken != Token.PointComma)
            {
                // there doesn't have to be a return value.
                iRef.ParamValues = ReadExpression(); // this can also read in a comma seperated list.
                if (IsFunctionWithReturn() == false)
                {
                    LogPosError("return statements only allowed in functions that havea a return value.", false);
                }

                iRef.Reference = "returnvalue";
            }
            else if (IsFunctionWithReturn())
            {
                iRef.Reference = "returnvalue";
            }
            else
            {
                iRef.Reference = "return";
            }

            iRes.Items.Add(iRef);
            CollectCode(iRes, id);
            iRef.End = LastTokenPos;
        }

        /// <summary>checks if the function body currently being read, has a return value.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsFunctionWithReturn()
        {
            NNLFunctionNode iFunction = null;
            foreach (var iNode in fNS)
            {
                iFunction = iNode as NNLFunctionNode;
                if (iFunction != null)
                {
                    break;
                }
            }

            if (iFunction != null)
            {
                return iFunction.ReturnValues != null && iFunction.ReturnValues.Count > 0;
            }

            return false;
        }

        /// <summary>if there is a :id in front of a code item, get the name and return it.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string TryGetCodeIdentifier()
        {
            if (Scanner.CurrentToken == Token.LengthSpec)
            {
                Advance();
                return GetIdentifier();
            }

            return null;
        }

        /// <summary>reads an expression from the source and returns it.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        internal NNLStatementNode ReadExpression()
        {
            var iLeft = ReadOrExpression();
            var iOp = Token.End;
            while (IsAssignOperator(ref iOp))
            {
                var iRight = ReadOrExpression();
                var iStatement = new NNLBinaryStatement(NodeType.Assign);
                iStatement.Start = iLeft.Start;
                iStatement.FileName = ParserTitle;
                iStatement.End = LastTokenPos;
                iStatement.LeftPart = iLeft;
                iStatement.RightPart = iRight;
                iStatement.Operator = iOp;
                iLeft = iStatement;
            }

            return iLeft;
        }

        /// <summary>The read or expression.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadOrExpression()
        {
            var iRes = ReadAndExpression();
            while (Scanner.CurrentToken == Token.Or)
            {
                AdvanceWithSpaceSkip();
                var iNew = new NNLBinaryStatement(NodeType.ResultStatement);
                iNew.Start = iRes.Start;
                iNew.FileName = ParserTitle;
                iNew.Operator = Token.Or;
                iNew.LeftPart = iRes;
                iNew.RightPart = ReadAndExpression();
                iRes.End = LastTokenPos;
                iRes = iNew;
            }

            return iRes;
        }

        /// <summary>The read and expression.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadAndExpression()
        {
            var iRes = ReadCompareExpression();
            while (Scanner.CurrentToken == Token.DoubleAnd)
            {
                AdvanceWithSpaceSkip();
                var iNew = new NNLBinaryStatement(NodeType.ResultStatement);
                iNew.Start = iRes.Start;
                iNew.FileName = ParserTitle;
                iNew.Operator = Token.DoubleAnd;
                iNew.LeftPart = iRes;
                iNew.RightPart = ReadCompareExpression();
                iRes.End = LastTokenPos;
                iRes = iNew;
            }

            return iRes;
        }

        /// <summary>The read compare expression.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadCompareExpression()
        {
            var iRes = ReadListExpression();
            var iOp = Token.End;
            while (IsCompareOperator(ref iOp))
            {
                var iNew = new NNLBinaryStatement(NodeType.ResultStatement);
                iNew.Start = iRes.Start;
                iNew.Operator = iOp;
                iNew.LeftPart = iRes;
                iNew.RightPart = ReadListExpression();
                iRes.End = LastTokenPos;
                iRes = iNew;
            }

            return iRes;
        }

        /// <summary>The read list expression.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadListExpression()
        {
            var iRes = ReadAdd();
            NNLUnionNode iList = null;
            while (Scanner.CurrentToken == Token.Comma)
            {
                AdvanceWithSpaceSkip();
                var iNew = ReadAdd();
                if (iList == null)
                {
                    iList = new NNLUnionNode();
                    iList.Start = iRes.Start;
                    iNew.FileName = ParserTitle;
                    iList.Items.Add(iRes);
                    iRes = iList;
                }

                iList.Items.Add(iNew);
                iRes.End = LastTokenPos;
            }

            return iRes;
        }

        /// <summary>The read add.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadAdd()
        {
            var iLeft = ReadMultiply();
            var iOp = Token.End;
            while (IsAddOperator(ref iOp))
            {
                var iStatement = new NNLBinaryStatement(NodeType.ResultStatement);
                iStatement.Start = iLeft.Start;
                iStatement.FileName = ParserTitle;
                iStatement.LeftPart = iLeft;
                iStatement.RightPart = ReadMultiply();
                iStatement.End = LastTokenPos;
                iStatement.Operator = iOp;
                iLeft = iStatement;
            }

            return iLeft;
        }

        /// <summary>The read multiply.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadMultiply()
        {
            var iLeft = ReadUnaryExp();
            var iOp = Token.End;
            while (IsMultiplyOperator(ref iOp))
            {
                var iStatement = new NNLBinaryStatement(NodeType.ResultStatement);
                iStatement.Start = iLeft.Start;
                iStatement.FileName = ParserTitle;
                iStatement.LeftPart = iLeft;
                iStatement.RightPart = ReadUnaryExp();
                iStatement.End = LastTokenPos;
                iStatement.Operator = iOp;
                iLeft = iStatement;
            }

            return iLeft;
        }

        /// <summary>reads a binding path (primarerly for output/input patterns).</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        internal NNLStatementNode ReadBindingPath()
        {
            NNLStatementNode iRes = null;
            var iOp = Token.End;
            if (IsUnaryOperator(ref iOp))
            {
                iRes = BuildUnaryExp(iOp);
            }
            else
            {
                LogPosError("Binding path expected", false);
            }

            return iRes;
        }

        /// <summary>( ['waitfor'] <see langword="namespace"/> path | 'ref' '(' expression
        ///     ')' | 'L' '(' { OutputPatter } ')' | '(' expression ')' [ '++' | '--'
        ///     ] | ''' text ''' | '"' text '"' | <see langword="int"/> |<see langword="double"/> } . outputPattern = <see langword="static"/>
        ///     | variable | assetvariable | thesvariable | topicvariable .</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadUnaryExp()
        {
            NNLStatementNode iRes = null;
            var iOp = Token.End;
            if (IsUnaryOperator(ref iOp))
            {
                iRes = BuildUnaryExp(iOp);
                SkipSpaces();
            }
            else if (Scanner.CurrentToken == Token.GroupStart)
            {
                AdvanceWithSpaceSkip();
                var iStart = new FilePos();
                iStart.Pos = Scanner.CurrentStart;
                iStart.Line = CurLine;
                iStart.Column = 1 + (Scanner.CurrentStart - CurLineAt);
                if (Scanner.CurrentToken != Token.GroupEnd)
                {
                    iRes = ReadExpression();
                }
                else
                {
                    iRes = new NNLUnionNode(); // if no content, make certain we render an empty 'union' statement.
                }

                iRes.Start = iStart;
                iRes.FileName = ParserTitle;
                Expect(Token.GroupEnd);
                iRes.End = LastTokenPos;
                if (Scanner.CurrentToken == Token.MinusMinus || Scanner.CurrentToken == Token.AddAdd)
                {
                    iRes = BuildFunctionCall(iRes, Scanner.CurrentToken == Token.MinusMinus ? "decrement" : "increment");
                }
            }
            else if (Scanner.IsWord("ref"))
            {
                iRes = new NNLUnaryStatement(NodeType.ResultStatement);
                SetNodeStart(iRes);
                AdvanceWithSpaceSkip();
                Expect(Token.GroupStart);
                ((NNLUnaryStatement)iRes).Operator = Token.ByRef;
                ((NNLUnaryStatement)iRes).Child = ReadExpression();
                Expect(Token.GroupEnd);
                iRes.End = LastTokenPos;
            }
            else if (Scanner.IsWord("l"))
            {
                // a list
                iRes = ReadList();
            }
            else if (Scanner.CurrentToken == Token.Quote)
            {
                var iStatic = new NNLStaticNode(NeuronType.String);
                SetNodeStart(iStatic);
                iStatic.Value = GetStaticString(Token.Quote);
                iRes = iStatic;
                iRes.End = LastTokenPos;
            }
            else if (Scanner.CurrentToken == Token.SingleQuote)
            {
                var iStatic = new NNLStaticNode(NeuronType.String);
                SetNodeStart(iStatic);
                iStatic.Value = GetStaticString(Token.SingleQuote);
                iStatic.InDict = true;
                iRes = iStatic;
                iRes.End = LastTokenPos;
            }
            else if (Scanner.WordType == WordTokenType.Decimal)
            {
                var iStatic = new NNLStaticNode(NeuronType.Double);
                SetNodeStart(iStatic);
                iStatic.Value = GetStaticDouble();
                iRes = iStatic;
                iRes.End = LastTokenPos;
            }
            else if (Scanner.WordType == WordTokenType.Integer)
            {
                var iStatic = new NNLStaticNode(NeuronType.Int);
                SetNodeStart(iStatic);
                iStatic.Value = GetStaticInt();
                iRes = iStatic;
                iRes.End = LastTokenPos;
            }
            else
            {
                bool iWaitFor;
                if (Scanner.IsWord("waitfor"))
                {
                    iWaitFor = true;
                    AdvanceWithSpaceSkip();
                }
                else
                {
                    iWaitFor = false;
                }

                iRes = GetNSPath(iWaitFor);
            }

            return iRes;
        }

        /// <summary>The read list.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadList()
        {
            var iOp = Token.End;
            NNLStatementNode iRes;
            var iList = new NNLListNode();
            SetNodeStart(iList);
            AdvanceWithSpaceSkip();
            Expect(Token.GroupStart);
            while (Scanner.CurrentToken != Token.End && (Scanner.CurrentToken != Token.GroupEnd || Scanner.IsEscaped))
            {
                // when the ) is escaped, keep reading.
                if (IsUnaryOperator(ref iOp) && iOp != Token.Minus)
                {
                    // the - sign is not part of the bindings, but is a unary operator, filter on this.
                    iRes = BuildUnaryExp(iOp);
                }
                else
                {
                    NNLStaticNode iStatic;
                    if (Scanner.WordType == WordTokenType.Decimal)
                    {
                        iStatic = new NNLStaticNode(NeuronType.Double);
                        SetNodeStart(iStatic);
                        iStatic.Value = GetStaticDouble(false);
                    }
                    else if (Scanner.WordType == WordTokenType.Integer && Scanner.CurrentValue[0] != '0')
                    {
                        // when we have a number that starts with 0, we prefer the text version (like 007)
                        iStatic = new NNLStaticNode(NeuronType.Int);
                        SetNodeStart(iStatic);
                        iStatic.Value = GetStaticInt(false);
                    }
                    else
                    {
                        iStatic = new NNLStaticNode(NeuronType.String);
                        SetNodeStart(iStatic);
                        iStatic.Value = Scanner.CurrentValue;
                        iStatic.InDict = true;
                        Advance(); // a list also collects spaces.
                    }

                    iRes = iStatic;
                    iRes.End = LastTokenPos;
                }

                iList.Items.Add(iRes);
            }

            Expect(Token.GroupEnd);
            iList.End = LastTokenPos;
            return iList;
        }

        /// <summary>The build unary exp.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode BuildUnaryExp(Token iOp)
        {
            NNLStatementNode iRes = null;
            NNLStatementNode iChild;
            if (iOp == Token.Minus)
            {
                // a -x is different compared to an $x
                iChild = ReadExpression();
            }
            else
            {
                iChild = ReadPath(iOp);
            }

            if (iChild is NNLStaticNode)
            {
                // we can simply inver the value of a static while we are parsing.
                var iStatic = (NNLStaticNode)iChild;
                if (iStatic.Value is int)
                {
                    iStatic.Value = -((int)iStatic.Value);
                }
                else if (iStatic.Value is double)
                {
                    iStatic.Value = -((double)iStatic.Value);
                }
                else
                {
                    LogPosError("a - can only be placed in front of an int or a double", false);
                }

                iRes = iStatic;
            }
            else
            {
                iRes = new NNLUnaryStatement(NodeType.ResultStatement);
                SetNodeStart(iRes);
                ((NNLUnaryStatement)iRes).Operator = iOp;
                ((NNLUnaryStatement)iRes).Child = iChild;
            }

            return iRes;
        }

        /// <summary>a <see langword="namespace"/> path, ended by the name of a neuron,
        ///     possibly with some argument values (if it 's a callable item like an
        ///     expressionBlock).</summary>
        /// <param name="waitFor">The wait For.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode GetNSPath(bool waitFor = false)
        {
            var iList = new NNLPathNode();
            SetNodeStart(iList);
            NNLStatementNode iRes = iList;
            var iRef = new NNLReferenceNode();
            iRef.Reference = GetIdentifier();
            iList.Items.Add(iRef);
            while (Scanner.CurrentToken == Token.Dot)
            {
                AdvanceWithSpaceSkip();
                iRef = new NNLReferenceNode();
                iRef.Reference = GetIdentifier();
                iList.Items.Add(iRef);
            }

            if (Scanner.CurrentToken == Token.GroupStart)
            {
                // it's a function call.
                iRef.IsFunctionCall = true;
                iRef.IsWaitFor = waitFor;
                AdvanceWithSpaceSkip();
                if (Scanner.CurrentToken != Token.GroupEnd)
                {
                    iRef.ParamValues = ReadExpression(); // this can also read in a comma seperated list.
                    var iParamValues = iRef.ParamValues as NNLNodesList;
                    if (iParamValues != null)
                    {
                        iParamValues.IsParam = true;

                            // list needs to know that it is for param values, so that it can render differently.
                    }
                }

                Expect(Token.GroupEnd);
            }
            else if (waitFor)
            {
                LogPosError("WaitFor only allowed with function calls", false);
            }
            else if (iRef.Reference == "break")
            {
                LogPosWarning(
                    "break is normally used like a function. Are you sure you want a refernce to the instruction instead of calling it. To call the function, write 'break()'.");
            }

            if (Scanner.CurrentToken == Token.OptionStart)
            {
                var iIndex = ReadIndex(iList);
                iRes = iIndex != null ? iIndex : iRes;
            }

            if (Scanner.CurrentToken == Token.MinusMinus || Scanner.CurrentToken == Token.AddAdd)
            {
                iRes = BuildFunctionCall(iRes, Scanner.CurrentToken == Token.MinusMinus ? "decrement" : "increment");
            }

            iRes.End = LastTokenPos;
            return iRes;
        }

        /// <summary>The build function call.</summary>
        /// <param name="args">The args.</param>
        /// <param name="functionName">The function name.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode BuildFunctionCall(NNLStatementNode args, string functionName)
        {
            var iRes = new NNLPathNode();
            var iRef = new NNLReferenceNode();
            iRef.Reference = functionName;
            iRef.IsFunctionCall = true;
            iRef.ParamValues = args;
            args = iRef;
            AdvanceWithSpaceSkip();
            iRes.Items.Add(iRef);
            return iRes;
        }

        /// <summary>The read index.</summary>
        /// <param name="iList">The i list.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadIndex(NNLPathNode iList)
        {
            iList.End = LastTokenPos;
            AdvanceWithSpaceSkip();
            var iIndexer = new NNLBinaryStatement(NodeType.Index);
            iIndexer.Start = iList.Start;
            iIndexer.FileName = ParserTitle;
            iIndexer.Operator = Token.OptionStart;
            iIndexer.LeftPart = ReadExpression();

                // we need to inverse the order of the arguments: the index must come first, then the items to select from.
            iIndexer.RightPart = iList;
            Expect(Token.OptionEnd);
            return iIndexer;
        }

        /// <summary>everything</summary>
        /// <returns>The <see cref="NNLBindingPathItem"/>.</returns>
        private NNLBindingPathItem GetBracketedIdentifier()
        {
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                NNLBindingPathItem iRes;
                AdvanceWithSpaceSkip();
                var iOp = Token.End;
                if (IsUnaryOperator(ref iOp))
                {
                    var iRef = new NNLBindingPathItemSubPath();
                    SetNodeStart(iRef);
                    iRef.Node = BuildUnaryExp(iOp);
                    iRes = iRef;
                    SkipSpaces(); // the unaryExp doesnt' skip spaces, we do this.
                }
                else
                {
                    var iRefs = new NNLCompoundRefNode();
                    SetNodeStart(iRefs);
                    while (Scanner.CurrentToken != Token.GroupEnd && Scanner.CurrentToken != Token.End
                           && Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.LoopEnd)
                    {
                        iRefs.Compound.Add(Scanner.CurrentValue);
                        AdvanceWithSpaceSkip();
                    }

                    iRefs.Name = Scanner.ToParse.Substring(iRefs.Start.Pos, Scanner.CurrentStart - iRefs.Start.Pos);

                        // get the exact text and store it as the name, this way we can easely assign it later on.
                    iRes = iRefs;
                }

                iRes.End = LastTokenPos;
                Consume(Token.GroupEnd); // don't skip spaces. This can be a problem for output patterns.
                return iRes;
            }

            if (Scanner.CurrentToken != Token.End)
            {
                var iRef = new NNLBindingPathDotItem();
                SetNodeStart(iRef);
                iRef.Name = Scanner.CurrentValue;
                Advance(); // don't skip spaces. This can be a problem for output patterns.
                iRef.End = LastTokenPos;
                return iRef;
            }

            LogPosError("Unexpected end: (bracketed) identifier expected.", false);
            return null;
        }

        /// <summary>The read path.</summary>
        /// <param name="op">The op.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadPath(Token op)
        {
            var iRes = new NNLBindingPathNode();
            SetNodeStart(iRes);

            if (op != Token.At)
            {
                iRes.First = GetBracketedIdentifier();
            }
            else
            {
                iRes.First = GetVarReference();
            }

            if (Scanner.CurrentToken == Token.GroupStart)
            {
                // first item could be a function call.
                AdvanceWithSpaceSkip();
                var icall = new NNLBindingPathRefItem();
                var iPath = new NNLPathNode(); // this contains a function call.
                icall.Node = iPath;
                var iRef = new NNLReferenceNode();
                iPath.Items.Add(iRef);
                iRef.Reference = iRes.First.Name; // the name of the function.
                iRef.IsFunctionCall = true;
                if (Scanner.CurrentToken != Token.GroupEnd)
                {
                    // only try to read any parameters when the arg list isn't immediately closed: ()
                    iRef.ParamValues = ReadExpression();
                }

                Consume(Token.GroupEnd); // don't skip spaces. This can be a problem for output patterns.
                if (iRes.First is NNLCompoundRefNode)
                {
                    LogPosError("Unknown function: " + iRes.First.Name);
                }

                iRes.First = icall;
            }
            else if (Scanner.CurrentToken == Token.OptionStart)
            {
                var iIndex = new NNLPathIndexNode();
                SetNodeStart(iIndex);
                AdvanceWithSpaceSkip();
                iIndex.IndexValue = ReadExpression();
                Consume(Token.OptionEnd); // don't skip spaces. This can be a problem for output patterns.
                iRes.End = LastTokenPos;
                iRes.Items.Add(iIndex);
            }

            var iOp = Token.End;
            while (IsPathOperator(ref iOp) || IsOptionStart(ref iOp))
            {
                switch (iOp)
                {
                    case Token.LengthSpec:
                        var icall = new NNLPathCallNode();
                        SetNodeStart(icall);
                        if (Scanner.CurrentToken != Token.Word || Scanner.WordType != WordTokenType.Word)
                        {
                            LogPosError("identifier expected");
                        }

                        icall.ToCall = Scanner.CurrentValue;
                        Advance(); // don't skip spaces. This can be a problem for output patterns.
                        if (Scanner.CurrentToken == Token.GroupStart)
                        {
                            AdvanceWithSpaceSkip();
                            if (Scanner.CurrentToken != Token.GroupEnd)
                            {
                                icall.ParamValues = ReadExpression();
                            }

                            Consume(Token.GroupEnd); // don't skip spaces. This can be a problem for output patterns.
                        }

                        iRes.Items.Add(icall);
                        icall.End = LastTokenPos;
                        break;
                    case Token.Dot:
                        var iToAdd = GetBracketedIdentifier();
                        if (iToAdd != null)
                        {
                            iRes.Items.Add(iToAdd);
                        }

                        break;
                    case Token.ArrowRight:
                        var iAr = new NNLBindingPathArrowItem(Token.ArrowRight);
                        SetNodeStart(iAr);
                        iAr.PointsTo = GetBracketedIdentifier();
                        iRes.Items.Add(iAr);
                        iAr.End = LastTokenPos;
                        break;
                    case Token.ArrowLeft:
                        iAr = new NNLBindingPathArrowItem(Token.ArrowLeft);
                        SetNodeStart(iAr);
                        iAr.PointsTo = GetBracketedIdentifier();
                        iRes.Items.Add(iAr);
                        iAr.End = LastTokenPos;
                        break;
                    case Token.OptionStart:
                        var iIndex = new NNLPathIndexNode();
                        SetNodeStart(iIndex);
                        iIndex.IndexValue = ReadExpression();
                        Consume(Token.OptionEnd); // don't skip spaces. This can be a problem for output patterns.
                        iRes.End = LastTokenPos;
                        iRes.Items.Add(iIndex);
                        break;
                    default:
                        LogPosError("Internal error: unknown var path operator.");
                        break;
                }
            }

            iRes.End = LastTokenPos;
            return iRes;
        }

        /// <summary><para>reads a binding path item that references a varialbe name or some
        ///         other part of the code. This is used for statements like @varname
        ///         which provide access to nnl variables in binding paths.</para>
        /// <para>ex: @varname @(ns.path.varname)</para>
        /// </summary>
        /// <returns>The <see cref="NNLBindingPathItem"/>.</returns>
        private NNLBindingPathItem GetVarReference()
        {
            var iRes = new NNLBindingPathRefItem();
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                var iRefs = new NNLCompoundRefNode();
                SetNodeStart(iRefs);
                AdvanceWithSpaceSkip();
                iRes.Node = (NNLPathNode)GetNSPath();
                Expect(Token.GroupEnd);
                iRefs.End = LastTokenPos;
            }
            else
            {
                var iNode = new NNLPathNode();
                SetNodeStart(iNode);
                var iRef = new NNLReferenceNode();
                SetNodeStart(iRef);
                iRef.Reference = Scanner.CurrentValue;
                AdvanceWithSpaceSkip();
                iRef.End = LastTokenPos;
                iNode.Items.Add(iRef);
                iNode.End = LastTokenPos;
                iRes.Node = iNode;
            }

            return iRes;
        }

        /// <summary>The is compare operator.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsCompareOperator(ref Token iOp)
        {
            iOp = Token.End;
            if (Enumerable.Contains(COMPAREOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
            }
            else if (Scanner.IsWord("contains"))
            {
                iOp = Token.Contains;
            }
            else if (Scanner.IsWord("notcontains"))
            {
                iOp = Token.NotContains;
            }

            if (iOp != Token.End)
            {
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>The is add operator.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsAddOperator(ref Token iOp)
        {
            if (Enumerable.Contains(ADDOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>The is multiply operator.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsMultiplyOperator(ref Token iOp)
        {
            if (Enumerable.Contains(MULTIPLYOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>checks if the current token is one of the assign operators, if so,
        ///     this</summary>
        /// <param name="iOp"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsAssignOperator(ref Token iOp)
        {
            if (Enumerable.Contains(ASSIGNOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>The is unary operator.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsUnaryOperator(ref Token iOp)
        {
            if (Enumerable.Contains(UNARYOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>The is path operator.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsPathOperator(ref Token iOp)
        {
            if (Enumerable.Contains(PATHOPS, Scanner.CurrentToken))
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>The is option start.</summary>
        /// <param name="iOp">The i op.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool IsOptionStart(ref Token iOp)
        {
            if (Scanner.CurrentToken == Token.OptionStart)
            {
                iOp = Scanner.CurrentToken;
                AdvanceWithSpaceSkip();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     this is for orphaned 'else' parts.
        /// </summary>
        private void ReadElse()
        {
            LogPosError("no corresponding if for this else");
            AdvanceWithSpaceSkip();
            var iNeedCond = Scanner.IsWord("if") || Scanner.IsWord("while");
            ReadConditionalPart(iNeedCond); // don't collect the value for this conditional part, knowhere to put it.
        }

        /// <summary>'switch' ['while'] '{' { caseItem } [DefaultCaseItem] '}' CaseItem =
        ///     'case' <see cref="Expression"/> ':' InneerCodeBlock . DefaultCaseItem
        ///     = 'default' ':' InneerCodeBlock .</summary>
        /// <param name="id"></param>
        private void ReadSwitch(string id)
        {
            var iNode = new NNLSwitchNode();
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();

            if (Scanner.IsWord("while"))
            {
                AdvanceWithSpaceSkip();
                iNode.IsLooped = true;
            }

            Expect(Token.GroupStart);
            iNode.Condition = ReadExpression(); // the item to switch on.
            Expect(Token.GroupEnd);
            Expect(Token.LoopStart);
            ReadSwitchContentItem(iNode);
            ReadSwitchDefaultItem(iNode);
            Expect(Token.LoopEnd);
            iNode.End = LastTokenPos;
        }

        /// <summary>reads a single case item or reference to a case item.</summary>
        /// <param name="iNode"></param>
        private void ReadSwitchContentItem(NNLSwitchNode iNode)
        {
            var iLoop = true;
            while (iLoop)
            {
                var iId = TryGetCodeIdentifier();
                if (Scanner.IsWord("case"))
                {
                    AdvanceWithSpaceSkip();
                    iNode.Items.Add(ReadCase());
                    if (string.IsNullOrEmpty(iId) == false)
                    {
                        CollectInClass(iId, iNode.Items[iNode.Items.Count - 1]);
                    }

                    iLoop = true;
                }
                else if (Scanner.CurrentToken == Token.Word && Scanner.IsWord("default") == false
                         && Scanner.NextStart < Scanner.ToParse.Length && Scanner.ToParse[Scanner.NextStart] == ';')
                {
                    // it's a reference to a case item.
                    var iRef = GetNSPath();
                    iNode.Items.Add(iRef);
                    Expect(Token.PointComma);
                    iLoop = true;
                }
                else
                {
                    iLoop = false;
                }
            }
        }

        /// <summary>reads a possible 'default' value for the switch. Doesn't need to check
        ///     for a reference, this is done by<see cref="NNLParser.ReadSwitchContentItem"/> .</summary>
        /// <param name="iNode"></param>
        private void ReadSwitchDefaultItem(NNLSwitchNode iNode)
        {
            var iId = TryGetCodeIdentifier();
            if (Scanner.IsWord("default"))
            {
                var iDefault = new NNLCondPartNode();
                SetNodeStart(iDefault);
                AdvanceWithSpaceSkip();
                iNode.Items.Add(iDefault);
                Expect(Token.LengthSpec);
                ReadCodeBlock(iDefault, true);
                if (string.IsNullOrEmpty(iId) == false)
                {
                    CollectInClass(iId, iDefault);
                }

                iDefault.End = LastTokenPos;
            }
        }

        /// <summary>The read case.</summary>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadCase()
        {
            var iNode = new NNLCondPartNode();
            SetNodeStart(iNode);
            iNode.Condition = ReadExpression();
            Expect(Token.LengthSpec);
            ReadCodeBlock(iNode, true);
            iNode.End = LastTokenPos;
            return iNode;
        }

        /// <summary><para>reads a for loop.</para>
        /// <para>'for' '(' vardecl ';' expression ';' expression ')' codeblock</para>
        /// </summary>
        /// <param name="id">The id.</param>
        private void ReadFor(string id)
        {
            var iNode = new NNLForNode();
            SetNodeStart(iNode);
            AdvanceWithSpaceSkip();
            Expect(Token.GroupStart);
            ReadForVarDecl(iNode);
            CollectCode(iNode, id);
            ReadForCond(iNode);
            try
            {
                iNode.Incrementor = ReadExpression();
            }
            catch
            {
                AdvanceToNextSyncPos();
            }

            Expect(Token.GroupEnd);
            ReadCodeBlock(iNode, true);
            iNode.End = LastTokenPos;
        }

        /// <summary>The read for cond.</summary>
        /// <param name="iNode">The i node.</param>
        private void ReadForCond(NNLForNode iNode)
        {
            try
            {
                iNode.Condition = ReadExpression();
            }
            catch
            {
                AdvanceToNextSyncPos();
            }

            Expect(Token.PointComma);
        }

        /// <summary>The read for var decl.</summary>
        /// <param name="iNode">The i node.</param>
        private void ReadForVarDecl(NNLForNode iNode)
        {
            try
            {
                var iDecl = ReadVarDecl(VarScope.Local);
                iNode.LoopVar = iDecl;
                if (iDecl.InitValue == null)
                {
                    LogPosError("Variable declaration should be initialized");
                }

                ConvertInitOfLocalDecl(iDecl);
                CollectLeaf(iNode.LoopVar.Name, iNode.LoopVar);

                    // need to add the var to the dict so that it is known within the scope
            }
            catch
            {
                AdvanceToNextSyncPos();
            }

            Expect(Token.PointComma);
        }

        /// <summary>'foreach' '(' (varDecl | Expression) 'in' expression ')'
        ///     InnercodeBlock</summary>
        /// <param name="id"></param>
        private void ReadForEach(string id)
        {
            var iNode = new NNLForEachNode();
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();
            Expect(Token.GroupStart);
            if (Scanner.IsWord(VARDECLTYPES))
            {
                var iDecl = ReadVarDecl();
                ConvertInitOfLocalDecl(iDecl);
                CollectLeaf(iDecl.Name, iDecl);
                iNode.LoopVar = iDecl;
            }
            else
            {
                iNode.LoopVar = ReadExpression();
            }

            Expect("in");
            iNode.Condition = ReadExpression();
            Expect(Token.GroupEnd);
            ReadCodeBlock(iNode, true);
            iNode.End = LastTokenPos;
        }

        /// <summary>for do...while statements. 'do' InnerCodeBlock 'while' '('<see cref="Expression"/> ')'</summary>
        /// <param name="id">The id.</param>
        private void ReadDo(string id)
        {
            var iNode = new NNLCondStatement(NodeType.Do);
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();
            var iPart = new NNLCondPartNode();
            iNode.Items.Add(iPart);
            var iId = TryGetCodeIdentifier();
            if (string.IsNullOrEmpty(iId) == false)
            {
                CollectInClass(iId, iPart);
            }

            ReadCodeBlock(iNode, true);
            Expect("while");
            Expect(Token.GroupStart);
            iPart.Condition = ReadExpression();
            Expect(Token.GroupEnd);
            iNode.End = LastTokenPos;
        }

        /// <summary>reads a conditional (if/while) ('if'|'while') '(' expression ')
        ///     CodeBlock { 'else' 'if' '(' expression ') CodeBlock } ['else'
        ///     CodeBlock]</summary>
        /// <param name="nodeType"></param>
        /// <param name="id"></param>
        /// <param name="condType">the value 'if' or 'while'</param>
        private void ReadCond(NodeType nodeType, string id, string condType)
        {
            var iNode = new NNLCondStatement(nodeType);
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();
            iNode.Items.Add(ReadConditionalPart(true));
            while (Scanner.IsWord("else"))
            {
                AdvanceWithSpaceSkip();
                if (Scanner.IsWord(condType))
                {
                    AdvanceWithSpaceSkip();
                    iNode.Items.Add(ReadConditionalPart(true));
                }
                else
                {
                    iNode.Items.Add(ReadConditionalPart(false));
                }
            }

            iNode.End = LastTokenPos;
        }

        /// <summary>The read conditional part.</summary>
        /// <param name="needCondition">The need condition.</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode ReadConditionalPart(bool needCondition)
        {
            var iNode = new NNLCondPartNode();
            SetNodeStart(iNode);
            if (Scanner.CurrentToken == Token.GroupStart)
            {
                AdvanceWithSpaceSkip();
                iNode.Condition = ReadExpression();
                Expect(Token.GroupEnd);
            }
            else if (needCondition)
            {
                LogPosError("condition expected", false);
            }

            var iId = TryGetCodeIdentifier();
            ReadCodeBlock(iNode, true);
            if (string.IsNullOrEmpty(iId) == false)
            {
                CollectInClass(iId, iNode);
            }

            iNode.End = LastTokenPos;
            return iNode;
        }

        /// <summary>The set node start.</summary>
        /// <param name="node">The node.</param>
        private void SetNodeStart(NNLStatementNode node)
        {
            var iPos = new FilePos();
            iPos.Pos = Scanner.CurrentStart;
            iPos.Line = CurLine;
            iPos.Column = 1 + (Scanner.CurrentStart - CurLineAt);
            node.Start = iPos;
            node.FileName = ParserTitle;
        }

        /// <summary>'lock' [CodeId] '(' <see cref="Expression"/> ')' CodeBlock</summary>
        /// <param name="id"></param>
        private void ReadLock(string id)
        {
            var iNode = new NNLLockNode();
            CollectCodeAndStart(iNode, id);
            AdvanceWithSpaceSkip();
            Expect(Token.GroupStart);
            var iItems = new System.Collections.Generic.List<NNLStatementNode>();
            while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                   && Scanner.CurrentToken != Token.GroupEnd)
            {
                iItems.Add(ReadExpression());
            }

            iNode.NeuronLocks = iItems;
            if (Scanner.CurrentToken == Token.PointComma)
            {
                iItems = new System.Collections.Generic.List<NNLStatementNode>();
                while (Scanner.CurrentToken != Token.PointComma && Scanner.CurrentToken != Token.End
                       && Scanner.CurrentToken != Token.GroupEnd)
                {
                    iItems.Add(ReadExpression());
                }

                iNode.LinkLocks = iItems;
            }

            Expect(Token.GroupEnd);
            var iId = TryGetCodeIdentifier();
            ReadCodeBlock(iNode, true);
            if (string.IsNullOrEmpty(iId) == false)
            {
                CollectInClass(iId, iNode);
            }

            iNode.End = LastTokenPos;
        }

        /// <summary>stores the <paramref name="node"/> with the specified<paramref name="name"/> in the currently active class (or root).
        ///     Checks if the <paramref name="name"/> is unique in the current
        ///     context, if not, it logs a non destructive error (parsing continues).</summary>
        /// <param name="name">The name.</param>
        /// <param name="node">The node.</param>
        internal void CollectNS(string name, NNLNode node)
        {
            node.Name = name;
            var iNS = fNS.Peek();
            node.Parent = iNS;
            if (iNS.Children.ContainsKey(name))
            {
                LogPosError(name + " already used in the current scope", false);
                iNS.Children[name] = node;
            }
            else
            {
                iNS.Children.Add(name, node);
                if (iNS.IsDefinedInParent(name))
                {
                    LogPosWarning(string.Format("{0} hides an object in the parent namespace", name));
                }
            }

            fNS.Push(node);
            ProcessAttributes(node);
        }

        /// <summary>walks through all the attributes that were assigned to the<paramref name="node"/> and executes them.</summary>
        /// <param name="node"></param>
        private void ProcessAttributes(NNLNode node)
        {
            if (fAttributes.Count > 0)
            {
                foreach (var i in fAttributes)
                {
                    if (i.Count == 1)
                    {
                        if (i[0].ToLower() == "callos")
                        {
                            fCompiler.CallOSFunction = node;
                        }
                        else
                        {
                            LogPosError("invalid attribute: " + i[0], false);
                        }
                    }
                    else if (i.Count > 2)
                    {
                        if (i[0].ToLower() == "external")
                        {
                            i.RemoveAt(0);
                            node.ExternalDef = string.Concat(i); // rebuld the xml element data.
                        }
                        else if (i[0].ToLower() == "property")
                        {
                            i.RemoveAt(0);
                            node.ModuleProperty = string.Concat(i);
                        }
                        else
                        {
                            LogPosError("Invalid attribute: " + string.Concat(i), false);
                        }
                    }
                    else
                    {
                        LogPosError("Invalid attribute: " + string.Concat(i), false);
                    }
                }

                fAttributes.Clear();
            }
        }

        /// <summary>Collects the binding. If there is a previous binding with the same
        ///     name for the same operator, this is returned, so that a previously
        ///     defined binding can be expanded upon.</summary>
        /// <param name="node">The node.</param>
        /// <returns>The <see cref="NNLBinding"/>.</returns>
        private NNLBinding CollectBinding(NNLBinding node)
        {
            NNLBinding iFound;
            foreach (var i in fNS)
            {
                // we need to check the entire namespace tree to see if there was already a binding defined for the current type.
                if (i.Bindings.TryGetValue(node.Operator, out iFound))
                {
                    if (iFound.Name.ToLower() != node.Name.ToLower())
                    {
                        LogPosError(
                            string.Format(
                                "There is already a binding defined in the scope for the '{0}' operator", 
                                node.Operator)); // check if the binding isn't defined already in the current scope.
                    }
                    else
                    {
                        fNS.Push(node); // a binding is also a namespace
                        return iFound;
                    }
                }
            }

            // when we get here, the binding was not yet defined, so collect it in the namespace.
            var iNS = fNS.Peek();
            AddToNS(iNS, node, node.Name);

                // also needs to be added to the children so that the binding can be reached by it's name.
            iNS.Bindings[node.Operator] = node; // add the binding to the top nodes
            fNS.Push(node); // a binding is also a namespace
            return node;
        }

        /// <summary>Collects the <paramref name="node"/> and adds it to the current
        ///     namespace. If the <paramref name="name"/> is already used, an error is
        ///     generated. Does not make the item a new ns.</summary>
        /// <param name="name">The name.</param>
        /// <param name="node">The node.</param>
        private void CollectLeaf(string name, NNLStatementNode node)
        {
            node.Name = name;
            var iNS = fNS.Peek();
            AddToNS(iNS, node, name);
            ClearAttribs();
        }

        /// <summary>
        ///     checks if ther are any attribs defined, if so, an error is logged.
        /// </summary>
        private void ClearAttribs()
        {
            if (fAttributes.Count > 0)
            {
                LogPosError("No attributes allowed here", false);
                fAttributes.Clear();
            }
        }

        /// <summary>The add to ns.</summary>
        /// <param name="iNS">The i ns.</param>
        /// <param name="node">The node.</param>
        /// <param name="name">The name.</param>
        private void AddToNS(NNLNode iNS, NNLStatementNode node, string name)
        {
            if (iNS.Children.ContainsKey(name))
            {
                LogPosError(name + " already used in the current scope", false);
                iNS.Children[name] = node;
            }
            else
            {
                iNS.Children.Add(name, node);
                if (iNS.IsDefinedInParent(name))
                {
                    LogPosWarning(string.Format("{0} hides an object in the parent namespace", name));
                }

                node.Parent = iNS;
            }
        }

        /// <summary>Tries to find a class at the specified path. If none is found, the
        ///     default <paramref name="fallback"/> class is returned. For each path
        ///     name that is not the last, and which can not be found, another class
        ///     is also created.</summary>
        /// <remarks>The first name in the path can either be found at the root or at the
        ///     current <see langword="namespace"/> position (if it is not the root
        ///     already).</remarks>
        /// <param name="names"></param>
        /// <param name="fallback">The fallback.</param>
        /// <returns>The <see cref="NNLClassNode"/>.</returns>
        private NNLClassNode GetClass(System.Collections.Generic.List<string> names, NNLClassNode fallback)
        {
            NNLNode iToSearch;
            NNLStatementNode iFound = null;
            NNLNode iRes = null;
            if (fNS.Count > 1)
            {
                iToSearch = fNS.Peek();
            }
            else
            {
                iToSearch = fRoot;
            }

            var iName = 0;

            if (iToSearch.Children.TryGetValue(names[iName], out iFound) == false && iToSearch != fRoot)
            {
                iToSearch = fRoot;
                iToSearch.Children.TryGetValue(names[iName], out iFound);
            }

            if (iFound != null && iFound is NNLClassNode)
            {
                iRes = iFound as NNLNode;
                iName++;
                while (iName < names.Count && iRes != null)
                {
                    if (iRes.Children.TryGetValue(names[iName], out iFound))
                    {
                        iRes = iFound as NNLNode;
                        iName++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (iRes == null)
            {
                iRes = fNS.Peek();
            }

            while (iName < names.Count - 1)
            {
                var iNew = new NNLClassNode();
                iNew.Name = names[iName++];
                if (iRes.Children.ContainsKey(iNew.Name) == false)
                {
                    // if the name is already defined, we are in troubles.
                    iRes.Children.Add(iNew.Name, iNew);
                }
                else
                {
                    LogPosError(
                        string.Format(
                            "{0} is already used, can't create class with specified name in path {1} ", 
                            iNew.Name, 
                            string.Join(", ", names.ToArray())));
                }

                iNew.Parent = iRes;
                iRes = iNew;
            }

            if (iName == names.Count - 1)
            {
                // it's still a new class
                fallback.Name = names[iName];
                iRes.Children.Add(fallback.Name, fallback);
                fallback.Parent = iRes;
                iRes = fallback;
            }

            fNS.Push(iRes);
            return (NNLClassNode)iRes;
        }

        /// <summary>The collect in class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="node">The node.</param>
        private void CollectInClass(string name, NNLStatementNode node)
        {
            node.Name = name;
            NNLNode iNS = null;
            foreach (var i in fNS)
            {
                // walk over the stack, get the first class node. we do it like this and not through the 'parent' list, cause this can sometimes not yet be fully build (like for function content)
                if (i is NNLClassNode)
                {
                    iNS = i;
                    break;
                }
            }

            if (iNS != null)
            {
                if (iNS.Children.ContainsKey(name))
                {
                    LogPosError(name + " already used in the current scope", false);
                    iNS.Children[name] = node;
                }
                else
                {
                    iNS.Children.Add(name, node);
                    if (iNS.IsDefinedInParent(name))
                    {
                        LogPosWarning("{0} hides an object in the parent namespace");
                    }
                }
            }
        }

        /// <summary>The collect code.</summary>
        /// <param name="node">The node.</param>
        /// <param name="id">The id.</param>
        private void CollectCode(NNLStatementNode node, string id)
        {
            if (node != null)
            {
                // could be null due to bad reading (vardecl or soemthing)
                var iList = fCode.Peek();
                if (iList != null)
                {
                    iList.Items.Add(node);
                }
                else
                {
                    LogPosError("Code statements only allowed inside a function body.", false);
                }

                if (id != null)
                {
                    CollectInClass(id, node);
                }
            }
        }

        /// <summary>The collect code and start.</summary>
        /// <param name="node">The node.</param>
        /// <param name="id">The id.</param>
        private void CollectCodeAndStart(NNLStatementNode node, string id)
        {
            CollectCode(node, id);
            SetNodeStart(node);
        }

        /// <summary>The f object sync points.</summary>
        private static readonly Token[] fObjectSyncPoints =
            {
                Token.End, Token.LoopStart, Token.LoopEnd, Token.Assign, 
                Token.LengthSpec
            }; // sync points useful for objects.

        /// <summary>The f cur sync points.</summary>
        private static Token[] fCurSyncPoints = fObjectSyncPoints;
    }
}