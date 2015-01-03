// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   provides xml streaming for a textpattern.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides xml streaming for a textpattern.
    /// </summary>
    public class TopicXmlStreamer : BaseXmlStreamer, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        ///     Gets the <see cref="ID" /> that was read in the xml for this topic
        ///     (which isn't stored in the topic, but used by the project streamer).
        /// </summary>
        public string ID { get; private set; }

        /// <summary>Imports the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="parseErrors">The parse errors.</param>
        /// <param name="stillToResolve">The still To Resolve.</param>
        /// <param name="result">The result is returned as a ref, so that we can assign it before
        ///     reading the content. If there is an error in reading the content, we
        ///     still pass the result.</param>
        /// <param name="tracker">The tracker.</param>
        internal static void Import(
            string filename, System.Collections.Generic.List<ParsableTextPatternBase> parseErrors, System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> stillToResolve, 
            ref TextPatternEditor result, 
            Search.ProcessTrackerItem tracker)
        {
            var iStreamer = new TopicXmlStreamer();
            if (parseErrors != null)
            {
                iStreamer.fParseErrors = parseErrors;
            }

            if (stillToResolve != null)
            {
                iStreamer.fStillToResolve = stillToResolve;
            }

            var iNew = NeuronFactory.GetCluster();
            Brain.Current.Add(iNew);
            iNew.Meaning = (ulong)PredefinedNeurons.TextPatternTopic;
            iStreamer.fEditor = new TextPatternEditor(iNew);
            iStreamer.Tracker = new PosTracker { Tracker = tracker };
            result = iStreamer.fEditor;
            iStreamer.fEditor.IsOpen = true;

                // need to open it, otherwise we can't add stuff to the items, it won't have an object
            try
            {
                iStreamer.fEditor.Items.IsActive = false;
                using (
                    var iFile = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    if (iStreamer.Tracker != null)
                    {
                        iStreamer.Tracker.Stream = iFile;
                    }

                    // XmlSerializer iSer = new XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iStreamer.ReadXml(iReader);
                        }
                    }
                }
            }
            finally
            {
                iStreamer.fEditor.IsOpen = false;

                    // we close again, otherwise something strange happens when showing the imported object. This is also saver.
            }
        }

        /// <summary>Exports the specified <paramref name="editor"/> to an xml file with
        ///     the specified name and path.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="filename">The filename.</param>
        public static void Export(TextPatternEditor editor, string filename)
        {
            var iStreamer = new TopicXmlStreamer();

            using (
                var iFile = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings)) iStreamer.WriteTopic(iWriter, editor);
            }
        }

        /// <summary>Writes a list of topic items to xml. These items can be in any order.
        ///     This is used for the clipboard.</summary>
        /// <param name="values">The values.</param>
        /// <param name="editor">The editor. Can be null.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string WriteTopicParts(System.Collections.Generic.IList<PatternEditorItem> values, 
            TextPatternEditor editor)
        {
            fEditor = editor;
            using (var iFile = new System.IO.MemoryStream())
            {
                var iSettings = CreateWriterSettings();
                using (var iWriter = System.Xml.XmlWriter.Create(iFile, iSettings))
                {
                    iWriter.WriteStartElement("Clipboard"); // this is need to encapsulate multiple items.
                    foreach (var i in values)
                    {
                        if (i is InputPattern)
                        {
                            WriteInput(iWriter, (InputPattern)i);
                        }
                        else if (i is OutputPattern)
                        {
                            WriteOutput(iWriter, (OutputPattern)i);
                        }
                        else if (i is PatternRule)
                        {
                            WritePatternRule(iWriter, (PatternRule)i);
                        }
                        else if (i is PatternRuleOutput)
                        {
                            WriteConditional(iWriter, (PatternRuleOutput)i);
                        }
                        else if (i is ConditionPattern)
                        {
                            WriteCondition(iWriter, (ConditionPattern)i);
                        }
                        else if (i is DoPattern)
                        {
                            WriteDoPattern(iWriter, (DoPattern)i);
                        }
                        else if (i is InvalidPatternResponse)
                        {
                            WriteInvalid(iWriter, (InvalidPatternResponse)i);
                        }
                    }

                    iWriter.WriteEndElement();
                }

                iFile.Flush();
                iFile.Position = 0;
                var iReader = new System.IO.StreamReader(iFile);
                return iReader.ReadToEnd();
            }
        }

        /// <summary>Reads the rules from a string containing xml data. Returns all the
        ///     read patternRules, so it can be focused if needed.</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<PatternEditorItem> ReadRules(
            string items, 
            TextPatternEditor editor, 
            PatternRule insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false && iReader.Name == "Rule")
                            {
                                ReadPatternRule(iReader);
                                iRes.Add(fCurrentPattern);
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>Reads all the expressions out of the string (in xml format) and
        ///     creates new objects for them, depending on the<paramref name="insertAt"/> type.</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressions(
            string items, 
            TextPatternEditor editor, 
            PatternEditorItem insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false)
                            {
                                if (iReader.Name == "Expression")
                                {
                                    var iNew = ReadExpression(iReader);
                                    var iItem = InsertNewAt(iNew);
                                    iRes.Add(iItem);
                                }
                                else
                                {
                                    iReader.Read();
                                }
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>The read expressions in invalid.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressionsInInvalid(
            string items, 
            InvalidPatternResponseCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        var iIndex = list.IndexOf(insertAt as InvalidPatternResponse);
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false)
                        {
                            if (iReader.Name == "Expression")
                            {
                                var iNew = ReadExpression(iReader);
                                PatternEditorItem iItem = EditorsHelper.AddNewInvalidPatternResponse(
                                    list, 
                                    iNew, 
                                    iIndex++);
                                iRes.Add(iItem);
                            }
                            else
                            {
                                iReader.Read();
                            }
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>The read expressions in output.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressionsInOutput(
            string items, 
            PatternOutputsCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        var iIndex = list.IndexOf(insertAt as OutputPattern);
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false)
                        {
                            if (iReader.Name == "Expression")
                            {
                                var iNew = ReadExpression(iReader);
                                PatternEditorItem iItem = EditorsHelper.AddNewOutputPattern(list, iNew, iIndex++);
                                iRes.Add(iItem);
                            }
                            else
                            {
                                iReader.Read();
                            }
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>The read expressions in conditions.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressionsInConditions(
            string items, 
            ConditionalOutputsCollection list, 
            PatternRuleOutput insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        var iIndex = list.IndexOf(insertAt);
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false)
                        {
                            if (iReader.Name == "Expression")
                            {
                                var iNew = ReadExpression(iReader);
                                var iRuleOutput = EditorsHelper.AddNewConditionalToPattern(list, iIndex++);
                                PatternEditorItem iItem = EditorsHelper.AddNewCondition(iRuleOutput, iNew);
                                iRes.Add(iItem);
                            }
                            else
                            {
                                iReader.Read();
                            }
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>The read expressions in topic filter.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressionsInTopicFilter(
            string items, 
            TopicFilterCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        var iIndex = list.IndexOf(insertAt as TopicFilterPattern);
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false)
                        {
                            if (iReader.Name == "Expression")
                            {
                                var iNew = ReadExpression(iReader);
                                var iItem = EditorsHelper.AddNewTopicFilter(list, iNew, iIndex++);
                                iRes.Add(iItem);
                            }
                            else
                            {
                                iReader.Read();
                            }
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>The read expressions in input.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadExpressionsInInput(
            string items, 
            InputPatternCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        var iIndex = list.IndexOf(insertAt as InputPattern);
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false)
                        {
                            if (iReader.Name == "Expression")
                            {
                                var iNew = ReadExpression(iReader);
                                PatternEditorItem iItem = EditorsHelper.AddNewTextPattern(list, iNew, iIndex++);
                                iRes.Add(iItem);
                            }
                            else
                            {
                                iReader.Read();
                            }
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>Creates a new pattern object of the same type as the 'insertAt' and
        ///     inserts it at the specified place, when possible.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="PatternEditorItem"/>.</returns>
        private PatternEditorItem InsertNewAt(string value)
        {
            PatternEditorItem iRes = null;
            if (fInsertAt is InputPattern)
            {
                iRes = EditorsHelper.AddNewTextPattern(
                    fInsertAt.Rule.TextPatterns, 
                    value, 
                    fInsertAt.Rule.TextPatterns.IndexOf(fInsertAt as InputPattern));
            }
            else if (fInsertAt is PatternRule)
            {
                iRes = EditorsHelper.AddNewTextPattern(fInsertAt.Rule.TextPatterns, value);
            }
            else if (fInsertAt is OutputPattern)
            {
                iRes = EditorsHelper.AddNewOutputPattern(
                    fInsertAt.RuleOutput.Outputs, 
                    value, 
                    fInsertAt.RuleOutput.Outputs.IndexOf(fInsertAt as OutputPattern));
            }
            else if (fInsertAt is ConditionPattern)
            {
                var iNew = EditorsHelper.AddNewConditionalToPattern(
                    fInsertAt.Rule.Conditionals, 
                    fInsertAt.Rule.Conditionals.IndexOf(fInsertAt.RuleOutput));
                iRes = EditorsHelper.AddNewCondition(iNew, value);
            }
            else if (fInsertAt is InvalidPatternResponse)
            {
                iRes = EditorsHelper.AddNewInvalidPatternResponse(
                    fInsertAt.Output.InvalidResponses, 
                    value, 
                    fInsertAt.Output.InvalidResponses.IndexOf(fInsertAt as InvalidPatternResponse));
            }
            else
            {
                throw new System.NotSupportedException();
            }

            return iRes;
        }

        /// <summary>Reads the invalid patterns from a string containing xml data.</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadInvalidPatterns(
            string items, 
            TextPatternEditor editor, 
            PatternEditorItem insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            fCurrentPattern = insertAt.Rule;
            fCurrentOut = insertAt.Output;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false && iReader.Name == "InvalidResponse")
                            {
                                PatternEditorItem iNew = ReadInvalid(iReader);
                                iRes.Add(iNew);
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>The read invalid patterns.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadInvalidPatterns(
            string items, 
            InvalidPatternResponseCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            if (insertAt != null)
            {
                fCurrentPattern = insertAt.Rule;
                fCurrentOut = insertAt.Output;
            }
            else
            {
                fCurrentOut = list.Owner as OutputPattern;
            }

            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false && iReader.Name == "InvalidResponse")
                        {
                            PatternEditorItem iNew = ReadInvalid(iReader);
                            iRes.Add(iNew);
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>Reads the condition patterns from a string containing xml data</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadConditionPatterns(
            string items, 
            TextPatternEditor editor, 
            PatternEditorItem insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            fCurrentPattern = insertAt.Rule;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false && iReader.Name == "Condition")
                            {
                                PatternRuleOutput iRuleOutput;
                                if (editor.IsQuestionsSelected == false)
                                {
                                    iRuleOutput = EditorsHelper.AddNewConditionalToPattern(
                                        insertAt.Rule.Conditionals, 
                                        insertAt.Rule.Conditionals.IndexOf(insertAt.RuleOutput));
                                }
                                else
                                {
                                    iRuleOutput = EditorsHelper.AddNewConditionalToPattern(
                                        editor.Questions, 
                                        editor.Questions.IndexOf(insertAt.RuleOutput));
                                }

                                PatternEditorItem iNew = ReadCondition(iReader, iRuleOutput);
                                iRes.Add(iNew);
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>The read condition patterns.</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadConditionPatterns(
            string items, 
            ConditionalOutputsCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            fCurrentPattern = insertAt.Rule;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false && iReader.Name == "Condition")
                        {
                            var iRuleOutput = EditorsHelper.AddNewConditionalToPattern(
                                list, 
                                list.IndexOf(insertAt.RuleOutput));
                            PatternEditorItem iNew = ReadCondition(iReader, iRuleOutput);
                            iRes.Add(iNew);
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>Reads the output patterns from a string containing xml data</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadOutputPatterns(
            string items, 
            TextPatternEditor editor, 
            PatternEditorItem insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            fCurrentPattern = insertAt.Rule;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false && iReader.Name == "Output")
                            {
                                ReadOutput(iReader, insertAt.RuleOutput.Outputs);
                                iRes.Add(fCurrentOut);
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>Reads the output patterns from a string containing xml data</summary>
        /// <param name="items">The items.</param>
        /// <param name="list">The list.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadOutputPatterns(
            string items, 
            PatternOutputsCollection list, 
            PatternEditorItem insertAt)
        {
            fInsertAt = insertAt;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                var iSettings = CreateReaderSettings();
                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                        while (iReader.EOF == false && iReader.Name == "Output")
                        {
                            ReadOutput(iReader, list);
                            iRes.Add(fCurrentOut);
                        }
                    }
                }
            }

            ResolveForwardRefs();
            return iRes;
        }

        /// <summary>Reads the input patterns from a string containing xml data</summary>
        /// <param name="items">The items.</param>
        /// <param name="editor">The editor.</param>
        /// <param name="insertAt">The insert at.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal System.Collections.Generic.List<PatternEditorItem> ReadInputPatterns(
            string items, 
            TextPatternEditor editor, 
            PatternEditorItem insertAt)
        {
            fEditor = editor;
            fInsertAt = insertAt;
            fCurrentPattern = insertAt.Rule;
            var iRes = new System.Collections.Generic.List<PatternEditorItem>();

            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                using (var iFile = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(items)))
                {
                    var iSer = new System.Xml.Serialization.XmlSerializer(typeof(XmlStore));
                    var iSettings = CreateReaderSettings();
                    using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                    {
                        if (iReader.IsStartElement())
                        {
                            iReader.ReadStartElement(); // this is for the 'clipboard' wrapper.
                            while (iReader.EOF == false && iReader.Name == "Pattern")
                            {
                                var iNew = ReadInput(iReader);
                                iRes.Add(iNew);
                            }
                        }
                    }
                }

                ResolveForwardRefs();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }

            return iRes;
        }

        /// <summary>The write topic.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="editor">The editor.</param>
        public void WriteTopic(System.Xml.XmlWriter writer, TextPatternEditor editor)
        {
            fEditor = editor;
            var iIsOpen = editor.IsOpen;
            editor.IsOpen = true; // make certain that the editor is loaded.
            try
            {
                writer.WriteStartElement("Topic");
                WriteXml(writer);
                writer.WriteEndElement();
            }
            finally
            {
                editor.IsOpen = iIsOpen;
            }
        }

        /// <summary>Reads the contents of a Topic. Doesn't read the 'BrainFile' element
        ///     name itself, nor does it try to go to the start element. Warning: this
        ///     doesn't report the errors which are collected in fParseErrors, this is
        ///     up to the caller so that it can group this together and possibly try a
        ///     reparse first.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="editor">The editor.</param>
        public void ReadTopic(System.Xml.XmlReader reader, TextPatternEditor editor)
        {
            fEditor = editor;
            fEditor.IsOpen = true; // make certain that the editor is open, otherwise we can't add stuff.
            ReadXml(reader);
        }

        #region fields

        /// <summary>The f editor.</summary>
        private TextPatternEditor fEditor;

        /// <summary>The f current pattern.</summary>
        private PatternRule fCurrentPattern;

        /// <summary>The f insert at.</summary>
        private PatternEditorItem fInsertAt;

                                  // for when we are reading single pattern items from an xml stream (clipboard) and want to insert at a certain location.

        /// <summary>The f current out.</summary>
        private OutputPattern fCurrentOut;

        /// <summary>The f current cond list.</summary>
        private ConditionalOutputsCollection fCurrentCondList;

                                             // so we can read questions and conditionals with the same routine.
        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TopicXmlStreamer"/> class. use this constructor if you want to export multiple Topics that can
        ///     share mappings between outputs (for the 'response for' section).</summary>
        /// <param name="id">The id.</param>
        internal TopicXmlStreamer(string id = null)
        {
            ID = id;
        }

        /// <summary>Initializes a new instance of the <see cref="TopicXmlStreamer"/> class. Use this contructor if you want to import multple editors, using the
        ///     same name mappings for output patterns (so that 'responses for' can be
        ///     resolved accross multiple Topics).</summary>
        /// <param name="parseErrors">The parse Errors.</param>
        /// <param name="stillToResolve">The still To Resolve.</param>
        internal TopicXmlStreamer(System.Collections.Generic.List<ParsableTextPatternBase> parseErrors, System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> stillToResolve)
        {
            fParseErrors = parseErrors;
            fStillToResolve = stillToResolve;
        }

        /// <summary>Initializes a new instance of the <see cref="TopicXmlStreamer"/> class. 
        ///     Default constructor, for stand alone, single pattern def import/export</summary>
        internal TopicXmlStreamer()
        {
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

        #region reading

        /// <summary>Generates an object from its XML representation. Warning: this
        ///     function doesn't call<see cref="TopicXmlStreamer.ResolveForwardRefs(System.Collections.Generic.List{JaStDev.HAB.Designer.ProjectStreamingOperation.ToResolve})"/>
        ///     which has to be done for a full parse. This needs to be done
        ///     seperatly, so you can read multiple editors before trying to resolve
        ///     external references.</summary>
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

            ReadInfo(reader, fEditor.NeuronInfo);
            if (fEditor is ObjectTextPatternEditor && string.IsNullOrEmpty(fEditor.NeuronInfo.DisplayTitle) == false)
            {
                fEditor.Name = fEditor.NeuronInfo.DisplayTitle;

                    // assign the name again for ObjectTextPatternEditors, they don't have their name automtically from the neuronInfo, but need it to get correctly registered.
            }

            if (reader.Name == "local")
            {
                fEditor.IsLocal = XmlStore.ReadElement<bool>(reader, "local");
            }

            if (reader.Name.ToLower() == "topicfilters")
            {
                ReadTopicFilters(reader);
            }

            if (CheckCancelAndPos())
            {
                return;
            }

            ReadRules(reader);
            if (reader.Name.ToLower() == "questions")
            {
                // some files can have missing questions (early version).
                ReadQuestions(reader);
            }

            reader.ReadEndElement();

            // readXml is usually part of a bigger operation, reading  multiple editors, so we need to try and resolve these refs only when all have been loaded, so don't do now
            // ResolveForwardRefs();                                                                 
        }

        /// <summary>Resolves the forward refs.</summary>
        /// <param name="toProcess">To process.</param>
        public static void ResolveForwardRefs(System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> toProcess)
        {
            var iStreamer = new TopicXmlStreamer();
            iStreamer.fStillToResolve = toProcess;
            iStreamer.ResolveForwardRefs();
        }

        /// <summary>The get conditional.</summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="grp">The grp.</param>
        /// <returns>The <see cref="PatternRuleOutput"/>.</returns>
        private static PatternRuleOutput GetConditional(OutputPattern pattern, ResponsesForGroup grp)
        {
            var iCondList = pattern.RuleOutput;
            string iCond = null;
            if (iCondList != null && iCondList.Condition != null)
            {
                iCond = iCondList.Condition.Expression;
            }

            foreach (var i in grp.Conditionals)
            {
                if ((i.Condition == null && iCondList == null)
                    || (i.Condition != null && i.Condition.Expression == iCond))
                {
                    return i;
                }
            }

            var iRes = EditorsHelper.AddNewConditionalToPattern(grp.Conditionals, grp.Conditionals.Count - 2);

                // when we get here, there was not matching conditional found, so create one. we add just before the last one: this way, the last is always the empty condition + the 'toConverts' are stored in the order that they were read, so if we recreate them in the same oder, we preserver the meaning of the sequence for conditions.
            if (iCondList.Condition != null)
            {
                EditorsHelper.AddNewCondition(iRes, iCondList.Condition.Expression);
            }

            if (iCondList.Do != null)
            {
                iRes.Do = EditorsHelper.CreateDoPattern(iCondList.Do.Expression);
            }

            // foreach (DoPattern i in iCondList.DoPatterns)                                                                     //also copy over the do patterns.
            // EditorsHelper.AddNewDoPattern(iRes.DoPatterns, i.Expression);
            return iRes;
        }

        /// <summary>
        ///     tries to resolve all the 'ResponseFor' values that weren't found when
        ///     they were declared, hopefully cause the actual pattern still had to be
        ///     read in. For all invalid refererences, we create a new pattern def.
        /// </summary>
        private void ResolveForwardRefs()
        {
            System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> iToProcess;
            do
            {
                iToProcess = fStillToResolve;
                fStillToResolve = new System.Collections.Generic.List<ProjectStreamingOperation.ToResolve>();

                    // so we can still collect when they wont resolve.
                foreach (var i in iToProcess)
                {
                    ResolveResponse(i);
                }
            }
            while (fStillToResolve.Count > 0 && iToProcess.Count != fStillToResolve.Count);

            if (fStillToResolve.Count > 0)
            {
                var iStr = new System.Text.StringBuilder("Failed to find 'ResponseFor' references for: ");
                iStr.AppendLine();
                foreach (var i in fStillToResolve)
                {
                    iStr.Append(i.AddRefToPath);
                    iStr.Append(", referencing: ");
                    iStr.Append(i);
                }

                throw new System.InvalidOperationException(iStr.ToString());
            }
        }

        /// <summary>The read topic filters.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTopicFilters(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fEditor.TopicFilters.IsActive = false;
            while (reader.Name.ToLower() != "topicfilters" && reader.EOF == false)
            {
                ReadTopicFilter(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read topic filter.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadTopicFilter(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName); // name is skipped for text patterns.
            var iTemp = ReadExpression(reader);
            var iPattern = EditorsHelper.AddNewTopicFilter(fEditor.TopicFilters, iTemp);
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                iPattern.NeuronInfo.DescriptionText = iDesc;
            }

            if (iPattern.HasError)
            {
                fParseErrors.Add(iPattern);
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads the questions for the topic.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadQuestions(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fCurrentCondList = fEditor.Questions; // so we add to the correct list.
            fEditor.Questions.IsActive = false;
            while (reader.Name.ToLower() != "questions" && reader.EOF == false)
            {
                ReadConditional(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads all the patterns.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadRules(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Rules" && reader.EOF == false)
            {
                ReadPatternRule(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read pattern rule.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadPatternRule(System.Xml.XmlReader reader)
        {
            if (CheckCancelAndPos())
            {
                return;
            }

            var iIndex = -1;
            var iInsertAt = fInsertAt as PatternRule;
            if (iInsertAt != null)
            {
                iIndex = fEditor.Items.IndexOf(iInsertAt);
            }

            fCurrentPattern = EditorsHelper.MakePatternRule(fEditor, iIndex); // in case we need to do an insert.
            fCurrentPattern.IsLoaded = true;

                // make certain that the rule is loaded so that we can add stuff to the list.
            try
            {
                fCurrentPattern.TextPatterns.IsActive = false;
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                ReadInfo(reader, fCurrentPattern.NeuronInfo);
                if (Parsers.TopicsDictionary.CheckUniqueRuleName(
                    fCurrentPattern.NeuronInfo.DisplayTitle, 
                    fEditor.Item, 
                    fCurrentPattern.Item) == false)
                {
                    LogService.Log.LogWarning(
                        "Topic.Rule", 
                        string.Format(
                            "Duplicate rule name encountered: {0}. This may cause problems in patterns that try to reference this rule.", 
                            fCurrentPattern.NeuronInfo.DisplayTitle));
                }

                ReadPatterns(reader);
                ReadResponsesForGroups(reader);
                ReadConditionals(reader, fCurrentPattern.Conditionals);
                fCurrentPattern.Outputs.IsActive = false;
                ReadPatternOutputs(reader, fCurrentPattern.Outputs);
                var iIsOutSeq = false;
                if (XmlStore.TryReadElement(reader, "IsOutputSequenced", ref iIsOutSeq))
                {
                    fCurrentPattern.OutputSet.IsOutputSequenced = iIsOutSeq;
                }

                var iDo = ReadDoPatterns(reader);
                if (iDo != null)
                {
                    fCurrentPattern.Outputs.CreateOwnerLinkIfNeeded();

                        // make certain that the 'outputset' has a neuron, otherwise can't assign do 
                    fCurrentPattern.OutputSet.Do = iDo;
                }

                ReadEvaluate(reader);
                ReadCalculate(reader);
                reader.ReadEndElement();
            }
            finally
            {
                fCurrentPattern.IsLoaded = false;
            }
        }

        /// <summary>The read responses for groups.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadResponsesForGroups(System.Xml.XmlReader reader)
        {
            if (reader.Name == "ResponsesFor")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                fCurrentPattern.ResponsesFor.IsActive = false;
                while (reader.Name != "ResponsesFor" && reader.EOF == false)
                {
                    ReadResponseGroup(reader, fCurrentPattern.ResponsesFor);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read response group.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadResponseGroup(System.Xml.XmlReader reader, ResponseValuesCollection list)
        {
            if (reader.Name == "ResponseForGroup")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iNew = EditorsHelper.AddNewResponsesGroup(list);
                ReadResponsesFor(reader, iNew);
                ReadConditionals(reader, iNew.Conditionals);
                reader.ReadEndElement();
            }
        }

        /// <summary>The read calculate.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadCalculate(System.Xml.XmlReader reader)
        {
            var iName = reader.Name.ToLower();
            if (iName == "calculate")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iDo = new System.Text.StringBuilder();
                while (reader.Name.ToLower() != "calculate" && reader.EOF == false)
                {
                    ReadDoPattern(reader, iDo);
                }

                var iNew = EditorsHelper.CreateDoPattern(iDo.ToString());
                fCurrentPattern.ToCal = iNew;
                if (iNew.HasError)
                {
                    fParseErrors.Add(iNew);
                }

                reader.ReadEndElement();
            }
            else if (iName == "tocal")
            {
                fCurrentPattern.ToCal = ReadDoPattern(reader);
            }
        }

        /// <summary>The read evaluate.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadEvaluate(System.Xml.XmlReader reader)
        {
            var iName = reader.Name.ToLower();
            if (iName == "evaluate")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                var iRes = new System.Text.StringBuilder();
                while (reader.Name.ToLower() != "evaluate" && reader.EOF == false)
                {
                    ReadDoPattern(reader, iRes);
                }

                reader.ReadEndElement();
                fCurrentPattern.ToEval = EditorsHelper.CreateDoPattern(iRes.ToString());
                if (fCurrentPattern.ToEval.HasError)
                {
                    fParseErrors.Add(fCurrentPattern.ToEval);
                }
            }
            else if (iName == "eval")
            {
                fCurrentPattern.ToEval = ReadDoPattern(reader);
            }
        }

        /// <summary>The read do patterns.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="DoPattern"/>.</returns>
        private DoPattern ReadDoPatterns(System.Xml.XmlReader reader)
        {
            var iName = reader.Name.ToLower();
            if (iName == "dopatterns")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return null;
                }

                var iRes = new System.Text.StringBuilder();
                while (reader.Name != "DoPatterns" && reader.EOF == false)
                {
                    ReadDoPattern(reader, iRes);
                }

                reader.ReadEndElement();
                var iDo = EditorsHelper.CreateDoPattern(iRes.ToString());
                if (iDo.HasError)
                {
                    fParseErrors.Add(iDo);
                }

                return iDo;
            }

            if (iName == "do")
            {
                return ReadDoPattern(reader);
            }

            return null;
        }

        /// <summary>The read do pattern.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadDoPattern(System.Xml.XmlReader reader, System.Text.StringBuilder result)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName); // name is skipped for text patterns.
            var iTemp = ReadExpression(reader);
            if (string.IsNullOrEmpty(iTemp) == false)
            {
                result.AppendLine(iTemp + ";");
            }

            reader.ReadEndElement();
        }

        /// <summary>The read do pattern.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="DoPattern"/>.</returns>
        private DoPattern ReadDoPattern(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName); // name is skipped for text patterns.
            var iTemp = ReadExpression(reader);
            var iPattern = EditorsHelper.CreateDoPattern(iTemp);

                // .AddNewDoPattern(list, iTemp, list.IndexOf(fInsertAt as DoPattern));
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                iPattern.NeuronInfo.DescriptionText = iDesc;
            }

            if (iPattern.HasError)
            {
                fParseErrors.Add(iPattern);
            }

            reader.ReadEndElement();
            return iPattern;
        }

        /// <summary>The read conditionals.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadConditionals(System.Xml.XmlReader reader, ConditionalOutputsCollection list)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fCurrentCondList = list;
            list.IsActive = false;
            while (reader.Name != "Conditionals" && reader.EOF == false)
            {
                ReadConditional(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read conditional.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadConditional(System.Xml.XmlReader reader)
        {
            if (CheckCancelAndPos())
            {
                return;
            }

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName);

            var iCond = EditorsHelper.AddNewConditionalToPattern(fCurrentCondList);
            ReadCondition(reader, iCond);
            iCond.Outputs.IsActive = false;
            ReadPatternOutputs(reader, iCond.Outputs);

            var iRead = false;
            if (XmlStore.TryReadElement(reader, "IsOutputSequenced", ref iRead))
            {
                iCond.IsOutputSequenced = iRead; // false doesn't need to be saved, it's the default value.
            }

            iCond.Do = ReadDoPatterns(reader);
            reader.ReadEndElement();
        }

        /// <summary>The read condition.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cond">The cond.</param>
        /// <returns>The <see cref="ConditionPattern"/>.</returns>
        private ConditionPattern ReadCondition(System.Xml.XmlReader reader, PatternRuleOutput cond)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName); // name is skipped for text patterns.
            var iTemp = ReadExpression(reader);
            var iPattern = EditorsHelper.AddNewCondition(cond, iTemp);
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                iPattern.NeuronInfo.DescriptionText = iDesc;
            }

            if (iPattern.HasError)
            {
                fParseErrors.Add(iPattern);
            }

            reader.ReadEndElement();
            return iPattern;
        }

        /// <summary>The read patterns.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadPatterns(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Patterns" && reader.EOF == false)
            {
                ReadInput(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read input.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="InputPattern"/>.</returns>
        private InputPattern ReadInput(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            string iName;
            string iDesc;
            ReadInfo(reader, out iDesc, out iName); // name is skipped for text patterns.
            var iTemp = ReadExpression(reader);
            var iPattern = EditorsHelper.AddNewTextPattern(
                fCurrentPattern.TextPatterns, 
                iTemp, 
                fCurrentPattern.TextPatterns.IndexOf(fInsertAt as InputPattern));
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                iPattern.NeuronInfo.DescriptionText = iDesc;
            }

            string iDupMode = null;
            if (XmlStore.TryReadElement(reader, "DuplicationMode", ref iDupMode))
            {
                // if no value, default is false
                if (iDupMode == "fallback")
                {
                    iPattern.AllowDuplicate = null;
                }
                else if (iDupMode == "partial")
                {
                    iPattern.AllowDuplicate = true;
                }
            }

            if (iPattern.HasError)
            {
                fParseErrors.Add(iPattern);
            }

            reader.ReadEndElement();
            return iPattern;
        }

        /// <summary>The read pattern outputs.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadPatternOutputs(System.Xml.XmlReader reader, PatternOutputsCollection list)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Outputs" && reader.EOF == false)
            {
                ReadOutput(reader, list);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read output.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="list">The list.</param>
        private void ReadOutput(System.Xml.XmlReader reader, PatternOutputsCollection list)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iDesc = ReadDesc(reader);
            var iTemp = string.Empty;
            if (reader.Name == "Expression")
            {
                // the expression can be missing if it was empty.
                iTemp = ReadOutputExpression(reader);
            }

            var iBoolVal = true;
            fCurrentOut = EditorsHelper.AddNewOutputPattern(list, iTemp, list.IndexOf(fInsertAt as OutputPattern));
            if (XmlStore.TryReadElement(reader, "QuestionCanFollow", ref iBoolVal))
            {
                fCurrentOut.QuestionCanFollow = iBoolVal;
            }

            if (string.IsNullOrEmpty(iDesc) == false)
            {
                fCurrentOut.NeuronInfo.DescriptionText = iDesc;
            }

            if (reader.Name.ToLower() == "do")
            {
                fCurrentOut.Do = ReadDoPattern(reader);
            }

            ReadInvalidResponses(reader);
            reader.ReadEndElement();
        }

        /// <summary>The read responses for.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        private void ReadResponsesFor(System.Xml.XmlReader reader, ResponsesForGroup item)
        {
            if (reader.Name == "ResponsesForItems")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "ResponsesForItems" && reader.EOF == false)
                {
                    ReadResponseFor(reader, item);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read response for.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReadResponseFor(System.Xml.XmlReader reader, ResponsesForGroup item = null)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iIsPattern = false;
            if (reader.AttributeCount > 0)
            {
                iIsPattern = reader.GetAttribute(0).ToLower() == "pattern";

                    // if the style attribute defines 'pattern', need to read the content differently.
            }

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            if (iIsPattern)
            {
                var iValue = reader.ReadString();
                var iPattern = EditorsHelper.AddNewPatternStyleResponseFor(item.ResponseFor, iValue, -1);
                if (iPattern.HasError)
                {
                    fParseErrors.Add(iPattern);
                }
            }
            else
            {
                if (reader.Name.ToLower() == "global")
                {
                    throw new System.InvalidOperationException("response-for in global no longer supported.");

                        // ReadResponseForGlobal(reader);
                }

                ReadResponseForTopic(reader, item);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read response for topic.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReadResponseForTopic(System.Xml.XmlReader reader, ResponsesForGroup item = null)
        {
            var iTopicTxt = XmlStore.ReadElement<string>(reader, "topic");
            if (reader.Name == "rule")
            {
                var iRuleTxt = XmlStore.ReadElement<string>(reader, "rule");
                string iCondTxt = null;
                string iResponseTxt = null;
                if (reader.Name.ToLower() == "responsefor")
                {
                    iResponseTxt = XmlStore.ReadElement<string>(reader, "responseFor");
                }

                if (reader.Name.ToLower() == "condition")
                {
                    iCondTxt = XmlStore.ReadElement<string>(reader, "condition");
                }

                var iOutText = XmlStore.ReadElement<string>(reader, "output");
                var iToResolve = new ProjectStreamingOperation.ToResolve
                                     {
                                         GroupToAddRefTo = item, 
                                         AddRefTo = fCurrentOut, 
                                         Topic = iTopicTxt, 
                                         Rule = iRuleTxt, 
                                         Condition = iCondTxt, 
                                         ResponseFor = iResponseTxt, 
                                         Output = iOutText
                                     };
                iToResolve.AddRefToPath = string.Format(
                    "{0}.{1}", 
                    fEditor.Name, 
                    fCurrentPattern.NeuronInfo.DisplayTitle);
                ResolveResponseForTopic(iToResolve);
            }
            else if (reader.Name == "questions")
            {
                XmlStore.ReadElement<bool>(reader, "questions"); // there's a questions
                var iToResolve = new ProjectStreamingOperation.ToResolve
                                     {
                                         GroupToAddRefTo = item, 
                                         AddRefTo = fCurrentOut, 
                                         Topic = iTopicTxt, 
                                         Questions = true
                                     };
                if (reader.Name.ToLower() == "condition")
                {
                    iToResolve.Condition = XmlStore.ReadElement<string>(reader, "condition");
                }

                iToResolve.Output = XmlStore.ReadElement<string>(reader, "output");
                iToResolve.AddRefToPath = string.Format(
                    "{0}.{1}", 
                    fEditor.Name, 
                    fCurrentPattern.NeuronInfo.DisplayTitle);
                ResolveResponseForTopicQuestion(iToResolve);
            }
            else
            {
                throw new System.InvalidOperationException("Unexpected value in topic: " + reader.Name);
            }
        }

        // private void ReadResponseForGlobal(XmlReader reader)
        // {
        // string iCondTxt = null;
        // string iglobalTxt = XmlStore.ReadElement<string>(reader, "global");
        // if (reader.Name == "condition")
        // iCondTxt = XmlStore.ReadElement<string>(reader, "condition");
        // string iResponseForTxt = XmlStore.ReadElement<string>(reader, "output");
        // ProjectStreamingOperation.ToResolve iToResolve = new ProjectStreamingOperation.ToResolve() { AddRefTo = fCurrentOut, global = iglobalTxt, Condition = iCondTxt, Output = iResponseForTxt };
        // iToResolve.AddRefToPath = string.Format("{0}.{1}.{2}", fEditor.Name, fCurrentPattern.NeuronInfo.DisplayTitle, fCurrentOut.Expression);
        // ResolveResponseForGlobal(iToResolve);
        // }

        /// <summary>The resolve response.</summary>
        /// <param name="toResolve">The to resolve.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ResolveResponse(ProjectStreamingOperation.ToResolve toResolve)
        {
            fCurrentOut = toResolve.AddRefTo;
            if (string.IsNullOrEmpty(toResolve.global))
            {
                if (toResolve.Questions == false)
                {
                    ResolveResponseForTopic(toResolve);
                }
                else
                {
                    ResolveResponseForTopicQuestion(toResolve);
                }
            }
            else
            {
                throw new System.InvalidOperationException("Response-for in global section no longer supported.");
            }

            // ResolveResponseForGlobal(toResolve);
        }

        /// <summary>The resolve response for topic question.</summary>
        /// <param name="toResolve">The to resolve.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ResolveResponseForTopicQuestion(ProjectStreamingOperation.ToResolve toResolve)
        {
            try
            {
                var iTopic = Parsers.TopicsDictionary.Get(toResolve.Topic) as NeuronCluster;
                if (iTopic != null)
                {
                    NeuronCluster iParents = null;
                    var iConds = iTopic.FindFirstOut((ulong)PredefinedNeurons.Questions) as NeuronCluster;
                    if (iConds != null)
                    {
                        // could be that the questions aren't loaded yet.
                        System.Collections.Generic.List<NeuronCluster> iChildren;
                        using (var ilist = iConds.Children) iChildren = ilist.ConvertTo<NeuronCluster>();
                        foreach (var i in iChildren)
                        {
                            var iCond = i.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                            if ((iCond != null && iCond.Text.Equals(toResolve.Condition))
                                || (iCond == null && string.IsNullOrEmpty(toResolve.Condition)))
                            {
                                iParents = i;
                                break;
                            }
                        }

                        Factories.Default.CLists.Recycle(iChildren);
                    }

                    if (iParents != null)
                    {
                        System.Collections.Generic.List<TextNeuron> iChildren;
                        using (var iList = iParents.Children) iChildren = iList.ConvertTo<TextNeuron>();
                        foreach (var i in iChildren)
                        {
                            if (i != null && i.Text == toResolve.Output)
                            {
                                if (toResolve.GroupToAddRefTo != null)
                                {
                                    toResolve.GroupToAddRefTo.ResponseFor.Add(new ResponseForOutput(i));
                                }
                                else
                                {
                                    throw new System.InvalidOperationException();

                                        // very old files had a responsefor underneath the outputs, no longer supported.
                                }

                                return;
                            }
                        }
                    }
                }

                fStillToResolve.Add(toResolve);

                    // when we get here, we couldn't find the reference, need to try again later, when everything has been imported.
            }
            catch
            {
                fStillToResolve.Add(toResolve);

                    // an error also causes a retry. Common errors are topic or rule not found.
            }
        }

        /// <summary>The resolve response for topic.</summary>
        /// <param name="toResolve">The to resolve.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ResolveResponseForTopic(ProjectStreamingOperation.ToResolve toResolve)
        {
            try
            {
                var iTopic = Parsers.TopicsDictionary.Get(toResolve.Topic) as NeuronCluster;
                if (iTopic != null)
                {
                    var iRule = Parsers.TopicsDictionary.GetRule(iTopic, toResolve.Rule) as NeuronCluster;
                    if (iRule != null)
                    {
                        NeuronCluster iParents = null;
                        NeuronCluster iConds;
                        if (string.IsNullOrEmpty(toResolve.ResponseFor) == false)
                        {
                            iConds = ResolveFromResponseToOuts(toResolve, iRule);
                            if (iConds != null)
                            {
                                iParents = ResolveToParentListFromCond(iConds, toResolve.Condition);
                            }
                        }
                        else if (string.IsNullOrEmpty(toResolve.Condition) == false)
                        {
                            iConds = iRule.FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
                            iParents = ResolveToParentListFromCond(iConds, toResolve.Condition);
                        }
                        else
                        {
                            iParents = iRule.FindFirstOut((ulong)PredefinedNeurons.TextPatternOutputs) as NeuronCluster;
                        }

                        if (iParents != null)
                        {
                            using (var iList = iParents.Children)
                                foreach (var i in iList.ConvertTo<TextNeuron>())
                                {
                                    if (i != null && i.Text == toResolve.Output)
                                    {
                                        if (toResolve.GroupToAddRefTo != null)
                                        {
                                            toResolve.GroupToAddRefTo.ResponseFor.Add(new ResponseForOutput(i));
                                        }
                                        else
                                        {
                                            throw new System.InvalidOperationException();

                                                // very old files had a response-for underneath the output, no longer supported.
                                        }

                                        return;
                                    }
                                }
                        }
                    }
                }

                // when we get here, we couldn't find the reference, need to try again later, when everything has been imported.
                fStillToResolve.Add(toResolve);
            }
            catch
            {
                fStillToResolve.Add(toResolve);

                    // an error also causes a retry. Common errors are topic or rule not found.
            }
        }

        /// <summary>The resolve to parent list from cond.</summary>
        /// <param name="iConds">The i conds.</param>
        /// <param name="cond">The cond.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ResolveToParentListFromCond(NeuronCluster iConds, string cond)
        {
            System.Collections.Generic.List<NeuronCluster> iChildren;
            using (var iList = iConds.Children) iChildren = iList.ConvertTo<NeuronCluster>();
            try
            {
                if (string.IsNullOrEmpty(cond) == false)
                {
                    foreach (var i in iChildren)
                    {
                        var iCond = i.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                        if (iCond != null && iCond.Text == cond)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    for (var i = iChildren.Count - 1; i >= 0; i--)
                    {
                        var iCl = iChildren[0];
                        var iCond = iCl.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                        if (iCond == null || string.IsNullOrEmpty(iCond.Text))
                        {
                            return iCl;
                        }
                    }
                }
            }
            finally
            {
                Factories.Default.CLists.Recycle(iChildren);
            }

            return null;
        }

        /// <summary>The resolve from response to outs.</summary>
        /// <param name="toResolve">The to resolve.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ResolveFromResponseToOuts(
            ProjectStreamingOperation.ToResolve toResolve, 
            NeuronCluster rule)
        {
            var iResponsesGrp = rule.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
            if (iResponsesGrp != null)
            {
                System.Collections.Generic.List<NeuronCluster> iConds;
                using (var iChildren = iResponsesGrp.Children) iConds = iChildren.ConvertTo<NeuronCluster>();
                try
                {
                    foreach (var i in iConds)
                    {
                        var iResponsesFor = i.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
                        if (iResponsesFor != null)
                        {
                            System.Collections.Generic.List<TextNeuron> iOuts;
                            using (var iChildren = iResponsesGrp.Children) iOuts = iChildren.ConvertTo<TextNeuron>();
                            foreach (var u in iOuts)
                            {
                                if (u.Text == toResolve.ResponseFor)
                                {
                                    return i;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iConds);
                }
            }

            return null;
        }

        // private void ResolveResponseForGlobal(ProjectStreamingOperation.ToResolve toResolve)
        // {
        // NeuronCluster iParents = null;
        // if (toResolve.global == "repetition")
        // {
        // NeuronCluster iRoot = Brain.Current[(ulong)PredefinedNeurons.RepeatOutputPatterns] as NeuronCluster;
        // Debug.Assert(iRoot != null);
        // List<Neuron> iChildren;
        // using (ChildrenAccessor iList = iRoot.Children)
        // iChildren = iList.ConvertTo<Neuron>();
        // foreach (Neuron i in iChildren)
        // {
        // TextNeuron iCond = i.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
        // if (iCond != null && iCond.Text == toResolve.Condition)
        // {
        // iParents = i as NeuronCluster;
        // break;
        // }
        // }
        // Factories.Default.NLists.Recycle(iChildren);
        // }
        // else if (toResolve.global == "ConversationStarts")
        // iParents = Brain.Current[(ulong)PredefinedNeurons.ConversationStarts] as NeuronCluster;
        // else if (toResolve.global == "fallback")
        // iParents = Brain.Current[(ulong)PredefinedNeurons.ResponsesForEmptyParse] as NeuronCluster;
        // else
        // throw new InvalidOperationException("Unknown value for 'global': " + toResolve.global);
        // if (iParents != null)
        // {
        // using (ChildrenAccessor iList = iParents.Children)
        // foreach (TextNeuron i in iList.ConvertTo<TextNeuron>())
        // {
        // if (i != null && i.Text == toResolve.Condition)
        // {
        // fCurrentOut.ResponseFor.Add(new ResponseForOutput(i));
        // return;
        // }
        // }
        // }
        // //when we get here, we couldn't find the reference, need to try again later, when everything has been imported.
        // fStillToResolve.Add(toResolve);
        // }

        /// <summary>The read invalid responses.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadInvalidResponses(System.Xml.XmlReader reader)
        {
            if (reader.Name == "InvalidResponses")
            {
                var iSeqStr = reader.GetAttribute("IsSequenced");
                bool iSeq;
                if (bool.TryParse(iSeqStr, out iSeq))
                {
                    fCurrentOut.IsInvalidResponsesSequenced = iSeq;
                }

                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                fCurrentOut.InvalidResponses.IsActive = false;
                while (reader.Name != "InvalidResponses" && reader.EOF == false)
                {
                    ReadInvalid(reader);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read invalid.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="InvalidPatternResponse"/>.</returns>
        private InvalidPatternResponse ReadInvalid(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iInvalid = EditorsHelper.AddNewInvalidPatternResponse(
                fCurrentOut.InvalidResponses, 
                null, 
                fCurrentOut.InvalidResponses.IndexOf(fInsertAt as InvalidPatternResponse));

                // create with null, cause we need the object before reading the info.
            if (reader.AttributeCount > 0)
            {
                var iRR = reader.GetAttribute("RequiresResponse");
                bool iRRBool;
                if (bool.TryParse(iRR, out iRRBool))
                {
                    iInvalid.RequiresResponse = iRRBool;
                }
            }

            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            iInvalid.NeuronInfo.DescriptionText = ReadDesc(reader);
            var iTemp = ReadExpression(reader);
            iInvalid.Expression = iTemp;
            if (iInvalid.HasError)
            {
                fParseErrors.Add(iInvalid);
            }

            reader.ReadEndElement();
            return iInvalid;
        }

        #endregion

        #region writing

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (fEditor is ObjectTextPatternEditor)
            {
                // need to make certain that the topic has a name + this will update the name of the editor.
                ResolveTopicName(fEditor.Items.Cluster);

                    // attached topics use the name of the topic cluster, not that of the object.
            }
            else
            {
                ResolveTopicName(fEditor.Item);
            }

            WriteInfo(writer, fEditor.NeuronInfo);
            if (ID != null)
            {
                XmlStore.WriteElement(writer, "ID", ID);
            }

            if (fEditor.IsLocal)
            {
                XmlStore.WriteElement(writer, "local", true);
            }

            WriteTopicFilters(writer);
            WritePatternRules(writer);
            WriteQuestions(writer);
        }

        /// <summary>The write topic filters.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteTopicFilters(System.Xml.XmlWriter writer)
        {
            if (fEditor.TopicFilters != null && fEditor.TopicFilters.Count > 0)
            {
                writer.WriteStartElement("TopicFilters");
                foreach (var i in fEditor.TopicFilters)
                {
                    WriteTopicFilter(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write topic filter.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="i">The i.</param>
        private void WriteTopicFilter(System.Xml.XmlWriter writer, TopicFilterPattern i)
        {
            writer.WriteStartElement("Pattern");
            WriteInfo(writer, i.NeuronInfo.DescriptionText, null);
            if (string.IsNullOrEmpty(i.Expression) == false)
            {
                WriteExpression(writer, i.Expression);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write questions.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteQuestions(System.Xml.XmlWriter writer)
        {
            if (fEditor.Questions != null && fEditor.Questions.Count > 0)
            {
                writer.WriteStartElement("Questions");
                foreach (var i in fEditor.Questions)
                {
                    WriteConditional(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write pattern rules.</summary>
        /// <param name="writer">The writer.</param>
        private void WritePatternRules(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Rules");
            foreach (var i in fEditor.Items)
            {
                var iLoaded = i.IsLoaded;
                try
                {
                    i.IsLoaded = true; // if we don't load the rules, can't export the data.
                    WritePatternRule(writer, i);
                }
                finally
                {
                    i.IsLoaded = iLoaded;
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>The write pattern rule.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="pattern">The pattern.</param>
        private void WritePatternRule(System.Xml.XmlWriter writer, PatternRule pattern)
        {
            ResolveRuleName(fEditor.Item, pattern.Item);
            writer.WriteStartElement("Rule");
            WriteInfo(writer, pattern.NeuronInfo);
            fCurrentPattern = pattern;
            WriteInputPatterns(writer);
            WriteResponsesGroups(writer);
            WriteConditionals(writer, pattern.Conditionals);
            WriteOutputs(writer, fCurrentPattern.Outputs);
            XmlStore.WriteElement(writer, "IsOutputSequenced", fCurrentPattern.OutputSet.IsOutputSequenced);
            if (fCurrentPattern.Do != null)
            {
                WriteDoPattern(writer, fCurrentPattern.Do);
            }
            else if (fCurrentPattern.DoPatterns != null)
            {
                WriteDoPatterns(writer, fCurrentPattern.DoPatterns);
            }

            WriteToEvaluate(writer);
            WriteToCalculate(writer);
            writer.WriteEndElement();
        }

        /// <summary>The write responses groups.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteResponsesGroups(System.Xml.XmlWriter writer)
        {
            if (fCurrentPattern.ResponsesFor != null && fCurrentPattern.ResponsesFor.Count > 0)
            {
                writer.WriteStartElement("ResponsesFor");
                foreach (var i in fCurrentPattern.ResponsesFor)
                {
                    WriteResponsesForGroup(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write responses for group.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="grp">The grp.</param>
        private void WriteResponsesForGroup(System.Xml.XmlWriter writer, ResponsesForGroup grp)
        {
            writer.WriteStartElement("ResponseForGroup");
            WriteResponsesFor(writer, grp.ResponseFor);
            WriteConditionals(writer, grp.Conditionals);
            writer.WriteEndElement();
        }

        /// <summary>The write conditionals.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="list">The list.</param>
        private void WriteConditionals(System.Xml.XmlWriter writer, ConditionalOutputsCollection list)
        {
            writer.WriteStartElement("Conditionals");
            foreach (var i in list)
            {
                WriteConditional(writer, i);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write conditional.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cond">The cond.</param>
        private void WriteConditional(System.Xml.XmlWriter writer, PatternRuleOutput cond)
        {
            writer.WriteStartElement("Conditional");
            WriteInfo(writer, cond.NeuronInfo.DescriptionText, null);

            WriteCondition(writer, cond.Condition);
            WriteOutputs(writer, cond.Outputs);
            XmlStore.WriteElement(writer, "IsOutputSequenced", cond.IsOutputSequenced);
            if (cond.Do != null)
            {
                WriteDoPattern(writer, cond.Do);
            }
            else if (cond.DoPatterns != null)
            {
                WriteDoPatterns(writer, cond.DoPatterns);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write condition.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cond">The cond.</param>
        private void WriteCondition(System.Xml.XmlWriter writer, ConditionPattern cond)
        {
            writer.WriteStartElement("Condition");
            if (cond != null)
            {
                // could be null if there was just another rebuild done without closing the project first.
                WriteInfo(writer, cond.NeuronInfo.DescriptionText, null);
                if (string.IsNullOrEmpty(cond.Expression) == false)
                {
                    WriteExpression(writer, cond.Expression);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>The write to evaluate.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteToEvaluate(System.Xml.XmlWriter writer)
        {
            if (fCurrentPattern.ToEval != null)
            {
                WriteDoPattern(writer, fCurrentPattern.ToEval, "Eval");
            }
            else if (fCurrentPattern.ToEvaluate != null)
            {
                writer.WriteStartElement("Evaluate");
                foreach (var i in fCurrentPattern.ToEvaluate)
                {
                    WriteDoPattern(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write to calculate.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteToCalculate(System.Xml.XmlWriter writer)
        {
            if (fCurrentPattern.ToCal != null)
            {
                WriteDoPattern(writer, fCurrentPattern.ToCal, "ToCal");
            }

            if (fCurrentPattern.ToCalculate != null)
            {
                writer.WriteStartElement("Calculate");
                foreach (var i in fCurrentPattern.ToCalculate)
                {
                    WriteDoPattern(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write do patterns.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="list">The list.</param>
        private void WriteDoPatterns(System.Xml.XmlWriter writer, DoPatternCollection list)
        {
            if (list != null)
            {
                writer.WriteStartElement("DoPatterns");
                foreach (var i in list)
                {
                    WriteDoPattern(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write do pattern.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="i">The i.</param>
        /// <param name="label">The label.</param>
        private void WriteDoPattern(System.Xml.XmlWriter writer, DoPattern i, string label = "Do")
        {
            writer.WriteStartElement(label);
            WriteInfo(writer, i.NeuronInfo.DescriptionText, null);
            if (string.IsNullOrEmpty(i.Expression) == false)
            {
                WriteExpression(writer, i.Expression);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write input patterns.</summary>
        /// <param name="writer">The writer.</param>
        private void WriteInputPatterns(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Patterns");
            foreach (var i in fCurrentPattern.TextPatterns)
            {
                WriteInput(writer, i);
            }

            writer.WriteEndElement();
        }

        /// <summary>The write input.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="i">The i.</param>
        private void WriteInput(System.Xml.XmlWriter writer, InputPattern i)
        {
            writer.WriteStartElement("Pattern");
            WriteInfo(writer, i.NeuronInfo.DescriptionText, null);
            if (string.IsNullOrEmpty(i.Expression) == false)
            {
                WriteExpression(writer, i.Expression);
            }

            if (i.AllowDuplicate.HasValue == false)
            {
                XmlStore.WriteElement(writer, "DuplicationMode", "fallback");
            }
            else if (i.AllowDuplicate.Value)
            {
                XmlStore.WriteElement(writer, "DuplicationMode", "partial");
            }

            writer.WriteEndElement();
        }

        /// <summary>The write outputs.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="outputs">The outputs.</param>
        private void WriteOutputs(System.Xml.XmlWriter writer, PatternOutputsCollection outputs)
        {
            writer.WriteStartElement("Outputs");
            foreach (var i in outputs)
            {
                WriteOutput(writer, i);
            }

            writer.WriteEndElement();
        }

        /// <summary>we always generate a name for an <paramref name="output"/> when it is
        ///     used as an 'AnswerFor' value.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="output">The output.</param>
        private void WriteOutput(System.Xml.XmlWriter writer, OutputPattern output)
        {
            writer.WriteStartElement("Output");

            // Neuron iFound = output.Item.FindFirstClusteredBy((ulong)PredefinedNeurons.ResponseForOutputs);
            // string iName = null;
            // if (iFound != null)                                                                                      //generate a name, we use UID for this.
            // iName = GetUniqueGuid((TextNeuron)output.Item);
            WriteDesc(writer, output.NeuronInfo.DescriptionText);
            if (string.IsNullOrEmpty(output.Expression) == false)
            {
                WriteOutputExpression(writer, output.Expression);
            }

            XmlStore.WriteElement(writer, "QuestionCanFollow", output.QuestionCanFollow);
            if (output.Do != null)
            {
                WriteDoPattern(writer, output.Do);
            }

            WriteInvalidResponses(writer, output);
            writer.WriteEndElement();
        }

        /// <summary>The write responses for.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="output">The output.</param>
        private void WriteResponsesFor(System.Xml.XmlWriter writer, ResponsesForCollection output)
        {
            if (output.Count > 0)
            {
                writer.WriteStartElement("ResponsesForItems");
                foreach (var i in output)
                {
                    WriteResponseFor(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write response for.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="response">The response.</param>
        private void WriteResponseFor(System.Xml.XmlWriter writer, ResponseForOutput response)
        {
            writer.WriteStartElement("ResponseFor");

            var iParent = response.Item.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternOutputs);
            if (iParent != null)
            {
                WriteResponseFor(writer, iParent, (TextNeuron)response.Item);

                    // it's a regular response-for (reference to an outputu pattern).
            }
            else
            {
                WriteResponseFor(writer, (TextNeuron)response.Item); // it's another pattern definition.
            }

            writer.WriteEndElement();
        }

        /// <summary>writes a response-for that is it's own pattern and not a reference to
        ///     another pattern.</summary>
        /// <param name="writer"></param>
        /// <param name="item"></param>
        private void WriteResponseFor(System.Xml.XmlWriter writer, TextNeuron item)
        {
            writer.WriteAttributeString("style", "pattern");
            writer.WriteValue(item.Text);
        }

        /// <summary>Writes the path to a textNeuron, when it is part of a regular text
        ///     pattern editor, or a repetition.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="item">The item.</param>
        private void WriteResponseFor(System.Xml.XmlWriter writer, NeuronCluster parent, TextNeuron item)
        {
            var iRule = parent.FindFirstIn((ulong)PredefinedNeurons.TextPatternOutputs) as NeuronCluster;
            NeuronCluster iTopic = null;
            TextNeuron iResponseFor = null;

            TextNeuron iCondition = null;
            if (iRule == null)
            {
                iCondition = parent.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
                var iTemp = parent.FindFirstClusteredBy((ulong)PredefinedNeurons.Condition);
                if (iTemp != null)
                {
                    // if the parent of the output isn't clustered by a condition (or linked to a rule), it's part of a repetition 
                    var iNewRule = iTemp.FindFirstIn((ulong)PredefinedNeurons.Condition) as NeuronCluster;
                    if (iNewRule != null)
                    {
                        // if the condition is not attached to a rule, it's attached to the topic (questions section) or another responses-for section.
                        iRule = iTemp.FindFirstIn((ulong)PredefinedNeurons.Condition) as NeuronCluster;
                        iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
                    }
                    else
                    {
                        var iResponseCluster =
                            iTemp.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
                        if (iResponseCluster != null)
                        {
                            iResponseFor = iResponseCluster.FindFirstChild() as TextNeuron;
                            iResponseCluster = iTemp.FindFirstClusteredBy((ulong)PredefinedNeurons.ResponseForOutputs);
                            if (iResponseCluster != null)
                            {
                                iRule =
                                    iResponseCluster.FindFirstIn((ulong)PredefinedNeurons.ResponseForOutputs) as
                                    NeuronCluster;
                            }
                            else
                            {
                                throw new System.InvalidOperationException(
                                    "invalid response-for path: can't generated it.");
                            }

                            iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
                        }
                        else
                        {
                            iTopic = iTemp.FindFirstIn((ulong)PredefinedNeurons.Questions) as NeuronCluster;
                            iRule = null;
                        }
                    }
                }
            }
            else
            {
                iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
            }

            if (iTopic != null)
            {
                XmlStore.WriteElement(writer, "topic", ResolveTopicName(iTopic));
                if (iRule != null)
                {
                    XmlStore.WriteElement(writer, "rule", ResolveRuleName(iTopic, iRule));
                }
                else
                {
                    XmlStore.WriteElement(writer, "questions", true);
                }
            }
            else
            {
                XmlStore.WriteElement(writer, "global", "repetition");
            }

            if (iResponseFor != null)
            {
                XmlStore.WriteElement(writer, "responseFor", iResponseFor.Text);
            }

            if (iCondition != null)
            {
                XmlStore.WriteElement(writer, "condition", iCondition.Text);
            }

            XmlStore.WriteElement(writer, "output", item.Text);
        }

        /// <summary>The resolve rule name.</summary>
        /// <param name="topic">The topic.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ResolveRuleName(Neuron topic, Neuron rule)
        {
            var iInfo = BrainData.Current.NeuronInfo[rule];
            System.Diagnostics.Debug.Assert(iInfo != null);
            if (string.IsNullOrEmpty(iInfo.Title))
            {
                iInfo.DisplayTitle = Parsers.TopicsDictionary.GetUniqueRuleName("Rule", topic);
            }

            return iInfo.DisplayTitle;
        }

        /// <summary>Resolves the name of the topic: if it doesn't have a name assigned,
        ///     generated a unique name. This is required in case the name got lost
        ///     for some reaseon (old project perhaps).</summary>
        /// <param name="topic">The topic.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ResolveTopicName(Neuron topic)
        {
            var iInfo = BrainData.Current.NeuronInfo[topic];
            System.Diagnostics.Debug.Assert(iInfo != null);
            if (string.IsNullOrEmpty(iInfo.Title))
            {
                iInfo.DisplayTitle = Parsers.TopicsDictionary.GetUnique("Topic");
            }

            return iInfo.DisplayTitle;
        }

        /// <summary>The write invalid responses.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="output">The output.</param>
        private void WriteInvalidResponses(System.Xml.XmlWriter writer, OutputPattern output)
        {
            if (output.InvalidResponses.Count > 0)
            {
                writer.WriteStartElement("InvalidResponses");
                writer.WriteAttributeString("IsSequenced", output.IsInvalidResponsesSequenced.ToString());
                foreach (var i in output.InvalidResponses)
                {
                    WriteInvalid(writer, i);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write invalid.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="invalid">The invalid.</param>
        private void WriteInvalid(System.Xml.XmlWriter writer, InvalidPatternResponse invalid)
        {
            writer.WriteStartElement("InvalidResponse");
            writer.WriteAttributeString("RequiresResponse", invalid.RequiresResponse.ToString());
            WriteDesc(writer, invalid.NeuronInfo.DescriptionText);
            if (string.IsNullOrEmpty(invalid.Expression) == false)
            {
                WriteExpression(writer, invalid.Expression);
            }

            writer.WriteEndElement();
        }

        #endregion

        #endregion
    }
}