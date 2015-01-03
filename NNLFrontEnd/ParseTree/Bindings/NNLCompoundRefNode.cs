// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLCompoundRefNode.cs" company="">
//   
// </copyright>
// <summary>
//   for asset/thes/.. paths: these can contain 'compound words' in brackets.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     for asset/thes/.. paths: these can contain 'compound words' in brackets.
    /// </summary>
    internal class NNLCompoundRefNode : NNLBindingPathItem
    {
        /// <summary>The f compound.</summary>
        private System.Collections.Generic.List<string> fCompound;

        #region Compound

        /// <summary>
        ///     Gets the children.
        /// </summary>
        public System.Collections.Generic.List<string> Compound
        {
            get
            {
                if (fCompound == null)
                {
                    fCompound = new System.Collections.Generic.List<string>();
                }

                return fCompound;
            }
        }

        #endregion

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
            from.BindingItems.TryGetValue(Token.Dot, out iRes);
            return iRes;
        }

        /// <summary>renders the item as a parameter value for a setter function. This
        ///     fails by default. Also used to build the item from the content for the<see cref="RenderGet"/> function.</summary>
        /// <param name="renderTo"></param>
        internal override void RenderParam(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                if (Compound.Count == 1)
                {
                    Item = BrainHelper.GetNeuronFor(Compound[0]); // no need to build a compound if it is a single word.
                }
                else
                {
                    Item = BrainHelper.GetCompoundWord(Compound);

                        // temporarely set the item to the text value so we can pass it along for building the calls to pass along the values.
                }
            }

            renderTo.Add(Item);
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
                var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                string iName;
                if (prev == null)
                {
                    iName = string.Format("{0}", DeclType.Var);
                }
                else
                {
                    iArgs.Add(prev);
                    iName = string.Format("{0}-{1}", prevType, DeclType.Var);
                }

                if (Compound.Count == 1)
                {
                    Item = BrainHelper.GetNeuronFor(Compound[0]); // no need to build a compound if it is a single word.
                }
                else
                {
                    Item = BrainHelper.GetCompoundWord(Compound);

                        // temporarely set the item to the text value so we can pass it along for building the calls to pass along the values.
                }

                iArgs.Add(this);
                var iFrom = bindItem as NNLBindItem;
                NNLFunctionNode iFunction;
                if (iFrom.Getter.TryGetValue(iName, out iFunction) == false)
                {
                    LogPosError(string.Format("No get function specified in binding for .{0}", Name), renderTo);
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
            // first check if the bindItem can't be converted to a static.
            var iFrom = bindItem as NNLBindItem;
            string iName;
            if (prev == DeclType.none)
            {
                iName = string.Format("{0}", DeclType.Var);
            }
            else
            {
                iName = string.Format("{0}-{1}", prev, DeclType.Var);
            }

            NNLFunctionNode iFunction;
            if (iFrom.Getter.TryGetValue(iName, out iFunction) == false)
            {
                LogPosError(string.Format("No 'get-function' specified in binding for .{0}", Name), renderTo);
                return DeclType.Var;
            }

            if (iFunction.ReturnValues != null && iFunction.ReturnValues.Count > 0)
            {
                return iFunction.ReturnValues[0].GetTypeDecl(renderTo);
            }

            return DeclType.none;
        }
    }
}