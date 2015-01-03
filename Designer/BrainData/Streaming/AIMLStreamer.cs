// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AIMLStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   provides support for reading aiml files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     provides support for reading aiml files.
    /// </summary>
    public class AIMLStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>The aimlsourcename.</summary>
        private const string AIMLSOURCENAME = "AIML reader";

        /// <summary>
        ///     keeps track of the nr of conditions found in a single,
        /// </summary>
        private int fCondCount;

        /// <summary>The f current rule name.</summary>
        private string fCurrentRuleName; // so we can build nice error messages.

        /// <summary>
        ///     used to build up all the 'do' patterns for a single category.
        /// </summary>
        private System.Text.StringBuilder fDo;

        /// <summary>The f file name.</summary>
        private string fFileName;

        /// <summary>
        ///     keeps track of the nr of gender elements found in a single category
        /// </summary>
        private int fGenderCount;

        /// <summary>
        ///     so we know when we are in a 'think' element. Used for the 'set'
        ///     instruction -> when in think, it doesn't need to return a value.
        ///     Otherwise it does.
        /// </summary>
        private bool fInThink;

        /// <summary>
        ///     keeps track of the nr of times a 'learn' section was found inside a
        ///     template.
        /// </summary>
        private int fLearnCount;

        /// <summary>
        ///     used to build up the current output pattern (can be nested:
        ///     conditionals and the likes can overwrite the value with a local copy
        ///     so they can build their own output.
        /// </summary>
        private System.Text.StringBuilder fOutput;

        /// <summary>
        ///     Store all the output patterns in here.
        /// </summary>
        private NeuronCluster fOutputPatterns;

        /// <summary>
        ///     keeps track of the nr of person(2) elements found in a single category
        /// </summary>
        private int fPersonCount;

        /// <summary>The f prev was path.</summary>
        private bool fPrevWasPath;

                     // keeps track if the previous statement that was written to the output was a binding path.

        /// <summary>
        ///     keeps track of the nr of times a 'random' element was found inside a
        ///     template.
        /// </summary>
        private int fRandomCount;

        /// <summary>The f reader.</summary>
        private System.Xml.XmlReader fReader; // the reader that has all the settings correct.

        /// <summary>The fresults.</summary>
        private System.Collections.Generic.IList<TextPatternEditor> fresults;

        /// <summary>
        ///     the rule we are currently reaeding.
        /// </summary>
        private NeuronCluster fRule;

        /// <summary>
        ///     the set instruction can return a value, which depends on how the
        ///     predicate is defined: it can either return the name or the value of
        ///     the predicate (asset value). to return this value and insert it in the
        ///     output, we need a var, so a counter.
        /// </summary>
        private int fSetCount;

        /// <summary>
        ///     keeps track of the nr of srai elements in a template, so we can
        ///     generate original names.
        /// </summary>
        private int fSrCount;

        /// <summary>The f text reader.</summary>
        private System.Xml.XmlTextReader fTextReader; // the same reader, but provides line position info.

        /// <summary>
        ///     the topic we are currently reading.
        /// </summary>
        private NeuronCluster fTopic;

        /// <summary>The f topic name.</summary>
        private string fTopicName; // the name of the topic, so we can return it.

        /// <summary>
        ///     all the rules in the current topic, for getRule. This is a speed-up
        ///     operation.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<string, ulong> fRules =
            new System.Collections.Generic.Dictionary<string, ulong>();

        /// <summary>
        ///     gets the cluster that stores the output pattern. If there was a 'that'
        ///     element, this list is already created. Otherwise, we need to create
        ///     one and attach to the rule.s
        /// </summary>
        internal NeuronCluster OutputPatterns
        {
            get
            {
                if (fOutputPatterns == null)
                {
                    fOutputPatterns = BrainHelper.MakeCluster((ulong)PredefinedNeurons.TextPatternOutputs);
                    Link.Create(fRule, fOutputPatterns, (ulong)PredefinedNeurons.TextPatternOutputs);
                }

                return fOutputPatterns;
            }
        }

        /// <summary>Imports the specified file and returns the topic.</summary>
        /// <param name="fileName">The file Name.</param>
        /// <param name="results">The results.</param>
        /// <param name="tracker">The tracker.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool Import(
            string fileName, System.Collections.Generic.IList<TextPatternEditor> results, 
            Search.ProcessTrackerItem tracker)
        {
            var iStreamer = new AIMLStreamer();
            var iErrors = false;
            try
            {
                iStreamer.fresults = results;
                iStreamer.fFileName = fileName;
                iStreamer.Tracker = new PosTracker { Tracker = tracker };
                using (
                    var iFile = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    if (iStreamer.Tracker != null)
                    {
                        iStreamer.Tracker.Stream = iFile; // we need this to know where in the file we are exactly.
                    }

                    var iSettings = new System.Xml.XmlReaderSettings();
                    iSettings.IgnoreComments = true;
                    iSettings.IgnoreProcessingInstructions = true;
                    iSettings.IgnoreWhitespace = false;

                    var iTemp = new System.IO.StreamReader(iFile, System.Text.Encoding.GetEncoding("ISO-8859-1"), true);

                        // we need this for the encoding (some aiml files have some special signs that require this encoding.
                    iStreamer.fTextReader = new System.Xml.XmlTextReader(iTemp); // the textreader provides location info
                    using (var iReader = System.Xml.XmlReader.Create(iStreamer.fTextReader, iSettings))
                    {
                        // we need the xml reader to perform the actual reading.
                        iStreamer.fReader = iReader;
                        if (iReader.IsStartElement())
                        {
                            iStreamer.ReadXml(iReader);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(AIMLSOURCENAME, e.Message);
                iErrors = false;
            }

            return iErrors;
        }

        /// <summary>writes to the output stream, taking into account that the previous
        ///     item might be a path and the current item can start with a '.'</summary>
        /// <param name="value"></param>
        /// <param name="isPath"></param>
        private void WriteToOut(string value, bool isPath = false)
        {
            var iTrimmed = value.Trim();

                // remove any spaces befor checking if we need to add an escape, cause the spaces ares skipped by the parser
            if (fPrevWasPath
                && (iTrimmed.StartsWith(".") || iTrimmed.StartsWith(":") || iTrimmed.StartsWith("=")
                    || iTrimmed.StartsWith("+") || iTrimmed.StartsWith("*") || iTrimmed.StartsWith("[")
                    || iTrimmed.StartsWith("]") || iTrimmed.StartsWith("->") || iTrimmed.StartsWith("-")
                    || iTrimmed.StartsWith("%") || iTrimmed.StartsWith("/")
                    || (value.Length > 0 && char.IsWhiteSpace(value[0]) == false && fOutput.Length > 0
                        && char.IsWhiteSpace(fOutput[fOutput.Length - 1]) == false)))
            {
                // if the string starts with a letter and the previous also, make certain that there is a divide between the 2.
                if (iTrimmed.StartsWith("#") || iTrimmed.StartsWith("$") || iTrimmed.StartsWith("^")
                    || iTrimmed.StartsWith("~"))
                {
                    // when the next item is a new path, don't need to render \
                    fOutput.Append(value);
                }
                else
                {
                    fOutput.AppendFormat("\\{0}", value);
                }
            }
            else
            {
                fOutput.Append(value);
            }

            fPrevWasPath = isPath;
        }

        #region internal types

        /// <summary>
        ///     <para>
        ///         contains a single child node of the template node. This is used to
        ///         read in the entire template content
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 by 1, so we can see if there are multiple children for the template
        ///                 and which ones (if it's a think + random/conditional, the
        ///                 random-conditional is still a root). This allows us ot render less
        ///                 'out' rules that can't be reached by patterns.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        private class TemplateContentItem
        {
            /// <summary>Gets or sets the name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the content.</summary>
            public string Content { get; set; }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (true)
            {
                SkipSpaces(); // can be spaces here.
                var iName = reader.Name.ToLower();
                if (iName == "topic")
                {
                    ReadTopic();
                }
                else if (iName == "category")
                {
                    ReadCategory();
                }
                else
                {
                    break;
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>The skip spaces.</summary>
        private void SkipSpaces()
        {
            while (fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
            {
                fReader.Read();
            }
        }

        /// <summary>searches for a rule with the specified name in the current topic.</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster FindRule(string value)
        {
            ulong iRes;
            if (fRules.TryGetValue(value, out iRes))
            {
                return Brain.Current[iRes] as NeuronCluster;

                    // this is the least common situation, so make that the slowest.
            }

            return null;
        }

        /// <summary>The read category.</summary>
        private void ReadCategory()
        {
            if (CheckCancelAndPos())
            {
                return;
            }

            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            if (fTopic == null)
            {
                // if there is not 'top' topic, create one
                PrepareTopic(System.IO.Path.GetFileNameWithoutExtension(fFileName) + " - no filter");
            }

            SkipSpaces();
            fOutputPatterns = null; // make certain that this is reset so that it has a correct state.
            fRandomCount = fGenderCount = fPersonCount = fSetCount = fCondCount = fSrCount = 0;

                // reset this value for every category.
            var iPatternTxt = ReadPattern();
            fCurrentRuleName = iPatternTxt;
            fRule = FindRule(iPatternTxt);
            if (fRule == null)
            {
                BuildRule(iPatternTxt);
                fRules.Add(iPatternTxt, fRule.ID);

                    // we do this here, not in BuildRule, cause that is also used for auto-rendered rules (for output sections),which we don't need to search for.
                iPatternTxt = ReplaceVars(iPatternTxt, "v");
                var iPattern = NeuronFactory.GetText(iPatternTxt);
                Brain.Current.Add(iPattern);
                using (var iChildren = fRule.ChildrenW)

                    // add the pattern to the rule. Only needs to be done 1 time, cause the same aiml pattern always translates to the same nnl pattern.
                    iChildren.Add(iPattern);
                ParseInput(iPattern, null);
            }

            SkipSpaces();
            ReadThat();
            SkipSpaces();
            ReadTemplate();
            SkipSpaces();
            fReader.ReadEndElement();
            SkipSpaces();
        }

        /// <summary>reads the pattern definition, which can contain 'bot' and 'user' asset
        ///     paths.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadPattern()
        {
            string iRes;

            fOutput = new System.Text.StringBuilder();

                // reset, don't need to store any previous value, this is always a root element.
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return string.Empty;
            }

            SkipSpaces();

            while (true)
            {
                SkipSpaces();
                var iName = fReader.Name.ToLower();
                if (fReader.NodeType == System.Xml.XmlNodeType.Text
                    || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
                {
                    ReadOuputText();
                }
                else if ((iName == "pattern" && fReader.NodeType == System.Xml.XmlNodeType.EndElement) || fReader.EOF)
                {
                    break;
                }
                else if (iName == "get")
                {
                    ReadGet();
                }
                else if (iName == "bot")
                {
                    ReadBot();
                }
                else
                {
                    ReadCustomElement();
                }
            }

            fReader.ReadEndElement();
            iRes = fOutput.ToString();
            fOutput = null;
            return iRes;
        }

        /// <summary>creates a rule and assigns it to<see cref="JaStDev.HAB.Designer.AIMLStreamer.fRule"/></summary>
        /// <param name="name"></param>
        private void BuildRule(string name)
        {
            fRule = NeuronFactory.GetCluster();
            Brain.Current.Add(fRule);
            fRule.Meaning = (ulong)PredefinedNeurons.PatternRule;
            using (var iChildren = fTopic.ChildrenW) iChildren.Add(fRule);
            var iNameOfRule = BrainHelper.GetNeuronForText(name);
            Link.Create(fRule, iNameOfRule, (ulong)PredefinedNeurons.NameOfMember);
            BrainData.Current.NeuronInfo[fRule].SetTitleNoUndo(name); // so tha the deigner is also correct.
        }

        /// <summary>
        ///     reads a pattern-that.
        /// </summary>
        private void ReadThat()
        {
            if (fReader.Name.ToLower() == "that")
            {
                var iThatTxt = XmlStore.ReadElNoCase<string>(fReader, "that");
                iThatTxt = ReplaceVars(iThatTxt, "t");
                var iThat = NeuronFactory.GetText(iThatTxt); // the pattern isn't added to the dict, it is compiled.
                Brain.Current.Add(iThat);

                var iResponseForOutputs =
                    fRule.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
                if (iResponseForOutputs == null)
                {
                    iResponseForOutputs = BrainHelper.MakeCluster((ulong)PredefinedNeurons.ResponseForOutputs);
                    Link.Create(fRule, iResponseForOutputs, (ulong)PredefinedNeurons.ResponseForOutputs);
                }

                fOutputPatterns = BrainHelper.MakeCluster((ulong)PredefinedNeurons.TextPatternOutputs);
                var iCondition = BrainHelper.MakeCluster((ulong)PredefinedNeurons.Condition, fOutputPatterns);
                var iLocalResponseForOutputs = BrainHelper.MakeCluster(
                    (ulong)PredefinedNeurons.ResponseForOutputs, 
                    iThat);
                Link.Create(iCondition, iLocalResponseForOutputs, (ulong)PredefinedNeurons.ResponseForOutputs);
                using (IDListAccessor iChildren = iResponseForOutputs.ChildrenW) iChildren.Add(iCondition);
                ParseInput(iThat, iResponseForOutputs);
            }
        }

        /// <summary>parses an input pattern and makes certain that the errors are logged.</summary>
        /// <param name="toParse"></param>
        /// <param name="localTo"></param>
        private void ParseInput(TextNeuron toParse, Neuron localTo)
        {
            try
            {
                var iParser = new Parsers.InputParser(toParse, AIMLSOURCENAME, localTo);
                iParser.Parse();
            }
            catch (System.Exception e)
            {
                Parsers.InputParser.RemoveInputPattern(toParse);

                    // when something went wrong, we remove the compilation, cause it's invalid anyway.
                var iSource = string.Format("({0}).({1})", fTopicName, fCurrentRuleName);
                LogService.Log.LogError(iSource, BuildLogText(e.Message));
            }
        }

        /// <summary>adds the filename and location to the <paramref name="text"/> and
        ///     returns it.</summary>
        /// <param name="text"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string BuildLogText(string text)
        {
            return string.Format(
                "L:{1} C:{2}  {0}: {3}", 
                fFileName, 
                fTextReader.LineNumber, 
                fTextReader.LinePosition, 
                text);
        }

        /// <summary>
        ///     Reads a template and transforms it into do and output patterns.
        /// </summary>
        private void ReadTemplate()
        {
            if (fReader.Name.ToLower() == "template")
            {
                fOutput = new System.Text.StringBuilder();

                    // reset, don't need to store any previous value, this is always a root element.
                fDo = new System.Text.StringBuilder(); // same as output.
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iContent = ReadTemplateContentItems("template");
                ProcessTemplateContent(iContent, true);
                fReader.ReadEndElement();
                ProcessOutput();
                ProcessDo();
                fOutput = null;
                fDo = null;
            }
            else
            {
                throw new System.InvalidOperationException("template element expected.");
            }
        }

        /// <summary>Calculates if a root random/conditional is allowed and walks through
        ///     the list of template content-items and reads them in completely.</summary>
        /// <param name="content">The content.</param>
        /// <param name="fromTemplate">when true, called from template and both child cond/random can be
        ///     root, otherwise only random.</param>
        private void ProcessTemplateContent(System.Collections.Generic.List<TemplateContentItem> content, 
            bool fromTemplate = false)
        {
            bool? iAllowRoot = false;
            if (fromTemplate)
            {
                if (content.Count == 1)
                {
                    iAllowRoot = true;
                }
                else if (content.Count == 2 && (content[0].Name == "think" || content[1].Name == "think"))
                {
                    iAllowRoot = true;
                }
            }
            else
            {
                iAllowRoot = null;
            }

            var iPrev = fReader;
            try
            {
                foreach (var i in content)
                {
                    if (i.Name == "text")
                    {
                        ProcessOutputText(i.Content);
                    }
                    else
                    {
                        using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(i.Content)))
                        {
                            var iSettings = new System.Xml.XmlReaderSettings();
                            iSettings.IgnoreComments = true;
                            iSettings.IgnoreProcessingInstructions = true;
                            iSettings.IgnoreWhitespace = false;
                            using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                            {
                                fReader = iReader;
                                fReader.Read(); // read first item.
                                if (iAllowRoot.HasValue)
                                {
                                    ReadTemplateElement(i, iAllowRoot.Value);
                                }
                                else
                                {
                                    ReadTemplateElement(i, i.Name == "random");

                                        // when iAllowRoot is null, we are calling from a condition, which still allows a random to be root, but not a condition
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                fReader = iPrev;
            }
        }

        /// <summary>The read template element.</summary>
        /// <param name="toRead">The to read.</param>
        /// <param name="allowRoot">The allow root.</param>
        private void ReadTemplateElement(TemplateContentItem toRead, bool allowRoot)
        {
            if (fReader.NodeType == System.Xml.XmlNodeType.Text || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
            {
                ProcessOutputText(toRead.Content);
            }
            else if (toRead.Name == "star")
            {
                ReadStar();
            }
            else if (toRead.Name == "thatstar")
            {
                ReadThatStar();
            }
            else if (toRead.Name == "topicstar")
            {
                ReadTopicStar();
            }
            else if (toRead.Name == "that")
            {
                ReadTemplateThat();
            }
            else if (toRead.Name == "srai" || toRead.Name == "sr")
            {
                ReadSrai();
            }
            else if (toRead.Name == "random")
            {
                ReadRandom(allowRoot);
            }
            else if (toRead.Name == "condition")
            {
                ReadCondition(allowRoot);
            }
            else if (toRead.Name == "uppercase")
            {
                ReadUpperCase();
            }
            else if (toRead.Name == "lowercase")
            {
                ReadLowerCase();
            }
            else if (toRead.Name == "formal")
            {
                ReadFormalCase();
            }
            else if (toRead.Name == "sentence")
            {
                ReadSentenceCase();
            }
            else if (toRead.Name == "get")
            {
                ReadGet();
            }
            else if (toRead.Name == "set")
            {
                ReadSet();
            }
            else if (toRead.Name == "gossip")
            {
                ReadGossip();
            }
            else if (toRead.Name == "person")
            {
                ReadPerson();
            }
            else if (toRead.Name == "person2")
            {
                ReadPerson2();
            }
            else if (toRead.Name == "gender")
            {
                ReadGender();
            }
            else if (toRead.Name == "think")
            {
                ReadThink();
            }
            else if (toRead.Name == "learn" || toRead.Name == "learnf")
            {
                ReadLearn(toRead.Name);
            }
            else if (toRead.Name == "system")
            {
                ReadSystem();
            }
            else if (toRead.Name == "javascript")
            {
                ReadJavaScript();
            }
            else if (toRead.Name == "input")
            {
                ReadInput();
            }
            else if (toRead.Name == "bot")
            {
                ReadBot();
            }
            else if (toRead.Name == "date")
            {
                ReadDate();
            }
            else if (toRead.Name == "id")
            {
                ReadId();
            }
            else if (toRead.Name == "size")
            {
                ReadSize();
            }
            else if (toRead.Name == "version")
            {
                ReadVersion();
            }
            else if (toRead.Name == "br")
            {
                // sometimes used, so put an enter for this.
                ReadBreak();
            }
            else if (toRead.Name == "request")
            {
                ReadInput();
            }
            else if (toRead.Name == "response")
            {
                ReadTemplateThat();
            }
            else if (toRead.Name == "explode")
            {
                ReadExplode();
            }
            else if (toRead.Name == "eval")
            {
                LogService.Log.LogError(AIMLSOURCENAME, "eval element not allowed here.");
                fReader.ReadOuterXml(); // skip the entire eval element.
            }
            else if (toRead.Name == "oob")
            {
                ReadOob();
            }
            else
            {
                ReadCustomElement();
            }
        }

        /// <summary>reads all the child elemnets of the template and stores them in a
        ///     buffer so we can do some verifications on the entire content.</summary>
        /// <param name="elName">The el Name.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<TemplateContentItem> ReadTemplateContentItems(string elName)
        {
            var iRes = new System.Collections.Generic.List<TemplateContentItem>();

            while (true)
            {
                SkipSpaces();
                var iName = fReader.Name.ToLower();
                if (fReader.NodeType == System.Xml.XmlNodeType.Text
                    || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
                {
                    var iNew = new TemplateContentItem { Name = "text", Content = fReader.Value };
                    iRes.Add(iNew);
                    fReader.Read(); // advance the cursor.
                }
                else if ((iName == elName && fReader.NodeType == System.Xml.XmlNodeType.EndElement) || fReader.EOF)
                {
                    break;
                }
                else
                {
                    var iNew = new TemplateContentItem { Name = iName, Content = fReader.ReadOuterXml() };
                    if (string.IsNullOrEmpty(iNew.Content))
                    {
                        LogService.Log.LogError(
                            AIMLSOURCENAME, 
                            BuildLogText(
                                "There appears to be an error in the aiml definition somewhere round this line."));
                    }

                    iRes.Add(iNew);
                }
            }

            return iRes;
        }

        /// <summary>The process do.</summary>
        private void ProcessDo()
        {
            if (fDo.Length > 0 && fOutputPatterns != null)
            {
                // don't need to assign a 'do' section if there is currently no output section.
                var iDoTxt = fDo.ToString().Trim();
                var iDo = NeuronFactory.GetText(iDoTxt); // get the output pattern.
                Brain.Current.Add(iDo);
                Link.Create(fOutputPatterns, iDo, (ulong)PredefinedNeurons.DoPatterns);
                try
                {
                    var iParser = new Parsers.DoParser(iDo, AIMLSOURCENAME);
                    iParser.Parse();
                }
                catch (System.Exception e)
                {
                    Parsers.DoParser.RemoveDoPattern(iDo);

                        // when something went wrong, we remove the compilation, cause it's invalid anyway.
                    var iSource = string.Format("({0}).({1})", fTopicName, fCurrentRuleName);
                    LogService.Log.LogError(iSource, BuildLogText(e.Message));
                }
            }
        }

        /// <summary>The process do li.</summary>
        /// <param name="output">The output.</param>
        private void ProcessDoLi(TextNeuron output)
        {
            if (output != null && fDo.Length > 0)
            {
                var iDoTxt = fDo.ToString().Trim();

                    // cut any white space cause to make certain that there are no extra enters at the end.
                var iDo = NeuronFactory.GetText(iDoTxt); // get the output pattern.
                Brain.Current.Add(iDo);
                Link.Create(output, iDo, (ulong)PredefinedNeurons.DoPatterns);
                try
                {
                    var iParser = new Parsers.DoParser(iDo, AIMLSOURCENAME);
                    iParser.Parse();
                }
                catch (System.Exception e)
                {
                    Parsers.DoParser.RemoveDoPattern(iDo);

                        // when something went wrong, we remove the compilation, cause it's invalid anyway.
                    var iSource = string.Format("({0}).({1})", fTopicName, fCurrentRuleName);
                    LogService.Log.LogError(iSource, BuildLogText(e.Message));
                }
            }
        }

        /// <summary>The process output.</summary>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        private TextNeuron ProcessOutput()
        {
            if (fOutput.Length > 0)
            {
                var iOutTxt = fOutput.ToString().Trim();
                var iOut = NeuronFactory.GetText(iOutTxt); // get the output pattern.
                Brain.Current.Add(iOut);
                using (var iChildren = OutputPatterns.ChildrenW)

                    // add to the current output list (either on the rule or the 'that' (represented by a condition). Use the prop, so we always have the correct object.
                    iChildren.Add(iOut);
                try
                {
                    var iParser = new Parsers.OutputParser(iOut, AIMLSOURCENAME);
                    iParser.Parse();
                }
                catch (System.Exception e)
                {
                    Parsers.OutputParser.RemoveOutputPattern(iOut);

                        // when something went wrong, we remove the compilation, cause it's invalid anyway.
                    var iSource = string.Format("({0}).({1}).({2})", fTopicName, fCurrentRuleName, iOutTxt);
                    LogService.Log.LogError(iSource, e.Message);
                }

                return iOut;
            }

            return null;
        }

        /// <summary>reads all the elements from the AIML category 'aiml-template-elements'
        ///     which are all the items that can be contained by a template (and some
        ///     other elements).</summary>
        /// <param name="outer">The outer element. Reading continues untill the end of the specified
        ///     item is found.</param>
        /// <param name="allowEval">if set to <c>true</c> if the eval element is allowed, otherwise false.</param>
        private void ReadTemplateElements(string outer, bool allowEval = false)
        {
            while (true)
            {
                SkipSpaces();
                var iName = fReader.Name.ToLower();
                if (fReader.NodeType == System.Xml.XmlNodeType.Text
                    || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
                {
                    ReadOuputText();
                }
                else if ((iName == outer && fReader.NodeType == System.Xml.XmlNodeType.EndElement) || fReader.EOF)
                {
                    break;
                }
                else if (iName == "star")
                {
                    ReadStar();
                }
                else if (iName == "thatstar")
                {
                    ReadThatStar();
                }
                else if (iName == "topicstar")
                {
                    ReadTopicStar();
                }
                else if (iName == "that")
                {
                    ReadTemplateThat();
                }
                else if (iName == "srai" || iName == "sr")
                {
                    ReadSrai();
                }
                else if (iName == "random")
                {
                    ReadRandom(false);
                }
                else if (iName == "condition")
                {
                    ReadCondition(false);
                }
                else if (iName == "uppercase")
                {
                    ReadUpperCase();
                }
                else if (iName == "lowercase")
                {
                    ReadLowerCase();
                }
                else if (iName == "formal")
                {
                    ReadFormalCase();
                }
                else if (iName == "sentence")
                {
                    ReadSentenceCase();
                }
                else if (iName == "get")
                {
                    ReadGet();
                }
                else if (iName == "set")
                {
                    ReadSet();
                }
                else if (iName == "gossip")
                {
                    ReadGossip();
                }
                else if (iName == "person")
                {
                    ReadPerson();
                }
                else if (iName == "person2")
                {
                    ReadPerson2();
                }
                else if (iName == "gender")
                {
                    ReadGender();
                }
                else if (iName == "think")
                {
                    ReadThink();
                }
                else if (iName == "learn" || iName == "learnf")
                {
                    ReadLearn(iName);
                }
                else if (iName == "system")
                {
                    ReadSystem();
                }
                else if (iName == "javascript")
                {
                    ReadJavaScript();
                }
                else if (iName == "input")
                {
                    ReadInput();
                }
                else if (iName == "bot")
                {
                    ReadBot();
                }
                else if (iName == "date")
                {
                    ReadDate();
                }
                else if (iName == "id")
                {
                    ReadId();
                }
                else if (iName == "size")
                {
                    ReadSize();
                }
                else if (iName == "version")
                {
                    ReadVersion();
                }
                else if (iName == "br")
                {
                    // sometimes used, so put an enter for this.
                    ReadBreak();
                }
                else if (iName == "request")
                {
                    ReadInput();
                }
                else if (iName == "response")
                {
                    ReadTemplateThat();
                }
                else if (iName == "explode")
                {
                    ReadExplode();
                }
                else if (iName == "eval")
                {
                    if (allowEval == false)
                    {
                        LogService.Log.LogError(AIMLSOURCENAME, "eval element not allowed here.");
                        fReader.ReadOuterXml(); // skip the entire eval element.
                    }
                    else
                    {
                        WriteToOut(ReadEval(), fPrevWasPath);
                    }
                }
                else if (iName == "oob")
                {
                    ReadOob();
                }
                else
                {
                    ReadCustomElement();
                }
            }
        }

        /// <summary>
        ///     reads oob (out of band) elements (for android commands). temporarely
        ///     skip content.
        /// </summary>
        private void ReadOob()
        {
            LogService.Log.LogWarning(AIMLSOURCENAME, "oob elements are not yet supported, skipping content.");
            fReader.ReadOuterXml();
        }

        /// <summary>The read custom element.</summary>
        private void ReadCustomElement()
        {
            var iPrevOut = fOutput;
            fOutput = new System.Text.StringBuilder(string.Empty);
            try
            {
                var iName = fReader.Name;
                var wasEmpty = fReader.IsEmptyElement;
                if (wasEmpty == false)
                {
                    fOutput.AppendFormat("<{0}>", fReader.Name);
                }
                else
                {
                    fOutput.AppendFormat("<{0}", fReader.Name);
                }

                if (fReader.MoveToFirstAttribute())
                {
                    fOutput.AppendFormat(" {0}='{1}'", fReader.Name, fReader.Value);
                    while (fReader.MoveToNextAttribute())
                    {
                        fOutput.AppendFormat(" {0}='{1}'", fReader.Name, fReader.Value);
                    }
                }

                fReader.Read();
                if (wasEmpty)
                {
                    fOutput.Append("/>");
                }
                else
                {
                    ReadTemplateElements(iName.ToLower());
                    fOutput.AppendFormat("</{0}>", iName);
                    fReader.ReadEndElement();
                }

                iPrevOut.Append(fOutput);
            }
            finally
            {
                fOutput = iPrevOut;
            }
        }

        /// <summary>The read response.</summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ReadResponse()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The read break.</summary>
        private void ReadBreak()
        {
            var wasEmpty = fReader.IsEmptyElement;
            WriteToOut("\n");
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>
        ///     Reads the text content of an output element.
        /// </summary>
        private void ReadOuputText()
        {
            if (fOutput == null)
            {
                LogService.Log.LogError(
                    AIMLSOURCENAME, 
                    BuildLogText("text element can't be processed: don't know where to add the value!"));
            }
            else
            {
                ProcessOutputText(fReader.Value);
            }

            fReader.Read(); // move to the next item.
        }

        /// <summary>The process output text.</summary>
        /// <param name="value">The value.</param>
        private void ProcessOutputText(string value)
        {
            var iToWrite = value.Replace("$", "\\$");

                // need to make certain that all the binding items are escaped, cause when used in AIML, the text was expected, not a binding.
            iToWrite = iToWrite.Replace("^", "\\^").Replace("~", "\\~").Replace("#", "\\#");
            WriteToOut(iToWrite);
        }

        /// <summary>The read version.</summary>
        private void ReadVersion()
        {
            var wasEmpty = fReader.IsEmptyElement;
            WriteToOut("~version.FullVersion:Render", true);
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read explode.</summary>
        private void ReadExplode()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            var iToExplode = fReader.Value; // the text value between the 2 explode parts.
            WriteToOut(string.Format("$Explode(L({0}))", iToExplode), true);
            fReader.ReadEndElement();
        }

        /// <summary>The read size.</summary>
        private void ReadSize()
        {
            var wasEmpty = fReader.IsEmptyElement;
            WriteToOut("~RuleCount", true);
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read id.</summary>
        private void ReadId()
        {
            var wasEmpty = fReader.IsEmptyElement;
            WriteToOut("#user.id", true);
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read date.</summary>
        private void ReadDate()
        {
            string iFormat;
            var wasEmpty = fReader.IsEmptyElement;
            if (fReader.AttributeCount > 0)
            {
                // some dialects support a formatting attribute for the date. The most common form uses the php format: http://php.net/manual/en/function.strftime.php
                iFormat = fReader.GetAttribute("format"); // we skip the other attributes for the time being.
                WriteToOut(ConvertDateFormat(iFormat), true);
            }
            else
            {
                WriteToOut("$Time", true);
            }

            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The convert date format.</summary>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ConvertDateFormat(string format)
        {
            var iRes = new System.Text.StringBuilder();
            var iFormatter = false;
            foreach (var i in format)
            {
                if (i == '%')
                {
                    if (iFormatter == false)
                    {
                        iFormatter = true;
                    }
                    else
                    {
                        iFormatter = false;
                        iRes.Append(i);
                    }
                }
                else if (iFormatter)
                {
                    switch (i)
                    {
                        case 'a':
                            iRes.Append("^noun.day[$time:DayOfWeek-1]");
                            break;
                        case 'A':
                            iRes.Append("^noun.day[$time:DayOfWeek-1]");
                            break;
                        case 'd':
                            iRes.Append("$time:day");
                            break;
                        case 'W':
                            iRes.Append("$time:DayOfWeek");
                            break;
                        case 'x':
                            iRes.Append("$time:hour\\:$time:minute");
                            break;
                        case 'I':
                            iRes.Append("$time:hour12");
                            break;
                        case 'p':
                            iRes.Append("$time:AMPM");
                            break;
                        case 'y':
                            iRes.Append("$time:Year");
                            break;
                        case 'Y':
                            iRes.Append("$time:Year");
                            break;
                        case 'b':
                            iRes.Append("^noun.Month[$time:Month-1]");
                            break;
                        case 'B':
                            iRes.Append("^noun.Month[$time:Month-1]");
                            break;
                        case 'm':
                            iRes.Append("$time:month");
                            break;
                        default:
                            break;
                    }

                    iFormatter = false;
                }
                else
                {
                    iRes.Append(i);
                }
            }

            return iRes.ToString();
        }

        /// <summary>The read bot.</summary>
        private void ReadBot()
        {
            var wasEmpty = fReader.IsEmptyElement;
            if (fReader.AttributeCount > 0)
            {
                var iGet = fReader.GetAttribute(0);
                WriteToOut(string.Format("#bot.{0}", iGet), true);
            }
            else
            {
                LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("nothing to get from bot"));
            }

            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read get.</summary>
        private void ReadGet()
        {
            var wasEmpty = fReader.IsEmptyElement;
            if (fReader.AttributeCount > 0)
            {
                var iGet = fReader.GetAttribute(0);
                WriteToOut(string.Format("#user.{0}", CheckSingleWord(iGet)), true);
            }
            else
            {
                LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("nothing to get"));
            }

            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>chesks if the text contais a single word, if not, it it put between
        ///     brackets.</summary>
        /// <param name="toCheck"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string CheckSingleWord(string toCheck)
        {
            foreach (var i in toCheck)
            {
                if (char.IsLetter(i) == false)
                {
                    return string.Format("({0})", toCheck);
                }
            }

            return toCheck;
        }

        /// <summary>The read topic star.</summary>
        private void ReadTopicStar()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            int iIndex;
            if (wasEmpty == false)
            {
                var iVal = fReader.GetAttribute(0);
                if (int.TryParse(iVal, out iIndex) == false)
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("integer expected as value for attribute 'index' on element 'star'."));
                }
            }
            else
            {
                iIndex = 1;
            }

            WriteToOut(string.Format("$topic{0}", iIndex), true);

            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read that star.</summary>
        private void ReadThatStar()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            int iIndex;
            if (wasEmpty == false)
            {
                var iVal = fReader.GetAttribute(0);
                if (int.TryParse(iVal, out iIndex) == false)
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("integer expected as value for attribute 'index' on element 'thatstar'."));
                }
            }
            else
            {
                iIndex = 1;
            }

            WriteToOut(string.Format("$that{0}", iIndex), true);

            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read input.</summary>
        private void ReadInput()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            int iIndex;
            if (wasEmpty == false)
            {
                var iVal = fReader.GetAttribute(0);
                if (int.TryParse(iVal, out iIndex) == false)
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("integer expected as value for attribute 'index' on element 'input'."));
                }

                if (fReader.AttributeCount > 1)
                {
                    LogService.Log.LogWarning(
                        AIMLSOURCENAME, 
                        BuildLogText("Secondary index on 'input' elements not yet supported."));
                }
            }
            else
            {
                iIndex = 1;
            }

            WriteToOut(string.Format("$PrevIn({0})", iIndex), true);

            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>
        ///     returns the last output statement
        /// </summary>
        private void ReadTemplateThat()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            int iIndex;
            if (wasEmpty == false)
            {
                var iVal = fReader.GetAttribute(0);
                if (int.TryParse(iVal, out iIndex) == false)
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("integer expected as value for attribute 'index' on element 'that'."));
                }

                if (fReader.AttributeCount > 1)
                {
                    LogService.Log.LogWarning(
                        AIMLSOURCENAME, 
                        BuildLogText("Secondary index on 'that' elements not yet supported."));
                }
            }
            else
            {
                iIndex = 1;
            }

            WriteToOut(string.Format("$PrevOut({0})", iIndex), true);

            if (wasEmpty)
            {
                return;
            }

            fReader.ReadEndElement();
        }

        /// <summary>The read java script.</summary>
        private void ReadJavaScript()
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            var iValue = fReader.ReadInnerXml();
            LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("javascript elements not yet supported."));
            fDo.AppendLine(string.Format("System.JavaScript(\"{0}\");", iValue));

            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
        }

        /// <summary>The read system.</summary>
        private void ReadSystem()
        {
            string iSysContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("system");
                fReader.ReadEndElement();
                SkipSpaces();
                iSysContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            fDo.AppendLine(string.Format("System.StartProcess({0});", AddListInstruction(iSysContent)));
        }

        /// <summary>The read learn.</summary>
        /// <param name="elName">The el name.</param>
        private void ReadLearn(string elName)
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            if (fReader.Name.ToLower() == "category")
            {
                ReadLearnCategory();
            }
            else
            {
                LogService.Log.LogError(AIMLSOURCENAME, BuildLogText("category element expected"));
                fReader.ReadOuterXml(); // skip the element.
            }

            SkipSpaces();
            fReader.ReadEndElement();
            SkipSpaces();
        }

        /// <summary>The read learn category.</summary>
        private void ReadLearnCategory()
        {
            var iLCount = fLearnCount++;
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            var iPattern = ReadLearnPattern();
            SkipSpaces();
            var iThat = ReadLearnThat();
            SkipSpaces();
            var iTemplate = ReadLearnTemplate();
            SkipSpaces();
            fDo.AppendLine(string.Format("var pattern{1} = {0};", AddListInstruction(iPattern), iLCount));
            if (string.IsNullOrEmpty(iThat) == false)
            {
                fDo.AppendLine(string.Format("var that{1} = {0};", AddListInstruction(iThat), iLCount));
            }

            fDo.AppendLine(string.Format("var template{1} = {0};", AddListInstruction(iTemplate), iLCount));
            if (string.IsNullOrEmpty(iThat) == false)
            {
                fDo.AppendLine(
                    string.Format("Chatbot.Learn.LearnCategoryThat(pattern{0}, template{0}, that{0});", iLCount));
            }
            else
            {
                fDo.AppendLine(string.Format("Chatbot.Learn.LearnCategory(pattern{0}, template{0});", iLCount));
            }

            fReader.ReadEndElement();
            SkipSpaces();
        }

        /// <summary>The read learn template.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string ReadLearnTemplate()
        {
            string iRes = null;
            if (fReader.Name.ToLower() == "template")
            {
                var iPrevOut = fOutput;
                fOutput = new System.Text.StringBuilder();

                    // reset, don't need to store any previous value, this is always a root element.
                try
                {
                    var wasEmpty = fReader.IsEmptyElement;
                    fReader.Read();
                    if (wasEmpty)
                    {
                        return string.Empty;
                    }

                    ReadTemplateElements("template", true);
                    fReader.ReadEndElement();
                    iRes = fOutput.ToString();
                }
                finally
                {
                    fOutput = iPrevOut;
                }
            }
            else
            {
                throw new System.InvalidOperationException("template element expected.");
            }

            return iRes;
        }

        /// <summary>The read learn that.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadLearnThat()
        {
            string iThatTxt = null;
            if (fReader.Name.ToLower() == "that")
            {
                var iRes = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return string.Empty;
                }

                while (true)
                {
                    var iName = fReader.Name.ToLower();
                    if (fReader.NodeType == System.Xml.XmlNodeType.Text
                        || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
                    {
                        iRes.Append(fReader.Value);
                        fReader.Read();
                    }
                    else if (iName == "that" || fReader.EOF)
                    {
                        break;
                    }
                    else if (iName == "eval")
                    {
                        iRes.Append(ReadEval());
                    }
                    else
                    {
                        LogService.Log.LogError(
                            AIMLSOURCENAME, 
                            BuildLogText(string.Format("Unknown element: '{0}', skipping content", iName)));
                        fReader.ReadOuterXml(); // simply skip this element.
                    }
                }

                fReader.ReadEndElement();
                iThatTxt = ReplaceVars(iThatTxt, "t");
            }

            return iThatTxt;
        }

        /// <summary>The read learn pattern.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadLearnPattern()
        {
            var iRes = new System.Text.StringBuilder();
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return string.Empty;
            }

            while (true)
            {
                var iName = fReader.Name.ToLower();
                if (fReader.NodeType == System.Xml.XmlNodeType.Text
                    || fReader.NodeType == System.Xml.XmlNodeType.Whitespace)
                {
                    iRes.Append(fReader.Value);
                    fReader.Read();
                }
                else if (iName == "pattern" || fReader.EOF)
                {
                    break;
                }
                else if (iName == "eval")
                {
                    iRes.Append(ReadEval());
                }
                else
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText(string.Format("Unknown element: '{0}', skipping content", iName)));
                    fReader.ReadOuterXml(); // simply skip this element.
                }
            }

            fReader.ReadEndElement();
            return ReplaceVars(iRes.ToString(), "v");
        }

        /// <summary>The read eval.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReadEval()
        {
            string iEvalContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return string.Empty;
                }

                ReadTemplateElements("eval");
                fReader.ReadEndElement();
                iEvalContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            return iEvalContent;
        }

        /// <summary>The read think.</summary>
        private void ReadThink()
        {
            var iPrev = fOutput; // so we can restore.
            var iPrevInThink = fInThink;
            fInThink = true;
            try
            {
                fOutput = new System.Text.StringBuilder();

                    // we wont be doing anything with this object, but we need it to filter out any possible output that got rendered in the think element.
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("think");
                fReader.ReadEndElement();
                SkipSpaces();
            }
            finally
            {
                fOutput = iPrev;
                fInThink = iPrevInThink;
            }
        }

        /// <summary>The read gender.</summary>
        private void ReadGender()
        {
            string iGenderContent;
            var iGenderCount = fGenderCount++; // store the value now, so we can have nested srai tags.
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("gender");
                fReader.ReadEndElement();
                SkipSpaces();
                iGenderContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            fDo.AppendLine(string.Format("$g{0} = {1};", iGenderCount, AddListInstruction(iGenderContent)));
            WriteToOut(string.Format("$g{0}:gender()", iGenderCount), true);
        }

        /// <summary>The read person 2.</summary>
        private void ReadPerson2()
        {
            string iPersonContent;
            var iPersonCount = fPersonCount++; // store the value now, so we can have nested srai tags.
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("person2");
                fReader.ReadEndElement();
                SkipSpaces();
                iPersonContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            fDo.AppendLine(string.Format("$p{0} = {1};", iPersonCount, AddListInstruction(iPersonContent)));
            WriteToOut(string.Format("$p{0}:InvertPerson2", iPersonCount), true);
        }

        /// <summary>The read person.</summary>
        private void ReadPerson()
        {
            string iPersonContent;
            var iPersonCount = fPersonCount++; // store the value now, so we can have nested srai tags.
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("person");
                fReader.ReadEndElement();
                SkipSpaces();
                iPersonContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            fDo.AppendLine(string.Format("$p{0} = {1};", iPersonCount, AddListInstruction(iPersonContent)));
            WriteToOut(string.Format("$p{0}:InvertPerson", iPersonCount), true);
        }

        /// <summary>The read gossip.</summary>
        private void ReadGossip()
        {
            string iGContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("gossip");
                fReader.ReadEndElement();
                SkipSpaces();
                iGContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("Gossip element not supported."));
        }

        /// <summary>The read set.</summary>
        private void ReadSet()
        {
            string iSetContent, iSet = null;
            var iPrev = fOutput; // so we can restore.
            try
            {
                var iElName = fReader.Name; // srai has a shortform 'sr', so we need to make the end-element variable.
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                if (fReader.AttributeCount > 0)
                {
                    iSet = fReader.GetAttribute(0);
                }
                else
                {
                    LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("nothing to set"));
                }

                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements(iElName);
                fReader.ReadEndElement();
                SkipSpaces();
                iSetContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            fDo.AppendLine(string.Format("#user.{0} = {1};", CheckSingleWord(iSet), AddListInstruction(iSetContent)));
            if (fInThink == false)
            {
                // when not in think, we need to pass the result of set to the output, but this depends on how the asset item is configured, so we need to render an 'if'.
                WriteToOut(string.Format("#user.{0}:NameOrValue", iSet), true);
            }
        }

        /// <summary>checks if the text contains multiple strings, if so, an L() is placed
        ///     around the text.</summary>
        /// <param name="toCheck"></param>
        /// <returns>The <see cref="string"/>.</returns>
        private string AddListInstruction(string toCheck)
        {
            // toCheck = toCheck.Trim();                                                     //front and back enters need to be skipped, an AIML interpreter wont print them either.
            foreach (var i in toCheck)
            {
                if (char.IsLetter(i) == false)
                {
                    return string.Format("L({0})", toCheck.Trim());

                        // remove space in front and back of an L() instruction, otherwise it fails, enters and spaces are unimportant during the parse anyway.
                }
            }

            return string.Format("'{0}'", toCheck);
        }

        /// <summary>The read sentence case.</summary>
        private void ReadSentenceCase()
        {
            string iCaseContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("sentencecase");
                fReader.ReadEndElement();
                SkipSpaces();
                iCaseContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            WriteToOut(string.Format("$SentenceCase({0})", AddListInstruction(iCaseContent)), true);
        }

        /// <summary>The read formal case.</summary>
        private void ReadFormalCase()
        {
            string iCaseContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("formal");
                fReader.ReadEndElement();
                SkipSpaces();
                iCaseContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            WriteToOut(string.Format("$UCase({0})", AddListInstruction(iCaseContent)), true);
        }

        /// <summary>The read lower case.</summary>
        private void ReadLowerCase()
        {
            string iCaseContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();

                iCaseContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            WriteToOut(string.Format("$AllLCase({0})", AddListInstruction(iCaseContent)), true);
        }

        /// <summary>The read upper case.</summary>
        private void ReadUpperCase()
        {
            string iCaseContent;
            var iPrev = fOutput; // so we can restore.
            try
            {
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("uppercase");
                fReader.ReadEndElement();
                SkipSpaces();
                iCaseContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            WriteToOut(string.Format("$AllUCase({0})", AddListInstruction(iCaseContent)), true);
        }

        /// <summary>Reads a condition element.</summary>
        /// <param name="isTopLevel">if set to <c>true</c> the condition is the top element to be rendered,
        ///     so we can use the 'conditional' section.</param>
        private void ReadCondition(bool isTopLevel)
        {
            string iName = null;
            string iValue = null;
            var iCondCount = fCondCount++;

                // store the valuue and increment, so that the rendered counts correspond with the ordedr.
            var wasEmpty = fReader.IsEmptyElement;
            if (wasEmpty == false && fReader.AttributeCount > 0)
            {
                // get the attributes, if there are any.
                iName = fReader.GetAttribute(0); // the name.
                if (fReader.AttributeCount > 1)
                {
                    iValue = fReader.GetAttribute(1);
                    if (iValue == "*")
                    {
                        // pandora bots extention.
                        iValue = string.Empty;
                    }
                }
            }

            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            if (iValue != null)
            {
                ProcessSimpleCondition(iName, iValue, isTopLevel, iCondCount);
            }
            else
            {
                var iIsFirst = true;
                var iLastReached = false;
                var iElName = fReader.Name.ToLower();
                while (iElName != "condition" && fReader.EOF == false)
                {
                    if (iElName != "li")
                    {
                        // some errors can exist in the aiml, like 'think' elements inside a condition,which is not allowed. Skip these elements
                        HandleInvalidElement();
                    }
                    else
                    {
                        if (iLastReached)
                        {
                            LogService.Log.LogError(
                                AIMLSOURCENAME, 
                                BuildLogText(
                                    "after a 'li' element with no attributes, no more 'li' elements are allowed in a condition."));
                        }

                        iLastReached = ReadCondLi(iName, iCondCount, iIsFirst, isTopLevel);
                        iIsFirst = false;
                    }

                    SkipSpaces();
                    iElName = fReader.Name.ToLower();
                }

                if (isTopLevel == false)
                {
                    fDo.AppendLine("}");
                    WriteToOut(string.Format("$c{0}", iCondCount), true);
                }
            }

            fReader.ReadEndElement();
        }

        /// <summary>The process simple condition.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="isTopLevel">The is top level.</param>
        /// <param name="count">The count.</param>
        private void ProcessSimpleCondition(string name, string value, bool isTopLevel, int count)
        {
            if (isTopLevel)
            {
                var iNew = new System.Text.StringBuilder();
                var iPrev = fOutput;
                var iPrevPatterns = fOutputPatterns;
                fOutputPatterns = BrainHelper.MakeCluster((ulong)PredefinedNeurons.TextPatternOutputs);
                try
                {
                    fOutput = iNew;
                    var iConds = BrainHelper.MakeCluster((ulong)PredefinedNeurons.Condition, fOutputPatterns);
                    Link.Create(fRule, iConds, (ulong)PredefinedNeurons.Condition);
                    TextNeuron iCond;
                    if (value != "*")
                    {
                        iCond =
                            NeuronFactory.GetText(
                                string.Format("#user.{0} == {1}", CheckSingleWord(name), AddListInstruction(value)));
                    }
                    else
                    {
                        iCond = NeuronFactory.GetText(string.Format("Count(#user.{0}) > 0", CheckSingleWord(name)));
                    }

                    Link.Create(fOutputPatterns, iConds, (ulong)PredefinedNeurons.Condition);
                    if (isTopLevel)
                    {
                        // if it's toplevel, allow
                        var iContent = ReadTemplateContentItems("condition");
                        ProcessTemplateContent(iContent);
                    }
                    else
                    {
                        ReadTemplateElements("condition");
                    }

                    ProcessOutput();

                        // if the condition didn't have a random, but instead simply something to render, it still need to be processed.
                    fOutput = iPrev;
                }
                finally
                {
                    fOutputPatterns = iPrevPatterns;
                }
            }
            else
            {
                if (value != "*")
                {
                    fDo.AppendLine(
                        string.Format("if(#user.{0} == {1}){{", CheckSingleWord(name), AddListInstruction(value)));
                }
                else
                {
                    fDo.AppendLine(
                        string.Format("if(Count(#user.{0}) > 0){{", CheckSingleWord(name), AddListInstruction(value)));
                }

                var iPrevOut = fOutput;
                fOutput = new System.Text.StringBuilder(); // need to capture outputs and do.
                try
                {
                    ReadTemplateElements("condition");
                    fDo.AppendLine(string.Format("$c{0} = {1};", count, AddListInstruction(fOutput.ToString())));

                        // add the render statement to the do, this do wi
                    fDo.AppendLine("}");
                }
                finally
                {
                    fOutput = iPrevOut;
                }

                WriteToOut(string.Format("$c{0}", count), true);
            }
        }

        /// <summary>reads a conditional 'li' statement.</summary>
        /// <param name="name">If the name is supplied, the element should have a 'value' attribute,
        ///     or no attributes, in which case it must be the last in the list.</param>
        /// <param name="condCount">The cond count.</param>
        /// <param name="isFirst">if set to <c>true</c> [is first].</param>
        /// <param name="isTopLevel">if set to <c>true</c> [is top level].</param>
        /// <returns><see langword="true"/> if the last item is reached.</returns>
        private bool ReadCondLi(string name, int condCount, bool isFirst, bool isTopLevel)
        {
            var iRes = false;
            string iName = null, iValue = null;
            if (string.IsNullOrEmpty(name) == false)
            {
                iName = name;
                if (fReader.AttributeCount > 0)
                {
                    iValue = fReader.GetAttribute(0);
                }
            }
            else if (fReader.AttributeCount > 0)
            {
                if (fReader.AttributeCount > 1)
                {
                    iName = fReader.GetAttribute(0);
                    iValue = fReader.GetAttribute(1);
                }
                else
                {
                    iName = null;
                    iValue = fReader.GetAttribute(0);
                }
            }

            if (isTopLevel == false)
            {
                iRes = ProcessCondLi(iName, iValue, condCount, isFirst);
            }
            else if (string.IsNullOrEmpty(iValue) == false || string.IsNullOrEmpty(iName) == false)
            {
                iRes = ProcessTopLevelCondLi(iName, iValue, condCount, isFirst);
            }
            else
            {
                ProcessEndTopLevelCondI(condCount);
                iRes = true;
            }

            SkipSpaces();
            return iRes;
        }

        /// <summary>handles a conditional li that has no value and name attribute (so an
        ///     'else' condition).</summary>
        /// <param name="condCount"></param>
        private void ProcessEndTopLevelCondI(int condCount)
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();

            var iContent = ReadTemplateContentItems("li");

            var iPrevDo = fDo;
            var iPrev = fOutput;
            var iPrevOutput = fOutputPatterns;
            try
            {
                fOutput = new System.Text.StringBuilder();
                fDo = new System.Text.StringBuilder();
                ProcessTemplateContent(iContent);
                ProcessOutput(); // render the output of the 'li' so we can attach the 'do' section to it.
                fReader.ReadEndElement();
                SkipSpaces();
            }
            finally
            {
                fOutput = iPrev;
                fDo = iPrevDo;
                fOutputPatterns = iPrevOutput;
            }
        }

        /// <summary>The process top level cond li.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="condCount">The cond count.</param>
        /// <param name="isFirst">The is first.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessTopLevelCondLi(string name, string value, int condCount, bool isFirst)
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return true;
            }

            SkipSpaces();
            var iPrevDo = fDo;
            var iPrev = fOutput;
            var iPrevPatterns = fOutputPatterns;
            fOutputPatterns = BrainHelper.MakeCluster((ulong)PredefinedNeurons.TextPatternOutputs);
            try
            {
                fOutput = new System.Text.StringBuilder();
                fDo = new System.Text.StringBuilder(iPrevDo.ToString());
                NeuronCluster iConds;
                if (isFirst)
                {
                    iConds = BrainHelper.MakeCluster((ulong)PredefinedNeurons.Condition, fOutputPatterns);
                    Link.Create(fRule, iConds, (ulong)PredefinedNeurons.Condition);
                }
                else
                {
                    iConds = fRule.FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
                    using (var iChildren = iConds.ChildrenW) iChildren.Add(fOutputPatterns);
                    System.Diagnostics.Debug.Assert(iConds != null);
                }

                if (string.IsNullOrEmpty(value) == false)
                {
                    TextNeuron iCond;
                    if (name == "*")
                    {
                        iCond = NeuronFactory.GetText(string.Format("Count(#user.{0}) > 0", CheckSingleWord(value)));

                            // * means 'any', but it can't be empty.
                    }
                    else if (value == "*")
                    {
                        iCond = NeuronFactory.GetText(string.Format("Count(#user.{0}) > 0", CheckSingleWord(name)));
                    }
                    else
                    {
                        iCond =
                            NeuronFactory.GetText(
                                string.Format("#user.{0} == {1}", CheckSingleWord(name), AddListInstruction(value)));
                    }

                    Brain.Current.Add(iCond);
                    Link.Create(fOutputPatterns, iCond, (ulong)PredefinedNeurons.Condition);
                    try
                    {
                        var iParser = new Parsers.ConditionParser(iCond, AIMLSOURCENAME);
                        iParser.Parse();
                    }
                    catch (System.Exception e)
                    {
                        Parsers.ConditionParser.RemoveCondPattern(iCond);

                            // when something went wrong, we remove the compilation, cause it's invalid anyway.
                        var iSource = string.Format("({0}).({1})", fTopicName, fCurrentRuleName);
                        LogService.Log.LogError(iSource, BuildLogText(e.Message));
                    }
                }

                var iContent = ReadTemplateContentItems("li");
                ProcessTemplateContent(iContent);
                fReader.ReadEndElement();
                ProcessOutput();

                    // if the condition didn't have a random, but instead simply something to render, it still need to be processed.
                ProcessDo();
                SkipSpaces();
            }
            finally
            {
                fOutputPatterns = iPrevPatterns;
                fOutput = iPrev;
                fDo = iPrevDo;
            }

            return name == null || value == null;
        }

        /// <summary>The process cond li.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="condCount">The cond count.</param>
        /// <param name="isFirst">The is first.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessCondLi(string name, string value, int condCount, bool isFirst)
        {
            var iRes = false;
            var iNew = new System.Text.StringBuilder();
            var iPrev = fOutput;
            var iNewDo = new System.Text.StringBuilder();
            var iPrevDo = fDo;
            try
            {
                fOutput = iNew; // reset, don't need to store any previous value, this is always a root element.
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return true;
                }

                ReadTemplateElements("li");
                fReader.ReadEndElement();
            }
            finally
            {
                fOutput = iPrev;
                fDo = iPrevDo;
            }

            if (isFirst)
            {
                if (name != null && value != null)
                {
                    if (name == "*")
                    {
                        fDo.AppendLine(string.Format("if(Count(#user.{0}) > 0){{", CheckSingleWord(value)));

                            // * actually means 'any', but an empty value is not allowed.
                    }
                    else if (value == "*")
                    {
                        fDo.AppendLine(string.Format("if(Count(#user.{0}) > 0){{", CheckSingleWord(name)));

                            // * actually means 'any', but an empty value is not allowed.
                    }
                    else
                    {
                        fDo.AppendLine(
                            string.Format("if(#user.{0} == {1}){{", CheckSingleWord(name), AddListInstruction(value)));
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("First element should have a name & value attribute."));
                }
            }
            else
            {
                if (name != null && value != null)
                {
                    fDo.AppendLine("}");
                    if (name == "*")
                    {
                        fDo.AppendLine(string.Format("else if(Count(#user.{0}) > 0){{", CheckSingleWord(value)));

                            // * actually means 'any', but an empty value is not allowed.
                    }
                    else if (value == "*")
                    {
                        fDo.AppendLine(string.Format("else if(Count(#user.{0}) > 0){{", CheckSingleWord(name)));

                            // * actually means 'any', but an empty value is not allowed.
                    }
                    else
                    {
                        fDo.AppendLine(
                            string.Format(
                                "else if(#user.{0} == {1}){{", 
                                CheckSingleWord(name), 
                                AddListInstruction(value)));
                    }
                }
                else
                {
                    // if (name == null && value == null)
                    fDo.AppendLine("}");
                    fDo.AppendLine("else{");
                    iRes = true;
                }

                // else
                // Log.LogError(AIMLSOURCENAME, BuildLogText("Both name & value attributes should be supplied in a condition-li element."));
            }

            if (iNewDo.Length > 0)
            {
                fDo.AppendLine(iNewDo.ToString());

                    // before we add the text bits, make certain that all the code that was defined inside the li, is also rendered.
            }

            if (iNew.Length > 0)
            {
                fDo.AppendLine(string.Format("$c{0} = {1};", condCount, AddListInstruction(iNew.ToString())));

                    // add the render statement to the do, this do wi
            }

            return iRes;
        }

        /// <summary>reads the 'random' element. If a random is the child of another
        ///     element (and not a root condition), then the contents of the element
        ///     need to be rendered in a new rule (that only has output), and replace
        ///     the random with ':render'. When a random element is the child of a
        ///     condition (or condition-li), and the condition is the root of the
        ///     template, the random element can render it's content as the output of
        ///     the conditionals and no extra rule is required.</summary>
        /// <param name="asRoot">The as Root.</param>
        private void ReadRandom(bool asRoot)
        {
            var wasEmpty = fReader.IsEmptyElement;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            if (asRoot == false)
            {
                string iRuleNameTxt;
                var iPrevRule = fRule;
                var iPrevOutPatterns = fOutputPatterns;
                try
                {
                    fOutputPatterns = null; // reset so that a new one is created when needed.
                    var iRuleName = fRule.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);
                    iRuleNameTxt = string.Format("{0} Out {1}", BrainHelper.GetTextFrom(iRuleName), fRandomCount++);
                    BuildRule(iRuleNameTxt);
                    var iElName = fReader.Name.ToLower();
                    while (iElName != "random" && fReader.EOF == false)
                    {
                        if (iElName != "li")
                        {
                            // some errors can exist in the aiml, like 'think' elements inside a condition,which is not allowed. Skip these elements
                            HandleInvalidElement();
                        }
                        else
                        {
                            ReadLi();
                        }

                        SkipSpaces();
                        iElName = fReader.Name.ToLower();
                    }
                }
                finally
                {
                    fRule = iPrevRule;
                    fOutputPatterns = iPrevOutPatterns;
                }

                WriteToOut(string.Format("~({0}).({1}):Render", fTopicName, iRuleNameTxt), true);
            }
            else
            {
                var iElName = fReader.Name.ToLower();
                while (iElName != "random" && fReader.EOF == false)
                {
                    if (iElName != "li")
                    {
                        // some errors can exist in the aiml, like 'think' elements inside a condition,which is not allowed. Skip these elements
                        HandleInvalidElement();
                    }
                    else
                    {
                        ReadLi();
                    }

                    SkipSpaces();
                    iElName = fReader.Name.ToLower();
                }
            }

            fReader.ReadEndElement();
        }

        /// <summary>The handle invalid element.</summary>
        private void HandleInvalidElement()
        {
            string iInvalid;
            if (fReader.NodeType == System.Xml.XmlNodeType.Text)
            {
                // text needs to be depicted differetnly.
                iInvalid = fReader.Value;
                fReader.Read(); // go to next item
            }
            else
            {
                iInvalid = fReader.ReadOuterXml();
            }

            LogService.Log.LogWarning(AIMLSOURCENAME, BuildLogText("Invalid element inside random: " + iInvalid));
        }

        /// <summary>The read li.</summary>
        private void ReadLi()
        {
            var iPrev = fOutput;
            var iPrevDo = fDo;
            try
            {
                fOutput = new System.Text.StringBuilder();

                    // reset, don't need to store any previous value, this is always a root element.
                fDo = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements("li");
                fReader.ReadEndElement();
                var iOut = ProcessOutput(); // a 'li' in a 'random' simply collect the output pattern.
                ProcessDoLi(iOut);
            }
            finally
            {
                fOutput = iPrev;
                fDo = iPrevDo;
            }

            SkipSpaces();
        }

        /// <summary>The read srai.</summary>
        private void ReadSrai()
        {
            string iSraiContent;
            var iSraiCount = fSrCount++; // store the value now, so we can have nested srai tags.
            var iPrev = fOutput; // so we can restore.
            try
            {
                var iElName = fReader.Name; // srai has a shortform 'sr', so we need to make the end-element variable.
                fOutput = new System.Text.StringBuilder();
                var wasEmpty = fReader.IsEmptyElement;
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                ReadTemplateElements(iElName);
                fReader.ReadEndElement();
                iSraiContent = fOutput.ToString();
            }
            finally
            {
                fOutput = iPrev;
            }

            WriteToOut(string.Format("~srai({0})", AddListInstruction(iSraiContent)), true);
        }

        /// <summary>
        ///     reads the 'start' element.
        /// </summary>
        private void ReadStar()
        {
            var wasEmpty = fReader.IsEmptyElement;
            int iIndex;
            if (fReader.AttributeCount > 0)
            {
                var iVal = fReader.GetAttribute(0);
                if (int.TryParse(iVal, out iIndex) == false)
                {
                    LogService.Log.LogError(
                        AIMLSOURCENAME, 
                        BuildLogText("integer expected as value for attribute 'index' on element 'star'."));
                }
            }
            else
            {
                iIndex = 1;
            }

            WriteToOut(string.Format("$v{0}", iIndex), true);

            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            SkipSpaces();
            fReader.ReadEndElement();
            SkipSpaces();
        }

        /// <summary>replaces * and _ in the pattern by $vXX where xx is a sequence nr for
        ///     the current category.</summary>
        /// <param name="text">The text for which to replace all the * and _ signs</param>
        /// <param name="varName">Name of the var to use (number will be added at the end of the
        ///     varname).</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ReplaceVars(string text, string varName)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                var iRes = new System.Text.StringBuilder();
                var iCount = 1;
                var iPos = 0;
                while (iPos < text.Length)
                {
                    if (text[iPos] == '*')
                    {
                        var iTemp = iPos + 1;
                        while (iTemp < text.Length && char.IsSeparator(text[iTemp]))
                        {
                            // skip all the spaces and check if we have a * * or * _ -> if this is the case, the first var needs to be rendered witha length specifier.
                            iTemp++;
                        }

                        if (iTemp < text.Length && (text[iTemp] == '*' || text[iTemp] == '_'))
                        {
                            iRes.Append(string.Format("${0}{1}:1", varName, iCount++));
                        }
                        else
                        {
                            iRes.Append(string.Format("${0}{1}", varName, iCount++));
                        }
                    }
                    else if (text[iPos] == '_')
                    {
                        var iTemp = iPos + 1;
                        while (iTemp < text.Length && char.IsSeparator(text[iTemp]))
                        {
                            // skip all the spaces and check if we have a * * or * _ -> if this is the case, the first var needs to be rendered witha length specifier.
                            iTemp++;
                        }

                        if (iTemp < text.Length && (text[iTemp] == '*' || text[iTemp] == '_'))
                        {
                            iRes.Append(string.Format("${0}{1}:1%1.2", varName, iCount++)); // assign a custom weight.
                        }
                        else
                        {
                            iRes.Append(string.Format("${0}{1}%1.2", varName, iCount++));
                        }
                    }
                    else
                    {
                        iRes.Append(text[iPos]);
                    }

                    iPos++;
                }

                return iRes.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        ///     reads a topic definition.
        /// </summary>
        private void ReadTopic()
        {
            var wasEmpty = fReader.IsEmptyElement;
            string iTopicName;
            if (fReader.AttributeCount > 0)
            {
                iTopicName = fReader.GetAttribute(0); // first attribute must be the name.
            }
            else
            {
                iTopicName = string.Empty;
            }

            var iPrevTopic = fTopic;
            try
            {
                fReader.Read();
                if (wasEmpty)
                {
                    return;
                }

                SkipSpaces();
                PrepareTopic(iTopicName, false);
                while (fReader.Name.ToLower() != "topic" && fReader.EOF == false)
                {
                    ReadCategory();
                }

                fReader.ReadEndElement();
            }
            finally
            {
                fTopic = iPrevTopic;

                    // restore the previous topic (usually that for the file). aiml files can have multiple topics in a single file.
            }
        }

        /// <summary>Makes certain that there is a topic object.</summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="autoGenerated">The auto Generated.</param>
        private void PrepareTopic(string topicName, bool autoGenerated = true)
        {
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(topicName) == false);
            fRules.Clear(); // new topic, new rules dict.
            fTopicName = topicName; // store the topicname, so we can return it.
            fTopic = Parsers.TopicsDictionary.Find(topicName) as NeuronCluster;
            if (fTopic == null)
            {
                fTopic = NeuronFactory.GetCluster();
                Brain.Current.Add(fTopic);
                fTopic.Meaning = (ulong)PredefinedNeurons.TextPatternTopic;
                BrainData.Current.NeuronInfo[fTopic].DisplayTitle = topicName;

                    // by doing it like this, we make certain that the name is properly attached to the topic  (through 'NameOfMember' link), so that an objectcan also be found through it's name at runtime.
                var iRes = new TextPatternEditor(fTopic);

                    // add the new topic to the result list, so it will be shown to the user.
                iRes.Name = topicName;
                fresults.Add(iRes);

                if (autoGenerated == false && string.IsNullOrEmpty(topicName) == false)
                {
                    // if it's a topic defined in aiml, we also need to generate a filter.
                    var iTopicFilter = NeuronFactory.GetText(topicName);
                    Brain.Current.Add(iTopicFilter);
                    var iTopicFilters = BrainHelper.MakeCluster((ulong)PredefinedNeurons.TopicFilter, iTopicFilter);
                    Link.Create(fTopic, iTopicFilters, (ulong)PredefinedNeurons.TopicFilter);
                }
            }
            else
            {
                var iFound =
                    (from i in BrainData.Current.Editors.AllTextPatternEditors() where i.Item == fTopic select i)
                        .FirstOrDefault(); // also show the existing topic if possible.
                if (iFound != null && fresults.Contains(iFound) == false)
                {
                    fresults.Add(iFound);
                }

                LoadRulesForCurTopic();
            }
        }

        /// <summary>
        ///     loads all the already existing rules in a dict so we can find them
        ///     quickly. This is a speed up.
        /// </summary>
        private void LoadRulesForCurTopic()
        {
            if (Parsers.TopicsDictionary.NameGetter == null)
            {
                throw new System.InvalidOperationException("Please provide a name supplier for the topicDictionary.");
            }

            if (fTopic != null)
            {
                System.Collections.Generic.List<ulong> iToSearch;
                using (var iList = fTopic.Children)
                {
                    iToSearch = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);

                        // we only need a get, can be done unsave without lock, cause we only need a guide.
                    iToSearch.AddRange(iList); // only get the id's, convert as needed.
                }

                try
                {
                    foreach (var i in iToSearch)
                    {
                        fRules.Add(Parsers.TopicsDictionary.NameGetter(i).ToLower(), i);
                    }
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iToSearch);
                }
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}