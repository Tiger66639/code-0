// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronToolBoxItem.cs" company="">
//   
// </copyright>
// <summary>
//   A toolboxItem that Produces an item that is the reference to a neuron.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A toolboxItem that Produces an item that is the reference to a neuron.
    /// </summary>
    public class NeuronToolBoxItem : ToolBoxItem, INeuronInfo, INeuronWrapper
    {
        // , IXmlSerializable
        #region INeuronWrapper Members

        /// <summary>Gets the item.</summary>
        Neuron INeuronWrapper.Item
        {
            get
            {
                return Item;
            }
        }

        #endregion

        // #region IXmlSerializable Members

        // public XmlSchema GetSchema()
        // {
        // return null;
        // }

        // public void ReadXml(XmlReader reader)
        // {
        // bool wasEmpty = reader.IsEmptyElement;

        // reader.Read();
        // if (wasEmpty) return;

        // reader.ReadStartElement("Neuron");
        // string iVal = reader.ReadString();
        // ulong iConverted = ulong.Parse(iVal);
        // Item = Brain.Current[iConverted];
        // reader.ReadEndElement();

        // //reader.ReadEndElement();
        // }

        // public void WriteXml(XmlWriter writer)
        // {
        // if (fItem != null)
        // {
        // writer.WriteStartElement("Neuron");
        // writer.WriteString(fItem.ID.ToString());
        // writer.WriteEndElement();
        // }
        // }

        // #endregion

        /// <summary>Need to reraise events.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fNeuronInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Category" || e.PropertyName == "Title" || e.PropertyName == "DisplayTitle")
            {
                OnPropertyChanged(e.PropertyName);
                OnPropertyChanged("Item");

                    // we also update item, cause this event handler is the only way that this class can find out when a neuron's type was changed, resulting in a new object type.
            }
        }

        /// <summary>Simply return the item that this object represents.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public override Neuron GetData()
        {
            return Item;
        }

        /// <summary>The get result type.</summary>
        /// <returns>The <see cref="Type"/>.</returns>
        public override System.Type GetResultType()
        {
            return Item.GetType();
        }

        #region Fields

        /// <summary>The f item id.</summary>
        private ulong fItemId;

        /// <summary>The f neuron info.</summary>
        private NeuronData fNeuronInfo;

        #endregion

        #region prop

        #region Item

        /// <summary>
        ///     Gets/sets the <see cref="Neuron" /> object that this object wraps.
        /// </summary>
        /// <remarks>
        ///     ItemId is the lead here (stores the data in it's format) cause the
        ///     item itself can change (when the neuron is changed).
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Item
        {
            get
            {
                if (fItemId != Neuron.EmptyId)
                {
                    return Brain.Current[fItemId];
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    ItemID = value.ID;
                }
                else
                {
                    ItemID = Neuron.EmptyId;
                }
            }
        }

        #endregion

        /// <summary>
        ///     The id of the item we wrap, for streaming to xml
        /// </summary>
        /// <remarks>
        ///     When set, we will try to retrieve the category and name from the links
        ///     to list.
        /// </remarks>
        public ulong ItemID
        {
            get
            {
                return fItemId;
            }

            set
            {
                if (value != fItemId)
                {
                    if (fNeuronInfo != null)
                    {
                        fNeuronInfo.PropertyChanged -= fNeuronInfo_PropertyChanged;
                        fNeuronInfo = null; // need to reset the neuron info so it is reloaded.
                    }

                    fItemId = value;
                    OnPropertyChanged("Neuron");
                    OnPropertyChanged("Category");
                    OnPropertyChanged("Title");
                }
            }
        }

        #region INeuronInfo Members

        /// <summary>
        ///     We use an property for this so we can delay load the nueron info. We
        ///     do this so we can properly open the xml file when there is no
        ///     brainData.Current yet.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronData NeuronInfo
        {
            get
            {
                if (fNeuronInfo == null && BrainData.Current != null && Item != null)
                {
                    // BrainData.Current is not present when we are loading the data.
                    fNeuronInfo = BrainData.Current.NeuronInfo[Item.ID]; // should always return a result.
                    System.Diagnostics.Debug.Assert(fNeuronInfo != null);
                    fNeuronInfo.PropertyChanged += fNeuronInfo_PropertyChanged;
                }

                return fNeuronInfo;
            }
        }

        #endregion

        /// <summary>Gets or sets the category.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public override string Category
        {
            get
            {
                if (NeuronInfo != null)
                {
                    return NeuronInfo.Category;
                }

                return null;
            }

            set
            {
                if (NeuronInfo != null)
                {
                    NeuronInfo.Category = value;
                }
            }
        }

        /// <summary>Gets the title.</summary>
        [System.Xml.Serialization.XmlIgnore]
        public override string Title
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

        #endregion
    }
}