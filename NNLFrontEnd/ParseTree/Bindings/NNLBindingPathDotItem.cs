// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathDotItem.cs" company="">
//   
// </copyright>
// <summary>
//   represents a regular .x item in a binding-path.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     represents a regular .x item in a binding-path.
    /// </summary>
    internal class NNLBindingPathDotItem : NNLBindingPathItem
    {
        /// <summary>should be implemented by descendents and return the bindingItem that
        ///     should be used that matches the <see langword="operator"/> for this
        ///     path Item and which is a child of the specified binding item.</summary>
        /// <param name="from">The parent binding item that defines which are the possible next
        ///     binding items. (this is a state machine that determins how a path can
        ///     be constructed)</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        internal override NNLBindItemBase GetNextCallBackItemFrom(NNLBindItemBase from)
        {
            if (from != null)
            {
                NNLBindItemBase iRes;
                from.BindingItems.TryGetValue(Token.Dot, out iRes);
                return iRes;
            }

            return null;
        }

        /// <summary>renders the item as a parameter value for a setter function. This
        ///     fails by default. Also used to build the item from the content for the<see cref="RenderGet"/> function.</summary>
        /// <param name="renderTo"></param>
        internal override void RenderParam(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                Item = TextNeuron.GetFor(Name);

                    // temporarely set the item to the text value so we can pass it along for building the calls to pass along the values.
            }

            renderTo.Add(Item);
        }

        /// <summary>The try render static.</summary>
        /// <param name="renderTo">The render to.</param>
        protected virtual void TryRenderStatic(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                Item = ParserBase.ConvertStringToPos(Name); // first try as a pos value, important for the thes paths.
                if (Item == null)
                {
                    ulong iId;
                    if (ParserBase.Statics.TryGetValue(Name, out iId))
                    {
                        Item = Brain.Current[iId];
                    }
                    else
                    {
                        Item = TextNeuron.GetFor(Name);
                    }
                }
            }
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
                var iFrom = bindItem as NNLBindItemIndex;
                if (iFrom != null)
                {
                    var iFromBindItem = iFrom as NNLBindItem;
                    NNLBindItem iStatic;
                    var iIsStatic = false;
                    if (iFromBindItem != null && string.IsNullOrEmpty(Name) == false
                        && iFromBindItem.Statics.TryGetValue(Name, out iStatic))
                    {
                        // first check if the bindItem can't be converted to a static.
                        iFrom = iStatic;
                        iIsStatic = true;
                    }

                    if (iIsStatic == false)
                    {
                        // don't need to render the param when it's static.
                        if (bindItem.RootBinding.UseStatics == false)
                        {
                            RenderParam(renderTo);

                                // temporarely set the item to the text value so we can pass it along for building the calls to pass along the values.
                        }
                        else
                        {
                            TryRenderStatic(renderTo);
                        }
                    }

                    var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                    string iName;
                    if (prev == null)
                    {
                        if (iIsStatic)
                        {
                            iName = string.Empty;
                        }
                        else
                        {
                            iName = string.Format("{0}", DeclType.Var);
                            iArgs.Add(this);
                        }
                    }
                    else
                    {
                        if (iIsStatic)
                        {
                            iArgs.Add(prev);
                            iName = string.Format("{0}", prevType);
                        }
                        else
                        {
                            iArgs.Add(prev);
                            iArgs.Add(this);
                            iName = string.Format("{0}-{1}", prevType, DeclType.Var);
                        }
                    }

                    var iFunction = iFrom.Getter[iName];
                    if (iFunction == null)
                    {
                        LogPosError(string.Format("No get function specified in binding for .{0}", iName), renderTo);
                        Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                            // assign the item so taht we only try to compile 1 time.
                    }
                    else
                    {
                        Item = iFunction.RenderCall(renderTo, iArgs);
                    }
                }
            }
            else
            {
                LogPosError("trying to use a Get function from invalid bind section", renderTo);
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
            if (iFrom != null)
            {
                var iFromBindItem = iFrom;
                NNLBindItem iStatic;
                var iIsStatic = false;
                if (iFromBindItem != null && string.IsNullOrEmpty(Name) == false
                    && iFromBindItem.Statics.TryGetValue(Name, out iStatic))
                {
                    iFrom = iStatic;
                    iIsStatic = true;
                }

                string iName;
                if (prev == DeclType.none)
                {
                    if (iIsStatic)
                    {
                        iName = string.Empty;
                    }
                    else
                    {
                        iName = string.Format("{0}", DeclType.Var);
                    }
                }
                else
                {
                    if (iIsStatic)
                    {
                        iName = string.Format("{0}", prev);
                    }
                    else
                    {
                        iName = string.Format("{0}-{1}", prev, DeclType.Var);
                    }
                }

                var iFunction = iFrom.Getter[iName];
                if (iFunction == null)
                {
                    LogPosError(string.Format("No get function specified in binding for .{0}", Name), renderTo);
                    return DeclType.Var;
                }

                if (iFunction.ReturnValues != null && iFunction.ReturnValues.Count > 0)
                {
                    return iFunction.ReturnValues[0].GetTypeDecl(renderTo);
                }

                return DeclType.none;
            }

            LogPosError("trying to use a Get function from invalid bind section", renderTo);
            return DeclType.none;
        }
    }
}