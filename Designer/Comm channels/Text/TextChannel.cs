// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextChannel.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for the <see cref="TextSin" /> . Provides some extra
//   props for visualisation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A wrapper class for the <see cref="TextSin" /> . Provides some extra
    ///     props for visualisation.
    /// </summary>
    public class TextChannel : CommChannel, Test.ITesteable
    {
        #region fields

        /// <summary>The f dialog data.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> fDialogData =
            new System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData>();

        /// <summary>The f input splitter value.</summary>
        private double fInputSplitterValue;

        /// <summary>The f dialog splitter value.</summary>
        private double fDialogSplitterValue;

        /// <summary>The f neuron splitter value.</summary>
        private double fNeuronSplitterValue;

        /// <summary>The f view in.</summary>
        private bool fViewIn;

        /// <summary>The f view out.</summary>
        private bool fViewOut;

        /// <summary>The f speaker.</summary>
        private System.Speech.Synthesis.SpeechSynthesizer fSpeaker; // when set, we need to produce audio out.

        // bool fAudioOn;
        /// <summary>The f output data.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> fOutputData =
            new System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData>();

        /// <summary>The f input data.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> fInputData =
            new System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData>();

        // DebugNeuron fInputNeuron;
        /// <summary>The f process mode.</summary>
        private TextSinProcessMode? fProcessMode;

        /// <summary>The f input text.</summary>
        private string fInputText = "Type here"; // the default value.

        #endregion

        #region props

        #region InputSplitterValue

        /// <summary>
        ///     Gets/sets the size that should be used for the splitter that controls
        ///     the input dialog.
        /// </summary>
        public double InputSplitterValue
        {
            get
            {
                return fInputSplitterValue;
            }

            set
            {
                fInputSplitterValue = value;
                OnPropertyChanged("InputSplitterValue");
            }
        }

        #endregion

        #region DialogSplitterValue

        /// <summary>
        ///     Gets/sets the size that should be used for the splitter that controls
        ///     the conversation dialog.
        /// </summary>
        public double DialogSplitterValue
        {
            get
            {
                return fDialogSplitterValue;
            }

            set
            {
                fDialogSplitterValue = value;
                OnPropertyChanged("DialogSplitterValue");
            }
        }

        #endregion

        #region NeuronSplitterValue

        /// <summary>
        ///     Gets/sets the size that should be used for the splitter that seperates
        ///     the 2 neuron displays.
        /// </summary>
        public double NeuronSplitterValue
        {
            get
            {
                return fNeuronSplitterValue;
            }

            set
            {
                fNeuronSplitterValue = value;
                OnPropertyChanged("NeuronSplitterValue");
            }
        }

        #endregion

        #region AudioOn

        /// <summary>
        ///     Gets/sets wether oudio output is activated or not.
        /// </summary>
        public bool AudioOn
        {
            get
            {
                return fSpeaker != null;
            }

            set
            {
                if (AudioOn != value)
                {
                    if (value)
                    {
                        fSpeaker = new System.Speech.Synthesis.SpeechSynthesizer();
                    }
                    else
                    {
                        fSpeaker.Dispose();
                        fSpeaker = null;
                    }

                    OnPropertyChanged("AudioOn");
                }
            }
        }

        #endregion

        #region ViewIn

        /// <summary>
        ///     Gets/sets if the input node view should be visible.
        /// </summary>
        public bool ViewIn
        {
            get
            {
                return fViewIn;
            }

            set
            {
                fViewIn = value;
                OnPropertyChanged("ViewIn");
            }
        }

        #endregion

        #region ViewOut

        /// <summary>
        ///     Gets/sets if the output node view should be visible.
        /// </summary>
        public bool ViewOut
        {
            get
            {
                return fViewOut;
            }

            set
            {
                fViewOut = value;
                OnPropertyChanged("ViewOut");
            }
        }

        #endregion

        #region OutputData

        /// <summary>
        ///     Gets the list of tracking data rendered for output events (text
        ///     received from the brain).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> OutputData
        {
            get
            {
                return fOutputData;
            }
        }

        #endregion

        #region InputData

        /// <summary>
        ///     Gets the list of tracking data rendered for input events (text sent to
        ///     the brain).
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> InputData
        {
            get
            {
                return fInputData;
            }
        }

        #endregion

        #region InputText

        /// <summary>
        ///     Gets/sets the text currently typed in, but not yet sent to the
        ///     network. This allows us to remember the input text when the user has
        ///     changed tabs. Default value = 'Type here'.
        /// </summary>
        public string InputText
        {
            get
            {
                return fInputText;
            }

            set
            {
                fInputText = value;
                OnPropertyChanged("InputText");
            }
        }

        #endregion

        #region DialogData

        /// <summary>
        ///     Gets the data containing all the dialog statements.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<TextChanellTrackingData> DialogData
        {
            get
            {
                return fDialogData;
            }
        }

        #endregion

        #region ProcessMode

        /// <summary>
        ///     Gets/sets the the processing mode that should be used.
        /// </summary>
        public TextSinProcessMode? ProcessMode
        {
            get
            {
                return fProcessMode;
            }

            set
            {
                OnPropertyChanging("ProcessMode", fProcessMode, value);
                fProcessMode = value;
                OnPropertyChanged("ProcessMode");
            }
        }

        #endregion

        #region Textsin (ITesteable Members)

        /// <summary>
        ///     Gets or sets the textsin that should be used to communicate with
        ///     during testing.
        /// </summary>
        /// <value>
        ///     The textsin.
        /// </value>
        public TextSin Textsin
        {
            get
            {
                return (TextSin)Sin;
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>Sets the Sensory <see langword="interface"/> that this object is a
        ///     wrapper of.</summary>
        /// <param name="sin">The sin.</param>
        protected internal override void SetSin(Sin sin)
        {
            var iSin = sin as TextSin;
            if (iSin != null)
            {
                iSin.TextOut += Sin_TextOut;
            }

            base.SetSin(sin);
        }

        /// <summary>The sin_ text out.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Sin_TextOut(object sender, OutputEventArgs<string> e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action<OutputEventArgs<string>>(HandleIncommingData), 
                e);
        }

        /// <summary>The handle incomming data.</summary>
        /// <param name="e">The e.</param>
        private void HandleIncommingData(OutputEventArgs<string> e)
        {
            var iData = new TextChanellTrackingData(e.Value, Enumerable.ToArray(e.Data), "PC");
            OutputData.Add(iData);
            DialogData.Add(iData);
            if (fSpeaker != null)
            {
                fSpeaker.SpeakAsync(e.Value);
            }
        }

        /// <summary>Sends the text to sin.</summary>
        /// <param name="value">The value.</param>
        internal void SendTextToSin(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                var iSin = (TextSin)Sin;
                if (iSin != null)
                {
                    Neuron[] iRes;
                    if (ProcessMode.HasValue == false)
                    {
                        iRes = iSin.Process(value, ProcessorFactory.GetProcessor());
                    }
                    else
                    {
                        iRes = iSin.Process(value, ProcessorFactory.GetProcessor(), ProcessMode.Value);
                    }

                    if (iRes != null)
                    {
                        var iData = new TextChanellTrackingData(value, iRes, "You");
                        InputData.Add(iData);
                        DialogData.Add(iData);
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "TextChannel.SendText", 
                            "Sin failed to return a neuron for the output text!");
                    }
                }
            }
        }

        /// <summary>Sends the text to sin. Use this when called from a thread other then
        ///     UI thread.</summary>
        /// <param name="value">The value.</param>
        public void SendTextToSinAsync(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                var iSin = (TextSin)Sin;
                if (iSin != null)
                {
                    Neuron[] iRes;
                    if (ProcessMode.HasValue == false)
                    {
                        iRes = iSin.Process(value, ProcessorFactory.GetProcessor());
                    }
                    else
                    {
                        iRes = iSin.Process(value, ProcessorFactory.GetProcessor(), ProcessMode.Value);
                    }

                    if (iRes != null)
                    {
                        var iData = new TextChanellTrackingData(value, iRes, "You");
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Action<TextChanellTrackingData>(InputData.Add), 
                            iData); // do async cause UI gets updated cause of this.
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Action<TextChanellTrackingData>(DialogData.Add), 
                            iData); // do async cause UI gets updated cause of this.
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "TextChannel.SendText", 
                            "Sin failed to return a neuron for the output text!");
                    }
                }
            }
        }

        /// <summary>
        ///     Clears all the data from the channel.
        /// </summary>
        internal void ClearData()
        {
            OutputData.Clear();
            InputData.Clear();
            DialogData.Clear();
        }

        #endregion

        #region Xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <remarks>When streaming to a module (for export), we do a mapping, to the index
        ///     of the neuron in the module that is currently being exported, and off
        ///     course visa versa, when reading from a module.</remarks>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "InputSplitterValue", InputSplitterValue);
            XmlStore.WriteElement(writer, "DialogSplitterValue", DialogSplitterValue);
            XmlStore.WriteElement(writer, "NeuronSplitterValue", NeuronSplitterValue);
            XmlStore.WriteElement(writer, "AudioOn", AudioOn);
            XmlStore.WriteElement(writer, "ViewIn", ViewIn);
            XmlStore.WriteElement(writer, "ViewOut", ViewOut);
            XmlStore.WriteElement(writer, "InputText", InputText);
            if (ProcessMode.HasValue)
            {
                XmlStore.WriteElement(writer, "ProcessMode", ProcessMode.Value);
            }
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <remarks>Descendents need to perform mapping between module index and neurons
        ///     when importing from modules.</remarks>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            InputSplitterValue = XmlStore.ReadElement<double>(reader, "InputSplitterValue");
            DialogSplitterValue = XmlStore.ReadElement<double>(reader, "DialogSplitterValue");
            NeuronSplitterValue = XmlStore.ReadElement<double>(reader, "NeuronSplitterValue");

            AudioOn = XmlStore.ReadElement<bool>(reader, "AudioOn");
            ViewIn = XmlStore.ReadElement<bool>(reader, "ViewIn");
            ViewOut = XmlStore.ReadElement<bool>(reader, "ViewOut");
            string iFound = null;
            if (XmlStore.TryReadElement(reader, "InputText", ref iFound))
            {
                InputText = iFound;
            }

            var iMode = TextSinProcessMode.LetterStream;
            if (XmlStore.TryReadEnum(reader, "ProcessMode", ref iMode))
            {
                ProcessMode = iMode;
            }
        }

        #endregion
    }
}