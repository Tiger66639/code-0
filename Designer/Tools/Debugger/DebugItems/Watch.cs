// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Watch.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for variables that can be observed by the debugger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A wrapper class for variables that can be observed by the debugger.
    /// </summary>
    public class Watch : Data.ObservableObject, INeuronInfo, INeuronWrapper
    {
        #region ID

        /// <summary>
        ///     Gets/sets the id of the variable to wrap.
        /// </summary>
        public ulong ID
        {
            get
            {
                return fId;
            }

            set
            {
                if (fId != value)
                {
                    fId = value;
                    if (BrainData.Current != null && BrainData.Current.NeuronInfo != null)
                    {
                        RegisterNeuron();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the values for the watch, based on the currently selected
        ///     processor.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<DebugNeuron> Values
        {
            get
            {
                return fValues;
            }

            internal set
            {
                fValues = value;
                OnPropertyChanged("Values");
            }
        }

        #region InvalidChangeData

        /// <summary>
        ///     Gets the data object that was generated because a neuron, monitored by
        ///     this watch, was changed in a processor other than the one that it was
        ///     attached to.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public InvalidChangeDebugData InvalidChangeData
        {
            get
            {
                return fInvalidChangeData;
            }

            internal set
            {
                fInvalidChangeData = value;
                OnPropertyChanged("InvalidChangeData");
            }
        }

        #endregion

        #region AttachValuesToProcessor

        /// <summary>
        ///     Gets/sets wether values for this variable will be attached to the
        ///     active processor when they are assigned to the variable.
        /// </summary>
        /// <remarks>
        ///     When a watch gets removed from the <see cref="WatchCollection" /> it
        ///     belongs to (usually in
        ///     <see cref="JaStDev.HAB.Designer.DesignerDataFile.Watches" /> ) the
        ///     collection will remove the variable from the AttachedDict. this way we
        ///     don't have to monitor a remove or destroy or something.
        /// </remarks>
        public bool AttachValuesToProcessor
        {
            get
            {
                return fAttachValuesToProcessor;
            }

            set
            {
                if (value != fAttachValuesToProcessor)
                {
                    fAttachValuesToProcessor = value;
                    if (value)
                    {
                        ProcessorManager.Current.AtttachedDict.MonitorWatch(this);
                    }
                    else
                    {
                        ProcessorManager.Current.AtttachedDict.RemoveWatch(this);
                    }

                    OnPropertyChanged("AttachValuesToProcessor");
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public NeuronData NeuronInfo { get; private set; }

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
                if (Brain.Current.TryFindNeuron(ID, out iFound))
                {
                    return iFound;
                }

                return null;
            }
        }

        #endregion

        /// <summary>Loads the values for the variable that we are wrapping as found in the
        ///     context of the specified processor.</summary>
        /// <param name="proc">The proc.</param>
        internal void LoadValuesFor(Processor proc)
        {
            var iVar = Brain.Current[ID] as Variable;
            if (iVar != null)
            {
                var iVal = new System.Collections.ObjectModel.ObservableCollection<DebugNeuron>();
                foreach (var i in iVar.GetValueWithoutInit(proc))
                {
                    if (i != null)
                    {
                        DebugNeuron iToAdd;
                        if (Values != null)
                        {
                            // we try to reuse any debug neurons, so that the collapsed/expanded value stays the same.
                            var iFound = (from ii in Values where ii != null && ii.Item == i select ii).FirstOrDefault();
                            if (iFound != null)
                            {
                                Values[Values.IndexOf(iFound)] = null;

                                    // make certain we only reuse the same debugNeuron once, in case the same neuron is multiple times in the result. We don't remove, but set to null, so that we can also expand items that are on the same location as a previosly expanded item.
                                iToAdd = iFound;
                            }
                            else
                            {
                                DebugNeuron iPrev = null;
                                if (Values.Count > iVal.Count && Values.Count > 0)
                                {
                                    iPrev = Values[iVal.Count];
                                }

                                iToAdd = new DebugNeuron(i);
                                if (iPrev != null)
                                {
                                    iToAdd.IsExpanded = iPrev.IsExpanded;
                                }
                            }
                        }
                        else
                        {
                            iToAdd = new DebugNeuron(i);
                        }

                        iVal.Add(iToAdd);
                    }
                }

                Values = iVal;
            }
        }

        /// <summary>The register neuron.</summary>
        internal void RegisterNeuron()
        {
            NeuronInfo = BrainData.Current.NeuronInfo[fId];
            if (NeuronInfo != null && !(NeuronInfo.Neuron is Variable))
            {
                // if it's not a var, not allowed.    
                NeuronInfo = null;
                LogService.Log.LogError("Watch.ID", "A watch can only wrap variables!");
            }
        }

        #region Fields

        /// <summary>The f id.</summary>
        private ulong fId;

        /// <summary>The f values.</summary>
        private System.Collections.ObjectModel.ObservableCollection<DebugNeuron> fValues;

        /// <summary>The f invalid change data.</summary>
        private InvalidChangeDebugData fInvalidChangeData;

        /// <summary>The f attach values to processor.</summary>
        private bool fAttachValuesToProcessor;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Watch" /> class.
        /// </summary>
        public Watch()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Watch"/> class.</summary>
        /// <param name="id">The id.</param>
        public Watch(ulong id)
        {
            ID = id;
        }

        #endregion
    }
}