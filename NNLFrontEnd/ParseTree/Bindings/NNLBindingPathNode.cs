// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindingPathNode.cs" company="">
//   
// </copyright>
// <summary>
//   specifies the varaible/thes/topic/asset path that was declared. it
//   contains all the steps that the user specified and which need to be
//   rendered with the binding defined for the <see langword="operator" />
//   stored in the unary statement that contains this path as it's child.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     specifies the varaible/thes/topic/asset path that was declared. it
    ///     contains all the steps that the user specified and which need to be
    ///     rendered with the binding defined for the <see langword="operator" />
    ///     stored in the unary statement that contains this path as it's child.
    /// </summary>
    internal class NNLBindingPathNode : NNLNodesList
    {
        /// <summary>The fbinding.</summary>
        private NNLBinding fbinding;

        /// <summary>The f first.</summary>
        private NNLBindingPathItem fFirst;

        /// <summary>Initializes a new instance of the <see cref="NNLBindingPathNode"/> class.</summary>
        public NNLBindingPathNode()
            : base(NodeType.Path)
        {
        }

        // the first value in the path.
        /// <summary>Gets or sets the first.</summary>
        public NNLBindingPathItem First
        {
            get
            {
                return fFirst;
            }

            set
            {
                fFirst = value;
                if (fFirst.Parent == null)
                {
                    fFirst.Parent = this;
                }
            }
        }

        /// <summary>
        ///     Gets the list of children of this node. We <see langword="override" />
        ///     this prop so we can return the children of the binding. This allows us
        ///     to write things like #functionName where 'functionname is defined as a
        ///     childfunction of the binding. For this type of function-call, a
        ///     <see cref="NNLBindingPatRefItem" /> is generated that points to a
        ///     path, referencing the function.
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, NNLStatementNode> Children
        {
            get
            {
                if (fbinding != null)
                {
                    return fbinding.Children;
                }

                return base.Children;
            }
        }

        /// <summary>calculates the typedecl for a getter, based on the specified binding.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <param name="binding">The binding.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        internal DeclType GetTypeDecl(NNLModuleCompiler renderTo, NNLBinding binding)
        {
            if (binding != null)
            {
                fbinding = binding;
                NNLBindItemBase iGetCallbackFrom = binding.Root;
                var iPrev = First.GetTypeDecl(renderTo, iGetCallbackFrom, DeclType.none);
                var iPrevItem = First;
                foreach (NNLBindingPathItem i in Items)
                {
                    var iFromBindItem = iGetCallbackFrom as NNLBindItem;
                    NNLBindItem iStatic;
                    if (iFromBindItem != null && string.IsNullOrEmpty(iPrevItem.Name) == false
                        && iFromBindItem.Statics.TryGetValue(iPrevItem.Name, out iStatic))
                    {
                        iGetCallbackFrom = iStatic;
                    }

                    iGetCallbackFrom = i.GetNextCallBackItemFrom(iGetCallbackFrom);
                    if (iGetCallbackFrom != null)
                    {
                        iPrev = i.GetTypeDecl(renderTo, iGetCallbackFrom, iPrev);
                    }
                    else
                    {
                        i.LogPosError("Invalid operator in path", renderTo);
                        return DeclType.none;
                    }

                    iPrevItem = i;
                }

                return iPrev;
            }

            return DeclType.none;
        }

        /// <summary>renders this path as a get and also the rightpart as a get. If at the
        ///     end of this path, there is an <see langword="operator"/> overload for
        ///     the specified operator, then this is generated and returned.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="RightPart">The right part.</param>
        /// <param name="op">The op.</param>
        /// <param name="returnType">return type of the <see langword="operator"/> overloader (for<see langword="bool"/> expressions, this needs to be bool), can be
        ///     null.</param>
        /// <returns><see langword="null"/> if no operator, otherwise the result of the
        ///     function call</returns>
        internal Neuron RenderGetAndOperatorOverload(
            NNLModuleCompiler renderTo, 
            NNLBinding binding, 
            NNLStatementNode RightPart, 
            Token op, 
            DeclType? returnType = null)
        {
            RightPart.Render(renderTo);
            if (RightPart.Item == null)
            {
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // we assign the item so that it doesnt get regenerated each time and renders the errors multiple times.
                LogPosError("Invalid right side of bool expression", renderTo);
            }
            else
            {
                var iLastInPath = InternalRenderGet(renderTo, binding);
                if (iLastInPath != null)
                {
                    var iNames = BuildNames(renderTo, RightPart, op, returnType);
                    var iFunction = GetFunction(renderTo, iNames, iLastInPath, op, false);
                    if (iFunction != null)
                    {
                        var iArgs = new System.Collections.Generic.List<NNLStatementNode> { this, RightPart };

                            // required for rendering the function call
                        return iFunction.RenderCall(renderTo, iArgs, renderTo.RenderingTo.Peek());
                    }
                }
            }

            return null;
        }

        /// <summary>Renders the get.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="binding">The binding.</param>
        internal void RenderGet(NNLModuleCompiler renderTo, NNLBinding binding)
        {
            if (Item == null)
            {
                InternalRenderGet(renderTo, binding);
            }
        }

        /// <summary>The internal render get.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="binding">The binding.</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        private NNLBindItemBase InternalRenderGet(NNLModuleCompiler renderTo, NNLBinding binding)
        {
            var iPrevCodeRefs = renderTo.BindingPathCodeRefs;
            fbinding = binding;
            RenderedItems = new System.Collections.Generic.List<Neuron>();

                // collects neurons that were rendered by NNLBindingPathItemSubPath
            System.Collections.Generic.List<Neuron> iNewCodeRefs;
            if (renderTo.BindingPathCodeRefs == null)
            {
                // only the root defines this list, this way, all sub paths handle the passing of these variables correclty.
                iNewCodeRefs = new System.Collections.Generic.List<Neuron>();
                renderTo.BindingPathCodeRefs = iNewCodeRefs;
            }
            else
            {
                iNewCodeRefs = iPrevCodeRefs;
            }

            NNLBindItemBase iGetCallbackFrom = binding.Root;
            renderTo.RenderingTo.Push(RenderedItems);

                // so that child items can render multiple objects. When rendering a param values list, multiple items need to be rendered in the parent list.
            try
            {
                First.RenderGet(renderTo, iGetCallbackFrom, null, DeclType.none);
                Item = First.Item;
                var iPrevType = First.GetTypeDecl(renderTo, iGetCallbackFrom, DeclType.none);
                var iPrev = First;
                foreach (NNLBindingPathItem i in Items)
                {
                    var iFromBindItem = iGetCallbackFrom as NNLBindItem;
                    NNLBindItem iStatic;
                    if (iFromBindItem != null && string.IsNullOrEmpty(iPrev.Name) == false
                        && iFromBindItem.Statics.TryGetValue(iPrev.Name, out iStatic))
                    {
                        iGetCallbackFrom = iStatic;
                    }

                    iGetCallbackFrom = i.GetNextCallBackItemFrom(iGetCallbackFrom);
                    if (iGetCallbackFrom != null)
                    {
                        i.RenderGet(renderTo, iGetCallbackFrom, iPrev, iPrevType);
                        Item = i.Item;
                        iPrevType = i.GetTypeDecl(renderTo, iGetCallbackFrom, iPrevType);
                    }
                    else
                    {
                        i.LogPosError("Invalid operator in path", renderTo);
                        Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                            // give a value, so we only render the errors 1 time.
                        return null;
                    }

                    iPrev = i;
                }
            }
            finally
            {
                renderTo.RenderingTo.Pop();
                renderTo.BindingPathCodeRefs = iPrevCodeRefs;
            }

            if (First is NNLBindingPathItemSubPath && Items.Count == 0)
            {
                // if the first is a function call and nothing after it, don't need to do anything more.
                var iRenderingTo = renderTo.RenderingTo.Peek();
                iRenderingTo.AddRange(RenderedItems);
            }
            else if (RenderedItems.Count > 0)
            {
                HandleGetOverload(renderTo, iNewCodeRefs);
            }

            return iGetCallbackFrom;
        }

        /// <summary>checks if the binding has a function overload for get 'this'. If so,
        ///     the code cluster currently found in 'Item', is converted to a param
        ///     for the function that was found (which is then added to the current
        ///     code list) and a new function call is created to the overload which is
        ///     added to the code list. If there is no overloaded 'this', 'Item' is
        ///     converted into a 'call instruction' and added to the code list. In
        ///     both case, 'Item' is finaly converted to the 'return value' variable.</summary>
        /// <param name="renderTo"></param>
        /// <param name="codeRefs">The code Refs.</param>
        private void HandleGetOverload(NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> codeRefs)
        {
            var iTempArgs = new System.Collections.Generic.List<Neuron>();
            var iRenderingTo = renderTo.RenderingTo.Peek();
            if (fbinding.HasFunctions)
            {
                var iName = string.Format("this-{0}:{1}", DeclType.Var, DeclType.Var);
                var iShort = string.Format("this:{0}", DeclType.Var);

                    // in case there is only a definition with no arguments. 
                NNLStatementNode iFound;
                if (fbinding.Children.TryGetValue(iName, out iFound)
                    || fbinding.Children.TryGetValue(iShort, out iFound))
                {
                    RenderPopFor(RenderedItems, renderTo, codeRefs);
                    Item = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);

                        // make the callback, put it in item, so that 'HandleGetOverload can easily work with it.
                    iFound.Render(renderTo);

                    // even if we are using the shortname, we still need to provide the callback function as an argument, so push it.
                    RenderPushFor(iRenderingTo, renderTo, codeRefs); // do first, so they can be retrieved last.
                    iRenderingTo.Add(GetPushValue(this, renderTo));

                        // Item currently contains the code list, which becomes the value.
                    iTempArgs.Add(iFound.Item);
                    var iArgs = GetParentsFor(iTempArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);

                        // still need to render the function call
                    iRenderingTo.Add(GetStatement((ulong)PredefinedNeurons.CallInstruction, iArgs, renderTo));
                    Item = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];

                        // we have a get, so we always need to return value.
                    return; // we have created the new call, so everything is done.
                }
            }

            iRenderingTo.AddRange(RenderedItems);
        }

        /// <summary>Makes certain that all items are pushed up the function - arguments
        ///     stack.</summary>
        /// <param name="callback">The callback.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="codeRefs">The code refs.</param>
        private void RenderPopFor(System.Collections.Generic.List<Neuron> callback, 
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> codeRefs)
        {
            if (codeRefs.Count > 0)
            {
                var iArgsCl = GetParentsFor(codeRefs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                callback.Insert(0, GetStatement((ulong)PredefinedNeurons.PopValueInstruction, iArgsCl, renderTo));
            }
        }

        /// <summary>Makes certain that all items are pushed up the function - arguments
        ///     stack.</summary>
        /// <param name="oldFunction">The old Function.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="codeRefs">The code refs.</param>
        private void RenderPushFor(System.Collections.Generic.List<Neuron> oldFunction, 
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> codeRefs)
        {
            if (codeRefs.Count > 0)
            {
                var iTemp = new System.Collections.Generic.List<Neuron>(codeRefs);

                    // we make a local copy so we can reverse the items (they need to be added in the reverse order cause it's a stack.
                iTemp.Reverse();
                foreach (var i in iTemp)
                {
                    oldFunction.Add(GetPushValue(i, renderTo));
                }
            }
        }

        /// <summary>The render set.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="RightPart">The right part.</param>
        /// <param name="op">The op.</param>
        internal void RenderSet(NNLModuleCompiler renderTo, NNLBinding binding, NNLStatementNode RightPart, Token op)
        {
            if (Item == null)
            {
                var iPrevCodeRefs = renderTo.BindingPathCodeRefs;
                System.Collections.Generic.List<Neuron> iNewCodeRefs;
                if (renderTo.BindingPathCodeRefs == null)
                {
                    // only the root defines this list, this way, all sub paths handle the passing of these variables correclty.
                    iNewCodeRefs = new System.Collections.Generic.List<Neuron>();
                    renderTo.BindingPathCodeRefs = iNewCodeRefs;
                }
                else
                {
                    iNewCodeRefs = iPrevCodeRefs;
                }

                var iArgs = new System.Collections.Generic.List<NNLStatementNode>();
                var iNames = new System.Collections.Generic.List<string>();
                NNLFunctionNode iFunction;
                fbinding = binding;
                RenderedItems = new System.Collections.Generic.List<Neuron>();
                renderTo.RenderingTo.Push(RenderedItems);

                    // so that child items can render multiple objects. When rendering a param values list, multiple items need to be rendered in the parent list.
                try
                {
                    iFunction = RenderLeftPartForSet(renderTo, RightPart, op, iArgs, iNames);
                    if (iFunction != null)
                    {
                        // don't need to render anymore if the leftpart already had a problem.
                        // note: for statements in the form: #bot.mem.who.val = $var:resolvePerson; it has to be done AFTER RenderLeftPartForSet
                        RightPart.Render(renderTo);

                            // just before rendering the set function, render the right part, this is the last param, so needs to be rendered last in order to get the order of the parameters correct.
                    }
                }
                finally
                {
                    renderTo.RenderingTo.Pop();
                    renderTo.BindingPathCodeRefs = iPrevCodeRefs;
                }

                if (iFunction != null)
                {
                    if (RightPart.Item is Variable && !(RightPart.Item is ReturnValue))
                    {
                        // don't need to do this if it's a static. Return values also don't need to be passed along the border.
                        iNewCodeRefs.Add(RightPart.Item);

                            // the right part of the value also needs to be passed over the border of the threads.
                    }

                    RenderSet(renderTo, iFunction, iArgs, iNames, op, iNewCodeRefs);
                }
            }
        }

        /// <summary>The render left part for set.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="RightPart">The right part.</param>
        /// <param name="op">The op.</param>
        /// <param name="iArgs">The i args.</param>
        /// <param name="iNames">The i names.</param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        private NNLFunctionNode RenderLeftPartForSet(
            NNLModuleCompiler renderTo, 
            NNLStatementNode RightPart, 
            Token op, System.Collections.Generic.List<NNLStatementNode> iArgs, System.Collections.Generic.List<string> iNames)
        {
            NNLBindItemBase iGetCallbackFrom = fbinding.Root;
            NNLBindingPathItem iLast;
            var iPrevType = DeclType.none;
            NNLBindingPathItem iPrev = null;
            NNLFunctionNode iFunction = null;
            var iIsStatic = false;
            NNLBindItem iStatic = null;
            if (Items.Count > 0)
            {
                First.RenderGet(renderTo, iGetCallbackFrom, null, DeclType.none);
                Item = First.Item;
                iPrevType = First.GetTypeDecl(renderTo, iGetCallbackFrom, DeclType.none);
                iPrev = First;
                for (var iCount = 0; iCount < Items.Count - 1; iCount++)
                {
                    var i = (NNLBindingPathItem)Items[iCount];
                    var iFromBindItem = iGetCallbackFrom as NNLBindItem;
                    if (iFromBindItem != null && string.IsNullOrEmpty(iPrev.Name) == false
                        && iFromBindItem.Statics.TryGetValue(iPrev.Name, out iStatic))
                    {
                        iGetCallbackFrom = iStatic;
                    }

                    iGetCallbackFrom = GetNextCallback(renderTo, i, iGetCallbackFrom, iPrev, ref iPrevType);
                    if (iGetCallbackFrom == null)
                    {
                        return null;
                    }
                }

                iLast = (NNLBindingPathItem)Items[Items.Count - 1];
                iGetCallbackFrom = iLast.GetNextCallBackItemFrom(iGetCallbackFrom);
            }
            else
            {
                iLast = First;
            }

            var iCheckStaticBindItem = iGetCallbackFrom as NNLBindItem;
            if (iCheckStaticBindItem != null && string.IsNullOrEmpty(iLast.Name) == false
                && iCheckStaticBindItem.Statics.TryGetValue(iLast.Name, out iStatic))
            {
                // first check if the bindItem can't be converted to a static.
                iIsStatic = true;
                iGetCallbackFrom = iStatic;
            }

            BuildNames(renderTo, RightPart, iNames, iIsStatic, iArgs, iPrevType, iPrev, iLast, op);
            if (iIsStatic == false && Items.Count == 0)
            {
                iLast.RenderParam(renderTo);

                    // item needs to be rendered in such a way that it can be used a a parameter value.
                iFunction = GetFunction(renderTo, iNames, iGetCallbackFrom, op);
            }
            else
            {
                iFunction = GetFunction(renderTo, iNames, iGetCallbackFrom, op);
                if (iFunction == null && Items.Count > 0 && iIsStatic == false)
                {
                    // if we didn't find a function, and we tried with 3 params, try with 2 params: iLast is included in the left 'getter'.
                    iGetCallbackFrom = GetNextCallback(renderTo, iLast, iGetCallbackFrom, iPrev, ref iPrevType);
                    if (iGetCallbackFrom == null)
                    {
                        return null;
                    }

                    BuildNames(renderTo, RightPart, iNames, true, iArgs, iPrevType, iPrev, iLast, op);

                        // we force as 'isStatic', so we only use 2 params.
                    iFunction = GetFunction(renderTo, iNames, iGetCallbackFrom, op);
                }
                else if (iFunction != null)
                {
                    iLast.RenderParam(renderTo);

                        // item needs to be rendered in such a way that it can be used a a parameter value.
                }
            }

            if (iFunction == null)
            {
                LogPosError(string.Format("No set function specified in binding for {0}", op), renderTo);
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // assign the item so taht we only try to compile 1 time.
            }

            iArgs.Add(RightPart);
            return iFunction;
        }

        /// <summary>The get next callback.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="i">The i.</param>
        /// <param name="iGetCallbackFrom">The i get callback from.</param>
        /// <param name="iPrev">The i prev.</param>
        /// <param name="iPrevType">The i prev type.</param>
        /// <returns>The <see cref="NNLBindItemBase"/>.</returns>
        private NNLBindItemBase GetNextCallback(
            NNLModuleCompiler renderTo, 
            NNLBindingPathItem i, 
            NNLBindItemBase iGetCallbackFrom, 
            NNLBindingPathItem iPrev, 
            ref DeclType iPrevType)
        {
            iGetCallbackFrom = i.GetNextCallBackItemFrom(iGetCallbackFrom);
            if (iGetCallbackFrom != null)
            {
                i.RenderGet(renderTo, iGetCallbackFrom, iPrev, iPrevType);
                Item = i.Item;
                iPrevType = i.GetTypeDecl(renderTo, iGetCallbackFrom, iPrevType);
            }
            else
            {
                i.LogPosError("Invalid operator in path", renderTo);
                Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                    // give a value, so we only render the errors 1 time.
                return null;
            }

            return iGetCallbackFrom;
        }

        /// <summary>builds the names for the setter function.</summary>
        /// <param name="renderTo"></param>
        /// <param name="RightPart"></param>
        /// <param name="iNames"></param>
        /// <param name="iIsStatic"></param>
        /// <param name="iArgs"></param>
        /// <param name="iPrevType"></param>
        /// <param name="iPrev"></param>
        /// <param name="iLast"></param>
        /// <param name="op"></param>
        private void BuildNames(
            NNLModuleCompiler renderTo, 
            NNLStatementNode RightPart, System.Collections.Generic.List<string> iNames, 
            bool iIsStatic, System.Collections.Generic.List<NNLStatementNode> iArgs, 
            DeclType iPrevType, 
            NNLBindingPathItem iPrev, 
            NNLBindingPathItem iLast, 
            Token op)
        {
            iNames.Clear();
            iArgs.Clear();
            if (iIsStatic)
            {
                // it's a static
                if (Items.Count > 0)
                {
                    iNames.Add(
                        string.Format(
                            "{0}-{1}-{2}", 
                            Tokenizer.GetSymbolForToken(op), 
                            iPrevType, 
                            RightPart.GetTypeDecl(renderTo)));
                    iNames.Add(string.Format("{0}-{1}-{2}", Tokenizer.GetSymbolForToken(op), iPrevType, DeclType.Var));
                    iNames.Add(
                        string.Format("{0}-{1}-{2}", Tokenizer.GetSymbolForToken(op), DeclType.Var, DeclType.Var));
                    iArgs.Add(iPrev);
                }
                else
                {
                    iNames.Add(
                        string.Format("{0}-{1}", Tokenizer.GetSymbolForToken(op), RightPart.GetTypeDecl(renderTo)));
                    iNames.Add(string.Format("{0}-{1}", Tokenizer.GetSymbolForToken(op), DeclType.Var));
                }
            }
            else
            {
                if (Items.Count > 0)
                {
                    iNames.Add(
                        string.Format(
                            "{0}-{1}-{2}-{3}", 
                            Tokenizer.GetSymbolForToken(op), 
                            iPrevType, 
                            DeclType.Var, 
                            RightPart.GetTypeDecl(renderTo)));
                    iNames.Add(
                        string.Format(
                            "{0}-{1}-{2}-{3}", 
                            Tokenizer.GetSymbolForToken(op), 
                            iPrevType, 
                            DeclType.Var, 
                            DeclType.Var));
                    iNames.Add(
                        string.Format(
                            "{0}-{1}-{2}-{3}", 
                            Tokenizer.GetSymbolForToken(op), 
                            DeclType.Var, 
                            DeclType.Var, 
                            DeclType.Var));
                    iArgs.Add(iPrev);
                }
                else
                {
                    iNames.Add(
                        string.Format(
                            "{0}-{1}-{2}", 
                            Tokenizer.GetSymbolForToken(op), 
                            DeclType.Var, 
                            RightPart.GetTypeDecl(renderTo)));
                    iNames.Add(
                        string.Format("{0}-{1}-{2}", Tokenizer.GetSymbolForToken(op), DeclType.Var, DeclType.Var));
                }

                iArgs.Add(iLast);
            }
        }

        /// <summary>Builds the names for a get <see langword="operator"/> overload</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="RightPart">The right part.</param>
        /// <param name="op">The op.</param>
        /// <param name="returnType">The return Type.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<string> BuildNames(
            NNLModuleCompiler renderTo, 
            NNLStatementNode RightPart, 
            Token op, 
            DeclType? returnType)
        {
            var iNames = new System.Collections.Generic.List<string>();
            var iThisType = GetTypeDecl(renderTo);
            string iReturnType;
            if (returnType.HasValue)
            {
                iReturnType = string.Format(":{0}", returnType.Value);
            }
            else
            {
                iReturnType = string.Empty;
            }

            var iTypeOfRight = RightPart.GetTypeDecl(renderTo);
            iNames.Add(
                string.Format("{0}-{1}-{2}{3}", Tokenizer.GetSymbolForToken(op), iThisType, iTypeOfRight, iReturnType));
            iNames.Add(
                string.Format("{0}-{1}-{2}{3}", Tokenizer.GetSymbolForToken(op), iThisType, DeclType.Var, iReturnType));
            iNames.Add(
                string.Format(
                    "{0}-{1}-{2}{3}", 
                    Tokenizer.GetSymbolForToken(op), 
                    DeclType.Var, 
                    DeclType.Var, 
                    iReturnType));
            if (string.IsNullOrEmpty(iReturnType) == false && returnType != DeclType.Var)
            {
                iNames.Add(
                    string.Format(
                        "{0}-{1}-{2}:{3}", 
                        Tokenizer.GetSymbolForToken(op), 
                        iThisType, 
                        iTypeOfRight, 
                        DeclType.Var));
                iNames.Add(
                    string.Format(
                        "{0}-{1}-{2}:{3}", 
                        Tokenizer.GetSymbolForToken(op), 
                        iThisType, 
                        DeclType.Var, 
                        DeclType.Var));
                iNames.Add(
                    string.Format(
                        "{0}-{1}-{2}:{3}", 
                        Tokenizer.GetSymbolForToken(op), 
                        DeclType.Var, 
                        DeclType.Var, 
                        DeclType.Var));
            }

            return iNames;
        }

        /// <summary>Gets the function.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="names">The names.</param>
        /// <param name="from">From.</param>
        /// <param name="op">The op.</param>
        /// <param name="forSetter">if set to <c>true</c> , it's for a setter function. in this case, an
        ///     error is rendered when no function is found, othewise there isn't</param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        private NNLFunctionNode GetFunction(
            NNLModuleCompiler renderTo, System.Collections.Generic.List<string> names, 
            NNLBindItemBase from, 
            Token op, 
            bool forSetter = true)
        {
            NNLFunctionNode iFunction = null;
            var iFrom = from as NNLBindItemIndex;
            if (iFrom == null)
            {
                var iBind = from.RootBinding;
                iFunction = GetFunctionFrom(names, iBind.OperatorOverloads);
                if (iFunction == null && forSetter)
                {
                    LogPosError(
                        "Invalid set operation: the binding doesn't define an overloaded setter at this stage", 
                        renderTo);
                    Item = Brain.Current[(ulong)PredefinedNeurons.Empty];

                        // assign the item so taht we only try to compile 1 time.
                }
            }
            else
            {
                if (op == Token.Assign)
                {
                    iFunction = GetFunctionFrom(names, iFrom.Setter);
                }
                else
                {
                    iFunction = GetFunctionFrom(names, iFrom.OperatorOverloads);
                }

                if (iFunction == null)
                {
                    // still need to check the rootbinding maybe there is an operator overload there.
                    var iBind = from.RootBinding;
                    iFunction = GetFunctionFrom(names, iBind.OperatorOverloads);
                }
            }

            return iFunction;
        }

        /// <summary>The get function from.</summary>
        /// <param name="names">The names.</param>
        /// <param name="dict">The dict.</param>
        /// <returns>The <see cref="NNLFunctionNode"/>.</returns>
        private NNLFunctionNode GetFunctionFrom(System.Collections.Generic.List<string> names, System.Collections.Generic.Dictionary<string, NNLFunctionNode> dict)
        {
            NNLFunctionNode iFunction = null;
            foreach (var i in names)
            {
                dict.TryGetValue(i, out iFunction);
                if (iFunction != null)
                {
                    return iFunction;
                }
            }

            return null;
        }

        /// <summary>performs the final stage for rendering a set: checks the<see langword="operator"/> (get from the 'setters' list or one of the
        ///     other <see langword="operator"/> overloads.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <param name="iFunction">The i Function.</param>
        /// <param name="args">The args.</param>
        /// <param name="names">The names.</param>
        /// <param name="op">The op.</param>
        /// <param name="codeRefs">contains a list of neurons (usually vars) that were used in the path,
        ///     and which possibly need to be passed into a possible callback.</param>
        private void RenderSet(
            NNLModuleCompiler renderTo, 
            NNLFunctionNode iFunction, System.Collections.Generic.List<NNLStatementNode> args, System.Collections.Generic.List<string> names, 
            Token op, System.Collections.Generic.List<Neuron> codeRefs)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek();
            iFunction.Render(renderTo); // make certain that the functio is rendered.
            if (fbinding.HasFunctions)
            {
                // if the binding defines a setter-overload, things need to be rendered differently.
                var iName = string.Format("this-{0}", DeclType.Var);

                    // this is the name of the function that takes to cluster-to-call as argument. 
                var iShort = "this"; // in case there is only a definition with no arguments. 
                NNLStatementNode iFound;
                var iTempArgs = new System.Collections.Generic.List<Neuron>();
                if (fbinding.Children.TryGetValue(iName, out iFound)
                    || fbinding.Children.TryGetValue(iShort, out iFound))
                {
                    RenderPopFor(RenderedItems, renderTo, codeRefs);
                    var iSetterResult = iFunction.RenderCall(renderTo, args, RenderedItems);
                    if (iSetterResult.ID != (ulong)PredefinedNeurons.ReturnValue)
                    {
                        // could be that the setter is stil trying to return a value, should not be legal.
                        RenderedItems.Add(iSetterResult);
                    }
                    else
                    {
                        LogPosError("Setters should not return a value", renderTo);
                    }

                    Item = GetParentsFor(RenderedItems, (ulong)PredefinedNeurons.Code, renderTo, null);

                        // make the callback, put it in item, so that 'HandleGetOverload can easily work with it.
                    RenderPushFor(iRenderingTo, renderTo, codeRefs); // do first, so they can be retrieved last.
                    iRenderingTo.Add(GetPushValue(this, renderTo));

                        // Item currently contains the code list, which becomes the value.
                    iFound.Render(renderTo);
                    iTempArgs.Add(iFound.Item);
                    var iArgs = GetParentsFor(iTempArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);

                        // still need to render the function call
                    Item = GetStatement((ulong)PredefinedNeurons.CallInstruction, iArgs, renderTo);
                    return;
                }
            }

            iRenderingTo.AddRange(RenderedItems);
            Item = iFunction.RenderCall(renderTo, args);
        }
    }
}