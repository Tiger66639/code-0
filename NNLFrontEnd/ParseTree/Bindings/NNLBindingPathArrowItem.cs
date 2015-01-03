// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathArrowItem.cs" company="">
//   
// </copyright>
// <summary>
//   stores the data for a binding Path (asset/thes/var/topic path) that
//   starts with a -&gt; or left arrow. This way, we know what to render.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     stores the data for a binding Path (asset/thes/var/topic path) that
    ///     starts with a -> or left arrow. This way, we know what to render.
    /// </summary>
    internal class NNLBindingPathArrowItem : NNLBindingPathItem
    {
        /// <summary>Initializes a new instance of the <see cref="NNLBindingPathArrowItem"/> class. Initializes a new instance of the<see cref="NNLBindingPathArrowItem"/> class.</summary>
        /// <param name="direction">The direction.</param>
        public NNLBindingPathArrowItem(Token direction)
        {
            Direction = direction;
        }

        #region Direction

        /// <summary>
        ///     Gets the direction of the arrow (should be either ArrowLeft or
        ///     ArrowRight)
        /// </summary>
        public Token Direction { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets or sets the value to which it points to.
        /// </summary>
        /// <value>
        ///     The points to.
        /// </value>
        public NNLStatementNode PointsTo { get; set; }

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
            from.BindingItems.TryGetValue(Direction, out iRes);
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
                PointsTo.Render(renderTo);
                var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                string iName;
                if (prev == null)
                {
                    iName = DeclType.Var.ToString();
                }
                else
                {
                    iArgs.Add(prev);
                    iName = string.Format("{0}-{1}", prevType, PointsTo.GetTypeDecl(renderTo));
                }

                iArgs.Add(PointsTo);
                var iFunction = ((NNLBindItem)bindItem).Getter[iName];
                if (iFunction == null)
                {
                    LogPosError(
                        string.Format(
                            "No get function specified in binding for {0}{1}", 
                            Tokenizer.GetSymbolForToken(Direction), 
                            PointsTo.Name), 
                        renderTo);
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
            string iName;
            if (prev == DeclType.none)
            {
                iName = DeclType.Var.ToString();
            }
            else
            {
                iName = string.Format("{0}-{1}", prev, PointsTo.GetTypeDecl(renderTo));
            }

            var iFunction = ((NNLBindItem)bindItem).Getter[iName];
            if (iFunction == null)
            {
                LogPosError(
                    string.Format(
                        "No get function specified in binding for {0}{1}", 
                        Tokenizer.GetSymbolForToken(Direction), 
                        PointsTo.Name), 
                    renderTo);
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