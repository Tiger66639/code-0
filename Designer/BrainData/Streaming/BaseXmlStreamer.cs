// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseXmlStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   a base class for classes that provide xml streaming.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a base class for classes that provide xml streaming.
    /// </summary>
    public class BaseXmlStreamer
    {
        /// <summary>The topicel.</summary>
        public const string TOPICEL = "Topic";

        /// <summary>The f already rendered.</summary>
        private static System.Collections.Generic.HashSet<Neuron> fAlreadyRendered;

        /// <summary>The f already read.</summary>
        private static System.Collections.Generic.Dictionary<ulong, ulong> fAlreadyRead;

        /// <summary>The f conjugation meanings.</summary>
        private System.Collections.Generic.Dictionary<string, Neuron> fConjugationMeanings;

        // private static Dictionary<string, ulong> fPlacesholders;

        /// <summary>The f pos mappings.</summary>
        private System.Collections.Generic.Dictionary<string, Neuron> fPosMappings;

        #region AlreadyRead

        /// <summary>
        ///     keeps track of the objects that have already been read, so we can find
        ///     references to objects that have alraedy been read. this is the
        ///     counterpart of <see cref="fAlreadyRendered" /> we map again to an
        ///     <see langword="ulong" /> and not an object, to save memory during
        ///     processing: not all objects need to be kept in memory during the whole
        ///     import process.
        /// </summary>
        internal static System.Collections.Generic.Dictionary<ulong, ulong> AlreadyRead
        {
            get
            {
                if (fAlreadyRead == null)
                {
                    fAlreadyRead = new System.Collections.Generic.Dictionary<ulong, ulong>();
                }

                return fAlreadyRead;
            }

            set
            {
                fAlreadyRead = value;
            }
        }

        #endregion

        #region AlreadyRendered

        /// <summary>
        ///     keeps track of all the objects that have already been rendered, so
        ///     that we don't need to render these 2 times (saves lots of space) this
        ///     is <see langword="static" /> so that multiple streamers can access this
        ///     during a single 'meta' operation, like a project-export.
        /// </summary>
        internal static System.Collections.Generic.HashSet<Neuron> AlreadyRendered
        {
            get
            {
                if (fAlreadyRendered == null)
                {
                    fAlreadyRendered = new System.Collections.Generic.HashSet<Neuron>();
                }

                return fAlreadyRendered;
            }

            set
            {
                fAlreadyRendered = value;
            }
        }

        #endregion

        #region PosMappings

        /// <summary>
        ///     Gets the dict that provides fast mappings between neuron and pos name.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, Neuron> PosMappings
        {
            get
            {
                if (fPosMappings == null)
                {
                    fPosMappings = new System.Collections.Generic.Dictionary<string, Neuron>();
                    for (var i = 1; i < BrainData.Current.Thesaurus.PosFilters.Count; i++)
                    {
                        // we start from 1 cause the first is the null item.
                        var iItem = BrainData.Current.Thesaurus.PosFilters[i];
                        fPosMappings.Add(iItem.NeuronInfo.DisplayTitle.ToLower(), iItem.Item);
                    }
                }

                return fPosMappings;
            }
        }

        #endregion

        #region ConjugationMeanings

        /// <summary>
        ///     Gets the dict of conjugation meanings, for faster loading.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, Neuron> ConjugationMeanings
        {
            get
            {
                if (fConjugationMeanings == null)
                {
                    fConjugationMeanings = new System.Collections.Generic.Dictionary<string, Neuron>();
                    for (var i = 0; i < BrainData.Current.Thesaurus.ConjugationMeanings.Count; i++)
                    {
                        // we start from 1 cause the first is the null item.
                        var iItem = BrainData.Current.Thesaurus.ConjugationMeanings[i];
                        if (iItem != null || iItem.Item != null || iItem.Item.ID != Neuron.TempId)
                        {
                            fConjugationMeanings.Add(iItem.NeuronInfo.DisplayTitle.ToLower(), iItem.Item);
                        }
                    }
                }

                return fConjugationMeanings;
            }
        }

        #endregion

        #region Statics

        /// <summary>
        ///     Gets/sets the list of <see langword="static" /> neurons used by objects
        ///     that indicate that the word references one fo the 'person' indicators,
        ///     like I, you, he, she, it, we, they, my, mine, myself, your, yours,
        ///     yourself,...
        /// </summary>
        /// <remarks>
        ///     This dictionay should be filled
        /// </remarks>
        public static System.Collections.Generic.Dictionary<string, ulong> PersonMap { get; set; }

        #endregion

        /// <summary>
        ///     gets the object used by the system to report the current position
        ///     within the operation + canceling the operation.
        /// </summary>
        internal PosTracker Tracker { get; set; }

        /// <summary>
        ///     Tries to resolve the parse errors (as a second change) and prints an
        ///     error when it failed.
        /// </summary>
        protected void ResolveParseErrors()
        {
            foreach (var iError in fParseErrors)
            {
                try
                {
                    iError.Parse();

                        // first try a repars, could be that there was an error because of the missing topics.
                    if (iError.HasError)
                    {
                        LogService.Log.LogError("Import topic", iError.ParseError);
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError("Import topic", e.Message);
                }
            }

            try
            {
                TopicXmlStreamer.ResolveForwardRefs(fStillToResolve);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Import topic", e.Message);
            }
        }

        /// <summary>Creates a common reader settings object.</summary>
        /// <returns>The <see cref="XmlReaderSettings"/>.</returns>
        protected static System.Xml.XmlReaderSettings CreateReaderSettings()
        {
            var iSettings = new System.Xml.XmlReaderSettings();
            iSettings.IgnoreComments = true;
            iSettings.IgnoreProcessingInstructions = true;
            iSettings.IgnoreWhitespace = true;
            return iSettings;
        }

        /// <summary>Creates the common writer settings.</summary>
        /// <returns>The <see cref="XmlWriterSettings"/>.</returns>
        public static System.Xml.XmlWriterSettings CreateWriterSettings()
        {
            var iSettings = new System.Xml.XmlWriterSettings();

            // iSettings.Indent = false;
            iSettings.Indent = true;
            iSettings.NewLineHandling = System.Xml.NewLineHandling.None;
            return iSettings;
        }

        #region position

        /// <summary>
        ///     Checks if the process needs to be canceled + reports the current
        ///     position.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the process was canceled.
        /// </returns>
        protected bool CheckCancelAndPos()
        {
            if (Tracker != null && Tracker.Tracker != null)
            {
                if (Tracker.Tracker.IsCanceled)
                {
                    return true;
                }

                Tracker.Tracker.CurrentPos += Tracker.Stream.Position - Tracker.LastFilePos;
                Tracker.LastFilePos = Tracker.Stream.Position;
            }

            return false;
        }

        #endregion

        #region inner types

        /// <summary>
        ///     for keeping track of the position within the operation.
        /// </summary>
        internal class PosTracker
        {
            /// <summary>Gets or sets the tracker.</summary>
            public Search.ProcessTrackerItem Tracker { get; set; }

            /// <summary>
            ///     the file so we can get the current position.
            /// </summary>
            public System.IO.FileStream Stream { get; set; }

            /// <summary>
            ///     So we can properly increment. When there are multiple files to be
            ///     read, we can't simply assign the file pos to the current position.
            /// </summary>
            public long LastFilePos { get; set; }
        }

        #endregion

        #region for reading patterns 

        /// <summary>The f still to resolve.</summary>
        protected System.Collections.Generic.List<ProjectStreamingOperation.ToResolve> fStillToResolve =
            new System.Collections.Generic.List<ProjectStreamingOperation.ToResolve>();

                                                                                       // keeps track of all the 'ResponseFor' items that couldn't yet be resolved (content was defined later in the file). If this list stil contains any items when done, there were incorrect references in the file. We will generate some custom patterns for these.

        /// <summary>The f parse errors.</summary>
        protected System.Collections.Generic.List<ParsableTextPatternBase> fParseErrors =
            new System.Collections.Generic.List<ParsableTextPatternBase>();

                                                                           // all the objects that had problems during the parsing process. 
        #endregion

        #region object

        /// <summary>reads an object definition from the stream</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected NeuronCluster ReadObject(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            NeuronCluster iRes = null;

            var iRefStr = reader.GetAttribute("Ref");
            if (string.IsNullOrEmpty(iRefStr))
            {
                iRefStr = reader.GetAttribute("ID");
                var iAsLink = true;

                    // determins how the pos needs to be declared: as a link from object to pos or through the posgroup. (if there  is a pos)
                var iPos = reader.GetAttribute("POS");
                if (iPos == null)
                {
                    iPos = reader.GetAttribute("POSGRP");
                    iAsLink = false;
                }

                if (iPos != null)
                {
                    var iPosNeuron = GetPosNeuron(iPos);
                    if (reader.AttributeCount == 4)
                    {
                        // it's a placeholder, so read as one.
                        iRes = ReadPlaceholderObj(reader, iPosNeuron, iAsLink);
                    }
                    else
                    {
                        iRes = ReadNormalObj(reader, iPosNeuron, iAsLink);
                    }
                }
                else if (reader.AttributeCount == 3)
                {
                    // it's a placeholder, so read as one.
                    iRes = ReadPlaceholderObj(reader, null, iAsLink);
                }
                else
                {
                    iRes = ReadNormalObj(reader, null, iAsLink);
                }

                if (wasEmpty)
                {
                    reader.Read();
                }
                else
                {
                    ReadObjectBindings(reader, iRes);
                    reader.ReadEndElement();
                }

                AlreadyRead.Add(ulong.Parse(iRefStr), iRes.ID);
            }
            else
            {
                iRes = Brain.Current[AlreadyRead[ulong.Parse(iRefStr)]] as NeuronCluster;
                if (wasEmpty)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadEndElement();
                }
            }

            return iRes;
        }

        /// <summary>The read object bindings.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadObjectBindings(System.Xml.XmlReader reader, NeuronCluster result)
        {
            if (reader.Name == "Bindings")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "Bindings" && reader.EOF == false)
                {
                    if (reader.Name == "int")
                    {
                        ReadIntForObject(reader, result);
                    }
                    else if (reader.Name == "double")
                    {
                        ReadDoubleForObject(reader, result);
                    }
                    else if (reader.Name == "static")
                    {
                        ReadStaticForObject(reader, result);
                    }
                    else if (reader.Name == "ref")
                    {
                        ReadRefForObject(reader, result);
                    }
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read ref for object.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadRefForObject(System.Xml.XmlReader reader, NeuronCluster result)
        {
            var iVal = XmlStore.ReadElement<string>(reader, "ref");
            if (string.IsNullOrEmpty(iVal) == false && PersonMap != null)
            {
                iVal = iVal.ToLower();
                ulong iId;
                if (PersonMap.TryGetValue(iVal, out iId))
                {
                    using (var iChildren = result.ChildrenW) iChildren.Add(iId);
                }
            }
        }

        /// <summary>The read static for object.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadStaticForObject(System.Xml.XmlReader reader, NeuronCluster result)
        {
            var iId = XmlStore.ReadElement<ulong>(reader, "static");
            if (Brain.Current.IsValidID(iId))
            {
                using (var iChildren = result.ChildrenW) iChildren.Add(iId);
            }
        }

        /// <summary>The read double for object.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadDoubleForObject(System.Xml.XmlReader reader, NeuronCluster result)
        {
            var iVal = XmlStore.ReadElement<double>(reader, "double");
            var iNew = NeuronFactory.GetDouble(iVal);
            Brain.Current.Add(iNew);
            using (var iChildren = result.ChildrenW) iChildren.Add(iNew);
        }

        /// <summary>The read int for object.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The result.</param>
        private void ReadIntForObject(System.Xml.XmlReader reader, NeuronCluster result)
        {
            var iVal = XmlStore.ReadElement<int>(reader, "int");
            var iNew = NeuronFactory.GetInt(iVal);
            Brain.Current.Add(iNew);
            using (var iChildren = result.ChildrenW) iChildren.Add(iNew);
        }

        /// <summary>The get pos neuron.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetPosNeuron(string name)
        {
            var iName = name.ToLower();
            Neuron iRes;
            if (PosMappings.TryGetValue(iName, out iRes) == false)
            {
                ThesaurusRelItem iItem;
                var iNew = NeuronFactory.GetNeuron();
                Brain.Current.Add(iNew);
                iItem = new ThesaurusRelItem(iNew);
                iItem.NeuronInfo.DisplayTitle = name;
                BrainData.Current.Thesaurus.PosFilters.Add(iItem);
                PosMappings.Add(iItem.NeuronInfo.DisplayTitle.ToLower(), iNew);
                return iNew;
            }

            return iRes;
        }

        /// <summary>The read normal obj.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="posAsLink">The pos as link.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ReadNormalObj(System.Xml.XmlReader reader, Neuron pos, bool posAsLink)
        {
            return ReadObjectOrPosGroup(reader, "Object", pos, posAsLink);
        }

        /// <summary>The read object or pos group.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="posAsLink">The pos as link.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private NeuronCluster ReadObjectOrPosGroup(System.Xml.XmlReader reader, string name, Neuron pos, bool posAsLink)
        {
            NeuronCluster iRes = null;
            var iName = reader.GetAttribute("Name");
            reader.Read(); // attrib has been consumed, go to the next item.
            var iDesc = ReadDesc(reader);
            var iChildren = new System.Collections.Generic.List<Neuron>();
            var iPos = posAsLink == false ? pos : null;

                // if the pos is a link, we don't want to go creating posgroups for the children of the item + add the cluster to it.
            while (reader.Name != name && reader.Name != "Bindings" && reader.EOF == false)
            {
                // read all the synonyms.
                iChildren.Add(ReadObjectSyn(reader, iPos, ref iRes, iName));
            }

            if (iRes == null)
            {
                // there was no posgroup declared, so create one, don't need to add the synonyms, we always check for the presences of all synonyms
                iRes = NeuronFactory.GetCluster();
                Brain.Current.Add(iRes);
                if (name == "Object")
                {
                    iRes.Meaning = (ulong)PredefinedNeurons.Object;
                }
                else
                {
                    iRes.Meaning = (ulong)PredefinedNeurons.POSGroup;
                }
            }

            if (pos != null)
            {
                if (posAsLink)
                {
                    iRes.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, pos);

                        // if the object is not in a posgroup(cause no index defined), we still need to assign it the pos.
                }
            }

            var iChilLock = iRes.ChildrenW;
            iChilLock.Lock(iChildren);
            try
            {
                foreach (var i in iChildren)
                {
                    if (iChilLock.ContainsUnsafe(i) == false)
                    {
                        iChilLock.AddUnsafe(i);
                    }
                }
            }
            finally
            {
                iChilLock.Unlock(iChildren);
                iChilLock.Dispose();
            }

            var iData = BrainData.Current.NeuronInfo[iRes];
            if (string.IsNullOrEmpty(iDesc) == false)
            {
                iData.DescriptionText = iDesc;
            }

            if (string.IsNullOrEmpty(iName) == false)
            {
                iData.DisplayTitle = iName;
            }

            return iRes;
        }

        /// <summary>reads a posgroup from the stream.</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected NeuronCluster ReadPosGroup(System.Xml.XmlReader reader)
        {
            var iPos = reader.GetAttribute("POS");
            Neuron iPosNeuron = null;
            if (iPos == null)
            {
                iPos = reader.GetAttribute("POSGRP");
                iPosNeuron = GetPosNeuron(iPos);
            }

            return ReadObjectOrPosGroup(reader, "PosGroup", iPosNeuron, true);
        }

        /// <summary>Reads the placeholder obj.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="posAsLink">if set to <c>true</c> [pos as link].</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        protected NeuronCluster ReadPlaceholderObj(System.Xml.XmlReader reader, Neuron pos, bool posAsLink)
        {
            NeuronCluster iRes = null;
            string iName = null;
            iName = reader.GetAttribute("Name");

            // ulong iId;
            if (reader.IsEmptyElement == false)
            {
                // if there is a desc, go to the next item, if there isn't, we need to wait cause the object closer is consumed by the caller.
                reader.Read();
            }

            var iDesc = ReadDesc(reader);
            Neuron iNameOfObj = null;

            if (reader.Name == "Compound")
            {
                iNameOfObj = ReadCompound(reader);
            }
            else
            {
                iNameOfObj = ReadText(reader);
            }

            iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            BrainData.Current.NeuronInfo[iRes].DisplayTitle = iName;

            // Placesholders.Add(iName, iRes.ID);                                                 //dummys still store the name as a textneuron that is not registered in the dict. This way, we can still find them using the name of the object, but they are not accessessable through the parse, cause the name isn't in the dict and not registered as normal.
            if (iNameOfObj != null)
            {
                Link.Create(iRes, iNameOfObj, (ulong)PredefinedNeurons.NameOfMember);
            }

            // if (Placesholders.TryGetValue(iName, out iId) == false || BrainData.Current.NeuronInfo[iId].DisplayTitle.ToLower() != iName.ToLower())
            // {

            // }
            // else
            // iRes = Brain.Current[iId] as NeuronCluster;
            if (pos != null && posAsLink)
            {
                // when not a poslink, don't need to do anything, cause the other option is a posgroup, but it's a placeholder, so there is no text that can define the posgroup.
                iRes.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, pos);

                    // if the object is not in a posgroup(cause no index defined), we still need to assign it the pos.
            }

            return iRes;
        }

        /// <summary>Reads a single synonym of an object. If the synonym has an index
        ///     declared into it's posgroup, then we go look for the object. The first
        ///     synonym that declares in index, declares the object. All consecutive
        ///     index references will have to <see langword="ref"/> the same object,
        ///     or a new one will be created.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="pos">The pos.</param>
        /// <param name="objectRef">The object ref.</param>
        /// <param name="nameOfObject">The name Of Object.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadObjectSyn(
            System.Xml.XmlReader reader, 
            Neuron pos, 
            ref NeuronCluster objectRef, 
            string nameOfObject)
        {
            Neuron iFound;
            int iIndex;
            bool iInPosGrp;
            if (reader.Name == "Compound")
            {
                iFound = ReadCompound(reader, out iIndex, out iInPosGrp);
            }
            else
            {
                iFound = ReadText(reader, out iIndex, out iInPosGrp);
            }

            if (iIndex != -1)
            {
                // there is an index declared of the object is in a posgroup, so look up the object.
                // Debug.Assert(pos != null);
                var iTemp = new System.Collections.Generic.List<NeuronCluster>();
                if (iFound.ClusteredByIdentifier != null)
                {
                    System.Collections.Generic.List<NeuronCluster> iParents;
                    using (var iList = iFound.ClusteredBy) iParents = iFound.ClusteredBy.ConvertTo<NeuronCluster>();
                    try
                    {
                        foreach (var i in iParents)
                        {
                            if (i != null && i.Meaning == (ulong)PredefinedNeurons.Object)
                            {
                                iTemp.Add(i);
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.CLists.Recycle(iParents);
                    }
                }

                if (iTemp.Count > iIndex)
                {
                    // the item has enough parents for the object, so get it + check if it is no dummy.
                    if (objectRef == null)
                    {
                        objectRef = iTemp[iIndex];
                    }
                    else if (objectRef != iTemp[iIndex])
                    {
                        // there is an object, but it is different, this should only happen when this is a dummy object that we created to fill in the empty places, so we delete the previous and replace with the ref that we already have.
                        IDListAccessor iClLock = iFound.ClusteredByW;
                        var iToRelease = iClLock.Lock(iTemp[iIndex], objectRef);
                        try
                        {
                            var iSubIndex = iClLock.IndexOfUnsafe(iTemp[iIndex].ID);
                            iClLock.RemoveUnsafe(iTemp[iIndex], iSubIndex);
                            if (iSubIndex < iClLock.CountUnsafe)
                            {
                                iClLock.InsertUnsafe(iSubIndex, objectRef);
                            }
                            else
                            {
                                iClLock.AddUnsafe(objectRef);
                            }
                        }
                        finally
                        {
                            iClLock.Unlock(iToRelease);
                            Factories.Default.NLists.Recycle(iToRelease);
                            iClLock.Dispose();
                        }

                        System.Diagnostics.Debug.Assert(AlreadyRead.ContainsValue(iTemp[iIndex].ID) == false);
                        Brain.Current.Delete(iTemp[iIndex]);
                    }
                }
                else
                {
                    while (iTemp.Count < iIndex)
                    {
                        // we add temp objects until we reach the location of the object that we want, next we check if there is already a ref or not.
                        iTemp.Add(BrainHelper.CreateObject(iFound));
                    }

                    if (objectRef == null)
                    {
                        objectRef = BrainHelper.CreateObject(iFound);
                    }
                    else
                    {
                        using (IDListAccessor iChildren = objectRef.ChildrenW)

                            // add a reference of the newly found text to the object, cause it was not yet there.
                            iChildren.Add(iFound);
                    }
                }
            }

            if (iInPosGrp && pos != null)
            {
                var iPosGrp = BrainHelper.GetPOSGroup(iFound, pos.ID);
                if (iPosGrp != null)
                {
                    var iChildLock = iPosGrp.ChildrenW;
                    iChildLock.Lock(objectRef);
                    try
                    {
                        if (iChildLock.ContainsUnsafe(objectRef) == false)
                        {
                            iChildLock.AddUnsafe(objectRef);
                        }
                    }
                    finally
                    {
                        iChildLock.Unlock(objectRef);
                        iChildLock.Dispose();
                    }

                    BrainData.Current.NeuronInfo[iPosGrp].DisplayTitle =
                        BrainData.Current.NeuronInfo.GetDisplayTitleFor(iFound.ID) + "("
                        + BrainData.Current.NeuronInfo.GetDisplayTitleFor(pos.ID) + ")"; // the posgroup's label 
                }
            }

            return iFound;
        }

        /// <summary>Reads a the text neuron, without antisipating for posgroup indexes, so
        ///     this can only be used for compound words or for textneurons that are
        ///     used directly, without being wrapped in an object.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        protected TextNeuron ReadText(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            TextNeuron iFound = null;
            var iStr = reader.GetAttribute("Value");
            var iInDict = true;
            if (reader.AttributeCount > 1)
            {
                bool.TryParse(reader.GetAttribute("InDict"), out iInDict);
            }

            if (string.IsNullOrEmpty(iStr) == false)
            {
                if (iInDict)
                {
                    iFound = TextNeuron.GetFor(iStr);
                }
                else
                {
                    iFound = NeuronFactory.GetText(iStr);
                    Brain.Current.Add(iFound);
                }
            }

            if (wasEmpty)
            {
                reader.Read(); // go to the next item.
            }
            else
            {
                ReadDesc(reader, iFound);
                reader.ReadEndElement();
            }

            return iFound;
        }

        /// <summary>The read text.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="index">The index.</param>
        /// <param name="inPosGrp">The in pos grp.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected TextNeuron ReadText(System.Xml.XmlReader reader, out int index, out bool inPosGrp)
        {
            index = -1;
            var wasEmpty = reader.IsEmptyElement;
            TextNeuron iFound;
            var iStr = reader.GetAttribute("Value");
            if (string.IsNullOrEmpty(iStr) == false)
            {
                iFound = TextNeuron.GetFor(iStr);
            }
            else
            {
                throw new System.InvalidOperationException("Name of object expected!");
            }

            if (reader.AttributeCount > 1)
            {
                // if tere is more 1 attribute, there is also an index declared for the object in the object + if it is in a cluster.
                index = int.Parse(reader.GetAttribute("Index"));
                inPosGrp = bool.Parse(reader.GetAttribute("POSGRP"));
            }
            else
            {
                inPosGrp = false;
            }

            reader.Read();
            if (!wasEmpty)
            {
                ReadDesc(reader, iFound);
                reader.ReadEndElement();
            }

            return iFound;
        }

        /// <summary>The read int neuron.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="IntNeuron"/>.</returns>
        protected IntNeuron ReadIntNeuron(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iStr = reader.GetAttribute("Value");

            var iVal = int.Parse(iStr);
            var iRes = NeuronFactory.GetInt(iVal);
            Brain.Current.Add(iRes);
            if (wasEmpty)
            {
                reader.Read(); // go to the next item.
            }
            else
            {
                ReadDesc(reader, iRes);
                reader.ReadEndElement();
            }

            return iRes;
        }

        /// <summary>The read double neuron.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="DoubleNeuron"/>.</returns>
        protected DoubleNeuron ReadDoubleNeuron(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            var iStr = reader.GetAttribute("Value");

            var iVal = double.Parse(iStr);
            var iRes = NeuronFactory.GetDouble(iVal);
            Brain.Current.Add(iRes);
            if (wasEmpty)
            {
                reader.Read(); // go to the next item.
            }
            else
            {
                ReadDesc(reader, iRes);
                reader.ReadEndElement();
            }

            return iRes;
        }

        /// <summary>The read compound.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadCompound(System.Xml.XmlReader reader)
        {
            int iIndex;
            bool inPosGRP;
            return ReadCompound(reader, out iIndex, out inPosGRP);
        }

        /// <summary>The read text neuron.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadTextNeuron(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Compound")
            {
                return ReadCompound(reader);
            }

            if (reader.Name == "Text")
            {
                return ReadText(reader);
            }

            if (reader.Name == "Object")
            {
                return ReadObject(reader);
            }

            return null;
        }

        /// <summary>Reads a compound word + a possible <paramref name="index"/> into the
        ///     posgroup (for the object).</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="index">The index.</param>
        /// <param name="inPosGrp">The in Pos Grp.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadCompound(System.Xml.XmlReader reader, out int index, out bool inPosGrp)
        {
            index = -1;
            inPosGrp = false;
            var wasEmpty = reader.IsEmptyElement;
            string iName = null;
            string iDesc = null;
            Neuron iRes = null;
            if (reader.AttributeCount > 0)
            {
                // if the compound had a title, read and store it.
                iName = reader.GetAttribute("Name");
                var iIndex = reader.GetAttribute("Index");
                if (iIndex != null)
                {
                    index = int.Parse(iIndex);
                }

                iIndex = reader.GetAttribute("POSGRP");
                if (iIndex != null)
                {
                    inPosGrp = bool.Parse(iIndex);
                }
                else
                {
                    inPosGrp = false;
                }
            }

            reader.Read();
            if (!wasEmpty)
            {
                iDesc = ReadDesc(reader);
                var iItems = new System.Collections.Generic.List<Neuron>();
                while (reader.Name != "Compound" && reader.EOF == false)
                {
                    Neuron iRead = ReadText(reader);
                    if (iRead != null)
                    {
                        iItems.Add(iRead);
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Name of object expected!");
                    }
                }

                iRes = BrainHelper.GetCompoundWord(iItems);
                if (string.IsNullOrEmpty(iName))
                {
                    // it's a compound word, it diserves a title, so create one now. This is a left over from olden days, when th compounds didn't automaticaly get labled.                                     
                    var iStrBuilder = new System.Text.StringBuilder();
                    foreach (TextNeuron i in iItems)
                    {
                        if (i != iItems[0])
                        {
                            iStrBuilder.Append(" ");
                        }

                        iStrBuilder.Append(i.Text);
                    }

                    iName = iStrBuilder.ToString();
                }

                BrainData.Current.NeuronInfo[iRes].DisplayTitle = iName;
                reader.ReadEndElement();
            }

            return iRes;
        }

        /// <summary>Writes a <paramref name="neuron"/> that should be interpreted as some
        ///     kind of text (textneuron/compound/object)</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="compact">The compact.</param>
        protected void WriteTextNeuron(System.Xml.XmlWriter writer, Neuron neuron, bool compact = false)
        {
            var iText = neuron as TextNeuron;
            if (iText != null)
            {
                WriteText(writer, iText, null);
            }
            else
            {
                var iCluster = neuron as NeuronCluster;
                if (iCluster.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                {
                    WriteCompound(writer, iCluster, null);
                }
                else if (iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                {
                    if (compact == false)
                    {
                        WriteObjectRef(writer, iCluster);
                    }
                    else
                    {
                        WriteObjectRefCompact(writer, iCluster);
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("Unexpected attribute");
                }
            }
        }

        /// <summary>Writes an object out, but only writes out pos/posgroup info when
        ///     really needed. an object needs to write it's pos if it has no
        ///     recursive relationship for 'is a' or 'hyponym', which is also mapped
        ///     to 'is a'.</summary>
        /// <param name="writer"></param>
        /// <param name="item">The item.</param>
        protected void WriteObjectRefCompact(System.Xml.XmlWriter writer, NeuronCluster item)
        {
            writer.WriteStartElement("Object");
            if (AlreadyRendered.Contains(item) == false)
            {
                var iWritePos = true;
                foreach (var i in BrainData.Current.Thesaurus.Relationships)
                {
                    var iName = i.NeuronInfo.DisplayTitle.ToLower();
                    if (iName == "hyponym" || iName == "is a")
                    {
                        Neuron iFound = item.FindFirstClusteredBy(i.Item.ID);
                        if (iFound != null)
                        {
                            iWritePos = false;
                            break;
                        }
                    }
                }

                writer.WriteAttributeString("ID", item.ID.ToString());

                    // need to store the id, so we can do mappings when the object gets used multiple times
                if (iWritePos)
                {
                    WritePosForObjectRef(writer, item);
                }

                var iNames = new System.Collections.Generic.List<Neuron>();
                var iBindings = new System.Collections.Generic.List<Neuron>();
                GetObjectContent(item, iNames, iBindings);
                WriteNamesForObjectRef(writer, item, iNames);

                if (iBindings.Count == 0)
                {
                    // try to auto create a binding for numbers.
                    var iTryConvert = BrainData.Current.NeuronInfo[item].DisplayTitle;
                    if (iTryConvert.EndsWith("th") && iTryConvert.Length > 2)
                    {
                        iTryConvert = iTryConvert.Substring(0, iTryConvert.Length - 2);
                    }
                    else if (iTryConvert.EndsWith("s") && iTryConvert.Length > 1)
                    {
                        iTryConvert = iTryConvert.Substring(0, iTryConvert.Length - 1);
                    }

                    int iParsedInt;
                    if (int.TryParse(iTryConvert, out iParsedInt))
                    {
                        var iIntN = NeuronFactory.GetInt(iParsedInt);
                        Brain.Current.MakeTemp(iIntN); // make it temp cause we check on id for statics.
                        iBindings.Add(iIntN);
                    }
                    else
                    {
                        double iParseDouble;
                        if (double.TryParse(iTryConvert, out iParseDouble))
                        {
                            var iDN = NeuronFactory.GetDouble(iParseDouble);
                            Brain.Current.MakeTemp(iDN); // make it temp cause we check on id for statics.
                            iBindings.Add(iDN);
                        }
                    }
                }

                WriteBindingsForObjectRef(writer, item, iBindings);
                AlreadyRendered.Add(item);
            }
            else
            {
                writer.WriteAttributeString("Ref", item.ID.ToString());
            }

            writer.WriteEndElement();
        }

        /// <summary>Writes a reference to an object. This is done by specifying each
        ///     textneuron or compound word in the text + and, the index in the
        ///     posgroup for each textneuron-compound word, if there is one. If it's a
        ///     placeholder, this is also stored.</summary>
        /// <remarks>When it's a placeholder we also streams the id of the object, so that
        ///     we can check if the object already existed during an import. Usually</remarks>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        protected void WriteObjectRef(System.Xml.XmlWriter writer, NeuronCluster item)
        {
            writer.WriteStartElement("Object");
            if (AlreadyRendered.Contains(item) == false)
            {
                writer.WriteAttributeString("ID", item.ID.ToString());

                    // need to store the id, so we can do mappings when the object gets used multiple times
                WritePosForObjectRef(writer, item);
                var iNames = new System.Collections.Generic.List<Neuron>();
                var iBindings = new System.Collections.Generic.List<Neuron>();
                GetObjectContent(item, iNames, iBindings);
                WriteNamesForObjectRef(writer, item, iNames);
                WriteBindingsForObjectRef(writer, item, iBindings);
                AlreadyRendered.Add(item);
            }
            else
            {
                writer.WriteAttributeString("Ref", item.ID.ToString());
            }

            writer.WriteEndElement();
        }

        /// <summary>The write bindings for object ref.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        /// <param name="bindings">The bindings.</param>
        private void WriteBindingsForObjectRef(
            System.Xml.XmlWriter writer, 
            NeuronCluster item, System.Collections.Generic.List<Neuron> bindings)
        {
            if (bindings.Count > 0)
            {
                writer.WriteStartElement("Bindings");
                foreach (var i in bindings)
                {
                    if (i.ID < (ulong)PredefinedNeurons.EndOfStatic)
                    {
                        XmlStore.WriteElement(writer, "static", i.ID);
                    }
                    else if (i is IntNeuron)
                    {
                        XmlStore.WriteElement(writer, "int", ((IntNeuron)i).Value);
                    }
                    else if (i is DoubleNeuron)
                    {
                        XmlStore.WriteElement(writer, "double", ((DoubleNeuron)i).Value);
                    }
                    else
                    {
                        XmlStore.WriteElement(writer, "ref", BrainData.Current.NeuronInfo[i].DisplayTitle);
                    }
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>The write names for object ref.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        /// <param name="iNames">The i names.</param>
        private void WriteNamesForObjectRef(
            System.Xml.XmlWriter writer, 
            NeuronCluster item, System.Collections.Generic.List<Neuron> iNames)
        {
            if (iNames.Count > 0)
            {
                // if the object has no children, it's a placeholder, which is rendered differently
                writer.WriteAttributeString("Name", BrainData.Current.NeuronInfo[item].DisplayTitle);
                WriteDesc(writer, item);
                foreach (var i in iNames)
                {
                    if (i is TextNeuron)
                    {
                        WriteText(writer, (TextNeuron)i, item);
                    }
                    else
                    {
                        WriteCompound(writer, (NeuronCluster)i, item);
                    }
                }
            }
            else
            {
                writer.WriteAttributeString("Placeholder", item.ID.ToString());

                    // if it is a placeholder, we write the id, so that we can check if it exists in the new (with the same caption), so that we don't need to recreate it. 
                writer.WriteAttributeString("Name", BrainData.Current.NeuronInfo[item].DisplayTitle);
                WriteDesc(writer, item);
                var iName = item.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);

                    // dummies also have a name (cause they need to be accessible from a pattern path), but the name is registered differently.
                if (iName == null)
                {
                    // fix old thesaurus that didn't store the name of the dummy
                    iName = EditorsHelper.ConvertStringToNeurons(BrainData.Current.NeuronInfo[item].DisplayTitle);
                    Link.Create(item, iName, (ulong)PredefinedNeurons.NameOfMember);
                }

                if (iName is TextNeuron)
                {
                    WriteText(writer, (TextNeuron)iName, null);
                }
                else if (iName is NeuronCluster)
                {
                    WriteCompound(writer, (NeuronCluster)iName, null);
                }
            }
        }

        /// <summary>The write pos for object ref.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron WritePosForObjectRef(System.Xml.XmlWriter writer, NeuronCluster item)
        {
            Neuron iPos = null;
            var iPosGroup = item.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);

                // an object usually only belongs to only 1 posgroup, if any.
            if (iPosGroup != null)
            {
                iPos = iPosGroup.FindFirstOut((ulong)PredefinedNeurons.POS);
                if (iPos != null)
                {
                    writer.WriteAttributeString("POSGRP", BrainData.Current.NeuronInfo[iPos].DisplayTitle);

                        // to indicate that it was  part of a posgroup
                }
            }
            else
            {
                iPos = item.FindFirstOut((ulong)PredefinedNeurons.POS); // to indicate that it has a pos link.
                if (iPos != null)
                {
                    writer.WriteAttributeString("POS", BrainData.Current.NeuronInfo[iPos].DisplayTitle);
                }
            }

            return iPos;
        }

        /// <summary>Writes the value of a textneuron + a possible description If<paramref name="forObject"/> is specified, the index of the object
        ///     cluster in the 'posgroup' of the textneuron (if there is any), is also
        ///     written. This allows us to disambiguate between different meanings of
        ///     the same word.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="text">The text.</param>
        /// <param name="forObject">The for Object.</param>
        protected void WriteText(System.Xml.XmlWriter writer, TextNeuron text, NeuronCluster forObject)
        {
            writer.WriteStartElement("Text");
            writer.WriteAttributeString("Value", text.Text);
            if (TextSin.Words.ContainsKey(text.Text) == false)
            {
                writer.WriteAttributeString("InDict", "false");

                    // when the text is not in the dictionary, indicate this so that everything gets stored correctly.
            }

            WriteIndexAndPosGrp(writer, text, forObject);
            WriteDesc(writer, text);
            writer.WriteEndElement();
        }

        /// <summary>The write pos group.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        protected void WritePosGroup(System.Xml.XmlWriter writer, NeuronCluster item)
        {
            writer.WriteStartElement("PosGroup");

            var iPos = item.FindFirstOut((ulong)PredefinedNeurons.POS); // to indicate that it has a pos link.
            if (iPos != null)
            {
                writer.WriteAttributeString("POS", BrainData.Current.NeuronInfo[iPos].DisplayTitle);
            }

            var iNames = GetObjectNames(item);

                // we can have to use the same approach as the object. This can contain synonyms?
            if (iNames.Count > 0)
            {
                // if the object has no children, it's a placeholder, which is rendered differently
                writer.WriteAttributeString("Name", BrainData.Current.NeuronInfo[item].DisplayTitle);

                WriteDesc(writer, item);
                foreach (var i in iNames)
                {
                    if (i is TextNeuron)
                    {
                        WriteText(writer, (TextNeuron)i, item);
                    }
                    else
                    {
                        WriteCompound(writer, (NeuronCluster)i, item);
                    }
                }
            }
            else
            {
                writer.WriteAttributeString("Placeholder", item.ID.ToString());

                    // if it is a placeholder, we write the id, so that we can check if it exists in the new (with the same caption), so that we don't need to recreate it. 
                writer.WriteAttributeString("Name", BrainData.Current.NeuronInfo[item].DisplayTitle);
                WriteDesc(writer, item);
            }

            writer.WriteEndElement();
        }

        /// <summary>Looks up the index nr of the <paramref name="text"/> in the object
        ///     cluster (using the BrainData.Current.Thesaurus's currently selected
        ///     pos. The index is calculated on the objects.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="text">The text.</param>
        /// <param name="forObject">For object.</param>
        private void WriteIndexAndPosGrp(System.Xml.XmlWriter writer, Neuron text, NeuronCluster forObject)
        {
            if (forObject != null)
            {
                var iTemp = new System.Collections.Generic.List<NeuronCluster>();
                if (text.ClusteredByIdentifier != null)
                {
                    System.Collections.Generic.List<NeuronCluster> iParents;
                    using (var iList = text.ClusteredBy) iParents = text.ClusteredBy.ConvertTo<NeuronCluster>();
                    try
                    {
                        foreach (var i in iParents)
                        {
                            if (i != null && i.Meaning == (ulong)PredefinedNeurons.Object)
                            {
                                iTemp.Add(i);
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.CLists.Recycle(iParents);
                    }
                }

                var iIndex = iTemp.IndexOf(forObject);
                System.Diagnostics.Debug.Assert(iIndex != -1);
                writer.WriteAttributeString("Index", iIndex.ToString());
                var iPosGroup = forObject.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);

                    // an object usually only belongs to only 1 posgroup, if any.
                if (iPosGroup != null)
                {
                    var iInPosGroup = false;
                    if (iPosGroup.ChildrenIdentifier != null)
                    {
                        using (var iChildren = iPosGroup.Children)
                            foreach (var i in iChildren)
                            {
                                if (i == text.ID)
                                {
                                    iInPosGroup = true;
                                    break;
                                }
                            }
                    }

                    if (iInPosGroup)
                    {
                        writer.WriteAttributeString("POSGRP", "true");
                    }
                    else
                    {
                        writer.WriteAttributeString("POSGRP", "false");
                    }
                }
                else
                {
                    writer.WriteAttributeString("POSGRP", "false");
                }
            }
        }

        /// <summary>The write int neuron.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        protected void WriteIntNeuron(System.Xml.XmlWriter writer, IntNeuron value)
        {
            writer.WriteStartElement("Int");
            writer.WriteAttributeString("Value", value.Value.ToString());
            WriteDesc(writer, value);
            writer.WriteEndElement();
        }

        /// <summary>The write double neuron.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        protected void WriteDoubleNeuron(System.Xml.XmlWriter writer, DoubleNeuron value)
        {
            writer.WriteStartElement("Double");
            writer.WriteAttributeString("Value", value.Value.ToString());
            WriteDesc(writer, value);
            writer.WriteEndElement();
        }

        /// <summary>The write compound.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="compound">The compound.</param>
        /// <param name="forObject">The for object.</param>
        protected void WriteCompound(System.Xml.XmlWriter writer, NeuronCluster compound, NeuronCluster forObject)
        {
            writer.WriteStartElement("Compound");
            if (string.IsNullOrEmpty(BrainData.Current.NeuronInfo[compound].Title) == false)
            {
                // don't store an auto generated title.
                writer.WriteAttributeString("Name", BrainData.Current.NeuronInfo[compound].Title);

                    // we store the display title as well, cause it might contain the full text.
            }

            WriteIndexAndPosGrp(writer, compound, forObject);
            WriteDesc(writer, compound);
            if (compound.ChildrenIdentifier != null)
            {
                using (var iList = compound.Children)
                    foreach (var i in iList.ConvertTo<TextNeuron>())
                    {
                        WriteText(writer, i, null);
                    }
            }

            writer.WriteEndElement();
        }

        /// <summary>Determines whether the <paramref name="item"/> (which must be an
        ///     object cluster), is contained by a posgroup for the currently selected
        ///     (in the thesaurus) pos .</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if [is object in pos group] [the specified item];
        ///     otherwise, <c>false</c> .</returns>
        private bool IsObjectInPosGroup(NeuronCluster item)
        {
            if (item.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = item.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                try
                {
                    foreach (var i in iClusters)
                    {
                        if (i.Meaning == BrainData.Current.Thesaurus.SelectedPOSFilter.ID)
                        {
                            return true;
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusters);
                }
            }

            return false;
        }

        /// <summary>Gets all the names for the object.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<Neuron> GetObjectNames(NeuronCluster item)
        {
            var iNames = new System.Collections.Generic.List<Neuron>();
            if (item.ChildrenIdentifier != null)
            {
                System.Collections.Generic.List<Neuron> iList;
                using (var iChildren = item.Children) iList = iChildren.ConvertTo<Neuron>();
                try
                {
                    foreach (var i in iList)
                    {
                        var iText = i as TextNeuron;
                        if (iText != null)
                        {
                            iNames.Add(iText);
                        }
                        else
                        {
                            var iCluster = i as NeuronCluster;
                            if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                            {
                                iNames.Add(iCluster);
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iList);
                }
            }

            return iNames;
        }

        /// <summary>Gets all the names for the object.</summary>
        /// <param name="item">The item.</param>
        /// <param name="iNames">The i Names.</param>
        /// <param name="bindings">The bindings.</param>
        private void GetObjectContent(
            NeuronCluster item, System.Collections.Generic.List<Neuron> iNames, System.Collections.Generic.List<Neuron> bindings)
        {
            if (item.ChildrenIdentifier != null)
            {
                System.Collections.Generic.List<Neuron> iList;
                using (var iChildren = item.Children) iList = iChildren.ConvertTo<Neuron>();
                try
                {
                    foreach (var i in iList)
                    {
                        var iText = i as TextNeuron;
                        if (iText != null)
                        {
                            iNames.Add(iText);
                        }
                        else
                        {
                            var iCluster = i as NeuronCluster;
                            if (iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                            {
                                iNames.Add(iCluster);
                            }
                            else if (PersonMap != null && PersonMap.ContainsValue(i.ID)
                                     || i.ID < (ulong)PredefinedNeurons.EndOfStatic || i is IntNeuron
                                     || i is DoubleNeuron)
                            {
                                bindings.Add(i);
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iList);
                }
            }
        }

        #endregion

        #region info

        /// <summary>Writes the info from 2 strings.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="name">The name.</param>
        protected static void WriteInfo(System.Xml.XmlWriter writer, string desc, string name)
        {
            if (string.IsNullOrEmpty(name) == false)
            {
                XmlStore.WriteElement(writer, "Name", name);
            }

            if (string.IsNullOrEmpty(desc) == false)
            {
                writer.WriteRaw(desc);
            }
        }

        /// <summary>Writes the info from 2 strings.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="desc">The desc.</param>
        protected static void WriteDesc(System.Xml.XmlWriter writer, string desc)
        {
            if (string.IsNullOrEmpty(desc) == false)
            {
                writer.WriteRaw(desc);
            }
        }

        /// <summary>Writes the description for the Item if there is one.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="item">The item.</param>
        protected virtual void WriteDesc(System.Xml.XmlWriter writer, Neuron item)
        {
            if (item != null)
            {
                var iData = BrainData.Current.NeuronInfo[item];
                if (string.IsNullOrEmpty(iData.DescriptionText) == false)
                {
                    writer.WriteRaw(iData.DescriptionText);
                }
            }
        }

        /// <summary>Writes the <paramref name="info"/> from the <see cref="NeuronData"/>
        ///     object.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="info">The info.</param>
        protected static void WriteInfo(System.Xml.XmlWriter writer, NeuronData info)
        {
            if (info != null)
            {
                if (string.IsNullOrEmpty(info.Title) == false)
                {
                    XmlStore.WriteElement(writer, "Name", info.Title);
                }

                if (string.IsNullOrEmpty(info.DescriptionText) == false)
                {
                    writer.WriteRaw(info.DescriptionText);
                }
            }
        }

        /// <summary>Reads the info section</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="data">The data.</param>
        protected static void ReadInfo(System.Xml.XmlReader reader, NeuronData data)
        {
            string iTemp = null;
            if (XmlStore.TryReadElement(reader, "Name", ref iTemp))
            {
                data.DisplayTitle = iTemp;
            }

            if (reader.Name == "FlowDocument")
            {
                // could be that there was no neuron info written
                data.DescriptionText = reader.ReadOuterXml();
            }
        }

        /// <summary>Reads the info section, but doesn't store the 'name' in the
        ///     'displayTitle' field, but returns it sepedratly. This is used for the
        ///     TextNeurons, which don't have a seperate display title, their value is
        ///     always passed along to the</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="data">The data.</param>
        /// <param name="name">The name.</param>
        protected static void ReadInfo(System.Xml.XmlReader reader, NeuronData data, out string name)
        {
            name = null;
            XmlStore.TryReadElement(reader, "Name", ref name);
            if (reader.Name == "FlowDocument")
            {
                // could be that there was no neuron info written
                data.DescriptionText = reader.ReadOuterXml();
            }
        }

        /// <summary>Reads the info section, but doesn't store the 'name' in the
        ///     'displayTitle' field, but returns it sepedratly. This is used for the
        ///     TextNeurons, which don't have a seperate display title, their value is
        ///     always passed along to the</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="name">The name.</param>
        protected static void ReadInfo(System.Xml.XmlReader reader, out string desc, out string name)
        {
            name = null;
            XmlStore.TryReadElement(reader, "Name", ref name);
            if (reader.Name == "FlowDocument")
            {
                // could be that there was no neuron info written
                desc = reader.ReadOuterXml();
            }
            else
            {
                desc = null;
            }
        }

        /// <summary>Reads a descrption at the current location, if there is one, and
        ///     returns it.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="string"/>.</returns>
        protected static string ReadDesc(System.Xml.XmlReader reader)
        {
            if (reader.Name == "FlowDocument")
            {
                // could be that there was no neuron info written
                return reader.ReadOuterXml();
            }

            return null;
        }

        /// <summary>Reads the description at the current pos, fi there is one and assigns
        ///     it to the neuron.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="toUpdate">To update.</param>
        private void ReadDesc(System.Xml.XmlReader reader, Neuron toUpdate)
        {
            var iStr = ReadDesc(reader);
            if (string.IsNullOrEmpty(iStr) == false)
            {
                BrainData.Current.NeuronInfo[toUpdate].DescriptionText = iStr;
            }
        }

        #endregion

        #region expressions

        /// <summary>Writes the expression, as a string, so that the ssml tags stay intact.
        ///     only for patterns that don't contain sub xml tags (like the output
        ///     patterns).</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        protected static void WriteExpression(System.Xml.XmlWriter writer, string value)
        {
            writer.WriteStartElement("Expression");
            writer.WriteValue(value);

            // writer.WriteRaw(value);                          
            writer.WriteEndElement();
        }

        /// <summary>The read expression.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="string"/>.</returns>
        protected static string ReadExpression(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Expression")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return null;
                }

                var iVal = reader.ReadString();
                reader.ReadEndElement();
                return iVal;
            }

            return null;
        }

        /// <summary>Writes the expression, as a string, so that the ssml tags stay intact.
        ///     only for output patterns that contain sub xml tags (like the output
        ///     patterns) should use this.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        protected void WriteOutputExpression(System.Xml.XmlWriter writer, string value)
        {
            writer.WriteStartElement("Expression");
            writer.WriteRaw(value);
            writer.WriteEndElement();
        }

        /// <summary>The read output expression.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="string"/>.</returns>
        protected static string ReadOutputExpression(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Expression")
            {
                // bool wasEmpty = reader.IsEmptyElement;
                // reader.Read();
                // if (wasEmpty) return null;
                var iVal = reader.ReadInnerXml();

                // reader.ReadEndElement();
                return iVal;
            }

            return null;
        }

        #endregion

        #region time

        /// <summary>Writes a <paramref name="time"/> cluster.</summary>
        /// <param name="writer">The writer.</param>
        /// <param name="time">The time.</param>
        protected void WriteTime(System.Xml.XmlWriter writer, NeuronCluster time)
        {
            var iValue = Time.GetTime(time);
            if (iValue.HasValue)
            {
                XmlStore.WriteElement(writer, "Time", iValue.Value.ToString());
            }
            else
            {
                throw new System.InvalidCastException("Can't convert cluster to time value");
            }
        }

        /// <summary>Reads a time cluster.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected Neuron ReadTime(System.Xml.XmlReader reader)
        {
            var iTime = XmlStore.ReadElement<string>(reader, "Time");

            var iConv = System.DateTime.Parse(iTime);
            return Time.Current.GetTimeCluster(iConv);
        }

        #endregion
    }
}