// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   Provides timing functionality to the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Provides timing functionality to the brain.
    /// </summary>
    /// <remarks>
    ///     When the timersin is active, the <see cref="Neuron.StatementsCluster" />
    ///     of the timersin will be used as callback for each timer tick.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.IsActive, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Interval, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.TimerNeuron, typeof(Neuron))]
    public class TimerNeuron : Neuron
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimerNeuron" /> class.
        /// </summary>
        public TimerNeuron()
        {
            fTimer = new System.Timers.Timer();
            fTimer.Elapsed += Timer_Elapsed;
        }

        #region Interval

        /// <summary>
        ///     Gets/sets the time interval between 2 consecutive executes expressed
        ///     in milliseconds.
        /// </summary>
        /// <remarks>
        ///     When no value is specified, the default of 1 second is used.
        /// </remarks>
        public double Interval
        {
            get
            {
                return fInterval;
            }

            set
            {
                if (fInterval != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fInterval = value;
                        fTimer.Interval = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    SetIsChangedNoUnfreeze(true);

                        // don't unfreeze, otherwise changing the value would cause to many mem leaks.
                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Interval", this));
                    }
                }
            }
        }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets if this timer is currently active or not.
        /// </summary>
        /// <remarks>
        ///     This property is not backed by a neuron cause the brain should not be
        ///     able to set this value through a neuron, since this doesn't trigger a
        ///     change in the timer. This needs to be done through the output system.
        /// </remarks>
        public bool IsActive
        {
            get
            {
                return fTimer.Enabled;
            }

            set
            {
                if (value != fTimer.Enabled)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fTimer.Enabled = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    SetIsChangedNoUnfreeze(true);
                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("IsActive", this));
                    }
                }
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextNeuron" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.TimerNeuron;
            }
        }

        #endregion

        #region StatementsCluster

        /// <summary>
        ///     Gets/sets the expressions that should be executed when the time has
        ///     ellapsed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public NeuronCluster StatementsCluster
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Statements) as NeuronCluster;
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Statements, value);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        ///     The timer used to call the code. Note: The brain keeps a hard
        ///     <see langword="ref" /> to all the sins, so the timer sin object will
        ///     never get unloaded, which is important when the timer is active.
        /// </summary>
        private readonly System.Timers.Timer fTimer;

        /// <summary>
        ///     Stores the previously created processor object. This is used to check
        ///     if it is still active. If so, we don't need to start a new processor,
        ///     but need to wait untill the prev is ready.
        /// </summary>
        private Processor fPrevProcess;

        /// <summary>The f interval.</summary>
        private double fInterval;

        #endregion

        #region Functions

        /// <summary>Handles the Elapsed event of the <see cref="fTimer"/> control.</summary>
        /// <remarks><para>If the previous processor hasn't finished when the new tick arrives,
        ///         no new processor is started, but the tick is skipped.</para>
        /// <para>When a tick is passed, we call the '<see cref="JaStDev.HAB.Neuron.Actions"/> code attached to this sin.
        ///         We also check if the <see cref="Interval"/> is changed and update
        ///         here. We do this here so that the brain can change it's own processing
        ///         interval.</para>
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event
        ///     data.</param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (fPrevProcess == null || fPrevProcess.CurrentState == Processor.State.Terminated)
            {
                var iProcessor = ProcessorFactory.GetProcessor();
                if (iProcessor != null)
                {
                    var iStatements = StatementsCluster;
                    if (iStatements != null)
                    {
                        fPrevProcess = iProcessor;
                        ThreadManager.Default.ReserveTimer(iProcessor, iStatements);
                    }
                    else
                    {
                        LogService.Log.LogWarning(
                            "TimerNeuron.Timer_Elapsed", 
                            "No code assigned to Timer (failed to find a Statements cluster)! Timer has been stopped.");
                        IsActive = false;
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "TimerNeuron.Timer_Elapsed", 
                        "Processor factory failed to create processor!");
                }
            }
            else
            {
                LogService.Log.LogWarning(
                    "TimerNeuron.Timer_Elapsed", 
                    "Timer callback code was still running, skipping timer slot.");
            }
        }

        /// <summary>Reads the class from xml file.</summary>
        /// <param name="reader"></param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            Interval = XmlStore.ReadElement<double>(reader, "Interval");
            IsActive = XmlStore.ReadElement<bool>(reader, "IsActive");
        }

        /// <summary>Writes the class to xml files</summary>
        /// <param name="writer">The xml writer to use</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "Interval", Interval);
            XmlStore.WriteElement(writer, "IsActive", IsActive);
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
            Interval = reader.ReadDouble();
            IsActive = reader.ReadBoolean();
            return iRes;
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            writer.Write(Interval);
            writer.Write(IsActive);
        }

        /// <summary>Copies all the data from this neuron to the argument.</summary>
        /// <remarks><para>By default, it only copies over all of the links (which includes the
        ///         'LinksOut' and 'LinksIn' lists.</para>
        /// <para>Inheriters should reimplement this function and copy any extra
        ///         information required for their specific type of neuron.</para>
        /// </remarks>
        /// <param name="copyTo">The object to copy their data to.</param>
        protected internal override void CopyTo(Neuron copyTo)
        {
            base.CopyTo(copyTo);
            var iCopyTo = copyTo as TimerNeuron;
            if (iCopyTo != null)
            {
                iCopyTo.IsActive = IsActive;
                iCopyTo.Interval = Interval;
            }
        }

        #endregion
    }
}