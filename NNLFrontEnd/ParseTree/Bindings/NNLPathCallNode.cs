// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLPathCallNode.cs" company="">
//   
// </copyright>
// <summary>
//   for function calls in asset/thes paths.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     for function calls in asset/thes paths.
    /// </summary>
    internal class NNLPathCallNode : NNLBindingPathItem
    {
        /// <summary>The f param values.</summary>
        private NNLStatementNode fParamValues;

        #region ParamValues

        /// <summary>
        ///     Gets the possible parameter values. This can be a listnode, which
        ///     represents a comma seperated list.
        /// </summary>
        public NNLStatementNode ParamValues
        {
            get
            {
                return fParamValues;
            }

            set
            {
                fParamValues = value;
                if (fParamValues != null)
                {
                    fParamValues.Parent = this;
                }
            }
        }

        #endregion

        #region ToCall

        /// <summary>
        ///     Gets the possible parameter values. This can be a listnode, which
        ///     represents a comma seperated list.
        /// </summary>
        public string ToCall { get; set; }

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
            from.BindingItems.TryGetValue(Token.LengthSpec, out iRes);
            if (iRes == null)
            {
                throw new System.InvalidOperationException(": not allowed in this position");
            }

            return iRes;
        }

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The dest to render to.</param>
        /// <param name="bindItem">The bind Item.</param>
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
                var iBindItem = (NNLBindItemFunctions)bindItem;
                var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                var iNames = new System.Collections.Generic.List<System.Text.StringBuilder>();
                iNames.Add(new System.Text.StringBuilder(ToCall));
                if (prev != null)
                {
                    ProcessNames(iNames, prevType);
                    iArgs.Add(prev);
                }

                if (ParamValues is NNLNodesList && !(ParamValues is NNLPathNode))
                {
                    // a single pathNode references another var.
                    foreach (var i in ((NNLNodesList)ParamValues).Items)
                    {
                        i.Render(renderTo);
                        iArgs.Add(i);
                        ProcessNames(iNames, i.GetTypeDecl(renderTo));
                    }
                }
                else if (ParamValues != null)
                {
                    ParamValues.Render(renderTo);
                    iArgs.Add(ParamValues);
                    ProcessNames(iNames, ParamValues.GetTypeDecl(renderTo));
                }

                NNLFunctionNode iFunction = null;
                iNames.Add(new System.Text.StringBuilder(ToCall));

                    // in the very first versions, there was no param info in the name, still have to account for those.
                foreach (var iName in iNames)
                {
                    if (iBindItem.Functions.TryGetValue(iName.ToString(), out iFunction))
                    {
                        break;
                    }
                }

                if (iFunction == null)
                {
                    LogPosError(string.Format("No function specified in binding for :{0}", ToCall), renderTo);
                    Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                        // assign the item so taht we only try to compile 1 time.
                }
                else
                {
                    Item = iFunction.RenderCall(renderTo, iArgs);
                }
            }
        }

        /// <summary>The process names.</summary>
        /// <param name="names">The names.</param>
        /// <param name="value">The value.</param>
        private void ProcessNames(System.Collections.Generic.List<System.Text.StringBuilder> names, DeclType value)
        {
            System.Collections.Generic.List<System.Text.StringBuilder> iSecond = null;
            if (value != DeclType.Var)
            {
                // the function name can always be a var, which excepts any type.
                iSecond = new System.Collections.Generic.List<System.Text.StringBuilder>();
                foreach (var iName in names)
                {
                    var iNew = new System.Text.StringBuilder(iName.ToString());
                    if (iNew.Length > 0)
                    {
                        iNew.Append("-");
                    }

                    iNew.Append(DeclType.Var);
                    iSecond.Add(iNew);
                }
            }

            foreach (var iName in names)
            {
                if (iName.Length > 0)
                {
                    iName.Append("-");
                }

                iName.Append(value);
            }

            if (iSecond != null)
            {
                names.AddRange(iSecond);
            }
        }

        /// <summary>Gets the type decl.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="bindItem">The bind Item.</param>
        /// <param name="prev">The prev.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo, NNLBindItemBase bindItem, DeclType prev)
        {
            var iBindItem = (NNLBindItemFunctions)bindItem;

            // List<NNLStatementNode> iArgs = new List<NNLStatementNode>();
            var iNames = new System.Collections.Generic.List<System.Text.StringBuilder>();
            iNames.Add(new System.Text.StringBuilder(ToCall));
            if (prev != DeclType.none)
            {
                ProcessNames(iNames, prev);
            }

            if (ParamValues is NNLNodesList && !(ParamValues is NNLPathNode))
            {
                // a single pathNode references another var.
                foreach (var i in ((NNLNodesList)ParamValues).Items)
                {
                    // i.Render(renderTo);
                    // iArgs.Add(i);
                    ProcessNames(iNames, i.GetTypeDecl(renderTo));
                }
            }
            else if (ParamValues != null)
            {
                // ParamValues.Render(renderTo);
                // iArgs.Add(ParamValues);
                ProcessNames(iNames, ParamValues.GetTypeDecl(renderTo));
            }

            NNLFunctionNode iFunction = null;
            iNames.Add(new System.Text.StringBuilder(ToCall));

                // in the very first versions, there was no param info in the name, still have to account for those.
            foreach (var iName in iNames)
            {
                if (iBindItem.Functions.TryGetValue(iName.ToString(), out iFunction))
                {
                    break;
                }
            }

            if (iFunction != null)
            {
                if (iFunction != null && iFunction.ReturnValues != null && iFunction.ReturnValues.Count > 0)
                {
                    return iFunction.ReturnValues[0].GetTypeDecl(renderTo);
                }
            }

            return DeclType.none;
        }
    }
}