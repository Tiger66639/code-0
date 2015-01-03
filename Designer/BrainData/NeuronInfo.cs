// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronInfo.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for neurons that provides access to the
//   <see cref="NeuronData" /> for the neuron wrapped by this object (defined
//   by <see cref="JaStDev.HAB.Designer.NeuronInfo.ID" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for neurons that provides access to the
    ///     <see cref="NeuronData" /> for the neuron wrapped by this object (defined
    ///     by <see cref="JaStDev.HAB.Designer.NeuronInfo.ID" /> .
    /// </summary>
    /// <remarks>
    ///     This class can be used statically in xaml to create references to neurons
    ///     for instance in a combobox that always needs to have the same fixed
    ///     neurons.
    /// </remarks>
    public class NeuronInfo : INeuronInfo
    {
        #region Info

        /// <summary>
        ///     Gets the infor for this neuron.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronData Info { get; internal set; }

        #endregion

        #region ID

        /// <summary>
        ///     Gets/sets the id of the neuron we should incapsulate. This has to be a
        ///     statically declared neuron so we can easely use it from xaml.
        /// </summary>
        public PredefinedNeurons ID
        {
            get
            {
                return fID;
            }

            set
            {
                if (fID != value && System.Environment.HasShutdownStarted == false)
                {
                    // when shutting down, don't try to get anything from the network.
                    fID = value;
                    if (Neuron.IsEmpty((ulong)fID) == false)
                    {
                        Info = BrainData.Current.NeuronInfo[(ulong)fID];
                        Brain.Current.TryFindNeuron((ulong)fID, out fItem); // this can fail when terminating the app.
                    }
                    else
                    {
                        Info = null;
                        fItem = null;
                    }
                }
            }
        }

        #endregion

        #region Item

        /// <summary>
        ///     Gets the actual neuron.
        /// </summary>
        /// <remarks>
        ///     This allows us to get the neuron as the result value in a combobox for
        ///     instance.
        /// </remarks>
        public Neuron Item
        {
            get
            {
                return fItem;
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>Gets the neuron info.</summary>
        NeuronData INeuronInfo.NeuronInfo
        {
            get
            {
                return Info;
            }
        }

        #endregion

        #region Fields

        /// <summary>The f id.</summary>
        private PredefinedNeurons fID;

        /// <summary>The f item.</summary>
        private Neuron fItem;

        #endregion
    }
}