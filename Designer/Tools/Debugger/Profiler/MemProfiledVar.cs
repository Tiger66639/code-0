// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemProfiledVar.cs" company="">
//   
// </copyright>
// <summary>
//   The mem profiled var.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Profiler
{
    /// <summary>The mem profiled var.</summary>
    public class MemProfiledVar : Data.OwnedObject<MemProfiledProcessor>, INeuronWrapper, INeuronInfo
    {
        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>Initializes a new instance of the <see cref="MemProfiledVar"/> class.</summary>
        /// <param name="item">The item.</param>
        public MemProfiledVar(Neuron item)
        {
            Item = item;
        }

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