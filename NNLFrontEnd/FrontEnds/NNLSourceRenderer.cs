// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLSourceRenderer.cs" company="">
//   
// </copyright>
// <summary>
//   renders neurons to disk in the NNL language.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     renders neurons to disk in the NNL language.
    /// </summary>
    public class NNLSourceRenderer
    {
        /// <summary>The f indent.</summary>
        private int fIndent; // determins the amount of indent to add before each line.

        /// <summary>The f instructions.</summary>
        private System.Collections.Generic.HashSet<string> fInstructions;

        /// <summary>
        ///     stores all the neurons that also need to be declared/rendered cause
        ///     they are somewhere referenced, but not in the renderlist.
        /// </summary>
        private System.Collections.Generic.HashSet<Neuron> fRenderAlso =
            new System.Collections.Generic.HashSet<Neuron>();

        /// <summary>The f target.</summary>
        private System.IO.TextWriter fTarget; // the target to render to.

        /// <summary>
        ///     the actual set of neurons that was requested to be rendered, so that
        ///     we don't render things 2 times.
        /// </summary>
        private System.Collections.Generic.HashSet<Neuron> fToRender = new System.Collections.Generic.HashSet<Neuron>();

        /// <summary>The f current path.</summary>
        private readonly System.Collections.Generic.HashSet<Neuron> fCurrentPath =
            new System.Collections.Generic.HashSet<Neuron>();

                                                                    // to keep track of the path in the tree that we currently are at, so we don't get stuck in a loop cause a parent item is it's own child.

        /// <summary>
        ///     callback to the get the name of a neuron.
        /// </summary>
        private readonly ISourceRendererDict fDict;

        /// <summary>The f named objects.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, string> fNamedObjects =
            new System.Collections.Generic.Dictionary<ulong, string>();

                                                                              // keeps track of the objects that have already gotten a name, to make certain that we don't render 2 objects with the same name (which is allowed in the assembly section).

        /// <summary>The f names to render.</summary>
        private readonly System.Collections.Generic.Dictionary<ulong, string> fNamesToRender =
            new System.Collections.Generic.Dictionary<ulong, string>();

                                                                              // keeps track of conditionals that are being rendered but who received a name, so before rendering the conditional content, they need to render the name from this dict.

        /// <summary>The f prev writers.</summary>
        private readonly System.Collections.Generic.Stack<System.IO.TextWriter> fPrevWriters =
            new System.Collections.Generic.Stack<System.IO.TextWriter>();

                                                                                // so we can switch textWriters to temp strings, in case we need to render a name for a conditional

        /// <summary>The f rendered objects.</summary>
        private readonly System.Collections.Generic.HashSet<string> fRenderedObjects =
            new System.Collections.Generic.HashSet<string>();

                                                                    // keeps track of named objects that have actually been rendered, so we know if the full item needs to be rendered or only the name.
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="NNLSourceRenderer"/> class. Initializes a new instance of the <see cref="NNLSourceRenderer"/>
        ///     class.</summary>
        /// <param name="dict">The dict.</param>
        /// <param name="target">The target.</param>
        public NNLSourceRenderer(ISourceRendererDict dict, System.IO.TextWriter target)
        {
            fDict = dict;
            fTarget = target;
        }

        #endregion

        /// <summary>Gets the instructions.</summary>
        private System.Collections.Generic.HashSet<string> Instructions
        {
            get
            {
                if (fInstructions == null)
                {
                    fInstructions = new System.Collections.Generic.HashSet<string>();
                    for (ulong i = 1; i < (ulong)PredefinedNeurons.EndOfStatic; i++)
                    {
                        if (Brain.Current.IsExistingID(i))
                        {
                            var iName = ((PredefinedNeurons)i).ToString();
                            if (iName != null)
                            {
                                iName = iName.ToLower();
                                if (iName.EndsWith("instruction") && iName != "instruction")
                                {
                                    // only add instructions, all the rest needs to be added through code decl
                                    iName = iName.Remove(iName.Length - 11);
                                    foreach (var u in iName)
                                    {
                                        if (Enumerable.Contains(NNLModuleCompiler.InvalidChars, u))
                                        {
                                            LogService.Log.LogError(
                                                "Source renderer", 
                                                string.Format("Invalid name: {0} for static: {1}", iName, i));
                                        }
                                    }

                                    fInstructions.Add(iName);
                                }
                            }
                            else
                            {
                                LogService.Log.LogError(
                                    "Module compiler", 
                                    string.Format("No name found for static: {1}", iName, i));
                            }
                        }
                    }
                }

                return fInstructions;
            }
        }

        /// <summary>Renders the code for the specified objects.</summary>
        /// <param name="toWrite">To write.</param>
        public void Render(System.Collections.Generic.IEnumerable<Neuron> toWrite)
        {
            try
            {
                if (toWrite is System.Collections.Generic.HashSet<Neuron>)
                {
                    fToRender = (System.Collections.Generic.HashSet<Neuron>)toWrite;
                }
                else
                {
                    fToRender = new System.Collections.Generic.HashSet<Neuron>(toWrite);
                }

                foreach (var i in toWrite)
                {
                    RenderNeuron(i);
                }

                while (fRenderAlso.Count > 0)
                {
                    foreach (var i in fRenderAlso)
                    {
                        // before rendering the extra items, copy them to the list of already rendered items, so we don't accidentally try to render them multiple times. This way, we don't loose them. We can add to this list cause these items have already been rednered.
                        fToRender.Add(i);
                    }

                    var iToRender = fRenderAlso;
                    fRenderAlso = new System.Collections.Generic.HashSet<Neuron>();
                    foreach (var i in iToRender)
                    {
                        RenderNeuron(i);
                    }
                }
            }
            finally
            {
                fTarget.Flush(); // in case of error, make certain that last line is rendered.
            }
        }

        /// <summary>The render.</summary>
        /// <param name="toWrite">The to write.</param>
        public void Render(Neuron toWrite)
        {
            try
            {
                fToRender = new System.Collections.Generic.HashSet<Neuron>();
                fToRender.Add(toWrite);

                RenderNeuron(toWrite);
                while (fRenderAlso.Count > 0)
                {
                    foreach (var i in fRenderAlso)
                    {
                        // before rendering the extra items, copy them to the list of already rendered items, so we don't accidentally try to render them multiple times. This way, we don't loose them. We can add to this list cause these items have already been rednered.
                        fToRender.Add(i);
                    }

                    var iToRender = fRenderAlso;
                    fRenderAlso = new System.Collections.Generic.HashSet<Neuron>();
                    foreach (var i in iToRender)
                    {
                        RenderNeuron(i);
                    }
                }
            }
            finally
            {
                fTarget.Flush(); // in case of error, make certain that last line is rendered.
            }
        }

        /// <summary>The render neuron.</summary>
        /// <param name="i">The i.</param>
        private void RenderNeuron(Neuron i)
        {
            WriteDescription(i.ID, true);
            if ((i is SystemVariable) || (i.ID < (ulong)PredefinedNeurons.EndOfStatic))
            {
                if (i is NeuronCluster)
                {
                    fTarget.Write("cluster {0}", GetName(i.ID));
                }
                else
                {
                    fTarget.Write("neuron {0}", GetName(i.ID));
                }
            }
            else if (i.GetType() == typeof(TextNeuron))
            {
                fTarget.Write("string {0}", GetName(i.ID));
            }
            else if (i is IntNeuron)
            {
                fTarget.Write("int {0}", GetName(i.ID));
            }
            else if (i is DoubleNeuron)
            {
                fTarget.Write("double {0}", GetName(i.ID));
            }
            else
            {
                fTarget.Write("{0} {1}", i.GetType().Name, GetName(i.ID));
            }

            if (i is NeuronCluster)
            {
                RenderClusterSection((NeuronCluster)i);
            }
            else if (i is IntNeuron)
            {
                RenderIntSection((IntNeuron)i);
            }
            else if (i is DoubleNeuron)
            {
                RenderDoubleSection((DoubleNeuron)i);
            }
            else if (i is TextNeuron && !(i is Sin))
            {
                RenderTextSection((TextNeuron)i);
            }
            else if (i is Variable)
            {
                RenderVarSection((Variable)i);
            }

            if (i.ID < (ulong)PredefinedNeurons.EndOfStatic)
            {
                // references to static neurons need to be declared as well.
                fTarget.Write(" : {0}", i.ID);
            }

            WriteLine(string.Empty);
            fIndent++;
            WriteLine("{");
            try
            {
                if (i is NeuronCluster)
                {
                    RenderThisCode((NeuronCluster)i);
                }

                RenderLinkCode(i);
            }
            finally
            {
                fIndent--;
            }

            WriteLine(string.Empty); // the code block doesn't do that at the end, so that the indenting can be adjusted.
            WriteLine("}");
        }

        /// <summary>The render var section.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderVarSection(Variable toRender)
        {
            var iVal = toRender.Value;
            var iSplit = toRender.SplitReaction;
            if (iSplit != null)
            {
                WriteSplitValue(iSplit.ID);
            }

            if (iVal != null)
            {
                fTarget.Write(" = ");
                WriteCodeItem(iVal);
            }
        }

        /// <summary>The write split value.</summary>
        /// <param name="split">The split.</param>
        private void WriteSplitValue(ulong split)
        {
            fTarget.Write("(");
            if (split == (ulong)PredefinedNeurons.Duplicate)
            {
                fTarget.Write("duplicate");
            }
            else if (split == (ulong)PredefinedNeurons.Copy)
            {
                fTarget.Write("copy");
            }

            if (split == (ulong)PredefinedNeurons.Empty)
            {
                fTarget.Write("clear");
            }

            fTarget.Write(")");
        }

        /// <summary>The render text section.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderTextSection(TextNeuron toRender)
        {
            var iToRender = toRender.Text.Replace("\n", "\\n");
            iToRender = iToRender.Replace("\r", "\\r");
            iToRender = iToRender.Replace("\t", "\\t");
            fTarget.Write(" = \"{0}\"", toRender.Text);
        }

        /// <summary>The render double section.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderDoubleSection(DoubleNeuron toRender)
        {
            fTarget.Write(" = {0}", toRender.Value);
        }

        /// <summary>The render int section.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderIntSection(IntNeuron toRender)
        {
            fTarget.Write(" = {0}", toRender.Value);
        }

        /// <summary>The render cluster section.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderClusterSection(NeuronCluster toRender)
        {
            if (toRender.Meaning != Neuron.EmptyId)
            {
                fTarget.Write("({0})", GetName(toRender.Meaning));
                var iMeaning = Brain.Current[toRender.Meaning];
                if (fToRender.Contains(iMeaning) == false && fRenderAlso.Contains(iMeaning) == false)
                {
                    // make certain that it is at least declared in the code, instructions are always declared, so don't need extra rendering
                    fRenderAlso.Add(iMeaning);
                }
            }
        }

        /// <summary>checks if the item has any documentation, if so, render this is
        ///     comment.</summary>
        /// <param name="toRender"></param>
        /// <param name="canMulitLine">The can Mulit Line.</param>
        private void WriteDescription(ulong toRender, bool canMulitLine = false)
        {
            var iDesc = fDict.GetDescriptionText(toRender);
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                if (canMulitLine == false)
                {
                    iDesc = iDesc.Replace("\n", "  ");
                    iDesc = iDesc.Replace("\r", "  ");
                    fTarget.Write("   //{0}", iDesc);
                }
                else
                {
                    fTarget.WriteLine("/*\n  {0} */", iDesc);
                }
            }
        }

        /// <summary>renders the code in the cluster for the specified neuron as link
        ///     meaning.</summary>
        /// <param name="toRender">The to Render.</param>
        private void RenderLinkCode(Neuron toRender)
        {
            var iToRender = new System.Collections.Generic.Dictionary<Neuron, NeuronCluster>();
            if (toRender.LinksOutIdentifier != null)
            {
                var iLinks = Factories.Default.LinkLists.GetBuffer();
                try
                {
                    using (var iList = toRender.LinksOut) iLinks.AddRange(iList); // make local copy, so we dont' have deadlocks.
                    foreach (var u in iLinks)
                    {
                        var iTo = u.To as NeuronCluster;
                        if (iTo != null && iTo.Meaning == (ulong)PredefinedNeurons.Code)
                        {
                            iToRender.Add(u.Meaning, iTo);
                        }
                    }
                }
                finally
                {
                    Factories.Default.LinkLists.Recycle(iLinks);
                }
            }

            var count = 0; // dict can't be accessed through index, but need to check if first or not.
            foreach (var u in iToRender)
            {
                if (count > 0 || toRender is NeuronCluster)
                {
                    WriteLine(string.Empty);

                        // codeblock doesn't write a new line at end, so that the indent can be changed if need be. when toRender is cluster, it already rendered the 'this' code.
                }

                WriteDescription(u.Value.ID, true);
                WriteLine(GetName(u.Key.ID) + "()");
                WriteCodeBlock(u.Value, false);
                if (fToRender.Contains(u.Key) == false && fRenderAlso.Contains(u.Key) == false)
                {
                    // link meanings need to be declared somewhere, so make certain that they are rendered.
                    fRenderAlso.Add(u.Key);
                }

                count++;
            }
        }

        /// <summary>The render this code.</summary>
        /// <param name="toRender">The to render.</param>
        private void RenderThisCode(NeuronCluster toRender)
        {
            WriteLine("this()");
            WriteCodeBlock(toRender, false);
        }

        /// <summary>Gets a unique name for the specified id.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetName(ulong id)
        {
            string iFound = null;
            if (fNamedObjects.TryGetValue(id, out iFound) == false)
            {
                iFound = MakeNameSave(fDict.GetName(id).ToLower());
                var iTemp = iFound;
                var iCount = 0;
                while (fNamedObjects.ContainsValue(iFound))
                {
                    // if not a unique name, make certain it is.
                    iCount++;
                    iFound = iTemp + iCount;
                }

                fNamedObjects.Add(id, iFound);
            }

            return iFound;
        }

        /// <summary>The make name save.</summary>
        /// <param name="p">The p.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string MakeNameSave(string p)
        {
            var iRes = p.Replace(" ", string.Empty); // need to make certain that things are properly formatted for the scanner.
            iRes = iRes.Replace("(", "_");
            iRes = iRes.Replace(")", "_");
            iRes = iRes.Replace("{", "_");
            iRes = iRes.Replace("}", "_");
            iRes = iRes.Replace("[", "_");
            iRes = iRes.Replace("]", "_");
            iRes = iRes.Replace("&", "And");
            iRes = iRes.Replace("|", "Or");
            iRes = iRes.Replace("*", "Mul");
            iRes = iRes.Replace("/", "Div");
            iRes = iRes.Replace("-", "Min");
            iRes = iRes.Replace("+", "Add");
            iRes = iRes.Replace("%", "Mod");
            iRes = iRes.Replace(";", "Comma");
            iRes = iRes.Replace(">", "Bigger");
            iRes = iRes.Replace("<", "Smaller");
            iRes = iRes.Replace("=", "Equal");
            iRes = iRes.Replace("!", "Not");
            if (Instructions.Contains(iRes))
            {
                // the name is used by an instruction (had that in the chatbot pattern definition)
                iRes = "_" + iRes;
            }

            return iRes.Replace('-', '_');
        }

        /// <summary>The write code block.</summary>
        /// <param name="toRender">The to render.</param>
        /// <param name="allowSingleLine">The allow single line.</param>
        private void WriteCodeBlock(NeuronCluster toRender, bool allowSingleLine = true)
        {
            System.Collections.Generic.List<ulong> iToRender;
            using (var iChildren = toRender.Children) iToRender = new System.Collections.Generic.List<ulong>(iChildren);
            var iSingleLine = false;
            if (iToRender.Count == 1)
            {
                var iItem = Brain.Current[iToRender[0]];
                iSingleLine = iItem is Statement || iItem is Assignment || iItem is ResultStatement
                              || iItem is ExpressionsBlock;
            }

            fIndent++;
            if (iSingleLine == false || allowSingleLine == false)
            {
                WriteLine("{");
            }
            else
            {
                Indent();
            }

            if (toRender != null)
            {
                try
                {
                    for (var i = 0; i < iToRender.Count; i++)
                    {
                        if (i > 0)
                        {
                            WriteLine(string.Empty);
                        }

                        var u = Brain.Current[iToRender[i]];
                        WriteCodeItem(u);
                        if (!(u is ConditionalStatement) && !(u is LockExpression))
                        {
                            // a conditional closes with a } (it's an inner block), not a code item, so don't close with a ;
                            fTarget.Write(";");
                        }

                        WriteDescription(iToRender[i]);
                    }
                }
                finally
                {
                    fIndent--;
                    if (iSingleLine == false)
                    {
                        WriteLine(string.Empty);
                    }
                }
            }
            else
            {
                fIndent--;
            }

            if (iSingleLine == false || allowSingleLine == false)
            {
                fTarget.Write("}");
            }
        }

        /// <summary>writes the item, starts a new line and performs the correct amount of
        ///     indent.</summary>
        /// <param name="p"></param>
        private void WriteLine(string p)
        {
            fTarget.WriteLine(p);
            for (var i = 0; i < fIndent; i++)
            {
                fTarget.Write("   ");
            }
        }

        /// <summary>
        ///     writes spaces to indent the curren line.
        /// </summary>
        private void Indent()
        {
            fTarget.Write("   ");
        }

        /// <summary>The write code item.</summary>
        /// <param name="u">The u.</param>
        private void WriteCodeItem(Neuron u)
        {
            if (u is Assignment)
            {
                writeAssignment((Assignment)u);
            }
            else if (u is BoolExpression)
            {
                fTarget.Write("(");
                WriteBoolExp((BoolExpression)u);
                fTarget.Write(")");
            }
            else if (u is ByRefExpression)
            {
                WriteByRef((ByRefExpression)u);
            }
            else if (u is ConditionalExpression)
            {
                WriteCondExp((ConditionalExpression)u);
            }
            else if (u is ConditionalStatement)
            {
                WriteCondStat((ConditionalStatement)u);
            }
            else if (u is LockExpression)
            {
                WriteLock((LockExpression)u);
            }
            else if (u is ExpressionsBlock)
            {
                WriteCallExpBlock((ExpressionsBlock)u);
            }
            else if (u is Statement)
            {
                WriteStatement((Statement)u);
            }
            else if (u is ResultStatement)
            {
                WriteStatement((ResultStatement)u);
            }
            else if (u is Variable)
            {
                WriteVar((Variable)u);
            }
            else
            {
                writeStatic(u);
            }
        }

        /// <summary>The write bool cond.</summary>
        /// <param name="u">The u.</param>
        private void WriteBoolCond(Neuron u)
        {
            if (u is BoolExpression)
            {
                WriteBoolExp((BoolExpression)u);
            }
            else if (u is ByRefExpression)
            {
                WriteByRef((ByRefExpression)u);
            }
            else if (u is ExpressionsBlock)
            {
                WriteCallExpBlock((ExpressionsBlock)u);
            }
            else if (u is Statement)
            {
                WriteStatement((Statement)u);
            }
            else if (u is ResultStatement)
            {
                WriteStatement((ResultStatement)u);
            }
            else if (u is Variable)
            {
                WriteVar((Variable)u);
            }
            else
            {
                writeStatic(u);
            }
        }

        /// <summary>The write statement.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteStatement(ResultStatement toRender)
        {
            if (RenderArithmetic(toRender) == false)
            {
                var iName = GetInstructionName(toRender.Instruction);
                if (iName == "preparelocal")
                {
                    // it's a type decleration.
                    System.Collections.Generic.List<Variable> iToInit;
                    using (var iChildren = toRender.ArgumentsCluster.Children) iToInit = iChildren.ConvertTo<Variable>();
                    foreach (var i in iToInit)
                    {
                        if (i != iToInit[0])
                        {
                            fTarget.Write("; ");
                        }

                        if (i.CanGetInt())
                        {
                            fTarget.Write("int ");
                        }
                        else if (i.CanGetDouble())
                        {
                            fTarget.Write("double ");
                        }
                        else if (i.CanGetBool())
                        {
                            fTarget.Write("bool ");
                        }
                        else
                        {
                            fTarget.Write("var ");
                        }

                        WriteVar(i);
                    }
                }
                else
                {
                    fTarget.Write(iName);

                        // remove the instruction at the end so we can render hte name of the instruction.
                    WriteStatementArgs(toRender.ArgumentsCluster, ", ");
                }
            }
        }

        /// <summary>The get instruction name.</summary>
        /// <param name="inst">The inst.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetInstructionName(Instruction inst)
        {
            var iName = inst.GetType().Name.Replace("Instruction", string.Empty);
            return iName.Replace("instruction", string.Empty);
        }

        /// <summary>The write statement.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteStatement(Statement toRender)
        {
            var iName = GetInstructionName(toRender.Instruction);
            fTarget.Write(iName); // remove the instruction at the end so we can render hte name of the instruction.
            WriteStatementArgs(toRender.ArgumentsCluster, ", ");
        }

        /// <summary>checks if the instruction is arithimetic, if so, it's rendered
        ///     differently, for eas of reading.</summary>
        /// <param name="toRender"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool RenderArithmetic(ResultStatement toRender)
        {
            Instruction iInst = toRender.Instruction;
            string iOp = null;
            if (iInst.ID == (ulong)PredefinedNeurons.AdditionInstruction)
            {
                iOp = " + ";
            }
            else if (iInst.ID == (ulong)PredefinedNeurons.MinusInstruction)
            {
                iOp = " - ";
            }
            else if (iInst.ID == (ulong)PredefinedNeurons.DivideInstruction)
            {
                iOp = " / ";
            }
            else if (iInst.ID == (ulong)PredefinedNeurons.MultiplyInstruction)
            {
                iOp = " * ";
            }
            else if (iInst.ID == (ulong)PredefinedNeurons.ModulusInstruction)
            {
                iOp = " % ";
            }

            if (iOp != null)
            {
                WriteStatementArgs(toRender.ArgumentsCluster, iOp);
                return true;
            }

            return false;
        }

        /// <summary>renders the arguments of a statement.</summary>
        /// <param name="toRender">the list of items to render.</param>
        /// <param name="between">the string to put between the items (comma, arithmetic operator)</param>
        private void WriteStatementArgs(NeuronCluster toRender, string between)
        {
            System.Collections.Generic.List<Neuron> iList;
            fTarget.Write("(");
            if (toRender != null && toRender.ChildrenIdentifier != null)
            {
                using (var iChildren = toRender.Children) iList = iChildren.ConvertTo<Neuron>();
                for (var i = 0; i < iList.Count; i++)
                {
                    var u = iList[i];
                    if (i > 0)
                    {
                        fTarget.Write(between);
                    }

                    if (u is ByRefExpression)
                    {
                        WriteByRef((ByRefExpression)u);
                    }
                    else if (u is BoolExpression)
                    {
                        WriteBoolExp((BoolExpression)u);
                    }
                    else if (u is Statement)
                    {
                        WriteStatement((Statement)u);
                    }
                    else if (u is ResultStatement)
                    {
                        WriteStatement((ResultStatement)u);
                    }
                    else if (u is Variable)
                    {
                        WriteVar((Variable)u);
                    }
                    else
                    {
                        writeStatic(u);
                    }
                }

                Factories.Default.NLists.Recycle(iList);
            }

            fTarget.Write(")");
        }

        /// <summary>The write static.</summary>
        /// <param name="toRender">The to render.</param>
        private void writeStatic(Neuron toRender)
        {
            if (toRender is IntNeuron)
            {
                fTarget.Write(((IntNeuron)toRender).Value);
            }
            else if (toRender is DoubleNeuron)
            {
                fTarget.Write(
                    ((DoubleNeuron)toRender).Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    // to make certain that floats are rendered as 0.0 and not 0,0 
            }
            else if (toRender is TextNeuron && !(toRender is Sin))
            {
                // sins inherit from textNeuron, need to filter on this, cause we don't want to render the sins as text.
                var iToRender = ((TextNeuron)toRender).Text.Replace("\n", "\\n");
                iToRender = iToRender.Replace("\r", "\\r");
                iToRender = iToRender.Replace("\t", "\\t");
                ulong iInDict;
                if (TextSin.Words.TryGetID(((TextNeuron)toRender).Text, out iInDict) == false
                    || iInDict != toRender.ID)
                {
                    fTarget.Write("\"{0}\"", iToRender);
                }
                else
                {
                    fTarget.Write("'{0}'", iToRender);
                }
            }
            else
            {
                if (toRender is Instruction)
                {
                    fTarget.Write(GetInstructionName((Instruction)toRender));
                }
                else
                {
                    if (fToRender.Contains(toRender) == false && fRenderAlso.Contains(toRender) == false)
                    {
                        // variables need to be declared somewhere, so make certain that they are rendered.
                        fRenderAlso.Add(toRender);
                    }

                    fTarget.Write(GetName(toRender.ID));
                }
            }
        }

        /// <summary>The write call exp block.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteCallExpBlock(ExpressionsBlock toRender)
        {
            fTarget.Write("{0}()", GetName(toRender.ID));
            if (fToRender.Contains(toRender) == false && fRenderAlso.Contains(toRender) == false)
            {
                // expression blocks need to be declared somewhere, so make certain that they are rendered.
                fRenderAlso.Add(toRender);
            }
        }

        /// <summary>The write var.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteVar(Variable toRender)
        {
            if (fToRender.Contains(toRender) == false && fRenderAlso.Contains(toRender) == false)
            {
                // variables need to be declared somewhere, so make certain that they are rendered.
                fRenderAlso.Add(toRender);
            }

            fTarget.Write(GetName(toRender.ID));
        }

        /// <summary>The write lock.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteLock(LockExpression toRender)
        {
            fTarget.Write("lock(");
            System.Collections.Generic.IList<Neuron> iNeurons = toRender.NeuronsToLock;
            if (iNeurons != null)
            {
                for (var i = 0; i < iNeurons.Count; i++)
                {
                    if (i > 0)
                    {
                        fTarget.Write(", ");
                    }

                    WriteCodeItem(iNeurons[i]);
                }
            }

            iNeurons = toRender.LinksToLock;
            if (iNeurons != null && iNeurons.Count > 0)
            {
                fTarget.Write("; ");
                for (var i = 0; i < iNeurons.Count; i++)
                {
                    if (i > 0)
                    {
                        fTarget.Write(", ");
                    }

                    WriteCodeItem(iNeurons[i]);
                }
            }

            WriteLine(")");
            WriteCodeBlock(toRender.StatementsCluster);
        }

        /// <summary>The write cond stat.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteCondStat(ConditionalStatement toRender)
        {
            string iName;
            if (fCurrentPath.Contains(toRender) == false)
            {
                fPrevWriters.Push(fTarget);

                    // we create a new temp target, so that we can check if this conditional needs a name, if so, render it and then render the rest of the string. + we also check this way if it needs to be rendered or not (when the name was already rendered, don't render again).
                var iStr = new System.Text.StringBuilder();
                fTarget = new System.IO.StringWriter(iStr);
                fCurrentPath.Add(toRender);
                try
                {
                    WriteCondStatFull(toRender);
                }
                finally
                {
                    fCurrentPath.Remove(toRender);
                    fTarget = fPrevWriters.Pop();
                }

                if (fNamesToRender.TryGetValue(toRender.ID, out iName))
                {
                    fNamesToRender.Remove(toRender.ID);
                    if (fRenderedObjects.Contains(iName) == false)
                    {
                        // if the object has already been labeled (and rendered) somewhere else, simply use the label, don't try to regenerate.
                        fTarget.Write(":{0} ", iName);
                        fRenderedObjects.Add(iName);
                        fTarget.Write(iStr.ToString());
                    }
                    else
                    {
                        WriteLine(string.Format("{0};", iName));
                    }
                }
                else
                {
                    fTarget.Write(iStr.ToString());
                }
            }
            else
            {
                iName = GetName(toRender.ID);
                if (fNamesToRender.ContainsKey(toRender.ID) == false)
                {
                    fNamesToRender.Add(toRender.ID, iName);
                }

                fTarget.Write("{0};", iName);
            }
        }

        /// <summary>The write cond stat full.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteCondStatFull(ConditionalStatement toRender)
        {
            var iLoopStyle = toRender.LoopStyle;
            var iCased = false;
            if (iLoopStyle.ID == (ulong)PredefinedNeurons.Until)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                WriteLine("do");
                WriteCondExp(iList[0]);
                fTarget.Write(" while (");
                WriteBoolCond(iList[0].Condition);
                WriteLine(")");
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.ForEach)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("foreach (");
                WriteBoolCond(toRender.LoopItem);
                fTarget.Write(" in ");
                WriteBoolCond(iList[0].Condition);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.QueryLoop)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("select (");
                WriteBoolCond(iList[0].Condition);
                fTarget.Write(" from ");
                WriteBoolCond(toRender.LoopItem);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.QueryLoopChildren)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("select (");
                WriteBoolCond(iList[0].Condition);
                fTarget.Write(" from child ");
                WriteBoolCond(toRender.LoopItem);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.QueryLoopClusters)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("select (");
                WriteBoolCond(iList[0].Condition);
                fTarget.Write(" from cluster ");
                WriteBoolCond(toRender.LoopItem);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.QueryLoopIn)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("select (");
                WriteBoolCond(iList[0].Condition);
                fTarget.Write(" from in ");
                WriteBoolCond(toRender.LoopItem);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else if (iLoopStyle.ID == (ulong)PredefinedNeurons.QueryLoopOut)
            {
                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                fTarget.Write("select (");
                WriteBoolCond(iList[0].Condition);
                fTarget.Write(" from out ");
                WriteBoolCond(toRender.LoopItem);
                WriteLine(")");
                WriteForEachExp(toRender.Conditions[0]);
            }
            else
            {
                var iCondText = string.Empty;
                if (iLoopStyle.ID == (ulong)PredefinedNeurons.Looped)
                {
                    fTarget.Write("while");
                    iCondText = "while";
                }
                else if (iLoopStyle.ID == (ulong)PredefinedNeurons.Case)
                {
                    iCased = true;
                    fTarget.Write("switch (");
                    WriteBoolCond(toRender.CaseItem);
                    WriteLine(")");
                    fIndent++;
                    WriteLine("{");
                }
                else if (iLoopStyle.ID == (ulong)PredefinedNeurons.CaseLooped)
                {
                    iCased = true;
                    fTarget.Write("switch while(");
                    WriteBoolCond(toRender.CaseItem);
                    WriteLine(")");
                    fIndent++;
                    WriteLine("{");
                }
                else if (iLoopStyle.ID == (ulong)PredefinedNeurons.Normal)
                {
                    fTarget.Write("if");
                    iCondText = "if";
                }

                System.Collections.Generic.IList<ConditionalExpression> iList = toRender.Conditions;
                for (var i = 0; i < iList.Count; i++)
                {
                    if (i > 0)
                    {
                        WriteLine(string.Empty);
                    }

                    var iExp = iList[i];
                    WriteCondExp(iExp, i, iCased, iCondText);
                }

                if (iLoopStyle.ID == (ulong)PredefinedNeurons.CaseLooped
                    || iLoopStyle.ID == (ulong)PredefinedNeurons.Case)
                {
                    fIndent--;
                    WriteLine(string.Empty); // the code block doesn't do that at the end, so that the indenting can be adjusted.
                    WriteLine("}");
                }
            }
        }

        /// <summary>a conditional expression starts with an 'else' if it is not the first
        ///     + case items are written differently.</summary>
        /// <param name="toRender"></param>
        /// <param name="index"></param>
        /// <param name="asCase">The as Case.</param>
        /// <param name="condText">The cond Text.</param>
        private void WriteCondExp(
            ConditionalExpression toRender, 
            int index = 0, 
            bool asCase = false, 
            string condText = "")
        {
            string iName;
            if (fCurrentPath.Contains(toRender) == false)
            {
                fPrevWriters.Push(fTarget);

                    // we create a new temp target, so that we can check if this conditional needs a name, if so, render it and then render the rest of the string.
                var iStr = new System.Text.StringBuilder();
                fTarget = new System.IO.StringWriter(iStr);
                fCurrentPath.Add(toRender);
                try
                {
                    if (toRender.Condition != null)
                    {
                        // could be empty condition (at the end).
                        if (asCase == false)
                        {
                            if (index != 0)
                            {
                                fTarget.Write("else {0}(", condText);
                            }
                            else
                            {
                                fTarget.Write(" (");
                            }

                            WriteBoolCond(toRender.Condition);
                            WriteLine(")");
                        }
                        else
                        {
                            fTarget.Write("case ");
                            WriteBoolCond(toRender.Condition);
                            WriteLine(":");
                        }
                    }
                    else
                    {
                        if (asCase == false)
                        {
                            WriteLine("else");
                        }
                        else
                        {
                            WriteLine("default:");
                        }
                    }

                    WriteCodeBlock(toRender.StatementsCluster);
                }
                finally
                {
                    fCurrentPath.Remove(toRender);
                    fTarget = fPrevWriters.Pop();
                }

                if (fNamesToRender.TryGetValue(toRender.ID, out iName))
                {
                    fNamesToRender.Remove(toRender.ID);
                    if (fRenderedObjects.Contains(iName) == false)
                    {
                        // if the object has already been labeled (and rendered) somewhere else, simply use the label, don't try to regenerate.
                        fTarget.Write(":{0} ", iName);
                        fRenderedObjects.Add(iName);
                        fTarget.Write(iStr.ToString());
                    }
                    else
                    {
                        WriteLine(string.Format("{0};", iName));
                    }
                }
                else
                {
                    fTarget.Write(iStr.ToString());
                }
            }
            else
            {
                iName = GetName(toRender.ID);
                if (fNamesToRender.ContainsKey(toRender.ID) == false)
                {
                    fNamesToRender.Add(toRender.ID, iName);
                }

                fTarget.Write("{0};", iName);
            }
        }

        /// <summary>a conditional expression starts with an 'else' if it is not the first
        ///     + case items are written differently.</summary>
        /// <param name="toRender"></param>
        private void WriteForEachExp(ConditionalExpression toRender)
        {
            string iName;
            if (fCurrentPath.Contains(toRender) == false)
            {
                fPrevWriters.Push(fTarget);

                    // we create a new temp target, so that we can check if this conditional needs a name, if so, render it and then render the rest of the string.
                var iStr = new System.Text.StringBuilder();
                fTarget = new System.IO.StringWriter(iStr);
                fCurrentPath.Add(toRender);
                try
                {
                    WriteCodeBlock(toRender.StatementsCluster);
                }
                finally
                {
                    fCurrentPath.Remove(toRender);
                    fTarget = fPrevWriters.Pop();
                }

                if (fNamesToRender.TryGetValue(toRender.ID, out iName))
                {
                    fNamesToRender.Remove(toRender.ID);
                    fTarget.Write(":{0} ", iName);
                }

                fTarget.Write(iStr.ToString());
            }
            else
            {
                iName = GetName(toRender.ID);
                if (fNamesToRender.ContainsKey(toRender.ID) == false)
                {
                    fNamesToRender.Add(toRender.ID, iName);
                }

                fTarget.Write("{0};", iName);
            }
        }

        /// <summary>The write by ref.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteByRef(ByRefExpression toRender)
        {
            fTarget.Write("ref(");
            WriteCodeItem(toRender.Argument);
            fTarget.Write(")");
        }

        /// <summary>The write bool exp.</summary>
        /// <param name="toRender">The to render.</param>
        private void WriteBoolExp(BoolExpression toRender)
        {
            WriteCodeItem(toRender.LeftPart);
            fTarget.Write(" " + fDict.GetName(toRender.Operator.ID) + " ");

                // for the operator, we use the exact string representation, which is the correct one, if we use 'this.GetName', the signs are converted to text cause they need to be read as names, this as operators.
            WriteCodeItem(toRender.RightPart);
        }

        /// <summary>The write assignment.</summary>
        /// <param name="toRender">The to render.</param>
        private void writeAssignment(Assignment toRender)
        {
            WriteCodeItem(toRender.LeftPart);
            fTarget.Write(" = ");
            WriteCodeItem(toRender.RightPart);
        }
    }
}