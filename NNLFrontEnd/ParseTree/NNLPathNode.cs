// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLPathNode.cs" company="">
//   
// </copyright>
// <summary>
//   contains a list of items that define a path to select/execute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     contains a list of items that define a path to select/execute.
    /// </summary>
    internal class NNLPathNode : NNLNodesList
    {
        /// <summary>
        ///     we buffer this value, primarely for 'using' clauses, so they can get
        ///     to the terminus fast.
        /// </summary>
        private NNLStatementNode fPathTerminus;

        /// <summary>Initializes a new instance of the <see cref="NNLPathNode"/> class.</summary>
        public NNLPathNode()
            : base(NodeType.Path)
        {
        }

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            var iReffered = GetPathTerminus(renderTo);
            if (iReffered != null)
            {
                var iNode = (NNLReferenceNode)Items[Items.Count - 1];
                if (iNode.IsFunctionCall == false)
                {
                    return iReffered.GetTypeDecl(renderTo);
                }

                var iFunc = iReffered as NNLFunctionNode;
                if (iFunc == null)
                {
                    var iCluster = iReffered as NNLClusterNode;
                    if (iCluster != null && iCluster.Content != null)
                    {
                        return iCluster.Content.GetTypeDeclForCall(renderTo);
                    }

                    var iInst = iReffered as NNLNeuronNode;
                    if (iInst != null)
                    {
                        iInst.Render(renderTo);
                        if (iInst.Item != null)
                        {
                            return GetTypDeclFromInstruction(iInst.Item.ID);
                        }

                        return DeclType.none;
                    }

                    if (iReffered.GetType() == typeof(NNLExpBlockNode))
                    {
                        return ((NNLExpBlockNode)iReffered).Content.GetTypeDeclForCall(renderTo);
                    }

                    LogPosError(
                        string.Format("{0} is used as a function, but it is not a function", iNode.Reference), 
                        renderTo);
                }
                else
                {
                    return iFunc.GetTypeDeclForCall(renderTo);
                }
            }

            return base.GetTypeDecl(renderTo);
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iReffered = GetPathTerminus(renderTo);
                if (iReffered != null)
                {
                    var iNode = (NNLReferenceNode)Items[Items.Count - 1];
                    iReffered.Render(renderTo);
                    if (iNode.IsFunctionCall == false)
                    {
                        Item = iReffered.Item;
                    }
                    else
                    {
                        if (iReffered is NNLOSFunction)
                        {
                            Item = RenderOSFunctionCall(iReffered, renderTo);
                        }
                        else
                        {
                            var iFunc = iReffered as NNLFunctionNode;
                            if (iFunc == null)
                            {
                                var iCluster = iReffered as NNLClusterNode;
                                if (iCluster != null)
                                {
                                    if (iCluster.Content != null)
                                    {
                                        Item = RenderFunctionCall(iCluster.Content, renderTo, iNode.IsWaitFor);
                                    }
                                    else
                                    {
                                        LogPosError(
                                            string.Format(
                                                "{0} is used as a function, but the NeuronCluster has no 'this' section.", 
                                                iNode.Reference), 
                                            renderTo);
                                    }
                                }
                                else
                                {
                                    var iInst = iReffered as NNLNeuronNode;
                                    if (iInst != null)
                                    {
                                        RenderStatement(iInst, renderTo);
                                    }
                                    else if (iReffered.GetType() == typeof(NNLExpBlockNode))
                                    {
                                        RenderExpBlock((NNLExpBlockNode)iReffered, renderTo);
                                    }
                                    else
                                    {
                                        LogPosError(
                                            string.Format(
                                                "{0} is used as a function, but it is not a function", 
                                                iNode.Reference), 
                                            renderTo);
                                    }
                                }
                            }
                            else
                            {
                                Item = RenderFunctionCall(iFunc, renderTo, iNode.IsWaitFor);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>The render os function call.</summary>
        /// <param name="func">The func.</param>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron RenderOSFunctionCall(NNLStatementNode func, NNLModuleCompiler renderTo)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek(); // we need to add multiple items in this list.
            var iNode = (NNLReferenceNode)Items[Items.Count - 1];
            Neuron iRes;

            RenderParam(iNode.ParamValues, renderTo, iRenderingTo);
            RenderParam(func, renderTo, iRenderingTo);

            renderTo.CallOSFunction.Render(renderTo);
            if (renderTo.CallOSFunction is NNLExpBlockNode)
            {
                iRes = renderTo.CallOSFunction.Item;
            }
            else
            {
                var iArgValues = new System.Collections.Generic.List<Neuron>();
                iArgValues.Add(renderTo.CallOSFunction.Item);
                var iArgs = GetParentsFor(iArgValues, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
                iRes = GetStatement((ulong)PredefinedNeurons.CallInstruction, iArgs, renderTo);
            }

            return iRes;
        }

        /// <summary>The render exp block.</summary>
        /// <param name="toCall">The to call.</param>
        /// <param name="renderTo">The render to.</param>
        private void RenderExpBlock(NNLExpBlockNode toCall, NNLModuleCompiler renderTo)
        {
            var iNode = (NNLReferenceNode)Items[Items.Count - 1];
            var iRenderingTo = renderTo.RenderingTo.Peek(); // we need to add multiple items in this list.
            var iNrPar = RenderParam(iNode.ParamValues, renderTo, iRenderingTo);

                // make certain that the parameter values are pushed on the stack before the expBlock is called.
            CheckNrOfParameters(renderTo, iNrPar, toCall.Content);
            if (toCall.Content.ReturnValues != null && toCall.Content.ReturnValues.Count > 0)
            {
                iRenderingTo.Add(toCall.Item);
                Item = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];
            }
            else
            {
                Item = toCall.Item; // an expression can simply be linked to.
            }
        }

        /// <summary>The render statement.</summary>
        /// <param name="inst">The inst.</param>
        /// <param name="renderTo">The render to.</param>
        private void RenderStatement(NNLNeuronNode inst, NNLModuleCompiler renderTo)
        {
            NeuronCluster iArgs = null;
            var iNode = (NNLReferenceNode)Items[Items.Count - 1];
            var iPrevRenderingArgs = renderTo.RenderingArguments;
            var iPrevAllowFunctionCalls = renderTo.AllowFunctionCalls;
            renderTo.RenderingArguments = true;

                // important for conditionals -> if referenced as arg value, use a neuron, don't try to generated any thing required to do a call.
            renderTo.AllowFunctionCalls = inst.GetAllowFunctionsCalls();
            try
            {
                if (iNode.ParamValues is NNLPathNode || iNode.ParamValues is NNLNeuronNode
                    || iNode.ParamValues is NNLClusterNode || iNode.ParamValues is NNLUnaryStatement
                    || iNode.ParamValues is NNLBinaryStatement)
                {
                    // single value arguments
                    iNode.ParamValues.Render(renderTo);
                    if (iNode.ParamValues.Item != null)
                    {
                        var iChildren = new System.Collections.Generic.List<Neuron> { iNode.ParamValues.Item };
                        iArgs = GetParentsFor(iChildren, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);
                    }
                }
                else if (iNode.ParamValues is NNLNodesList)
                {
                    // could be null ->no args
                    var iArgsList = (NNLNodesList)iNode.ParamValues;
                    iArgsList.RenderReverseItems(renderTo);

                        // render the list in reverse so that the 'returnvalue's are in the correct order (it's a stack)
                    iArgs = GetParentsFor(
                        iArgsList.RenderedItems, 
                        (ulong)PredefinedNeurons.ArgumentsList, 
                        renderTo, 
                        null);
                }
            }
            finally
            {
                renderTo.RenderingArguments = iPrevRenderingArgs;
                renderTo.AllowFunctionCalls = iPrevAllowFunctionCalls;
            }

            if (inst.Item is ResultInstruction)
            {
                Item = GetResultStatement(inst.Item.ID, iArgs, renderTo);
            }
            else
            {
                Item = GetStatement(inst.Item.ID, iArgs, renderTo);
            }

            renderTo.Add(this);
        }

        /// <summary>Creates a function call</summary>
        /// <param name="func">The func.</param>
        /// <param name="renderTo">The render To.</param>
        /// <param name="asBlockedCall">The as Blocked Call.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron RenderFunctionCall(NNLFunctionNode func, NNLModuleCompiler renderTo, bool asBlockedCall)
        {
            var iRenderingTo = renderTo.RenderingTo.Peek(); // we need to add multiple items in this list.
            Neuron iRes = null;

            ulong iToCall;
            if (asBlockedCall == false)
            {
                iToCall = (ulong)PredefinedNeurons.CallInstruction;
            }
            else
            {
                iToCall = (ulong)PredefinedNeurons.BlockedCallInstruction;
            }

            if (func.IsInline == false)
            {
                NeuronCluster iArgs;
                var iNode = (NNLReferenceNode)Items[Items.Count - 1];

                var iNrPar = RenderParam(iNode.ParamValues, renderTo, iRenderingTo);
                CheckNrOfParameters(renderTo, iNrPar, func);
                var iChildren = new System.Collections.Generic.List<Neuron>();
                if (func.Item != null)
                {
                    iChildren.Add(func.Item); // when we call the function, we need to supply what needs to be called.
                    iArgs = GetParentsFor(iChildren, (ulong)PredefinedNeurons.ArgumentsList, renderTo, null);

                    var iIn = iArgs.FindAllIn((ulong)PredefinedNeurons.Arguments);
                    foreach (var i in iIn)
                    {
                        var iTemp = i as Statement;
                        if (iTemp != null && iTemp.Instruction.ID == iToCall)
                        {
                            iRes = iTemp;
                            break;
                        }
                    }

                    if (iRes == null)
                    {
                        iRes = NeuronFactory.Get<Statement>();
                        Brain.Current.Add(iRes);
                        Link.Create(iRes, iToCall, (ulong)PredefinedNeurons.Instruction);
                        Link.Create(iRes, iArgs, (ulong)PredefinedNeurons.Arguments);
                    }

                    Item = iRes;

                        // we temporerly set item to iRes, so that we can add to the module through 'Add(this)', so the name gets assigned to the correct neuron, always be the same function.
                    renderTo.Add(this);
                    renderTo.Add(iArgs);
                    if (func.ReturnValues != null && func.ReturnValues.Count > 0)
                    {
                        iRenderingTo.Add(iRes); // the call instruction has to happen before retrieving the result.
                        iRes = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];
                    }
                }
                else
                {
                    iRes = Brain.Current[(ulong)PredefinedNeurons.ReturnValue];

                        // something went wrong during the render, can't calculate the value for the statement, still we store something in Item to make certain that this will only render an error 1 time.
                }
            }
            else
            {
                func.RenderItemsInto(iRenderingTo, renderTo);

                    // when we do an inline, we dont' store an item, cause nothing to add (everything to add is already added.
            }

            return iRes;
        }

        /// <summary>gest the node that the last item in the path refers to, starting from
        ///     the root.</summary>
        /// <param name="renderTo">for error logging. Can be <see langword="null"/> (no errors will be
        ///     logged</param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        internal NNLStatementNode GetPathTerminus(NNLModuleCompiler renderTo)
        {
            if (fPathTerminus == null)
            {
                NNLStatementNode iReffered = null;
                var iNode = (NNLReferenceNode)Items[0];

                var iParent = FirstNodeParent;
                var iFound = false;
                while (iParent != null && iFound == false)
                {
                    iFound = iParent.Children.TryGetValue(iNode.Reference, out iReffered);
                    iParent = iParent.FirstNodeParent;
                }

                if (iFound == false && !(this is NNLUsingPathNode))
                {
                    // using clauses should go look inside other using clauses, that creates an ethernal loop if the item isn't defined.
                    foreach (var i in AllUsings())
                    {
                        if (i != this)
                        {
                            // i == this if we want to resolve a 'using' clause and it returns the same 'include' as we are trying to resolve.
                            iParent = i.GetPathTerminus(renderTo) as NNLNode;

                                // get the nod to which the using clause is pointing.
                            while (iParent != null && iFound == false)
                            {
                                iFound = iParent.Children.TryGetValue(iNode.Reference, out iReffered);
                                iParent = iParent.FirstNodeParent;
                            }

                            if (iFound)
                            {
                                break;
                            }
                        }
                    }

                    if (iFound == false)
                    {
                        // check if it's in the mappings dict.
                        ulong iId;
                        if (ParserBase.Statics.TryGetValue(iNode.Reference, out iId))
                        {
                            // it's a static, use this and create a new node for this item + add it to the root namesapce, so it can be re-used.
                            iReffered = new NNLNeuronNode(NeuronType.Neuron);
                            iReffered.ID = iId;
                            iReffered.Item = Brain.Current[iId];
                            iReffered.Name = iNode.Reference;
                            renderTo.Root.Children.Add(iNode.Reference, iReffered);
                        }
                    }
                }

                if (iReffered == null)
                {
                    iReffered = TryGetOSFunction(iNode.Reference);
                }

                if (iReffered == null)
                {
                    if (renderTo != null)
                    {
                        LogPosError(
                            string.Format("Unknown identifier: {0} in {1}", iNode.Reference, ToString()), 
                            renderTo);
                    }

                    return null;
                }

                for (var i = 1; i < Items.Count; i++)
                {
                    iNode = (NNLReferenceNode)Items[i];
                    if (((NNLNode)iReffered).Children.TryGetValue(iNode.Reference, out iReffered) == false)
                    {
                        if (renderTo != null)
                        {
                            LogPosError(
                                string.Format("Unknown identifier: {0} in {1}", iNode.Reference, Name), 
                                renderTo);
                        }

                        return null;
                    }
                }

                fPathTerminus = iReffered;
            }

            return fPathTerminus;
        }

        /// <summary>looks up the specified <paramref name="name"/> in the list of os
        ///     functions that were registered through the reflection sin.</summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="NNLStatementNode"/>.</returns>
        private NNLStatementNode TryGetOSFunction(string name)
        {
            ulong iFound;
            if (DoParser.Functions.TryGetValue(name, out iFound))
            {
                var iNew = new NNLOSFunction();
                iNew.Item = Brain.Current[iFound];
                return iNew;
            }

            return null;
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Items.Count > 0)
            {
                var iRes = new System.Text.StringBuilder(Items[0].ToString());
                for (var i = 1; i < Items.Count; i++)
                {
                    iRes.Append(".");
                    iRes.Append(Items[i]);
                }

                return iRes.ToString();
            }

            return string.Empty;
        }

        /// <summary>checks if this path performs a function call to something other than
        ///     an instruction.</summary>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool IsFunctionCall(NNLModuleCompiler renderTo)
        {
            var iReffered = GetPathTerminus(renderTo);
            var iNode = (NNLReferenceNode)Items[Items.Count - 1];
            if (iNode.IsFunctionCall)
            {
                if (iReffered is NNLOSFunction)
                {
                    return true;
                }

                var iFunc = iReffered as NNLFunctionNode;
                if (iFunc == null)
                {
                    var iCluster = iReffered as NNLClusterNode;
                    if (iCluster != null)
                    {
                        return true;
                    }

                    var iInst = iReffered as NNLNeuronNode;
                    if (iInst != null)
                    {
                        return false;
                    }

                    if (iReffered.GetType() == typeof(NNLExpBlockNode))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}