// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorsHelper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the way neurons (or branches) should be deleted from an editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Defines the way neurons (or branches) should be deleted from an editor.
    /// </summary>
    public enum DeletionMethod
    {
        /// <summary>
        ///     Don't do anything
        /// </summary>
        Nothing, 

        /// <summary>
        ///     <see cref="Remove" /> from list, don't delete from network.
        /// </summary>
        Remove, 

        /// <summary>
        ///     <see cref="Remove" /> from list, if no longer referenced as child or
        ///     to-part of a link, delete it from the brain.
        /// </summary>
        DeleteIfNoRef, 

        /// <summary>
        ///     <see cref="Delete" /> from brain (even if there are still referenes).
        /// </summary>
        Delete
    }

    /// <summary>
    ///     A helper class for managing editor objects: frames, objects,...
    /// </summary>
    internal partial class EditorsHelper
    {
        /// <summary>Creates a new object cluster (or compound word, when there was a space
        ///     in the inputtext) based on the input text provided by the user. When
        ///     there is a space in the inputtext, a new compound word is created
        ///     (when required). The user can also select to create a new posgroup for
        ///     the obect if one doesn't yet exist. If there already is one, the
        ///     object is always added to it. Otherwise, the <paramref name="pos"/> is
        ///     simply attached to the object.</summary>
        /// <param name="includeMeaning">if set to <c>true</c> , the 'Create meaning' option is<see langword="checked"/> for the user..</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateNewObject(bool includeMeaning, Neuron pos)
        {
            string iObjectName = null;
            var iIn = new DlgNewObject();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.IncludeMeaning = includeMeaning;
            iIn.POS = pos;
            if ((bool)iIn.ShowDialog())
            {
                iObjectName = iIn.Name;
                var iCluster = NeuronFactory.GetCluster();
                WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
                var iClusterData = BrainData.Current.NeuronInfo[iCluster.ID];
                iClusterData.DisplayTitle = iObjectName;
                if (string.IsNullOrEmpty(iIn.Description) == false)
                {
                    iClusterData.DescriptionText = iIn.Description;
                }

                iCluster.Meaning = (ulong)PredefinedNeurons.Object;
                if (iIn.IncludeMeaning)
                {
                    var iNeuron = NeuronFactory.GetNeuron();
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    using (var iList = iCluster.ChildrenW) iList.Add(iNeuron);
                    BrainData.Current.DefaultMeaningIds.Add(iNeuron.ID);
                    BrainData.Current.NeuronInfo[iNeuron.ID].DisplayTitle = iObjectName;
                }

                CreateObjectData(iCluster, iIn);
                return iCluster;
            }

            return null;
        }

        /// <summary>Adds the text portion and link neuron to the object for<see cref="EditorsHelper.CreateNewObject"/> . Also creates the
        ///     posgroup if needed and all the related objects.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="info">The info.</param>
        private static void CreateObjectData(NeuronCluster cluster, DlgNewObject info)
        {
            var iPos = info.POS;
            var iObjectName = info.Name;
            if (string.IsNullOrEmpty(iObjectName) == false)
            {
                var iToAdd = ConvertStringToNeurons(iObjectName);
                System.Diagnostics.Debug.Assert(iToAdd != null);
                if (info.AsDummy == false)
                {
                    using (var iList = cluster.ChildrenW) iList.Add(iToAdd);

                    NeuronCluster iGroup = null; // don't forget to handle the posgroup.
                    if (iPos != null)
                    {
                        iGroup = BrainHelper.FindPOSGroup(iToAdd, iPos.ID);
                        if (iGroup == null)
                        {
                            if (info.CreatePOSGroup)
                            {
                                var iList = new System.Collections.Generic.List<Neuron>();
                                iList.Add(cluster);
                                iList.Add(iToAdd);

                                    // don't forget to add the text to the posgroup as well, otherwise we can't find it again.
                                iGroup = BrainHelper.CreatePOSGroup(iList, iPos.ID);
                                var iUndo = new NeuronUndoItem { Action = BrainAction.Created, Neuron = iGroup };
                                WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                                    // must be done before setting neuron Info, for the undo, otherwise the item gets deleted before changing the title, and can't recreate anymore correctly.
                                BrainData.Current.NeuronInfo[iGroup.ID].DisplayTitle = iObjectName;

                                    // we also need to provide a title for the posgroup.
                            }
                            else
                            {
                                Link.Create(cluster, iPos, (ulong)PredefinedNeurons.POS);

                                    // simply create a link, don't need undo data, this is destroyed if the object is destroyed.
                            }
                        }
                        else
                        {
                            TryAddChildToCluster(iGroup, cluster);
                        }

                        if (info.Conjugations != null && info.Conjugations.Count > 0)
                        {
                            CreateConjugations(info, cluster, iObjectName);
                        }
                    }
                }
                else
                {
                    if (iPos != null)
                    {
                        // dummys can also have a pos, so the filter function of the thesaurus also works on them.
                        Link.Create(cluster, iPos, (ulong)PredefinedNeurons.POS);

                            // simply create a link, don't need undo data, this is destroyed if the object is destroyed
                    }

                    Link.Create(cluster, iToAdd, (ulong)PredefinedNeurons.NameOfMember);

                        // dummys still store the name. This way, we can still find them using the name of the object, but they are not accessessable through the parse, cause the name isn't registered as normal. don't need undo data for the link, both neurons get deleted in an undo anyway, causing the link to be deleted (and recreated for a redo).
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Please provide a valid name: empty strings are not allowed.", 
                    "New object", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Uses a scanner to parse the input and convert it to a list of neurons.
        ///     If multiple words were found, a compound is returned, otherwise a
        ///     textneuron.</summary>
        /// <param name="toConv"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron ConvertStringToNeurons(string toConv)
        {
            Neuron iToAdd;
            var iToConv = toConv.ToLower();
            var iScanner = new Parsers.Tokenizer(iToConv); // we don't store capital in the actual neurons.
            iScanner.AllowEscaping = false; // we treat the / sign as a regular token, not an escape sign.
            var iParts = new System.Collections.Generic.List<string>();

            while (iScanner.CurrentToken != Parsers.Token.End)
            {
                if (iScanner.CurrentToken != Parsers.Token.Space)
                {
                    // we skip spaces, these aren't stored in the compound words.
                    iParts.Add(iScanner.CurrentValue);
                }

                iScanner.GetNext();
            }

            if (iParts.Count > 1)
            {
                iToAdd = GetCompoundWord(iParts, iToConv);
            }
            else
            {
                iToAdd = GetTextNeuronFor(toConv);
            }

            return iToAdd;
        }

        /// <summary>The create conjugations.</summary>
        /// <param name="info">The info.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="name">The name.</param>
        private static void CreateConjugations(DlgNewObject info, NeuronCluster cluster, string name)
        {
            System.Collections.Generic.IList<DlgNewObject.Conjugation> iItems = info.Conjugations;
            var iPos = info.POS;

            var iRendered = new System.Collections.Generic.Dictionary<string, NeuronCluster>();

                // so we only render each conjugation 1 time (for 'are' -> need to make certain that it's always the same, otherwise the parse don't work correctly
            foreach (var i in iItems)
            {
                if (i.Meaning != Neuron.EmptyId && string.IsNullOrEmpty(i.Name) == false)
                {
                    var iToAdd = ConvertStringToNeurons(i.Name);
                    NeuronCluster iObject;
                    if (iRendered.TryGetValue(i.Name, out iObject) == false)
                    {
                        iObject = CreateObjectFor(iToAdd, iPos.ID);
                        iRendered.Add(i.Name, iObject);
                    }

                    System.Diagnostics.Debug.Assert(iObject != null);
                    var iLink = Link.Create(cluster, iObject, i.Meaning);
                    var iUndo = new LinkUndoItem(iLink, BrainAction.Created);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    var iData = BrainData.Current.NeuronInfo[iObject.ID];
                    iData.DisplayTitle = i.Name;
                    iData.StoreDescription(BrainData.Current.NeuronInfo[i.Meaning].DisplayTitle + " of " + name);
                }
            }
        }

        /// <summary>Gets the object cluster for the specified word and pos, generating
        ///     undo data if a new item needed to be created.</summary>
        /// <param name="child">The child.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private static NeuronCluster CreateObjectFor(Neuron child, ulong pos)
        {
            var iRes = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iList = iRes.ChildrenW) iList.Add(child);
            var iLink = Link.Create(iRes, pos, (ulong)PredefinedNeurons.POS);
            return iRes;
        }

        /// <summary>Gets the text neuron for the specified word, generating undo data if a
        ///     new item needed to be created.</summary>
        /// <param name="word">The word.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        internal static TextNeuron GetTextNeuronFor(string word)
        {
            TextNeuron iRes;
            if (TextSin.Words.TryGetNeuron(word, out iRes) == false)
            {
                iRes = TextSin.Words.GetNeuronFor(word); // get it through the textsin, so that it is added to the dict.
                var iUndoData = new NeuronUndoItem { Neuron = iRes, ID = iRes.ID, Action = BrainAction.Created };
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            }

            return iRes;
        }

        /// <summary>Gets the compound word for the list of strings. If it doesn't exist
        ///     yet, one is created.</summary>
        /// <param name="parts">The parts.</param>
        /// <param name="caption">The caption.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetCompoundWord(System.Collections.Generic.List<string> parts, string caption)
        {
            Neuron iRes;
            var iItems = new System.Collections.Generic.List<Neuron>();
            foreach (var i in parts)
            {
                iItems.Add(GetTextNeuronFor(i));
            }

            iRes = BrainHelper.GetCompoundWord(iItems);
            if (iRes == null)
            {
                var iCompound = NeuronFactory.GetCluster();
                iCompound.Meaning = (ulong)PredefinedNeurons.CompoundWord;
                WindowMain.AddItemToBrain(iCompound);
                using (var iList = iCompound.ChildrenW) iList.AddRange(iItems);
                iRes = iCompound;
            }

            return iRes;
        }

        /// <summary>Creates an object for the specified text. No posgroup or or pos is
        ///     assigned.</summary>
        /// <param name="value">The value.</param>
        /// <param name="desc">The desc.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateObject(string value, string desc = null)
        {
            var iText = ConvertStringToNeurons(value);
            var iRes = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iList = iRes.ChildrenW) iList.Add(iText);
            var iInfo = BrainData.Current.NeuronInfo[iRes];
            iInfo.DisplayTitle = value;
            if (desc != null)
            {
                iInfo.StoreDescription(desc);
            }

            return iRes;
        }

        /// <summary>Creates an object for the specified text. No posgroup or or pos is
        ///     assigned.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateObject(TextNeuron value)
        {
            var iRes = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iList = iRes.ChildrenW) iList.Add(value);
            BrainData.Current.NeuronInfo[iRes].DisplayTitle = value.Text;
            return iRes;
        }

        /// <summary>Adds a synonym to the object cluster.</summary>
        /// <param name="addTo">The object to add a synonym to</param>
        /// <param name="toAdd">The synonym to add.</param>
        /// <param name="pos">The pos.</param>
        public static void AddSynonym(NeuronCluster addTo, string toAdd, Neuron pos)
        {
            WindowMain.UndoStore.BeginUndoGroup();
            try
            {
                var iText = ConvertStringToNeurons(toAdd);
                System.Diagnostics.Debug.Assert(addTo != null);
                TryAddChildToCluster(addTo, iText);
                BrainData.Current.NeuronInfo[addTo].DisplayTitle += ", " + toAdd;
                if (pos != null)
                {
                    var iPosgrp = BrainHelper.FindPOSGroup(iText, pos.ID);

                        // if there is a posgoup for the textneuron (otherwise create it), add the object to it since it can now also be triggered by that posgroup.
                    if (iPosgrp != null)
                    {
                        // only add to the posgroup if there already was one, don't create a new one, cause the word might already have existed, in which case it won't contain all the objects.
                        TryAddChildToCluster(iPosgrp, addTo);
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Changes the text part of an object. This includes the compound or
        ///     textneurons currently stored in the object as well as the display
        ///     title. if the textneurons/compounds are no longer used, they are
        ///     deleted. Also generates undo data for the change.</summary>
        /// <param name="toChange">The to Change.</param>
        /// <param name="value">The value.</param>
        public static void ChangeTextForObject(NeuronCluster toChange, string value)
        {
            var iText = ConvertStringToNeurons(value);
            var iRemoved = new System.Collections.Generic.List<Neuron>();
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = toChange.Children) iChildren = iList.ConvertTo<Neuron>();
            try
            {
                foreach (var i in iChildren)
                {
                    if (i is TextNeuron
                        || (i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord))
                    {
                        using (var iList = toChange.ChildrenW) iList.Remove(i.ID);
                        iRemoved.Add(i);
                    }
                }

                iChildren.Add(iText);
            }
            finally
            {
                Factories.Default.NLists.Recycle(iChildren);
            }

            var iRemoveUndo = new RemoveClusterUndoItem();
            iRemoveUndo.Cluster = toChange;
            iRemoveUndo.Index = -1; // simply to an add of the removed items.
            iRemoveUndo.Items = iRemoved;
            WindowMain.UndoStore.AddCustomUndoItem(iRemoveUndo);

            var iAddUndo = new AddClusterUndoItem();
            iAddUndo.Cluster = toChange;
            iAddUndo.Items = new System.Collections.Generic.List<Neuron> { iText };

            foreach (var i in iRemoved)
            {
                if (i.CanBeDeleted && BrainHelper.HasReferences(i) == false)
                {
                    var iDelUndo = new NeuronUndoItem { Action = BrainAction.Removed, Neuron = i };
                    WindowMain.UndoStore.AddCustomUndoItem(iDelUndo);
                    Brain.Current.Delete(i);
                }
            }

            BrainData.Current.NeuronInfo[toChange].DisplayTitle = value;
        }

        /// <summary>Creates a <see cref="Neuron"/> cluster that represents a compound word
        ///     for the specified string and returns the corresponding object cluster
        ///     for the compound word. Generates undo data.</summary>
        /// <remarks>To ge the elements of the compound word, we use a simple split on ' '.</remarks>
        /// <param name="parts">The parts.</param>
        /// <param name="caption">The caption.</param>
        /// <returns>A neuroncluster which represents the compound word cluster: it
        ///     contains a textneuron for each word.</returns>
        public static Neuron CreateCompoundWord(System.Collections.Generic.List<string> parts, string caption)
        {
            // need to make certain we work in lower caps.
            var iCompound = NeuronFactory.GetCluster();
            iCompound.Meaning = (ulong)PredefinedNeurons.CompoundWord;
            WindowMain.AddItemToBrain(iCompound);
            using (var iList = iCompound.ChildrenW)
                foreach (var iWord in parts)
                {
                    var iText = GetTextNeuronFor(iWord);
                    iList.Add(iText); // lock on each add. This is saver.
                }

            BrainData.Current.NeuronInfo[iCompound.ID].DisplayTitle = caption;
            return iCompound;
        }

        /// <summary>Makes the object, which consists out of a cluster, a text neuron and a
        ///     neuron that can be used as a meaning.</summary>
        /// <returns>The <see cref="MindMapCluster"/>.</returns>
        public static MindMapCluster MakeObject()
        {
            var iCluster = CreateNewObject(false, null);
            if (iCluster != null)
            {
                return MindMapNeuron.CreateFor(iCluster) as MindMapCluster;
            }

            return null;
        }

        /// <summary>
        ///     Asks the user for the name of a new frame and creates this frame.
        /// </summary>
        /// <remarks>
        ///     The frame's data is not loaded (
        ///     <see cref="JaStDev.HAB.Designer.Frame.IsLoaded" /> is still false).
        ///     This is up to the caller to activate it.
        /// </remarks>
        /// <returns>
        ///     A neuron cluster that represents the frame.
        /// </returns>
        public static Frame MakeFrame()
        {
            string iObjectName = null;
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "Frame name:";
            iIn.Answer = "New frame";
            iIn.Title = "New frame";
            if ((bool)iIn.ShowDialog())
            {
                iObjectName = iIn.Answer;
                var iCluster = NeuronFactory.GetCluster();
                iCluster.Meaning = (ulong)PredefinedNeurons.Frame;
                WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
                BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = iObjectName;
                return new Frame(iCluster);
            }

            return null;
        }

        /// <summary>Create a deep copy of the frame: all the elements are referenced, but
        ///     the list of sequences is also duplicated to allow for easy editing.</summary>
        /// <param name="iFrame">The i frame.</param>
        /// <returns>The <see cref="Frame"/>.</returns>
        public static Frame DuplicateFrame(Frame iFrame)
        {
            var iCluster = iFrame.Item.Duplicate() as NeuronCluster; // a frame is always a cluster.
            var iUndoData = new NeuronUndoItem { Neuron = iCluster, ID = iCluster.ID, Action = BrainAction.Created };
            BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = "Copy of " + iFrame.NeuronInfo.DisplayTitle;
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);

            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = iCluster.Children) iChildren = iList.ConvertTo<Neuron>();
            for (var i = 0; i < iChildren.Count; i++)
            {
                var iOld = iChildren[i];
                var iNew = DuplicateFrameElement(iOld, iCluster);
                var iReplaceUndo = new ReplaceClusterUndoItem();
                iReplaceUndo.Item = iOld;
                iReplaceUndo.Cluster = iCluster;
                iReplaceUndo.Action = System.Collections.Specialized.NotifyCollectionChangedAction.Replace;
                WindowMain.UndoStore.AddCustomUndoItem(iReplaceUndo);
                using (var iList = iCluster.ChildrenW) iList[i] = iNew.ID;
            }

            DuplicateSequences(iCluster, (NeuronCluster)iFrame.Item);
            return new Frame(iCluster);
        }

        /// <summary>Creates a duplicate of all the sequences attached to the<paramref name="frame"/> and stores them back in the frame, replacing
        ///     the originals. Generates undo data.</summary>
        /// <param name="frame">The cluster.</param>
        /// <param name="orFrame">The or Frame.</param>
        private static void DuplicateSequences(NeuronCluster frame, NeuronCluster orFrame)
        {
            var iSequences = frame.FindFirstOut((ulong)PredefinedNeurons.FrameSequences) as NeuronCluster;
            if (iSequences != null)
            {
                var iNewSequences = iSequences.Duplicate() as NeuronCluster;

                    // we duplicate so that we have all the data outside that of the frame
                System.Collections.Generic.List<NeuronCluster> iSeqs;
                using (var iList = iNewSequences.Children) iSeqs = iList.ConvertTo<NeuronCluster>();
                {
                    for (var i = 0; i < iSeqs.Count; i++)
                    {
                        var iOld = iSeqs[i];
                        var iNew = DuplicateFrameSequence(iOld, orFrame, frame);
                        var iReplaceUndo = new ReplaceClusterUndoItem();
                        iReplaceUndo.Item = iOld;
                        iReplaceUndo.Cluster = iNewSequences;
                        iReplaceUndo.Action = System.Collections.Specialized.NotifyCollectionChangedAction.Replace;
                        WindowMain.UndoStore.AddCustomUndoItem(iReplaceUndo);
                        using (var iList = iNewSequences.ChildrenW) iList[i] = iNew.ID;
                    }
                }

                var iLink = Link.Find(frame, iSequences, Brain.Current[(ulong)PredefinedNeurons.FrameSequences]);
                DeleteLink(iLink);
                iLink = Link.Find(orFrame, iNewSequences, Brain.Current[(ulong)PredefinedNeurons.FrameSequences]);
                DeleteLink(iLink); // also need undo data for this, so we can always redo the action correctly.
            }
        }

        /// <summary>Creates a duplicate of the cluster and duplcates every child in the
        ///     list as well. Generates undo data.</summary>
        /// <param name="old">The old.</param>
        /// <param name="orFrame">The original frame that we are duplicating. can be<see langword="null"/> if you are only duplicating a single sequence,
        ///     to the same frame. If you are duplicating an entire frame, we need to
        ///     update the link to the frame element.</param>
        /// <param name="newFrame">The new Frame.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster DuplicateFrameSequence(
            NeuronCluster old, 
            NeuronCluster orFrame, 
            NeuronCluster newFrame)
        {
            var iCluster = old.Duplicate() as NeuronCluster; // a frame is always a cluster.
            var iUndoData = new NeuronUndoItem { Neuron = iCluster, ID = iCluster.ID, Action = BrainAction.Created };
            BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = "Copy of "
                                                                     + BrainData.Current.NeuronInfo[old.ID].DisplayTitle;
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);

            using (IDListAccessor iChildren = iCluster.ChildrenW)
            {
                // using seperate locks for each statement cause we are creating new items inside the loop, which could cause deadlocks.
                for (var i = 0; i < iChildren.Count; i++)
                {
                    var iOld = Brain.Current[iChildren[i]];
                    var iNew = iOld.Duplicate();
                    var iReplaceUndo = new ReplaceClusterUndoItem();
                    iReplaceUndo.Item = iOld;
                    iReplaceUndo.Cluster = iCluster;
                    iReplaceUndo.Action = System.Collections.Specialized.NotifyCollectionChangedAction.Replace;
                    WindowMain.UndoStore.AddCustomUndoItem(iReplaceUndo);
                    iChildren[i] = iNew.ID;
                    if (orFrame != null && newFrame != null)
                    {
                        // we only try to adjust the ref to the frame element if the duplicate was to a new frame.
                        var iOrEl = iNew.FindFirstOut((ulong)PredefinedNeurons.FrameSequenceItemValue);
                        int iIndex;
                        using (var iList = orFrame.Children)
                            iIndex = iList.IndexOf(iOrEl.ID);

                                // get the index of the orignal frame element in the orignal frame, so we can get the frame element in the new frame at the same index.
                        var iLink = Link.Find(
                            iNew, 
                            iOrEl, 
                            Brain.Current[(ulong)PredefinedNeurons.FrameSequenceItemValue]);

                            // we get the link so that we cn change it.
                        var iLinkUndo = new LinkUndoItem(iLink, BrainAction.Changed);

                            // we need to remove the old restriciton + record this in the undo data (for consistency, if we don't it gets undone incorrectly)
                        WindowMain.UndoStore.AddCustomUndoItem(iLinkUndo);
                        using (var iList = newFrame.Children) iLink.To = Brain.Current[iList[iIndex]];
                    }
                }
            }

            return iCluster;
        }

        /// <summary>The duplicate visual f.</summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The <see cref="VisualFrame"/>.</returns>
        public static VisualFrame DuplicateVisualF(VisualFrame frame)
        {
            var iCluster = NeuronFactory.GetCluster();
            var iUndoData = new NeuronUndoItem { Neuron = iCluster, ID = iCluster.ID, Action = BrainAction.Created };
            BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = "Copy of " + frame.NeuronInfo.DisplayTitle;
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            iCluster.Meaning = (ulong)PredefinedNeurons.VisualFrame;

            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = iCluster.Children) iChildren = iList.ConvertTo<Neuron>();
            for (var i = 0; i < iChildren.Count; i++)
            {
                var iOld = iChildren[i];
                var iNew = iOld.Duplicate();
                WindowMain.AddItemToBrain(iNew);
                using (var iList = iCluster.ChildrenW) iList.Add(iNew);
            }

            Factories.Default.NLists.Recycle(iChildren);
            return new VisualFrame(iCluster);
        }

        /// <summary>
        ///     Creates an empty neuron and wraps it in a <see cref="FrameElement" />
        ///     object.
        /// </summary>
        /// <returns>
        ///     <see langword="null" /> if the user canceled, otherwise a new object
        ///     wrapper for a cluster.
        /// </returns>
        public static FrameElement MakeFrameElement()
        {
            var iRes = NeuronFactory.GetNeuron();
            WindowMain.AddItemToBrain(iRes);
            var iEl = new FrameElement(iRes);
            iEl.NeuronInfo.DisplayTitle = "FE: ";
            return iEl;
        }

        /// <summary>Creates a frame element using the specified neuron as role.</summary>
        /// <param name="role">The role.</param>
        /// <returns>The <see cref="FrameElement"/>.</returns>
        public static FrameElement MakeFrameElement(Neuron role)
        {
            var iRes = NeuronFactory.GetNeuron();
            WindowMain.AddItemToBrain(iRes);
            var iLink = new Link(role, iRes, (ulong)PredefinedNeurons.VerbNetRole);

                // no undo data required for the link, this is automicatlly destroyed when the newly created neuron gets deleted.
            var iEl = new FrameElement(iRes);
            iEl.NeuronInfo.DisplayTitle = "FE: " + BrainData.Current.NeuronInfo[role.ID].DisplayTitle;
            return iEl;
        }

        /// <summary>Creates a duplicate (deep copy) of the specified neuron, using the
        ///     Frame element structure, so any restrictions on the element whill also
        ///     be duplicated. For all new data, undo data is also generated.</summary>
        /// <param name="source">The source to copy.</param>
        /// <param name="frame">The frame that will own the new element. This is required to correctly
        ///     map the 'IsEvoker' link to the new frame cluster. If we don't do this,
        ///     we get a data inconsistency cause the element would still point to the
        ///     original one.</param>
        /// <returns>a deep copy of the orignal</returns>
        public static Neuron DuplicateFrameElement(Neuron source, Neuron frame)
        {
            var iNew = source.Duplicate();

                // we create a new element, by duplicating the prev, so that we have the same role and other links.
            RemoveSequenceItemRefs(iNew);
            var iSourceFrame = source.FindFirstOut((ulong)PredefinedNeurons.IsFrameEvoker);
            var iFrameEvoker = Brain.Current[(ulong)PredefinedNeurons.IsFrameEvoker];
            var iFound = Link.Find(source, frame, iFrameEvoker);
            if (iFound != null)
            {
                // if the source is a frame evoker, the element will point to 2 frames (because the cluster got duplicated, so it got a pointer to the original, which should be removed as well), the original and the new, so we need to remove the original pointer so that only the new remains.
                Link[] iTemp;
                using (var iList = iNew.LinksOut) iTemp = Enumerable.ToArray(iList);
                for (var i = 0; i < iTemp.Length; i++)
                {
                    var iLink = iTemp[i];
                    if (iLink.MeaningID == (ulong)PredefinedNeurons.IsFrameEvoker && iLink.ToID != frame.ID)
                    {
                        // get the link to a frame evoker that doesn't point to the new frame
                        iLink.Destroy();

                            // don't need to create undo data, this link should never have been duplicated. The duplicate gets deleted in its intiretiy.
                        break;
                    }
                }

                iFound.Destroy();

                    // we need to remove the old restriciton, no need for undo data, since this doesn't belong to the init data set.
            }
            else if (iSourceFrame != null)
            {
                iFound = Link.Find(iNew, iSourceFrame, iFrameEvoker);
                if (iFound != null)
                {
                    iFound.Destroy();
                    Link.Create(iNew, frame, (ulong)PredefinedNeurons.IsFrameEvoker);

                        // the old was a frame evoker, so make the new a frame evoker of it's frame.
                }
            }

            var iUndoData = new NeuronUndoItem { Neuron = iNew, ID = iNew.ID, Action = BrainAction.Created };

                // a duplicate doesn't generate undo data.
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            BrainData.Current.NeuronInfo[iNew.ID].CopyFrom(BrainData.Current.NeuronInfo[source.ID]); // must be done
            var iRestrictions = source.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions);
            if (iRestrictions != null)
            {
                var iMeaning = Brain.Current[(ulong)PredefinedNeurons.VerbNetRestrictions];
                var iLink = Link.Find(iNew, iRestrictions, iMeaning);
                System.Diagnostics.Debug.Assert(iLink != null);
                iLink.Destroy(); // don't need undo data since we are still creating the initial state of the neuron.
                var iNewRes = DuplicateFERestriction(iRestrictions); // get a duplicate of the restrictions
                iLink = Link.Find(source, iNewRes, iMeaning);

                    // need to remove the link from the new restriction to the orignal source
                System.Diagnostics.Debug.Assert(iLink != null);
                iLink.Destroy(); // don't need undo data, is for init state.
                iLink = new Link(iNewRes, iNew, iMeaning);
                var iLinkUndo = new LinkUndoItem(iLink, BrainAction.Created);
                WindowMain.UndoStore.AddCustomUndoItem(iLinkUndo);
            }

            return iNew;
        }

        /// <summary>Removes all the sequence item refs (in the incomming links) from the
        ///     neuron.</summary>
        /// <param name="iNew">The i new.</param>
        private static void RemoveSequenceItemRefs(Neuron iNew)
        {
            System.Collections.Generic.List<Link> iToRemove;
            using (var iLinks = iNew.LinksIn)
                iToRemove =
                    (from i in iLinks where i.MeaningID == (ulong)PredefinedNeurons.FrameSequenceItemValue select i)
                        .ToList();
            foreach (var i in iToRemove)
            {
                i.Destroy();
            }
        }

        /// <summary>Creates a new Frame element restriction.</summary>
        /// <param name="segment">The first, empty segment of the restriction.</param>
        /// <returns>The newly created restriction</returns>
        public static FERestriction MakeFERestriction(out FERestrictionSegment segment)
        {
            var iRes = NeuronFactory.GetCluster(); // a restriction is a cluster containing segments.
            iRes.Meaning = (ulong)PredefinedNeurons.VerbNetRestriction;
            WindowMain.AddItemToBrain(iRes);
            iRes.SetFirstOutgoingLinkTo(
                (ulong)PredefinedNeurons.VerbNetRestrictionModifier, 
                Brain.Current[(ulong)PredefinedNeurons.RestrictionModifierInclude]);

                // we already indicate that, by default, the value needs to be included. This doesn't have to be wrapped with an undo item cause during the undo, the neuron will be deleted, includin the link. 
            var iSegment = NeuronFactory.GetNeuron(); // we provide an initial, default empty segment.
            WindowMain.AddItemToBrain(iSegment);
            segment = new FERestrictionSegment(iSegment);
            var iWrapper = new FERestriction(iRes);
            iWrapper.Segments.Add(segment);
            return iWrapper;
        }

        /// <summary>creates a <see cref="FERestriction"/> without adding a new segment..</summary>
        /// <returns>The <see cref="FERestriction"/>.</returns>
        public static FERestriction MakeFERestriction()
        {
            var iRes = NeuronFactory.GetCluster(); // a restriction is a cluster containing segments.
            iRes.Meaning = (ulong)PredefinedNeurons.VerbNetRestriction;
            WindowMain.AddItemToBrain(iRes);
            iRes.SetFirstOutgoingLinkTo(
                (ulong)PredefinedNeurons.VerbNetRestrictionModifier, 
                Brain.Current[(ulong)PredefinedNeurons.RestrictionModifierInclude]);

                // we already indicate that, by default, the value needs to be included. This doesn't have to be wrapped with an undo item cause during the undo, the neuron will be deleted, includin the link. 
            var iWrapper = new FERestriction(iRes);
            return iWrapper;
        }

        /// <summary>Makes the FE segment.</summary>
        /// <param name="parent">The parent.</param>
        /// <returns>The <see cref="FERestrictionSegment"/>.</returns>
        public static FERestrictionSegment MakeFESegment(FERestriction parent)
        {
            var iNew = NeuronFactory.GetNeuron();
            WindowMain.AddItemToBrain(iNew);
            var iRes = new FERestrictionSegment(iNew);
            parent.Segments.Add(iRes);
            return iRes;
        }

        /// <summary>Creates a new Frame element restriction group.</summary>
        /// <returns>The <see cref="FERestrictionGroup"/>.</returns>
        public static FERestrictionGroup MakeFERestrictionGroup()
        {
            var iRes = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.VerbNetRestrictions;
            iRes.SetFirstOutgoingLinkTo(
                (ulong)PredefinedNeurons.VerbNetLogicValue, 
                Brain.Current[(ulong)PredefinedNeurons.Or]);

                // we already indicate that, by default, the 'or' operator should be used.
            return new FERestrictionGroup(iRes);
        }

        /// <summary>Creates a new Frame element custom filter.</summary>
        /// <returns>The <see cref="FECustomRestriction"/>.</returns>
        public static FECustomRestriction MakeFECustomFilter()
        {
            var iCluster = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.Code;
            var iRes = new FECustomRestriction(iCluster);
            iRes.NeuronInfo.DisplayTitle = "Custom restriction";
            return iRes;
        }

        /// <summary>Creates a new Frame element custom filter.</summary>
        /// <returns>The <see cref="FERestrictionBool"/>.</returns>
        public static FERestrictionBool MakeFEBoolFilter()
        {
            var iCluster = NeuronFactory.Get<BoolExpression>();
            WindowMain.AddItemToBrain(iCluster);
            var iRes = new FERestrictionBool(iCluster);
            return iRes;
        }

        /// <summary>Duplicates the frame element restriction by creating a deep copy: if
        ///     the restriction is a group, all the children are also duplicated. For
        ///     each duplication, undo data is created. If the restriction contains
        ///     segments, they are also duplicated correctly.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        internal static Neuron DuplicateFERestriction(Neuron source)
        {
            var iRes = source.Duplicate();
            var iUndoData = new NeuronUndoItem { Neuron = iRes, ID = iRes.ID, Action = BrainAction.Created };

                // a duplicate doesn't generate undo data.
            WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            if (source is NeuronCluster)
            {
                var iDest = (NeuronCluster)iRes;
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = iDest.Children) iChildren = iList.ConvertTo<Neuron>();
                for (var i = 0; i < iChildren.Count; i++)
                {
                    // we need to create a duplicate of each child to create a deep copy.
                    var iNew = DuplicateFERestriction(iChildren[i]);

                        // this also duplicates the segments correctly because a semgnet is just a neuron with some outgoign links
                    var iReplaceData = new ReplaceClusterUndoItem();
                    iReplaceData.Cluster = iDest;
                    iReplaceData.Index = i;
                    iReplaceData.Item = iChildren[i];
                    WindowMain.UndoStore.AddCustomUndoItem(iReplaceData);
                    using (var iList = iDest.ChildrenW) iList[i] = iNew.ID;
                }

                Factories.Default.NLists.Recycle(iChildren);
                return iRes;
            }

            return iRes;
        }

        /// <summary>
        ///     Asks the user for the name of a new object to be used as a frame
        ///     sequence and creates it.
        /// </summary>
        /// <returns>
        ///     <see langword="null" /> if the user canceled, otherwise a new object
        ///     wrapper for a cluster.
        /// </returns>
        public static FrameSequence MakeFrameSequence()
        {
            string iObjectName = null;
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "sequence name:";
            iIn.Answer = "New sequence";
            iIn.Title = "New sequence";
            if ((bool)iIn.ShowDialog())
            {
                iObjectName = iIn.Answer;
                var iCluster = NeuronFactory.GetCluster();
                iCluster.Meaning = (ulong)PredefinedNeurons.FrameSequence;
                WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
                BrainData.Current.NeuronInfo[iCluster.ID].DisplayTitle = iObjectName;
                return new FrameSequence(iCluster);
            }

            return null;
        }

        /// <summary>
        ///     Asks the user for the name of a new flow and creates it if 'ok' is
        ///     pressed.
        /// </summary>
        /// <returns>
        ///     Null if the user canceled, otherwise a new flow.
        /// </returns>
        public static Flow MakeFlow()
        {
            string iObjectName = null;
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "Flow name:";
            iIn.Answer = "New flow";
            iIn.Title = "New flow";
            if ((bool)iIn.ShowDialog())
            {
                iObjectName = iIn.Answer;
                var iCluster = NeuronFactory.GetCluster();
                iCluster.Meaning = (ulong)PredefinedNeurons.Flow;
                WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
                var iRes = new Flow();
                iRes.ItemID = iCluster.ID;
                iRes.NeuronInfo.DisplayTitle = iObjectName;
                iRes.Name = iObjectName;
                return iRes;
            }

            return null;
        }

        /// <summary>Creates a neuron cluster (meaning FlowItemConditional) and a new
        ///     option (conditiona statement) for a flow that wraps the cluster.</summary>
        /// <returns>The <see cref="FlowItemConditional"/>.</returns>
        public static FlowItemConditional MakeFlowOption()
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.FlowItemConditional;
            WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
            var iLink = new Link(Brain.Current.FalseNeuron, iCluster, (ulong)PredefinedNeurons.FlowItemIsLoop);
            var iRes = new FlowItemConditional(iCluster);
            return iRes;
        }

        /// <summary>Creates a neuron cluster (meaning FlowItemConditional) and a new loop
        ///     (conditiona statement) for a flow that wraps the cluster.</summary>
        /// <returns>The <see cref="FlowItemConditional"/>.</returns>
        public static FlowItemConditional MakeFlowLoop()
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.FlowItemConditional;
            WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
            var iLink = new Link(Brain.Current.TrueNeuron, iCluster, (ulong)PredefinedNeurons.FlowItemIsLoop);
            var iRes = new FlowItemConditional(iCluster);
            return iRes;
        }

        /// <summary>Creates a neuron cluster (meaning FlowItemConditionalPart) and a new
        ///     conditional statement part for a flow that wraps the cluster.</summary>
        /// <remarks>A conditional statement part is the '|' in {|||} and [|||]</remarks>
        /// <returns>The <see cref="FlowItemConditionalPart"/>.</returns>
        internal static FlowItemConditionalPart MakeFlowCondPart()
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.FlowItemConditionalPart;
            WindowMain.AddItemToBrain(iCluster); // we use this function cause it takes care of the undo data.
            var iRes = new FlowItemConditionalPart(iCluster);
            return iRes;
        }

        /// <summary>The insert flow item.</summary>
        /// <param name="toAdd">The to add.</param>
        /// <param name="relativeTo">The relative to.</param>
        internal static void InsertFlowItem(FlowItem toAdd, FlowItem relativeTo)
        {
            var iOwner = relativeTo.Owner as FlowItemBlock;
            if (iOwner == null)
            {
                var iFlow = relativeTo.Owner as Flow;
                System.Diagnostics.Debug.Assert(iFlow != null);
                iFlow.Items.Insert(iFlow.Items.IndexOf(relativeTo), toAdd);
            }
            else
            {
                if (!(toAdd is FlowItemConditionalPart))
                {
                    if (iOwner is FlowItemConditionalPart)
                    {
                        iOwner.Items.Insert(iOwner.Items.IndexOf(relativeTo), toAdd);
                    }
                    else
                    {
                        var iNewPart = MakeFlowCondPart();
                        iOwner.Items.Insert(iOwner.Items.IndexOf(relativeTo), iNewPart);
                        iNewPart.Items.Add(toAdd);
                    }
                }
                else
                {
                    if (iOwner is FlowItemConditional)
                    {
                        iOwner.Items.Insert(iOwner.Items.IndexOf(relativeTo), toAdd);
                    }
                    else
                    {
                        var iBlock = iOwner.Owner as FlowItemBlock;
                        if (iBlock == null)
                        {
                            var iFlow = relativeTo.Owner as Flow;
                            System.Diagnostics.Debug.Assert(iFlow != null);
                            iFlow.Items.Insert(iFlow.Items.IndexOf(iOwner), toAdd);
                        }
                        else
                        {
                            iBlock.Items.Insert(iBlock.Items.IndexOf(iOwner), toAdd);
                        }
                    }
                }
            }

            toAdd.IsSelected = true;

                // we do this at the end, when it has been added to the list, this way, all the others are also updated correctly.
        }

        /// <summary><para>Adds a <paramref name="flow"/> item relative to the second<paramref name="flow"/> item. Relative to means:</para>
        /// <list type="bullet"><item><description>for static: after the static.</description></item>
        /// <item><description>
        ///                 for conditional part: add at end of the part
        ///             </description></item>
        /// <item><description>
        ///                 fo conditional: add at end of conditional, make certain that there is
        ///                 a new part also added if the added item is not a part.
        ///             </description></item>
        /// </list>
        /// </summary>
        /// <param name="toAdd">To add.</param>
        /// <param name="relativeTo">The relative to.</param>
        /// <param name="flow">The flow that should own the item. This is required in case<paramref name="relativeTo"/> is empty.</param>
        internal static void AddFlowItem(FlowItem toAdd, FlowItem relativeTo, Flow flow)
        {
            if (relativeTo is FlowItemStatic)
            {
                int iIndex;
                var iOwner = relativeTo.Owner as FlowItemBlock;
                if (iOwner == null)
                {
                    System.Diagnostics.Debug.Assert(flow != null);
                    iIndex = flow.Items.IndexOf(relativeTo) + 1;
                    flow.Items.Insert(iIndex, toAdd);
                }
                else
                {
                    if (toAdd is FlowItemConditionalPart)
                    {
                        // we are adding a | when we are on a static, so we need to go up 1 more owner, so we add to the loop/option
                        relativeTo = iOwner;
                        iOwner = (FlowItemBlock)iOwner.Owner;
                    }

                    iIndex = iOwner.Items.IndexOf(relativeTo) + 1;
                    iOwner.Items.Insert(iIndex, toAdd);
                }
            }
            else if (relativeTo is FlowItemConditional)
            {
                if (!(toAdd is FlowItemConditionalPart))
                {
                    var iNewPart = MakeFlowCondPart();
                    ((FlowItemConditional)relativeTo).Items.Add(iNewPart);
                    iNewPart.Items.Add(toAdd);
                }
                else
                {
                    ((FlowItemConditional)relativeTo).Items.Add(toAdd);
                }
            }
            else if (relativeTo is FlowItemConditionalPart)
            {
                if (toAdd is FlowItemConditionalPart)
                {
                    var iCond = (FlowItemConditional)relativeTo.Owner;
                    iCond.Items.Add(toAdd);
                }
                else
                {
                    ((FlowItemConditionalPart)relativeTo).Items.Add(toAdd);
                }
            }
            else if (relativeTo == null)
            {
                System.Diagnostics.Debug.Assert(flow != null);
                flow.Items.Add(toAdd);
            }
            else
            {
                throw new System.InvalidOperationException("Unkown flowItem, don't know how to add relative to it.");
            }

            toAdd.IsSelected = true;

                // we do this at the end, when it has been added to the list, this way, all the others are also updated correctly.
        }

        /// <summary>Creates a wrapper flow item for the specified neuron. It checks if the<paramref name="neuron"/> is a cluster with as meaning
        ///     'FlowITemConditional' or 'FlowItemConditionalPart' and creates an
        ///     appropriate item accordingly.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="FlowItem"/>.</returns>
        public static FlowItem CreateFlowItemFor(Neuron neuron)
        {
            var iCluster = neuron as NeuronCluster;
            if (iCluster != null)
            {
                if (iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditional)
                {
                    return new FlowItemConditional(iCluster);
                }

                if (iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditionalPart)
                {
                    return new FlowItemConditionalPart(iCluster);
                }
            }

            return new FlowItemStatic(neuron);
        }

        /// <summary>Deletes the specified cluster and all the children recursively (so if
        ///     any of the children is a cluster, it's children will also be deleted).</summary>
        /// <param name="toDelete">To delete.</param>
        public static void DeleteRecursiveChildren(NeuronCluster toDelete)
        {
            System.Diagnostics.Debug.Assert(toDelete != null);
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = toDelete.Children) iChildren = iList.ConvertTo<Neuron>();
            try
            {
                foreach (var i in iChildren)
                {
                    var iSub = i as NeuronCluster;
                    if (iSub != null)
                    {
                        DeleteRecursiveChildren(iSub);
                    }
                    else
                    {
                        WindowMain.DeleteItemFromBrain(i);
                    }
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iChildren);
            }

            WindowMain.DeleteItemFromBrain(toDelete);
        }

        /// <summary>Deletes a cluster as if it were a frame: all the child neurons are
        ///     also deleted (including their restrictions), and a link on the<paramref name="frame"/> is searched for the sequences, each sequence
        ///     and the sequences cluster is also deleted. Properly generates undo
        ///     data for the delete, so that the <paramref name="frame"/> can recover
        ///     properly (finds it's neuron again when an undo is done))</summary>
        /// <param name="frame">The frame.</param>
        public static void DeleteFrame(Frame frame)
        {
            var iFrame = frame.Elements.Cluster;
            using (var iChildren = iFrame.Children)
            {
                while (iChildren.Count > 0)
                {
                    DeleteFrameElement(Brain.Current[iChildren[0]]);
                }
            }

            var iSeqs = iFrame.FindFirstOut((ulong)PredefinedNeurons.FrameSequences) as NeuronCluster;
            if (iSeqs != null)
            {
                DeleteRecursiveChildren(iSeqs);
            }

            WindowMain.DeleteItemFromBrain(iFrame);
        }

        /// <summary>Deletes a <paramref name="neuron"/> as if it were a frame element, so
        ///     if there is a link containing a restrictions cluster, this is deleted
        ///     recursively.</summary>
        /// <param name="neuron">The neuron.</param>
        public static void DeleteFrameElement(Neuron neuron)
        {
            var iFound = neuron.FindFirstOut((ulong)PredefinedNeurons.VerbNetRestrictions);
            if (iFound != null)
            {
                DeleteFERestriction(iFound);
            }

            System.Collections.Generic.List<Neuron> iSeqItems;
            using (var iLinks = neuron.LinksIn)
                iSeqItems =
                    (from i in iLinks where i.MeaningID == (ulong)PredefinedNeurons.FrameSequenceItemValue select i.From)
                        .ToList(); // we need to delete all the sequence items that relate to this frame element.

            foreach (var i in iSeqItems)
            {
                if (Neuron.IsEmpty(i.ID) == false)
                {
                    // the seq item could be reuesed and already deleted.
                    WindowMain.DeleteItemFromBrain(i);
                }
            }

            WindowMain.DeleteItemFromBrain(neuron);
        }

        /// <summary>Deletes a single frame element restriction from the network. When it
        ///     is a group or contains segments, it will be deleted recursively, a
        ///     single <paramref name="neuron"/> is simply deleted, without deleting
        ///     any outgoing links.</summary>
        /// <param name="neuron">The neuron.</param>
        internal static void DeleteFERestriction(Neuron neuron)
        {
            if (neuron is NeuronCluster)
            {
                DeleteRecursiveChildren((NeuronCluster)neuron);
            }
            else
            {
                WindowMain.DeleteItemFromBrain(neuron);
            }
        }

        /// <summary>The delete v frame.</summary>
        /// <param name="frame">The frame.</param>
        internal static void DeleteVFrame(VisualFrame frame)
        {
            var iFrame = frame.Items.Cluster;
            System.Collections.Generic.List<Neuron> iToDel;
            using (var iList = iFrame.Children) iToDel = iList.ConvertTo<Neuron>();
            foreach (var i in iToDel)
            {
                WindowMain.DeleteItemFromBrain(i);
            }

            Factories.Default.NLists.Recycle(iToDel);
            WindowMain.DeleteItemFromBrain(iFrame);
        }

        /// <summary>Gets the type of code <paramref name="item"/> that would be generated
        ///     for the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        internal static System.Type GetCodeItemTypeFor(Neuron item)
        {
            return GetCodeItemTypeFor(item.GetType());
        }

        /// <summary>Gets the type of code <paramref name="item"/> that would be generated
        ///     for the specified neuron type.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        internal static System.Type GetCodeItemTypeFor(System.Type item)
        {
            if (item == typeof(ResultStatement))
            {
                return typeof(CodeItemResultStatement);
            }

            if (item == typeof(Statement))
            {
                return typeof(CodeItemStatement);
            }

            if (item == typeof(ConditionalStatement))
            {
                return typeof(CodeItemConditionalStatement);
            }

            if (item == typeof(ConditionalExpression))
            {
                return typeof(CodeItemConditionalExpression);
            }

            if (item == typeof(Global))
            {
                return typeof(CodeItemGlobal);
            }

            if (item == typeof(Variable))
            {
                return typeof(CodeItemVariable);
            }

            if (item == typeof(SearchExpression))
            {
                return typeof(CodeItemSearchExpression);
            }

            if (item == typeof(BoolExpression))
            {
                return typeof(CodeItemBoolExpression);
            }

            if (item == typeof(Assignment))
            {
                return typeof(CodeItemAssignment);
            }

            if (item == typeof(LockExpression))
            {
                return typeof(CodeItemLockExpression);
            }

            if (item == typeof(ExpressionsBlock))
            {
                return typeof(CodeItemCodeBlock);
            }

            if (item == typeof(ByRefExpression))
            {
                return typeof(CodeItemByRef);
            }

            return typeof(CodeItemStatic);
        }

        /// <summary>Creates the code <paramref name="item"/> wrapper for the specified
        ///     item.</summary>
        /// <param name="item">The item to wrap with a <see cref="CodeItem"/> object.</param>
        /// <returns>The <see cref="CodeItem"/>.</returns>
        public static CodeItem CreateCodeItemFor(Neuron item)
        {
            return CreateCodeItemFor(item, true);
        }

        /// <summary>Creates the code <paramref name="item"/> wrapper for the specified
        ///     item.</summary>
        /// <remarks>This function allows you to specify if the code items need to be
        ///     registered with the eventMonitor (so they respond to changes in the
        ///     brain) or not. This is useful to display the code being executed,
        ///     without ever making it active, since it isn't. This safes memory and
        ///     processor speed.</remarks>
        /// <param name="item">The item to wrap with a <see cref="CodeItem"/> object.</param>
        /// <param name="isActive">if set to <c>true</c><see cref="JaStDev.HAB.Designer.EditorItem.IsActive"/> will be true,
        ///     otherwise false.</param>
        /// <returns>The <see cref="CodeItem"/>.</returns>
        public static CodeItem CreateCodeItemFor(Neuron item, bool isActive)
        {
            if (item is ResultStatement)
            {
                return new CodeItemResultStatement((ResultStatement)item, isActive);
            }

            if (item is Statement)
            {
                return new CodeItemStatement((Statement)item, isActive);
            }

            if (item is ConditionalStatement)
            {
                return new CodeItemConditionalStatement((ConditionalStatement)item, isActive);
            }

            if (item is ConditionalExpression)
            {
                return new CodeItemConditionalExpression((ConditionalExpression)item, isActive);
            }

            if (item is Global)
            {
                return new CodeItemGlobal((Global)item, isActive);
            }

            if (item is Local)
            {
                return new CodeItemLocal((Local)item, isActive);
            }

            if (item is Variable)
            {
                return new CodeItemVariable((Variable)item, isActive);
            }

            if (item is SearchExpression)
            {
                return new CodeItemSearchExpression((SearchExpression)item, isActive);
            }

            if (item is BoolExpression)
            {
                return new CodeItemBoolExpression((BoolExpression)item, isActive);
            }

            if (item is Assignment)
            {
                return new CodeItemAssignment((Assignment)item, isActive);
            }

            if (item is LockExpression)
            {
                return new CodeItemLockExpression((LockExpression)item, isActive);
            }

            if (item is ExpressionsBlock)
            {
                return new CodeItemCodeBlock((ExpressionsBlock)item, isActive);
            }

            if (item is ByRefExpression)
            {
                return new CodeItemByRef((ByRefExpression)item, isActive);
            }

            return new CodeItemStatic(item, isActive);
        }

        /// <summary>Determines whether the specified items can all be deleted (If there
        ///     are any items).</summary>
        /// <param name="toDelete">List of neurons to delete.</param>
        /// <returns><c>true</c> if they can all be deleted; otherwise, <c>false</c> .</returns>
        public static bool CanBeDeleted(System.Collections.Generic.IEnumerable<Neuron> toDelete)
        {
            var iCant = 0; // keeps track of how many can be deleted.
            var iFound = 0;
            foreach (var i in toDelete)
            {
                if (i.CanBeDeleted == false)
                {
                    iCant++;
                    iFound++;
                }
            }

            return iCant == 0 && iFound > 0;
        }

        /// <summary>Sets the first outgoing link the specified Neuron, to the new value.
        ///     During this process, the proper undo data is generated.</summary>
        /// <remarks>Note, we can't rely on OnPropertyChanging event handling of the undo
        ///     system, to handle link changes, cause the editor item that generated
        ///     the event can be replaced. Instead, we must use<see cref="LinkUndoItem"/> data.</remarks>
        /// <param name="fromN">The neuron to change an outgoing link for.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        public static void SetFirstOutgoingLinkTo(Neuron fromN, ulong meaning, EditorItem value)
        {
            Link iLink = null;
            LinkUndoItem iUndoData = null;
            if (fromN.LinksOutIdentifier != null)
            {
                using (var iList = fromN.LinksOut) iLink = (from i in iList where i.MeaningID == meaning select i).FirstOrDefault();
            }

            if (value != null)
            {
                if (iLink != null)
                {
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Changed);
                    iLink.To = value.Item;
                }
                else
                {
                    iLink = new Link(value.Item, fromN, meaning);
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Created);
                }
            }
            else if (iLink != null)
            {
                iUndoData = new LinkUndoItem(iLink, BrainAction.Removed);
                iLink.Destroy();
            }

            if (iUndoData != null)
            {
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            }
        }

        /// <summary>Sets the first outgoing link to the specified id, while generating
        ///     undo data.</summary>
        /// <param name="fromN">From.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        public static void SetFirstOutgoingLinkTo(Neuron fromN, ulong meaning, ulong value)
        {
            Link iLink = null;
            LinkUndoItem iUndoData = null;
            if (fromN.LinksOutIdentifier != null)
            {
                using (var iList = fromN.LinksOut) iLink = (from i in iList where i.MeaningID == meaning select i).FirstOrDefault();
            }

            if (Neuron.IsEmpty(value) == false)
            {
                if (iLink != null)
                {
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Changed);
                    iLink.To = Brain.Current[value];
                }
                else
                {
                    iLink = Link.Create(fromN, value, meaning);
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Created);
                }
            }
            else if (iLink != null)
            {
                iUndoData = new LinkUndoItem(iLink, BrainAction.Removed);
                iLink.Destroy();
            }

            if (iUndoData != null)
            {
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            }
        }

        /// <summary>Sets the first outgoing link to the specified id, while generating
        ///     undo data.</summary>
        /// <param name="fromN">From.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        public static void SetFirstOutgoingLinkTo(Neuron fromN, ulong meaning, Neuron value)
        {
            Link iLink = null;
            LinkUndoItem iUndoData = null;
            if (fromN.LinksOutIdentifier != null)
            {
                using (var iList = fromN.LinksOut) iLink = (from i in iList where i.MeaningID == meaning select i).FirstOrDefault();
            }

            if (value != null && Neuron.IsEmpty(value.ID) == false)
            {
                if (iLink != null)
                {
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Changed);
                    iLink.To = value;
                }
                else
                {
                    iLink = Link.Create(fromN, value, meaning);
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Created);
                }
            }
            else if (iLink != null)
            {
                iUndoData = new LinkUndoItem(iLink, BrainAction.Removed);
                iLink.Destroy();
            }

            if (iUndoData != null)
            {
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            }
        }

        /// <summary>Sets the first outgoing link the specified Neuron, to the new value.
        ///     During this process, the proper undo data is generated.</summary>
        /// <remarks>Note, we can't rely on OnPropertyChanging event handling of the undo
        ///     system, to handle link changes, cause the editor item that generated
        ///     the event can be replaced. Instead, we must use<see cref="LinkUndoItem"/> data.</remarks>
        /// <param name="fromN">The neuron to change the outgoing link for.</param>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value"><para>The value as a bool, this is resolved to<see cref="JaStDev.HAB.PredefinedNeurons.True"/></para>
        /// <para>or <see cref="JaStDev.HAB.PredefinedNeurons.True"/> .</para>
        /// </param>
        public static void SetFirstOutgoingLinkTo(Neuron fromN, ulong meaning, bool value)
        {
            Link iLink = null;
            LinkUndoItem iUndoData = null;
            if (fromN.LinksOutIdentifier != null)
            {
                using (var iList = fromN.LinksOut) iLink = (from i in iList where i.MeaningID == meaning select i).FirstOrDefault();
            }

            Neuron iVal = null;
            if (value)
            {
                iVal = Brain.Current.TrueNeuron;
            }

            if (iLink != null)
            {
                if (iVal != null)
                {
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Changed);
                    iLink.To = iVal;
                }
                else
                {
                    iUndoData = new LinkUndoItem(iLink, BrainAction.Removed);
                    iLink.Destroy();
                }
            }
            else
            {
                iLink = new Link(iVal, fromN, meaning);
                iUndoData = new LinkUndoItem(iLink, BrainAction.Created);
            }

            if (iUndoData != null)
            {
                WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
            }
        }

        /// <summary>Asks the user for a new name and assigns it to the specified neuron.</summary>
        /// <param name="toRename">To rename.</param>
        /// <param name="title">The title to use for the dialog box that is displayed to the user.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool RenameItem(Neuron toRename, string title)
        {
            var iData = BrainData.Current.NeuronInfo[toRename.ID];
            var iIn = new DlgStringQuestion();
            iIn.Owner = System.Windows.Application.Current.MainWindow;
            iIn.Question = "new name:";
            iIn.Answer = iData.DisplayTitle;
            iIn.Title = title;
            if ((bool)iIn.ShowDialog())
            {
                iData.DisplayTitle = iIn.Answer;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a new data object, sets some default dat and registers it with
        ///     the clipboard. All that remains is filling in the rest of the dat
        /// </summary>
        /// <returns>
        ///     A new data object to be used for the clipboard.
        /// </returns>
        internal static System.Windows.DataObject GetDataObject()
        {
            var iData = new System.Windows.DataObject();
            iData.SetData(Properties.Resources.APPREFFORMAT, System.Diagnostics.Process.GetCurrentProcess().Id, false);

                // we add the processor id, so we can check that a paste comes from the same process (which is required in some cases).
            return iData;
        }

        /// <summary>
        ///     Determines whether the data on the clipboard comes from this
        ///     application. If this is not the case, the operation is not allowed.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [is valid cipboard data]; otherwise, <c>false</c> .
        /// </returns>
        internal static bool IsValidCipboardData()
        {
            if (System.Windows.Clipboard.ContainsData(Properties.Resources.APPREFFORMAT))
            {
                return (int)System.Windows.Clipboard.GetData(Properties.Resources.APPREFFORMAT)
                       == System.Diagnostics.Process.GetCurrentProcess().Id;
            }

            return false;
        }

        /// <summary>Deletes a log item.</summary>
        /// <param name="item">The item.</param>
        internal static void DeleteLogItem(Neuron item)
        {
            var iToDel = new System.Collections.Generic.List<Neuron>();
            iToDel.Add(item);
            var iToDelIfEmpty = new System.Collections.Generic.List<Neuron>();
            var iTime = item.FindFirstOut((ulong)PredefinedNeurons.Time) as NeuronCluster;
            if (iTime != null)
            {
                iToDel.Add(iTime);
            }

            var iItem = item as NeuronCluster;
            if (iItem != null)
            {
                System.Collections.Generic.List<NeuronCluster> iChildren;
                using (var iList = iItem.Children) iChildren = iList.ConvertTo<NeuronCluster>();
                var iConvHistory = Brain.Current[(ulong)PredefinedNeurons.ConversationLogHistory];
                foreach (var i in iChildren)
                {
                    iToDel.Add(i);
                    var iLink = Link.Find(item, i, iConvHistory);
                    if (iLink != null)
                    {
                        iToDelIfEmpty.AddRange(iLink.Info.ConvertTo<Neuron>());
                    }
                }

                Factories.Default.CLists.Recycle(iChildren);
            }

            var iDel = new NeuronDeleter(DeletionMethod.Delete, DeletionMethod.DeleteIfNoRef);
            iDel.Start(iToDel);
            iDel.NeuronDeletionMethod = DeletionMethod.DeleteIfNoRef;
            iDel.Start(iToDelIfEmpty);
        }

        /// <summary>Deletes an object and all of it's children, while generating undo
        ///     data. If the posgroup is no longer valid (no more refs), this is also
        ///     deleted. The children are only deleted if there are no more refs left.</summary>
        /// <param name="toDelete">To delete.</param>
        internal static void DeleteObject(NeuronCluster toDelete)
        {
            System.Collections.Generic.IList<Neuron> iChildren = null;
            if (toDelete.ChildrenIdentifier != null)
            {
                using (var iList = toDelete.Children)
                    iChildren = iList.ConvertTo<Neuron>();

                        // don't wrap foreach, this will delete, but for as long as iList is active, the list is locked for reading.
            }

            System.Collections.Generic.List<NeuronCluster> iParents = null;
            if (toDelete.ClusteredByIdentifier != null)
            {
                using (var iList = toDelete.ClusteredBy)
                    iParents = iList.ConvertTo<NeuronCluster>();

                        // we get all the parents, so we can see which get to be empty after deleting the object. All clusters that become empty and which are part of the thesaurus, should also be deleted (these are the 'childlists'). This includes the pos parent.
            }

            WindowMain.DeleteItemFromBrain(toDelete); // the cluster itself always needs to be deleted.
            if (iParents != null)
            {
                DeleteParentsOfObject(iParents);
            }

            if (iChildren != null)
            {
                DeleteChildrenOfObject(iChildren);
            }
        }

        /// <summary>Deletes all the <paramref name="children"/> of an object that can also
        ///     be deleted when the object is deleted.</summary>
        /// <param name="children">The i children.</param>
        private static void DeleteChildrenOfObject(System.Collections.Generic.IList<Neuron> children)
        {
            foreach (var i in children)
            {
                System.Collections.Generic.IList<Neuron> iSubChildren = null;
                var iCompound = i as NeuronCluster;
                if (iCompound != null && iCompound.ChildrenIdentifier != null)
                {
                    // it's a compound, check if the different text neurons still have any refs.
                    using (var iList = iCompound.Children) iSubChildren = iList.ConvertTo<Neuron>();
                }

                if (i.IsDeleted == false && BrainHelper.HasReferences(i) == false)
                {
                    // it has no more references, so delete it. 
                    WindowMain.DeleteItemFromBrain(i);
                }

                if (iSubChildren != null)
                {
                    // try to delete the children of the compound after the compound itself was deleted, otherwise, the children still have a parent and will never be deleted.
                    foreach (var u in iSubChildren)
                    {
                        if (u.IsDeleted == false && BrainHelper.HasReferences(u) == false)
                        {
                            WindowMain.DeleteItemFromBrain(u);
                        }
                    }
                }
            }
        }

        /// <summary>The delete parents of object.</summary>
        /// <param name="parents">The parents.</param>
        private static void DeleteParentsOfObject(System.Collections.Generic.List<NeuronCluster> parents)
        {
            foreach (var i in parents)
            {
                if (i.Meaning == (ulong)PredefinedNeurons.POSGroup)
                {
                    using (var iList = i.Children)
                        if (iList.Count == 1)
                        {
                            WindowMain.DeleteItemFromBrain(i);
                        }
                }

                var iFound =
                    (from u in BrainData.Current.Thesaurus.Relationships
                     where u.Item != null && u.Item.ID == i.Meaning
                     select i).FirstOrDefault();

                    // check if the cluster is part of the thesaurus 'relationships'. If so, and it has become empty, delete it.
                if (iFound != null)
                {
                    using (var iList = i.Children)
                        if (iList.Count == 0)
                        {
                            WindowMain.DeleteItemFromBrain(i);
                        }
                }
            }
        }

        /// <summary>Deletes the <paramref name="link"/> while generating undo data.</summary>
        /// <param name="link">The link.</param>
        internal static void DeleteLink(Link link)
        {
            var iUndo = new LinkUndoItem(link, BrainAction.Removed);
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            link.Destroy();
        }

        /// <summary>Adds the child to the cluster, while generating undo data. If the
        ///     child is already in there, it isn't added again.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="toAdd">The to Add.</param>
        internal static void TryAddChildToCluster(NeuronCluster cluster, Neuron toAdd)
        {
            var iChildren = cluster.ChildrenW;
            iChildren.Lock(toAdd);
            try
            {
                if (iChildren.ContainsUnsafe(toAdd) == false)
                {
                    iChildren.AddUnsafe(toAdd);
                    var iUndo = new AddClusterUndoItem();
                    iUndo.Cluster = cluster;
                    iUndo.Items = new System.Collections.Generic.List<Neuron>();
                    iUndo.Items.Add(toAdd);
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                }
            }
            finally
            {
                iChildren.Unlock(toAdd);
                iChildren.Dispose();
            }
        }

        /// <summary>Copies the list of patterns to an XML string.</summary>
        /// <param name="values">The values.</param>
        /// <param name="editor">The editor.</param>
        /// <returns>The <see cref="string"/>.</returns>
        internal static string CopyPatternsToXml(System.Collections.Generic.IList<PatternEditorItem> values, 
            TextPatternEditor editor)
        {
            var iStreamer = new TopicXmlStreamer();
            return iStreamer.WriteTopicParts(values, editor);
        }

        /// <summary>Copies the patterns from an XML string and inserts them at the
        ///     specified location.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteRulesFromClipboard(
            TextPatternEditor editor, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.TEXTPATTERNDEFFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (editor != null)
                {
                    PatternRule iRule;
                    if (selected != null)
                    {
                        iRule = selected.Rule;
                    }
                    else
                    {
                        iRule = null;
                    }

                    return iStreamer.ReadRules(iItems, editor, iRule);
                }

                return iStreamer.ReadExpressions(iItems, null, selected);
            }

            return null;
        }

        /// <summary>The paste invalid patterns from clipboard.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteInvalidPatternsFromClipboard(
            TextPatternEditor editor, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.INVALIDPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (editor != null && selected.Output != null)
                {
                    return iStreamer.ReadInvalidPatterns(iItems, editor, selected);
                }

                return iStreamer.ReadExpressions(iItems, editor, selected);
            }

            return null;
        }

        /// <summary>The paste invalid patterns from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteInvalidPatternsFromClipboard(
            InvalidPatternResponseCollection list, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.INVALIDPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadInvalidPatterns(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste pattern conditions from clipboard.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternConditionsFromClipboard(
            TextPatternEditor editor, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.CONDITIONPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (selected.RuleOutput != null)
                {
                    return iStreamer.ReadConditionPatterns(iItems, editor, selected);
                }

                return iStreamer.ReadExpressions(iItems, editor, selected);
            }

            return null;
        }

        /// <summary>The paste pattern conditions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternConditionsFromClipboard(
            ConditionalOutputsCollection list, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.CONDITIONPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadConditionPatterns(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste pattern outputs from clipboard.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternOutputsFromClipboard(
            TextPatternEditor editor, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.OUTPUTPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (selected != null)
                {
                    if (selected.RuleOutput != null)
                    {
                        return iStreamer.ReadOutputPatterns(iItems, editor, selected);
                    }

                    return iStreamer.ReadExpressions(iItems, editor, selected);
                }

                throw new System.InvalidOperationException("No rule selected: can't perform paste operation");
            }

            return null;
        }

        /// <summary>The paste pattern outputs from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternOutputsFromClipboard(
            PatternOutputsCollection list, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.OUTPUTPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadOutputPatterns(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste pattern inputs from clipboard.</summary>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternInputsFromClipboard(
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.OUTPUTPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadInputPatterns(iItems, (TextPatternEditor)selected.Root, selected);
            }

            return null;
        }

        /// <summary>The paste pattern outputs from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternOutputsFromClipboard(
            ConditionalOutputsCollection list, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.OUTPUTPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iRuleOutput = selected.RuleOutput;
                System.Diagnostics.Debug.Assert(iRuleOutput != null);
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadOutputPatterns(iItems, iRuleOutput.Outputs, selected);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="props">The props.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            ChatbotProperties props, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (props.IsOpeningStatSelected)
                {
                    return iStreamer.ReadExpressionsInOutput(iItems, props.ConversationStarts, selected);
                }

                if (props.IsRepetetitionSelected)
                {
                    return iStreamer.ReadExpressionsInOutput(iItems, selected.RuleOutput.Outputs, selected);
                }

                if (props.IsFallbackSelected)
                {
                    return iStreamer.ReadExpressionsInOutput(iItems, props.FallBacks, selected);
                }

                if (props.IsDoAfterSelected)
                {
                    throw new System.NotImplementedException();
                }

                    // return iStreamer.ReadExpressionsInDo(iItems, props.DoAfterStatement, selected);
                if (props.IsDoOnStartupSelected)
                {
                    throw new System.NotImplementedException();
                }

                    // return iStreamer.ReadExpressionsInDo(iItems, props.DoOnStartup, selected);
                throw new System.InvalidOperationException();
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            PatternOutputsCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadExpressionsInOutput(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            ConditionalOutputsCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadExpressionsInConditions(iItems, list, selected as PatternRuleOutput);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            InvalidPatternResponseCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadExpressionsInInvalid(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            InputPatternCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadExpressionsInInput(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            TopicFilterCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            var iItems = System.Windows.Clipboard.GetData(format) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                return iStreamer.ReadExpressionsInTopicFilter(iItems, list, selected);
            }

            return null;
        }

        /// <summary>The paste expressions from clipboard.</summary>
        /// <param name="list">The list.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="List"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static System.Collections.Generic.List<PatternEditorItem> PasteExpressionsFromClipboard(
            DoPatternCollection list, 
            PatternEditorItem selected, 
            string format)
        {
            throw new System.NotImplementedException();

            // string iItems = Clipboard.GetData(format) as string;
            // if (iItems != null)
            // {
            // TopicXmlStreamer iStreamer = new TopicXmlStreamer();
            // return iStreamer.ReadExpressionsInDo(iItems, list, selected);
            // }
            // return null;
        }

        /// <summary>The paste pattern inputs from clipboard.</summary>
        /// <param name="editor">The editor.</param>
        /// <param name="selected">The selected.</param>
        /// <returns>The <see cref="List"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static System.Collections.Generic.List<PatternEditorItem> PastePatternInputsFromClipboard(
            TextPatternEditor editor, 
            PatternEditorItem selected)
        {
            var iItems = System.Windows.Clipboard.GetData(Properties.Resources.TEXTPATTERNFORMAT) as string;
            if (iItems != null)
            {
                var iStreamer = new TopicXmlStreamer();
                if (selected != null)
                {
                    if (editor != null && selected.Rule != null)
                    {
                        return iStreamer.ReadInputPatterns(iItems, editor, selected);
                    }

                    return iStreamer.ReadExpressions(iItems, editor, selected);
                }

                throw new System.InvalidOperationException("No rule selected: can't perform paste operation");
            }

            return null;
        }

        /// <summary>Peforms a paste operation of output patterns on a responseForOutput
        ///     item.</summary>
        /// <param name="grp">The grp.</param>
        /// <param name="insertAt">The insert at.</param>
        public static void PasteOutputsToResonponseFor(ResponsesForGroup grp, ResponseForOutput insertAt)
        {
            System.Collections.Generic.List<ulong> iItems;
            if (System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
            {
                iItems =
                    System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat) as
                    System.Collections.Generic.List<ulong>;
            }
            else if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
            {
                iItems = new System.Collections.Generic.List<ulong>();
                iItems.Add((ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat));
            }
            else
            {
                iItems = null;
            }

            if (iItems != null)
            {
                foreach (var i in iItems)
                {
                    var iItem = Brain.Current[i] as TextNeuron;
                    if (iItem != null)
                    {
                        if (insertAt != null)
                        {
                            grp.ResponseFor.Insert(grp.ResponseFor.IndexOf(insertAt), new ResponseForOutput(iItem));
                        }
                        else
                        {
                            grp.ResponseFor.Add(new ResponseForOutput(iItem));
                        }
                    }
                }
            }
        }

        /// <summary>Adds the pattern rule to the editor, just below the<paramref name="selected"/> item, if possible.</summary>
        /// <param name="toAdd">To add.</param>
        /// <param name="selected">The selected.</param>
        /// <param name="editor">The editor.</param>
        internal static void AddPatternRule(PatternRule toAdd, PatternEditorItem selected, TextPatternEditor editor)
        {
            PatternRule iPrev = null;
            if (selected is PatternRule)
            {
                iPrev = (PatternRule)selected;
            }
            else if (selected != null)
            {
                iPrev = selected.Owner as PatternRule;
                if (iPrev == null)
                {
                    var iRuleOutput = selected.Owner as PatternRuleOutput;
                    if (iRuleOutput != null)
                    {
                        iPrev = iRuleOutput.Owner as PatternRule;
                    }
                }
            }

            var iIndex = -1;
            if (iPrev != null)
            {
                // if there was no prev item, do an insert.
                iIndex = editor.Items.IndexOf(iPrev);
            }

            if (iIndex > -1)
            {
                editor.Items.Insert(iIndex, toAdd);
            }
            else
            {
                editor.Items.Add(toAdd);
            }
        }

        /// <summary>Deletes the text pattern editor from the project, including all the
        ///     child data. Also makes certain that the editor isn't locked by the
        ///     project overview (because it is visible there). creates an undo group
        ///     to make certain everything is done in the correct order.</summary>
        /// <param name="toDelete">To delete.</param>
        internal static void DeleteTextPatternEditor(TextPatternEditor toDelete)
        {
            var iTryAgain = new System.Collections.Generic.HashSet<Neuron>();

                // if there are refernces to rules on the same page, some items might need a second try before they can be deleted (after the relationship is delted)
            WindowMain.UndoStore.BeginUndoGroup();
            toDelete.IsOpen = true;

                // need to make certain that the data is loaded, otherwise we don't know what to delete. 
            try
            {
                RegWithTopicsDictionaryUndoItem iRegUndo = null;
                if (!(toDelete is ObjectTextPatternEditor))
                {
                    Parsers.TopicsDictionary.Remove(toDelete.Name, toDelete.Item);

                        // needs to be done before the editor is deleted, otherwise we can't remove the name ref anymore since the id has changed.
                    iRegUndo = new RegWithTopicsDictionaryUndoItem(toDelete.Item);
                }
                else if (toDelete.Items.Cluster != null && toDelete.Items.Cluster.ID != Neuron.TempId)
                {
                    Parsers.TopicsDictionary.Remove(
                        BrainData.Current.NeuronInfo[toDelete.Items.Cluster].DisplayTitle, 
                        toDelete.Items.Cluster);

                        // needs to be done before the editor is deleted, otherwise we can't remove the name ref anymore since the id has changed.
                    iRegUndo = new RegWithTopicsDictionaryUndoItem(toDelete.Item);
                }

                if (iRegUndo != null)
                {
                    WindowMain.UndoStore.AddCustomUndoItem(iRegUndo);
                }

                foreach (var i in toDelete.Items.ToArray())
                {
                    // we need to make a local copy of the list, cause deleting the editor while it is open causes the 'collection changed' exception.
                    DeletePatternRule(i, iTryAgain);
                }

                DeleteQuestions(toDelete.Item);
                toDelete.NeuronInfo.IsLocked = false; // make certain the editor is removable.
                var iName = toDelete.Item.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);

                    // get the name of the topic, so we can check if it is no longer used anywhere.
                if (Neuron.IsEmpty(toDelete.Items.Cluster.ID) == false)
                {
                    // don't try to delete a temp cluster.
                    DeleteAttachedVariables(toDelete.Item);
                    if (toDelete.Items.Cluster.CanBeDeleted)
                    {
                        WindowMain.DeleteItemFromBrain(toDelete.Items.Cluster);

                            // always go to the cluster, not the item, cause this can be an ObjectTextPatterneditor, which would mean it has 2 different sets: a list for the children and an item. for normal textpatternEditors these 2 lists are the same
                    }
                    else if (toDelete.Items.Cluster != null)
                    {
                        // if temp list?
                        iTryAgain.Add(toDelete.Items.Cluster);
                    }
                }

                if (iName != null && BrainHelper.HasReferences(iName) == false)
                {
                    if (iName is TextNeuron)
                    {
                        WindowMain.DeleteItemFromBrain(iName);
                    }
                    else if (iName is NeuronCluster)
                    {
                        // the name could be a compound.
                        DeleteObject((NeuronCluster)iName);
                    }
                }

                DeleteTextPatternRetries(iTryAgain);
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>looks for variables attached to the neuron. If they have no more
        ///     references, they get deleted. variables can get attached to a topic if
        ///     the topic was activated by a timer.</summary>
        /// <param name="neuron"></param>
        private static void DeleteAttachedVariables(Neuron neuron)
        {
            var iToDel = new System.Collections.Generic.List<Variable>();
            using (ListAccessor<Link> iOut = neuron.LinksOut)
            {
                foreach (var i in iOut)
                {
                    var iTo = i.To as Variable;
                    if (iTo != null)
                    {
                        iToDel.Add(iTo);
                    }
                }
            }

            foreach (var i in iToDel)
            {
                if (BrainHelper.HasIncommingReferences(i) == false)
                {
                    WindowMain.DeleteItemFromBrain(i);
                }
            }
        }

        /// <summary>retries to delete the items that couldn't be removed the first time
        ///     (cause they still had some references in the patterns).</summary>
        /// <param name="iTryAgain"></param>
        internal static void DeleteTextPatternRetries(System.Collections.Generic.HashSet<Neuron> iTryAgain)
        {
            foreach (var i in iTryAgain)
            {
                if (i.CanBeDeleted)
                {
                    WindowMain.DeleteItemFromBrain(i);
                }
            }
        }

        /// <summary>checks if the specified topic has a questions cluster attached. If so,
        ///     this is deleted together with it's contents.</summary>
        /// <param name="neuron"></param>
        internal static void DeleteQuestions(Neuron neuron)
        {
            var iQuestions = neuron.FindFirstOut((ulong)PredefinedNeurons.Questions) as NeuronCluster;
            DeleteConditionals(iQuestions);
        }

        /// <summary>Deletes a pattern definition from the network.</summary>
        /// <param name="rule">The pattern def.</param>
        /// <param name="tryAgain">The try Again.</param>
        internal static void DeletePatternRule(PatternRule rule, System.Collections.Generic.HashSet<Neuron> tryAgain)
        {
            var iRule = rule.Item as NeuronCluster;
            DeleteInputs(iRule);
            DeleteCode(rule);
            DeleteOutputs(iRule);
            var iName = iRule.FindFirstOut((ulong)PredefinedNeurons.NameOfMember);

                // get the name of the rule, so we can check if it is no longer used anywhere.
            if (iRule.CanBeDeleted)
            {
                // && (iName == null || BrainHelper.HasOtherReferences(iName, new List<ulong>() { iRule.ID }) == false)
                WindowMain.DeleteItemFromBrain(iRule);
                if (iName != null)
                {
                    if (BrainHelper.HasReferences(iName) == false)
                    {
                        if (iName is TextNeuron)
                        {
                            WindowMain.DeleteItemFromBrain(iName);
                        }
                        else if (iName is NeuronCluster)
                        {
                            // the name could be a compound.
                            DeleteObject((NeuronCluster)iName);
                        }
                    }
                    else if (tryAgain.Contains(iName) == false)
                    {
                        tryAgain.Add(iName);
                    }
                }
            }
            else if (tryAgain.Contains(iRule) == false)
            {
                tryAgain.Add(iRule);
            }
        }

        /// <summary>The delete code.</summary>
        /// <param name="rule">The rule.</param>
        private static void DeleteCode(PatternRule rule)
        {
            if (rule.ToCalculate != null)
            {
                DeleteDoPatterns(rule.ToCalculate);
            }

            if (rule.ToEvaluate != null)
            {
                DeleteDoPatterns(rule.ToEvaluate);
            }

            var iCode = rule.Item.FindFirstOut((ulong)PredefinedNeurons.Evaluate) as TextNeuron;
            if (iCode != null)
            {
                DeleteDoPattern(iCode);
            }

            iCode = rule.Item.FindFirstOut((ulong)PredefinedNeurons.Calculate) as TextNeuron;
            if (iCode != null)
            {
                DeleteDoPattern(iCode);
            }
        }

        /// <summary>Deletes all the outputs.</summary>
        /// <param name="rule">The rule.</param>
        private static void DeleteOutputs(Neuron rule)
        {
            var iConds = rule.FindFirstOut((ulong)PredefinedNeurons.Condition) as NeuronCluster;
            DeleteConditionals(iConds);
            var iOuts = rule.FindFirstOut((ulong)PredefinedNeurons.TextPatternOutputs) as NeuronCluster;
            if (iOuts != null)
            {
                DeleteConditionalPattern(iOuts);
            }

            var iResponses = rule.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;
            if (iResponses != null)
            {
                System.Collections.Generic.List<NeuronCluster> iResponseItems = null;
                using (IDListAccessor iChildren = iResponses.Children) iResponseItems = iChildren.ConvertTo<NeuronCluster>();
                if (iResponseItems != null)
                {
                    foreach (var iOut in iResponseItems)
                    {
                        DeleteResponsesGroup(iOut);
                    }
                }

                if (iResponses != null && Neuron.IsEmpty(iResponses.ID) == false)
                {
                    WindowMain.DeleteItemFromBrain(iResponses);
                }
            }
        }

        /// <summary>The delete conditionals.</summary>
        /// <param name="toDelete">The to delete.</param>
        private static void DeleteConditionals(NeuronCluster toDelete)
        {
            if (toDelete != null)
            {
                System.Collections.Generic.List<NeuronCluster> iContItems = null;
                using (IDListAccessor iChildren = toDelete.Children) iContItems = iChildren.ConvertTo<NeuronCluster>();
                if (iContItems != null)
                {
                    foreach (var iOut in iContItems)
                    {
                        DeleteConditionalPattern(iOut);
                    }
                }

                if (toDelete != null && Neuron.IsEmpty(toDelete.ID) == false)
                {
                    WindowMain.DeleteItemFromBrain(toDelete);
                }
            }
        }

        /// <summary>Deletes all the inputs in a <paramref name="rule"/></summary>
        /// <param name="rule">The rule.</param>
        private static void DeleteInputs(NeuronCluster rule)
        {
            System.Collections.Generic.List<Neuron> iToDel = null;
            using (IDListAccessor iChildren = rule.Children) iToDel = iChildren.ConvertTo<Neuron>();
            try
            {
                foreach (var i in iToDel)
                {
                    Parsers.InputParser.RemoveInputPattern(i);

                        // if there was a previously parsed pattern attached, remove that.
                    var iUndo = new ClearTextPatternBuildUndoItem();
                    iUndo.Pattern = (TextNeuron)i;
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    WindowMain.DeleteItemFromBrain(i);
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iToDel);
            }
        }

        /// <summary>Deletes a single group of responses( for another output).</summary>
        /// <param name="owner">The owner.</param>
        internal static void DeleteResponsesGroup(NeuronCluster owner)
        {
            var iResponsesFor = owner.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs) as NeuronCluster;

                // this is actually the cluster that contains all the children, not the actual textneuron.
            if (iResponsesFor != null)
            {
                DeleteResponsesFor(iResponsesFor);
            }

            System.Collections.Generic.List<NeuronCluster> iConds = null;
            using (IDListAccessor iChildren = owner.Children) iConds = iChildren.ConvertTo<NeuronCluster>();
            if (iConds != null)
            {
                try
                {
                    foreach (var iOut in iConds)
                    {
                        // we make a local copy cause we are going to modify the list.
                        DeleteConditionalPattern(iOut);
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iConds);
                }
            }

            WindowMain.DeleteItemFromBrain(owner);
        }

        /// <summary>deletes the cluster that contains all the 'responsesFor' outputs
        ///     and/or patterns + the cluster itself</summary>
        /// <param name="toDel">The to Del.</param>
        private static void DeleteResponsesFor(NeuronCluster toDel)
        {
            System.Collections.Generic.List<TextNeuron> iTexts = null;
            using (IDListAccessor iChildren = toDel.Children) iTexts = iChildren.ConvertTo<TextNeuron>();
            if (iTexts != null)
            {
                foreach (var i in iTexts)
                {
                    if (i.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternOutputs) == null)
                    {
                        // it's not a reference to an already existing output pattern, but a filter pattern, which also needs to be deleted.
                        Parsers.InputParser.RemoveInputPattern(i);

                            // if there was a previously parsed pattern attached, remove that.
                        var iUndo = new ClearTextPatternBuildUndoItem();
                        iUndo.Pattern = i;
                        WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                        WindowMain.DeleteItemFromBrain(i);
                    }
                }
            }

            WindowMain.DeleteItemFromBrain(toDel);
        }

        /// <summary>Deletes a conditional pattern.</summary>
        /// <param name="cond">The cond.</param>
        public static void DeleteConditionalPattern(NeuronCluster cond)
        {
            System.Collections.Generic.List<TextNeuron> iOuts = null;
            using (IDListAccessor iChildren = cond.Children) iOuts = iChildren.ConvertTo<TextNeuron>();
            if (iOuts != null)
            {
                foreach (var i in iOuts)
                {
                    // create local copy cause we are going to change the list during the loop
                    DeletePatternOutput(i);
                }
            }

            var iDos = cond.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iDos != null)
            {
                DeleteDoPattern(iDos);
            }

            var iCondition = cond.FindFirstOut((ulong)PredefinedNeurons.Condition) as TextNeuron;
            if (iCondition != null)
            {
                // if there is a condition attached tot he output, also delete it.
                Parsers.ConditionParser.RemoveCondPattern(iCondition);
                var iUndo = new ClearConditionPatterUndoItem { Pattern = iCondition };
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                WindowMain.DeleteItemFromBrain(iCondition);
            }

            // also need to delete the cluster that contains all the outputs. This is the actual output obect. 
            if (cond != null && Neuron.IsEmpty(cond.ID) == false)
            {
                // don't need to delete a temp cluster.
                var iUsed = cond.FindFirstOut((ulong)PredefinedNeurons.UsedResponses);
                if (iUsed != null)
                {
                    WindowMain.DeleteItemFromBrain(iUsed);

                        // if there is an extra cluster to keep the random list unique for as long as there are still items that weren't used before.
                }

                WindowMain.DeleteItemFromBrain(cond);
            }
        }

        /// <summary>Deletes the do patterns list.</summary>
        /// <param name="toDel">To del.</param>
        public static void DeleteDoPatterns(DoPatternCollection toDel)
        {
            var iToDel = new System.Collections.Generic.List<Neuron>();
            foreach (var i in toDel)
            {
                iToDel.Add(i.Item);
            }

            foreach (TextNeuron i in iToDel)
            {
                DeleteDoPattern(i);
            }

            var iConds = toDel.Cluster;
            if (iConds != null && Neuron.IsEmpty(iConds.ID) == false)
            {
                WindowMain.DeleteItemFromBrain(iConds);
            }
        }

        /// <summary>deletes a single do pattern.</summary>
        /// <param name="toDel"></param>
        public static void DeleteDoPattern(TextNeuron toDel)
        {
            Parsers.DoParser.RemoveDoPattern(toDel);
            var iUndo = new ClearDoPatterUndoItem { Pattern = toDel };
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);
            WindowMain.DeleteItemFromBrain(toDel);
        }

        /// <summary>Deletes a PatternOutput object from the network.</summary>
        /// <param name="output">The output.</param>
        internal static void DeletePatternOutput(TextNeuron output)
        {
            Parsers.OutputParser.RemoveOutputPattern(output);
            var iUndo = new ClearOutputPatterUndoItem();
            iUndo.Pattern = output;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);

            var iInvalidCl = output.FindFirstOut((ulong)PredefinedNeurons.InvalidResponsesForPattern) as NeuronCluster;
            DeleteInvalidResponses(iInvalidCl);

            var iDo = output.FindFirstOut((ulong)PredefinedNeurons.DoPatterns) as TextNeuron;
            if (iDo != null)
            {
                // if there is a do pattern attached tot he output, also delete it.
                Parsers.DoParser.RemoveDoPattern(iDo);
                var iUndo2 = new ClearDoPatterUndoItem { Pattern = iDo };
                WindowMain.UndoStore.AddCustomUndoItem(iUndo2);
                WindowMain.DeleteItemFromBrain(iDo);
            }

            var iResponsesFor = output.FindFirstOut((ulong)PredefinedNeurons.ResponseForOutputs);

                // this is actually the cluster that contains all the children, not the actual textneuron.
            if (iResponsesFor != null)
            {
                WindowMain.DeleteItemFromBrain(iResponsesFor);
            }

            WindowMain.DeleteItemFromBrain(output);
        }

        /// <summary>The delete invalid responses.</summary>
        /// <param name="iInvalidCl">The i invalid cl.</param>
        public static void DeleteInvalidResponses(NeuronCluster iInvalidCl)
        {
            if (iInvalidCl != null && Neuron.IsEmpty(iInvalidCl.ID) == false)
            {
                ClearOutputPatterUndoItem iUndo;
                System.Collections.Generic.List<Neuron> iToDel;
                using (var iList = iInvalidCl.Children) iToDel = iList.ConvertTo<Neuron>();
                foreach (var i in iToDel)
                {
                    Parsers.OutputParser.RemoveOutputPattern((TextNeuron)i);
                    iUndo = new ClearOutputPatterUndoItem();
                    iUndo.Pattern = (TextNeuron)i;
                    WindowMain.UndoStore.AddCustomUndoItem(iUndo);
                    WindowMain.DeleteItemFromBrain(i);
                }

                Factories.Default.NLists.Recycle(iToDel);
                WindowMain.DeleteItemFromBrain(iInvalidCl);
            }
        }

        /// <summary>Creates a new, empty pattern definition and adds it to the specified
        ///     editor.</summary>
        /// <param name="editor">The i editor.</param>
        /// <param name="insertAt">The insert At.</param>
        /// <returns>The <see cref="PatternRule"/>.</returns>
        internal static PatternRule MakePatternRule(TextPatternEditor editor, int insertAt = -1)
        {
            var iNew = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iNew);
            iNew.Meaning = (ulong)PredefinedNeurons.PatternRule;

            var iRes = new PatternRule(iNew);
            if (insertAt == -1)
            {
                editor.Items.Add(iRes);
            }
            else
            {
                if (insertAt > -1 && insertAt < editor.Items.Count)
                {
                    editor.Items.Insert(insertAt, iRes);
                }
                else
                {
                    editor.Items.Add(iRes);
                }
            }

            return iRes;
        }

        /// <summary>Adds a new text pattern using the specified string and tries to parse
        ///     it.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="InputPattern"/>.</returns>
        internal static InputPattern AddNewTextPattern(InputPatternCollection list, string value, int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new InputPattern(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            iNew.Selectionrange = new SelectionRange { Start = value.Length, Length = 0 };

                // set the cursor pos at the end.
            return iNew;
        }

        /// <summary>The add new topic filter.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="TopicFilterPattern"/>.</returns>
        internal static TopicFilterPattern AddNewTopicFilter(TopicFilterCollection list, string value, int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new TopicFilterPattern(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            iNew.Selectionrange = new SelectionRange { Start = value.Length, Length = 0 };

                // set the cursor pos at the end.
            return iNew;
        }

        /// <summary>Adds a new text pattern using the specified string and tries to parse
        ///     it.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="ResponseForOutput"/>.</returns>
        internal static ResponseForOutput AddNewPatternStyleResponseFor(
            ResponsesForCollection list, 
            string value, 
            int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new ResponseForOutput(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            iNew.Selectionrange = new SelectionRange { Start = value.Length, Length = 0 };

                // set the cursor pos at the end.
            return iNew;
        }

        /// <summary>Creates a new condition and assigns it to the conditional</summary>
        /// <param name="cond">The cond.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="ConditionPattern"/>.</returns>
        internal static ConditionPattern AddNewCondition(PatternRuleOutput cond, string value)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            Link.Create(cond.Item, iVal, (ulong)PredefinedNeurons.Condition);
            cond.Condition.Expression = value; // do after adding to list, this will parse the expression correctly
            return cond.Condition;
        }

        /// <summary>Adds a new do-pattern using the specified string and tries to parse
        ///     it.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="DoPattern"/>.</returns>
        internal static DoPattern AddNewDoPattern(DoPatternCollection list, string value, int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new DoPattern(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            return iNew;
        }

        /// <summary>creates a new do pattern for the specified text and generates undo
        ///     data for this.</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="DoPattern"/>.</returns>
        internal static DoPattern CreateDoPattern(string value)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new DoPattern(iVal);
            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            return iNew;
        }

        /// <summary>Adds a new conditional pattern output section to the list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="index">The index to add the conditional pattern at, default is -1 meaning at
        ///     the end.</param>
        /// <returns>The <see cref="PatternRuleOutput"/>.</returns>
        internal static PatternRuleOutput AddNewConditionalToPattern(ConditionalOutputsCollection list, int index = -1)
        {
            var iCluster = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.TextPatternOutputs;
            var iRes = new PatternRuleOutput(iCluster);
            if (index == -1 || list.Count == 0)
            {
                list.Add(iRes);
            }
            else
            {
                list.Insert(index, iRes);
            }

            return iRes;
        }

        /// <summary>The add new responses group.</summary>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="ResponsesForGroup"/>.</returns>
        internal static ResponsesForGroup AddNewResponsesGroup(ResponseValuesCollection list, int index = -1)
        {
            var iCluster = NeuronFactory.GetCluster();
            WindowMain.AddItemToBrain(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.Condition;
            var iRes = new ResponsesForGroup(iCluster);
            if (index == -1)
            {
                list.Add(iRes);
            }
            else
            {
                list.Insert(index, iRes);
            }

            return iRes;
        }

        /// <summary>Adds a new output pattern based on the specified<paramref name="value"/> and parses it.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="OutputPattern"/>.</returns>
        internal static OutputPattern AddNewOutputPattern(PatternOutputsCollection list, string value, int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new OutputPattern(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            return iNew;
        }

        /// <summary>The add new invalid pattern response.</summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="InvalidPatternResponse"/>.</returns>
        internal static InvalidPatternResponse AddNewInvalidPatternResponse(
            InvalidPatternResponseCollection list, 
            string value, 
            int index = -1)
        {
            var iVal = NeuronFactory.Get<TextNeuron>();
            WindowMain.AddItemToBrain(iVal);
            var iNew = new InvalidPatternResponse(iVal);
            if (index == -1)
            {
                list.Add(iNew);
            }
            else
            {
                list.Insert(index, iNew);
            }

            iNew.Expression = value; // do after adding to list, this will parse the expression correctly
            return iNew;
        }

        #region ChangeTo

        /// <summary>Asks the user if he really wants to change the<paramref name="neuron"/> to the specified <paramref name="type"/>
        ///     (checks if data is lost). When the user says yes, the operation is
        ///     performed.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="type">The type.</param>
        internal static void TryChangeTypeTo(Neuron neuron, System.Type type)
        {
            if (AskOkForNeuronChange(neuron, type) == System.Windows.MessageBoxResult.Yes)
            {
                neuron = neuron.ChangeTypeTo(type);
            }
        }

        /// <summary>Tries to change the <paramref name="type"/> of all the specified<paramref name="neurons"/> to new type.</summary>
        /// <param name="neurons">The neurons.</param>
        /// <param name="type">The type.</param>
        internal static void TryChangeTypeTo(Neuron[] neurons, System.Type type)
        {
            foreach (var i in neurons)
            {
                var iRes = AskOkForNeuronChange(i, type);
                if (iRes == System.Windows.MessageBoxResult.Yes)
                {
                    i.ChangeTypeTo(type);
                }
                else if (iRes == System.Windows.MessageBoxResult.Cancel)
                {
                    break;
                }
            }
        }

        /// <summary>Asks the ok from the user to change the <paramref name="neuron"/> to
        ///     the new type.</summary>
        /// <remarks>We set this in a seperate function cause we need to check to
        ///     convertion -&gt; if it is allowed + what message text to display to the
        ///     user.</remarks>
        /// <param name="neuron">The neuron.</param>
        /// <param name="type">The type.</param>
        /// <returns>True if the user said ok, <see langword="false"/> otherwise.</returns>
        internal static System.Windows.MessageBoxResult AskOkForNeuronChange(Neuron neuron, System.Type type)
        {
            string iMsg;
            if (neuron is NeuronCluster || neuron is ValueNeuron)
            {
                // these are all the types that, when changed will loose data.
                iMsg = string.Format(
                    "Changing the neuron: {0} to a {1} will loose some data, continue?", 
                    neuron, 
                    type.Name);
            }
            else
            {
                iMsg = string.Format("Change the neuron: {0} to a {1}?", neuron, type.Name);
            }

            var iRes = System.Windows.MessageBox.Show(
                iMsg, 
                "Change neuron type", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question, 
                System.Windows.MessageBoxResult.Yes);
            return iRes;
        }

        #endregion
    }
}