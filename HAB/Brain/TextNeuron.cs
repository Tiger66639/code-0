// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="Neuron" /> which is used by the <see cref="TextSin" /> to represent it's context specific
//   data.  More specifically, it also stores the text that the neuron represents.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see cref="Neuron" /> which is used by the <see cref="TextSin" /> to represent it's context specific
    ///     data.  More specifically, it also stores the text that the neuron represents.
    /// </summary>
    /// <remarks>
    ///     This class returns the value for <see cref="TextNeuron.Text" /> for it's <see cref="TextNeuron.ToString" /> method.
    ///     <para>
    ///         The textsin stores all the textneurons in a dictionary as entry points. Only the first thextneuron for a single
    ///         text
    ///         is stored.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.TextNeuron, typeof(Neuron))]
    public class TextNeuron : ValueNeuron
    {
        /// <summary>The f text.</summary>
        private string fText;

        #region blob

        /// <summary>
        ///     Gets or sets the value of the neuron as a blob.
        /// </summary>
        /// <value>The object encapsulated by the neuron.</value>
        /// <remarks>
        ///     This can be used during streaming of the neuron.
        /// </remarks>
        public override object Blob
        {
            get
            {
                return Text;
            }

            set
            {
                if (value is string)
                {
                    Text = (string)value;
                }
                else if (value == null)
                {
                    Text = null;
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.TextNeuron;
            }
        }

        #endregion

        /// <summary>
        ///     Clears all the data from this instance.
        /// </summary>
        /// <remarks>
        ///     This function is automically called when a neuron is deleted.
        ///     This includes incomming and outgoing links, clustered by, children (if it is a clusterr), and any possible values.
        ///     <para>
        ///         Descendents can enhance this function and clean more data.
        ///     </para>
        /// </remarks>
        protected internal override void Clear()
        {
            base.Clear();
            if (string.IsNullOrEmpty(Text) == false)
            {
                TextSin.Words.Remove(this); // wil check if the id matches.
            }

            fText = null;
        }

        /// <summary>
        ///     Gets the text representation of the class.
        /// </summary>
        /// <returns>Returns the <see cref="TextNeuron.Text" /> value.</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>Copies all the data from this neuron to the argument.</summary>
        /// <param name="copyTo">The object to copy their data to.</param>
        /// <remarks>By default, it only copies over all of the links (which includes the 'LinksOut' and 'LinksIn' lists.
        /// <para>
        ///         Inheriters should reimplement this function and copy any extra information required for their specific type
        ///         of neuron.</para>
        /// <para>The copied version doesn't get stored in the textsin dict, only the first one.</para>
        /// </remarks>
        protected internal override void CopyTo(Neuron copyTo)
        {
            base.CopyTo(copyTo);
            var iCopyTo = copyTo as TextNeuron;
            if (iCopyTo != null)
            {
                iCopyTo.fText = fText;
            }
        }

        /// <summary>The compare with.</summary>
        /// <param name="right">The neuron to compare it with.</param>
        /// <param name="op">The operator to use.</param>
        /// <returns>True if the operator is correct.</returns>
        protected internal override bool CompareWith(Neuron right, Neuron op)
        {
            var iRight = right as TextNeuron;
            if (iRight != null)
            {
                var iVal = ((TextNeuron)right).Text;
                switch (op.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return Text == iRight.Text;
                    case (ulong)PredefinedNeurons.Smaller:
                        return string.Compare(Text, iRight.Text) < 0;
                    case (ulong)PredefinedNeurons.SmallerOrEqual:
                        return string.Compare(Text, iRight.Text) <= 0;
                    case (ulong)PredefinedNeurons.Bigger:
                        return string.Compare(Text, iRight.Text) > 0;
                    case (ulong)PredefinedNeurons.BiggerOrEqual:
                        return string.Compare(Text, iRight.Text) >= 0;
                    case (ulong)PredefinedNeurons.Different:
                        return Text != iRight.Text;
                    default:
                        LogService.Log.LogError(
                            "TextNeuron.CompareWith", 
                            string.Format("Invalid operator found: {0}.", op));
                        return false;
                }
            }

            return base.CompareWith(right, op);
        }

        /// <summary>Changes the type of this neuron to the new specified type.  This action creates and destroys the object.</summary>
        /// <param name="type">The requested type.</param>
        /// <returns>The new object that represents the neuron of the new type.</returns>
        /// <remarks>Values are copied over when possible.  Links are always copied over.
        ///     The new object is registed with the brain.
        ///     For a textneuron, the text is removed from the textsin dict, otherwise we can get errors later on.</remarks>
        public override Neuron ChangeTypeTo(System.Type type)
        {
            var iRes = base.ChangeTypeTo(type);
            if (iRes != null && string.IsNullOrEmpty(Text) == false)
            {
                // when this textneuron is stored in the textsin's dict, we need to remove ourselves.
                TextSin.Words.Remove(this);
            }

            return iRes;
        }

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            var iPrev = System.Xml.WhitespaceHandling.None;
            base.ReadXml(reader);
            if (reader is System.Xml.XmlTextReader)
            {
                // we must make certain that we read all the white spaces correctly, we can only set this on an XmlTextreader.
                iPrev = ((System.Xml.XmlTextReader)reader).WhitespaceHandling;
                ((System.Xml.XmlTextReader)reader).WhitespaceHandling = System.Xml.WhitespaceHandling.All;
            }

            reader.ReadStartElement("Text");
            fText = reader.ReadString();
            if (reader is System.Xml.XmlTextReader)
            {
                ((System.Xml.XmlTextReader)reader).WhitespaceHandling = iPrev;
            }

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The xml writer to use</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteStartElement("Text");
            writer.WriteString(Text);
            writer.WriteEndElement();
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
            fText = reader.ReadString();
            return iRes;
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            if (fText != null)
            {
                // can be null for sins (which inherit from textneuron).
                writer.Write(fText);
            }
            else
            {
                writer.Write(string.Empty);
            }
        }

        /// <summary>Compares this neuron with the other neuron.
        ///     By default, this first converts the neuron to a string and compares the 2 string values.</summary>
        /// <param name="other">The other.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int CompareTo(Neuron other)
        {
            if (other is TextNeuron)
            {
                return Text.CompareTo(((TextNeuron)other).Text);
            }

            return base.CompareTo(other);
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextNeuron" /> class.
        /// </summary>
        internal TextNeuron()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TextNeuron"/> class.</summary>
        /// <param name="text">The text to wrap.</param>
        internal TextNeuron(string text)
        {
            Text = text;
        }

        /// <summary>Gets the TextNeuron for the specified text.  If a textNeuron already existed (and which was registered with the<see cref="TextSin"/>), the existing item is returned, otherwise a new one is created.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="freezeOn">The freeze On.</param>
        /// <returns>The <see cref="TextNeuron"/>.</returns>
        public static TextNeuron GetFor(string toWrap, Processor freezeOn = null)
        {
            return TextSin.Words.GetNeuronFor(toWrap, freezeOn);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="TextNeuron" /> class.
        ///     The object is recycled, so make certain that the field is reset.
        /// </summary>
        ~TextNeuron()
        {
            fText = null;
        }

        #endregion

        #region Text

        /// <summary>
        ///     Gets/sets the text that the neuron respresents.
        /// </summary>
        public string Text
        {
            get
            {
                return fText;
            }

            set
            {
                if (fText != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);
                    try
                    {
                        fText = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    SetIsChangedNoUnfreeze(true);

                        // don't unfreeze, otherwise changing the value would cause to many mem leaks.
                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Text", this));
                    }
                }
            }
        }

        /// <summary>used by the neuronfactory to provide an initial value.</summary>
        /// <param name="value"></param>
        internal void SetInitValue(string value)
        {
            fText = value;
        }

        #endregion
    }
}