// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionedMindMapItem.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="MindMapItem" /> that stores position info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A <see cref="MindMapItem" /> that stores position info.
    /// </summary>
    public class PositionedMindMapItem : MindMapItem, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>Sets the position in 1 call by simulating a drag operation..</summary>
        /// <remarks><para>Normally, a drag operation isn't different from setting the<paramref name="x"/> and <paramref name="y"/> props. However, when a
        ///         cluster is dragged, it's children should also be moved. This is done
        ///         through this call.</para>
        /// <para>This is a <see langword="virtual"/> function so that descendents can
        ///         change some of the behaviour a drop operation does on a positioned
        ///         mindmap item.</para>
        /// </remarks>
        /// <param name="x">The x pos</param>
        /// <param name="y">The y pos.</param>
        /// <param name="alreadyMoved">Keeps track of all the items that have already moved, so that we know
        ///     which still to move. This is required cause 2 clusters might own the
        ///     same child, if both are moved, they will both try to offset the child,
        ///     which is not required, only 1 had to, the other needs to skip the
        ///     operation.</param>
        public virtual void SetPositionFromDrag(
            double x, 
            double y, System.Collections.Generic.List<MindMapNeuron> alreadyMoved)
        {
            X = x;
            Y = y;
        }

        #region Fields

        /// <summary>The f y.</summary>
        private double fY;

        /// <summary>The f x.</summary>
        private double fX;

        /// <summary>The f width.</summary>
        private double fWidth;

        /// <summary>The f height.</summary>
        private double fHeight;

        #endregion

        #region Prop

        #region X

        /// <summary>
        ///     Gets/sets the horizontal offset of the item on the graph.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children.
        /// </remarks>
        public virtual double X
        {
            get
            {
                return fX;
            }

            set
            {
                if (fX != value)
                {
                    if (value <= 0)
                    {
                        value = 0;
                    }

                    OnPropertyChanging("X", fX, value);
                    fX = value;
                    OnPropertyChanged("X");
                }
            }
        }

        #endregion

        #region Y

        /// <summary>
        ///     Gets/sets the vertical offset of the item on the graph.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children.
        /// </remarks>
        public virtual double Y
        {
            get
            {
                return fY;
            }

            set
            {
                if (fY != value)
                {
                    if (value <= 0)
                    {
                        value = 0;
                    }

                    OnPropertyChanging("Y", fY, value);
                    fY = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width of the item.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children.
        /// </remarks>
        /// <value>
        ///     The width.
        /// </value>
        public virtual double Width
        {
            get
            {
                return fWidth;
            }

            set
            {
                if (fWidth != value)
                {
                    if (value <= 4)
                    {
                        // make certain we can't make it to small.
                        value = 4;
                    }

                    OnPropertyChanging("Width", fWidth, value);
                    fWidth = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the height of the item.
        /// </summary>
        /// <remarks>
        ///     <see langword="virtual" /> so that <see cref="MindMapCluster" /> can
        ///     check and change val as desired, depending on children.
        /// </remarks>
        public virtual double Height
        {
            get
            {
                return fHeight;
            }

            set
            {
                if (fHeight != value)
                {
                    if (value <= 4)
                    {
                        // make certain we can't make it to small.
                        value = 4;
                    }

                    OnPropertyChanging("Height", fHeight, value);
                    fHeight = value;
                    OnPropertyChanged("Height");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets the bounding rect of the object.
        /// </summary>
        /// <value>
        ///     The bounding rect.
        /// </value>
        public override System.Windows.Rect BoundingRect
        {
            get
            {
                return new System.Windows.Rect(X, Y, Width, Height);
            }
        }

        #region Owner

        /// <summary>
        ///     Gets the owning <see cref="MindMap" /> of the item.
        /// </summary>
        /// <remarks>
        ///     we <see langword="override" /> cause when the owner is set, we need to
        ///     update it's width and height.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public override MindMap Owner
        {
            get
            {
                return base.Owner;
            }

            set
            {
                base.Owner = value;
                if (value != null)
                {
                    if (Height + Y > value.Height)
                    {
                        value.Height = Height + Y;
                    }

                    if (Width + X > value.Width)
                    {
                        value.Width = Width + X;
                    }
                }
            }
        }

        #endregion

        /// <summary>The duplicate.</summary>
        /// <returns>A deep copy of this object.</returns>
        public override MindMapItem Duplicate()
        {
            var iRes = (PositionedMindMapItem)base.Duplicate();
            iRes.X = X;
            iRes.Y = Y;
            iRes.Width = Width;
            iRes.Height = Height;
            return iRes;
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

        /// <summary>Reads the content of the XML file (all the properties)</summary>
        /// <param name="reader">The reader.</param>
        protected virtual void ReadXmlContent(System.Xml.XmlReader reader)
        {
            ZIndex = XmlStore.ReadElement<int>(reader, "ZIndex");
            X = XmlStore.ReadElement<double>(reader, "X");
            Y = XmlStore.ReadElement<double>(reader, "Y");
            Width = XmlStore.ReadElement<double>(reader, "Width");
            Height = XmlStore.ReadElement<double>(reader, "Height");
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement<double>(writer, "ZIndex", ZIndex);
            XmlStore.WriteElement(writer, "X", X);
            XmlStore.WriteElement(writer, "Y", Y);
            XmlStore.WriteElement(writer, "Width", Width);
            XmlStore.WriteElement(writer, "Height", Height);
        }

        #endregion
    }
}