// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextSin.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the way that that the text channel will process input text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    #region inner types

    /// <summary>
    ///     Defines the way that that the text channel will process input text.
    /// </summary>
    public enum TextSinProcessMode
    {
        /// <summary>
        ///     Items are added as TextNeurons, found in the dictionary.  When a word is not found, it is
        ///     automatically added to the internal dict. Words are split automatically, using the
        ///     <see cref="Parsers.Tokenizer.Split" /> function.
        /// </summary>
        DictionaryWords, 

        /// <summary>
        ///     Items are added as a cluster of letters (so never a lookup).  This is analysed further using
        ///     more of a visual analysis (size of word, split in 3 parts,...)
        /// </summary>
        ClusterWords, 

        /// <summary>
        ///     Every letter is sent seperatly.  This algorithm mimics the audio system best. 1 single neuron is
        ///     generated, that links to each letter in the input stream, where each lette is an int neuron.
        /// </summary>
        LetterStream, 

        /// <summary>
        ///     Every word is retrieved from the dictionary and stored in a cluster, which becomes the 'to' part. From is
        ///     a normal neuron. the link is done with 'ContainsWord'.
        /// </summary>
        ClusterAndDict
    }

    #endregion

    /// <summary>
    ///     A Sin that is able to process string data as input/output.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For input: text will be transformed into TextNeurons containing (words/ints/doubles) , letter sequence clusters
    ///         or individual letters,
    ///         depending on the procesing method that is chosen.
    ///     </para>
    ///     <para>
    ///         For output (from the brain to an event): TextNeurons or 'object clusters' that contain a single textneuron or
    ///         the
    ///         <see cref="PredefinedNeurons.BeginTextBlock" /> and <see cref="PredefinedNeurons.EndTextBlock" /> if you need
    ///         to group words together into
    ///         blocks. By default, the TextSin will raise an event for each neuron that it needs to output.  However, when it
    ///         receives a 'BeginTextBlock',
    ///         it will accumulate each string (it doesn't insert spaces or anything, simply add each string to the end of the
    ///         last result) untill it receives
    ///         the EndTextBlock or again the BeginTxtBlock neuron.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ContainsWord, typeof(Neuron))]

    // create the default neuron we use as initial link types for words we receive as input.
    [NeuronID((ulong)PredefinedNeurons.ContainsDouble, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ContainsInt, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.LetterSequence, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Letter, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Verb, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Noun, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Pronoun, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Adjective, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Adverb, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Preposition, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Conjunction, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Interjection, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Article, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Determiner, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Complementizer, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.POS, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.BeginTextBlock, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.EndTextBlock, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.FirstUppercase, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.AllUppercase, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.UppercaseMap, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.AllLowerCase, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TextSin, typeof(Neuron))]
    public class TextSin : Sin
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.TextSin;
            }
        }

        #endregion

        #region Words

        /// <summary>
        ///     Gets the dictionary containing all the known words with their corresponding Neuron id.
        /// </summary>
        /// <remarks>
        ///     this is loaded from
        /// </remarks>
        public static WordsDictionary Words
        {
            get
            {
                if (fWords == null && Brain.Current != null)
                {
                    lock (fWordsLock)
                    {
                        if (fWords != null)
                        {
                            // if we are the second in row to get the lock, someone created the item before us.
                            return fWords;
                        }

                        if (Brain.Current.Storage.DataPath != null)
                        {
                            // only try to load if there was a datapath.  No datapath is possible when there is no project loaded yet.  
                            var iWords = WordsDictionary.Load(Brain.Current.Storage.DataPath);
                            fWords = iWords; // once loaded, store dict, this way the SaveDict function works ok.
                        }
                    }

                    if (fWords == null)
                    {
                        fWords = new WordsDictionary();
                    }
                }
                else if (Brain.Current == null)
                {
                    LogService.Log.LogError("TextSin.Words", "Can't load dictionray, Brain not yet loaded!");
                }

                return fWords;
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Raised when the TextSin has got some text it wants to output.
        /// </summary>
        public event OutputEventHandler<string> TextOut;

        #endregion

        /// <summary>
        ///     saves the dictionary (if there is any to the neuron entries store.
        /// </summary>
        /// <remarks>
        ///     We don't use the flush method for doing this, cuase it is global, should be handled globally.
        /// </remarks>
        public static void SaveDict()
        {
            if (fWords != null && fWords.IsChanged && Brain.Current != null)
            {
                // can't use the fWords lock at this stage, cause this could freeze everything up (saveDict happens during flush, which locks the network, but loading the dict also requires a lock on the network -> stuck.
                fWords.Save(Brain.Current.Storage.DataPath);
            }
        }

        /// <summary>
        ///     Resets the dictionary content to null.
        /// </summary>
        public static void ResetDict()
        {
            fWords = null;
        }

        /// <summary>
        ///     Need to provide an implemntation, does nothing, check <see cref="TextSin.SaveDict" /> for more info.
        /// </summary>
        public override void Flush()
        {
        }

        #region Fields

        /// <summary>The f words.</summary>
        private static WordsDictionary fWords;

        /// <summary>The f words lock.</summary>
        private static readonly object fWordsLock = new object();

                                       // this is used during the creation of the words dict, to make certain only 1 thread tries to load it.

        // static IList<ulong> fCharacters;
        /// <summary>
        ///     used to build up a text from multiple neurons that are inside a BeginTextBlock - EndTExtBlock or BeginTextBLock -
        ///     BeginTextBlock.
        ///     When null, there is no block.
        /// </summary>
        private OutputBuilder fToSend;

        /// <summary>
        ///     Contains the list of neurons that, together, create the <see cref="TextSin.fToSend" /> value.  When null, there is
        ///     no text block building.
        /// </summary>
        private System.Collections.Generic.List<Neuron> fNeuronsToSend;

        #endregion

        #region Functions

        /// <summary>Tries to parse the specified string into neurons which can be processed by the processor.</summary>
        /// <remarks>Initial implementation always uses the <see cref="ProcessMode.DictionaryWords"/> technique.</remarks>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <returns>the neuron that was used as the input event.</returns>
        public Neuron[] Process(string value, Processor processor)
        {
            return ProcessWithDict(value, processor);
        }

        /// <summary>Processes the specified value.</summary>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <param name="mode">The mode that should be used to process the value</param>
        /// <param name="info">The info.</param>
        /// <returns>the neurons that were used as the input event.</returns>
        public Neuron[] Process(
            string value, 
            Processor processor, 
            TextSinProcessMode mode, System.Collections.Generic.List<Neuron> info = null)
        {
            switch (mode)
            {
                case TextSinProcessMode.DictionaryWords:
                    return ProcessWithDict(value, processor);
                case TextSinProcessMode.ClusterWords:
                    return ProcessWithClusters(value, processor);
                case TextSinProcessMode.LetterStream:
                    return ProcessAsLetterStream(value, processor);
                case TextSinProcessMode.ClusterAndDict:
                    return ProcessWithClusterAndDict(value, processor, info);
                default:
                    throw new System.InvalidOperationException();
            }
        }

        /// <summary>The process.</summary>
        /// <param name="values">The values.</param>
        /// <param name="proc">The proc.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The <see cref="Neuron[]"/>.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public Neuron[] Process(System.Collections.Generic.IList<string> values, 
            Processor proc, 
            TextSinProcessMode mode)
        {
            if (mode == TextSinProcessMode.ClusterAndDict)
            {
                return ProcessWithClusterAndDict(values, proc);
            }

            throw new System.NotSupportedException("Not yet supported");
        }

        /// <summary>Tries to parse the specified string into neurons which can be processed by the processor.</summary>
        /// <remarks>Initial implementation uses a very simple string split algorithm combined with a dictionary lookup
        ///     to get the text nodes. Numbers and doubles are treated seperatly, in a more convenient fashion.
        ///     If a word is not found in the dictionary, a new TextNeuron is made, the processor will try to
        ///     figure out more about the word (or if it can't, ask more info about, completely depends on the rules and actions
        ///     that are defined.</remarks>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <returns>the neurons that were used as the input event.</returns>
        protected virtual Neuron[] ProcessWithDict(string value, Processor processor)
        {
            if (processor == null)
            {
                throw new System.ArgumentNullException("processor");
            }

            var iWords = value.Split(null);
            var iToProcess = NeuronFactory.GetNeuron();

                // this is the neuron we will eventually send to the brain to indicate that the input processing event happened.  This is also the store of the words, so the processor can solve it.
            Brain.Current.Add(iToProcess);
            foreach (var iWord in iWords)
            {
                Neuron iNeuron = null;
                var iRelationship = PredefinedNeurons.Empty;
                iNeuron = BrainHelper.TryGetNumber(iWord, ref iRelationship);
                if (iNeuron == null)
                {
                    iNeuron = Words.GetNeuronFor(iWord);
                    iRelationship = PredefinedNeurons.ContainsWord;
                }

                System.Diagnostics.Debug.Assert(iNeuron != null && iRelationship != PredefinedNeurons.Empty);
                var iLink = new Link(iNeuron, iToProcess, (ulong)iRelationship);

                    // that's all that is required for the link, it is automatically added to all correct lists.
            }

            Process(iToProcess, processor, value);
            return new[] { iToProcess };
        }

        /// <summary>Tries to parse the specified string into neurons which can be processed by the processor.</summary>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <param name="info">possibly, a list of neurons that should be used as info for the link.
        ///     this is used to specify which topics are allowed for processing the input</param>
        /// <returns>the neurons that were used as the input event.</returns>
        /// <remarks>uses internal split algorithm
        ///     to get the text nodes. Numbers and doubles are treated seperatly, in a more convenient fashion.
        ///     If a word is not found in the dictionary, a new TextNeuron is made, the processor will try to
        ///     figure out more about the word (or if it can't, ask more info about, completely depends on the rules and actions
        ///     that are defined. All textneurons are added to a cluster which will be the 'to' part.
        ///     Spaces and newlines are kept (this is important for the textpattner matcher: otherwise we can't read things like
        ///     directory paths correctly).</remarks>
        protected virtual Neuron[] ProcessWithClusterAndDict(
            string value, 
            Processor processor, System.Collections.Generic.List<Neuron> info)
        {
            if (processor == null)
            {
                throw new System.ArgumentNullException("processor");
            }

            var iToProcess = NeuronFactory.GetNeuron();

                // this is the neuron we will eventually send to the brain to indicate that the input processing event happened.  This is also the store of the words, so the processor can solve it.
            Brain.Current.Add(iToProcess);
            var iTo = NeuronFactory.GetCluster();
            Brain.Current.Add(iTo);
            foreach (var iWord in SplitString(value))
            {
                Neuron iNeuron = null;
                var iRelationship = PredefinedNeurons.Empty;
                iNeuron = BrainHelper.TryGetNumber(iWord, ref iRelationship);
                if (iNeuron == null)
                {
                    iNeuron = Words.GetNeuronFor(iWord);
                }

                System.Diagnostics.Debug.Assert(iNeuron != null);
                using (var iChildren = iTo.ChildrenW) iChildren.Add(iNeuron);
            }

            var iLink = new Link(iTo, iToProcess, (ulong)PredefinedNeurons.ContainsWord);

                // that's all that is required for the link, it is automatically added to all correct lists.
            if (info != null)
            {
                iLink.InfoW.AddRange(info);
            }

            Process(iToProcess, processor, value);
            return new[] { iToProcess };
        }

        /// <summary>Tries to parse the specified strings into neurons which can be processed by the processor.</summary>
        /// <remarks>uses internal split algorithm
        ///     to get the text nodes. Numbers and doubles are treated seperatly, in a more convenient fashion.
        ///     If a word is not found in the dictionary, a new TextNeuron is made, the processor will try to
        ///     figure out more about the word (or if it can't, ask more info about, completely depends on the rules and actions
        ///     that are defined. All textneurons are added to a cluster which will be the 'to' part.
        ///     Spaces and newlines are kept (this is important for the textpattner matcher: otherwise we can't read things like
        ///     directory paths correctly).</remarks>
        /// <param name="values">The values.</param>
        /// <param name="proc">The proc.</param>
        /// <returns>the neurons that were used as the input event.</returns>
        private Neuron[] ProcessWithClusterAndDict(System.Collections.Generic.IList<string> values, Processor proc)
        {
            var iTotalStrings = new System.Text.StringBuilder();
            if (proc == null)
            {
                throw new System.ArgumentNullException("processor");
            }

            var iToProcess = NeuronFactory.GetNeuron();

                // this is the neuron we will eventually send to the brain to indicate that the input processing event happened.  This is also the store of the words, so the processor can solve it.
            Brain.Current.Add(iToProcess);
            var iTo = NeuronFactory.GetCluster();
            Brain.Current.Add(iTo);
            iTo.Meaning = (ulong)PredefinedNeurons.Or; // so the system knows it's a group of possible clusters.
            foreach (var i in values)
            {
                if (iTotalStrings.Length > 0)
                {
                    iTotalStrings.Append(" || ");
                }

                iTotalStrings.Append(i);
                var iChild = NeuronFactory.GetCluster();
                Brain.Current.Add(iChild);
                using (var iChildren = iTo.ChildrenW) iChildren.Add(iChild);
                foreach (var iWord in SplitString(i))
                {
                    Neuron iNeuron = null;
                    var iRelationship = PredefinedNeurons.Empty;
                    iNeuron = BrainHelper.TryGetNumber(iWord, ref iRelationship);
                    if (iNeuron == null)
                    {
                        iNeuron = Words.GetNeuronFor(iWord);
                    }

                    System.Diagnostics.Debug.Assert(iNeuron != null);
                    using (var iChildren = iChild.ChildrenW) iChildren.Add(iNeuron);
                }
            }

            var iLink = new Link(iTo, iToProcess, (ulong)PredefinedNeurons.ContainsWord);

                // that's all that is required for the link, it is automatically added to all correct lists.
            Process(iToProcess, proc, iTotalStrings.ToString());
            return new[] { iToProcess };
        }

        /// <summary>Splits a string using the Tokenizer. All spaces/newlines are kept.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public static System.Collections.Generic.IEnumerable<string> SplitString(string value)
        {
            var iScanner = new Parsers.Tokenizer(value);
            iScanner.AllowEscaping = false;
            while (iScanner.CurrentToken != Parsers.Token.End)
            {
                yield return iScanner.CurrentValue;
                iScanner.GetNext();
            }
        }

        /// <summary>Tries to parse the specified string into neurons which can be processed by the processor.</summary>
        /// <remarks>This implementation will process each word as a cluster of letters.</remarks>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <returns>the neurons that were used as the input event.</returns>
        protected virtual Neuron[] ProcessWithClusters(string value, Processor processor)
        {
            if (processor == null)
            {
                throw new System.ArgumentNullException("processor");
            }

            var iWords = value.Split(null);
            var iToProcess = NeuronFactory.GetNeuron();

                // this is the neuron we will eventually send to the brain to indicate that the input processing event happened.  This is also the store of the words, so the processor can solve it.
            Brain.Current.Add(iToProcess);
            foreach (var iWord in iWords)
            {
                var iNeuron = NeuronFactory.GetCluster();
                foreach (var iLetter in iWord)
                {
                    Neuron iToAdd = Words.GetNeuronFor(new string(iLetter, 1));
                    using (var iChildren = iNeuron.ChildrenW) iChildren.Add(iToAdd.ID);
                }

                var iLink = new Link(iNeuron, iToProcess, (ulong)PredefinedNeurons.LetterSequence);

                    // that's all that is required for the link, it is automatically added to all correct lists.
            }

            Process(iToProcess, processor, value);
            return new[] { iToProcess };
        }

        /// <summary>Tries to parse the specified string into neurons which can be processed by the processor.</summary>
        /// <remarks>This implementation will process each letter as a seperate event instance. This means that there will be a new
        ///     neuron put
        ///     on the stack for each character in the input.  The neuron points to an IntNeuron.  IntNeurons are also created for
        ///     each letter.  Possible other algorithms might reuse int neurons.</remarks>
        /// <param name="value">The string text to parse.</param>
        /// <param name="processor">The processor to initiate the process with. This should be provided so that visualizers
        ///     can hook into it by providing custom processors (like the debuger).</param>
        /// <returns>the neurons that were used as the input event.</returns>
        protected virtual Neuron[] ProcessAsLetterStream(string value, Processor processor)
        {
            if (processor == null)
            {
                throw new System.ArgumentNullException("processor");
            }

            var iRes = new System.Collections.Generic.List<Neuron>();
            var iReversed = (from i in value select i).Reverse();

                // we need to reverse the order of the chars so that the first char is processed first.
            foreach (var i in iReversed)
            {
                var iFrom = NeuronFactory.GetNeuron(); // indicates the event of a new instance of the specified letter.
                Brain.Current.Add(iFrom);
                iRes.Add(iFrom);
                var iTo = NeuronFactory.GetInt(i);

                    // the letter to use: always a new one cause we can't create mutliple links between the same neurons with the same meaning.
                Brain.Current.Add(iTo);
                var iLink = new Link(iTo, iFrom, (ulong)PredefinedNeurons.Letter);
            }

            Process(iRes, processor, value);
            return iRes.ToArray();
        }

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend">The to Send.</param>
        /// <remarks><para>will only perform an output if there are objects monitoring the <see cref="TextSin.TextOut"/> event.</para>
        /// <para>This method is called by the <see cref="Brain"/> itself during/after processing of input.</para>
        /// <para>Allowed output values are:
        ///         - Any value neuron, like TextNeuron (-&gt; text is sent), DoubleNeuron or IntNeuron (-&gt; value is sent).
        ///         - An object cluster, in that case, the 'TextNeuron' is extracted</para>
        /// </remarks>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                if (i.ID == (ulong)PredefinedNeurons.BeginTextBlock)
                {
                    // need to start a new block, check if we need to close a previous one first.
                    if (fToSend != null)
                    {
                        OnTextOut(fToSend.ToString(), fNeuronsToSend);
                    }

                    fToSend = new OutputBuilder();
                    fNeuronsToSend = new System.Collections.Generic.List<Neuron>();
                }
                else if (i.ID == (ulong)PredefinedNeurons.EndTextBlock)
                {
                    // need to close the current block: send the event + close the block.
                    if (fToSend != null)
                    {
                        OnTextOut(fToSend.ToString(), fNeuronsToSend);
                    }

                    fToSend = null;
                    fNeuronsToSend = null;
                }
                else if (i.ID == (ulong)PredefinedNeurons.FirstUppercase)
                {
                    if (fToSend != null)
                    {
                        fToSend.UppercaseForNext = UppercaseStyle.FirstLetter;
                    }
                }
                else if (i.ID == (ulong)PredefinedNeurons.AllUppercase)
                {
                    if (fToSend != null)
                    {
                        fToSend.UppercaseForNext = UppercaseStyle.All;
                    }
                }
                else if (i.ID == (ulong)PredefinedNeurons.AllLowerCase)
                {
                    if (fToSend != null)
                    {
                        fToSend.UppercaseForNext = UppercaseStyle.AllLow;
                    }
                }
                else if (i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.UppercaseMap)
                {
                    if (fToSend != null)
                    {
                        fToSend.StoreUpperCaseMap((NeuronCluster)i);
                    }
                }
                else
                {
                    OutputNeuron(i);
                }
            }
        }

        /// <summary>Outputs a single neuron.</summary>
        /// <param name="toSend">To send.</param>
        private void OutputNeuron(Neuron toSend)
        {
            var iRes = BrainHelper.GetTextFrom(toSend);
            if (iRes == null)
            {
                iRes = toSend.ToString(); // this will transform any text, double or int and catch any other.
            }

            if (fToSend != null)
            {
                fToSend.Append(iRes);
                fNeuronsToSend.Add(toSend);
            }
            else
            {
                var iList = new System.Collections.Generic.List<Neuron> { toSend };
                OnTextOut(iRes, iList);
            }
        }

        /// <summary>Called when there is new text output.</summary>
        /// <param name="value">The value as a string.</param>
        /// <param name="data">The list of neurons that make up the string.</param>
        protected virtual void OnTextOut(string value, System.Collections.Generic.IList<Neuron> data)
        {
            if (TextOut != null)
            {
                TextOut(this, new OutputEventArgs<string> { Value = value, Data = data });
            }
        }

        /// <summary>Removes the entry point from the sin.</summary>
        /// <param name="toRemove">To remove.</param>
        /// <remarks>By default, a <see cref="Sin"/> doesn't need to have entrypoints, so this function doesn't do anything.  Some
        ///     sins use dictionaries.  They need to be kept in sync when items are removed.  That's why we have this function.</remarks>
        protected internal static void RemoveEntryPoint(Neuron toRemove)
        {
            var iToRemove = toRemove as TextNeuron;
            if (iToRemove != null && iToRemove.Text != null)
            {
                Words.Remove(iToRemove);
            }
        }

        #endregion
    }
}