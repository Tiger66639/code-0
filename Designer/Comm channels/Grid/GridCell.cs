// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridCell.cs" company="">
//   
// </copyright>
// <summary>
//   represents a single value in the current state of the
//   <see cref="GridChannel" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     represents a single value in the current state of the
    ///     <see cref="GridChannel" />
    /// </summary>
    public class GridCell : Data.OwnedObject<GridChannel>, INeuronWrapper, INeuronInfo
    {
        #region fields

        /// <summary>The f neuron info.</summary>
        private NeuronData fNeuronInfo;

        #endregion

        /// <summary>The f value.</summary>
        private Neuron fValue;

        /// <summary>Initializes a new instance of the <see cref="GridCell"/> class.</summary>
        /// <param name="toWrap">The to Wrap.</param>
        /// <param name="owner">The owner.</param>
        public GridCell(Neuron toWrap, GridChannel owner)
        {
            Item = toWrap;
            System.Diagnostics.Debug.Assert(owner != null);
            Owner = owner;
        }

        #region INeuronInfo Members

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        public NeuronData NeuronInfo
        {
            get
            {
                if (fNeuronInfo == null && Item != null)
                {
                    fNeuronInfo = BrainData.Current.NeuronInfo[Item];
                }

                return fNeuronInfo;
            }
        }

        #endregion

        #region INeuronWrapper Members

        /// <summary>
        ///     Gets the item.
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion

        #region Value

        /// <summary>
        ///     Gets/sets the value that this cell currently represents.
        /// </summary>
        public Neuron Value
        {
            get
            {
                return fValue;
            }

            set
            {
                if (value != fValue)
                {
                    fValue = value;
                    fNeuronInfo = null;
                    Owner.ChangeValue(this);
                    OnPropertyChanged("Value");
                }
            }
        }

        /// <summary>
        ///     Gets the value as an enumerable. This is a convenience prop so that it
        ///     can easily be passed on to the sin.
        /// </summary>
        public System.Collections.Generic.IEnumerable<Neuron> Values
        {
            get
            {
                yield return fValue;
            }
        }

        #endregion
    }
}