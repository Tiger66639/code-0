// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLPathIndexNode.cs" company="">
//   
// </copyright>
// <summary>
//   used in a path (ns path or thes/asset path) to indicate that an index []
//   is specified.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     used in a path (ns path or thes/asset path) to indicate that an index []
    ///     is specified.
    /// </summary>
    internal class NNLPathIndexNode : NNLBindingPathItem
    {
        /// <summary>The f index value.</summary>
        private NNLStatementNode fIndexValue;

        /// <summary>
        ///     Gets or sets the index value.
        /// </summary>
        /// <value>
        ///     The index value.
        /// </value>
        public NNLStatementNode IndexValue
        {
            get
            {
                return fIndexValue;
            }

            set
            {
                fIndexValue = value;
                if (fIndexValue != null && fIndexValue.Parent == null)
                {
                    fIndexValue.Parent = this;
                }
            }
        }

        /// <summary>should be implemented by descendents and return the bindingItem that
        ///     should be used that matches the <see langword="operator"/> for this
        ///     path Item and which is a child of the specified binding item.</summary>
        /// <param name="from">The parent binding item that defines which are the possible next
        ///     binding items. (this is a state machine that determins how a path can
        ///     be constructed)</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        internal override NNLBindItemBase GetNextCallBackItemFrom(NNLBindItemBase from)
        {
            NNLBindItemBase iRes;
            from.BindingItems.TryGetValue(Token.OptionStart, out iRes);
            return iRes;
        }

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The dest to render to.</param>
        /// <param name="bindItem">The binding item that defines all the functions that can be called at
        ///     the current stage.</param>
        /// <param name="prev">The prev item, if any (so the return type can be checked)</param>
        /// <param name="prevType">The prev Type.</param>
        internal override void RenderGet(
            NNLModuleCompiler renderTo, 
            NNLBindItemBase bindItem, 
            NNLBindingPathItem prev, 
            DeclType prevType)
        {
            if (Item == null)
            {
                var iBindItem = (NNLBindItemIndex)bindItem;
                var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                string iName;
                iArgs.Add(prev);
                iName = string.Format("{0}-{1}", prevType, IndexValue.GetTypeDecl(renderTo));

                iArgs.Add(IndexValue);
                NNLFunctionNode iFunction;
                if (iBindItem.Getter.TryGetValue(iName, out iFunction) == false)
                {
                    LogPosError(string.Format("No GetAt function specified in binding"), renderTo);
                    Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                        // assign the item so taht we only try to compile 1 time.
                }
                else
                {
                    Item = iFunction.RenderCall(renderTo, iArgs);
                }
            }
        }

        /// <summary>Gets the type decl.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="bindItem">The bind Item.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo, NNLBindItemBase bindItem, DeclType prev)
        {
            var iBindItem = (NNLBindItemIndex)bindItem;
            var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
            string iName;
            iName = string.Format("{0}-{1}", prev, IndexValue.GetTypeDecl(renderTo));

            NNLFunctionNode iFunction;
            if (iBindItem.Getter.TryGetValue(iName, out iFunction) && iFunction.ReturnValues.Count > 0)
            {
                return iFunction.ReturnValues[0].GetTypeDecl(renderTo);
            }

            return DeclType.none;
        }
    }
}