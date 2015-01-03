// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemProfilerItem.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   Stores and presents (for the UI) all the info for a single neuron that
//   was montiored by the <see cref="MemProfiler" />
//   </para>
//   <para>and which was found to be a leak.</para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Profiler
{
    /// <summary>
    ///     <para>
    ///         Stores and presents (for the UI) all the info for a single neuron that
    ///         was montiored by the <see cref="MemProfiler" />
    ///     </para>
    ///     <para>and which was found to be a leak.</para>
    /// </summary>
    public class MemProfilerItem : Data.OwnedObject<MemProfiledProcessor>, INeuronWrapper, INeuronInfo
    {
        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f variables.</summary>
        private readonly System.Collections.Generic.List<MemProfiledVar> fVariables =
            new System.Collections.Generic.List<MemProfiledVar>();

        /// <summary>Initializes a new instance of the <see cref="MemProfilerItem"/> class. Initializes a new instance of the <see cref="MemProfilerItem"/>
        ///     class.</summary>
        /// <param name="item">The item.</param>
        /// <param name="createdAt">The created At.</param>
        public MemProfilerItem(Neuron item, Search.DisplayPath createdAt)
        {
            Item = item;
            CreatedAt = createdAt;
            CreatedBy = (DebugProcessor)Processor.CurrentProcessor;
        }

        #region CreatedAt

        /// <summary>
        ///     Gets the location at which the neuron was created.
        /// </summary>
        public Search.DisplayPath CreatedAt { get; private set; }

        #endregion

        #region CreatedBy

        /// <summary>
        ///     Gets the processor that created the neuron.
        /// </summary>
        public DebugProcessor CreatedBy { get; private set; }

        #endregion

        #region CreatedFor

        /// <summary>
        ///     Gets/sets the processor for which the item was created. This is only
        ///     valid during a split.
        /// </summary>
        public DebugProcessor CreatedFor { get; set; }

        #endregion

        #region UnfrozenAt

        /// <summary>
        ///     Gets/sets the location at which the neuron got unfrozen.
        /// </summary>
        public Search.DisplayPath UnfrozenAt { get; set; }

        #endregion

        /// <summary>
        ///     Gets the list of variables that had a reference to the specified
        ///     neuron when the processor died.
        /// </summary>
        /// <value>
        ///     The variables.
        /// </value>
        public System.Collections.Generic.List<MemProfiledVar> Variables
        {
            get
            {
                return fVariables;
            }
        }

        #region DuplicatedFor

        /// <summary>
        ///     Gets the global that triggered the creation of the leak, during a
        ///     duplication process.
        /// </summary>
        public MemProfiledVar DuplicatedFor { get; internal set; }

        #endregion

        #region HasDuplicatedFor

        /// <summary>
        ///     Gets a value indicating whether this instance has a 'DuplicatedFor'
        ///     global.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has duplicated for; otherwise,
        ///     <c>false</c> .
        /// </value>
        public bool HasDuplicatedFor
        {
            get
            {
                return DuplicatedFor != null;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether the item is selected or not (to bind to from UI)
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                fIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
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
                return BrainData.Current.NeuronInfo[Item.ID];
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
        public Neuron Item { get; private set; }

        #endregion
    }
}