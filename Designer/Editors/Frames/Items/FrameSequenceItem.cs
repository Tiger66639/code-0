// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameSequenceItem.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for the neuron that represents a frame sequence item (1
//   item in a sequence).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for the neuron that represents a frame sequence item (1
    ///     item in a sequence).
    /// </summary>
    public class FrameSequenceItem : FrameItemBase
    {
        /// <summary>The f event monitor.</summary>
        private FSItemEventMonitor fEventMonitor;

        /// <summary>Initializes a new instance of the <see cref="FrameSequenceItem"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public FrameSequenceItem(Neuron toWrap)
            : base(toWrap)
        {
            fEventMonitor = EventManager.Current.RegisterFrameSequenceItem(this);
        }

        #region Element

        /// <summary>
        ///     Gets/sets the neuron used to indicate which importance a frame element
        ///     has.
        /// </summary>
        public Neuron Element
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.FrameSequenceItemValue);
            }

            set
            {
                var iCur = Element;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.FrameSequenceItemValue, value);
                }
            }
        }

        #endregion

        #region ElementInfo

        /// <summary>
        ///     Gets the extra neuron info for the
        ///     <see cref="JaStDev.HAB.Designer.FrameSequenceItem.Element" /> , so
        ///     that the title can be displayed properly.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public NeuronData ElementInfo
        {
            get
            {
                var iEl = Element;
                if (iEl != null)
                {
                    return BrainData.Current.NeuronInfo[iEl.ID];
                }

                return null;
            }
        }

        #endregion

        #region ResultType

        /// <summary>
        ///     Gets/sets the result type for the element's result cluster. This prop
        ///     allows us to <see langword="override" /> the element's
        ///     <see cref="ResultType" /> value. This way, we can change the result
        ///     type depending on the sequence, while providing a default value.
        /// </summary>
        public Neuron ResultType
        {
            get
            {
                return Item.FindFirstOut((ulong)PredefinedNeurons.FrameElementResultType);
            }

            set
            {
                var iCur = ResultType;
                if (iCur != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(Item, (ulong)PredefinedNeurons.FrameElementResultType, value);
                }
            }
        }

        #endregion
    }
}