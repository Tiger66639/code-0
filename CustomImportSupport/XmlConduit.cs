// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlConduit.cs" company="">
//   
// </copyright>
// <summary>
//   The read state.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>The read state.</summary>
    internal enum ReadState
    {
        /// <summary>The none.</summary>
        None, 

        /// <summary>The reading attrib.</summary>
        ReadingAttrib, 

        /// <summary>The reading text.</summary>
        ReadingText, // just found text, but haven't checked yet if the element only contains text or is mixed.

        /// <summary>The processing text.</summary>
        ProcessingText // found text in a mixed element, sending BeginBlock then text value first.
    }

    /// <summary>
    ///     stores some data for an element that is needs to be buffered during the reading of the xml file. This is used in a
    ///     stack
    ///     so that the previous values get stored.
    /// </summary>
    internal class ElementData
    {
        /// <summary>The rendered start.</summary>
        public bool RenderedStart;
    }

    /// <summary>
    ///     a generic data conduit for xml files.
    /// </summary>
    public class XmlConduit : BaseConduit
    {
        /// <summary>
        ///     Gets or sets the current parse state: normal or reading attributes.
        /// </summary>
        /// <value>
        ///     The state of the cur.
        /// </value>
        internal ReadState CurState
        {
            get
            {
                return fCurState;
            }

            set
            {
                fCurState = value;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the conduit is open or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public override bool IsOpen
        {
            get
            {
                return fReader != null;
            }
        }

        /// <summary>
        ///     Gets a value to indicate if the process needs to be started with a location and destination or only a location
        ///     argument.
        /// </summary>
        /// <returns>
        ///     true or false
        /// </returns>
        public override bool NeedsDestination()
        {
            return false;
        }

        /// <summary>Opens the file at the specified location. So that data can be read by the <see cref="ICustomConduit.ReadLine"/>
        ///     function.
        ///     This is for a pull scenario.</summary>
        /// <param name="location">The location.</param>
        public override void Open(string location)
        {
            fFile = new System.IO.FileStream(location, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            var iSettings = CreateReaderSettings();
            fReader = System.Xml.XmlReader.Create(fFile, iSettings);
            fPrevEl = null; // make certain that this is correctly reset.
        }

        /// <summary>Creates a common reader settings object.</summary>
        /// <returns>The <see cref="XmlReaderSettings"/>.</returns>
        private System.Xml.XmlReaderSettings CreateReaderSettings()
        {
            var iSettings = new System.Xml.XmlReaderSettings();
            iSettings.IgnoreComments = true;
            iSettings.IgnoreProcessingInstructions = true;
            iSettings.IgnoreWhitespace = true;
            return iSettings;
        }

        /// <summary>
        ///     Closes this instance.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (fReader != null)
            {
                fReader.Close();
            }

            if (fFile != null)
            {
                fFile.Close();
            }

            fReader = null;
            fFile = null;
            fPrevEl = null;
            fPrevEls.Clear();
        }

        /// <summary>reads a single line of data from the file.
        ///     This is used together with <see cref="ICustomConduit.Opern"/> and  <see cref="ICustomConduit.Close"/></summary>
        /// <param name="result"></param>
        /// <returns>false if no more line could be read, otherwise true.</returns>
        public override bool ReadLine(System.Collections.Generic.List<Neuron> result)
        {
            var iDoRead = true; // so we can switch of the reader before doing a goto.
            var iProc = Processor.CurrentProcessor;

                // a bit faster if we make a local copy. We need to know the processor so we can freeze newly created neurons and prevent memory leaks.
            if (CancelRequested)
            {
                return false;
            }

            start:
            try
            {
                if (fCurState == ReadState.None)
                {
                    if (iDoRead)
                    {
                        fReader.Read();
                    }
                    else
                    {
                        iDoRead = true; // only skip 1 time.
                    }

                    if (fReader.EOF == false)
                    {
                        switch (fReader.NodeType)
                        {
                            case System.Xml.XmlNodeType.Element:
                                if (fReader.IsEmptyElement)
                                {
                                    if (fReader.HasAttributes)
                                    {
                                        // no value, perhaps some attribs.
                                        result.Add(BrainHelper.GetNeuronFor(fReader.Name, Culture, iProc));
                                        result.Add(BrainHelper.GetNeuronFor(fReader.Value, Culture, iProc));
                                        CurState = ReadState.ReadingAttrib; // try to read the attribs if there are any.
                                    }
                                    else
                                    {
                                        result.Add(BrainHelper.GetNeuronFor(fReader.Name, Culture, iProc));
                                        result.Add(Brain.Current[(ulong)PredefinedNeurons.Empty]);
                                    }

                                    fPrevEls.Push(new ElementData { RenderedStart = false });

                                        // start element has not yet been rendered for this item.
                                }
                                else
                                {
                                    var iDoJump = false;
                                    if (string.IsNullOrEmpty(fPrevEl) == false && fPrevEls.Peek().RenderedStart == false)
                                    {
                                        // if the prev element wasn't closed yet, we have a sub element.
                                        result.Add(BrainHelper.GetNeuronFor(fPrevEl, Culture, iProc));
                                        result.Add(Brain.Current[(ulong)PredefinedNeurons.BeginTextBlock]);

                                            // it's an element with sub elements, so give a value that indicates that a group has begun.
                                        fPrevEls.Peek().RenderedStart = true;
                                    }
                                    else
                                    {
                                        iDoJump = true;

                                            // if there was no prev element yet, don't have anything to render yet, so jump to the start again.
                                    }

                                    fPrevEl = fReader.Name;
                                    if (fReader.HasAttributes)
                                    {
                                        CurState = ReadState.ReadingAttrib; // try to read the attribs if there are any.
                                    }

                                    fPrevEls.Push(new ElementData { RenderedStart = false });

                                        // start element has not yet been rendered for this item.
                                    if (iDoJump)
                                    {
                                        goto start;
                                    }
                                }

                                break;
                            case System.Xml.XmlNodeType.EndElement:
                                fPrevEl = null;

                                    // reset the previous element when one is closed, this way we know that a closing has been found, so that a new element can begin.

                                // fFoundText = false;
                                if (fPrevEls.Peek().RenderedStart)
                                {
                                    // it's a text element that had attributes (cause the startBlock was rendered), so still need render a closeblock
                                    result.Add(BrainHelper.GetNeuronFor(fReader.Name, Culture, iProc));
                                    result.Add(Brain.Current[(ulong)PredefinedNeurons.EndTextBlock]);
                                }
                                else
                                {
                                    goto start;

                                        // if we found a text element, there shouldn't be any other children and don't need to close the element.
                                }

                                break;
                            case System.Xml.XmlNodeType.Text:

                                // fFoundText = true;
                                if (fPrevEls.Peek().RenderedStart)
                                {
                                    result.Add(Brain.Current[(ulong)PredefinedNeurons.Value]);

                                        // if we have already rendered the element with the 'starblock', use 'statics.value' for the text.
                                    result.Add(BrainHelper.GetNeuronFor(fReader.Value, Culture, iProc));
                                }
                                else
                                {
                                    fText = fReader.Value;
                                    CurState = ReadState.ReadingText;
                                    goto start; // haven't actually presented any data yet, so go back to the start.
                                }

                                break;
                        }

                        return true;
                    }

                    return false;
                }

                if (fCurState == ReadState.ReadingText)
                {
                    // we just found some text, but aren't certain if the element that contains the text only contains text, or is mixed, so check forthis. If it is mixed, first send
                    fReader.Read();
                    if (fReader.NodeType == System.Xml.XmlNodeType.EndElement)
                    {
                        result.Add(BrainHelper.GetNeuronFor(fPrevEl, Culture, iProc));
                        result.Add(BrainHelper.GetNeuronFor(fText, Culture, iProc));
                        CurState = ReadState.None;
                    }
                    else
                    {
                        result.Add(BrainHelper.GetNeuronFor(fPrevEl, Culture, iProc));
                        result.Add(Brain.Current[(ulong)PredefinedNeurons.BeginTextBlock]);

                            // it's an element with sub elements, so give a value that indicates that a group has begun.
                        fCurState = ReadState.ProcessingText;

                            // don't assign to CurState, this would reset fRenderedStart.
                        fPrevEls.Peek().RenderedStart = true;
                    }

                    return true;
                }

                if (fCurState == ReadState.ProcessingText)
                {
                    // handling text in a mixed element: if fText is still assigned, send this. If it is not assigned, process the next element (which is already parsed by the xmlreader).
                    if (fText != null)
                    {
                        result.Add(Brain.Current[(ulong)PredefinedNeurons.Value]);

                            // if we have already rendered the element with the 'starblock', use 'statics.value' for the text.
                        result.Add(BrainHelper.GetNeuronFor(fText, Culture, iProc));
                        fText = null;
                    }
                    else
                    {
                        CurState = ReadState.None;

                            // this makes certain that RenderedStart is reset (the start of any new element isn't rendered yet.
                        iDoRead = false;

                            // don't read, we already advanced to reader to find out if there was only text in the element or something more.
                        goto start;
                    }

                    return true;
                }

                if (fPrevEls.Peek().RenderedStart == false)
                {
                    // render the beginblock if we need to render attribs and haven't started the block yet.
                    result.Add(BrainHelper.GetNeuronFor(fPrevEl, Culture, iProc));
                    result.Add(Brain.Current[(ulong)PredefinedNeurons.BeginTextBlock]);

                        // it's an element with sub elements, so give a value that indicates that a group has begun.
                    fPrevEls.Peek().RenderedStart = true;
                    return true;
                }

                if (fReader.MoveToNextAttribute())
                {
                    result.Add(BrainHelper.GetNeuronFor(fReader.Name, Culture, iProc));
                    result.Add(BrainHelper.GetNeuronFor(fReader.Value, Culture, iProc));
                    return true;
                }

                fCurState = ReadState.None; // no more attributes, so go to next node.
                goto start;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Xml conduit", e.ToString());
                return false;
            }
        }

        #region fields

        /// <summary>The f reader.</summary>
        private System.Xml.XmlReader fReader;

        /// <summary>The f file.</summary>
        private System.IO.FileStream fFile;

        /// <summary>The f cur state.</summary>
        private ReadState fCurState = ReadState.None;

        /// <summary>The f prev els.</summary>
        private readonly System.Collections.Generic.Stack<ElementData> fPrevEls =
            new System.Collections.Generic.Stack<ElementData>();

        /// <summary>The f prev el.</summary>
        private string fPrevEl;

        /// <summary>The f text.</summary>
        private string fText;

                       // when an element has mixed content: sub elements and text, but starts with text, we need to figure this out, so the text gets buffered

        // bool fFoundText = false;
        #endregion
    }
}