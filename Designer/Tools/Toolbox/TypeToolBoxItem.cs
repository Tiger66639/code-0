// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeToolBoxItem.cs" company="">
//   
// </copyright>
// <summary>
//   A toolboxitem that produces items based on type information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A toolboxitem that produces items based on type information.
    /// </summary>
    /// <remarks>
    ///     When the content of this toolbox item is requested, a new object is
    ///     generated. This <see cref="Neuron" /> is registered to the brain, so it
    ///     gets a valid ID. To make certain that we don't generate items that are
    ///     never consumed, the drag drop support is delayed, that is a neuron is
    ///     only requested at the drop, not the drag.
    /// </remarks>
    public class TypeToolBoxItem : ToolBoxItem
    {
        /// <summary>The f category.</summary>
        private string fCategory;

        /// <summary>The f display title.</summary>
        private string fDisplayTitle;

        #region ItemType

        /// <summary>
        ///     Gets/sets the type of the item we should produce when the toolbox item
        ///     is used.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Type ItemType { get; set; }

        #endregion

        /// <summary>
        ///     Gets/sets the typename of the item this is a toolbox for. This is
        ///     primarely used for streaming.
        /// </summary>
        public string ItemTypeName
        {
            get
            {
                if (ItemType != null)
                {
                    return ItemType.AssemblyQualifiedName;

                        // we use AssemblyQualifiedName so we can load the type from the correct assembly, otherwise it will only find classes in this assembly and corlib.
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    ItemType = null;
                }
                else
                {
                    ItemType = Brain.Current.GetNeuronType(value);
                }
            }
        }

        /// <summary>Gets or sets the category.</summary>
        public override string Category
        {
            get
            {
                return fCategory;
            }

            set
            {
                OnPropertyChanging("Category", fCategory, value);
                fCategory = value;
                OnPropertyChanged("Category");
            }
        }

        /// <summary>
        ///     Required to <see langword="override" /> for toolboxitem, internally we
        ///     use <see cref="DisplayTitle" /> which allows a setter.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override string Title
        {
            get
            {
                return fDisplayTitle;
            }
        }

        #region DisplayTitle

        /// <summary>
        ///     Gets/sets the title to use. This is provided so we can init it from
        ///     xaml through prop setter.
        /// </summary>
        public string DisplayTitle
        {
            get
            {
                return fDisplayTitle;
            }

            set
            {
                fDisplayTitle = value;
                OnPropertyChanged("DisplayTitle");
                OnPropertyChanged("Title");
            }
        }

        #endregion

        /// <summary>Retrieves the <see cref="Neuron"/> for this toolbox item.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public override Neuron GetData()
        {
            Neuron iItem = null;
            WindowMain.UndoStore.BeginUndoGroup(true);

                // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
            try
            {
                iItem = NeuronFactory.Get(ItemType);
                if (iItem is TextNeuron)
                {
                    // try to provide a default value for strings.
                    ((TextNeuron)iItem).Text = "new item";
                }

                WindowMain.AddItemToBrain(iItem); // we use this way of adding cause it takes care of the undo data.
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }

            return iItem;
        }

        /// <summary>Retrieves the type</summary>
        /// <returns>The <see cref="Type"/>.</returns>
        public override System.Type GetResultType()
        {
            return ItemType;
        }
    }
}