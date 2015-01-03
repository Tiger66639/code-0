// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextChanellTrackingData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data required by the <see cref="TextChannel" /> to keep
//   a link between text values displayed on the screen and neurons in the
//   brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the data required by the <see cref="TextChannel" /> to keep
    ///     a link between text values displayed on the screen and neurons in the
    ///     brain.
    /// </summary>
    public class TextChanellTrackingData : Data.ObservableObject
    {
        #region Fields

        /// <summary>The f data.</summary>
        private Neuron[] fData;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TextChanellTrackingData"/> class. Initializes a new instance of the<see cref="TextChanellTrackingData"/> class.</summary>
        /// <param name="text">The text representation of the data.</param>
        /// <param name="data">The data.</param>
        /// <param name="originator">The originator of the data.</param>
        public TextChanellTrackingData(string text, Neuron data, string originator)
        {
            Text = text;
            Originator = originator;
            Data = new[] { data };
        }

        /// <summary>Initializes a new instance of the <see cref="TextChanellTrackingData"/> class. Initializes a new instance of the<see cref="TextChanellTrackingData"/> class.</summary>
        /// <param name="text">The text representation of the data.</param>
        /// <param name="data">The data.</param>
        /// <param name="originator">The originator of the data.</param>
        public TextChanellTrackingData(string text, Neuron[] data, string originator)
        {
            Text = text;
            Originator = originator;
            Data = data;
        }

        #endregion

        #region Prop

        #region Text

        /// <summary>
        ///     Gets/sets the text value representation of the data.
        /// </summary>
        public string Text { get; internal set; }

        #endregion

        #region Data

        /// <summary>
        ///     Gets/sets the neurons that represent the text block in the brain.
        /// </summary>
        public Neuron[] Data
        {
            get
            {
                return fData;
            }

            set
            {
                fData = value;
                DebugData = new System.Collections.Generic.List<DebugNeuron>();
                if (fData != null)
                {
                    foreach (var i in fData)
                    {
                        DebugData.Add(new DebugNeuron(i));
                    }
                }
            }
        }

        #endregion

        #region DebugData

        /// <summary>
        ///     Gets the debug representation of the
        ///     <see cref="TextChannelTrackingData.Data" /> .
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> DebugData { get; internal set; }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the if this item is selected.
        /// </summary>
        /// <remarks>
        ///     By making this raise an event, multiple ui items can bind to it so
        ///     they become selected at the same time.
        /// </remarks>
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

        #region Originator

        /// <summary>
        ///     Gets the name of the one who caused the event (the pc, you,...)
        /// </summary>
        public string Originator { get; internal set; }

        #endregion

        #endregion
    }
}