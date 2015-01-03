// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowItemStatic.cs" company="">
//   
// </copyright>
// <summary>
//   A flow item that represents <see langword="static" /> content: other
//   neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A flow item that represents <see langword="static" /> content: other
    ///     neurons.
    /// </summary>
    public class FlowItemStatic : FlowItem
    {
        /// <summary>The f is link.</summary>
        private bool fIsLink;

        /// <summary>Initializes a new instance of the <see cref="FlowItemStatic"/> class.</summary>
        /// <param name="toWrap">The item to wrap.</param>
        public FlowItemStatic(Neuron toWrap)
            : base(toWrap)
        {
        }

        #region IsLink

        /// <summary>
        ///     Gets wether this <see langword="static" /> item references another
        ///     flow.
        /// </summary>
        public bool IsLink
        {
            get
            {
                return fIsLink;
            }

            internal set
            {
                fIsLink = value;
                OnPropertyChanged("IsLink");
            }
        }

        #endregion

        /// <summary>The on item changed.</summary>
        /// <param name="value">The value.</param>
        protected override void OnItemChanged(Neuron value)
        {
            base.OnItemChanged(value);
            var iVal = value as NeuronCluster;
            IsLink = iVal != null && iVal.Meaning == (ulong)PredefinedNeurons.Flow;
        }
    }
}