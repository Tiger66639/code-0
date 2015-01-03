// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectTextPatternEditor.cs" company="">
//   
// </copyright>
// <summary>
//   A textPattern editor that shows patterns which are attached to another
//   neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A textPattern editor that shows patterns which are attached to another
    ///     neuron.
    /// </summary>
    public class ObjectTextPatternEditor : TextPatternEditor
    {
        /// <summary>Initializes a new instance of the <see cref="ObjectTextPatternEditor"/> class. Initializes a new instance of the<see cref="ObjectTextPatternEditor"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ObjectTextPatternEditor(Neuron toWrap)
            : base(toWrap)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectTextPatternEditor"/> class. 
        ///     Initializes a new instance of the<see cref="ObjectTextPatternEditor"/> class. Required for xml
        ///     serialization.</summary>
        public ObjectTextPatternEditor()
        {
        }

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        public override string DocumentTitle
        {
            get
            {
                return Properties.Resources.PatternsForText + Name;
            }
        }

        /// <summary>
        ///     Registers the item that was read from xml. Topics must be registed
        ///     with the parser (when not in viewer mode).
        /// </summary>
        public override void Register()
        {
            RegisterNoTopicName();
        }

        /// <summary>Changes the name.</summary>
        /// <param name="value">The value.</param>
        protected override void ChangeName(string value)
        {
            var iPrev = Name;
            ChangeNameNoRegister(value);
            if (iPrev != null)
            {
                // the first time that the name gets assigned, the editor just got created and we don't need to change any TopicsDict (if new dict, the name will get registered upon actual creation, otherwise the name is already registered).
                var iTopic = Item.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic);
                if (iTopic != null)
                {
                    var iInfo = BrainData.Current.NeuronInfo[iTopic]; // need to keep the topicsDict in sync.
                    Parsers.TopicsDictionary.Remove(iInfo.DisplayTitle, iTopic);
                    iInfo.DisplayTitle = Parsers.TopicsDictionary.GetUnique(value + "_Topic");

                        // we store the topic-name also in the display title of the real topic id, so it is properly labeled as wel. (can work with a regulr TextPAtternEditor), is also important for exporting to xml: all textpatterns need a topicId.
                    Parsers.TopicsDictionary.Add(iInfo.DisplayTitle, iTopic);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Changes the name of this objectEditor without changing the
        ///         <see cref="JaStDev.HAB.Designer.NeuronData.DisplayTitle" /> value.
        ///         This is used for Sub editors: a pattern editor that is a child of an
        ///         asset editor: it should always have a set name, based on the value of
        ///         the AssetEditor.
        ///     </para>
        ///     <para>Called when all the data UI data should be loaded.</para>
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void LoadUIData()
        {
            var iItem = Item;
            if (iItem != null)
            {
                var iFound = iItem.FindFirstOut((ulong)PredefinedNeurons.TextPatternTopic) as NeuronCluster;
                if (iFound != null)
                {
                    Items = new PatternDefCollection(this, iFound);
                }
                else
                {
                    Items = new PatternDefCollection(this, (ulong)PredefinedNeurons.TextPatternTopic);
                }

                LoadQuestions();
                LoadTopicFilters();
            }
        }

        /// <summary>The load topic filters.</summary>
        protected override void LoadTopicFilters()
        {
            Neuron iItem = Items.Cluster;
            var iFound = iItem.FindFirstOut((ulong)PredefinedNeurons.TopicFilter) as NeuronCluster;
            if (iFound != null)
            {
                TopicFilters = new TopicFilterCollection(this, iFound);
            }
            else
            {
                TopicFilters = new TopicFilterCollection(this, (ulong)PredefinedNeurons.TopicFilter);

                    // thi is not really correct. It causes the question cluster to be attached to the object, not the topic. To solve this, we trap the link creation and set it up correctly.
            }
        }

        /// <summary>
        ///     Loads the questions section. The questions need to be attached to the
        ///     actual topic cluster, not the object.
        /// </summary>
        protected override void LoadQuestions()
        {
            Neuron iItem = Items.Cluster;
            var iFound = iItem.FindFirstOut((ulong)PredefinedNeurons.Questions) as NeuronCluster;
            if (iFound != null)
            {
                Questions = new ConditionalOutputsCollection(this, iFound);
            }
            else
            {
                Questions = new ConditionalOutputsCollection(this, (ulong)PredefinedNeurons.Questions);

                    // thi is not really correct. It causes the question cluster to be attached to the object, not the topic. To solve this, we trap the link creation and set it up correctly.
            }
        }

        /// <summary>The outgoing link removed.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkRemoved(Link link)
        {
            if (link.MeaningID != (ulong)PredefinedNeurons.Questions)
            {
                // when the questions link gets created between object and question list, we delete and recreate the link so that it is between topic and question cluster, that's why we make certain that the 'qustions' collection doesn't get recreated. This is a bit of a problem, cause if the link betwen the topic and the questions gets destroyed, we don't see this.
                base.OutgoingLinkRemoved(link);
            }
        }

        /// <summary>Called when an outgoing <paramref name="link"/> got created.
        ///     Inheriters can use this to respond to changes. We check if the
        ///     linkMeaning is for the topic. If so, the topic cluster has been
        ///     created, and we should try to loade the <see cref="NeuronInfo"/>
        ///     again.</summary>
        /// <param name="link">The link.</param>
        protected internal override void OutgoingLinkCreated(Link link)
        {
            base.OutgoingLinkCreated(link);
            if (link.MeaningID == (ulong)PredefinedNeurons.TextPatternTopic)
            {
                // the actual topic was created (first instantiation of cluster).
                var iInfo = BrainData.Current.NeuronInfo[link.ToID];
                iInfo.DisplayTitle = Parsers.TopicsDictionary.GetUnique(Name + "_Topic");

                    // we store the topic-name also in the display title of the real topic id, so it is properly labeled as wel. (can work with a regulr TextPAtternEditor), is also important for exporting to xml: all textpatterns need a topicId.
                BrainData.Current.AttachedTopics.Add(link.ToID);
            }
            else if (link.MeaningID == (ulong)PredefinedNeurons.Questions
                     || link.MeaningID == (ulong)PredefinedNeurons.TopicFilter)
            {
                // when a questions list gets attached to the object, we need to reset the link so that it points from the topic (instead of the object) to the questions 
                Items.CreateOwnerLinkIfNeeded();
                var iTo = link.To;
                iTo.SetFirstIncommingLinkTo(link.MeaningID, Items.Cluster);
            }
        }
    }
}