// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLStatementNode.cs" company="">
//   
// </copyright>
// <summary>
//   base class for nodes that represent code statements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     base class for nodes that represent code statements.
    /// </summary>
    internal class NNLStatementNode
    {
        /// <summary>Initializes a new instance of the <see cref="NNLStatementNode"/> class. Initializes a new instance of the <see cref="NNLStatementNode"/>
        ///     class.</summary>
        /// <param name="type">The type.</param>
        public NNLStatementNode(NodeType type)
        {
            Type = type;
        }

        #region Type

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public NodeType Type { get; set; }

        #endregion

        #region Start

        /// <summary>
        ///     Gets/sets the start of the label in the source text
        /// </summary>
        public FilePos Start { get; set; }

        #endregion

        #region Length

        /// <summary>
        ///     Gets the <see cref="Length" /> of the item in the source text.
        /// </summary>
        public int Length
        {
            get
            {
                return End.Pos - Start.Pos;
            }
        }

        #endregion

        #region Parent

        /// <summary>
        ///     Gets/sets the parent node, so we quickly travers the tree.
        /// </summary>
        public NNLStatementNode Parent { get; set; }

        #endregion

        /// <summary>
        ///     gets the first parent that is a node (not a statement node)
        /// </summary>
        public NNLNode FirstNodeParent
        {
            get
            {
                var iParent = Parent;
                while (iParent != null)
                {
                    if (iParent is NNLNode)
                    {
                        return (NNLNode)iParent;
                    }

                    iParent = iParent.Parent;
                }

                return null;
            }
        }

        #region End

        /// <summary>
        ///     gets/sets the end position of the item.
        /// </summary>
        public FilePos End { get; set; }

        #endregion

        #region FileName

        /// <summary>
        ///     Gets/sets the name of the file in which this object was defined. So we
        ///     can render errors in the generate stage.
        /// </summary>
        public string FileName { get; set; }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the node (if any).
        /// </summary>
        public string Name { get; set; }

        #endregion

        /// <summary>
        ///     Gets the NS path.
        /// </summary>
        public string NSPath
        {
            get
            {
                var iStr = new System.Text.StringBuilder();
                var iParent = Parent;
                while (iParent != null)
                {
                    iStr.Insert(0, ".");
                    iStr.Insert(0, iParent.Name);
                    iParent = iParent.Parent;
                }

                iStr.Append(Name);
                return iStr.ToString();
            }
        }

        /// <summary>
        ///     when this item has already been loaded, this buffers the value so we
        ///     can access it faster again. for adding multple items, use
        ///     <see cref="JaStDev.HAB.Parsers.NNLNodesList.Items" />
        /// </summary>
        public Neuron Item { get; set; }

        #region ExtraItems

        /// <summary>
        ///     Gets the list of items that were rendered as extras for calculating
        ///     the statement. This happens when there is a function call. In this
        ///     case, we need to re-arrange the code so that everything works ok.
        /// </summary>
        public System.Collections.Generic.List<Neuron> ExtraItems { get; internal set; }

        #endregion

        /// <summary>
        ///     Gets/sets the id that was assigned to the neuron in the source. we
        ///     should try to get a neuron with the specified id from the network.
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal virtual void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                // if the item is already assigned, don't complain. This happens during hte rendering of a call in a binding path.
                throw new System.NotImplementedException();
            }

            renderTo.Add(Item);
        }

        /// <summary>renders the item, taking into account that there are possibly extra
        ///     items that need to be renered and added to the previous code.</summary>
        /// <param name="renderTo"></param>
        /// <param name="renderExtraIn">The render Extra In.</param>
        internal void RenderItem(NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> renderExtraIn)
        {
            System.Collections.Generic.List<Neuron> iExtraItems;

                // every item can possibly render extra items, we need to take this into account and make certain that if the item was already rendered, we also use the extra items.
            if (ExtraItems == null)
            {
                iExtraItems = new System.Collections.Generic.List<Neuron>();
                renderTo.RenderingTo.Push(iExtraItems);
            }
            else
            {
                iExtraItems = ExtraItems;
            }

            try
            {
                Render(renderTo);
            }
            finally
            {
                renderTo.RenderingTo.Pop();
                if (iExtraItems.Count == 0 && ExtraItems != null)
                {
                    // some inheriters still render their own ExtraItems (AND-OR bool binary nodes, so they can group multiple booleans in a single callback). We need to make certain that the correct code is added as extra items.
                    iExtraItems = ExtraItems;
                }

                if (iExtraItems.Count > 0)
                {
                    if (ExtraItems == null)
                    {
                        // could be taht we collected the extra items, and not the inheriter itself.
                        ExtraItems = iExtraItems;
                    }

                    if (iExtraItems.Count > 0 && renderTo.RenderingArguments && renderTo.AllowFunctionCalls == false)
                    {
                        // check if it was allowed to do a function call at this position.
                        LogPosError("Function calls not allowed here.", renderTo);
                    }

                    renderExtraIn.AddRange(iExtraItems);
                }
            }
        }

        /// <summary>Logs the error.</summary>
        /// <param name="message">The message.</param>
        /// <param name="renderTo">The render To.</param>
        protected internal void LogPosError(string message, NNLModuleCompiler renderTo)
        {
            var iError = string.Format("L:{0} C:{1} {2}", Start.Line, Start.Column, message);
            renderTo.WriteError(iError, FileName);
        }

        /// <summary>Gets the <see langword="ref"/> by for the specified item. If none is
        ///     existing, one is created.</summary>
        /// <param name="item">The item.</param>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="ByRefExpression"/>.</returns>
        protected ByRefExpression GetByRef(Neuron item, NNLModuleCompiler renderTo)
        {
            var iRes = item.FindFirstIn((ulong)PredefinedNeurons.Argument) as ByRefExpression;
            if (iRes == null)
            {
                iRes = NeuronFactory.Get<ByRefExpression>();
                Brain.Current.Add(iRes);
                iRes.Argument = item;
            }

            renderTo.Add(iRes);
            return iRes;
        }

        /// <summary>Gets the parents for the specified <paramref name="list"/> of neurons</summary>
        /// <param name="list">The list.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        internal static NeuronCluster GetParentsFor(System.Collections.Generic.List<Neuron> list, 
            ulong meaning, 
            NNLModuleCompiler renderTo, 
            string name)
        {
            NeuronCluster iParent = null;
            var iParents = Neuron.FindCommonParents(list);
            if (iParents != null)
            {
                foreach (NeuronCluster i in iParents)
                {
                    if (i.Meaning == meaning)
                    {
                        IDListAccessor iList = i.Children;
                        iList.Lock();
                        try
                        {
                            if (iList.CountUnsafe == list.Count)
                            {
                                iParent = i;
                                for (var u = 0; u < iList.CountUnsafe; u++)
                                {
                                    if (iList.GetUnsafe(u) != list[u].ID)
                                    {
                                        iParent = null;
                                        break;
                                    }
                                }

                                if (iParent != null)
                                {
                                    break;
                                }
                            }
                        }
                        finally
                        {
                            iList.Dispose(); // also unlocks.
                        }
                    }
                }
            }

            if (iParent == null)
            {
                iParent = NeuronFactory.GetCluster();
                Brain.Current.Add(iParent);
                iParent.Meaning = meaning;
                using (var iChildren = iParent.ChildrenW) iChildren.AddRange(list);
                if (string.IsNullOrEmpty(name) == false)
                {
                    NNLModuleCompiler.NetworkDict.SetName(iParent, name);
                }
            }

            renderTo.Add(iParent);

                // always add, when the other one is dropped, the item will remain in existence, since we still have a ref to it.
            return iParent;
        }

        /// <summary>Finds a result statement that links to the specified instruction and
        ///     arguments cluster. If non is found, a new one is created.</summary>
        /// <param name="op">The op.</param>
        /// <param name="args">The args.</param>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal static Neuron GetResultStatement(ulong op, NeuronCluster args, NNLModuleCompiler renderTo)
        {
            var iRes = GetStatementFromInst<ResultStatement>(op, args);
            if (iRes == null)
            {
                iRes = NeuronFactory.Get<ResultStatement>();
                Brain.Current.Add(iRes);
                Link.Create(iRes, op, (ulong)PredefinedNeurons.Instruction);
                if (args != null)
                {
                    Link.Create(iRes, args, (ulong)PredefinedNeurons.Arguments);
                }
            }

            renderTo.Add(iRes);
            return iRes;
        }

        /// <summary>Finds a statement that links to the specified instruction and
        ///     arguments cluster. If non is found, a new one is created.</summary>
        /// <param name="op">The op.</param>
        /// <param name="args">The args.</param>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal static Neuron GetStatement(ulong op, NeuronCluster args, NNLModuleCompiler renderTo)
        {
            var iRes = GetStatementFromInst<Statement>(op, args);
            if (iRes == null)
            {
                iRes = NeuronFactory.Get<Statement>();
                Brain.Current.Add(iRes);
                Link.Create(iRes, op, (ulong)PredefinedNeurons.Instruction);
                if (args != null)
                {
                    Link.Create(iRes, args, (ulong)PredefinedNeurons.Arguments);
                }
            }

            renderTo.Add(iRes);
            return iRes;
        }

        /// <summary>The get statement from inst.</summary>
        /// <param name="op">The op.</param>
        /// <param name="args">The args.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/>.</returns>
        private static T GetStatementFromInst<T>(ulong op, NeuronCluster args) where T : Neuron
        {
            if (args != null)
            {
                using (var iLinks = args.LinksIn)
                {
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == (ulong)PredefinedNeurons.Arguments)
                        {
                            var iStat = i.From as T;
                            if (iStat != null)
                            {
                                var iInst = iStat.FindFirstOut((ulong)PredefinedNeurons.Instruction);

                                    // still a potential deadlock: potential cache write inside neuronlock.
                                if (iInst != null && iInst.ID == op)
                                {
                                    return iStat;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var iInst = Brain.Current[op];
                using (var iLinks = iInst.LinksIn)
                {
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == (ulong)PredefinedNeurons.Instruction)
                        {
                            var iStat = i.From as T;
                            if (iStat != null)
                            {
                                var iArgs = iStat.FindFirstOut((ulong)PredefinedNeurons.Arguments);

                                    // still a potential deadlock: potential cache write inside neuronlock.
                                if (iArgs == null)
                                {
                                    return iStat;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>Tries to calculate the type of the object. By default, this is a var,
        ///     meaning any type.</summary>
        /// <param name="renderTo">The render To.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal virtual DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            return DeclType.Var;
        }

        /// <summary>calcualtes the typedecl of an instruction.</summary>
        /// <param name="inst"></param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected static DeclType GetTypDeclFromInstruction(ulong inst)
        {
            var iInst = Brain.Current[inst] as Instruction;
            if (iInst != null)
            {
                if (iInst is ICalculateInt)
                {
                    return DeclType.Int;
                }

                if (iInst is ICalculateDouble)
                {
                    return DeclType.Double;
                }

                if (iInst is ICalculateBool)
                {
                    return DeclType.Bool;
                }

                if (iInst is IExecResultStatement)
                {
                    return DeclType.Var;

                        // we return a var cause this type of instruction needs the arguments to determin the type.
                }

                if (iInst is ResultInstruction)
                {
                    return DeclType.Var;
                }

                return DeclType.none;
            }

            throw new System.InvalidOperationException("not an instruction, can't calculate type.");
        }

        /// <summary>renders a parameter value for a function.</summary>
        /// <param name="toRender">To render.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="renderingTo">The rendering to.</param>
        /// <returns>The number of parameters that were rendered.</returns>
        protected int RenderParam(
            NNLStatementNode toRender, 
            NNLModuleCompiler renderTo, System.Collections.Generic.List<Neuron> renderingTo)
        {
            var iRes = 0;
            if (toRender != null)
            {
                renderTo.RenderingArguments = true;
                try
                {
                    var iParams = toRender as NNLUnionNode;
                    if (iParams != null)
                    {
                        // multiple params
                        var iReverse = new System.Collections.Generic.List<NNLStatementNode>(iParams.Items);
                        iReverse.Reverse();

                            // we need to render the params in the inverse order cause it's a stack and we get them of the stack in the same order as decleration (works best)
                        foreach (var iToRender in iReverse)
                        {
                            iToRender.Render(renderTo);
                            if (iToRender.Item != null)
                            {
                                // if it's null, someting went wrong and an error was probably generated.
                                renderingTo.Add(GetPushValue(iToRender, renderTo));
                                iRes++;
                            }
                        }
                    }
                    else
                    {
                        toRender.Render(renderTo);
                        if (toRender.Item != null)
                        {
                            renderingTo.Add(GetPushValue(toRender, renderTo));
                        }

                        iRes++;
                    }
                }
                finally
                {
                    renderTo.RenderingArguments = false;
                }
            }

            return iRes;
        }

        /// <summary>checks if the nr of parameters that were supplied are ok for the
        ///     function to call. When this is not the case, errors are generated.</summary>
        /// <param name="renderTo"></param>
        /// <param name="nrParams"></param>
        /// <param name="toCall"></param>
        protected void CheckNrOfParameters(NNLModuleCompiler renderTo, int nrParams, NNLFunctionNode toCall)
        {
            if (toCall.Params != null)
            {
                if (toCall.Params.Count != nrParams)
                {
                    LogPosError(string.Format("Invalid nr of parameters supplied for {0}.", toCall.Name), renderTo);
                }
            }
            else if (nrParams != 0)
            {
                LogPosError(string.Format("Function {0} has no parameters.", toCall.Name), renderTo);
            }
        }

        /// <summary>gets a call to the pushValue instruction.</summary>
        /// <param name="toRender"></param>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron GetPushValue(NNLStatementNode toRender, NNLModuleCompiler renderTo)
        {
            var iArgs = new System.Collections.Generic.List<Neuron>();
            iArgs.Add(toRender.Item);
            var iArgsCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
            return GetStatement((ulong)PredefinedNeurons.PushValueInstruction, iArgsCl, renderTo);
        }

        /// <summary>gets a call to the pushValue instruction.</summary>
        /// <param name="toRender"></param>
        /// <param name="renderTo"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron GetPushValue(Neuron toRender, NNLModuleCompiler renderTo)
        {
            var iArgs = new System.Collections.Generic.List<Neuron>();
            iArgs.Add(toRender);
            var iArgsCl = GetParentsFor(iArgs, (ulong)PredefinedNeurons.ArgumentsList, renderTo, string.Empty);
            return GetStatement((ulong)PredefinedNeurons.PushValueInstruction, iArgsCl, renderTo);
        }

        /// <summary>builds an if statement. Whent he condition is met, The<see langword="bool"/> exp itself is returned. This is then used by
        ///     the cond part as a case.</summary>
        /// <param name="boolExp">The condition for the if statement.</param>
        /// <param name="renderTo"></param>
        /// <param name="returnValue">The return Value.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected static Neuron GetConditionalReturn(Neuron boolExp, NNLModuleCompiler renderTo, Neuron returnValue)
        {
            ConditionalStatement iRes = null;
            if (boolExp != null)
            {
                // can be null for last conditional part
                var iCond = boolExp.FindFirstIn((ulong)PredefinedNeurons.Condition) as ConditionalExpression;

                    // get the conditional part attached to the bool
                if (iCond != null)
                {
                    var iCode = iCond.Statements;
                    if (iCode.Count == 1 && iCode[0] is Statement)
                    {
                        // check that the conditional part only has 1 code item: a return of the 
                        var iStat = (Statement)iCode[0];
                        if (iStat.Instruction.ID == (ulong)PredefinedNeurons.ReturnValueInstruction
                            && iStat.Arguments.Count == 1 && iStat.Arguments[0] == returnValue)
                        {
                            foreach (var i in iCond.FindAllClusteredBy((ulong)PredefinedNeurons.Code))
                            {
                                var iTemp = i.FindFirstIn((ulong)PredefinedNeurons.Condition) as ConditionalStatement;
                                if (iTemp != null)
                                {
                                    using (var iChildren = i.Children)
                                        if (iChildren.Count == 1)
                                        {
                                            iRes = iTemp;
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }

            if (iRes == null)
            {
                iRes = CreateConditionalReturn(boolExp, renderTo, returnValue);
            }

            return iRes;
        }

        /// <summary>The create conditional return.</summary>
        /// <param name="boolExp">The bool exp.</param>
        /// <param name="renderTo">The render to.</param>
        /// <param name="returnValue">The return value.</param>
        /// <returns>The <see cref="ConditionalStatement"/>.</returns>
        private static ConditionalStatement CreateConditionalReturn(
            Neuron boolExp, 
            NNLModuleCompiler renderTo, 
            Neuron returnValue)
        {
            var iCond = NeuronFactory.Get<ConditionalExpression>();
            Brain.Current.Add(iCond);
            renderTo.Add(iCond);
            if (boolExp != null)
            {
                // the last part can be null.
                Link.Create(iCond, boolExp, (ulong)PredefinedNeurons.Condition);
            }

            var iStat = NeuronFactory.Get<Statement>();
            Brain.Current.Add(iStat);
            renderTo.Add(iStat);
            Link.Create(iStat, (ulong)PredefinedNeurons.ReturnValueInstruction, (ulong)PredefinedNeurons.Instruction);
            var iArgs = NeuronFactory.GetCluster();
            Brain.Current.Add(iArgs);
            renderTo.Add(iArgs);
            iArgs.Meaning = (ulong)PredefinedNeurons.ArgumentsList;
            using (var iChildren = iArgs.ChildrenW) iChildren.Add(returnValue);
            Link.Create(iStat, iArgs, (ulong)PredefinedNeurons.Arguments);

            var iCode = NeuronFactory.GetCluster();
            Brain.Current.Add(iCode);
            renderTo.Add(iCond);
            iCode.Meaning = (ulong)PredefinedNeurons.Code;
            Link.Create(iCond, iCode, (ulong)PredefinedNeurons.Statements);
            using (var iChildren = iCode.ChildrenW) iChildren.Add(iStat);

            var iRes = NeuronFactory.Get<ConditionalStatement>();
            Brain.Current.Add(iRes);
            renderTo.Add(iRes);
            var iCondParts = NeuronFactory.GetCluster();
            Brain.Current.Add(iCondParts);
            iCondParts.Meaning = (ulong)PredefinedNeurons.Code;
            renderTo.Add(iCondParts);
            using (var iChildren = iCondParts.ChildrenW) iChildren.Add(iCond);
            Link.Create(iRes, iCondParts, (ulong)PredefinedNeurons.Condition);
            Link.Create(iRes, (ulong)PredefinedNeurons.Normal, (ulong)PredefinedNeurons.LoopStyle);

                // need to make certain that it's designated as an if statement.
            return iRes;
        }

        /// <summary>write the item to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public virtual void Write(System.IO.BinaryWriter stream)
        {
            if (Name == null)
            {
                // in case there was no name specified.
                Name = string.Empty;
            }

            stream.Write(Name);
            if (Item != null)
            {
                stream.Write(Item.ID);
            }
            else
            {
                stream.Write((System.UInt64)0);
            }

            if (ExtraItems != null)
            {
                stream.Write(ExtraItems.Count);
                foreach (var i in ExtraItems)
                {
                    stream.Write(i.ID);
                }
            }
            else
            {
                stream.Write(0);
            }
        }

        /// <summary>reads the item from a pre-compiled stream.</summary>
        /// <param name="reader"></param>
        public virtual void Read(System.IO.BinaryReader reader)
        {
            Name = reader.ReadString();
            var iID = reader.ReadUInt64();
            if (iID != Neuron.EmptyId)
            {
                // could be empty (for bindings.)
                Item = Brain.Current[iID];
            }

            var iCount = reader.ReadInt32();
            if (iCount > 0)
            {
                ExtraItems = new System.Collections.Generic.List<Neuron>();
                while (iCount > 0)
                {
                    var iId = reader.ReadUInt64();
                    ExtraItems.Add(Brain.Current[iId]);
                    iCount--;
                }
            }
        }

        /// <summary>Writes the dictionary to the stream.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="toWrite">To write.</param>
        protected void WriteList(
            System.IO.BinaryWriter stream, System.Collections.Generic.Dictionary<string, NNLFunctionNode> toWrite)
        {
            if (toWrite != null)
            {
                stream.Write(toWrite.Count);
                foreach (var i in toWrite)
                {
                    stream.Write(i.Key);
                    i.Value.Write(stream);
                }
            }
            else
            {
                stream.Write(0);
            }
        }

        /// <summary>Reads the dictionary from file.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        protected System.Collections.Generic.Dictionary<string, NNLFunctionNode> ReadList(System.IO.BinaryReader reader)
        {
            var iCount = reader.ReadInt32();
            if (iCount > 0)
            {
                var iRes = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                while (iCount > 0)
                {
                    var iKey = reader.ReadString();
                    var iFunction = new NNLFunctionNode();
                    iFunction.Read(reader);
                    iRes.Add(iKey, iFunction);
                    iCount--;
                }

                return iRes;
            }

            return null;
        }
    }
}