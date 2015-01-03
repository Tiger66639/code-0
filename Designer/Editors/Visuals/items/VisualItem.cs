// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualItem.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a single point in a visual cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Represents a single point in a visual cluster.
    /// </summary>
    /// <remarks>
    ///     Should stil get it's own event monitor, so we can check for changes in
    ///     value an operator.
    /// </remarks>
    public class VisualItem : Data.OwnedObject<VisualFrame>, INeuronWrapper, INeuronInfo
    {
        /// <summary>Initializes a new instance of the <see cref="VisualItem"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        public VisualItem(IntNeuron toWrap)
        {
            fToWrap = toWrap;
        }

        #region Value

        /// <summary>
        ///     Gets/sets the integer value that is assigned to the neuron
        /// </summary>
        public int Value
        {
            get
            {
                return fToWrap.Value;
            }

            set
            {
                if (fToWrap.Value != value)
                {
                    OnPropertyChanging("Value", fToWrap.Value, value);
                    fToWrap.Value = value;
                    OnPropertyChanged("Value");
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        #endregion

        #region Operator

        /// <summary>
        ///     Gets/sets the integer value that is assigned to the neuron
        /// </summary>
        public Neuron Operator
        {
            get
            {
                return fToWrap.FindFirstOut((ulong)PredefinedNeurons.Operator);
            }

            set
            {
                if (Operator != value)
                {
                    EditorsHelper.SetFirstOutgoingLinkTo(fToWrap, (ulong)PredefinedNeurons.Operator, value);
                    OnPropertyChanged("Operator");
                    OnPropertyChanged("HasOperator");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance has an opeator assigned
        ///     or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has opeator; otherwise, <c>false</c> .
        /// </value>
        public bool HasOpeator
        {
            get
            {
                return fToWrap.FindFirstOut((ulong)PredefinedNeurons.Operator) != null;
            }
        }

        #region IsChecked

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is checked; otherwise, <c>false</c> .
        /// </value>
        public bool IsChecked
        {
            get
            {
                return fToWrap.Value == Owner.Owner.HighValue;
            }

            set
            {
                if (value != IsChecked)
                {
                    if (value)
                    {
                        Value = Owner.Owner.HighValue;
                        fToWrap.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, Owner.Owner.HighValOperator);

                            // if there is no operator declared in the owner, this will reset it to null
                    }
                    else
                    {
                        Value = Owner.Owner.LowValue;
                        fToWrap.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, Owner.Owner.LowValOperator);
                    }

                    OnPropertyChanged("IsChecked");
                    OnPropertyChanged("Operator");
                    OnPropertyChanged("HasOperator");
                }
            }
        }

        #endregion

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                if (fNeuronData == null)
                {
                    if (fToWrap != null)
                    {
                        fNeuronData = BrainData.Current.NeuronInfo[fToWrap.ID];
                    }
                }

                return fNeuronData;
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item
        {
            get
            {
                return fToWrap;
            }
        }

        #endregion

        #region fields

        /// <summary>The f to wrap.</summary>
        private readonly IntNeuron fToWrap;

        /// <summary>The f neuron data.</summary>
        private NeuronData fNeuronData;

                           // we need to keep a ref to the object, because the ui bindings buffer the object, which might cause problems if the designer unloads it.
        #endregion
    }
}