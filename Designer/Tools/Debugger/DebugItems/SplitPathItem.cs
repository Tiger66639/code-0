// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitPathItem.cs" company="">
//   
// </copyright>
// <summary>
//   Contains info regarding a single neuron is a path left by multiple split
//   operations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains info regarding a single neuron is a path left by multiple split
    ///     operations.
    /// </summary>
    public class SplitPathItem : Data.ObservableObject, INeuronWrapper
    {
        #region fields

        /// <summary>The f is break point.</summary>
        private bool fIsBreakPoint;

        /// <summary>The f debug neuron.</summary>
        private DebugNeuron fDebugNeuron;

        #endregion

        #region Prop

        #region ItemID

        /// <summary>
        ///     Gets/sets the id of the item that is wrapped. This is for streaming
        ///     purposes.
        /// </summary>
        public ulong ItemID { get; set; }

        #endregion

        #region IsBreakPoint

        /// <summary>
        ///     Gets/sets the if processors should pause when they reach this point in
        ///     the path.
        /// </summary>
        public bool IsBreakPoint
        {
            get
            {
                return fIsBreakPoint;
            }

            set
            {
                fIsBreakPoint = value;
                OnPropertyChanged("IsBreakPoint");
            }
        }

        #endregion

        #region DebugNeuron

        /// <summary>
        ///     Gets the neuron as a
        ///     <see cref="JaStDev.HAB.Designer.SplitPathItem.DebugNeuron" /> . This
        ///     is for visualising the data.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public DebugNeuron DebugNeuron
        {
            get
            {
                if (fDebugNeuron == null)
                {
                    var iItem = Item;
                    if (iItem != null)
                    {
                        fDebugNeuron = new DebugNeuron(iItem);
                    }
                }

                return fDebugNeuron;
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
        [System.Xml.Serialization.XmlIgnore]
        public Neuron Item
        {
            get
            {
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(ItemID, out iFound))
                {
                    return iFound;
                }

                return null;
            }
        }

        #endregion

        #endregion
    }
}