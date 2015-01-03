// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommChannel.cs" company="">
//   
// </copyright>
// <summary>
//   Incapsulates the functionality for a communication channel (
//   <see cref="JaStDev.HAB.Designer.CommChannel.Sin" /> ).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Incapsulates the functionality for a communication channel (
    ///     <see cref="JaStDev.HAB.Designer.CommChannel.Sin" /> ).
    /// </summary>
    /// <remarks>
    ///     Warning: when implementing a new descendent, don't forget to add to
    ///     <see cref="CommChannelCollection" /> for reading and writing to xmls
    ///     stream.
    /// </remarks>
    public abstract class CommChannel : Data.OwnedObject, 
                                        INeuronInfo, 
                                        INeuronWrapper, 
                                        IDocumentInfo, 
                                        System.Xml.Serialization.IXmlSerializable
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChannel" /> class.
        /// </summary>
        public CommChannel()
        {
            Resources = new Data.ObservedCollection<ResourceReference>(this);
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (fItemData == null && Sin != null)
                {
                    // when a project gets loaded, we can't yet load the NeuronData cause it is not yet accessible, so do this in a delayed fashion.
                    fItemData = BrainData.Current.NeuronInfo[Sin.ID];
                }

                return fItemData;
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item
        {
            get
            {
                return Sin;
            }
        }

        #endregion

        /// <summary>Sends the resource to the sensory interface, in case it is supported.</summary>
        /// <remarks>The default throws exception.</remarks>
        /// <param name="fileName">Name of the file.</param>
        internal virtual void SendResource(string fileName)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Called from the UI thread just after the project has been loaded. This
        ///     allows communication channels to perform load tasks that can only be
        ///     done from the UI.
        /// </summary>
        /// <remarks>
        ///     By default, this will only make certain that the channel is visible.
        /// </remarks>
        internal virtual void AfterLoaded()
        {
            UpdateOpenDocuments();
        }

        #region Fields

        /// <summary>The f is visible.</summary>
        private bool fIsVisible;

        // string fName;
        /// <summary>The f item data.</summary>
        private NeuronData fItemData;

        /// <summary>The f dictionary.</summary>
        private Data.SerializableDictionary<string, object> fDictionary =
            new Data.SerializableDictionary<string, object>();

        #endregion

        #region prop

        #region IsVisible

        /// <summary>
        ///     Gets/sets the if this comm channel is curently visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return fIsVisible;
            }

            set
            {
                if (fIsVisible != value)
                {
                    OnPropertyChanging("IsVisible", fIsVisible, value);
                    fIsVisible = value;
                    UpdateOpenDocuments();
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        /// <summary>
        ///     Called when the visibility of the commchannel is changed.
        /// </summary>
        protected internal virtual void UpdateOpenDocuments()
        {
            if (BrainData.Current != null && BrainData.Current.DesignerData != null)
            {
                // when designerData is not set, not all the data is loaded yet. + this is also called when the project is cleared, so that comm channels get a change of clearing any 'global stuff', like the character windows of the chatbots.
                if (IsVisible)
                {
                    var iMain = (WindowMain)System.Windows.Application.Current.MainWindow;
                    if (iMain != null)
                    {
                        iMain.AddItemToOpenDocuments(this);
                    }
                    else if (BrainData.Current != null && BrainData.Current.OpenDocuments != null)
                    {
                        BrainData.Current.OpenDocuments.Add(this); // this actually shows the item
                    }
                }
                else
                {
                    BrainData.Current.OpenDocuments.Remove(this);
                }
            }
        }

        #endregion

        #region NeuronID

        /// <summary>
        ///     Gets/sets the ID of the
        ///     <see cref="JaStDev.HAB.Designer.CommChannel.Sin" />
        /// </summary>
        /// <remarks>
        ///     This prop is primarely for streaming
        /// </remarks>
        public ulong NeuronID
        {
            get
            {
                if (Sin != null)
                {
                    return Sin.ID;
                }

                return Neuron.EmptyId;
            }

            set
            {
                if (value != Neuron.EmptyId)
                {
                    if (Brain.Current.IsValidID(value))
                    {
                        Neuron iFound;
                        if (Brain.Current.TryFindNeuron(value, out iFound))
                        {
                            SetSin(iFound as Sin);
                            if (Sin == null)
                            {
                                LogService.Log.LogError(
                                    "Communication channel", 
                                    string.Format(
                                        "Failed to load neuron with id {0} because it is not a Sin. You probably have a corrupt comm channels file.", 
                                        value));
                            }
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "Communication channel", 
                                string.Format("No neuron found with id: {0}.", value));
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "Communication channel", 
                            string.Format(
                                "Failed to load neuron with id {0} because it is out of range of the brain's content. You probably have a corrupt comm channels file.", 
                                value));
                    }

                    if (Sin == null)
                    {
                        // we weren't assigning a null, so we need to have somthing, if this is not the case, oops.
                        ProjectManager.Default.DataError = true;
                    }
                }
                else
                {
                    SetSin(null);
                }
            }
        }

        /// <summary>Sets the Sensory <see langword="interface"/> that this object is a
        ///     wrapper of.</summary>
        /// <param name="sin">The sin.</param>
        protected internal virtual void SetSin(Sin sin)
        {
            Sin = sin;
            if (sin != null && BrainData.Current != null && BrainData.Current.NeuronInfo != null)
            {
                // neuroninfo can be null when a project is loaded.  SetSin gets called after the entire project is loaded.
                fItemData = BrainData.Current.NeuronInfo[Sin.ID];
            }
            else
            {
                fItemData = null;
            }
        }

        #endregion

        #region Sin

        /// <summary>
        ///     Gets the sin that this object incapsulates.
        /// </summary>
        /// <value>
        ///     The sin.
        /// </value>
        public Sin Sin { get; private set; }

        #endregion

        #region Resources

        /// <summary>
        ///     Gets the list of resources that are defined for this sin.
        /// </summary>
        public Data.ObservedCollection<ResourceReference> Resources { get; private set; }

        #endregion

        #endregion

        #region IDocumentInfo Members

        /// <summary>
        ///     Gets or sets the document title.
        /// </summary>
        /// <value>
        ///     The document title.
        /// </value>
        public string DocumentTitle
        {
            get
            {
                if (NeuronInfo != null)
                {
                    return NeuronInfo.DisplayTitle;
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public string DocumentInfo
        {
            get
            {
                if (Item != null && NeuronInfo != null)
                {
                    return "Communication channel: " + NeuronInfo.DisplayTitle + ", Neuron: " + Item.ID;
                }

                return "Communication channel";
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public string DocumentType
        {
            get
            {
                return "Communication channel";
            }
        }

        /// <summary>
        ///     Gets or sets the document icon.
        /// </summary>
        /// <value>
        ///     The document icon.
        /// </value>
        public object DocumentIcon
        {
            get
            {
                return "/Images/Commchannel/network.png";
            }
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
        /// <remarks>Descendents need to perform mapping between module index and neurons
        ///     when importing from modules.</remarks>
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

            ReadXmlContent(reader);
            reader.ReadEndElement();
        }

        /// <summary>The read xml content.</summary>
        /// <param name="reader">The reader.</param>
        protected virtual void ReadXmlContent(System.Xml.XmlReader reader)
        {
            fIsVisible = XmlStore.ReadElement<bool>(reader, "IsVisible");
            NeuronID = XmlStore.ReadElement<ulong>(reader, "NeuronID");
            var iIsEmpty = reader.IsEmptyElement;
            reader.ReadStartElement("Resources");
            Resources.Clear();
            if (iIsEmpty == false)
            {
                var iSer = new System.Xml.Serialization.XmlSerializer(typeof(ResourceReference));
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    var iNew = iSer.Deserialize(reader) as ResourceReference;
                    Resources.Add(iNew);
                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <remarks>When streaming to a module (for export), we do a mapping, to the index
        ///     of the neuron in the module that is currently being exported, and off
        ///     course visa versa, when reading from a module.</remarks>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "IsVisible", IsVisible);
            XmlStore.WriteElement(writer, "NeuronID", NeuronID);
            var iSer = new System.Xml.Serialization.XmlSerializer(typeof(ResourceReference));
            writer.WriteStartElement("Resources");
            foreach (var i in Resources)
            {
                iSer.Serialize(writer, i);
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}