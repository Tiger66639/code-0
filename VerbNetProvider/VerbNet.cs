// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VerbNet.cs" company="">
//   
// </copyright>
// <summary>
//   Provides an access point into the verbnet data for loading and importing.
//   To import, you need to create a local object. This is not required for
//   loading.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VerbNetProvider
{
    using System.Linq;

    /// <summary>
    ///     Provides an access point into the verbnet data for loading and importing.
    ///     To import, you need to create a local object. This is not required for
    ///     loading.
    /// </summary>
    /// <remarks>
    ///     <para>How is the data mapped:</para>
    ///     <list type="number">
    ///         <item>
    ///             <description>class = 1 NND Frame</description>
    ///         </item>
    ///         <item>
    ///             <description>member = 1 frme evoker (as object cluster)</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 role = 1 frame element, as neuron, pointing to Role-object cluster,
    ///                 Restrictions cluster, with possibly a logical attached to it.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 frame = 1 frame sequence. The elements in a frame sequence are all
    ///                 neurons that point to one of the follwing items:
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>a frame element</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 an object cluster that can be used anywhere in the sequence
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 a text block Each element can possibly also have selectional
    ///                 restrictions.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class VerbNet
    {
        /// <summary>The f frame.</summary>
        private JaStDev.HAB.NeuronCluster fFrame; // the frame that we are currnetly importing to.

        /// <summary>The f role descriptions.</summary>
        private System.Collections.Generic.Dictionary<themRoleType, string> fRoleDescriptions;

                                                                            // the default description info for all the standard roles that are used in verbnet.  Thi is statically filled.

        /// <summary>The f role items.</summary>
        private readonly System.Collections.Generic.Dictionary<string, JaStDev.HAB.Neuron> fRoleItems =
            new System.Collections.Generic.Dictionary<string, JaStDev.HAB.Neuron>();

                                                                                           // stores the class items that ref the roles, indexed with the name of the role, so that the sequence builder can easely find them.

        /// <summary>
        ///     Gets or sets the object that will label the created items (so the
        ///     designer can assign a description to an object).
        /// </summary>
        /// <value>
        ///     The labeler.
        /// </value>
        public IVNLabeler Labeler { get; set; }

        #region Role descriptions

        /// <summary>Gets the role descriptions.</summary>
        public System.Collections.Generic.Dictionary<themRoleType, string> RoleDescriptions
        {
            get
            {
                if (fRoleDescriptions == null)
                {
                    fRoleDescriptions = new System.Collections.Generic.Dictionary<themRoleType, string>();
                    fRoleDescriptions.Add(
                        themRoleType.Actor, 
                        "used for some communication classes (e.g., Chitchat-37.6, Marry-36.2, Meet-36.2) when both arguments can be considered symmetrical (pseudo-agents).");
                    fRoleDescriptions.Add(
                        themRoleType.Actor1, 
                        "used for some communication classes (e.g., Chitchat-37.6, Marry-36.2, Meet-36.2) when both arguments can be considered symmetrical (pseudo-agents).");
                    fRoleDescriptions.Add(
                        themRoleType.Actor2, 
                        "used for some communication classes (e.g., Chitchat-37.6, Marry-36.2, Meet-36.2) when both arguments can be considered symmetrical (pseudo-agents).");
                    fRoleDescriptions.Add(
                        themRoleType.Agent, 
                        "generally a human or an animate subject. Used mostly as a volitional agent, but also used in VerbNet for internally controlled subjects such as forces and machines.");
                    fRoleDescriptions.Add(
                        themRoleType.Asset, 
                        "used for the Sum of Money Alternation, present in classes such as Build-26.1, Get-13.5.1, and Obtain-13.5.2 with `currency' as a selectional restriction.");
                    fRoleDescriptions.Add(
                        themRoleType.Attribute, 
                        "attribute of Patient/Theme refers to a quality of something that is being changed, as in (The price)att of oil soared. At the moment, we have only one class using this role Calibratable cos-45.6 to capture the Possessor Subject Possessor-Attribute Factoring Alternation. The selectional restriction `scalar' (defined as a quantity, such as mass, length, time, or temperature, which is completely specified by a number on an appropriate scale) ensures the nature of Attribute. ");
                    fRoleDescriptions.Add(
                        themRoleType.Beneficiary, 
                        "the entity that benefits from some action. Used by such classes asBuild-26.1, Get-13.5.1, Performance-26.7, Preparing-26.3, and Steal-10.5. Generally introduced by the preposition `for', or double object variant in the benefactive alternation. ");
                    fRoleDescriptions.Add(
                        themRoleType.Cause, 
                        "used mostly by classes involving Psychological Verbs and Verbs Involving the Body. ");
                    fRoleDescriptions.Add(
                        themRoleType.Destination, 
                        "used for spatial locations. end point of the motion, or direction towards which the motion is directed. Used with a `to' prepositional phrase by classes of change of location, such as Banish-10.2, and Verbs of Sending and Carrying. Also used as location direct objects in classes where the concept of destination is implicit (and location could not be Source), such as Butter-9.9, or Image impression-25.1. ");
                    fRoleDescriptions.Add(
                        themRoleType.Source, 
                        "used for spatial locations. start point of the motion. Usually introduced by a source prepositional phrase (mostly headed by `from' or `out of'). It is also used as a direct object in such classes as Clear-10.3, Leave-51.2, and Wipe instr-10.4.2.");
                    fRoleDescriptions.Add(
                        themRoleType.Location, 
                        "used for spatial locations. underspecified destination, source, or place, in general introduced by a locative or path prepositional phrase. ");
                    fRoleDescriptions.Add(
                        themRoleType.Experiencer, 
                        "used for a participant that is aware or experiencing something. In VerbNet it is used by classes involving Psychological Verbs, Verbs of Perception, Touch, and Verbs Involving the Body. ");
                    fRoleDescriptions.Add(
                        themRoleType.Extent, 
                        "used only in the Calibratable-45.6 class, to specify the range or degree of change, as in The price of oil soared (10%)ext. This role may be added to other classes. ");
                    fRoleDescriptions.Add(
                        themRoleType.Instrument, 
                        "used for objects (or forces) that come in contact with an object and cause some change in them. Generally introduced by a `with' prepositional phrase. Also used as a subject in the Instrument Subject Alternation and as a direct object in the Poke-19 class for the Through/With Alternation and in the Hit-18.1 class for the With/Against Alternation. ");
                    fRoleDescriptions.Add(
                        themRoleType.Material, 
                        "used in the Build and Grow classes to capture the key semantic components of the arguments. Used by classes from Verbs of Creation and Transformation that allow for the Material/Product Alternation. start point of transformation. ");
                    fRoleDescriptions.Add(
                        themRoleType.Product, 
                        "used in the Build and Grow classes to capture the key semantic components of the arguments. Used by classes from Verbs of Creation and Transformation that allow for the Material/Product Alternation. end result of transformation.");
                    fRoleDescriptions.Add(
                        themRoleType.Patient, 
                        "used for participants that are undergoing a process or that have been affected in some way. Verbs that explicitly (or implicitly) express changes of state have Patient as their usual direct object. We also use Patient1 and Patient2 for some classes of Verbs of Combining and Attaching and Verbs of Separating and Disassembling, where there are two roles that undergo some change with no clear distinction between them. ");
                    fRoleDescriptions.Add(
                        themRoleType.Patient1, 
                        "used for participants that are undergoing a process or that have been affected in some way. Verbs that explicitly (or implicitly) express changes of state have Patient as their usual direct object. We also use Patient1 and Patient2 for some classes of Verbs of Combining and Attaching and Verbs of Separating and Disassembling, where there are two roles that undergo some change with no clear distinction between them. ");
                    fRoleDescriptions.Add(
                        themRoleType.Patient2, 
                        "used for participants that are undergoing a process or that have been affected in some way. Verbs that explicitly (or implicitly) express changes of state have Patient as their usual direct object. We also use Patient1 and Patient2 for some classes of Verbs of Combining and Attaching and Verbs of Separating and Disassembling, where there are two roles that undergo some change with no clear distinction between them. ");
                    fRoleDescriptions.Add(themRoleType.Predicate, "used for classes with a predicative complement. ");
                    fRoleDescriptions.Add(
                        themRoleType.Recipient, 
                        "target of the transfer. Used by some classes of Verbs of Change of Possession, Verbs of Communication, and Verbs Involving the Body. The selection restrictions on this role always allow for animate and sometimes for organization recipients. ");
                    fRoleDescriptions.Add(
                        themRoleType.Stimulus, 
                        "used by Verbs of Perception for events or objects that elicit some response from an xperiencer. This role usually imposes no restrictions. ");
                    fRoleDescriptions.Add(
                        themRoleType.Theme, 
                        "used for participants in a location or undergoing a change of location. Also, Theme1 and Theme2 are used for a few classes where there seems to be no distinction between the arguments, such as Differ-23.4 and Exchange-13.6 classes. ");
                    fRoleDescriptions.Add(
                        themRoleType.Theme1, 
                        "used for participants in a location or undergoing a change of location. Also, Theme1 and Theme2 are used for a few classes where there seems to be no distinction between the arguments, such as Differ-23.4 and Exchange-13.6 classes. ");
                    fRoleDescriptions.Add(
                        themRoleType.Theme2, 
                        "used for participants in a location or undergoing a change of location. Also, Theme1 and Theme2 are used for a few classes where there seems to be no distinction between the arguments, such as Differ-23.4 and Exchange-13.6 classes. ");
                    fRoleDescriptions.Add(
                        themRoleType.Time, 
                        "class-specific role, used in Begin-55.1 class to express time. ");
                    fRoleDescriptions.Add(
                        themRoleType.Topic, 
                        "topic of communication verbs to handle theme/topic of the conversation or transfer of message. In some cases, like the verbs in the Say-37.7 class, it would seem better to have `Message' instead of `Topic', but we decided not to proliferate the number of roles. ");
                }
                else if (JaStDev.HAB.Brain.Current == null)
                {
                    JaStDev.LogService.Log.LogError(
                        "VerbNet.Roles", 
                        "Can't load VerNetRoles dictionray, Brain not yet loaded!");
                }

                return fRoleDescriptions;
            }
        }

        #endregion

        /// <summary>Loads all the verbnet xml files found in the specified<paramref name="path"/> and returns them as a list.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        public static System.Collections.Generic.IList<VNCLASS> Load(string path)
        {
            var iRes = new System.Collections.Generic.List<VNCLASS>();

            if (path[path.Length - 1] != System.IO.Path.DirectorySeparatorChar
                && path[path.Length - 1] != System.IO.Path.VolumeSeparatorChar)
            {
                // need to make certain that the path ends correctly.
                path = path + System.IO.Path.DirectorySeparatorChar;
            }

            if (System.IO.Directory.Exists(path))
            {
                var valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(VNCLASS));
                var iSettings = new System.Xml.XmlReaderSettings { ProhibitDtd = false };

                    // VerbNet classes contain a dtd.  We need to change default reader settings of .net xmlreader to allow for this.
                foreach (var i in System.IO.Directory.GetFiles(path, "*.xml"))
                {
                    VNCLASS iNew;
                    using (var iReader = System.Xml.XmlReader.Create(i, iSettings)) iNew = (VNCLASS)valueSerializer.Deserialize(iReader);
                    iRes.Add(iNew);
                }
            }
            else
            {
                throw new System.InvalidOperationException(string.Format("Invalid path: {0}", path));
            }

            return iRes;
        }

        /// <summary>Imports all the the <see cref="VerbNet"/> classes that have the<see cref="VerbNetProvider.VNCLASS.NeedsImport"/> selected.</summary>
        /// <remarks>This is a class local function, and not static, cause we need to
        ///     access some project local info (the different roles in the
        ///     project,..). By making it an object local class, you need to create a
        ///     verbnet class for importing the data.</remarks>
        /// <param name="iList">The i list.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public System.Collections.Generic.List<JaStDev.HAB.NeuronCluster> ImportSelected(System.Collections.Generic.IList<VNCLASS> iList)
        {
            var iRes = new System.Collections.Generic.List<JaStDev.HAB.NeuronCluster>();
            var iSelected = from i in iList where i.NeedsImport select i;
            foreach (var i in iSelected)
            {
                fRoleItems.Clear();

                    // we store a map from role to frame element for reach frame that we import, otherwise, we mix up values.
                var iToAdd = ImportClass(i);
                iRes.Add(iToAdd);
            }

            JaStDev.LogService.Log.LogWarning(
                "VerbNet.ImportSelected", 
                "Semantic info for verbnet frames is not yet imported!");
            return iRes;
        }

        /// <summary>The import class.</summary>
        /// <param name="vn">The vn.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private JaStDev.HAB.NeuronCluster ImportClass(VNCLASS vn)
        {
            System.Diagnostics.Debug.Assert(Labeler != null);
            JaStDev.HAB.NeuronCluster iSequences;
            fFrame = JaStDev.HAB.BrainHelper.CreateFrame(out iSequences);
            Labeler.SetTitle(fFrame, vn.ID);
            ImportMembers(vn);
            ImportRoles(vn);
            ImportFrames(vn, iSequences);
            return fFrame;
        }

        /// <summary>Imports all the frames of a verbnet class into the specified cluster.
        ///     These become the sequences of a NND-frame.</summary>
        /// <param name="vn">The vn.</param>
        /// <param name="to">To.</param>
        private void ImportFrames(VNCLASS vn, JaStDev.HAB.NeuronCluster to)
        {
            foreach (var i in vn.FRAMES)
            {
                var iSeq = JaStDev.HAB.NeuronFactory.GetCluster();
                iSeq.Meaning = (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequence;
                JaStDev.HAB.Brain.Current.Add(iSeq);
                SetFrameInfo(iSeq, i);
                foreach (var iSyntax in i.SYNTAX.Items)
                {
                    JaStDev.HAB.Neuron iToAdd = null;
                    if (iSyntax is NP)
                    {
                        iToAdd = CreateNPSequenceItem((NP)iSyntax);
                    }
                    else if (iSyntax is VERB)
                    {
                        iToAdd = CreateSequenceItem(themRoleType.Verb);
                    }
                    else if (iSyntax is ADJ)
                    {
                        iToAdd = CreateSequenceItem(JaStDev.HAB.PredefinedNeurons.Adjective);
                    }
                    else if (iSyntax is ADV)
                    {
                        iToAdd = CreateSequenceItem(JaStDev.HAB.PredefinedNeurons.Adverb);
                    }
                    else if (iSyntax is LEX)
                    {
                        iToAdd = CreateLEXSequenceItem((LEX)iSyntax);
                    }
                    else if (iSyntax is PREP)
                    {
                        iToAdd = CreatePREPSequenceItem((PREP)iSyntax, vn.ID);
                    }

                    if (iToAdd != null)
                    {
                        using (var iChildren = iSeq.ChildrenW) iChildren.Add(iToAdd);
                    }
                }

                using (var iChildren = to.ChildrenW) iChildren.Add(iSeq);
            }
        }

        /// <summary>Creates a new, raw sequence item, pointing to the specified predefined
        ///     neuron, using FrameSequenceItemValue as meaning.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron CreateSequenceItem(JaStDev.HAB.PredefinedNeurons value)
        {
            var iRes = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRes);

            JaStDev.HAB.Neuron iFrameEl;
            if (fRoleItems.TryGetValue(value.ToString(), out iFrameEl) == false)
            {
                iFrameEl = JaStDev.HAB.BrainHelper.CreateFrameElement(JaStDev.HAB.Brain.Current[(ulong)value], null);
                Labeler.SetTitle(iFrameEl, value.ToString());
                using (var iChildren = fFrame.ChildrenW) iChildren.Add(iFrameEl);
                fRoleItems[value.ToString()] = iFrameEl;
            }

            JaStDev.HAB.Link.Create(iRes, iFrameEl, (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequenceItemValue);
            return iRes;
        }

        /// <summary>The create sequence item.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron CreateSequenceItem(themRoleType value)
        {
            var iRes = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRes);
            JaStDev.HAB.Neuron iTo;
            if (fRoleItems.TryGetValue(value.ToString(), out iTo))
            {
                JaStDev.HAB.Link.Create(iRes, iTo, (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequenceItemValue);
            }
            else
            {
                JaStDev.LogService.Log.LogError(
                    "VerbNet.CreateSequenceItem", 
                    "Inconsistency in sequence definition: the sequence contains a reference to a non defined frame element (role).");
            }

            return iRes;
        }

        /// <summary>Creates a frame sequence <paramref name="item"/> that represents the
        ///     LEX(ical unit) item, which is a <see langword="ref"/> to a textneuron.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron CreateLEXSequenceItem(LEX item)
        {
            var iRes = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRes);

            JaStDev.HAB.Neuron iFrameEl;
            if (fRoleItems.TryGetValue(item.value, out iFrameEl) == false)
            {
                iFrameEl = BuildFrameElementForRole(item.value);
            }

            JaStDev.HAB.Link.Create(iRes, iFrameEl, (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequenceItemValue);
            return iRes;
        }

        /// <summary>The create prep sequence item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="className">The class name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron CreatePREPSequenceItem(PREP item, string className)
        {
            var iRes = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRes);
            JaStDev.HAB.Neuron iFrameEl;
            if (string.IsNullOrEmpty(item.value) == false)
            {
                if (fRoleItems.TryGetValue(item.value, out iFrameEl) == false)
                {
                    var iArgs = new JaStDev.HAB.GetObjectArgs();
                    iArgs.Text = item.value;
                    iArgs.AttachedID = (ulong)JaStDev.HAB.PredefinedNeurons.Preposition;
                    iArgs.MeaningID = (ulong)JaStDev.HAB.PredefinedNeurons.POS;
                    iFrameEl = JaStDev.HAB.BrainHelper.CreateFrameElement(
                        JaStDev.HAB.BrainHelper.GetObject(iArgs), 
                        null);
                    Labeler.SetTitle(iFrameEl, item.value);
                    using (var iChildren = fFrame.ChildrenW) iChildren.Add(iFrameEl);
                    fRoleItems[item.value] = iFrameEl;
                }
            }
            else
            {
                if (fRoleItems.TryGetValue(JaStDev.HAB.PredefinedNeurons.Preposition.ToString(), out iFrameEl) == false)
                {
                    iFrameEl =
                        JaStDev.HAB.BrainHelper.CreateFrameElement(
                            JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.Preposition], 
                            null);
                    Labeler.SetTitle(iFrameEl, JaStDev.HAB.PredefinedNeurons.Preposition.ToString());
                    using (var iChildren = fFrame.ChildrenW) iChildren.Add(iFrameEl);
                    fRoleItems[JaStDev.HAB.PredefinedNeurons.Preposition.ToString()] = iFrameEl;
                }
            }

            JaStDev.HAB.Link.Create(iRes, iFrameEl, (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequenceItemValue);
            return iRes;
        }

        /// <summary>The create np sequence item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron CreateNPSequenceItem(NP item)
        {
            var iRes = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iRes);

            JaStDev.HAB.Neuron iFrameEl;
            if (fRoleItems.TryGetValue(item.value, out iFrameEl) == false)
            {
                iFrameEl = BuildFrameElementForRole(item.value);
            }

            var iLink = new JaStDev.HAB.Link(
                iFrameEl, 
                iRes, 
                (ulong)JaStDev.HAB.PredefinedNeurons.FrameSequenceItemValue);
            return iRes;
        }

        /// <summary>The set frame info.</summary>
        /// <param name="seq">The seq.</param>
        /// <param name="frame">The frame.</param>
        private void SetFrameInfo(JaStDev.HAB.NeuronCluster seq, FRAMESFRAME frame)
        {
            var iDesc = new System.Text.StringBuilder(frame.DESCRIPTION.secondary);
            iDesc.AppendLine("Examples:");
            if (frame.EXAMPLES.Length > 0)
            {
                // the example array contains another array of xml nodes.
                foreach (var iEx in (System.Xml.XmlNode[])frame.EXAMPLES[0])
                {
                    iDesc.AppendLine(iEx.Value);
                }
            }

            Labeler.SetInfo(seq, frame.DESCRIPTION.primary, iDesc.ToString());
        }

        /// <summary>Imports all the roles of a verbnet class into the specified cluster.
        ///     These become the frame elements.</summary>
        /// <param name="vn">The vn.</param>
        private void ImportRoles(VNCLASS vn)
        {
            foreach (var i in vn.THEMROLES)
            {
                JaStDev.HAB.Neuron iToAdd;
                var iRole = GetRole(i.type);
                if (i.SELRESTRS.Items != null)
                {
                    var iRestrictions = GetSelectionRestrictions(i.SELRESTRS);
                    iToAdd = JaStDev.HAB.BrainHelper.CreateFrameElement(iRole, iRestrictions);
                }
                else
                {
                    iToAdd = JaStDev.HAB.BrainHelper.CreateFrameElement(iRole, null);
                }

                Labeler.SetRefToTitle(iToAdd, iRole);
                using (var iChildren = fFrame.ChildrenW) iChildren.Add(iToAdd);
                fRoleItems[i.type.ToString()] = iToAdd;
            }
        }

        /// <summary>builds the list of restrictions for a frame element. This is stored in
        ///     a cluster containing Restriction items, the cluster gets a logical<see langword="operator"/> assigned to indicate how to interprete the
        ///     restriction items.</summary>
        /// <param name="items">The items.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        private JaStDev.HAB.NeuronCluster GetSelectionRestrictions(SELRESTRS items)
        {
            var iRestrictions = JaStDev.HAB.NeuronFactory.GetCluster();
            JaStDev.HAB.Brain.Current.Add(iRestrictions);
            iRestrictions.Meaning = (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestrictions;
            foreach (var i in items.Items)
            {
                var selRest = i as SELRESTRSSELRESTR;
                JaStDev.HAB.Neuron iTemp;
                if (selRest != null)
                {
                    iTemp = GetselectionRestriction(selRest);
                }
                else
                {
                    iTemp = GetSelectionRestrictions((SELRESTRS)i);
                }

                using (var iChildren = iRestrictions.ChildrenW) iChildren.Add(iTemp);
            }

            JaStDev.HAB.Neuron iLogical = null;
            if (items.logic != null)
            {
                if (items.logic.ToLower() == "or")
                {
                    iLogical = JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.Or];
                }
                else if (items.logic.ToLower() == "and")
                {
                    iLogical = JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.And];
                }
            }
            else
            {
                iLogical = JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.And];

                    // we presume the 'And' operator when no other is selected.
            }

            if (iRestrictions != null && iLogical != null)
            {
                JaStDev.HAB.Link.Create(iRestrictions, iLogical, (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetLogicValue);
            }

            return iRestrictions;
        }

        /// <summary>The getselection restriction.</summary>
        /// <param name="selRest">The sel rest.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron GetselectionRestriction(SELRESTRSSELRESTR selRest)
        {
            var iTemp = JaStDev.HAB.NeuronFactory.GetNeuron();
            JaStDev.HAB.Brain.Current.Add(iTemp);
            JaStDev.HAB.Link.Create(
                iTemp, 
                GetRestriction(selRest.type), 
                (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestriction);
            if (string.IsNullOrEmpty(selRest.Value) == false)
            {
                if (selRest.Value == "+")
                {
                    JaStDev.HAB.Link.Create(
                        iTemp, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.RestrictionModifierInclude, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestrictionModifier);
                }
                else if (selRest.Value == "-")
                {
                    JaStDev.HAB.Link.Create(
                        iTemp, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.RestrictionModifierExclude, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestrictionModifier);
                }
            }

            return iTemp;
        }

        /// <summary>Gets the ID of the neuron that represents the verbnet role in the
        ///     brain.</summary>
        /// <remarks>If there is no neuron yet defined for this type of role, one is
        ///     created and wrapped in an object that also contains a textNeuron for
        ///     the key.</remarks>
        /// <param name="key">The textvalue of the role. Must be a valid key.</param>
        /// <returns>The <paramref name="key"/> of the neuron that represents the role.</returns>
        public JaStDev.HAB.NeuronCluster GetRole(themRoleType key)
        {
            var iKey = key.ToString().ToLower();
            var iText = JaStDev.HAB.TextNeuron.GetFor(iKey);
            var iObj = JaStDev.HAB.BrainHelper.FindPOSGroup(iText, (ulong)JaStDev.HAB.PredefinedNeurons.Noun);
            if (iObj == null)
            {
                iObj =
                    JaStDev.HAB.BrainHelper.CreatePOSGroup(
                        new System.Collections.Generic.List<JaStDev.HAB.Neuron> { iText }, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.Noun);

                    // we use a posgroup, since we don't know the exact meaning of the word that we want, we do know that we have a noun, since it is a filter from a thesaurus group that is used.
                string iDesc;
                if (RoleDescriptions.TryGetValue(key, out iDesc))
                {
                    Labeler.SetInfo(iObj, key.ToString(), iDesc);
                }
                else
                {
                    Labeler.SetTitle(iObj, iKey);
                }

                Labeler.StoreRoleRoot(iObj.ID);
            }

            return iObj;
        }

        /// <summary>Creates a new frame element for the specified text and adds it to the
        ///     frame element list.</summary>
        /// <param name="role">The role.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private JaStDev.HAB.Neuron BuildFrameElementForRole(string role)
        {
            var iText = JaStDev.HAB.TextNeuron.GetFor(role);
            var iObj = JaStDev.HAB.BrainHelper.FindPOSGroup(iText, (ulong)JaStDev.HAB.PredefinedNeurons.Noun);
            if (iObj == null)
            {
                iObj =
                    JaStDev.HAB.BrainHelper.CreatePOSGroup(
                        new System.Collections.Generic.List<JaStDev.HAB.Neuron> { iText }, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.Noun);

                    // we use a posgroup, since we don't know the exact meaning of the word that we want, we do know that we have a noun, since it is a filter from a thesaurus group that is used.
                Labeler.SetTitle(iObj, role);
                Labeler.StoreRoleRoot(iObj.ID);
            }

            var iToAdd = JaStDev.HAB.BrainHelper.CreateFrameElement(iObj, null);
            Labeler.SetRefToTitle(iToAdd, iObj);
            using (var iChildren = fFrame.ChildrenW) iChildren.Add(iToAdd);
            fRoleItems[role] = iToAdd;
            return iToAdd;
        }

        /// <summary>Gets the ID of the neuron that represents the verbnet restriction in
        ///     the brain.</summary>
        /// <remarks>If there is no neuron yet defined for this type of restriction, one is
        ///     created and wrapped in an object that also contains a textNeuron for
        ///     the key.</remarks>
        /// <param name="key">The textvalue of the restriction.</param>
        /// <returns>The <paramref name="key"/> of the neuron that represents the
        ///     restriction. Can be <see langword="null"/> in case the input string
        ///     was null.</returns>
        public JaStDev.HAB.NeuronCluster GetRestriction(selrestrType key)
        {
            var iKey = key.ToString().ToLower();
            var iText = JaStDev.HAB.TextNeuron.GetFor(iKey);
            var iObj = JaStDev.HAB.BrainHelper.FindPOSGroup(iText, (ulong)JaStDev.HAB.PredefinedNeurons.Noun);
            if (iObj == null)
            {
                iObj =
                    JaStDev.HAB.BrainHelper.CreatePOSGroup(
                        new System.Collections.Generic.List<JaStDev.HAB.Neuron> { iText }, 
                        (ulong)JaStDev.HAB.PredefinedNeurons.Noun);

                    // we use a posgroup, since we don't know the exact meaning of the word that we want, we do know that we have a noun, since it is a filter from a thesaurus group that is used.
                Labeler.SetTitle(iObj, iKey);
                return iObj;
            }

            return iObj;
        }

        /// <summary>Imports all the members of a verbnet class. These become the filters
        ///     for the verb role.</summary>
        /// <remarks>The members are imported as a single role: the verbRole, with an 'Or'
        ///     filter to select out of one of the members.</remarks>
        /// <param name="vn">The vn.</param>
        private void ImportMembers(VNCLASS vn)
        {
            JaStDev.HAB.Neuron iToAdd;
            var iRole = GetRole(themRoleType.Verb);
            var iRestrictions = JaStDev.HAB.NeuronFactory.GetCluster();
            JaStDev.HAB.Brain.Current.Add(iRestrictions);
            iRestrictions.Meaning = (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestrictions;

            foreach (var i in vn.MEMBERS)
            {
                var iTemp = JaStDev.HAB.NeuronFactory.GetNeuron();
                JaStDev.HAB.Brain.Current.Add(iTemp);

                var iText = JaStDev.HAB.TextNeuron.GetFor(i.name);
                var iRestriction = JaStDev.HAB.BrainHelper.FindPOSGroup(
                    iText, 
                    (ulong)JaStDev.HAB.PredefinedNeurons.Verb);
                if (iRestriction == null)
                {
                    iRestriction =
                        JaStDev.HAB.BrainHelper.CreatePOSGroup(
                            new System.Collections.Generic.List<JaStDev.HAB.Neuron> { iText }, 
                            (ulong)JaStDev.HAB.PredefinedNeurons.Verb);

                        // we use a posgroup, since we don't know the exact meaning of the word that we want, we do know that we have a noun, since it is a filter from a thesaurus group that is used.
                    Labeler.SetTitle(iRestriction, i.name);
                }

                JaStDev.HAB.Link.Create(iTemp, iRestriction, (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestriction);
                JaStDev.HAB.Link.Create(
                    iTemp, 
                    (ulong)JaStDev.HAB.PredefinedNeurons.RestrictionModifierInclude, 
                    (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetRestrictionModifier);
                using (var iChildren = iRestrictions.ChildrenW) iChildren.Add(iTemp);
            }

            var iLogical = JaStDev.HAB.Brain.Current[(ulong)JaStDev.HAB.PredefinedNeurons.Or];
            if (iRestrictions != null && iLogical != null)
            {
                JaStDev.HAB.Link.Create(iRestrictions, iLogical, (ulong)JaStDev.HAB.PredefinedNeurons.VerbNetLogicValue);
            }

            iToAdd = JaStDev.HAB.BrainHelper.CreateFrameElement(iRole, iRestrictions);
            JaStDev.HAB.Link.Create(iToAdd, fFrame, (ulong)JaStDev.HAB.PredefinedNeurons.IsFrameEvoker);

                // we need to indicate that the verb role, is the evoker.
            Labeler.SetRefToTitle(iToAdd, iRole);
            using (var iChildren = fFrame.ChildrenW) iChildren.Add(iToAdd);
            fRoleItems[themRoleType.Verb.ToString()] = iToAdd;
        }
    }
}