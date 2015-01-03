// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindItemIndex.cs" company="">
//   
// </copyright>
// <summary>
//   binding item for index path operators.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     binding item for index path operators.
    /// </summary>
    internal class NNLBindItemIndex : NNLBindItemBase
    {
        /// <summary>The f getter.</summary>
        private System.Collections.Generic.Dictionary<string, NNLFunctionNode> fGetter;

        /// <summary>The f operator overloads.</summary>
        private System.Collections.Generic.Dictionary<string, NNLFunctionNode> fOperatorOverloads;

        /// <summary>The f setter.</summary>
        private System.Collections.Generic.Dictionary<string, NNLFunctionNode> fSetter;

        #region Getter

        /// <summary>
        ///     Gets/sets the getter function
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLFunctionNode> Getter
        {
            get
            {
                if (fGetter == null)
                {
                    fGetter = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                }

                return fGetter;
            }
        }

        #endregion

        #region Setter

        /// <summary>
        ///     Gets/sets the setter function
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLFunctionNode> Setter
        {
            get
            {
                if (fSetter == null)
                {
                    fSetter = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                }

                return fSetter;
            }
        }

        #endregion

        #region OperatorOverloads

        /// <summary>
        ///     Gets the list of <see langword="operator" /> overloads defined for this
        ///     binding item (other than the setter and getter).
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLFunctionNode> OperatorOverloads
        {
            get
            {
                if (fOperatorOverloads == null)
                {
                    fOperatorOverloads = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                }

                return fOperatorOverloads;
            }
        }

        #endregion

        /// <summary>The add getter.</summary>
        /// <param name="callback">The callback.</param>
        /// <param name="parser">The parser.</param>
        internal void AddGetter(NNLFunctionNode callback, NNLParser parser)
        {
            if (Operator != Token.Word)
            {
                // when 'word -> we are for a static word
                if (callback.Params.Count > 2 || callback.Params.Count < 1)
                {
                    parser.LogPosError("Invalid nr of arguments specified (1 or 2 expected)");

                        // 1 arg: handle first item in path, 2 arg: handle next items or index ref. more args allowed for regular functions.
                }
            }
            else if (callback.Params.Count > 1)
            {
                parser.LogPosError("Invalid nr of arguments specified (0 or 1 expected)");

                    // 0 arg: handle first item in path, 1 arg: handle next items or index ref. more args allowed for regular functions.
            }

            if (callback.ReturnValues.Count != 1)
            {
                parser.LogPosError("a getter needs a return value");
            }

            var iName = new System.Text.StringBuilder();

                // build the name of the callback based on the argument types, this allows us to quickly find the correct binding based on the arguments.
            foreach (var i in callback.Params)
            {
                if (iName.Length > 0)
                {
                    iName.Append("-");
                }

                iName.Append(i.TypeDecl);
            }

            var iNameStr = iName.ToString();
            if (Getter.ContainsKey(iNameStr) == false)
            {
                Getter.Add(iNameStr, callback);
            }
            else
            {
                iName = new System.Text.StringBuilder("(");
                foreach (var i in callback.Params)
                {
                    if (iName.Length > 0)
                    {
                        iName.Append(", ");
                    }

                    iName.Append(i.TypeDecl);
                }

                iName.AppendFormat("):{0}", callback.ReturnValues[0].TypeDecl);
                parser.LogPosError(string.Format("a function of the form {0} is already defined in this getter", iName));
            }
        }

        /// <summary>The add setter.</summary>
        /// <param name="callback">The callback.</param>
        /// <param name="parser">The parser.</param>
        internal void AddSetter(NNLFunctionNode callback, NNLParser parser)
        {
            if (Operator != Token.Word)
            {
                // when 'word -> we are for a static word
                if (callback.Params.Count > 3 || callback.Params.Count < 2)
                {
                    parser.LogPosError("Invalid nr of arguments specified (2 or 3 expected)");

                        // 2: setter for first item in path, 3: setter for next items in the path.
                }
            }
            else if (callback.Params.Count > 2 || callback.Params.Count < 1)
            {
                parser.LogPosError("Invalid nr of arguments specified (1 or 2 expected)");

                    // 0 arg: handle first item in path, 1 arg: handle next items or index ref. more args allowed for regular functions.
            }

            if (callback.ReturnValues != null && callback.ReturnValues.Count != 0)
            {
                parser.LogPosError("a setter can't have a return value");
            }

            var iName = new System.Text.StringBuilder(Tokenizer.GetSymbolForToken(Token.Assign));

                // build the name of the callback based on the argument types, this allows us to quickly find the correct binding based on the arguments. We start with the assign cause that provides us a more generic way with operator overloads for building the name (and searching on this name0
            foreach (var i in callback.Params)
            {
                if (iName.Length > 0)
                {
                    iName.Append("-");
                }

                iName.Append(i.TypeDecl);
            }

            var iNameStr = iName.ToString();
            if (Setter.ContainsKey(iNameStr) == false)
            {
                Setter.Add(iNameStr, callback);
            }
            else
            {
                iName = new System.Text.StringBuilder("(");
                foreach (var i in callback.Params)
                {
                    if (iName.Length > 0)
                    {
                        iName.Append(", ");
                    }

                    iName.Append(i.TypeDecl);
                }

                iName.Append(")");
                parser.LogPosError(string.Format("a function of the form {0} is already defined in this setter", iName));
            }
        }

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            base.Render(renderTo);
            if (fOperatorOverloads != null)
            {
                foreach (var i in fOperatorOverloads)
                {
                    i.Value.Render(renderTo);
                }
            }

            if (fGetter != null)
            {
                foreach (var i in fGetter)
                {
                    i.Value.Render(renderTo);
                }
            }

            if (fSetter != null)
            {
                foreach (var i in fSetter)
                {
                    i.Value.Render(renderTo);
                }
            }
        }

        /// <summary>write the data to a <paramref name="stream"/> so that the binding can
        ///     be loaded without a full recompile.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            WriteList(stream, fGetter);
            WriteList(stream, fSetter);
            WriteList(stream, fOperatorOverloads);
        }

        /// <summary>Reads the content from the specified binary reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            fGetter = ReadList(reader);
            fSetter = ReadList(reader);
            fOperatorOverloads = ReadList(reader);
        }
    }
}