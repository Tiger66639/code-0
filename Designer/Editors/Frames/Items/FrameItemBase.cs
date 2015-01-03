// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameItemBase.cs" company="">
//   
// </copyright>
// <summary>
//   The frame item base.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The frame item base.</summary>
    public class FrameItemBase : Data.OwnedObject<INeuronWrapper>, INeuronWrapper, INeuronInfo
    {
        #region fields

        /// <summary>The f neuron data.</summary>
        private NeuronData fNeuronData;

                           // we need to keep a ref to the object, because the ui bindings buffer the object, which might cause problems if the designer unloads it.
        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FrameItemBase"/> class. Initializes a new instance of the <see cref="FrameElement"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public FrameItemBase(Neuron toWrap)
        {
            Item = toWrap;
        }

        #endregion

        #region NeuronInfo (INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (fNeuronData == null)
                {
                    if (Item != null)
                    {
                        fNeuronData = BrainData.Current.NeuronInfo[Item.ID];
                    }
                }

                return fNeuronData;
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #endregion

        /// <summary>Called by the monitor when a link has changed that is depicted as a
        ///     property (so that everybody knows about the change).</summary>
        /// <param name="name">The name.</param>
        internal void CallPropertyChangedChanged(string name)
        {
            OnPropertyChanged(name);
        }
    }
}