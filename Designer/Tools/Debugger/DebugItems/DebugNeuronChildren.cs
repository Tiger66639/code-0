// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugNeuronChildren.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data required for a single set of children of a
//   <see cref="DebugNeuron" /> . This is usually for the Input links, the
//   output links or the children.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the data required for a single set of children of a
    ///     <see cref="DebugNeuron" /> . This is usually for the Input links, the
    ///     output links or the children.
    /// </summary>
    public class DebugNeuronChildren : Data.OwnedObject<DebugNeuron>
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DebugNeuronChildren"/> class. Initializes a new instance of the <see cref="DebugNeuronChildren"/>
        ///     class.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="name">The name.</param>
        public DebugNeuronChildren(DebugNeuron owner, string name)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            Owner = owner;
            Name = name;
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the lists of incomming links.
        /// </summary>
        /// <remarks>
        ///     This is internally settable so we can quickly remove and clean out the
        ///     lis if it is no longer needed.
        /// </remarks>
        public System.Collections.ObjectModel.ObservableCollection<DebugRef> Children
        {
            get
            {
                return fChildren;
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets/sets if the content of the list is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return fIsLoaded;
            }

            set
            {
                if (fIsLoaded != value)
                {
                    if (value)
                    {
                        // need to clean up.
                        Owner.CreateChildrenFor(this);
                    }
                    else
                    {
                        UnloadChildren(); // release all resources as soon as possible
                        Children.Clear();
                    }

                    fIsLoaded = value;
                    OnPropertyChanged("IsLoaded");
                }
            }
        }

        #endregion

        #region HasChildren

        /// <summary>
        ///     Gets if there are children. Use this when
        ///     <see cref="JaStDev.HAB.Designer.DebugNeuronChildren.IsLoaded" /> to
        ///     see if there are any children.
        /// </summary>
        /// <remarks>
        ///     this property is indicated as changed whenever a link changes where
        ///     <see cref="DebugNeuron.item" /> is involved.
        /// </remarks>
        public bool HasChildren
        {
            get
            {
                return Owner.HasChildren(this);
            }
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets the name of the list to use for display purposes.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        /// <summary>
        ///     Updates the <see cref="HasChildren" /> value.
        /// </summary>
        internal void UpdateHasChildren()
        {
            OnPropertyChanged("HasChildren");
        }

        /// <summary>The unload children.</summary>
        internal void UnloadChildren()
        {
            foreach (var i in fChildren)
            {
                i.PointsTo.Item = null;
            }
        }

        #region fields

        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f children.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<DebugRef> fChildren =
            new System.Collections.ObjectModel.ObservableCollection<DebugRef>();

        #endregion
    }
}