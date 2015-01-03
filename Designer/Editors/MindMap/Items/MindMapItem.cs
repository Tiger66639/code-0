// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapItem.cs" company="">
//   
// </copyright>
// <summary>
//   An item that can be displayed on a <see cref="MindMap" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An item that can be displayed on a <see cref="MindMap" /> .
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Ineritors should reimplement the 'Duplicate' function. Could be that this
    ///         class needs to be streamed manually with xmlwriter.WriteRaw for
    ///         flowDocument.
    ///     </para>
    ///     <para>
    ///         This is an owned object, with <see cref="MindMap" /> as the owner, so
    ///         that it can update certain properties of the owner in response to
    ///         property changes here (like the width, height, or link line points).
    ///     </para>
    /// </remarks>
    [System.Xml.Serialization.XmlInclude(typeof(MindMapLink))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapNeuron))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapNote))]
    [System.Xml.Serialization.XmlInclude(typeof(MindMapCluster))]
    public abstract class MindMapItem : Data.OwnedObject<MindMap>, IDescriptionable
    {
        #region functions

        /// <summary>
        ///     creates a deep copy of this object.
        /// </summary>
        /// <remarks>
        ///     Currently primarely used for dragging.
        /// </remarks>
        /// <returns>
        ///     A deep copy of this object.
        /// </returns>
        public virtual MindMapItem Duplicate()
        {
            var iRes = System.Activator.CreateInstance(GetType()) as MindMapItem;
            iRes.fDescription = fDescription;
            return iRes;
        }

        #endregion

        #region Fields

        /// <summary>The f description.</summary>
        private string fDescription;

        /// <summary>The f z index.</summary>
        private int fZIndex;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        #endregion

        #region Prop

        #region Adorner

        /// <summary>
        ///     Gets/sets the Adorner used by the mindmap item.
        /// </summary>
        /// <remarks>
        ///     This property is added because we need a secure location to store the
        ///     adorner for each item in such a way we can always get to it, even if
        ///     the listbox item that represents the mindmap item is already removed.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Windows.Documents.Adorner Adorner { get; set; }

        #endregion

        #region ZIndex

        /// <summary>
        ///     Gets/sets the Z-index of this item.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This value determins the order in which they are drawn on the canvas.
        ///         The bigger this value, the more likely the item will appear at the
        ///         top.
        ///     </para>
        ///     <para>By default, this is 0.</para>
        /// </remarks>
        public int ZIndex
        {
            get
            {
                return fZIndex;
            }

            set
            {
                OnPropertyChanging("ZIndex", fZIndex, value);
                fZIndex = value;
                OnPropertyChanged("ZIndex");
            }
        }

        #endregion

        #region Description

        /// <summary>
        ///     Gets the description for this object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Always returns a disconnected item. When the return value is modified,
        ///         it is not saved back in the mind map item, so you need to reassign it.
        ///         This is crap, I know, it's because flowDocuments take up to much mem,
        ///         so need to save as string.
        ///     </para>
        ///     <para>
        ///         this is <see langword="virtual" /> so that the
        ///         <see cref="MindMapNeuron" /> can <see langword="override" /> this.
        ///     </para>
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public virtual System.Windows.Documents.FlowDocument Description
        {
            get
            {
                if (fDescription != null)
                {
                    var stringReader = new System.IO.StringReader(fDescription);
                    var xmlReader = System.Xml.XmlReader.Create(stringReader);
                    return System.Windows.Markup.XamlReader.Load(xmlReader) as System.Windows.Documents.FlowDocument;
                }

                return Helper.CreateDefaultFlowDoc();
            }

            set
            {
                var iVal = System.Windows.Markup.XamlWriter.Save(value);
                if (fDescription != iVal)
                {
                    fDescription = iVal;
                    OnPropertyChanged("Description");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the description as xaml text.
        /// </summary>
        /// <value>
        ///     The description text.
        /// </value>
        public virtual string DescriptionText
        {
            get
            {
                return fDescription;
            }

            set
            {
                fDescription = value;
                OnPropertyChanged("Description");
                OnPropertyChanged("DescriptionText");
            }
        }

        #region IDescriptionable Members

        /// <summary>Gets the description title.</summary>
        public virtual string DescriptionTitle
        {
            get
            {
                return "Mindmap item";
            }
        }

        #endregion

        /// <summary>
        ///     this is used to determin a custom ordening for saving the items in the
        ///     list. This is used to make certain that link objects are at the back
        ///     of the list (so that they can load properly).
        /// </summary>
        /// <value>
        ///     The index of the type.
        /// </value>
        protected internal virtual int TypeIndex
        {
            get
            {
                return 1;
            }
        }

        #region BoundingRect

        /// <summary>
        ///     Gets the bounding rect of the object.
        /// </summary>
        /// <value>
        ///     The bounding rect.
        /// </value>
        public abstract System.Windows.Rect BoundingRect { get; }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the value that indicates if this item is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    if (Owner != null)
                    {
                        // if owner, set field through the list update.
                        if (value)
                        {
                            Owner.SelectedItems.Insert(0, this);

                                // we do an insert at the first place whenever we get selected, this way we are set as the first ones.
                        }
                        else
                        {
                            Owner.SelectedItems.Remove(this);
                        }
                    }
                    else
                    {
                        fIsSelected = value;
                        OnPropertyChanged("IsSelected");
                    }
                }
                else if (value && Owner != null)
                {
                    Owner.SelectedItems.Move(Owner.SelectedItems.IndexOf(this), 0);

                        // when we get reselected, we make certain that we are the activally selected items.
                }
            }
        }

        /// <summary>Sets the is selected value, without updating the
        ///     MindMapItemSelectionList. This is called from that list.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetIsSelected(bool value)
        {
            fIsSelected = value;
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #endregion
    }
}