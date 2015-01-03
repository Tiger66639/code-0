// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainHelper.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the arguments for the <see cref="BrainHelper.GetObject" />
//   function.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     Contains all the arguments for the <see cref="BrainHelper.GetObject" />
    ///     function.
    /// </summary>
    public class GetObjectArgs
    {
        /// <summary>The f attached id.</summary>
        private ulong fAttachedID = Neuron.EmptyId;

                      // by default, don't search for an attached neuron with the specified id

        /// <summary>
        ///     Gets wether the object was created or already existed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is new; otherwise, <c>false</c> .
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsNew { get; internal set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the text that the object need to encapsulate. Use this
        ///         property or <see cref="JaStDev.HAB.GetObjectArgs.TextNeuron" />
        ///     </para>
        ///     <para>to provide the text to search for.</para>
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the text neuron that the object cluster needs to contain.
        ///         Use this property or <see cref="JaStDev.HAB.GetObjectArgs.Text" />
        ///     </para>
        ///     <para>to provide the text to search for.</para>
        /// </summary>
        /// <value>
        ///     The text neuron.
        /// </value>
        public TextNeuron TextNeuron { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the meaning to search for on the object. The
        ///     value to where the link points to will be compared with
        ///     <see cref="GetObjectArgs.AttachedValue" />
        /// </summary>
        /// <value>
        ///     The meaning ID.
        /// </value>
        public ulong MeaningID { get; set; }

        /// <summary>
        ///     Gets or sets the integer value to which the result object must point
        ///     to using the meaning specified in
        ///     <see cref="JaStDev.HAB.GetObjectArgs.MeaningID" /> . The actual value
        ///     will be stored in an IntNeuron. This should only be used if
        ///     <see cref="JaStDev.HAB.GetObjectArgs.AttachedID" /> =
        ///     <see cref="Neuron.EmptyID" /> (which is the default).
        /// </summary>
        /// <value>
        ///     The attached value.
        /// </value>
        public int AttachedInt { get; set; }

        #region AttachedID

        /// <summary>
        ///     Gets/sets the id of the object that should be attached to the item
        ///     that needs to be returned, the link should have as meaning,
        ///     <see cref="JaStDev.HAB.GetObjectArgs.MeaningID" />
        /// </summary>
        public ulong AttachedID
        {
            get
            {
                return fAttachedID;
            }

            set
            {
                fAttachedID = value;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Provides some general routines that work on the <see cref="Brain" /> .
    /// </summary>
    public class BrainHelper
    {
        /// <summary>Creates a neuron cluster in the form of an object containing a
        ///     textneuron and a 'meaning' neuron.</summary>
        /// <param name="value"><para>The value for the textneuron. If there is already a textneuron
        ///         registered with the <see cref="TextSin"/></para>
        /// <para>for the same value (case insesitive), this neuron is reused.</para>
        /// </param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The neuronCluster that was newly created.</returns>
        public static NeuronCluster CreateObject(string value, out Neuron meaning)
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.Object;
            Brain.Current.Add(iCluster);

            meaning = NeuronFactory.GetNeuron();
            Brain.Current.Add(meaning);
            using (var iList = iCluster.ChildrenW) iList.Add(meaning);

            var iText = GetNeuronForText(value);
            using (var iList = iCluster.ChildrenW) iList.Add(iText);
            return iCluster;
        }

        /// <summary>Creates a neuron cluster in the form of an object containing a
        ///     textneuron.</summary>
        /// <param name="value"><para>The value for the textneuron. If there is already a textneuron
        ///         registered with the <see cref="TextSin"/></para>
        /// <para>for the same value (case insesitive), this neuron is reused.</para>
        /// </param>
        /// <returns>The neuronCluster that was newly created.</returns>
        public static NeuronCluster CreateObject(string value)
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.Object;
            Brain.Current.Add(iCluster);

            var iText = TextSin.Words.GetNeuronFor(value);
            using (var iList = iCluster.ChildrenW) iList.Add(iText);
            return iCluster;
        }

        /// <summary>Creates a neuron cluster in the form of an object containing a
        ///     textneuron.</summary>
        /// <param name="text">The text.</param>
        /// <returns>The neuronCluster that was newly created.</returns>
        public static NeuronCluster CreateObject(Neuron text)
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.Object;
            Brain.Current.Add(iCluster);
            using (var iList = iCluster.ChildrenW) iList.Add(text);
            return iCluster;
        }

        /// <summary>Creates a new plain vanilla, frame without any other info already
        ///     attached.</summary>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateFrame()
        {
            var iRes = NeuronFactory.GetCluster();
            iRes.Meaning = (ulong)PredefinedNeurons.Frame;
            Brain.Current.Add(iRes);
            return iRes;
        }

        /// <summary>Creates a new frame and it's evokers cluster and returns both.</summary>
        /// <param name="sequences">The sequences.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateFrame(out NeuronCluster sequences)
        {
            var iRes = NeuronFactory.GetCluster();
            iRes.Meaning = (ulong)PredefinedNeurons.Frame;
            Brain.Current.Add(iRes);

            sequences = NeuronFactory.GetCluster();
            sequences.Meaning = (ulong)PredefinedNeurons.FrameSequences;
            Brain.Current.Add(sequences);
            var iNew = new Link(sequences, iRes, (ulong)PredefinedNeurons.FrameSequences);

            return iRes;
        }

        /// <summary>Gets the cluster with the specified text value and a link with the
        ///     specified meaning, pointing to the specified value. If none exists,
        ///     one is created.</summary>
        /// <remarks>This function allows you to search for an 'object' cluster that points
        ///     to a specific neuron, like a SynSetID of FrameElementID.</remarks>
        /// <param name="args">The args.</param>
        /// <returns>A neuroncluster that represents the object</returns>
        public static NeuronCluster GetObject(GetObjectArgs args)
        {
            NeuronCluster iRes = null;
            if (args.TextNeuron == null && args.Text != null)
            {
                // check  if the args are ok
                TextNeuron iFound;
                if (TextSin.Words.TryGetNeuron(args.Text, out iFound))
                {
                    args.TextNeuron = iFound;
                }
            }
            else if (args.TextNeuron == null)
            {
                throw new System.ArgumentNullException(
                    "Either TextNeuron or Text must be provided to search for an object.");
            }

            if (args.TextNeuron != null)
            {
                iRes = FindObject(args);
                if (iRes == null)
                {
                    iRes = CreateObject(args);
                }
            }
            else
            {
                iRes = CreateObject(args);
            }

            return iRes;
        }

        /// <summary>The find object.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster FindObject(GetObjectArgs args)
        {
            NeuronCluster iRes;
            if (args.AttachedID == Neuron.EmptyId)
            {
                iRes = FindObject(args.TextNeuron, args.MeaningID, args.AttachedInt);
            }
            else
            {
                iRes = FindObject(args.TextNeuron, args.MeaningID, args.AttachedID);
            }

            return iRes;
        }

        /// <summary>The create object.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private static NeuronCluster CreateObject(GetObjectArgs args)
        {
            NeuronCluster iRes;
            if (args.TextNeuron == null)
            {
                if (args.AttachedID == Neuron.EmptyId)
                {
                    iRes = CreateObject(args.Text, args.MeaningID, args.AttachedInt);
                }
                else
                {
                    iRes = CreateObject(args.Text, args.MeaningID, args.AttachedID);
                }
            }
            else
            {
                if (args.AttachedID == Neuron.EmptyId)
                {
                    iRes = CreateObject(args.TextNeuron, args.MeaningID, args.AttachedInt);
                }
                else
                {
                    iRes = CreateObject(args.TextNeuron, args.MeaningID, args.AttachedID);
                }
            }

            args.IsNew = true;
            return iRes;
        }

        /// <summary>Tries to find the Object Cluster that is a parent of the specified<paramref name="text"/> neuron, and which has an outgoing link to the
        ///     neuron withe the specified id (attachedID), where the link has the
        ///     specified meaning.</summary>
        /// <remarks>This is used, for instance, by the verbnet importer, to search for
        ///     verb-object clusters in the network.</remarks>
        /// <param name="text">The text.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="attachedID">The attached ID.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster FindObject(TextNeuron text, ulong meaningID, ulong attachedID)
        {
            if (text != null && text.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusteredBy;
                using (var iList = text.ClusteredBy) iClusteredBy = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iClusters = from i in iClusteredBy where i.Meaning == (ulong)PredefinedNeurons.Object select i;
                    foreach (var i in iClusters)
                    {
                        var iFound = i.FindFirstOut(meaningID);
                        if (iFound != null && iFound.ID == attachedID)
                        {
                            return i;
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusteredBy);
                }
            }

            return null;
        }

        /// <summary>Tries to find the neuron cluster that represents the object for the
        ///     specified <paramref name="text"/> neuron and synsetid. If it doesn't
        ///     find something, it returns null.</summary>
        /// <param name="text">The text.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="attachedInt">The attached Int.</param>
        /// <returns>A neuroncluster that represents the object, or <see langword="null"/>
        ///     if nothing is found.</returns>
        public static NeuronCluster FindObject(TextNeuron text, ulong meaningID, int attachedInt)
        {
            if (text != null && text.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusteredBy;
                using (var iList = text.ClusteredBy) iClusteredBy = iList.ConvertTo<NeuronCluster>();
                try
                {
                    var iClusters = from i in iClusteredBy where i.Meaning == (ulong)PredefinedNeurons.Object select i;
                    foreach (var i in iClusters)
                    {
                        var iFound = i.FindFirstOut(meaningID) as IntNeuron;
                        if (iFound != null && iFound.Value == attachedInt)
                        {
                            return i;
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusteredBy);
                }
            }

            return null;
        }

        /// <summary>Creates a new object cluster and a new textneuron for the specified
        ///     string that is added to the cluster. The object cluster also gets an
        ///     outgoing link to the specified attachedID, using the specified meaning
        ///     for the link.</summary>
        /// <param name="text">The text.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="attachedID">The attached ID.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateObject(string text, ulong meaningID, ulong attachedID)
        {
            var iText = NeuronFactory.Get<TextNeuron>();
            iText.Text = text;
            Brain.Current.Add(iText);
            TextSin.Words.Add(text, iText.ID);
            return CreateObject(iText, meaningID, attachedID);
        }

        /// <summary>Creates a new cluster with meaning 'object' and adds a new<paramref name="text"/> neuron to it.</summary>
        /// <remarks>Doesn't directly add to textsin dict, but is done indirectly.</remarks>
        /// <param name="text">The text that should be assigned to the text neuron that is created.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="attachedInt">The attached Int.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateObject(string text, ulong meaningID, int attachedInt)
        {
            var iText = NeuronFactory.Get<TextNeuron>();
            iText.Text = text;
            Brain.Current.Add(iText);
            TextSin.Words.Add(text, iText.ID);
            return CreateObject(iText, meaningID, attachedInt);
        }

        /// <summary>Creates a new cluster with the meaning 'object' and adds the specified<paramref name="text"/> neuron to the object cluster. It will also
        ///     create an outgoing link from the cluster, to the<paramref name="attachedID"/> value, using the specified meaning for
        ///     the link.</summary>
        /// <param name="text">The text.</param>
        /// <param name="meaningID">The ID of the neuron, used as meaning for an outgoing link on the
        ///     object cluster.</param>
        /// <param name="attachedID">The ID of the neuron that should be attached to the object cluster.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateObject(TextNeuron text, ulong meaningID, ulong attachedID)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;

            Link.Create(iRes, attachedID, meaningID);
            using (var iList = iRes.ChildrenW) iList.Add(text);
            return iRes;
        }

        /// <summary>Creates a new cluster with meaning 'Object' and adds the textneuron to
        ///     it. It also attached the specified <see langword="int"/> value with a
        ///     link using the specified meaning.</summary>
        /// <remarks>Also makes certain that the textneuron is stored in the dictionary of
        ///     the textsin so that it can be used.</remarks>
        /// <param name="text">The neuron to add to the cluster.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <param name="attachedInt">The attached Int.</param>
        /// <returns>A neuron cluster with a child an a link to an <see langword="int"/>
        ///     neuron, all registered.</returns>
        public static NeuronCluster CreateObject(TextNeuron text, ulong meaningID, int attachedInt)
        {
            var iRes = CreateClusterWithAttachedVal(attachedInt, meaningID);
            iRes.Meaning = (ulong)PredefinedNeurons.Object;
            using (var iList = iRes.ChildrenW) iList.Add(text);
            TextSin.Words.Add(text.Text, text.ID); // always store the data in lowercase.
            return iRes;
        }

        /// <summary>Creates a <see cref="NeuronCluster"/> and assigns it the specified
        ///     value as attached value through a link with the specified meaning. The
        ///     cluster doesn't have a meaning.</summary>
        /// <param name="val">The val.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>A cluster with a link to an intneuron, both registered.</returns>
        public static NeuronCluster CreateClusterWithAttachedVal(int val, ulong meaning)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            var iId = NeuronFactory.GetInt(val);
            Brain.Current.Add(iId);
            var iLink = new Link(iId, iRes, meaning);
            return iRes;
        }

        /// <summary>Deletes the object and all the children of the object.</summary>
        /// <param name="toDelete">To delete.</param>
        public static void DeleteObject(NeuronCluster toDelete)
        {
            System.Collections.Generic.List<Neuron> iChildren;
            var iNextChildren = new System.Collections.Generic.List<Neuron>();
            using (var iList = toDelete.Children) iChildren = iList.ConvertTo<Neuron>();
            try
            {
                Brain.Current.Delete(toDelete);
                while (iChildren.Count > 0)
                {
                    foreach (var i in iChildren)
                    {
                        if (i.CanBeDeleted && HasReferences(i) == false)
                        {
                            var iChild = i as NeuronCluster; // an object can contain another compound.
                            if (iChild != null && iChild.ChildrenIdentifier != null)
                            {
                                System.Collections.Generic.List<Neuron> iSub;
                                using (var iList = iChild.Children) iSub = iList.ConvertTo<Neuron>();
                                iNextChildren.AddRange(iSub);
                                Factories.Default.NLists.Recycle(iSub);
                            }

                            Brain.Current.Delete(i);
                        }
                    }

                    iChildren = iNextChildren;
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iChildren);
            }
        }

        /// <summary>Deletes the neuron as if it were one that respresents some form of
        ///     text: a texneuron, object, posgroup, compound.</summary>
        /// <param name="value">The i prev val.</param>
        public static void DeleteText(Neuron value)
        {
            if (value is NeuronCluster)
            {
                DeleteObject((NeuronCluster)value);
            }
            else
            {
                Brain.Current.Delete(value); // don't need mem leaks
            }
        }

        /// <summary>The create flow.</summary>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreateFlow()
        {
            var iCluster = NeuronFactory.GetCluster();
            iCluster.Meaning = (ulong)PredefinedNeurons.Flow;
            Brain.Current.Add(iCluster);
            return iCluster;
        }

        /// <summary>Creates a frame element referencing the specified<paramref name="role"/> and restrictions.</summary>
        /// <param name="role">The role.</param>
        /// <param name="restriction">The restriction, can be null.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron CreateFrameElement(Neuron role, NeuronCluster restriction)
        {
            var iRes = NeuronFactory.GetNeuron();
            Brain.Current.Add(iRes);
            var iLink = new Link(role, iRes, (ulong)PredefinedNeurons.VerbNetRole);
            if (restriction != null)
            {
                iLink = new Link(restriction, iRes, (ulong)PredefinedNeurons.VerbNetRestrictions);
            }

            return iRes;
        }

        /// <summary>Builds a text string for the specified object cluster. Every text item
        ///     is included in a comma seperated list.</summary>
        /// <param name="cluster">the object to convert to text.</param>
        /// <returns>a string that represents the full text content of the object (all the
        ///     synonyms)</returns>
        public static string BuildLabelForObject(NeuronCluster cluster)
        {
            var iRes = new System.Text.StringBuilder();
            System.Collections.Generic.List<Neuron> ilist;
            using (var iChildren = cluster.Children) ilist = iChildren.ConvertTo<Neuron>();
            foreach (var i in ilist)
            {
                if ((i is TextNeuron)
                    || (i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord))
                {
                    if (iRes.Length > 0)
                    {
                        iRes.Append(", ");
                    }

                    iRes.Append(GetTextFrom(i));
                }
            }

            Factories.Default.NLists.Recycle(ilist);
            return iRes.ToString();
        }

        /// <summary>Gets the (first) text neuron or compound word from an object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetTextFromObject(NeuronCluster obj)
        {
            Neuron iText = null;
            System.Collections.Generic.List<Neuron> ilist;
            using (var iChildren = obj.Children) ilist = iChildren.ConvertTo<Neuron>();
            foreach (var i in ilist)
            {
                // we need to get the textneuron or compound word of the object, so we can search a posgroup from there.
                if (i is TextNeuron)
                {
                    iText = i;
                    break;
                }

                if (i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord)
                {
                    iText = i;
                    break;
                }
            }

            Factories.Default.NLists.Recycle(ilist);
            return iText;
        }

        /// <summary>Builds the text string that represents the value of a compound word
        ///     cluster. Between each word, a space is inserted.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetTextFromCompound(NeuronCluster cluster)
        {
            var iBuild = new System.Text.StringBuilder();
            var iFirst = true;
            System.Collections.Generic.List<Neuron> ilist;
            using (var iChildren = cluster.Children) ilist = iChildren.ConvertTo<Neuron>();
            foreach (var i in ilist)
            {
                var iText = GetTextFrom(i);

                    // we get the text this way, cause it could be an int or double (normally not allowed, but....)
                if (iFirst == false)
                {
                    iBuild.Append(" ");
                }

                iBuild.Append(iText);
                iFirst = false;
            }

            Factories.Default.NLists.Recycle(ilist);
            return iBuild.ToString();
        }

        /// <summary>Gets the text for a pos group.</summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetTextFromPosGroup(NeuronCluster cluster)
        {
            System.Collections.Generic.List<Neuron> iChildren;
            using (var iList = cluster.Children) iChildren = iList.ConvertTo<Neuron>();
            try
            {
                foreach (var i in iChildren)
                {
                    var iText = i as TextNeuron;
                    if (iText != null)
                    {
                        return iText.Text;
                    }

                    if (i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord)
                    {
                        return GetTextFromCompound((NeuronCluster)i);
                    }
                }
            }
            finally
            {
                Factories.Default.NLists.Recycle(iChildren);
            }

            return null;
        }

        /// <summary>Gets the text from the neuron. If it is a textneuron, the text value
        ///     is returned. If it is a posgroup, the textvalue of the textneuron is
        ///     returned. If it is an object, the text value of the first textneuron
        ///     or compound word is returned.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetTextFrom(Neuron neuron)
        {
            var iText = neuron as TextNeuron;
            if (iText != null)
            {
                return iText.Text;
            }

            var iObj = neuron as NeuronCluster;
            if (iObj != null)
            {
                if (iObj.Meaning == (ulong)PredefinedNeurons.Object)
                {
                    System.Collections.Generic.List<Neuron> iList;
                    using (var iChildren = iObj.Children) iList = iChildren.ConvertTo<Neuron>();
                    try
                    {
                        foreach (var i in iList)
                        {
                            // we need to get the textneuron or compound word of the object, so we can search a posgroup from there.
                            if (i is TextNeuron)
                            {
                                return ((TextNeuron)i).Text;
                            }
                            else if (i is NeuronCluster
                                     && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.CompoundWord)
                            {
                                return GetTextFromCompound((NeuronCluster)i);
                            }
                        }
                    }
                    finally
                    {
                        Factories.Default.NLists.Recycle(iList);
                    }
                }
                else if (iObj.Meaning == (ulong)PredefinedNeurons.POSGroup)
                {
                    return GetTextFromPosGroup(iObj);
                }
                else if (iObj.Meaning == (ulong)PredefinedNeurons.CompoundWord)
                {
                    return GetTextFromCompound(iObj);
                }
                else if (iObj.Meaning == (ulong)PredefinedNeurons.Argument
                         || iObj.Meaning == (ulong)PredefinedNeurons.List)
                {
                    System.Collections.Generic.List<Neuron> iChildren;
                    using (var iList = iObj.Children)
                        iChildren = iList.ConvertTo<Neuron>();

                            // if it's a cluster, we try to convert the content of the cluster to strings. This allows us to put a nr of different types of word types together into a single bag (used by the reflectionSin).
                    try
                    {
                        var iBuilder = new System.Text.StringBuilder();
                        foreach (var i in iChildren)
                        {
                            var iFound = GetTextFrom(i);
                            if (string.IsNullOrEmpty(iFound) == false)
                            {
                                iBuilder.Append(iFound);
                            }
                        }

                        return iBuilder.ToString();
                    }
                    finally
                    {
                        Factories.Default.NLists.Recycle(iChildren);
                    }
                }
                else if (iObj.Meaning == (ulong)PredefinedNeurons.Time)
                {
                    var iTime = Time.GetTime(iObj);
                    if (iTime.HasValue)
                    {
                        return iTime.ToString();
                    }
                }
                else if (iObj.Meaning == (ulong)PredefinedNeurons.TimeSpan)
                {
                    var iTime = Time.GetTimeSpan(iObj);
                    if (iTime.HasValue)
                    {
                        return iTime.ToString();
                    }
                }
            }

            return neuron.ToString();
        }

        /// <summary>Determines whether the specified neuron has any outgoing references.</summary>
        /// <param name="toCheck">To check.</param>
        /// <returns><c>true</c> if the specified to check has references; otherwise,<c>false</c> .</returns>
        public static bool HasOutReferences(Neuron toCheck)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.LinksOut, false);
            try
            {
                if (toCheck.LinksOutIdentifier != null)
                {
                    if (toCheck.LinksOutIdentifier.Count > 0)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.LinksOut, false);
            }

            return false;
        }

        /// <summary>Determines whether the specified neuron to check has any incomming,
        ///     outgoing references or parent clusters.</summary>
        /// <param name="toCheck">To check.</param>
        /// <returns><c>true</c> if the specified to check has references; otherwise,<c>false</c> .</returns>
        public static bool HasReferences(Neuron toCheck)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.All, false);
            try
            {
                if (toCheck.LinksInIdentifier != null)
                {
                    if (toCheck.LinksInDirect.Count > 0)
                    {
                        return true;
                    }
                }

                if (toCheck.LinksOutIdentifier != null)
                {
                    if (toCheck.LinksOutIdentifier.Count > 0)
                    {
                        return true;
                    }
                }

                if (toCheck.ClusteredByIdentifier != null)
                {
                    if (toCheck.ClusteredByDirect.Count > 0)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.All, false);
            }

            return false;
        }

        /// <summary>returns <see langword="true"/> if the neurons has incomming links or
        ///     has a parent cluster.</summary>
        /// <param name="toCheck"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool HasIncommingReferences(Neuron toCheck)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.All, false);
            try
            {
                if (toCheck.LinksInIdentifier != null)
                {
                    if (toCheck.LinksInDirect.Count > 0)
                    {
                        return true;
                    }
                }

                if (toCheck.ClusteredByIdentifier != null)
                {
                    if (toCheck.ClusteredByDirect.Count > 0)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.All, false);
            }

            return false;
        }

        /// <summary>returns <see langword="true"/> if hte specified item to check only has
        ///     incomming refernces (clusteredBy or linksIn) from one of the specified
        ///     items (optionally) and no one else.</summary>
        /// <param name="toCheck">neuron to check</param>
        /// <param name="allowed">list of items that are not alowed</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool OnlyIncommingFrom(Neuron toCheck, System.Collections.Generic.IList<ulong> allowed)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.All, false);
            try
            {
                if (toCheck.LinksInIdentifier != null && toCheck.LinksInIdentifier.Count > 0)
                {
                    foreach (var i in toCheck.LinksInIdentifier)
                    {
                        if (allowed.Contains(i.FromID) == false)
                        {
                            return false;
                        }
                    }
                }

                if (toCheck.ClusteredByIdentifier != null)
                {
                    foreach (var i in toCheck.ClusteredByDirect)
                    {
                        if (allowed.Contains(i) == false)
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.All, false);
            }

            return true;
        }

        /// <summary>Determines whether the specified neuron to check has any incomming,
        ///     outgoing references or parent clusters other then the list specified.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="toExclude">The to Exclude.</param>
        /// <returns><c>true</c> if the specified to check has references; otherwise,<c>false</c> .</returns>
        public static bool HasOtherReferences(Neuron toCheck, System.Collections.Generic.List<ulong> toExclude)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.All, false);
            try
            {
                if (toCheck.LinksInIdentifier != null && toCheck.LinksInIdentifier.Count > 0)
                {
                    foreach (var i in toCheck.LinksInIdentifier)
                    {
                        if (toExclude.Contains(i.FromID) == false)
                        {
                            return true;
                        }
                    }
                }

                if (toCheck.LinksOutIdentifier != null && toCheck.LinksOutIdentifier.Count > 0)
                {
                    foreach (var i in toCheck.LinksOutIdentifier)
                    {
                        if (toExclude.Contains(i.ToID) == false)
                        {
                            return true;
                        }
                    }
                }

                if (toCheck.ClusteredByIdentifier != null && toCheck.ClusteredByDirect.Count > 0)
                {
                    foreach (var i in toCheck.ClusteredByDirect)
                    {
                        if (toExclude.Contains(i) == false)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.All, false);
            }

            return false;
        }

        /// <summary>Determines whether the specified neuron to check has any incomming,
        ///     outgoing references or parent clusters other then the list specified.</summary>
        /// <param name="toCheck">To check.</param>
        /// <param name="toExclude">The to Exclude.</param>
        /// <returns><c>true</c> if the specified to check has references; otherwise,<c>false</c> .</returns>
        public static bool HasOtherReferences(Neuron toCheck, System.Collections.Generic.HashSet<ulong> toExclude)
        {
            LockManager.Current.RequestLock(toCheck, LockLevel.All, false);
            try
            {
                if (toCheck.LinksInIdentifier != null && toCheck.LinksInIdentifier.Count > 0)
                {
                    foreach (var i in toCheck.LinksInIdentifier)
                    {
                        if (toExclude.Contains(i.FromID) == false)
                        {
                            return true;
                        }
                    }
                }

                if (toCheck.LinksOutIdentifier != null && toCheck.LinksOutIdentifier.Count > 0)
                {
                    foreach (var i in toCheck.LinksOutIdentifier)
                    {
                        if (toExclude.Contains(i.ToID) == false)
                        {
                            return true;
                        }
                    }
                }

                if (toCheck.ClusteredByIdentifier != null && toCheck.ClusteredByDirect.Count > 0)
                {
                    foreach (var i in toCheck.ClusteredByDirect)
                    {
                        if (toExclude.Contains(i) == false)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                LockManager.Current.ReleaseLock(toCheck, LockLevel.All, false);
            }

            return false;
        }

        /// <summary>Deletes the itemm if it is no longer referenced.</summary>
        /// <param name="toDel">To del.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool DeleteIfEmpty(Neuron toDel)
        {
            if (toDel.CanBeDeleted && HasReferences(toDel) == false)
            {
                Brain.Current.Delete(toDel);
                return true;
            }

            return false;
        }

        /// <summary>recursively deletes the neurons in the hashet untill no more can be
        ///     deleted.</summary>
        /// <param name="retries">The retries.</param>
        public static void DeleteRetries(System.Collections.Generic.HashSet<Neuron> retries)
        {
            var iRetryNext = new System.Collections.Generic.HashSet<Neuron>();

            while (retries != null)
            {
                foreach (var i in retries)
                {
                    if (i.IsDeleted == false)
                    {
                        if (i.CanBeDeleted && HasReferences(i) == false)
                        {
                            Brain.Current.Delete(i);
                        }
                        else
                        {
                            iRetryNext.Add(i);
                        }
                    }
                }

                if (iRetryNext.Count > 0)
                {
                    retries = iRetryNext;
                    iRetryNext = new System.Collections.Generic.HashSet<Neuron>();
                }
                else
                {
                    retries = null;
                }
            }
        }

        /// <summary>retrieves the pos value for the specified object. When no pos is
        ///     declared on this item, the 'is a' <paramref name="relationship"/></summary>
        /// <param name="toSearch">To search.</param>
        /// <param name="relationship">When the object doesn't declare a pos, use this relationship to try
        ///     and find one through the parents.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetPosFor(Neuron toSearch, Neuron relationship)
        {
            if (toSearch != null)
            {
                var iFound = toSearch.FindFirstOut((ulong)PredefinedNeurons.POS);

                    // get the pos value, first on the object itself, if there isn't any, check if it is in a posgroup.
                if (iFound == null)
                {
                    var iPosGroup = toSearch.FindFirstClusteredBy((ulong)PredefinedNeurons.POSGroup);
                    if (iPosGroup != null)
                    {
                        iFound = iPosGroup.FindFirstOut((ulong)PredefinedNeurons.POS);
                    }
                }

                if (iFound == null && relationship != null)
                {
                    var iParents = toSearch.FindAllClusteredBy(relationship.ID);
                    foreach (var i in iParents)
                    {
                        var iChildren = i.FindAllIn(relationship.ID);
                        foreach (var u in iChildren)
                        {
                            iFound = GetPosFor(u as NeuronCluster, relationship);
                            if (iFound != null)
                            {
                                break;
                            }
                        }
                    }
                }

                return iFound;
            }

            return null;
        }

        /// <summary>creates a cluster with the specified meaning.</summary>
        /// <param name="meaning">The meaning.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster MakeCluster(ulong meaning, Neuron value = null)
        {
            var iRes = NeuronFactory.GetCluster();
            Brain.Current.Add(iRes);
            iRes.Meaning = meaning;
            if (value != null)
            {
                using (var iChildren = iRes.ChildrenW) iChildren.Add(value);
            }

            return iRes;
        }

        #region PosGroup

        /// <summary>Gets the POS group for the specified <paramref name="pos"/> on the
        ///     specified neurons.</summary>
        /// <param name="content">The content.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster GetPOSGroup(System.Collections.Generic.IList<Neuron> content, ulong pos)
        {
            var iRes = FindPOSGroup(content[0], pos);
            if (iRes == null)
            {
                iRes = CreatePOSGroup(content, pos);
            }

            return iRes;
        }

        /// <summary>Creates a cluster with meaning<see cref="PredefinedNeurons.POSGoup"/> , pointing to the specified
        ///     textneuron or compound word cluster, for the specifiied part of
        ///     speech.</summary>
        /// <param name="content">The content.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster CreatePOSGroup(System.Collections.Generic.IList<Neuron> content, ulong pos)
        {
            var iRes = NeuronFactory.GetCluster();
            iRes.Meaning = (ulong)PredefinedNeurons.POSGroup;
            Brain.Current.Add(iRes);
            Link.Create(iRes, pos, (ulong)PredefinedNeurons.POS);
            using (var iList = iRes.ChildrenW) iList.AddRange(content);
            return iRes;
        }

        /// <summary>Finds the POS group for specified <paramref name="pos"/> that contains
        ///     the specified textneuron, compound word of object cluster.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster FindPOSGroup(Neuron obj, ulong pos)
        {
            if (obj.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iParents;
                using (var iList = obj.ClusteredBy) iParents = iList.ConvertTo<NeuronCluster>();
                try
                {
                    foreach (var i in iParents)
                    {
                        if (i.Meaning == (ulong)PredefinedNeurons.POSGroup)
                        {
                            var iPos = i.FindFirstOut((ulong)PredefinedNeurons.POS);
                            if (iPos != null && iPos.ID == pos)
                            {
                                return i;
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iParents);
                }
            }

            return null;
        }

        /// <summary>Gets the POS group for the specified <paramref name="pos"/> on the
        ///     specified obj. If non is found, one is created.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="pos">The pos.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public static NeuronCluster GetPOSGroup(Neuron obj, ulong pos)
        {
            var iRes = FindPOSGroup(obj, pos);
            if (iRes == null)
            {
                var iList = new System.Collections.Generic.List<Neuron>();
                iList.Add(obj);
                iRes = CreatePOSGroup(iList, pos);
            }

            return iRes;
        }

        #endregion

        #region CompoundWord

        /// <summary>Creates a <see cref="Neuron"/> cluster that represents a compound word
        ///     for the specified string and returns the corresponding object cluster
        ///     for the compound word.</summary>
        /// <remarks>To ge the elements of the compound word, we use a simple split on ' '.</remarks>
        /// <param name="text">The text containing the compound word.</param>
        /// <returns>A neuroncluster which represents the compound word cluster: it
        ///     contains a textneuron for each word.</returns>
        public static NeuronCluster CreateCompoundWord(string text)
        {
            text = text.ToLower(); // need to make certain we work in lower caps.
            var iWords = text.Split(' '); // extract all the seperate words.
            var iCompound = NeuronFactory.GetCluster();
            iCompound.Meaning = (ulong)PredefinedNeurons.CompoundWord;
            Brain.Current.Add(iCompound);
            foreach (var iWord in iWords)
            {
                var iText = TextSin.Words.GetNeuronFor(iWord);
                using (var iList = iCompound.ChildrenW) iList.Add(iText);
            }

            return iCompound;
        }

        /// <summary>Creates a <see cref="Neuron"/> cluster that represents a compound word
        ///     for the specified string and returns the corresponding object cluster
        ///     for the compound word, if there are multiple words in the string.
        ///     Otherwise, it returns the textneuron that represents the string.</summary>
        /// <remarks>To ge the elements of the compound word, we use a simple split on ' '.</remarks>
        /// <param name="text">The text containing the compound word.</param>
        /// <returns>A neuroncluster which represents the compound word cluster: it
        ///     contains a textneuron for each word. or a single textneuron</returns>
        public static Neuron TryCreateCompoundWord(string text)
        {
            text = text.ToLower(); // need to make certain we work in lower caps.
            var iWords = text.Split(' '); // extract all the seperate words.
            if (iWords.Length > 1)
            {
                return CreateCompountWord(iWords);
            }

            return TextNeuron.GetFor(text);
        }

        /// <summary>converts the text, which must only contains a single word, into an
        ///     int, <see langword="double"/> or textneuron.</summary>
        /// <param name="word"></param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron GetNeuronForSingleWord(string word)
        {
            int iIntVal;
            Neuron iToAdd = null;
            if (int.TryParse(word, out iIntVal))
            {
                iToAdd = NeuronFactory.GetInt(iIntVal);
                Brain.Current.Add(iToAdd);
            }
            else
            {
                double iDoubleVal;
                if (double.TryParse(word, out iDoubleVal))
                {
                    iToAdd = NeuronFactory.GetDouble(iDoubleVal);
                    Brain.Current.Add(iToAdd);
                }
                else
                {
                    iToAdd = TextNeuron.GetFor(word);
                }
            }

            return iToAdd;
        }

        /// <summary>Get or creates (if not yet existing), a <see cref="Neuron"/> cluster
        ///     that represents a compound word for the specified string and returns
        ///     the corresponding object cluster for the compound word, if there are
        ///     multiple words in the string. Otherwise, it returns the textneuron
        ///     that represents the string.</summary>
        /// <remarks>To ge the elements of the compound word, we use</remarks>
        /// <param name="text">The text containing the compound word.</param>
        /// <param name="freezeOn">The freeze On.</param>
        /// <returns>A neuroncluster which represents the compound word cluster: it
        ///     contains a textneuron for each word. or a single textneuron</returns>
        public static Neuron GetNeuronForText(string text, Processor freezeOn = null)
        {
            text = text.ToLower(); // need to make certain we work in lower caps.

            var iWords = Parsers.Tokenizer.Split(text); // extract all the seperate words.
            if (iWords.Length > 1)
            {
                var iTexts = new System.Collections.Generic.List<Neuron>();
                foreach (var i in iWords)
                {
                    Neuron iToAdd = TextNeuron.GetFor(i, freezeOn);
                    iTexts.Add(iToAdd);
                }

                return GetCompoundWord(iTexts, freezeOn); // when we  get here, we couldn't find it, so creat a new one.
            }

            return TextNeuron.GetFor(text, freezeOn);
        }

        /// <summary>gets or creates a neuron for the specified <paramref name="text"/>
        ///     string. if it can be converted to a number, a new intneuron or
        ///     doubleNeuron is returned, otherwise, a textneuron or compound is
        ///     returned, when possible, this is an already existing object.</summary>
        /// <param name="text"></param>
        /// <param name="format">The format.</param>
        /// <param name="freezeOn">The freeze On.</param>
        /// <param name="allowCompounds">The allow Compounds.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetNeuronFor(
            string text, 
            System.IFormatProvider format = null, 
            Processor freezeOn = null, 
            bool allowCompounds = true)
        {
            System.DateTime iDate;
            var iLower = text.ToLower();
            if (iLower == "true")
            {
                return Brain.Current.TrueNeuron;
            }

            if (iLower == "false")
            {
                return Brain.Current.FalseNeuron;
            }

            var iRelationship = PredefinedNeurons.Empty; // this is for the TryGetNumber callback
            var iNeuron = TryGetNumber(text, ref iRelationship, format);
            if (iNeuron == null)
            {
                if (System.DateTime.TryParse(iLower, format, System.Globalization.DateTimeStyles.None, out iDate))
                {
                    // only try datetime after we have tried numbers, cause some doubles can get parsed as a number, which we don't want.
                    return Time.Current.GetTimeCluster(iDate, freezeOn);
                }

                if (allowCompounds)
                {
                    iNeuron = GetNeuronForText(text, freezeOn);
                }
                else
                {
                    iNeuron = TextNeuron.GetFor(text, freezeOn);
                }
            }

            return iNeuron;
        }

        /// <summary>Converts the value to a neuron if possible neuron, bool, int,<see langword="double"/> and text is supported. Text will always be
        ///     converted to a text neuron or compound (text values are not converted
        ///     to bool, int,..)</summary>
        /// <param name="i">The i.</param>
        /// <exception cref="System.InvalidOperationException">When the value can't be converted to a neuron</exception>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetNeuronFor(object i)
        {
            if (i == null)
            {
                return Brain.Current[(ulong)PredefinedNeurons.Empty];
            }

            if (i is bool)
            {
                if ((bool)i)
                {
                    return Brain.Current.TrueNeuron;
                }

                return Brain.Current.FalseNeuron;
            }

            if (i is int)
            {
                Neuron iRes = NeuronFactory.GetInt((int)i);
                Brain.Current.Add(iRes);
                return iRes;
            }

            if (i is double)
            {
                Neuron iRes = NeuronFactory.GetDouble((double)i);
                Brain.Current.Add(iRes);
                return iRes;
            }

            if (i is string)
            {
                return GetNeuronForText((string)i);
            }

            if (i is Neuron)
            {
                return (Neuron)i;
            }

            throw new System.InvalidOperationException(
                string.Format(
                    "Unexpected value in variable list: {0}, type: {1}. Can only handle null, bool, int, double, string or neuron"
                    + i, 
                    i.GetType()));
        }

        /// <summary>Checks if the argument can be parsed as a <see langword="double"/> or
        ///     int, if so, it creates a neuron for the value and returns it,
        ///     otherwise, it returns null.</summary>
        /// <param name="word">The text that needs to be checked.</param>
        /// <param name="relationship">The relationship that should be used by a textneuron for the result
        ///     value. this is a shortcut for textsins.</param>
        /// <param name="format">The format.</param>
        /// <returns>A neuron that contains the <see langword="double"/> value found in
        ///     the string, if it was possible to convert it, otherwise null.</returns>
        public static Neuron TryGetNumber(
            string word, 
            ref PredefinedNeurons relationship, 
            System.IFormatProvider format = null)
        {
            int iIntRes;
            if (int.TryParse(word, System.Globalization.NumberStyles.Any, format, out iIntRes))
            {
                var iNeuron = NeuronFactory.GetInt(iIntRes);
                Brain.Current.MakeTemp(iNeuron);
                relationship = PredefinedNeurons.ContainsInt;
                return iNeuron;
            }

            double iNr;
            if (double.TryParse(word, System.Globalization.NumberStyles.Any, format, out iNr))
            {
                var iNeuron = NeuronFactory.GetDouble(iNr);
                Brain.Current.MakeTemp(iNeuron);
                relationship = PredefinedNeurons.ContainsDouble;
                return iNeuron;
            }

            return null;
        }

        /// <summary>Get (does not create if not yet existing), a <see cref="Neuron"/>
        ///     cluster that represents a compound word for the specified string and
        ///     returns the corresponding object cluster for the compound word, if
        ///     there are multiple words in the string. Otherwise, it returns the
        ///     textneuron that represents the string.</summary>
        /// <remarks>To ge the elements of the compound word, we use a simple split on ' '.</remarks>
        /// <param name="text">The text containing the compound word.</param>
        /// <returns>A neuroncluster which represents the compound word cluster: it
        ///     contains a textneuron for each word. or a single textneuron</returns>
        public static Neuron TryFindCompoundWord(string text)
        {
            text = text.ToLower(); // need to make certain we work in lower caps.
            var iWords = text.Split(' '); // extract all the seperate words.
            if (iWords.Length > 1)
            {
                var iTexts = new System.Collections.Generic.List<Neuron>();
                foreach (var i in iWords)
                {
                    TextNeuron iTextNeuron;
                    if (TextSin.Words.TryGetNeuron(i, out iTextNeuron))
                    {
                        iTexts.Add(iTextNeuron);
                    }
                    else
                    {
                        return null;
                    }
                }

                return GetCompoundWord(iTexts);
            }

            TextNeuron iTextNeuron;
            if (TextSin.Words.TryGetNeuron(text, out iTextNeuron))
            {
                return iTextNeuron;
            }

            return null;
        }

        /// <summary>Gets the compound word for the specified list of strings. If one
        ///     already exists, this is returned, Otherwise a new one is created.
        ///     Always creates compound, even if it is only 1 item.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetCompoundWord(System.Collections.Generic.List<string> toConvert)
        {
            var iTemp = new System.Collections.Generic.List<Neuron>();
            foreach (var i in toConvert)
            {
                iTemp.Add(TextNeuron.GetFor(i));
            }

            return GetCompoundWord(iTemp);
        }

        /// <summary>
        ///     GetCompound needs to be thread save, cause it gets used in queries. So
        ///     to make it a singular affair, this lock is used.
        /// </summary>
        private static readonly object fGetCompoundLock = new object();

        /// <summary>Gets the compound word for the specified text neurons. If one already
        ///     exists, this is returned, Otherwise a new one is created. Always
        ///     creates compound, even if it is only 1 item.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <param name="freezeOn">The freeze On.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetCompoundWord(System.Collections.Generic.List<Neuron> toConvert, 
            Processor freezeOn = null)
        {
            var iMemFac = Factories.Default;
            var iFound = Factories.Default.CLists.GetBuffer();
            try
            {
                lock (fGetCompoundLock)
                {
                    // make certain that no other thread can create the compound while we are looking for it (and can't find it yet).
                    var iFirst = GetWithSmallestNrClusters(toConvert);

                        // get the textneuron that has the smallest amount of clusters. This makes the algorithm faster.
                    if (iFirst != null && iFirst.ClusteredByIdentifier != null)
                    {
                        GetCompoundsFrom(iFirst, iFound);
                        foreach (var i in iFound)
                        {
                            // check if any cluster is ok.
                            LockManager.Current.RequestLock(i, LockLevel.Children, false);
                            try
                            {
                                var iChildren = i.ChildrenIdentifier;
                                if (iChildren != null && iChildren.Count == toConvert.Count)
                                {
                                    var iIsEqual = true;
                                    for (var count = 0; count < toConvert.Count; count++)
                                    {
                                        // count is ok, if any is still dif, try next one.
                                        if (iChildren[count] != toConvert[count].ID)
                                        {
                                            // the order matters cause a compound word represents something.
                                            iIsEqual = false;
                                            break;
                                        }
                                    }

                                    if (iIsEqual)
                                    {
                                        return i; // found it, so return.
                                    }
                                }
                            }
                            finally
                            {
                                LockManager.Current.ReleaseLock(i, LockLevel.Children, false);
                            }
                        }
                    }

                    return CreateCompountWord(toConvert, freezeOn);

                        // when we  get here, we couldn't find it, so creat a new one.
                }
            }
            finally
            {
                iMemFac.CLists.Recycle(iFound);
            }
        }

        /// <summary>adds all the compounds from the specified item to the specified list.</summary>
        /// <param name="iFirst"></param>
        /// <param name="iFound"></param>
        private static void GetCompoundsFrom(Neuron iFirst, System.Collections.Generic.List<NeuronCluster> iFound)
        {
            if (iFirst.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = iFirst.ClusteredBy) iClusters = iFirst.ClusteredBy.ConvertTo<NeuronCluster>();
                try
                {
                    var iItems = from i in iClusters
                                 where i != null && i.Meaning == (ulong)PredefinedNeurons.CompoundWord
                                 select i;
                    foreach (var iC in iItems)
                    {
                        // copy to list, so we can work on a local copy outside of the lock.
                        iFound.Add(iC);
                    }
                }
                finally
                {
                    Factories.Default.CLists.Recycle(iClusters);
                }
            }
        }

        /// <summary>walks through the list of neurons and returns the one with the smalles
        ///     nr of parents.</summary>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private static Neuron GetWithSmallestNrClusters(System.Collections.Generic.List<Neuron> items)
        {
            Neuron iRes = null;
            var iSmallest = int.MaxValue;
            foreach (var i in items)
            {
                if (i.ClusteredByIdentifier != null)
                {
                    // could be that it has no child list yet, cause never had any children
                    LockManager.Current.RequestLock(i, LockLevel.Parents, false);
                    try
                    {
                        var iCount = i.ClusteredByDirect.Count;
                        if (iSmallest > iCount)
                        {
                            iRes = i;
                            iSmallest = iCount;
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(i, LockLevel.Parents, false);
                    }
                }
                else
                {
                    return null; // if 1 has no children, there can't be any common.
                }
            }

            return iRes;
        }

        /// <summary>Creates the compount word for the specified neurons.</summary>
        /// <param name="iTexts">The i texts.</param>
        /// <param name="freezeOn">The freeze On.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron CreateCompountWord(System.Collections.Generic.List<Neuron> iTexts, 
            Processor freezeOn = null)
        {
            var iCompound = NeuronFactory.GetCluster();
            Brain.Current.Add(iCompound);
            iCompound.Meaning = (ulong)PredefinedNeurons.CompoundWord;
            using (var iList = iCompound.ChildrenW) iList.AddRange(iTexts);
            if (freezeOn != null)
            {
                // individual textneurons can't be frozen, cause if the compound gets used, the textneuorns don't get unfrozen anymore.
                iCompound.SetIsFrozen(true, freezeOn);
            }

            return iCompound;
        }

        /// <summary>The create compount word.</summary>
        /// <param name="iWords">The i words.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron CreateCompountWord(string[] iWords)
        {
            var iCompound = NeuronFactory.GetCluster();
            iCompound.Meaning = (ulong)PredefinedNeurons.CompoundWord;
            Brain.Current.Add(iCompound);
            foreach (var iWord in iWords)
            {
                Neuron iText = TextNeuron.GetFor(iWord);
                using (var iList = iCompound.ChildrenW) iList.Add(iText);
            }

            return iCompound;
        }

        #endregion
    }
}