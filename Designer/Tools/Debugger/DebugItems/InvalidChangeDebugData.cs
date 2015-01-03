// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidChangeDebugData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains debug info, generated when a neuron was changed in a processor,
//   other than to which it was attached. This object allows us to link 2
//   <see cref="ProcItem" /> s together, so they can show the debug info
//   together.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains debug info, generated when a neuron was changed in a processor,
    ///     other than to which it was attached. This object allows us to link 2
    ///     <see cref="ProcItem" /> s together, so they can show the debug info
    ///     together.
    /// </summary>
    public class InvalidChangeDebugData : Data.ObservableObject
    {
        /// <summary>The f message.</summary>
        private string fMessage;

        /// <summary>
        ///     Gets or sets the <see cref="ProcItem" /> that caused the error.
        /// </summary>
        /// <value>
        ///     The originator.
        /// </value>
        public ProcItem Originator { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="ProcItem" /> that owned the neuron (because
        ///     it was attached to the procItem's processor).
        /// </summary>
        /// <value>
        ///     The owner.
        /// </value>
        public ProcItem Owner { get; set; }

        #region Message

        /// <summary>
        ///     Gets or sets the message that was generated to describe what was
        ///     changed.
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Message
        {
            get
            {
                return fMessage;
            }

            set
            {
                fMessage = value;
                OnPropertyChanged("Message");
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the neuron that was changed.
        /// </summary>
        /// <value>
        ///     The neuron.
        /// </value>
        public ulong NeuronID { get; set; }

        /// <summary>
        ///     Gets or sets the list of watches that attached the neuron to the
        ///     processor (can be <see langword="null" /> if the user attached the
        ///     neuron directly). There can be multiple watches, when they all monitor
        ///     the same variable. Normally this doesn't happen, but we are prepared.
        /// </summary>
        /// <value>
        ///     The watch.
        /// </value>
        public System.Collections.Generic.List<Watch> Watches { get; set; }
    }
}